using System;
using System.Collections;
using System.IO;
using UnityEngine;
using VContainer;
namespace LandoltRush
{
    // Explicit opt-in executable smoke test. Normal play never enters this coroutine.
    public sealed class SmokeTestComponent : MonoBehaviour
    {
        public GameLifetimeScope Scope;
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")<0)yield break;
            string directory=Path.Combine(Application.dataPath,"../SmokeTest");Directory.CreateDirectory(directory);
            Application.targetFrameRate=60;
            yield return new WaitForSecondsRealtime(.7f);
            yield return Capture(directory,"01-title.png");
            var presenter=Scope.Container.Resolve<GameMainPresentator>();var rod=Scope.Rod;var data=Scope.Data;
            rod.TestPosition=new Vector2(1.3f,0);Scope.Input.Emit(GameCommand.Start);yield return null;
            presenter.Spawn(StaticRing());rod.TestPosition=new Vector2(.73f,0);yield return null;yield return null;
            if(!Require(data.SuccessCount==1&&data.Score==100,"first success",directory))yield break;
            rod.TestPosition=new Vector2(1.3f,0);rod.Sample(Scope.RodParam);presenter.Spawn(StaticRing());rod.TestPosition=new Vector2(.73f,0);yield return null;yield return null;
            if(!Require(data.Score==220&&data.ComboCount==2,"second success / combo",directory))yield break;
            rod.TestPosition=new Vector2(2,-1);rod.Sample(Scope.RodParam);
            presenter.Spawn(new RingSpawnData{Position=new Vector2(-1.5f,.3f),Scale=1.2f,Angle=35,Velocity=new Vector2(.3f,-.1f),AngularVelocity=35});
            yield return Capture(directory,"02-playing.png");
            Scope.Input.Emit(GameCommand.Pause);int saved=data.Score;yield return new WaitForSecondsRealtime(.15f);
            if(!Require(data.Phase==GamePhase.Paused&&data.Score==saved,"pause",directory))yield break;
            Scope.Input.Emit(GameCommand.Pause);
            rod.TestPosition=new Vector2(6,-3);rod.Sample(Scope.RodParam);
            presenter.Spawn(new RingSpawnData{Position=new Vector2(-9.3f,2),Velocity=new Vector2(25,0),Scale=1,AngularVelocity=30});
            yield return new WaitForSecondsRealtime(.9f);
            if(!Require(data.MissCount==1&&data.Score==220&&data.ComboCount==0,"miss",directory))yield break;
            rod.TestPosition=new Vector2(-1.3f,0);rod.Sample(Scope.RodParam);presenter.Spawn(StaticRing());rod.TestPosition=new Vector2(-.73f,0);
            yield return null;yield return null;
            if(!Require(data.Phase==GamePhase.Finished&&Scope.Result.Root.activeSelf,"game over panel",directory))yield break;
            yield return Capture(directory,"03-result.png");
            Scope.Input.Emit(GameCommand.Restart);yield return null;
            if(!Require(data.Phase==GamePhase.Playing&&data.Score==0&&data.MissCount==0,"restart",directory))yield break;
            Scope.Input.Emit(GameCommand.Title);yield return null;
            if(!Require(data.Phase==GamePhase.Title&&Scope.UI.TitleRoot.activeSelf,"title",directory))yield break;
            File.WriteAllText(Path.Combine(directory,"result.txt"),"PASS: title, start, success, combo, pause, miss, game over, restart, return to title.\nScreenshots captured from the Windows player.\n");
            Application.Quit(0);
        }
        static RingSpawnData StaticRing()=>new RingSpawnData{Position=Vector2.zero,Scale=1,Angle=0};
        static bool Require(bool success,string label,string directory)
        {if(success)return true;File.WriteAllText(Path.Combine(directory,"result.txt"),"FAIL: "+label);Debug.LogError("Smoke test failed: "+label);Application.Quit(2);return false;}
        static IEnumerator Capture(string directory,string name)
        {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name));yield return new WaitForSecondsRealtime(.2f);}
    }
}
