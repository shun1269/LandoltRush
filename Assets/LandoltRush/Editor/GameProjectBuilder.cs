using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LandoltRush.Editor
{
    public static class GameProjectBuilder
    {
        const string Root="Assets/LandoltRush";
        static readonly Color Paper=new Color(.949f,.945f,.918f),Ink=new Color(.055f,.075f,.08f),Teal=new Color(0,.51f,.44f),Muted=new Color(.41f,.45f,.43f);
        static Font font;static Material material;
        static GameObject Obj(string name,Transform parent=null){var o=new GameObject(name);if(parent)o.transform.SetParent(parent,false);return o;}
        static T Param<T>(string name)where T:ScriptableObject
        {
            string path=$"{Root}/Settings/{name}.asset";var p=AssetDatabase.LoadAssetAtPath<T>(path);
            if(!p){p=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(p,path);}return p;
        }
        static LineRenderer Line(string name,Transform parent,float width,Color color,int order)
        {
            var line=Obj(name,parent).AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;
            line.widthMultiplier=width;line.startColor=line.endColor=color;line.sortingOrder=order;line.numCapVertices=6;return line;
        }

        [MenuItem("LANDOLT RUSH/Create or update game scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory(Root+"/Settings");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Materials");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            font=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/ZenKakuGothicNew-Regular.ttf");
            material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ink.mat");
            if(!material){material=new Material(Shader.Find("LandoltRush/Ink"));AssetDatabase.CreateAsset(material,Root+"/Materials/Ink.mat");}
            var config=Param<GameConfig>("GameConfig");var rodParam=Param<RodParam>("RodParam");var ringParam=Param<RingParam>("RingParam");var spawnParam=Param<SpawnParam>("SpawnParam");
            var camera=Obj("Game Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
            camera.orthographic=true;camera.orthographicSize=4.5f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Paper;
            camera.gameObject.AddComponent<AudioListener>();
            camera.gameObject.AddComponent<ViewportComponent>();
            var eventSystem=Obj("EventSystem");eventSystem.AddComponent<EventSystem>();eventSystem.AddComponent<InputSystemUIInputModule>();
            var scope=Obj("GameLifetimeScope").AddComponent<GameLifetimeScope>();
            scope.Config=config;scope.RodParam=rodParam;scope.RingParam=ringParam;scope.SpawnParam=spawnParam;
            scope.Data=Obj("GameData").AddComponent<GameData>();
            scope.Spawner=Obj("Ring Spawner").AddComponent<LandoltRingSpawner>();
            scope.Collision=Obj("Tip Collision").AddComponent<RodTipCollision>();
            scope.Sound=Obj("Sound").AddComponent<SoundComponent>();
            scope.gameObject.AddComponent<SmokeTestComponent>().Scope=scope;
            var prototype=Obj("LandoltRing");var component=prototype.AddComponent<LandoltRingComponent>();component.Viewer=prototype.AddComponent<RingViewer>();
            prototype.GetComponent<MeshRenderer>().sharedMaterial=material;prototype.GetComponent<MeshRenderer>().sortingOrder=3;
            ringParam.Prefab=PrefabUtility.SaveAsPrefabAsset(prototype,Root+"/Prefabs/LandoltRing.prefab").GetComponent<LandoltRingComponent>();
            UnityEngine.Object.DestroyImmediate(prototype);EditorUtility.SetDirty(ringParam);
            var rodView=Obj("Rod View").AddComponent<RodView>();rodView.Shaft=Line("Shaft",rodView.transform,.036f,Teal,5);
            rodView.TipOutline=Line("Tip",rodView.transform,.025f,Teal,6);
            scope.Rod=Obj("Rod Controller").AddComponent<RodController>();scope.Rod.Camera=camera;scope.Rod.View=rodView;
            scope.Title=Obj("Title Decoration").AddComponent<TitleDecorationViewer>();scope.Title.Param=ringParam;
            var titleRing=Obj("Title Ring",scope.Title.transform);titleRing.transform.position=new Vector3(0,1.9f,0);titleRing.transform.localScale=Vector3.one*.9f;
            scope.Title.Ring=titleRing.AddComponent<RingViewer>();titleRing.GetComponent<MeshRenderer>().sharedMaterial=material;
            GameUIBuilder.Build(scope,camera,font);
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/GameScene.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity",true)};
            PlayerSettings.productName="LANDOLT RUSH";PlayerSettings.companyName="Landolt Rush";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            AssetDatabase.SaveAssets();Debug.Log("LANDOLT RUSH scene created.");
        }
        public static void PrepareAndVerify()
        {
            try{CreateScene();GameVerification.Run();EditorApplication.Exit(0);}
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void BuildEverything()
        {
            try
            {
                CreateScene();GameVerification.Run();
                Directory.CreateDirectory("Builds/Windows");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/GameScene.unity"},locationPathName="Builds/Windows/LANDOLT RUSH.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
                if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
                Debug.Log("LANDOLT_BUILD_SUCCESS");EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
