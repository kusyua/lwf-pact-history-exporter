using System;
using HarmonyLib;
using UI;
using UI.PactHistory;
using UI.SelectableWindow.Cell;
using UI.SelectableWindow.ISelectableWindows;
using UI.SelectableWindow.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LwfPactHistoryExporter.Patches
{
    [HarmonyPatch(typeof(PactHistoryUIController))]
    internal static class PactHistoryExportButtonPatch
    {
        private const string ExportButtonName = "LwfPactHistoryExportButton";

        [HarmonyPostfix]
        [HarmonyPatch("Open", new[] { typeof(InGameWindowPresentationContext), typeof(Transform), typeof(Action) })]
        private static void AddExportButton(PactHistoryUIController __instance)
        {
            if ((bool)FindExistingExportButton(__instance.transform))
            {
                return;
            }

            Button backButton = AccessTools.Field(typeof(PactHistoryUIController), "_backToFactoryButton")?.GetValue(__instance) as Button;
            RectTransform backButtonRect = backButton?.transform as RectTransform;
            if (!(bool)backButtonRect || !(bool)backButtonRect.parent)
            {
                Plugin.Log?.LogWarning("Could not add the pact history export button because the back button layout was not found.");
                return;
            }

            CellComponents template = FindResultButtonTemplate();
            if (!(bool)template)
            {
                Plugin.Log?.LogWarning("Could not add the pact history export button because the result button template was not found.");
                return;
            }

            CellComponents exportButton = UnityEngine.Object.Instantiate(template, backButtonRect.parent);
            exportButton.name = ExportButtonName;
            exportButton.gameObject.SetActive(true);
            exportButton.SetText("Export");
            exportButton.AssignAction(PactHistoryExportService.RequestExport);
            exportButton.HandleSelectedEffect(false);

            PlaceBesideBackButton(exportButton.transform as RectTransform, backButtonRect);
        }

        private static void PlaceBesideBackButton(RectTransform exportButtonRect, RectTransform backButtonRect)
        {
            if (!(bool)exportButtonRect)
            {
                return;
            }

            const float spacing = 16f;
            exportButtonRect.anchorMin = backButtonRect.anchorMin;
            exportButtonRect.anchorMax = backButtonRect.anchorMax;
            exportButtonRect.pivot = backButtonRect.pivot;
            exportButtonRect.localScale = backButtonRect.localScale;
            float exportButtonWidth = Mathf.Max(180f, backButtonRect.rect.width * 1.8f);
            exportButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, exportButtonWidth);
            exportButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backButtonRect.rect.height);
            exportButtonRect.anchoredPosition = backButtonRect.anchoredPosition - new Vector2(((backButtonRect.rect.width + exportButtonWidth) / 2f) + spacing, 0f);

            TMPro.TextMeshProUGUI label = exportButtonRect.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if ((bool)label)
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = 16f;
                label.fontSizeMax = Mathf.Max(16f, label.fontSize);
            }
        }

        private static Transform FindExistingExportButton(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == ExportButtonName)
                {
                    return child;
                }
            }

            return null;
        }

        private static CellComponents FindResultButtonTemplate()
        {
            ResultUIManager manager = UnityEngine.Object.FindFirstObjectByType<ResultUIManager>(FindObjectsInactive.Include);
            WindowWithCells window = manager?.GetWindow();
            return window == null
                ? null
                : AccessTools.Field(typeof(WindowWithCells), "_cellComponents")?.GetValue(window) as CellComponents;
        }
    }
}
