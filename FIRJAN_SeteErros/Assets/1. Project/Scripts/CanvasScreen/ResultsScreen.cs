using System.Collections;
using System.Text;
using _4._NFC_Firjan.Scripts.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Tela de resultados do Jogo dos 7 Erros
/// Exibe pontuação, mensagem final e opções para o jogador
/// </summary>
public class ResultsScreen : CanvasScreen
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI resultMessageText;
    [SerializeField] private TextMeshProUGUI errorsFoundText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private TextMeshProUGUI nfcStatusText;

    [Header("Skill Bars")]
    [SerializeField] private SegmentedBar empatiaBar;
    [SerializeField] private SegmentedBar criatividadeBar;
    [SerializeField] private SegmentedBar resolucaoProblemasBar;

    [Header("Animation")]
    [SerializeField] private float fillDelay = 0.5f; // delay entre o preenchimento das barras
    [SerializeField] private float fillSpeed = 0.1f; // velocidade de preenchimento por segmento

    [Header("Auto Restart Timer")]
    [SerializeField] private TextMeshProUGUI timerText; // mesmo timer do gameplay
    [SerializeField] private Image timerFillImage;      // mesmo fill do gameplay
    [SerializeField] private float autoRestartTime = 20f; // 20 segundos para reiniciar

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Conditional Elements")]
    [SerializeField] private GameObject scorePanel; // Painel de pontuação (só aparece se completou)
    [SerializeField] private GameObject timeoutPanel; // Painel de timeout (só aparece se não completou)

    // Auto restart timer control
    private float currentTime;
    private float initialTime;
    private bool timerActive = false;

    public override void OnEnable()
    {
        base.OnEnable();

        // Configura botões
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Remove listeners dos botões
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(PlayAgain);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(BackToMenu);
        }
    }

    private void Update()
    {
        if (timerActive)
        {
            HandleAutoRestartTimer();
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();
        DisplayResults();
        StartAutoRestartTimer();
    }

    /// <summary>
    /// Exibe os resultados do jogo
    /// </summary>
    private void DisplayResults()
    {
        // Obtém dados do resultado
        ScoreData score = GameResultData.GetScore();
        string resultMessage = GameResultData.GetResultMessage();

        // Exibe mensagem principal
        if (resultMessageText != null)
        {
            resultMessageText.text = resultMessage;
        }

        // Exibe número de erros encontrados
        if (errorsFoundText != null)
        {
            errorsFoundText.text = $"Erros encontrados: {GameResultData.errorsFound}/7";
        }

        // Exibe tempo restante (se completou o jogo)
        if (timeRemainingText != null && GameResultData.completedAllErrors)
        {
            int minutes = Mathf.FloorToInt(GameResultData.timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(GameResultData.timeRemaining % 60f);
            timeRemainingText.text = $"Tempo restante: {minutes:00}:{seconds:00}";
        }
        else if (timeRemainingText != null)
        {
            timeRemainingText.text = "Tempo esgotado!";
        }

        // Anima as barras de habilidades com delay
        StartCoroutine(AnimateSkillBars(score));

        // Configura painéis condicionais
        ConfigureConditionalPanels();
        UpdateNfcStatus();
    }

    /// <summary>
    /// Configura a exibição dos painéis baseado no resultado
    /// </summary>
    private void ConfigureConditionalPanels()
    {
        if (GameResultData.completedAllErrors)
        {
            // Completou o jogo - mostra painel de sucesso
            if (scorePanel != null)
                scorePanel.SetActive(true);

            if (timeoutPanel != null)
                timeoutPanel.SetActive(false);
        }
        else
        {
            // Não completou - mostra painel de timeout
            if (scorePanel != null)
                scorePanel.SetActive(false);

            if (timeoutPanel != null)
                timeoutPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Reinicia o jogo
    /// </summary>
    public void PlayAgain()
    {
        // Para o timer de auto-reinicialização
        timerActive = false;

        // Reseta os dados do jogo
        GameResultData.Reset();

        // Volta para a tela de gameplay
        CallScreenByName("cta");
    }

    /// <summary>
    /// Volta para o menu principal
    /// </summary>
    public void BackToMenu()
    {
        // Para o timer de auto-reinicialização
        timerActive = false;

        // Reseta os dados do jogo
        GameResultData.Reset();

        // Volta para a tela inicial
        CallNextScreen();
    }

    /// <summary>
    /// Para debug - simula resultado de sucesso
    /// </summary>
    [ContextMenu("Simulate Success")]
    private void SimulateSuccess()
    {
        GameResultData.errorsFound = 7;
        GameResultData.timeRemaining = 30f;
        GameResultData.completedAllErrors = true;
        DisplayResults();
    }

    /// <summary>
    /// Para debug - simula resultado de falha
    /// </summary>
    [ContextMenu("Simulate Failure")]
    private void SimulateFailure()
    {
        GameResultData.errorsFound = 3;
        GameResultData.timeRemaining = 0f;
        GameResultData.completedAllErrors = false;
        DisplayResults();
    }

    /// <summary>
    /// Inicia o timer de auto-reinicialização
    /// </summary>
    private void StartAutoRestartTimer()
    {
        currentTime = autoRestartTime;
        initialTime = autoRestartTime;
        timerActive = true;
        UpdateTimerUI();
    }

    /// <summary>
    /// Gerencia o timer de auto-reinicialização
    /// </summary>
    private void HandleAutoRestartTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            timerActive = false;
            RestartApplication();
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Atualiza o timer na UI (mesmo sistema do GameplayScreen)
    /// </summary>
    private void UpdateTimerUI()
    {
        UpdateTimerFill();

        if (timerText != null)
        {
            int secondsRemaining = Mathf.CeilToInt(currentTime);
            if (secondsRemaining < 0) secondsRemaining = 0;
            timerText.text = $"{secondsRemaining}";
        }
    }

    /// <summary>
    /// Atualiza o fill da imagem do timer (mesmo sistema do GameplayScreen)
    /// </summary>
    private void UpdateTimerFill()
    {
        if (timerFillImage == null) return;

        if (initialTime <= 0f)
        {
            timerFillImage.fillAmount = 0f;
            return;
        }

        float normalizedTime = Mathf.Clamp01(currentTime / initialTime);
        timerFillImage.fillAmount = normalizedTime;
    }

    /// <summary>
    /// Reinicia a aplicação carregando a scene 0
    /// </summary>
    private void RestartApplication()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Anima o preenchimento das barras de habilidades gradualmente
    /// </summary>
    private System.Collections.IEnumerator AnimateSkillBars(ScoreData score)
    {
        // Reseta todas as barras para 0
        if (empatiaBar != null) empatiaBar.SetValue(0);
        if (criatividadeBar != null) criatividadeBar.SetValue(0);
        if (resolucaoProblemasBar != null) resolucaoProblemasBar.SetValue(0);

        // Delay inicial
        yield return new WaitForSeconds(fillDelay);

        // Anima Empatia
        if (empatiaBar != null)
        {
            yield return StartCoroutine(AnimateBar(empatiaBar, score.empatia));
            yield return new WaitForSeconds(fillDelay);
        }

        // Anima Criatividade
        if (criatividadeBar != null)
        {
            yield return StartCoroutine(AnimateBar(criatividadeBar, score.criatividade));
            yield return new WaitForSeconds(fillDelay);
        }

        // Anima Resolução de Problemas
        if (resolucaoProblemasBar != null)
        {
            yield return StartCoroutine(AnimateBar(resolucaoProblemasBar, score.resolucaoProblemas));
        }
    }

    /// <summary>
    /// Anima uma barra específica até o valor desejado
    /// </summary>
    private System.Collections.IEnumerator AnimateBar(SegmentedBar bar, int targetValue)
    {
        int currentValue = 0;
        
        while (currentValue < targetValue)
        {
            currentValue++;
            bar.SetValue(currentValue);
            yield return new WaitForSeconds(fillSpeed);
        }
    }

    private void UpdateNfcStatus()
    {
        if (nfcStatusText == null)
        {
            return;
        }

        NFCGameService service = NFCGameService.Instance;

        if (service == null)
        {
            nfcStatusText.text = "NFC: Serviço indisponível.";
            return;
        }

        if (!service.HasCard)
        {
            nfcStatusText.text = "NFC: Nenhum cartão detectado.";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Cartão: {service.CurrentNfcId}");

        if (!string.IsNullOrEmpty(service.CurrentReaderName))
        {
            builder.AppendLine($"Leitor: {service.CurrentReaderName}");
        }

        EndGameResponseModel response = service.LastResponse;

        if (response?.attributes != null)
        {
            builder.AppendLine($"Empatia total: {response.attributes.empathy}");
            builder.AppendLine($"Criatividade total: {response.attributes.creativity}");
            builder.AppendLine($"Resolução total: {response.attributes.problem_solving}");
        }
        else
        {
            builder.Append("Sincronizando com o servidor...");
        }

        nfcStatusText.text = builder.ToString();
    }
}