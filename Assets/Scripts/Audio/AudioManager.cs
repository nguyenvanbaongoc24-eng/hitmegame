using UnityEngine;

namespace HitMe.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public static System.Action<string, float, float> OnSfxRequested;
        public static System.Action<string, string> OnMusicRequested;

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;

        [Header("Clips")]
        public AudioClip sfxClick;
        public AudioClip sfxPlace;
        public AudioClip sfxReady;
        public AudioClip sfxReveal;
        public AudioClip sfxThrow;
        public AudioClip sfxFly; // Silent placeholder for missing slipper flight SFX
        public AudioClip sfxHit;
        public AudioClip sfxDeath;
        public AudioClip sfxCountdownTick;
        public AudioClip sfxCountdownLast;
        public AudioClip sfxWin;
        public AudioClip sfxLose;
        public AudioClip sfxWallThud;
        public AudioClip sfxError;
        public AudioClip sfxCoin;
        public AudioClip sfxPurchase;
        public AudioClip sfxChatPop;
        public AudioClip bgmMenu;
        public AudioClip bgmMatch;

        private const string PrefsMusicVol = "HitMe_MusicVolume";
        private const string PrefsSfxVol = "HitMe_SFXVolume";

        private float _musicVolume = 0.5f;
        private float _sfxVolume = 0.8f;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (bgmSource != null) bgmSource.volume = _musicVolume;
                PlayerPrefs.SetFloat(PrefsMusicVol, _musicVolume);
                PlayerPrefs.Save();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (sfxSource != null) sfxSource.volume = _sfxVolume;
                PlayerPrefs.SetFloat(PrefsSfxVol, _sfxVolume);
                PlayerPrefs.Save();
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

            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            _musicVolume = PlayerPrefs.GetFloat(PrefsMusicVol, 0.5f);
            _sfxVolume = PlayerPrefs.GetFloat(PrefsSfxVol, 0.8f);

            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = _musicVolume;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = _sfxVolume;
        }

        public static bool ShouldPlayUnityAudio()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // The embedded web UI owns audio only when it is actually present.
            // Standalone Unity WebGL has no hitmeUi.onUnity receiver.
            return ShouldPlayUnityAudioForMode(HitMe.UI.HitMeWebBridge.IsWebUiMode() == 1);
#else
            return ShouldPlayUnityAudioForMode(false);
