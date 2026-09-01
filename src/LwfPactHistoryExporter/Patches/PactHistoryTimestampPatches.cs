using HarmonyLib;
using UI.PactHistory;
using UI.SelectableWindow.Cell;

namespace LwfPactHistoryExporter.Patches
{
    [HarmonyPatch(typeof(PactHistoryStore), nameof(PactHistoryStore.Add))]
    internal static class PactHistoryStoreAddPatch
    {
        private static void Postfix(PactCellSnapshot snapshot)
        {
            PactHistoryExportService.RecordPactTimestamp(snapshot);
        }
    }

    [HarmonyPatch(typeof(PactHistoryStore), nameof(PactHistoryStore.Clear))]
    internal static class PactHistoryStoreClearPatch
    {
        private static void Prefix()
        {
            PactHistoryExportService.ClearPactTimestamps();
        }
    }
}
