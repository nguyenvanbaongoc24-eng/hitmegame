using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HitMe.Core;
using HitMe.Gameplay;
using HitMe.UI;
using HitMe.Audio;

namespace HitMe.Core
{
    public class MatchStateMachine : MonoBehaviour
    {
        [Header("Components")]
        public UIManager uiManager;
        public AimController aimController;
        public ActorView playerActor;
        public ActorView botActor;
        public GameObject projectilePrefab;

        [Header("State")]
        public MatchState currentState = MatchState.Initializing;
        public int currentRound = 1;

        private BotBrain _botBrain = new BotBrain();
        private LockedAction _playerLockedAction;
        private LockedAction _botLockedAction;

        private bool _isPlayerLocked = false;
        private bool _isBotLocked = false;
        private float _phaseTimer = 0f;
        private int _lastTickSec = -1;

        private List<HitRecord> _currentBatchHits = new List<HitRecord>();

        // Read-only surface for presentation. It does not own or mutate gameplay state.
        public bool IsPlayerActionLocked => _isPlayerLocked;
        public bool IsBotActionLocked => _isBotLocked;
        public float PhaseSecondsRemaining => Mathf.Max(0f, _phaseTimer);

        private void Start()
        {
            if (uiManager != null)
            {
                uiManager.OnClickTrainWithBot += HandleStartTrainWithBot;
                uiManager.OnClickReady += HandlePlayerClickReady;
                uiManager.OnClickRematch += HandleRematch;
                uiManager.OnClickBackToHome += HandleBackToHome;
            }

            AudioManager.Instance?.PlayMenuBgm();
        }

        private void HandleStartTrainWithBot()
        {
            StartNewMatch();
        }

        private void HandleRematch()
        {
            StartNewMatch();
        }

        private void HandleBackToHome()
        {
            currentState = MatchState.Initializing;
            aimController.DisableInput();
            if (playerActor != null) playerActor.SetVisibility(false);
            if (botActor != null) botActor.SetVisibility(false);
            uiManager.ShowMainMenu();
            AudioManager.Instance?.PlayMenuBgm();
        }

        public void StartNewMatch()
        {
            currentRound = 1;
            if (playerActor != null)
            {
                playerActor.stats = new ActorStats();
                playerActor.ResetActor(new Vector2(0f, -3.5f), CombatRules.InitialHP);
            }
            if (botActor != null)
            {
                botActor.stats = new ActorStats();
                botActor.ResetActor(new Vector2(0f, 3.0f), CombatRules.InitialHP);
            }

            uiManager.ShowInGameHUD();
            uiManager.UpdateRound(currentRound);
            uiManager.UpdateHP(playerActor.currentHP, playerActor.maxHP, botActor.currentHP, botActor.maxHP);
            AudioManager.Instance?.PlayMatchBgm();

            StartCoroutine(PreparingRoundRoutine());
        }

        private IEnumerator PreparingRoundRoutine()
        {
            currentState = MatchState.PreparingRound;
            uiManager.UpdateRound(currentRound);
            uiManager.UpdatePhaseBanner("CHUẨN BỊ VÒNG ĐẤU");
            uiManager.SetReadyButtonState(false);

            _isPlayerLocked = false;
            _isBotLocked = false;
            _playerLockedAction = null;
            _botLockedAction = null;
            _currentBatchHits.Clear();

            // 1. Prepare Bot draft (GP-009: Bot does NOT read player secret position)
            Vector2 initialBotPos = botActor != null ? (Vector2)botActor.transform.position : new Vector2(0f, 3.0f);
            _botBrain.PrepareNewRound(initialBotPos);

            // 2. Secret opponent position (GP-002): Hide bot completely during Aiming phase
            if (botActor != null)
            {
                botActor.SetPosition(_botBrain.StartPosition);
                botActor.SetVisibility(false);
                botActor.ShowAim(false);
            }

            // 3. Setup player initial position and direction
            Vector2 currentPlayerPos = playerActor != null ? (Vector2)playerActor.transform.position : new Vector2(0f, -3.5f);
            if (playerActor != null)
            {
                playerActor.SetVisibility(true);
            }

            yield return new WaitForSeconds(0.4f);

            // 4. Start 5-second Window (GP-003): Player can move anywhere in arena + aim!
            currentState = MatchState.Aiming;
            uiManager.UpdatePhaseBanner("CHẠM ĐỂ ĐI — KÉO ĐỂ NGẮM (5s)");
            _phaseTimer = CombatRules.AimDuration;
            _lastTickSec = Mathf.CeilToInt(_phaseTimer);

            aimController.EnableInput(currentPlayerPos, Vector2.up);

            while (currentState == MatchState.Aiming)
            {
                float dt = Time.deltaTime;
                _phaseTimer -= dt;

                int curSec = Mathf.CeilToInt(Mathf.Max(0f, _phaseTimer));
                if (curSec != _lastTickSec && curSec > 0)
                {
                    _lastTickSec = curSec;
                    AudioManager.Instance?.PlayTick();
                }

                uiManager.UpdateTimer(_phaseTimer);

                // Bot think and move tick
                if (!_isBotLocked && _botBrain.UpdateTick(dt))
                {
                    _isBotLocked = true;
                    _botLockedAction = _botBrain.GetLockedAction(botActor.actorId);
                }

                // Check timeout auto-lock (GP-005)
                if (_phaseTimer <= 0f)
                {
                    AutoLockAll();
                    break;
                }

                // Check if both locked before timeout (GP-006)
                if (_isPlayerLocked && _isBotLocked)
                {
                    break;
                }

                yield return null;
            }

            // Transition to Revealing
            yield return StartCoroutine(RevealingRoutine());
        }

