using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CodexCompilationCheck
{
    [Serializable]
    private sealed class Status
    {
        public string checkedAtUtc;
        public string unityVersion;
        public bool isCompiling;
        public bool isUpdating;
        public bool scriptCompilationFailed;
    }

    public static void Run()
    {
        var status = new Status
        {
            checkedAtUtc = DateTime.UtcNow.ToString("O"),
            unityVersion = Application.unityVersion,
            isCompiling = EditorApplication.isCompiling,
            isUpdating = EditorApplication.isUpdating,
            scriptCompilationFailed = EditorUtility.scriptCompilationFailed
        };
        Directory.CreateDirectory("Library/codex-unity-console");
        File.WriteAllText("Library/codex-unity-console/compilation-status.json",
            JsonUtility.ToJson(status, true));
    }
}
