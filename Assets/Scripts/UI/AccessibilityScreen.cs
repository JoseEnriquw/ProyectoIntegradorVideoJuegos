using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UHFPS.Runtime
{
    public class AccessibilityScreen : MonoBehaviour
    {
        [Header("Fading Settings")]
        public CanvasGroup ScreenCanvasGroup;
        public float FadeSpeed = 1.5f;

        [Header("Warning Display Settings")]
        public CanvasGroup WarningCanvasGroup;
        public float WarningDisplayTime = 5f;

        [Header("Scene Loading")]
        public string MainMenuSceneName = "MainMenu";

        private bool isTransitioning = false;

        private void Awake()
        {
            // Initial state
            if (ScreenCanvasGroup != null)
            {
                ScreenCanvasGroup.alpha = 0f;
                ScreenCanvasGroup.interactable = false;
                ScreenCanvasGroup.blocksRaycasts = false;
            }

            if (WarningCanvasGroup != null)
            {
                WarningCanvasGroup.alpha = 0f;
                WarningCanvasGroup.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            // Lock/unlock cursor for menu interaction
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            StartCoroutine(IntroSequence());
        }

        private IEnumerator IntroSequence()
        {
            // 1. Fade in the whole screen
            if (ScreenCanvasGroup != null)
            {
                ScreenCanvasGroup.interactable = true;
                ScreenCanvasGroup.blocksRaycasts = true;
                yield return StartCoroutine(FadeCanvasGroup(ScreenCanvasGroup, 0f, 1f, FadeSpeed));
            }

            // 2. Fade in Warning
            if (WarningCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(WarningCanvasGroup, 0f, 1f, FadeSpeed));
                yield return new WaitForSeconds(WarningDisplayTime);
            }

            // 3. Go directly to main menu
            yield return StartCoroutine(OutroAndLoad());
        }

        private IEnumerator OutroAndLoad()
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            // Fade out the whole screen
            if (ScreenCanvasGroup != null)
            {
                ScreenCanvasGroup.interactable = false;
                ScreenCanvasGroup.blocksRaycasts = false;
                yield return StartCoroutine(FadeCanvasGroup(ScreenCanvasGroup, 1f, 0f, FadeSpeed));
            }

            // Load the main menu scene
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float speed)
        {
            float elapsed = 0f;
            cg.alpha = start;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * speed;
                cg.alpha = Mathf.Lerp(start, end, elapsed);
                yield return null;
            }

            cg.alpha = end;
        }
    }
}
