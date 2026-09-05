using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    [ExecuteAlways,RequireComponent(typeof(CanvasRenderer))]
    public sealed class DamageBorderGraphic : MaskableGraphic
    {
        public float BorderWidth=140;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;const int steps=16;
            float width=Mathf.Min(BorderWidth,Mathf.Min(r.width,r.height)*.45f);
            for(int i=0;i<steps;i++)
            {
                float t0=(float)i/steps,t1=(float)(i+1)/steps;
                float a=width*t0,b=width*t1;
                var outer=new[]{new Vector2(r.xMin+a,r.yMin+a),new Vector2(r.xMax-a,r.yMin+a),new Vector2(r.xMax-a,r.yMax-a),new Vector2(r.xMin+a,r.yMax-a)};
                var inner=new[]{new Vector2(r.xMin+b,r.yMin+b),new Vector2(r.xMax-b,r.yMin+b),new Vector2(r.xMax-b,r.yMax-b),new Vector2(r.xMin+b,r.yMax-b)};
                Color c0=color,c1=color;c0.a*=Mathf.Pow(1-t0,2);c1.a*=Mathf.Pow(1-t1,2);
                for(int j=0;j<4;j++)
                {
                    int next=(j+1)%4,v=vh.currentVertCount;
                    vh.AddVert(outer[j],c0,Vector2.zero);vh.AddVert(outer[next],c0,Vector2.zero);
                    vh.AddVert(inner[next],c1,Vector2.zero);vh.AddVert(inner[j],c1,Vector2.zero);
                    vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
                }
            }
        }
    }
}
