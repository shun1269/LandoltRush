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
        static RectTransform RectObj(string name,Transform parent,Vector2 min,Vector2 max,Vector2 pos,Vector2 size)
        {
            var o=new GameObject(name,typeof(RectTransform));var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        static RectTransform Full(string name,Transform parent)=>RectObj(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        static Text Label(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=RectObj(name,parent,new Vector2(0,1),new Vector2(0,1),new Vector2(x+w/2,-y-h/2),new Vector2(w,h));
            var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        static Image Box(string name,Transform parent,float x,float y,float w,float h,Color color)
        {
            var r=RectObj(name,parent,new Vector2(0,1),new Vector2(0,1),new Vector2(x+w/2,-y-h/2),new Vector2(w,h));
            var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        static Button Button(string name,Transform parent,string label,float x,float y,float w,float h,bool primary)
        {
            var image=Box(name,parent,x,y,w,h,primary?Teal:new Color(.89f,.90f,.86f));image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var colors=button.colors;colors.highlightedColor=new Color(.85f,.96f,.91f);colors.pressedColor=new Color(.65f,.85f,.78f);button.colors=colors;
            var text=Label("Label",image.transform,label,0,0,w,h,20,primary?Color.white:Ink,TextAnchor.MiddleCenter);
            return button;
        }
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
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            var rodView=Obj("Rod View").AddComponent<RodView>();rodView.Shaft=Line("Shaft — visual only",rodView.transform,.036f,Teal,5);
            rodView.TipOutline=Line("Tip",rodView.transform,.025f,Teal,6);
            scope.Rod=Obj("Rod Controller").AddComponent<RodController>();scope.Rod.Camera=camera;scope.Rod.View=rodView;
            // A subtle coordinate field gives movement a fixed visual reference.
            var grid=Obj("Coordinate Field");
            for(int x=-8;x<=8;x++)for(int y=-4;y<=4;y++)
            {
                var dot=Line("Dot",grid.transform,.015f,new Color(.65f,.7f,.66f,.35f),0);
                dot.positionCount=2;dot.SetPosition(0,new Vector3(x-.012f,y));dot.SetPosition(1,new Vector3(x+.012f,y));
            }
            scope.Title=Obj("Title Decoration").AddComponent<TitleDecorationViewer>();scope.Title.Param=ringParam;
            var titleRing=Obj("Title Ring",scope.Title.transform);titleRing.transform.position=new Vector3(3.5f,.15f,0);titleRing.transform.localScale=Vector3.one*2.55f;
            scope.Title.Ring=titleRing.AddComponent<RingViewer>();titleRing.GetComponent<MeshRenderer>().sharedMaterial=material;
            var pointer=Line("Illustrated rod",scope.Title.transform,.035f,Teal,5);pointer.positionCount=2;
            pointer.SetPosition(0,new Vector3(10,-6));pointer.SetPosition(1,new Vector3(5.18f,-.64f));
            var canvas=Obj("GameUI").AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;canvas.gameObject.AddComponent<GraphicRaycaster>();
            scope.UI=canvas.gameObject.AddComponent<GameUIViewer>();scope.Input=canvas.gameObject.AddComponent<GameInputInfo>();
            scope.Result=canvas.gameObject.AddComponent<ResultPanelViewer>();scope.Feedback=canvas.gameObject.AddComponent<FeedbackViewer>();
            var ui=scope.UI;var input=scope.Input;
            var design=RectObj("Design Frame",canvas.transform,Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,new Vector2(1600,900));
            var title=Full("TitlePanel",design);ui.TitleRoot=title.gameObject;
            Label("Edition",title,"PRECISION ARCADE     /     001",75,56,650,32,18,Teal);
            Label("Title line 1",title,"LANDOLT",70,218,850,105,96,Ink).fontStyle=FontStyle.Bold;
            Label("Title line 2",title,"RUSH",70,317,700,118,112,Ink).fontStyle=FontStyle.Bold;
            Label("Tagline",title,"Find the gap. Keep the flow.",78,466,760,48,28,Ink);
            Label("Description",title,"Guide the tip into the opening.\nTouch the black ring and the run ends.",78,530,700,70,21,Muted);
            input.StartButton=Button("StartButton",title,"START RUN     →",80,650,320,66,true);
            Label("Start shortcut",title,"ENTER / SPACE",424,665,350,36,16,Muted);
            Label("Title footer",title,"MOUSE TO AIM     ·     ONLY THE TIP COUNTS",80,814,980,34,17,Muted);
            input.QuitButton=Button("QuitButton",title,"QUIT",1380,806,130,44,false);
            var hud=Full("HUD",design);ui.HudRoot=hud.gameObject;
            Box("Header background",hud,0,0,1600,122,new Color(Paper.r,Paper.g,Paper.b,.96f));
            Box("Header rule",hud,55,121,1490,1,new Color(.72f,.77f,.71f));
            Label("Score label",hud,"SCORE",66,25,300,25,16,Muted);
            ui.ScoreText=Label("Score",hud,"000000",63,51,400,60,48,Ink);ui.ScoreText.fontStyle=FontStyle.Bold;
            Label("Brand",hud,"LANDOLT / RUSH",570,47,460,38,24,Ink,TextAnchor.MiddleCenter);
            ui.ComboText=Label("Combo",hud,"—  COMBO",1120,33,280,36,24,Teal,TextAnchor.MiddleRight);
            var track=Box("Combo track",hud,1150,84,250,5,new Color(.8f,.83f,.77f));
            var fill=Full("Combo gauge",track.transform);ui.Gauge=fill.gameObject.AddComponent<Image>();ui.Gauge.color=Teal;ui.Gauge.raycastTarget=false;
            input.PauseButton=Button("PauseButton",hud,"II",1450,43,60,48,false);
            Box("Footer background",hud,0,827,1600,73,new Color(Paper.r,Paper.g,Paper.b,.96f));
            ui.StatusText=Label("Status",hud,"AIM FOR THE GAP",66,843,890,30,18,Teal);
            Label("Keys",hud,"ESC  PAUSE     /     R  RESTART",1070,843,480,30,16,Muted,TextAnchor.MiddleRight);
            var flash=Full("Flash",design);scope.Feedback.Flash=flash.gameObject.AddComponent<Image>();scope.Feedback.Flash.raycastTarget=false;scope.Feedback.Flash.color=Color.clear;
            scope.Feedback.Popup=Label("Feedback",design,"",500,175,600,52,28,Teal,TextAnchor.MiddleCenter);
            scope.Feedback.Burst=Line("Success pulse",null,.022f,Teal,7);
            var result=Full("ResultPanel",design);scope.Result.Root=result.gameObject;
            var shade=result.gameObject.AddComponent<Image>();shade.color=new Color(Paper.r,Paper.g,Paper.b,.95f);
            Label("Result eyebrow",result,"END OF RUN",500,180,600,36,18,Teal,TextAnchor.MiddleCenter);
            Label("Game over",result,"GAME OVER",250,230,1100,112,88,Ink,TextAnchor.MiddleCenter).fontStyle=FontStyle.Bold;
            Label("Contact hint",result,"The tip touched the black ring. Try another angle.",350,353,900,44,23,Muted,TextAnchor.MiddleCenter);
            scope.Result.Score=Label("Final score",result,"000000",400,430,800,100,76,Teal,TextAnchor.MiddleCenter);
            scope.Result.Stats=Label("Stats",result,"",200,551,1200,45,20,Ink,TextAnchor.MiddleCenter);
            input.RestartButton=Button("RestartButton",result,"TRY AGAIN     [R]",475,658,320,68,true);
            input.TitleButton=Button("TitleButton",result,"BACK TO TITLE",815,658,310,68,false);
            var pause=Full("PausePanel",design);ui.PauseRoot=pause.gameObject;var pauseShade=pause.gameObject.AddComponent<Image>();pauseShade.color=new Color(Paper.r,Paper.g,Paper.b,.96f);
            Label("Paused",pause,"TAKE A BREATH",200,310,1200,95,72,Ink,TextAnchor.MiddleCenter).fontStyle=FontStyle.Bold;
            Label("Pause help",pause,"Your run is paused. Move the mouse into position, then resume.",250,434,1100,40,22,Muted,TextAnchor.MiddleCenter);
            input.ResumeButton=Button("ResumeButton",pause,"RESUME     [ESC]",620,542,360,68,true);
            input.MuteButton=Button("MuteButton",design,"SOUND ON  [M]",1280,15,230,28,false);
            ui.MuteText=input.MuteButton.GetComponentInChildren<Text>();ui.MuteText.fontSize=13;
            ui.HudRoot.SetActive(false);ui.PauseRoot.SetActive(false);scope.Result.Root.SetActive(false);rodView.gameObject.SetActive(false);
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
