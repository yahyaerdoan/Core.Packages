namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Constants;

public static class ExceptionErrorCodes
{
    public const string BusinessRuleViolated = "BusinessRule.Violated";
    public const string ConcurrencyConflict = "Concurrency.Conflict";
    public const string DataConflict = "Data.Conflict";
    public const string InvalidJson = "Request.InvalidJson";
    public const string InvalidQuery = "Query.Invalid";
}
