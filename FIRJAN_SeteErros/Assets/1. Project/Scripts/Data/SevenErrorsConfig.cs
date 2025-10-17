using UnityEngine;

/// <summary>
/// Configuração dos 7 erros do jogo
/// Contém as mensagens educativas e configurações para cada erro
/// </summary>
[CreateAssetMenu(fileName = "SevenErrorsConfig", menuName = "Seven Errors Game/Error Configuration")]
public class SevenErrorsConfig : ScriptableObject
{
    [System.Serializable]
    public class ErrorData
    {
        public string errorTitle;
        [TextArea(3, 5)]
        public string errorMessage;
        public string errorName;
        [Tooltip("Posição recomendada do hotspot na imagem (normalizada 0-1)")]
        public Vector2 recommendedPosition = Vector2.zero;
        [Tooltip("Tamanho recomendado do hotspot")]
        public Vector2 recommendedSize = Vector2.one;
    }

    [Header("Game Configuration")]
    public float gameTimeInSeconds = 120f;
    public int totalErrors = 7;
    public int maxWrongAttempts = 3;

    [Header("Wrong Attempt Settings")]
    [Range(0f, 1f)]
    public float redTintIntensity = 0.3f;
    public float redTintDuration = 0.5f;

    [Header("Error Messages")]
    public ErrorData[] errors = new ErrorData[7];

    [Header("Result Messages")]
    [TextArea(3, 5)]
    public string successMessage = "Parabéns! Você encontrou os 7 erros que definitivamente não serão repetidos no próximo evento! Os pontos das habilidades foram creditados em seu cartão:";

    [TextArea(3, 5)]
    public string timeoutMessage = "Tempo esgotado! Você não encontrou todos os 7 erros. Respire fundo, observe com atenção e tente novamente.";

    [TextArea(3, 5)]
    public string maxWrongAttemptsMessage = "Você não encontrou todos os 7 erros. Respire fundo, observe com atenção e tente novamente.";

    [Header("Score Configuration")]
    public ScoreRange highScore = new ScoreRange { empatia = 8, criatividade = 7, resolucaoProblemas = 6 };
    public ScoreRange mediumScore = new ScoreRange { empatia = 7, criatividade = 6, resolucaoProblemas = 5 };
    public ScoreRange lowScore = new ScoreRange { empatia = 6, criatividade = 5, resolucaoProblemas = 4 };

    [Header("Score Thresholds")]
    [Range(0, 7)]
    public int highScoreThreshold = 5; // 5-7 errors
    [Range(0, 7)]
    public int mediumScoreThreshold = 2; // 2-4 errors

    [System.Serializable]
    public struct ScoreRange
    {
        public int empatia;
        public int criatividade;
        public int resolucaoProblemas;
    }

