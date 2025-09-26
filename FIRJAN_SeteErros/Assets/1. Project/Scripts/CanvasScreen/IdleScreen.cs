using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela inicial do Jogo dos 7 Erros
/// Apresenta o título do jogo e botão para iniciar
/// </summary>
public class IdleScreen : CanvasScreen
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Text titleText;


    public override void OnEnable()
    {
        base.OnEnable();

        // Configura o botão de start se existir
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();

        // Configura o texto do título se existir
        if (titleText != null)
        {
            titleText.text = "Jogo dos 7 Erros";
        }
    }

    /// <summary>
    /// Inicia o jogo chamando a tela de gameplay
    /// </summary>
    public void StartGame()
    {
        CallNextScreen();
    }
}