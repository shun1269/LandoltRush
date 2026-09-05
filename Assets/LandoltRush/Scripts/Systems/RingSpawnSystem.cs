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
            Vector2 velocity = (target - spawn).normalized * Range(param.MinMoveSpeed, param.MaxMoveSpeed);
            float duration = VisibleTravelTime(bounds, spawn, velocity, ring.OuterRadius * scale);
            float minimumRotation = Mathf.Max(param.MinRotateSpeed, 1080f / Mathf.Max(.001f, duration));
            return new RingSpawnData { Side = top ? SpawnSide.Top : SpawnSide.Left, Position = spawn, Target = target,
                Velocity = velocity, Scale = scale,
                Angle = Range(0, 360), AngularVelocity = Range(minimumRotation, Mathf.Max(minimumRotation, param.MaxRotateSpeed)) * (random.Next(2) == 0 ? -1 : 1) };
        }

        // Use only the time when the whole ring is visible, excluding its entry and exit.
        public static float VisibleTravelTime(Rect bounds, Vector2 position, Vector2 velocity, float radius)
        {
            var interior = Rect.MinMaxRect(bounds.xMin + radius, bounds.yMin + radius,
                bounds.xMax - radius, bounds.yMax - radius);
            float duration = TravelTime(interior, position, velocity);
            // Custom sizes/paths may never fit fully inside. Use the centre's passage then.
            return duration > 0 ? duration : TravelTime(bounds, position, velocity);
        }

        static float TravelTime(Rect bounds, Vector2 position, Vector2 velocity)
        {
            if (bounds.width <= 0 || bounds.height <= 0 || velocity.sqrMagnitude < .000001f) return 0;
            float enter = 0, exit = float.PositiveInfinity;
            if (!ClipAxis(position.x, velocity.x, bounds.xMin, bounds.xMax, ref enter, ref exit) ||
                !ClipAxis(position.y, velocity.y, bounds.yMin, bounds.yMax, ref enter, ref exit)) return 0;
            return Mathf.Max(0, exit - enter);
        }

        static bool ClipAxis(float position, float velocity, float min, float max, ref float enter, ref float exit)
        {
            if (Mathf.Abs(velocity) < .000001f) return position >= min && position <= max;
            float a = (min - position) / velocity, b = (max - position) / velocity;
            enter = Mathf.Max(enter, Mathf.Min(a, b));
            exit = Mathf.Min(exit, Mathf.Max(a, b));
            return exit > enter;
        }
    }
}
