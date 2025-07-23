using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Whisper.net;

namespace LMBackend.Controllers;

[Route("/api/v{version:apiVersion}/ws")]
[ApiController]
[ApiVersion("1.0")]
public class WebSocketController : Controller
{
    private readonly ISttService _sttService;
    private readonly ConcurrentQueue<SttChunk> wavFileQueue = new ConcurrentQueue<SttChunk>();

    public WebSocketController(ISttService sttService)
    {
        _sttService = sttService;
    }

    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleAudioWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task HandleAudioWebSocket(WebSocket socket)
    {
        string chunkDir = @"D:\test";
        Directory.CreateDirectory(chunkDir);

        using MemoryStream bufferStream = new MemoryStream();
        byte[] buffer = new byte[8192];

        bool isStopped = false;
        bool isPaused = false;
        Task transcriptionTask = Task.Run(async () =>
        {
            while (true)
            {
                if (wavFileQueue.TryDequeue(out SttChunk chunk))
                {
                    Console.WriteLine("Calling whisper... " + chunk.File);
                    try
                    {
                        IAsyncEnumerable<SegmentData> datas = _sttService.WhisperChunk(chunk.File);
                        await foreach (SegmentData data in datas)
                        {
                            double start = chunk.Start + data.Start.TotalSeconds;
                            double end = chunk.Start + data.End.TotalSeconds;
                            await SendTranscript(socket, data.Text, start, end, chunk.IsLast);
                        }
                        if (chunk.IsLast)
                        {
                            Console.WriteLine("Last chunk!");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    //Console.WriteLine("transcriptionTask nothing to dequeue.");
                }
                await Task.Delay(1000); // Check every 1s
            }
        });

        int fileCounter = 0;
        int currentFileIndex = 0;
        Task combineTask = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    // Only combine if current index > counter + 10, or is paused/stopped
                    if (fileCounter - currentFileIndex > 10 || isPaused || isStopped)
                    {
                        // Take the 1 file before current, to overlap
                        int start = Math.Max(currentFileIndex - 1, 0);
                        if (start >= fileCounter)
                        {
                            // No file to combine
                            if (isStopped)
                            {
                                // Leave if stopped
                                break;
                            }
                            await Task.Delay(1000);  // Wait for more files
                            continue;
                        }
                        try
                        {
                            //Console.WriteLine("Creating wav file...");
                            string combinedFile = CombineAudioFiles(chunkDir, start, fileCounter);
                            Console.WriteLine("Created wav file: " + combinedFile);
                            SttChunk chunk = new SttChunk
                            {
                                File = combinedFile,
                                Start = start,
                                End = fileCounter,
                                IsLast = isStopped
                            };
                            wavFileQueue.Enqueue(chunk);
                            currentFileIndex = fileCounter;  // Advance after enqueueing
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    //else if (isStopped)
                    //{
                    //    // Last files % 5 < 5
                    //    int remain = fileCounter % 5;
                    //    if (remain > 0)
                    //    {
                    //        int start = fileCounter - remain;
                    //        string combinedFile = CombineAudioFiles(chunkDir, start, fileCounter);
                    //        Console.WriteLine("Created wav file: " + combinedFile);
                    //        SttChunk chunk = new SttChunk
                    //        {
                    //            File = combinedFile,
                    //            Start = start,
                    //            End = fileCounter,
                    //            IsLast = isStopped
                    //        };
                    //        wavFileQueue.Enqueue(chunk);
                    //    }
                    //    break;
                    //}
                    await Task.Delay(1000); // Check every second
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });

        try
        {            
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    await bufferStream.WriteAsync(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                // This condition confirms we have received complete blob
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    if (bufferStream.Length > 0)
                    {
                        string fileName = $"chunk_{fileCounter++}.webm";
                        fileName = Path.Combine(chunkDir, fileName);
                        await System.IO.File.WriteAllBytesAsync(fileName, bufferStream.ToArray());
                        bufferStream.SetLength(0); // Clear for next chunk
                        //Console.WriteLine($"Write {result.Count} bytes from websocket");
                        //Console.WriteLine($"Write audio file {fileCounter} from websocket");
                    }

                    // append audio after every 5s, assume each chunk is 1s long

                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Websocket get close message.");
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Check what command was sent
                    string command = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Command: " + command);
                    if (command == "stop")
                    {
                        isPaused = isStopped = true;
                    }
                    else if (command == "pause")
                    {
                        isPaused = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to write websocket: " + ex);
        }
        finally
        {
            await transcriptionTask;
            await combineTask;
            transcriptionTask.Dispose();
            combineTask.Dispose();
            wavFileQueue.Clear();
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            Console.WriteLine("Websocket handler leave.");
        }
    }

    private static string CombineAudioFiles(string baseDir, int start, int end)
    {
        Console.WriteLine($"CombineAudioFiles {start} - {end}");

        // Kill any ffmpeg instance if exist
        Process[] processes = Process.GetProcessesByName("ffmpeg");
        foreach (Process p in processes)
        {
            try
            {
                p.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Create a concat list text file
        string concatFileName = $"list_{start}-{end}.txt";
        concatFileName = Path.Combine(baseDir, concatFileName);
        List<string> files = new List<string>();
        for (int i=start; i<=end; i++)
        {
            string theFile = $"chunk_{i}.webm";
            theFile = Path.Combine(baseDir, theFile);
            string line = $"file '{theFile}'";
            files.Add(line);
        }
        System.IO.File.WriteAllLines(concatFileName, files.ToArray());

        // Call ffmpeg
        // ffmpeg -f concat -safe 0 -i mylist.txt -c copy output.wav
        string outputName = $"output_{start}-{end}.wav";
        outputName = Path.Combine(baseDir, outputName);
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-f concat -safe 0 -i {concatFileName} -ar 16000 -acodec pcm_s16le -ac 1 {outputName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process process = Process.Start(psi);
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {error}");
        }
        return outputName;
    }

    private static async Task SendTranscript(WebSocket socket, string text, double start, double end, bool isStopped)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            SttResult result = new SttResult
            {
                start = start,
                end = end,
                text = text,
                isStopped = isStopped
            };
            string json = JsonSerializer.Serialize(result);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private struct SttChunk
    {
        public string File;
        public int Start;
        public int End;
        public bool IsLast;
    }

    private class SttResult
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
        public bool isStopped { get; set; }
    }
}