        private void HandlePlayerClickReady()
        {
            if (currentState != MatchState.Aiming || _isPlayerLocked) return;

            _isPlayerLocked = true;
            // Freeze movement atomically at current accepted position
            aimController.DisableInput();
            _playerLockedAction = new LockedAction(
                playerActor.actorId,
                playerActor.transform.position,
                aimController.CurrentDirection,
                "dep_cao_su"
            );
            uiManager.SetReadyButtonState(true);
            uiManager.UpdatePhaseBanner("ĐÃ KHÓA HÀNH ĐỘNG!");
        }

        private void AutoLockAll()
        {
            if (!_isPlayerLocked)
            {
                _isPlayerLocked = true;
                aimController.DisableInput();
                _playerLockedAction = new LockedAction(
                    playerActor.actorId,
                    playerActor.transform.position,
                    aimController.CurrentDirection,
                    "dep_cao_su"
                );
                uiManager.SetReadyButtonState(true);
            }

            if (!_isBotLocked)
            {
                _isBotLocked = true;
                _botLockedAction = _botBrain.GetLockedAction(botActor.actorId);
            }
        }

        private IEnumerator RevealingRoutine()
        {
            currentState = MatchState.Revealing;
            aimController.DisableInput();
            uiManager.UpdateTimer(0f);
            uiManager.UpdatePhaseBanner("REVEAL ĐỐI THỦ!");

            // REVEAL (GP-006, GP-013): Show all actors simultaneously at their locked positions!
            AudioManager.Instance?.PlayReveal();

            if (playerActor != null && _playerLockedAction != null)
            {
                playerActor.SetPosition(_playerLockedAction.position);
                playerActor.SetPose(CharacterPose.Aim);
            }

            if (botActor != null && _botLockedAction != null)
            {
                botActor.SetPosition(_botLockedAction.position);
                botActor.SetVisibility(true);
                botActor.SetPose(CharacterPose.Aim);
            }

            yield return new WaitForSeconds(0.8f);

            // Transition to Resolving
            yield return StartCoroutine(ResolvingRoutine());
        }

        private IEnumerator ResolvingRoutine()
        {
            currentState = MatchState.Resolving;
            uiManager.UpdatePhaseBanner("ĐỒNG THỜI NÉM VŨ KHÍ!");

            // Wind up animation
            if (playerActor != null) playerActor.SetPose(CharacterPose.ThrowWindup);
            if (botActor != null) botActor.SetPose(CharacterPose.ThrowWindup);

            yield return new WaitForSeconds(0.35f);

            // Release animation & throw
            if (playerActor != null)
            {
                playerActor.SetPose(CharacterPose.ThrowRelease);
                playerActor.stats.throwsCount++;
            }
            if (botActor != null)
            {
                botActor.SetPose(CharacterPose.ThrowRelease);
                botActor.stats.throwsCount++;
            }

            AudioManager.Instance?.PlayThrow();

            // Spawn projectiles simultaneously from exact locked positions
            SpawnProjectiles();

            // Wait for projectiles flight and impact (max ~1.0s)
            yield return new WaitForSeconds(1.0f);

            // BATCH DAMAGE RESOLVE (GP-008): Apply all hits gathered during this batch
            ApplyBatchDamage();

            uiManager.UpdateHP(playerActor.currentHP, playerActor.maxHP, botActor.currentHP, botActor.maxHP);

            yield return new WaitForSeconds(1.2f);

            // Transition to Round Summary
            EvaluateRoundSummary();
        }

