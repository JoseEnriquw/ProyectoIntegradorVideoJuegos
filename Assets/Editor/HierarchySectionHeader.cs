using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class HierarchySectionHeader
{
    static HierarchySectionHeader()
    {
        // Nos suscribimos al evento que dibuja la ventana de la jerarquía
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.EntityIdToObject((EntityId)instanceID);
        var gameObject = obj as GameObject;

        // Comprobamos si existe y si su nombre empieza con "---"
        if (gameObject != null && gameObject.name.StartsWith("---", System.StringComparison.Ordinal))
        {
            // Dibujamos un fondo de color gris oscuro por encima del texto original
            EditorGUI.DrawRect(selectionRect, new Color(0.18f, 0.18f, 0.18f, 1f));

            // Limpiamos los guiones para quedarnos solo con el texto ("--- PLAYER ---" -> "PLAYER")
            string cleanerName = gameObject.name.Replace("-", "").Trim();

            // Creamos un estilo para centrar el texto y ponerlo en negrita
            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            textStyle.normal.textColor = Color.white;

            // Dibujamos nuestro propio texto centrado
            EditorGUI.LabelField(selectionRect, cleanerName, textStyle);
        }
    }
}
