using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PuzzleInteraction : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject puzzleBoard;   // TODO el tablero (Canvas o Panel)
    public GameObject hiddenObject;  // Hoja escondida

    [Header("Configuración")]
    public float delayBeforeClose = 2f;

    private bool isActive = false;

    void Start()
    {
        puzzleBoard.SetActive(false);
        hiddenObject.SetActive(false);
    }

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPuzzle();
        }
    }

    public void Interact()
    {
        if (isActive) return;

        isActive = true;

        puzzleBoard.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitPuzzle()
    {
        isActive = false;

        puzzleBoard.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 🔥 CUANDO SE COMPLETA
    public void OnSolved()
    {
        StartCoroutine(SolveSequence());
    }

    IEnumerator SolveSequence()
    {
        Debug.Log("Puzzle resuelto ⏳");

        yield return new WaitForSecondsRealtime(delayBeforeClose);

        // 🔥 OPCIÓN 1: DESACTIVAR COMPLETO
        puzzleBoard.SetActive(false);
        //gameObject.SetActive(false);
        Destroy(gameObject,2f);

        // 🔥 Mostrar objeto oculto
        hiddenObject.SetActive(true);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Tablero oculto, objeto revelado ✅");
    }
}