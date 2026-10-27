using System;
using HitMe.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HitMe.Gameplay
{
    public class AimController : MonoBehaviour
    {
        private const int NoPointer = int.MinValue;
        private const int MousePointer = -1;

        public Camera mainCamera;
        public ActorView playerActor;
        public Transform destinationMarker;
        public SpriteRenderer destinationMarkerSr;

        [Header("Gesture")]
        [SerializeField] private float dragThresholdReferencePixels = 12f;
        [SerializeField] private Vector2 referenceResolution = new Vector2(390f, 844f);
        [SerializeField] private bool enforceTopDownWorldCamera = true;

        private bool _isInputActive;
        private int _activePointerId = NoPointer;
        private Vector2 _gestureStartScreen;
        private Vector2 _gestureOriginWorld;
        private Vector2Int _lastScreenSize;

        public event Action<Vector2> AimChanged;
        public event Action<Vector2, Vector2> ValidActionReleased;

        public Vector2 CurrentPosition => playerActor != null ? (Vector2)playerActor.transform.position : Vector2.zero;
        public Vector2 CurrentDirection { get; private set; } = Vector2.up;
        public bool HasValidAim { get; private set; }
        public bool IsGestureActive => _activePointerId != NoPointer;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (destinationMarkerSr == null && destinationMarker != null)
                destinationMarkerSr = destinationMarker.GetComponent<SpriteRenderer>();

            RefreshCameraProjection();

            ShowDestinationMarker(false);
        }

        public void EnableInput(Vector2 initialPos, Vector2 initialDir)
        {
            ResetGesture(true);
            _isInputActive = true;
            CurrentDirection = CombatRules.NormalizeDirectionOrDefault(initialDir, Vector2.up);

            if (playerActor != null)
            {
                playerActor.SetPosition(initialPos);
                playerActor.SetPose(CharacterPose.Idle);
                playerActor.ShowAim(false);
            }
        }

        public void DisableInput()
        {
            _isInputActive = false;
            ResetGesture(false);

            if (playerActor != null)
            {
                playerActor.ShowAim(false);
                playerActor.SetPose(CharacterPose.Idle);
            }
        }

        private void Update()
        {
            if (!_isInputActive || playerActor == null || mainCamera == null) return;

            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize != _lastScreenSize) RefreshCameraProjection();

            if (Input.touchCount > 0)
            {
                HandleTouches();
                return;
            }

            HandleMouse();
        }

        private void HandleTouches()
        {
            if (_activePointerId == NoPointer)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        TryBeginPointer(touch.fingerId, touch.position);
                        break;
                    }
                }
            }

            if (_activePointerId == NoPointer) return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId != _activePointerId) continue;

                switch (touch.phase)
                {
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        UpdatePointer(touch.position);
                        break;
                    case TouchPhase.Ended:
                        ReleasePointer(touch.position);
                        break;
                    case TouchPhase.Canceled:
                        ResetGesture(true);
                        break;
                }
                return;
            }
        }

        private void HandleMouse()
        {
            Vector2 screenPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
                TryBeginPointer(MousePointer, screenPosition);

            if (_activePointerId != MousePointer) return;

            if (Input.GetMouseButton(0))
                UpdatePointer(screenPosition);

            if (Input.GetMouseButtonUp(0))
                ReleasePointer(screenPosition);
        }

        private void TryBeginPointer(int pointerId, Vector2 screenPosition)
        {
            if (_activePointerId != NoPointer || IsPointerOverUi(pointerId)) return;
            if (!TryScreenToArenaPlane(screenPosition, out Vector2 worldPosition)) return;
            if (!CombatRules.IsInsideArena(worldPosition)) return;

            _activePointerId = pointerId;
            _gestureStartScreen = screenPosition;
            _gestureOriginWorld = CombatRules.ClampToArena(worldPosition);
            HasValidAim = false;

            playerActor.SetPosition(_gestureOriginWorld);
            playerActor.SetPose(CharacterPose.Idle);
            playerActor.ShowAim(false);

            if (destinationMarker != null)
            {
                destinationMarker.position = new Vector3(_gestureOriginWorld.x, _gestureOriginWorld.y, -0.05f);
                ShowDestinationMarker(true);
            }
        }

        private void UpdatePointer(Vector2 screenPosition)
        {
            if (!TryScreenToArenaPlane(screenPosition, out Vector2 pointerWorld)) return;

            float screenDistance = Vector2.Distance(_gestureStartScreen, screenPosition);
            Vector2 aimVector = pointerWorld - _gestureOriginWorld;
            bool directionValid = CombatRules.TryNormalizeDirection(aimVector, out Vector2 normalized);
            HasValidAim = screenDistance >= GetScaledDragThreshold() && directionValid;

            if (!HasValidAim)
            {
                playerActor.ShowAim(false);
                playerActor.SetPose(CharacterPose.Idle);
                return;
            }

            CurrentDirection = normalized;
            playerActor.SetPose(CharacterPose.Aim);
            playerActor.UpdateAimTarget(_gestureOriginWorld, pointerWorld, CurrentDirection);
            playerActor.ShowAim(true);
            AimChanged?.Invoke(CurrentDirection);
        }

        private void ReleasePointer(Vector2 screenPosition)
        {
            UpdatePointer(screenPosition);
            bool shouldCommit = HasValidAim;
            Vector2 position = _gestureOriginWorld;
            Vector2 direction = CurrentDirection;
            ResetGesture(true);

            if (shouldCommit && _isInputActive)
                ValidActionReleased?.Invoke(position, direction);
        }

        private bool TryScreenToArenaPlane(Vector2 screenPosition, out Vector2 worldPosition)
        {
            return ArenaProjection.TryScreenPointToArena(mainCamera, screenPosition, out worldPosition);
        }

        private void RefreshCameraProjection()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (enforceTopDownWorldCamera) ArenaProjection.ConfigureTopDownCamera(mainCamera);
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        private bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null) return false;
            return pointerId == MousePointer
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        private float GetScaledDragThreshold()
        {
            float widthScale = Screen.width / Mathf.Max(1f, referenceResolution.x);
            float heightScale = Screen.height / Mathf.Max(1f, referenceResolution.y);
            return Mathf.Clamp(dragThresholdReferencePixels * Mathf.Min(widthScale, heightScale), 8f, 36f);
        }

        private void ResetGesture(bool keepInputActive)
        {
            _activePointerId = NoPointer;
            HasValidAim = false;
            ShowDestinationMarker(false);
            if (playerActor != null) playerActor.ShowAim(false);
            if (!keepInputActive) _isInputActive = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _activePointerId != NoPointer)
                ResetGesture(true);
        }

        private void OnDisable()
        {
            ResetGesture(false);
        }

        private void ShowDestinationMarker(bool show)
        {
            if (destinationMarkerSr != null) destinationMarkerSr.enabled = show;
        }
    }
}
