using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    public static void BuildIOS()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        buildPlayerOptions.scenes = GetScenes();
        buildPlayerOptions.locationPathName = "ios";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;

        PlayerSettings.applicationIdentifier = "com.zihinarenasi.zihinarenasi";
        PlayerSettings.companyName = "ZihinArenasi";
        PlayerSettings.productName = "Zihin Arenasi";
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.iOS.buildNumber = "6";

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else
        {
            Debug.Log("Build failed");
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }
}