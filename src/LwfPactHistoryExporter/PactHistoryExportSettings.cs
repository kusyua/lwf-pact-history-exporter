using BepInEx.Configuration;
using UnityEngine;

namespace LwfPactHistoryExporter
{
    internal static class PactHistoryExportSettings
    {
        internal static ConfigEntry<bool> IncludePactTimestamps { get; private set; }

        internal static ConfigEntry<string> OutputFormat { get; private set; }

        internal static ConfigEntry<int> JpegQuality { get; private set; }

        internal static ConfigEntry<int> JpegTargetSizeMiB { get; private set; }

        internal static ConfigEntry<int> DebugTestPanelCount { get; private set; }

        internal static ConfigEntry<KeyboardShortcut> DebugExportShortcut { get; private set; }

        internal static void Initialize(ConfigFile config)
        {
            IncludePactTimestamps = config.Bind(
                "Display",
                "IncludePactTimestamps",
                true,
                "Show each pact's in-run acquisition time above its panel in exported images.");

            OutputFormat = config.Bind(
                "Output",
                "Format",
                "png",
                "Image format: png (lossless) or jpg (smaller lossy files).");

            JpegQuality = config.Bind(
                "Output",
                "JpegQuality",
                90,
                "Initial JPEG quality from 1 to 100. Lower values make smaller files.");

            JpegTargetSizeMiB = config.Bind(
                "Output",
                "JpegTargetSizeMiB",
                8,
                "JPEG size target in MiB. The exporter lowers quality down to 50 when necessary; 0 disables the target.");

            DebugTestPanelCount = config.Bind(
                "Debug",
                "TestPanelCount",
                0,
                "Development-only test output count. Set above 0 to repeat the current pact snapshots up to this count; 0 disables the test shortcut.");

            DebugExportShortcut = config.Bind(
                "Debug",
                "ExportShortcut",
                new KeyboardShortcut(KeyCode.F8),
                "Development-only shortcut for test export. It does nothing while TestPanelCount is 0.");
        }
    }
}
