using System.Collections.Generic;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Buffer de datos que almacena pares clave-valor para serialización y transferencia de datos.
    /// Hereda de Dictionary{string, object} con métodos de acceso tipado.
    /// </summary>
    public class StorableCollection : Dictionary<string, object>
    {
        /// <summary>
        /// Obtiene un valor tipado por clave. Lanza excepción si no existe o no se puede convertir.
        /// </summary>
        public T Get<T>(string key)
        {
            if (TryGetValue(key, out var value))
                if (value is T valueT)
                    return valueT;

            throw new System.NullReferenceException($"Could not find item with key '{key}' or could not convert to type '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Intenta obtener un valor tipado por clave. Retorna true si se pudo obtener.
        /// </summary>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (TryGetValue(key, out var valueO))
            {
                if (valueO is T valueT)
                {
                    value = valueT;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
