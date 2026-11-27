#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CodexEditorBridgeRunner
{
    [Serializable]
    private class BridgeRequest
    {
        public string id;
        public string action;
        public string method;
        public string menuItem;
        public string payload;
        public bool saveAssets = true;
        public bool refreshAssets = true;
        public string createdAtUtc;
    }

    [Serializable]
    private class BridgeResponse
    {
        public string id;
        public bool ok;
        public string action;
        public string target;
        public string message;
        public string error;
        public string stackTrace;
        public string processedAtUtc;
    }

    private const string DefaultAction = "executeMethod";
    private const string ActionMethod = "executeMethod";
    private const string ActionMenuItem = "executeMenuItem";
    private const string RequestPattern = "request-*.json";
    private const string ResponsePattern = "response-*.json";
    private const string L10nResultPattern = "unity-guru-l10n-*-result.json";
    private const string RequestPrefix = "request-";
    private const string ResponsePrefix = "response-";
    private const double PollIntervalSeconds = 0.4;
    private const double CleanupIntervalSeconds = 300.0;
    private const double EphemeralFileTtlSeconds = 2 * 24 * 3600;
    private const double ResultFileTtlSeconds = 7 * 24 * 3600;
    private const double LegacyFileTtlSeconds = 0.0;

    private static bool _isProcessing;
    private static double _nextPollTime;
    private static double _nextCleanupTime;

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static string BridgeDir => Path.Combine(ProjectRoot, "Library", "codex-unity-bridge");
    private static string LegacyBridgeDir => Path.Combine(ProjectRoot, "ProjectSettings", "codex-unity-bridge");

    static CodexEditorBridgeRunner()
    {
        Directory.CreateDirectory(BridgeDir);
        CleanupLegacyBridgeArtifacts();
        CleanupBridgeArtifacts();
        _nextCleanupTime = EditorApplication.timeSinceStartup + CleanupIntervalSeconds;
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
        Debug.Log($"[CodexEditorBridge] Runner active. BridgeDir={BridgeDir}");
    }

    private static void OnEditorUpdate()
    {
        if (_isProcessing) return;
        var now = EditorApplication.timeSinceStartup;
        if (now >= _nextCleanupTime)
        {
            CleanupBridgeArtifacts();
            _nextCleanupTime = now + CleanupIntervalSeconds;
        }

        if (now < _nextPollTime) return;

        _nextPollTime = now + PollIntervalSeconds;
        ProcessNextRequest();
    }

    private static void ProcessNextRequest()
    {
        if (!Directory.Exists(BridgeDir)) return;

        var requestFiles = Directory.GetFiles(BridgeDir, RequestPattern)
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .ToArray();
        if (requestFiles.Length == 0) return;

        _isProcessing = true;
        try
        {
            ProcessRequestFile(requestFiles[0]);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private static void ProcessRequestFile(string requestPath)
    {
        BridgeResponse response;
        BridgeRequest request = null;
        try
        {
            request = LoadRequest(requestPath);
            response = ExecuteRequest(request);
        }
        catch (Exception ex)
        {
            var reqId = request != null && !string.IsNullOrEmpty(request.id)
                ? request.id
                : ExtractRequestIdFromFile(requestPath);
            response = new BridgeResponse
            {
                id = reqId,
                ok = false,
                action = request != null ? request.action : DefaultAction,
                target = request != null ? GetRequestTarget(request) : "",
                error = ex.Message,
                stackTrace = ex.ToString(),
                message = "Bridge execution failed.",
                processedAtUtc = DateTime.UtcNow.ToString("o"),
            };
        }

        WriteResponse(response);

        try
        {
            File.Delete(requestPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CodexEditorBridge] Failed to delete request file: {requestPath}\n{e}");
        }
    }

    private static BridgeRequest LoadRequest(string requestPath)
    {
        var json = File.ReadAllText(requestPath);
        var request = JsonUtility.FromJson<BridgeRequest>(json) ?? new BridgeRequest();

        if (string.IsNullOrEmpty(request.id))
        {
            request.id = ExtractRequestIdFromFile(requestPath);
        }

        if (string.IsNullOrEmpty(request.action))
        {
            request.action = DefaultAction;
        }

        return request;
    }

    private static BridgeResponse ExecuteRequest(BridgeRequest request)
    {
        var response = new BridgeResponse
        {
            id = request.id,
            ok = false,
            action = request.action,
            target = GetRequestTarget(request),
            processedAtUtc = DateTime.UtcNow.ToString("o"),
        };

        Debug.Log($"[CodexEditorBridge] Request[{request.id}] action={request.action} target={response.target}");

        try
        {
            if (request.action == ActionMenuItem)
            {
                ExecuteMenuItem(request.menuItem);
                response.message = $"Executed menu item: {request.menuItem}";
            }
            else
            {
                ExecuteMethod(request.method, request.payload);
                response.message = $"Executed method: {request.method}";
            }

            if (request.saveAssets) AssetDatabase.SaveAssets();
            if (request.refreshAssets) AssetDatabase.Refresh();

            response.ok = true;
        }
        catch (Exception ex)
        {
            response.ok = false;
            response.error = ex.Message;
            response.stackTrace = ex.ToString();
            response.message = "Bridge request failed.";
        }

        Debug.Log($"[CodexEditorBridge] Response[{response.id}] ok={response.ok} message={response.message}");

        return response;
    }

    private static void ExecuteMenuItem(string menuItem)
    {
        if (string.IsNullOrEmpty(menuItem))
        {
            throw new InvalidOperationException("menuItem is empty.");
        }

        var ok = EditorApplication.ExecuteMenuItem(menuItem);
        if (!ok)
        {
            throw new InvalidOperationException($"Unity failed to execute menu item: {menuItem}");
        }
    }

    private static void ExecuteMethod(string methodPath, string payload)
    {
        if (string.IsNullOrEmpty(methodPath))
        {
            throw new InvalidOperationException("method is empty.");
        }

        int idx = methodPath.LastIndexOf('.');
        if (idx <= 0 || idx >= methodPath.Length - 1)
        {
            throw new InvalidOperationException($"Invalid method path: {methodPath}");
        }

        string typeName = methodPath.Substring(0, idx);
        string methodName = methodPath.Substring(idx + 1);

        var type = ResolveType(typeName);
        if (type == null)
        {
            throw new InvalidOperationException($"Type not found: {typeName}");
        }

        var methods = type
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == methodName)
            .ToArray();
        if (methods.Length == 0)
        {
            throw new InvalidOperationException($"Method not found: {methodPath}");
        }

        MethodInfo zeroArgMethod = null;
        MethodInfo oneStringArgMethod = null;
        foreach (var m in methods)
        {
            var ps = m.GetParameters();
            if (ps.Length == 0)
            {
                zeroArgMethod = m;
                break;
            }

            if (ps.Length == 1 && ps[0].ParameterType == typeof(string))
            {
                oneStringArgMethod = m;
            }
        }

        if (zeroArgMethod != null)
        {
            zeroArgMethod.Invoke(null, Array.Empty<object>());
            return;
        }

        if (oneStringArgMethod != null)
        {
            oneStringArgMethod.Invoke(null, new object[] { payload ?? string.Empty });
            return;
        }

        throw new InvalidOperationException(
            $"Unsupported signature for {methodPath}. Only static () or static (string) is supported.");
    }

    private static Type ResolveType(string typeName)
    {
        var direct = Type.GetType(typeName);
        if (direct != null) return direct;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t != null) return t;
        }

        if (!typeName.Contains("."))
        {
            Type fallback = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (t.Name != typeName) continue;
                    if (fallback != null) return null; // ambiguous short type name
                    fallback = t;
                }
            }

            return fallback;
        }

        return null;
    }

    private static string ExtractRequestIdFromFile(string requestPath)
    {
        var file = Path.GetFileNameWithoutExtension(requestPath) ?? "";
        if (file.StartsWith(RequestPrefix, StringComparison.Ordinal))
        {
            return file.Substring(RequestPrefix.Length);
        }

        return Guid.NewGuid().ToString("N");
    }

    private static string GetRequestTarget(BridgeRequest request)
    {
        if (request == null) return "";
        if (request.action == ActionMenuItem) return request.menuItem ?? "";
        return request.method ?? "";
    }

    private static void WriteResponse(BridgeResponse response)
    {
        if (response == null) return;
        if (string.IsNullOrEmpty(response.id))
        {
            response.id = Guid.NewGuid().ToString("N");
        }

        Directory.CreateDirectory(BridgeDir);
        string path = Path.Combine(BridgeDir, $"{ResponsePrefix}{response.id}.json");
        File.WriteAllText(path, JsonUtility.ToJson(response, true));
    }

    private static void CleanupBridgeArtifacts()
    {
        if (!Directory.Exists(BridgeDir)) return;

        CleanupPattern(BridgeDir, RequestPattern, EphemeralFileTtlSeconds);
        CleanupPattern(BridgeDir, ResponsePattern, EphemeralFileTtlSeconds);
        CleanupPattern(BridgeDir, L10nResultPattern, ResultFileTtlSeconds);
    }

    private static void CleanupLegacyBridgeArtifacts()
    {
        if (!Directory.Exists(LegacyBridgeDir)) return;

        CleanupPattern(LegacyBridgeDir, RequestPattern, LegacyFileTtlSeconds);
        CleanupPattern(LegacyBridgeDir, ResponsePattern, LegacyFileTtlSeconds);
        CleanupPattern(LegacyBridgeDir, L10nResultPattern, LegacyFileTtlSeconds);
    }

    private static void CleanupPattern(string dir, string pattern, double ttlSeconds)
    {
        var now = DateTime.UtcNow;
        foreach (var file in Directory.GetFiles(dir, pattern))
        {
            try
            {
                var age = now - File.GetLastWriteTimeUtc(file);
                if (age.TotalSeconds > ttlSeconds)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CodexEditorBridge] Cleanup skip for file={file}. {e.Message}");
            }
        }
    }
}
#endif
