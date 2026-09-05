using UnityEngine;
namespace LandoltRush
{
    public sealed class TitleDecorationViewer : MonoBehaviour
    {
        public RingViewer Ring;public RingParam Param;
        void Start()=>Ring.Build(Param);
        void Update()=>Ring.transform.rotation=Quaternion.Euler(0,0,-25+Mathf.Sin(Time.unscaledTime*.4f)*14);
    }
}
