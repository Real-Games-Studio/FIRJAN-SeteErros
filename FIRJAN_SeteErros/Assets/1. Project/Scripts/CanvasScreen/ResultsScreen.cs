using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI empatiaScoreText;
    [SerializeField] private TextMeshProUGUI criatividadeScoreText;
    [SerializeField] private TextMeshProUGUI resolucaoProblemasScoreText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Conditional Elements")]
    [SerializeField] private GameObject scorePanel; // Painel de pontuação (só aparece se completou)
    [SerializeField] private GameObject timeoutPanel; // Painel de timeout (só aparece se não completou)


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

    public override void TurnOn()
    {
        base.TurnOn();
        DisplayResults();
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

        // Exibe pontuações das habilidades
        if (empatiaScoreText != null)
        {
            empatiaScoreText.text = $"Empatia: {score.empatia}";
        }

        if (criatividadeScoreText != null)
        {
            criatividadeScoreText.text = $"Criatividade: {score.criatividade}";
        }

        if (resolucaoProblemasScoreText != null)
        {
            resolucaoProblemasScoreText.text = $"Resolução de problemas: {score.resolucaoProblemas}";
        }

        // Configura painéis condicionais
        ConfigureConditionalPanels();
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
        // Reseta os dados do jogo
        GameResultData.Reset();

        // Volta para a tela de gameplay
        CallScreenByName("GameplayScreen");
    }

    /// <summary>
    /// Volta para o menu principal
    /// </summary>
    public void BackToMenu()
    {
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
}