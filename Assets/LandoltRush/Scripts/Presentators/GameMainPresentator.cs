using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using VContainer.Unity;
namespace LandoltRush
{
    public sealed class GameMainPresentator : IStartable,ITickable,IDisposable
    {
        readonly GameData data;readonly GameConfig config;readonly RodParam rodParam;readonly RingParam ringParam;
        readonly RodController rod;readonly RodTipCollision collision;readonly LandoltRingSpawner spawner;
        readonly GameUIViewer ui;readonly ResultPanelViewer result;readonly GameInputInfo input;readonly FeedbackViewer feedback;
        readonly SoundComponent sound;readonly RingSpawnSystem spawnSystem;readonly ComboSystem combo;readonly ScoreSystem score;
        readonly GameJudgeSystem judge;readonly TitleDecorationViewer title;
        readonly SpawnTempoSystem tempo;
        readonly CompositeDisposable subscriptions=new CompositeDisposable();
        readonly Dictionary<LandoltRingComponent,IDisposable> ringSubscriptions=new Dictionary<LandoltRingComponent,IDisposable>();
        float spawnDelay,resultDelay=-1;bool hadFocus=true;
        public GameMainPresentator(GameData data,GameConfig config,RodParam rodParam,RingParam ringParam,
            RodController rod,RodTipCollision collision,LandoltRingSpawner spawner,GameUIViewer ui,ResultPanelViewer result,
            GameInputInfo input,FeedbackViewer feedback,SoundComponent sound,RingSpawnSystem spawnSystem,
            ComboSystem combo,ScoreSystem score,GameJudgeSystem judge,TitleDecorationViewer title,SpawnTempoSystem tempo)
        {
            this.data=data;this.config=config;this.rodParam=rodParam;this.ringParam=ringParam;this.rod=rod;this.collision=collision;
            this.spawner=spawner;this.ui=ui;this.result=result;this.input=input;this.feedback=feedback;this.sound=sound;
            this.spawnSystem=spawnSystem;this.combo=combo;this.score=score;this.judge=judge;this.title=title;
            this.tempo=tempo;
        }
        public void Start()
        {
            Application.targetFrameRate=120;
            subscriptions.Add(input.OnCommand.Subscribe(Command));subscriptions.Add(collision.OnDetected.Subscribe(Hit));
            ShowTitle();ui.Sound(sound.Muted);
        }
        void Command(GameCommand command)
        {
            switch(command)
            {
                case GameCommand.Start: if(data.Phase==GamePhase.Title||data.Phase==GamePhase.Finished)StartGame();else if(data.Phase==GamePhase.Paused)Pause(false);break;
                case GameCommand.Restart: if(data.Phase!=GamePhase.Title)StartGame();break;
                case GameCommand.Title: ShowTitle();break;
                case GameCommand.Pause: if(data.Phase==GamePhase.Playing)Pause(true);else if(data.Phase==GamePhase.Paused)Pause(false);break;
                case GameCommand.Mute:sound.Toggle();ui.Sound(sound.Muted);break;
                case GameCommand.Quit:Application.Quit();break;
            }
        }
        void StartGame()
        {
            ClearRings();judge.Start();result.Hide();feedback.Clear();title.gameObject.SetActive(false);
            rod.SetActive(true,rodParam);spawnDelay=config.FirstSpawnDelay;resultDelay=-1;ui.Refresh(data,config);
        }
        void ShowTitle()
        {
            ClearRings();judge.Title();result.Hide();feedback.Clear();rod.SetActive(false,rodParam);
            resultDelay=-1;title.gameObject.SetActive(true);ui.Refresh(data,config);
        }
        void Pause(bool paused)
        {
            judge.Pause(paused);if(paused)rod.Freeze();else rod.SetActive(true,rodParam);ui.Refresh(data,config);
        }
        public void Tick()
        {
            bool focused=Application.isFocused;
            if(hadFocus&&!focused&&data.Phase==GamePhase.Playing)Pause(true);
            hadFocus=focused;
            Step(Time.deltaTime);
        }
        public void Step(float deltaTime)
        {
            if(data.Phase==GamePhase.Finished&&resultDelay>=0)
            {
                resultDelay-=Mathf.Max(0,deltaTime);
                if(resultDelay<=0){resultDelay=-1;result.Show(data);}
            }
            if(data.Phase!=GamePhase.Playing)return;
            float dt=Mathf.Max(0,deltaTime);rod.Sample(rodParam);
            tempo.Tick(dt);
            combo.Tick(dt,spawner.Rings.Count>0);
            foreach(var ring in spawner.Rings)ring.Advance(dt);
            collision.Check(rod,spawner.Rings,rodParam,ringParam);
            if(data.Phase==GamePhase.Playing)
            {
                // Exit callbacks may remove rings, so traverse backwards.
                for(int i=spawner.Rings.Count-1;i>=0;i--)spawner.Rings[i].CheckExit(rod.Bounds);
                spawnDelay=Mathf.Max(0,spawnDelay-dt);
                if(spawnDelay<=0&&spawner.Rings.Count<Mathf.Max(1,config.MaxConcurrentRings))
                {
                    Spawn(spawnSystem.Create(rod.Bounds));
                    spawnDelay=tempo.Interval;
                }
            }
            ui.Refresh(data,config);
        }
        public void Spawn(RingSpawnData spawn)
        {
            var ring=spawner.Spawn(spawn,ringParam);
            ringSubscriptions.Add(ring,ring.OnExited.Subscribe(_=>Miss(ring)));
        }
        void Hit(RingContact contact)
        {
            var ring=contact.Ring;var hit=contact.Kind;
            if(data.Phase!=GamePhase.Playing||ring==null||ring.Resolved||!ringSubscriptions.ContainsKey(ring))return;
            if(hit!=HitKind.Gap){Miss(ring);return;}
            ring.Resolve();
            combo.Add();score.AddSuccess();sound.Play(hit);feedback.Success(ring.Position,ring.Angle,ring.Scale,rod.Bounds);RemoveRing(ring);
        }
        void Miss(LandoltRingComponent ring)
        {
            if(data.Phase!=GamePhase.Playing||ring==null||!ringSubscriptions.ContainsKey(ring))return;
            if(!judge.Miss())return;
            combo.Reset();sound.Play(HitKind.Black);feedback.Damage();
            RemoveRing(ring);
            if(data.Phase==GamePhase.Finished){rod.Freeze();spawner.StopAll();resultDelay=.6f;}
        }
        void RemoveRing(LandoltRingComponent ring)
        {
            if(ringSubscriptions.TryGetValue(ring,out var subscription)){subscription.Dispose();ringSubscriptions.Remove(ring);}
            spawner.Remove(ring);
        }
        void ClearRings(){foreach(var subscription in ringSubscriptions.Values)subscription.Dispose();ringSubscriptions.Clear();spawner.Clear();}
        public void Dispose(){foreach(var subscription in ringSubscriptions.Values)subscription.Dispose();ringSubscriptions.Clear();subscriptions.Dispose();}
    }
}
