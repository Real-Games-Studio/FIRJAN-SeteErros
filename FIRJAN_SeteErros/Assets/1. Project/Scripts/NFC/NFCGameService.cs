using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using _4._NFC_Firjan.Scripts.NFC;
using _4._NFC_Firjan.Scripts.Server;
using UnityEngine;

/// <summary>
/// Serviço central para integração do Jogo dos 7 Erros com o sistema NFC/REST da FIRJAN.
/// Responsável por carregar a configuração do servidor, escutar eventos do leitor NFC,
/// consultar dados existentes e enviar novas pontuações ao backend.
/// </summary>
public class NFCGameService : MonoBehaviour
{
    private const int GameId = 3; // Identificador do jogo "Jogo dos 7 erros"

    [Header("Scene References")]
    [SerializeField] private NFCReceiver nfcReceiver;
    [SerializeField] private ServerComunication serverComunication;

    [Header("Configuration")]
    [SerializeField] private string serverConfigFileName = "serverconfig.json";
    [SerializeField] private bool autoLoadConfigOnAwake = true;
    [SerializeField] private bool autoFetchDataOnConnect = true;

    /// <summary>
    /// Dados de configuração lidos do JSON.
    /// </summary>
    [Serializable]
    private class ServerConfigData
    {
        public string ip = "127.0.0.1";
        public int port = 8080;
    }

    private bool registrationAttemptedForCurrentCard;

    public static NFCGameService Instance { get; private set; }

    public string CurrentNfcId { get; private set; }
    public string CurrentReaderName { get; private set; }
    public bool IsReaderConnected { get; private set; }
    public bool HasCard => !string.IsNullOrEmpty(CurrentNfcId);
    public EndGameResponseModel LastResponse { get; private set; }
    public ScoreData LastScoreSent { get; private set; }

    // Dados pendentes para envio quando cartão for conectado
    private ScoreData? pendingScore;
    private bool pendingCompletedAllErrors;
    private int pendingErrorsFound;
    private int pendingWrongAttempts;
    private float pendingTimeRemaining;
    private bool hasPendingData;

    // Armazena o último ID de cartão lido para uso no envio
    private string lastReadNfcId;

    public event Action<string> OnNfcCardConnected;
    public event Action OnNfcCardDisconnected;
    public event Action<EndGameResponseModel> OnNfcDataUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[NFC] Mais de um NFCGameService encontrado. Mantendo o primeiro e destruindo o novo.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (nfcReceiver == null)
        {
            nfcReceiver = FindFirstObjectByType<NFCReceiver>();
            if (nfcReceiver == null)
            {
                Debug.LogWarning("[NFC] NFCReceiver não encontrado na cena. Conecte o componente via inspetor.");
            }
        }

        if (serverComunication == null)
        {
            serverComunication = FindFirstObjectByType<ServerComunication>();
            if (serverComunication == null)
            {
                Debug.LogWarning("[NFC] ServerComunication não encontrado na cena. Conecte o componente via inspetor.");
            }
        }

