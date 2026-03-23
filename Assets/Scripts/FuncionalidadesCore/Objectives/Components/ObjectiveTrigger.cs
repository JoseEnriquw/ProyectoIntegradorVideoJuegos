using UnityEngine;

namespace FuncionalidadesCore.Objectives
{
    /// <summary>
    /// Componente listo para usar (Collider Trigger).
    /// Permite agregar o completar un objetivo/sub-objetivo matemáticamente con solo pisar una zona física.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectiveTrigger : MonoBehaviour
    {
        public enum TriggerAction { AgregarObjetivo, CompletarSubObjetivo, CompletarObjetivoFinal }
        
        [Header("Búsqueda del Manager")]
        [Tooltip("El script buscará e inyectará automáticamente el ObjectiveManagerCore de tu escena.")]
        private ObjectiveManagerCore objectiveManager;

        [Header("Acción al pisar el Trigger")]
        public TriggerAction Accion;
        public string ObjectiveGUID;
        [Tooltip("Obligatorio solo si la acción es CompletarSubObjetivo")]
        public string SubObjectiveGUID;
        [Tooltip("Cantidad a agregar para un SubObjetivo que requiera recolectar cosas (Ej: 1/5)")]
        public ushort IncrementAmount = 1;

        [Header("Opciones")]
        public string TagRequerido = "Player";
        public bool SoloUnaVez = true;
        
        private bool yaFired = false;

        private void Start()
        {
            objectiveManager = FindObjectOfType<ObjectiveManagerCore>();
            
            // Asegurarnos de que el collider actúe como trigger
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (SoloUnaVez && yaFired) return;

            if (!string.IsNullOrEmpty(TagRequerido) && !other.CompareTag(TagRequerido))
                return;

            if (objectiveManager == null)
            {
                Debug.LogError("[ObjectiveTrigger] Faltó poner un ObjectiveManagerCore en la escena.");
                return;
            }

            RealizarAccion();
        }

        private void RealizarAccion()
        {
            switch (Accion)
            {
                case TriggerAction.AgregarObjetivo:
                    objectiveManager.AddObjective(ObjectiveGUID);
                    break;
                case TriggerAction.CompletarSubObjetivo:
                    objectiveManager.CompleteSubObjective(ObjectiveGUID, SubObjectiveGUID, IncrementAmount);
                    break;
                case TriggerAction.CompletarObjetivoFinal:
                    objectiveManager.CompleteObjective(ObjectiveGUID);
                    break;
            }

            yaFired = true;
            Debug.Log($"[ObjectiveTrigger] Acción {Accion} ejecutada en objetivo '{ObjectiveGUID}'.");
        }
    }
}