    private void OnValidate()
    {
        // Garante que temos exatamente 7 erros
        if (errors.Length != 7)
        {
            System.Array.Resize(ref errors, 7);
        }

        // Preenche títulos e mensagens padrão se estiverem vazias
        if (string.IsNullOrEmpty(errors[0].errorTitle))
            errors[0].errorTitle = "Respeito às Filas Preferenciais";
        if (errors[0].errorMessage == "")
            errors[0].errorMessage = "Exato! Filas preferenciais garantem o acesso de quem mais precisa. Respeitá-las é um gesto simples de cidadania e empatia.";

        if (string.IsNullOrEmpty(errors[1].errorTitle))
            errors[1].errorTitle = "Acessibilidade em Rampas";
        if (errors[1].errorMessage == "")
            errors[1].errorMessage = "Bem observado! Para quem usa cadeira de rodas, a rampa não é uma opção, é a única passagem. Garantir o acesso é garantir a liberdade de todos.";

        if (string.IsNullOrEmpty(errors[2].errorTitle))
            errors[2].errorTitle = "Segurança em Emergências";
        if (errors[2].errorMessage == "")
            errors[2].errorMessage = "Perfeito! Em uma emergência, cada segundo conta e um caminho livre pode salvar vidas. Segurança é uma responsabilidade coletiva.";

        if (string.IsNullOrEmpty(errors[3].errorTitle))
            errors[3].errorTitle = "Vagas Preferenciais";
        if (errors[3].errorMessage == "")
            errors[3].errorMessage = "Isso mesmo! Vagas preferenciais não são sobre privilégio, são sobre necessidade. Elas diminuem a distância para quem realmente precisa. Respeito é fundamental.";

        if (string.IsNullOrEmpty(errors[4].errorTitle))
            errors[4].errorTitle = "Assentos Prioritários";
        if (errors[4].errorMessage == "")
            errors[4].errorMessage = "Ótima percepção! Em espaços compartilhados, o assento prioritário garante o conforto e a segurança de quem precisa. Estar atento ao redor é o primeiro passo da empatia.";

        if (string.IsNullOrEmpty(errors[5].errorTitle))
            errors[5].errorTitle = "Cuidado com o Ambiente";
        if (errors[5].errorMessage == "")
            errors[5].errorMessage = "Você encontrou! Manter nosso ambiente limpo torna o parque mais agradável para todos e demonstra nosso cuidado com o que é coletivo. Um pequeno gesto faz uma grande diferença.";

        if (string.IsNullOrEmpty(errors[6].errorTitle))
            errors[6].errorTitle = "Inclusão e Piso Tátil";
        if (errors[6].errorMessage == "")
            errors[6].errorMessage = "Exato! Para uma pessoa com deficiência visual, o piso tátil são os olhos que a guiam pelo caminho. Bloqueá-lo é como criar uma barreira invisível. Inclusão é garantir que todos os caminhos estejam abertos.";

        // Define nomes padrão dos erros
        if (string.IsNullOrEmpty(errors[0].errorName)) errors[0].errorName = "Fila presencial ignorada";
        if (string.IsNullOrEmpty(errors[1].errorName)) errors[1].errorName = "Rampa de acesso bloqueada";
        if (string.IsNullOrEmpty(errors[2].errorName)) errors[2].errorName = "Saída de emergência obstruída";
        if (string.IsNullOrEmpty(errors[3].errorName)) errors[3].errorName = "Vaga preferencial ocupada indevidamente";
        if (string.IsNullOrEmpty(errors[4].errorName)) errors[4].errorName = "Uso indevido de assento prioritário";
        if (string.IsNullOrEmpty(errors[5].errorName)) errors[5].errorName = "Lixo jogado na rua";
        if (string.IsNullOrEmpty(errors[6].errorName)) errors[6].errorName = "Piso tátil bloqueado";
    }

    /// <summary>
    /// Retorna o título do erro pelo índice
    /// </summary>
    public string GetErrorTitle(int index)
    {
        if (index >= 0 && index < errors.Length)
        {
            return errors[index].errorTitle;
        }
        return $"Erro {index + 1}";
    }

    /// <summary>
    /// Retorna a mensagem do erro pelo índice
    /// </summary>
    public string GetErrorMessage(int index)
    {
        if (index >= 0 && index < errors.Length)
        {
            return errors[index].errorMessage;
        }
        return "Erro encontrado!";
    }

    /// <summary>
    /// Retorna o nome do erro pelo índice
    /// </summary>
    public string GetErrorName(int index)
    {
        if (index >= 0 && index < errors.Length)
        {
            return errors[index].errorName;
        }
        return $"Erro {index + 1}";
    }

    /// <summary>
    /// Calcula a pontuação baseada no número de erros encontrados
    /// </summary>
    public ScoreData CalculateScore(int errorsFound)
    {
        ScoreData score = new ScoreData();

        if (errorsFound >= highScoreThreshold)
        {
            score.empatia = highScore.empatia;
            score.criatividade = highScore.criatividade;
            score.resolucaoProblemas = highScore.resolucaoProblemas;
        }
        else if (errorsFound >= mediumScoreThreshold)
        {
            score.empatia = mediumScore.empatia;
            score.criatividade = mediumScore.criatividade;
            score.resolucaoProblemas = mediumScore.resolucaoProblemas;
        }
        else
        {
            score.empatia = lowScore.empatia;
            score.criatividade = lowScore.criatividade;
            score.resolucaoProblemas = lowScore.resolucaoProblemas;
        }

        return score;
    }

    /// <summary>
    /// Retorna a mensagem de resultado apropriada
    /// </summary>
    public string GetResultMessage(bool completedAllErrors, bool maxWrongAttemptsReached)
    {
        if (completedAllErrors)
        {
            return successMessage;
        }
        else if (maxWrongAttemptsReached)
        {
            return maxWrongAttemptsMessage;
        }
        else
        {
            return timeoutMessage;
        }
    }
}