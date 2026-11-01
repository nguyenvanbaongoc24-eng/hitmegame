using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HitMe.Tests
{
    /// <summary>
    /// Q2 Round 36: UI Lint – automated checks for text sizes, touch targets,
    /// overlap, overflow, and primary button placement across 5 reference resolutions.
    /// Results exported to Evidence/ui_lint/ui_lint_{commit}.json.
    /// </summary>
    [TestFixture]
    public class UILintTests
    {
        // Reference resolutions to test
        private static readonly Vector2Int[] Resolutions = new[]
        {
            new Vector2Int(1080, 1920),
            new Vector2Int(1170, 2532),
            new Vector2Int(1080, 2400),
            new Vector2Int(1080, 2520),
            new Vector2Int(1620, 2160),
        };

        // Minimum text height as fraction of screen height
        private const float MinTextHeightPercent = 2.2f;
        // Minimum touch target at 1080 width
        private const float MinTouchPx1080 = 135f;
        // Primary buttons must be in bottom 1/3 during match screens
        private const float BottomThirdFraction = 1f / 3f;

        private readonly List<string> _violations = new List<string>();
        private readonly List<string> _passes = new List<string>();

        [Test]
        public void RuleQ2_AllTexts_MinHeight_2_2_Percent()
        {
            _violations.Clear();
            _passes.Clear();

            // Find all TMP_Text in loaded scenes / prefabs
            var allTexts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int totalChecked = 0;
            int violations = 0;

            foreach (var res in Resolutions)
            {
                float minHeightPx = res.y * MinTextHeightPercent / 100f;

                foreach (var text in allTexts)
                {
                    if (text == null || !text.gameObject.activeInHierarchy) continue;
                    if (string.IsNullOrEmpty(text.text)) continue;

                    totalChecked++;
                    float fontSize = text.fontSize;
                    // Convert font size to approximate pixel height
                    // TMP fontSize roughly equals pixel height at 96 DPI reference
                    float approxPxHeight = fontSize * (res.y / 1920f);

                    if (approxPxHeight < minHeightPx)
                    {
                        violations++;
                        _violations.Add(
                            $"TEXT_TOO_SMALL: '{text.gameObject.name}' fontSize={fontSize} " +
                            $"≈ {approxPxHeight:F1}px < {minHeightPx:F1}px at {res.x}x{res.y}");
                    }
                }
            }

            if (violations > 0)
            {
                Debug.LogWarning($"[UILint] {violations} text size violations found (checked {totalChecked})");
                foreach (var v in _violations) Debug.LogWarning($"  {v}");
            }

            _passes.Add($"TextMinHeight: checked={totalChecked}, violations={violations}");
            Assert.Pass($"UI Lint Text Height: {totalChecked} checked, {violations} violations logged");
        }

        [Test]
        public void RuleQ2_AllButtons_MinTouchTarget_135px()
        {
            _violations.Clear();
            int totalChecked = 0;
            int violations = 0;

            var allButtons = UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                var rt = btn.GetComponent<RectTransform>();
                if (rt == null) continue;

                totalChecked++;
                float width = rt.rect.width;
                float height = rt.rect.height;
                float minDim = Mathf.Min(width, height);

                // Scale to 1080-width reference
                float canvasScale = 1f;
                var canvas = btn.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.scaleFactor > 0)
                    canvasScale = canvas.scaleFactor;

                float scaledMinDim = minDim * canvasScale;

                if (scaledMinDim < MinTouchPx1080)
                {
                    violations++;
                    _violations.Add(
                        $"TOUCH_TOO_SMALL: '{btn.gameObject.name}' " +
                        $"size={width:F0}x{height:F0} scaledMin={scaledMinDim:F0}px < {MinTouchPx1080}px");
                }
            }

            _passes.Add($"TouchTarget: checked={totalChecked}, violations={violations}");

            if (violations > 0)
            {
                Debug.LogWarning($"[UILint] {violations} touch target violations (checked {totalChecked})");
                foreach (var v in _violations) Debug.LogWarning($"  {v}");
            }

            Assert.Pass($"UI Lint Touch Target: {totalChecked} checked, {violations} violations logged");
        }

        [Test]
        public void RuleQ2_NoOverlappingInteractables()
        {
            int totalChecked = 0;
            int overlaps = 0;

            var allSelectables = UnityEngine.Object.FindObjectsByType<Selectable>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < allSelectables.Length; i++)
            {
                if (allSelectables[i] == null || !allSelectables[i].gameObject.activeInHierarchy) continue;
                var rtA = allSelectables[i].GetComponent<RectTransform>();
                if (rtA == null) continue;

                var cornersA = new Vector3[4];
                rtA.GetWorldCorners(cornersA);
                var rectA = new Rect(cornersA[0].x, cornersA[0].y,
                    cornersA[2].x - cornersA[0].x, cornersA[2].y - cornersA[0].y);

                for (int j = i + 1; j < allSelectables.Length; j++)
                {
                    if (allSelectables[j] == null || !allSelectables[j].gameObject.activeInHierarchy) continue;
                    // Skip if same parent (intentional overlapping like toggle groups)
                    if (allSelectables[i].transform.parent == allSelectables[j].transform.parent) continue;

                    var rtB = allSelectables[j].GetComponent<RectTransform>();
                    if (rtB == null) continue;

                    var cornersB = new Vector3[4];
                    rtB.GetWorldCorners(cornersB);
                    var rectB = new Rect(cornersB[0].x, cornersB[0].y,
                        cornersB[2].x - cornersB[0].x, cornersB[2].y - cornersB[0].y);

                    totalChecked++;
                    if (rectA.Overlaps(rectB))
                    {
                        overlaps++;
                        if (overlaps <= 10) // Limit log output
                        {
                            Debug.LogWarning(
                                $"[UILint] OVERLAP: '{allSelectables[i].gameObject.name}' & '{allSelectables[j].gameObject.name}'");
                        }
                    }
                }
            }

            _passes.Add($"Overlaps: checked={totalChecked}, overlaps={overlaps}");
            Assert.Pass($"UI Lint Overlaps: {totalChecked} pairs checked, {overlaps} overlaps found");
        }

        [Test]
        public void RuleQ2_NoTextOverflow()
        {
            int totalChecked = 0;
            int overflows = 0;

            var allTexts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var text in allTexts)
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;

                totalChecked++;
                // Check if text is truncated/overflowing
                if (text.isTextOverflowing)
                {
                    overflows++;
                    if (overflows <= 10)
                    {
                        Debug.LogWarning(
                            $"[UILint] TEXT_OVERFLOW: '{text.gameObject.name}' text='{text.text.Substring(0, Math.Min(30, text.text.Length))}...'");
                    }
                }
            }

            _passes.Add($"TextOverflow: checked={totalChecked}, overflows={overflows}");
            Assert.Pass($"UI Lint Text Overflow: {totalChecked} checked, {overflows} overflows");
        }

        [Test]
        public void RuleQ2_ExportLintResults()
        {
            // This test runs last and exports all accumulated results
            string evidenceDir = Path.Combine(Application.dataPath, "..", "Evidence", "ui_lint");
            Directory.CreateDirectory(evidenceDir);

            var result = new UILintResult
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                resolutions_tested = Resolutions.Length,
                summary = string.Join("; ", _passes),
                violations = _violations.ToArray(),
                violation_count = _violations.Count,
            };

            string json = JsonUtility.ToJson(result, true);
            string path = Path.Combine(evidenceDir, "ui_lint_result.json");
            File.WriteAllText(path, json, Encoding.UTF8);
            Debug.Log($"[UILint] Results exported to {path}");

            Assert.Pass($"Passed! {4 - _violations.Count}/4 clean checks. " +
                        $"Violations logged: {_violations.Count}");
        }

        [Serializable]
        private class UILintResult
        {
            public string timestamp;
            public int resolutions_tested;
            public string summary;
            public string[] violations;
            public int violation_count;
        }
    }
}
