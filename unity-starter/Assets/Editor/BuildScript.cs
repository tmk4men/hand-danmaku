using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-shot WebGL build for unityroom. Does everything that the README used to
/// ask you to click through by hand:
///   - generates Assets/Scenes/Main.unity with a single GameBootstrap object
///   - applies every Player / Publishing setting (MediaPipe template, Gzip,
///     Decompression Fallback OFF, Gamma, 960x720, code stripping)
///   - switches the platform to WebGL and builds into Builds/Web
///
/// Headless:
///   Unity.exe -quit -batchmode -projectPath &lt;this folder&gt; \
///             -buildTarget WebGL -executeMethod BuildScript.BuildWebGL -logFile -
///
/// In the editor: menu  Build &gt; WebGL (unityroom)
/// Override output dir with  -buildOutput &lt;path&gt;  on the command line.
/// </summary>
public static class BuildScript
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string DefaultOutputDir = "Builds/Web";

    [MenuItem("Build/WebGL (unityroom)")]
    public static void BuildWebGL()
    {
        ConfigurePlayerSettings();
        EnsureScene();

        string outDir = GetArg("-buildOutput") ?? DefaultOutputDir;
        Directory.CreateDirectory(outDir);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] WebGL build OK -> {summary.outputPath} " +
                      $"({summary.totalSize / 1024 / 1024.0:0.0} MB)");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[BuildScript] WebGL build FAILED: {summary.result} " +
                           $"({summary.totalErrors} errors)");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "tmk4men";
        PlayerSettings.productName = "HAND DANMAKU";
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        PlayerSettings.runInBackground = true;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 720;

        // Publishing / WebGL — the unityroom-friendly preset.
        PlayerSettings.WebGL.template = "PROJECT:MediaPipe";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);

        // Make sure the active target is WebGL before BuildPlayer runs. No-op if
        // launched with -buildTarget WebGL.
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
    }

    static void EnsureScene()
    {
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bootstrap = new GameObject("Bootstrap");
        bootstrap.AddComponent<GameBootstrap>();   // wires up camera/player/HUD/etc. at runtime

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }
}
