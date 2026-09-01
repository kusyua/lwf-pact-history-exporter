using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using GameState;
using TMPro;
using UI.PactHistory;
using UI.SelectableWindow.Cell;
using UnityEngine;
using UnityEngine.UI;

namespace LwfPactHistoryExporter
{
    internal static class PactHistoryExportService
    {
        private const string ExportDirectoryName = "PactHistoryExports";
        private const float DefaultCellWidth = 960f;
        private const float DefaultCellHeight = 520f;
        private const float TimestampHeight = 32f;
        private const float Padding = 24f;
        private const float CellSpacing = 16f;
        private const int PreferredColumnCount = 5;
        private const int CaptureLayer = 31;

        private static readonly Dictionary<PactCellSnapshot, double> PactElapsedTimes = new Dictionary<PactCellSnapshot, double>();

        private static PactHistoryExportRunner _runner;

        private static bool _isExporting;

        internal static void RecordPactTimestamp(PactCellSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            GameStateManager gameStateManager = GameStateManager.Instance;
            if ((bool)gameStateManager)
            {
                PactElapsedTimes[snapshot] = gameStateManager.GetElapsedGameplaySeconds();
            }
        }

        internal static void ClearPactTimestamps()
        {
            PactElapsedTimes.Clear();
        }

        internal static void RequestExport()
        {
            RequestExport(isTestExport: false, testPanelCount: 0);
        }

        internal static void RequestDebugExport()
        {
            int testPanelCount = PactHistoryExportSettings.DebugTestPanelCount?.Value ?? 0;
            if (testPanelCount <= 0)
            {
                return;
            }

            RequestExport(isTestExport: true, testPanelCount: testPanelCount);
        }

        private static void RequestExport(bool isTestExport, int testPanelCount)
        {
            if (_isExporting)
            {
                Plugin.Log?.LogWarning("Pact history export is already in progress.");
                return;
            }

            PactHistoryExportRunner runner = GetOrCreateRunner();
            if (runner == null)
            {
                Plugin.Log?.LogError("Could not create the pact history export runner.");
                return;
            }

            runner.StartExport(isTestExport, testPanelCount);
        }

        private static PactHistoryExportRunner GetOrCreateRunner()
        {
            if ((bool)_runner)
            {
                return _runner;
            }

            GameObject gameObject = new GameObject("LwfPactHistoryExporter");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            _runner = gameObject.AddComponent<PactHistoryExportRunner>();
            return _runner;
        }

        private sealed class PactHistoryExportRunner : MonoBehaviour
        {
            internal void StartExport(bool isTestExport, int testPanelCount)
            {
                StartCoroutine(ExportRoutine(isTestExport, testPanelCount));
            }

            private IEnumerator ExportRoutine(bool isTestExport, int testPanelCount)
            {
                _isExporting = true;
                try
                {
                    IReadOnlyList<PactCellSnapshot> snapshots = PactHistoryStore.GetSnapshots();
                    if (snapshots == null || snapshots.Count == 0)
                    {
                        Plugin.Log?.LogWarning("Pact history export skipped because there are no pact history entries.");
                        yield break;
                    }

                    if (isTestExport)
                    {
                        snapshots = CreateTestSnapshots(snapshots, testPanelCount);
                        Plugin.Log?.LogInfo($"Starting development test export with {snapshots.Count} pact panels.");
                    }

                    PactCellComponents template = FindCellTemplate();
                    if (!(bool)template)
                    {
                        Plugin.Log?.LogError("Pact history export failed because the game pact cell template was not found.");
                        yield break;
                    }

                    template.Initialize();
                    float cellWidth = ResolveCellWidth(template);
                    float cellHeight = ResolveCellHeight(template);
                    int maxTextureSize = SystemInfo.maxTextureSize;
                    if (cellWidth > maxTextureSize)
                    {
                        Plugin.Log?.LogError($"Pact history export failed because the pact panel width ({cellWidth}) exceeds the texture limit ({maxTextureSize}).");
                        yield break;
                    }

                    bool includePactTimestamps = PactHistoryExportSettings.IncludePactTimestamps != null && PactHistoryExportSettings.IncludePactTimestamps.Value;
                    bool exportAsJpeg = IsJpegOutput();
                    float entryHeight = cellHeight + (includePactTimestamps ? TimestampHeight : 0f);
                    int columnCount = Mathf.Min(PreferredColumnCount, snapshots.Count);
                    while (columnCount > 1 && GetPageWidth(cellWidth, columnCount) > maxTextureSize)
                    {
                        columnCount--;
                    }

                    if (GetPageWidth(cellWidth, columnCount) > maxTextureSize)
                    {
                        Plugin.Log?.LogError($"Pact history export failed because the image width ({GetPageWidth(cellWidth, columnCount)}) exceeds the texture limit ({maxTextureSize}).");
                        yield break;
                    }

                    int rowsPerPage = Mathf.Max(1, Mathf.FloorToInt((maxTextureSize - (Padding * 2f) + CellSpacing) / (entryHeight + CellSpacing)));
                    int entriesPerPage = Mathf.Max(1, columnCount * rowsPerPage);
                    DateTime exportedAt = DateTime.Now;
                    string exportDirectory = Path.Combine(Paths.GameRootPath, ExportDirectoryName);
                    Directory.CreateDirectory(exportDirectory);

                    int pageCount = Mathf.CeilToInt((float)snapshots.Count / entriesPerPage);
                    List<string> exportedPaths = new List<string>(pageCount);
                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        int firstEntryIndex = pageIndex * entriesPerPage;
                        int entryCount = Mathf.Min(entriesPerPage, snapshots.Count - firstEntryIndex);
                        string outputPath = GetOutputPath(exportDirectory, exportedAt, pageIndex, pageCount, exportAsJpeg, isTestExport);
                        yield return CapturePage(template, snapshots, firstEntryIndex, entryCount, columnCount, cellWidth, cellHeight, includePactTimestamps, exportAsJpeg, outputPath);
                        exportedPaths.Add(outputPath);
                    }

                    Plugin.Log?.LogInfo($"Exported {snapshots.Count} pact history panels to {exportedPaths.Count} image file(s): {exportDirectory}");
                }
                finally
                {
                    _isExporting = false;
                }
            }

