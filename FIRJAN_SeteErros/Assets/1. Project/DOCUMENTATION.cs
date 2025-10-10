/*
==============================================
JOGO DOS 7 ERROS - DOCUMENTAÇÃO COMPLETA
==============================================

Este documento explica como configurar e usar o sistema completo do Jogo dos 7 Erros.

==============================================
ESTRUTURA DO PROJETO
==============================================

📁 Assets/1. Project/Scripts/
├── 📁 CanvasScreen/
│   ├── GameplayScreen.cs           // Tela principal do jogo
│   └── BackgroundClickDetector.cs  // Detecta cliques no fundo
├── 📁 Gameplay/
│   └── ErrorHotspot.cs             // Hotspots clicáveis dos erros

├── 📁 Config/
│   └── SevenErrorsConfig.cs        // Configuração do jogo
└── 📁 Utilities/
    ├── GameCameraSetup.cs          // Configuração automática da câmera
    ├── HotspotSetupUtility.cs      // Geração automática de hotspots
    └── GameTestUtility.cs          // Utilitários para testes

==============================================
CONFIGURAÇÃO INICIAL
==============================================

1. SETUP DA CENA:
   - Crie uma cena com Canvas
   - Adicione GameplayScreen como componente principal
   - Configure a câmera como Orthographic

2. CONFIGURAÇÃO DA IMAGEM:
   - Importe a imagem dos 7 erros
   - Crie um GameObject com Image component
   - Adicione BackgroundClickDetector para detectar cliques errados

3. SETUP DOS HOTSPOTS:
   - Use HotspotSetupUtility para gerar hotspots automaticamente
   - Ou crie manualmente GameObjects com ErrorHotspot components
   - Posicione sobre as áreas de erro na imagem



==============================================
COMPONENTES PRINCIPAIS
==============================================

1. GameplayScreen.cs
   - Controla timer, pontuação e lógica principal
   - Gerencia zoom e controles de câmera

   - Três condições de fim: completar, tempo esgotado, 3 erros

2. ErrorHotspot.cs
   - Sistema baseado em Button UI
   - Feedback visual quando encontrado
   - Proteção contra múltiplos cliques
   - Desabilita durante zoom/pan



4. BackgroundClickDetector.cs
   - Detecta cliques fora dos hotspots
   - Sistema de 3 tentativas erradas
   - Feedback visual e sonoro

==============================================
SISTEMA DE CONTROLES
==============================================

1. TOUCH/MOUSE:
   - Clique simples: interação com hotspots
   - Pinça/Ctrl+Scroll: zoom in/out
   - Drag durante zoom: pan da câmera

2. PROTEÇÃO DE INPUT:
   - Botões desabilitados durante zoom/pan
   - Evita cliques acidentais
   - Reabilita automaticamente

3. ZOOM E PAN:
   - Zoom mínimo: 1x (configurável)
   - Zoom máximo: 3x (configurável)
   - Pan limitado aos bounds da imagem

==============================================
UTILIDADES PARA DESENVOLVIMENTO
==============================================

1. GameTestUtility.cs:
   - Teclas de teste: R (restart), C (completar), W (erro)
   - Interface visual de debug
   - Validação automática de componentes

2. HotspotSetupUtility.cs:
   - Geração automática de hotspots
   - Configuração de posições
   - Texto de debug numerado

3. GameCameraSetup.cs:
   - Configuração automática de câmera
   - Setup de Canvas e UI
   - Validação de componentes

==============================================
CONFIGURAÇÃO DE PREFABS
==============================================

1. HOTSPOT PREFAB:
   - GameObject com Image + Button + ErrorHotspot
   - Tamanho recomendado: 100x100 pixels
   - Cor semi-transparente para debug

2. POPUP PREFAB:
   - ErrorPopup para informações educativas
   - Animação de abertura/fechamento
   - Botão de fechar integrado

==============================================
FLUXO DO JOGO
==============================================

1. INÍCIO:
   - GameplayScreen inicializa componentes
   - Timer de 2 minutos inicia
   - Hotspots ficam ativos para clique

2. DURANTE O JOGO:
   - Player clica nos erros (hotspots)
   - Popup educativo aparece quando erro é encontrado
   - Cliques no fundo contam como tentativas erradas
   - Zoom e pan disponíveis

3. FIM DO JOGO:
   a) Todos os 7 erros encontrados - SUCESSO
   b) Timer esgota - TIMEOUT
   c) 3 tentativas erradas - GAME OVER

4. PÓS-JOGO:
   - Pontuação calculada

   - GameResultData salvo para próximas telas

==============================================
TROUBLESHOOTING
==============================================

1. PROBLEMAS COMUNS:

Q: Cliques duplos nos hotspots?
A: Sistema foi migrado para Button UI, problema resolvido.

Q: Zoom interfere com botões?
A: Sistema desabilita botões durante gestos, reabilita depois.



Q: Hotspots não respondem?
A: Verifique se têm Button component e estão como filhos do Canvas.

2. DEBUG:
   - Use GameTestUtility para testes rápidos
   - Logs detalhados no console
   - Validação automática de setup

3. PERFORMANCE:
   - Sistema otimizado para mobile
   - Coroutines para animações
   - Event system para comunicação

==============================================
EXTENSÕES FUTURAS
==============================================

1. NÍVEIS ADICIONAIS:
   - Adicionar mais imagens em SevenErrorsConfig
   - Sistema de dificuldade progressiva

2. MULTIPLAYER:
   - Competição entre jogadores
   - Ranking de pontuações

3. ANALYTICS:
   - Tracking de interações
   - Métricas de gameplay

==============================================
CHECKLIST DE IMPLEMENTAÇÃO
==============================================

✅ Sistema de hotspots baseado em Button
✅ Controles de zoom e pan
✅ Sistema de 3 tentativas erradas
✅ Timer de 2 minutos
✅ Utilitários de teste e debug
✅ Documentação completa
✅ Sistema de eventos para comunicação

==============================================
CONTATO E SUPORTE
==============================================

Para dúvidas ou problemas:
1. Consulte os logs no Console do Unity
2. Use GameTestUtility para debugging
3. Verifique a configuração com os utilitários
4. Consulte esta documentação

Versão: 1.0 - Jogo dos 7 Erros Completo
Data: 2024
*/