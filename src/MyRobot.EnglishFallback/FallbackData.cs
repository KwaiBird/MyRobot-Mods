using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyRobot.EnglishFallback;

internal static class FallbackData
{
    internal static bool Missing<T>(Dictionary<T, string> translations, T key) where T : notnull
        => !translations.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value);

    // Match the game's trim, first '=' split, case-sensitive enum and newline rules.
    internal static Dictionary<T, string> Parse<T>(IEnumerable<string> lines) where T : struct, Enum
    {
        var result = new Dictionary<T, string>();
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
            int split = line.IndexOf('=');
            if (split < 0 || !Enum.TryParse<T>(line.Substring(0, split).Trim(), out var key) || !Enum.IsDefined(typeof(T), key)) continue;
            result[key] = line.Substring(split + 1).Trim().Replace(@"\n", "\n");
        }
        return result;
    }

    internal static string JoinSubtitles(string[] lines) => string.Join("*", lines);
    internal static bool Placeholder(string? text)
        => string.IsNullOrWhiteSpace(text) || text == "NULL" || text == "__NULL__";

    // Same normalization as AchievementsHUD.GetAchievementSteamNameFromApiName.
    internal static string AchievementKey(string apiName)
        => Regex.Replace(apiName.ToUpperInvariant().Replace(" ", "_"), "[^a-zA-Z_]", "");

    internal static string AchievementTitle(string? displayName, string apiName, string identifier)
        => !Placeholder(displayName) ? displayName! : !Placeholder(apiName) ? apiName : identifier;

}
