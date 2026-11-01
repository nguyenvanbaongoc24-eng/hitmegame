using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Audio
{
    /// <summary>
    /// Lô 5: AudioService with zero-alloc AudioSource pool and iOS Safari first-touch audio unlocker.
    /// Manages BGM and SFX playback, volume persistence via PlayerPrefs.
    /// Missing SFX (flight/dép bay): Marked as missing without synthesizing fake audio.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public int sfxPoolSize = 10;

        [Header("BGM Clips")]
        public AudioClip bgmMenu;
        public AudioClip bgmMatch;

        [Header("SFX Clips")]
        public AudioClip sfxClick;
        public AudioClip sfxPlace;
        public AudioClip sfxReady;
        public AudioClip sfxReveal;
        public AudioClip sfxThrow;
        public AudioClip sfxHit;
        public AudioClip sfxDamage;
        public AudioClip sfxCountdownTick;
        public AudioClip sfxCountdownLast;
        public AudioClip sfxWin;
        public AudioClip sfxLose;

        /// <summary>
        /// Slipper flight SFX: Missing in current assets.
        /// Preserved as null / placeholder per specification (do not synthesize fake audio).
        /// </summary>
        public AudioClip sfxFlightMissing = null;

        private readonly List<AudioSource> _sfxPool = new List<AudioSource>(16);
        private int _poolIndex = 0;

        private bool _audioUnlocked = false;
        private float _musicVolume = 0.50f;
        private float _sfxVolume = 0.80f;
        private bool _musicMuted = false;
        private bool _sfxMuted = false;

        public const string PrefKeyMusicVol = "HitMe_MusicVol";
        public const string PrefKeySfxVol = "HitMe_SfxVol";
        public const string PrefKeyMusicMute = "HitMe_MusicMute";
        public const string PrefKeySfxMute = "HitMe_SfxMute";

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (bgmSource != null) bgmSource.volume = _musicMuted ? 0f : _musicVolume;
                SavePrefs();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                SavePrefs();
            }
        }

        public bool MusicMuted
        {
            get => _musicMuted;
            set
            {
                _musicMuted = value;
                if (bgmSource != null) bgmSource.volume = _musicMuted ? 0f : _musicVolume;
                SavePrefs();
            }
        }

        public bool SfxMuted
        {
            get => _sfxMuted;
            set
            {
                _sfxMuted = value;
                SavePrefs();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadPrefs();

            // Setup BGM Source
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = _musicMuted ? 0f : _musicVolume;

            // Setup zero-alloc SFX AudioSource pool
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.volume = _sfxVolume;
                _sfxPool.Add(src);
            }
        }

        private void Update()
        {
            // iOS Safari Audio Unlock: On first user touch/click, resume WebAudio context
            if (!_audioUnlocked)
            {
                if (Input.anyKeyDown || Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                {
                    UnlockAudio();
                }
            }
        }

        /// <summary>
        /// iOS Safari requires user gesture to un-mute WebAudio AudioContext.
        /// </summary>
        public void UnlockAudio()
        {
            if (_audioUnlocked) return;
            _audioUnlocked = true;

            // Play a short silent sample or activate audio sources
            if (_sfxPool.Count > 0 && _sfxPool[0] != null)
            {
                _sfxPool[0].PlayOneShot(_sfxPool[0].clip != null ? _sfxPool[0].clip : sfxClick, 0.001f);
            }
        }

        // ──────────────────────────────────────────────────
        // Zero-Alloc Playback via Pool
        // ──────────────────────────────────────────────────

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || _sfxMuted || _sfxPool.Count == 0) return;

            var src = _sfxPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _sfxPool.Count;

            src.volume = _sfxVolume;
            src.clip = clip;
            src.Play();
        }

        public void PlayBgm(AudioClip clip)
        {
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.volume = _musicMuted ? 0f : _musicVolume;
            bgmSource.Play();
        }

        public void PlayMenuBgm() => PlayBgm(bgmMenu);
        public void PlayMatchBgm() => PlayBgm(bgmMatch);

        public void PlayClick() => PlaySfx(sfxClick);
        public void PlayPlace() => PlaySfx(sfxPlace);
        public void PlayReady() => PlaySfx(sfxReady);
        public void PlayReveal() => PlaySfx(sfxReveal);
        public void PlayThrow() => PlaySfx(sfxThrow);
        public void PlayHit() => PlaySfx(sfxHit);
        public void PlayDamage() => PlaySfx(sfxDamage != null ? sfxDamage : sfxHit);
        public void PlayCountdownTick() => PlaySfx(sfxCountdownTick);
        public void PlayCountdownLast() => PlaySfx(sfxCountdownLast != null ? sfxCountdownLast : sfxCountdownTick);
        public void PlayWin() => PlaySfx(sfxWin);
        public void PlayLose() => PlaySfx(sfxLose);

        public void PlayFlight()
        {
            // Missing SFX: logged as missing, no fake audio played
            if (sfxFlightMissing != null) PlaySfx(sfxFlightMissing);
        }

        // ──────────────────────────────────────────────────
        // Preferences Storage (wrapped in try/catch for WebGL)
        // ──────────────────────────────────────────────────

        private void LoadPrefs()
        {
            try
            {
                _musicVolume = PlayerPrefs.GetFloat(PrefKeyMusicVol, 0.50f);
                _sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVol, 0.80f);
                _musicMuted = PlayerPrefs.GetString(PrefKeyMusicMute, "0") == "1";
                _sfxMuted = PlayerPrefs.GetString(PrefKeySfxMute, "0") == "1";
            }
            catch
            {
                _musicVolume = 0.50f;
                _sfxVolume = 0.80f;
                _musicMuted = false;
                _sfxMuted = false;
            }
        }

        private void SavePrefs()
        {
            try
            {
                PlayerPrefs.SetFloat(PrefKeyMusicVol, _musicVolume);
                PlayerPrefs.SetFloat(PrefKeySfxVol, _sfxVolume);
                PlayerPrefs.SetString(PrefKeyMusicMute, _musicMuted ? "1" : "0");
                PlayerPrefs.SetString(PrefKeySfxMute, _sfxMuted ? "1" : "0");
                PlayerPrefs.Save();
            }
            catch
            {
                // WebGL sandbox protection
            }
        }
    }
}
