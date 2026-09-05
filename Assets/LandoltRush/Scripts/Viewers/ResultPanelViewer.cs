using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class ResultPanelViewer : MonoBehaviour
    {
        public GameObject Root;
        public Text Score,Stats;
        public void Hide()=>Root.SetActive(false);
        public void Show(GameData data)
        {Root.SetActive(true);Score.text=data.Score.ToString("D6");Stats.text=$"{data.MaxCombo:00}   MAX COMBO     /     {data.SuccessCount:00}   SUCCESS     /     {data.MissCount:00}   MISS";}
    }
}
