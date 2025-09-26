using UnityEngine;

/// <summary>
/// Classe estática para armazenar dados do resultado do jogo
/// Permite comunicação entre as telas de gameplay e resultados
/// </summary>
public static class GameResultData
{
    public static int errorsFound = 0;
    public static float timeRemaining = 0f;
    public static bool completedAllErrors = false;
    public static bool maxWrongAttemptsReached = false;
    public static int wrongAttempts = 0;
    public static SevenErrorsConfig gameConfig = null;

    /// <summary>
    /// Calcula a pontuação baseada nos erros encontrados
    /// </summary>
    /// <returns>ScoreData com as pontuações de cada habilidade</returns>
    public static ScoreData GetScore()
    {
        if (gameConfig != null)
        {
            return gameConfig.CalculateScore(errorsFound);
        }

        // Fallback para valores padrão
        ScoreData score = new ScoreData();

        if (errorsFound >= 5) // 5 a 7 erros
        {
            score.empatia = 8;
            score.criatividade = 7;
            score.resolucaoProblemas = 6;
        }
        else if (errorsFound >= 2) // 2 a 4 erros
        {
            score.empatia = 7;
            score.criatividade = 6;
            score.resolucaoProblemas = 5;
        }
        else // menos de 2 erros
        {
            score.empatia = 6;
            score.criatividade = 5;
            score.resolucaoProblemas = 4;
        }

        return score;
    }

    /// <summary>
    /// Retorna a mensagem apropriada baseada no resultado
    /// </summary>
    /// <returns>Mensagem de resultado</returns>
    public static string GetResultMessage()
    {
        if (gameConfig != null)
        {
            return gameConfig.GetResultMessage(completedAllErrors, maxWrongAttemptsReached);
        }

        // Fallback para mensagens padrão
        if (completedAllErrors)
        {
            return "Parabéns! Você encontrou os 7 erros que definitivamente não serão repetidos no próximo evento! Os pontos das habilidades foram creditados em seu cartão:";
        }
        else if (maxWrongAttemptsReached)
        {
            return "Você não encontrou todos os 7 erros. Respire fundo, observe com atenção e tente novamente.";
        }
        else
        {
            return "Tempo esgotado! Você não encontrou todos os 7 erros. Respire fundo, observe com atenção e tente novamente.";
        }
    }    /// <summary>
         /// Reseta os dados do resultado
         /// </summary>
    public static void Reset()
    {
        errorsFound = 0;
        timeRemaining = 0f;
        completedAllErrors = false;
        maxWrongAttemptsReached = false;
        wrongAttempts = 0;
        // gameConfig mantém sua referência
    }

    /// <summary>
    /// Define a configuração do jogo
    /// </summary>
    /// <param name="config">Configuração do jogo</param>
    public static void SetGameConfig(SevenErrorsConfig config)
    {
        gameConfig = config;
    }
}

/// <summary>
/// Estrutura para armazenar dados de pontuação
/// </summary>
[System.Serializable]
public struct ScoreData
{
    public int empatia;
    public int criatividade;
    public int resolucaoProblemas;

    public ScoreData(int empatia = 0, int criatividade = 0, int resolucaoProblemas = 0)
    {
        this.empatia = empatia;
        this.criatividade = criatividade;
        this.resolucaoProblemas = resolucaoProblemas;
    }

    public override string ToString()
    {
        return $"Empatia: {empatia}\nCriatividade: {criatividade}\nResolução de problemas: {resolucaoProblemas}";
    }
}