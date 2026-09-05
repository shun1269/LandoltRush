using UnityEngine;
namespace LandoltRush
{
    [CreateAssetMenu(menuName = "Landolt Rush/Rod Param")]
    public sealed class RodParam : ScriptableObject
    {
        public float RightPivotMargin = 2;
        public float BottomPivotMargin = 2;
        [Min(.01f)] public float TipRadius = .045f;
    }
}
