using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HitMe.UI
{
    /// <summary>
    /// Q4 Round 36: Debug panel activated by ?debug=1 in URL.
    /// Shows build info, FPS, screen size, last 50 log lines, feature flags, and copy button.
    /// Catches unhandled exceptions and shows friendly Vietnamese error with error code + commit.
    /// Zero allocation per frame when hidden. Not active in Release unless ?debug=1.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private static DebugOverlay _instance;
        public static DebugOverlay Instance => _instance;

        private bool _visible;
        private bool _debugEnabled;
        private Canvas _canvas;
        private GameObject _panelRoot;
        private TextMeshProUGUI _infoText;
        private TextMeshProUGUI _logText;
        private Button _btnCopy;
        private Button _btnClose;

        // Circular log buffer (last 50 lines)
        private const int MaxLogLines = 50;
        private readonly string[] _logBuffer = new string[MaxLogLines];
        private int _logHead;
        private int _logCount;

        // FPS tracking
        private float _fpsTimer;
        private int _fpsFrameCount;
        private float _currentFps;
        private static readonly StringBuilder _sb = new StringBuilder(2048);

        // Error overlay
        private GameObject _errorOverlay;
        private TextMeshProUGUI _errorText;

        // Build commit (injected at build time or read from index.html)
        private string _buildCommit = "unknown";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            _debugEnabled = IsDebugRequested();

            if (_debugEnabled)
            {
                Application.logMessageReceived += OnLogMessage;
                Application.logMessageReceivedThreaded += OnLogMessageThreaded;
                BuildUI();
            }

            // Always register exception handler (friendly error in Vietnamese)
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessage;
            Application.logMessageReceivedThreaded -= OnLogMessageThreaded;
        }

        private void Update()
        {
            if (!_debugEnabled || !_visible) return;

            _fpsFrameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 1f)
            {
                _currentFps = _fpsFrameCount / _fpsTimer;
                _fpsFrameCount = 0;
                _fpsTimer = 0f;
                RefreshInfoText();
            }
        }

        /// <summary>
        /// Check URL query for ?debug=1 (WebGL) or command-line args
        /// </summary>
        private bool IsDebugRequested()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string url = Application.absoluteURL ?? "";
            return url.Contains("debug=1");
#else
            // In editor or standalone, check command line
            string[] args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.Contains("debug=1") || arg.Contains("-debug")) return true;
            }
            return Application.isEditor;
