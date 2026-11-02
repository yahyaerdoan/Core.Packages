using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace Core.ApplicationLayer.Pipelines.Validations.Constants;

public static class ValidationErrorCodes
{
    [ErrorCode(ResultStatus.UnprocessableContent, "One or more fields are invalid; \"errors\" maps each field to its messages.", General = true)]
    public const string Failed = "Validation.Failed";
}
