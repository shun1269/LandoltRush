using System;
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
        readonly CompositeDisposable subscriptions=new CompositeDisposable();IDisposable ringSubscription;
        float spawnDelay;bool hadFocus=true;
        static readonly Color Teal=new Color(.0f,.57f,.48f),Red=new Color(.87f,.22f,.20f);
        public GameMainPresentator(GameData data,GameConfig config,RodParam rodParam,RingParam ringParam,
            RodController rod,RodTipCollision collision,LandoltRingSpawner spawner,GameUIViewer ui,ResultPanelViewer result,
            GameInputInfo input,FeedbackViewer feedback,SoundComponent sound,RingSpawnSystem spawnSystem,
            ComboSystem combo,ScoreSystem score,GameJudgeSystem judge,TitleDecorationViewer title)
        {
            this.data=data;this.config=config;this.rodParam=rodParam;this.ringParam=ringParam;this.rod=rod;this.collision=collision;
            this.spawner=spawner;this.ui=ui;this.result=result;this.input=input;this.feedback=feedback;this.sound=sound;
            this.spawnSystem=spawnSystem;this.combo=combo;this.score=score;this.judge=judge;this.title=title;
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
            ClearRing();judge.Start();result.Hide();feedback.Clear();title.gameObject.SetActive(false);
            rod.SetActive(true,rodParam);spawnDelay=config.FirstSpawnDelay;ui.Status("AIM FOR THE GAP");ui.Refresh(data,config);
        }
        void ShowTitle()
        {
            ClearRing();judge.Title();result.Hide();feedback.Clear();rod.SetActive(false,rodParam);
            title.gameObject.SetActive(true);ui.Refresh(data,config);
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
            if(data.Phase!=GamePhase.Playing)return;
            float dt=Time.deltaTime;rod.Sample(rodParam);
            var ring=spawner.Current;
            combo.Tick(dt,ring!=null);
            if(ring==null)
            {
                spawnDelay-=dt;
                if(spawnDelay<=0)Spawn(spawnSystem.Create(rod.Bounds));
            }
            else
            {
                ring.Advance(dt);collision.Check(rod,ring,rodParam,ringParam);
                if(data.Phase==GamePhase.Playing&&spawner.Current==ring)ring.CheckExit(rod.Bounds);
            }
            ui.Refresh(data,config);
        }
        public void Spawn(RingSpawnData spawn)
        {
            ClearRing();var ring=spawner.Spawn(spawn,ringParam);
            ringSubscription=ring.OnExited.Subscribe(_=>Miss());
        }
        void Hit(HitKind hit)
        {
            if(data.Phase!=GamePhase.Playing||spawner.Current==null||spawner.Current.Resolved)return;
            var ring=spawner.Current;ring.Resolve();
            if(hit==HitKind.Black)
            {
                if(!judge.Finish())return;rod.Freeze();sound.Play(hit);
                feedback.Play(rod.Tip,"CONTACT",Red);ui.Status("RUN COMPLETE");result.Show(data);
            }
            else
            {
                combo.Add();int points=score.AddSuccess();sound.Play(hit);
                feedback.Play(ring.Position,$"+{points}   /   NICE GAP",Teal);
                ui.Status(data.ComboCount>1?$"KEEP IT FLOWING  /  {data.ComboCount} IN A ROW":"NICE. FIND THE NEXT GAP.");
                ClearRing();spawnDelay=config.SuccessSpawnDelay;
            }
        }
        void Miss()
        {
            if(data.Phase!=GamePhase.Playing)return;
            judge.Miss();combo.Reset();sound.Play(HitKind.None);ui.Status("MISSED  /  COMBO RESET — KEEP GOING");
            ClearRing();spawnDelay=config.MissSpawnDelay;
        }
        void ClearRing(){ringSubscription?.Dispose();ringSubscription=null;spawner.Clear();}
        public void Dispose(){ringSubscription?.Dispose();subscriptions.Dispose();}
    }
}
