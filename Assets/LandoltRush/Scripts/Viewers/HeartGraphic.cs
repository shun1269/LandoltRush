using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    [ExecuteAlways,RequireComponent(typeof(CanvasRenderer))]
    public sealed class HeartGraphic : MaskableGraphic
    {
        [SerializeField] bool filled=true;
        public bool Filled { get=>filled; set{if(filled==value)return;filled=value;SetVerticesDirty();} }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;const int count=64;
            Vector2 center=r.center;
            for(int i=0;i<count;i++)
            {
                Vector2 a=Point(i*2*Mathf.PI/count,r),b=Point((i+1)*2*Mathf.PI/count,r);
                int v=vh.currentVertCount;
                vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);
                if(filled){vh.AddVert(center,color,Vector2.zero);vh.AddTriangle(v,v+1,v+2);}
                else
                {
                    vh.AddVert(Vector2.Lerp(center,b,.74f),color,Vector2.zero);
                    vh.AddVert(Vector2.Lerp(center,a,.74f),color,Vector2.zero);
                    vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
                }
            }
        }
        static Vector2 Point(float t,Rect r)
        {
            float s=Mathf.Sin(t),x=16*s*s*s,y=13*Mathf.Cos(t)-5*Mathf.Cos(2*t)-2*Mathf.Cos(3*t)-Mathf.Cos(4*t);
            return new Vector2(r.xMin+(x/32+.5f)*r.width,r.yMin+(y+17)/30*r.height);
        }
    }
}
