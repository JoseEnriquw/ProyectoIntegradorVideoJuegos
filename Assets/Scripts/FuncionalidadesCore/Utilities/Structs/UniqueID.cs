using System;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Genera IDs únicos para identificar objetos en el sistema de guardado/carga.
    /// </summary>
    [Serializable]
    public sealed class UniqueID
    {
        public string Id;

        public UniqueID()
        {
            GenerateIfEmpty();
        }

        /// <summary>Genera un ID solo si está vacío.</summary>
        public void GenerateIfEmpty()
        {
            if (!string.IsNullOrEmpty(Id))
                return;

            Generate();
        }

        /// <summary>Genera un nuevo ID único (GUID).</summary>
        public void Generate()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public static implicit operator string(UniqueID uniqueID) => uniqueID.Id;
        public override string ToString() => Id;
    }
}
