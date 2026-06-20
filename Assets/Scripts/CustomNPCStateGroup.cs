using UnityEngine;
using UHFPS.Runtime;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    /// <summary>
    /// Contenedor maestro dinámico y altamente personalizable para cualquier tipo de NPC.
    /// Soporta NPCs pasivos (interacciones) o enemigos complejos con estadísticas ajustables.
    /// </summary>
    [CreateAssetMenu(fileName = "CustomNPCStateGroup", menuName = "UHFPS/AI/Custom NPC State Group")]
    public class CustomNPCStateGroup : AIStatesGroup
    {
        [Header("Configuraciones del Animador (Nombres exactos)")]
        [Tooltip("Nombre del parámetro Bool en el Animator para cuando está quieto")]
        public string IdleParameter = "Idle";
        [Tooltip("Nombre del parámetro Bool en el Animator para cuando camina / patrulla")]
        public string WalkParameter = "Walk";
        [Tooltip("Nombre del parámetro Bool en el Animator para cuando corre (Persecución)")]
        public string RunParameter = "Run";
        
        [Header("Animaciones Especiales")]
        [Tooltip("Parámetro (Trigger o Bool) para hacer gestos o interactuar en rutinas comunes")]
        public string InteractParameter = "Interact";
        [Tooltip("Parámetro (Trigger) utilizado cuando lanza un ataque / atrapa al player")]
        public string AttackTrigger = "Attack";
        [Tooltip("Nombre del parámetro Bool en el Animator para apuntar al jugador (Vigilante)")]
        public string PointingParameter = "Pointing";

        // ─────────────────────────────────────────────────────────────
        //  SISTEMA DE SONIDO PARA TERROR
        // ─────────────────────────────────────────────────────────────

        [Header("① Alerta — Al detectar al jugador")]
        [Tooltip("Sonidos que emite el NPC al detectar al jugador. Se elige uno al azar. " +
                 "Ideal: gritos, rugidos, frases de pánico.")]
        public AudioClip[] sonidosAlerta;

        [Range(0f, 1f)]
        public float volumenAlerta = 1f;

        // ─────────────────────────────────────────────────────────────

        [Header("② Sonidos periódicos durante la persecución")]
        [Tooltip("¿Repetir sonidos de voz mientras persigue al jugador? " +
                 "Activa esto para que el NPC gruña/amenace continuamente.")]
        public bool repetirDurantePersecucion = true;

        [Tooltip("Tiempo mínimo (seg.) entre cada sonido periódico de persecución")]
        public float intervaloSonidoMin = 3f;

        [Tooltip("Tiempo máximo (seg.) entre cada sonido periódico de persecución. " +
                 "La variación aleatoria lo hace más impredecible y aterrador.")]
        public float intervaloSonidoMax = 7f;

        [Tooltip("Sonidos que se reproducen periódicamente mientras corre detrás del jugador. " +
                 "Ideal: respiración agitada, amenazas, gruñidos entrecortados.")]
        public AudioClip[] sonidosPersecucion;

        [Range(0f, 1f)]
        public float volumenPersecucion = 0.85f;

        // ─────────────────────────────────────────────────────────────

        [Header("③ Sonido ambiental en loop durante la persecución")]
        [Tooltip("¿Reproducir un sonido en loop durante toda la persecución? " +
                 "Ideal: música de tensión, zumbidos, pasos rítmicos, latidos.")]
        public bool usarSonidoLoop = false;

        [Tooltip("Clip que se reproduce en LOOP mientras dura la persecución. " +
                 "Requiere que el NPC tenga un segundo AudioSource, o se usa el principal si está disponible.")]
        public AudioClip sonidoLoop;

        [Range(0f, 1f)]
        public float volumenLoop = 0.6f;

        // ─────────────────────────────────────────────────────────────

        [Header("④ Sonido de Ataque")]
        [Tooltip("Sonidos que se reproducen cuando el NPC golpea/atrapa al jugador. " +
                 "Ideal: impactos, gritos de victoria, rugidos cortos.")]
        public AudioClip[] sonidosAtaque;

        [Range(0f, 1f)]
        public float volumenAtaque = 1f;

        // ─────────────────────────────────────────────────────────────

        [Header("⑤ Sonido al perder al jugador")]
        [Tooltip("Sonidos que se reproducen cuando el NPC pierde de vista al jugador y abandona la búsqueda. " +
                 "Ideal: gruñidos de frustración, murmullos amenazantes, silencio tenso con un clip corto.")]
        public AudioClip[] sonidosPerdidaVista;

        [Range(0f, 1f)]
        public float volumenPerdidaVista = 0.9f;

        // ─────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────

        /// <summary>Devuelve un clip de alerta al azar.</summary>
        public AudioClip ObtenerSonidoAleatorio(AudioClip[] array)
        {
            if (array == null || array.Length == 0) return null;
            // Filtramos nulls por si algún slot quedó vacío en el Inspector
            var validos = System.Array.FindAll(array, c => c != null);
            if (validos.Length == 0) return null;
            return validos[Random.Range(0, validos.Length)];
        }

        // ─────────────────────────────────────────────────────────────
        //  SISTEMA DE DAÑO
        // ─────────────────────────────────────────────────────────────

        [Header("Sistema de Daño / Atrápalo (Para enemigos)")]
        [Tooltip("¿Si marca al jugador lo mata instantáneamente sin importar la vida?")]
        public bool InstakillOnCatch = false;
        
        [Tooltip("Si NO hace instakill, ¿cuánto daño baja? Rango: Mínimo (X) y Máximo (Y)")]
        public MinMaxInt DamageRange = new MinMaxInt(20, 35);
        
        [Tooltip("Distancia mínima recomendada a la que impacta el ataque o se considera que fue atrapado.")]
        public float RangoAtaque = 1.5f;

        /// <summary>Limpia los bools de movimiento del Animator entre transiciones.</summary>
        public void ResetMovementParameters(Animator animator)
        {
            if(animator == null) return;
            animator.SetBool(IdleParameter, false);
            animator.SetBool(WalkParameter, false);
            animator.SetBool(RunParameter, false);
        }
    }
}
