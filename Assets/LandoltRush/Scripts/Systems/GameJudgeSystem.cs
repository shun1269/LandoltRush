namespace LandoltRush
{
    public sealed class GameJudgeSystem
    {
        readonly GameData data;
        public GameJudgeSystem(GameData data) { this.data = data; }
        public void Start()
        {
            data.Phase = GamePhase.Playing; data.Result = ResultType.None;
            data.Score = data.ComboCount = data.MaxCombo = data.SuccessCount = data.MissCount = 0;
            data.ComboRemainingTime = 0;
            data.ElapsedPlayTime = 0;
        }
        public bool Finish()
        {
            if (data.Phase != GamePhase.Playing) return false;
            data.Phase = GamePhase.Finished; data.Result = ResultType.GameOver; return true;
        }
        public void Miss() { data.MissCount++; }
        public void Title() { data.Phase = GamePhase.Title; }
        public void Pause(bool paused) { data.Phase = paused ? GamePhase.Paused : GamePhase.Playing; }
    }
}
