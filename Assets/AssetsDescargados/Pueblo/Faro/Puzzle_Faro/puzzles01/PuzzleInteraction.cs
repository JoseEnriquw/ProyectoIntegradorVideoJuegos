using UnityEngine;
using System.Collections;

public class PuzzleInteraction : MonoBehaviour
{
    public GameObject puzzleBoard;
    public GameObject hiddenObject;

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

    public void OnSolved()
    {
        StartCoroutine(SolveSequence());
    }

    IEnumerator SolveSequence()
    {
        yield return new WaitForSecondsRealtime(delayBeforeClose);

        puzzleBoard.SetActive(false);
        hiddenObject.SetActive(true);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}