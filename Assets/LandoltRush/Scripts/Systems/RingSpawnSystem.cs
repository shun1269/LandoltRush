using UnityEngine;
namespace LandoltRush
{
    public sealed class RingSpawnSystem
    {
        readonly SpawnParam param; readonly RingParam ring;
        readonly System.Random random = new System.Random();
        public RingSpawnSystem(SpawnParam param, RingParam ring) { this.param = param; this.ring = ring; }
        float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
        public RingSpawnData Create(Rect bounds)
        {
            float scale = Range(param.MinScale, param.MaxScale);
            float margin = ring.OuterRadius * scale + param.SpawnMargin;
            float edge = Mathf.Min(param.EdgeExclusionMargin, Mathf.Min(bounds.width, bounds.height) * .35f);
            bool top = Range(0, Mathf.Max(.001f, param.TopSpawnWeight + param.LeftSpawnWeight)) < param.TopSpawnWeight;
            Vector2 spawn = top ? new Vector2(Range(bounds.xMin + edge, bounds.xMax - edge), bounds.yMax + margin)
                : new Vector2(bounds.xMin - margin, Range(bounds.yMin + edge, bounds.yMax - edge));
            Vector2 target = bounds.center + new Vector2(Range(-bounds.width, bounds.width), Range(-bounds.height, bounds.height)) * (.5f * param.TargetAreaRate);
            return new RingSpawnData { Side = top ? SpawnSide.Top : SpawnSide.Left, Position = spawn, Target = target,
                Velocity = (target - spawn).normalized * Range(param.MinMoveSpeed, param.MaxMoveSpeed), Scale = scale,
                Angle = Range(0, 360), AngularVelocity = Range(param.MinRotateSpeed, param.MaxRotateSpeed) * (random.Next(2) == 0 ? -1 : 1) };
        }
    }
}
