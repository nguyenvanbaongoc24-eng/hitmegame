using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlacementInput : MonoBehaviour
    {
        public Camera viewCamera;
        public MatchController controller;
        private bool _dragging;
        private Vector2 _pressPoint;
        private void Update()
        {
            if (controller == null || controller.State == null || controller.State.Phase != Phase.Placing) return;
            if (Input.GetMouseButtonDown(0)) { _pressPoint = World(Input.mousePosition); _dragging = Vector2.Distance(_pressPoint, controller.LocalPosition) < .7f; if (!_dragging) controller.SetLocalPlacement(_pressPoint); }
            if (Input.GetMouseButton(0) && _dragging) controller.SetLocalDirection(World(Input.mousePosition) - controller.LocalPosition);
        }
        private Vector2 World(Vector3 screen) { var point = viewCamera.ScreenToWorldPoint(screen); return new Vector2(point.x, point.y); }
    }
}
