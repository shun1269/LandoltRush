using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
namespace LandoltRush.Editor
{
    public static class GamePreviewRenderer
    {
        public static void PrepareVerifyAndRender()
        {
            try
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
                GameVerification.Run();
                GameProjectBuilder.CreateScene();GameVerification.Run();RenderAll();
                Debug.Log("LANDOLT_UI_PREVIEWS_READY");EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void RenderAll()
        {
            GameLifetimeScope scope=null;
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if(root.TryGetComponent<GameLifetimeScope>(out var found)){scope=found;break;}
            if(!scope)throw new InvalidOperationException("No game scope in scene.");
            var camera=scope.Rod.Camera;camera.aspect=16f/9f;camera.rect=new Rect(0,0,1,1);
            var canvas=scope.UI.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.enabled=false;canvas.scaleFactor=1;
            var data=scope.Data;scope.Title.Ring.Build(scope.RingParam);scope.Title.Ring.transform.rotation=Quaternion.Euler(0,0,-25);
            scope.Feedback.Clear();data.Phase=GamePhase.Title;scope.UI.Refresh(data,scope.Config);scope.UI.Sound(false);
            Capture(camera,canvas,"01-title");
            scope.Title.gameObject.SetActive(false);data.Phase=GamePhase.Playing;data.Lives=3;data.Score=1680;data.ComboCount=12;data.MaxCombo=12;data.ComboRemainingTime=4.2f;
            scope.UI.Refresh(data,scope.Config);scope.UI.Advance(1);
            var captured=scope.Spawner.Spawn(new RingSpawnData{Position=new Vector2(2,.3f),Scale=scope.SpawnParam.MinScale,Angle=-30},scope.RingParam);
            scope.Spawner.Spawn(new RingSpawnData{Position=new Vector2(-3,1.5f),Scale=scope.SpawnParam.MaxScale,Angle=-75},scope.RingParam);
            scope.Rod.TestPosition=new Vector2(2.8f,-.2f);scope.Rod.SetActive(true,scope.RodParam);
            Capture(camera,canvas,"02-playing");
            data.ComboCount=13;scope.UI.Refresh(data,scope.Config);
            Directory.CreateDirectory("Builds/UIPreviews/ComboPulse");
            for(int i=0;i<=14;i++)
            {
                if(i>0)scope.UI.Advance(.02f);
                scope.UI.Refresh(data,scope.Config);
                Capture(camera,canvas,"ComboPulse/frame-"+i.ToString("D2"));
            }
            data.ComboCount=12;scope.UI.Refresh(data,scope.Config);scope.UI.Advance(1);
            scope.Feedback.Success(captured.Position,captured.Angle,captured.Scale,scope.Rod.Bounds);scope.Spawner.Remove(captured);
            Directory.CreateDirectory("Builds/UIPreviews/SuccessDive");
            for(int i=0;i<=30;i++)
            {
                if(i>0)scope.Feedback.Advance(.02f);
                Capture(camera,canvas,"SuccessDive/frame-"+i.ToString("D2"));
            }
            scope.Feedback.Clear();
            scope.Spawner.Spawn(new RingSpawnData{Position=new Vector2(2,.3f),Scale=scope.SpawnParam.MinScale,Angle=-30},scope.RingParam);
            data.Lives=2;data.ComboCount=0;data.ComboRemainingTime=0;scope.UI.Refresh(data,scope.Config);scope.Feedback.Damage();scope.Feedback.Advance(.04f);
            Capture(camera,canvas,"03-miss");
            scope.Feedback.Clear();data.Phase=GamePhase.Finished;scope.UI.Refresh(data,scope.Config);scope.Result.Show(data);
            Capture(camera,canvas,"04-result");
        }
        static void Capture(Camera camera,Canvas canvas,string name)
        {
            Directory.CreateDirectory("Builds/UIPreviews");
            var target=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
            Canvas.ForceUpdateCanvases();
            foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))graphic.SetAllDirty();
            Canvas.ForceUpdateCanvases();
            if(GraphicsSettings.currentRenderPipeline!=null)
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            else camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
            File.WriteAllBytes("Builds/UIPreviews/"+name+".png",texture.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(texture);target.Release();UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
