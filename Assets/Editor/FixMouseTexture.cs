using UnityEngine;
using UnityEditor;

public class FixMouseTexture
{
    [MenuItem("Tools/Fix Mouse Texture Transparency")]
    public static void FixTexture()
    {
        string path = "Assets/Sprites/letreros/controls/mouse.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[FixMouseTexture] Failed to get importer for {path}");
            return;
        }

        Undo.RecordObject(importer, "Fix Mouse Texture Import Settings");
        importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        Debug.Log("[FixMouseTexture] Successfully updated mouse.png to import FromGrayScale.");
    }
}
