using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class GameUIViewer : MonoBehaviour
    {
        public GameObject TitleRoot,HudRoot,PauseRoot,ComboRoot;
        public Text ScoreText,ComboText,MuteText;
        public Image Gauge;
        public HeartGraphic[] Hearts;
        int previousCombo=-1;float pulse;bool hasCombo;
        public void Refresh(GameData data,GameConfig config)
        {
            TitleRoot.SetActive(data.Phase==GamePhase.Title);HudRoot.SetActive(data.Phase!=GamePhase.Title);
            PauseRoot.SetActive(data.Phase==GamePhase.Paused);
            ComboRoot.SetActive(data.Phase==GamePhase.Playing||data.Phase==GamePhase.Paused);
            ScoreText.text=data.Score.ToString();ComboText.text=data.ComboCount.ToString();
            hasCombo=data.ComboCount>0;
            if(data.ComboCount>previousCombo&&previousCombo>=0)pulse=1;
            if(!hasCombo||!ComboRoot.activeSelf)pulse=0;previousCombo=data.ComboCount;
            ApplyComboPulse();
            Gauge.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(data.ComboRemainingTime/config.ComboLimitTime),1);
            for(int i=0;i<Hearts.Length;i++)
            {
                Hearts[i].Filled=i<data.Lives;
                Hearts[i].color=i<data.Lives?new Color(.76f,.35f,.32f):new Color(.76f,.76f,.71f);
            }
        }
        void Update()=>Advance(Time.unscaledDeltaTime);
        public void Advance(float delta)
        {
            pulse=Mathf.Max(0,pulse-Mathf.Max(0,delta)/.24f);
            ApplyComboPulse();
        }
        void ApplyComboPulse()
        {
            // Immediate punch on the new number, then a quick eased return to rest.
            float strength=pulse*pulse;
            ComboText.rectTransform.localScale=Vector3.one*(1+.30f*strength);
            ComboText.color=hasCombo?new Color(.08f,.40f,.32f,.22f+.16f*strength):new Color(.12f,.18f,.15f,.085f);
        }
        public void Sound(bool muted)=>MuteText.text=muted?"音なし":"音あり";
    }
}
