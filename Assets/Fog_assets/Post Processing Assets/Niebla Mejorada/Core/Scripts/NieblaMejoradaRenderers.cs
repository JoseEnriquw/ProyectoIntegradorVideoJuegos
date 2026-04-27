using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NieblaMejorada.Core
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView] // We need this flag
    public class NieblaMejoradaRenderers : MonoBehaviour
    {
        public List<CustomRenderer> depthRenderers = new List<CustomRenderer>();
        public List<CustomRenderer> fogOffsetRenderers = new List<CustomRenderer>();
    }


    [Serializable]
    public class CustomRenderer
    {
        public bool render = true;
        public bool alwaysRender = true;
        public Renderer renderer;
        public bool drawAllSubmeshes = false;
        public Material material;
    }
}