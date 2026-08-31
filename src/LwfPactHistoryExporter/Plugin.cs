using BepInEx;
using HarmonyLib;

namespace LwfPactHistoryExporter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "io.github.kusyua.lwf.pacthistoryexporter";
        public const string PluginName = "LWF Pact History Exporter";
        public const string PluginVersion = "0.1.0";

        private void Awake()
        {
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
            new Harmony(PluginGuid).PatchAll();
        }
    }
}
