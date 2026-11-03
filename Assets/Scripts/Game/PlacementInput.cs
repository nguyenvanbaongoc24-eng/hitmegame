using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlacementInput : MonoBehaviour
    {
        public Camera viewCamera;
        public MatchController controller;
        private readonly ActivePointerGate _touchGate = new ActivePointerGate();
        public string LastInputSource { get; private set; } = "none";
        public PointerPhase LastPointerPhase { get; private set; }
        public Vector2 LastRawScreenPosition { get; private set; }
        public Vector2 LastWorldPosition { get; private set; }

        private void Update()
        {
            if (controller == null || controller.State == null || controller.State.Phase != Phase.Placing)
            {
                _touchGate.Reset();
                return;
            }

            // 1. Process real touch events first (mobile / WebKit / Android)
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0); // Primary finger only per rule G6
                var phase = ToPointerPhase(touch.phase);

                if (phase == PointerPhase.Began)
                {
                    if (IsPointerOverHud(touch.position, touch.fingerId)) return;
                    _touchGate.Reset();
                }

                if (_touchGate.TryAccept(touch.fingerId, phase))
                {
                    Forward("touch#" + touch.fingerId, touch.position, phase, touch.fingerId);
                    return;
                }
            }

            // 2. Desktop mouse / pointer fallback
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverHud(Input.mousePosition, 0)) return;
                _touchGate.Reset();
                if (_touchGate.TryAccept(0, PointerPhase.Began))
                {
                    Forward("mouse", Input.mousePosition, PointerPhase.Began, 0);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (_touchGate.TryAccept(0, PointerPhase.Moved))
                {
                    Forward("mouse", Input.mousePosition, PointerPhase.Moved, 0);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_touchGate.TryAccept(0, PointerPhase.Ended))
                {
                    Forward("mouse", Input.mousePosition, PointerPhase.Ended, 0);
                }
            }
        }

        private bool IsPointerOverHud(Vector2 screenPos, int pointerId)
        {
            float screenH = Screen.height;
            // Immediate top bar check (Timer, Phase title, Settings gear)
            if (screenPos.y > screenH * 0.88f) return true;
            // Immediate bottom bar check (Weapon pill, Chat, Navigation)
            if (screenPos.y < screenH * 0.10f) return true;

            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null)
            {
                var eventData = new UnityEngine.EventSystems.PointerEventData(es) { position = screenPos };
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                es.RaycastAll(eventData, results);
                foreach (var r in results)
                {
                    if (r.gameObject == null) continue;
                    string gname = r.gameObject.name;
                    // Ignore root canvases and full-screen layout containers
                    if (gname == "ProductionCanvas" || gname == "SafeAreaRoot" || gname == "GameplayHudPanel" || gname == "HUD")
                    {
                        continue;
                    }
                    // Block placement only if over an actual interactive UI Button or Selectable control
                    if (r.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null ||
                        r.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Forward(string source, Vector2 raw, PointerPhase phase, int pointerId)
        {
            if (viewCamera == null) viewCamera = Camera.main;
            var world = World(raw);
            LastInputSource = source;
            LastPointerPhase = phase;
            LastRawScreenPosition = raw;
            LastWorldPosition = world;
            controller.HandlePointer(pointerId, raw, world, phase);
        }

        private Vector2 World(Vector3 screen)
        {
            if (viewCamera == null) return Vector2.zero;
            var point = viewCamera.ScreenToWorldPoint(screen);
            return new Vector2(point.x, point.y);
        }

        private static PointerPhase ToPointerPhase(TouchPhase phase)
        {
            if (phase == TouchPhase.Began) return PointerPhase.Began;
            if (phase == TouchPhase.Ended) return PointerPhase.Ended;
            if (phase == TouchPhase.Canceled) return PointerPhase.Canceled;
            return PointerPhase.Moved;
        }
    }
}
