using Serilog;

namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;

/// <summary>Serilog-backed logger; register as a singleton so the container disposes it on shutdown and buffered events are flushed.</summary>
public abstract class BaseLoggerService : IDisposable
{
    protected ILogger Logger { get; set; } = Serilog.Core.Logger.None;

    protected BaseLoggerService() { }

    protected BaseLoggerService(ILogger logger)
    {
        Logger = logger;
    }

    public void Verbose(string message)
    {
        Logger.Verbose(message);
    }

    public void Fatal(string message)
    {
        Logger.Fatal(message);
    }

    public void Info(string message)
    {
        Logger.Information(message);
    }

    public void Warn(string message)
    {
        Logger.Warning(message);
    }

    public void Debug(string message)
    {
        Logger.Debug(message);
    }

    public void Error(string message)
    {
        Logger.Error(message);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            (Logger as IDisposable)?.Dispose();
        }
    }
}
