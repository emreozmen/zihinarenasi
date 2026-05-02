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
        // ── PBX Project ──
        string projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTargetGuid = proj.GetUnityMainTargetGuid();
        string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

        // Game Center entitlement
        string entitlementsFileName = "GameCenter.entitlements";
        string fullEntitlementsPath = Path.Combine(path, entitlementsFileName);

        File.Copy(
            Path.Combine(Application.dataPath, "GameCenter.entitlements"),
            fullEntitlementsPath,
            true
        );

        proj.AddFile(entitlementsFileName, entitlementsFileName);
        // SetBuildProperty kullan - AddBuildProperty mevcut degere ekler (bug!)
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);

        // Firebase / SPM paketleri icin signing sorununu coz:
        // Tum target'larda CODE_SIGNING_REQUIRED = NO yap (main ve framework haric)
        foreach (string targetGuid in proj.GetAllTargetGuids())
        {
            if (targetGuid == mainTargetGuid || targetGuid == frameworkTargetGuid)
                continue;

            proj.SetBuildProperty(targetGuid, "CODE_SIGNING_REQUIRED", "NO");
            proj.SetBuildProperty(targetGuid, "CODE_SIGNING_ALLOWED", "NO");
        }

        proj.WriteToFile(projPath);
        Debug.Log("Game Center entitlement eklendi, Firebase SPM signing duzeltildi.");

        // ── Info.plist ──
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;

        // iOS 14+ App Tracking Transparency
        root.SetString("NSUserTrackingUsageDescription",
            "Zihin Arenasi, sana daha alakali reklamlar gosterebilmek icin reklam verilerini kullanmak istiyor.");

        plist.WriteToFile(plistPath);
        Debug.Log("Info.plist guncellendi.");
#endif
    }
}
#endif
