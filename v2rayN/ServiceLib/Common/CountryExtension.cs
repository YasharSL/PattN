namespace ServiceLib.Common;

/// <summary>
/// Extension methods for country code utilities
/// </summary>
public static class CountryExtension
{
    /// <summary>
    /// Converts an ISO 3166-1 alpha-2 code to a flag emoji.
    /// Example: "US" -> "🇺🇸", "NL" -> "🇳🇱"
    /// </summary>
    public static string? CountryToEmoji(this string? countryCode)
    {
        if (countryCode is not { Length: 2 })
        {
            return null;
        }

        var upper = countryCode.ToUpperInvariant();
        var first = upper[0];
        var second = upper[1];
        if (first is < 'A' or > 'Z' || second is < 'A' or > 'Z')
        {
            return null;
        }

        const int regionalIndicatorA = 0x1F1E6;
        return string.Concat(
            char.ConvertFromUtf32(regionalIndicatorA + (first - 'A')),
            char.ConvertFromUtf32(regionalIndicatorA + (second - 'A')));
    }
}
