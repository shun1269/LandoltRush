using UnityEngine;
namespace LandoltRush
{
    [CreateAssetMenu(menuName = "Landolt Rush/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public int BaseScore = 100;
        public int ComboBonusScore = 20;
        [Min(0.1f)] public float ComboLimitTime = 6f;
        public float FirstSpawnDelay = .6f;
        [Min(.1f)] public float InitialSpawnInterval = 4f;
        [Min(.1f)] public float MinimumSpawnInterval = .9f;
        [Min(1f)] public float SpawnRampSeconds = 90f;
        [Min(1)] public int MaxConcurrentRings = 12;
    }
}
