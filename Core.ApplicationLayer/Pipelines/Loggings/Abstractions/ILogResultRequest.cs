namespace Core.ApplicationLayer.Pipelines.Loggings.Abstractions;

/// <summary>Opt-in marker for LogResultAddingBehavior - unlike ILogAddRequest (logs the request before it
/// runs), this logs the outcome (success/failure, status, title, detail, errors) after it runs, so failed
/// business-rule results (Result.BadRequest etc., which never throw) show up in the log instead of only
/// reaching the caller as an HTTP response.</summary>
public interface ILogResultRequest
{
}
