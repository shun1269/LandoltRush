using UnityEngine;
namespace LandoltRush
{
    [CreateAssetMenu(menuName = "Landolt Rush/Ring Param")]
    public sealed class RingParam : ScriptableObject
    {
        public LandoltRingComponent Prefab;
        [Min(.1f)] public float OuterRadius = .9f;
        [Min(.05f)] public float InnerRadius = .55f;
        [Range(20, 90)] public float GapDegrees = 50;
        public Color Ink = new Color(.055f, .075f, .08f);
    }
}
