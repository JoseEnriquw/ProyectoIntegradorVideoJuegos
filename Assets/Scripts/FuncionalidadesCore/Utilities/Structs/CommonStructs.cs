using System;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Wrapper de Layer de Unity con conversiones implícitas y comparación.
    /// </summary>
    [Serializable]
    public struct Layer
    {
        public int index;
        public static implicit operator int(Layer layer) => layer.index;
        public static implicit operator Layer(int intVal) { Layer r = default; r.index = intVal; return r; }
        public bool CompareLayer(GameObject obj) => obj.layer == this;
    }

    /// <summary>
    /// Wrapper de Tag de Unity con conversiones implícitas y comparación.
    /// </summary>
    [Serializable]
    public struct Tag
    {
        public string tag;
        public static implicit operator string(Tag tag) => tag.tag;
        public static implicit operator Tag(string tag) { Tag r = default; r.tag = tag; return r; }
        public bool CompareTag(GameObject obj) => obj.CompareTag(this);
    }

    /// <summary>
    /// Wrapper de AudioClip con volumen configurable.
    /// </summary>
    [Serializable]
    public sealed class SoundClip
    {
        public AudioClip audioClip;
        public float volume = 1f;

        public SoundClip(AudioClip audioClip, float volume = 1f)
        {
            this.audioClip = audioClip;
            this.volume = volume;
        }
    }

    /// <summary>
    /// Porcentaje serializable [0-100].
    /// </summary>
    [Serializable]
    public struct Percentage
    {
        [Range(0, 100)]
        public float value;

        public Percentage(float value) { this.value = Mathf.Clamp(value, 0, 100); }
        public float Ratio => value / 100f;
        public static implicit operator float(Percentage p) => p.value;
    }
}
