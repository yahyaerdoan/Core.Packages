using Microsoft.AspNetCore.Identity;

using ResultHandler.Core.Base;

namespace Core.ApplicationLayer.Extensions;

/// <summary>Turns a failed <see cref="IdentityResult"/> into a field-scoped <see cref="OperationResult"/>, the same shape FluentValidation failures already produce via <see cref="ResultHandler.Core.Abstractions.IFieldFailureFactory{TSelf}"/>.</summary>
public static class IdentityResultExtensions
{
    private static readonly IReadOnlyDictionary<string, string> DefaultFieldByErrorCode = new Dictionary<string, string>
    {
        [nameof(IdentityErrorDescriber.DuplicateUserName)] = "UserName",
        [nameof(IdentityErrorDescriber.InvalidUserName)] = "UserName",
        [nameof(IdentityErrorDescriber.DuplicateEmail)] = "Email",
        [nameof(IdentityErrorDescriber.InvalidEmail)] = "Email",
        [nameof(IdentityErrorDescriber.PasswordMismatch)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordTooShort)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordRequiresDigit)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordRequiresLower)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordRequiresUpper)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)] = "Password",
        [nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars)] = "Password",
        [nameof(IdentityErrorDescriber.InvalidToken)] = "Token",
        [nameof(IdentityErrorDescriber.InvalidRoleName)] = "RoleName",
        [nameof(IdentityErrorDescriber.DuplicateRoleName)] = "RoleName",
        [nameof(IdentityErrorDescriber.UserAlreadyInRole)] = "Role",
        [nameof(IdentityErrorDescriber.UserNotInRole)] = "Role",
    };

    /// <summary>
    /// Groups <paramref name="result"/>'s errors into a fieldErrors dictionary. <paramref name="fieldByErrorCode"/>
    /// overrides the default code-to-field map for a specific call site (e.g. renaming "Password" to
    /// "CurrentPassword" for a change-password command); a code neither map knows falls back to "General".
    /// </summary>
    public static OperationResult ToOperationResult(this IdentityResult result, IReadOnlyDictionary<string, string>? fieldByErrorCode = null)
        => OperationResult.Failure(BuildFieldErrors(result, fieldByErrorCode));

    /// <inheritdoc cref="ToOperationResult(IdentityResult, IReadOnlyDictionary{string, string}?)"/>
    public static OperationDataResult<T> ToOperationResult<T>(this IdentityResult result, IReadOnlyDictionary<string, string>? fieldByErrorCode = null)
        => OperationDataResult<T>.Failure(BuildFieldErrors(result, fieldByErrorCode));

    private static Dictionary<string, IReadOnlyList<string>> BuildFieldErrors(IdentityResult result, IReadOnlyDictionary<string, string>? fieldByErrorCode)
        => result.Errors
        .GroupBy(error => fieldByErrorCode?.GetValueOrDefault(error.Code) ?? DefaultFieldByErrorCode
        .GetValueOrDefault(error.Code, "General"))
        .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(e => e.Description)]);
}
