using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public int size = 3;
    public float spacing = 200f;

    public Tile[] tiles;

    private Vector2Int emptyPos;
    private bool isShuffling = false;

    void Start()
    {
        Initialize();
        Shuffle();
    }

    void Initialize()
    {
        int index = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (index < tiles.Length)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    tiles[index].Init(this, pos, spacing);
                    index++;
                }
                else
                {
                    emptyPos = new Vector2Int(x, y);
                }
            }
        }
    }

    public void TryMove(Tile tile)
    {
        if (!IsAdjacent(tile.gridPos, emptyPos))
            return;

        MoveTile(tile);
    }

    void MoveTile(Tile tile)
    {
        Vector2Int oldPos = tile.gridPos;

        tile.SetGridPosition(emptyPos, spacing);
        emptyPos = oldPos;

        if (!isShuffling)
            CheckWin();
    }

    bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    void Shuffle()
    {
        isShuffling = true;

        for (int i = 0; i < 200; i++)
        {
            Tile randomTile = tiles[Random.Range(0, tiles.Length)];

            if (IsAdjacent(randomTile.gridPos, emptyPos))
            {
                MoveTile(randomTile);
            }
        }

        isShuffling = false;

        if (IsSolved())
        {
            Shuffle();
            return;
        }
    }

    bool IsSolved()
    {
        foreach (Tile tile in tiles)
        {
            if (!tile.IsCorrect())
                return false;
        }
        return true;
    }

    void CheckWin()
    {
        foreach (Tile tile in tiles)
        {
            if (!tile.IsCorrect())
                return;
        }

        FindObjectOfType<PuzzleInteraction>()?.OnSolved();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Solve();
        }
    }

    void Solve()
    {
        foreach (Tile tile in tiles)
        {
            tile.SetGridPosition(tile.correctPos, spacing);
        }

        CheckWin();
    }
}