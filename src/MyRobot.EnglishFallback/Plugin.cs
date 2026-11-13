using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using Steamworks;

namespace MyRobot.EnglishFallback;

[BepInPlugin(Id, "MyRobot English Fallback", "1.0")]
[BepInProcess("Robot.exe")]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Id = "local.myrobot.englishfallback";
    private Harmony? harmony;
    private static Dictionary<UITerm, string> english = new Dictionary<UITerm, string>();

    private void Awake()
    {
        if (!Config.Bind("General", "Enabled", true, "Use English for missing UI/dialog translations. Restart required.").Value) return;
        try
        {
            english = FallbackData.Parse<UITerm>(File.ReadAllLines(Path.Combine(Application.dataPath, "Language", "english.lang")));
            Logger.LogInfo($"Loaded {english.Count} English UI entries from the installed game.");
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"English UI data unavailable; dialog fallback remains available: {exception.Message}");
        }
        harmony = new Harmony(Id);
        try
        {
            harmony.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo("English fallback enabled for missing/blank UI translations and dialog subtitles.");
        }
        catch (Exception exception)
        {
            harmony.UnpatchSelf();
            Logger.LogError(exception);
        }
    }

    private void OnDestroy() => harmony?.UnpatchSelf();

    [HarmonyPatch(typeof(Lexicon), "GetUITranslation")]
    private static class UiFallback
    {
        private static bool Prefix(UITerm line, Dictionary<UITerm, string> ___uiTerms, ref string __result)
        {
            // NULL is an intentional sentinel used by the game; preserve it.
            if (line == UITerm.NULL || !FallbackData.Missing(___uiTerms, line) ||
                !english.TryGetValue(line, out var text) || string.IsNullOrWhiteSpace(text)) return true;
            __result = text;
            return false;
        }
    }

    [HarmonyPatch(typeof(Lexicon), "GetDialogTranslation")]
    private static class DialogFallback
    {
        private static bool Prefix(Lexicon __instance, Dialog dialog,
            Dictionary<DialogTerm, string> ___dialogTerms, ref string __result)
        {
            if (!dialog || __instance.currentSystemLanguage == SystemLanguage.English) return true;
            if (Enum.TryParse<DialogTerm>(dialog.name, out var term) && !FallbackData.Missing(___dialogTerms, term)) return true;
            var voices = dialog.voiceLines;
            if (voices == null || voices.Length == 0) return true;
            var subtitles = new string[voices.Length];
            bool hasText = false;
            for (int i = 0; i < voices.Length; i++)
            {
                subtitles[i] = voices[i]?.subtitles ?? string.Empty;
                hasText |= !string.IsNullOrWhiteSpace(subtitles[i]);
            }
            if (!hasText) return true;
            // Keep empty slots and '*' separators to preserve voice-line indexing.
            __result = FallbackData.JoinSubtitles(subtitles);
            return false;
        }
    }
    [HarmonyPatch(typeof(AchievementsHUD), nameof(AchievementsHUD.RefreshAchievementList))]
    private static class AchievementFallback
    {
        private static void Postfix(Dictionary<AchievementSteamName, AchievementsHUD.EntryState> ___entryStates)
        {
            // Native code leaves the prefab's __NULL__ label when the UITerm enum is absent.
            // Only repair these entries; translated labels, icons and unlock state stay native.
            foreach (var pair in ___entryStates)
            {
                if (Enum.TryParse<UITerm>(pair.Key.ToString().ToLowerInvariant(), out _)) continue;
                var label = pair.Value?.entry?.text;
                if (label == null || !FallbackData.Placeholder(label.text)) continue;
                string title = pair.Key.ToString();
                if (SteamManager.Initialized)
                {
                    uint count = SteamUserStats.GetNumAchievements();
                    for (uint i = 0; i < count; i++)
                    {
                        string apiName = SteamUserStats.GetAchievementName(i);
                        if (FallbackData.AchievementKey(apiName) != pair.Key.ToString()) continue;
                        title = FallbackData.AchievementTitle(SteamUserStats.GetAchievementDisplayAttribute(apiName, "name"), apiName, pair.Key.ToString());
                        break;
                    }
                }
                label.text = title;
            }
        }
    }

}
