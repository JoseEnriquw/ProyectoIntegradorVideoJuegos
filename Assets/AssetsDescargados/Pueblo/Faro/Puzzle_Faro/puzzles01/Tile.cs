using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("Grid Logic")]
    public Vector2Int correctPos;
    public Vector2Int gridPos;

    private PuzzleManager manager;
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(PuzzleManager m, Vector2Int startPos)
    {
        manager = m;
        correctPos = startPos;

        SetGridPosition(startPos);

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void SetGridPosition(Vector2Int pos)
    {
        gridPos = pos;

        int size = manager.size;

        // 🔥 tamaño REAL del tile (esto arregla TODO)
        float tileSize = rect.sizeDelta.x;

        // 🔥 separación automática (10% del tamaño)
        float spacing = tileSize * 1.1f;

        float totalSize = spacing * (size - 1);
        float offset = totalSize / 2f;

        rect.anchoredPosition = new Vector2(
            (pos.x * spacing) - offset,
            (pos.y * spacing) - offset
        );
    }

    void OnClick()
    {
        manager.TryMove(this);
    }

    public bool IsCorrect()
    {
        return gridPos == correctPos;
    }
}