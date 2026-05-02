#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if UNITY_EDITOR && UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class iOSPostBuild
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

#if UNITY_EDITOR && UNITY_IOS
        // ── PBX Project: Game Center entitlement ──
        string projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string targetGuid = proj.GetUnityMainTargetGuid();

        string entitlementsPath = "GameCenter.entitlements";
        string fullEntitlementsPath = Path.Combine(path, entitlementsPath);

        File.Copy(
            Path.Combine(Application.dataPath, "GameCenter.entitlements"),
            fullEntitlementsPath,
            true
        );

        proj.AddFile(entitlementsPath, entitlementsPath);
        proj.AddBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementsPath);
        proj.WriteToFile(projPath);

        Debug.Log("Game Center entitlement eklendi.");

        // ── Info.plist: ATT + AdMob ──
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;

        // iOS 14+ App Tracking Transparency - AdMob için zorunlu
        root.SetString("NSUserTrackingUsageDescription",
            "Zihin Arenası, sana daha alakalı reklamlar gösterebilmek için reklam verilerini kullanmak istiyor.");

        plist.WriteToFile(plistPath);

        Debug.Log("Info.plist güncellendi: NSUserTrackingUsageDescription eklendi.");
#endif
    }
}
#endif
