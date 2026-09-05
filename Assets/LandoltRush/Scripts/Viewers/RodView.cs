using UnityEngine;
namespace LandoltRush
{
    public sealed class RodView : MonoBehaviour
    {
        public LineRenderer Shaft, TipOutline;
        public void Render(Vector2 pivot,Vector2 tip,float radius)
        {
            Shaft.positionCount=2;Shaft.SetPosition(0,pivot);Shaft.SetPosition(1,tip);
            TipOutline.positionCount=32;TipOutline.loop=true;
            for(int i=0;i<32;i++)TipOutline.SetPosition(i,tip+RingGeometry.Polar(radius,i*360f/32));
        }
    }
}
