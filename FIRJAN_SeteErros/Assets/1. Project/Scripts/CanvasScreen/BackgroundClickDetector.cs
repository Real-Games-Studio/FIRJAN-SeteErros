using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para detectar cliques na imagem de fundo
/// Quando clicado, registra como tentativa errada
/// </summary>
[RequireComponent(typeof(Button))]
public class BackgroundClickDetector : MonoBehaviour
{
    private GameplayScreen gameplayScreen;
    private Button backgroundButton;

    private void Awake()
    {
        backgroundButton = GetComponent<Button>();
        gameplayScreen = FindFirstObjectByType<GameplayScreen>();

        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(OnBackgroundClicked);
        }
    }

    private void OnBackgroundClicked()
    {
        if (gameplayScreen != null && backgroundButton.interactable)
        {
            Debug.Log("Clique na imagem de fundo detectado!");
            gameplayScreen.OnBackgroundClicked();
        }
    }

    /// <summary>
    /// Simula um clique no background (para testes)
    /// </summary>
    public void SimulateClick()
    {
        OnBackgroundClicked();
    }

    private void OnDestroy()
    {
        if (backgroundButton != null)
        {
            backgroundButton.onClick.RemoveListener(OnBackgroundClicked);
        }
    }
}