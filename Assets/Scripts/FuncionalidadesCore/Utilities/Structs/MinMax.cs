using System;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Estructura para representar un rango float con mínimo y máximo.
    /// Incluye conversiones implícitas a/desde Vector2.
    /// </summary>
    [Serializable]
    public struct MinMax
    {
        public float min;
        public float max;

        public bool Flipped => max < min;
        public bool HasValue => min != 0 || max != 0;
        public float RealMin => Mathf.Min(min, max);
        public float RealMax => Mathf.Max(max, min);
        public Vector2 RealVector => this;
        public Vector2 Vector => new(min, max);

        public MinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator Vector2(MinMax minMax) => new(minMax.RealMin, minMax.RealMax);

        public static implicit operator MinMax(Vector2 vector)
        {
            MinMax result = default;
            result.min = vector.x;
            result.max = vector.y;
            return result;
        }

        public MinMax Flip() => new MinMax(max, min);
        public override string ToString() => $"({RealMin}, {RealMax})";
    }

    /// <summary>
    /// Versión int de MinMax.
    /// </summary>
    [Serializable]
    public struct MinMaxInt
    {
        public int min;
        public int max;

        public bool Flipped => max < min;
        public int RealMin => Flipped ? max : min;
        public int RealMax => Flipped ? min : max;
        public Vector2Int RealVector => this;
        public Vector2Int Vector => new(min, max);

        public MinMaxInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator Vector2Int(MinMaxInt minMax) => new(minMax.RealMin, minMax.RealMax);

        public static implicit operator MinMaxInt(Vector2Int vector)
        {
            MinMaxInt result = default;
            result.min = vector.x;
            result.max = vector.y;
            return result;
        }

        public MinMaxInt Flip() => new MinMaxInt(max, min);
    }
}
