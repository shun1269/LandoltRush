using UnityEngine;
namespace LandoltRush
{
    [CreateAssetMenu(menuName = "Landolt Rush/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public int BaseScore = 100;
        public int ComboBonusScore = 20;
        [Min(0.1f)] public float ComboLimitTime = 2f;
        public float FirstSpawnDelay = .6f;
        public float SuccessSpawnDelay = .28f;
        public float MissSpawnDelay = .4f;
    }
}
