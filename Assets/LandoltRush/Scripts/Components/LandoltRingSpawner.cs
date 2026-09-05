using UnityEngine;
using System.Collections.Generic;
namespace LandoltRush
{
    public sealed class LandoltRingSpawner : MonoBehaviour
    {
        readonly List<LandoltRingComponent> rings=new List<LandoltRingComponent>();
        public IReadOnlyList<LandoltRingComponent> Rings=>rings;
        public LandoltRingComponent Spawn(RingSpawnData data,RingParam param)
        {
            var ring=Instantiate(param.Prefab,transform);ring.name="Active Landolt Ring";ring.Initialize(data,param);rings.Add(ring);return ring;
        }
        public void Remove(LandoltRingComponent ring)
        {
            if(ring==null||!rings.Remove(ring))return;
            ring.Resolve();if(Application.isPlaying)Destroy(ring.gameObject);else DestroyImmediate(ring.gameObject);
        }
        public void StopAll(){foreach(var ring in rings)ring.Resolve();}
        public void Clear(){for(int i=rings.Count-1;i>=0;i--)Remove(rings[i]);}
    }
}
