[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\LwfPactHistoryExporter\LwfPactHistoryExporter.csproj"
$manifestPath = Join-Path $repositoryRoot "thunderstore\manifest.json"
$readmePath = Join-Path $repositoryRoot "thunderstore\README.md"
$iconPath = Join-Path $repositoryRoot "thunderstore\icon.png"
$changelogPath = Join-Path $repositoryRoot "CHANGELOG.md"
$pluginPath = Join-Path $repositoryRoot "src\LwfPactHistoryExporter\Plugin.cs"

foreach ($path in @($projectPath, $manifestPath, $readmePath, $iconPath, $changelogPath, $pluginPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required package input is missing: $path"
    }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($manifest.name) -or [string]::IsNullOrWhiteSpace($manifest.version_number)) {
    throw "thunderstore/manifest.json must contain name and version_number."
}

$pluginSource = Get-Content -LiteralPath $pluginPath -Raw
$pluginVersionMatch = [regex]::Match($pluginSource, 'PluginVersion\s*=\s*"(?<version>[^"]+)"')
if (-not $pluginVersionMatch.Success) {
    throw "Could not find PluginVersion in $pluginPath"
}

if ($pluginVersionMatch.Groups["version"].Value -ne $manifest.version_number) {
    throw "PluginVersion ($($pluginVersionMatch.Groups["version"].Value)) does not match manifest version_number ($($manifest.version_number))."
}

& dotnet build $projectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed."
}

$dllName = "LwfPactHistoryExporter.dll"
$dllPath = Join-Path $repositoryRoot "src\LwfPactHistoryExporter\bin\$Configuration\netstandard2.1\$dllName"
if (-not (Test-Path -LiteralPath $dllPath)) {
    throw "Build output is missing: $dllPath"
}

$packageName = "$($manifest.name)-$($manifest.version_number)"
$distDirectory = Join-Path $repositoryRoot "dist"
$packageDirectory = Join-Path $distDirectory $packageName
$zipPath = Join-Path $distDirectory "$packageName.zip"

if (Test-Path -LiteralPath $packageDirectory) {
    Remove-Item -LiteralPath $packageDirectory -Recurse -Force
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Path (Join-Path $packageDirectory "LwfPactHistoryExporter") -Force | Out-Null
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $packageDirectory "manifest.json")
Copy-Item -LiteralPath $readmePath -Destination (Join-Path $packageDirectory "README.md")
Copy-Item -LiteralPath $iconPath -Destination (Join-Path $packageDirectory "icon.png")
Copy-Item -LiteralPath $changelogPath -Destination (Join-Path $packageDirectory "CHANGELOG.md")
Copy-Item -LiteralPath $dllPath -Destination (Join-Path $packageDirectory "LwfPactHistoryExporter\$dllName")

Compress-Archive -Path (Join-Path $packageDirectory "*") -DestinationPath $zipPath -CompressionLevel Optimal

$expectedEntries = @(
    "manifest.json",
    "README.md",
    "icon.png",
    "CHANGELOG.md",
    "LwfPactHistoryExporter/$dllName"
)
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entryNames = @($archive.Entries | ForEach-Object { $_.FullName -replace "\\", "/" })
    foreach ($entry in $expectedEntries) {
        if ($entry -notin $entryNames) {
            throw "Package archive is missing: $entry"
        }
    }
}
finally {
    $archive.Dispose()
}

Write-Output "Thunderstore package created: $zipPath"
Write-Output "Mod Manager payload: BepInEx/plugins/LwfPactHistoryExporter/$dllName"
