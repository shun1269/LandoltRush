using R3;
using UnityEngine;
namespace LandoltRush
{
    public sealed class RodTipCollision : MonoBehaviour
    {
        readonly Subject<HitKind> detected=new Subject<HitKind>();
        public Observable<HitKind> OnDetected=>detected;
        public void Check(RodController rod,LandoltRingComponent ring,RodParam tip,RingParam param)
        {
            if(ring==null||ring.Resolved)return;
            HitKind hit=RingGeometry.Sweep(rod.PreviousTip,rod.Tip,tip.TipRadius,ring.PreviousPosition,ring.Position,
                ring.PreviousAngle,ring.AngleDelta,ring.Scale,param.InnerRadius,param.OuterRadius,param.GapDegrees);
            if(hit!=HitKind.None)detected.OnNext(hit);
        }
        void OnDestroy()=>detected.Dispose();
    }
}
