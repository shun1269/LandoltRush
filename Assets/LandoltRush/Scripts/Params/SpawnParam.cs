using UnityEngine;
namespace LandoltRush
{
    [CreateAssetMenu(menuName = "Landolt Rush/Spawn Param")]
    public sealed class SpawnParam : ScriptableObject
    {
        [Min(0)] public float TopSpawnWeight = 1, LeftSpawnWeight = 1;
        public float SpawnMargin = .15f, EdgeExclusionMargin = 1f;
        [Range(.1f,.8f)] public float TargetAreaRate = .45f;
        public float MinMoveSpeed = 1.5f, MaxMoveSpeed = 3f;
        public float MinScale = .8f, MaxScale = 1.3f;
        public float MinRotateSpeed = 30f, MaxRotateSpeed = 180f;
    }
}
