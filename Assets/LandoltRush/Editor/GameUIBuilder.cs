using UnityEngine;
using UnityEngine.UI;

namespace LandoltRush.Editor
{
    public static class GameUIBuilder
    {
        static readonly Color Paper=new Color(.949f,.945f,.918f),Ink=new Color(.09f,.12f,.105f),Muted=new Color(.45f,.48f,.43f);
        static Font font;
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.anchoredPosition=new Vector2(x+w/2,-y-h/2);r.sizeDelta=new Vector2(w,h);return r;
        }
        static RectTransform Full(string name,Transform parent)
        {
            var r=Rect(name,parent,0,0,0,0);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.anchoredPosition=Vector2.zero;return r;
        }
        static RectTransform Frame(Transform parent)
        {
            var r=Rect("Design Frame",parent,0,0,1600,900);r.anchorMin=r.anchorMax=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;return r;
        }
        static Text Text(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color color,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            var t=Rect(name,parent,x,y,w,h).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color;
            t.alignment=alignment;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        static Button Button(Transform parent,string name,string label,float x,float y,float w,float h,bool primary)
        {
            var r=Rect(name,parent,x,y,w,h);var panel=r.gameObject.AddComponent<RoundedPanelGraphic>();panel.color=primary?Ink:new Color(.88f,.885f,.85f,.65f);
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=panel;
            var c=b.colors;c.highlightedColor=new Color(.84f,.94f,.87f);c.pressedColor=new Color(.68f,.8f,.72f);b.colors=c;
            // Keyboard shortcuts are handled centrally by GameInputInfo, avoiding a
            // second submit on a previously selected button when pressing Space.
            var navigation=b.navigation;navigation.mode=Navigation.Mode.None;b.navigation=navigation;
            Text("Label",r,label,0,0,w,h,23,primary?Paper:Ink);return b;
        }
        static void Backdrop(Transform parent)
        {var image=Full("Paper",parent).gameObject.AddComponent<Image>();image.color=Paper;image.raycastTarget=true;image.transform.SetAsFirstSibling();}
        static Canvas Canvas(string name,RenderMode mode)
        {
            var canvas=new GameObject(name,typeof(RectTransform)).AddComponent<Canvas>();canvas.renderMode=mode;return canvas;
        }
        public static void Build(GameLifetimeScope scope,Camera camera,Font japaneseFont)
        {
            font=japaneseFont;
            if(!font)throw new System.InvalidOperationException("Japanese font asset is missing.");
            var canvas=Canvas("GameUI",RenderMode.ScreenSpaceOverlay);
            canvas.sortingOrder=100;
            var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            canvas.gameObject.AddComponent<GraphicRaycaster>();var frame=Frame(canvas.transform);
            scope.UI=canvas.gameObject.AddComponent<GameUIViewer>();scope.Input=canvas.gameObject.AddComponent<GameInputInfo>();
            scope.Result=canvas.gameObject.AddComponent<ResultPanelViewer>();scope.Feedback=canvas.gameObject.AddComponent<FeedbackViewer>();
            var ui=scope.UI;var input=scope.Input;

            // This canvas sits physically behind the rings, not over the playfield.
            var background=Canvas("Combo Background",RenderMode.WorldSpace);background.worldCamera=camera;background.sortingOrder=-20;
            var br=background.GetComponent<RectTransform>();br.sizeDelta=new Vector2(1600,900);br.position=new Vector3(0,0,2);br.localScale=Vector3.one*.01f;
            ui.ComboRoot=background.gameObject;
            Text("Combo label",br,"コンボ",450,298,700,40,22,new Color(.2f,.35f,.28f,.35f));
            ui.ComboText=Text("Combo number",br,"0",200,320,1200,250,260,new Color(.12f,.18f,.15f,.085f));
            var track=Rect("Combo track",br,630,605,340,6);var image=track.gameObject.AddComponent<Image>();image.color=new Color(.2f,.35f,.28f,.09f);image.raycastTarget=false;
            ui.Gauge=Full("Combo gauge",track).gameObject.AddComponent<Image>();ui.Gauge.color=new Color(.15f,.47f,.35f,.4f);ui.Gauge.raycastTarget=false;

            var title=Full("TitlePanel",frame);ui.TitleRoot=title.gameObject;
            var wordmark=Text("Title",title,"Landolt Rush",260,382,1080,130,86,Ink);
            wordmark.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            input.StartButton=Button(title,"StartButton","はじめる",680,590,240,66,true);
            input.QuitButton=Button(title,"QuitButton","おわる",710,682,180,48,false);

            var hud=Full("HUD",frame);ui.HudRoot=hud.gameObject;
            Text("Score label",hud,"スコア",74,50,250,36,20,Muted,TextAnchor.MiddleLeft);
            ui.ScoreText=Text("Score",hud,"0",71,85,400,72,48,Ink,TextAnchor.MiddleLeft);
            ui.Hearts=new HeartGraphic[3];
            for(int i=0;i<3;i++)
            {
                ui.Hearts[i]=Rect("Life "+(i+1),hud,1195+i*52,65,34,32).gameObject.AddComponent<HeartGraphic>();
                ui.Hearts[i].color=new Color(.76f,.35f,.32f);ui.Hearts[i].raycastTarget=false;
            }
            input.PauseButton=Button(hud,"PauseButton","一時停止",1390,56,144,48,false);
            input.PauseButton.GetComponentInChildren<Text>().fontSize=18;

            var result=Full("ResultPanel",frame);scope.Result.Root=result.gameObject;Backdrop(result);
            Text("Score label",result,"スコア",400,220,800,48,24,Muted);
            scope.Result.Score=Text("Final score",result,"0",200,272,1200,142,108,Ink);
            Text("Max combo label",result,"最大コンボ",400,452,800,40,22,Muted);
            scope.Result.MaxCombo=Text("Max combo",result,"0",400,493,800,88,62,Ink);
            input.RestartButton=Button(result,"RestartButton","もう一度",535,665,250,66,true);
            input.TitleButton=Button(result,"TitleButton","タイトルへ",815,665,250,66,false);

            var pause=Full("PausePanel",frame);ui.PauseRoot=pause.gameObject;Backdrop(pause);
            Text("Paused",pause,"ひとやすみ",300,332,1000,90,46,Ink);
            input.ResumeButton=Button(pause,"ResumeButton","つづける",670,497,260,66,true);
            input.MuteButton=Button(frame,"MuteButton","音あり",1400,810,134,44,false);
            ui.MuteText=input.MuteButton.GetComponentInChildren<Text>();ui.MuteText.fontSize=18;

            // Damage stays above the panels so a final hit is visible during transition.
            scope.Feedback.Border=Full("Damage border",frame).gameObject.AddComponent<DamageBorderGraphic>();
            scope.Feedback.Border.raycastTarget=false;scope.Feedback.Border.color=Color.clear;
            scope.Feedback.Popup=Text("Miss",frame,"",500,675,600,54,32,new Color(.83f,.17f,.15f,0));
            scope.Feedback.Dive=new GameObject("Success dive").AddComponent<RingDiveViewer>();
            scope.Feedback.Dive.Param=scope.RingParam;scope.Feedback.Dive.Material=scope.Rod.View.Shaft.sharedMaterial;
            ui.HudRoot.SetActive(false);ui.ComboRoot.SetActive(false);ui.PauseRoot.SetActive(false);scope.Result.Root.SetActive(false);scope.Rod.View.gameObject.SetActive(false);
        }
    }
}
