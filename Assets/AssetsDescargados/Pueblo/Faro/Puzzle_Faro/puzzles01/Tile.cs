using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2Int correctPos;
    public Vector2Int gridPos;

    private PuzzleManager manager;
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(PuzzleManager m, Vector2Int startPos, float spacing)
    {
        manager = m;
        correctPos = startPos;

        SetGridPosition(startPos, spacing);

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void SetGridPosition(Vector2Int pos, float spacing)
    {
        gridPos = pos;

        rect.anchoredPosition = new Vector2(
            pos.x * spacing,
            pos.y * spacing
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