            private IEnumerator CapturePage(PactCellComponents template, IReadOnlyList<PactCellSnapshot> snapshots, int firstEntryIndex, int entryCount, int columnCount, float cellWidth, float cellHeight, bool includePactTimestamps, bool exportAsJpeg, string outputPath)
            {
                float entryHeight = cellHeight + (includePactTimestamps ? TimestampHeight : 0f);
                int rowCount = Mathf.CeilToInt((float)entryCount / columnCount);
                float pageHeight = (Padding * 2f) + (rowCount * entryHeight) + ((rowCount - 1) * CellSpacing);
                int textureWidth = Mathf.CeilToInt(GetPageWidth(cellWidth, columnCount));
                int textureHeight = Mathf.CeilToInt(pageHeight);
                RenderTexture renderTexture = null;
                Texture2D texture = null;
                GameObject cameraObject = null;
                GameObject canvasObject = null;
                RenderTexture previousRenderTexture = RenderTexture.active;

                try
                {
                    renderTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32)
                    {
                        antiAliasing = 1
                    };
                    renderTexture.Create();

                    cameraObject = new GameObject("PactHistoryExportCamera");
                    Camera camera = cameraObject.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = Color.clear;
                    camera.cullingMask = 1 << CaptureLayer;
                    camera.orthographic = true;
                    camera.targetTexture = renderTexture;
                    camera.enabled = false;

                    canvasObject = new GameObject("PactHistoryExportCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                    SetLayerRecursively(canvasObject, CaptureLayer);
                    Canvas canvas = canvasObject.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1f;
                    canvas.pixelPerfect = true;
                    CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

                    TMP_FontAsset font = ResolveFont(template);
                    for (int index = 0; index < entryCount; index++)
                    {
                        PactCellSnapshot snapshot = snapshots[firstEntryIndex + index];
                        int column = index % columnCount;
                        int row = index / columnCount;
                        float x = Padding + (column * (cellWidth + CellSpacing));
                        float y = Padding + (row * (entryHeight + CellSpacing));
                        if (includePactTimestamps)
                        {
                            CreatePactTimestamp(canvas.transform, font, snapshot, x, y, cellWidth);
                            y += TimestampHeight;
                        }

                        PactCellComponents cell = UnityEngine.Object.Instantiate(template, canvas.transform);
                        SetLayerRecursively(cell.gameObject, CaptureLayer);
                        cell.Initialize();
                        cell.ApplySnapshot(snapshot);
                        RectTransform rectTransform = cell.transform as RectTransform;
                        if ((bool)rectTransform)
                        {
                            rectTransform.anchorMin = new Vector2(0f, 1f);
                            rectTransform.anchorMax = new Vector2(0f, 1f);
                            rectTransform.pivot = new Vector2(0f, 1f);
                            rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
                            rectTransform.anchoredPosition = new Vector2(x, -y);
                            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                        }
                    }

                    yield return new WaitForEndOfFrame();
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active = renderTexture;
                    texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                    texture.ReadPixels(new Rect(0f, 0f, textureWidth, textureHeight), 0, 0);
                    texture.Apply();
                    File.WriteAllBytes(outputPath, exportAsJpeg ? EncodeJpeg(texture) : texture.EncodeToPNG());
                    Plugin.Log?.LogInfo($"Pact history image written: {outputPath}");
                }
                finally
                {
                    RenderTexture.active = previousRenderTexture;
                    if ((bool)texture)
                    {
                        UnityEngine.Object.Destroy(texture);
                    }

                    if ((bool)canvasObject)
                    {
                        UnityEngine.Object.Destroy(canvasObject);
                    }

                    if ((bool)cameraObject)
                    {
                        UnityEngine.Object.Destroy(cameraObject);
                    }

                    if (renderTexture != null)
                    {
                        renderTexture.Release();
                        UnityEngine.Object.Destroy(renderTexture);
                    }
                }
            }
        }

