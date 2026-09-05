using System;
using System.IO;
using UnityEngine;
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
            Check(Hit(new Vector2(1.3f,0),new Vector2(.72f,0))==HitKind.Gap,"Tip enters gap");
            Check(Hit(new Vector2(-1.3f,0),new Vector2(-.72f,0))==HitKind.Black,"Tip touches ink");
            Check(Hit(new Vector2(-2,0),new Vector2(2,0))==HitKind.Black,"Fast sweep and black precedence");
            Check(Hit(Vector2.zero,Vector2.zero)==HitKind.None,"Ring hole is not gap");
            Check(Hit(new Vector2(0,-2),new Vector2(0,2))==HitKind.Black,"Vertical tunnelling prevented");
            Check(Hit(new Vector2(2,2),new Vector2(3,3))==HitKind.None,"Distant tip safe even if shaft crosses ring");
            Check(Hit(new Vector2(.72f,0),new Vector2(.72f,0),90)==HitKind.Black,"Rotation changes black region");
            Check(Hit(new Vector2(.72f,0),new Vector2(.72f,0),0,90)==HitKind.Black,"Rotating ring hits stationary tip");
            Check(Hit(new Vector2(2,0),new Vector2(2,0),180,0,new Vector2(3,0))==HitKind.Black,"Moving ring hits stationary tip");
            Check(Hit(new Vector2(-.93f,0),new Vector2(-.93f,0))==HitKind.Black,"Tip radius at outer edge");
            Check(Hit(new Vector2(-1f,0),new Vector2(-1f,0))==HitKind.None,"Outside expanded radius safe");
            Check(Hit(RingGeometry.Polar(.73f,23),RingGeometry.Polar(.73f,23))==HitKind.Black,"Gap boundary black precedence");
            var obj=new GameObject("Verification data");var data=obj.AddComponent<GameData>();
            var config=ScriptableObject.CreateInstance<GameConfig>();var spawn=ScriptableObject.CreateInstance<SpawnParam>();var ring=ScriptableObject.CreateInstance<RingParam>();
            var combo=new ComboSystem(data,config);var score=new ScoreSystem(data,config);var judge=new GameJudgeSystem(data);judge.Start();
            combo.Add();Check(score.AddSuccess()==100,"First score");combo.Tick(9,false);Check(data.ComboCount==1&&data.ComboRemainingTime==2,"Spawn wait freezes combo");
            combo.Tick(.5f,true);combo.Add();Check(score.AddSuccess()==120&&data.Score==220,"Combo scoring");
            combo.Tick(2.1f,true);Check(data.ComboCount==0&&data.MaxCombo==2,"Combo expiration retains maximum");
            combo.Add();judge.Miss();combo.Reset();Check(data.MissCount==1&&data.Score==220&&data.ComboCount==0,"Miss resets only combo");
            Check(judge.Finish()&&!judge.Finish()&&data.Result==ResultType.GameOver,"Game over only once");
            judge.Start();Check(data.Score==0&&data.SuccessCount==0&&data.MissCount==0&&data.Phase==GamePhase.Playing,"Restart clears run state");
            judge.Pause(true);Check(data.Phase==GamePhase.Paused,"Pause");judge.Pause(false);judge.Title();Check(data.Phase==GamePhase.Title,"Return to title");
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
            UnityEngine.Object.DestroyImmediate(ringObject);UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(config);UnityEngine.Object.DestroyImmediate(spawn);UnityEngine.Object.DestroyImmediate(ring);
            Directory.CreateDirectory("Builds");File.WriteAllText("Builds/verification.txt",$"PASS — {checks} assertions\nGeometry, swept tip, moving / rotating ring, priority, score, combo, spawn, miss, restart, title.\n");
            Debug.Log($"LANDOLT_TESTS_PASSED: {checks}");
        }
    }
}
