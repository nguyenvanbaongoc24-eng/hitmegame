using UnityEngine;

namespace HitMe.Core
{
    public enum WeaponTrajectoryType
    {
        Straight,
        Curved,
        Lobbed
    }

    [CreateAssetMenu(fileName = "NewWeapon", menuName = "HIT ME/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string weaponId;
        public string weaponName;
        [TextArea] public string description;
        public Sprite icon;
        public Sprite projectileSprite;
        public int damage = 1;
        public float speed = 14.0f;
        public float hitRadius = 0.5f;
        public WeaponTrajectoryType trajectoryType = WeaponTrajectoryType.Straight;
        public AudioClip sfxThrow;
        public AudioClip sfxImpact;
    }
}
