# Sistema de Gerenciamento de Idiomas (LanguageManager)

## 📋 Visão Geral

Este sistema permite que o jogo suporte múltiplos idiomas (Português e Inglês) de forma simples e automática.

## 🗂️ Arquivos Criados

1. **LanguageManager.cs** - Gerenciador principal de idiomas
2. **LanguageSwitcher.cs** - Componente para botões de troca de idioma
3. **LocalizedText.cs** - Componente para textos que são traduzidos automaticamente
4. **language_pt.json** - Arquivo JSON com todas as traduções em Português
5. **language_en.json** - Arquivo JSON com todas as traduções em Inglês

## 🚀 Como Usar

### 1. Configuração Inicial (Unity Editor)

1. **Criar GameObject do LanguageManager:**
   - Na cena principal, crie um GameObject vazio chamado "LanguageManager"
   - Adicione o componente `LanguageManager.cs`
   - Arraste os arquivos JSON para os campos:
     - `Portuguese Json`: language_pt.json
     - `English Json`: language_en.json
   - Selecione o idioma inicial em `Current Language`

2. **Criar Botões de Troca de Idioma:**
   - Crie dois botões na UI (um para PT, outro para EN)
   - Adicione o componente `LanguageSwitcher.cs` em cada botão
   - No botão PT, configure:
     - `Target Language`: Portuguese
   - No botão EN, configure:
     - `Target Language`: English
   - (Opcional) Configure feedback visual:
     - `Button Image`: A imagem do botão
     - `Selected Color`: Cor quando selecionado (ex: branco)
     - `Unselected Color`: Cor quando não selecionado (ex: cinza)

3. **Adicionar Textos Localizados:**
   - Em qualquer TextMeshProUGUI que precise ser traduzido:
   - Adicione o componente `LocalizedText.cs`
   - Selecione a categoria do texto em `Text Key`
   - Se o texto tiver valores dinâmicos (como {0}, {1}), configure em `Format Values`

### 2. Uso por Código

```csharp
// Trocar para Português
LanguageManager.Instance.SetPortuguese();

// Trocar para Inglês
LanguageManager.Instance.SetEnglish();

// Alternar entre idiomas
LanguageManager.Instance.ToggleLanguage();

// Obter idioma atual
LanguageManager.Language current = LanguageManager.Instance.GetCurrentLanguage();

// Obter texto específico
string errorTitle = LanguageManager.Instance.GetErrorTitle(0); // Título do erro 1
string errorMessage = LanguageManager.Instance.GetErrorMessage(0); // Mensagem do erro 1

// Obter título de situação com número dinâmico
string situationTitle = LanguageManager.Instance.GetSituationTitle(1); // "Situação 1" ou "Situation 1"

// Acessar todos os dados
var data = LanguageManager.Instance.GetCurrentData();
string ctaTitle = data.cta.titulo1;
string headerTitle = data.header.titulo1;

// Escutar mudanças de idioma
void OnEnable()
{
    LanguageManager.OnLanguageChanged += OnLanguageChanged;
}

void OnDisable()
{
    LanguageManager.OnLanguageChanged -= OnLanguageChanged;
}

void OnLanguageChanged()
{
    Debug.Log("Idioma alterado!");
    // Atualize sua UI aqui
}
```

### 3. LocalizedText - Uso Avançado

```csharp
// Para textos com valores dinâmicos
LocalizedText localizedText = GetComponent<LocalizedText>();
localizedText.SetFormatValues("1"); // Para "Situação {0}" -> "Situação 1"

// Forçar atualização
localizedText.UpdateText();
```

## 📁 Estrutura dos Arquivos JSON

Os arquivos JSON estão organizados em seções:

- **common**: Textos compartilhados
- **cta**: Tela de chamada para ação (Call to Action)
- **situation1, situation2, situation3**: Textos das situações do jogo de empatia
- **situation_results**: Tela de resultados das situações
- **game_over**: Tela final
- **header**: Cabeçalho e popups
- **seven_errors**: Todos os textos do jogo dos 7 erros

### Exemplo de Acesso:

```json
{
    "seven_errors": {
        "gameTitle": "JOGO DOS 7 ERROS",
        "error1Title": "Respeito às Filas Preferenciais"
    }
}
```

```csharp
// Acesso no código
string title = LanguageManager.Instance.GetCurrentData().seven_errors.gameTitle;
```

## ✨ Recursos Especiais

### 1. Feedback Visual Automático nos Botões
Os botões de idioma mudam automaticamente de cor quando o idioma é trocado.

### 2. Atualização Automática de Textos
Todos os componentes `LocalizedText` são atualizados automaticamente quando o idioma muda.

### 3. Formatação Dinâmica
Suporta textos com placeholders como "Situação {0}" que são substituídos por valores dinâmicos.

### 4. Event System
O sistema dispara eventos quando o idioma muda, permitindo que outros sistemas reajam.

## 🔧 Integração com SevenErrorsConfig

O `SevenErrorsConfig` pode ser integrado com o LanguageManager:

```csharp
// Em vez de usar os textos do SevenErrorsConfig diretamente,
// use o LanguageManager para obter os textos traduzidos

// Antes:
string errorTitle = sevenErrorsConfig.GetErrorTitle(0);

// Depois (com LanguageManager):
string errorTitle = LanguageManager.Instance.GetErrorTitle(0);
```

## 📝 Adicionando Novos Textos

Para adicionar novos textos ao sistema:

1. **Adicione ao JSON** (nos dois arquivos: PT e EN):
```json
{
    "seven_errors": {
        "newText": "Novo Texto"
    }
}
```

2. **Adicione à classe LanguageData** em LanguageManager.cs:
```csharp
[System.Serializable]
public class SevenErrorsTexts
{
    // ... textos existentes ...
    public string newText;
}
```

3. **Adicione à enumeração** em LocalizedText.cs:
```csharp
public enum TextCategory
{
    // ... categorias existentes ...
    SevenErrorsNewText,
}
```

4. **Adicione ao switch** em LocalizedText.cs:
```csharp
case TextCategory.SevenErrorsNewText:
    return data.seven_errors?.newText ?? "";
```

## ⚠️ Notas Importantes

- Os arquivos JSON devem estar na pasta `StreamingAssets` para serem acessíveis
- O LanguageManager usa Singleton pattern - só pode haver uma instância
- O componente LocalizedText requer TextMeshProUGUI
- Todos os textos têm fallback para string vazia ("") se não encontrados
- As tags HTML do TextMeshPro são preservadas (como `<br>`, `<color>`, `<size>`)

## 🎯 Exemplo Completo de Uso

```csharp
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI errorCountText;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        var data = LanguageManager.Instance.GetCurrentData();
        titleText.text = data.seven_errors.gameTitle;

        // Para texto com formatação
        int errorsFound = 3;
        errorCountText.text = $"{data.seven_errors.errorsFound}: {errorsFound}/7";
    }
}
```

## 🐛 Troubleshooting

**Problema**: Textos não aparecem
- Verifique se os arquivos JSON estão na pasta StreamingAssets
- Verifique se os JSONs estão atribuídos no LanguageManager
- Verifique se há erros de sintaxe no JSON

**Problema**: Idioma não muda
- Verifique se o LanguageManager está na cena
- Verifique se os botões têm o componente LanguageSwitcher
- Verifique o Console do Unity para erros

**Problema**: Alguns textos não são traduzidos
- Verifique se o LocalizedText está adicionado ao GameObject
- Verifique se a categoria correta está selecionada
- Verifique se o texto existe no JSON
