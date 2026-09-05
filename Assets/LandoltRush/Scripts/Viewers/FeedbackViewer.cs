using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class FeedbackViewer : MonoBehaviour
    {
        public Text Popup;
        public DamageBorderGraphic Border;
        public RingDiveViewer Dive;
        float missAge=2;
        static readonly Color Red=new Color(.83f,.17f,.15f);
        public void Success(Vector2 position,float angle,float scale,Rect bounds)=>Dive.Play(position,angle,scale,bounds);
        public void Damage(){missAge=0;Popup.text="ミス";Advance(0);}
        public void Clear(){missAge=2;Popup.text="";Border.color=Color.clear;Dive.Clear();}
        void Update()=>Advance(Time.unscaledDeltaTime);
        public void Advance(float delta)
        {
            Dive.Advance(delta);missAge+=delta;float damage=Mathf.Clamp01(1-missAge/.55f);
            Popup.color=new Color(Red.r,Red.g,Red.b,Mathf.Clamp01(1-missAge/.7f));
            Border.color=new Color(Red.r,Red.g,Red.b,.65f*damage*damage);
        }
    }
}
