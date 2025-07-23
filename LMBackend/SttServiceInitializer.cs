namespace LMBackend;

/// <summary>
/// Middleware call this service to warm up the ISttService.
/// </summary>
public class SttServiceInitializer : IHostedService
{
    private readonly ISttService _sttService;

    public SttServiceInitializer(ISttService sttService)
    {
        _sttService = sttService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sttService.BuildProcessor();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
