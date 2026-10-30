using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HitMe.Core;
using HitMe.Audio;
using HitMe.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HitMe.UI
{
    public enum ProductionScreen
    {
        Home,
        CreateRoom,
        JoinRoom,
        Lobby,
        BotTraining,
        Gameplay,
        Result
    }

    public sealed class ProductionUIController : MonoBehaviour
    {
        private static readonly string[] CachedTenthsTimerStrings = BuildTenthsTimerCache(150);
        private static readonly string[] CachedRound5Strings =
        {
            "VÒNG 0/5",
            "VÒNG 1/5",
            "VÒNG 2/5",
            "VÒNG 3/5",
            "VÒNG 4/5",
            "VÒNG 5/5"
        };
        private static readonly string[] CachedHpCardStrings =
        {
            "HP: 0/3",
            "HP: 1/3",
            "HP: 2/3",
            "HP: 3/3"
        };
        private static readonly string[] CachedRankStrings =
        {
            "#1", "#2", "#3", "#4", "#5", "#6"
        };
        private static readonly string[] CachedResultHpStrings =
        {
            "0 HP", "1 HP", "2 HP", "3 HP"
        };

        private const string HpHiddenText = "HP: ???";
        private const string StatusHiddenText = "ĐANG ẨN";
        private const string StatusAliveText = "● CÒN SỐNG";
        private const string StatusEliminatedText = "✖ ĐÃ LOẠI";

        private static string[] BuildTenthsTimerCache(int maxTenths)
        {
            var table = new string[maxTenths + 1];
            for (int i = 0; i <= maxTenths; i++)
            {
                int whole = i / 10;
                int tenth = i % 10;
                table[i] = whole + "." + tenth + "s";
            }
            return table;
        }

        [Header("Controller Reference")]
        public MatchController matchController;

        [Header("Screen Panels")]
        public GameObject homePanel;
        public GameObject createRoomPanel;
        public GameObject joinRoomPanel;
        public GameObject lobbyPanel;
        public GameObject botTrainingPanel;
        public GameObject gameplayHudPanel;
        public GameObject resultPanel;

        [Header("Toast Notification")]
        public GameObject toastObject;
        public TextMeshProUGUI txtToast;

        [Header("Build Fingerprint Watermark")]
        public TextMeshProUGUI txtBuildFingerprint;

        [Header("Home Screen Controls")]
        public TextMeshProUGUI txtHomeLogo;
        public TextMeshProUGUI txtSelectedLoadout;
        public Button btnQuickMatch;
        public Button btnCreateRoom;
        public Button btnJoinRoom;
        public Button btnBotTraining;

        [Header("Create Room Controls")]
        public TMP_InputField inputRoomName;
        public TextMeshProUGUI txtPlayerCount;
        public TextMeshProUGUI txtPrivacySetting;
        public Button btnSubmitCreateRoom;
        public Button btnBackFromCreate;

        [Header("Join Room Controls")]
        public TMP_InputField inputRoomCode;
        public Button btnPasteCode;
        public Button btnScanQr;
        public GameObject roomFoundCard;
        public TextMeshProUGUI txtFoundRoomInfo;
        public Button btnSubmitJoinRoom;
        public Button btnBackFromJoin;

        [Header("Lobby Controls")]
        public TextMeshProUGUI txtLobbyHeader;
        public TextMeshProUGUI txtLobbyRoomCode;
        public Transform lobbySlotsContainer;
        public List<LobbySlotUI> lobbySlots = new List<LobbySlotUI>();
        public Button btnLobbyReady;
        public TextMeshProUGUI txtLobbyReadyLabel;
        public Button btnLeaveRoom;
        public TextMeshProUGUI txtLobbyCountdown;
        public Button btnLobbyToBotTraining;

        [Header("Bot Training Controls")]
        public TextMeshProUGUI txtBotTrainingHeader;
        public TextMeshProUGUI txtSelectedBotCount;
        public TextMeshProUGUI txtSelectedCharacter;
        public Button btnDecreaseBots;
        public Button btnIncreaseBots;
        public Button btnPrevChar;
        public Button btnNextChar;
        public Button btnStartBotTraining;
        public Button btnBackFromBotTraining;

        [Header("Gameplay HUD Controls")]
        public GameObject topPlacingBar;
        public GameObject roundPill;
        public GameObject timerPill;
        public GameObject phaseBannerObj;
        public GameObject weaponPillObj;
        public TextMeshProUGUI txtRound;
        public TextMeshProUGUI txtTimer;
        public TextMeshProUGUI txtPhaseBanner;
        public Button btnInGameReady;
        public TextMeshProUGUI txtInGameReadyLabel;
        public Transform hpCardsContainer;
        public List<PlayerHpCardUI> hpCards = new List<PlayerHpCardUI>();
        public TextMeshProUGUI txtWeaponIndicator;

        [Header("Result Controls")]
        public TextMeshProUGUI txtResultTitle;
        public TextMeshProUGUI txtResultSubtitle;
        public Transform resultRankingsContainer;
        public List<ResultRowUI> resultRows = new List<ResultRowUI>();
        public Button btnReplay;
        public Button btnBackToHome;

        public ProductionScreen CurrentScreen { get; private set; } = ProductionScreen.Home;

        private Coroutine _toastCoroutine;
        private Coroutine _lobbyCountdownCoroutine;
        private bool _isLocalPlayerReady;
        private string _activeRoomCode = "HM4826";
        private int _selectedBotCount = 3;
        private int _selectedPracticeCharIndex = 0;

        private GameObject _fpsOverlayObj;
        private TextMeshProUGUI _txtFps;
        private float _fpsUpdateTimer;
        private int _fpsAccumulatedFrames;
        private int _fpsCornerTaps;
        private float _fpsLastTapTime;
        private static readonly string[] CachedFpsStrings = InitFpsStrings();

        private static string[] InitFpsStrings()
        {
            var arr = new string[151];
            for (int i = 0; i <= 150; i++)
            {
                arr[i] = i + " FPS";
            }
            return arr;
        }

        private int _lastDisplayedRound = -1;
        private int _lastDisplayedMaxRounds = -1;
        private int _lastDisplayedTimerTenths = -1;
        private bool _lastTimerLowWarning;
        private Phase _lastDisplayedPhase = (Phase)(-1);
        private Phase _lastSyncedStatePhase = (Phase)(-1);
        private int _lastConfiguredHpCardCount = -1;
        private int _currentLobbyHumanCount = 4;
        private readonly List<PlayerState> _uiRankedBuffer = new List<PlayerState>(6);
        private readonly List<string> _uiWinnersBuffer = new List<string>(6);

        [Serializable]
        public class LobbySlotUI
        {
            public GameObject root;
            public TextMeshProUGUI txtName;
            public TextMeshProUGUI txtStatus;
            public Image imgAvatar;
            public bool isReady;
        }

        [Serializable]
        public class PlayerHpCardUI
        {
            public GameObject root;
            public Image imgBg;
            public TextMeshProUGUI txtName;
            public TextMeshProUGUI txtHearts;
            public TextMeshProUGUI txtHpNum;
            public string playerId;

            [NonSerialized] public string lastDisplayedName;
            [NonSerialized] public int lastDisplayedHpCode = -999; // -1 = hidden, 0..3 = hp
            [NonSerialized] public int lastDisplayedStatusCode = -999; // -1 = hidden, 0 = dead, 1 = alive
        }

        [Serializable]
        public class ResultRowUI
        {
            public GameObject root;
            public TextMeshProUGUI txtRank;
            public TextMeshProUGUI txtName;
            public TextMeshProUGUI txtHp;
            public TextMeshProUGUI txtStatus;
            public Image imgBg;
        }

        private void Awake()
        {
            if (matchController == null)
            {
                matchController = FindAnyObjectByType<MatchController>();
            }

            ResolveHudElementContainers();
            BindEvents();
        }

        public void ResolveHudElementContainers()
        {
            if (roundPill == null && txtRound != null && txtRound.transform.parent != null)
            {
                roundPill = txtRound.transform.parent.gameObject;
            }
            if (timerPill == null && txtTimer != null && txtTimer.transform.parent != null)
            {
                timerPill = txtTimer.transform.parent.gameObject;
            }
            if (phaseBannerObj == null && txtPhaseBanner != null && txtPhaseBanner.transform.parent != null)
            {
                phaseBannerObj = txtPhaseBanner.transform.parent.gameObject;
            }
            if (weaponPillObj == null && txtWeaponIndicator != null && txtWeaponIndicator.transform.parent != null)
            {
                weaponPillObj = txtWeaponIndicator.transform.parent.gameObject;
            }
        }

        private void Start()
        {
            ParseAndApplyUrlScales();

            if (matchController != null)
            {
                matchController.OnRoundStarted += HandleRoundStarted;
                matchController.OnPhaseChanged += HandlePhaseChanged;
                matchController.OnTimerTick += HandleTimerTick;
                matchController.OnToastRequested += ShowToast;
                matchController.OnMatchEnded += HandleMatchEnded;
            }

            EnsureAndRefreshBuildFingerprint();
            EnsureAndRefreshFpsOverlay();

            int practiceBots = GetUrlInt("practiceBots", -1);
            if (practiceBots < 0) practiceBots = GetUrlInt("bots", -1);

            if (IsUrlAutoPlay())
            {
                ShowScreen(ProductionScreen.Gameplay);
            }
            else if (practiceBots >= 1)
            {
                int charIdx = GetUrlInt("practiceChar", 0);
                EnsureBotTrainingPanel();
                StartPracticeWithBots(practiceBots, charIdx);
                if (GetUrlInt("autoReady", 0) == 1)
                {
                    StartCoroutine(AutoReadyCoroutine());
                }
            }
            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=botTraining", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EnsureBotTrainingPanel();
                ShowScreen(ProductionScreen.BotTraining);
            }
            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=lobby1p", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                UpdateLobbyRoster(1, "Phòng Chiến Hữu", "HM4826");
            }
            else
            {
                ShowScreen(ProductionScreen.Home);
            }
        }

        private void Update()
        {
            UpdateFpsOverlay();

            if (matchController == null || matchController.State == null) return;

            Phase statePhase = matchController.State.Phase;
            if (statePhase != _lastSyncedStatePhase ||
                (statePhase == Phase.MatchEnd && CurrentScreen == ProductionScreen.Gameplay && !IsUrlAutoPlay()))
            {
                ApplyPhaseState(statePhase);
            }
        }

        private static bool IsUrlAutoPlay()
        {
            return Application.absoluteURL != null &&
                   Application.absoluteURL.IndexOf("autoPlay=", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int GetUrlInt(string key, int defaultValue)
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return defaultValue;
            int idx = url.IndexOf(key + "=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return defaultValue;
            idx += key.Length + 1;
            int end = url.IndexOf('&', idx);
            string valStr = end > idx ? url.Substring(idx, end - idx) : url.Substring(idx);
            int res;
            return int.TryParse(valStr, out res) ? res : defaultValue;
        }

        private System.Collections.IEnumerator AutoReadyCoroutine()
        {
            yield return new WaitForSeconds(0.8f);
            if (btnInGameReady != null && btnInGameReady.interactable)
            {
                btnInGameReady.onClick.Invoke();
            }
        }

        private static void ParseAndApplyUrlScales()
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return;
            int idx = url.IndexOf("charScale=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                idx += "charScale=".Length;
                int end = url.IndexOf('&', idx);
                string valStr = end > idx ? url.Substring(idx, end - idx) : url.Substring(idx);
                float parsed;
                if (float.TryParse(valStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed) && parsed > 0.1f)
                {
                    PlayerView.SetVisualCharacterScale(parsed, true);
                    WeaponFlightView.SetVisualWeaponScale(parsed, true);
                    Debug.Log($"[ProductionUI] Applied charScale={parsed} from URL");
                }
            }
        }

        public void EnsureAndRefreshBuildFingerprint()
        {
            if (txtBuildFingerprint == null)
            {
                var existing = transform.Find("SafeAreaRoot/BuildFingerprintWatermark") ?? transform.Find("BuildFingerprintWatermark");
                if (existing != null)
                {
                    txtBuildFingerprint = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (txtBuildFingerprint == null)
            {
                Transform parent = transform.Find("SafeAreaRoot") ?? transform;
                var go = new GameObject("BuildFingerprintWatermark", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(24f, 16f);
                rt.sizeDelta = new Vector2(700f, 40f);

                txtBuildFingerprint = go.AddComponent<TextMeshProUGUI>();
                if (txtHomeLogo != null && txtHomeLogo.font != null)
                {
                    txtBuildFingerprint.font = txtHomeLogo.font;
                }
                else if (txtRound != null && txtRound.font != null)
                {
                    txtBuildFingerprint.font = txtRound.font;
                }
                txtBuildFingerprint.fontSize = 22f;
                txtBuildFingerprint.fontStyle = FontStyles.Bold;
                txtBuildFingerprint.color = new Color(1f, 0.92f, 0.23f, 0.95f);
                txtBuildFingerprint.alignment = TextAlignmentOptions.BottomLeft;
                txtBuildFingerprint.raycastTarget = false;
            }
            else
            {
                var rt = txtBuildFingerprint.rectTransform;
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(24f, 16f);
                rt.sizeDelta = new Vector2(700f, 40f);
                txtBuildFingerprint.fontSize = 22f;
                txtBuildFingerprint.fontStyle = FontStyles.Bold;
                txtBuildFingerprint.color = new Color(1f, 0.92f, 0.23f, 0.95f);
                txtBuildFingerprint.alignment = TextAlignmentOptions.BottomLeft;
            }

            txtBuildFingerprint.transform.SetAsLastSibling();
            string fingerprint = ResolveBuildFingerprint();
            txtBuildFingerprint.text = fingerprint;
            Debug.Log("[BuildFingerprint] " + fingerprint);
        }

        public static string ResolveBuildFingerprint()
        {
            var asset = Resources.Load<TextAsset>("BuildFingerprint");
            if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
            {
                return asset.text.Trim();
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "Graybox";
            string ver = string.IsNullOrEmpty(Application.version) ? "dev" : Application.version;
            return "build " + ver + " · " + sceneName;
        }

        public void EnsureAndRefreshFpsOverlay()
        {
            if (_fpsOverlayObj == null)
            {
                Transform parent = transform.Find("SafeAreaRoot") ?? transform;
                var existing = parent.Find("FpsOverlay");
                if (existing != null)
                {
                    _fpsOverlayObj = existing.gameObject;
                    _txtFps = existing.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            if (_fpsOverlayObj == null)
            {
                Transform parent = transform.Find("SafeAreaRoot") ?? transform;
                var go = new GameObject("FpsOverlay", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.70f, 0.94f);
                rt.anchorMax = new Vector2(0.98f, 0.98f);
                rt.pivot = new Vector2(1f, 1f);
                rt.sizeDelta = Vector2.zero;

                var txtObj = new GameObject("Text", typeof(RectTransform));
                txtObj.transform.SetParent(go.transform, false);
                var trt = txtObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.sizeDelta = Vector2.zero;

                _txtFps = txtObj.AddComponent<TextMeshProUGUI>();
                if (txtRound != null && txtRound.font != null) _txtFps.font = txtRound.font;
                _txtFps.fontSize = 14f;
                _txtFps.color = new Color(0.20f, 0.90f, 0.40f, 0.85f);
                _txtFps.alignment = TextAlignmentOptions.TopRight;
                _txtFps.raycastTarget = false;
                _txtFps.text = "60 FPS";

                var btnObj = new GameObject("TapDetector", typeof(RectTransform));
                btnObj.transform.SetParent(go.transform, false);
                var brt = btnObj.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.sizeDelta = Vector2.zero;
                var hitImg = btnObj.AddComponent<Image>();
                hitImg.color = new Color(0f, 0f, 0f, 0.001f);
                hitImg.raycastTarget = true;
                var btn = btnObj.AddComponent<Button>();
                btn.onClick.AddListener(OnFpsCornerTapped);

                _fpsOverlayObj = go;
            }

            _fpsOverlayObj.SetActive(true);
            _fpsOverlayObj.transform.SetAsLastSibling();
        }

        public void OnFpsCornerTapped()
        {
            float now = Time.unscaledTime;
            if (now - _fpsLastTapTime > 1.2f)
            {
                _fpsCornerTaps = 0;
            }
            _fpsLastTapTime = now;
            _fpsCornerTaps++;
            if (_fpsCornerTaps >= 3)
            {
                _fpsCornerTaps = 0;
                if (_fpsOverlayObj != null)
                {
                    _fpsOverlayObj.SetActive(!_fpsOverlayObj.activeSelf);
                }
            }
        }

        private void UpdateFpsOverlay()
        {
            if (_txtFps == null || _fpsOverlayObj == null || !_fpsOverlayObj.activeSelf) return;
            _fpsAccumulatedFrames++;
            _fpsUpdateTimer += Time.unscaledDeltaTime;
            if (_fpsUpdateTimer >= 0.6f)
            {
                int fps = Mathf.RoundToInt(_fpsAccumulatedFrames / _fpsUpdateTimer);
                if (fps < 0) fps = 0;
                else if (fps > 150) fps = 150;
                _txtFps.text = CachedFpsStrings[fps];
                _fpsAccumulatedFrames = 0;
                _fpsUpdateTimer = 0f;
            }
        }

        private void BindEvents()
        {
            if (btnQuickMatch != null)
            {
                btnQuickMatch.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    _activeRoomCode = "HM" + UnityEngine.Random.Range(1000, 9999);
                    EnterLobbyWithAutoFill("Phòng Chơi Nhanh", _activeRoomCode);
                });
            }

            if (btnCreateRoom != null)
            {
                btnCreateRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowScreen(ProductionScreen.CreateRoom);
                });
            }

            if (btnJoinRoom != null)
            {
                btnJoinRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowScreen(ProductionScreen.JoinRoom);
                });
            }

            if (btnBotTraining != null)
            {
                btnBotTraining.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    EnsureBotTrainingPanel();
                    ShowScreen(ProductionScreen.BotTraining);
                });
            }

            if (btnSubmitCreateRoom != null)
            {
                btnSubmitCreateRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    string roomName = inputRoomName != null && !string.IsNullOrEmpty(inputRoomName.text)
                        ? inputRoomName.text
                        : "Phòng Chiến Hữu";
                    _activeRoomCode = "HM" + UnityEngine.Random.Range(1000, 9999);
                    EnterLobbyWithAutoFill(roomName, _activeRoomCode);
                });
            }

            if (btnBackFromCreate != null)
            {
                btnBackFromCreate.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowScreen(ProductionScreen.Home);
                });
            }

            if (btnPasteCode != null)
            {
                btnPasteCode.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    if (inputRoomCode != null) inputRoomCode.text = "HM4826";
                    UpdateJoinRoomSearchStatus("HM4826");
                });
            }

            if (btnScanQr != null)
            {
                btnScanQr.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    if (inputRoomCode != null) inputRoomCode.text = "HM4826";
                    UpdateJoinRoomSearchStatus("HM4826");
                });
            }

            if (btnSubmitJoinRoom != null)
            {
                btnSubmitJoinRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    string code = inputRoomCode != null && !string.IsNullOrEmpty(inputRoomCode.text)
                        ? inputRoomCode.text.ToUpperInvariant()
                        : "HM4826";
                    EnterLobbyWithAutoFill("Phòng Chiến Hữu", code);
                });
            }

            if (btnBackFromJoin != null)
            {
                btnBackFromJoin.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowScreen(ProductionScreen.Home);
                });
            }

            if (btnLobbyReady != null)
            {
                btnLobbyReady.onClick.AddListener(ToggleLobbyReady);
            }

            if (btnLeaveRoom != null)
            {
                btnLeaveRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);
                    ShowScreen(ProductionScreen.Home);
                });
            }

            if (btnInGameReady != null)
            {
                btnInGameReady.onClick.AddListener(() =>
                {
                    if (matchController != null)
                    {
                        matchController.RequestReady();
                        if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = "ĐÃ KHÓA";
                        if (btnInGameReady != null) btnInGameReady.interactable = false;
                    }
                });
            }

            if (btnReplay != null)
            {
                btnReplay.onClick.AddListener(OnReplayClicked);
            }

            if (btnBackToHome != null)
            {
                btnBackToHome.onClick.AddListener(OnBackToHomeClicked);
            }
        }

        public void OnReplayClicked()
        {
            AudioManager.Instance?.PlayClick();
            StartGameplayMatch();
        }

        public void OnBackToHomeClicked()
        {
            AudioManager.Instance?.PlayClick();
            if (matchController != null)
            {
                matchController.StopAndCleanupMatch();
            }
            _lastDisplayedPhase = (Phase)(-1);
            _lastSyncedStatePhase = (Phase)(-1);
            ShowScreen(ProductionScreen.Home);
        }

        public void ShowScreen(ProductionScreen screen)
        {
            CurrentScreen = screen;

            if (homePanel != null) homePanel.SetActive(screen == ProductionScreen.Home);
            if (createRoomPanel != null) createRoomPanel.SetActive(screen == ProductionScreen.CreateRoom);
            if (joinRoomPanel != null) joinRoomPanel.SetActive(screen == ProductionScreen.JoinRoom);
            if (lobbyPanel != null) lobbyPanel.SetActive(screen == ProductionScreen.Lobby);
            if (botTrainingPanel != null) botTrainingPanel.SetActive(screen == ProductionScreen.BotTraining);
            if (gameplayHudPanel != null) gameplayHudPanel.SetActive(screen == ProductionScreen.Gameplay);
            if (resultPanel != null) resultPanel.SetActive(screen == ProductionScreen.Result);

            if (txtBuildFingerprint != null)
            {
                txtBuildFingerprint.transform.SetAsLastSibling();
            }

            if (screen == ProductionScreen.Home && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuBgm();
            }
        }

        public void ShowToast(string message)
        {
            ShowToast(message, 2.0f);
        }

        public void ShowToast(string message, float duration)
        {
            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(ToastRoutine(message, duration));
        }

        private IEnumerator ToastRoutine(string message, float duration)
        {
            if (toastObject != null)
            {
                if (txtToast != null) txtToast.text = message;
                toastObject.SetActive(true);
            }

            yield return new WaitForSeconds(duration);

            if (toastObject != null)
            {
                toastObject.SetActive(false);
            }
        }

        public void UpdateJoinRoomSearchStatus(string code)
        {
            if (roomFoundCard != null)
            {
                bool found = code.Length >= 6;
                roomFoundCard.SetActive(found);
                if (txtFoundRoomInfo != null && found)
                {
                    txtFoundRoomInfo.text = "✅ Đã tìm thấy: Phòng Chiến Hữu\nChủ phòng: Nam Làng • 4/6 người • Sân Làng";
                }
            }
        }

        private void EnterLobbyWithAutoFill(string roomName, string code)
        {
            UpdateLobbyRoster(4, roomName, code);
        }

        public void UpdateLobbyRoster(int humanCount, string roomName = "Phòng Chiến Hữu", string code = "HM4826")
        {
            ShowScreen(ProductionScreen.Lobby);
            _isLocalPlayerReady = false;
            _currentLobbyHumanCount = Mathf.Clamp(humanCount, 1, 6);

            if (txtLobbyHeader != null)
            {
                txtLobbyHeader.text = $"{roomName.ToUpperInvariant()} ({_currentLobbyHumanCount}/6 NGƯỜI THẬT) • SÂN LÀNG";
            }
            if (txtLobbyRoomCode != null)
            {
                txtLobbyRoomCode.text = "MÃ PHÒNG: " + code;
            }

            if (_currentLobbyHumanCount < 2)
            {
                if (txtLobbyCountdown != null)
                {
                    txtLobbyCountdown.text = "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot.";
                }
            }
            else
            {
                if (txtLobbyCountdown != null)
                {
                    txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
                }
            }

            if (txtLobbyReadyLabel != null)
            {
                txtLobbyReadyLabel.text = "SẴN SÀNG";
            }

            for (int i = 0; i < lobbySlots.Count && i < 6; i++)
            {
                var slot = lobbySlots[i];
                if (slot.root != null) slot.root.SetActive(true);

                if (i < _currentLobbyHumanCount)
                {
                    if (slot.txtName != null)
                    {
                        slot.txtName.text = i == 0 ? "Nam Làng (Bạn)" : (i < PlayerView.CharacterNames.Length ? PlayerView.CharacterNames[i] : $"Người chơi #{i + 1}");
                    }
                    if (i == 0)
                    {
                        slot.isReady = false;
                        if (slot.txtStatus != null)
                        {
                            slot.txtStatus.text = "CHƯA SẴN SÀNG";
                            slot.txtStatus.color = new Color(1f, 0.7f, 0.2f);
                        }
                    }
                    else
                    {
                        slot.isReady = true;
                        if (slot.txtStatus != null)
                        {
                            slot.txtStatus.text = "ĐÃ SẴN SÀNG";
                            slot.txtStatus.color = new Color(0.2f, 1f, 0.4f);
                        }
                    }
                }
                else
                {
                    // Empty slot: Mockup "Vào phòng" 6 chỗ, chỗ trống ghi "Đang chờ..."
                    slot.isReady = false;
                    if (slot.txtName != null)
                    {
                        slot.txtName.text = $"Chỗ trống #{i + 1}";
                    }
                    if (slot.txtStatus != null)
                    {
                        slot.txtStatus.text = "Đang chờ...";
                        slot.txtStatus.color = new Color(0.65f, 0.72f, 0.80f);
                    }
                }
            }

            EnsureLobbyToBotTrainingButton();
            if (btnLobbyToBotTraining != null)
            {
                btnLobbyToBotTraining.gameObject.SetActive(_currentLobbyHumanCount < 2);
            }
            if (btnLobbyReady != null)
            {
                btnLobbyReady.interactable = _currentLobbyHumanCount >= 2;
            }
        }

        private void ToggleLobbyReady()
        {
            if (_currentLobbyHumanCount < 2)
            {
                ShowToast("Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot.");
                return;
            }

            _isLocalPlayerReady = !_isLocalPlayerReady;
            AudioManager.Instance?.PlayReady();

            if (lobbySlots.Count > 0 && lobbySlots[0].txtStatus != null)
            {
                lobbySlots[0].isReady = _isLocalPlayerReady;
                lobbySlots[0].txtStatus.text = _isLocalPlayerReady ? "ĐÃ SẴN SÀNG" : "CHƯA SẴN SÀNG";
                lobbySlots[0].txtStatus.color = _isLocalPlayerReady ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.7f, 0.2f);
            }

            if (txtLobbyReadyLabel != null)
            {
                txtLobbyReadyLabel.text = _isLocalPlayerReady ? "HỦY SẴN SÀNG" : "SẴN SÀNG";
            }

            bool allReady = true;
            for (int i = 0; i < _currentLobbyHumanCount && i < lobbySlots.Count; i++)
            {
                if (!lobbySlots[i].isReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);
                _lobbyCountdownCoroutine = StartCoroutine(LobbyCountdownRoutine());
            }
            else
            {
                if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);
                if (txtLobbyCountdown != null) txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
            }
        }

        private IEnumerator LobbyCountdownRoutine()
        {
            string[] countdownMsgs =
            {
                "",
                "Tất cả đã sẵn sàng! Trận đấu bắt đầu sau 1...",
                "Tất cả đã sẵn sàng! Trận đấu bắt đầu sau 2...",
                "Tất cả đã sẵn sàng! Trận đấu bắt đầu sau 3..."
            };
            for (int sec = 3; sec >= 1; sec--)
            {
                if (txtLobbyCountdown != null)
                {
                    txtLobbyCountdown.text = countdownMsgs[sec];
                }
                AudioManager.Instance?.PlayCountdownTick();
                yield return new WaitForSeconds(1.0f);
            }

            StartGameplayMatch();
        }

        public void EnsureLobbyToBotTrainingButton()
        {
            if (btnLobbyToBotTraining != null) return;
            if (lobbyPanel == null) return;

            var existing = lobbyPanel.transform.Find("BtnGoToBotTraining");
            if (existing != null)
            {
                btnLobbyToBotTraining = existing.GetComponent<Button>();
                return;
            }

            var go = new GameObject("BtnGoToBotTraining", typeof(RectTransform));
            go.transform.SetParent(lobbyPanel.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.12f, 0.13f);
            rt.anchorMax = new Vector2(0.88f, 0.19f);
            rt.sizeDelta = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.46f, 0.85f, 0.95f);
            btnLobbyToBotTraining = go.AddComponent<Button>();
            btnLobbyToBotTraining.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayClick();
                EnsureBotTrainingPanel();
                ShowScreen(ProductionScreen.BotTraining);
            });

            var txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(go.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            if (txtLobbyHeader != null && txtLobbyHeader.font != null) txt.font = txtLobbyHeader.font;
            txt.fontSize = 17f;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.text = "SANG MENU LUYỆN VỚI BOT";
        }

        public void EnsureBotTrainingPanel()
        {
            if (botTrainingPanel != null) return;

            Transform safeArea = transform.Find("SafeAreaRoot") ?? transform;
            var existing = safeArea.Find("BotTrainingPanel");
            if (existing != null)
            {
                botTrainingPanel = existing.gameObject;
                return;
            }

            var panelObj = new GameObject("BotTrainingPanel", typeof(RectTransform));
            panelObj.transform.SetParent(safeArea, false);
            var rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            var bgImg = panelObj.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.09f, 0.15f, 0.98f);

            var titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.08f, 0.83f);
            titleRt.anchorMax = new Vector2(0.92f, 0.91f);
            titleRt.sizeDelta = Vector2.zero;
            var txtTitle = titleObj.AddComponent<TextMeshProUGUI>();
            if (txtHomeLogo != null && txtHomeLogo.font != null) txtTitle.font = txtHomeLogo.font;
            else if (txtLobbyHeader != null && txtLobbyHeader.font != null) txtTitle.font = txtLobbyHeader.font;
            txtTitle.fontSize = 26f;
            txtTitle.fontStyle = FontStyles.Bold;
            txtTitle.alignment = TextAlignmentOptions.Center;
            txtTitle.color = new Color(1f, 0.85f, 0.25f);
            txtTitle.text = "LUYỆN VỚI BOT (CỤC BỘ)";

            var subObj = new GameObject("Subtitle", typeof(RectTransform));
            subObj.transform.SetParent(panelObj.transform, false);
            var subRt = subObj.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.06f, 0.74f);
            subRt.anchorMax = new Vector2(0.94f, 0.82f);
            subRt.sizeDelta = Vector2.zero;
            var txtSub = subObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtSub.font = txtTitle.font;
            txtSub.fontSize = 15f;
            txtSub.alignment = TextAlignmentOptions.Center;
            txtSub.color = new Color(0.80f, 0.88f, 0.95f);
            txtSub.text = "Chế độ một mình trên máy • Không cần mạng\nĐúng luật v1.2 (3 HP, 5s đặt, ném đồng thời)";

            // Bot selector card
            var botCard = new GameObject("BotSelectorCard", typeof(RectTransform));
            botCard.transform.SetParent(panelObj.transform, false);
            var botCardRt = botCard.GetComponent<RectTransform>();
            botCardRt.anchorMin = new Vector2(0.10f, 0.54f);
            botCardRt.anchorMax = new Vector2(0.90f, 0.69f);
            botCardRt.sizeDelta = Vector2.zero;
            var botCardBg = botCard.AddComponent<Image>();
            botCardBg.color = new Color(0.12f, 0.16f, 0.25f, 0.95f);

            var botHeaderObj = new GameObject("Label", typeof(RectTransform));
            botHeaderObj.transform.SetParent(botCard.transform, false);
            var botHeaderRt = botHeaderObj.GetComponent<RectTransform>();
            botHeaderRt.anchorMin = new Vector2(0.05f, 0.54f);
            botHeaderRt.anchorMax = new Vector2(0.95f, 0.92f);
            botHeaderRt.sizeDelta = Vector2.zero;
            var txtBotHeader = botHeaderObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtBotHeader.font = txtTitle.font;
            txtBotHeader.fontSize = 15f;
            txtBotHeader.alignment = TextAlignmentOptions.Center;
            txtBotHeader.color = new Color(0.68f, 0.78f, 0.90f);
            txtBotHeader.text = "SỐ LƯỢNG BOT (1–5 BOT):";

            var botValObj = new GameObject("Value", typeof(RectTransform));
            botValObj.transform.SetParent(botCard.transform, false);
            var botValRt = botValObj.GetComponent<RectTransform>();
            botValRt.anchorMin = new Vector2(0.20f, 0.10f);
            botValRt.anchorMax = new Vector2(0.80f, 0.50f);
            botValRt.sizeDelta = Vector2.zero;
            txtSelectedBotCount = botValObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtSelectedBotCount.font = txtTitle.font;
            txtSelectedBotCount.fontSize = 19f;
            txtSelectedBotCount.fontStyle = FontStyles.Bold;
            txtSelectedBotCount.alignment = TextAlignmentOptions.Center;
            txtSelectedBotCount.color = Color.white;

            // Bot decrease button
            var btnDecBotObj = new GameObject("BtnDecBot", typeof(RectTransform));
            btnDecBotObj.transform.SetParent(botCard.transform, false);
            var btnDecBotRt = btnDecBotObj.GetComponent<RectTransform>();
            btnDecBotRt.anchorMin = new Vector2(0.06f, 0.10f);
            btnDecBotRt.anchorMax = new Vector2(0.22f, 0.50f);
            btnDecBotRt.sizeDelta = Vector2.zero;
            var btnDecBotBg = btnDecBotObj.AddComponent<Image>();
            btnDecBotBg.color = new Color(0.25f, 0.32f, 0.44f);
            btnDecreaseBots = btnDecBotObj.AddComponent<Button>();
            btnDecreaseBots.onClick.AddListener(() => ChangeBotCount(-1));
            var txtDecObj = new GameObject("Text", typeof(RectTransform));
            txtDecObj.transform.SetParent(btnDecBotObj.transform, false);
            var txtDecRt = txtDecObj.GetComponent<RectTransform>();
            txtDecRt.anchorMin = Vector2.zero; txtDecRt.anchorMax = Vector2.one; txtDecRt.sizeDelta = Vector2.zero;
            var txtDec = txtDecObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtDec.font = txtTitle.font;
            txtDec.fontSize = 22f; txtDec.alignment = TextAlignmentOptions.Center; txtDec.color = Color.white; txtDec.text = "-";

            // Bot increase button
            var btnIncBotObj = new GameObject("BtnIncBot", typeof(RectTransform));
            btnIncBotObj.transform.SetParent(botCard.transform, false);
            var btnIncBotRt = btnIncBotObj.GetComponent<RectTransform>();
            btnIncBotRt.anchorMin = new Vector2(0.78f, 0.10f);
            btnIncBotRt.anchorMax = new Vector2(0.94f, 0.50f);
            btnIncBotRt.sizeDelta = Vector2.zero;
            var btnIncBotBg = btnIncBotObj.AddComponent<Image>();
            btnIncBotBg.color = new Color(0.25f, 0.32f, 0.44f);
            btnIncreaseBots = btnIncBotObj.AddComponent<Button>();
            btnIncreaseBots.onClick.AddListener(() => ChangeBotCount(1));
            var txtIncObj = new GameObject("Text", typeof(RectTransform));
            txtIncObj.transform.SetParent(btnIncBotObj.transform, false);
            var txtIncRt = txtIncObj.GetComponent<RectTransform>();
            txtIncRt.anchorMin = Vector2.zero; txtIncRt.anchorMax = Vector2.one; txtIncRt.sizeDelta = Vector2.zero;
            var txtInc = txtIncObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtInc.font = txtTitle.font;
            txtInc.fontSize = 22f; txtInc.alignment = TextAlignmentOptions.Center; txtInc.color = Color.white; txtInc.text = "+";

            // Character selector card
            var charCard = new GameObject("CharSelectorCard", typeof(RectTransform));
            charCard.transform.SetParent(panelObj.transform, false);
            var charCardRt = charCard.GetComponent<RectTransform>();
            charCardRt.anchorMin = new Vector2(0.10f, 0.36f);
            charCardRt.anchorMax = new Vector2(0.90f, 0.51f);
            charCardRt.sizeDelta = Vector2.zero;
            var charCardBg = charCard.AddComponent<Image>();
            charCardBg.color = new Color(0.12f, 0.16f, 0.25f, 0.95f);

            var charHeaderObj = new GameObject("Label", typeof(RectTransform));
            charHeaderObj.transform.SetParent(charCard.transform, false);
            var charHeaderRt = charHeaderObj.GetComponent<RectTransform>();
            charHeaderRt.anchorMin = new Vector2(0.05f, 0.54f);
            charHeaderRt.anchorMax = new Vector2(0.95f, 0.92f);
            charHeaderRt.sizeDelta = Vector2.zero;
            var txtCharHeader = charHeaderObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtCharHeader.font = txtTitle.font;
            txtCharHeader.fontSize = 15f;
            txtCharHeader.alignment = TextAlignmentOptions.Center;
            txtCharHeader.color = new Color(0.68f, 0.78f, 0.90f);
            txtCharHeader.text = "NHÂN VẬT CỦA BẠN:";

            var charValObj = new GameObject("Value", typeof(RectTransform));
            charValObj.transform.SetParent(charCard.transform, false);
            var charValRt = charValObj.GetComponent<RectTransform>();
            charValRt.anchorMin = new Vector2(0.20f, 0.10f);
            charValRt.anchorMax = new Vector2(0.80f, 0.50f);
            charValRt.sizeDelta = Vector2.zero;
            txtSelectedCharacter = charValObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtSelectedCharacter.font = txtTitle.font;
            txtSelectedCharacter.fontSize = 19f;
            txtSelectedCharacter.fontStyle = FontStyles.Bold;
            txtSelectedCharacter.alignment = TextAlignmentOptions.Center;
            txtSelectedCharacter.color = new Color(0.35f, 0.85f, 1f);

            // Prev char button
            var btnPrevObj = new GameObject("BtnPrevChar", typeof(RectTransform));
            btnPrevObj.transform.SetParent(charCard.transform, false);
            var btnPrevRt = btnPrevObj.GetComponent<RectTransform>();
            btnPrevRt.anchorMin = new Vector2(0.06f, 0.10f);
            btnPrevRt.anchorMax = new Vector2(0.22f, 0.50f);
            btnPrevRt.sizeDelta = Vector2.zero;
            var btnPrevBg = btnPrevObj.AddComponent<Image>();
            btnPrevBg.color = new Color(0.25f, 0.32f, 0.44f);
            btnPrevChar = btnPrevObj.AddComponent<Button>();
            btnPrevChar.onClick.AddListener(() => ChangePracticeCharacter(-1));
            var txtPrevObj = new GameObject("Text", typeof(RectTransform));
            txtPrevObj.transform.SetParent(btnPrevObj.transform, false);
            var txtPrevRt = txtPrevObj.GetComponent<RectTransform>();
            txtPrevRt.anchorMin = Vector2.zero; txtPrevRt.anchorMax = Vector2.one; txtPrevRt.sizeDelta = Vector2.zero;
            var txtPrev = txtPrevObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtPrev.font = txtTitle.font;
            txtPrev.fontSize = 18f; txtPrev.alignment = TextAlignmentOptions.Center; txtPrev.color = Color.white; txtPrev.text = "<";

            // Next char button
            var btnNextObj = new GameObject("BtnNextChar", typeof(RectTransform));
            btnNextObj.transform.SetParent(charCard.transform, false);
            var btnNextRt = btnNextObj.GetComponent<RectTransform>();
            btnNextRt.anchorMin = new Vector2(0.78f, 0.10f);
            btnNextRt.anchorMax = new Vector2(0.94f, 0.50f);
            btnNextRt.sizeDelta = Vector2.zero;
            var btnNextBg = btnNextObj.AddComponent<Image>();
            btnNextBg.color = new Color(0.25f, 0.32f, 0.44f);
            btnNextChar = btnNextObj.AddComponent<Button>();
            btnNextChar.onClick.AddListener(() => ChangePracticeCharacter(1));
            var txtNextObj = new GameObject("Text", typeof(RectTransform));
            txtNextObj.transform.SetParent(btnNextObj.transform, false);
            var txtNextRt = txtNextObj.GetComponent<RectTransform>();
            txtNextRt.anchorMin = Vector2.zero; txtNextRt.anchorMax = Vector2.one; txtNextRt.sizeDelta = Vector2.zero;
            var txtNext = txtNextObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtNext.font = txtTitle.font;
            txtNext.fontSize = 18f; txtNext.alignment = TextAlignmentOptions.Center; txtNext.color = Color.white; txtNext.text = ">";

            // Action Start button
            var btnStartObj = new GameObject("BtnStartPractice", typeof(RectTransform));
            btnStartObj.transform.SetParent(panelObj.transform, false);
            var btnStartRt = btnStartObj.GetComponent<RectTransform>();
            btnStartRt.anchorMin = new Vector2(0.12f, 0.20f);
            btnStartRt.anchorMax = new Vector2(0.88f, 0.28f);
            btnStartRt.sizeDelta = Vector2.zero;
            var btnStartBg = btnStartObj.AddComponent<Image>();
            btnStartBg.color = new Color(0.12f, 0.65f, 0.35f, 0.98f);
            btnStartBotTraining = btnStartObj.AddComponent<Button>();
            btnStartBotTraining.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayClick();
                StartPracticeWithBots(_selectedBotCount, _selectedPracticeCharIndex);
            });
            var btnStartTxtObj = new GameObject("Text", typeof(RectTransform));
            btnStartTxtObj.transform.SetParent(btnStartObj.transform, false);
            var btnStartTxtRt = btnStartTxtObj.GetComponent<RectTransform>();
            btnStartTxtRt.anchorMin = Vector2.zero; btnStartTxtRt.anchorMax = Vector2.one; btnStartTxtRt.sizeDelta = Vector2.zero;
            var txtStart = btnStartTxtObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtStart.font = txtTitle.font;
            txtStart.fontSize = 20f;
            txtStart.fontStyle = FontStyles.Bold;
            txtStart.alignment = TextAlignmentOptions.Center;
            txtStart.color = Color.white;
            txtStart.text = "BẮT ĐẦU LUYỆN TẬP";

            // Back button
            var btnBackObj = new GameObject("BtnBack", typeof(RectTransform));
            btnBackObj.transform.SetParent(panelObj.transform, false);
            var btnBackRt = btnBackObj.GetComponent<RectTransform>();
            btnBackRt.anchorMin = new Vector2(0.20f, 0.09f);
            btnBackRt.anchorMax = new Vector2(0.80f, 0.16f);
            btnBackRt.sizeDelta = Vector2.zero;
            var btnBackBg = btnBackObj.AddComponent<Image>();
            btnBackBg.color = new Color(0.25f, 0.30f, 0.42f, 0.95f);
            btnBackFromBotTraining = btnBackObj.AddComponent<Button>();
            btnBackFromBotTraining.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayClick();
                ShowScreen(ProductionScreen.Home);
            });
            var btnBackTxtObj = new GameObject("Text", typeof(RectTransform));
            btnBackTxtObj.transform.SetParent(btnBackObj.transform, false);
            var btnBackTxtRt = btnBackTxtObj.GetComponent<RectTransform>();
            btnBackTxtRt.anchorMin = Vector2.zero; btnBackTxtRt.anchorMax = Vector2.one; btnBackTxtRt.sizeDelta = Vector2.zero;
            var txtBack = btnBackTxtObj.AddComponent<TextMeshProUGUI>();
            if (txtTitle.font != null) txtBack.font = txtTitle.font;
            txtBack.fontSize = 16f;
            txtBack.alignment = TextAlignmentOptions.Center;
            txtBack.color = new Color(0.9f, 0.9f, 0.9f);
            txtBack.text = "QUAY LẠI TRANG CHỦ";

            botTrainingPanel = panelObj;
            RefreshBotTrainingDisplay();
        }

        public void ChangeBotCount(int delta)
        {
            AudioManager.Instance?.PlayClick();
            _selectedBotCount = Mathf.Clamp(_selectedBotCount + delta, 1, 5);
            RefreshBotTrainingDisplay();
        }

        public void ChangePracticeCharacter(int delta)
        {
            AudioManager.Instance?.PlayClick();
            int total = PlayerView.CharacterNames.Length;
            _selectedPracticeCharIndex = ((_selectedPracticeCharIndex + delta) % total + total) % total;
            RefreshBotTrainingDisplay();
        }

        public void RefreshBotTrainingDisplay()
        {
            if (txtSelectedBotCount != null)
            {
                int totalPlayers = _selectedBotCount + 1;
                txtSelectedBotCount.text = $"{_selectedBotCount} BOT (1v{_selectedBotCount} • Tổng {totalPlayers} người)";
            }
            if (txtSelectedCharacter != null)
            {
                string charName = (_selectedPracticeCharIndex >= 0 && _selectedPracticeCharIndex < PlayerView.CharacterNames.Length)
                    ? PlayerView.CharacterNames[_selectedPracticeCharIndex]
                    : "Nam Làng";
                txtSelectedCharacter.text = charName;
            }
        }

        public void StartPracticeWithBots(int botCount = 3, int characterIndex = 0)
        {
            if (matchController != null)
            {
                matchController.localCharacterIndex = characterIndex;
                if (matchController.configAsset != null)
                {
                    matchController.configAsset.maxPlayers = Mathf.Clamp(botCount + 1, 2, 6);
                }
            }
            StartGameplayMatch();
        }

        public void StartGameplayMatch()
        {
            if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);
            _lastDisplayedPhase = (Phase)(-1);
            _lastSyncedStatePhase = (Phase)(-1);
            ShowScreen(ProductionScreen.Gameplay);
            ApplyPhaseState(Phase.Placing);

            if (btnInGameReady != null)
            {
                btnInGameReady.interactable = true;
                if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = "SẴN SÀNG";
            }

            if (matchController != null)
            {
                matchController.StartMatch();
            }
        }

        private void HandleRoundStarted(int round, int maxRounds)
        {
            if (txtRound != null && (round != _lastDisplayedRound || maxRounds != _lastDisplayedMaxRounds))
            {
                _lastDisplayedRound = round;
                _lastDisplayedMaxRounds = maxRounds;
                if (maxRounds == 5 && round >= 0 && round < CachedRound5Strings.Length)
                {
                    txtRound.text = CachedRound5Strings[round];
                }
                else
                {
                    txtRound.text = "VÒNG " + round + "/" + maxRounds;
                }
            }

            if (btnInGameReady != null)
            {
                btnInGameReady.interactable = true;
                if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = "SẴN SÀNG";
            }

            Phase authoritativePhase = (matchController != null && matchController.State != null)
                ? matchController.State.Phase
                : Phase.Placing;
            ApplyPhaseState(authoritativePhase);
            UpdateHpCards();
        }

        private void HandlePhaseChanged(Phase phase)
        {
            Phase authoritativePhase = (matchController != null && matchController.State != null)
                ? matchController.State.Phase
                : phase;
            ApplyPhaseState(authoritativePhase);
            UpdateHpCards();
        }

        public void ApplyPhaseState(Phase phase)
        {
            ResolveHudElementContainers();
            _lastSyncedStatePhase = phase;

            bool isPlacing = phase == Phase.Placing;
            bool isMatchEnd = phase == Phase.MatchEnd;

            // P1 & P3: Round, Timer, and Sẵn sàng are in the same top safe-area row and ONLY visible during Placing.
            SetTopPlacingRowVisible(isPlacing && !isMatchEnd);

            // P1: When MatchEnd, always hide instruction HUD / "KẾT QUẢ VÒNG" banner and weapon pill.
            SetPhaseBannerVisible(!isMatchEnd);
            SetWeaponPillVisible(!isMatchEnd);

            if (!isMatchEnd && txtPhaseBanner != null && phase != _lastDisplayedPhase)
            {
                _lastDisplayedPhase = phase;
                switch (phase)
                {
                    case Phase.Placing:
                        txtPhaseBanner.text = "CHỌN HƯỚNG NÉM • Chạm để đặt, kéo để ngắm";
                        break;
                    case Phase.Locked:
                        txtPhaseBanner.text = "ĐÃ KHÓA HƯỚNG NÉM";
                        break;
                    case Phase.Reveal:
                        txtPhaseBanner.text = "LỘ DIỆN ĐỐI THỦ!";
                        break;
                    case Phase.Throwing:
                        txtPhaseBanner.text = "NÉM DÉP!";
                        break;
                    case Phase.Resolve:
                    case Phase.RoundEnd:
                        txtPhaseBanner.text = "KẾT QUẢ VÒNG";
                        break;
                }
            }

            if (isMatchEnd && !IsUrlAutoPlay())
            {
                if (matchController != null && matchController.State != null)
                {
                    PopulateResultFromMatchState(matchController.State);
                }
                else
                {
                    ShowScreen(ProductionScreen.Result);
                }
            }
        }

        private void SetTopPlacingRowVisible(bool visible)
        {
            if (topPlacingBar != null && topPlacingBar.activeSelf != visible)
            {
                topPlacingBar.SetActive(visible);
            }
            if (roundPill != null && roundPill.activeSelf != visible)
            {
                roundPill.SetActive(visible);
            }
            if (txtRound != null && txtRound.gameObject.activeSelf != visible)
            {
                txtRound.gameObject.SetActive(visible);
            }

            if (timerPill != null && timerPill.activeSelf != visible)
            {
                timerPill.SetActive(visible);
            }
            if (txtTimer != null && txtTimer.gameObject.activeSelf != visible)
            {
                txtTimer.gameObject.SetActive(visible);
            }

            if (btnInGameReady != null && btnInGameReady.gameObject.activeSelf != visible)
            {
                btnInGameReady.gameObject.SetActive(visible);
            }
        }

        private void SetPhaseBannerVisible(bool visible)
        {
            if (phaseBannerObj != null && phaseBannerObj.activeSelf != visible)
            {
                phaseBannerObj.SetActive(visible);
            }
            if (txtPhaseBanner != null && txtPhaseBanner.gameObject.activeSelf != visible)
            {
                txtPhaseBanner.gameObject.SetActive(visible);
            }
        }

        private void SetWeaponPillVisible(bool visible)
        {
            if (weaponPillObj != null && weaponPillObj.activeSelf != visible)
            {
                weaponPillObj.SetActive(visible);
            }
            if (txtWeaponIndicator != null && txtWeaponIndicator.gameObject.activeSelf != visible)
            {
                txtWeaponIndicator.gameObject.SetActive(visible);
            }
        }

        private void HandleTimerTick(float secondsRemaining)
        {
            if (txtTimer == null) return;
            int tenths = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(0f, secondsRemaining) * 10f), 0, CachedTenthsTimerStrings.Length - 1);
            if (tenths != _lastDisplayedTimerTenths)
            {
                _lastDisplayedTimerTenths = tenths;
                txtTimer.text = CachedTenthsTimerStrings[tenths];
            }

            bool lowWarning = secondsRemaining <= 3f;
            if (lowWarning != _lastTimerLowWarning)
            {
                _lastTimerLowWarning = lowWarning;
                txtTimer.color = lowWarning ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        public void ConfigureHpCardsLayout(int count)
        {
            _lastConfiguredHpCardCount = count;
            int clamped = Mathf.Clamp(count, 1, hpCards.Count);
            Vector2[] anchors;
            Vector2 cardSize;

            switch (clamped)
            {
                case 2:
                    anchors = new[] { new Vector2(0.28f, 0.50f), new Vector2(0.72f, 0.50f) };
                    cardSize = new Vector2(380f, 62f);
                    break;
                case 3:
                    anchors = new[] { new Vector2(0.17f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.83f, 0.50f) };
                    cardSize = new Vector2(328f, 60f);
                    break;
                case 4:
                    anchors = new[]
                    {
                        new Vector2(0.28f, 0.76f), new Vector2(0.72f, 0.76f),
                        new Vector2(0.28f, 0.24f), new Vector2(0.72f, 0.24f)
                    };
                    cardSize = new Vector2(380f, 60f);
                    break;
                case 5:
                    anchors = new[]
                    {
                        new Vector2(0.17f, 0.76f), new Vector2(0.50f, 0.76f), new Vector2(0.83f, 0.76f),
                        new Vector2(0.33f, 0.24f), new Vector2(0.67f, 0.24f)
                    };
                    cardSize = new Vector2(328f, 60f);
                    break;
                case 6:
                default:
                    anchors = new[]
                    {
                        new Vector2(0.17f, 0.76f), new Vector2(0.50f, 0.76f), new Vector2(0.83f, 0.76f),
                        new Vector2(0.17f, 0.24f), new Vector2(0.50f, 0.24f), new Vector2(0.83f, 0.24f)
                    };
                    cardSize = new Vector2(328f, 60f);
                    break;
            }

            for (int i = 0; i < hpCards.Count; i++)
            {
                var card = hpCards[i];
                if (card.root == null) continue;
                if (i < clamped)
                {
                    var rt = card.root.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = anchors[i];
                        rt.anchorMax = anchors[i];
                        rt.anchoredPosition = Vector2.zero;
                        rt.sizeDelta = cardSize;
                    }
                    if (!card.root.activeSelf) card.root.SetActive(true);
                }
                else
                {
                    if (card.root.activeSelf) card.root.SetActive(false);
                }
            }
        }

        public void UpdateHpCards()
        {
            if (matchController == null || matchController.State == null) return;
            var state = matchController.State;
            var phase = state.Phase;

            if (_lastConfiguredHpCardCount != state.Players.Count)
            {
                ConfigureHpCardsLayout(state.Players.Count);
            }

            for (int i = 0; i < hpCards.Count && i < state.Players.Count; i++)
            {
                var card = hpCards[i];
                var player = state.Players[i];
                card.playerId = player.Id;

                if (card.root != null && !card.root.activeSelf)
                {
                    card.root.SetActive(true);
                }

                if (card.txtName != null && !string.Equals(card.lastDisplayedName, player.Name, StringComparison.Ordinal))
                {
                    card.lastDisplayedName = player.Name;
                    card.txtName.text = player.Name;
                }

                // S3: HP is ALWAYS public in all phases (only position & aim direction are hidden during Placing).
                int clampedHp = Mathf.Clamp(player.Hp, 0, 3);
                if (card.txtHearts != null && card.lastDisplayedHpCode != clampedHp)
                {
                    card.lastDisplayedHpCode = clampedHp;
                    card.txtHearts.text = CachedHpCardStrings[clampedHp];
                    card.txtHearts.color = player.IsAlive
                        ? (clampedHp == 3 ? new Color(0.25f, 1f, 0.44f) : (clampedHp == 2 ? new Color(1f, 0.86f, 0.25f) : new Color(1f, 0.42f, 0.42f)))
                        : new Color(1f, 0.44f, 0.44f);
                }

                bool isLocal = player.Id == "player";
                int statusCode = !player.IsAlive ? 0 : ((phase == Phase.Placing && !isLocal) ? -1 : 1);
                if (card.txtHpNum != null && card.lastDisplayedStatusCode != statusCode)
                {
                    card.lastDisplayedStatusCode = statusCode;
                    if (statusCode == 0)
                    {
                        card.txtHpNum.text = StatusEliminatedText;
                        card.txtHpNum.color = new Color(1f, 0.42f, 0.42f);
                    }
                    else if (statusCode == -1)
                    {
                        card.txtHpNum.text = StatusHiddenText;
                        card.txtHpNum.color = new Color(0.65f, 0.72f, 0.80f);
                    }
                    else
                    {
                        card.txtHpNum.text = StatusAliveText;
                        card.txtHpNum.color = new Color(0.44f, 0.95f, 0.65f);
                    }
                }

                if (card.imgBg != null)
                {
                    card.imgBg.color = player.IsAlive
                        ? (statusCode == -1 ? new Color(0.92f, 0.95f, 1f, 1f) : Color.white)
                        : new Color(0.54f, 0.42f, 0.44f, 0.92f);
                }
            }
        }

        public void PopulateResultFromMatchState(MatchState state)
        {
            if (state == null) return;
            MatchRanking.BuildRankingsAndWinners(state, _uiRankedBuffer, _uiWinnersBuffer);
            PopulateResultBoard(_uiRankedBuffer, _uiWinnersBuffer);
        }

        private void HandleMatchEnded(List<PlayerState> ranked, List<string> winnerIds)
        {
            if (IsUrlAutoPlay())
            {
                return;
            }

            PopulateResultBoard(ranked, winnerIds);
        }

        public void PopulateResultBoard(List<PlayerState> ranked, List<string> winnerIds)
        {
            SetTopPlacingRowVisible(false);
            SetPhaseBannerVisible(false);
            SetWeaponPillVisible(false);
            ShowScreen(ProductionScreen.Result);

            bool isTie = winnerIds != null && winnerIds.Count > 1;
            bool isLocalWinner = winnerIds != null && winnerIds.Contains("player");

            if (txtResultTitle != null)
            {
                if (isTie)
                {
                    txtResultTitle.text = isLocalWinner ? "ĐỒNG CHIẾN THẮNG!" : "HÒA TOP 1!";
                    txtResultTitle.color = isLocalWinner ? new Color(1f, 0.92f, 0.25f) : new Color(0.44f, 0.88f, 1f);
                }
                else if (winnerIds != null && winnerIds.Count == 1)
                {
                    txtResultTitle.text = isLocalWinner ? "CHIẾN THẮNG!" : "KẾT THÚC TRẬN ĐẤU";
                    txtResultTitle.color = isLocalWinner ? new Color(1f, 0.92f, 0.25f) : new Color(1f, 0.54f, 0.40f);
                }
                else
                {
                    txtResultTitle.text = "HÒA NHAU!";
                    txtResultTitle.color = new Color(0.44f, 0.88f, 1f);
                }
            }

            if (txtResultSubtitle != null && ranked != null && winnerIds != null)
            {
                var sb = new StringBuilder(isTie ? "Đồng thắng: " : "Người thắng: ");
                bool first = true;
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (!winnerIds.Contains(ranked[i].Id)) continue;
                    if (!first) sb.Append(", ");
                    sb.Append(ranked[i].Name);
                    sb.Append(" (").Append(Mathf.Max(0, ranked[i].Hp)).Append("/3 HP • ").Append(ranked[i].HitsLanded).Append(" Hit)");
                    first = false;
                }
                if (first) sb.Append("Không có");
                txtResultSubtitle.text = sb.ToString();
            }

            if (ranked == null) return;

            for (int i = 0; i < resultRows.Count; i++)
            {
                var row = resultRows[i];
                if (i >= ranked.Count)
                {
                    if (row.root != null) row.root.SetActive(false);
                    continue;
                }

                var p = ranked[i];
                bool isMe = p.Id == "player";
                bool won = winnerIds != null && winnerIds.Contains(p.Id);

                if (row.root != null) row.root.SetActive(true);
                if (row.txtRank != null)
                {
                    row.txtRank.text = i < CachedRankStrings.Length ? CachedRankStrings[i] : ("#" + (i + 1));
                }
                if (row.txtName != null)
                {
                    row.txtName.text = isMe ? (p.Name + " (Bạn)") : p.Name;
                }
                if (row.txtHp != null)
                {
                    int hpClamped = Mathf.Clamp(p.Hp, 0, 3);
                    row.txtHp.text = "HP: " + hpClamped + "/3 • HIT: " + p.HitsLanded;
                    row.txtHp.color = p.IsAlive ? new Color(0.85f, 0.95f, 1f) : new Color(0.75f, 0.54f, 0.54f);
                }

                if (row.txtStatus != null)
                {
                    if (won)
                    {
                        row.txtStatus.text = isTie ? "★ ĐỒNG HẠNG 1" : "★ THẮNG";
                        row.txtStatus.color = new Color(1f, 0.92f, 0.25f);
                    }
                    else
                    {
                        row.txtStatus.text = p.IsAlive ? "Còn sống" : "✖ Đã loại";
                        row.txtStatus.color = p.IsAlive ? Color.white : new Color(1f, 0.44f, 0.44f);
                    }
                }

                if (row.imgBg != null)
                {
                    row.imgBg.color = isMe ? new Color(1f, 1f, 1f, 1f) : new Color(0.92f, 0.95f, 1f, 0.92f);
                }
            }
        }
    }
}
