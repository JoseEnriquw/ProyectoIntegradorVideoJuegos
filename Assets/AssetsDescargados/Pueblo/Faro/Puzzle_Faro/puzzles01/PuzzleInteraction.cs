using UnityEngine;
using System.Collections;

public class PuzzleInteraction : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject puzzleBoard;   // UI o tablero del puzzle
    public GameObject hiddenObject;  // Objeto oculto (ej: llave)

    [Header("Opciones")]
    public bool pauseGame = true;    // Pausar el juego al abrir puzzle

    private bool isActive = false;

    void Start()
    {
        // Asegurar que el puzzle empieza oculto
        if (puzzleBoard != null)
            puzzleBoard.SetActive(false);

        if (hiddenObject != null)
            hiddenObject.SetActive(false);
    }

    void Update()
    {
        // Permitir salir con ESC
        if (isActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPuzzle();
        }
    }

    // 👉 LLAMADO DESDE UHFPS (interacción con tecla E)
    public void Interact()
    {
        if (isActive) return;

        Debug.Log("Puzzle abierto 🔥");

        isActive = true;

        if (puzzleBoard != null)
            puzzleBoard.SetActive(true);

        if (pauseGame)
            Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 👉 BOTÓN SALIR o tecla ESC
    public void ExitPuzzle()
    {
        Debug.Log("Saliste del puzzle ❌");

        isActive = false;

        if (puzzleBoard != null)
            puzzleBoard.SetActive(false);

        if (pauseGame)
            Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 👉 CUANDO COMPLETAS EL PUZZLE
    public void OnSolved()
    {
        Debug.Log("Puzzle resuelto ✅");

        isActive = false;

        if (puzzleBoard != null)
            puzzleBoard.SetActive(false);

        if (hiddenObject != null)
            hiddenObject.SetActive(true);

        if (pauseGame)
            Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Opcional: destruir el objeto interactuable
        Destroy(gameObject);
    }
}