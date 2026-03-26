using System.Collections.Generic;
using UnityEngine;
using System;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public enum Language { Spanish, English }

    [SerializeField] private Language currentLanguage = Language.Spanish;
    [SerializeField] private TextAsset[] localizationFiles; // Asigná desde el Inspector

    private Dictionary<string, string> localizedTexts;
    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Leer idioma guardado o usar por defecto
        int lang = PlayerPrefs.GetInt("Language", (int)Language.Spanish);
        LoadLanguage((Language)lang);
    }

    public void SetLanguage(Language language)
    {
        PlayerPrefs.SetInt("Language", (int)language);
        PlayerPrefs.Save();
        LoadLanguage(language);
    }


    public void LoadLanguage(Language language)
    {
        currentLanguage = language;

        var file = localizationFiles[(int)language];
        localizedTexts = new Dictionary<string, string>();
        string[] lines = file.text.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(';');
            if (parts.Length >= 2)
                localizedTexts[parts[0].Trim()] = parts[1].Trim();
        }

        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (localizedTexts == null || !localizedTexts.TryGetValue(key, out var value))
            return $"[{key}]";
        return value;
    }

    public Language GetCurrentLanguage() => currentLanguage;
}
