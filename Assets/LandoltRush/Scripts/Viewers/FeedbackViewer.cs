using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class FeedbackViewer : MonoBehaviour
    {
        public Text Popup;
        public Image Flash;
        public LineRenderer Burst;
        float age=2; Vector2 origin; Color tint;
        public void Play(Vector2 position,string message,Color color)
        {origin=position;tint=color;age=0;Popup.text=message;Popup.color=color;Burst.startColor=Burst.endColor=color;}
        public void Clear(){age=2;Popup.text="";Flash.color=Color.clear;Burst.positionCount=0;}
        void Update()
        {
            age+=Time.unscaledDeltaTime;float a=Mathf.Clamp01(1-age/.65f);
            Popup.color=new Color(tint.r,tint.g,tint.b,a);
            Flash.color=new Color(tint.r,tint.g,tint.b,a*.09f);
            Burst.positionCount=a>0?64:0;Burst.loop=true;
            Burst.startColor=Burst.endColor=new Color(tint.r,tint.g,tint.b,a*.6f);
            for(int i=0;i<Burst.positionCount;i++)Burst.SetPosition(i,origin+RingGeometry.Polar(.2f+age*2.2f,i*360f/64));
        }
    }
}
