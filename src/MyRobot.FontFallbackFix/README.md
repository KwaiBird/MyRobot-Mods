# MyRobot Font Fallback Fix

Font Fallback Fix uses fonts already included with MyRobot to help display missing characters that appear as squares. It selects a fallback font for Japanese, Simplified Chinese, Korean, or Russian based on the in-game language setting.

The fix was verified in the settings menu for Japanese, Simplified Chinese, and Korean. Russian fallback font registration was verified, and the unmodded Russian settings menu did not reproduce missing characters during testing.

## Install or disable

Install BepInEx 6 Unity Mono for Windows x64 and close MyRobot. Extract the Font Fallback Fix standalone ZIP or the combined ZIP into the folder containing `Robot.exe`. The Font Fallback Fix DLL goes here:

```text
BepInEx/plugins/MyRobot.FontFallbackFix/MyRobot.FontFallbackFix.dll
```

To disable it, close the game and move the DLL out of the plugins folder. You can also set `Enabled = false` under `[General]` in `BepInEx/config/local.myrobot.japanesefontfallback.cfg` and restart the game.

## Build from source

From the repository root, run the following command in PowerShell. Replace the example paths with paths to your installed game and BepInEx assemblies.

```powershell
dotnet build src/MyRobot.FontFallbackFix -c Release "-p:GameManagedDir=C:/Games/MyRobot/Robot_Data/Managed" "-p:BepInExCoreDir=C:/Games/MyRobot/BepInEx/core"
```

The DLL is written to `src/MyRobot.FontFallbackFix/bin/Release/netstandard2.1/`.

## Development and verification

OpenAI Codex was used to assist development. The mod author reviewed the changes and verification results, performed in-game testing, and is responsible for final release verification.

This package contains no game assets, extracted or decompiled game code, translations, fonts, launchers, or diagnostic tools.

## License

This mod is licensed under 0BSD (Zero-Clause BSD).

Game and third-party rights remain with their respective rights holders. Original game files, copied or decompiled game code, game assets, and separate third-party library binaries are not distributed here. Third-party components retain their own licenses.
