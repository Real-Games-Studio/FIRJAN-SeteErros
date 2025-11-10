# 🌐 Sistema de Idiomas - Jogo dos 7 Erros

## 📦 Arquivos do Sistema

```
Assets/
├── 1. Project/Scripts/
│   ├── Managers/
│   │   └── LanguageManager.cs ⭐
│   └── UI/
│       ├── LanguageSwitcher.cs
│       └── LocalizedText.cs
└── StreamingAssets/
    ├── language_pt.json 🇧🇷
    └── language_en.json 🇺🇸
```

---

## 🚀 Setup Rápido

### 1️⃣ Adicionar LanguageManager na Cena
1. Criar GameObject vazio: "LanguageManager"
2. Add Component → `LanguageManager`
3. Arrastar os JSONs:
   - Portuguese Json → `language_pt.json`
   - English Json → `language_en.json`

### 2️⃣ Criar Botões PT/EN
1. Criar 2 botões: **[PT]** **[EN]**
2. No botão PT:
   - Add Component → `LanguageSwitcher`
   - Target Language = **Portuguese**
3. No botão EN:
   - Add Component → `LanguageSwitcher`
   - Target Language = **English**

---

## 📋 Conteúdo dos JSONs

### **Os 7 Erros:**

| Erro | Título PT | Título EN |
|------|-----------|-----------|
| 1 | Respeito às Filas Preferenciais | Respect for Priority Queues |
| 2 | Acessibilidade em Rampas | Accessibility on Ramps |
| 3 | Segurança em Emergências | Emergency Safety |
| 4 | Vagas Preferenciais | Preferential Parking Spaces |
| 5 | Assentos Prioritários | Priority Seats |
| 6 | Cuidado com o Ambiente | Environmental Care |
| 7 | Inclusão e Piso Tátil | Inclusion and Tactile Paving |

### **Para Cada Erro:**
- ✅ Título (`error1Title` a `error7Title`)
- ✅ Mensagem educativa (`error1Message` a `error7Message`)
- ✅ Nome do erro (`error1Name` a `error7Name`)

### **Mensagens de Resultado:**
- ✅ `successMessage` - Quando encontra todos os erros
- ✅ `timeoutMessage` - Quando o tempo acaba
- ✅ `maxWrongAttemptsMessage` - Quando atinge máximo de tentativas

---

## 💻 Como Usar no Código

### **Substituir SevenErrorsConfig por LanguageManager**

#### Antes (sem tradução):
```csharp
string title = sevenErrorsConfig.GetErrorTitle(0);
string message = sevenErrorsConfig.GetErrorMessage(0);
string name = sevenErrorsConfig.GetErrorName(0);
string resultMsg = sevenErrorsConfig.GetResultMessage(completed, maxAttempts);
```

#### Depois (com tradução):
```csharp
string title = LanguageManager.Instance.GetErrorTitle(0);
string message = LanguageManager.Instance.GetErrorMessage(0);
string name = LanguageManager.Instance.GetErrorName(0);
string resultMsg = LanguageManager.Instance.GetResultMessage(completed, maxAttempts);
```

---

## 📝 Exemplos Práticos

### **Exemplo 1: Mostrar Popup de Erro**

```csharp
using UnityEngine;

public class ErrorHotspot : MonoBehaviour
{
    [SerializeField] private int errorIndex; // 0-6
    [SerializeField] private ErrorPopup popup;

    void OnClick()
    {
        // Obter textos traduzidos
        string title = LanguageManager.Instance.GetErrorTitle(errorIndex);
        string message = LanguageManager.Instance.GetErrorMessage(errorIndex);

        // Mostrar popup
        popup.ShowPopup(title, message);

        // Log (opcional)
        string errorName = LanguageManager.Instance.GetErrorName(errorIndex);
        Debug.Log($"Erro encontrado: {errorName}");
    }
}
```

### **Exemplo 2: Tela de Resultados**

```csharp
using UnityEngine;
using TMPro;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultMessageText;

    public void ShowResult(bool completedAll, bool maxWrongAttempts)
    {
        string message = LanguageManager.Instance.GetResultMessage(
            completedAll,
            maxWrongAttempts
        );

        resultMessageText.text = message;
    }
}
```

### **Exemplo 3: Com Fallback para SevenErrorsConfig**

Se quiser manter compatibilidade com o sistema antigo:

```csharp
public class ErrorManager : MonoBehaviour
{
    [SerializeField] private SevenErrorsConfig config;

    string GetErrorTitle(int index)
    {
        // Tenta usar LanguageManager primeiro
        if (LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetErrorTitle(index);
        }

        // Fallback para SevenErrorsConfig
        return config.GetErrorTitle(index);
    }

    string GetErrorMessage(int index)
    {
        if (LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetErrorMessage(index);
        }

        return config.GetErrorMessage(index);
    }
}
```

---

## 🎨 Trocar Idioma

### **Via Botões (Automático)**
Basta clicar nos botões PT ou EN configurados com `LanguageSwitcher`

### **Via Código**
```csharp
// Trocar para Português
LanguageManager.Instance.SetPortuguese();

// Trocar para Inglês
LanguageManager.Instance.SetEnglish();

// Alternar entre PT e EN
LanguageManager.Instance.ToggleLanguage();

// Verificar idioma atual
var lang = LanguageManager.Instance.GetCurrentLanguage();
if (lang == LanguageManager.Language.Portuguese)
{
    Debug.Log("Idioma: Português");
}
else
{
    Debug.Log("Language: English");
}
```

