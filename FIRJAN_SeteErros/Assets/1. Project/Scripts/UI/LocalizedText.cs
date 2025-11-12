using UnityEngine;
using TMPro;

/// <summary>
/// Componente que atualiza automaticamente o texto baseado no idioma selecionado
/// Adicione este script a qualquer TextMeshProUGUI que precise ser traduzido
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public enum TextType
    {
        Custom,
        // Seven Errors
        Error1Title,
        Error1Message,
        Error1Name,
        Error2Title,
        Error2Message,
        Error2Name,
        Error3Title,
        Error3Message,
        Error3Name,
        Error4Title,
        Error4Message,
        Error4Name,
        Error5Title,
        Error5Message,
        Error5Name,
        Error6Title,
        Error6Message,
        Error6Name,
        Error7Title,
        Error7Message,
        Error7Name,
        SuccessMessage,
        TimeoutMessage,
        MaxWrongAttemptsMessage,
        // Main Screen
        MainScreenTitle,
        MainScreenDescription,
        MainScreenBtnStart,
        // Final Screen
        FinalScreenTitle,
        FinalScreenContainerTitle1,
        FinalScreenContainerTitle2,
        FinalScreenContainerTitle3,
        FinalScreenNfc,
        FinalScreenTitleDescriptions,
        FinalScreenTitleFailure,
        FinalScreenDescription,
        FinalScreenFinishButton,
        // Header
        HeaderTitle,
        // Close Popup
        ClosePopupPanelText,
        ClosePopupConfirm,
        ClosePopupCancel
    }

    [Header("Localization Settings")]
    [Tooltip("Tipo de texto a ser localizado")]
    [SerializeField] private TextType textType = TextType.Custom;

    [Header("Custom Text (for TextType.Custom)")]
    [Tooltip("Chave customizada para buscar no JSON - use apenas se TextType = Custom")]
    [SerializeField] private string customKey = "";

    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Inscreve no evento de mudança de idioma
        LanguageManager.OnLanguageChanged += UpdateText;

        // Atualiza texto inicial
        UpdateText();
    }

    private void OnDestroy()
    {
        // Desinscreve do evento
        LanguageManager.OnLanguageChanged -= UpdateText;
    }

    /// <summary>
    /// Atualiza o texto baseado no idioma atual
    /// </summary>
    private void UpdateText()
    {
        if (textComponent == null || LanguageManager.Instance == null)
        {
            return;
        }

        string localizedText = GetLocalizedString();

        if (!string.IsNullOrEmpty(localizedText))
        {
            textComponent.text = localizedText;
        }
    }

    /// <summary>
    /// Retorna o texto localizado baseado no tipo selecionado
    /// </summary>
    private string GetLocalizedString()
    {
        if (LanguageManager.Instance == null)
        {
            return textComponent.text; // Mantém o texto atual se não houver LanguageManager
        }

        var data = LanguageManager.Instance.GetCurrentData();
        if (data == null)
        {
            return textComponent.text;
        }

        switch (textType)
        {
            // Seven Errors
            case TextType.Error1Title: return data.seven_errors?.error1Title ?? textComponent.text;
            case TextType.Error1Message: return data.seven_errors?.error1Message ?? textComponent.text;
            case TextType.Error1Name: return data.seven_errors?.error1Name ?? textComponent.text;
            case TextType.Error2Title: return data.seven_errors?.error2Title ?? textComponent.text;
            case TextType.Error2Message: return data.seven_errors?.error2Message ?? textComponent.text;
            case TextType.Error2Name: return data.seven_errors?.error2Name ?? textComponent.text;
            case TextType.Error3Title: return data.seven_errors?.error3Title ?? textComponent.text;
            case TextType.Error3Message: return data.seven_errors?.error3Message ?? textComponent.text;
            case TextType.Error3Name: return data.seven_errors?.error3Name ?? textComponent.text;
            case TextType.Error4Title: return data.seven_errors?.error4Title ?? textComponent.text;
            case TextType.Error4Message: return data.seven_errors?.error4Message ?? textComponent.text;
            case TextType.Error4Name: return data.seven_errors?.error4Name ?? textComponent.text;
            case TextType.Error5Title: return data.seven_errors?.error5Title ?? textComponent.text;
            case TextType.Error5Message: return data.seven_errors?.error5Message ?? textComponent.text;
            case TextType.Error5Name: return data.seven_errors?.error5Name ?? textComponent.text;
            case TextType.Error6Title: return data.seven_errors?.error6Title ?? textComponent.text;
            case TextType.Error6Message: return data.seven_errors?.error6Message ?? textComponent.text;
            case TextType.Error6Name: return data.seven_errors?.error6Name ?? textComponent.text;
            case TextType.Error7Title: return data.seven_errors?.error7Title ?? textComponent.text;
            case TextType.Error7Message: return data.seven_errors?.error7Message ?? textComponent.text;
            case TextType.Error7Name: return data.seven_errors?.error7Name ?? textComponent.text;
            case TextType.SuccessMessage: return data.seven_errors?.successMessage ?? textComponent.text;
            case TextType.TimeoutMessage: return data.seven_errors?.timeoutMessage ?? textComponent.text;
            case TextType.MaxWrongAttemptsMessage: return data.seven_errors?.maxWrongAttemptsMessage ?? textComponent.text;

            // Main Screen
            case TextType.MainScreenTitle: return data.main_screen?.title ?? textComponent.text;
            case TextType.MainScreenDescription: return data.main_screen?.description ?? textComponent.text;
            case TextType.MainScreenBtnStart: return data.main_screen?.btnStart ?? textComponent.text;

            // Final Screen
            case TextType.FinalScreenTitle: return data.final_screen?.title ?? textComponent.text;
            case TextType.FinalScreenContainerTitle1: return data.final_screen?.containerTitle1 ?? textComponent.text;
            case TextType.FinalScreenContainerTitle2: return data.final_screen?.containerTitle2 ?? textComponent.text;
            case TextType.FinalScreenContainerTitle3: return data.final_screen?.containerTitle3 ?? textComponent.text;
            case TextType.FinalScreenNfc: return data.final_screen?.nfc ?? textComponent.text;
            case TextType.FinalScreenTitleDescriptions: return data.final_screen?.titleDescriptions ?? textComponent.text;
            case TextType.FinalScreenTitleFailure: return data.final_screen?.titleFailure ?? textComponent.text;
            case TextType.FinalScreenDescription: return data.final_screen?.description ?? textComponent.text;
            case TextType.FinalScreenFinishButton: return data.final_screen?.finishButton ?? textComponent.text;

            // Header
            case TextType.HeaderTitle: return data.header?.headerTitle ?? textComponent.text;

            // Close Popup
            case TextType.ClosePopupPanelText: return data.close_popup?.panelText ?? textComponent.text;
            case TextType.ClosePopupConfirm: return data.close_popup?.confirm ?? textComponent.text;
            case TextType.ClosePopupCancel: return data.close_popup?.cancel ?? textComponent.text;

            case TextType.Custom:
                Debug.LogWarning($"LocalizedText: TextType.Custom não implementado para chave '{customKey}'");
                return textComponent.text;
            default:
                return textComponent.text;
        }
    }

    /// <summary>
    /// Força atualização do texto (útil para debug ou mudanças em runtime)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Preview em tempo de edição
        if (Application.isPlaying && textComponent != null)
        {
            UpdateText();
        }
    }
#endif
}
