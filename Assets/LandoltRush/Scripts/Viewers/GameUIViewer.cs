using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class GameUIViewer : MonoBehaviour
    {
        public GameObject TitleRoot,HudRoot,PauseRoot;
        public Text ScoreText,ComboText,StatusText,MuteText;
        public Image Gauge;
        public void Refresh(GameData data,GameConfig config)
        {
            TitleRoot.SetActive(data.Phase==GamePhase.Title);HudRoot.SetActive(data.Phase!=GamePhase.Title);
            PauseRoot.SetActive(data.Phase==GamePhase.Paused);
            ScoreText.text=data.Score.ToString("D6");ComboText.text=data.ComboCount>0?$"{data.ComboCount:00}  COMBO":"—  COMBO";
            Gauge.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(data.ComboRemainingTime/config.ComboLimitTime),1);
        }
        public void Status(string message)=>StatusText.text=message;
        public void Sound(bool muted)=>MuteText.text=muted?"SOUND OFF  [M]":"SOUND ON  [M]";
    }
}