#endif
        }

        private void BuildUI()
        {
            // Create canvas
            var canvasGo = new GameObject("DebugCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Panel root (semi-transparent dark background)
            _panelRoot = new GameObject("DebugPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.05f);
            panelRect.anchorMax = new Vector2(0.98f, 0.95f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImg = _panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.02f, 0.02f, 0.06f, 0.92f);

            // Info text (top half)
            var infoGo = new GameObject("InfoText");
            infoGo.transform.SetParent(_panelRoot.transform, false);
            _infoText = infoGo.AddComponent<TextMeshProUGUI>();
            _infoText.fontSize = 18;
            _infoText.color = new Color(0.85f, 1f, 0.85f, 1f);
            _infoText.alignment = TextAlignmentOptions.TopLeft;
            _infoText.enableWordWrapping = true;
            _infoText.overflowMode = TextOverflowModes.Truncate;
            var infoRect = _infoText.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.02f, 0.55f);
            infoRect.anchorMax = new Vector2(0.98f, 0.98f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            // Log text (bottom half)
            var logGo = new GameObject("LogText");
            logGo.transform.SetParent(_panelRoot.transform, false);
            _logText = logGo.AddComponent<TextMeshProUGUI>();
            _logText.fontSize = 14;
            _logText.color = new Color(0.80f, 0.80f, 0.80f, 1f);
            _logText.alignment = TextAlignmentOptions.BottomLeft;
            _logText.enableWordWrapping = true;
            _logText.overflowMode = TextOverflowModes.Truncate;
            var logRect = _logText.GetComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0.02f, 0.08f);
            logRect.anchorMax = new Vector2(0.98f, 0.53f);
            logRect.offsetMin = Vector2.zero;
            logRect.offsetMax = Vector2.zero;

            // Copy button
            var btnCopyGo = new GameObject("BtnCopy");
            btnCopyGo.transform.SetParent(_panelRoot.transform, false);
            var btnCopyRect = btnCopyGo.AddComponent<RectTransform>();
            btnCopyRect.anchorMin = new Vector2(0.02f, 0.01f);
            btnCopyRect.anchorMax = new Vector2(0.48f, 0.07f);
            btnCopyRect.offsetMin = Vector2.zero;
            btnCopyRect.offsetMax = Vector2.zero;
            var btnCopyImg = btnCopyGo.AddComponent<Image>();
            btnCopyImg.color = new Color(0.16f, 0.38f, 0.89f, 1f);
            _btnCopy = btnCopyGo.AddComponent<Button>();
            _btnCopy.onClick.AddListener(CopyLog);
            var btnCopyLabel = new GameObject("Label");
            btnCopyLabel.transform.SetParent(btnCopyGo.transform, false);
            var btnCopyTmp = btnCopyLabel.AddComponent<TextMeshProUGUI>();
            btnCopyTmp.text = "SAO CHÉP NHẬT KÝ";
            btnCopyTmp.fontSize = 18;
            btnCopyTmp.color = Color.white;
            btnCopyTmp.alignment = TextAlignmentOptions.Center;
            var btnCopyLabelRect = btnCopyTmp.GetComponent<RectTransform>();
            btnCopyLabelRect.anchorMin = Vector2.zero;
            btnCopyLabelRect.anchorMax = Vector2.one;
            btnCopyLabelRect.offsetMin = Vector2.zero;
            btnCopyLabelRect.offsetMax = Vector2.zero;

            // Close button
            var btnCloseGo = new GameObject("BtnClose");
            btnCloseGo.transform.SetParent(_panelRoot.transform, false);
            var btnCloseRect = btnCloseGo.AddComponent<RectTransform>();
            btnCloseRect.anchorMin = new Vector2(0.52f, 0.01f);
            btnCloseRect.anchorMax = new Vector2(0.98f, 0.07f);
            btnCloseRect.offsetMin = Vector2.zero;
            btnCloseRect.offsetMax = Vector2.zero;
            var btnCloseImg = btnCloseGo.AddComponent<Image>();
            btnCloseImg.color = new Color(0.80f, 0.17f, 0.17f, 1f);
            _btnClose = btnCloseGo.AddComponent<Button>();
            _btnClose.onClick.AddListener(ToggleVisibility);
            var btnCloseLabel = new GameObject("Label");
            btnCloseLabel.transform.SetParent(btnCloseGo.transform, false);
            var btnCloseTmp = btnCloseLabel.AddComponent<TextMeshProUGUI>();
            btnCloseTmp.text = "ĐÓNG";
            btnCloseTmp.fontSize = 18;
            btnCloseTmp.color = Color.white;
            btnCloseTmp.alignment = TextAlignmentOptions.Center;
            var btnCloseLabelRect = btnCloseTmp.GetComponent<RectTransform>();
            btnCloseLabelRect.anchorMin = Vector2.zero;
            btnCloseLabelRect.anchorMax = Vector2.one;
            btnCloseLabelRect.offsetMin = Vector2.zero;
            btnCloseLabelRect.offsetMax = Vector2.zero;

            _panelRoot.SetActive(false);
            _visible = false;

            // Read build commit from index.html or Application.version
            _buildCommit = Application.version;
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // Try to read from a global JS var if set
                _buildCommit = GetBuildCommitFromJS();
            }
            catch { }
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetBuildCommitFromJS();
#else
        private static string GetBuildCommitFromJS() => Application.version;
#endif

        /// <summary>
        /// Build error overlay for unhandled exceptions (friendly Vietnamese)
        /// </summary>
        private void BuildErrorOverlay()
        {
            if (_errorOverlay != null) return;

            var canvas = _canvas;
            if (canvas == null)
            {
                var canvasGo = new GameObject("ErrorCanvas");
                canvasGo.transform.SetParent(transform, false);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;
                canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            _errorOverlay = new GameObject("ErrorOverlay");
            _errorOverlay.transform.SetParent(canvas.transform, false);
            var rect = _errorOverlay.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.30f);
            rect.anchorMax = new Vector2(0.95f, 0.70f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var bg = _errorOverlay.AddComponent<Image>();
            bg.color = new Color(0.80f, 0.10f, 0.10f, 0.95f);

            var textGo = new GameObject("ErrorText");
            textGo.transform.SetParent(_errorOverlay.transform, false);
            _errorText = textGo.AddComponent<TextMeshProUGUI>();
            _errorText.fontSize = 22;
            _errorText.color = Color.white;
            _errorText.alignment = TextAlignmentOptions.Center;
            _errorText.enableWordWrapping = true;
            var textRect = _errorText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f);
            textRect.anchorMax = new Vector2(0.95f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var dismissGo = new GameObject("BtnDismiss");
            dismissGo.transform.SetParent(_errorOverlay.transform, false);
            var dismissRect = dismissGo.AddComponent<RectTransform>();
            dismissRect.anchorMin = new Vector2(0.3f, 0.02f);
            dismissRect.anchorMax = new Vector2(0.7f, 0.12f);
            dismissRect.offsetMin = Vector2.zero;
            dismissRect.offsetMax = Vector2.zero;
            dismissGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);
            dismissGo.AddComponent<Button>().onClick.AddListener(() => _errorOverlay.SetActive(false));
            var dismissLabel = new GameObject("Label");
            dismissLabel.transform.SetParent(dismissGo.transform, false);
            var dismissTmp = dismissLabel.AddComponent<TextMeshProUGUI>();
            dismissTmp.text = "ĐÓNG";
            dismissTmp.fontSize = 18;
            dismissTmp.color = Color.white;
            dismissTmp.alignment = TextAlignmentOptions.Center;
            var dismissLabelRect = dismissTmp.GetComponent<RectTransform>();
            dismissLabelRect.anchorMin = Vector2.zero;
            dismissLabelRect.anchorMax = Vector2.one;
            dismissLabelRect.offsetMin = Vector2.zero;
            dismissLabelRect.offsetMax = Vector2.zero;

            _errorOverlay.SetActive(false);
        }

        public void ToggleVisibility()
        {
            if (!_debugEnabled) return;
            _visible = !_visible;
            if (_panelRoot != null) _panelRoot.SetActive(_visible);
            if (_visible) RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshInfoText();
            RefreshLogText();
        }

        private void RefreshInfoText()
        {
            if (_infoText == null) return;
            _sb.Clear();
            _sb.AppendLine($"<b>BUILD:</b> {_buildCommit}");
            _sb.AppendLine($"<b>FPS:</b> {_currentFps:F1}");
            _sb.AppendLine($"<b>KHUNG:</b> {Screen.width}×{Screen.height} ({Screen.dpi:F0} DPI)");
            _sb.AppendLine($"<b>SAFE AREA:</b> {Screen.safeArea}");

            // Feature flags
            string reducedMotion = PlayerPrefs.GetString("HitMe_ReducedMotion", "0");
            string lang = PlayerPrefs.GetString("HitMe_Language", "vi");
            _sb.AppendLine($"<b>CỜ:</b> ReducedMotion={reducedMotion}, Lang={lang}");
            _sb.AppendLine($"<b>BỘ NHỚ:</b> {(GC.GetTotalMemory(false) / (1024f * 1024f)):F1} MB");
            _sb.AppendLine($"<b>THỜI GIAN:</b> {Time.realtimeSinceStartup:F0}s");

            _infoText.SetText(_sb);
        }

        private void RefreshLogText()
        {
            if (_logText == null) return;
            _sb.Clear();
            int start = (_logHead - _logCount + MaxLogLines) % MaxLogLines;
            for (int i = 0; i < _logCount; i++)
            {
                int idx = (start + i) % MaxLogLines;
                _sb.AppendLine(_logBuffer[idx]);
            }
            _logText.SetText(_sb);
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            AddLogLine(condition, type);

            // Catch exceptions and show friendly error
            if (type == LogType.Exception || type == LogType.Error)
            {
                ShowFriendlyError(condition);
            }
        }

        private void OnLogMessageThreaded(string condition, string stackTrace, LogType type)
        {
            // Thread-safe: just buffer the log line
            AddLogLine(condition, type);
        }

        private void AddLogLine(string condition, LogType type)
        {
            string prefix = type switch
            {
                LogType.Error => "<color=#FF4444>[LỖI]</color>",
                LogType.Exception => "<color=#FF2222>[NGOẠI LỆ]</color>",
                LogType.Warning => "<color=#FFAA00>[CẢNH BÁO]</color>",
                _ => "<color=#AAAAAA>[LOG]</color>"
            };

            string line = $"{prefix} {condition}";
            if (line.Length > 200) line = line.Substring(0, 200) + "...";

            _logBuffer[_logHead] = line;
            _logHead = (_logHead + 1) % MaxLogLines;
            if (_logCount < MaxLogLines) _logCount++;
        }

        private void CopyLog()
        {
            _sb.Clear();
            _sb.AppendLine($"=== HIT ME Debug Log ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===");
            _sb.AppendLine($"Build: {_buildCommit}");
            _sb.AppendLine($"Screen: {Screen.width}x{Screen.height} @ {Screen.dpi:F0} DPI");
            _sb.AppendLine($"FPS: {_currentFps:F1}");
            _sb.AppendLine($"Memory: {(GC.GetTotalMemory(false) / (1024f * 1024f)):F1} MB");
            _sb.AppendLine();

            int start = (_logHead - _logCount + MaxLogLines) % MaxLogLines;
            for (int i = 0; i < _logCount; i++)
            {
                int idx = (start + i) % MaxLogLines;
                // Strip rich text tags for clipboard
                string clean = System.Text.RegularExpressions.Regex.Replace(
                    _logBuffer[idx] ?? "", "<.*?>", "");
                _sb.AppendLine(clean);
            }

            GUIUtility.systemCopyBuffer = _sb.ToString();
            Debug.Log("[DebugOverlay] Đã sao chép nhật ký vào clipboard.");
        }

        private void ShowFriendlyError(string condition)
        {
            BuildErrorOverlay();
            if (_errorOverlay == null || _errorText == null) return;

            int errorCode = Mathf.Abs(condition.GetHashCode()) % 10000;
            _errorText.text = $"Rất tiếc, đã xảy ra lỗi.\n\n" +
                              $"Mã lỗi: E{errorCode:D4}\n" +
                              $"Phiên bản: {_buildCommit}\n\n" +
                              $"Vui lòng thử tải lại trang.\n" +
                              $"Nếu lỗi lặp lại, xin gửi mã lỗi cho nhóm phát triển.";
            _errorOverlay.SetActive(true);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowFriendlyError(ex.Message);
            }
        }
    }
}
