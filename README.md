# LWF Pact History Exporter

A BepInEx 5 mod for **Lazy Witch's Factory** that exports the pact history as PNG or JPEG images using the game's own pact-panel presentation.

> Work in progress. No release is available yet.

## 日本語概要

リザルト画面から開く「契約履歴」を、ゲーム内と同じパネル表示のまま PNG または JPEG 画像として保存する MOD です。

ローカル環境で動作確認中の開発版です。まだ利用可能なリリースはありません。

## Behavior

- Adds an `Export` action to the pact-history screen.
- Reuse the game's pact-history snapshots and pact-panel prefab.
- Arranges up to five rendered panels per row.
- Can show each pact's in-run acquisition time above its panel when it is available.
- Saves timestamped image files under `PactHistoryExports` in the game directory.
- Can instead save JPEG files with an adjustable quality and size target.
- Splits output into multiple PNG files when a single texture would exceed the runtime limit.

## Requirements

- Lazy Witch's Factory (Steam, Windows x64)
- BepInEx 5

## Installation

No release is available yet. When a release is available, installation will be:

1. Install the Windows x64 version of BepInEx 5 into the game directory (the folder containing `LazyWitchsFactory.exe`).
2. Copy the release DLL to `BepInEx/plugins/LwfPactHistoryExporter/`.
3. Start the game once and check `BepInEx/LogOutput.log` if the plugin does not load.

The MOD will not include BepInEx or any game files. Installation is performed at the user's own risk; make a save-data backup before testing MODs.

## Removal

1. Close the game.
2. Delete only `BepInEx/plugins/LwfPactHistoryExporter/`.
3. Optionally delete `PactHistoryExports/` in the game directory to remove exported images.

Do not delete `BepInEx/core/`, `winhttp.dll`, or other MOD folders when removing this MOD.

## Configuration

After the MOD has loaded once, BepInEx creates:

```text
<GameDir>/BepInEx/config/io.github.kusyua.lwf.pacthistoryexporter.cfg
```

| Section | Setting | Default | Description |
| --- | --- | --- | --- |
| `Display` | `IncludePactTimestamps` | `true` | Shows each pact's in-run acquisition time above its panel. Set to `false` to hide the per-pact timestamps. |
| `Output` | `Format` | `png` | Output format: `png` for lossless images, or `jpg`/`jpeg` for smaller lossy files. Any other value produces PNG. |
| `Output` | `JpegQuality` | `90` | Initial JPEG quality (1–100). Used only when `Format` is JPEG. |
| `Output` | `JpegTargetSizeMiB` | `8` | JPEG size target in MiB. The exporter lowers quality in steps to 50 when needed. `0` disables the target; this is a target, not a guaranteed maximum. |
| `Debug` | `TestPanelCount` | `0` | Development test count. A positive value enables the test-export shortcut and repeats current pact snapshots up to this count. |
| `Debug` | `ExportShortcut` | `F8` | Shortcut for test export. It does nothing while `TestPanelCount` is `0`. |

Output names begin with `PactHistory_`; development test exports begin with `PactHistory_Test_`. If a single image would exceed the runtime texture limit, it is split into numbered parts.

## Development requirements

- .NET SDK 8 or later
- `ilspycmd` when inspecting the current game assembly

The current investigation is based on Unity `6000.0.80f1`, Mono, and BepInEx 5. Game updates may change private implementation details used by this mod.

## Local development setup

Copy the example machine-specific build settings:

```powershell
Copy-Item .\Directory.Build.user.props.example .\Directory.Build.user.props
```

Edit `GameDir` in `Directory.Build.user.props` so it points to the directory containing `LazyWitchsFactory.exe`.

`Directory.Build.user.props` is ignored by Git. Do not commit local paths, game assemblies, BepInEx binaries, decompiled game sources, logs, saves, or generated PNG files.

## Build

```powershell
dotnet build .\src\LwfPactHistoryExporter\LwfPactHistoryExporter.csproj -c Release
```

Output:

```text
src/LwfPactHistoryExporter/bin/Release/netstandard2.1/LwfPactHistoryExporter.dll
```

Copy the DLL to:

```text
<GameDir>/BepInEx/plugins/LwfPactHistoryExporter/
```

## Development test export

With at least one pact-history entry available, set `TestPanelCount` to a positive number in the `[Debug]` section of the BepInEx configuration file, then press the configured `ExportShortcut` (default: `F8`) in-game. The exporter repeats the current snapshots up to that number and writes files whose names include `_Test_`. It does not alter saves or pact history.

Keep `TestPanelCount = 0` for ordinary play; this is the default and disables the shortcut.

## Compatibility and support

- Plugin GUID: `io.github.kusyua.lwf.pacthistoryexporter`
- Target framework: `netstandard2.1`
- Tested with BepInEx `5.4.23.5`, Unity `6000.0.80f1`, and the Steam Windows x64 release of Lazy Witch's Factory on 2026-09-01.
- The game's pact-history one-screen display mode is compatible with the exporter as tested on that date.
- Per-pact timestamps are recorded in memory during the current run. An unavailable timestamp is displayed as `—`.
- Game updates can change private UI implementation details and may require a MOD update.
- This mod is intended only for people who own Lazy Witch's Factory.
- This project is not affiliated with or endorsed by the developer or publisher of Lazy Witch's Factory.
- This repository does not redistribute game assets or game assemblies.

See [the mod-policy notes](docs/MOD_POLICY.md) for the project rules derived from the official guidelines.

## Release process

Complete [the release checklist](docs/RELEASE_CHECKLIST.md), including selecting a license and verifying the game's mod-distribution policy, before distributing a release.

This source repository is intended to remain private. End-user releases will be distributed through Thunderstore after the export feature has been implemented and verified locally in the game.
