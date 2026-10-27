using UnityEngine;

namespace HitMe.Core
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "HIT ME/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public string characterId;
        public string characterName;
        [TextArea] public string description;
        public Sprite avatarIcon;
        public int baseHp = 3;
        public float moveSpeed = 5.2f;

        [Header("Character Visual Poses")]
        public Sprite spriteIdle;
        public Sprite spriteMove;
        public Sprite spriteAim;
        public Sprite spriteThrowWindup;
        public Sprite spriteThrowRelease;
        public Sprite spriteHit;
        public Sprite spriteVictory;
        public Sprite spriteDefeat;
    }
}
