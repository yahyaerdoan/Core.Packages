using FluentValidation;
using FluentValidation.Resources;

namespace Core.ApplicationLayer.Validations;

/// <summary>Overrides FluentValidation's default quoted messages with plain phrasing, solution-wide.</summary>
public static class FriendlyValidationMessages
{
    /// <summary>Call once at startup, before the host is built. Forces culture "en" since LanguageManager won't fall back from CurrentUICulture (e.g. "en-US") to it.</summary>
    public static void Apply()
    {
        var english = ValidatorOptions.Global.LanguageManager as LanguageManager
            ?? throw new InvalidOperationException("Expected FluentValidation's default LanguageManager to still be configured.");

        english.Culture = new System.Globalization.CultureInfo("en");

        english.AddTranslation("en", "NotEmptyValidator", "{PropertyName} is required.");
        english.AddTranslation("en", "NotNullValidator", "{PropertyName} is required.");
        english.AddTranslation("en", "EmptyValidator", "{PropertyName} must be empty.");
        english.AddTranslation("en", "NullValidator", "{PropertyName} must be empty.");
        english.AddTranslation("en", "MaximumLengthValidator", "{PropertyName} must be {MaxLength} characters or fewer.");
        english.AddTranslation("en", "MinimumLengthValidator", "{PropertyName} must be at least {MinLength} characters.");
        english.AddTranslation("en", "LengthValidator", "{PropertyName} must be between {MinLength} and {MaxLength} characters.");
        english.AddTranslation("en", "ExactLengthValidator", "{PropertyName} must be exactly {MaxLength} characters.");
        english.AddTranslation("en", "GreaterThanValidator", "{PropertyName} must be greater than {ComparisonValue}.");
        english.AddTranslation("en", "GreaterThanOrEqualValidator", "{PropertyName} must be {ComparisonValue} or greater.");
        english.AddTranslation("en", "LessThanValidator", "{PropertyName} must be less than {ComparisonValue}.");
        english.AddTranslation("en", "LessThanOrEqualValidator", "{PropertyName} must be {ComparisonValue} or less.");
        english.AddTranslation("en", "EqualValidator", "{PropertyName} must equal {ComparisonValue}.");
        english.AddTranslation("en", "NotEqualValidator", "{PropertyName} must not equal {ComparisonValue}.");
        english.AddTranslation("en", "InclusiveBetweenValidator", "{PropertyName} must be between {From} and {To}.");
        english.AddTranslation("en", "ExclusiveBetweenValidator", "{PropertyName} must be between {From} and {To} (exclusive).");
        english.AddTranslation("en", "EmailValidator", "{PropertyName} must be a valid email address.");
        english.AddTranslation("en", "RegularExpressionValidator", "{PropertyName} is not in the correct format.");
        english.AddTranslation("en", "PredicateValidator", "{PropertyName} is not valid.");
        english.AddTranslation("en", "AsyncPredicateValidator", "{PropertyName} is not valid.");
        english.AddTranslation("en", "CreditCardValidator", "{PropertyName} must be a valid credit card number.");
        english.AddTranslation("en", "ScalePrecisionValidator", "{PropertyName} must not have more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals.");
        english.AddTranslation("en", "EnumValidator", "{PropertyName} has a range of values which does not include {PropertyValue}.");
    }
}
