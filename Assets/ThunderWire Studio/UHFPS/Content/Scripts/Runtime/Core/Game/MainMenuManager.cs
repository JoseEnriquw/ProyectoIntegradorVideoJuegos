using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Main Menu Manager")]
    public class MainMenuManager : MonoBehaviour
    {
        public BackgroundFader BackgroundFader;
        public string NewGameSceneName;
        public bool NewGameRemoveSaves;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void NewGame()
        {
            if (string.IsNullOrEmpty(NewGameSceneName))
                throw new System.NullReferenceException("The new game scene name field is empty!");

            SaveGameManager.ClearLoadType();
            StartCoroutine(LoadNewGame());
        }

        IEnumerator LoadNewGame()
        {
            yield return BackgroundFader.StartBackgroundFade(false);
            if(NewGameRemoveSaves) yield return new WaitToTaskComplete(SaveGameManager.RemoveAllSaves());

            SaveGameManager.LoadSceneName = NewGameSceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            SaveGameManager.ClearLoadType();
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        IEnumerator LoadSceneRoutine(string sceneName)
        {
            if (BackgroundFader != null)
                yield return BackgroundFader.StartBackgroundFade(false);

            SaveGameManager.LoadSceneName = sceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        public void LoadSceneDirect(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            SaveGameManager.ClearLoadType();
            SceneManager.LoadScene(sceneName);
        }
    }
}