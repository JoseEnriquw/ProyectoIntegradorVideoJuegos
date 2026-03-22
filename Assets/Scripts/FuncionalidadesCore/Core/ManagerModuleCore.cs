using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Base abstracta para módulos de manager.
    /// Los módulos extienden funcionalidad del GameContext con lifecycle propio (Awake/Start/Update).
    /// Desacoplado: usa IGameContext en lugar de GameManager concreto.
    /// </summary>
    [System.Serializable]
    public abstract class ManagerModuleCore
    {
        /// <summary>Referencia al contexto de juego (inyectada por el manager).</summary>
        public IGameContext GameContext { get; internal set; }

        /// <summary>Referencia al MonoBehaviour propietario (para coroutines, etc.).</summary>
        public MonoBehaviour Owner { get; internal set; }

        /// <summary>Se llama una vez al inicializar (equivalente a Awake).</summary>
        public virtual void OnAwake() { }

        /// <summary>Se llama una vez después de OnAwake (equivalente a Start).</summary>
        public virtual void OnStart() { }

        /// <summary>Se llama cada frame (equivalente a Update).</summary>
        public virtual void OnUpdate() { }
    }
}
