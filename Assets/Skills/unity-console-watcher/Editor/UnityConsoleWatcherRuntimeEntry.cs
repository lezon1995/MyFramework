#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UnityConsoleWatcherRuntimeEntry
{
    [Serializable]
    private sealed class ConsoleEventRecord
    {
        public string timestampUtc;
        public string logType;
        public string severity;
        public string message;
        public string stackTrace;
        public bool isError;
        public bool isException;
        public string projectPath;
    }

    [Serializable]
    private sealed class ConsoleSummaryRecord
    {
        public string sessionStartedUtc;
        public string updatedAtUtc;
        public string source;
        public string projectPath;
        public int totalCount;
        public int logCount;
        public int warningCount;
        public int errorCount;
        public int exceptionCount;
    }

    private static readonly object SyncRoot = new object();
    private static readonly string ProjectPath = Directory.GetCurrentDirectory();
    private static readonly string OutputDir = Path.Combine(ProjectPath, "Library", "codex-unity-console");
    private static readonly string EventsFilePath = Path.Combine(OutputDir, "events.jsonl");
    private static readonly string SummaryFilePath = Path.Combine(OutputDir, "summary.json");
    private static readonly string HeartbeatFilePath = Path.Combine(OutputDir, "heartbeat.txt");

    private static DateTime _sessionStartedUtc;
    private static DateTime _lastHeartbeatWriteUtc;
    private static int _logCount;
    private static int _warningCount;
    private static int _errorCount;
    private static int _exceptionCount;

    static UnityConsoleWatcherRuntimeEntry()
    {
        try
        {
            Directory.CreateDirectory(OutputDir);
            _sessionStartedUtc = DateTime.UtcNow;
            _lastHeartbeatWriteUtc = DateTime.MinValue;

            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            WriteHeartbeat();
            WriteSummary("runtime-ready");

            Debug.Log("[unity-console-watcher] Runtime stream active: " + EventsFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[unity-console-watcher] Failed to initialize runtime stream: " + ex);
        }
    }

    public static void Run()
    {
        Debug.Log("[unity-console-watcher] Runtime entry alive. Stream path: " + EventsFilePath);
        WriteHeartbeat();
        WriteSummary("run-called");
    }

    private static void OnEditorUpdate()
    {
        if ((DateTime.UtcNow - _lastHeartbeatWriteUtc).TotalSeconds < 1.0)
        {
            return;
        }

        WriteHeartbeat();
    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType logType)
    {
        try
        {
            var timestampUtc = DateTime.UtcNow;
            var normalizedCondition = NormalizeNewlines(condition);
            var normalizedStackTrace = NormalizeNewlines(stackTrace);
            var isException = IsException(logType, normalizedCondition);
            var isError = isException || IsError(logType, normalizedCondition);
            var severity = ResolveSeverity(logType, isError, isException);

            lock (SyncRoot)
            {
                switch (severity)
                {
                    case "EXCEPTION":
                        _exceptionCount += 1;
                        break;
                    case "ERROR":
                        _errorCount += 1;
                        break;
                    case "WARNING":
                        _warningCount += 1;
                        break;
                    default:
                        _logCount += 1;
                        break;
                }

                var record = new ConsoleEventRecord
                {
                    timestampUtc = ToUtcString(timestampUtc),
                    logType = logType.ToString(),
                    severity = severity,
                    message = normalizedCondition,
                    stackTrace = normalizedStackTrace,
                    isError = isError,
                    isException = isException,
                    projectPath = ProjectPath,
                };

                File.AppendAllText(EventsFilePath, JsonUtility.ToJson(record) + Environment.NewLine);
                WriteSummaryLocked("log-update", timestampUtc);
            }
        }
        catch (Exception)
        {
            // Avoid recursive logging from callback failures.
        }
    }

    private static void WriteHeartbeat()
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            lock (SyncRoot)
            {
                Directory.CreateDirectory(OutputDir);
                File.WriteAllText(HeartbeatFilePath, ToUtcString(nowUtc) + Environment.NewLine);
                _lastHeartbeatWriteUtc = nowUtc;
            }
        }
        catch (Exception)
        {
            // Swallow I/O failures to avoid noisy recursive log callbacks.
        }
    }

    private static void WriteSummary(string source)
    {
        lock (SyncRoot)
        {
            WriteSummaryLocked(source, DateTime.UtcNow);
        }
    }

    private static void WriteSummaryLocked(string source, DateTime updatedUtc)
    {
        Directory.CreateDirectory(OutputDir);
        var summary = new ConsoleSummaryRecord
        {
            sessionStartedUtc = ToUtcString(_sessionStartedUtc),
            updatedAtUtc = ToUtcString(updatedUtc),
            source = source,
            projectPath = ProjectPath,
            totalCount = _logCount + _warningCount + _errorCount + _exceptionCount,
            logCount = _logCount,
            warningCount = _warningCount,
            errorCount = _errorCount,
            exceptionCount = _exceptionCount,
        };

        File.WriteAllText(SummaryFilePath, JsonUtility.ToJson(summary, true));
    }

    private static string NormalizeNewlines(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static bool IsException(LogType logType, string message)
    {
        if (logType == LogType.Exception || logType == LogType.Assert)
        {
            return true;
        }

        return ContainsIgnoreCase(message, "exception");
    }

    private static bool IsError(LogType logType, string message)
    {
        if (logType == LogType.Error)
        {
            return true;
        }

        if (!ContainsIgnoreCase(message, "error"))
        {
            return false;
        }

        return !ContainsIgnoreCase(message, "0 errors");
    }

    private static string ResolveSeverity(LogType logType, bool isError, bool isException)
    {
        if (isException)
        {
            return "EXCEPTION";
        }

        if (isError)
        {
            return "ERROR";
        }

        if (logType == LogType.Warning)
        {
            return "WARNING";
        }

        return "LOG";
    }

    private static bool ContainsIgnoreCase(string value, string keyword)
    {
        return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string ToUtcString(DateTime value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}
#endif
