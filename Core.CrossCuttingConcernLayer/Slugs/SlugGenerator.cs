using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.CrossCuttingConcernLayer.Slugs;

public static class SlugGenerator
{
    public const int DefaultMaxLength = 80;

    private const int _suffixReserve = 4;
    private const char _turkishDotlessI = 'ı';
    private static readonly Regex NonAlphanumeric = new("[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Lowercase ASCII words joined by dashes ("Duct Cleaning İzmir" → "duct-cleaning-izmir"), cut at a word boundary.</summary>
    public static string Slugify(string value, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Replace(_turkishDotlessI, 'i').Normalize(NormalizationForm.FormKD);
        StringBuilder withoutDiacritics = new(normalized.Length);

        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                _ = withoutDiacritics.Append(character);
            }
        }

        string slug = NonAlphanumeric.Replace(withoutDiacritics.ToString().ToLowerInvariant(), "-").Trim('-');
        return Truncate(slug, maxLength);
    }

    /// <summary>
    /// A slug no existing row uses, with one lookup: <paramref name="takenStartingWith"/> gets a prefix and returns the slugs already stored that
    /// start with it (e.g. Where(x =&gt; x.Slug.StartsWith(prefix)).Select(x =&gt; x.Slug)). Collisions get -1, -2, …; text without letters or
    /// digits gets a random slug.
    /// </summary>
    public static async Task<string> GenerateUniqueAsync(string value, Func<string, Task<IReadOnlyCollection<string>>> takenStartingWith, int maxLength = DefaultMaxLength)
    {
        string baseSlug = Slugify(value, maxLength);
        if (baseSlug.Length == 0)
        {
            return Guid.NewGuid().ToString("N")[..Math.Min(12, maxLength)];
        }

        string stem = baseSlug.Length > maxLength - _suffixReserve ? Truncate(baseSlug, Math.Max(1, maxLength - _suffixReserve)) : baseSlug;
        HashSet<string> taken = new(await takenStartingWith(stem), StringComparer.OrdinalIgnoreCase);

        if (!taken.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (int suffix = 1; suffix <= taken.Count + 1; suffix++)
        {
            string candidate = $"{stem}-{suffix}";
            if (candidate.Length > maxLength)
            {
                break;
            }

            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }

        string guid = Guid.NewGuid().ToString("N");
        string prefix = Truncate(stem, Math.Max(0, maxLength - guid.Length - 1));
        return prefix.Length == 0 ? guid[..Math.Min(guid.Length, maxLength)] : $"{prefix}-{guid}";
    }

    private static string Truncate(string slug, int maxLength)
    {
        if (slug.Length <= maxLength)
        {
            return slug;
        }

        string truncated = slug[..maxLength];
        int lastDash = truncated.LastIndexOf('-');

        return lastDash > maxLength / 2 ? truncated[..lastDash] : truncated.TrimEnd('-');
    }
}
