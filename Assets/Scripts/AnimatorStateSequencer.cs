using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorStateSequencer : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationStep
    {
        [Tooltip("Nombre del estado de animación en el Animator Controller")]
        public string stateName;

        [Tooltip("Duración en segundos que permanecerá en este estado antes de pasar al siguiente")]
        public float duration;

        [Range(0f, 2f)]
        [Tooltip("Duración de la transición de mezcla (Crossfade) en segundos (0 para cambio instantáneo)")]
        public float transitionDuration;

        [Tooltip("Desplazamiento de posición (Local) para la pierna izquierda superior (mover muslo a los lados)")]
        public Vector3 leftLegPositionOffset;

        [Tooltip("Desplazamiento de posición (Local) para la pierna derecha superior (mover muslo a los lados)")]
        public Vector3 rightLegPositionOffset;

        [Tooltip("Rotación adicional (Euler Angles) para la pierna izquierda superior (muslo)")]
        public Vector3 leftLegRotationOffset;

        [Tooltip("Rotación adicional (Euler Angles) para la pierna derecha superior (muslo)")]
        public Vector3 rightLegRotationOffset;

        [Tooltip("Rotación adicional (Euler Angles) para la pierna izquierda inferior (pantorrilla/rodilla)")]
        public Vector3 leftLowerLegRotationOffset;

        [Tooltip("Rotación adicional (Euler Angles) para la pierna derecha inferior (pantorrilla/rodilla)")]
        public Vector3 rightLowerLegRotationOffset;

        [Tooltip("Rotación adicional (Euler Angles) para el pie izquierdo")]
        public Vector3 leftFootRotationOffset;

        [Tooltip("Rotación adicional (Euler Angles) para el pie derecho")]
        public Vector3 rightFootRotationOffset;
    }

    [Header("Secuencia de Animaciones")]
    [Tooltip("Lista ordenada de animaciones a reproducir")]
    public List<AnimationStep> steps = new List<AnimationStep>();

    [Tooltip("¿Debería reiniciarse la secuencia en bucle una vez termine?")]
    public bool loop = false;

    [Header("Ajustes de Posición")]
    [Tooltip("Bloquear la posición y rotación inicial del GameObject para evitar desplazamientos accidentales")]
    public bool lockRootTransform = true;

    [Tooltip("Bloquear la posición X y Z local del hueso Hips (cadera) para evitar desplazamientos/deslizamientos de la malla")]
    public bool lockHipsPosition = true;

    private Animator animator;
    private Coroutine sequenceCoroutine;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Transform hipsTransform;
    private Vector3 initialHipsLocalPosition;

    private Transform leftLegTransform;
    private Transform rightLegTransform;
    private Transform leftLowerLegTransform;
    private Transform rightLowerLegTransform;
    private Transform leftFootTransform;
    private Transform rightFootTransform;

    private Vector3 activeLeftLegPositionOffset;
    private Vector3 activeRightLegPositionOffset;
    private Vector3 activeLeftLegOffset;
    private Vector3 activeRightLegOffset;
    private Vector3 activeLeftLowerLegOffset;
    private Vector3 activeRightLowerLegOffset;
    private Vector3 activeLeftFootOffset;
    private Vector3 activeRightFootOffset;
    private int currentStepIndex = -1;

    private void Start()
    {
        animator = GetComponent<Animator>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // Intentar obtener los huesos si es un rig Humanoid
        if (animator.isHuman)
        {
            hipsTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hipsTransform != null)
            {
                initialHipsLocalPosition = hipsTransform.localPosition;
            }

            leftLegTransform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightLegTransform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            leftLowerLegTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rightLowerLegTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            leftFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning($"[AnimatorStateSequencer] No se han definido pasos de animación en {gameObject.name}");
            return;
        }

        sequenceCoroutine = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
        {
            AnimationStep currentStep = steps[currentStepIndex];

            // Interpolamos suavemente los offsets actuales hacia el objetivo del paso activo.
            float lerpSpeed = currentStep.transitionDuration > 0 ? (1f / currentStep.transitionDuration) : 10f;
            activeLeftLegPositionOffset = Vector3.Lerp(activeLeftLegPositionOffset, currentStep.leftLegPositionOffset, Time.deltaTime * lerpSpeed);
            activeRightLegPositionOffset = Vector3.Lerp(activeRightLegPositionOffset, currentStep.rightLegPositionOffset, Time.deltaTime * lerpSpeed);
            activeLeftLegOffset = Vector3.Lerp(activeLeftLegOffset, currentStep.leftLegRotationOffset, Time.deltaTime * lerpSpeed);
            activeRightLegOffset = Vector3.Lerp(activeRightLegOffset, currentStep.rightLegRotationOffset, Time.deltaTime * lerpSpeed);
            activeLeftLowerLegOffset = Vector3.Lerp(activeLeftLowerLegOffset, currentStep.leftLowerLegRotationOffset, Time.deltaTime * lerpSpeed);
            activeRightLowerLegOffset = Vector3.Lerp(activeRightLowerLegOffset, currentStep.rightLowerLegRotationOffset, Time.deltaTime * lerpSpeed);
            activeLeftFootOffset = Vector3.Lerp(activeLeftFootOffset, currentStep.leftFootRotationOffset, Time.deltaTime * lerpSpeed);
            activeRightFootOffset = Vector3.Lerp(activeRightFootOffset, currentStep.rightFootRotationOffset, Time.deltaTime * lerpSpeed);
        }
    }

    private void LateUpdate()
    {
        if (lockRootTransform)
        {
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
        }

        if (lockHipsPosition && hipsTransform != null)
        {
            Vector3 hipsPos = hipsTransform.localPosition;
            hipsPos.x = initialHipsLocalPosition.x;
            hipsPos.z = initialHipsLocalPosition.z;
            hipsTransform.localPosition = hipsPos;
        }

        // Aplicar desplazamientos adicionales de posición a los muslos (abrir pelvis en el espacio local del Hips)
        if (leftLegTransform != null && activeLeftLegPositionOffset != Vector3.zero)
        {
            leftLegTransform.localPosition += activeLeftLegPositionOffset;
        }
        if (rightLegTransform != null && activeRightLegPositionOffset != Vector3.zero)
        {
            rightLegTransform.localPosition += activeRightLegPositionOffset;
        }

        // Aplicar rotaciones adicionales a los muslos (piernas superiores) en espacio de la cadera (multiplicando a la izquierda)
        if (leftLegTransform != null && activeLeftLegOffset != Vector3.zero)
        {
            leftLegTransform.localRotation = Quaternion.Euler(activeLeftLegOffset) * leftLegTransform.localRotation;
        }
        if (rightLegTransform != null && activeRightLegOffset != Vector3.zero)
        {
            rightLegTransform.localRotation = Quaternion.Euler(activeRightLegOffset) * rightLegTransform.localRotation;
        }

        // Aplicar rotaciones adicionales a las rodillas (piernas inferiores) - RESTRINGIDO al eje X local (bisagra) para evitar dislocación
        if (leftLowerLegTransform != null && activeLeftLowerLegOffset != Vector3.zero)
        {
            leftLowerLegTransform.localRotation *= Quaternion.Euler(activeLeftLowerLegOffset.x, 0f, 0f);
        }
        if (rightLowerLegTransform != null && activeRightLowerLegOffset != Vector3.zero)
        {
            rightLowerLegTransform.localRotation *= Quaternion.Euler(activeRightLowerLegOffset.x, 0f, 0f);
        }

        // Aplicar rotaciones adicionales a los pies en espacio de la pantorrilla (multiplicando a la izquierda)
        if (leftFootTransform != null && activeLeftFootOffset != Vector3.zero)
        {
            leftFootTransform.localRotation = Quaternion.Euler(activeLeftFootOffset) * leftFootTransform.localRotation;
        }
        if (rightFootTransform != null && activeRightFootOffset != Vector3.zero)
        {
            rightFootTransform.localRotation = Quaternion.Euler(activeRightFootOffset) * rightFootTransform.localRotation;
        }
    }

    private IEnumerator PlaySequence()
    {
        int currentIndex = 0;

        while (currentIndex < steps.Count)
        {
            currentStepIndex = currentIndex;
            AnimationStep step = steps[currentIndex];
            if (!string.IsNullOrEmpty(step.stateName))
            {
                // Verificar si el estado existe en la capa base (0) del Animator para evitar que el personaje desaparezca/se corrompa
                if (animator.HasState(0, Animator.StringToHash(step.stateName)))
                {
                    if (step.transitionDuration > 0)
                    {
                        animator.CrossFadeInFixedTime(step.stateName, step.transitionDuration);
                    }
                    else
                    {
                        animator.Play(step.stateName);
                    }
                    Debug.Log($"[AnimatorStateSequencer] Reproduciendo animación: {step.stateName} por {step.duration} segundos.");
                }
                else
                {
                    Debug.LogError($"[AnimatorStateSequencer] ERROR: El estado de animación '{step.stateName}' no existe en el Animator Controller de '{gameObject.name}'. Por favor, añádelo en la ventana Animator. Se omitirá este paso.");
                }
            }

            yield return new WaitForSeconds(step.duration);

            currentIndex++;
            if (loop && currentIndex >= steps.Count)
            {
                currentIndex = 0;
            }
        }
    }

    private void OnDisable()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }
    }
}
