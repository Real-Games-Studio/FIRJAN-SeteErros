using UnityEngine;
using System.Collections;

/// <summary>
/// Gerenciador principal do Jogo dos 7 Erros
/// Controla configurações globais e estados do jogo
/// </summary>
public class SevenErrorsGameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float gameTimeLimit = 120f; // 2 minutes
    
    [Header("Screen Names")]
    [SerializeField] private string idleScreenName = "IdleScreen";
    [SerializeField] private string gameplayScreenName = "GameplayScreen";
    [SerializeField] private string resultsScreenName = "ResultsScreen";
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorFoundSound;
    [SerializeField] private AudioClip timeoutSound;
    
    // Singleton instance
    public static SevenErrorsGameManager Instance { get; private set; }
    
    // Game state
    private GameState currentState = GameState.Menu;
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Results
    }
    
    private void Awake()
    {
        // Implementa singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// Inicializa configurações do jogo
    /// </summary>
    private void InitializeGame()
    {
        // Configurações de tela
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Inicia música de fundo se configurada
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
        
        // Define estado inicial
        SetGameState(GameState.Menu);
        
        Debug.Log("Seven Errors Game initialized successfully!");
    }
    
    /// <summary>
    /// Inicia uma nova partida
    /// </summary>
    public void StartNewGame()
    {
        GameResultData.Reset();
        SetGameState(GameState.Playing);
        ScreenManager.SetCallScreen(gameplayScreenName);
        
        if (debugMode)
        {
            Debug.Log("New game started!");
        }
    }
    
    /// <summary>
    /// Pausa o jogo
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }
    
    /// <summary>
    /// Resume o jogo
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }
    
    /// <summary>
    /// Termina o jogo e vai para resultados
    /// </summary>
    public void EndGame()
    {
        SetGameState(GameState.Results);
        ScreenManager.SetCallScreen(resultsScreenName);
    }
    
    /// <summary>
    /// Volta ao menu principal
    /// </summary>
    public void BackToMenu()
    {
        GameResultData.Reset();
        SetGameState(GameState.Menu);
        ScreenManager.SetCallScreen(idleScreenName);
        Time.timeScale = 1f; // Garante que o tempo volte ao normal
    }
    
    /// <summary>
    /// Define o estado do jogo
    /// </summary>
    /// <param name="newState">Novo estado do jogo</param>
    private void SetGameState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;
        
        if (debugMode)
        {
            Debug.Log($"Game state changed from {previousState} to {newState}");
        }
        
        // Lógica adicional baseada no estado
        OnGameStateChanged(newState, previousState);
    }
    
    /// <summary>
    /// Chamado quando o estado do jogo muda
    /// </summary>
    /// <param name="newState">Novo estado</param>
    /// <param name="previousState">Estado anterior</param>
    private void OnGameStateChanged(GameState newState, GameState previousState)
    {
        switch (newState)
        {
            case GameState.Menu:
                // Lógica para menu
                break;
                
            case GameState.Playing:
                // Lógica para gameplay
                break;
                
            case GameState.Paused:
                // Lógica para pausa
                break;
                
            case GameState.Results:
                // Lógica para resultados
                break;
        }
    }
    
    /// <summary>
    /// Toca som de erro encontrado
    /// </summary>
    public void PlayErrorFoundSound()
    {
        if (errorFoundSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(errorFoundSound);
        }
    }
    
    /// <summary>
    /// Toca som de sucesso
    /// </summary>
    public void PlaySuccessSound()
    {
        if (successSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(successSound);
        }
    }
    
    /// <summary>
    /// Toca som de timeout
    /// </summary>
    public void PlayTimeoutSound()
    {
        if (timeoutSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(timeoutSound);
        }
    }
    
    /// <summary>
    /// Retorna o estado atual do jogo
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Retorna se o jogo está em modo debug
    /// </summary>
    public bool IsDebugMode()
    {
        return debugMode;
    }
    
    /// <summary>
    /// Retorna o tempo limite configurado para o jogo
    /// </summary>
    public float GetGameTimeLimit()
    {
        return gameTimeLimit;
    }
    
    /// <summary>
    /// Força o fim do jogo (para debug)
    /// </summary>
    [ContextMenu("Force End Game")]
    private void ForceEndGame()
    {
        EndGame();
    }
    
    /// <summary>
    /// Alterna modo debug
    /// </summary>
    [ContextMenu("Toggle Debug Mode")]
    private void ToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"Debug mode: {(debugMode ? "ON" : "OFF")}");
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentState == GameState.Playing)
        {
            PauseGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentState == GameState.Playing)
        {
            PauseGame();
        }
    }
}