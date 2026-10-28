using UnityEngine;

namespace HitMe.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

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

        public void PlayBgm(AudioClip clip)
        {
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlayMenuBgm() => PlayBgm(bgmMenu);
        public void PlayMatchBgm() => PlayBgm(bgmMatch);

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void PlayClick() => PlaySfx(sfxClick);
        public void PlayPlace() => PlaySfx(sfxPlace);
        public void PlayReady() => PlaySfx(sfxReady);
        public void PlayReveal() => PlaySfx(sfxReveal);
        public void PlayThrow() => PlaySfx(sfxThrow);
        public void PlayHit() => PlaySfx(sfxHit);
        public void PlayDeath() => PlaySfx(sfxDeath);
        public void PlayTick() => PlaySfx(sfxCountdownTick);
        public void PlayCountdownLast() => PlaySfx(sfxCountdownLast != null ? sfxCountdownLast : sfxCountdownTick);
        public void PlayWin() => PlaySfx(sfxWin);
        public void PlayLose() => PlaySfx(sfxLose);

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
