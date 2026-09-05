using UnityEngine;

namespace LandoltRush
{
    // The rendered mesh and collision polygons use the same vertices.
    // Sweep the tip and shaft independently so their consequences can differ.
    public static class RingGeometry
    {
        public const int Segments = 80;
        struct Quad { public Vector2 A,B,C,D; }
        static Quad[] blackQuads;
        static float cachedInner,cachedOuter,cachedGap;
        static Quad[] BlackQuads(float inner,float outer,float gap)
        {
            if(blackQuads!=null&&cachedInner==inner&&cachedOuter==outer&&cachedGap==gap)return blackQuads;
            cachedInner=inner;cachedOuter=outer;cachedGap=gap;blackQuads=new Quad[Segments];
            for(int i=0;i<Segments;i++)
            {
                float a=gap*.5f+(360-gap)*i/Segments,b=gap*.5f+(360-gap)*(i+1)/Segments;
                blackQuads[i]=new Quad{A=Polar(inner,a),B=Polar(outer,a),C=Polar(outer,b),D=Polar(inner,b)};
            }
            return blackQuads;
        }
        public static Vector2 Polar(float radius, float degrees)
        { float a = degrees * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius; }
        public static Vector2 Rotate(Vector2 v, float degrees)
        { Vector2 p = Polar(1, degrees); return new Vector2(v.x * p.x - v.y * p.y, v.x * p.y + v.y * p.x); }

