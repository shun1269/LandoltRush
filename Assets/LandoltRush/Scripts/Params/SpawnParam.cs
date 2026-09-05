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
        public float MinScale = 1.2f, MaxScale = 1.6f;
        [Tooltip("回転速度（度/秒）。最低回転数に届かない場合は上限を超えて補正します。")]
        public float MinRotateSpeed = 180f, MaxRotateSpeed = 240f;
        [Min(0), Tooltip("環全体が画面内にある間の最低回転数。0で補正を無効にします。")]
        public float MinVisibleRotations = 2f;
    }
}
