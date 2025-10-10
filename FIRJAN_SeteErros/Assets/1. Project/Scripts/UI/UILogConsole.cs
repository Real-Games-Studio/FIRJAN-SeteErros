using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Exibe logs da aplicação em um componente TextMeshProUGUI.
/// Útil para depuração em builds standalone onde o console não está visível.
/// </summary>
[DefaultExecutionOrder(1000)]
public class UILogConsole : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text logOutput;
    [SerializeField] private int maxLines = 15;
    [SerializeField] private bool includeStackTraceOnError = false;

    private readonly Queue<string> pendingLogs = new Queue<string>();
    private readonly List<string> logLines = new List<string>();
    private readonly object syncRoot = new object();
    private StringBuilder builder;

    private void Awake()
    {
        if (logOutput == null)
        {
            logOutput = GetComponent<TMP_Text>();
        }

        builder = new StringBuilder();
    }

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += HandleLogMessage;
    }

    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLogMessage;
    }

    private void Update()
    {
        FlushPendingLogs();
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        string prefix = type switch
        {
            LogType.Warning => "[WARN] ",
            LogType.Error => "[ERROR] ",
            LogType.Assert => "[ASSERT] ",
            LogType.Exception => "[EXCEPTION] ",
            _ => string.Empty
        };

        string message = prefix + condition;

        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && includeStackTraceOnError)
        {
            message += $"\n{stackTrace}";
        }

        lock (syncRoot)
        {
            pendingLogs.Enqueue(message);
        }
    }

    private void FlushPendingLogs()
    {
        bool hasNewEntries = false;

        lock (syncRoot)
        {
            while (pendingLogs.Count > 0)
            {
                string entry = pendingLogs.Dequeue();
                logLines.Add(entry);

                while (logLines.Count > maxLines)
                {
                    logLines.RemoveAt(0);
                }

                hasNewEntries = true;
            }
        }

        if (!hasNewEntries || logOutput == null)
        {
            return;
        }

        builder.Clear();
        for (int i = 0; i < logLines.Count; i++)
        {
            builder.AppendLine(logLines[i]);
        }

        logOutput.text = builder.ToString();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxLines < 1)
        {
            maxLines = 1;
        }
    }
#endif
}
