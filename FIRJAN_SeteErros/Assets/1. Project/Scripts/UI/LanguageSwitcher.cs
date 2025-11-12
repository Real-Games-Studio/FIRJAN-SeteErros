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

    [Header("Sprite Swap (Optional)")]
    [Tooltip("Sprite normal do botão PT")]
    [SerializeField] private Sprite ptNormalSprite;

    [Tooltip("Sprite selecionado do botão PT")]
    [SerializeField] private Sprite ptSelectedSprite;

    [Tooltip("Sprite normal do botão EN")]
    [SerializeField] private Sprite enNormalSprite;

    [Tooltip("Sprite selecionado do botão EN")]
    [SerializeField] private Sprite enSelectedSprite;

    private Image ptButtonImage;
    private Image enButtonImage;

    private void Start()
    {
        // Se não houver botões atribuídos, tenta pegar do próprio objeto
        if (portugueseButton == null)
        {
            portugueseButton = GetComponent<Button>();
        }

        if (englishButton == null)
        {
            englishButton = GetComponent<Button>();
        }

        // Pega as referências das imagens dos botões
        if (portugueseButton != null)
        {
            ptButtonImage = portugueseButton.GetComponent<Image>();
        }

        if (englishButton != null)
        {
            enButtonImage = englishButton.GetComponent<Image>();
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

        // Atualiza sprites dos botões manualmente (força o estado selected permanente)
        UpdateButtonSprites(isPortuguese);
    }

    /// <summary>
    /// Atualiza os sprites dos botões para manter o estado selecionado
    /// </summary>
    private void UpdateButtonSprites(bool isPortuguese)
    {
        // Atualiza sprite do botão PT
        if (ptButtonImage != null && ptNormalSprite != null && ptSelectedSprite != null)
        {
            ptButtonImage.sprite = isPortuguese ? ptSelectedSprite : ptNormalSprite;
        }

        // Atualiza sprite do botão EN
        if (enButtonImage != null && enNormalSprite != null && enSelectedSprite != null)
        {
            enButtonImage.sprite = isPortuguese ? enNormalSprite : enSelectedSprite;
        }
    }
}
