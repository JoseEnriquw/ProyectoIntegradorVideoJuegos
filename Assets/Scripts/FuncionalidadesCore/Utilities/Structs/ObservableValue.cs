using System.Collections.Generic;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Valor observable que detecta cambios. Útil para opciones de configuración.
    /// </summary>
    public class ObservableValue<T>
    {
        private T _value;
        private bool _isChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    _isChanged = true;
                }
            }
        }

        /// <summary>Establece el valor sin marcar como cambiado.</summary>
        public T SilentValue
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>Si el valor ha sido modificado desde la última vez que se reseteó.</summary>
        public bool IsChanged => _isChanged;

        public ObservableValue(T initialValue)
        {
            _value = initialValue;
            _isChanged = false;
        }

        public ObservableValue()
        {
            _value = default;
            _isChanged = false;
        }

        /// <summary>Resetea la bandera de cambio.</summary>
        public void ResetFlag() => _isChanged = false;

        public override string ToString() => $"[{_isChanged}] {_value}";
    }
}
