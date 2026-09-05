using UnityEngine;
using UnityEngine.SceneManagement;
namespace LandoltRush
{
    public static class SampleSceneRedirect
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OpenGame()
        {
            if(SceneManager.GetActiveScene().name=="SampleScene"&&Application.CanStreamedLevelBeLoaded("GameScene"))SceneManager.LoadScene("GameScene");
        }
    }
}
