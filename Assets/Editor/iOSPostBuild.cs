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
    // Apple Developer Team ID (Apple Distribution sertifikasindaki parantez icindeki kod)
    private const string DevelopmentTeamId = "H66NL7YDHZ";

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

#if UNITY_EDITOR && UNITY_IOS
        // ── PBX Project ──
        string projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTargetGuid      = proj.GetUnityMainTargetGuid();
        string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

        // ── Game Center entitlement ──
        string entitlementsFileName = "GameCenter.entitlements";
        string fullEntitlementsPath = Path.Combine(path, entitlementsFileName);
        File.Copy(
            Path.Combine(Application.dataPath, "GameCenter.entitlements"),
            fullEntitlementsPath,
            true
        );
        proj.AddFile(entitlementsFileName, entitlementsFileName);
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);

        // ── Firebase / GoogleUtilities SPM signing fix ──
        // Cloud Build, pbxproj'daki DEVELOPMENT_TEAM satirlarini sed ile siliyor.
        // xcconfig dosyasina yazarsak sed dokunamaz; Xcode oradan okur.
        // SPM paketleri (GoogleUtilities vb.) bu sayede DEVELOPMENT_TEAM'i bulur.
        string xcconfigContent = "// Cloud Build Firebase SPM signing fix\n"
                               + "DEVELOPMENT_TEAM = " + DevelopmentTeamId + "\n";
        string xcconfigFileName = "CloudBuildSPMFix.xcconfig";
        string xcconfigFilePath = Path.Combine(path, xcconfigFileName);
        File.WriteAllText(xcconfigFilePath, xcconfigContent);

        string xcconfigGuid = proj.AddFile(xcconfigFileName, xcconfigFileName, PBXSourceTree.Source);

        // Release config'e base xcconfig olarak ata
        foreach (string configName in new[] { "Release", "Debug", "ReleaseForRunning", "ReleaseForProfiling" })
        {
            string configGuid = proj.BuildConfigByName(mainTargetGuid, configName);
            if (!string.IsNullOrEmpty(configGuid))
                proj.SetBaseReferenceForBuildConfig(configGuid, xcconfigGuid);

            string fwConfigGuid = proj.BuildConfigByName(frameworkTargetGuid, configName);
            if (!string.IsNullOrEmpty(fwConfigGuid))
                proj.SetBaseReferenceForBuildConfig(fwConfigGuid, xcconfigGuid);
        }

        // Framework target'ta signing gerektirme (Firebase bagimliliklari icin)
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_REQUIRED", "NO");
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_ALLOWED",  "NO");

        proj.WriteToFile(projPath);
        Debug.Log("[iOSPostBuild] Game Center entitlement + Firebase SPM fix uygulandı.");

        // ── Info.plist ──
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict root = plist.root;

        // iOS 14+ App Tracking Transparency (AdMob zorunlu)
        root.SetString("NSUserTrackingUsageDescription",
            "Zihin Arenasi, sana daha alakali reklamlar gosterebilmek icin reklam verilerini kullanmak istiyor.");

        plist.WriteToFile(plistPath);
        Debug.Log("[iOSPostBuild] Info.plist güncellendi.");
#endif
    }
}
#endif
