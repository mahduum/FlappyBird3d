using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Behaviors
{
    public class MainMenu : MonoBehaviour
    {
        [UsedImplicitly]
        public void OnClickPlayGame()
        {
            //use loading manager here
            SceneManager.LoadSceneAsync("FlappyScene", LoadSceneMode.Single);
        }

        [UsedImplicitly]
        public void OnClickHighScores()
        {
        
        }

        [UsedImplicitly]
        public void OnClickQuit()
        {
            //save all settings and then:
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
