using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Interfaz para aplicar opciones gráficas al motor de rendering.
    /// Desacopla el sistema de opciones de URP/HDRP/BiRP.
    /// </summary>
    public interface IGraphicsOptionsApplier
    {
        /// <summary>Aplicar una resolución de pantalla.</summary>
        void ApplyResolution(int width, int height, FullScreenMode mode);

        /// <summary>Aplicar un ajuste de calidad por nombre y valor.</summary>
        void ApplyQualitySetting(string settingName, object value);

        /// <summary>Aplicar antialiasing.</summary>
        void ApplyAntiAliasing(int level);

        /// <summary>Aplicar VSync.</summary>
        void ApplyVSync(bool enabled);
    }

    /// <summary>
    /// Interfaz para persistencia de opciones (guardar/cargar configuración).
    /// </summary>
    public interface IOptionsPersistence
    {
        /// <summary>Guardar opciones en formato JSON.</summary>
        System.Threading.Tasks.Task SaveOptions(string json);

        /// <summary>Cargar opciones desde almacenamiento.</summary>
        System.Threading.Tasks.Task<string> LoadOptions();

        /// <summary>Si existen opciones guardadas.</summary>
        bool HasSavedOptions { get; }
    }
}
