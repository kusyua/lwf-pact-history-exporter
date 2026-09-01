using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LwfPactHistoryExporter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "io.github.kusyua.lwf.pacthistoryexporter";
        public const string PluginName = "LWF Pact History Exporter";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            PactHistoryExportSettings.Initialize(Config);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
            new Harmony(PluginGuid).PatchAll();
        }

        private void Update()
        {
            if (PactHistoryExportSettings.DebugExportShortcut != null && PactHistoryExportSettings.DebugExportShortcut.Value.IsDown())
            {
                PactHistoryExportService.RequestDebugExport();
            }
        }
    }
}
