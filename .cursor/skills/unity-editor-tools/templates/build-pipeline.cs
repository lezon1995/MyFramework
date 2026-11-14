using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MyGame.EditorTools
{
    /// <summary>
    /// 自动化构建脚本。支持命令行/CI 调用。
    /// </summary>
    public static class BuildPipelineTool
    {
        private const string OutputRoot = "Build";
        private const string SceneRoot = "Assets/GameResources/_Gameplay/Levels";

        /// <summary>
        /// 命令行入口示例：
        /// Unity -batchmode -quit -projectPath . -executeMethod MyGame.EditorTools.BuildPipelineTool.BuildAndroid -buildTarget Android -logFile build.log
        /// </summary>
        public static void BuildAndroid()
        {
            RunBuild(BuildTarget.Android, BuildTargetGroup.Android, ".apk");
        }

        public static void BuildWindows()
        {
            RunBuild(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, ".exe");
        }

        public static void BuildAndroidDevelopment()
        {
            RunBuild(BuildTarget.Android, BuildTargetGroup.Android, ".apk", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        private static void RunBuild(BuildTarget target, BuildTargetGroup group, string extension,
            BuildOptions options = BuildOptions.None)
        {
            string outputPath = Path.Combine(OutputRoot, target.ToString(),
                $"{Application.productName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var playerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                target = target,
                targetGroup = group,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(playerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] 成功: {outputPath}, 用时 {summary.totalTime.TotalSeconds:F1}s, " +
                          $"大小 {summary.totalSize / (1024f * 1024f):F1} MB");
                Exit(0);
            }
            else
            {
                Debug.LogError($"[Build] 失败: {summary.result}, 错误 {summary.totalErrors} 个, " +
                               $"警告 {summary.totalWarnings} 个");
                Exit(1);
            }
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { SceneRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    scenes.Add(path);
                }
            }

            return scenes.ToArray();
        }

        private static void Exit(int code)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
