using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.Mono;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyRobot.JapaneseFontFallback;

[BepInPlugin(PluginGuid, "MyRobot Font Fallback Fix", "1.0")]
[BepInProcess("Robot.exe")]
public sealed class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "local.myrobot.japanesefontfallback";
    private Type? lexiconType;
    private PropertyInfo? languageProperty;
    private MethodInfo? fontMethod;
    private EventInfo? languageEvent;
    private MonoBehaviour? lexicon;
    private List<TMP_FontAsset>? ownedList;
    private TMP_FontAsset? ownedFont;

    private void Awake()
    {
        if (!Config.Bind("General", "Enabled", true,
                "Enable font fallback for the selected game language. Restart after changing.").Value)
            return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ScheduleRefresh();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnbindLexicon();
        ScheduleRefresh();
    }

    private void ScheduleRefresh()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshWhenReady());
    }

    private IEnumerator RefreshWhenReady()
    {
        // Let the game's language listeners update their text before reparsing it.
        yield return null;
        for (int attempt = 0; attempt < 40; attempt++)
        {
            bool ready = false;
            try
            {
                ready = TryApply();
            }
            catch (Exception error)
            {
                if (RemoveOwnedFallback()) RefreshVisibleText();
                if (attempt == 0)
                    Logger.LogWarning("Language font fallback initialization: " + error.GetType().Name);
            }
            if (ready) yield break;
            yield return new WaitForSecondsRealtime(0.5f);
        }
        Logger.LogWarning("Language font fallback unavailable. The next scene or language change will retry.");
    }

    private bool TryBindLexicon()
    {
        if (lexicon) return true;
        UnbindLexicon();
        if (lexiconType == null)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != "Assembly-CSharp") continue;
                Type? type = assembly.GetType("Lexicon");
                if (type == null) return false;
                languageProperty = type.GetProperty("currentSystemLanguage");
                fontMethod = type.GetMethod("GetFontForSystemLanguage", new[] { typeof(SystemLanguage) });
                languageEvent = type.GetEvent("onLanguageChange");
                if (languageProperty?.PropertyType != typeof(SystemLanguage) ||
                    fontMethod?.ReturnType != typeof(TMP_FontAsset) ||
                    languageEvent?.EventHandlerType != typeof(Action))
                    return false;

                lexiconType = type;
                break;
            }
        }
        if (lexiconType == null) return false;

        MonoBehaviour? candidate = null;
        bool ambiguous = false;
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (var obj in UnityEngine.Object.FindObjectsByType(lexiconType, FindObjectsSortMode.None))
        {
            if (obj is not MonoBehaviour instance) continue;
            if (instance.gameObject.scene == activeScene)
            {
                candidate = instance;
                ambiguous = false;
                break;
            }
            if (candidate) ambiguous = true;
            candidate = instance;
        }
        if (!candidate || ambiguous) return false;
        languageEvent!.AddEventHandler(candidate, (Action)ScheduleRefresh);
        lexicon = candidate;
        return true;
    }

    private bool TryApply()
    {
        if (!TryBindLexicon())
        {
            if (RemoveOwnedFallback()) RefreshVisibleText();
            return false;
        }

        var language = (SystemLanguage)languageProperty!.GetValue(lexicon)!;
        string? probe = language switch
        {
            SystemLanguage.Japanese => "日本語設定",
            SystemLanguage.Chinese => "窗口模式选择义",
            SystemLanguage.Korean => "전체화면창모드",
            SystemLanguage.Russian => "Оконныйрежим",
            _ => null
        };

        var font = probe == null ? null : (TMP_FontAsset?)fontMethod!.Invoke(lexicon, new object[] { language });
        List<TMP_FontAsset>? list = font ? TMP_Settings.fallbackFontAssets : null;
        if (font && ReferenceEquals(ownedFont, font) && ReferenceEquals(ownedList, list) &&
            list!.Contains(font!))
            return true;

        bool changed = RemoveOwnedFallback();
        try
        {
            if (probe == null) return true;
            if (!font || list == null || !font!.sourceFontFile ||
                font.atlasPopulationMode != AtlasPopulationMode.Dynamic)
                return false;

            if (list.Contains(font)) return true;
            font.TryAddCharacters(probe, out string _, false);
            if (!font.HasCharacters(probe, out uint[] _, false, false)) return false;

            list.Add(font);
            ownedList = list;
            ownedFont = font;
            changed = true;
            Logger.LogInfo($"Language font fallback registered: {language} ({font.name}).");
            return true;
        }
        finally
        {
            if (changed) RefreshVisibleText();
        }
    }

    private bool RemoveOwnedFallback()
    {
        bool removed = ownedList != null && ownedList.Remove(ownedFont!);
        ownedList = null;
        ownedFont = null;
        return removed;
    }

    private void RefreshVisibleText()
    {
        int failures = 0;
        foreach (TMP_Text text in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (!text || !text.isActiveAndEnabled || !text.gameObject.scene.IsValid()) continue;
            try { text.ForceMeshUpdate(false, true); }
            catch (Exception) { failures++; }
        }
        if (failures != 0)
            Logger.LogWarning($"Language font fallback: {failures} text objects could not refresh.");
    }

    private void UnbindLexicon()
    {
        if (!ReferenceEquals(lexicon, null))
            languageEvent!.RemoveEventHandler(lexicon, (Action)ScheduleRefresh);
        lexicon = null;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        try { UnbindLexicon(); }
        finally { RemoveOwnedFallback(); }
    }
}
