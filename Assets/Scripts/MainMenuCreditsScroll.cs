using UnityEngine;
using TMPro;

public class MainMenuCreditsScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 45f;
    public float startYOffset = -250f;
    public float loopResetY = 600f;

    private RectTransform rectTransform;
    private Vector2 defaultPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            defaultPos = rectTransform.anchoredPosition;
        }
    }

    void OnEnable()
    {
        ResetScroll();
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Move the rect transform upwards
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;

        // Reset if it goes beyond the loop boundary
        if (pos.y >= loopResetY)
        {
            pos.y = startYOffset;
        }

        rectTransform.anchoredPosition = pos;
    }

    public void ResetScroll()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(defaultPos.x, startYOffset);
        }
    }
}
