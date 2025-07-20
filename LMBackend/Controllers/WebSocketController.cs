using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace LMBackend.Controllers;

[Route("/ws")]
public class WebSocketController : Controller
{
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleAudioWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task HandleAudioWebSocket(WebSocket socket)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        string webmPath = Path.Combine(tempDir, "input.webm");
        string pcmPath = Path.Combine(tempDir, "chunk.wav");

        await using var webmStream = System.IO.File.Create(webmPath);

        byte[] buffer = new byte[8192];
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastChunkMs = 0;

        Task transcriptionTask = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000); // Check every second

                var elapsedMs = stopwatch.ElapsedMilliseconds;
                if (elapsedMs - lastChunkMs >= Constants.WHISPER_CHUNK_SECONDS * 1000)
                {
                    lastChunkMs = elapsedMs;

                    // Copy and convert webm so far
                    System.IO.File.Copy(webmPath, $"{webmPath}.copy", overwrite: true);
                    ConvertToWav($"{webmPath}.copy", pcmPath);

                    // Transcribe chunk
                    string transcript = RunWhisperChunk(pcmPath, offsetSeconds: 0);
                    await SendTranscript(socket, transcript);
                }
            }
        });

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await webmStream.WriteAsync(buffer.AsMemory(0, result.Count));
                }
            }
        }
        finally
        {
            transcriptionTask.Dispose();
            Directory.Delete(tempDir, true);
            if (socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        }
    }

    private static void ConvertToWav(string input, string output)
    {
        string ffmpegCmd = $"ffmpeg -y -i \"{input}\" -ar {Constants.WHISPER_SAMPLE_RATE} -ac 1 -f wav \"{output}\"";
        RunShell(ffmpegCmd);
    }

    private static void RunShell(string command)
    {
        ProcessStartInfo psi = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
        {
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
    }

    private static string RunWhisperChunk(string wavPath, int offsetSeconds)
    {
        string args = $"-m \"{Constants.WHISPER_MODEL_PATH}\" -f \"{wavPath}\" --no-timestamps -otxt --offset_t {offsetSeconds}";
        ProcessStartInfo psi = new ProcessStartInfo("/bin/bash", $"-c \"cd {Constants.WHISPER_DIR} && ./main {args}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(psi);
        process.WaitForExit();

        string transcriptPath = Path.ChangeExtension(wavPath, ".txt");
        if (System.IO.File.Exists(transcriptPath))
        {
            return System.IO.File.ReadAllText(transcriptPath);
        }

        return string.Empty;
    }

    private static async Task SendTranscript(WebSocket socket, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
