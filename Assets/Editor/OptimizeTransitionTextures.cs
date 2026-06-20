using UnityEngine;
using UnityEditor;
using System.IO;

public class OptimizeTransitionTextures
{
    [MenuItem("Tools/Optimize Transition Textures")]
    public static void OptimizeTextures()
    {
        string folderPath = "Assets/Sprites/UI/Transicion/Nuevas";
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"[OptimizeTransitionTextures] Directory not found: {folderPath}");
            return;
        }

        string[] fileEntries = Directory.GetFiles(folderPath, "*.png");
        int count = 0;

        foreach (string filePath in fileEntries)
        {
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                Undo.RecordObject(importer, "Optimize Transition Texture Import Settings");
                
                // Configure for high quality UI display
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed; // No compression for maximum quality
                importer.alphaIsTransparency = true;
                
                importer.SaveAndReimport();
                Debug.Log($"[OptimizeTransitionTextures] Optimized: {filePath}");
                count++;
            }
        }

        Debug.Log($"[OptimizeTransitionTextures] Successfully optimized {count} transition textures to Uncompressed format.");
    }
}
