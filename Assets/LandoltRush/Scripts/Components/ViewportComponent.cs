using UnityEngine;
namespace LandoltRush
{
    [RequireComponent(typeof(Camera))]
    public sealed class ViewportComponent : MonoBehaviour
    {
        Camera view;int width,height;
        void Awake(){view=GetComponent<Camera>();Fit();}
        void Update(){if(Screen.width!=width||Screen.height!=height)Fit();}
        void Fit()
        {
            width=Screen.width;height=Screen.height;float ratio=(float)width/Mathf.Max(1,height)/(16f/9f);
            view.rect=ratio>1?new Rect((1-1/ratio)*.5f,0,1/ratio,1):new Rect(0,(1-ratio)*.5f,1,ratio);
        }
    }
}
