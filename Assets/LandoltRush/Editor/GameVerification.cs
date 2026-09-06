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
            var combo=new ComboSystem(data,config);var score=new ScoreSystem(data,config);var judge=new GameJudgeSystem(data,config);judge.Start();
            Check(data.Lives==3,"Start with three lives");
            combo.Add();Check(score.AddSuccess()==100,"First score");combo.Tick(9,false);Check(data.ComboCount==1&&data.ComboRemainingTime==6,"Empty field freezes six-second combo");
            combo.Tick(.5f,true);combo.Add();Check(score.AddSuccess()==120&&data.Score==220,"Combo scoring");
            combo.Tick(2.1f,true);Check(data.ComboCount==2,"Combo survives more than two seconds");
            combo.Tick(4f,true);Check(data.ComboCount==0&&data.MaxCombo==2,"Combo expiration retains maximum");
            combo.Add();judge.Miss();combo.Reset();Check(data.MissCount==1&&data.Score==220&&data.ComboCount==0,"Miss resets only combo");
            Check(data.Lives==2&&data.Phase==GamePhase.Playing,"First miss costs one life");
            judge.Miss();Check(data.Lives==1&&data.Phase==GamePhase.Playing,"Second miss keeps game running");
            judge.Miss();Check(data.Lives==0&&data.Phase==GamePhase.Finished&&data.Result==ResultType.GameOver,"Third miss finishes game");
            Check(!judge.Miss()&&data.Lives==0&&data.MissCount==3,"No damage after game ends");
            judge.Start();Check(data.Lives==3&&data.Score==0&&data.SuccessCount==0&&data.MissCount==0&&data.Phase==GamePhase.Playing,"Restart restores lives and clears run state");
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
            Check(Mathf.Approximately(RingSpawnSystem.VisibleTravelTime(bounds,new Vector2(-10,0),new Vector2(2,0),1),7),"Horizontal fully visible travel time");
            Check(Mathf.Approximately(RingSpawnSystem.VisibleTravelTime(bounds,new Vector2(0,6),new Vector2(0,-2),1),3.5f),"Vertical fully visible travel time");
            Check(Mathf.Approximately(RingSpawnSystem.VisibleTravelTime(new Rect(-5,-5,10,10),new Vector2(-6,-6),new Vector2(2,2),1),4),"Diagonal fully visible travel time");
            Check(RingSpawnSystem.VisibleTravelTime(bounds,Vector2.zero,Vector2.zero,1)==0,"Stationary travel has no finite passage");
            Check(spawn.MinVisibleRotations==2&&spawn.MinRotateSpeed==180&&spawn.MaxRotateSpeed==240,"Two visible turns with baseline rotation 180-240");
            for(int i=0;i<500;i++)
            {
                var d=system.Create(bounds);if(d.Side==SpawnSide.Top)top++;else left++;
                Check(bounds.Contains(d.Target),"Target inside screen");
                Check(d.Side==SpawnSide.Top?d.Position.y-ring.OuterRadius*d.Scale>bounds.yMax:d.Position.x+ring.OuterRadius*d.Scale<bounds.xMin,"Fully offscreen spawn");
                Check(Vector2.Dot(d.Target-d.Position,d.Velocity)>0,"Moves towards target");
                Check(d.Scale>=1.2f&&d.Scale<=1.6f&&Mathf.Abs(d.AngularVelocity)>=180,"Size and minimum rotation unchanged");
                float required=720f/RingSpawnSystem.VisibleTravelTime(bounds,d.Position,d.Velocity,ring.OuterRadius*d.Scale);
                Check(Mathf.Abs(d.AngularVelocity)<=Mathf.Max(240,required)+.001f,"Only two-turn requirement can exceed 240 degrees per second");
                VerifyVisibleRotations(d,bounds,ring.OuterRadius*d.Scale);
                Check(d.Velocity.magnitude>=1.4999f&&d.Velocity.magnitude<=3.0001f,"Movement speed remains unchanged");
            }
            Check(top>150&&left>150,"Both spawn sides");
            spawn.MinVisibleRotations=0;spawn.MinRotateSpeed=30;spawn.MaxRotateSpeed=30;
            Check(Mathf.Approximately(Mathf.Abs(system.Create(bounds).AngularVelocity),30),"Zero minimum turns disables travel correction");
            var ringObject=new GameObject("Entry verification");var comp=ringObject.AddComponent<LandoltRingComponent>();comp.Viewer=ringObject.AddComponent<RingViewer>();
            comp.Initialize(new RingSpawnData{Position=new Vector2(-10,0),Velocity=new Vector2(4,0),Scale=1},ring);
            comp.CheckExit(bounds);Check(!comp.Entered&&!comp.Resolved,"No premature miss at spawn");comp.Advance(1);comp.CheckExit(bounds);Check(comp.Entered&&!comp.Resolved,"Entry state");
            comp.Advance(5);comp.CheckExit(bounds);Check(comp.Resolved,"Miss after fully exiting");
            VerifyMultipleRings();
            UnityEngine.Object.DestroyImmediate(ringObject);UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(config);UnityEngine.Object.DestroyImmediate(spawn);UnityEngine.Object.DestroyImmediate(ring);
            Directory.CreateDirectory("Builds");File.WriteAllText("Builds/verification.txt",$"PASS — {checks} assertions\nConstant title rotation; immediate combo enlargement, eased return, rapid retrigger and reset cleanup.\nMinimum scale 1.2, at least two fully visible turns across 500 sampled trajectories, baseline rotation 180-240 degrees per second, travel correction and unchanged movement speeds.\nSuccess dive: actual capture, shape/orientation, screen-filling expansion, fade, overlapping captures, bounded reuse, expiry and restart/title cleanup.\nThree lives, contact/escape damage, duplicate prevention, final-life transition, restart, Japanese UI/glyphs, hearts, combo background, swept collisions and timed multi-ring spawns.\n");
            Debug.Log($"LANDOLT_TESTS_PASSED: {checks}");
        }
        static void VerifyVisibleRotations(RingSpawnData data,Rect bounds,float radius)
        {
            // Independently sample the path instead of reusing the production clipping formula.
            const float step=1f/240f;int visibleSteps=0;
            for(int i=0;i<30*240;i++)
            {
                Vector2 p=data.Position+data.Velocity*(i*step);
                if(p.x-radius>=bounds.xMin&&p.x+radius<=bounds.xMax&&p.y-radius>=bounds.yMin&&p.y+radius<=bounds.yMax)visibleSteps++;
            }
            Check(visibleSteps>0,"Generated ring fully enters viewport");
            Check((visibleSteps+2)*step*Mathf.Abs(data.AngularVelocity)>=720f,"At least two fully visible turns (two sample tolerance)");
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
                // Damage contacts are emitted before successful entries on other rings.
                var dangerous=spawner.Spawn(new RingSpawnData{Position=direction*1.7f,Scale=1,Angle=angle},scope.RingParam);
                scope.Collision.Check(rod,spawner.Rings,scope.RodParam,scope.RingParam);
                Check(events.Count==2&&events[0].Kind==HitKind.Black&&events[0].Ring==dangerous&&events[1].Kind==HitKind.Gap,"Damage on second ring precedes first-ring success");
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
            VerifyLivesAndUI(scope);
            VerifySuccessDive(scope);
            VerifyTitleAndComboMotion(scope);
        }
        static void VerifyTitleAndComboMotion(GameLifetimeScope scope)
        {
            var title=scope.Title;var ring=title.Ring.transform;
            ring.rotation=Quaternion.Euler(0,0,-25);
            for(int i=0;i<4;i++)
            {
                float before=ring.eulerAngles.z;title.Advance(.5f);
                Check(Mathf.Abs(Mathf.DeltaAngle(before,ring.eulerAngles.z)+15)<.001f,"Title rotates clockwise at a constant 30 degrees per second");
            }
            float angle=ring.eulerAngles.z;title.Advance(12);
            Check(Mathf.Abs(Mathf.DeltaAngle(angle,ring.eulerAngles.z))<.001f,"Title completes a full turn without reversing");
            var data=scope.Data;var ui=scope.UI;var config=scope.Config;
            data.Phase=GamePhase.Playing;data.ComboCount=0;ui.Refresh(data,config);
            data.ComboCount=1;ui.Refresh(data,config);
            Check(ui.ComboText.text=="1"&&ui.ComboText.rectTransform.localScale.x>1.2f,"New combo number immediately pops larger");
            float peak=ui.ComboText.rectTransform.localScale.x;ui.Advance(.08f);
            float shrinking=ui.ComboText.rectTransform.localScale.x;
            Check(shrinking>1&&shrinking<peak,"Combo pulse quickly eases towards normal size");
            ui.Refresh(data,config);
            Check(Mathf.Approximately(ui.ComboText.rectTransform.localScale.x,shrinking),"Unchanged combo does not retrigger pulse each frame");
            data.ComboCount=2;ui.Refresh(data,config);
            Check(Mathf.Approximately(ui.ComboText.rectTransform.localScale.x,peak),"Rapid next combo retriggers full pulse without accumulating scale");
            ui.Advance(.3f);Check(ui.ComboText.rectTransform.localScale==Vector3.one,"Combo settles back to original size");
            data.ComboCount=3;ui.Refresh(data,config);data.ComboCount=0;ui.Refresh(data,config);
            Check(ui.ComboText.rectTransform.localScale==Vector3.one,"Miss clears pulse immediately");
            data.ComboCount=4;ui.Refresh(data,config);data.Phase=GamePhase.Title;ui.Refresh(data,config);
            Check(ui.ComboText.rectTransform.localScale==Vector3.one,"Returning to title clears pulse");
            data.ComboCount=0;ui.Refresh(data,config);
        }
        static void VerifySuccessDive(GameLifetimeScope scope)
        {
            var data=scope.Data;var config=scope.Config;var rod=scope.Rod;var dive=scope.Feedback.Dive;
            Check(dive!=null&&dive.Param==scope.RingParam&&dive.Material!=null,"Success dive scene references");
            using(var p=new GameMainPresentator(data,config,scope.RodParam,scope.RingParam,rod,scope.Collision,scope.Spawner,
                scope.UI,scope.Result,scope.Input,scope.Feedback,scope.Sound,new RingSpawnSystem(scope.SpawnParam,scope.RingParam),
                new ComboSystem(data,config),new ScoreSystem(data,config),new GameJudgeSystem(data,config),scope.Title,new SpawnTempoSystem(data,config)))
            {
                p.Start();scope.Input.Emit(GameCommand.Start);
                Vector2 origin=new Vector2(2,.3f),direction=(rod.Pivot-origin).normalized;
                float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
                rod.TestPosition=origin+direction*2;rod.SetActive(true,scope.RodParam);
                p.Spawn(new RingSpawnData{Position=origin,Scale=1.2f,Angle=angle});
                rod.TestPosition=origin+direction*.5f;p.Step(0);
                Check(data.Score==100&&data.Lives==3&&scope.Spawner.Rings.Count==0,"Successful capture scores and removes gameplay ring");
                Check(dive.ActiveCount==1,"Actual success starts a dive");
                var copy=dive.GetComponentInChildren<RingViewer>();
                Check((Vector2)copy.transform.position==origin&&Mathf.Approximately(copy.transform.localScale.x,1.2f)&&Mathf.Abs(Mathf.DeltaAngle(copy.transform.eulerAngles.z,angle))<.01f,"Dive inherits position, size and gap orientation");
                Check(copy.GetComponent<MeshFilter>().sharedMesh.vertexCount==(RingGeometry.Segments+1)*2,"Dive preserves C-shaped ring mesh");
                scope.Feedback.Advance(.3f);
                Check(copy.transform.localScale.x>3&&(Vector2)copy.transform.position==Vector2.Lerp(origin,rod.Bounds.center,.5f),"Dive expands towards screen centre");
                var tint=new MaterialPropertyBlock();copy.GetComponent<MeshRenderer>().GetPropertyBlock(tint);
                Check(tint.GetColor("_Color").a>0&&tint.GetColor("_Color").a<1,"Expanded ring fades instead of blocking view");
                scope.Feedback.Success(new Vector2(-3,1),90,1.6f,rod.Bounds);
                Check(dive.ActiveCount==2,"Consecutive successes overlap without cancelling");
                Check(dive.GetComponentsInChildren<Collider2D>(true).Length==0&&scope.Spawner.Rings.Count==0,"Visual copies have no gameplay collision");
                scope.Feedback.Advance(.29f);
                float inner=scope.RingParam.InnerRadius*copy.transform.localScale.x;
                Check(inner>rod.Bounds.size.magnitude*.5f,"Inner opening passes all viewport corners");
                scope.Feedback.Advance(.4f);Check(dive.ActiveCount==0,"Success visuals expire");
                for(int i=0;i<32;i++)scope.Feedback.Success(origin,angle,1.2f,rod.Bounds);
                Check(dive.ActiveCount==16&&dive.transform.childCount==16,"Rapid combos reuse a bounded effect pool");
                scope.Input.Emit(GameCommand.Restart);Check(dive.ActiveCount==0,"Restart clears all success visuals");
                scope.Feedback.Success(origin,angle,1.2f,rod.Bounds);scope.Input.Emit(GameCommand.Title);
                Check(dive.ActiveCount==0,"Returning to title clears success visuals");rod.TestPosition=null;
            }
        }
        static void VerifySpawnSchedule(GameLifetimeScope scope)
        {
            var data=scope.Data;var config=scope.Config;var rod=scope.Rod;var spawner=scope.Spawner;
            var combo=new ComboSystem(data,config);var tempo=new SpawnTempoSystem(data,config);
            using(var presenter=new GameMainPresentator(data,config,scope.RodParam,scope.RingParam,rod,scope.Collision,spawner,
                scope.UI,scope.Result,scope.Input,scope.Feedback,scope.Sound,new RingSpawnSystem(scope.SpawnParam,scope.RingParam),
                combo,new ScoreSystem(data,config),new GameJudgeSystem(data,config),scope.Title,tempo))
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
        static void VerifyLivesAndUI(GameLifetimeScope scope)
        {
            var d=scope.Data;var c=scope.Config;var rod=scope.Rod;var spawner=scope.Spawner;
            using(var p=new GameMainPresentator(d,c,scope.RodParam,scope.RingParam,rod,scope.Collision,spawner,
                scope.UI,scope.Result,scope.Input,scope.Feedback,scope.Sound,new RingSpawnSystem(scope.SpawnParam,scope.RingParam),
                new ComboSystem(d,c),new ScoreSystem(d,c),new GameJudgeSystem(d,c),scope.Title,new SpawnTempoSystem(d,c)))
            {
                p.Start();rod.TestPosition=new Vector2(6,-3);scope.Input.Emit(GameCommand.Start);
                Check(scope.UI.Hearts.Length==3&&scope.UI.Hearts[2].Filled,"Three filled hearts at start");
                p.Spawn(new RingSpawnData{Position=Vector2.Lerp(rod.Tip,rod.Pivot,.3f),Scale=1});p.Step(0);
                Check(d.Lives==2&&d.MissCount==1&&d.Phase==GamePhase.Playing,"Shaft contact damages once");
                Check(!scope.UI.Hearts[2].Filled&&scope.UI.Hearts[1].Filled,"Heart removed on damage");
                Check(scope.Feedback.Popup.text=="ミス"&&scope.Feedback.Border.color.a>0,"Japanese miss and red border");
                p.Step(0);Check(d.Lives==2&&d.MissCount==1,"Resolved contact cannot damage twice");
                scope.Feedback.Advance(.7f);Check(scope.Feedback.Border.color.a==0,"Damage border fades away");
                rod.TestPosition=new Vector2(-1.3f,0);rod.SetActive(true,scope.RodParam);
                p.Spawn(new RingSpawnData{Position=Vector2.zero,Scale=1});rod.TestPosition=new Vector2(-.73f,0);p.Step(0);
                Check(d.Lives==1&&d.Phase==GamePhase.Playing,"Tip contact now loses a life instead of ending immediately");
                d.Score=480;d.MaxCombo=4;
                var bounds=rod.Bounds;
                p.Spawn(new RingSpawnData{Position=new Vector2(bounds.xMin+1,bounds.yMax-1.5f),Velocity=new Vector2((bounds.width+4)*5,0),Scale=1});p.Step(.001f);p.Step(.2f);
                Check(d.Lives==0&&d.Phase==GamePhase.Finished&&d.MissCount==3,$"Escaped ring consumes last life (lives {d.Lives}, misses {d.MissCount}, phase {d.Phase})");
                Check(!scope.Result.Root.activeSelf&&!rod.CanMove,"Last hit freezes play before result transition");
                p.Step(.61f);Check(scope.Result.Root.activeSelf&&scope.Result.Score.text=="480"&&scope.Result.MaxCombo.text=="4","Result shows score and max combo");
                scope.Input.Emit(GameCommand.Restart);
                Check(d.Lives==3&&scope.UI.Hearts[2].Filled&&!scope.Result.Root.activeSelf&&scope.Feedback.Border.color.a==0,"Restart resets lives, result and damage feedback");
                rod.TestPosition=new Vector2(6,-3);rod.SetActive(true,scope.RodParam);
                for(int i=1;i<=3;i++)p.Spawn(new RingSpawnData{Position=Vector2.Lerp(rod.Tip,rod.Pivot,i*.2f),Scale=1});
                p.Step(0);Check(d.Lives==0&&d.MissCount==3,"Three simultaneous misses consume exactly three lives");
                p.Step(1);Check(d.Lives==0&&d.MissCount==3,"Finished session ignores further damage");
                scope.Input.Emit(GameCommand.Title);rod.TestPosition=null;
            }
            foreach(var text in scope.UI.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                Check(text.text=="Landolt Rush"||!System.Text.RegularExpressions.Regex.IsMatch(text.text,"[A-Za-z]"),"All non-title UI text is Japanese");
                if(text.text=="Landolt Rush")continue;
                text.font.RequestCharactersInTexture(text.text,text.fontSize,text.fontStyle);
                foreach(char ch in text.text)if(ch>127)Check(text.font.HasCharacter(ch),"Japanese glyph exists: "+ch);
            }
            foreach(var graphic in scope.UI.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                Check(graphic.GetComponent<CanvasRenderer>()!=null,"UI graphic has a renderer: "+graphic.name);
            Check(scope.UI.ComboText.fontSize>=200&&scope.UI.ComboRoot.GetComponent<Canvas>().renderMode==RenderMode.WorldSpace,"Large combo rendered behind rings");
        }
    }
}
