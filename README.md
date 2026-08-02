# Bolt Mod Manager

A mod manager for **any** game on Windows. Bolt never copies files over your installation: every
modification stays in its own folder and is linked into the game with symbolic links, so the
original files are always recoverable.

## How it works

```
<games root>/<Game name>/
├── Game.bltg          profiles and modification lists
├── Modifications/     one folder per imported package
└── Backups/           original game files replaced by a modification
```

Enabling a modification links its files into the game folder and moves the file it replaces into
`Backups`. Disabling it removes the link and puts the original file back. Because the modification
and backup folders are derived from the location of `Game.bltg`, a game folder can be moved or
renamed without breaking anything.

Elevation is only requested when Windows actually refuses an operation. With **Developer Mode**
enabled — or when the game lives outside a protected folder — everything runs without a UAC prompt.
The status bar shows which mode is in effect.

## Features

- Any game: point Bolt at a folder and an executable
- Multiple profiles per game, switched in one click
- Import `.zip`, `.7z`, and `.rar` packages through a single-pass extractor, processing up to two
  archives concurrently with total, remaining, and cancellation-safe progress reporting
- Edit a modification's name, description, version, category, and deployed file layout without
  rebuilding its archive
- Browse the modification as an expandable file tree, move or exclude whole selections, and open
  common text files in Notepad or a configured editor
- Enable, disable and delete modifications in bulk
- Restore every Bolt-managed game file and backup while leaving unknown game files untouched
- Conflict detection when two enabled modifications provide the same file
- Recent games, search, light and dark themes following the system

## Project layout

| Folder | Responsibility |
| --- | --- |
| `Core` | Domain models and abstractions. No UI, no file system. |
| `Infrastructure` | Storage, archives, symbolic links and the elevated helper. |
| `Services` | Application logic: session, game process, import, deployment. |
| `UI` | Theme, custom controls and windows. |

`Program.cs` is the only composition root; services are resolved through dependency injection and
never reach for a global provider.

## Requirements

- Windows 10 1703 or newer
- .NET 8 Desktop Runtime

## Building

```bash
dotnet build Bolt.sln
```

## User data

Preferences and the crash log live in `%APPDATA%\Bolt`.
