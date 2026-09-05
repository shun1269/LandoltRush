using UnityEngine;
namespace LandoltRush
{
    // Display-only copies: captured rings have already left the collision/spawn systems.
    public sealed class RingDiveViewer : MonoBehaviour
    {
        public RingParam Param;
        public Material Material;
        public const float Duration = .6f;
        const int Capacity = 16;
        static readonly Color Teal = new Color(0, .65f, .54f);
        sealed class Dive
        {
            public RingViewer Ring;
            public LineRenderer Rim;
            public Vector2 Origin, Centre;
            public float Age, InitialScale, FinalScale;
        }
        readonly Dive[] dives = new Dive[Capacity];
        int next;
        public int ActiveCount
        {
            get { int count=0;foreach(var dive in dives)if(dive!=null&&dive.Ring.gameObject.activeSelf)count++;return count; }
        }
        public void Play(Vector2 position, float angle, float scale, Rect bounds)
        {
            var dive=dives[next];
            if(dive==null)dives[next]=dive=CreateDive();
            next=(next+1)%Capacity;
            dive.Origin=position;dive.Centre=bounds.center;dive.InitialScale=scale;
            // At the end even the inner edge has passed all four screen corners.
            dive.FinalScale=Mathf.Max(scale, bounds.size.magnitude*.65f/Param.InnerRadius);
            dive.Age=0;dive.Ring.transform.rotation=Quaternion.Euler(0,0,angle);
            dive.Ring.gameObject.SetActive(true);Render(dive);
        }
        Dive CreateDive()
        {
            var root=new GameObject("Captured ring");root.transform.SetParent(transform,false);
            var ring=root.AddComponent<RingViewer>();ring.Build(Param);
            var renderer=ring.GetComponent<MeshRenderer>();renderer.sharedMaterial=Material;renderer.sortingOrder=8;
            var edge=new GameObject("Inner edge");edge.transform.SetParent(root.transform,false);
            var rim=edge.AddComponent<LineRenderer>();rim.sharedMaterial=Material;rim.sortingOrder=9;
            rim.useWorldSpace=false;rim.loop=false;rim.widthMultiplier=.018f;rim.numCapVertices=3;
            rim.positionCount=RingGeometry.Segments+1;
            for(int i=0;i<rim.positionCount;i++)
                rim.SetPosition(i,RingGeometry.Polar(Param.InnerRadius,Param.GapDegrees*.5f+(360-Param.GapDegrees)*i/RingGeometry.Segments));
            return new Dive{Ring=ring,Rim=rim};
        }
        public void Advance(float delta)
        {
            foreach(var dive in dives)
            {
                if(dive==null||!dive.Ring.gameObject.activeSelf)continue;
                dive.Age+=Mathf.Max(0,delta);
                if(dive.Age>=Duration)dive.Ring.gameObject.SetActive(false);
                else Render(dive);
            }
        }
        void Render(Dive dive)
        {
            float t=Mathf.Clamp01(dive.Age/Duration);
            float zoom=t*t*t;
            dive.Ring.transform.position=Vector2.Lerp(dive.Origin,dive.Centre,Mathf.SmoothStep(0,1,t));
            dive.Ring.transform.localScale=Vector3.one*Mathf.Lerp(dive.InitialScale,dive.FinalScale,zoom);
            var color=Color.Lerp(Param.Ink,Teal,Mathf.Clamp01(t*5));
            color.a=1-Mathf.SmoothStep(0,1,t);dive.Ring.SetColor(color);
            dive.Rim.startColor=dive.Rim.endColor=new Color(Teal.r,Teal.g,Teal.b,color.a*Mathf.Clamp01(t*8)*.75f);
        }
        public void Clear()
        {
            foreach(var dive in dives)if(dive!=null)dive.Ring.gameObject.SetActive(false);
            next=0;
        }
    }
}
