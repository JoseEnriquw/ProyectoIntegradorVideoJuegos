//using TMPro;
//using UnityEngine;

//[RequireComponent(typeof(TextMeshProUGUI))]
//public class LocalizedText : MonoBehaviour
//{
//    [SerializeField] private string localizationKey;

//    private TextMeshProUGUI text;

//    private void Awake()
//    {
//        text = GetComponent<TextMeshProUGUI>();
//    }
//    private void Start()
//    {
//        if (LocalizationManager.Instance == null)
//        {
//            Debug.LogWarning($"No se encontró LocalizationManager para {gameObject.name}");
//            return;
//        }

//        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
//        UpdateText();
//    }
//    private void OnEnable()
//    {
//        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
//        UpdateText();
//    }

//    private void OnDisable()
//    {
//        if (LocalizationManager.Instance != null)
//            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
//    }

//    private void UpdateText()
//    {
//        if (text != null && LocalizationManager.Instance != null)
//        {
//            text.text = LocalizationManager.Instance.GetText(localizationKey);
//        }
//    }
//}
using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator Start()
    {
        while (LocalizationManager.Instance == null)
            yield return null;

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        if (text != null && LocalizationManager.Instance != null)
            text.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
