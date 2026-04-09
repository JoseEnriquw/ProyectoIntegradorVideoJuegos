using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2Int correctPos;
    public Vector2Int gridPos;

    private PuzzleManager manager;
    private RectTransform rectTransform;
    private Button button;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    void Start()
    {
        manager = FindObjectOfType<PuzzleManager>();
        button.onClick.AddListener(OnClick);
    }

    public void SetPosition(Vector2Int newPos, float spacing)
    {
        gridPos = newPos;

        rectTransform.anchoredPosition = new Vector2(
            newPos.x * spacing,
            newPos.y * spacing
        );
    }

    void Update()
    {
        // 🔥 SOLO habilita click si es movible
        if (manager != null)
        {
            button.interactable = manager.IsTileMovable(this);
        }
    }

    void OnClick()
    {
        // 🔒 doble validación (seguridad)
        if (!manager.IsTileMovable(this)) return;

        manager.Move(this);
    }

    public bool IsCorrect()
    {
        return gridPos == correctPos;
    }
}