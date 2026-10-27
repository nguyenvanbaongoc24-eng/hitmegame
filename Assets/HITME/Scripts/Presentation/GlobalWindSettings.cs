using UnityEngine;

namespace HitMe.Presentation
{
    [CreateAssetMenu(fileName = "GlobalWindSettings", menuName = "HIT ME/Visual/Global Wind Settings")]
    public sealed class GlobalWindSettings : ScriptableObject
    {
        [Range(0f, 1f)] public float strength = .22f;
        [Range(.01f, 4f)] public float frequency = .7f;
        [Range(0f, 30f)] public float maxAngle = 3.5f;
    }
}
