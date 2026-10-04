using Core.PersistenceLayer.Dynamics.Dynamic;

namespace Core.PersistenceLayer.Dynamics.Extensions;

/// <summary>What a client-supplied <see cref="DynamicQuery"/> may contain. Shared by request validation and <see cref="IQueryableDynamicFilterExtensions.ToDynamic{T}(IQueryable{T}, DynamicQuery, IReadOnlySet{string}?)"/>.</summary>
public static class DynamicQueryRules
{
    public const int MaxFilters = 20;
    public const int MaxSorts = 5;

    internal static readonly string[] Directions = ["asc", "desc"];
    internal static readonly string[] Logics = ["and", "or"];

    internal static readonly Dictionary<string, string> Operators = new()
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "isnull", "== null" },
        { "isnotnull", "!= null" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };

    /// <summary>Problems with the query, empty when it can be applied. With <paramref name="allowedFields"/>, every filter and sort field must be one of them (case-insensitive).</summary>
    public static IReadOnlyList<string> Validate(DynamicQuery? query, IReadOnlySet<string>? allowedFields)
    {
        List<string> errors = [];
        if (query is null)
        {
            return errors;
        }

        if (query.Filter is { } root)
        {
            IList<Filter> filters = IQueryableDynamicFilterExtensions.GetAllFilters(root);
            if (filters.Count > MaxFilters)
            {
                errors.Add($"A query can have at most {MaxFilters} filters.");
            }

            foreach (Filter filter in filters)
            {
                CheckField(filter.Field, allowedFields, errors);

                if (!Operators.ContainsKey(filter.Operator ?? string.Empty))
                {
                    errors.Add($"Unknown filter operator '{filter.Operator}'.");
                }

                if (filter.Logic is not null && !Logics.Contains(filter.Logic))
                {
                    errors.Add($"Unknown filter logic '{filter.Logic}'.");
                }
            }
        }

        if (query.Sort is { } sorts)
        {
            List<Sort> sortList = [.. sorts];
            if (sortList.Count > MaxSorts)
            {
                errors.Add($"A query can sort by at most {MaxSorts} fields.");
            }

            foreach (Sort sort in sortList)
            {
                CheckField(sort.Field, allowedFields, errors);

                if (!Directions.Contains(sort.Direction))
                {
                    errors.Add($"Unknown sort direction '{sort.Direction}'.");
                }
            }
        }

        return errors;
    }

    private static void CheckField(string? field, IReadOnlySet<string>? allowedFields, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            errors.Add("A filter or sort is missing its field.");
        }
        else if (allowedFields is not null && !allowedFields.Contains(field, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"'{field}' can't be used to filter or sort this list.");
        }
    }
}
