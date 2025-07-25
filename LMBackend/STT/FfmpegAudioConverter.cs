using System.Diagnostics;

namespace LMBackend.STT;

public class FfmpegAudioConverter : IAudioConverter
{
    public string ConvertToWav(string input)
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
        //process.OutputDataReceived += Process_OutputDataReceived;
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            Console.WriteLine("Error args: " + arguments);
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {error}");
        }

        // Delete completed files
        try
        {
            File.Delete(input);
        }
        catch { }
        return outputName;
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
