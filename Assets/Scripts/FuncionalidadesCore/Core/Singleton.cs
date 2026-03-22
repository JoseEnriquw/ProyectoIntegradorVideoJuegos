using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Base genérica para implementar el patrón Singleton en MonoBehaviours.
    /// Garantiza una única instancia accesible globalmente via Singleton.Instance.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        /// <summary>
        /// Acceso global a la instancia del Singleton.
        /// Si no existe, lo busca en la escena usando FindFirstObjectByType.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<T>();

                return _instance;
            }
        }

        /// <summary>
        /// Verifica si la instancia existe sin crear/buscar una nueva.
        /// </summary>
        public static bool HasReference => _instance != null;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
            {
                Debug.LogWarning($"[Singleton] There are multiple instances of {typeof(T).Name} in the scene!");
            }
        }
#endif
    }
}
