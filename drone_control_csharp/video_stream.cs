using System.Diagnostics;
using TelloCommander.Commander;

namespace drone_control_csharp;

public class VideoStream
{
    private const string FfmpegPath = "ffmpeg";
    private readonly int _udpVideoPort;
    private Process _ffmpegProcess;
    
    
    public VideoStream(int port = 1111)
    {
        _udpVideoPort = port;
    }

    public void StartStream()
    {
        StartFfmpeg("-f sdl \"Drone Feed\"");
    }

    public void StartRecording(string filename)
    {
        StartFfmpeg($"-c copy {filename}.mp4");
    }

    public void StopRecording()
    {
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited) _ffmpegProcess.Kill();
        _ffmpegProcess = null;
    }

    private void StartFfmpeg(string arguments)
    {
        if (_ffmpegProcess != null){return;}
        ProcessStartInfo info = new ProcessStartInfo()
        {
            FileName = FfmpegPath,
            Arguments = $"-i udp://0.0.0.0:{_udpVideoPort} " + arguments,
            UseShellExecute = true,
            //LoadUserProfile = true,
            //WindowStyle = ProcessWindowStyle.Minimized
        };
        _ffmpegProcess = Process.Start(info);
        Console.WriteLine($"Started ffmpeg process: {_ffmpegProcess.Id}");
    }
}