using UnityEngine;
using System.Collections;

public class MainMenuNPCAnimationController : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationStep
    {
        [Tooltip("Nombre del estado de animación en el Animator (ej: Walk, Idle, Attack, etc.)")]
        public string stateName;
        
        [Tooltip("Cantidad de segundos que se reproducirá esta animación antes de pasar a la siguiente")]
        public float duration;
        
        [Tooltip("Tiempo de transición de suavizado (Crossfade) en segundos")]
        public float crossfadeTime;
    }

    [Header("Secuencia de Animaciones")]
    public AnimationStep[] animationSequence;

    [Header("Configuración de la Secuencia")]
    public bool loopSequence = true;
    public bool playOnStart = true;

    private Animator animator;
    private Coroutine sequenceCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("MainMenuNPCAnimationController: No se encontró ningún componente Animator en " + name);
            return;
        }

        if (playOnStart && animationSequence != null && animationSequence.Length > 0)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(RunAnimationSequence());
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    private IEnumerator RunAnimationSequence()
    {
        int currentIndex = 0;
        while (true)
        {
            if (animationSequence == null || animationSequence.Length == 0) yield break;

            AnimationStep step = animationSequence[currentIndex];
            if (!string.IsNullOrEmpty(step.stateName))
            {
                float crossfade = Mathf.Max(0f, step.crossfadeTime);
                animator.CrossFade(step.stateName, crossfade);
                Debug.Log($"MainMenuNPCAnimationController: Reproduciendo estado '{step.stateName}' con crossfade de {crossfade}s");
            }

            yield return new WaitForSeconds(step.duration);

            currentIndex++;
            if (currentIndex >= animationSequence.Length)
            {
                if (loopSequence)
                {
                    currentIndex = 0;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}
