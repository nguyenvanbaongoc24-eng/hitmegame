using UnityEngine;
using UnityEngine.EventSystems;
using HitMe.Core;

namespace HitMe.Gameplay
{
    public class AimController : MonoBehaviour
    {
        public Camera mainCamera;
        public ActorView playerActor;
        public Transform destinationMarker;
        public SpriteRenderer destinationMarkerSr;

        private bool _isInputActive = false;
        private bool _isAimDragging = false;
        private Vector2 _targetDestination;
        private bool _isMovingToDestination = false;

        public Vector2 CurrentPosition => playerActor != null ? (Vector2)playerActor.transform.position : Vector2.zero;
        public Vector2 CurrentDirection { get; private set; } = Vector2.up;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (destinationMarkerSr == null && destinationMarker != null)
            {
                destinationMarkerSr = destinationMarker.GetComponent<SpriteRenderer>();
            }
            ShowDestinationMarker(false);
        }

        public void EnableInput(Vector2 initialPos, Vector2 initialDir)
        {
            _isInputActive = true;
            _isAimDragging = false;
            _isMovingToDestination = false;
            CurrentDirection = initialDir.normalized;

            if (playerActor != null)
            {
                playerActor.SetPosition(initialPos);
                playerActor.SetPose(CharacterPose.Aim);
                Vector2 target = initialPos + CurrentDirection * 3.5f;
                playerActor.UpdateAimTarget(initialPos, target, CurrentDirection);
                playerActor.ShowAim(true);
            }
            ShowDestinationMarker(false);
        }

        public void DisableInput()
        {
            _isInputActive = false;
            _isAimDragging = false;
            _isMovingToDestination = false;

            ShowDestinationMarker(false);

            if (playerActor != null)
            {
                playerActor.ShowAim(false);
                playerActor.SetPose(CharacterPose.Idle);
            }
        }

        private void Update()
        {
            if (!_isInputActive || playerActor == null || mainCamera == null) return;

            float dt = Time.deltaTime;

            // 1. Get current mouse position clamped inside arena
            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clampedMouse = CombatRules.ClampToArena(mouseWorld);

            // 2. Continuously track aim across the entire arena map (di chuột -> thước ngắm đi theo)
            Vector2 originFeet = (Vector2)playerActor.transform.position + new Vector2(0f, -0.45f);
            Vector2 aimVector = clampedMouse - originFeet;
            if (aimVector.sqrMagnitude > 0.05f)
            {
                CurrentDirection = aimVector.normalized;
            }

            // Always update aim visual to follow mouse cursor
            playerActor.UpdateAimTarget(playerActor.transform.position, clampedMouse, CurrentDirection);

            // Face direction of aiming
            if (!_isMovingToDestination && playerActor.characterRenderer != null)
            {
                if (CurrentDirection.x < -0.1f) playerActor.characterRenderer.flipX = true;
                else if (CurrentDirection.x > 0.1f) playerActor.characterRenderer.flipX = false;
            }

            // 3. Handle click/tap to place character anywhere on the arena
            HandleInput(clampedMouse);

            // 4. Smooth walking towards destination
            if (_isMovingToDestination)
            {
                Vector2 currentPos = playerActor.transform.position;
                float dist = Vector2.Distance(currentPos, _targetDestination);

                if (dist > 0.08f)
                {
                    playerActor.MoveStep(_targetDestination, 8.5f, dt);
                }
                else
                {
                    _isMovingToDestination = false;
                    playerActor.SetPosition(_targetDestination);
                    playerActor.SetPose(CharacterPose.Aim);
                    ShowDestinationMarker(false);
                }
            }
        }

        private void HandleInput(Vector2 clampedMouse)
        {
            // Ignore clicks on UI buttons
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // When clicking or tapping anywhere on the arena -> move character to that location!
            if (Input.GetMouseButtonDown(0))
            {
                SetMoveDestination(clampedMouse);
            }
        }

        private void SetMoveDestination(Vector2 worldTarget)
        {
            _targetDestination = CombatRules.ClampToArena(worldTarget);
            _isMovingToDestination = true;

            HitMe.Audio.AudioManager.Instance?.PlayPlace();

            if (destinationMarker != null)
            {
                destinationMarker.position = new Vector3(_targetDestination.x, _targetDestination.y, -0.05f);
                ShowDestinationMarker(true);
            }
        }

        private void ShowDestinationMarker(bool show)
        {
            if (destinationMarkerSr != null) destinationMarkerSr.enabled = show;
        }
    }
}
