using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace LandoltRush
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        public GameConfig Config;public RodParam RodParam;public RingParam RingParam;public SpawnParam SpawnParam;
        public GameData Data;public RodController Rod;public RodTipCollision Collision;public LandoltRingSpawner Spawner;
        public GameUIViewer UI;public ResultPanelViewer Result;public GameInputInfo Input;
        public FeedbackViewer Feedback;public SoundComponent Sound;public TitleDecorationViewer Title;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(Config);builder.RegisterInstance(RodParam);builder.RegisterInstance(RingParam);builder.RegisterInstance(SpawnParam);
            builder.RegisterComponent(Data);builder.RegisterComponent(Rod);builder.RegisterComponent(Collision);builder.RegisterComponent(Spawner);
            builder.RegisterComponent(UI);builder.RegisterComponent(Result);builder.RegisterComponent(Input);builder.RegisterComponent(Feedback);
            builder.RegisterComponent(Sound);builder.RegisterComponent(Title);
            builder.Register<RingSpawnSystem>(Lifetime.Scoped);builder.Register<ScoreSystem>(Lifetime.Scoped);
            builder.Register<ComboSystem>(Lifetime.Scoped);builder.Register<GameJudgeSystem>(Lifetime.Scoped);
            builder.RegisterEntryPoint<GameMainPresentator>().AsSelf();
        }
    }
}
