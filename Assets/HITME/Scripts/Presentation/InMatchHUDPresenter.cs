using System;
using UnityEngine;
using UnityEngine.UI;
using HitMe.UI;
using HitMe.Gameplay;

namespace HitMe.Presentation
{
    /// <summary>
    /// Presentation presenter for In-Match HUD in HIT ME VN.
    /// Manages the polished mobile HUD: HP hearts, round counter, animated timer,
    /// ready status chip, weapon slot, and aim indicator preview.
    /// Strictly presentation only — never mutates gameplay authority.
    /// </summary>
    public class InMatchHUDPresenter : MonoBehaviour
    {
        [Header("Authoritative Bindings")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private AimController _aimController;

        [Header("Top Bar")]
        [SerializeField] private Text _roundText;
        [SerializeField] private Text _timerText;
        [SerializeField] private Image _timerFill;
        [SerializeField] private Button _pauseButton;

        [Header("Player Status (Left)")]
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Image[] _playerHeartIcons; // 3 hearts
        [SerializeField] private Sprite _heartFullSprite;
        [SerializeField] private Sprite _heartEmptySprite;

        [Header("Opponent Status (Right)")]
        [SerializeField] private Text _opponentNameText;
        [SerializeField] private Image[] _opponentHeartIcons; // 3 hearts

        [Header("Phase Banner")]
        [SerializeField] private GameObject _phaseBannerContainer;
        [SerializeField] private Text _phaseBannerText;

        [Header("Bottom HUD")]
        [SerializeField] private Image _weaponIcon;
        [SerializeField] private Text _weaponNameText;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Text _readyButtonText;
        [SerializeField] private GameObject _lockedStatusBadge;
        [SerializeField] private Button _quickChatButton;
        [SerializeField] private Button _emoteButton;

        [Header("Aim Indicator Preview")]
        [SerializeField] private RectTransform _aimRulerTransform;
        [SerializeField] private Image _aimDottedLine;
        [SerializeField] private Image _aimArrowHead;

        private int _maxHp = 3;
        private int _currentPlayerHp = 3;
        private int _currentOpponentHp = 3;

        private void Awake()
        {
            if (_uiManager == null) _uiManager = FindAnyObjectByType<UIManager>();
            if (_aimController == null) _aimController = FindAnyObjectByType<AimController>();

            if (_readyButton != null)
            {
                _readyButton.onClick.AddListener(HandleReadyClick);
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(HandlePauseClick);
            }
        }

        private void Update()
        {
            UpdateAimIndicator();
        }

        private void HandleReadyClick()
        {
            if (_uiManager != null && _uiManager.btnReady != null)
            {
                _uiManager.btnReady.onClick.Invoke();
            }
            SetLockedState(true);
        }

        private void HandlePauseClick()
        {
            if (_uiManager != null)
            {
                _uiManager.ShowNotice("Tạm dừng trận đấu");
            }
        }

        public void UpdateRound(int round, int maxRounds = 20)
        {
            if (_roundText != null)
            {
                _roundText.text = $"LƯỢT {round}/{maxRounds}";
            }
        }

        public void UpdateTimer(float secondsRemaining, float totalSeconds = 5f)
        {
            if (_timerText != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining));
                _timerText.text = $"{s:00}s";
                _timerText.color = (s <= 2) ? new Color(1f, 0.25f, 0.25f) : new Color(1f, 0.95f, 0.65f);
            }

            if (_timerFill != null)
            {
                _timerFill.fillAmount = Mathf.Clamp01(secondsRemaining / Mathf.Max(0.01f, totalSeconds));
            }
        }

        public void UpdateHearts(int playerHp, int opponentHp, int maxHp = 3)
        {
            _maxHp = maxHp;
            _currentPlayerHp = Mathf.Clamp(playerHp, 0, maxHp);
            _currentOpponentHp = Mathf.Clamp(opponentHp, 0, maxHp);

            if (_playerHeartIcons != null)
            {
                for (int i = 0; i < _playerHeartIcons.Length; i++)
                {
                    if (_playerHeartIcons[i] != null)
                    {
                        bool active = i < _currentPlayerHp;
                        _playerHeartIcons[i].sprite = active ? _heartFullSprite : _heartEmptySprite;
                        _playerHeartIcons[i].color = active ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    }
                }
            }

            if (_opponentHeartIcons != null)
            {
                for (int i = 0; i < _opponentHeartIcons.Length; i++)
                {
                    if (_opponentHeartIcons[i] != null)
                    {
                        bool active = i < _currentOpponentHp;
                        _opponentHeartIcons[i].sprite = active ? _heartFullSprite : _heartEmptySprite;
                        _opponentHeartIcons[i].color = active ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    }
                }
            }
        }

        public void SetPhaseBanner(string phaseName)
        {
            if (_phaseBannerContainer != null && _phaseBannerText != null)
            {
                bool show = !string.IsNullOrEmpty(phaseName);
                _phaseBannerContainer.SetActive(show);
                if (show) _phaseBannerText.text = phaseName;
            }
        }

        public void SetLockedState(bool isLocked)
        {
            if (_readyButton != null)
            {
                _readyButton.interactable = !isLocked;
            }

            if (_lockedStatusBadge != null)
            {
                _lockedStatusBadge.SetActive(isLocked);
            }

            if (_readyButtonText != null)
            {
                _readyButtonText.text = isLocked ? "ĐÃ SẴN SÀNG" : "KHÓA LƯỢT";
            }
        }

        public void SetEquippedWeapon(Sprite icon, string name)
        {
            if (_weaponIcon != null) _weaponIcon.sprite = icon;
            if (_weaponNameText != null) _weaponNameText.text = name;
        }

        private void UpdateAimIndicator()
        {
            if (_aimRulerTransform == null) return;

            if (_aimController != null && _aimController.isActiveAndEnabled)
            {
                _aimRulerTransform.gameObject.SetActive(true);
                Vector2 dir = _aimController.CurrentDirection;
                if (dir.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    _aimRulerTransform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
            else
            {
                _aimRulerTransform.gameObject.SetActive(false);
            }
        }
    }
}
