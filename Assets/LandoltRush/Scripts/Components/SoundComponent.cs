using UnityEngine;
namespace LandoltRush
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundComponent : MonoBehaviour
    {
        AudioSource source;AudioClip success,failure,miss;
        public bool Muted {get;private set;}
        void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.volume=.24f;success=Tone("Success",740,1120,.16f);failure=Tone("Contact",180,45,.38f);miss=Tone("Miss",280,200,.12f);}
        static AudioClip Tone(string name,float start,float end,float duration)
        {
            int count=(int)(44100*duration);var samples=new float[count];float phase=0;
            for(int i=0;i<count;i++){float t=(float)i/count;phase+=Mathf.Lerp(start,end,t)*2*Mathf.PI/44100;samples[i]=Mathf.Sin(phase)*Mathf.Sin(Mathf.PI*t)*Mathf.Pow(1-t,.5f);}
            var clip=AudioClip.Create(name,count,1,44100,false);clip.SetData(samples,0);return clip;
        }
        public void Play(HitKind kind){if(!Muted)source.PlayOneShot(kind==HitKind.Gap?success:kind==HitKind.Black?failure:miss);}
        public void Toggle(){Muted=!Muted;source.mute=Muted;}
        void OnDestroy(){if(success)Destroy(success);if(failure)Destroy(failure);if(miss)Destroy(miss);}
    }
}
