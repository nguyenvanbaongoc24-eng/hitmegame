using System;



using System.Collections;



using System.Collections.Generic;



using System.Text;



using HitMe.Core;



using HitMe.Audio;



using HitMe.Game;



using HitMe.Economy;



using HitMe.NetClientV1;



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



        private const string StatusAliveText = "CÒN SỐNG";



        private const string StatusEliminatedText = "ĐÃ LOẠI";







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



        [Header("Emote and Visual Assets")]

        public Sprite spriteSpeechBubble;

        public Sprite[] charHeadSprites;



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



        public Image imgHomeCharPortrait;



        public Image imgHomeCharCardIcon;



        public TextMeshProUGUI txtHomeCharName;



        public TextMeshProUGUI txtHomeCharDesc;



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



        public Button btnCopyRoomCode;



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



        public TextMeshProUGUI txtSelectedCharDetail;



        public Button btnDecreaseBots;



        public Button btnIncreaseBots;



        public Button btnPrevChar;



        public Button btnNextChar;



        public Button btnStartBotTraining;



        public Button btnBackFromBotTraining;



        public List<Image> botCharCardFrames = new List<Image>();



        public List<Button> botCharCardButtons = new List<Button>();







        [Header("Quick Match Controls")]



        public GameObject quickMatchOverlay;



        public TextMeshProUGUI txtQuickMatchStatus;



        public Button btnQuickMatchToBots;



        public Button btnCancelQuickMatch;







        [Header("UI Theme Reference")]



        public UiTheme theme;







        [Header("Juice & Feedback (Lô 4)")]



        public JuiceFeedbackController juiceFeedback;







        [Header("Gameplay HUD Controls")]



        public GameObject topPlacingBar;



        public GameObject roundPill;



        public GameObject timerPill;



        public Image timerPulseRing;



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







        [Header("Leave Confirmation Dialog")]



        public GameObject dialogLeaveRoom;



        public TextMeshProUGUI txtLeaveDialogTitle;



        public TextMeshProUGUI txtLeaveDialogMessage;



        public Button btnConfirmLeave;



        public Button btnCancelLeave;







        [Header("Round Result Overlay")]



        public GameObject roundResultOverlay;



        public TextMeshProUGUI txtRoundResultTitle;



        public TextMeshProUGUI txtRoundResultDetail;







        [Header("Settings Dialog (Lô 5)")]



        public GameObject dialogSettings;



        public TextMeshProUGUI txtSettingsTitle;



        public Slider sliderMusicVolume;



        public Slider sliderSfxVolume;



        public Toggle toggleMusic;



        public Toggle toggleSfx;



        public Toggle toggleReducedMotion;



        public Toggle toggleShowFps;



        public Button btnLanguageVi;



        public Button btnLanguageEn;



        public Button btnCloseSettings;



        public Button btnLeaveMatchFromSettings;



        public Button btnHomeSettings;



        public Button btnInGameSettings;
        public Button btnInGameEmote;







        [Header("Tutorial Dialog (Lô 6)")]



        public GameObject dialogTutorial;



        public TextMeshProUGUI txtTutorialTitle;



        public TextMeshProUGUI txtTutorialStepHeader;



        public TextMeshProUGUI txtTutorialStepBody;



        public Button btnTutorialSkip;



        public Button btnTutorialNext;



        public TextMeshProUGUI txtTutorialNextLabel;



        public GameObject[] tutorialStepIndicators;



        private int _currentTutorialStep = 0;







        [Header("Home Header & Resources (Lô 7 & 8)")]



        public TextMeshProUGUI txtHomePlayerName;



        public TextMeshProUGUI txtHomeGold;



        public TextMeshProUGUI txtHomeGems;



        public Button btnHomeProfile;



        public Button btnHomeShop;



        public Button btnHomeInventory;



        public Button btnHomeLeaderboard;



        public Button btnHomeLogin;







        [Header("Profile Dialog (Lô 7)")]



        public GameObject dialogProfile;



        public TextMeshProUGUI txtProfileTitle;



        public TextMeshProUGUI txtProfilePlayerName;



        public TextMeshProUGUI txtProfilePlayerId;



        public TextMeshProUGUI txtProfileGold;



        public TMP_InputField inputProfileNewName;



        public Button btnProfileRename;



        public TextMeshProUGUI txtProfileRenameCost;



        public Button btnCloseProfile;







        [Header("Shop Dialog (Lô 7)")]



        public GameObject dialogShop;



        public TextMeshProUGUI txtShopTitle;



        public TextMeshProUGUI txtShopSubtitle;



        public TextMeshProUGUI txtShopGold;



        public Transform shopItemsContainer;



        public Button btnCloseShop;







        [Header("Inventory Dialog (Lô 7)")]



        public GameObject dialogInventory;



        public TextMeshProUGUI txtInventoryTitle;



        public TextMeshProUGUI txtInventoryEquippedWeapon;



        public Transform inventoryItemsContainer;



        public Button btnCloseInventory;







        [Header("Leaderboard Dialog (Lô 7)")]



        public GameObject dialogLeaderboard;



        public TextMeshProUGUI txtLeaderboardTitle;



        public Transform leaderboardRowsContainer;



        public Button btnCloseLeaderboard;







        [Header("Login Dialog (Lô 8)")]



        public GameObject dialogLogin;



        public TextMeshProUGUI txtLoginTitle;



        public TMP_InputField inputLoginGuestName;



        public Button btnSubmitGuestLogin;



        public Button btnLoginGoogle;



        public Button btnLoginFacebook;



        public Button btnLoginApple;



        public Button btnCloseLogin;



        public Button btnReportBug;



        public TextMeshProUGUI txtSettingsBuildVersion;



        private Button _btnLobbyMapSelector;



        private TextMeshProUGUI _txtLobbyMapSelector;







        public ProductionScreen CurrentScreen { get; private set; } = ProductionScreen.Home;



        public string SelectedCharacterId => HitMe.Data.CharacterRegistry.GetByIndex(_selectedPracticeCharIndex).id;



        public string SelectedWeaponId => "dep_to_ong";







        private int _currentLeaderboardTab = 0; // 0 = Leaderboard, 1 = Quests



        private GameObject _leaderboardPlaceholderObj;



        private TextMeshProUGUI _txtLeaderboardPlaceholderTitle;



        private TextMeshProUGUI _txtLeaderboardPlaceholderBody;



        private Button _btnTabLeaderboard;



        private Button _btnTabQuests;



        private TextMeshProUGUI _txtTabLeaderboardLabel;



        private TextMeshProUGUI _txtTabQuestsLabel;



        private Button _btnHomeLangToggle;



        private TextMeshProUGUI _txtHomeLangToggle;







        private Coroutine _toastCoroutine;



        private Coroutine _lobbyCountdownCoroutine;



        private Coroutine _quickMatchCoroutine;



        private System.Action _pendingLeaveAction;



        private bool _isLocalPlayerReady;



        private string _activeRoomCode = string.Empty;



        private int _selectedBotCount = 3;



        private int _selectedPracticeCharIndex = 0;



        private IGameClient _onlineClient;
        private bool _roomRequestPending;
        private bool _quickMatchPending;
        private bool _suppressRoomState;
        private Coroutine _roomRequestTimeout;



        public IGameClient OnlineClient => _onlineClient;







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



            public Image[] heartIcons;



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



            public TextMeshProUGUI txtRankBadge;



            public TextMeshProUGUI txtName;



            public TextMeshProUGUI txtHp;



            public TextMeshProUGUI txtStatus;



            public Image imgBg;



        }







        private void Awake()



        {



            if (theme == null)



            {



                theme = UiTheme.Instance;



            }







            if (matchController == null)



            {



                matchController = FindAnyObjectByType<MatchController>();



            }







            UiStrings.OnLanguageChanged += HandleLanguageChanged;



            HitMe.UI.RingBufferLogger.Initialize();



            ResolveHudElementContainers();



            BindEvents();



        }







        private void OnDestroy()



        {



            UiStrings.OnLanguageChanged -= HandleLanguageChanged;



            if (matchController != null)



            {



                matchController.OnRoundStarted -= HandleRoundStarted;



                matchController.OnPhaseChanged -= HandlePhaseChanged;



                matchController.OnTimerTick -= HandleTimerTick;



                matchController.OnToastRequested -= ShowToast;



                matchController.OnMatchEnded -= HandleMatchEnded;



            }



        }







        private void HandleLanguageChanged()



        {



            RefreshAllScreenTexts();



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







                public void HideAllUGUIForWebMode()
        {
            if (HitMeWebBridge.IsWebUiMode() == 1)
            {
                var canvases = FindObjectsOfType<Canvas>(true);
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] != null) canvases[i].enabled = false;
                }
                if (dialogTutorial != null) dialogTutorial.SetActive(false);
                PlayerPrefs.SetString(UiStrings.PrefKeyTutorialCompleted, "1");
                Debug.Log("[ProductionUIController] Web UI mode detected: All uGUI Canvases disabled.");
            }
        }

        private void Start()



        {



            if (HitMeWebBridge.IsWebUiMode() == 1) HideAllUGUIForWebMode();

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



            // EnsureHomeLanguageToggle(); -- HOME-02: No language button on Home

SyncHomeCharacterDisplay();



            EnforceFontAcrossAllTexts();







            int practiceBots = GetUrlInt("practiceBots", -1);



            if (practiceBots < 0) practiceBots = GetUrlInt("bots", -1);







            if (Application.absoluteURL != null && (Application.absoluteURL.IndexOf("lab=1", StringComparison.OrdinalIgnoreCase) >= 0 || Application.absoluteURL.IndexOf("screen=lab", StringComparison.OrdinalIgnoreCase) >= 0))



            {



                int labN = GetUrlInt("n", 2);



                StartLabSimulation(labN);



            }



            else if (IsUrlAutoPlay())



            {



                ShowScreen(ProductionScreen.Gameplay);



            }



            else if (practiceBots >= 1 && (Application.absoluteURL == null || Application.absoluteURL.IndexOf("screen=botTraining", StringComparison.OrdinalIgnoreCase) < 0))



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



                if (practiceBots >= 1)



                {



                    _selectedBotCount = Mathf.Clamp(practiceBots, 1, 5);



                }



                int charIdx = GetUrlInt("practiceChar", 0);



                _selectedPracticeCharIndex = Mathf.Clamp(charIdx, 0, 6);



                RefreshBotTrainingDisplay();



                ShowScreen(ProductionScreen.BotTraining);



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=lobby1p", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Lobby);



                UpdateLobbyRoster(1, "Phòng Chiến Hữu", "HM4826");



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=lobby3p", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Lobby);



                UpdateLobbyRoster(3, "Phòng Chiến Hữu", "HM4826");



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=lobby6p", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Lobby);



                UpdateLobbyRoster(6, "Phòng Chiến Hữu", "HM4826");



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=quickmatch", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                bool isTimeout = Application.absoluteURL.IndexOf("timeout=1", StringComparison.OrdinalIgnoreCase) >= 0;



                ShowQuickMatchDialog(isTimeout);



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=settings", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                if (Application.absoluteURL.IndexOf("lang=en", StringComparison.OrdinalIgnoreCase) >= 0)



                {



                    SetLanguageFromSettings(AppLanguage.English);



                }



                ShowSettingsDialog();



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=tutorial", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Gameplay);



                int tutStep = GetUrlInt("step", 0);



                if (Application.absoluteURL.IndexOf("lang=en", StringComparison.OrdinalIgnoreCase) >= 0)



                {



                    SetLanguageFromSettings(AppLanguage.English);



                }



                ShowTutorial(tutStep);



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=profile", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                ShowProfileDialog();



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=shop", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                ShowShopDialog();



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=inventory", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                ShowInventoryDialog();



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=leaderboard", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                ShowLeaderboardDialog();



            }



            else if (Application.absoluteURL != null && Application.absoluteURL.IndexOf("screen=login", StringComparison.OrdinalIgnoreCase) >= 0)



            {



                ShowScreen(ProductionScreen.Home);



                ShowLoginDialog();



            }



            else



            {



                ShowScreen(ProductionScreen.Home);



                RefreshBotTrainingDisplay();



                RefreshAccountDisplay();



            }







            string autoRoom = GetUrlString("room");



            if (string.IsNullOrEmpty(autoRoom)) autoRoom = GetUrlString("roomCode");



            if (!string.IsNullOrEmpty(autoRoom))



            {



                StartCoroutine(AutoJoinRoomCoroutine(autoRoom.Trim().ToUpperInvariant()));



            }







            RefreshAllScreenTexts();



            HitMeWebBridge.MarkHomeShown();



            Debug.Log("[HITME] HOME_SHOWN");



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







        private static string GetUrlString(string key)



        {



            string url = Application.absoluteURL;



            if (string.IsNullOrEmpty(url)) return null;



            int idx = url.IndexOf(key + "=", StringComparison.OrdinalIgnoreCase);



            if (idx < 0) return null;



            idx += key.Length + 1;



            int end = url.IndexOf('&', idx);



            return end > idx ? url.Substring(idx, end - idx) : url.Substring(idx);



        }







        private System.Collections.IEnumerator AutoJoinRoomCoroutine(string roomCode)
        {
            yield return new WaitForSeconds(0.4f);
            if (inputRoomCode != null) inputRoomCode.text = roomCode;
            if (btnSubmitJoinRoom != null) btnSubmitJoinRoom.onClick.Invoke();
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



                rt.anchorMax = new Vector2(1f, 0f);



                rt.pivot = new Vector2(0.5f, 0f);



                rt.anchoredPosition = new Vector2(0f, 4f);



                rt.sizeDelta = new Vector2(0f, 20f);



                txtBuildFingerprint.fontSize = 11f;



                txtBuildFingerprint.fontStyle = FontStyles.Normal;



                txtBuildFingerprint.color = new Color(1f, 1f, 1f, 0.40f);



                txtBuildFingerprint.alignment = TextAlignmentOptions.Bottom;



            }







            txtBuildFingerprint.transform.SetAsLastSibling();



            string fingerprint = ResolveBuildFingerprint();



            string betaBadge = UiStrings.Get(UiStrings.BetaBadge);



            txtBuildFingerprint.text = $"{fingerprint}";



            txtBuildFingerprint.gameObject.SetActive(false);



            Debug.Log("[BuildFingerprint] " + txtBuildFingerprint.text);



        }







        public static string ResolveBuildFingerprint()



        {



            var asset = Resources.Load<TextAsset>("BuildFingerprint");



            if (asset != null && !string.IsNullOrWhiteSpace(asset.text))



            {



                string text = asset.text.Trim();



                text = text.Replace(" · Production", "").Replace(" · production", "").Replace("Production", "").Replace(" · Graybox", "").Replace(" · graybox", "").Replace("Graybox", "").Trim();



                return text;



            }







            string ver = string.IsNullOrEmpty(Application.version) ? "dev" : Application.version;



            return "build " + ver;



        }







        private static bool IsDebugOverlayEnabled()



        {



            string url = Application.absoluteURL;



            if (string.IsNullOrEmpty(url)) return false;



            return url.IndexOf("debug=1", StringComparison.OrdinalIgnoreCase) >= 0 ||



                   url.IndexOf("debug=true", StringComparison.OrdinalIgnoreCase) >= 0;



        }







        public void EnsureAndRefreshFpsOverlay()



        {



            if (!IsDebugOverlayEnabled())



            {



                if (_fpsOverlayObj != null) _fpsOverlayObj.SetActive(false);



                return;



            }







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



                hitImg.raycastTarget = false;



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
            ConfigureRoomCodeInput();



            if (btnQuickMatch != null)



            {



                btnQuickMatch.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    StartQuickMatchSearch();



                });



            }







            if (btnCancelQuickMatch != null)



            {



                btnCancelQuickMatch.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    StopQuickMatchSearch();



                });



            }







            if (btnQuickMatchToBots != null)



            {



                btnQuickMatchToBots.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    StopQuickMatchSearch();



                    EnsureBotTrainingPanel();



                    ShowScreen(ProductionScreen.BotTraining);



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







            if (btnCopyRoomCode != null)



            {



                btnCopyRoomCode.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    string baseUrl = "https://hitmegame-iota.vercel.app/";



                    if (!string.IsNullOrEmpty(Application.absoluteURL))



                    {



                        try



                        {



                            var uri = new System.Uri(Application.absoluteURL);



                            baseUrl = uri.GetLeftPart(System.UriPartial.Path);



                        }



                        catch { }



                    }



                    if (!baseUrl.EndsWith("/")) baseUrl += "/";



                    string inviteUrl = $"{baseUrl}?room={_activeRoomCode}";



                    GUIUtility.systemCopyBuffer = inviteUrl;



                    ShowToast(UiStrings.Get(UiStrings.InviteLinkCopiedToast), 2.5f);



                });



            }







            if (btnLobbyToBotTraining != null)



            {



                btnLobbyToBotTraining.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    EnsureBotTrainingPanel();



                    ShowScreen(ProductionScreen.BotTraining);



                });



            }







            if (btnDecreaseBots != null)



            {



                btnDecreaseBots.onClick.AddListener(() => ChangeBotCount(-1));



            }







            if (btnIncreaseBots != null)



            {



                btnIncreaseBots.onClick.AddListener(() => ChangeBotCount(1));



            }







            if (btnStartBotTraining != null)



            {



                btnStartBotTraining.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    StartPracticeWithBots(_selectedBotCount, _selectedPracticeCharIndex);



                });



            }







            if (btnBackFromBotTraining != null)



            {



                btnBackFromBotTraining.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    ShowScreen(ProductionScreen.Home);



                });



            }







            for (int i = 0; i < botCharCardButtons.Count; i++)



            {



                int capturedIdx = i;



                if (botCharCardButtons[i] != null)



                {



                    botCharCardButtons[i].onClick.RemoveAllListeners();



                    botCharCardButtons[i].onClick.AddListener(() => SetSelectedPracticeCharacter(capturedIdx));



                }



            }







            if (homePanel != null)



            {



                var btnPrev = homePanel.transform.Find("BtnHeroPrev")?.GetComponent<Button>();



                if (btnPrev != null)



                {



                    btnPrev.onClick.RemoveAllListeners();



                    btnPrev.onClick.AddListener(() => ChangePracticeCharacter(-1));



                }



                var btnNext = homePanel.transform.Find("BtnHeroNext")?.GetComponent<Button>();



                if (btnNext != null)



                {



                    btnNext.onClick.RemoveAllListeners();



                    btnNext.onClick.AddListener(() => ChangePracticeCharacter(1));



                }



                var charCardBtn = homePanel.transform.Find("CharCard")?.GetComponent<Button>();



                if (charCardBtn != null)



                {



                    charCardBtn.onClick.RemoveAllListeners();



                    charCardBtn.onClick.AddListener(() => ChangePracticeCharacter(1));



                }



                var weaponCardBtn = homePanel.transform.Find("WeaponCard")?.GetComponent<Button>();



                if (weaponCardBtn != null)



                {



                    weaponCardBtn.onClick.RemoveAllListeners();



                    weaponCardBtn.onClick.AddListener(() =>



                    {



                        AudioManager.Instance?.PlayClick();



                        ShowToast("Vũ khí: Dép Tổ Ong (Sát thương 1 HP • Tốc độ bay nhanh)");



                    });



                }



            }







            if (btnSubmitCreateRoom != null)
                btnSubmitCreateRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    if (!NetConfig.IsOnlineActive)
                    {
                        ShowErrorToast("Chưa kết nối máy chủ. Vui lòng mở bản trực tuyến.");
                        return;
                    }
                    if (_roomRequestPending) return;
                    EnsureOnlineClient();
                    _suppressRoomState = false;
                    _roomRequestPending = true;
                    ShowToast("Đang tạo phòng trực tuyến...", 2.0f);
                    StartCoroutine(SubmitRoomRequest(null));
                });

            if (btnBackFromCreate != null)



            {



                btnBackFromCreate.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    if (_roomRequestPending)
                    {
                        _suppressRoomState = true;
                        _onlineClient?.LeaveRoom();
                    }
                    CancelRoomRequest();
                    ShowScreen(ProductionScreen.Home);



                });



            }







            if (btnPasteCode != null)
                btnPasteCode.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    string clipboard = GUIUtility.systemCopyBuffer;
                    if (inputRoomCode != null && !string.IsNullOrWhiteSpace(clipboard))
                        inputRoomCode.text = clipboard.Trim();
                    else
                    {
                        ShowToast("Giữ ô mã phòng để dán mã đã sao chép.", 2.5f);
                        inputRoomCode?.ActivateInputField();
                    }
                });

            if (btnScanQr != null)
                btnScanQr.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowToast("Quét QR chưa khả dụng. Hãy nhập mã phòng.", 2.5f);
                    inputRoomCode?.ActivateInputField();
                });

            if (btnSubmitJoinRoom != null)
                btnSubmitJoinRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    if (!RoomCode.TryNormalize(inputRoomCode != null ? inputRoomCode.text : null, out string code))
                    {
                        ShowErrorToast("Mã phòng phải có 6 ký tự hợp lệ.");
                        return;
                    }
                    if (!NetConfig.IsOnlineActive)
                    {
                        ShowErrorToast("Chưa kết nối máy chủ. Vui lòng mở bản trực tuyến.");
                        return;
                    }
                    if (_roomRequestPending) return;
                    EnsureOnlineClient();
                    _suppressRoomState = false;
                    _roomRequestPending = true;
                    if (inputRoomCode != null) inputRoomCode.SetTextWithoutNotify(code);
                    ShowToast($"Đang kết nối vào phòng {code}...", 2.0f);
                    StartCoroutine(SubmitRoomRequest(code));
                });

            if (btnBackFromJoin != null)



            {



                btnBackFromJoin.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    if (_roomRequestPending)
                    {
                        _suppressRoomState = true;
                        _onlineClient?.LeaveRoom();
                    }
                    CancelRoomRequest();
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



                    ShowLeaveConfirmationDialog("XÁC NHẬN RỜI PHÒNG", "Bạn có chắc chắn muốn rời phòng không?\nMọi kết quả của trận đấu sẽ không được lưu.", () =>



                    {



                        if (NetConfig.IsOnlineActive && _onlineClient != null && _onlineClient.IsConnected)



                        {



                            _onlineClient.LeaveRoom();



                        }



                        if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);



                        ShowScreen(ProductionScreen.Home);



                    });



                });



            }







            if (btnConfirmLeave != null)



            {



                btnConfirmLeave.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    ConfirmLeaveDialog();



                });



            }







            if (btnCancelLeave != null)



            {



                btnCancelLeave.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    HideLeaveConfirmationDialog();



                });



            }







            if (btnInGameReady != null)



            {



                btnInGameReady.onClick.AddListener(() =>



                {



                    if (matchController != null)



                    {



                        matchController.RequestReady();



                        if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = UiStrings.BtnLocked;



                        if (btnInGameReady != null)



                        {



                            btnInGameReady.interactable = false;



                            var btnImg = btnInGameReady.GetComponent<Image>();



                            if (btnImg != null && theme != null && theme.btnCtaBlue != null)



                            {



                                btnImg.sprite = theme.btnCtaBlue;



                            }



                        }



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







            // Lô 5: Wire Settings dialog listeners



            if (btnHomeSettings != null)



            {



                btnHomeSettings.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    ShowSettingsDialog();



                });



            }







            if (btnInGameSettings != null)



            {



                btnInGameSettings.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    ShowSettingsDialog();



                });



            }







            if (btnCloseSettings != null)



            {



                btnCloseSettings.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    HideSettingsDialog();



                });



            }







            if (btnLanguageVi != null)



            {



                btnLanguageVi.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    SetLanguageFromSettings(AppLanguage.Vietnamese);



                });



            }







            if (btnLanguageEn != null)



            {



                btnLanguageEn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    SetLanguageFromSettings(AppLanguage.English);



                });



            }







            if (sliderMusicVolume != null)



            {



                sliderMusicVolume.onValueChanged.AddListener(v =>



                {



                    if (Audio.AudioService.Instance != null) Audio.AudioService.Instance.MusicVolume = v;



                    if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = v;



                });



            }







            if (sliderSfxVolume != null)



            {



                sliderSfxVolume.onValueChanged.AddListener(v =>



                {



                    if (Audio.AudioService.Instance != null) Audio.AudioService.Instance.SfxVolume = v;



                    if (AudioManager.Instance != null) AudioManager.Instance.SfxVolume = v;



                });



            }







            if (toggleReducedMotion != null)



            {



                toggleReducedMotion.onValueChanged.AddListener(isOn =>



                {



                    if (juiceFeedback != null) juiceFeedback.ReducedMotion = isOn;



                });



            }







            if (btnLeaveMatchFromSettings != null)



            {



                btnLeaveMatchFromSettings.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    HideSettingsDialog();



                    ShowLeaveConfirmationDialog("XÁC NHẬN RỜI TRẬN", "Bạn có chắc chắn muốn rời trận không?", () =>



                    {



                        OnBackToHomeClicked();



                    });



                });



            }







            if (btnTutorialSkip != null)



            {



                btnTutorialSkip.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    SkipTutorial();



                });



            }







            if (btnTutorialNext != null)



            {



                btnTutorialNext.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    NextTutorialStep();



                });



            }







            // Lô 7 & 8: Home navigation and dialog buttons



            if (btnHomeProfile != null) btnHomeProfile.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); ShowProfileDialog(); });



            if (btnCloseProfile != null) btnCloseProfile.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); HideProfileDialog(); });



            if (btnProfileRename != null) btnProfileRename.onClick.AddListener(() => { OnRenameAccount(inputProfileNewName != null ? inputProfileNewName.text : ""); });







            if (btnHomeShop != null) btnHomeShop.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); ShowShopDialog(); });



            if (btnCloseShop != null) btnCloseShop.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); HideShopDialog(); });







            if (btnHomeInventory != null) btnHomeInventory.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); ShowInventoryDialog(); });



            if (btnCloseInventory != null) btnCloseInventory.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); HideInventoryDialog(); });







            if (btnHomeLeaderboard != null) btnHomeLeaderboard.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); ShowLeaderboardDialog(); });



            if (btnCloseLeaderboard != null) btnCloseLeaderboard.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); HideLeaderboardDialog(); });







            if (btnHomeLogin != null) btnHomeLogin.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); ShowLoginDialog(); });



            if (btnCloseLogin != null) btnCloseLogin.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); HideLoginDialog(); });



            if (btnSubmitGuestLogin != null) btnSubmitGuestLogin.onClick.AddListener(() => { OnSubmitGuestLogin(inputLoginGuestName != null ? inputLoginGuestName.text : ""); });







            // Character cycling & weapon inventory tap on Home



            if (imgHomeCharPortrait != null)



            {



                var charBtn = imgHomeCharPortrait.GetComponent<Button>() ?? imgHomeCharPortrait.gameObject.AddComponent<Button>();



                charBtn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    CycleHomeCharacter();



                });



                // EnsureHomeCharArrows(); -- HOME-04: Use 3D gold circle buttons from setup



            }



            if (txtHomeCharName != null)



            {



                var nameBtn = txtHomeCharName.GetComponent<Button>() ?? txtHomeCharName.gameObject.AddComponent<Button>();



                nameBtn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    CycleHomeCharacter();



                });



            }



            var wepPill = homePanel != null ? homePanel.transform.Find("LoadoutCard/WeaponPillHome") : null;



            if (wepPill != null)



            {



                var wepBtn = wepPill.GetComponent<Button>() ?? wepPill.gameObject.AddComponent<Button>();



                wepBtn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    ShowInventoryDialog();



                });



            }







            // Lô 4: Wire juice feedback — register all buttons for press-scale



            if (juiceFeedback != null)



            {



                var canvas = GetComponentInParent<Canvas>();



                if (canvas != null)



                {



                    juiceFeedback.RegisterAllButtons(canvas.transform);



                }



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



            if (joinRoomPanel != null)



            {



                joinRoomPanel.SetActive(screen == ProductionScreen.JoinRoom);



                if (screen == ProductionScreen.JoinRoom)
                {
                    var legacyKeypad = joinRoomPanel.transform.Find("KeypadContainer");
                    if (legacyKeypad != null) legacyKeypad.gameObject.SetActive(false);
                    UpdateJoinRoomSearchStatus(inputRoomCode != null ? inputRoomCode.text : string.Empty);
                }



            }



            if (lobbyPanel != null) lobbyPanel.SetActive(screen == ProductionScreen.Lobby);



            if (botTrainingPanel != null) botTrainingPanel.SetActive(screen == ProductionScreen.BotTraining);



            if (gameplayHudPanel != null) gameplayHudPanel.SetActive(screen == ProductionScreen.Gameplay);



            if (screen == ProductionScreen.Gameplay)



            {



                EnsureInGameEmoteTray();



            }



            else



            {



                if (_inGameEmoteTray != null) _inGameEmoteTray.SetActive(false);



            }



            if (resultPanel != null) resultPanel.SetActive(screen == ProductionScreen.Result);







            if (screen != ProductionScreen.Gameplay && roundResultOverlay != null)



            {



                roundResultOverlay.SetActive(false);



            }







            if (dialogLeaveRoom != null && dialogLeaveRoom.activeSelf)



            {



                HideLeaveConfirmationDialog();



            }







            if (txtBuildFingerprint != null)



            {



                txtBuildFingerprint.transform.SetAsLastSibling();



            }







            if (screen == ProductionScreen.Home && AudioManager.Instance != null)



            {



                AudioManager.Instance.PlayMenuBgm();



            }



        }







        public void ShowLeaveConfirmationDialog(string title, string message, System.Action onConfirm)



        {



            _pendingLeaveAction = onConfirm;



            if (txtLeaveDialogTitle != null) txtLeaveDialogTitle.text = title;



            if (txtLeaveDialogMessage != null) txtLeaveDialogMessage.text = message;



            if (dialogLeaveRoom != null)



            {



                dialogLeaveRoom.transform.SetAsLastSibling();



                dialogLeaveRoom.SetActive(true);



            }



        }







        public void HideLeaveConfirmationDialog()



        {



            _pendingLeaveAction = null;



            if (dialogLeaveRoom != null)



            {



                dialogLeaveRoom.SetActive(false);



            }



        }







        public void ConfirmLeaveDialog()



        {



            var action = _pendingLeaveAction;



            HideLeaveConfirmationDialog();



            action?.Invoke();



        }







        // ──────────────────────────────────────────────────



        // Lô 5: Settings Dialog (Overlay - Does not pause match)



        // ──────────────────────────────────────────────────







        public void ShowSettingsDialog()



        {



            if (dialogSettings != null)



            {



                dialogSettings.transform.SetAsLastSibling();



                dialogSettings.SetActive(true);



            }



            RefreshSettingsUI();



        }







        public void HideSettingsDialog()



        {



            if (dialogSettings != null)



            {



                dialogSettings.SetActive(false);



            }



        }







        public void RefreshSettingsUI()



        {



            if (txtSettingsTitle != null) txtSettingsTitle.text = UiStrings.SettingsTitle;







            var audioSvc = Audio.AudioService.Instance;



            if (audioSvc != null)



            {



                if (sliderMusicVolume != null) sliderMusicVolume.value = audioSvc.MusicVolume;



                if (sliderSfxVolume != null) sliderSfxVolume.value = audioSvc.SfxVolume;



                if (toggleMusic != null) toggleMusic.isOn = !audioSvc.MusicMuted;



                if (toggleSfx != null) toggleSfx.isOn = !audioSvc.SfxMuted;



            }







            if (toggleReducedMotion != null && juiceFeedback != null)



            {



                toggleReducedMotion.isOn = juiceFeedback.ReducedMotion;



            }







            if (btnLeaveMatchFromSettings != null)



            {



                btnLeaveMatchFromSettings.gameObject.SetActive(CurrentScreen == ProductionScreen.Gameplay);



            }







            EnsureSettingsReportBugAndBuildVersion();



        }







        public void EnsureSettingsReportBugAndBuildVersion()



        {



            if (dialogSettings == null) return;







            var card = dialogSettings.transform.childCount > 0 ? dialogSettings.transform.GetChild(0) : dialogSettings.transform;







            if (btnReportBug == null)



            {



                var existing = card.Find("BtnReportBug");



                if (existing != null)



                {



                    btnReportBug = existing.GetComponent<Button>();



                }



                else



                {



                    var bugObj = new GameObject("BtnReportBug", typeof(RectTransform));



                    bugObj.transform.SetParent(card, false);



                    var rt = bugObj.GetComponent<RectTransform>();



                    rt.anchorMin = new Vector2(0.5f, 0.21f);



                    rt.anchorMax = new Vector2(0.5f, 0.21f);



                    rt.pivot = new Vector2(0.5f, 0.5f);



                    rt.sizeDelta = new Vector2(440f, 75f);







                    var img = bugObj.AddComponent<Image>();



                    var btnSprite = btnCloseSettings != null ? btnCloseSettings.GetComponent<Image>()?.sprite : null;



                    if (btnSprite != null)



                    {



                        img.sprite = btnSprite;



                        img.type = Image.Type.Sliced;



                        img.color = new Color(0.96f, 0.65f, 0.12f, 1f);



                    }



                    else



                    {



                        img.color = new Color(0.85f, 0.35f, 0.12f, 0.95f);



                    }







                    btnReportBug = bugObj.AddComponent<Button>();



                    btnReportBug.targetGraphic = img;







                    var txtObj = new GameObject("Text", typeof(RectTransform));



                    txtObj.transform.SetParent(bugObj.transform, false);



                    var txtRt = txtObj.GetComponent<RectTransform>();



                    txtRt.anchorMin = Vector2.zero;



                    txtRt.anchorMax = Vector2.one;



                    txtRt.sizeDelta = Vector2.zero;







                    var txt = txtObj.AddComponent<TextMeshProUGUI>();



                    txt.font = GetDefaultFont();



                    txt.fontSize = 26f;



                    txt.fontStyle = FontStyles.Bold;



                    txt.alignment = TextAlignmentOptions.Center;



                    txt.color = Color.white;



                    txt.text = UiStrings.Get(UiStrings.SettingsBtnReportBug);



                }



            }







            if (btnReportBug != null)



            {



                var rt = btnReportBug.GetComponent<RectTransform>();



                rt.anchorMin = new Vector2(0.5f, 0.21f);



                rt.anchorMax = new Vector2(0.5f, 0.21f);



                rt.pivot = new Vector2(0.5f, 0.5f);



                rt.sizeDelta = new Vector2(440f, 75f);







                var img = btnReportBug.GetComponent<Image>();



                if (img != null && btnCloseSettings != null && img.sprite == null)



                {



                    img.sprite = btnCloseSettings.GetComponent<Image>()?.sprite;



                    img.type = Image.Type.Sliced;



                    img.color = new Color(0.96f, 0.65f, 0.12f, 1f);



                }







                btnReportBug.onClick.RemoveAllListeners();



                btnReportBug.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    OnReportBugClicked();



                });







                var txt = btnReportBug.GetComponentInChildren<TextMeshProUGUI>();



                if (txt != null)



                {



                    txt.text = UiStrings.Get(UiStrings.SettingsBtnReportBug);



                }



            }







            if (btnCloseSettings != null)



            {



                var rtClose = btnCloseSettings.GetComponent<RectTransform>();



                rtClose.anchorMin = new Vector2(0.5f, 0.125f);



                rtClose.anchorMax = new Vector2(0.5f, 0.125f);



                rtClose.pivot = new Vector2(0.5f, 0.5f);



                rtClose.sizeDelta = new Vector2(440f, 75f);



            }







            if (btnLeaveMatchFromSettings != null)



            {



                var rtLeave = btnLeaveMatchFromSettings.GetComponent<RectTransform>();



                rtLeave.anchorMin = new Vector2(0.5f, 0.045f);



                rtLeave.anchorMax = new Vector2(0.5f, 0.045f);



                rtLeave.pivot = new Vector2(0.5f, 0.5f);



                rtLeave.sizeDelta = new Vector2(440f, 65f);



                btnLeaveMatchFromSettings.gameObject.SetActive(CurrentScreen == ProductionScreen.Gameplay);



            }







            if (txtSettingsBuildVersion == null)



            {



                var existing = card.Find("TxtSettingsBuildVersion");



                if (existing != null)



                {



                    txtSettingsBuildVersion = existing.GetComponent<TextMeshProUGUI>();



                }



                else



                {



                    var verObj = new GameObject("TxtSettingsBuildVersion", typeof(RectTransform));



                    verObj.transform.SetParent(card, false);



                    var rt = verObj.GetComponent<RectTransform>();



                    rt.anchorMin = new Vector2(0.05f, 0.005f);



                    rt.anchorMax = new Vector2(0.95f, 0.025f);



                    rt.sizeDelta = Vector2.zero;







                    txtSettingsBuildVersion = verObj.AddComponent<TextMeshProUGUI>();



                    txtSettingsBuildVersion.font = GetDefaultFont();



                    txtSettingsBuildVersion.fontSize = 14f;



                    txtSettingsBuildVersion.alignment = TextAlignmentOptions.Center;



                    txtSettingsBuildVersion.color = new Color(0.7f, 0.75f, 0.85f, 0.8f);



                }



            }



            else



            {



                var rt = txtSettingsBuildVersion.rectTransform;



                rt.anchorMin = new Vector2(0.05f, 0.005f);



                rt.anchorMax = new Vector2(0.95f, 0.025f);



                rt.sizeDelta = Vector2.zero;



            }







            if (txtSettingsBuildVersion != null)



            {



                string fp = ResolveBuildFingerprint();



                txtSettingsBuildVersion.text = (UiStrings.CurrentLanguage == AppLanguage.English ? "Build: " : "Phiên bản: ") + fp;



            }



        }







        public void OnReportBugClicked()



        {



            string fp = ResolveBuildFingerprint();



            string lang = UiStrings.CurrentLanguage.ToString();



            string screen = CurrentScreen.ToString();



            string room = !string.IsNullOrEmpty(_activeRoomCode) ? _activeRoomCode : "N/A";







            string report = HitMe.UI.RingBufferLogger.BuildBugReport(fp, lang, screen, room);



            try



            {



                GUIUtility.systemCopyBuffer = report;



            }



            catch (Exception ex)



            {



                Debug.LogWarning("[ProductionUIController] Failed to copy to clipboard: " + ex.Message);



            }







            ShowToast(UiStrings.Get(UiStrings.ReportBugCopiedToast));



        }







        public void SetLanguageFromSettings(AppLanguage lang)



        {



            UiStrings.SetLanguage(lang);



            RefreshSettingsUI();



            RefreshAllScreenTexts();



        }







        public void RefreshAllScreenTexts()



        {



            try



            {



                // 1. Enforce fonts



                EnforceFontAcrossAllTexts();







                // 2. Home screen labels & buttons



                if (txtHomeLogo != null)



                {



                    var logoImg = txtHomeLogo.GetComponent<Image>() ?? txtHomeLogo.transform.parent?.GetComponent<Image>();



                    if (logoImg != null && logoImg.sprite != null)



                        txtHomeLogo.text = "";



                    else



                        txtHomeLogo.text = UiStrings.GameTitle;



                }



                var subTxt = homePanel != null ? homePanel.transform.Find("HomeSubtitle")?.GetComponent<TextMeshProUGUI>() : null;



                if (subTxt != null) subTxt.text = UiStrings.Get(UiStrings.HomeSubtitle);







                UpdateBtnText(btnQuickMatch, UiStrings.Get(UiStrings.BtnPlayNow));



                UpdateBtnText(btnCreateRoom, UiStrings.Get(UiStrings.BtnCreateRoom));



                UpdateBtnText(btnJoinRoom, UiStrings.Get(UiStrings.BtnJoinRoom));



                UpdateBtnText(btnBotTraining, UiStrings.Get(UiStrings.BtnPracticeBots));







                UpdateBtnText(btnHomeShop, UiStrings.Get(UiStrings.ShopTitle));



                UpdateBtnText(btnHomeInventory, UiStrings.Get(UiStrings.InventoryTitle));



                UpdateBtnText(btnHomeLeaderboard, UiStrings.Get("BẢNG XẾP HẠNG"));



                // HOME-07: Tab 1 must remain "Trang chủ"

                if (btnHomeLogin != null)

                {

                    var tmp = btnHomeLogin.GetComponentInChildren<TextMeshProUGUI>();

                    if (tmp != null) tmp.text = "Trang chủ";

                }



                // btnHomeSettings is a gear icon button, do not overwrite with text







                // 3. Bot Training Panel



                RefreshBotTrainingDisplay();







                // 4. Lobby Panel



                if (txtLobbyHeader != null)



                {



                    txtLobbyHeader.text = $"{UiStrings.Get(UiStrings.LobbyTitle)} ({_currentLobbyHumanCount}/6 {UiStrings.Get("NGƯỜI THẬT")})";



                }



                UpdateBtnText(btnLobbyReady, _isLocalPlayerReady ? UiStrings.Get(UiStrings.BtnLocked) : UiStrings.Get(UiStrings.BtnReady));



                UpdateBtnText(btnLeaveRoom, UiStrings.Get(UiStrings.BtnLeaveRoom));



                UpdateBtnText(btnLobbyToBotTraining, UiStrings.Get(UiStrings.BtnGoToBotTraining));



                if (txtLobbyCountdown != null)



                {



                    if (_currentLobbyHumanCount <= 1)



                        txtLobbyCountdown.text = UiStrings.Get(UiStrings.LobbyOnePlayerWarning);



                    else



                        txtLobbyCountdown.text = UiStrings.Get(UiStrings.LobbyWaitingAllReady);



                }







                // 5. Gameplay HUD



                if (txtInGameReadyLabel != null)



                {



                    txtInGameReadyLabel.text = _isLocalPlayerReady ? UiStrings.Get(UiStrings.BtnLocked) : UiStrings.Get(UiStrings.BtnReady);



                }



                if (txtRound != null && _lastDisplayedRound > 0)



                {



                    txtRound.text = string.Format(UiStrings.Get("VÒNG {0}/{1}"), _lastDisplayedRound, _lastDisplayedMaxRounds);



                }



                if (txtPhaseBanner != null && matchController != null && matchController.State != null)



                {



                    ApplyPhaseState(matchController.State.Phase);



                }







                // 6. Result Screen



                if (txtResultTitle != null) txtResultTitle.text = UiStrings.Get(UiStrings.MatchResultTitle);



                UpdateBtnText(btnReplay, UiStrings.Get(UiStrings.BtnReplay));



                UpdateBtnText(btnBackToHome, UiStrings.Get(UiStrings.BtnBackToHome));







                // 7. Dialogs & Overlays



                RefreshSettingsUI();



                RefreshTutorialUI();



                RefreshProfileUI();



                RefreshShopUI();



                RefreshInventoryUI();



                RefreshLeaderboardUI();



                RefreshLoginUI();



                RefreshHomeLangToggleVisual();



            }



            catch (Exception ex)



            {



                Debug.LogWarning($"[ProductionUIController] RefreshAllScreenTexts caught: {ex.Message}");



            }



        }







        private static void UpdateBtnText(Button btn, string text)



        {



            if (btn == null) return;



            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();



            if (tmp != null) tmp.text = text;



        }







        public TMP_FontAsset GetDefaultFont()



        {



            if (theme != null && theme.defaultFont != null) return theme.defaultFont;



            if (txtHomeLogo != null && txtHomeLogo.font != null) return txtHomeLogo.font;



            if (txtRound != null && txtRound.font != null) return txtRound.font;



            if (txtLobbyHeader != null && txtLobbyHeader.font != null) return txtLobbyHeader.font;



            return Resources.Load<TMP_FontAsset>("Fonts/BeVietnamPro-Bold SDF");



        }







        public void EnforceFontAcrossAllTexts()



        {



            try



            {



                var targetFont = GetDefaultFont();



                if (targetFont == null) return;



                var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);



                for (int i = 0; i < allTexts.Length; i++)



                {



                    var txt = allTexts[i];



                    if (txt != null)



                    {



                        if (txt.font == null || (txt.font.name != null && txt.font.name.Contains("LiberationSans")))



                        {



                            txt.font = targetFont;



                        }



                    }



                }



            }



            catch (Exception ex)



            {



                Debug.LogWarning($"[ProductionUIController] EnforceFont caught: {ex.Message}");



            }



        }







        public void EnsureHomeLanguageToggle() { /* HOME compliance: disabled */ }







        public void RefreshHomeLangToggleVisual()



        {



            if (_txtHomeLangToggle != null)



            {



                _txtHomeLangToggle.text = UiStrings.CurrentLanguage == AppLanguage.Vietnamese ? "VI | en" : "vi | EN";



            }



        }







        public void CycleHomeCharacter()



        {



            int next = _selectedPracticeCharIndex;



            for (int k = 0; k < 7; k++)



            {



                next = (next + 1) % 7;



                if (HitMe.Data.CharacterRegistry.GetByIndex(next).isEnabled)



                {



                    _selectedPracticeCharIndex = next;



                    break;



                }



            }



            RefreshBotTrainingDisplay();



            var names = UiStrings.GetLocalizedCharacterNames();



            if (_selectedPracticeCharIndex < names.Length)



            {



                ShowToast(names[_selectedPracticeCharIndex]);



            }



        }







        public void EnsureHomeCharArrows() { /* HOME compliance: disabled */ }







        // ──────────────────────────────────────────────────



        // Tutorial 3-Step Gesture System (Lô 6)



        // ──────────────────────────────────────────────────



        public int CurrentTutorialStep => _currentTutorialStep;







        public void ShowTutorial(int step = 0)



        {



            _currentTutorialStep = Mathf.Clamp(step, 0, 2);



            if (dialogTutorial != null)



            {



                dialogTutorial.transform.SetAsLastSibling();



                dialogTutorial.SetActive(true);



            }



            RefreshTutorialUI();



        }







        public void NextTutorialStep()



        {



            if (_currentTutorialStep < 2)



            {



                _currentTutorialStep++;



                RefreshTutorialUI();



            }



            else



            {



                CompleteTutorial();



            }



        }







        public void SkipTutorial()



        {



            CompleteTutorial();



        }







        public void CompleteTutorial()



        {



            try



            {



                PlayerPrefs.SetString(UiStrings.PrefKeyTutorialCompleted, "1");



                PlayerPrefs.Save();



            }



            catch { /* WebGL sandbox */ }







            if (dialogTutorial != null)



            {



                dialogTutorial.SetActive(false);



            }



        }







        public void RefreshTutorialUI()



        {



            if (txtTutorialTitle != null) txtTutorialTitle.text = UiStrings.Get(UiStrings.TutorialTitle);



            if (txtTutorialStepHeader != null)



            {



                string headerKey = _currentTutorialStep == 0 ? UiStrings.TutorialStep1Title



                    : (_currentTutorialStep == 1 ? UiStrings.TutorialStep2Title : UiStrings.TutorialStep3Title);



                txtTutorialStepHeader.text = UiStrings.Get(headerKey);



            }



            if (txtTutorialStepBody != null)



            {



                string bodyKey = _currentTutorialStep == 0 ? UiStrings.TutorialStep1Body



                    : (_currentTutorialStep == 1 ? UiStrings.TutorialStep2Body : UiStrings.TutorialStep3Body);



                txtTutorialStepBody.text = UiStrings.Get(bodyKey);



            }



            if (txtTutorialNextLabel != null)



            {



                txtTutorialNextLabel.text = _currentTutorialStep == 2



                    ? UiStrings.Get(UiStrings.BtnTutorialDone)



                    : UiStrings.Get(UiStrings.BtnTutorialNext);



            }



            if (tutorialStepIndicators != null)



            {



                for (int i = 0; i < tutorialStepIndicators.Length; i++)



                {



                    if (tutorialStepIndicators[i] != null)



                    {



                        var img = tutorialStepIndicators[i].GetComponent<Image>();



                        if (img != null)



                        {



                            img.color = i == _currentTutorialStep



                                ? new Color(1f, 0.85f, 0.20f, 1f)



                                : new Color(1f, 1f, 1f, 0.35f);



                        }



                    }



                }



            }



        }







        // ──────────────────────────────────────────────────



        // Lô 7: Profile, Shop, Inventory, Leaderboard



        // Lô 8: Guest Login



        // L1 / L6: Online Account State & Server Readiness



        // ──────────────────────────────────────────────────



        private bool _isOnlineAccountLoaded = false;



        public bool IsOnlineAccountLoaded



        {



            get => _isOnlineAccountLoaded;



            set



            {



                _isOnlineAccountLoaded = value;



                RefreshAccountDisplay();



                RefreshProfileUI();



                RefreshLeaderboardUI();



            }



        }







        public bool IsOnlineActiveMode => HitMe.NetClientV1.NetConfig.IsOnlineActive || (_onlineClient != null && _onlineClient.IsConnected);







        public void RefreshAccountDisplay()



        {



            var acc = Game.FakeAccountService.Instance;



            if (acc == null) return;







            if (txtHomePlayerName != null) txtHomePlayerName.text = acc.DisplayName;



            if (IsOnlineActiveMode && !_isOnlineAccountLoaded)



            {



                if (txtHomeGold != null) txtHomeGold.text = UiStrings.Get(UiStrings.LoadingData);



                if (txtHomeGems != null) txtHomeGems.text = "-";



            }



            else



            {



                if (txtHomeGold != null) txtHomeGold.text = acc.Gold.ToString() + " G";



                if (txtHomeGems != null) txtHomeGems.text = acc.Gems.ToString();



            }



        }







        public void ShowProfileDialog()



        {



            if (dialogProfile != null)



            {



                dialogProfile.transform.SetAsLastSibling();



                dialogProfile.SetActive(true);



            }



            RefreshProfileUI();



        }







        public void HideProfileDialog()



        {



            if (dialogProfile != null) dialogProfile.SetActive(false);



        }







        public void RefreshProfileUI()



        {



            var acc = Game.FakeAccountService.Instance;



            if (acc == null) return;







            RankService.EnsureInitialized();



            var tier = RankService.GetTierForPoints(acc.RankPoints);



            string tierName = tier != null ? tier.GetLocalizedName(!UiStrings.IsVietnamese) : "Đồng";



            string tierBadge = tier != null ? tier.badge : "";







            if (txtProfileTitle != null) txtProfileTitle.text = UiStrings.Get(UiStrings.ProfileTitle);



            if (txtProfilePlayerName != null) txtProfilePlayerName.text = acc.DisplayName;



            if (txtProfilePlayerId != null) txtProfilePlayerId.text = "ID: " + acc.PlayerId;







            if (txtProfileGold != null)



            {



                if (IsOnlineActiveMode && !_isOnlineAccountLoaded)



                {



                    txtProfileGold.text = $"{UiStrings.Get(UiStrings.LoadingData)} • {acc.MatchesPlayed} Trận";



                }



                else



                {



                    txtProfileGold.text = $"{acc.Gold} VÀNG • {tierName} ({acc.RankPoints} Đ) • {acc.MatchesPlayed} Trận";



                }



            }







            if (txtProfileRenameCost != null)



            {



                txtProfileRenameCost.text = acc.NameChangeCount == 0



                    ? UiStrings.Get(UiStrings.FreeRenameLabel)



                    : "Đổi tên lần 2+: 99 VÀNG";



            }



        }







        public void ShowShopDialog()



        {



            if (dialogShop != null)



            {



                dialogShop.transform.SetAsLastSibling();



                dialogShop.SetActive(true);



            }



            RefreshShopUI();



        }







        public void HideShopDialog()



        {



            if (dialogShop != null) dialogShop.SetActive(false);



        }







        public void RefreshShopUI()



        {



            var acc = Game.FakeAccountService.Instance;



            ItemCatalog.EnsureInitialized();



            var items = ItemCatalog.GetItems();







            if (txtShopTitle != null) txtShopTitle.text = UiStrings.Get(UiStrings.ShopTitle);



            if (txtShopSubtitle != null) txtShopSubtitle.text = UiStrings.Get(UiStrings.ShopSubtitle);



            if (txtShopGold != null && acc != null) txtShopGold.text = acc.Gold.ToString() + " VÀNG";







            var shopCard = dialogShop != null && dialogShop.transform.childCount > 0 ? dialogShop.transform.GetChild(0) : null;



            if (shopCard != null)



            {



                for (int i = 0; i < items.Count && i < 7; i++)



                {



                    var item = items[i];



                    var rowTr = shopCard.Find("ShopRow" + i);



                    if (rowTr != null)



                    {



                        bool isOwned = ItemCatalog.IsOwned(item.id);



                        bool isEquipped = ItemCatalog.EquippedWeaponId == item.id;



                        bool isMissingSprite = item.id == "coc_bia_hoi" || item.id == "xe_dap_phuong_hoang" || item.id == "xe_may_honda_cub" || item.id == "chu_cho_vang";







                        var txtName = rowTr.Find("ItemName")?.GetComponent<TextMeshProUGUI>();



                        if (txtName != null)



                        {



                            string rarityTag = $"[{item.rarity}]";



                            string missingTag = isMissingSprite ? " [THIẾU ẢNH]" : "";



                            string displayName = item.GetLocalizedName(!UiStrings.IsVietnamese);



                            txtName.text = $"{displayName} ({item.price}G) {rarityTag}{missingTag}";



                        }







                        var btnAction = rowTr.Find("BtnAction")?.GetComponent<Button>();



                        var txtBtn = btnAction != null ? btnAction.transform.Find("Label")?.GetComponent<TextMeshProUGUI>() : null;



                        if (btnAction != null && txtBtn != null)



                        {



                            btnAction.onClick.RemoveAllListeners();



                            if (isEquipped)



                            {



                                txtBtn.text = UiStrings.Get(UiStrings.BtnEquipped);



                                btnAction.interactable = false;



                            }



                            else if (isOwned)



                            {



                                txtBtn.text = "TRANG BỊ";



                                btnAction.interactable = true;



                                string idCopy = item.id;



                                btnAction.onClick.AddListener(() => OnEquipWeapon(idCopy));



                            }



                            else



                            {



                                txtBtn.text = $"{item.price}G {UiStrings.Get(UiStrings.BtnBuy)}";



                                btnAction.interactable = acc != null && acc.Gold >= item.price;



                                string idCopy = item.id;



                                btnAction.onClick.AddListener(() => OnBuyWeapon(idCopy));



                            }



                        }



                    }



                }



            }



        }







        public void ShowInventoryDialog()



        {



            if (dialogInventory != null)



            {



                dialogInventory.transform.SetAsLastSibling();



                dialogInventory.SetActive(true);



            }



            RefreshInventoryUI();



        }







        public void HideInventoryDialog()



        {



            if (dialogInventory != null) dialogInventory.SetActive(false);



        }







        public void RefreshInventoryUI()



        {



            ItemCatalog.EnsureInitialized();



            string eqId = ItemCatalog.EquippedWeaponId;



            var eqItem = ItemCatalog.GetItem(eqId);







            if (txtInventoryTitle != null) txtInventoryTitle.text = UiStrings.Get(UiStrings.InventoryTitle);



            if (txtInventoryEquippedWeapon != null)



            {



                string eqName = eqItem != null ? eqItem.GetLocalizedName(!UiStrings.IsVietnamese) : UiStrings.WeaponSandalName;



                txtInventoryEquippedWeapon.text = UiStrings.Get(UiStrings.BtnEquipped) + ": " + eqName;



            }







            var invCard = dialogInventory != null && dialogInventory.transform.childCount > 0 ? dialogInventory.transform.GetChild(0) : null;



            if (invCard != null)



            {



                var slot0 = invCard.Find("Slot0");



                if (slot0 != null)



                {



                    var nameTxt = slot0.Find("Name")?.GetComponent<TextMeshProUGUI>();



                    if (nameTxt != null)



                    {



                        string eqName = eqItem != null ? eqItem.GetLocalizedName(!UiStrings.IsVietnamese) : "DÉP TỔ ONG";



                        string rarity = eqItem != null ? $"[{eqItem.rarity}]" : "[Common]";



                        nameTxt.text = $"{eqName} {rarity} (Đang dùng)";



                    }



                }



            }



        }







        public void ShowLeaderboardDialog()



        {



            if (dialogLeaderboard != null)



            {



                dialogLeaderboard.transform.SetAsLastSibling();



                dialogLeaderboard.SetActive(true);



                EnsureLeaderboardTabsAndPlaceholder();



            }



            RefreshLeaderboardUI();



        }







        public void HideLeaderboardDialog()



        {



            if (dialogLeaderboard != null) dialogLeaderboard.SetActive(false);



        }







        public void EnsureLeaderboardTabsAndPlaceholder()



        {



            if (dialogLeaderboard == null) return;



            var modalBox = dialogLeaderboard.transform.childCount > 0 ? dialogLeaderboard.transform.GetChild(0) : dialogLeaderboard.transform;







            if (_btnTabLeaderboard == null)



            {



                // Tab Header Bar



                var tabContainer = new GameObject("TabHeaderBar", typeof(RectTransform));



                tabContainer.transform.SetParent(modalBox, false);



                var tcRt = tabContainer.GetComponent<RectTransform>();



                tcRt.anchorMin = new Vector2(0.10f, 0.77f);



                tcRt.anchorMax = new Vector2(0.90f, 0.85f);



                tcRt.sizeDelta = Vector2.zero;







                // Tab 1: Leaderboard



                var tabLbObj = new GameObject("BtnTabLeaderboard", typeof(RectTransform));



                tabLbObj.transform.SetParent(tabContainer.transform, false);



                var tabLbRt = tabLbObj.GetComponent<RectTransform>();



                tabLbRt.anchorMin = new Vector2(0f, 0f);



                tabLbRt.anchorMax = new Vector2(0.48f, 1f);



                tabLbRt.sizeDelta = Vector2.zero;



                var imgLb = tabLbObj.AddComponent<Image>();



                imgLb.color = new Color(0.85f, 0.65f, 0.15f);



                _btnTabLeaderboard = tabLbObj.AddComponent<Button>();



                _btnTabLeaderboard.targetGraphic = imgLb;



                _btnTabLeaderboard.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    _currentLeaderboardTab = 0;



                    RefreshLeaderboardUI();



                });







                var txtLbObj = new GameObject("Text", typeof(RectTransform));



                txtLbObj.transform.SetParent(tabLbObj.transform, false);



                var txtLbRt = txtLbObj.GetComponent<RectTransform>();



                txtLbRt.anchorMin = Vector2.zero; txtLbRt.anchorMax = Vector2.one; txtLbRt.sizeDelta = Vector2.zero;



                _txtTabLeaderboardLabel = txtLbObj.AddComponent<TextMeshProUGUI>();



                _txtTabLeaderboardLabel.font = GetDefaultFont();



                _txtTabLeaderboardLabel.fontSize = 18f;



                _txtTabLeaderboardLabel.fontStyle = FontStyles.Bold;



                _txtTabLeaderboardLabel.alignment = TextAlignmentOptions.Center;



                _txtTabLeaderboardLabel.color = new Color(0.08f, 0.12f, 0.2f);



                _txtTabLeaderboardLabel.text = UiStrings.Get("BẢNG XẾP HẠNG");







                // Tab 2: Quests



                var tabQObj = new GameObject("BtnTabQuests", typeof(RectTransform));



                tabQObj.transform.SetParent(tabContainer.transform, false);



                var tabQRt = tabQObj.GetComponent<RectTransform>();



                tabQRt.anchorMin = new Vector2(0.52f, 0f);



                tabQRt.anchorMax = new Vector2(1f, 1f);



                tabQRt.sizeDelta = Vector2.zero;



                var imgQ = tabQObj.AddComponent<Image>();



                imgQ.color = new Color(0.2f, 0.28f, 0.4f);



                _btnTabQuests = tabQObj.AddComponent<Button>();



                _btnTabQuests.targetGraphic = imgQ;



                _btnTabQuests.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    _currentLeaderboardTab = 1;



                    RefreshLeaderboardUI();



                });







                var txtQObj = new GameObject("Text", typeof(RectTransform));



                txtQObj.transform.SetParent(tabQObj.transform, false);



                var txtQRt = txtQObj.GetComponent<RectTransform>();



                txtQRt.anchorMin = Vector2.zero; txtQRt.anchorMax = Vector2.one; txtQRt.sizeDelta = Vector2.zero;



                _txtTabQuestsLabel = txtQObj.AddComponent<TextMeshProUGUI>();



                _txtTabQuestsLabel.font = GetDefaultFont();



                _txtTabQuestsLabel.fontSize = 18f;



                _txtTabQuestsLabel.fontStyle = FontStyles.Bold;



                _txtTabQuestsLabel.alignment = TextAlignmentOptions.Center;



                _txtTabQuestsLabel.color = Color.white;



                _txtTabQuestsLabel.text = UiStrings.Get("NHIỆM VỤ");



            }







            if (_leaderboardPlaceholderObj == null)



            {



                // Placeholder / Quests container



                _leaderboardPlaceholderObj = new GameObject("QuestsContainer", typeof(RectTransform));



                _leaderboardPlaceholderObj.transform.SetParent(modalBox, false);



                var phRt = _leaderboardPlaceholderObj.GetComponent<RectTransform>();



                phRt.anchorMin = new Vector2(0.08f, 0.15f);



                phRt.anchorMax = new Vector2(0.92f, 0.74f);



                phRt.sizeDelta = Vector2.zero;







                var phImg = _leaderboardPlaceholderObj.AddComponent<Image>();



                phImg.color = new Color(0.08f, 0.12f, 0.20f, 0.95f);







                // Quests Title



                var phTitleObj = new GameObject("Title", typeof(RectTransform));



                phTitleObj.transform.SetParent(_leaderboardPlaceholderObj.transform, false);



                var phTitleRt = phTitleObj.GetComponent<RectTransform>();



                phTitleRt.anchorMin = new Vector2(0.05f, 0.86f);



                phTitleRt.anchorMax = new Vector2(0.95f, 0.98f);



                phTitleRt.sizeDelta = Vector2.zero;



                _txtLeaderboardPlaceholderTitle = phTitleObj.AddComponent<TextMeshProUGUI>();



                _txtLeaderboardPlaceholderTitle.font = GetDefaultFont();



                _txtLeaderboardPlaceholderTitle.fontSize = 22f;



                _txtLeaderboardPlaceholderTitle.fontStyle = FontStyles.Bold;



                _txtLeaderboardPlaceholderTitle.alignment = TextAlignmentOptions.Center;



                _txtLeaderboardPlaceholderTitle.color = new Color(1f, 0.85f, 0.25f);



                _txtLeaderboardPlaceholderTitle.text = "NHIỆM VỤ HÀNG NGÀY";







                // 3 Quest Rows



                for (int qi = 0; qi < 3; qi++)



                {



                    float yTop = 0.82f - (qi * 0.23f);



                    float yBot = yTop - 0.20f;



                    var qRow = new GameObject("QuestRow" + qi, typeof(RectTransform));



                    qRow.transform.SetParent(_leaderboardPlaceholderObj.transform, false);



                    var qrRt = qRow.GetComponent<RectTransform>();



                    qrRt.anchorMin = new Vector2(0.04f, yBot);



                    qrRt.anchorMax = new Vector2(0.96f, yTop);



                    qrRt.sizeDelta = Vector2.zero;



                    var qImg = qRow.AddComponent<Image>();



                    qImg.color = new Color(0.14f, 0.20f, 0.32f, 0.90f);







                    var qTextObj = new GameObject("QuestDesc", typeof(RectTransform));



                    qTextObj.transform.SetParent(qRow.transform, false);



                    var qtRt = qTextObj.GetComponent<RectTransform>();



                    qtRt.anchorMin = new Vector2(0.04f, 0.10f);



                    qtRt.anchorMax = new Vector2(0.68f, 0.90f);



                    qtRt.sizeDelta = Vector2.zero;



                    var qtComp = qTextObj.AddComponent<TextMeshProUGUI>();



                    qtComp.font = GetDefaultFont();



                    qtComp.fontSize = 17f;



                    qtComp.alignment = TextAlignmentOptions.MidlineLeft;



                    qtComp.color = Color.white;







                    var qBtnObj = new GameObject("BtnClaim", typeof(RectTransform));



                    qBtnObj.transform.SetParent(qRow.transform, false);



                    var qbRt = qBtnObj.GetComponent<RectTransform>();



                    qbRt.anchorMin = new Vector2(0.70f, 0.15f);



                    qbRt.anchorMax = new Vector2(0.96f, 0.85f);



                    qbRt.sizeDelta = Vector2.zero;



                    var qbImg = qBtnObj.AddComponent<Image>();



                    qbImg.color = new Color(0.2f, 0.65f, 0.35f);



                    var qBtnComp = qBtnObj.AddComponent<Button>();



                    qBtnComp.targetGraphic = qbImg;







                    var qBtnLblObj = new GameObject("Label", typeof(RectTransform));



                    qBtnLblObj.transform.SetParent(qBtnObj.transform, false);



                    var qblRt = qBtnLblObj.GetComponent<RectTransform>();



                    qblRt.anchorMin = Vector2.zero; qblRt.anchorMax = Vector2.one; qblRt.sizeDelta = Vector2.zero;



                    var qblComp = qBtnLblObj.AddComponent<TextMeshProUGUI>();



                    qblComp.font = GetDefaultFont();



                    qblComp.fontSize = 17f;



                    qblComp.fontStyle = FontStyles.Bold;



                    qblComp.alignment = TextAlignmentOptions.Center;



                    qblComp.color = Color.white;



                    qblComp.text = "NHẬN";



                }







                // Reset note footer



                var phBodyObj = new GameObject("Footer", typeof(RectTransform));



                phBodyObj.transform.SetParent(_leaderboardPlaceholderObj.transform, false);



                var phBodyRt = phBodyObj.GetComponent<RectTransform>();



                phBodyRt.anchorMin = new Vector2(0.05f, 0.02f);



                phBodyRt.anchorMax = new Vector2(0.95f, 0.12f);



                phBodyRt.sizeDelta = Vector2.zero;



                _txtLeaderboardPlaceholderBody = phBodyObj.AddComponent<TextMeshProUGUI>();



                _txtLeaderboardPlaceholderBody.font = GetDefaultFont();



                _txtLeaderboardPlaceholderBody.fontSize = 14f;



                _txtLeaderboardPlaceholderBody.alignment = TextAlignmentOptions.Center;



                _txtLeaderboardPlaceholderBody.color = new Color(0.70f, 0.80f, 0.90f);



                _txtLeaderboardPlaceholderBody.text = "Đặt lại lúc 00:00 (Giờ Việt Nam)";



            }



        }







        public void RefreshLeaderboardUI()



        {



            EnsureLeaderboardTabsAndPlaceholder();



            if (txtLeaderboardTitle != null)



            {



                txtLeaderboardTitle.text = _currentLeaderboardTab == 0



                    ? UiStrings.Get("BẢNG XẾP HẠNG")



                    : UiStrings.Get("NHIỆM VỤ");



            }







            if (_txtTabLeaderboardLabel != null) _txtTabLeaderboardLabel.text = UiStrings.Get("BẢNG XẾP HẠNG");



            if (_txtTabQuestsLabel != null) _txtTabQuestsLabel.text = UiStrings.Get("NHIỆM VỤ");







            if (_btnTabLeaderboard != null)



            {



                var img = _btnTabLeaderboard.targetGraphic as Image;



                if (img != null) img.color = _currentLeaderboardTab == 0 ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.18f, 0.25f, 0.38f);



                if (_txtTabLeaderboardLabel != null) _txtTabLeaderboardLabel.color = _currentLeaderboardTab == 0 ? new Color(0.08f, 0.12f, 0.2f) : Color.white;



            }



            if (_btnTabQuests != null)



            {



                var img = _btnTabQuests.targetGraphic as Image;



                if (img != null) img.color = _currentLeaderboardTab == 1 ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.18f, 0.25f, 0.38f);



                if (_txtTabQuestsLabel != null) _txtTabQuestsLabel.color = _currentLeaderboardTab == 1 ? new Color(0.08f, 0.12f, 0.2f) : Color.white;



            }







            // Tab 0: Leaderboard Rows



            var modalBox = dialogLeaderboard != null && dialogLeaderboard.transform.childCount > 0 ? dialogLeaderboard.transform.GetChild(0) : null;



            if (modalBox != null)



            {



                bool isOnlinePending = IsOnlineActiveMode && !_isOnlineAccountLoaded;



                var mockRanks = RankService.GetMockLeaderboard();



                for (int i = 0; i < modalBox.childCount; i++)



                {



                    var child = modalBox.GetChild(i);



                    if (child.name.StartsWith("Row"))



                    {



                        child.gameObject.SetActive(_currentLeaderboardTab == 0);



                        if (_currentLeaderboardTab == 0 && int.TryParse(child.name.Substring(3), out int rowIdx))



                        {



                            var txtRank = child.Find("Rank")?.GetComponent<TextMeshProUGUI>();



                            var txtName = child.Find("Name")?.GetComponent<TextMeshProUGUI>();



                            var txtScore = child.Find("Score")?.GetComponent<TextMeshProUGUI>();



                            if (txtRank != null) txtRank.text = "#" + (rowIdx + 1);







                            if (isOnlinePending)



                            {



                                if (txtName != null) txtName.text = UiStrings.Get(UiStrings.NoData);



                                if (txtScore != null) txtScore.text = "-";



                            }



                            else if (rowIdx < mockRanks.Count)



                            {



                                var entry = mockRanks[rowIdx];



                                if (txtName != null) txtName.text = $"{entry.name} [{entry.tier}]";



                                if (txtScore != null) txtScore.text = entry.points + " Đ";



                            }



                        }



                    }



                }



            }







            // Tab 1: Quests Rows



            if (_leaderboardPlaceholderObj != null)



            {



                _leaderboardPlaceholderObj.SetActive(_currentLeaderboardTab == 1);



                if (_currentLeaderboardTab == 1)



                {



                    var acc = Game.FakeAccountService.Instance;



                    var quests = QuestService.GetQuests();



                    for (int qi = 0; qi < quests.Count && qi < 3; qi++)



                    {



                        var q = quests[qi];



                        var qRow = _leaderboardPlaceholderObj.transform.Find("QuestRow" + qi);



                        if (qRow != null)



                        {



                            var qtComp = qRow.Find("QuestDesc")?.GetComponent<TextMeshProUGUI>();



                            if (qtComp != null)



                            {



                                qtComp.text = $"<b>{q.titleVi}</b> (+{q.rewardGold} Vàng)\n<size=14><color=#b0c4de>{q.descriptionVi} (Tiến độ: {q.currentProgress}/{q.target})</color></size>";



                            }







                            var qBtn = qRow.Find("BtnClaim")?.GetComponent<Button>();



                            var qbImg = qBtn != null ? qBtn.targetGraphic as Image : null;



                            var qbTxt = qBtn != null ? qBtn.transform.Find("Label")?.GetComponent<TextMeshProUGUI>() : null;







                            if (qBtn != null && qbTxt != null)



                            {



                                qBtn.onClick.RemoveAllListeners();



                                if (q.isClaimed)



                                {



                                    qbTxt.text = "ĐÃ NHẬN";



                                    qBtn.interactable = false;



                                    if (qbImg != null) qbImg.color = new Color(0.3f, 0.35f, 0.4f);



                                }



                                else if (q.IsCompleted)



                                {



                                    qbTxt.text = "NHẬN";



                                    qBtn.interactable = true;



                                    if (qbImg != null) qbImg.color = new Color(0.85f, 0.65f, 0.15f);



                                    string qIdCopy = q.id;



                                    qBtn.onClick.AddListener(() =>



                                    {



                                        if (QuestService.ClaimReward(qIdCopy, out int rewardGold))



                                        {



                                            acc?.AddGold(rewardGold);



                                            AudioManager.Instance?.PlayCoin();



                                            ShowToast($"Đã nhận +{rewardGold} Vàng từ nhiệm vụ!");



                                            RefreshLeaderboardUI();



                                            RefreshAccountDisplay();



                                        }



                                    });



                                }



                                else



                                {



                                    qbTxt.text = $"{q.currentProgress}/{q.target}";



                                    qBtn.interactable = false;



                                    if (qbImg != null) qbImg.color = new Color(0.2f, 0.28f, 0.38f);



                                }



                            }



                        }



                    }



                }



            }



        }







        public void ShowLoginDialog()



        {



            if (dialogLogin != null)



            {



                dialogLogin.transform.SetAsLastSibling();



                dialogLogin.SetActive(true);



            }



            RefreshLoginUI();



        }







        public void HideLoginDialog()



        {



            if (dialogLogin != null) dialogLogin.SetActive(false);



        }







        public void RefreshLoginUI()



        {



            if (txtLoginTitle != null) txtLoginTitle.text = UiStrings.Get(UiStrings.LoginTitle);



            var acc = Game.FakeAccountService.Instance;



            if (inputLoginGuestName != null && acc != null)



            {



                inputLoginGuestName.text = acc.DisplayName;



            }







            // Wire social buttons to display "Sắp ra mắt" toast (VI / EN)



            var loginCard = dialogLogin != null && dialogLogin.transform.childCount > 0 ? dialogLogin.transform.GetChild(0) : null;



            if (loginCard != null)



            {



                var btnG = btnLoginGoogle ?? loginCard.Find("BtnGoogle")?.GetComponent<Button>();



                if (btnG != null)



                {



                    btnG.interactable = true;



                    btnG.onClick.RemoveAllListeners();



                    btnG.onClick.AddListener(() =>



                    {



                        AudioManager.Instance?.PlayClick();



                        ShowToast(UiStrings.Get(UiStrings.LoginGoogleComingSoon));



                    });



                }



                var btnF = btnLoginFacebook ?? loginCard.Find("BtnFacebook")?.GetComponent<Button>();



                if (btnF != null)



                {



                    btnF.interactable = true;



                    btnF.onClick.RemoveAllListeners();



                    btnF.onClick.AddListener(() =>



                    {



                        AudioManager.Instance?.PlayClick();



                        ShowToast(UiStrings.Get(UiStrings.LoginFacebookComingSoon));



                    });



                }



                var btnA = btnLoginApple ?? loginCard.Find("BtnApple")?.GetComponent<Button>();



                if (btnA != null)



                {



                    btnA.interactable = true;



                    btnA.onClick.RemoveAllListeners();



                    btnA.onClick.AddListener(() =>



                    {



                        AudioManager.Instance?.PlayClick();



                        ShowToast(UiStrings.Get(UiStrings.LoginAppleComingSoon));



                    });



                }



            }



        }







        public void OnSubmitGuestLogin(string name)



        {



            var acc = Game.FakeAccountService.Instance;



            if (acc != null && !string.IsNullOrWhiteSpace(name))



            {



                acc.Rename(name);



                RefreshAccountDisplay();



                ShowToast("Đăng nhập Khách thành công: " + acc.DisplayName);



                HideLoginDialog();



            }



        }







        public void OnBuyWeapon(string weaponId)



        {



            if (IsOnlineActiveMode && !_isOnlineAccountLoaded)



            {



                ShowToast(UiStrings.IsVietnamese ? "Đang xử lý giao dịch với máy chủ..." : "Processing transaction with server...");



                return;



            }







            var acc = Game.FakeAccountService.Instance;



            ItemCatalog.EnsureInitialized();



            var item = ItemCatalog.GetItem(weaponId);







            if (acc != null && item != null)



            {



                if (ItemCatalog.IsOwned(weaponId))



                {



                    ShowToast("Bạn đã sở hữu vật phẩm này!");



                    return;



                }



                if (acc.Gold < item.price)



                {



                    AudioManager.Instance?.PlayError();



                    ShowToast("Không đủ vàng để mua!");



                    return;



                }







                int gold = acc.Gold;



                if (ItemCatalog.BuyItem(weaponId, ref gold, out string error))



                {



                    acc.DeductGold(item.price);



                    var inv = Game.FakeInventoryService.Instance;



                    if (inv != null) inv.BuyWeapon(weaponId, acc);







                    AudioManager.Instance?.PlayPurchase();



                    AudioManager.Instance?.PlayCoin();



                    RefreshShopUI();



                    RefreshInventoryUI();



                    RefreshAccountDisplay();



                    ShowToast("Đã mua thành công: " + item.GetLocalizedName(!UiStrings.IsVietnamese));



                }



                else



                {



                    AudioManager.Instance?.PlayError();



                    ShowToast("Lỗi mua vật phẩm: " + error);



                }



            }



        }







        public void OnEquipWeapon(string weaponId)



        {



            if (IsOnlineActiveMode && !_isOnlineAccountLoaded)



            {



                ShowToast(UiStrings.IsVietnamese ? "Đang đồng bộ trang bị với máy chủ..." : "Syncing equipment with server...");



                return;



            }







            ItemCatalog.EnsureInitialized();



            if (ItemCatalog.EquipItem(weaponId))



            {



                var inv = Game.FakeInventoryService.Instance;



                if (inv != null) inv.EquipWeapon(weaponId);







                AudioManager.Instance?.PlayClick();



                RefreshInventoryUI();



                RefreshShopUI();



                var item = ItemCatalog.GetItem(weaponId);



                string wName = item != null ? item.GetLocalizedName(!UiStrings.IsVietnamese) : weaponId;



                ShowToast("Đã trang bị: " + wName);



                if (txtSelectedLoadout != null)



                {



                    txtSelectedLoadout.text = "Vũ khí: " + wName;



                }



            }



        }







        public void OnRenameAccount(string newName)



        {



            var acc = Game.FakeAccountService.Instance;



            if (acc != null)



            {



                int cost = acc.NameChangeCount == 0 ? 0 : 99;



                if (cost > 0 && acc.Gold < cost)



                {



                    AudioManager.Instance?.PlayError();



                    ShowToast($"Cần {cost} vàng để đổi tên (Bạn có {acc.Gold} vàng)!");



                    return;



                }







                if (acc.Rename(newName, cost))



                {



                    AudioManager.Instance?.PlayClick();



                    RefreshProfileUI();



                    RefreshAccountDisplay();



                    ShowToast("Đổi tên thành công: " + acc.DisplayName);



                }



                else



                {



                    AudioManager.Instance?.PlayError();



                    ShowToast("Tên phải từ 2 đến 16 ký tự!");



                }



            }



        }







        public void ShowToast(string message)



        {



            ShowToast(message, 2.0f);



        }







        public void ShowErrorToast(string message, float duration = 3.0f)



        {



            AudioManager.Instance?.PlayError();



            ShowToast("[LỖI] " + message, duration);



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



                toastObject.transform.SetAsLastSibling();



                if (txtToast != null)



                {



                    txtToast.text = message;



                    if (message.Contains("[LỖI]") || message.Contains("[LỖI]"))



                    {



                        txtToast.color = new Color(1f, 0.4f, 0.4f);



                    }



                    else



                    {



                        txtToast.color = new Color(1f, 0.9f, 0.3f);



                    }



                }



                toastObject.SetActive(true);



            }







            yield return new WaitForSeconds(duration);







            if (toastObject != null)



            {



                toastObject.SetActive(false);



            }



        }







        private void ConfigureRoomCodeInput()
        {
            if (inputRoomCode == null) return;
            // Older serialized scenes contained an example room code as actual input.
            // Clear it even when the scene has not yet been regenerated.
            inputRoomCode.SetTextWithoutNotify(string.Empty);
            inputRoomCode.characterLimit = RoomCode.Length;
            inputRoomCode.lineType = TMP_InputField.LineType.SingleLine;
            inputRoomCode.contentType = TMP_InputField.ContentType.Alphanumeric;
            inputRoomCode.onValueChanged.AddListener(OnRoomCodeChanged);
        }

        private void OnRoomCodeChanged(string raw)
        {
            if (inputRoomCode == null) return;
            string code = string.IsNullOrEmpty(raw) ? string.Empty : raw.ToUpperInvariant();
            if (code != raw) inputRoomCode.SetTextWithoutNotify(code);
            UpdateJoinRoomSearchStatus(code);
        }

        public void UpdateJoinRoomSearchStatus(string code)
        {
            // Only a server room_state proves a room exists. Never show a fabricated room card.
            if (roomFoundCard != null) roomFoundCard.SetActive(false);
        }

        private void EnterLobbyWithAutoFill(string roomName, string code)



        {



            UpdateLobbyRoster(4, roomName, code);



        }







        public void UpdateLobbyRoster(int humanCount, string roomName = "Phòng Chiến Hữu", string code = "")



        {



            ShowScreen(ProductionScreen.Lobby);



            _isLocalPlayerReady = false;



            _currentLobbyHumanCount = Mathf.Clamp(humanCount, 1, 6);







            if (txtLobbyHeader != null)



            {



                txtLobbyHeader.text = string.IsNullOrEmpty(roomName)



                    ? $"{UiStrings.LobbyTitle} ({_currentLobbyHumanCount}/6 NGƯỜI THẬT)"



                    : $"{roomName.ToUpperInvariant()} ({_currentLobbyHumanCount}/6 NGƯỜI THẬT) • SÂN LÀNG";



            }



            if (txtLobbyRoomCode != null)



            {



                txtLobbyRoomCode.text = $"MÃ PHÒNG: {code} • SÂN LÀNG";



            }







            if (_currentLobbyHumanCount < 2)



            {



                if (txtLobbyCountdown != null)



                {



                    txtLobbyCountdown.text = UiStrings.LobbyOnePlayerWarning;



                    txtLobbyCountdown.color = theme != null ? theme.alertRed : new Color(0.95f, 0.35f, 0.35f);



                }



                if (btnLobbyReady != null)



                {



                    btnLobbyReady.interactable = false;



                    var btnImg = btnLobbyReady.GetComponent<Image>();



                    if (btnImg != null && theme != null) btnImg.color = theme.lockedButtonMuted;



                }



                if (btnLobbyToBotTraining != null)



                {



                    btnLobbyToBotTraining.gameObject.SetActive(true);



                }



            }



            else



            {



                if (txtLobbyCountdown != null)



                {



                    txtLobbyCountdown.text = UiStrings.LobbyWaitingAllReady;



                    txtLobbyCountdown.color = theme != null ? theme.subtitleGold : new Color(1f, 0.9f, 0.3f);



                }



                if (btnLobbyReady != null)



                {



                    btnLobbyReady.interactable = true;



                    var btnImg = btnLobbyReady.GetComponent<Image>();



                    if (btnImg != null) btnImg.color = Color.white;



                }



                if (btnLobbyToBotTraining != null)



                {



                    btnLobbyToBotTraining.gameObject.SetActive(false);



                }



            }







            if (txtLobbyReadyLabel != null)



            {



                txtLobbyReadyLabel.text = UiStrings.BtnReady;



            }







            for (int i = 0; i < lobbySlots.Count && i < 6; i++)



            {



                var slot = lobbySlots[i];



                if (slot.root != null) slot.root.SetActive(true);







                if (i < _currentLobbyHumanCount)



                {



                    if (slot.txtName != null)



                    {



                        slot.txtName.text = i == 0 ? "[CHỦ PHÒNG] Nam Làng (Bạn)" : (i < PlayerView.CharacterNames.Length ? PlayerView.CharacterNames[i] : $"Người chơi #{i + 1}");



                    }



                    if (i == 0)



                    {



                        slot.isReady = false;



                        if (slot.txtStatus != null)



                        {



                            slot.txtStatus.text = "CHƯA SẴN SÀNG";



                            slot.txtStatus.color = theme != null ? theme.subtitleGold : new Color(1f, 0.7f, 0.2f);



                        }



                    }



                    else



                    {



                        slot.isReady = true;



                        if (slot.txtStatus != null)



                        {



                            slot.txtStatus.text = "ĐÃ SẴN SÀNG";



                            slot.txtStatus.color = theme != null ? theme.actionGreen : new Color(0.2f, 1f, 0.4f);



                        }



                    }



                }



                else



                {



                    // Empty slot: Mockup "Vào phòng" 6 chỗ, chỗ trống ghi "Đang chờ…"



                    slot.isReady = false;



                    if (slot.txtName != null)



                    {



                        slot.txtName.text = $"Chỗ trống #{i + 1}";



                    }



                    if (slot.txtStatus != null)



                    {



                        slot.txtStatus.text = UiStrings.WaitingPlayerSlot;



                        slot.txtStatus.color = new Color(0.65f, 0.72f, 0.80f);



                    }



                }



            }







            EnsureLobbyToBotTrainingButton();



            EnsureLobbyMapSelector();



            if (btnLobbyToBotTraining != null)



            {



                btnLobbyToBotTraining.gameObject.SetActive(_currentLobbyHumanCount < 2);



            }



            if (btnLobbyReady != null)



            {



                btnLobbyReady.interactable = _currentLobbyHumanCount >= 2;



            }



        }







        public void StartQuickMatchSearch()



        {
            if (!NetConfig.IsOnlineActive)
            {
                ShowErrorToast("Ghép nhanh cần kết nối máy chủ.");
                return;
            }
            if (_quickMatchPending) return;
            EnsureOnlineClient();
            _suppressRoomState = false;
            _quickMatchPending = true;
            if (quickMatchOverlay != null) quickMatchOverlay.SetActive(true);
            if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = "Đang kết nối máy chủ...";
            if (btnQuickMatchToBots != null) btnQuickMatchToBots.gameObject.SetActive(false);
            StartCoroutine(SubmitQuickMatch());
        }

        private IEnumerator SubmitQuickMatch()
        {
            string token = null;
            string error = null;
            yield return ClientAuthService.GetVerifiedGuestToken(
                value => token = value, value => error = value);
            if (!_quickMatchPending) yield break;
            if (string.IsNullOrEmpty(token))
            {
                _quickMatchPending = false;
                if (quickMatchOverlay != null) quickMatchOverlay.SetActive(false);
                ShowErrorToast(error ?? "Không thể xác thực phiên khách.");
                yield break;
            }
            _onlineClient.QuickMatch(token);
            if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = UiStrings.MatchmakingSearching;
        }







        public void StopQuickMatchSearch()



        {
            bool wasSearching = _quickMatchPending;
            CompleteQuickMatchSearch();
            if (wasSearching)
            {
                _suppressRoomState = true;
                _onlineClient?.LeaveRoom();
            }
        }

        private void CompleteQuickMatchSearch()
        {
            _quickMatchPending = false;
            if (_quickMatchCoroutine != null)
            {
                StopCoroutine(_quickMatchCoroutine);
                _quickMatchCoroutine = null;
            }
            if (quickMatchOverlay != null) quickMatchOverlay.SetActive(false);
        }







        public void ShowQuickMatchDialog(bool isTimedOut)



        {



            if (quickMatchOverlay != null)



            {



                quickMatchOverlay.SetActive(true);



                if (isTimedOut)



                {



                    if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = UiStrings.MatchmakingTimeoutSuggest;



                    if (btnQuickMatchToBots != null) btnQuickMatchToBots.gameObject.SetActive(true);



                }



                else



                {



                    if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = UiStrings.MatchmakingSearching;



                    if (btnQuickMatchToBots != null) btnQuickMatchToBots.gameObject.SetActive(false);



                }



            }



        }







        private IEnumerator QuickMatchTimeoutCoroutine(float timeoutSeconds)



        {



            yield return new WaitForSeconds(timeoutSeconds);



            if (txtQuickMatchStatus != null)



            {



                txtQuickMatchStatus.text = UiStrings.MatchmakingTimeoutSuggest;



            }



            if (btnQuickMatchToBots != null)



            {



                btnQuickMatchToBots.gameObject.SetActive(true);



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







            if (NetConfig.IsOnlineActive && _onlineClient != null && _onlineClient.IsConnected)



            {



                _onlineClient.SetRoomReady(_isLocalPlayerReady);



            }







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







        public void StartLabSimulation(int n)



        {



            int total = Mathf.Clamp(n, 2, 6);



            ShowScreen(ProductionScreen.Lobby);



            UpdateLobbyRoster(total, "Phòng Online Lab", "LAB001");



            ShowToast($"[Online Lab] 1 người chơi thật + {total - 1} người chơi Loopback", 3.0f);



        }







        private IEnumerator SubmitRoomRequest(string roomCode)
        {
            string token = null;
            string error = null;
            yield return ClientAuthService.GetVerifiedGuestToken(
                value => token = value, value => error = value);
            if (!_roomRequestPending) yield break;
            if (string.IsNullOrEmpty(token))
            {
                CancelRoomRequest();
                ShowErrorToast(error ?? "Không thể xác thực phiên khách.");
                yield break;
            }
            StartRoomRequestTimeout();
            if (string.IsNullOrEmpty(roomCode)) _onlineClient.CreateRoom(token);
            else _onlineClient.JoinRoom(roomCode, token);
        }

        private void StartRoomRequestTimeout()
        {
            if (_roomRequestTimeout != null) StopCoroutine(_roomRequestTimeout);
            _roomRequestTimeout = StartCoroutine(RoomRequestTimeoutRoutine());
        }

        private IEnumerator RoomRequestTimeoutRoutine()
        {
            yield return new WaitForSecondsRealtime(20f);
            _roomRequestTimeout = null;
            if (!_roomRequestPending) yield break;
            _roomRequestPending = false;
            ShowErrorToast("Máy chủ chưa phản hồi. Vui lòng thử lại.");
        }

        private void CancelRoomRequest()
        {
            _roomRequestPending = false;
            if (_roomRequestTimeout == null) return;
            StopCoroutine(_roomRequestTimeout);
            _roomRequestTimeout = null;
        }

        public void EnsureOnlineClient()



        {



            if (_onlineClient != null) return;



            var client = new WebSocketGameClient();



            client.OnRoomStateUpdated += HandleOnlineRoomState;



            client.OnErrorReceived += HandleOnlineError;



            client.OnDisconnected += HandleOnlineDisconnected;
            client.OnQuickMatchWaiting += HandleQuickMatchWaiting;
            client.OnQuickMatchHint += HandleQuickMatchHint;



            _onlineClient = client;



        }

        private void HandleQuickMatchWaiting(string status)
        {
            if (!_quickMatchPending) return;
            if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = status;
        }

        private void HandleQuickMatchHint()
        {
            if (!_quickMatchPending) return;
            if (txtQuickMatchStatus != null) txtQuickMatchStatus.text = UiStrings.MatchmakingTimeoutSuggest;
            if (btnQuickMatchToBots != null) btnQuickMatchToBots.gameObject.SetActive(true);
        }







        private void HandleOnlineRoomState(RoomStateMessage room)



        {



            if (room == null) return;
            if (_suppressRoomState)
            {
                _onlineClient?.LeaveRoom();
                return;
            }
            if (_quickMatchPending) CompleteQuickMatchSearch();
            CancelRoomRequest();



            _activeRoomCode = room.roomCode;



            int count = room.players != null ? room.players.Length : 0;



            _currentLobbyHumanCount = Mathf.Clamp(count, 1, 6);



            UpdateLobbyRoster(count, "Phòng Chiến Hữu", room.roomCode);







            for (int i = 0; i < lobbySlots.Count && i < count; i++)



            {



                var p = room.players[i];



                var slot = lobbySlots[i];



                bool isHost = p.id == room.hostId;



                slot.isReady = p.ready;



                if (slot.txtName != null)



                {



                    slot.txtName.text = (isHost ? "[CHỦ PHÒNG] " : "") + p.name;



                }



                if (slot.txtStatus != null)



                {



                    slot.txtStatus.text = p.ready ? "ĐÃ SẴN SÀNG" : "CHƯA SẴN SÀNG";



                    slot.txtStatus.color = p.ready ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.7f, 0.2f);



                }



            }



        }







        private void HandleOnlineError(string code, string viMsg)



        {
            if (_quickMatchPending) CompleteQuickMatchSearch();



            CancelRoomRequest();
            ShowErrorToast(viMsg);



            if (code == "session_replaced" || code == "banned" || code == "reconnect_invalid" || code == "server_shutdown")



            {



                ShowScreen(ProductionScreen.Home);



            }



        }







        private void HandleOnlineDisconnected(string reason)



        {
            if (_quickMatchPending) CompleteQuickMatchSearch();
            bool wasJoining = _roomRequestPending;
            CancelRoomRequest();
            if (wasJoining) ShowScreen(ProductionScreen.Home);



            if (reason != "client_leave")



            {



                ShowToast("Đã ngắt kết nối máy chủ.", 2.5f);



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







        public void EnsureLobbyMapSelector()



        {



            if (lobbyPanel == null) return;







            if (_btnLobbyMapSelector == null)



            {



                var existing = lobbyPanel.transform.Find("BtnLobbyMapSelector");



                if (existing != null)



                {



                    _btnLobbyMapSelector = existing.GetComponent<Button>();



                    _txtLobbyMapSelector = _btnLobbyMapSelector.GetComponentInChildren<TextMeshProUGUI>();



                }



                else



                {



                    var mapObj = new GameObject("BtnLobbyMapSelector", typeof(RectTransform));



                    mapObj.transform.SetParent(lobbyPanel.transform, false);



                    var rt = mapObj.GetComponent<RectTransform>();



                    rt.anchorMin = new Vector2(0.12f, 0.20f);



                    rt.anchorMax = new Vector2(0.88f, 0.25f);



                    rt.sizeDelta = Vector2.zero;







                    var img = mapObj.AddComponent<Image>();



                    img.color = new Color(0.12f, 0.30f, 0.45f, 0.95f);



                    var outline = mapObj.AddComponent<Outline>();



                    outline.effectColor = new Color(0.4f, 0.8f, 1.0f, 0.8f);



                    outline.effectDistance = new Vector2(2f, -2f);







                    _btnLobbyMapSelector = mapObj.AddComponent<Button>();



                    _btnLobbyMapSelector.targetGraphic = img;







                    var txtObj = new GameObject("Text", typeof(RectTransform));



                    txtObj.transform.SetParent(mapObj.transform, false);



                    var txtRt = txtObj.GetComponent<RectTransform>();



                    txtRt.anchorMin = Vector2.zero;



                    txtRt.anchorMax = Vector2.one;



                    txtRt.sizeDelta = Vector2.zero;







                    _txtLobbyMapSelector = txtObj.AddComponent<TextMeshProUGUI>();



                    _txtLobbyMapSelector.font = GetDefaultFont();



                    _txtLobbyMapSelector.fontSize = 15f;



                    _txtLobbyMapSelector.fontStyle = FontStyles.Bold;



                    _txtLobbyMapSelector.alignment = TextAlignmentOptions.Center;



                    _txtLobbyMapSelector.color = Color.white;



                }



            }







            if (_btnLobbyMapSelector != null)



            {



                _btnLobbyMapSelector.onClick.RemoveAllListeners();



                _btnLobbyMapSelector.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    if (Game.MapManager.Instance != null)



                    {



                        var nextMap = Game.MapManager.Instance.CycleNextMap();



                        RefreshLobbyMapSelectorVisual();



                        if (nextMap != null)



                        {



                            ShowToast((UiStrings.CurrentLanguage == AppLanguage.English ? "Map: " : "Sân: ") + nextMap.GetDisplayName(UiStrings.CurrentLanguage == AppLanguage.English));



                        }



                    }



                });



            }







            RefreshLobbyMapSelectorVisual();



        }







        public void RefreshLobbyMapSelectorVisual()



        {



            if (_txtLobbyMapSelector == null) return;



            string prefix = UiStrings.Get(UiStrings.LobbyMapSelectorPrefix);



            string mapName = "Phố Cổ Nắng Gắt";



            if (Game.MapManager.Instance != null && Game.MapManager.Instance.CurrentMap != null)



            {



                mapName = Game.MapManager.Instance.CurrentMap.GetDisplayName(UiStrings.CurrentLanguage == AppLanguage.English);



            }



            _txtLobbyMapSelector.text = $"[BẢN ĐỒ] {prefix} {mapName}";



        }







        public void EnsureBotTrainingPanel()



        {



            if (botTrainingPanel != null)



            {



                RefreshBotTrainingDisplay();



                return;



            }







            Transform safeArea = transform.Find("SafeAreaRoot") ?? transform;



            var existing = safeArea.Find("BotTrainingPanel");



            if (existing != null)



            {



                botTrainingPanel = existing.gameObject;



                RefreshBotTrainingDisplay();



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







        #region N3: In-Game Room Code Keypad & Reconnect Overlay







        private GameObject _reconnectOverlay;



        private TextMeshProUGUI _txtReconnectCountdown;



        private Coroutine _reconnectCoroutine;







        public void ShowReconnectOverlay(float timeoutSeconds = 30f)



        {



            if (_reconnectOverlay == null)



            {



                Transform safeArea = transform.Find("SafeAreaRoot") ?? transform;



                var obj = new GameObject("ReconnectOverlay", typeof(RectTransform));



                obj.transform.SetParent(safeArea, false);



                var rt = obj.GetComponent<RectTransform>();



                rt.anchorMin = Vector2.zero;



                rt.anchorMax = Vector2.one;



                rt.sizeDelta = Vector2.zero;







                var bg = obj.AddComponent<Image>();



                bg.color = new Color(0.04f, 0.06f, 0.10f, 0.94f);







                TMP_FontAsset font = txtLobbyHeader != null ? txtLobbyHeader.font : null;







                var cardObj = new GameObject("Card", typeof(RectTransform));



                cardObj.transform.SetParent(obj.transform, false);



                var cardRt = cardObj.GetComponent<RectTransform>();



                cardRt.anchorMin = new Vector2(0.10f, 0.35f);



                cardRt.anchorMax = new Vector2(0.90f, 0.65f);



                cardRt.sizeDelta = Vector2.zero;



                var cardBg = cardObj.AddComponent<Image>();



                cardBg.color = new Color(0.12f, 0.16f, 0.24f, 0.98f);







                var titleObj = new GameObject("Title", typeof(RectTransform));



                titleObj.transform.SetParent(cardObj.transform, false);



                var titleRt = titleObj.GetComponent<RectTransform>();



                titleRt.anchorMin = new Vector2(0.05f, 0.65f);



                titleRt.anchorMax = new Vector2(0.95f, 0.95f);



                titleRt.sizeDelta = Vector2.zero;



                var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();



                if (font != null) titleTxt.font = font;



                titleTxt.fontSize = 24f;



                titleTxt.fontStyle = FontStyles.Bold;



                titleTxt.alignment = TextAlignmentOptions.Center;



                titleTxt.color = new Color(1f, 0.85f, 0.2f);



                titleTxt.text = "ĐANG KẾT NỐI LẠI MÁY CHỦ...";







                var countObj = new GameObject("Countdown", typeof(RectTransform));



                countObj.transform.SetParent(cardObj.transform, false);



                var countRt = countObj.GetComponent<RectTransform>();



                countRt.anchorMin = new Vector2(0.05f, 0.35f);



                countRt.anchorMax = new Vector2(0.95f, 0.65f);



                countRt.sizeDelta = Vector2.zero;



                _txtReconnectCountdown = countObj.AddComponent<TextMeshProUGUI>();



                if (font != null) _txtReconnectCountdown.font = font;



                _txtReconnectCountdown.fontSize = 20f;



                _txtReconnectCountdown.alignment = TextAlignmentOptions.Center;



                _txtReconnectCountdown.color = Color.white;



                _txtReconnectCountdown.text = "Thời gian còn lại: 30s";







                var btnObj = new GameObject("BtnCancelReconnect", typeof(RectTransform));



                btnObj.transform.SetParent(cardObj.transform, false);



                var btnRt = btnObj.GetComponent<RectTransform>();



                btnRt.anchorMin = new Vector2(0.25f, 0.08f);



                btnRt.anchorMax = new Vector2(0.75f, 0.30f);



                btnRt.sizeDelta = Vector2.zero;



                var btnBg = btnObj.AddComponent<Image>();



                btnBg.color = new Color(0.50f, 0.20f, 0.20f, 0.95f);



                var btn = btnObj.AddComponent<Button>();



                btn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    HideReconnectOverlay();



                    ShowScreen(ProductionScreen.Home);



                    ShowToast("Đã hủy kết nối lại và về trang chủ.");



                });







                var btnTxtObj = new GameObject("Text", typeof(RectTransform));



                btnTxtObj.transform.SetParent(btnObj.transform, false);



                var btnTxtRt = btnTxtObj.GetComponent<RectTransform>();



                btnTxtRt.anchorMin = Vector2.zero;



                btnTxtRt.anchorMax = Vector2.one;



                btnTxtRt.sizeDelta = Vector2.zero;



                var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();



                if (font != null) btnTxt.font = font;



                btnTxt.fontSize = 18f;



                btnTxt.fontStyle = FontStyles.Bold;



                btnTxt.alignment = TextAlignmentOptions.Center;



                btnTxt.color = Color.white;



                btnTxt.text = "VỀ TRANG CHỦ";







                _reconnectOverlay = obj;



            }







            _reconnectOverlay.SetActive(true);



            if (_reconnectCoroutine != null) StopCoroutine(_reconnectCoroutine);



            _reconnectCoroutine = StartCoroutine(ReconnectCountdownRoutine(timeoutSeconds));



        }







        public void HideReconnectOverlay()



        {



            if (_reconnectCoroutine != null)



            {



                StopCoroutine(_reconnectCoroutine);



                _reconnectCoroutine = null;



            }



            if (_reconnectOverlay != null)



            {



                _reconnectOverlay.SetActive(false);



            }



        }







        private IEnumerator ReconnectCountdownRoutine(float totalSeconds)



        {



            float remaining = totalSeconds;



            while (remaining > 0f)



            {



                if (_txtReconnectCountdown != null)



                {



                    _txtReconnectCountdown.text = $"Thời gian còn lại: {Mathf.CeilToInt(remaining)}s";



                }



                yield return new WaitForSeconds(1f);



                remaining -= 1f;



            }



            HideReconnectOverlay();



            ShowScreen(ProductionScreen.Home);



            ShowErrorToast("Phiên vào lại đã hết hạn (30s). Đã trở về trang chủ.");



        }







        #endregion







        public void ChangeBotCount(int delta)



        {



            AudioManager.Instance?.PlayClick();



            _selectedBotCount = Mathf.Clamp(_selectedBotCount + delta, 1, 5);



            RefreshBotTrainingDisplay();



        }







        

        public void SyncHomeCharacterDisplay()

        {

            if (matchController != null)

            {

                matchController.localCharacterIndex = _selectedPracticeCharIndex;

                if (matchController.configAsset != null)

                {

                    var idleSprites = matchController.configAsset.characterIdleSprites;

                    if (idleSprites != null && _selectedPracticeCharIndex >= 0 && _selectedPracticeCharIndex < idleSprites.Length)

                    {

                        if (imgHomeCharPortrait != null && idleSprites[_selectedPracticeCharIndex] != null)

                        {

                            imgHomeCharPortrait.sprite = idleSprites[_selectedPracticeCharIndex];

                        }

                        if (imgHomeCharCardIcon != null && idleSprites[_selectedPracticeCharIndex] != null)

                        {

                            imgHomeCharCardIcon.sprite = idleSprites[_selectedPracticeCharIndex];

                        }

                    }

                }

            }

            if (_selectedPracticeCharIndex >= 0 && _selectedPracticeCharIndex < UiStrings.CharacterNames.Length)

            {

                if (txtHomeCharName != null)

                {

                    txtHomeCharName.text = UiStrings.CharacterNames[_selectedPracticeCharIndex];

                }

            }

        }



        public void ChangePracticeCharacter(int delta)



        {



            AudioManager.Instance?.PlayClick();



            int dir = delta > 0 ? 1 : -1;



            int next = _selectedPracticeCharIndex;



            for (int k = 0; k < 7; k++)



            {



                next = ((next + dir) % 7 + 7) % 7;



                if (HitMe.Data.CharacterRegistry.GetByIndex(next).isEnabled)



                {



                    _selectedPracticeCharIndex = next;



                    break;



                }



            }



            if (matchController != null)



            {



                matchController.localCharacterIndex = _selectedPracticeCharIndex;



            }



            RefreshBotTrainingDisplay();

            SyncHomeCharacterDisplay();

            SyncHomeCharacterDisplay();



        }







        public void SetSelectedPracticeCharacter(int index)



        {



            AudioManager.Instance?.PlayClick();



            if (!HitMe.Data.CharacterRegistry.GetByIndex(index).isEnabled)



            {



                ShowToast("Nhân vật đang được hoàn thiện (Sắp ra mắt)!");



                return;



            }



            _selectedPracticeCharIndex = Mathf.Clamp(index, 0, 6);



            if (matchController != null)



            {



                matchController.localCharacterIndex = _selectedPracticeCharIndex;



            }



            RefreshBotTrainingDisplay();



        }







        public void RefreshBotTrainingDisplay()



        {



            if (txtSelectedBotCount != null)



            {



                int totalPlayers = _selectedBotCount + 1;



                txtSelectedBotCount.text = $"{totalPlayers} người (1 Bạn + {_selectedBotCount} Bot)";



            }



            if (_selectedPracticeCharIndex >= 0 && _selectedPracticeCharIndex < 7)



            {



                string charName = UiStrings.CharacterNames[_selectedPracticeCharIndex];



                string charDesc = UiStrings.CharacterDescriptions[_selectedPracticeCharIndex];







                if (txtSelectedCharacter != null)



                {



                    txtSelectedCharacter.text = charName;



                }



                if (txtSelectedCharDetail != null)



                {



                    txtSelectedCharDetail.text = charDesc + " • Vũ khí: Dép Tổ Ong";



                }







                if (botCharCardFrames != null && theme != null)



                {



                    for (int i = 0; i < botCharCardFrames.Count; i++)



                    {



                        if (botCharCardFrames[i] != null)



                        {



                            botCharCardFrames[i].sprite = (i == _selectedPracticeCharIndex) ? theme.cardCharSelected : theme.cardCharNormal;



                        }



                    }



                }







                if (txtHomeCharName != null)



                {



                    txtHomeCharName.text = charName;



                }



                if (txtHomeCharDesc != null)



                {



                    txtHomeCharDesc.text = charDesc;



                }



                if (matchController != null && matchController.configAsset != null)



                {



                    var idleSprites = matchController.configAsset.characterIdleSprites;



                    if (idleSprites != null && _selectedPracticeCharIndex < idleSprites.Length)



                    {



                        if (imgHomeCharPortrait != null)



                            imgHomeCharPortrait.sprite = idleSprites[_selectedPracticeCharIndex];



                        if (imgHomeCharCardIcon != null)

                        {

                            if (charHeadSprites != null && _selectedPracticeCharIndex < charHeadSprites.Length && charHeadSprites[_selectedPracticeCharIndex] != null)

                                imgHomeCharCardIcon.sprite = charHeadSprites[_selectedPracticeCharIndex];

                            else

                                imgHomeCharCardIcon.sprite = idleSprites[_selectedPracticeCharIndex];

                        }



                    }



                }



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

            if (matchController != null)

            {

                matchController.localCharacterIndex = _selectedPracticeCharIndex;

            }



            if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);



            _lastDisplayedPhase = (Phase)(-1);



            _lastSyncedStatePhase = (Phase)(-1);



            ShowScreen(ProductionScreen.Gameplay);



            ApplyPhaseState(Phase.Placing);







            if (btnInGameReady != null)



            {



                btnInGameReady.interactable = true;



                if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = UiStrings.BtnReady;



                var btnImg = btnInGameReady.GetComponent<Image>();



                if (btnImg != null && theme != null && theme.btnCtaYellow != null)



                {



                    btnImg.sprite = theme.btnCtaYellow;



                }



            }







            if (matchController != null)



            {



                matchController.StartMatch();



            }







            try



            {



                if (PlayerPrefs.GetString(UiStrings.PrefKeyTutorialCompleted, "0") != "1")



                {



                    ShowTutorial(0);



                }



            }



            catch { /* WebGL sandbox */ }



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



                if (txtInGameReadyLabel != null) txtInGameReadyLabel.text = UiStrings.BtnReady;



                var btnImg = btnInGameReady.GetComponent<Image>();



                if (btnImg != null && theme != null && theme.btnCtaYellow != null)



                {



                    btnImg.sprite = theme.btnCtaYellow;



                }



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







            bool isRoundEnd = phase == Phase.RoundEnd || phase == Phase.Resolve;



            if (roundResultOverlay != null)



            {



                if (isRoundEnd && !isMatchEnd)



                {



                    roundResultOverlay.SetActive(true);

                    StartCoroutine(AutoDismissRoundResult(roundResultOverlay, 1.5f));



                    if (txtRoundResultTitle != null && matchController != null && matchController.State != null)



                    {



                        txtRoundResultTitle.text = $"KẾT QUẢ VÒNG {matchController.State.RoundNumber}";



                    }



                    if (txtRoundResultDetail != null && matchController != null && matchController.State != null)

                    {

                        int hitsCount = (matchController.State.LastResolution != null && matchController.State.LastResolution.Hits != null)

                            ? matchController.State.LastResolution.Hits.Count

                            : 0;

                        txtRoundResultDetail.text = hitsCount > 0

                            ? $"Có {hitsCount} đòn ném trúng đích trong vòng này!"

                            : "Tất cả người chơi đều né thành công!";

                    }



                }



                else



                {



                    roundResultOverlay.SetActive(false);



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









        private IEnumerator AutoDismissRoundResult(GameObject overlay, float delay)

        {

            yield return new WaitForSeconds(delay);

            if (overlay != null && CurrentScreen == ProductionScreen.Gameplay && matchController != null && matchController.State != null && matchController.State.Phase != Phase.MatchEnd)

            {

                var cg = overlay.GetComponent<CanvasGroup>();

                if (cg == null) cg = overlay.AddComponent<CanvasGroup>();

                float fade = 0.35f;

                float elapsed = 0f;

                while (elapsed < fade)

                {

                    elapsed += Time.deltaTime;

                    cg.alpha = Mathf.Clamp01(1f - (elapsed / fade));

                    yield return null;

                }

                overlay.SetActive(false);

                cg.alpha = 1f;

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



                txtTimer.color = Color.white;

                txtTimer.outlineWidth = 0.25f;

                txtTimer.outlineColor = new Color32(0, 0, 0, 255);



            }







            // Pulse ring in final 2 seconds (delegates to JuiceFeedbackController if available)



            if (timerPulseRing != null)



            {



                bool pulseActive = secondsRemaining > 0f && secondsRemaining <= 2.0f;



                if (juiceFeedback != null)



                {



                    if (pulseActive && !timerPulseRing.gameObject.activeSelf)



                    {



                        juiceFeedback.StartCountdownPulse();



                    }



                    else if (!pulseActive && timerPulseRing.gameObject.activeSelf)



                    {



                        juiceFeedback.StopCountdownPulse();



                    }



                }



                else



                {



                    if (timerPulseRing.gameObject.activeSelf != pulseActive)



                    {



                        timerPulseRing.gameObject.SetActive(pulseActive);



                    }



                    if (pulseActive)



                    {



                        float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 12f);



                        timerPulseRing.transform.localScale = new Vector3(pulse, pulse, 1f);



                    }



                }



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



                    anchors = new[] { new Vector2(0.27f, 0.50f), new Vector2(0.73f, 0.50f) };



                    cardSize = new Vector2(460f, 78f);



                    break;



                case 3:



                    anchors = new[] { new Vector2(0.17f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.83f, 0.50f) };



                    cardSize = new Vector2(330f, 72f);



                    break;



                case 4:



                    anchors = new[]



                    {



                        new Vector2(0.27f, 0.76f), new Vector2(0.73f, 0.76f),



                        new Vector2(0.27f, 0.24f), new Vector2(0.73f, 0.24f)



                    };



                    cardSize = new Vector2(460f, 78f);



                    break;



                case 5:



                    anchors = new[]



                    {



                        new Vector2(0.17f, 0.76f), new Vector2(0.50f, 0.76f), new Vector2(0.83f, 0.76f),



                        new Vector2(0.33f, 0.24f), new Vector2(0.67f, 0.24f)



                    };



                    cardSize = new Vector2(330f, 72f);



                    break;



                case 6:



                default:



                    anchors = new[]



                    {



                        new Vector2(0.17f, 0.76f), new Vector2(0.50f, 0.76f), new Vector2(0.83f, 0.76f),



                        new Vector2(0.17f, 0.24f), new Vector2(0.50f, 0.24f), new Vector2(0.83f, 0.24f)



                    };



                    cardSize = new Vector2(330f, 72f);



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







        /// <summary>



        /// Lô 4: Quick shake animation on HP card when player takes damage.



        /// Duration from UiTheme.heartShakeDuration. Zero-alloc after warm-up.



        /// </summary>



        private IEnumerator ShakeHpCard(Transform cardTr)



        {



            float duration = (theme != null) ? theme.heartShakeDuration : 0.35f;



            float magnitude = 8f;



            Vector3 originalPos = cardTr.localPosition;



            float elapsed = 0f;



            while (elapsed < duration)



            {



                elapsed += Time.deltaTime;



                float t = elapsed / duration;



                float dampedMag = magnitude * (1f - t);



                float offsetX = Mathf.Sin(t * 30f) * dampedMag;



                cardTr.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);



                yield return null;



            }



            cardTr.localPosition = originalPos;



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



                if (card.lastDisplayedHpCode != clampedHp)



                {



                    // Lô 4: Shake HP card on damage (HP decreased)



                    bool tookDamage = card.lastDisplayedHpCode > clampedHp && card.lastDisplayedHpCode >= 0;



                    card.lastDisplayedHpCode = clampedHp;



                    if (tookDamage && card.root != null && (juiceFeedback == null || !juiceFeedback.ReducedMotion))



                    {



                        StartCoroutine(ShakeHpCard(card.root.transform));



                    }



                    if (card.txtHearts != null)



                    {



                        card.txtHearts.text = CachedHpCardStrings[clampedHp];



                        card.txtHearts.color = player.IsAlive



                            ? (clampedHp == 3 ? new Color(0.25f, 1f, 0.44f) : (clampedHp == 2 ? new Color(1f, 0.86f, 0.25f) : new Color(1f, 0.42f, 0.42f)))



                            : new Color(1f, 0.44f, 0.44f);



                    }







                    if (card.heartIcons != null && card.heartIcons.Length >= 3)



                    {



                        var fullSp = theme != null ? theme.heartFull : null;



                        var breakSp = theme != null ? theme.heartBreak : null;



                        for (int h = 0; h < 3; h++)



                        {



                            if (card.heartIcons[h] == null) continue;



                            bool isFull = h < clampedHp;



                            if (isFull)



                            {



                                if (fullSp != null) card.heartIcons[h].sprite = fullSp;



                                card.heartIcons[h].color = Color.white;



                            }



                            else



                            {



                                if (breakSp != null) card.heartIcons[h].sprite = breakSp;



                                card.heartIcons[h].color = new Color(1f, 1f, 1f, 0.35f);



                            }



                        }



                    }



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



            if (Application.absoluteURL != null && (Application.absoluteURL.IndexOf("lab=1", StringComparison.OrdinalIgnoreCase) >= 0 || Application.absoluteURL.IndexOf("screen=lab", StringComparison.OrdinalIgnoreCase) >= 0))



            {



                int labN = GetUrlInt("n", 2);



                StartLabSimulation(labN);



            }



            else if (IsUrlAutoPlay())



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







            // A5: Result SFX (Win/Lose)



            if (isLocalWinner)



            {



                if (AudioManager.Instance != null) AudioManager.Instance.PlayWin();



                else Audio.AudioService.Instance?.PlayWin();



            }



            else



            {



                if (AudioManager.Instance != null) AudioManager.Instance.PlayLose();



                else Audio.AudioService.Instance?.PlayLose();



            }







            // A5: Smooth transition



            if (resultPanel != null)



            {



                var cg = resultPanel.GetComponent<CanvasGroup>();



                if (cg == null) cg = resultPanel.AddComponent<CanvasGroup>();



                StartCoroutine(FadeInResultPanel(cg));



            }



            EnsureVictoryHalo(isLocalWinner);







            // P4-4: Record match stats, daily quest progress & rank points



            var acc = Game.FakeAccountService.Instance;



            if (acc != null)



            {



                acc.MatchesPlayed++;



                int myHits = 0;



                int myRank = 1;



                if (ranked != null)



                {



                    for (int i = 0; i < ranked.Count; i++)



                    {



                        if (ranked[i].Id == "player")



                        {



                            myHits = ranked[i].HitsLanded;



                            myRank = i + 1;



                            break;



                        }



                    }



                }



                QuestService.RecordMatchPlayed(isLocalWinner, myHits);



                int delta = RankService.CalculatePointsDelta(myRank, ranked != null ? ranked.Count : 4);



                acc.RankPoints = Mathf.Max(0, acc.RankPoints + delta);



            }







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



                if (row.txtRankBadge != null)



                {



                    row.txtRankBadge.text = $"HẠNG {i + 1} [TẠM]";



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



                        row.txtStatus.text = isTie ? "ĐỒNG HẠNG 1" : "CHIẾN THẮNG";



                        row.txtStatus.color = new Color(1f, 0.92f, 0.25f);



                    }



                    else



                    {



                        row.txtStatus.text = p.IsAlive ? "Còn sống" : "Đã loại";



                        row.txtStatus.color = p.IsAlive ? Color.white : new Color(1f, 0.44f, 0.44f);



                    }



                }







                if (row.imgBg != null)



                {



                    row.imgBg.color = isMe ? new Color(1f, 1f, 1f, 1f) : new Color(0.92f, 0.95f, 1f, 0.92f);



                }







                // A5: Friend request button after match



                EnsureRowFriendButton(row, p.Id, p.Name, isMe);



            }



        }







        private GameObject _victoryHaloObj;



        private Coroutine _haloRotateCoroutine;







        private void EnsureVictoryHalo(bool isVisible)



        {



            try



            {



                if (resultPanel == null) return;



                if (!isVisible)



                {



                    if (_victoryHaloObj != null) _victoryHaloObj.SetActive(false);



                    return;



                }







                if (_victoryHaloObj == null)



                {



                    var haloGo = new GameObject("VictoryHaloGlow", typeof(RectTransform));



                    haloGo.transform.SetParent(resultPanel.transform, false);



                    haloGo.transform.SetAsFirstSibling();



                    var rt = haloGo.GetComponent<RectTransform>();



                    rt.anchorMin = new Vector2(0.5f, 0.82f);



                    rt.anchorMax = new Vector2(0.5f, 0.82f);



                    rt.sizeDelta = new Vector2(280f, 280f);



                    var img = haloGo.AddComponent<Image>();



                    img.color = new Color(1f, 0.85f, 0.20f, 0.35f);



                    img.raycastTarget = false;



                    _victoryHaloObj = haloGo;



                }







                _victoryHaloObj.SetActive(true);



                if (_haloRotateCoroutine != null) StopCoroutine(_haloRotateCoroutine);



                _haloRotateCoroutine = StartCoroutine(RotateHaloRoutine(_victoryHaloObj.transform));



            }



            catch (Exception ex)



            {



                Debug.LogWarning($"[ProductionUIController] EnsureVictoryHalo caught: {ex.Message}");



            }



        }







        private IEnumerator RotateHaloRoutine(Transform target)



        {



            while (target != null && target.gameObject.activeInHierarchy)



            {



                target.Rotate(0f, 0f, -35f * Time.unscaledDeltaTime);



                yield return null;



            }



        }







        private IEnumerator FadeInResultPanel(CanvasGroup cg)



        {



            if (cg == null) yield break;



            cg.alpha = 0f;



            float duration = 0.25f;



            float elapsed = 0f;



            while (elapsed < duration)



            {



                elapsed += Time.unscaledDeltaTime;



                cg.alpha = Mathf.Clamp01(elapsed / duration);



                yield return null;



            }



            cg.alpha = 1f;



        }







        private void EnsureRowFriendButton(ResultRowUI row, string playerId, string playerName, bool isMe)



        {



            if (row == null || row.root == null) return;







            Transform existing = row.root.transform.Find("BtnFriendRequest");



            if (isMe)



            {



                if (existing != null) existing.gameObject.SetActive(false);



                return;



            }







            Button btn;



            TextMeshProUGUI txt;



            if (existing != null)



            {



                existing.gameObject.SetActive(true);



                btn = existing.GetComponent<Button>();



                txt = existing.GetComponentInChildren<TextMeshProUGUI>();



            }



            else



            {



                var obj = new GameObject("BtnFriendRequest", typeof(RectTransform));



                obj.transform.SetParent(row.root.transform, false);



                var rt = obj.GetComponent<RectTransform>();



                rt.anchorMin = new Vector2(0.78f, 0.20f);



                rt.anchorMax = new Vector2(0.98f, 0.80f);



                rt.sizeDelta = Vector2.zero;







                var img = obj.AddComponent<Image>();



                img.color = theme != null ? theme.actionGreen : new Color(0.18f, 0.65f, 0.38f, 0.95f);







                btn = obj.AddComponent<Button>();







                var txtObj = new GameObject("Text", typeof(RectTransform));



                txtObj.transform.SetParent(obj.transform, false);



                var txtRt = txtObj.GetComponent<RectTransform>();



                txtRt.anchorMin = Vector2.zero;



                txtRt.anchorMax = Vector2.one;



                txtRt.sizeDelta = Vector2.zero;







                txt = txtObj.AddComponent<TextMeshProUGUI>();



                TMP_FontAsset font = GetDefaultFont();



                if (font != null) txt.font = font;



                txt.fontSize = 13f;



                txt.fontStyle = FontStyles.Bold;



                txt.alignment = TextAlignmentOptions.Center;



                txt.color = Color.white;



            }







            if (btn != null && txt != null)



            {



                btn.interactable = true;



                txt.text = UiStrings.Get(UiStrings.BtnAddFriend);



                btn.onClick.RemoveAllListeners();



                btn.onClick.AddListener(() =>



                {



                    AudioManager.Instance?.PlayClick();



                    btn.interactable = false;



                    txt.text = UiStrings.Get(UiStrings.BtnFriendSent);



                    var img = btn.GetComponent<Image>();



                    if (img != null) img.color = new Color(0.40f, 0.45f, 0.50f, 0.85f);



                    ShowToast(string.Format(UiStrings.Get("Đã gửi lời mời kết bạn tới {0}!"), playerName));



                });



            }



        }







        #region In-Game Quick Chat & Emote Tray (A6)







        private GameObject _inGameEmoteTray;



        private Button _btnInGameEmote;







        public void EnsureInGameEmoteTray()



        {



            if (gameplayHudPanel == null) return;







            Transform safeArea = gameplayHudPanel.transform.Find("SafeAreaRoot") ?? gameplayHudPanel.transform;







            if (_btnInGameEmote == null)



            {



                Transform existingBtn = safeArea.Find("BtnInGameEmote");



                if (existingBtn != null)

                {

                    _btnInGameEmote = existingBtn.GetComponent<Button>();

                    var img = _btnInGameEmote.GetComponent<Image>();

                    var circleSprite = btnInGameSettings != null ? btnInGameSettings.GetComponent<Image>()?.sprite : null;

                    if (circleSprite != null && img != null && img.sprite == null)

                    {

                        img.sprite = circleSprite;

                        img.color = Color.white;

                    }

                    var oldTxt = existingBtn.Find("Text");

                    if (oldTxt != null) oldTxt.gameObject.SetActive(false);

                    var iconTr = existingBtn.Find("ChatIcon");

                    if (iconTr == null)

                    {

                        var iconObj = new GameObject("ChatIcon", typeof(RectTransform));

                        iconObj.transform.SetParent(existingBtn, false);

                        var iconRt = iconObj.GetComponent<RectTransform>();

                        iconRt.anchorMin = new Vector2(0.5f, 0.5f);

                        iconRt.anchorMax = new Vector2(0.5f, 0.5f);

                        iconRt.pivot = new Vector2(0.5f, 0.5f);

                        iconRt.sizeDelta = new Vector2(40f, 40f);

                        var iconImg = iconObj.AddComponent<Image>();

                        iconImg.sprite = spriteSpeechBubble;

#if UNITY_EDITOR

                        if (iconImg.sprite == null) iconImg.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Kit/ui_speech_bubble.png");

#endif

                        iconImg.preserveAspect = true;

                        iconImg.color = iconImg.sprite != null ? new Color(1.0f, 0.85f, 0.25f, 1.0f) : Color.clear;

                        iconImg.enabled = iconImg.sprite != null;

                        iconImg.raycastTarget = false;

                    }

                    else

                    {

                        var existingIconImg = iconTr.GetComponent<Image>();

                        if (existingIconImg != null && existingIconImg.sprite == null && spriteSpeechBubble != null)

                        {

                            existingIconImg.sprite = spriteSpeechBubble;

                            existingIconImg.color = new Color(1.0f, 0.85f, 0.25f, 1.0f);

                            existingIconImg.enabled = true;

                        }

                    }

                }



                else



                {



                    var btnObj = new GameObject("BtnInGameEmote", typeof(RectTransform));



                    btnObj.transform.SetParent(safeArea, false);



                    var rt = btnObj.GetComponent<RectTransform>();



                    rt.anchorMin = new Vector2(0.92f, 0.08f);



                    rt.anchorMax = new Vector2(0.92f, 0.08f);



                    rt.pivot = new Vector2(0.5f, 0.5f);



                    rt.sizeDelta = new Vector2(72f, 72f);







                    var img = btnObj.AddComponent<Image>();



                    var circleSprite = btnInGameSettings != null ? btnInGameSettings.GetComponent<Image>()?.sprite : null;



                    if (circleSprite != null)



                    {



                        img.sprite = circleSprite;



                        img.color = Color.white;



                    }



                    else



                    {



                        img.color = new Color(0.12f, 0.16f, 0.24f, 0.90f);



                    }







                    _btnInGameEmote = btnObj.AddComponent<Button>();



                    var iconObj = new GameObject("ChatIcon", typeof(RectTransform));

                    iconObj.transform.SetParent(btnObj.transform, false);

                    var iconRt = iconObj.GetComponent<RectTransform>();

                    iconRt.anchorMin = new Vector2(0.5f, 0.5f);

                    iconRt.anchorMax = new Vector2(0.5f, 0.5f);

                    iconRt.pivot = new Vector2(0.5f, 0.5f);

                    iconRt.sizeDelta = new Vector2(40f, 40f);



                    var iconImg = iconObj.AddComponent<Image>();

                    iconImg.sprite = spriteSpeechBubble;

#if UNITY_EDITOR

                    if (iconImg.sprite == null) iconImg.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Kit/ui_speech_bubble.png");

#endif

                    iconImg.preserveAspect = true;

                    iconImg.color = iconImg.sprite != null ? new Color(1.0f, 0.85f, 0.25f, 1.0f) : Color.clear;

                    iconImg.enabled = iconImg.sprite != null;

                    iconImg.raycastTarget = false;







                    _btnInGameEmote.onClick.AddListener(() =>



                    {



                        AudioManager.Instance?.PlayClick();



                        if (_inGameEmoteTray != null)



                        {



                            _inGameEmoteTray.SetActive(!_inGameEmoteTray.activeSelf);



                        }



                    });



                }



            }







            if (_inGameEmoteTray == null)



            {



                Transform existingTray = safeArea.Find("InGameEmoteTray");



                if (existingTray != null)



                {



                    _inGameEmoteTray = existingTray.gameObject;



                }



                else



                {



                    var trayObj = new GameObject("InGameEmoteTray", typeof(RectTransform));



                    trayObj.transform.SetParent(safeArea, false);



                    var trayRt = trayObj.GetComponent<RectTransform>();



                    trayRt.anchorMin = new Vector2(0.15f, 0.21f);



                    trayRt.anchorMax = new Vector2(0.96f, 0.36f);



                    trayRt.sizeDelta = Vector2.zero;







                    var trayBg = trayObj.AddComponent<Image>();



                    trayBg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);



                    var trayOutline = trayObj.AddComponent<Outline>();



                    trayOutline.effectColor = new Color(0.78f, 0.58f, 0.20f, 0.85f);



                    trayOutline.effectDistance = new Vector2(2f, -2f);







                    // Row 1: Emote Icons



                    string[] emotes = { "Haha", "Hay", "Né đi", "Ôi", "Cay" };



                    float colWidth = 1f / emotes.Length;



                    for (int i = 0; i < emotes.Length; i++)



                    {



                        string emote = emotes[i];



                        var itemObj = new GameObject("Emote_" + i, typeof(RectTransform));



                        itemObj.transform.SetParent(trayObj.transform, false);



                        var itemRt = itemObj.GetComponent<RectTransform>();



                        itemRt.anchorMin = new Vector2(i * colWidth + 0.02f, 0.54f);



                        itemRt.anchorMax = new Vector2((i + 1) * colWidth - 0.02f, 0.94f);



                        itemRt.sizeDelta = Vector2.zero;







                        var itemImg = itemObj.AddComponent<Image>();



                        itemImg.color = new Color(0.20f, 0.25f, 0.35f, 0.90f);







                        var itemBtn = itemObj.AddComponent<Button>();







                        var itemTxtObj = new GameObject("Text", typeof(RectTransform));



                        itemTxtObj.transform.SetParent(itemObj.transform, false);



                        var itemTxtRt = itemTxtObj.GetComponent<RectTransform>();



                        itemTxtRt.anchorMin = Vector2.zero;



                        itemTxtRt.anchorMax = Vector2.one;



                        itemTxtRt.sizeDelta = Vector2.zero;







                        var itemTxt = itemTxtObj.AddComponent<TextMeshProUGUI>();



                        itemTxt.font = GetDefaultFont();



                        itemTxt.fontSize = 20f;



                        itemTxt.alignment = TextAlignmentOptions.Center;



                        itemTxt.color = Color.white;



                        itemTxt.text = emote;







                        itemBtn.onClick.AddListener(() =>



                        {



                            SendInGameEmoteOrPhrase(emote);



                        });



                    }







                    // Row 2: Quick Chat Phrases



                    string[] phrases = { "Hay quá!", "Trận sau phục thù!", "Đừng ném tôi!" };



                    float pColWidth = 1f / phrases.Length;



                    for (int i = 0; i < phrases.Length; i++)



                    {



                        string phrase = phrases[i];



                        var pObj = new GameObject("Phrase_" + i, typeof(RectTransform));



                        pObj.transform.SetParent(trayObj.transform, false);



                        var pRt = pObj.GetComponent<RectTransform>();



                        pRt.anchorMin = new Vector2(i * pColWidth + 0.02f, 0.08f);



                        pRt.anchorMax = new Vector2((i + 1) * pColWidth - 0.02f, 0.48f);



                        pRt.sizeDelta = Vector2.zero;







                        var pImg = pObj.AddComponent<Image>();



                        pImg.color = new Color(0.16f, 0.22f, 0.32f, 0.90f);







                        var pBtn = pObj.AddComponent<Button>();







                        var pTxtObj = new GameObject("Text", typeof(RectTransform));



                        pTxtObj.transform.SetParent(pObj.transform, false);



                        var pTxtRt = pTxtObj.GetComponent<RectTransform>();



                        pTxtRt.anchorMin = Vector2.zero;



                        pTxtRt.anchorMax = Vector2.one;



                        pTxtRt.sizeDelta = Vector2.zero;







                        var pTxt = pTxtObj.AddComponent<TextMeshProUGUI>();



                        pTxt.font = GetDefaultFont();



                        pTxt.fontSize = 11f;



                        pTxt.fontStyle = FontStyles.Bold;



                        pTxt.alignment = TextAlignmentOptions.Center;



                        pTxt.color = new Color(1f, 0.9f, 0.4f);



                        pTxt.text = phrase;







                        pBtn.onClick.AddListener(() =>



                        {



                            SendInGameEmoteOrPhrase(phrase);



                        });



                    }







                    _inGameEmoteTray = trayObj;



                    _inGameEmoteTray.SetActive(false);



                }



            }



        }







        private void SendInGameEmoteOrPhrase(string content)



        {



            if (AudioManager.Instance != null) AudioManager.Instance.PlayChatPop();



            else Audio.AudioService.Instance?.PlayChatPop();







            if (_inGameEmoteTray != null) _inGameEmoteTray.SetActive(false);







            ShowToast($"Nam Làng: {content}", 2.0f);



        }







        #endregion



    }



}



