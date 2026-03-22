using System;
using System.Collections;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Utility para hacer fade in/out de CanvasGroups.
    /// </summary>
    public class CanvasGroupFader : MonoBehaviour
    {
        /// <summary>
        /// Inicia un fade creando un GameObject temporal que se autodestruye.
        /// </summary>
        public static void StartFadeInstance(CanvasGroup canvasGroup, bool fadeIn, float speed, Action onFade = null)
        {
            GameObject fadeGO = new("CanvasGroupFader");
            CanvasGroupFader fader = fadeGO.AddComponent<CanvasGroupFader>();
            fader.StartCoroutine(StartFade(canvasGroup, fadeIn, speed, () =>
            {
                onFade?.Invoke();
                Destroy(fadeGO);
            }));
        }

        /// <summary>
        /// Coroutine que realiza el fade de un CanvasGroup.
        /// </summary>
        public static IEnumerator StartFade(CanvasGroup canvasGroup, bool fadeIn, float speed, Action onFade = null)
        {
            canvasGroup.gameObject.SetActive(true);
            float currAlpha = canvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;

            while (fadeIn ? currAlpha < 1 : currAlpha > 0)
            {
                currAlpha = Mathf.MoveTowards(currAlpha, targetAlpha, Time.deltaTime * speed);
                canvasGroup.alpha = currAlpha;
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            onFade?.Invoke();
        }
    }

    /// <summary>
    /// Ejecuta una coroutine como componente temporal, se autodestruye al finalizar.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private IEnumerator coroutine;
        private CoroutineRunner self;

        /// <summary>Ejecuta una coroutine y retorna el Coroutine handle.</summary>
        public static Coroutine RunGet(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner.coroutine = coroutine;
            runner.self = runner;
            return runner.StartCoroutine(runner.RunCoroutine());
        }

        /// <summary>Ejecuta una coroutine y retorna el CoroutineRunner.</summary>
        public static CoroutineRunner Run(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner.coroutine = coroutine;
            runner.self = runner;
            runner.StartCoroutine(runner.RunCoroutine());
            return runner;
        }

        /// <summary>Detiene la coroutine y destruye el componente.</summary>
        public void Stop()
        {
            StopAllCoroutines();
            Destroy(self);
        }

        public IEnumerator RunCoroutine()
        {
            yield return coroutine;
            Destroy(self);
        }
    }
}