        if (autoLoadConfigOnAwake)
        {
            LoadServerConfiguration();
        }
    }

    private void OnEnable()
    {
        if (nfcReceiver != null)
        {
            nfcReceiver.OnNFCConnected.AddListener(HandleNfcConnected);
            nfcReceiver.OnNFCDisconnected.AddListener(HandleNfcDisconnected);
            nfcReceiver.OnNFCReaderConnected.AddListener(HandleReaderConnected);
            nfcReceiver.OnNFCReaderDisconected.AddListener(HandleReaderDisconnected);
        }
    }

    private void OnDisable()
    {
        if (nfcReceiver != null)
        {
            nfcReceiver.OnNFCConnected.RemoveListener(HandleNfcConnected);
            nfcReceiver.OnNFCDisconnected.RemoveListener(HandleNfcDisconnected);
            nfcReceiver.OnNFCReaderConnected.RemoveListener(HandleReaderConnected);
            nfcReceiver.OnNFCReaderDisconected.RemoveListener(HandleReaderDisconnected);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Envia a pontuação do jogo para o servidor REST FIRJAN usando o cartão NFC conectado.
    /// Se não houver cartão conectado, armazena os dados para envio automático quando conectar.
    /// </summary>
    /// <param name="score">Pontuação calculada localmente.</param>
    /// <param name="completedAllErrors">Indica se o jogador completou todos os erros.</param>
    /// <param name="errorsFound">Quantidade de erros encontrados.</param>
    /// <param name="wrongAttempts">Quantidade de tentativas erradas.</param>
    /// <param name="timeRemaining">Tempo restante (em segundos) quando o jogo terminou.</param>
    public async void SubmitGameResult(ScoreData score, bool completedAllErrors, int errorsFound, int wrongAttempts, float timeRemaining)
    {
        if (!HasCard)
        {
            Debug.Log("[NFC] Nenhum cartão conectado no momento. Armazenando dados para envio automático quando cartão for conectado.");

            // Armazena os dados para envio posterior
            pendingScore = score;
            pendingCompletedAllErrors = completedAllErrors;
            pendingErrorsFound = errorsFound;
            pendingWrongAttempts = wrongAttempts;
            pendingTimeRemaining = timeRemaining;
            hasPendingData = true;

            Debug.Log($"[NFC] Dados armazenados: Empatia: {score.empatia} | Criatividade: {score.criatividade} | Resolução: {score.resolucaoProblemas} | Erros: {errorsFound} | Tentativas erradas: {wrongAttempts}");
            return;
        }

        if (serverComunication == null)
        {
            Debug.LogError("[NFC] ServerComunication não está configurado. Resultado não enviado.");
            return;
        }

        await SendGameDataToServer(score, completedAllErrors, errorsFound, wrongAttempts, timeRemaining);
    }

    /// <summary>
    /// Tenta enviar dados pendentes usando o último cartão lido
    /// </summary>
    public async void TrySendPendingData()
    {
        if (hasPendingData && pendingScore.HasValue && !string.IsNullOrEmpty(lastReadNfcId))
        {
            Debug.Log($"[NFC] Tentando enviar dados pendentes com último cartão lido: {lastReadNfcId}");
            await SendGameDataToServer(pendingScore.Value, pendingCompletedAllErrors, pendingErrorsFound, pendingWrongAttempts, pendingTimeRemaining);

            // Limpa os dados pendentes
            hasPendingData = false;
            pendingScore = null;
        }
        else
        {
            Debug.LogWarning("[NFC] Não há dados pendentes ou nenhum cartão foi lido ainda.");
        }
    }

    /// <summary>
    /// Envia efetivamente os dados para o servidor
    /// </summary>
    private async Task SendGameDataToServer(ScoreData score, bool completedAllErrors, int errorsFound, int wrongAttempts, float timeRemaining)
    {
        // Usa o último ID lido, mesmo se o cartão foi desconectado
        string nfcIdToUse = !string.IsNullOrEmpty(CurrentNfcId) ? CurrentNfcId : lastReadNfcId;

        if (string.IsNullOrEmpty(nfcIdToUse))
        {
            Debug.LogError("[NFC] Nenhum ID de cartão disponível para envio!");
            return;
        }

        await EnsureCardRegistrationAsync();

        SevenErrorsGameModel payload = new SevenErrorsGameModel
        {
            nfcId = nfcIdToUse,
            gameId = GameId,
            skill1 = score.empatia,
            skill2 = score.criatividade,
            skill3 = score.resolucaoProblemas
        };

        LastScoreSent = score;

        try
        {
            Debug.Log($"[NFC] Preparando envio das habilidades do jogo. Cartão: {nfcIdToUse} | GameId: {GameId} | Empatia: {score.empatia} | Criatividade: {score.criatividade} | Resolução: {score.resolucaoProblemas} | Concluído: {completedAllErrors}");

            // Valida se os dados não são nulos ou inválidos
            if (string.IsNullOrEmpty(nfcIdToUse))
            {
                Debug.LogError("[NFC] ID do cartão está nulo ou vazio!");
                return;
            }

            HttpStatusCode status = await SendSevenErrorsData(payload);
            Debug.Log($"[NFC] Resposta do servidor ao enviar resultado: {(int)status} - {status}");

            if (status == HttpStatusCode.OK || status == HttpStatusCode.Created || status == HttpStatusCode.Accepted || status == HttpStatusCode.NoContent)
            {
                long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Debug.Log($"[NFC] Dados enviados com sucesso para o cartão {CurrentNfcId}. UnixTime: {unixTime} | Erros encontrados: {errorsFound} | Tentativas erradas: {wrongAttempts} | Tempo restante: {timeRemaining:F1}s");
                await RefreshNfcDataAsync();
            }
            else if (status == HttpStatusCode.Forbidden)
            {
                Debug.LogWarning($"[NFC] O cartão {CurrentNfcId} não está autorizado (403). Verifique se ele foi cadastrado previamente no servidor.");
            }
            else
            {
                Debug.LogWarning($"[NFC] Envio do resultado retornou código inesperado: {(int)status} - {status}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NFC] Erro ao enviar resultado do jogo para o cartão {CurrentNfcId}: {ex.Message}\n{ex}");
        }
    }

    /// <summary>
    /// Solicita os dados mais recentes do cartão NFC conectado.
    /// </summary>
    public async Task<EndGameResponseModel> RefreshNfcDataAsync()
    {
        if (!HasCard)
        {
            Debug.LogWarning("[NFC] Nenhum cartão conectado para atualizar os dados.");
            return null;
        }

        if (serverComunication == null)
        {
            Debug.LogError("[NFC] ServerComunication não está configurado. Conexão falhou.");
            return null;
        }

        try
        {
            EndGameResponseModel response = await serverComunication.GetNfcInfo(CurrentNfcId);

            if (response != null)
            {
                LastResponse = response;
                OnNfcDataUpdated?.Invoke(response);
                Debug.Log($"[NFC] Dados do cartão atualizados para {CurrentNfcId}: {response.ToString()}");
            }
            else
            {
                Debug.LogWarning($"[NFC] Servidor não retornou dados para o cartão {CurrentNfcId}.");
            }

            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NFC] Falha ao consultar dados do cartão {CurrentNfcId}: {ex.Message}\n{ex}");
            return null;
        }
    }

    private void LoadServerConfiguration()
    {
        if (serverComunication == null)
        {
            Debug.LogWarning("[NFC] ServerComunication não definido. Não é possível aplicar configuração do servidor.");
            return;
        }

        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, serverConfigFileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[NFC] Arquivo de configuração do servidor não encontrado em {path}. Valores do inspetor serão usados.");
                return;
            }

            string jsonContent = File.ReadAllText(path);
            ServerConfigData config = JsonUtility.FromJson<ServerConfigData>(jsonContent);

            if (config == null)
            {
                Debug.LogError("[NFC] Falha ao desserializar serverconfig.json. Verifique o formato do arquivo.");
                return;
            }

            serverComunication.Ip = config.ip;
            serverComunication.Port = config.port;

            Debug.Log($"[NFC] Configuração do servidor carregada: {config.ip}:{config.port}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NFC] Erro ao carregar configuração do servidor: {ex.Message}\n{ex}");
        }
    }

    private async void HandleNfcConnected(string cardId, string readerName)
    {
        CurrentNfcId = cardId;
        CurrentReaderName = readerName;
        lastReadNfcId = cardId; // Armazena o último ID lido
        registrationAttemptedForCurrentCard = false;

        Debug.Log($"[NFC] Cartão lido e conectado: {cardId} | Leitor: {readerName}");
        OnNfcCardConnected?.Invoke(cardId);

        await EnsureCardRegistrationAsync();

        if (autoFetchDataOnConnect)
        {
            await RefreshNfcDataAsync();
        }

        // Se há dados pendentes para envio, envia automaticamente
        if (hasPendingData && pendingScore.HasValue)
        {
            Debug.Log("[NFC] Cartão conectado e há dados pendentes. Enviando automaticamente...");
            await SendGameDataToServer(pendingScore.Value, pendingCompletedAllErrors, pendingErrorsFound, pendingWrongAttempts, pendingTimeRemaining);

            // Limpa os dados pendentes
            hasPendingData = false;
            pendingScore = null;
        }
    }

    private void HandleNfcDisconnected()
    {
        Debug.Log($"[NFC] Cartão desconectado: {CurrentNfcId}");
        CurrentNfcId = null;
        LastResponse = null;
        registrationAttemptedForCurrentCard = false;
        // NÃO limpa o lastReadNfcId para permitir envio posterior
        OnNfcCardDisconnected?.Invoke();
    }

    private void HandleReaderConnected(string readerName)
    {
        IsReaderConnected = true;
        CurrentReaderName = readerName;
        Debug.Log($"[NFC] Leitor conectado: {readerName}");
    }

    private void HandleReaderDisconnected()
    {
        IsReaderConnected = false;
        Debug.Log("[NFC] Leitor desconectado.");
        CurrentReaderName = null;
    }

    private async Task EnsureCardRegistrationAsync()
    {
        if (registrationAttemptedForCurrentCard)
        {
            return;
        }

        registrationAttemptedForCurrentCard = true;

        // Usa o último ID lido, mesmo se o cartão foi desconectado
        string nfcIdToUse = !string.IsNullOrEmpty(CurrentNfcId) ? CurrentNfcId : lastReadNfcId;

        if (string.IsNullOrEmpty(nfcIdToUse))
        {
            return;
        }

        if (serverComunication == null)
        {
            Debug.LogWarning("[NFC] Impossível registrar cartão - ServerComunication não configurado.");
            return;
        }

        try
        {
            string url = $"http://{serverComunication.Ip}:{serverComunication.Port}/users/{nfcIdToUse}";
            using (HttpClient client = new HttpClient())
            {
                StringContent payload = new StringContent("{}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, payload);
                Debug.Log($"[NFC] Registro do cartão {nfcIdToUse} retornou {(int)response.StatusCode} - {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NFC] Falha ao registrar cartão {nfcIdToUse}: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia dados do Jogo dos 7 Erros com nomes das habilidades
    /// </summary>
    private async Task<HttpStatusCode> SendSevenErrorsData(SevenErrorsGameModel gameData)
    {
        if (serverComunication == null)
        {
            Debug.LogError("[NFC] ServerComunication não configurado para envio.");
            return HttpStatusCode.ServiceUnavailable;
        }

        try
        {
            string url = $"http://{serverComunication.Ip}:{serverComunication.Port}/users/{gameData.nfcId}";

            using (HttpClient client = new HttpClient())
            {
                string jsonPayload = gameData.ToString();
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Adiciona headers específicos exatamente como no Postman
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "insomnia/11.6.0");

                Debug.Log($"[NFC] Enviando POST para: {url}");
                Debug.Log($"[NFC] Headers: Content-Type=application/json, User-Agent=insomnia/11.6.0");
                Debug.Log($"[NFC] JSON Payload: {jsonPayload}");

                HttpResponseMessage response = await client.PostAsync(url, content);

                Debug.Log($"[NFC] Resposta do servidor: {(int)response.StatusCode} - {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"[NFC] Erro na resposta: {responseBody}");
                }

                return response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NFC] Erro ao enviar dados das habilidades: {ex.Message}");
            return HttpStatusCode.InternalServerError;
        }
    }
}
