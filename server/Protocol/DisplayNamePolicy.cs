using System.Globalization;
using System.Text;

namespace HitMe.Server.Protocol;

public sealed class DisplayNamePolicy
{
    private readonly string[] blocked;
    public DisplayNamePolicy(IEnumerable<string>? blockedTerms = null) =>
        blocked = (blockedTerms ?? new[] { "admin", "moderator", "fuck" })
            .Where(term => !string.IsNullOrWhiteSpace(term)).Select(term => term.Trim().ToUpperInvariant()).ToArray();

    public static DisplayNamePolicy FromEnvironment(Func<string, string?> read) =>
        new((read("BANNED_NAME_TERMS") ?? "admin,moderator,fuck").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

    public bool IsAllowed(string? name)
    {
        if (name is null || name.Length is < 2 or > 24 || name != name.Trim()) return false;
        var normalized = name.Normalize(NormalizationForm.FormKC);
        if (normalized.Length is < 2 or > 24 || blocked.Any(term => normalized.ToUpperInvariant().Contains(term, StringComparison.Ordinal))) return false;
        foreach (var character in normalized)
        {
            var category = char.GetUnicodeCategory(character);
            if (category is not (UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or
                UnicodeCategory.TitlecaseLetter or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber or
                UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.SpaceSeparator))
                return false;
        }
        return true;
    }
}
