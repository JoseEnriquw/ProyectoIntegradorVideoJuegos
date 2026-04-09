using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public int size = 3;
    public float spacing = 200f;

    public Tile[] tiles;

    private Vector2Int emptyPos;
    private bool isSolved = false;

    void Start()
    {
        Setup();
        Shuffle();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            SolveInstant();
        }
    }

    void Setup()
    {
        int index = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (index < tiles.Length)
                {
                    tiles[index].correctPos = new Vector2Int(x, y);
                    tiles[index].SetPosition(new Vector2Int(x, y), spacing);
                    index++;
                }
                else
                {
                    emptyPos = new Vector2Int(x, y);
                }
            }
        }
    }

    public void Move(Tile tile)
    {
        if (isSolved) return;

        if (!IsAdjacent(tile.gridPos, emptyPos))
            return;

        Vector2Int oldPos = tile.gridPos;

        tile.SetPosition(emptyPos, spacing);
        emptyPos = oldPos;

        CheckWin();
    }

    public bool IsTileMovable(Tile tile)
    {
        if (isSolved) return false;

        return IsAdjacent(tile.gridPos, emptyPos);
    }

    bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    void Shuffle()
    {
        for (int i = 0; i < 100; i++)
        {
            Tile randomTile = tiles[Random.Range(0, tiles.Length)];

            if (IsAdjacent(randomTile.gridPos, emptyPos))
            {
                Vector2Int oldPos = randomTile.gridPos;
                randomTile.SetPosition(emptyPos, spacing);
                emptyPos = oldPos;
            }
        }
    }

    void SolveInstant()
    {
        foreach (Tile tile in tiles)
        {
            tile.SetPosition(tile.correctPos, spacing);
        }

        CheckWin();
    }

    void CheckWin()
    {
        foreach (Tile tile in tiles)
        {
            if (!tile.IsCorrect())
                return;
        }

        Debug.Log("GANASTE 🔥");

        isSolved = true;

        // 🔥 Desactivar interacción
        foreach (Tile tile in tiles)
        {
            tile.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        // 🔥 Avisar al sistema externo
        PuzzleInteraction interaction = FindObjectOfType<PuzzleInteraction>();
        if (interaction != null)
        {
            interaction.OnSolved();
        }
    }
}