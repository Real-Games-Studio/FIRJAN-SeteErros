using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para trocar o idioma do jogo
/// Adicione este script a botões na UI para alternar entre PT e EN
/// </summary>
public class LanguageSwitcher : MonoBehaviour
{
    [Header("Button References (Optional)")]
    [Tooltip("Botão para ativar Português - deixe vazio se este objeto for o botão")]
    [SerializeField] private Button portugueseButton;

    [Tooltip("Botão para ativar Inglês - deixe vazio se este objeto for o botão")]
    [SerializeField] private Button englishButton;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Objeto que será ativado quando o idioma correspondente estiver ativo")]
    [SerializeField] private GameObject selectedIndicatorPT;

    [Tooltip("Objeto que será ativado quando o idioma correspondente estiver ativo")]
    [SerializeField] private GameObject selectedIndicatorEN;

    private void Start()
    {
        // Se não houver botões atribuídos, tenta pegar do próprio objeto
        if (portugueseButton == null)
        {
            portugueseButton = GetComponent<Button>();
        }

        // Adiciona listeners aos botões
        if (portugueseButton != null)
        {
            portugueseButton.onClick.AddListener(SetPortuguese);
        }

        if (englishButton != null)
        {
            englishButton.onClick.AddListener(SetEnglish);
        }

        // Inscreve no evento de mudança de idioma
        LanguageManager.OnLanguageChanged += UpdateVisualFeedback;

        // Atualiza feedback visual inicial
        UpdateVisualFeedback();
    }

    private void OnDestroy()
    {
        // Remove listeners
        if (portugueseButton != null)
        {
            portugueseButton.onClick.RemoveListener(SetPortuguese);
        }

        if (englishButton != null)
        {
            englishButton.onClick.RemoveListener(SetEnglish);
        }

        // Desinscreve do evento
        LanguageManager.OnLanguageChanged -= UpdateVisualFeedback;
    }

    /// <summary>
    /// Define o idioma para Português
    /// Pode ser chamado por eventos de UI
    /// </summary>
    public void SetPortuguese()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetPortuguese();
        }
        else
        {
            Debug.LogWarning("LanguageManager não encontrado na cena!");
        }
    }

    /// <summary>
    /// Define o idioma para Inglês
    /// Pode ser chamado por eventos de UI
    /// </summary>
    public void SetEnglish()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetEnglish();
        }
        else
        {
            Debug.LogWarning("LanguageManager não encontrado na cena!");
        }
    }

    /// <summary>
    /// Alterna entre os idiomas
    /// Pode ser chamado por eventos de UI
    /// </summary>
    public void ToggleLanguage()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.ToggleLanguage();
        }
        else
        {
            Debug.LogWarning("LanguageManager não encontrado na cena!");
        }
    }

    /// <summary>
    /// Atualiza os indicadores visuais baseado no idioma atual
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (LanguageManager.Instance == null) return;

        bool isPortuguese = LanguageManager.Instance.GetCurrentLanguage() == LanguageManager.Language.Portuguese;

        // Atualiza indicadores visuais
        if (selectedIndicatorPT != null)
        {
            selectedIndicatorPT.SetActive(isPortuguese);
        }

        if (selectedIndicatorEN != null)
        {
            selectedIndicatorEN.SetActive(!isPortuguese);
        }

        // Atualiza interatividade dos botões (opcional)
        if (portugueseButton != null)
        {
            portugueseButton.interactable = !isPortuguese;
        }

        if (englishButton != null)
        {
            englishButton.interactable = isPortuguese;
        }
    }
}
