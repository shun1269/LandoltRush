using R3;
using UnityEngine;
using System.Collections.Generic;
namespace LandoltRush
{
    public sealed class RodTipCollision : MonoBehaviour
    {
        readonly Subject<RingContact> detected=new Subject<RingContact>();
        readonly List<RingContact> contacts=new List<RingContact>();
        public Observable<RingContact> OnDetected=>detected;
        public void Check(RodController rod,IReadOnlyList<LandoltRingComponent> rings,RodParam tip,RingParam param)
        {
            contacts.Clear();
            foreach(var ring in rings)
            {
                if(ring==null||ring.Resolved)continue;
                HitKind hit=Evaluate(rod,ring,tip,param);
                // Check every ring before emitting nonfatal outcomes. A tip collision
                // anywhere in this frame takes priority over all misses and successes.
                if(hit==HitKind.Black){detected.OnNext(new RingContact(ring,hit));return;}
                if(hit!=HitKind.None)contacts.Add(new RingContact(ring,hit));
            }
            foreach(var contact in contacts)if(contact.Kind==HitKind.Shaft)detected.OnNext(contact);
            foreach(var contact in contacts)if(contact.Kind==HitKind.Gap)detected.OnNext(contact);
        }
        public static HitKind Evaluate(RodController rod,LandoltRingComponent ring,RodParam tip,RingParam param)
        {
            HitKind hit=RingGeometry.Sweep(rod.PreviousTip,rod.Tip,tip.TipRadius,ring.PreviousPosition,ring.Position,
                ring.PreviousAngle,ring.AngleDelta,ring.Scale,param.InnerRadius,param.OuterRadius,param.GapDegrees);
            if(hit==HitKind.Black)return hit;
            if(RingGeometry.SweepShaft(rod.PreviousPivot,rod.Pivot,rod.PreviousTip,rod.Tip,tip.ShaftRadius,
                ring.PreviousPosition,ring.Position,ring.PreviousAngle,ring.AngleDelta,ring.Scale,param.InnerRadius,param.OuterRadius,param.GapDegrees))return HitKind.Shaft;
            return hit;
        }
        void OnDestroy()=>detected.Dispose();
    }
}
