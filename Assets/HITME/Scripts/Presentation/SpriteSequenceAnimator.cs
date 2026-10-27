using System;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.Presentation
{
    /// <summary>
    /// Plays transparent looping sprite sequences (e.g. Blender pre-rendered flag wave,
    /// tree canopy sway) with smooth frame pacing and zero memory allocations.
    /// Supports both SpriteRenderer (World) and Image (UI).
    /// </summary>
    public class SpriteSequenceAnimator : MonoBehaviour
    {
        [Header("Frames & Timing")]
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 14f;
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _playOnAwake = true;
        [SerializeField] private float _startPhaseOffset = 0f;

        [Header("Target Renderer")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _uiImage;

        private float _timer;
        private int _currentFrameIndex;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        public int FrameCount => _frames != null ? _frames.Length : 0;

        private void Awake()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_uiImage == null) _uiImage = GetComponent<Image>();

            if (_startPhaseOffset > 0f)
            {
                _timer = _startPhaseOffset;
            }

            if (_playOnAwake)
            {
                Play();
            }
        }

        public void Play()
        {
            _isPlaying = true;
            UpdateFrame(0);
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        private void Update()
        {
            if (!_isPlaying || _frames == null || _frames.Length == 0) return;

            float frameDuration = 1f / Mathf.Max(1f, _framesPerSecond);
            _timer += Time.deltaTime;

            if (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _frames.Length)
                {
                    if (_loop)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        _currentFrameIndex = _frames.Length - 1;
                        _isPlaying = false;
                    }
                }

                UpdateFrame(_currentFrameIndex);
            }
        }

        private void UpdateFrame(int index)
        {
            if (_frames == null || index < 0 || index >= _frames.Length) return;

            Sprite current = _frames[index];
            if (_spriteRenderer != null) _spriteRenderer.sprite = current;
            if (_uiImage != null) _uiImage.sprite = current;
        }
    }
}
