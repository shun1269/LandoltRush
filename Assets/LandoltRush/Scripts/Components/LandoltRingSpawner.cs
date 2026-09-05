using UnityEngine;
namespace LandoltRush
{
    public sealed class LandoltRingSpawner : MonoBehaviour
    {
        public LandoltRingComponent Current {get;private set;}
        public LandoltRingComponent Spawn(RingSpawnData data,RingParam param)
        { Clear();Current=Instantiate(param.Prefab,transform);Current.name="Active Landolt Ring";Current.Initialize(data,param);return Current; }
        public void Clear(){if(Current!=null){Current.Resolve();Destroy(Current.gameObject);}Current=null;}
    }
}
