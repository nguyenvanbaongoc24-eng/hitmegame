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

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    var phase = ToPointerPhase(touch.phase);
                    if (!_touchGate.TryAccept(touch.fingerId, phase)) continue;
                    Forward("touch#" + touch.fingerId, touch.position, phase);
                    break;
                }
                return;
            }

            _touchGate.Reset();
            var mouse = World(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) Forward("mouse", Input.mousePosition, PointerPhase.Began);
            else if (Input.GetMouseButton(0)) Forward("mouse", Input.mousePosition, PointerPhase.Moved);
            else if (Input.GetMouseButtonUp(0)) Forward("mouse", Input.mousePosition, PointerPhase.Ended);
        }
        private void Forward(string source, Vector2 raw, PointerPhase phase)
        {
            var world = World(raw); LastInputSource = source; LastPointerPhase = phase; LastRawScreenPosition = raw; LastWorldPosition = world;
            controller.HandlePointer(new PointerSample(ToCore(world), phase));
        }
        private Vector2 World(Vector3 screen) { var point = viewCamera.ScreenToWorldPoint(screen); return new Vector2(point.x, point.y); }
        private static Vec2 ToCore(Vector2 point) { return new Vec2(point.x, point.y); }
        private static PointerPhase ToPointerPhase(TouchPhase phase)
        {
            if (phase == TouchPhase.Began) return PointerPhase.Began;
            if (phase == TouchPhase.Ended) return PointerPhase.Ended;
            if (phase == TouchPhase.Canceled) return PointerPhase.Canceled;
            return PointerPhase.Moved;
        }
    }
}
