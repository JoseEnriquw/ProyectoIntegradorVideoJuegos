using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Interfaz para objetos que pueden guardar y cargar su estado.
    /// Usa StorableCollection (Dictionary) en lugar de JToken para evitar dependencia con Newtonsoft.Json.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Guarda el estado del objeto. Retorna un StorableCollection con los datos.</summary>
        StorableCollection OnSave();

        /// <summary>Carga el estado del objeto desde un StorableCollection.</summary>
        void OnLoad(StorableCollection data);
    }

    /// <summary>
    /// Interfaz para objetos que pueden ser instanciados en runtime y necesitan guardarse.
    /// </summary>
    public interface IRuntimeSaveable : ISaveable
    {
        /// <summary>ID único para identificar el objeto.</summary>
        UniqueID UniqueID { get; set; }
    }

    /// <summary>
    /// Interfaz para guardado/carga personalizado (complementario a ISaveable).
    /// </summary>
    public interface ISaveableCustom
    {
        StorableCollection OnCustomSave();
        void OnCustomLoad(StorableCollection data);
    }

    // =============================================
    // STRUCTS DE GUARDADO
    // Conversores entre tipos Unity y tipos serializables.
    // =============================================

    [System.Serializable]
    public struct SaveableVector2
    {
        public float x, y;
        public SaveableVector2(float x, float y) { this.x = x; this.y = y; }
        public SaveableVector2(Vector2 v) { x = v.x; y = v.y; }
        public Vector2 ToVector2() => new(x, y);
    }

    [System.Serializable]
    public struct SaveableVector3
    {
        public float x, y, z;
        public SaveableVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public SaveableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToVector3() => new(x, y, z);
    }

    [System.Serializable]
    public struct SaveableVector3Int
    {
        public int x, y, z;
        public SaveableVector3Int(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public SaveableVector3Int(Vector3Int v) { x = v.x; y = v.y; z = v.z; }
        public Vector3Int ToVector3Int() => new(x, y, z);
    }

    /// <summary>
    /// Métodos de extensión para guardar/cargar transforms y vectores.
    /// </summary>
    public static class SaveableExtensions
    {
        public static SaveableVector2 ToSaveable(this Vector2 v) => new(v);
        public static SaveableVector3 ToSaveable(this Vector3 v) => new(v);
        public static SaveableVector3Int ToSaveable(this Vector3Int v) => new(v);

        /// <summary>Agrega posición y rotación de un transform al StorableCollection.</summary>
        public static StorableCollection WithTransform(this StorableCollection sc, Transform t, bool includeScale = false)
        {
            sc.Add("position", t.position.ToSaveable());
            sc.Add("rotation", t.eulerAngles.ToSaveable());
            if (includeScale) sc.Add("scale", t.localScale.ToSaveable());
            return sc;
        }

        /// <summary>Carga posición, rotación y escala desde un StorableCollection a un Transform.</summary>
        public static void LoadTransform(this StorableCollection data, Transform t)
        {
            if (data.TryGetValue<SaveableVector3>("position", out var pos))
                t.position = pos.ToVector3();

            if (data.TryGetValue<SaveableVector3>("rotation", out var rot))
                t.eulerAngles = rot.ToVector3();

            if (data.TryGetValue<SaveableVector3>("scale", out var scale))
                t.localScale = scale.ToVector3();
        }
    }
}
