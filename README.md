# MyRobot Mods

This repository contains independent BepInEx plugins for the Windows version of MyRobot.

[MyRobot on Steam](https://store.steampowered.com/app/3597510/MyRobot/)

| Mod | What it does |
| --- | --- |
| [English Fallback](src/MyRobot.EnglishFallback/README.md) | Shows the game's English text where a translation is missing. |
| [Font Fallback Fix](src/MyRobot.FontFallbackFix/README.md) | Uses fonts bundled with the game to help render missing characters that appear as squares. |

## Install

Install [BepInEx 6 Unity Mono for Windows x64](https://builds.bepinex.dev/projects/bepinex_be). The mods were developed against BepInEx 6.0.0-be.791.

Choose the combined ZIP to install both plugins, or use a standalone ZIP for one plugin. With the game closed, extract the ZIP into the folder containing `Robot.exe` and launch the game. Each plugin can be disabled independently. See its README for the exact DLL path and build instructions.

## Development and verification

OpenAI Codex was used to assist development. The mod author reviewed the changes and verification results, performed in-game testing, and is responsible for final release verification.

This package contains no game assets, extracted or decompiled game code, translations, fonts, launchers, or diagnostic tools.

## License

This mod is licensed under 0BSD (Zero-Clause BSD).

Game and third-party rights remain with their respective rights holders. Original game files, copied or decompiled game code, game assets, and separate third-party library binaries are not distributed here. Third-party components retain their own licenses.
