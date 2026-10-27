using HitMe.Core;
using UnityEngine;

namespace HitMe.Gameplay
{
    /// <summary>
    /// The gameplay arena is a single world-space XY plane at Z=0. UI/Canvas
    /// coordinates never enter movement, aim or projectile calculations.
    /// </summary>
    public static class ArenaProjection
    {
        public const float GameplayPlaneZ = 0f;
        public const float DefaultCameraDistance = 10f;
        public const float FramingMargin = 0.55f;

        private static readonly Plane GameplayPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, GameplayPlaneZ));

        public static bool TryScreenPointToArena(Camera camera, Vector2 screenPoint, out Vector2 arenaPoint)
        {
            if (camera == null)
            {
                arenaPoint = default;
                return false;
            }

            Ray ray = camera.ScreenPointToRay(screenPoint);
            if (!GameplayPlane.Raycast(ray, out float distance) || distance < 0f)
            {
                arenaPoint = default;
                return false;
            }

            Vector3 hit = ray.GetPoint(distance);
            arenaPoint = new Vector2(hit.x, hit.y);
            return true;
        }

        public static void ConfigureTopDownCamera(Camera camera)
        {
            if (camera == null) return;

            float centerX = (CombatRules.ArenaMinX + CombatRules.ArenaMaxX) * 0.5f;
            float centerY = (CombatRules.ArenaMinY + CombatRules.ArenaMaxY) * 0.5f;
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.transform.SetPositionAndRotation(
                new Vector3(centerX, centerY, GameplayPlaneZ - DefaultCameraDistance),
                Quaternion.identity);
            camera.orthographicSize = CalculateOrthographicSize(camera.aspect);
        }

        public static float CalculateOrthographicSize(float aspect)
        {
            float safeAspect = Mathf.Max(0.01f, aspect);
            float halfHeight = (CombatRules.ArenaMaxY - CombatRules.ArenaMinY) * 0.5f + FramingMargin;
            float halfWidth = (CombatRules.ArenaMaxX - CombatRules.ArenaMinX) * 0.5f + FramingMargin;
            return Mathf.Max(halfHeight, halfWidth / safeAspect);
        }
    }
}
