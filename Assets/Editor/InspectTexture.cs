using UnityEngine;
using UnityEditor;

public class InspectTexture
{
    [MenuItem("Tools/Inspect Texture Importers")]
    public static void Inspect()
    {
        string[] paths = {
            "Assets/Sprites/letreros/controls/mouse.png",
            "Assets/Sprites/letreros/controls/crouch_icon.png"
        };

        foreach (var path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[InspectTexture] Failed to get importer for {path}");
                continue;
            }

            Debug.Log($"[InspectTexture] Path: {path}\n" +
                      $"textureType: {importer.textureType}\n" +
                      $"alphaSource: {importer.alphaSource}\n" +
                      $"alphaIsTransparency: {importer.alphaIsTransparency}\n" +
                      $"mipmapEnabled: {importer.mipmapEnabled}\n" +
                      $"wrapMode: {importer.wrapMode}\n" +
                      $"filterMode: {importer.filterMode}\n" +
                      $"maxTextureSize: {importer.maxTextureSize}");
        }
    }
}
