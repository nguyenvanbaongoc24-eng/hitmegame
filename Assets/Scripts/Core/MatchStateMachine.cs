using System;
using System.Collections;
using System.Collections.Generic;
using HitMe.Audio;
using HitMe.Gameplay;
using HitMe.UI;
using UnityEngine;

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
        [SerializeField] private int botSeed = 20261027;

        public event Action<int> PlanningStarted;
        public event Action<Vector2> AimChanged;
        public event Action<LockedAction> CommandLocked;
        public event Action RevealStarted;
        public event Action<string, LockedAction> ThrowReleased;
        public event Action<HitRecord> DamageApplied;
        public event Action<MatchResultType> MatchEnded;

        private readonly BotBrain _botBrain = new BotBrain();
        private readonly List<HitRecord> _currentBatchHits = new List<HitRecord>();
        private readonly List<ProjectileView> _activeProjectiles = new List<ProjectileView>();

        private LockedAction _playerLockedAction;
        private LockedAction _botLockedAction;
        private bool _isPlayerLocked;
        private bool _isBotLocked;
        private float _phaseTimer;
        private int _lastTickSec = -1;
        private int _pendingProjectiles;

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

            if (aimController != null)
            {
                aimController.ValidActionReleased += HandleValidActionReleased;
                aimController.AimChanged += HandleAimChanged;
            }

            AudioManager.Instance?.PlayMenuBgm();
        }

        private void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.OnClickTrainWithBot -= HandleStartTrainWithBot;
                uiManager.OnClickReady -= HandlePlayerClickReady;
                uiManager.OnClickRematch -= HandleRematch;
                uiManager.OnClickBackToHome -= HandleBackToHome;
            }

            if (aimController != null)
            {
                aimController.ValidActionReleased -= HandleValidActionReleased;
                aimController.AimChanged -= HandleAimChanged;
            }
        }

        private void HandleStartTrainWithBot() => StartNewMatch();
        private void HandleRematch() => StartNewMatch();

        private void HandleBackToHome()
        {
            StopAllCoroutines();
            CancelActiveProjectiles();
            ResetTransientState();
            currentState = MatchState.Initializing;
            if (aimController != null) aimController.DisableInput();
            if (playerActor != null) playerActor.SetVisibility(false);
            if (botActor != null) botActor.SetVisibility(false);
            uiManager?.ShowMainMenu();
            AudioManager.Instance?.PlayMenuBgm();
        }

        public void StartNewMatch()
        {
            StopAllCoroutines();
            CancelActiveProjectiles();
            ResetTransientState();
            _botBrain.Reset(botSeed);
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

            uiManager?.ShowInGameHUD();
            uiManager?.UpdateRound(currentRound);
            UpdateHpHud();
            AudioManager.Instance?.PlayMatchBgm();
            StartCoroutine(PreparingRoundRoutine());
        }

        private IEnumerator PreparingRoundRoutine()
        {
            currentState = MatchState.PreparingRound;
            uiManager?.UpdateRound(currentRound);
            uiManager?.UpdatePhaseBanner("CHUẨN BỊ VÒNG ĐẤU");
            uiManager?.SetReadyButtonState(false);

            _isPlayerLocked = false;
            _isBotLocked = false;
            _playerLockedAction = null;
            _botLockedAction = null;
            _currentBatchHits.Clear();

            Vector2 initialBotPos = botActor != null ? (Vector2)botActor.transform.position : new Vector2(0f, 3.0f);
            _botBrain.PrepareNewRound(initialBotPos);

            if (botActor != null)
            {
                botActor.SetPosition(_botBrain.StartPosition);
                botActor.SetVisibility(false);
                botActor.ShowAim(false);
            }

            Vector2 currentPlayerPos = playerActor != null ? (Vector2)playerActor.transform.position : new Vector2(0f, -3.5f);
            if (playerActor != null) playerActor.SetVisibility(true);

            yield return new WaitForSeconds(0.4f);

            currentState = MatchState.Aiming;
            uiManager?.UpdatePhaseBanner("CHẠM ĐỂ ĐẶT — GIỮ VÀ KÉO ĐỂ NGẮM");
            _phaseTimer = CombatRules.AimDuration;
            _lastTickSec = Mathf.CeilToInt(_phaseTimer);
            aimController?.EnableInput(currentPlayerPos, Vector2.up);
            PlanningStarted?.Invoke(currentRound);

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

                uiManager?.UpdateTimer(_phaseTimer);

                if (!_isBotLocked && _botBrain.UpdateTick(dt))
                    LockBotAction();

                if (_phaseTimer <= 0f)
                {
                    CommitPlayerDraftOrSkip();
                    if (!_isBotLocked) LockBotAction();
                    break;
                }

                if (_isPlayerLocked && _isBotLocked) break;
                yield return null;
            }

            yield return RevealingRoutine();
        }

        private void HandleAimChanged(Vector2 direction) => AimChanged?.Invoke(direction);

        private void HandleValidActionReleased(Vector2 position, Vector2 direction)
        {
            CommitPlayerAction(position, direction, true);
        }

        private void HandlePlayerClickReady()
        {
            // Keyboard/accessibility fallback. Pointer release uses the same commit gate.
            CommitPlayerDraftOrSkip(false);
        }

        public bool CommitPlayerAction(Vector2 position, Vector2 direction, bool willThrow)
        {
            if (currentState != MatchState.Aiming || _isPlayerLocked || playerActor == null || playerActor.currentHP <= 0)
                return false;

            if (willThrow && !CombatRules.TryNormalizeDirection(direction, out direction))
                return false;

            _isPlayerLocked = true;
            _playerLockedAction = new LockedAction(
                playerActor.actorId,
                CombatRules.ClampToArena(position),
                willThrow ? direction : Vector2.zero,
                "dep_to_ong",
                willThrow);

            aimController?.DisableInput();
            uiManager?.SetReadyButtonState(true);
            uiManager?.UpdatePhaseBanner(willThrow ? "ĐÃ KHÓA HÀNH ĐỘNG" : "ĐÃ BỎ LƯỢT");
            CommandLocked?.Invoke(_playerLockedAction);
            return true;
        }

        private void CommitPlayerDraftOrSkip(bool allowSkip = true)
        {
            if (_isPlayerLocked) return;

            if (aimController != null && aimController.HasValidAim)
            {
                CommitPlayerAction(aimController.CurrentPosition, aimController.CurrentDirection, true);
            }
            else if (allowSkip)
            {
                Vector2 position = playerActor != null ? (Vector2)playerActor.transform.position : Vector2.zero;
                CommitPlayerAction(position, Vector2.zero, false);
            }
        }

        private void LockBotAction()
        {
            if (_isBotLocked || botActor == null) return;
            _isBotLocked = true;
            _botLockedAction = _botBrain.GetLockedAction(botActor.actorId);
        }

        private IEnumerator RevealingRoutine()
        {
            currentState = MatchState.Revealing;
            aimController?.DisableInput();
            uiManager?.UpdateTimer(0f);
            uiManager?.UpdatePhaseBanner("MỞ VỊ TRÍ ĐỐI THỦ");
            RevealStarted?.Invoke();
            AudioManager.Instance?.PlayReveal();

            if (playerActor != null && _playerLockedAction != null)
            {
                playerActor.SetPosition(_playerLockedAction.position);
                playerActor.SetPose(_playerLockedAction.willThrow ? CharacterPose.Aim : CharacterPose.Idle);
            }

            if (botActor != null && _botLockedAction != null)
            {
                botActor.SetPosition(_botLockedAction.position);
                botActor.SetVisibility(true);
                botActor.SetPose(_botLockedAction.willThrow ? CharacterPose.Aim : CharacterPose.Idle);
            }

            yield return new WaitForSeconds(0.8f);
            yield return ResolvingRoutine();
        }

        private IEnumerator ResolvingRoutine()
        {
            currentState = MatchState.Resolving;
            uiManager?.UpdatePhaseBanner("ĐỒNG THỜI NÉM VŨ KHÍ");

            if (CanThrow(playerActor, _playerLockedAction)) playerActor.SetPose(CharacterPose.ThrowWindup);
            if (CanThrow(botActor, _botLockedAction)) botActor.SetPose(CharacterPose.ThrowWindup);
            yield return new WaitForSeconds(0.35f);

            ReleaseThrow(playerActor, _playerLockedAction);
            ReleaseThrow(botActor, _botLockedAction);
            AudioManager.Instance?.PlayThrow();

            SpawnProjectiles();
            while (_pendingProjectiles > 0)
                yield return null;

            ApplyBatchDamage();
            UpdateHpHud();
            yield return new WaitForSeconds(0.6f);
            EvaluateRoundSummary();
        }

        private static bool CanThrow(ActorView actor, LockedAction action)
        {
            return actor != null && actor.currentHP > 0 && action != null && action.willThrow;
        }

        private void ReleaseThrow(ActorView actor, LockedAction action)
        {
            if (!CanThrow(actor, action)) return;
            actor.SetPose(CharacterPose.ThrowRelease);
            actor.stats.throwsCount++;
            ThrowReleased?.Invoke(actor.actorId, action);
        }

        private void SpawnProjectiles()
        {
            _pendingProjectiles = 0;
            if (projectilePrefab == null) return;

            ActorView[] allActors = { playerActor, botActor };
            SpawnProjectileFor(playerActor, _playerLockedAction, allActors);
            SpawnProjectileFor(botActor, _botLockedAction, allActors);
        }

        private void SpawnProjectileFor(ActorView actor, LockedAction action, ActorView[] allActors)
        {
            if (!CanThrow(actor, action)) return;

            GameObject projectileObject = Instantiate(projectilePrefab);
            ProjectileView projectile = projectileObject.GetComponent<ProjectileView>();
            if (projectile == null)
            {
                Destroy(projectileObject);
                return;
            }

            _activeProjectiles.Add(projectile);
            _pendingProjectiles++;
            projectile.Initialize(actor.actorId, action.position, action.direction, allActors, OnProjectileHit, OnProjectileCompleted);
        }

        private void OnProjectileCompleted(ProjectileView projectile)
        {
            if (_activeProjectiles.Remove(projectile))
                _pendingProjectiles = Mathf.Max(0, _pendingProjectiles - 1);
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

            if (playerActor != null && shooterId == playerActor.actorId) playerActor.stats.hitsCount++;
            else if (botActor != null && shooterId == botActor.actorId) botActor.stats.hitsCount++;

            if (playerActor != null && targetId == playerActor.actorId) playerActor.SetPose(CharacterPose.Hit);
            if (botActor != null && targetId == botActor.actorId) botActor.SetPose(CharacterPose.Hit);
        }

        private void ApplyBatchDamage()
        {
            foreach (HitRecord hit in _currentBatchHits)
            {
                if (playerActor != null && hit.targetId == playerActor.actorId) playerActor.TakeDamage(hit.damage);
                else if (botActor != null && hit.targetId == botActor.actorId) botActor.TakeDamage(hit.damage);
                DamageApplied?.Invoke(hit);
            }
            _currentBatchHits.Clear();
        }

        private void EvaluateRoundSummary()
        {
            currentState = MatchState.RoundSummary;
            int playerHp = playerActor != null ? playerActor.currentHP : 0;
            int botHp = botActor != null ? botActor.currentHP : 0;
            MatchResultType result = CombatRules.DetermineMatchResult(playerHp, botHp, currentRound);

            if (result != MatchResultType.None)
            {
                EndMatch(result);
            }
            else
            {
                currentRound++;
                playerActor?.SetPose(CharacterPose.Idle);
                botActor?.SetPose(CharacterPose.Idle);
                StartCoroutine(PreparingRoundRoutine());
            }
        }

        private void EndMatch(MatchResultType result)
        {
            currentState = MatchState.MatchEnded;
            _phaseTimer = 0f;
            aimController?.DisableInput();

            if (playerActor != null && botActor != null)
            {
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
                    playerActor.SetPose(CharacterPose.Defeat);
                    botActor.SetPose(CharacterPose.Defeat);
                }

                uiManager?.ShowResult(result, playerActor.stats, botActor.stats);
            }

            MatchEnded?.Invoke(result);
        }

        private void UpdateHpHud()
        {
            if (playerActor == null || botActor == null) return;
            uiManager?.UpdateHP(playerActor.currentHP, playerActor.maxHP, botActor.currentHP, botActor.maxHP);
        }

        private void CancelActiveProjectiles()
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                if (_activeProjectiles[i] != null) _activeProjectiles[i].Cancel();
            }
            _activeProjectiles.Clear();
            _pendingProjectiles = 0;
        }

        private void ResetTransientState()
        {
            _phaseTimer = 0f;
            _lastTickSec = -1;
            _isPlayerLocked = false;
            _isBotLocked = false;
            _playerLockedAction = null;
            _botLockedAction = null;
            _currentBatchHits.Clear();
        }
    }
}
