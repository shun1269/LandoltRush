using UnityEngine;

namespace LandoltRush
{
    // The rendered mesh and collision polygons use the same vertices.
    // Only the tip's swept circle is tested; the shaft never participates.
    public static class RingGeometry
    {
        public const int Segments = 80;
        public static Vector2 Polar(float radius, float degrees)
        { float a = degrees * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius; }
        public static Vector2 Rotate(Vector2 v, float degrees)
        { Vector2 p = Polar(1, degrees); return new Vector2(v.x * p.x - v.y * p.y, v.x * p.y + v.y * p.x); }

        public static HitKind Sweep(Vector2 tipFrom, Vector2 tipTo, float tipRadius,
            Vector2 centerFrom, Vector2 centerTo, float angleFrom, float angleDelta,
            float scale, float inner, float outer, float gap)
        {
            bool success = false;
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
                    float a = gap * .5f + (360 - gap) * i / Segments;
                    float b = gap * .5f + (360 - gap) * (i + 1) / Segments;
                    if (HitsQuad(from, to, radius, Polar(inner, a), Polar(outer, a), Polar(outer, b), Polar(inner, b))) return HitKind.Black;
                }
                // Split the gap into small annular quads as well; its hole is never a success zone.
                for (int i = 0; i < 12; i++)
                {
                    float a = -gap * .5f + gap * i / 12, b = -gap * .5f + gap * (i + 1) / 12;
                    if (HitsQuad(from, to, tipRadius / scale, Polar(inner, a), Polar(outer, a), Polar(outer, b), Polar(inner, b))) success = true;
                }
            }
            return success ? HitKind.Gap : HitKind.None;
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
