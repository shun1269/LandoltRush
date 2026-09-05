using UnityEngine;

namespace LandoltRush
{
    public enum GamePhase { Title, Playing, Paused, Finished }
    public enum ResultType { None, GameOver }
    public enum SpawnSide { Top, Left }
    public enum HitKind { None, Gap, Black, Shaft }

    public readonly struct RingContact
    {
        public readonly LandoltRingComponent Ring;
        public readonly HitKind Kind;
        public RingContact(LandoltRingComponent ring, HitKind kind) { Ring=ring; Kind=kind; }
    }

    public sealed class GameData : MonoBehaviour
    {
        public GamePhase Phase;
        public ResultType Result;
        public int Score, ComboCount, MaxCombo, SuccessCount, MissCount;
        public float ComboRemainingTime;
        public float ElapsedPlayTime;
        public int Lives;
    }

    public struct RingSpawnData
    {
        public SpawnSide Side;
        public Vector2 Position, Target, Velocity;
        public float Scale, Angle, AngularVelocity;
    }
}
