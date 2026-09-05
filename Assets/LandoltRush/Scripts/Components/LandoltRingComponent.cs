using R3;
using UnityEngine;
namespace LandoltRush
{
    public sealed class LandoltRingComponent : MonoBehaviour
    {
        public RingViewer Viewer;
        public Vector2 Position {get;private set;}
        public Vector2 PreviousPosition {get;private set;}
        public float Angle {get;private set;}
        public float PreviousAngle {get;private set;}
        public float AngleDelta {get;private set;}
        public float Scale {get;private set;}
        public bool Resolved {get;private set;}
        public bool Entered {get;private set;}
        readonly Subject<Unit> exited=new Subject<Unit>();
        public Observable<Unit> OnExited=>exited;
        Vector2 velocity;float angularVelocity, radius;
        public void Initialize(RingSpawnData data,RingParam param)
        {
            Position=PreviousPosition=data.Position;Angle=PreviousAngle=data.Angle;Scale=data.Scale;
            velocity=data.Velocity;angularVelocity=data.AngularVelocity;radius=param.OuterRadius*Scale;
            Resolved=Entered=false;Viewer.Build(param);Apply();
        }
        public void Advance(float dt)
        {
            PreviousPosition=Position;PreviousAngle=Angle;AngleDelta=0;
            if(Resolved)return;
            Position+=velocity*dt;AngleDelta=angularVelocity*dt;Angle+=AngleDelta;Apply();
        }
        public void CheckExit(Rect bounds)
        {
            if(Resolved)return;
            bool overlaps=Position.x+radius>=bounds.xMin&&Position.x-radius<=bounds.xMax&&Position.y+radius>=bounds.yMin&&Position.y-radius<=bounds.yMax;
            if(overlaps)Entered=true;
            else if(Entered) { Resolved=true;exited.OnNext(Unit.Default); }
        }
        public void Resolve()=>Resolved=true;
        void Apply(){transform.position=Position;transform.rotation=Quaternion.Euler(0,0,Angle);transform.localScale=Vector3.one*Scale;}
        void OnDestroy()=>exited.Dispose();
    }
}
