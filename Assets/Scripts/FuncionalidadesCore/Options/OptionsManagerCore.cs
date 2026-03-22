using System;
using System.Collections.Generic;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Observable simple que notifica a suscriptores cuando el valor cambia.
    /// Reemplaza BehaviorSubject de System.Reactive sin dependencia externa.
    /// </summary>
    public class OptionSubject
    {
        private object _value;
        private readonly List<Action<object>> _observers = new();

        public object Value => _value;

        public OptionSubject(object initialValue)
        {
            _value = initialValue;
        }

        /// <summary>Establece un nuevo valor y notifica a todos los observers.</summary>
        public void SetValue(object newValue)
        {
            _value = newValue;
            for (int i = _observers.Count - 1; i >= 0; i--)
            {
                _observers[i]?.Invoke(_value);
            }
        }

        /// <summary>Suscribe un observer. Notifica inmediatamente con el valor actual.</summary>
        public void Subscribe(Action<object> observer)
        {
            _observers.Add(observer);
            observer?.Invoke(_value);
        }

        /// <summary>Remueve un observer.</summary>
        public void Unsubscribe(Action<object> observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>Remueve todos los observers.</summary>
        public void Clear()
        {
            _observers.Clear();
        }
    }

    /// <summary>
    /// Wrapper serializable para guardar opciones con JsonUtility.
    /// </summary>
    [Serializable]
    public class SerializableOptions
    {
        public List<string> keys = new();
        public List<string> values = new();

        public void Set(string key, string value)
        {
            int idx = keys.IndexOf(key);
            if (idx >= 0)
                values[idx] = value;
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public string Get(string key, string defaultValue = "")
        {
            int idx = keys.IndexOf(key);
            return idx >= 0 ? values[idx] : defaultValue;
        }

        public bool HasKey(string key) => keys.Contains(key);
    }

    /// <summary>
    /// Sistema de opciones desacoplado de UI y rendering pipeline.
    /// Sin dependencias externas: usa patrón Observer propio y JsonUtility.
    /// </summary>
    public class OptionsManagerCore : MonoBehaviour
    {
        [Header("Options Settings")]
        [SerializeField] private string optionsFileName = "options.json";
        [SerializeField] private bool showDebug = true;

        private IGraphicsOptionsApplier graphicsApplier;
        private IOptionsPersistence persistence;

        /// <summary>Subjects para observar cambios de opciones.</summary>
        public Dictionary<string, OptionSubject> OptionSubjects { get; private set; } = new();

        /// <summary>Datos serializables de las opciones.</summary>
        public SerializableOptions SerializableData { get; private set; } = new();

        /// <summary>Resolución y Fullscreen actuales.</summary>
        public ObservableValue<Resolution> CurrentResolution { get; private set; } = new();
        public ObservableValue<FullScreenMode> CurrentFullscreen { get; private set; } = new();

        private void OnDestroy()
        {
            foreach (var kvp in OptionSubjects)
                kvp.Value.Clear();
            OptionSubjects.Clear();
        }

        /// <summary>Inyectar implementaciones de gráficos y persistencia.</summary>
        public void Initialize(IGraphicsOptionsApplier graphicsApplier, IOptionsPersistence persistence)
        {
            this.graphicsApplier = graphicsApplier;
            this.persistence = persistence;
        }

        // --- Registro y observación de opciones ---

        /// <summary>Registrar una opción con un valor por defecto.</summary>
        public void RegisterOption(string name, object defaultValue)
        {
            if (!OptionSubjects.ContainsKey(name))
            {
                OptionSubjects[name] = new OptionSubject(defaultValue);
            }
        }

        /// <summary>Observar cambios en una opción específica. Notifica inmediatamente con valor actual.</summary>
        public void ObserveOption(string name, Action<object> onChange)
        {
            if (OptionSubjects.TryGetValue(name, out var subject))
            {
                subject.Subscribe(onChange);
            }
            else
            {
                Debug.LogWarning($"[OptionsManager] Option '{name}' not registered.");
            }
        }

        /// <summary>Establecer el valor de una opción.</summary>
        public void SetOptionValue(string name, object value)
        {
            if (OptionSubjects.TryGetValue(name, out var subject))
            {
                subject.SetValue(value);
                SerializableData.Set(name, value?.ToString() ?? "");
            }
        }

        /// <summary>Obtener el valor actual de una opción.</summary>
        public T GetOptionValue<T>(string name, T defaultValue = default)
        {
            if (OptionSubjects.TryGetValue(name, out var subject))
            {
                if (subject.Value is T typedValue)
                    return typedValue;
            }
            return defaultValue;
        }

        // --- Aplicar y guardar ---

        /// <summary>Aplicar todas las opciones pendientes y guardar.</summary>
        public async void ApplyOptions()
        {
            // Aplicar resolución
            if (CurrentResolution.IsChanged || CurrentFullscreen.IsChanged)
            {
                graphicsApplier?.ApplyResolution(
                    CurrentResolution.Value.width,
                    CurrentResolution.Value.height,
                    CurrentFullscreen.Value);

                CurrentResolution.ResetFlag();
                CurrentFullscreen.ResetFlag();
            }

            // Serializar y guardar
            string json = JsonUtility.ToJson(SerializableData, true);

            if (persistence != null)
                await persistence.SaveOptions(json);

            if (showDebug) Debug.Log("[OptionsManager] Options applied and saved.");
        }

        /// <summary>Cargar opciones desde almacenamiento.</summary>
        public async void LoadOptions()
        {
            if (persistence == null || !persistence.HasSavedOptions) return;

            string json = await persistence.LoadOptions();
            if (!string.IsNullOrEmpty(json))
            {
                SerializableData = JsonUtility.FromJson<SerializableOptions>(json);

                // Notificar a todos los observers
                for (int i = 0; i < SerializableData.keys.Count; i++)
                {
                    string key = SerializableData.keys[i];
                    if (OptionSubjects.TryGetValue(key, out var subject))
                    {
                        subject.SetValue(SerializableData.values[i]);
                    }
                }

                if (showDebug) Debug.Log("[OptionsManager] Options loaded from file.");
            }
        }

        /// <summary>Descartar cambios pendientes.</summary>
        public void DiscardChanges()
        {
            LoadOptions();
            if (showDebug) Debug.Log("[OptionsManager] Options discarded.");
        }
    }
}
