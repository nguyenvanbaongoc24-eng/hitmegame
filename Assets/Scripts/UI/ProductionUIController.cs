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
        private const string StatusHiddenText = "ẨN";
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

        [Header("Screen Panels")]
        public GameObject homePanel;
        public GameObject createRoomPanel;
        public GameObject joinRoomPanel;
        public GameObject lobbyPanel;
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

        [Header("Gameplay HUD Controls")]
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

        private int _lastDisplayedRound = -1;
        private int _lastDisplayedMaxRounds = -1;
        private int _lastDisplayedTimerTenths = -1;
        private bool _lastTimerLowWarning;
        private Phase _lastDisplayedPhase = (Phase)(-1);

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

            BindEvents();
        }

        private void Start()
        {
            if (matchController != null)
            {
                matchController.OnRoundStarted += HandleRoundStarted;
                matchController.OnPhaseChanged += HandlePhaseChanged;
                matchController.OnTimerTick += HandleTimerTick;
                matchController.OnToastRequested += ShowToast;
                matchController.OnMatchEnded += HandleMatchEnded;
            }

            EnsureAndRefreshBuildFingerprint();

            bool isUrlAutoPlay = Application.absoluteURL != null &&
                                 Application.absoluteURL.IndexOf("autoPlay=", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isUrlAutoPlay)
            {
                ShowScreen(ProductionScreen.Gameplay);
            }
            else
            {
                ShowScreen(ProductionScreen.Home);
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
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 8f);
                rt.sizeDelta = new Vector2(-36f, 24f);

                txtBuildFingerprint = go.AddComponent<TextMeshProUGUI>();
                if (txtHomeLogo != null && txtHomeLogo.font != null)
                {
                    txtBuildFingerprint.font = txtHomeLogo.font;
                }
                else if (txtRound != null && txtRound.font != null)
                {
                    txtBuildFingerprint.font = txtRound.font;
                }
                txtBuildFingerprint.fontSize = 15f;
                txtBuildFingerprint.color = new Color(0.92f, 0.95f, 1f, 0.48f);
                txtBuildFingerprint.alignment = TextAlignmentOptions.BottomRight;
                txtBuildFingerprint.raycastTarget = false;
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
                    StartGameplayMatch();
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
                btnReplay.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    StartGameplayMatch();
                });
            }

            if (btnBackToHome != null)
            {
                btnBackToHome.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowScreen(ProductionScreen.Home);
                });
            }
        }

        public void ShowScreen(ProductionScreen screen)
        {
            CurrentScreen = screen;

            if (homePanel != null) homePanel.SetActive(screen == ProductionScreen.Home);
            if (createRoomPanel != null) createRoomPanel.SetActive(screen == ProductionScreen.CreateRoom);
            if (joinRoomPanel != null) joinRoomPanel.SetActive(screen == ProductionScreen.JoinRoom);
            if (lobbyPanel != null) lobbyPanel.SetActive(screen == ProductionScreen.Lobby);
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
            ShowScreen(ProductionScreen.Lobby);
            _isLocalPlayerReady = false;

            if (txtLobbyHeader != null) txtLobbyHeader.text = roomName.ToUpperInvariant() + " • SÂN LÀNG";
            if (txtLobbyRoomCode != null) txtLobbyRoomCode.text = "MÃ PHÒNG: " + code;
            if (txtLobbyCountdown != null) txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
            if (txtLobbyReadyLabel != null) txtLobbyReadyLabel.text = "SẴN SÀNG";

            for (int i = 0; i < lobbySlots.Count && i < PlayerView.CharacterNames.Length; i++)
            {
                var slot = lobbySlots[i];
                if (slot.root != null) slot.root.SetActive(true);
                if (slot.txtName != null)
                {
                    slot.txtName.text = i == 0 ? "Nam Làng (Bạn)" : PlayerView.CharacterNames[i];
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
        }

        private void ToggleLobbyReady()
        {
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

            bool allReady = lobbySlots.Count > 0;
            for (int i = 0; i < lobbySlots.Count; i++)
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

        public void StartGameplayMatch()
        {
            if (_lobbyCountdownCoroutine != null) StopCoroutine(_lobbyCountdownCoroutine);
            ShowScreen(ProductionScreen.Gameplay);

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
            UpdateHpCards();
        }

        private void HandlePhaseChanged(Phase phase)
        {
            if (txtPhaseBanner != null && phase != _lastDisplayedPhase)
            {
                _lastDisplayedPhase = phase;
                switch (phase)
                {
                    case Phase.Placing:
                        txtPhaseBanner.text = "CHỌN HƯỚNG NÉM • Chạm để đặt, kéo để ngắm";
                        break;
                    case Phase.Reveal:
                        txtPhaseBanner.text = "LỘ DIỆN ĐỐI THỦ!";
                        break;
                    case Phase.Throwing:
                        txtPhaseBanner.text = "NÉM DÉP!";
                        break;
                    case Phase.Resolve:
                        txtPhaseBanner.text = "KẾT QUẢ VÒNG";
                        break;
                    case Phase.MatchEnd:
                        txtPhaseBanner.text = "KẾT QUẢ TRẬN ĐẤU";
                        break;
                }
            }

            UpdateHpCards();
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

        private void UpdateHpCards()
        {
            if (matchController == null || matchController.State == null) return;
            var state = matchController.State;
            var phase = state.Phase;

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

                bool isLocal = player.Id == "player";
                if (phase == Phase.Placing && !isLocal)
                {
                    if (card.txtHearts != null && card.lastDisplayedHpCode != -1)
                    {
                        card.lastDisplayedHpCode = -1;
                        card.txtHearts.text = HpHiddenText;
                        card.txtHearts.color = new Color(0.6f, 0.65f, 0.7f);
                    }
                    if (card.txtHpNum != null && card.lastDisplayedStatusCode != -1)
                    {
                        card.lastDisplayedStatusCode = -1;
                        card.txtHpNum.text = StatusHiddenText;
                        card.txtHpNum.color = new Color(0.6f, 0.65f, 0.7f);
                    }
                }
                else
                {
                    int clampedHp = Mathf.Clamp(player.Hp, 0, 3);
                    if (card.txtHearts != null && card.lastDisplayedHpCode != clampedHp)
                    {
                        card.lastDisplayedHpCode = clampedHp;
                        card.txtHearts.text = CachedHpCardStrings[clampedHp];
                        card.txtHearts.color = player.IsAlive
                            ? (clampedHp == 3 ? new Color(0.2f, 1f, 0.4f) : (clampedHp == 2 ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f)))
                            : Color.gray;
                    }

                    int statusCode = player.IsAlive ? 1 : 0;
                    if (card.txtHpNum != null && card.lastDisplayedStatusCode != statusCode)
                    {
                        card.lastDisplayedStatusCode = statusCode;
                        card.txtHpNum.text = player.IsAlive ? StatusAliveText : StatusEliminatedText;
                        card.txtHpNum.color = player.IsAlive ? new Color(0.8f, 0.9f, 1f) : Color.gray;
                    }
                }
            }
        }

        private void HandleMatchEnded(List<PlayerState> ranked, List<string> winnerIds)
        {
            bool isUrlAutoPlay = Application.absoluteURL != null &&
                                 Application.absoluteURL.IndexOf("autoPlay=", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isUrlAutoPlay)
            {
                return;
            }

            ShowScreen(ProductionScreen.Result);

            bool isLocalWinner = winnerIds.Contains("player");
            if (txtResultTitle != null)
            {
                txtResultTitle.text = isLocalWinner ? "CHIẾN THẮNG! 🏆" : (winnerIds.Count > 0 ? "THẤT BẠI! 💀" : "HÒA NHAU! ⚖️");
                txtResultTitle.color = isLocalWinner ? new Color(1f, 0.9f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            }

            if (txtResultSubtitle != null)
            {
                var sb = new StringBuilder("Người thắng: ");
                bool first = true;
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (!winnerIds.Contains(ranked[i].Id)) continue;
                    if (!first) sb.Append(", ");
                    sb.Append(ranked[i].Name);
                    first = false;
                }
                txtResultSubtitle.text = sb.ToString();
            }

            for (int i = 0; i < resultRows.Count && i < ranked.Count; i++)
            {
                var row = resultRows[i];
                var p = ranked[i];
                if (row.root != null) row.root.SetActive(true);
                if (row.txtRank != null)
                {
                    row.txtRank.text = i < CachedRankStrings.Length ? CachedRankStrings[i] : ("#" + (i + 1));
                }
                if (row.txtName != null) row.txtName.text = p.Name;
                if (row.txtHp != null)
                {
                    int hpIdx = Mathf.Clamp(p.Hp, 0, CachedResultHpStrings.Length - 1);
                    row.txtHp.text = CachedResultHpStrings[hpIdx];
                }

                bool won = winnerIds.Contains(p.Id);
                if (row.txtStatus != null)
                {
                    row.txtStatus.text = won ? "★ THẮNG" : (p.IsAlive ? "Còn sống" : "Đã loại");
                    row.txtStatus.color = won ? new Color(1f, 0.9f, 0.2f) : (p.IsAlive ? Color.white : Color.gray);
                }

                if (row.imgBg != null)
                {
                    bool isMe = p.Id == "player";
                    row.imgBg.color = isMe ? new Color(0.2f, 0.7f, 1f, 0.35f) : new Color(0.1f, 0.15f, 0.2f, 0.6f);
                }
            }
        }
    }
}
