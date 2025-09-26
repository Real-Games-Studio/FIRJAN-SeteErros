using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela principal do Jogo dos 7 Erros
/// Contém a mecânica principal do jogo, timer, contador de erros e hotspots
/// </summary>
public class GameplayScreen : CanvasScreen
{
    [Header("Game Configuration")]
    [SerializeField] private SevenErrorsConfig gameConfig;
    [SerializeField] private float gameTimeInSeconds = 120f; // 2 minutos

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI errorCounterText;
    [SerializeField] private Image gameImage;
    [SerializeField] private Transform gameImageParent; // Para zoom e pan
    [SerializeField] private ErrorPopup errorPopup; // Popup já existente como filho
    [SerializeField] private Image[] wrongAttemptMarkers; // Array de 3 X's para mostrar tentativas erradas

    [Header("Game Elements")]
    [SerializeField] private ErrorHotspot[] errorHotspots;

    [Header("Camera Control")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float panSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip wrongAttemptSound;

    // Game State
    private float currentTime;
    private int errorsFound = 0;
    private int totalErrors = 7;
    private int wrongAttempts = 0;
    private int maxWrongAttempts = 3;
    private bool gameActive = false;

    // Touch controls
    private Vector3 lastTouchPosition;
    private bool isDragging = false;

    // Visual feedback
    private Color originalImageColor;


    public override void TurnOn()
    {
        base.TurnOn();
        StartGame();
    }

    public override void TurnOff()
    {
        base.TurnOff();
        StopGame();
    }

    void Update()
    {
        if (gameActive)
        {
            HandleGameTimer();
            HandleTouchInput();
        }
    }

    /// <summary>
    /// Inicia o jogo resetando todos os valores
    /// </summary>
    private void StartGame()
    {
        gameActive = true;

        // Usa configuração se disponível
        if (gameConfig != null)
        {
            currentTime = gameConfig.gameTimeInSeconds;
            totalErrors = gameConfig.totalErrors;
            maxWrongAttempts = gameConfig.maxWrongAttempts;
        }
        else
        {
            currentTime = gameTimeInSeconds;
            totalErrors = 7;
            maxWrongAttempts = 3;
        }

        errorsFound = 0;
        wrongAttempts = 0;

        // Armazena cor original da imagem
        if (gameImage != null)
        {
            originalImageColor = gameImage.color;
        }

        // Reseta todos os hotspots
        foreach (var hotspot in errorHotspots)
        {
            if (hotspot != null)
            {
                hotspot.ResetHotspot();
                hotspot.OnErrorFound += OnErrorFound; // Reconecta o evento
            }
        }

        // Reseta marcadores de tentativas erradas
        ResetWrongAttemptMarkers();

        UpdateUI();

        // Reseta posição e zoom da imagem
        ResetImageTransform();
    }

    /// <summary>
    /// Para o jogo
    /// </summary>
    private void StopGame()
    {
        gameActive = false;

        // Remove listeners dos hotspots
        foreach (var hotspot in errorHotspots)
        {
            if (hotspot != null)
            {
                hotspot.OnErrorFound -= OnErrorFound;
            }
        }
    }

    /// <summary>
    /// Gerencia o timer do jogo
    /// </summary>
    private void HandleGameTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGameByTimeout();
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Gerencia entrada de toque para zoom e pan
    /// </summary>
    private void HandleTouchInput()
    {
        // Prioriza touch input quando disponível, evitando duplo processamento
        if (Input.touchCount > 0)
        {
            // Verifica se há toques na tela
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                HandleSingleTouch(touch);
            }
            else if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
        }
        // Suporte para mouse (apenas quando não há toques ativos)
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = gameCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameCamera.nearClipPlane));

                lastTouchPosition = worldPos;
                isDragging = true;
                NotifyPlayerActivity(); // Notifica atividade do jogador
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 currentMousePos = gameCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameCamera.nearClipPlane));
                Vector3 difference = lastTouchPosition - currentMousePos;
                gameImageParent.position += difference;
                lastTouchPosition = currentMousePos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            // Zoom com scroll do mouse
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                float currentZoom = gameCamera.orthographicSize;
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                gameCamera.orthographicSize = currentZoom;
            }
        }
    }

    /// <summary>
    /// Gerencia toque único (pan)
    /// </summary>
    private void HandleSingleTouch(Touch touch)
    {
        Vector3 touchWorldPos = gameCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, gameCamera.nearClipPlane));

        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPosition = touchWorldPos;
            isDragging = true;
            NotifyPlayerActivity(); // Notifica atividade do jogador
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            Vector3 difference = lastTouchPosition - touchWorldPos;
            gameImageParent.position += difference;
            lastTouchPosition = touchWorldPos;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// Gerencia zoom com pinça
    /// </summary>
    private void HandlePinchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        float currentZoom = gameCamera.orthographicSize;
        currentZoom += deltaMagnitudeDiff * zoomSpeed * 0.01f;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        gameCamera.orthographicSize = currentZoom;
    }

    /// <summary>
    /// Reseta a transformação da imagem
    /// </summary>
    private void ResetImageTransform()
    {
        if (gameImageParent != null)
        {
            gameImageParent.position = Vector3.zero;
        }

        if (gameCamera != null)
        {
            gameCamera.orthographicSize = minZoom;
        }
    }

    /// <summary>
    /// Chamado quando um erro é encontrado
    /// </summary>
    private void OnErrorFound(int errorIndex)
    {
        if (!gameActive) return;

        errorsFound++;
        UpdateUI();

        // Notifica atividade do jogador
        NotifyPlayerActivity();

        // Mostra popup com mensagem educativa
        ShowErrorPopup(errorIndex);

        // Verifica se todos os erros foram encontrados
        if (errorsFound >= totalErrors)
        {
            EndGameByCompletion();
        }
    }

    /// <summary>
    /// Mostra popup com mensagem educativa
    /// </summary>
    private void ShowErrorPopup(int errorIndex)
    {
        if (errorPopup != null)
        {
            string message = gameConfig != null ? gameConfig.GetErrorMessage(errorIndex) : "Erro encontrado!";
            errorPopup.ShowPopup(message);
        }
    }

    /// <summary>
    /// Termina o jogo por completar todos os erros
    /// </summary>
    private void EndGameByCompletion()
    {
        gameActive = false;

        // Salva dados do resultado
        GameResultData.errorsFound = errorsFound;
        GameResultData.timeRemaining = currentTime;
        GameResultData.completedAllErrors = true;
        GameResultData.maxWrongAttemptsReached = false;
        GameResultData.wrongAttempts = wrongAttempts;

        CallNextScreen();
    }

    /// <summary>
    /// Termina o jogo por timeout
    /// </summary>
    private void EndGameByTimeout()
    {
        gameActive = false;

        // Salva dados do resultado
        GameResultData.errorsFound = errorsFound;
        GameResultData.timeRemaining = 0f;
        GameResultData.completedAllErrors = false;
        GameResultData.maxWrongAttemptsReached = false;
        GameResultData.wrongAttempts = wrongAttempts;

        CallNextScreen();
    }

    /// <summary>
    /// Termina o jogo por excesso de tentativas erradas
    /// </summary>
    private void EndGameByWrongAttempts()
    {
        gameActive = false;

        // Salva dados do resultado
        GameResultData.errorsFound = errorsFound;
        GameResultData.timeRemaining = currentTime;
        GameResultData.completedAllErrors = false;
        GameResultData.maxWrongAttemptsReached = true;
        GameResultData.wrongAttempts = wrongAttempts;

        // Mostra popup com mensagem de tentativas esgotadas
        if (errorPopup != null && gameConfig != null)
        {
            errorPopup.ShowPopup(gameConfig.maxWrongAttemptsMessage, () =>
            {
                // Quando o popup for fechado, vai para a tela de resultados
                CallNextScreen();
            });
        }
        else
        {
            // Fallback se não houver popup configurado
            CallNextScreen();
        }
    }

    /// <summary>
    /// Atualiza elementos da UI
    /// </summary>
    private void UpdateUI()
    {
        UpdateTimerUI();
        UpdateErrorCounterUI();
    }

    /// <summary>
    /// Atualiza o timer na UI
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Atualiza o contador de erros na UI
    /// </summary>
    private void UpdateErrorCounterUI()
    {
        if (errorCounterText != null)
        {
            errorCounterText.text = $"{errorsFound}/{totalErrors}";
        }
    }

    /// <summary>
    /// Para teste - força o fim do jogo
    /// </summary>
    [ContextMenu("Force Game End")]
    private void ForceGameEnd()
    {
        EndGameByTimeout();
    }

    /// <summary>
    /// Para teste - adiciona um erro
    /// </summary>
    [ContextMenu("Add Error")]
    private void TestAddError()
    {
        OnErrorFound(errorsFound);
    }

    /// <summary>
    /// Notifica o ScreenCanvasController sobre atividade do jogador
    /// </summary>
    private void NotifyPlayerActivity()
    {
        if (ScreenCanvasController.instance != null)
        {
            ScreenCanvasController.instance.NFCInputHandler("player_activity");
        }
    }





    /// <summary>
    /// Chamado quando clica na imagem de fundo (tentativa errada)
    /// </summary>
    public void OnBackgroundClicked()
    {
        OnWrongAttempt();
    }

    /// <summary>
    /// Chamado quando o jogador erra (clica fora dos hotspots)
    /// </summary>
    private void OnWrongAttempt()
    {
        if (!gameActive) return;

        Debug.Log($"Tentativa errada registrada! Total: {wrongAttempts + 1}");

        wrongAttempts++;
        NotifyPlayerActivity();

        // Toca som de erro
        PlayWrongAttemptSound();

        // Mostra feedback visual
        StartCoroutine(ShowWrongAttemptFeedback());

        // Atualiza marcadores visuais
        UpdateWrongAttemptMarkers();

        // Verifica se atingiu o máximo de tentativas erradas
        if (wrongAttempts >= maxWrongAttempts)
        {
            EndGameByWrongAttempts();
        }
    }

    /// <summary>
    /// Toca som de tentativa errada
    /// </summary>
    private void PlayWrongAttemptSound()
    {
        if (audioSource != null && wrongAttemptSound != null)
        {
            audioSource.PlayOneShot(wrongAttemptSound);
        }
    }

    /// <summary>
    /// Feedback visual para tentativa errada (tela vermelha)
    /// </summary>
    private System.Collections.IEnumerator ShowWrongAttemptFeedback()
    {
        if (gameImage != null && gameConfig != null)
        {
            // Aplica tint vermelho
            Color redTint = originalImageColor;
            redTint.r = Mathf.Clamp01(originalImageColor.r + gameConfig.redTintIntensity);
            gameImage.color = redTint;

            // Espera a duração configurada
            yield return new WaitForSeconds(gameConfig.redTintDuration);

            // Volta à cor original
            gameImage.color = originalImageColor;
        }
    }

    /// <summary>
    /// Atualiza os marcadores visuais de tentativas erradas (X's)
    /// </summary>
    private void UpdateWrongAttemptMarkers()
    {
        if (wrongAttemptMarkers == null) return;

        for (int i = 0; i < wrongAttemptMarkers.Length; i++)
        {
            if (wrongAttemptMarkers[i] != null)
            {
                wrongAttemptMarkers[i].gameObject.SetActive(i < wrongAttempts);
            }
        }
    }

    /// <summary>
    /// Reseta os marcadores de tentativas erradas
    /// </summary>
    private void ResetWrongAttemptMarkers()
    {
        if (wrongAttemptMarkers == null) return;

        foreach (var marker in wrongAttemptMarkers)
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }
        }
    }
}