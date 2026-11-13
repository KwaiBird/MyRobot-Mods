# MyRobot English Fallback

English Fallback displays existing English text when the selected language has no translation for an entry and English text is available. English may appear alongside text in your selected language.

The plugin covers UI text and dialogue subtitles and can fill placeholder names for affected achievements. In-game testing confirmed that a missing settings label was displayed in English.

## Install or disable

Install BepInEx 6 Unity Mono for Windows x64 and close MyRobot. Extract the English Fallback standalone ZIP or the combined ZIP into the folder containing `Robot.exe`. The English Fallback DLL goes here:

```text
BepInEx/plugins/MyRobot.EnglishFallback/MyRobot.EnglishFallback.dll
```

To disable it, close the game and move the DLL out of the plugins folder. You can also set `Enabled = false` under `[General]` in `BepInEx/config/local.myrobot.englishfallback.cfg` and restart the game.

## Build from source

From the repository root, run the following command in PowerShell. Replace the example paths with paths to your installed game and BepInEx assemblies.

```powershell
dotnet build src/MyRobot.EnglishFallback -c Release "-p:GameManagedDir=C:/Games/MyRobot/Robot_Data/Managed" "-p:BepInExCoreDir=C:/Games/MyRobot/BepInEx/core"
```

The DLL is written to `src/MyRobot.EnglishFallback/bin/Release/netstandard2.1/`.

## Development and verification

OpenAI Codex was used to assist development. The mod author reviewed the changes and verification results, performed in-game testing, and is responsible for final release verification.

This package contains no game assets, extracted or decompiled game code, translations, fonts, launchers, or diagnostic tools.

## License

This mod is licensed under 0BSD (Zero-Clause BSD).

Game and third-party rights remain with their respective rights holders. Original game files, copied or decompiled game code, game assets, and separate third-party library binaries are not distributed here. Third-party components retain their own licenses.
