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
    [SerializeField] private TextMeshProUGUI resultTitleText; // Título "Parabéns!" / "Obrigado!"
    [SerializeField] private TextMeshProUGUI resultMessageText; // Descrição longa
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

    [Header("Score Scaling")]
    [Tooltip("Pontuação máxima possível para cada habilidade (usado para calcular proporção)")]
    [SerializeField] private int maxScoreValue = 8;
    [Tooltip("Número de segmentos na barra (deve corresponder ao número de segmentos nas SegmentedBars)")]
    [SerializeField] private int barSegments = 22;

    [Header("Auto Restart Timer")]
    [SerializeField] private TextMeshProUGUI timerText; // mesmo timer do gameplay
    [SerializeField] private Image timerFillImage;      // mesmo fill do gameplay
    [SerializeField] private float autoRestartTime = 20f; // 20 segundos para reiniciar

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button finishButton; // Botão que aparece após leitura do NFC

    [Header("Conditional Elements")]
    [SerializeField] private GameObject scorePanel; // Painel de pontuação (só aparece se completou)
    [SerializeField] private GameObject timeoutPanel; // Painel de timeout (só aparece se não completou)

    [Header("Songs")]
    [SerializeField] private AudioSource endSong;
    [SerializeField] private AudioSource nfcSong;

    [Header("NFC Feedback")]
    [Tooltip("GameObject que será desligado quando o NFC for lido")]
    [SerializeField] private GameObject objectToDisableOnNFC;

    // Auto restart timer control
    private float currentTime;
    private float initialTime;
    private bool timerActive = false;

    // Estado pós-NFC (para manter após troca de idioma)
    private bool nfcDataReceived = false;

    public override void OnEnable()
    {
        base.OnEnable();

        // Tenta registrar o listener do NFC
        TryRegisterNFCListener();

        // Registra listener de mudança de idioma
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        // Configura botões
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }

        if (finishButton != null)
        {
            finishButton.onClick.AddListener(BackToMenu);
            finishButton.gameObject.SetActive(false); // Inicia desligado
        }
    }

    /// <summary>
    /// Tenta registrar o listener do NFC (chama novamente se necessário)
    /// </summary>
    private void TryRegisterNFCListener()
    {
        if (NFCGameService.Instance != null)
        {
            // Remove listener anterior se existir (evita duplicatas)
            NFCGameService.Instance.OnNfcDataUpdated -= OnNfcDataReceived;
            // Adiciona o listener
            NFCGameService.Instance.OnNfcDataUpdated += OnNfcDataReceived;
            Debug.Log("[ResultsScreen] Listener OnNfcDataUpdated registrado com sucesso");
        }
        else
        {
            Debug.LogWarning("[ResultsScreen] NFCGameService.Instance é nulo! Tentando novamente...");
            // Tenta novamente após um frame
            StartCoroutine(RetryRegisterNFCListener());
        }
    }

    /// <summary>
    /// Retry para registrar o listener se o NFCGameService ainda não estiver pronto
    /// </summary>
    private System.Collections.IEnumerator RetryRegisterNFCListener()
    {
        // Espera até 10 frames ou até o serviço estar disponível
        for (int i = 0; i < 10; i++)
        {
            yield return null; // Espera um frame

            if (NFCGameService.Instance != null)
            {
                NFCGameService.Instance.OnNfcDataUpdated -= OnNfcDataReceived;
                NFCGameService.Instance.OnNfcDataUpdated += OnNfcDataReceived;
                Debug.Log($"[ResultsScreen] Listener OnNfcDataUpdated registrado com sucesso após {i + 1} tentativa(s)");
                yield break;
            }
        }

        Debug.LogError("[ResultsScreen] NFCGameService.Instance ainda é nulo após várias tentativas! Evento NFC não será escutado.");
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Remove listener do NFC
        if (NFCGameService.Instance != null)
        {
            NFCGameService.Instance.OnNfcDataUpdated -= OnNfcDataReceived;
        }

        // Remove listener de mudança de idioma
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;

        // Remove listeners dos botões
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(PlayAgain);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(BackToMenu);
        }

        if (finishButton != null)
        {
            finishButton.onClick.RemoveListener(BackToMenu);
        }
    }

    private void Update()
    {
        if (timerActive)
        {
            HandleAutoRestartTimer();
        }

#if UNITY_EDITOR
        // Debug: Pressione N para simular leitura NFC e POST de dados
        if (Input.GetKeyDown(KeyCode.N))
        {
            SimulateNFCReadAndPost();
        }
#endif
    }

    public override void TurnOn()
    {
        base.TurnOn();
        DisplayResults();
        StartAutoRestartTimer();
        endSong?.Play();
    }

    /// <summary>
    /// Exibe os resultados do jogo
    /// </summary>
    private void DisplayResults()
    {
        // Obtém dados do resultado
        ScoreData score = GameResultData.GetScore();
        string resultMessage = GameResultData.GetResultMessage();

        // Define o título baseado no resultado (Parabéns! ou Que pena!)
        if (resultTitleText != null && LanguageManager.Instance != null)
        {
            string title = GameResultData.completedAllErrors
                ? LanguageManager.Instance.GetSuccessTitle()
                : LanguageManager.Instance.GetFailureTitle();
            resultTitleText.text = title;
        }

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

        // Converte pontuações para segmentos proporcionais
        int empatiaSegments = ConvertScoreToSegments(score.empatia);
        int criatividadeSegments = ConvertScoreToSegments(score.criatividade);
        int resolucaoSegments = ConvertScoreToSegments(score.resolucaoProblemas);

        // Anima Empatia
        if (empatiaBar != null)
        {
            yield return StartCoroutine(AnimateBar(empatiaBar, empatiaSegments));
            yield return new WaitForSeconds(fillDelay);
        }

        // Anima Criatividade
        if (criatividadeBar != null)
        {
            yield return StartCoroutine(AnimateBar(criatividadeBar, criatividadeSegments));
            yield return new WaitForSeconds(fillDelay);
        }

        // Anima Resolução de Problemas
        if (resolucaoProblemasBar != null)
        {
            yield return StartCoroutine(AnimateBar(resolucaoProblemasBar, resolucaoSegments));
        }
    }

    /// <summary>
    /// Converte uma pontuação em número de segmentos proporcionalmente
    /// Calcula baseado no valor individual da habilidade, não nos erros totais
    /// </summary>
    private int ConvertScoreToSegments(int scoreValue)
    {
        if (maxScoreValue <= 0) return 0;

        // Calcula proporção baseada no valor INDIVIDUAL da habilidade
        // Se atingiu o valor máximo individual (8), retorna o máximo de segmentos (22)
        if (scoreValue >= maxScoreValue)
        {
            return barSegments;
        }

        // Calcula proporção exata do valor individual
        float proportion = (float)scoreValue / maxScoreValue;

        // Multiplica pela quantidade de segmentos e arredonda
        int segments = Mathf.RoundToInt(proportion * barSegments);

        // Garante que fica dentro dos limites
        return Mathf.Clamp(segments, 0, barSegments);
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

    /// <summary>
    /// Callback quando os dados NFC são atualizados após POST bem-sucedido
    /// </summary>
    private void OnNfcDataReceived(EndGameResponseModel response)
    {
        Debug.Log($"[ResultsScreen] OnNfcDataReceived chamado! Response: {response?.nfcId ?? "null"}");

        // Marca que os dados NFC foram recebidos
        nfcDataReceived = true;

        // Toca o som quando os dados NFC são atualizados (após POST bem-sucedido)
        if (nfcSong != null)
        {
            nfcSong.Play();
            Debug.Log("[ResultsScreen] Som NFC tocado");
        }
        else
        {
            Debug.LogWarning("[ResultsScreen] nfcSong é nulo!");
        }

        // Atualiza a UI com os novos dados
        UpdateNfcStatus();

        // Desliga o GameObject configurado (se houver)
        if (objectToDisableOnNFC != null)
        {
            objectToDisableOnNFC.SetActive(false);
            Debug.Log($"[ResultsScreen] GameObject '{objectToDisableOnNFC.name}' desligado");
        }
        else
        {
            Debug.LogWarning("[ResultsScreen] objectToDisableOnNFC não está configurado");
        }

        // Aplica o estado pós-NFC
        ApplyPostNfcState();
    }

    /// <summary>
    /// Aplica o estado visual pós-NFC (Obrigado + botão Finalizar)
    /// </summary>
    private void ApplyPostNfcState()
    {
        // Troca o TÍTULO para "Obrigado!" / "Thank you!"
        if (resultTitleText != null && LanguageManager.Instance != null)
        {
            string thanksMsg = LanguageManager.Instance.GetThanksMessage();
            resultTitleText.text = thanksMsg;
            Debug.Log($"[ResultsScreen] Título alterado para: {thanksMsg}");
        }
        else
        {
            Debug.LogWarning($"[ResultsScreen] resultTitleText={resultTitleText != null}, LanguageManager={LanguageManager.Instance != null}");
        }

        // Limpa o texto da descrição longa
        if (resultMessageText != null)
        {
            resultMessageText.text = "";
            Debug.Log("[ResultsScreen] Texto da descrição limpo");
        }
        else
        {
            Debug.LogWarning("[ResultsScreen] resultMessageText é nulo!");
        }

        // Ativa o botão "Finalizar" e atualiza seu texto
        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(true);

            // Atualiza o texto do botão com a tradução
            if (LanguageManager.Instance != null)
            {
                TextMeshProUGUI buttonText = finishButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = LanguageManager.Instance.GetFinishButtonText();
                    Debug.Log($"[ResultsScreen] Texto do botão Finalizar atualizado para: {buttonText.text}");
                }
            }

            Debug.Log("[ResultsScreen] Botão Finalizar ativado");
        }
        else
        {
            Debug.LogWarning("[ResultsScreen] finishButton não está configurado");
        }
    }

    /// <summary>
    /// Callback quando o idioma é alterado
    /// </summary>
    private void OnLanguageChanged()
    {
        Debug.Log($"[ResultsScreen] Idioma alterado. nfcDataReceived={nfcDataReceived}");

        // Se os dados NFC já foram recebidos, reaplica o estado pós-NFC
        if (nfcDataReceived)
        {
            // Aguarda um frame para garantir que LocalizedText atualizou primeiro
            StartCoroutine(ReapplyPostNfcStateAfterLanguageChange());
        }
        else
        {
            // Se ainda não recebeu NFC, atualiza a mensagem de resultado normalmente
            StartCoroutine(UpdateResultMessageAfterLanguageChange());
        }
    }

    /// <summary>
    /// Reaplica o estado pós-NFC após troca de idioma
    /// </summary>
    private System.Collections.IEnumerator ReapplyPostNfcStateAfterLanguageChange()
    {
        // Aguarda LocalizedText atualizar
        yield return null;

        // Reaplica o estado pós-NFC com o novo idioma
        ApplyPostNfcState();

        Debug.Log("[ResultsScreen] Estado pós-NFC reaplicado após mudança de idioma");
    }

    /// <summary>
    /// Atualiza a mensagem de resultado após troca de idioma
    /// (apenas quando NÃO estiver no estado pós-NFC)
    /// </summary>
    private System.Collections.IEnumerator UpdateResultMessageAfterLanguageChange()
    {
        // Aguarda LocalizedText atualizar
        yield return null;

        // Atualiza o título baseado no resultado (Parabéns! ou Que pena!)
        if (resultTitleText != null && LanguageManager.Instance != null)
        {
            string title = GameResultData.completedAllErrors
                ? LanguageManager.Instance.GetSuccessTitle()
                : LanguageManager.Instance.GetFailureTitle();
            resultTitleText.text = title;
            Debug.Log($"[ResultsScreen] Título atualizado após mudança de idioma: {title}");
        }

        // Atualiza a mensagem de resultado com o novo idioma
        if (resultMessageText != null)
        {
            string resultMessage = GameResultData.GetResultMessage();
            resultMessageText.text = resultMessage;
            Debug.Log($"[ResultsScreen] Mensagem de resultado atualizada após mudança de idioma: {resultMessage}");
        }

        Debug.Log("[ResultsScreen] Título e mensagem de resultado atualizados após mudança de idioma");
    }

