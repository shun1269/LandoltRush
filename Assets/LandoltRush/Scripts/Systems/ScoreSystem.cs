namespace LandoltRush
{
    public sealed class ScoreSystem
    {
        readonly GameData data; readonly GameConfig config;
        public ScoreSystem(GameData data, GameConfig config) { this.data = data; this.config = config; }
        public int AddSuccess()
        {
            int points = config.BaseScore + System.Math.Max(0, data.ComboCount - 1) * config.ComboBonusScore;
            data.Score += points; data.SuccessCount++; return points;
        }
    }
}
