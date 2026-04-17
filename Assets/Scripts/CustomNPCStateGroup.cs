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

        [Header("Sistema de Daño / Atrápalo (Para enemigos)")]
        [Tooltip("¿Si marca al jugador lo mata instantáneamente sin importar la vida?")]
        public bool InstakillOnCatch = false;
        
        [Tooltip("Si NO hace instakill, ¿cuánto daño baja? Rango: Mínimo (X) y Máximo (Y)")]
        public MinMaxInt DamageRange = new MinMaxInt(20, 35);
        
        [Tooltip("Distancia mínima recomendada a la que impacta el ataque o se considera que fue atrapado.")]
        public float RangoAtaque = 1.5f;

        /// <summary>
        /// Método auxiliar para limpiar los estados de movimiento entre transiciones 
        /// (te ahorrará código duplicado en los estados como PatrullajeAI o ChaseAI).
        /// </summary>
        public void ResetMovementParameters(Animator animator)
        {
            if(animator == null) return;
            animator.SetBool(IdleParameter, false);
            animator.SetBool(WalkParameter, false);
            animator.SetBool(RunParameter, false);
        }
    }
}
