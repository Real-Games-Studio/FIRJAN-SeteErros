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
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Image gameImage;
    [SerializeField] private Transform gameImageParent; // Para zoom e pan
    [SerializeField] private ErrorPopup errorPopup; // Popup já existente como filho

    [Header("Visual Progress Indicators")]
    [SerializeField] private Transform errorsFoundParent; // Parent com 7 imagens para mostrar erros encontrados
    [SerializeField] private Color errorFoundColor = Color.green; // Cor para quando encontra um erro
    [SerializeField] private Transform wrongAttemptsParent; // Parent com 3 imagens para mostrar tentativas erradas
    [SerializeField] private Color wrongAttemptColor = Color.red; // Cor para tentativas erradas

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
    private float initialGameTime;
    private int errorsFound = 0;
    private int totalErrors = 7;
    private int wrongAttempts = 0;
    private int maxWrongAttempts = 3;
    private bool gameActive = false;

    [Header("Integrations")]
    [SerializeField] private NFCGameService nfcGameService;

    // Touch controls
    private Vector3 lastTouchPosition;
    private bool isDragging = false;
    private bool isZooming = false;
    private bool buttonsDisabled = false;
    private bool awaitingFinalPopup = false;

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
        awaitingFinalPopup = false;

        GameResultData.Reset();
        GameResultData.SetGameConfig(gameConfig);

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
        initialGameTime = currentTime;

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

        // Reseta indicadores visuais
        ResetVisualIndicators();

        // Garante que o popup comece fechado
        if (errorPopup != null)
        {
            errorPopup.gameObject.SetActive(false);
        }

        UpdateUI();

        EnsureNfcServiceReference();

        // Reseta posição e zoom da imagem
        ResetImageTransform();

        // Garante fill do timer cheio no início
        UpdateTimerFill();
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
                // Zoom com pinça - desabilita botões temporariamente
                if (!isZooming)
                {
                    StartZooming();
                }
                HandlePinchZoom();
            }

        }
        // Suporte para mouse (apenas quando não há toques ativos)
        else
        {
            // Detecta Ctrl+Click para simular zoom pinça
            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = gameCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameCamera.nearClipPlane));

                if (ctrlPressed)
                {
                    // Ctrl+Click - inicia simulação de zoom pinça
                    if (!isZooming)
                    {
                        StartZooming();
                    }
                }
                else
                {
                    // Clique normal - pan/drag
                    lastTouchPosition = worldPos;
                    isDragging = true;
                }

                NotifyPlayerActivity(); // Notifica atividade do jogador
            }
            else if (Input.GetMouseButton(0) && isDragging && !ctrlPressed)
            {
                // Pan/drag normal (sem Ctrl)
                Vector3 mousePos = Input.mousePosition;
                Vector3 currentMousePos = gameCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameCamera.nearClipPlane));
                Vector3 difference = lastTouchPosition - currentMousePos;
                gameImageParent.position += difference;
                lastTouchPosition = currentMousePos;
            }
            else if (Input.GetMouseButton(0) && ctrlPressed && isZooming)
            {
                // Simula zoom pinça com Ctrl+Drag
                HandleMouseZoomSimulation();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                if (isZooming && ctrlPressed)
                {
                    StopZooming();
                }
            }

            // Zoom com scroll do mouse
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (!isZooming)
                {
                    StartZooming();
                }

                float currentZoom = gameCamera.orthographicSize;
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                gameCamera.orthographicSize = currentZoom;

                // Para o zoom após usar scroll (com delay)
                StartCoroutine(StopZoomAfterDelay(0.1f));
            }
        }

        // Para o zoom se não há mais interações
        if (isZooming && Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            StopZooming();
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
    /// Inicia o modo zoom - desabilita botões temporariamente
    /// </summary>
    private void StartZooming()
    {
        isZooming = true;
        DisableAllButtons();
    }

    /// <summary>
    /// Para o modo zoom - reabilita botões
    /// </summary>
    private void StopZooming()
    {
        isZooming = false;
        EnableAllButtons();
    }

    /// <summary>
    /// Simula zoom pinça com mouse (Ctrl+Drag)
    /// </summary>
    private void HandleMouseZoomSimulation()
    {
        float mouseY = Input.mousePosition.y;
        float deltaY = mouseY - lastTouchPosition.y;

        float currentZoom = gameCamera.orthographicSize;
        currentZoom -= deltaY * zoomSpeed * 0.001f; // Sensibilidade ajustada
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        gameCamera.orthographicSize = currentZoom;

        lastTouchPosition = Input.mousePosition;
    }

    /// <summary>
    /// Desabilita todos os botões (hotspots e fundo)
    /// </summary>
    private void DisableAllButtons()
    {
        if (buttonsDisabled) return;

        buttonsDisabled = true;

        // Desabilita hotspots
        foreach (var hotspot in errorHotspots)
        {
            if (hotspot != null)
            {
                hotspot.DisableButton();
            }
        }

        // Desabilita botão de fundo
        var bgDetector = FindFirstObjectByType<BackgroundClickDetector>();
        if (bgDetector != null)
        {
            var button = bgDetector.GetComponent<Button>();
            if (button != null) button.interactable = false;
        }
    }

    /// <summary>
    /// Reabilita todos os botões
    /// </summary>
    private void EnableAllButtons()
    {
        if (!buttonsDisabled) return;

        buttonsDisabled = false;

        // Reabilita hotspots
        foreach (var hotspot in errorHotspots)
        {
            if (hotspot != null)
            {
                hotspot.EnableButton();
            }
        }

        // Reabilita botão de fundo
        var bgDetector = FindFirstObjectByType<BackgroundClickDetector>();
        if (bgDetector != null)
        {
            var button = bgDetector.GetComponent<Button>();
            if (button != null) button.interactable = true;
        }
    }

    /// <summary>
    /// Para o zoom após um delay (para scroll wheel)
    /// </summary>
    private System.Collections.IEnumerator StopZoomAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isZooming && Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            StopZooming();
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

        bool isLastError = errorsFound >= totalErrors;

        if (isLastError)
        {
            awaitingFinalPopup = true;
            gameActive = false; // Pausa o jogo enquanto o jogador lê o último popup

            ShowErrorPopup(errorIndex, () =>
            {
                awaitingFinalPopup = false;
                EndGameByCompletion();
            });
        }
        else
        {
            // Mostra popup normal para erros intermediários
            ShowErrorPopup(errorIndex);
        }
    }

    /// <summary>
    /// Mostra popup com mensagem educativa
    /// </summary>
    private void ShowErrorPopup(int errorIndex, System.Action onClosed = null)
    {
        if (errorPopup != null)
        {
            string message = gameConfig != null ? gameConfig.GetErrorMessage(errorIndex) : "Erro encontrado!";
            ShowBlockingPopup(message, onClosed);
        }
        else
        {
            // Se não houver popup configurado, executa imediatamente o callback
            onClosed?.Invoke();
        }
    }

    /// <summary>
    /// Exibe um popup bloqueando interações até ser fechado
    /// </summary>
    private void ShowBlockingPopup(string message, System.Action onClosed = null)
    {
        if (errorPopup != null)
        {
            DisableAllButtons();

            errorPopup.ShowPopup(message, () =>
            {
                EnableAllButtons();
                onClosed?.Invoke();
            });
        }
        else
        {
            onClosed?.Invoke();
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

        SubmitNfcScore();

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

        SubmitNfcScore();

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

        SubmitNfcScore();

        // Mostra popup com mensagem de tentativas esgotadas
        if (gameConfig != null)
        {
            ShowBlockingPopup(gameConfig.maxWrongAttemptsMessage, () =>
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
        UpdateErrorsFoundVisual();
    }

    /// <summary>
    /// Atualiza o timer na UI
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
    /// Atualiza o fill da imagem do timer
    /// </summary>
    private void UpdateTimerFill()
    {
        if (timerFillImage == null) return;

        if (initialGameTime <= 0f)
        {
            timerFillImage.fillAmount = 0f;
            return;
        }

        float normalizedTime = Mathf.Clamp01(currentTime / initialGameTime);
        timerFillImage.fillAmount = normalizedTime;
    }

    /// <summary>
    /// Atualiza os indicadores visuais dos erros encontrados
    /// </summary>
    private void UpdateErrorsFoundVisual()
    {
        if (errorsFoundParent == null) return;

        // Pega apenas as imagens dos filhos diretos, ignorando a imagem do próprio parent
        var errorImages = new List<Image>();
        for (int childIndex = 0; childIndex < errorsFoundParent.childCount; childIndex++)
        {
            var childImage = errorsFoundParent.GetChild(childIndex).GetComponent<Image>();
            if (childImage != null)
            {
                errorImages.Add(childImage);
            }
        }

        for (int i = 0; i < errorImages.Count; i++)
        {
            if (i < errorsFound)
            {
                // Pinta a imagem com a cor configurada quando encontra esse erro
                errorImages[i].color = errorFoundColor;
            }
            else
            {
                // Mantém cor original/transparente se ainda não encontrou
                errorImages[i].color = Color.white;
            }
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
            ScreenCanvasController.instance.inactiveTimer = 0;
        }
    }

    private void EnsureNfcServiceReference()
    {
        if (nfcGameService == null)
        {
            nfcGameService = FindFirstObjectByType<NFCGameService>();
            if (nfcGameService == null)
            {
                GameObject serviceObject = new GameObject("NFC Game Service");
                nfcGameService = serviceObject.AddComponent<NFCGameService>();
                Debug.Log("[NFC] NFCGameService criado dinamicamente em tempo de execução.");
            }
        }
    }

    private void SubmitNfcScore()
    {
        EnsureNfcServiceReference();

        if (nfcGameService == null)
        {
            return;
        }

        ScoreData score = GameResultData.GetScore();
        nfcGameService.SubmitGameResult(score, GameResultData.completedAllErrors, GameResultData.errorsFound, GameResultData.wrongAttempts, GameResultData.timeRemaining);
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

        // Atualiza indicadores visuais das tentativas erradas
        UpdateWrongAttemptsVisual();

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
    /// Atualiza os indicadores visuais das tentativas erradas
    /// </summary>
    private void UpdateWrongAttemptsVisual()
    {
        if (wrongAttemptsParent == null) return;

        // Pega apenas as imagens dos filhos diretos, ignorando a imagem do próprio parent
        var wrongAttemptImages = new List<Image>();
        for (int childIndex = 0; childIndex < wrongAttemptsParent.childCount; childIndex++)
        {
            var childImage = wrongAttemptsParent.GetChild(childIndex).GetComponent<Image>();
            if (childImage != null)
            {
                wrongAttemptImages.Add(childImage);
            }
        }

        for (int i = 0; i < wrongAttemptImages.Count; i++)
        {
            if (i < wrongAttempts)
            {
                // Pinta a imagem com a cor configurada quando erra essa tentativa
                wrongAttemptImages[i].color = wrongAttemptColor;
            }
            else
            {
                // Mantém cor original/transparente se ainda não errou
                wrongAttemptImages[i].color = Color.white;
            }
        }
    }

    /// <summary>
    /// Reseta os indicadores visuais
    /// </summary>
    private void ResetVisualIndicators()
    {
        // Reseta indicadores de erros encontrados - apenas filhos diretos
        if (errorsFoundParent != null)
        {
            for (int childIndex = 0; childIndex < errorsFoundParent.childCount; childIndex++)
            {
                var childImage = errorsFoundParent.GetChild(childIndex).GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = Color.white;
                }
            }
        }

        // Reseta indicadores de tentativas erradas - apenas filhos diretos
        if (wrongAttemptsParent != null)
        {
            for (int childIndex = 0; childIndex < wrongAttemptsParent.childCount; childIndex++)
            {
                var childImage = wrongAttemptsParent.GetChild(childIndex).GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = Color.white;
                }
            }
        }
    }


}