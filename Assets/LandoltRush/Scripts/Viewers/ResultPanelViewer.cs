using UnityEngine;
using UnityEngine.UI;
namespace LandoltRush
{
    public sealed class ResultPanelViewer : MonoBehaviour
    {
        public GameObject Root;
        public Text Score,MaxCombo;
        public void Hide()=>Root.SetActive(false);
        public void Show(GameData data)
        {Root.SetActive(true);Score.text=data.Score.ToString();MaxCombo.text=data.MaxCombo.ToString();}
    }
}