#endif
        }

        public static bool ShouldPlayUnityAudioForMode(bool isWebUiMode)
        {
            return !isWebUiMode;
        }

        public void PlayBgm(AudioClip clip, string musicId = null)
        {
            if (!string.IsNullOrEmpty(musicId))
            {
                OnMusicRequested?.Invoke(musicId, "play");
            }
            if (!ShouldPlayUnityAudio()) return;
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            try
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AudioManager] PlayBgm safely ignored: {ex.Message}");
            }
        }

        public void PlayMenuBgm()
        {
            OnMusicRequested?.Invoke("bgm_menu", "play");
            if (ShouldPlayUnityAudio()) TransitionBgm(bgmMenu);
        }

        public void PlayMatchBgm()
        {
            OnMusicRequested?.Invoke("bgm_match", "play");
            if (ShouldPlayUnityAudio()) TransitionBgm(bgmMatch);
        }

        private Coroutine _fadeRoutine;

        public void TransitionBgm(AudioClip newClip, float fadeDuration = 0.4f)
        {
            if (newClip == null || bgmSource == null) return;
            if (bgmSource.clip == newClip && bgmSource.isPlaying) return;

            if (!isActiveAndEnabled)
            {
                PlayBgm(newClip);
                return;
            }

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, fadeDuration));
        }

        private System.Collections.IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
        {
            float startVol = bgmSource.isPlaying ? bgmSource.volume : 0f;
            float halfDuration = duration * 0.5f;

            if (bgmSource.isPlaying && halfDuration > 0.01f)
            {
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                    yield return null;
                }
            }

            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            if (halfDuration > 0.01f)
            {
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(0f, _musicVolume, t / halfDuration);
                    yield return null;
                }
            }
            bgmSource.volume = _musicVolume;
            _fadeRoutine = null;
        }

        public void DuckMusic(float factor = 0.3f)
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.volume = _musicVolume * Mathf.Clamp01(factor);
            }
        }

        public void UnduckMusic()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.volume = _musicVolume;
            }
        }

        public void PlaySfx(AudioClip clip, string sfxId = null, float vol = 1f, float pan = 0f)
        {
            if (!string.IsNullOrEmpty(sfxId))
            {
                OnSfxRequested?.Invoke(sfxId, vol, pan);
            }
            if (!ShouldPlayUnityAudio()) return;
            if (clip != null && sfxSource != null)
            {
                try
                {
                    sfxSource.PlayOneShot(clip, vol);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AudioManager] PlaySfx safely ignored: {ex.Message}");
                }
            }
        }

        public void PlayClick() => PlaySfx(sfxClick, "ui_tap");
        public void PlayPlace() => PlaySfx(sfxPlace, "place_pop");
        public void PlayReady() => PlaySfx(sfxReady, "ready_lock");
        public void PlayReveal() => PlaySfx(sfxReveal, "reveal");
        public void PlayThrow(string weaponId = "dep_to_ong")
        {
            string sfxId = "throw_dep";
            if (!string.IsNullOrEmpty(weaponId))
            {
                if (weaponId.IndexOf("chao", System.StringComparison.OrdinalIgnoreCase) >= 0) sfxId = "throw_chao";
                else if (weaponId.IndexOf("vot", System.StringComparison.OrdinalIgnoreCase) >= 0) sfxId = "throw_vot";
            }
            PlaySfx(sfxThrow, sfxId);
        }
        public void PlayHit(string weaponId = "dep_to_ong")
        {
            string sfxId = "hit_dep";
            if (!string.IsNullOrEmpty(weaponId))
            {
                if (weaponId.IndexOf("chao", System.StringComparison.OrdinalIgnoreCase) >= 0) sfxId = "hit_chao";
                else if (weaponId.IndexOf("vot", System.StringComparison.OrdinalIgnoreCase) >= 0) sfxId = "hit_vot";
            }
            PlaySfx(sfxHit, sfxId);
        }
        public void PlayDeath() => PlaySfx(sfxDeath, "ko");
        public void PlayTick() => PlaySfx(sfxCountdownTick, "timer_tick");
        public void PlayCountdownTick() => PlayTick();
        public void PlayCountdownLast() => PlaySfx(sfxCountdownLast != null ? sfxCountdownLast : sfxCountdownTick, "timer_end");
        public void PlayWin()
        {
            OnMusicRequested?.Invoke("jingle_win", "play");
            PlaySfx(sfxWin, "win");
        }
        public void PlayLose()
        {
            OnMusicRequested?.Invoke("jingle_lose", "play");
            PlaySfx(sfxLose, "lose");
        }
        public void PlayRoundStart() => PlaySfx(sfxReveal, "round_start");
        public void PlayRoundResult() => PlaySfx(sfxReady, "round_result");
        public void PlayAimTick() => PlaySfx(sfxClick, "aim_tick", 0.35f);
        public void PlayWallThud() => PlaySfx(sfxWallThud, "ui_back");
        public void PlayError() => PlaySfx(sfxError, "ui_error");
        public void PlayCoin() => PlaySfx(sfxCoin, "ui_confirm");
        public void PlayPurchase() => PlaySfx(sfxPurchase, "ui_confirm");
        public void PlayChatPop() => PlaySfx(sfxChatPop, "emo_pop");

        /// <summary>
        /// Slipper flight SFX. Asset QA confirmed missing "dép bay": uses silent placeholder.
        /// Does not emit sound; preserved for Phase 3 when audio asset is delivered.
        /// </summary>
        public void PlayFly()
        {
            if (sfxFly != null)
            {
                PlaySfx(sfxFly);
            }
        }
    }
}
