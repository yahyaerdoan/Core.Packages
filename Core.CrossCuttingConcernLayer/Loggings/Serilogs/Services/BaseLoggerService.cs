using Serilog;

namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;

public abstract class BaseLoggerService
{
    protected ILogger Logger { get; set; } = null!;

    protected BaseLoggerService() { }

    protected BaseLoggerService(ILogger logger)
    {
        Logger = logger;
    }

    public void Verbose(string message) => Logger.Verbose(message);
    public void Fatal(string message) => Logger.Fatal(message);
    public void Info(string message) => Logger.Information(message);
    public void Warn(string message) => Logger.Warning(message);
    public void Debug(string message) => Logger.Debug(message);
    public void Error(string message) => Logger.Error(message);
}
