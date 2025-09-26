# Jogo dos 7 Erros - Scripts Unity

Este documento descreve todos os scripts criados para o "Jogo dos 7 Erros" e como utilizá-los.

## Visão Geral do Sistema

O jogo utiliza o sistema de telas baseado em `CanvasScreen.cs` existente, com as seguintes telas principais:

1. **IdleScreen** - Tela inicial com botão "Começar"
2. **GameplayScreen** - Tela principal do jogo
3. **ResultsScreen** - Tela de resultados e pontuação

## Scripts Criados

### Telas (CanvasScreen)

#### `IdleScreen.cs`
- **Função**: Tela inicial do jogo
- **Características**:
  - Herda de `CanvasScreen`
  - Contém botão para iniciar o jogo
  - Configura automaticamente o nome da tela como "IdleScreen"

#### `GameplayScreen.cs`
- **Função**: Tela principal com mecânica do jogo
- **Características**:
  - Timer regressivo de 2 minutos
  - Sistema de zoom e pan com touch/mouse
  - Gerenciamento de hotspots de erro
  - Contador de erros encontrados (0/7)
  - Sistema de popup para mensagens educativas

#### `ResultsScreen.cs`
- **Função**: Exibe resultados e pontuação final
- **Características**:
  - Mostra erros encontrados e tempo restante
  - Calcula pontuação das habilidades
  - Botões para jogar novamente ou voltar ao menu
  - Mensagens diferentes para sucesso/timeout

### Gameplay

#### `ErrorHotspot.cs`
- **Função**: Área clicável que representa um erro
- **Características**:
  - Implementa `IPointerDownHandler` para detecção de clique
  - Sistema de feedback visual quando encontrado
  - Pode ser desabilitado após ser encontrado
  - Suporte para indicador visual de "encontrado"

#### `ErrorPopup.cs`
- **Função**: Popup que exibe mensagem educativa
- **Características**:
  - Animação de fade in/out
  - Animação de escala (efeito bounce)
  - Botão para fechar
  - Auto-destruição após fechar

### Gerenciamento de Dados

#### `GameResultData.cs`
- **Função**: Armazena dados entre telas
- **Características**:
  - Classe estática para persistir dados
  - Calcula pontuação baseada em erros encontrados
  - Integração com `SevenErrorsConfig`

#### `SevenErrorsConfig.cs` (ScriptableObject)
- **Função**: Configuração centralizadas do jogo
- **Características**:
  - Armazena mensagens educativas dos 7 erros
  - Configurações de pontuação por faixa
  - Tempo de jogo configurável
  - Posições recomendadas dos hotspots

### Gerenciadores

#### `SevenErrorsGameManager.cs`
- **Função**: Gerenciador principal do jogo
- **Características**:
  - Padrão Singleton
  - Controle de estados do jogo
  - Gerenciamento de áudio
  - Pausa/resume automático

### Utilitários

#### `ErrorHotspotSetupHelper.cs` (Editor Only)
- **Função**: Ajuda na configuração dos hotspots no editor
- **Características**:
  - Criação automática de hotspots
  - Posicionamento baseado na configuração
  - Ferramentas de debug e validação

## Como Configurar

### 1. Configuração Inicial

1. **Criar ScriptableObject de Configuração**:
   ```
   Assets > Create > Seven Errors Game > Error Configuration
   ```
   - Nomeie como "SevenErrorsConfig"
   - Configure mensagens, tempo de jogo e pontuações

2. **Configurar Telas**:
   - Crie GameObjects com Canvas para cada tela
   - Adicione os scripts `IdleScreen`, `GameplayScreen` e `ResultsScreen`
   - Configure os nomes das telas nos `ScreenData`

### 2. Configurar GameplayScreen

1. **UI Elements**:
   - `timerText`: Text component para mostrar tempo
   - `errorCounterText`: Text component para contador (0/7)
   - `gameImage`: Image com a arte do jogo
   - `gameImageParent`: Transform pai para zoom/pan
   - `popupPrefab`: Prefab do popup com `ErrorPopup.cs`
   - `popupCanvas`: Canvas para instanciar popups

2. **Game Elements**:
   - `errorHotspots`: Array com todos os hotspots
   - `gameConfig`: Referência ao ScriptableObject criado

3. **Camera Control**:
   - Configure limites de zoom e velocidades

### 3. Criar Hotspots dos Erros

#### Método Manual:
1. Crie GameObjects filhos da imagem
2. Adicione `ErrorHotspot.cs` e `Collider2D`
3. Posicione sobre as áreas dos erros
4. Configure `errorIndex` (0-6) em cada hotspot

#### Método Automático:
1. Adicione `ErrorHotspotSetupHelper.cs` em um GameObject
2. Configure as referências (config, prefab, parent)
3. Use "Create All Hotspots" no context menu
4. Ajuste posições manualmente se necessário

### 4. Configurar Popup Prefab

1. Crie um prefab com:
   - `CanvasGroup` para fade
   - `RectTransform` para animação de escala
   - `Text` para mensagem
   - `Button` para fechar
2. Adicione `ErrorPopup.cs`
3. Configure as referências

### 5. Configurar Audio (Opcional)

1. Adicione `SevenErrorsGameManager.cs` em um GameObject
2. Configure `AudioSource` e clips de som
3. Marque como DontDestroyOnLoad se necessário

## Fluxo do Jogo

1. **IdleScreen**: Jogador clica "Começar"
2. **GameplayScreen**: 
   - Timer inicia (120 segundos)
   - Jogador procura e clica nos erros
   - Cada erro mostra popup educativo
   - Jogo termina por completar todos os erros OU timeout
3. **ResultsScreen**:
   - Mostra pontuação baseada em erros encontrados
   - Opções: "Jogar Novamente" ou "Voltar ao Menu"

## Pontuação

### Faixas de Pontuação:
- **5-7 erros**: Empatia: 8, Criatividade: 7, Resolução: 6
- **2-4 erros**: Empatia: 7, Criatividade: 6, Resolução: 5  
- **0-1 erros**: Empatia: 6, Criatividade: 5, Resolução: 4

## Mensagens dos Erros

1. **Fila presencial ignorada**: "Exato! Filas preferenciais garantem o acesso de quem mais precisa..."
2. **Rampa de acesso bloqueada**: "Bem observado! Para quem usa cadeira de rodas..."
3. **Saída de emergência obstruída**: "Perfeito! Em uma emergência, cada segundo conta..."
4. **Vaga preferencial ocupada**: "Isso mesmo! Vagas preferenciais não são sobre privilégio..."
5. **Assento prioritário indevido**: "Ótima percepção! Em espaços compartilhados..."
6. **Lixo jogado na rua**: "Você encontrou! Manter nosso ambiente limpo..."
7. **Piso tátil bloqueado**: "Exato! Para uma pessoa com deficiência visual..."

## Funcionalidades de Debug

### Context Menus:
- **GameplayScreen**: "Force Game End", "Add Error"
- **ResultsScreen**: "Simulate Success", "Simulate Failure" 
- **ErrorHotspot**: "Force Find Error", "Show/Hide Hotspot"
- **ErrorHotspotSetupHelper**: Várias ferramentas de configuração

### Logs:
- Ativado quando `debugMode = true` no GameManager
- Mostra mudanças de estado e eventos importantes

## Integração com Sistema Existente

Os scripts foram projetados para integrar perfeitamente com:
- Sistema `CanvasScreen` existente
- `ScreenManager` para navegação entre telas
- Arquitetura de eventos existente

Todos os scripts herdam adequadamente de `CanvasScreen` e utilizam o sistema de eventos `ScreenManager.CallScreen` para navegação.