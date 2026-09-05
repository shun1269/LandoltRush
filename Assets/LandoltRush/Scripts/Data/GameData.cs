using UnityEngine;

namespace LandoltRush
{
    public enum GamePhase { Title, Playing, Paused, Finished }
    public enum ResultType { None, GameOver }
    public enum SpawnSide { Top, Left }
    public enum HitKind { None, Gap, Black }

    public sealed class GameData : MonoBehaviour
    {
        public GamePhase Phase;
        public ResultType Result;
        public int Score, ComboCount, MaxCombo, SuccessCount, MissCount;
        public float ComboRemainingTime;
    }

    public struct RingSpawnData
    {
        public SpawnSide Side;
        public Vector2 Position, Target, Velocity;
        public float Scale, Angle, AngularVelocity;
    }
}
