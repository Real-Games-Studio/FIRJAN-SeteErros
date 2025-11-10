using UnityEngine;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Gerenciador de idiomas - Jogo dos 7 Erros
/// Controla a troca entre Português e Inglês
/// Carrega automaticamente os JSONs de StreamingAssets
/// </summary>
public class LanguageManager : MonoBehaviour
{
    [System.Serializable]
    public class LanguageData
    {
        public SevenErrorsTexts seven_errors;
        public MainScreenTexts main_screen;
        public FinalScreenTexts final_screen;
        public HeaderTexts header;
        public ClosePopupTexts close_popup;
    }

    [System.Serializable]
    public class SevenErrorsTexts
    {
        // Error Titles
        public string error1Title;
        public string error2Title;
        public string error3Title;
        public string error4Title;
        public string error5Title;
        public string error6Title;
        public string error7Title;

        // Error Messages
        public string error1Message;
        public string error2Message;
        public string error3Message;
        public string error4Message;
        public string error5Message;
        public string error6Message;
        public string error7Message;

        // Error Names
        public string error1Name;
        public string error2Name;
        public string error3Name;
        public string error4Name;
        public string error5Name;
        public string error6Name;
        public string error7Name;

        // Result Messages
        public string successMessage;
        public string timeoutMessage;
        public string maxWrongAttemptsMessage;
    }

    [System.Serializable]
    public class MainScreenTexts
    {
        public string title;
        public string description;
        public string btnStart;
    }

    [System.Serializable]
    public class FinalScreenTexts
    {
        public string title;
        public string containerTitle1;
        public string containerTitle2;
        public string containerTitle3;
        public string nfc;
        public string titleDescriptions;
        public string description;
    }

    [System.Serializable]
    public class HeaderTexts
    {
        public string headerTitle;
    }

    [System.Serializable]
    public class ClosePopupTexts
    {
        public string panelText;
        public string confirm;
        public string cancel;
    }

    public enum Language
    {
        Portuguese,
        English
    }

    [Header("Language Settings")]
    [SerializeField] private Language currentLanguage = Language.Portuguese;

    [Header("JSON File Names (in StreamingAssets)")]
    [SerializeField] private string portugueseFileName = "language_pt.json";
    [SerializeField] private string englishFileName = "language_en.json";

    // Singleton instance
    public static LanguageManager Instance { get; private set; }

    // Current language data
    private LanguageData currentData;

    // Event for language change
    public static event Action OnLanguageChanged;

    private void Awake()
    {
        // Implementa singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(LoadLanguageCoroutine(currentLanguage));
    }

    /// <summary>
    /// Carrega o idioma especificado do StreamingAssets
    /// </summary>
    public void LoadLanguage(Language language)
    {
        StartCoroutine(LoadLanguageCoroutine(language));
    }

    /// <summary>
    /// Coroutine que carrega o JSON do StreamingAssets
    /// </summary>
    private IEnumerator LoadLanguageCoroutine(Language language)
    {
        currentLanguage = language;

        string fileName = language == Language.Portuguese ? portugueseFileName : englishFileName;
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        Debug.Log($"Loading language file from: {filePath}");

        string jsonText = null;

        // No Android, StreamingAssets precisa usar UnityWebRequest
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                jsonText = www.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Error loading language file {fileName}: {www.error}");
                yield break;
            }
        }
        else
        {
            // Em outras plataformas, pode usar File.ReadAllText
            if (File.Exists(filePath))
            {
                jsonText = File.ReadAllText(filePath);
            }
            else
            {
                Debug.LogError($"Language file not found: {filePath}");
                yield break;
            }
        }

        // Parse do JSON
        try
        {
            currentData = JsonUtility.FromJson<LanguageData>(jsonText);
            OnLanguageChanged?.Invoke();
            Debug.Log($"Language loaded successfully: {language}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing language JSON {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// Alterna entre Português e Inglês
    /// </summary>
    public void ToggleLanguage()
    {
        Language newLanguage = currentLanguage == Language.Portuguese ? Language.English : Language.Portuguese;
        LoadLanguage(newLanguage);
    }

    /// <summary>
    /// Define o idioma para Português
    /// </summary>
    public void SetPortuguese()
    {
        if (currentLanguage != Language.Portuguese)
        {
            LoadLanguage(Language.Portuguese);
        }
    }

    /// <summary>
    /// Define o idioma para Inglês
    /// </summary>
    public void SetEnglish()
    {
        if (currentLanguage != Language.English)
        {
            LoadLanguage(Language.English);
        }
    }

    /// <summary>
    /// Retorna o idioma atual
    /// </summary>
    public Language GetCurrentLanguage()
    {
        return currentLanguage;
    }

    /// <summary>
    /// Retorna os dados do idioma atual
    /// </summary>
    public LanguageData GetCurrentData()
    {
        return currentData;
    }

    // ===== MÉTODOS HELPERS - JOGO DOS 7 ERROS =====

    /// <summary>
    /// Retorna o título do erro (0-6)
    /// </summary>
    public string GetErrorTitle(int errorIndex)
    {
        if (currentData?.seven_errors == null) return $"Erro {errorIndex + 1}";

        switch (errorIndex)
        {
            case 0: return currentData.seven_errors.error1Title;
            case 1: return currentData.seven_errors.error2Title;
            case 2: return currentData.seven_errors.error3Title;
            case 3: return currentData.seven_errors.error4Title;
            case 4: return currentData.seven_errors.error5Title;
            case 5: return currentData.seven_errors.error6Title;
            case 6: return currentData.seven_errors.error7Title;
            default: return $"Erro {errorIndex + 1}";
        }
    }

    /// <summary>
    /// Retorna a mensagem educativa do erro (0-6)
    /// </summary>
    public string GetErrorMessage(int errorIndex)
    {
        if (currentData?.seven_errors == null) return "Erro encontrado!";

        switch (errorIndex)
        {
            case 0: return currentData.seven_errors.error1Message;
            case 1: return currentData.seven_errors.error2Message;
            case 2: return currentData.seven_errors.error3Message;
            case 3: return currentData.seven_errors.error4Message;
            case 4: return currentData.seven_errors.error5Message;
            case 5: return currentData.seven_errors.error6Message;
            case 6: return currentData.seven_errors.error7Message;
            default: return "Erro encontrado!";
        }
    }

    /// <summary>
    /// Retorna o nome do erro (0-6)
    /// </summary>
    public string GetErrorName(int errorIndex)
    {
        if (currentData?.seven_errors == null) return $"Erro {errorIndex + 1}";

        switch (errorIndex)
        {
            case 0: return currentData.seven_errors.error1Name;
            case 1: return currentData.seven_errors.error2Name;
            case 2: return currentData.seven_errors.error3Name;
            case 3: return currentData.seven_errors.error4Name;
            case 4: return currentData.seven_errors.error5Name;
            case 5: return currentData.seven_errors.error6Name;
            case 6: return currentData.seven_errors.error7Name;
            default: return $"Erro {errorIndex + 1}";
        }
    }

    /// <summary>
    /// Retorna a mensagem de resultado apropriada
    /// </summary>
    public string GetResultMessage(bool completedAllErrors, bool maxWrongAttemptsReached)
    {
        if (currentData?.seven_errors == null) return "";

        if (completedAllErrors)
        {
            return currentData.seven_errors.successMessage;
        }
        else if (maxWrongAttemptsReached)
        {
            return currentData.seven_errors.maxWrongAttemptsMessage;
        }
        else
        {
            return currentData.seven_errors.timeoutMessage;
        }
    }
}
