using UnityEngine;
namespace LandoltRush
{
    public sealed class SpawnTempoSystem
    {
        readonly GameData data;
        readonly GameConfig config;
        public SpawnTempoSystem(GameData data, GameConfig config) { this.data=data;this.config=config; }
        public void Tick(float delta)
        {
            if(data.Phase==GamePhase.Playing)data.ElapsedPlayTime+=Mathf.Max(0,delta);
        }
        public float Interval
        {
            get
            {
                float initial=Mathf.Max(.1f,config.InitialSpawnInterval);
                float minimum=Mathf.Clamp(config.MinimumSpawnInterval,.1f,initial);
                return Mathf.Lerp(initial,minimum,Mathf.Clamp01(data.ElapsedPlayTime/Mathf.Max(1,config.SpawnRampSeconds)));
            }
        }
    }
}
