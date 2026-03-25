using TMPro;
using UnityEngine;

public class MainMenuCanvas : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;

    private void Start()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("No existe LocalizationManager en la escena.");
            return;
        }

        int savedLanguage = PlayerPrefs.GetInt("Language", 0);

        if (savedLanguage < 0 || savedLanguage >= languageDropdown.options.Count)
            savedLanguage = 0;

        languageDropdown.value = savedLanguage;
        languageDropdown.RefreshShownValue();

        LocalizationManager.Instance.SetLanguage((LocalizationManager.Language)savedLanguage);

        languageDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= System.Enum.GetValues(typeof(LocalizationManager.Language)).Length)
        {
            Debug.LogWarning($"Índice de idioma inválido: {index}");
            return;
        }

        LocalizationManager.Instance.SetLanguage((LocalizationManager.Language)index);
    }
}