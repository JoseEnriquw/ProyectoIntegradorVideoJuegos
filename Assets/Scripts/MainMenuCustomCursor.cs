using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuCustomCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public Texture2D cursorTexture;
    public Vector2 hotspot = new Vector2(10f, 6f); // Pointing finger tip position
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        ApplyCustomCursor();
    }

    void OnEnable()
    {
        ApplyCustomCursor();
    }

    void OnDisable()
    {
        ResetCursor();
    }

    void OnDestroy()
    {
        ResetCursor();
    }

    public void ApplyCustomCursor()
    {
        if (cursorTexture != null)
        {
#if UNITY_EDITOR
            // Automatically fix import settings in editor
            string assetPath = AssetDatabase.GetAssetPath(cursorTexture);
            if (!string.IsNullOrEmpty(assetPath))
            {
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Cursor)
                {
                    importer.textureType = TextureImporterType.Cursor;
                    importer.SaveAndReimport();
                    Debug.Log($"[MainMenuCustomCursor] Automatically updated import type of {cursorTexture.name} to Cursor.");
                }
            }
#endif
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        }
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
