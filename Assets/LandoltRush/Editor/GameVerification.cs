using System;
using System.IO;
using UnityEngine;
using R3;
namespace LandoltRush.Editor
{
    public static class GameVerification
    {
        static int checks;
        static void Check(bool condition,string message){if(!condition)throw new Exception("Verification failed: "+message);checks++;}
        static HitKind Hit(Vector2 a,Vector2 b,float angle=0,float turn=0,Vector2? move=null)=>RingGeometry.Sweep(a,b,.045f,Vector2.zero,move??Vector2.zero,angle,turn,1,.55f,.9f,50);
        public static void Run()
        {
            checks=0;
            Check(Hit(new Vector2(1.3f,0),new Vector2(.72f,0))==HitKind.None,"Outer opening does not score");
            Check(Hit(new Vector2(.72f,0),new Vector2(.56f,0))==HitKind.None,"Tip edge crossing inner radius does not score");
            Check(Hit(new Vector2(.72f,0),new Vector2(.50f,0))==HitKind.Gap,"Tip centre reaches inner radius");
            Check(Hit(new Vector2(.50f,0),new Vector2(.3f,0))==HitKind.None,"Remaining inside hole does not score again");
            Check(Hit(new Vector2(.50f,0),new Vector2(1.3f,0))==HitKind.None,"Outward crossing does not score");
            Check(Hit(new Vector2(1.3f,0),new Vector2(.3f,0))==HitKind.Gap,"Fast safe inward sweep scores");
            Check(Hit(new Vector2(-1.3f,0),new Vector2(-.72f,0))==HitKind.Black,"Tip touches ink");
            Check(Hit(new Vector2(-2,0),new Vector2(2,0))==HitKind.Black,"Fast sweep and black precedence");
            Check(Hit(Vector2.zero,Vector2.zero)==HitKind.None,"Ring hole is not gap");
            Check(Hit(new Vector2(0,-2),new Vector2(0,2))==HitKind.Black,"Vertical tunnelling prevented");
            Check(Hit(new Vector2(2,2),new Vector2(3,3))==HitKind.None,"Distant tip does not cause game over");
            Check(Hit(new Vector2(.72f,0),new Vector2(.72f,0),90)==HitKind.Black,"Rotation changes black region");
            Check(Hit(new Vector2(.72f,0),new Vector2(.72f,0),0,90)==HitKind.Black,"Rotating ring hits stationary tip");
            Check(Hit(new Vector2(2,0),new Vector2(2,0),180,0,new Vector2(3,0))==HitKind.Black,"Moving ring hits stationary tip");
            Check(Hit(new Vector2(-.93f,0),new Vector2(-.93f,0))==HitKind.Black,"Tip radius at outer edge");
            Check(Hit(new Vector2(-1f,0),new Vector2(-1f,0))==HitKind.None,"Outside expanded radius safe");
            Check(Hit(RingGeometry.Polar(.73f,23),RingGeometry.Polar(.73f,23))==HitKind.Black,"Gap boundary black precedence");
            Check(Shaft(new Vector2(3,0),new Vector2(-3,0),new Vector2(-3,0)),"Shaft crosses ink while tip is outside");
            Check(!Shaft(new Vector2(3,0),new Vector2(.4f,0),new Vector2(.4f,0)),"Shaft fits through gap");
            Check(!Shaft(new Vector2(3,0),new Vector2(-3,4),new Vector2(-3,4)),"Fast swing start clear");
            Check(!Shaft(new Vector2(3,0),new Vector2(-3,-4),new Vector2(-3,-4)),"Fast swing end clear");
            Check(Shaft(new Vector2(3,0),new Vector2(-3,4),new Vector2(-3,-4)),"Fast shaft sweep detects middle of swing");
            Check(Shaft(new Vector2(3,0),new Vector2(.4f,0),new Vector2(.4f,0),90),"Rotating ink contacts stationary shaft");
            Check(Shaft(new Vector2(3,.915f),new Vector2(-3,.915f),new Vector2(-3,.915f)),"Shaft thickness at ink edge");
            Check(!Shaft(new Vector2(3,.93f),new Vector2(-3,.93f),new Vector2(-3,.93f)),"Shaft outside thickness is safe");
            Check(RingGeometry.SweepShaft(new Vector2(3,0),new Vector2(3,0),new Vector2(-3,0),new Vector2(-3,0),.018f,
                new Vector2(0,2),new Vector2(0,-2),0,0,1,.55f,.9f,50),"Moving ring contacts stationary shaft");
            var obj=new GameObject("Verification data");var data=obj.AddComponent<GameData>();
            var config=ScriptableObject.CreateInstance<GameConfig>();var spawn=ScriptableObject.CreateInstance<SpawnParam>();var ring=ScriptableObject.CreateInstance<RingParam>();
            var combo=new ComboSystem(data,config);var score=new ScoreSystem(data,config);var judge=new GameJudgeSystem(data);judge.Start();
            combo.Add();Check(score.AddSuccess()==100,"First score");combo.Tick(9,false);Check(data.ComboCount==1&&data.ComboRemainingTime==6,"Empty field freezes six-second combo");
            combo.Tick(.5f,true);combo.Add();Check(score.AddSuccess()==120&&data.Score==220,"Combo scoring");
            combo.Tick(2.1f,true);Check(data.ComboCount==2,"Combo survives more than two seconds");
            combo.Tick(4f,true);Check(data.ComboCount==0&&data.MaxCombo==2,"Combo expiration retains maximum");
            combo.Add();judge.Miss();combo.Reset();Check(data.MissCount==1&&data.Score==220&&data.ComboCount==0,"Miss resets only combo");
            Check(judge.Finish()&&!judge.Finish()&&data.Result==ResultType.GameOver,"Game over only once");
            judge.Start();Check(data.Score==0&&data.SuccessCount==0&&data.MissCount==0&&data.Phase==GamePhase.Playing,"Restart clears run state");
            judge.Pause(true);Check(data.Phase==GamePhase.Paused,"Pause");judge.Pause(false);judge.Title();Check(data.Phase==GamePhase.Title,"Return to title");
            var tempo=new SpawnTempoSystem(data,config);judge.Start();
            Check(Mathf.Approximately(tempo.Interval,4),"Initial spawn interval");tempo.Tick(45);
            Check(Mathf.Approximately(tempo.Interval,2.45f),"Spawn interval ramps halfway");
            judge.Pause(true);tempo.Tick(30);Check(data.ElapsedPlayTime==45,"Pause freezes acceleration");judge.Pause(false);
            tempo.Tick(45);Check(Mathf.Approximately(tempo.Interval,.9f),"Spawn interval reaches minimum");
            tempo.Tick(1000);Check(Mathf.Approximately(tempo.Interval,.9f),"Spawn acceleration is bounded");
            judge.Title();float elapsed=data.ElapsedPlayTime;tempo.Tick(20);Check(data.ElapsedPlayTime==elapsed,"Title freezes acceleration");
            judge.Start();Check(data.ElapsedPlayTime==0&&Mathf.Approximately(tempo.Interval,4),"Restart resets spawn acceleration");
            var bounds=new Rect(-8,-4.5f,16,9);var system=new RingSpawnSystem(spawn,ring);int top=0,left=0;
            for(int i=0;i<500;i++)
            {
                var d=system.Create(bounds);if(d.Side==SpawnSide.Top)top++;else left++;
                Check(bounds.Contains(d.Target),"Target inside screen");
                Check(d.Side==SpawnSide.Top?d.Position.y-ring.OuterRadius*d.Scale>bounds.yMax:d.Position.x+ring.OuterRadius*d.Scale<bounds.xMin,"Fully offscreen spawn");
                Check(Vector2.Dot(d.Target-d.Position,d.Velocity)>0,"Moves towards target");
                Check(d.Scale>=.8f&&d.Scale<=1.3f&&Mathf.Abs(d.AngularVelocity)>=30&&Mathf.Abs(d.AngularVelocity)<=180,"Random parameter limits");
            }
            Check(top>150&&left>150,"Both spawn sides");
            var ringObject=new GameObject("Entry verification");var comp=ringObject.AddComponent<LandoltRingComponent>();comp.Viewer=ringObject.AddComponent<RingViewer>();
            comp.Initialize(new RingSpawnData{Position=new Vector2(-10,0),Velocity=new Vector2(4,0),Scale=1},ring);
            comp.CheckExit(bounds);Check(!comp.Entered&&!comp.Resolved,"No premature miss at spawn");comp.Advance(1);comp.CheckExit(bounds);Check(comp.Entered&&!comp.Resolved,"Entry state");
            comp.Advance(5);comp.CheckExit(bounds);Check(comp.Resolved,"Miss after fully exiting");
            VerifyMultipleRings();
            UnityEngine.Object.DestroyImmediate(ringObject);UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(config);UnityEngine.Object.DestroyImmediate(spawn);UnityEngine.Object.DestroyImmediate(ring);
            Directory.CreateDirectory("Builds");File.WriteAllText("Builds/verification.txt",$"PASS — {checks} assertions\nInner-radius entry, swept shaft, multi-ring priority and lifecycle, six-second combo, timed spawn ramp, pause/restart, randomized spawn bounds.\n");
            Debug.Log($"LANDOLT_TESTS_PASSED: {checks}");
        }
        static bool Shaft(Vector2 pivot,Vector2 from,Vector2 to,float turn=0)=>RingGeometry.SweepShaft(pivot,pivot,from,to,.018f,Vector2.zero,Vector2.zero,0,turn,1,.55f,.9f,50);
        static void VerifyMultipleRings()
        {
            GameLifetimeScope scope=null;
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if(root.TryGetComponent<GameLifetimeScope>(out var found)){scope=found;break;}
            Check(scope!=null,"Generated scene has scope");
            var rod=scope.Rod;var spawner=scope.Spawner;var events=new System.Collections.Generic.List<RingContact>();
            using(var subscription=scope.Collision.OnDetected.Subscribe(contact=>events.Add(contact)))
            {
                Vector2 direction=new Vector2(10,-6.5f).normalized;float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
                rod.TestPosition=direction*1.3f;rod.SetActive(true,scope.RodParam);
                var safe=spawner.Spawn(new RingSpawnData{Position=Vector2.zero,Scale=1,Angle=angle},scope.RingParam);
                rod.TestPosition=direction*.45f;rod.Sample(scope.RodParam);safe.Advance(0);
                scope.Collision.Check(rod,spawner.Rings,scope.RodParam,scope.RingParam);
                Check(events.Count==1&&events[0].Kind==HitKind.Gap,"Aligned shaft and inward tip succeed");events.Clear();
                // A second ring lies on the tip's path with its solid side facing the tip.
                var dangerous=spawner.Spawn(new RingSpawnData{Position=direction*1.7f,Scale=1,Angle=angle},scope.RingParam);
                scope.Collision.Check(rod,spawner.Rings,scope.RodParam,scope.RingParam);
                Check(events.Count==1&&events[0].Kind==HitKind.Black&&events[0].Ring==dangerous,"Fatal hit on second ring suppresses first-ring success");
                Check(spawner.Rings.Count==2,"Spawn keeps existing ring");
                spawner.Remove(dangerous);Check(spawner.Rings.Count==1&&spawner.Rings[0]==safe,"Removing one ring keeps the other");
                spawner.StopAll();Check(safe.Resolved,"Game over freezes all remaining rings");spawner.Clear();events.Clear();
                rod.TestPosition=new Vector2(-2,0);rod.SetActive(true,scope.RodParam);
                var body=spawner.Spawn(new RingSpawnData{Position=Vector2.Lerp(rod.Tip,rod.Pivot,.3f),Scale=1,Angle=0},scope.RingParam);
                scope.Collision.Check(rod,spawner.Rings,scope.RodParam,scope.RingParam);
                Check(events.Count==1&&events[0].Kind==HitKind.Shaft&&events[0].Ring==body,"Shaft contact is a separate miss outcome");
                spawner.Clear();Check(spawner.Rings.Count==0,"Clear removes every ring");rod.TestPosition=null;rod.SetActive(false,scope.RodParam);
            }
            VerifySpawnSchedule(scope);
        }
        static void VerifySpawnSchedule(GameLifetimeScope scope)
        {
            var data=scope.Data;var config=scope.Config;var rod=scope.Rod;var spawner=scope.Spawner;
            var combo=new ComboSystem(data,config);var tempo=new SpawnTempoSystem(data,config);
            using(var presenter=new GameMainPresentator(data,config,scope.RodParam,scope.RingParam,rod,scope.Collision,spawner,
                scope.UI,scope.Result,scope.Input,scope.Feedback,scope.Sound,new RingSpawnSystem(scope.SpawnParam,scope.RingParam),
                combo,new ScoreSystem(data,config),new GameJudgeSystem(data),scope.Title,tempo))
            {
                presenter.Start();rod.TestPosition=new Vector2(6,-3);scope.Input.Emit(GameCommand.Start);
                presenter.Step(config.FirstSpawnDelay);Check(spawner.Rings.Count==1,"Automatic first spawn");
                var first=spawner.Rings[0];first.Initialize(new RingSpawnData{Position=new Vector2(-5,3),Scale=1},scope.RingParam);
                presenter.Step(tempo.Interval+.001f);Check(spawner.Rings.Count==2&&spawner.Rings[0]==first,"Timer spawns second ring before first resolves");
                for(int i=0;i<config.MaxConcurrentRings+2;i++)
                {
                    foreach(var ring in spawner.Rings)ring.Initialize(new RingSpawnData{Position=new Vector2(-5,3),Scale=1},scope.RingParam);
                    presenter.Step(config.InitialSpawnInterval);
                }
                Check(spawner.Rings.Count==config.MaxConcurrentRings,"Concurrent ring cap");
                scope.Input.Emit(GameCommand.Pause);float elapsed=data.ElapsedPlayTime;presenter.Step(500);
                Check(data.ElapsedPlayTime==elapsed&&spawner.Rings.Count==config.MaxConcurrentRings,"Pause freezes active ring schedule");
                scope.Input.Emit(GameCommand.Restart);
                Check(spawner.Rings.Count==0&&data.ElapsedPlayTime==0&&data.ComboCount==0,"Restart clears all rings and tempo");
                presenter.Step(config.FirstSpawnDelay*.9f);Check(spawner.Rings.Count==0,"Restart restores first spawn delay");
                presenter.Step(config.FirstSpawnDelay*.2f);Check(spawner.Rings.Count==1,"Restart spawns exactly one ring");
                scope.Input.Emit(GameCommand.Title);Check(spawner.Rings.Count==0&&data.Phase==GamePhase.Title,"Title clears all active rings");
                rod.TestPosition=null;
            }
        }
    }
}
