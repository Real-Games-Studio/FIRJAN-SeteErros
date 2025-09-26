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

    private void Awake()
    {
        // Garante que os componentes estão configurados
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (popupPanel == null)
            popupPanel = GetComponent<RectTransform>();
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
            canvasGroup.blocksRaycasts = false;
        }

        if (popupPanel != null)
        {
            popupPanel.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Mostra o popup com a mensagem especificada
    /// </summary>
    /// <param name="message">Mensagem educativa a ser exibida</param>
    /// <param name="onClosed">Callback opcional para quando o popup for fechado</param>
    public void ShowPopup(string message, System.Action onClosed = null)
    {
        // Define o callback
        OnPopupClosed = onClosed;

        // Define o texto da mensagem
        if (messageText != null)
        {
            messageText.text = message;
        }

        // Ativa o popup
        gameObject.SetActive(true);

        // Inicia animação de entrada
        StartCoroutine(ShowAnimation());
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