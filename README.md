# LWF Pact History Exporter

A BepInEx 5 mod for **Lazy Witch's Factory** that exports the pact history shown on the result screen as vertically arranged PNG images using the game's own pact-panel presentation.

> Work in progress. No release is available yet.

## 日本語概要

リザルト画面の「契約履歴」を、ゲーム内と同じパネル表示のまま縦に並べた PNG 画像として保存することを目標とする MOD です。

現在は公開可能なプロジェクト雛形と開発環境を整備している段階です。まだ利用可能なリリースはありません。

## Planned behavior

- Add an export action to the result screen.
- Reuse the game's pact-history snapshots and pact-panel prefab.
- Arrange the rendered panels vertically.
- Show the export date and time in the image header.
- Optionally show each pact's in-run acquisition time when it is available.
- Save timestamped PNG files under `PactHistoryExports` in the game directory.
- Split output into multiple PNG files when a single texture would exceed the runtime limit.

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

## Compatibility and support

- Plugin GUID: `io.github.kusyua.lwf.pacthistoryexporter`
- Target framework: `netstandard2.1`
- This mod is intended only for people who own Lazy Witch's Factory.
- This project is not affiliated with or endorsed by the developer or publisher of Lazy Witch's Factory.
- This repository does not redistribute game assets or game assemblies.

See [the mod-policy notes](docs/MOD_POLICY.md) for the project rules derived from the official guidelines.

## Release process

Complete [the release checklist](docs/RELEASE_CHECKLIST.md), including selecting a license and verifying the game's mod-distribution policy, before making a repository public or distributing a release.

This repository will remain private until the export feature has been implemented and verified locally in the game.
