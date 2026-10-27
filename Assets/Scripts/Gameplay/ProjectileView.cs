using System;
using UnityEngine;
using HitMe.Core;

namespace HitMe.Gameplay
{
    public class ProjectileView : MonoBehaviour
    {
        public string shooterId;
        public Vector2 direction;
        public float speed = CombatRules.ProjectileSpeed;
        public float maxLifeTime = 1.2f;

        private float _elapsed = 0f;
        private bool _isConsumed = false;
        private Action<string, string, Vector2> _onHitCallback;
        private ActorView[] _targets;

        public void Initialize(string shooterId, Vector2 startPos, Vector2 dir, ActorView[] potentialTargets, Action<string, string, Vector2> onHit)
        {
            this.shooterId = shooterId;
            this.direction = dir.normalized;
            this._targets = potentialTargets;
            this._onHitCallback = onHit;
            this._isConsumed = false;
            this._elapsed = 0f;

            transform.position = new Vector3(startPos.x, startPos.y, -0.3f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (_isConsumed) return;

            float dt = Time.deltaTime;
            _elapsed += dt;

            Vector2 prevPos = transform.position;
            Vector2 step = direction * (speed * dt);
            Vector2 nextPos = prevPos + step;
            transform.position = new Vector3(nextPos.x, nextPos.y, -0.3f);

            // Continuous sweep collision check against all eligible targets (excluding shooter)
            if (_targets != null)
            {
                foreach (var target in _targets)
                {
                    if (target == null || target.actorId == shooterId || target.currentHP <= 0) continue;

                    Vector2 targetPos = target.transform.position;
                    // Distance from line segment (prevPos -> nextPos) to target center
                    float dist = DistancePointToSegment(targetPos, prevPos, nextPos);
                    if (dist <= CombatRules.ActorHitRadius)
                    {
                        // FIRST HIT confirmed! GP-007: projectile is consumed immediately
                        _isConsumed = true;
                        _onHitCallback?.Invoke(shooterId, target.actorId, nextPos);
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            if (_elapsed >= maxLifeTime)
            {
                _isConsumed = true;
                Destroy(gameObject);
            }
        }

        private float DistancePointToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lenSq);
            Vector2 proj = a + t * ab;
            return Vector2.Distance(point, proj);
        }
    }
}
