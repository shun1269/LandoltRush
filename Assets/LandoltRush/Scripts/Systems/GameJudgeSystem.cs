namespace LandoltRush
{
    public sealed class GameJudgeSystem
    {
        readonly GameData data;
        readonly GameConfig config;
        public GameJudgeSystem(GameData data,GameConfig config) { this.data = data;this.config=config; }
        public void Start()
        {
            data.Phase = GamePhase.Playing; data.Result = ResultType.None;
            data.Score = data.ComboCount = data.MaxCombo = data.SuccessCount = data.MissCount = 0;
            data.ComboRemainingTime = 0;
            data.ElapsedPlayTime = 0;
            data.Lives = UnityEngine.Mathf.Clamp(config.MaxLives,1,3);
        }
        public bool Miss()
        {
            if (data.Phase != GamePhase.Playing) return false;
            data.MissCount++;data.Lives=System.Math.Max(0,data.Lives-1);
            if(data.Lives==0){data.Phase=GamePhase.Finished;data.Result=ResultType.GameOver;}
            return true;
        }
        public void Title() { data.Phase = GamePhase.Title; }
        public void Pause(bool paused) { data.Phase = paused ? GamePhase.Paused : GamePhase.Playing; }
    }
}
