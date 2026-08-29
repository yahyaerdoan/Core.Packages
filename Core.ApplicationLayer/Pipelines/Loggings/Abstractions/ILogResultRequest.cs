namespace Core.ApplicationLayer.Pipelines.Loggings.Abstractions;

/// <summary>Opt-in marker for LogResultAddingBehavior: logs the outcome after the handler runs, so
/// non-throwing failures (e.g. Result.BadRequest) still show up in the log.</summary>
public interface ILogResultRequest
{
}