#if UNITY_EDITOR
    /// <summary>
    /// [DEBUG ONLY] Simula a leitura do cartão NFC e o processo completo de POST
    /// Pressione N na tela de resultados para testar
    /// </summary>
    private void SimulateNFCReadAndPost()
    {
        Debug.Log("[DEBUG] Simulando leitura NFC e POST de dados...");

        // Marca que os dados NFC foram recebidos
        nfcDataReceived = true;

        // Simula dados do cartão NFC
        string fakeCardId = "DEBUG_CARD_" + System.DateTime.Now.Ticks;

        // Cria resposta simulada com os totais acumulados
        ScoreData currentScore = GameResultData.GetScore();

        EndGameResponseModel fakeResponse = new EndGameResponseModel
        {
            nfcId = fakeCardId,
            attributes = new EndGameResponseModel.Attributes
            {
                empathy = currentScore.empatia * 3,        // Simula acumulado (3x o valor atual)
                creativity = currentScore.criatividade * 3,
                problem_solving = currentScore.resolucaoProblemas * 3
            }
        };

        // Atualiza o texto do NFC com dados simulados
        if (nfcStatusText != null)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"[DEBUG] Cartão: {fakeCardId}");
            builder.AppendLine($"Leitor: DEBUG_READER");
            builder.AppendLine($"Empatia total: {fakeResponse.attributes.empathy}");
            builder.AppendLine($"Criatividade total: {fakeResponse.attributes.creativity}");
            builder.AppendLine($"Resolução total: {fakeResponse.attributes.problem_solving}");
            builder.AppendLine("");
            builder.AppendLine($"[Pontuação desta partida]");
            builder.AppendLine($"Empatia: +{currentScore.empatia}");
            builder.AppendLine($"Criatividade: +{currentScore.criatividade}");
            builder.AppendLine($"Resolução: +{currentScore.resolucaoProblemas}");

            nfcStatusText.text = builder.ToString();
        }

        // Toca o som do NFC para simular o feedback
        nfcSong?.Play();

        // Desliga o GameObject configurado (se houver)
        if (objectToDisableOnNFC != null)
        {
            objectToDisableOnNFC.SetActive(false);
            Debug.Log($"[DEBUG] GameObject '{objectToDisableOnNFC.name}' desligado");
        }

        // Aplica o estado pós-NFC
        ApplyPostNfcState();

        Debug.Log($"[DEBUG] NFC simulado com sucesso! Card: {fakeCardId}");
        Debug.Log($"[DEBUG] Totais simulados - Empatia: {fakeResponse.attributes.empathy}, Criatividade: {fakeResponse.attributes.creativity}, Resolução: {fakeResponse.attributes.problem_solving}");
    }
#endif
}