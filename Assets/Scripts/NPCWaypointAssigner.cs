using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Runtime.States
{
    /// <summary>
    /// Agrega este componente al GameObject del NPC para asignarle
    /// manualmente su grupo de waypoints desde el Inspector.
    /// Si este componente está presente, EstadoPatrullajeAI lo usará
    /// en lugar de buscar el grupo más cercano automáticamente.
    /// </summary>
    public class NPCWaypointAssigner : MonoBehaviour
    {
        [Header("Asignación Manual de Ruta")]
        [Tooltip("Arrastra aquí el AIWaypointsGroup que debe seguir este NPC.")]
        public AIWaypointsGroup grupoDeWaypoints;
    }
}