        private static PactCellComponents FindCellTemplate()
        {
            PactHistoryUIController controller = UnityEngine.Object.FindFirstObjectByType<PactHistoryUIController>(FindObjectsInactive.Include);
            if (!(bool)controller)
            {
                return null;
            }

            return HarmonyLib.AccessTools.Field(typeof(PactHistoryUIController), "_cellTemplate")?.GetValue(controller) as PactCellComponents;
        }

        private static float ResolveCellWidth(PactCellComponents template)
        {
            RectTransform rectTransform = template.transform as RectTransform;
            float width = (bool)rectTransform ? rectTransform.rect.width : 0f;
            return width > 1f ? width : DefaultCellWidth;
        }

        private static float ResolveCellHeight(PactCellComponents template)
        {
            RectTransform rectTransform = template.transform as RectTransform;
            if (!(bool)rectTransform)
            {
                return DefaultCellHeight;
            }

            float preferredHeight = LayoutUtility.GetPreferredHeight(rectTransform);
            if (preferredHeight > 1f)
            {
                return preferredHeight;
            }

            return rectTransform.rect.height > 1f ? rectTransform.rect.height : DefaultCellHeight;
        }

        private static TMP_FontAsset ResolveFont(PactCellComponents template)
        {
            TextMeshProUGUI text = template.GetComponentInChildren<TextMeshProUGUI>(true);
            return (bool)text ? text.font : TMP_Settings.defaultFontAsset;
        }

        private static void CreatePactTimestamp(Transform parent, TMP_FontAsset font, PactCellSnapshot snapshot, float x, float y, float width)
        {
            string text = PactElapsedTimes.TryGetValue(snapshot, out double elapsedSeconds)
                ? FormatElapsedTime(elapsedSeconds)
                : "—";
            CreateText(parent, "PactTimestamp", font, text, new Vector2(x, -y), new Vector2(width, TimestampHeight), TextAlignmentOptions.Left);
        }

        private static string FormatElapsedTime(double elapsedSeconds)
        {
            TimeSpan elapsed = TimeSpan.FromSeconds(Math.Max(0d, elapsedSeconds));
            return elapsed.TotalHours >= 1d ? elapsed.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture) : elapsed.ToString("mm\\:ss", CultureInfo.InvariantCulture);
        }

        private static float GetPageWidth(float cellWidth, int columnCount)
        {
            return (Padding * 2f) + (columnCount * cellWidth) + ((columnCount - 1) * CellSpacing);
        }

        private static void CreateText(Transform parent, string name, TMP_FontAsset font, string text, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
        {
            if (font == null)
            {
                return;
            }

            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.layer = CaptureLayer;
            gameObject.transform.SetParent(parent, false);
            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            TextMeshProUGUI tmp = gameObject.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = 28f;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.text = text;
        }

        private static bool IsJpegOutput()
        {
            string format = PactHistoryExportSettings.OutputFormat?.Value;
            return string.Equals(format, "jpg", StringComparison.OrdinalIgnoreCase) || string.Equals(format, "jpeg", StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<PactCellSnapshot> CreateTestSnapshots(IReadOnlyList<PactCellSnapshot> snapshots, int requestedCount)
        {
            int count = Mathf.Max(1, requestedCount);
            List<PactCellSnapshot> testSnapshots = new List<PactCellSnapshot>(count);
            for (int index = 0; index < count; index++)
            {
                testSnapshots.Add(snapshots[index % snapshots.Count]);
            }

            return testSnapshots;
        }

        private static byte[] EncodeJpeg(Texture2D texture)
        {
            int quality = Mathf.Clamp(PactHistoryExportSettings.JpegQuality?.Value ?? 90, 1, 100);
            int targetSizeMiB = Mathf.Max(0, PactHistoryExportSettings.JpegTargetSizeMiB?.Value ?? 8);
            long targetBytes = (long)targetSizeMiB * 1024L * 1024L;
            byte[] encoded = texture.EncodeToJPG(quality);
            while (targetBytes > 0L && encoded.Length > targetBytes && quality > 50)
            {
                quality = Mathf.Max(50, quality - 5);
                encoded = texture.EncodeToJPG(quality);
            }

            if (targetBytes > 0L && encoded.Length > targetBytes)
            {
                Plugin.Log?.LogWarning($"JPEG export is {encoded.Length / (1024f * 1024f):F1} MiB, above the configured {targetSizeMiB} MiB target at JPEG quality {quality}.");
            }

            return encoded;
        }

        private static string GetOutputPath(string exportDirectory, DateTime exportedAt, int pageIndex, int pageCount, bool exportAsJpeg, bool isTestExport)
        {
            string name = $"PactHistory{(isTestExport ? "_Test" : string.Empty)}_{exportedAt:yyyy-MM-dd_HHmmss}";
            if (pageCount > 1)
            {
                name += $"_part{pageIndex + 1:D2}of{pageCount:D2}";
            }

            return Path.Combine(exportDirectory, name + (exportAsJpeg ? ".jpg" : ".png"));
        }

        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
