using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    [ExecuteAlways,RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoundedPanelGraphic : MaskableGraphic
    {
        public float Radius=18;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;float radius=Mathf.Min(Radius,Mathf.Min(r.width,r.height)*.5f);
            vh.AddVert(r.center,color,Vector2.zero);
            var centers=new[]{new Vector2(r.xMax-radius,r.yMax-radius),new Vector2(r.xMin+radius,r.yMax-radius),new Vector2(r.xMin+radius,r.yMin+radius),new Vector2(r.xMax-radius,r.yMin+radius)};
            const int segments=10;
            for(int corner=0;corner<4;corner++)for(int i=0;i<=segments;i++)
            {
                float angle=(corner*90+i*90f/segments)*Mathf.Deg2Rad;
                vh.AddVert(centers[corner]+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,color,Vector2.zero);
            }
            int count=4*(segments+1);for(int i=0;i<count;i++)vh.AddTriangle(0,i+1,(i+1)%count+1);
        }
    }
}
