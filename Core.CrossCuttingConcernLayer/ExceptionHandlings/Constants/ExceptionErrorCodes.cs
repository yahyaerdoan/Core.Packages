using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Constants;

public static class ExceptionErrorCodes
{
    [ErrorCode(ResultStatus.Conflict, "The request breaks a business rule; \"detail\" says which.", General = true)]
    public const string BusinessRuleViolated = "BusinessRule.Violated";

    [ErrorCode(ResultStatus.Conflict, "Someone changed the record since it was loaded; reload it and try again.")]
    public const string ConcurrencyConflict = "Concurrency.Conflict";

    [ErrorCode(ResultStatus.Conflict, "The change conflicts with existing data, for example a duplicate value.")]
    public const string DataConflict = "Data.Conflict";

    [ErrorCode(ResultStatus.BadRequest, "The list query's filter or sort is invalid, for example an unknown field.")]
    public const string InvalidQuery = "Query.Invalid";
}