---

## 🔄 Atualizar Textos Quando Idioma Muda

### **Método 1: LocalizedText (Recomendado)**
Adicione o componente `LocalizedText` em qualquer TextMeshProUGUI que precise ser traduzido automaticamente.

**Categorias disponíveis:**
- `SevenErrorsError1Title` a `SevenErrorsError7Title`
- `SevenErrorsError1Message` a `SevenErrorsError7Message`
- `SevenErrorsError1Name` a `SevenErrorsError7Name`
- `SevenErrorsSuccessMessage`
- `SevenErrorsTimeoutMessage`
- `SevenErrorsMaxWrongAttemptsMessage`

### **Método 2: Event OnLanguageChanged**

```csharp
using UnityEngine;
using TMPro;

public class DynamicText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myText;
    [SerializeField] private int errorIndex;

    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= UpdateText;
    }

    void UpdateText()
    {
        myText.text = LanguageManager.Instance.GetErrorTitle(errorIndex);
    }
}
```

---

## 💾 Salvar Preferência de Idioma (Opcional)

```csharp
// Salvar escolha do usuário
public void SaveLanguagePreference()
{
    int lang = (int)LanguageManager.Instance.GetCurrentLanguage();
    PlayerPrefs.SetInt("Language", lang);
    PlayerPrefs.Save();
}

// Carregar ao iniciar o jogo
void Start()
{
    int savedLang = PlayerPrefs.GetInt("Language", 0); // 0 = PT padrão

    if (savedLang == 0)
        LanguageManager.Instance.SetPortuguese();
    else
        LanguageManager.Instance.SetEnglish();
}
```

---

## 📊 Estrutura do JSON

```json
{
    "seven_errors": {
        "error1Title": "Respeito às Filas Preferenciais",
        "error1Message": "Exato! Filas preferenciais...",
        "error1Name": "Fila presencial ignorada",

        "error2Title": "Acessibilidade em Rampas",
        "error2Message": "Bem observado! Para quem usa...",
        "error2Name": "Rampa de acesso bloqueada",

        ... (errors 3-7) ...

        "successMessage": "Parabéns! Você encontrou...",
        "timeoutMessage": "Tempo esgotado!...",
        "maxWrongAttemptsMessage": "Você não encontrou..."
    }
}
```

---

## ✅ Checklist de Implementação

### Setup Inicial
- [ ] LanguageManager criado na cena
- [ ] JSONs atribuídos no Inspector
- [ ] Botões PT/EN criados com LanguageSwitcher
- [ ] Testado: clicar nos botões troca idioma

### Integração no Código
- [ ] Substituir `sevenErrorsConfig.GetErrorTitle()` por `LanguageManager.Instance.GetErrorTitle()`
- [ ] Substituir `sevenErrorsConfig.GetErrorMessage()` por `LanguageManager.Instance.GetErrorMessage()`
- [ ] Substituir `sevenErrorsConfig.GetErrorName()` por `LanguageManager.Instance.GetErrorName()`
- [ ] Substituir `sevenErrorsConfig.GetResultMessage()` por `LanguageManager.Instance.GetResultMessage()`

### Testes
- [ ] Jogo completo em Português funciona
- [ ] Jogo completo em Inglês funciona
- [ ] Trocar idioma durante o jogo funciona
- [ ] Popups de erro aparecem no idioma correto
- [ ] Mensagens de resultado aparecem no idioma correto
- [ ] Sem erros no Console

---

## 🔍 API Reference

```csharp
// Trocar idioma
LanguageManager.Instance.SetPortuguese();
LanguageManager.Instance.SetEnglish();
LanguageManager.Instance.ToggleLanguage();

// Obter idioma atual
Language lang = LanguageManager.Instance.GetCurrentLanguage();

// Obter dados
LanguageData data = LanguageManager.Instance.GetCurrentData();

// Métodos helper
string title = LanguageManager.Instance.GetErrorTitle(0);      // índice 0-6
string message = LanguageManager.Instance.GetErrorMessage(0);   // índice 0-6
string name = LanguageManager.Instance.GetErrorName(0);         // índice 0-6

// Mensagem de resultado
string result = LanguageManager.Instance.GetResultMessage(
    completedAllErrors: true,
    maxWrongAttemptsReached: false
);

// Event de mudança
LanguageManager.OnLanguageChanged += MeuMetodo;
LanguageManager.OnLanguageChanged -= MeuMetodo;
```

---

## ❓ Troubleshooting

**Textos não aparecem:**
- Verificar se JSONs estão em `StreamingAssets`
- Verificar se estão atribuídos no LanguageManager
- Validar sintaxe JSON em jsonlint.com

**Idioma não muda:**
- Verificar se LanguageManager está na cena
- Verificar Console para erros
- Verificar se botões têm LanguageSwitcher configurado

**Alguns textos não atualizam:**
- Verificar se está usando os métodos do LanguageManager
- Verificar se evento OnLanguageChanged está inscrito

---

**Projeto:** FIRJAN - Jogo dos 7 Erros
**Idiomas:** 🇧🇷 Português | 🇺🇸 English
**Sistema:** Completo e otimizado! 🚀
