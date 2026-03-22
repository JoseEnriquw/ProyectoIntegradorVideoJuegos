using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Base abstracta para componentes del jugador.
    /// Provee acceso al PlayerManager buscando en la jerarquía de transforms.
    /// </summary>
    public abstract class PlayerComponentCore : MonoBehaviour
    {
        protected bool isEnabled = true;

        /// <summary>Habilita o deshabilita el componente lógicamente.</summary>
        public virtual void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        /// <summary>Estado lógico de habilitación del componente.</summary>
        public bool IsEnabled => isEnabled;
    }
}
