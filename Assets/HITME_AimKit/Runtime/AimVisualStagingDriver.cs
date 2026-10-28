using UnityEngine;
using UnityEngine.EventSystems;

namespace HitMe.AimKit
{
    /// <summary>
    /// Staging test driver for HIT ME Aim Kit V1.0.
    /// Demonstrates and validates all 8 aiming directions, weapon previews,
    /// short/long drag limits, and animation states (Aiming, Locked, Invalid).
    /// </summary>
    public class AimVisualStagingDriver : MonoBehaviour
    {
        [Header("Aim Kit Visual")]
        public HitMeAimVisual2D aimVisual;
        public Transform characterTransform;
        public Vector2 footOffset = new Vector2(0f, -0.45f);

        [Header("Weapon Sprites")]
        public Sprite[] weaponIcons;
        public int currentWeaponIndex = 0;

        [Header("Staging State")]
        public HitMeAimVisual2D.AimState currentState = HitMeAimVisual2D.AimState.Aiming;
        public bool isAimActive = true;
        public bool interactiveMouseAim = true;
        public float testLength = 3.2f;
        public Vector2 manualDirection = Vector2.up;

        public static readonly Vector2[] EightDirections = new Vector2[]
        {
            new Vector2(0f, 1f),                      // 0: Up
            new Vector2(0.7071f, 0.7071f).normalized, // 1: Up-Right
            new Vector2(1f, 0f),                      // 2: Right
            new Vector2(0.7071f, -0.7071f).normalized,// 3: Down-Right
            new Vector2(0f, -1f),                     // 4: Down
            new Vector2(-0.7071f, -0.7071f).normalized,// 5: Down-Left
            new Vector2(-1f, 0f),                     // 6: Left
            new Vector2(-0.7071f, 0.7071f).normalized // 7: Up-Left
        };

        public static readonly string[] DirectionNames = new string[]
        {
            "Up", "Up-Right", "Right", "Down-Right",
            "Down", "Down-Left", "Left", "Up-Left"
        };

        private Camera _mainCam;

        private void Awake()
        {
            _mainCam = Camera.main;
            if (characterTransform == null)
            {
                characterTransform = transform;
            }
        }

        private void Start()
        {
            if (isAimActive && aimVisual != null)
            {
                ApplyCurrentAim();
            }
        }

        private void Update()
        {
            if (aimVisual == null) return;

            // 1. Mouse interactive aiming in staging
            if (interactiveMouseAim && _mainCam != null)
            {
                if (Input.GetMouseButton(0))
                {
                    // Block input if over UI
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    Vector3 mouseScreen = Input.mousePosition;
                    Vector3 mouseWorld3 = _mainCam.ScreenToWorldPoint(mouseScreen);
                    Vector2 mouseWorld = new Vector2(mouseWorld3.x, mouseWorld3.y);

                    Vector2 origin = GetOrigin();
                    Vector2 delta = mouseWorld - origin;
                    float dist = delta.magnitude;

                    if (dist > 0.15f)
                    {
                        isAimActive = true;
                        manualDirection = delta.normalized;
                        testLength = dist;
                        currentState = HitMeAimVisual2D.AimState.Aiming;
                        ApplyCurrentAim();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    // Optional: hide on release to simulate match release
                    // In staging, keep visible if testing specific states
                }
            }

            // 2. Keyboard shortcuts for automated or manual QA inspection
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            // Keys 1 to 8: Direct 8 directions
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    SetDirection(i);
                }
            }

            // Key W: Cycle equipped weapon
            if (Input.GetKeyDown(KeyCode.W))
            {
                CycleWeapon();
            }

            // Key L: Toggle Locked state
            if (Input.GetKeyDown(KeyCode.L))
            {
                SetState(HitMeAimVisual2D.AimState.Locked);
            }

            // Key I: Toggle Invalid state
            if (Input.GetKeyDown(KeyCode.I))
            {
                SetState(HitMeAimVisual2D.AimState.Invalid);
            }

            // Key A: Toggle Aiming state
            if (Input.GetKeyDown(KeyCode.A))
            {
                SetState(HitMeAimVisual2D.AimState.Aiming);
            }

            // Key H / Space: Toggle Hide / Show
            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Space))
            {
                isAimActive = !isAimActive;
                if (!isAimActive) aimVisual.Hide();
                else ApplyCurrentAim();
            }

            // Key Tab: Toggle Short / Long distance
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                testLength = (testLength < 2.5f) ? 4.5f : 1.3f;
                ApplyCurrentAim();
            }
        }

        public void SetDirection(int directionIndex)
        {
            if (directionIndex >= 0 && directionIndex < EightDirections.Length)
            {
                manualDirection = EightDirections[directionIndex];
                isAimActive = true;
                ApplyCurrentAim();
            }
        }

        public void SetLength(float length)
        {
            testLength = length;
            ApplyCurrentAim();
        }

        public void SetWeapon(int index)
        {
            if (weaponIcons != null && weaponIcons.Length > 0)
            {
                currentWeaponIndex = Mathf.Clamp(index, 0, weaponIcons.Length - 1);
                ApplyCurrentAim();
            }
        }

        public void CycleWeapon()
        {
            if (weaponIcons != null && weaponIcons.Length > 0)
            {
                currentWeaponIndex = (currentWeaponIndex + 1) % weaponIcons.Length;
                ApplyCurrentAim();
            }
        }

        public void SetState(HitMeAimVisual2D.AimState state)
        {
            currentState = state;
            isAimActive = true;
            ApplyCurrentAim();
            aimVisual.SetState(state);
        }

        public void ApplyCurrentAim()
        {
            if (aimVisual == null) return;
            Vector2 origin = GetOrigin();
            Sprite weaponSprite = (weaponIcons != null && weaponIcons.Length > currentWeaponIndex && currentWeaponIndex >= 0)
                ? weaponIcons[currentWeaponIndex]
                : null;

            aimVisual.ShowAim(origin, manualDirection, testLength, weaponSprite);
            if (currentState != HitMeAimVisual2D.AimState.Aiming)
            {
                aimVisual.SetState(currentState);
            }
        }

        public Vector2 GetOrigin()
        {
            Vector2 pos = characterTransform != null ? (Vector2)characterTransform.position : Vector2.zero;
            return pos + footOffset;
        }
    }
}
