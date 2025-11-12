using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup que aparece quando um erro é encontrado, exibindo mensagem educativa
/// </summary>
public class ErrorPopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;     // Novo campo para o título
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform popupPanel;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Callback para quando o popup for fechado
    public System.Action OnPopupClosed;

    // Armazena os dados do erro atual para atualizar quando o idioma mudar
    private SevenErrorsConfig currentConfig;
    private int currentErrorIndex = -1;
    private bool isShowingError = false;

    // Armazena os dados da mensagem de resultado para atualizar quando o idioma mudar
    private bool isShowingResultMessage = false;
    private bool isCompletedAllErrors = false;
    private bool isMaxWrongAttemptsReached = false;

    private void Awake()
    {
        // Garante que os componentes estão configurados
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (popupPanel == null)
            popupPanel = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Inscreve no evento de mudança de idioma sempre que o popup é ativado
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        // Remove o listener quando o popup é desativado
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Start()
    {
        // Configura o botão de fechar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        // Inicia invisível
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (popupPanel != null)
        {
            popupPanel.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Mostra o popup com o título e mensagem especificados
    /// </summary>
    /// <param name="title">Título do erro</param>
    /// <param name="message">Mensagem educativa a ser exibida</param>
    /// <param name="onClosed">Callback opcional para quando o popup for fechado</param>
    public void ShowPopup(string title, string message, System.Action onClosed = null)
    {
        // Limpa dados de erro anterior
        isShowingError = false;
        currentConfig = null;
        currentErrorIndex = -1;
        isShowingResultMessage = false;

        // Define o callback
        OnPopupClosed = onClosed;

        // Define o título
        if (titleText != null)
        {
            titleText.text = title;
        }

        // Define o texto da mensagem
        if (messageText != null)
        {
            messageText.text = message;
        }

        // Ativa o popup
        gameObject.SetActive(true);

        // Inicia animação de entrada
        StartCoroutine(ShowAnimation());
    }

    /// <summary>
    /// Mostra o popup usando o índice do erro e o SevenErrorsConfig
    /// Busca automaticamente título e mensagem baseado no idioma atual
    /// ATUALIZA AUTOMATICAMENTE quando o idioma muda
    /// </summary>
    /// <param name="config">Configuração do jogo</param>
    /// <param name="errorIndex">Índice do erro (0-6)</param>
    /// <param name="onClosed">Callback opcional para quando o popup for fechado</param>
    public void ShowPopupForError(SevenErrorsConfig config, int errorIndex, System.Action onClosed = null)
    {
        if (config == null)
        {
            ShowPopup("Erro Encontrado!", "Erro encontrado!", onClosed);
            return;
        }

        // Limpa dados de mensagem de resultado
        isShowingResultMessage = false;

        // Armazena os dados para poder atualizar quando o idioma mudar
        currentConfig = config;
        currentErrorIndex = errorIndex;
        isShowingError = true;

        // Define o callback
        OnPopupClosed = onClosed;

        // Atualiza os textos
        UpdateErrorTexts();

        // Ativa o popup
        gameObject.SetActive(true);

        // Inicia animação de entrada
        StartCoroutine(ShowAnimation());
    }

    /// <summary>
    /// Mostra o popup com mensagem de resultado (timeout ou max wrong attempts)
    /// Busca automaticamente título e mensagem do SevenErrorsConfig baseado no idioma atual
    /// ATUALIZA AUTOMATICAMENTE quando o idioma muda
    /// </summary>
    /// <param name="config">Configuração do jogo</param>
    /// <param name="completedAllErrors">Se completou todos os erros</param>
    /// <param name="maxWrongAttemptsReached">Se atingiu o máximo de tentativas erradas</param>
    /// <param name="onClosed">Callback opcional para quando o popup for fechado</param>
    public void ShowPopupForResult(SevenErrorsConfig config, bool completedAllErrors, bool maxWrongAttemptsReached, System.Action onClosed = null)
    {
        if (config == null)
        {
            ShowPopup("Fim de Jogo", "O jogo terminou.", onClosed);
            return;
        }

        // Limpa dados de erro anterior
        isShowingError = false;
        currentConfig = null;
        currentErrorIndex = -1;

        // Armazena os dados para poder atualizar quando o idioma mudar
        currentConfig = config;
        isShowingResultMessage = true;
        isCompletedAllErrors = completedAllErrors;
        isMaxWrongAttemptsReached = maxWrongAttemptsReached;

        // Define o callback
        OnPopupClosed = onClosed;

        // Atualiza os textos
        UpdateResultTexts();

        // Ativa o popup
        gameObject.SetActive(true);

        // Inicia animação de entrada
        StartCoroutine(ShowAnimation());
    }

    /// <summary>
    /// Atualiza os textos do popup baseado no idioma atual
    /// </summary>
    private void UpdateErrorTexts()
    {
        if (!isShowingError || currentConfig == null || currentErrorIndex < 0)
            return;

        string title = currentConfig.GetErrorTitle(currentErrorIndex);
        string message = currentConfig.GetErrorMessage(currentErrorIndex);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    /// <summary>
    /// Atualiza os textos de resultado do popup baseado no idioma atual
    /// </summary>
    private void UpdateResultTexts()
    {
        if (!isShowingResultMessage || currentConfig == null)
            return;

        // Define título baseado no tipo de resultado
        string title;
        if (isMaxWrongAttemptsReached)
        {
            // Busca título traduzido para "Tentativas Esgotadas"
            title = GetMaxAttemptsTitle();
        }
        else
        {
            // Para timeout ou outros casos
            title = GetTimeoutTitle();
        }

        string message = currentConfig.GetResultMessage(isCompletedAllErrors, isMaxWrongAttemptsReached);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    /// <summary>
    /// Retorna o título traduzido para "Tentativas Esgotadas"
    /// </summary>
    private string GetMaxAttemptsTitle()
    {
        // Você pode adicionar essas chaves no JSON se quiser
        // Por enquanto, vamos usar valores hardcoded que mudam com o idioma
        if (LanguageManager.Instance != null)
        {
            var lang = LanguageManager.Instance.GetCurrentLanguage();
            return lang == LanguageManager.Language.Portuguese ? "Tentativas Esgotadas" : "Attempts Exhausted";
        }
        return "Tentativas Esgotadas";
    }

    /// <summary>
    /// Retorna o título traduzido para "Tempo Esgotado"
    /// </summary>
    private string GetTimeoutTitle()
    {
        if (LanguageManager.Instance != null)
        {
            var lang = LanguageManager.Instance.GetCurrentLanguage();
            return lang == LanguageManager.Language.Portuguese ? "Tempo Esgotado" : "Time's Up";
        }
        return "Tempo Esgotado";
    }

    /// <summary>
    /// Chamado quando o idioma muda
    /// </summary>
    private void OnLanguageChanged()
    {
        // Se o popup está mostrando um erro, atualiza os textos
        if (gameObject.activeSelf)
        {
            if (isShowingError)
            {
                UpdateErrorTexts();
            }
            else if (isShowingResultMessage)
            {
                UpdateResultTexts();
            }
        }
    }

    /// <summary>
    /// Mostra o popup apenas com a mensagem (para compatibilidade)
    /// </summary>
    /// <param name="message">Mensagem educativa a ser exibida</param>
    /// <param name="onClosed">Callback opcional para quando o popup for fechado</param>
    public void ShowPopup(string message, System.Action onClosed = null)
    {
        ShowPopup("Erro Encontrado!", message, onClosed);
    }    /// <summary>
         /// Fecha o popup
         /// </summary>
    public void ClosePopup()
    {
        StartCoroutine(HideAnimation());
    }

    /// <summary>
    /// Animação de aparecer do popup
    /// </summary>
    private IEnumerator ShowAnimation()
    {
        float elapsedTime = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;

            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }

            // Scale animation
            if (popupPanel != null)
            {
                float scaleValue = scaleCurve.Evaluate(progress);
                popupPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scaleValue);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Garante valores finais
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (popupPanel != null)
        {
            popupPanel.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Animação de esconder o popup
    /// </summary>
    private IEnumerator HideAnimation()
    {
        float elapsedTime = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        while (elapsedTime < fadeOutDuration)
        {
            float progress = elapsedTime / fadeOutDuration;

            // Fade out
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            // Scale animation
            if (popupPanel != null)
            {
                float scaleValue = scaleCurve.Evaluate(1f - progress);
                popupPanel.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, progress);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Garante valores finais e destrói o objeto
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (popupPanel != null)
        {
            popupPanel.localScale = Vector3.zero;
        }

        // Chama callback se definido
        OnPopupClosed?.Invoke();

        // Esconde o popup (não destrói pois é reutilizado)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fecha o popup quando clicado fora dele (opcional)
    /// </summary>
    public void OnBackgroundClick()
    {
        ClosePopup();
    }

    private void OnDestroy()
    {
        // Remove listener do botão
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
        }
    }
}