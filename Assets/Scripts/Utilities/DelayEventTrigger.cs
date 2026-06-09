using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class DelayEventTrigger : MonoBehaviour
    {
        [Header("Ajustes del Temporizador")]
        [Tooltip("La cantidad de segundos a esperar antes de disparar el evento.")]
        public float DelaySeconds = 2f;

        [Tooltip("Usa tiempo real (no afectado por pausas o escala de tiempo del juego).")]
        public bool UseUnscaledTime = false;

        [Tooltip("Si se marca, el temporizador comenzará automáticamente en el método Start().")]
        public bool AutoStartOnStart = false;

        [Tooltip("Si se marca, el temporizador comenzará automáticamente cada vez que el GameObject se active.")]
        public bool AutoStartOnEnable = false;

        [Header("Eventos")]
        [Tooltip("Evento que se dispara cuando se completa el tiempo de espera.")]
        public UnityEvent OnDelayComplete;

        private Coroutine delayCoroutine;

        private void OnEnable()
        {
            if (AutoStartOnEnable)
            {
                StartDelay();
            }
        }

        private void Start()
        {
            if (AutoStartOnStart)
            {
                StartDelay();
            }
        }

        /// <summary>
        /// Inicia el temporizador con el tiempo configurado por defecto.
        /// </summary>
        public void StartDelay()
        {
            StartDelay(DelaySeconds);
        }

        /// <summary>
        /// Inicia el temporizador con una cantidad de segundos personalizada.
        /// </summary>
        public void StartDelay(float seconds)
        {
            CancelDelay();
            delayCoroutine = StartCoroutine(DelayRoutine(seconds));
        }

        /// <summary>
        /// Cancela el temporizador si está en ejecución.
        /// </summary>
        public void CancelDelay()
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }
        }

        private IEnumerator DelayRoutine(float seconds)
        {
            if (UseUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(seconds);
            }
            else
            {
                yield return new WaitForSeconds(seconds);
            }

            OnDelayComplete?.Invoke();
            delayCoroutine = null;
        }

        private void OnDisable()
        {
            CancelDelay();
        }
    }
}
