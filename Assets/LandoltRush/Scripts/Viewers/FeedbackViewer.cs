using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class FeedbackViewer : MonoBehaviour
    {
        public Text Popup;
        public DamageBorderGraphic Border;
        public LineRenderer Burst;
        float successAge=2,missAge=2;Vector2 origin;
        static readonly Color Teal=new Color(0,.51f,.44f),Red=new Color(.83f,.17f,.15f);
        public void Success(Vector2 position){origin=position;successAge=0;Advance(0);}
        public void Damage(){missAge=0;Popup.text="ミス";Advance(0);}
        public void Clear(){successAge=missAge=2;Popup.text="";Border.color=Color.clear;Burst.positionCount=0;}
        void Update()=>Advance(Time.unscaledDeltaTime);
        public void Advance(float delta)
        {
            successAge+=delta;missAge+=delta;float a=Mathf.Clamp01(1-successAge/.55f),damage=Mathf.Clamp01(1-missAge/.55f);
            Popup.color=new Color(Red.r,Red.g,Red.b,Mathf.Clamp01(1-missAge/.7f));
            Border.color=new Color(Red.r,Red.g,Red.b,.65f*damage*damage);
            Burst.positionCount=a>0?64:0;Burst.loop=true;
            Burst.startColor=Burst.endColor=new Color(Teal.r,Teal.g,Teal.b,a*.5f);
            for(int i=0;i<Burst.positionCount;i++)Burst.SetPosition(i,origin+RingGeometry.Polar(.2f+successAge*2.2f,i*360f/64));
        }
    }
}
