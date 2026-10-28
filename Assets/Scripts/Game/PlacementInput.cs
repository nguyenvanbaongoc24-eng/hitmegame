using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlacementInput : MonoBehaviour
    {
        public Camera viewCamera;
        public MatchController controller;
        private int _activeFingerId = -1;

        private void Update()
        {
            if (controller == null || controller.State == null || controller.State.Phase != Phase.Placing)
            {
                _activeFingerId = -1;
                return;
            }

            if (Input.touchCount > 0)
            {
                Touch targetTouch = Input.GetTouch(0);
                bool found = false;
                if (_activeFingerId >= 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        var t = Input.GetTouch(i);
                        if (t.fingerId == _activeFingerId)
                        {
                            targetTouch = t;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        var t = Input.GetTouch(i);
                        if (t.phase == TouchPhase.Began)
                        {
                            targetTouch = t;
                            _activeFingerId = t.fingerId;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        targetTouch = Input.GetTouch(0);
                        _activeFingerId = targetTouch.fingerId;
                    }
                }

                controller.HandlePointer(new PointerSample(ToCore(World(targetTouch.position)), ToPointerPhase(targetTouch.phase)));
                if (targetTouch.phase == TouchPhase.Ended || targetTouch.phase == TouchPhase.Canceled)
                {
                    _activeFingerId = -1;
                }
                return;
            }

            _activeFingerId = -1;
            var mouse = World(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) controller.HandlePointer(new PointerSample(ToCore(mouse), PointerPhase.Began));
            else if (Input.GetMouseButton(0)) controller.HandlePointer(new PointerSample(ToCore(mouse), PointerPhase.Moved));
            else if (Input.GetMouseButtonUp(0)) controller.HandlePointer(new PointerSample(ToCore(mouse), PointerPhase.Ended));
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
