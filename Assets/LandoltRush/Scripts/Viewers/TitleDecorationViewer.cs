using UnityEngine;
namespace LandoltRush
{
    public sealed class TitleDecorationViewer : MonoBehaviour
    {
        public RingViewer Ring;public RingParam Param;
        void Start(){Ring.Build(Param);Ring.transform.rotation=Quaternion.Euler(0,0,-25);}
        void Update()=>Advance(Time.unscaledDeltaTime);
        public void Advance(float delta)
        {
            float angle=Mathf.Repeat(Ring.transform.eulerAngles.z-30f*Mathf.Max(0,delta),360);
            Ring.transform.rotation=Quaternion.Euler(0,0,angle);
        }
    }
}
