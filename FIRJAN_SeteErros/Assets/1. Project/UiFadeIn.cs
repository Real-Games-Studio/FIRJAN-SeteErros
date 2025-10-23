using UnityEngine;
using UnityEngine.UI;

public class UIFadeIn : MonoBehaviour
{
    [Header("Configurações de Fade")]
    [Tooltip("Tempo mínimo para o fade (segundos)")]
    public float tempoMin = 1f;

    [Tooltip("Tempo máximo para o fade (segundos)")]
    public float tempoMax = 3f;

    [Header("Configurações de Delay")]
    [Tooltip("Delay mínimo antes de iniciar o fade (segundos)")]
    public float delayMin = 0f;

    [Tooltip("Delay máximo antes de iniciar o fade (segundos)")]
    public float delayMax = 0.5f;

    [Tooltip("Usar CanvasGroup (recomendado para grupos de UI)")]
    public bool usarCanvasGroup = true;

    private CanvasGroup canvasGroup;
    private CanvasGroup canvasPai;
    private Graphic graphic;
    private float duracao;
    private float delay;
    private float tempoAtual;
    private bool terminou;
    private bool iniciado;
    private bool esperandoDelay;

    void Awake()
    {
        // Captura os componentes uma vez
        canvasGroup = GetComponent<CanvasGroup>();
        graphic = GetComponent<Graphic>();

        if (transform.parent != null)
            canvasPai = transform.parent.GetComponentInParent<CanvasGroup>();

        if (canvasGroup == null && graphic == null)
        {
            Debug.LogWarning("Nenhum componente de UI encontrado! Adicione um CanvasGroup ou Image/Text.");
            enabled = false;
        }
    }

    void OnEnable()
    {
        // Reinicia parâmetros sempre que o objeto for ativado
        duracao = Random.Range(tempoMin, tempoMax);
        delay = Random.Range(delayMin, delayMax);
        tempoAtual = 0f;
        terminou = false;
        iniciado = false;
        esperandoDelay = true;

        // Começa invisível
        if (usarCanvasGroup && canvasGroup != null)
            canvasGroup.alpha = 0f;
        else if (graphic != null)
        {
            Color c = graphic.color;
            c.a = 0f;
            graphic.color = c;
        }

        // Inicia a espera do delay
        StartCoroutine(EsperarDelay());
    }

    System.Collections.IEnumerator EsperarDelay()
    {
        yield return new WaitForSeconds(delay);
        esperandoDelay = false;
    }

    void Update()
    {
        if (terminou || esperandoDelay) return;

        // Espera o CanvasGroup pai estar totalmente visível
        if (!iniciado)
        {
            if (canvasPai == null || canvasPai.alpha < 1f)
                return;
            iniciado = true;
        }

        // Executa o fade-in
        tempoAtual += Time.deltaTime;
        float t = Mathf.Clamp01(tempoAtual / duracao);
        float novaOpacidade = Mathf.Lerp(0f, 1f, t);

        if (usarCanvasGroup && canvasGroup != null)
            canvasGroup.alpha = novaOpacidade;
        else if (graphic != null)
        {
            Color c = graphic.color;
            c.a = novaOpacidade;
            graphic.color = c;
        }

        if (t >= 1f)
            terminou = true;
    }
}