        public static HitKind Sweep(Vector2 tipFrom, Vector2 tipTo, float tipRadius,
            Vector2 centerFrom, Vector2 centerTo, float angleFrom, float angleDelta,
            float scale, float inner, float outer, float gap)
        {
            bool success = false;
            var quads=BlackQuads(inner,outer,gap);
            // Subdivide rotation, then test the entire relative translation exactly.
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(angleDelta) / .5f));
            for (int step = 0; step < steps; step++)
            {
                float t0 = (float)step / steps, t1 = (float)(step + 1) / steps;
                Vector2 from = Rotate(Vector2.Lerp(tipFrom, tipTo, t0) - Vector2.Lerp(centerFrom, centerTo, t0), -angleFrom - angleDelta * t0) / scale;
                Vector2 to = Rotate(Vector2.Lerp(tipFrom, tipTo, t1) - Vector2.Lerp(centerFrom, centerTo, t1), -angleFrom - angleDelta * t1) / scale;
                float theta = Mathf.Abs(angleDelta / steps) * Mathf.Deg2Rad;
                // Bound curvature of the rotating relative segment conservatively.
                float pad = theta * (to - from).magnitude * .25f + theta * theta * Mathf.Max(from.magnitude, to.magnitude) * .125f;
                float radius = tipRadius / scale + pad + .00001f;
                if (DistanceToSegment(Vector2.zero, from, to) > outer + radius) continue;
                for (int i = 0; i < Segments; i++)
                {
                    var q=quads[i];
                    if (HitsQuad(from, to, radius, q.A,q.B,q.C,q.D)) return HitKind.Black;
                }
                // The tip centre must cross inward through the inner radius. Merely
                // entering the outer opening, or starting inside the hole, earns nothing.
                if(from.sqrMagnitude>=inner*inner&&DistanceToSegment(Vector2.zero,from,to)<inner-.00001f)success=true;
            }
            return success ? HitKind.Gap : HitKind.None;
        }
        public static bool SweepShaft(Vector2 pivotFrom,Vector2 pivotTo,Vector2 tipFrom,Vector2 tipTo,float shaftRadius,
            Vector2 centerFrom,Vector2 centerTo,float angleFrom,float angleDelta,float scale,float inner,float outer,float gap)
        {
            var quads=BlackQuads(inner,outer,gap);
            Vector2 centerMove=centerTo-centerFrom;
            float travel=Mathf.Max((tipTo-tipFrom-centerMove).magnitude,(pivotTo-pivotFrom-centerMove).magnitude)/scale;
            int steps=Mathf.Max(1,Mathf.Max(Mathf.CeilToInt(Mathf.Abs(angleDelta)/.5f),Mathf.CeilToInt(travel/.15f)));
            for(int i=0;i<steps;i++)
            {
                float t0=(float)i/steps,t1=(float)(i+1)/steps;
                Vector2 c0=Vector2.Lerp(centerFrom,centerTo,t0),c1=Vector2.Lerp(centerFrom,centerTo,t1);
                Vector2 a=Rotate(Vector2.Lerp(pivotFrom,pivotTo,t0)-c0,-angleFrom-angleDelta*t0)/scale;
                Vector2 b=Rotate(Vector2.Lerp(tipFrom,tipTo,t0)-c0,-angleFrom-angleDelta*t0)/scale;
                Vector2 c=Rotate(Vector2.Lerp(pivotFrom,pivotTo,t1)-c1,-angleFrom-angleDelta*t1)/scale;
                Vector2 d=Rotate(Vector2.Lerp(tipFrom,tipTo,t1)-c1,-angleFrom-angleDelta*t1)/scale;
                float theta=Mathf.Abs(angleDelta/steps)*Mathf.Deg2Rad;
                float pad=theta*Mathf.Max((c-a).magnitude,(d-b).magnitude)*.25f+
                    theta*theta*Mathf.Max(Mathf.Max(a.magnitude,b.magnitude),Mathf.Max(c.magnitude,d.magnitude))*.125f;
                float r=shaftRadius/scale+pad+.00001f,limit=outer+r;
                if(Mathf.Min(Mathf.Min(a.x,b.x),Mathf.Min(c.x,d.x))>limit||Mathf.Max(Mathf.Max(a.x,b.x),Mathf.Max(c.x,d.x))<-limit||
                   Mathf.Min(Mathf.Min(a.y,b.y),Mathf.Min(c.y,d.y))>limit||Mathf.Max(Mathf.Max(a.y,b.y),Mathf.Max(c.y,d.y))<-limit)continue;
                foreach(var q in quads)
                {
                    // The convex hull covers every intermediate shaft segment, including
                    // a fast mouse swing whose two end positions both miss the ring.
                    if(HitsQuad(a,b,r,q.A,q.B,q.C,q.D)||HitsQuad(c,d,r,q.A,q.B,q.C,q.D)||
                       HitsQuad(a,c,r,q.A,q.B,q.C,q.D)||HitsQuad(a,d,r,q.A,q.B,q.C,q.D)||
                       HitsQuad(b,c,r,q.A,q.B,q.C,q.D)||HitsQuad(b,d,r,q.A,q.B,q.C,q.D)||
                       InTriangle(q.A,a,b,c)||InTriangle(q.A,a,b,d)||InTriangle(q.A,a,c,d)||InTriangle(q.A,b,c,d))return true;
                }
            }
            return false;
        }
        static bool InTriangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c)
        {
            if(Mathf.Abs(Cross(b-a,c-a))<1e-8f)return false;
            float u=Cross(b-a,p-a),v=Cross(c-b,p-b),w=Cross(a-c,p-c);
            return (u>=0&&v>=0&&w>=0)||(u<=0&&v<=0&&w<=0);
        }
        static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        static bool Inside(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        { return Cross(b-a,p-a)>=0 && Cross(c-b,p-b)>=0 && Cross(d-c,p-c)>=0 && Cross(a-d,p-d)>=0; }
        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        { Vector2 v=b-a; return (p - (a + v * Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(v.sqrMagnitude,1e-12f)))).magnitude; }
        static bool CloseSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float r)
        {
            float u=Cross(b-a,d-c);
            if (Mathf.Abs(u)>1e-8f) { float t=Cross(c-a,d-c)/u, s=Cross(c-a,b-a)/u; if(t>=0&&t<=1&&s>=0&&s<=1)return true; }
            return DistanceToSegment(a,c,d)<=r || DistanceToSegment(b,c,d)<=r || DistanceToSegment(c,a,b)<=r || DistanceToSegment(d,a,b)<=r;
        }
        static bool HitsQuad(Vector2 from, Vector2 to, float r, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        { return Inside(from,a,b,c,d)||Inside(to,a,b,c,d)||CloseSegments(from,to,a,b,r)||CloseSegments(from,to,b,c,r)||CloseSegments(from,to,c,d,r)||CloseSegments(from,to,d,a,r); }
    }
}
