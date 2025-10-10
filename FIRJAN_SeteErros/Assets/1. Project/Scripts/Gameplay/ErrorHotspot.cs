using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Representa um hotspot de erro clicável na imagem do jogo
/// Agora usa Button para detecção nativa de clique
/// </summary>
[RequireComponent(typeof(Button))]
public class ErrorHotspot : MonoBehaviour
{
    [Header("Error Configuration")]
    [SerializeField] private int errorIndex;
    [SerializeField] private bool isFound = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject foundIndicator; // Objeto que aparece quando encontrado (filho deste GameObject)
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightDuration = 0.5f;

    // Events
    public System.Action<int> OnErrorFound;

    // Public properties
    public bool IsFound => isFound;
    public int ErrorIndex => errorIndex;

    private Color initialColor;

    // Components
    private Button hotspotButton;
    private Image buttonImage;

    private void Awake()
    {
        hotspotButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();


        initialColor = buttonImage.color;
        // Configura o clique do botão
        if (hotspotButton != null)
        {
            hotspotButton.onClick.AddListener(OnHotspotClicked);
        }
    }

    private void Start()
    {
        // Desativa o indicador de encontrado no início
        if (foundIndicator != null)
        {
            foundIndicator.SetActive(false);
        }
        else
        {
            // Se não foi atribuído no inspector, tenta encontrar um GameObject filho com nome específico
            Transform childIndicator = transform.Find("FoundIndicator");
            if (childIndicator != null)
            {
                foundIndicator = childIndicator.gameObject;
                foundIndicator.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Chamado quando o botão do hotspot é clicado
    /// </summary>
    private void OnHotspotClicked()
    {
        if (!isFound)
        {
            FoundError();
        }
    }

    /// <summary>
    /// Implementação da interface IPointerDownHandler (desabilitado - GameplayScreen gerencia)
    /// </summary>
    /*public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFound)
        {
            FoundError();
        }
    }*/

    /// <summary>
    /// Detecta clique via OnMouseDown (desabilitado para evitar dupla detecção)
    /// </summary>
    /*private void OnMouseDown()
    {
        if (!isFound)
        {
            FoundError();
        }
    }*/

    /// <summary>
    /// Marca o erro como encontrado
    /// </summary>
    public void FoundError()
    {
        if (isFound) return;

        isFound = true;

        // Desativa o botão para não ser clicado novamente
        if (hotspotButton != null)
        {
            hotspotButton.interactable = false;
        }

        // Mostra feedback visual
        ShowFoundFeedback();

        // Dispara evento
        OnErrorFound?.Invoke(errorIndex);

        Debug.Log($"Erro {errorIndex + 1} encontrado!");
    }

    /// <summary>
    /// Mostra feedback visual quando o erro é encontrado
    /// </summary>
    private void ShowFoundFeedback()
    {
        // Ativa indicador visual (GameObject filho) quando acerta
        if (foundIndicator != null)
        {
            foundIndicator.SetActive(true);
            Debug.Log($"Ativado indicador visual para erro {errorIndex + 1}: {foundIndicator.name}");
        }

        // Efeito de highlight temporário
        StartCoroutine(HighlightEffect());
    }

    /// <summary>
    /// Efeito de highlight temporário
    /// </summary>
    private System.Collections.IEnumerator HighlightEffect()
    {
        if (buttonImage != null)
        {
            // Mostra o highlight
            buttonImage.color = highlightColor;

            // Espera a duração
            yield return new WaitForSeconds(highlightDuration);

            // // Remove o highlight
            buttonImage.color = Color.clear;
        }
    }

    /// <summary>
    /// Reseta o hotspot para o estado inicial
    /// </summary>
    public void ResetHotspot()
    {
        isFound = false;

        // Reativa o botão
        if (hotspotButton != null)
        {
            hotspotButton.interactable = true;
        }

        // Esconde o indicador
        if (foundIndicator != null)
        {
            foundIndicator.SetActive(false);
        }

        buttonImage.color = initialColor;
    }

    /// <summary>
    /// Para debug - força encontrar o erro
    /// </summary>
    [ContextMenu("Force Find Error")]
    private void ForceFind()
    {
        FoundError();
    }

    /// <summary>
    /// Mostra temporariamente o hotspot para debug
    /// </summary>
    [ContextMenu("Show Hotspot")]
    private void ShowHotspot()
    {
        if (buttonImage != null)
        {
            buttonImage.color = Color.red;
        }
    }

    /// <summary>
    /// Esconde o hotspot
    /// </summary>
    [ContextMenu("Hide Hotspot")]
    private void HideHotspot()
    {
        if (buttonImage != null)
        {
            buttonImage.color = Color.clear;
        }
    }

    // Para visualização no editor
    private void OnDrawGizmos()
    {
        Gizmos.color = isFound ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }

    /// <summary>
    /// Desabilita temporariamente o botão (durante zoom/pan)
    /// </summary>
    public void DisableButton()
    {
        if (hotspotButton != null && !isFound)
        {
            hotspotButton.interactable = false;
        }
    }

    /// <summary>
    /// Reabilita o botão após zoom/pan
    /// </summary>
    public void EnableButton()
    {
        if (hotspotButton != null && !isFound)
        {
            hotspotButton.interactable = true;
        }
    }

    /// <summary>
    /// Simula um clique no hotspot (para testes)
    /// </summary>
    public void SimulateClick()
    {
        OnHotspotClicked();
    }

    /// <summary>
    /// Configura o índice do hotspot
    /// </summary>
    public void SetErrorIndex(int index)
    {
        errorIndex = index;
    }

    private void OnDestroy()
    {
        // Remove o listener para evitar memory leaks
        if (hotspotButton != null)
        {
            hotspotButton.onClick.RemoveListener(OnHotspotClicked);
        }
    }
}