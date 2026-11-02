namespace Core.PersistenceLayer.Dynamics.Dynamic;

/// <summary>The values <see cref="Filter.Operator"/> accepts; anything else is rejected as a bad request.</summary>
public static class FilterOperators
{
    public const string Equal = "eq";
    public const string NotEqual = "neq";
    public const string LessThan = "lt";
    public const string LessThanOrEqual = "lte";
    public const string GreaterThan = "gt";
    public const string GreaterThanOrEqual = "gte";
    public const string IsNull = "isnull";
    public const string IsNotNull = "isnotnull";
    public const string StartsWith = "startswith";
    public const string EndsWith = "endswith";
    public const string Contains = "contains";
    public const string DoesNotContain = "doesnotcontain";
}
