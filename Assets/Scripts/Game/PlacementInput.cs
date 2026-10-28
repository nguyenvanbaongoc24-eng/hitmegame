using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlacementInput : MonoBehaviour
    {
        public Camera viewCamera;
        public MatchController controller;
        private void Update()
        {
            if (controller == null || controller.State == null || controller.State.Phase != Phase.Placing) return;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                var point = World(touch.position);
                if (touch.phase == TouchPhase.Began) controller.BeginLocalGesture(point);
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) controller.MoveLocalGesture(point);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) controller.EndLocalGesture();
                return;
            }
            var mouse = World(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) controller.BeginLocalGesture(mouse);
            else if (Input.GetMouseButton(0)) controller.MoveLocalGesture(mouse);
            else if (Input.GetMouseButtonUp(0)) controller.EndLocalGesture();
        }
        private Vector2 World(Vector3 screen) { var point = viewCamera.ScreenToWorldPoint(screen); return new Vector2(point.x, point.y); }
    }
}