        private void SpawnProjectiles()
        {
            if (projectilePrefab == null) return;

            ActorView[] allActors = new ActorView[] { playerActor, botActor };

            // Player projectile from locked position
            if (_playerLockedAction != null && playerActor != null)
            {
                GameObject projObj = Instantiate(projectilePrefab);
                ProjectileView proj = projObj.GetComponent<ProjectileView>();
                if (proj != null)
                {
                    proj.Initialize(
                        playerActor.actorId,
                        _playerLockedAction.position,
                        _playerLockedAction.direction,
                        allActors,
                        OnProjectileHit
                    );
                }
            }

            // Bot projectile from locked position
            if (_botLockedAction != null && botActor != null)
            {
                GameObject projObj = Instantiate(projectilePrefab);
                ProjectileView proj = projObj.GetComponent<ProjectileView>();
                if (proj != null)
                {
                    proj.Initialize(
                        botActor.actorId,
                        _botLockedAction.position,
                        _botLockedAction.direction,
                        allActors,
                        OnProjectileHit
                    );
                }
            }
        }

        private void OnProjectileHit(string shooterId, string targetId, Vector2 hitPos)
        {
            AudioManager.Instance?.PlayHit();

            HitRecord record = new HitRecord
            {
                shooterId = shooterId,
                targetId = targetId,
                hitPoint = hitPos,
                damage = CombatRules.DamagePerHit
            };
            _currentBatchHits.Add(record);

            if (shooterId == playerActor.actorId)
                playerActor.stats.hitsCount++;
            else if (shooterId == botActor.actorId)
                botActor.stats.hitsCount++;

            // Visual hit reaction on target
            if (targetId == playerActor.actorId) playerActor.SetPose(CharacterPose.Hit);
            if (targetId == botActor.actorId) botActor.SetPose(CharacterPose.Hit);
        }

        private void ApplyBatchDamage()
        {
            // Apply damage in batch (GP-008)
            foreach (var hit in _currentBatchHits)
            {
                if (hit.targetId == playerActor.actorId)
                {
                    playerActor.TakeDamage(hit.damage);
                }
                else if (hit.targetId == botActor.actorId)
                {
                    botActor.TakeDamage(hit.damage);
                }
            }
        }

        private void EvaluateRoundSummary()
        {
            currentState = MatchState.RoundSummary;

            bool playerDead = playerActor.currentHP <= 0;
            bool botDead = botActor.currentHP <= 0;

            if (playerDead && botDead)
            {
                // SIMULTANEOUS ALL-OUT (GP-008): DRAW (HÒA)!
                EndMatch(MatchResultType.Draw);
            }
            else if (playerDead)
            {
                EndMatch(MatchResultType.BotWin);
            }
            else if (botDead)
            {
                EndMatch(MatchResultType.PlayerWin);
            }
            else
            {
                // Both alive: Next round! Position is preserved so players can move again in round 2!
                currentRound++;
                if (playerActor != null) playerActor.SetPose(CharacterPose.Idle);
                if (botActor != null) botActor.SetPose(CharacterPose.Idle);
                StartCoroutine(PreparingRoundRoutine());
            }
        }

        private void EndMatch(MatchResultType result)
        {
            currentState = MatchState.MatchEnded;

            if (result == MatchResultType.PlayerWin)
            {
                playerActor.SetPose(CharacterPose.Victory);
                botActor.SetPose(CharacterPose.Defeat);
            }
            else if (result == MatchResultType.BotWin)
            {
                playerActor.SetPose(CharacterPose.Defeat);
                botActor.SetPose(CharacterPose.Victory);
            }
            else
            {
                // Draw
                playerActor.SetPose(CharacterPose.Defeat);
                botActor.SetPose(CharacterPose.Defeat);
            }

            uiManager.ShowResult(result, playerActor.stats, botActor.stats);
        }
    }
}
