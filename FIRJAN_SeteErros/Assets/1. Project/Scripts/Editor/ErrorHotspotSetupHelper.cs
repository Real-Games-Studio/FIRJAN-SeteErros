using UnityEngine;
using UnityEditor;

/// <summary>
/// Utilitário para ajudar na configuração dos hotspots dos erros no editor
/// </summary>
#if UNITY_EDITOR
[System.Serializable]
public class ErrorHotspotSetupHelper : MonoBehaviour
{
    [Header("Hotspot Configuration")]
    [SerializeField] private SevenErrorsConfig gameConfig;
    [SerializeField] private GameObject hotspotPrefab;
    [SerializeField] private Transform hotspotsParent;
    [SerializeField] private RectTransform imageTransform;
    
    [Header("Auto Setup")]
    [SerializeField] private bool showHotspots = false;
    
    /// <summary>
    /// Cria hotspots automaticamente baseado na configuração
    /// </summary>
    [ContextMenu("Create All Hotspots")]
    public void CreateAllHotspots()
    {
        if (gameConfig == null)
        {
            Debug.LogError("Game Config is required!");
            return;
        }
        
        if (hotspotPrefab == null)
        {
            Debug.LogError("Hotspot Prefab is required!");
            return;
        }
        
        if (hotspotsParent == null)
        {
            Debug.LogError("Hotspots Parent is required!");
            return;
        }
        
        // Remove hotspots existentes
        ClearAllHotspots();
        
        // Cria novos hotspots
        for (int i = 0; i < gameConfig.errors.Length; i++)
        {
            CreateHotspot(i);
        }
        
        Debug.Log($"Created {gameConfig.errors.Length} hotspots successfully!");
    }
    
    /// <summary>
    /// Cria um hotspot individual
    /// </summary>
    /// <param name="errorIndex">Índice do erro</param>
    private void CreateHotspot(int errorIndex)
    {
        GameObject hotspotObj = PrefabUtility.InstantiatePrefab(hotspotPrefab, hotspotsParent) as GameObject;
        
        if (hotspotObj == null)
        {
            Debug.LogError($"Failed to create hotspot {errorIndex}!");
            return;
        }
        
        // Configura nome
        hotspotObj.name = $"Hotspot_{errorIndex:00}_{gameConfig.GetErrorName(errorIndex)}";
        
        // Configura posição e tamanho se temos imageTransform
        if (imageTransform != null)
        {
            RectTransform hotspotRect = hotspotObj.GetComponent<RectTransform>();
            if (hotspotRect != null)
            {
                Vector2 normalizedPos = gameConfig.errors[errorIndex].recommendedPosition;
                Vector2 normalizedSize = gameConfig.errors[errorIndex].recommendedSize;
                
                // Converte posição normalizada para posição local
                Vector2 imageSize = imageTransform.rect.size;
                Vector2 localPos = new Vector2(
                    (normalizedPos.x - 0.5f) * imageSize.x,
                    (normalizedPos.y - 0.5f) * imageSize.y
                );
                
                hotspotRect.anchoredPosition = localPos;
                hotspotRect.sizeDelta = new Vector2(
                    normalizedSize.x * 100f, // Tamanho base de 100 pixels
                    normalizedSize.y * 100f
                );
            }
        }
        
        // Configura o componente ErrorHotspot
        ErrorHotspot hotspotComponent = hotspotObj.GetComponent<ErrorHotspot>();
        if (hotspotComponent == null)
        {
            hotspotComponent = hotspotObj.AddComponent<ErrorHotspot>();
        }
        
        // Usa reflexão para definir o errorIndex (caso o campo seja privado)
        System.Reflection.FieldInfo errorIndexField = typeof(ErrorHotspot).GetField("errorIndex", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (errorIndexField != null)
        {
            errorIndexField.SetValue(hotspotComponent, errorIndex);
        }
    }
    
    /// <summary>
    /// Remove todos os hotspots existentes
    /// </summary>
    [ContextMenu("Clear All Hotspots")]
    public void ClearAllHotspots()
    {
        if (hotspotsParent == null) return;
        
        int childCount = hotspotsParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = hotspotsParent.GetChild(i);
            if (child.GetComponent<ErrorHotspot>() != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        Debug.Log("All hotspots cleared!");
    }
    
    /// <summary>
    /// Alterna visibilidade dos hotspots para debug
    /// </summary>
    [ContextMenu("Toggle Hotspots Visibility")]
    public void ToggleHotspotsVisibility()
    {
        showHotspots = !showHotspots;
        
        if (hotspotsParent == null) return;
        
        foreach (Transform child in hotspotsParent)
        {
            ErrorHotspot hotspot = child.GetComponent<ErrorHotspot>();
            if (hotspot != null)
            {
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = showHotspots ? Color.red : Color.clear;
                }
            }
        }
        
        Debug.Log($"Hotspots visibility: {(showHotspots ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// Organiza hotspots em grid para facilitar edição
    /// </summary>
    [ContextMenu("Arrange Hotspots in Grid")]
    public void ArrangeHotspotsInGrid()
    {
        if (hotspotsParent == null) return;
        
        ErrorHotspot[] hotspots = hotspotsParent.GetComponentsInChildren<ErrorHotspot>();
        
        int columns = 3;
        float spacing = 200f;
        Vector2 startPosition = new Vector2(-spacing, spacing);
        
        for (int i = 0; i < hotspots.Length; i++)
        {
            RectTransform rectTransform = hotspots[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int row = i / columns;
                int col = i % columns;
                
                Vector2 position = startPosition + new Vector2(col * spacing, -row * spacing);
                rectTransform.anchoredPosition = position;
            }
        }
        
        Debug.Log($"Arranged {hotspots.Length} hotspots in grid!");
    }
    
    /// <summary>
    /// Valida configuração dos hotspots
    /// </summary>
    [ContextMenu("Validate Hotspot Setup")]
    public void ValidateHotspotSetup()
    {
        if (gameConfig == null)
        {
            Debug.LogError("Game Config is missing!");
            return;
        }
        
        if (hotspotsParent == null)
        {
            Debug.LogError("Hotspots Parent is missing!");
            return;
        }
        
        ErrorHotspot[] hotspots = hotspotsParent.GetComponentsInChildren<ErrorHotspot>();
        
        Debug.Log($"Found {hotspots.Length} hotspots (expected: {gameConfig.totalErrors})");
        
        if (hotspots.Length != gameConfig.totalErrors)
        {
            Debug.LogWarning($"Hotspot count mismatch! Expected: {gameConfig.totalErrors}, Found: {hotspots.Length}");
        }
        
        // Verifica se todos os hotspots têm Collider2D
        foreach (var hotspot in hotspots)
        {
            if (hotspot.GetComponent<Collider2D>() == null)
            {
                Debug.LogWarning($"Hotspot {hotspot.name} is missing Collider2D component!", hotspot);
            }
        }
        
        Debug.Log("Hotspot validation completed!");
    }
}
#endif