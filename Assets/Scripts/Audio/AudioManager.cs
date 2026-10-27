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
        public AudioClip sfxReady;
        public AudioClip sfxReveal;
        public AudioClip sfxThrow;
        public AudioClip sfxHit;
        public AudioClip sfxCountdownTick;
        public AudioClip sfxWin;
        public AudioClip sfxLose;
        public AudioClip bgmMenu;
        public AudioClip bgmMatch;

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

            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0.5f;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.8f;
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
        public void PlayReady() => PlaySfx(sfxReady);
        public void PlayReveal() => PlaySfx(sfxReveal);
        public void PlayThrow() => PlaySfx(sfxThrow);
        public void PlayHit() => PlaySfx(sfxHit);
        public void PlayTick() => PlaySfx(sfxCountdownTick);
        public void PlayWin() => PlaySfx(sfxWin);
        public void PlayLose() => PlaySfx(sfxLose);
    }
}
