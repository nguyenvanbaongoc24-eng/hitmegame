using System;
using UnityEngine;
using HitMe.Core;
using HitMe.Gameplay;

namespace HitMe.Presentation
{
    [Serializable]
    public struct MatchPresentationState
    {
        public string characterId;
        public string characterName;
        public string weaponId;
        public string weaponName;
        public int round;
        public float timer;
        public int playerHp;
        public int playerMaxHp;
        public int opponentHp;
        public int opponentMaxHp;
        public MatchState phase;
        public Vector2 aimDirection;
        public bool isActionLocked;
    }

    // Passive adapter for AG. Gameplay remains the only authority for phase, HP, timer and aim.
    public sealed class MatchPresentationAdapter : MonoBehaviour
    {
        [SerializeField] private MatchStateMachine match;
        [SerializeField] private AimController aim;
        [SerializeField] private ActorView player;
        [SerializeField] private ActorView opponent;
        [SerializeField] private CharacterData selectedCharacter;
        [SerializeField] private WeaponData selectedWeapon;

        public event Action<MatchPresentationState> StateChanged;
        public MatchPresentationState Current { get; private set; }

        public void Bind(MatchStateMachine stateMachine, AimController aimController, ActorView playerActor, ActorView opponentActor, CharacterData character, WeaponData weapon)
        {
            match = stateMachine; aim = aimController; player = playerActor; opponent = opponentActor;
            selectedCharacter = character; selectedWeapon = weapon;
            Publish();
        }

        private void Update() => Publish();

        private void Publish()
        {
            if (match == null || player == null || opponent == null) return;
            var next = new MatchPresentationState
            {
                characterId = selectedCharacter != null ? selectedCharacter.characterId : "nam_lang",
                characterName = selectedCharacter != null ? selectedCharacter.characterName : player.actorName,
                weaponId = selectedWeapon != null ? selectedWeapon.weaponId : "dep_to_ong",
                weaponName = selectedWeapon != null ? selectedWeapon.weaponName : "Dép tổ ong",
                round = match.currentRound,
                timer = match.PhaseSecondsRemaining,
                playerHp = player.currentHP,
                playerMaxHp = player.maxHP,
                opponentHp = opponent.currentHP,
                opponentMaxHp = opponent.maxHP,
                phase = match.currentState,
                aimDirection = aim != null ? aim.CurrentDirection : Vector2.up,
                isActionLocked = match.IsPlayerActionLocked
            };
            if (!Equivalent(Current, next))
            {
                Current = next;
                StateChanged?.Invoke(Current);
            }
        }

        private static bool Equivalent(MatchPresentationState a, MatchPresentationState b)
        {
            return a.round == b.round && a.timer.Equals(b.timer) && a.playerHp == b.playerHp && a.opponentHp == b.opponentHp &&
                   a.phase == b.phase && a.isActionLocked == b.isActionLocked && a.aimDirection == b.aimDirection && a.weaponId == b.weaponId;
        }
    }
}
