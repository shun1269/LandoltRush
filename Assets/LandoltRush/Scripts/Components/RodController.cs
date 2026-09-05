using UnityEngine;
using UnityEngine.InputSystem;
namespace LandoltRush
{
    public sealed class RodController : MonoBehaviour
    {
        public Camera Camera;
        public RodView View;
        public Vector2 Tip {get;private set;}
        public Vector2 PreviousTip {get;private set;}
        public bool CanMove {get;private set;}
        public Vector2? TestPosition {get;set;}
        public Rect Bounds
        {get{float h=Camera.orthographicSize,w=h*Camera.aspect;return new Rect(-w,-h,w*2,h*2);}}
        public void SetActive(bool active,RodParam param)
        {CanMove=active;View.gameObject.SetActive(active);if(active){Read(param);PreviousTip=Tip;}}
        public void Sample(RodParam param) {PreviousTip=Tip;if(CanMove)Read(param);}
        void Read(RodParam param)
        {
            Vector2 p=Mouse.current!=null?Mouse.current.position.ReadValue():new Vector2(Screen.width*.75f,Screen.height*.25f);
            Rect viewport=Camera.pixelRect;
            Tip=TestPosition??(Vector2)Camera.ScreenToWorldPoint(new Vector3(Mathf.Clamp(p.x,viewport.xMin,viewport.xMax),Mathf.Clamp(p.y,viewport.yMin,viewport.yMax),10));
            View.Render(new Vector2(Bounds.xMax+param.RightPivotMargin,Bounds.yMin-param.BottomPivotMargin),Tip,param.TipRadius);
        }
        public void Freeze() {CanMove=false;}
    }
}
