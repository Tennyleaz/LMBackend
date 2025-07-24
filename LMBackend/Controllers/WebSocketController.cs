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
    private readonly ConcurrentQueue<SttChunk> rawQueue = new ConcurrentQueue<SttChunk>();
    private readonly ConcurrentQueue<SttChunk> wavFileQueue = new ConcurrentQueue<SttChunk>();
    private const int SLICE_SEC = 1;
    private const string INPUT_TYPE = "ogg";
    private const int MAX_COMBINE = 5;

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
        string chunkDir = @"C:\temp audio";
        Directory.CreateDirectory(chunkDir);
        KillFfmpeg();

        using MemoryStream bufferStream = new MemoryStream();
        byte[] buffer = new byte[8192];

        bool isStopped = false;
        bool isQueueStopped = false;
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
                            double start = chunk.Start * SLICE_SEC + data.Start.TotalSeconds;
                            double end = chunk.Start * SLICE_SEC + data.End.TotalSeconds;
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
                    if (isQueueStopped)
                    {
                        Console.WriteLine("transcriptionTask isQueueStopped");
                        break;
                    }
                }
                await Task.Delay(1000); // Check every 1s
            }
            Console.WriteLine("transcriptionTask stopped");
            // Tell user to close again
            await SendTranscript(socket, null, 0, 0, true);
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
                    if (rawQueue.Count > 0|| isStopped)
                    {
                        // Check if no files
                        /*if (fileCounter == currentFileIndex)
                        {
                            if (isStopped)
                                break;
                            if (isPaused)
                                continue;
                        }
                        // Take the 1 file before current, to overlap
                        int start = Math.Max(currentFileIndex, 1);  // Started at 1
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
                        }*/
                        try
                        {
                            //Console.WriteLine("Creating wav file...");
                            if (rawQueue.TryDequeue(out var result))
                            {
                                Console.WriteLine("Createing wav file from: " + result.File);
                                //string combinedFile = CombineAudioFiles(chunkDir, result.Start, result.Start);
                                string combinedFile = ConvertToWav(result.File);
                                Console.WriteLine("Created wav file: " + combinedFile);
                                SttChunk chunk = new SttChunk
                                {
                                    File = combinedFile,
                                    Start = result.Start,
                                    End = fileCounter,
                                    IsLast = isStopped
                                };
                                wavFileQueue.Enqueue(chunk);
                            }
                            else if (isStopped)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        currentFileIndex = fileCounter;  // Advance after enqueueing
                    }
                    await Task.Delay(1000); // Check every second
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            isQueueStopped = true;
            Console.WriteLine("combineTask stopped");
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
                        fileCounter++;  // File count starts at 1
                        string fileName = $"chunk_{Guid.NewGuid()}.{INPUT_TYPE}";
                        fileName = Path.Combine(chunkDir, fileName);
                        await System.IO.File.WriteAllBytesAsync(fileName, bufferStream.ToArray());
                        bufferStream.SetLength(0);  // Clear for next chunk                                                   
                        //Console.WriteLine($"Write audio file {fileCounter} from websocket");
                        rawQueue.Enqueue(new SttChunk
                        {
                            Start = fileCounter,
                            End = 0,
                            File = fileName,
                            IsLast = isStopped
                        });
                    }
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
            Console.WriteLine("Websocket handler leave...");
            await transcriptionTask;
            await combineTask;
            transcriptionTask.Dispose();
            combineTask.Dispose();
            Console.WriteLine("Websocket handler leave... 1");
            wavFileQueue.Clear();
            Console.WriteLine("Websocket handler leave... 2");
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            Console.WriteLine("Websocket handler leave... 3");
            KillFfmpeg();
            ClearDir(chunkDir);
            Console.WriteLine("Websocket handler leave... done.");
        }
    }

    private static string CombineAudioFiles(string baseDir, int start, int end)
    {
        Console.WriteLine($"CombineAudioFiles {start} - {end}");

        // Kill any ffmpeg instance if exist
        KillFfmpeg();

        // Create a concat list text file
        string concatFileName = $"list_{start}-{end}.txt";
        concatFileName = Path.Combine(baseDir, concatFileName);
        List<string> files = new List<string>();
        for (int i=start; i<=end; i++)
        {
            string theFile = $"chunk_{i}.{INPUT_TYPE}";
            theFile = Path.Combine(baseDir, theFile);
            if (!System.IO.File.Exists(theFile))
            {
                Console.WriteLine("File not exist! " + theFile);
                continue;
            }
            string line = $"file '{theFile}'";
            files.Add(line);
        }
        System.IO.File.WriteAllLines(concatFileName, files.ToArray());

        // Call ffmpeg
        // ffmpeg -f concat -safe 0 -i mylist.txt -c copy output.wav
        string outputName = $"output_{start}-{end}.wav";
        outputName = Path.Combine(baseDir, outputName);
        string arguments = $"-f concat -safe 0 -i \"{concatFileName}\" -ar 16000 -acodec pcm_s16le -ac 1 \"{outputName}\"";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process process = Process.Start(psi);
        process.OutputDataReceived += Process_OutputDataReceived;
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            Console.WriteLine("Error args: " + arguments);
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {error}");
        }
        return outputName;
    }

    private static string ConvertToWav(string input)
    {
        // Kill any ffmpeg instance if exist
        KillFfmpeg();

        if (!System.IO.File.Exists(input))
        {
            Console.WriteLine("File not exist! " + input);
            return null;
        }

        // Call ffmpeg
        // ffmpeg -i chunk_1.webm -ar 16000 -ac 1 -acodec pcm_s16le chunk_1.wav
        string outputName = Path.ChangeExtension(input, ".wav");
        string arguments = $" -i \"{input}\" -ar 16000 -acodec pcm_s16le -ac 1 \"{outputName}\"";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process process = Process.Start(psi);
        process.OutputDataReceived += Process_OutputDataReceived;
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            Console.WriteLine("Error args: " + arguments);
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {error}");
        }
        return outputName;
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private static async Task SendTranscript(WebSocket socket, string text, double start, double end, bool isStopped)
    {
        //if (!string.IsNullOrWhiteSpace(text))
        {
            SttResult result = new SttResult
            {
                start = start,
                end = end,
                text = text,  // Could be null
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

    private static void ClearDir(string dir)
    {
        if (!Directory.Exists(dir))
            return;

        string[] files = Directory.GetFiles(dir);
        foreach (string file in files)
        {
            try
            {
                System.IO.File.Delete(file);
            }
            catch { }
        }
    }

    private static void KillFfmpeg()
    {
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
    }
}
