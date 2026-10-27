using System;
using UnityEngine;
using UnityEngine.UI;
using HitMe.UI;

namespace HitMe.Presentation
{
    /// <summary>
    /// Presentation controller for the Main Menu screen in HIT ME VN.
    /// Works both standalone (VisualPreview) and bound to the authoritative UIManager.
    /// </summary>
    public class MainMenuPresenter : MonoBehaviour
    {
        [Header("Runtime Binding")]
        [SerializeField] private UIManager _uiManager;

        [Header("Top Profile Bar")]
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Text _playerSubText;
        [SerializeField] private Text _coinBalanceText;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _addCoinButton;

        [Header("Character & Weapon Showcase")]
        [SerializeField] private Image _characterShowcaseImage;
        [SerializeField] private Image _characterShadow;
        [SerializeField] private Text _selectedCharacterNameText;
        [SerializeField] private Text _selectedWeaponNameText;
        [SerializeField] private Image _selectedWeaponIcon;
        [SerializeField] private Button _prevCharacterButton;
        [SerializeField] private Button _nextCharacterButton;
        [SerializeField] private Button _characterCardButton;
        [SerializeField] private Button _weaponCardButton;

        [Header("Action Buttons")]
        [SerializeField] private Button _quickPlayButton;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private Button _trainWithBotButton;

        [Header("Bottom Navigation")]
        [SerializeField] private Button _homeTabButton;
        [SerializeField] private Button _rankingTabButton;
        [SerializeField] private Button _questTabButton;

        [Header("Toast / Notification")]
        [SerializeField] private GameObject _toastPanel;
        [SerializeField] private Text _toastText;

        public event Action OnQuickPlayClicked;
        public event Action OnTrainWithBotClicked;
        public event Action OnCreateRoomClicked;
        public event Action OnJoinRoomClicked;
        public event Action OnCharacterSelectionClicked;
        public event Action OnWeaponSelectionClicked;

        private void Awake()
        {
            if (_uiManager == null)
            {
                _uiManager = FindAnyObjectByType<UIManager>();
            }

            BindButtons();
        }

        private void BindButtons()
        {
            if (_quickPlayButton != null)
                _quickPlayButton.onClick.AddListener(HandleQuickPlay);

            if (_trainWithBotButton != null)
                _trainWithBotButton.onClick.AddListener(HandleTrainWithBot);

            if (_createRoomButton != null)
                _createRoomButton.onClick.AddListener(HandleCreateRoom);

            if (_joinRoomButton != null)
                _joinRoomButton.onClick.AddListener(HandleJoinRoom);

            if (_characterCardButton != null)
                _characterCardButton.onClick.AddListener(HandleCharacterSelection);

            if (_weaponCardButton != null)
                _weaponCardButton.onClick.AddListener(HandleWeaponSelection);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(() => ShowToast("Cài đặt"));

            if (_addCoinButton != null)
                _addCoinButton.onClick.AddListener(() => ShowToast("Nạp xu (Sắp ra mắt)"));

            if (_rankingTabButton != null)
                _rankingTabButton.onClick.AddListener(() => ShowToast("Bảng xếp hạng (Sắp ra mắt)"));

            if (_questTabButton != null)
                _questTabButton.onClick.AddListener(() => ShowToast("Nhiệm vụ (Sắp ra mắt)"));
        }

        private void HandleQuickPlay()
        {
            OnQuickPlayClicked?.Invoke();
            if (_uiManager != null && _uiManager.btnQuickMatch != null)
            {
                _uiManager.btnQuickMatch.onClick.Invoke();
            }
            else
            {
                ShowToast("Bắt đầu Chơi Nhanh...");
            }
        }

        private void HandleTrainWithBot()
        {
            OnTrainWithBotClicked?.Invoke();
            if (_uiManager != null && _uiManager.btnTrainWithBot != null)
            {
                _uiManager.btnTrainWithBot.onClick.Invoke();
            }
            else
            {
                ShowToast("Vào luyện tập với Bot...");
            }
        }

        private void HandleCreateRoom()
        {
            OnCreateRoomClicked?.Invoke();
            if (_uiManager != null)
            {
                _uiManager.ShowNotice("Tính năng Tạo phòng đang phát triển (Chưa khả dụng ở bản local)");
            }
            else
            {
                ShowToast("Tạo phòng (Chưa khả dụng ở bản offline)");
            }
        }

        private void HandleJoinRoom()
        {
            OnJoinRoomClicked?.Invoke();
            ShowToast("Vào phòng (Chưa khả dụng ở bản offline)");
        }

        private void HandleCharacterSelection()
        {
            OnCharacterSelectionClicked?.Invoke();
            ShowToast("Chọn nhân vật: Nam Làng");
        }

        private void HandleWeaponSelection()
        {
            OnWeaponSelectionClicked?.Invoke();
            ShowToast("Chọn vũ khí: Dép tổ ong");
        }

        public void SetPlayerInfo(string name, int coins)
        {
            if (_playerNameText != null) _playerNameText.text = name;
            if (_coinBalanceText != null) _coinBalanceText.text = coins.ToString("N0");
        }

        public void SetCharacterShowcase(Sprite characterSprite, string name)
        {
            if (_characterShowcaseImage != null) _characterShowcaseImage.sprite = characterSprite;
            if (_selectedCharacterNameText != null) _selectedCharacterNameText.text = name;
        }

        public void SetWeaponShowcase(Sprite weaponIcon, string name)
        {
            if (_selectedWeaponIcon != null) _selectedWeaponIcon.sprite = weaponIcon;
            if (_selectedWeaponNameText != null) _selectedWeaponNameText.text = name;
        }

        public void ShowToast(string message)
        {
            if (_toastPanel != null && _toastText != null)
            {
                _toastText.text = message;
                _toastPanel.SetActive(true);
                CancelInvoke(nameof(HideToast));
                Invoke(nameof(HideToast), 2.5f);
            }
            else if (_uiManager != null)
            {
                _uiManager.ShowNotice(message);
            }
        }

        private void HideToast()
        {
            if (_toastPanel != null) _toastPanel.SetActive(false);
        }
    }
}
