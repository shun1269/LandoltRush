using UnityEngine;
namespace LandoltRush
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RingViewer : MonoBehaviour
    {
        Mesh mesh;
        MaterialPropertyBlock tint;
        public void Build(RingParam param)
        {
            ReleaseMesh();
            mesh = new Mesh { name = "Landolt annulus" };
            var vertices = new Vector3[(RingGeometry.Segments + 1) * 2];
            var triangles = new int[RingGeometry.Segments * 6];
            for(int i=0;i<=RingGeometry.Segments;i++)
            {
                float a=param.GapDegrees*.5f+(360-param.GapDegrees)*i/RingGeometry.Segments;
                vertices[i*2]=RingGeometry.Polar(param.InnerRadius,a);
                vertices[i*2+1]=RingGeometry.Polar(param.OuterRadius,a);
                if(i==RingGeometry.Segments)continue;
                int t=i*6,v=i*2;
                triangles[t]=v;triangles[t+1]=v+2;triangles[t+2]=v+1;
                triangles[t+3]=v+1;triangles[t+4]=v+2;triangles[t+5]=v+3;
            }
            var colors=new Color[vertices.Length];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;
            mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh=mesh;
            SetColor(param.Ink);
        }
        public void SetColor(Color color)
        {
            if(tint==null)tint=new MaterialPropertyBlock();
            tint.SetColor("_Color",color);GetComponent<MeshRenderer>().SetPropertyBlock(tint);
        }
        void ReleaseMesh(){if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
        void OnDestroy()=>ReleaseMesh();
    }
}
