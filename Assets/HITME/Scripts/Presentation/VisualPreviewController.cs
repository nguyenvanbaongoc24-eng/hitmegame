using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.Presentation
{
    public enum PreviewMode
    {
        MainMenu,
        ArenaStaging,
        InMatchHUD
    }

    /// <summary>
    /// Visual Preview controller for AG's dedicated VisualPreview scene.
    /// Provides live switching between Main Menu, Arena Staging, and In-Match HUD,
    /// weapon cycling, and interactive simulated gameplay phases without touching core logic.
    /// </summary>
    public class VisualPreviewController : MonoBehaviour
    {
        [Header("Preview Containers")]
        [SerializeField] private GameObject _mainMenuContainer;
        [SerializeField] private GameObject _arenaStagingContainer;
        [SerializeField] private GameObject _inMatchHUDContainer;

        [Header("Presenters")]
        [SerializeField] private MainMenuPresenter _mainMenuPresenter;
        [SerializeField] private InMatchHUDPresenter _hudPresenter;

        [Header("Character Showcase Sprites")]
        [SerializeField] private Sprite[] _characterShowcaseSprites;
        [SerializeField] private string[] _characterNames;
        [SerializeField] private Sprite[] _weaponIcons;
        [SerializeField] private string[] _weaponNames;

        [Header("Simulated Match State")]
        [SerializeField] private float _planningDuration = 5f;
        private float _currentTimer;
        private int _currentRound = 1;
        private int _playerHp = 3;
        private int _opponentHp = 3;
        private bool _isPlanningActive;
        private int _selectedCharIndex = 0;
        private int _selectedWeaponIndex = 0;

        private void Start()
        {
            SetPreviewMode(PreviewMode.MainMenu);
            UpdateShowcase();
        }

        public void SetPreviewMode(PreviewMode mode)
        {
            if (_mainMenuContainer != null) _mainMenuContainer.SetActive(mode == PreviewMode.MainMenu);
            if (_arenaStagingContainer != null) _arenaStagingContainer.SetActive(mode == PreviewMode.ArenaStaging || mode == PreviewMode.InMatchHUD);
            if (_inMatchHUDContainer != null) _inMatchHUDContainer.SetActive(mode == PreviewMode.InMatchHUD);

            if (mode == PreviewMode.InMatchHUD)
            {
                StartSimulatedPlanning();
            }
            else
            {
                _isPlanningActive = false;
            }
        }

        public void CycleCharacter(int delta)
        {
            if (_characterShowcaseSprites == null || _characterShowcaseSprites.Length == 0) return;
            _selectedCharIndex = (_selectedCharIndex + delta + _characterShowcaseSprites.Length) % _characterShowcaseSprites.Length;
            UpdateShowcase();
        }

        public void CycleWeapon(int delta)
        {
            if (_weaponIcons == null || _weaponIcons.Length == 0) return;
            _selectedWeaponIndex = (_selectedWeaponIndex + delta + _weaponIcons.Length) % _weaponIcons.Length;
            UpdateShowcase();
        }

        private void UpdateShowcase()
        {
            if (_mainMenuPresenter != null)
            {
                if (_characterShowcaseSprites != null && _selectedCharIndex < _characterShowcaseSprites.Length)
                {
                    string cName = (_characterNames != null && _selectedCharIndex < _characterNames.Length) ? _characterNames[_selectedCharIndex] : "Nam Làng";
                    _mainMenuPresenter.SetCharacterShowcase(_characterShowcaseSprites[_selectedCharIndex], cName);
                }

                if (_weaponIcons != null && _selectedWeaponIndex < _weaponIcons.Length)
                {
                    string wName = (_weaponNames != null && _selectedWeaponIndex < _weaponNames.Length) ? _weaponNames[_selectedWeaponIndex] : "Dép";
                    _mainMenuPresenter.SetWeaponShowcase(_weaponIcons[_selectedWeaponIndex], wName);
                }
            }
        }

        private void StartSimulatedPlanning()
        {
            _isPlanningActive = true;
            _currentTimer = _planningDuration;
            if (_hudPresenter != null)
            {
                _hudPresenter.UpdateRound(_currentRound);
                _hudPresenter.UpdateHearts(_playerHp, _opponentHp);
                _hudPresenter.SetLockedState(false);
                _hudPresenter.SetPhaseBanner("LƯỢT 1: CHỌN VỊ TRÍ & NGẮM");
            }
        }

        private void Update()
        {
            if (_isPlanningActive)
            {
                _currentTimer -= Time.deltaTime;
                if (_hudPresenter != null)
                {
                    _hudPresenter.UpdateTimer(_currentTimer, _planningDuration);
                }

                if (_currentTimer <= 0f)
                {
                    _isPlanningActive = false;
                    if (_hudPresenter != null)
                    {
                        _hudPresenter.SetLockedState(true);
                        _hudPresenter.SetPhaseBanner("HẾT GIỜ • ĐỒNG THỜI NÉM");
                    }
                }
            }
        }
    }
}
