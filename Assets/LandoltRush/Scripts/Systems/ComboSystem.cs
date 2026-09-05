using UnityEngine;
namespace LandoltRush
{
    public sealed class ComboSystem
    {
        readonly GameData data; readonly GameConfig config;
        public ComboSystem(GameData data, GameConfig config) { this.data = data; this.config = config; }
        public void Add() { data.ComboCount++; data.MaxCombo = Mathf.Max(data.MaxCombo, data.ComboCount); data.ComboRemainingTime = config.ComboLimitTime; }
        public void Reset() { data.ComboCount = 0; data.ComboRemainingTime = 0; }
        public void Tick(float delta, bool ringSpawned)
        {
            if (!ringSpawned || data.ComboCount == 0) return;
            data.ComboRemainingTime = Mathf.Max(0, data.ComboRemainingTime - delta);
            if (data.ComboRemainingTime <= 0) Reset();
        }
    }
}
