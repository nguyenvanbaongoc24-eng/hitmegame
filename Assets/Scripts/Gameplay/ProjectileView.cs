using System;
using HitMe.Core;
using UnityEngine;

namespace HitMe.Gameplay
{
    public class ProjectileView : MonoBehaviour
    {
        public string shooterId;
        public Vector2 direction;
        public float speed = CombatRules.ProjectileSpeed;
        public float maxLifeTime = 1.2f;

        private float _elapsed;
        private bool _isConsumed;
        private bool _completionNotified;
        private Action<string, string, Vector2> _onHitCallback;
        private Action<ProjectileView> _onCompletedCallback;
        private ActorView[] _targets;

        public void Initialize(
            string ownerId,
            Vector2 startPos,
            Vector2 dir,
            ActorView[] potentialTargets,
            Action<string, string, Vector2> onHit,
            Action<ProjectileView> onCompleted)
        {
            shooterId = ownerId;
            direction = CombatRules.NormalizeDirectionOrDefault(dir, Vector2.up);
            _targets = potentialTargets;
            _onHitCallback = onHit;
            _onCompletedCallback = onCompleted;
            _isConsumed = false;
            _completionNotified = false;
            _elapsed = 0f;

            transform.position = new Vector3(startPos.x, startPos.y, -0.3f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (_isConsumed) return;

            float dt = Time.deltaTime;
            _elapsed += dt;
            Vector2 previousPosition = transform.position;
            Vector2 nextPosition = previousPosition + direction * (speed * dt);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, -0.3f);

            if (_targets != null)
            {
                foreach (ActorView target in _targets)
                {
                    if (target == null || target.actorId == shooterId || target.currentHP <= 0) continue;
                    float combinedRadius = CombatRules.ActorHitRadius + CombatRules.ProjectileRadius;
                    if (!SegmentHitsCircle(previousPosition, nextPosition, target.transform.position, combinedRadius)) continue;

                    _onHitCallback?.Invoke(shooterId, target.actorId, nextPosition);
                    Complete();
                    return;
                }
            }

            if (_elapsed >= maxLifeTime) Complete();
        }

        public void Cancel()
        {
            if (_isConsumed) return;
            _isConsumed = true;
            _onCompletedCallback = null;
            Destroy(gameObject);
        }

        private void Complete()
        {
            if (_isConsumed) return;
            _isConsumed = true;
            NotifyCompleted();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            NotifyCompleted();
        }

        private void NotifyCompleted()
        {
            if (_completionNotified) return;
            _completionNotified = true;
            _onCompletedCallback?.Invoke(this);
        }

        public static bool SegmentHitsCircle(Vector2 start, Vector2 end, Vector2 center, float radius)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared < 0.0001f) return Vector2.Distance(center, start) <= radius;

            float t = Mathf.Clamp01(Vector2.Dot(center - start, segment) / lengthSquared);
            Vector2 closest = start + t * segment;
            return Vector2.Distance(center, closest) <= radius;
        }
    }
}
