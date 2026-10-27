using UnityEngine;
using HitMe.Core;

namespace HitMe.Gameplay
{
    public class ActorView : MonoBehaviour
    {
        [Header("Actor Info")]
        public string actorId = "player";
        public string actorName = "Nam Làng";
        public ActorType actorType = ActorType.Player;
        public int currentHP = CombatRules.InitialHP;
        public int maxHP = CombatRules.InitialHP;
        public ActorStats stats = new ActorStats();

        [Header("Renderers")]
        public SpriteRenderer characterRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer ringRenderer;
        public LineRenderer aimLine;
        public LineRenderer aimBeam;
        public Transform weaponTip;
        public SpriteRenderer weaponTipRenderer;
        public SpriteRenderer weaponGlowRenderer;

        [Header("Character Sprites")]
        public Sprite spriteIdle;
        public Sprite spriteMove;
        public Sprite spriteAim;
        public Sprite spriteThrowWindup;
        public Sprite spriteThrowRelease;
        public Sprite spriteHit;
        public Sprite spriteVictory;
        public Sprite spriteDefeat;

        [Header("Visual Config")]
        public Color ringColor = new Color(0.1f, 0.9f, 1f, 0.8f);

        private CharacterPose _currentPose = CharacterPose.Idle;

        private void Awake()
        {
            if (ringRenderer != null)
            {
                ringRenderer.color = ringColor;
            }
        }

        public void ResetActor(Vector2 initialPos, int hp = CombatRules.InitialHP)
        {
            currentHP = hp;
            maxHP = hp;
            transform.position = new Vector3(initialPos.x, initialPos.y, 0f);
            SetPose(CharacterPose.Idle);
            ShowAim(false);
            SetVisibility(true);
            if (characterRenderer != null) characterRenderer.flipX = false;
        }

        public void SetPose(CharacterPose pose)
        {
            _currentPose = pose;
            if (characterRenderer == null) return;

            Sprite targetSprite = spriteIdle;
            switch (pose)
            {
                case CharacterPose.Idle:
                    targetSprite = spriteIdle;
                    break;
                case CharacterPose.Move:
                    targetSprite = spriteMove != null ? spriteMove : spriteIdle;
                    break;
                case CharacterPose.Aim:
                    targetSprite = spriteAim != null ? spriteAim : spriteIdle;
                    break;
                case CharacterPose.ThrowWindup:
                    targetSprite = spriteThrowWindup != null ? spriteThrowWindup : spriteIdle;
                    break;
                case CharacterPose.ThrowRelease:
                    targetSprite = spriteThrowRelease != null ? spriteThrowRelease : spriteIdle;
                    break;
                case CharacterPose.Hit:
                    targetSprite = spriteHit != null ? spriteHit : spriteIdle;
                    break;
                case CharacterPose.Victory:
                    targetSprite = spriteVictory != null ? spriteVictory : spriteIdle;
                    break;
                case CharacterPose.Defeat:
                    targetSprite = spriteDefeat != null ? spriteDefeat : spriteIdle;
                    break;
            }

            if (targetSprite != null)
            {
                characterRenderer.sprite = targetSprite;
            }
        }

        public void MoveStep(Vector2 targetPos, float speed, float dt)
        {
            Vector2 currentPos = transform.position;
            Vector2 delta = targetPos - currentPos;
            float dist = delta.magnitude;

            if (dist > 0.05f)
            {
                Vector2 dir = delta / dist;
                Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * dt);
                newPos = CombatRules.ClampToArena(newPos);
                transform.position = new Vector3(newPos.x, newPos.y, 0f);

                SetPose(CharacterPose.Move);

                // Flip sprite based on travel direction
                if (characterRenderer != null)
                {
                    if (dir.x < -0.1f) characterRenderer.flipX = true;
                    else if (dir.x > 0.1f) characterRenderer.flipX = false;
                }
            }
            else
            {
                transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
                SetPose(CharacterPose.Aim);
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (characterRenderer != null) characterRenderer.enabled = isVisible;
            if (shadowRenderer != null) shadowRenderer.enabled = isVisible;
            if (ringRenderer != null) ringRenderer.enabled = isVisible;
            if (!isVisible) ShowAim(false);
        }

        public void SetPosition(Vector2 pos)
        {
            Vector2 clamped = CombatRules.ClampToArena(pos);
            transform.position = new Vector3(clamped.x, clamped.y, 0f);
        }

        public void ShowAim(bool show)
        {
            if (aimLine != null) aimLine.enabled = show;
            if (aimBeam != null) aimBeam.enabled = show;
            if (weaponTip != null) weaponTip.gameObject.SetActive(show);
        }

        public void UpdateAimLine(Vector2 origin, Vector2 direction, float length = 3.5f)
        {
            Vector2 target = origin + direction.normalized * length;
            UpdateAimTarget(origin, target, direction);
        }

        public void UpdateAimTarget(Vector2 origin, Vector2 targetWorldPos, Vector2 direction)
        {
            // Actor transform is the authoritative gameplay/feet position. Presentation
            // pivots belong on renderer children and must not offset the throw vector.
            Vector2 footPos = origin;
            float dist = Vector2.Distance(footPos, targetWorldPos);
            dist = Mathf.Max(dist, 1.0f);
            Vector2 endPoint = footPos + direction.normalized * dist;

            // 1. Widening Beam / Cone
            if (aimBeam != null)
            {
                aimBeam.enabled = true;
                aimBeam.positionCount = 2;
                aimBeam.startWidth = 0.12f;
                aimBeam.endWidth = 0.65f;
                aimBeam.SetPosition(0, new Vector3(footPos.x, footPos.y, -0.04f));
                aimBeam.SetPosition(1, new Vector3(endPoint.x, endPoint.y, -0.04f));
            }

            // 2. Trajectory Dotted Line
            if (aimLine != null)
            {
                aimLine.enabled = true;
                aimLine.positionCount = 2;
                aimLine.startWidth = 0.05f;
                aimLine.endWidth = 0.05f;
                aimLine.SetPosition(0, new Vector3(footPos.x, footPos.y, -0.07f));
                aimLine.SetPosition(1, new Vector3(endPoint.x, endPoint.y, -0.07f));
            }

            // 3. Reticle (Slipper + Glow) at target point
            if (weaponTip != null)
            {
                weaponTip.gameObject.SetActive(true);
                weaponTip.position = new Vector3(endPoint.x, endPoint.y, -0.1f);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                weaponTip.rotation = Quaternion.Euler(0f, 0f, angle);
                if (weaponTipRenderer != null) weaponTipRenderer.enabled = true;
                if (weaponGlowRenderer != null) weaponGlowRenderer.enabled = true;
            }
        }

        public void TakeDamage(int damage)
        {
            currentHP = Mathf.Max(0, currentHP - damage);
            if (currentHP == 0)
            {
                SetPose(CharacterPose.Defeat);
            }
            else
            {
                SetPose(CharacterPose.Hit);
            }
        }
    }
}
