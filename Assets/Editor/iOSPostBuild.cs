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
    // Apple Developer Team ID
    // (Apple Distribution sertifikasinin CN alanindaki parantez icindeki kod)
    private const string DevelopmentTeamId = "H66NL7YDHZ";

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

#if UNITY_EDITOR && UNITY_IOS
        string projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTargetGuid      = proj.GetUnityMainTargetGuid();
        string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

        // ── Game Center entitlement ──
        string entitlementsFileName = "GameCenter.entitlements";
        File.Copy(
            Path.Combine(Application.dataPath, "GameCenter.entitlements"),
            Path.Combine(path, entitlementsFileName),
            true
        );
        proj.AddFile(entitlementsFileName, entitlementsFileName);
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);

        // Framework icin signing kapat
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_REQUIRED", "NO");
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_ALLOWED",  "NO");

        proj.WriteToFile(projPath);

        // ── Firebase SPM signing fix: Gymfile ──
        // SPM paketleri (GoogleUtilities, Firebase) ayri Xcode projelerinde oldugu icin
        // pbxproj veya xcconfig degisiklikleri onlara ulasmiyor.
        // Cloud Build fastlane kullanir; proje dizininde Gymfile varsa xcargs'i okur.
        // xcargs ile DEVELOPMENT_TEAM tum targetlara (SPM dahil) komut satirindan gecilir.
        // Cloud Build'in sed'i pbxproj'u temizler ama komut satiri argumanlarina dokunamaz.
        string gymfilePath = Path.Combine(path, "Gymfile");
        // Eger Gymfile zaten varsa uzerine yazma, xcargs satiri ekle
        string gymfileContent = "";
        if (File.Exists(gymfilePath))
            gymfileContent = File.ReadAllText(gymfilePath);

        if (!gymfileContent.Contains("DEVELOPMENT_TEAM"))
        {
            gymfileContent += "\n# Firebase SPM signing fix\n"
                           + "xcargs \"DEVELOPMENT_TEAM=" + DevelopmentTeamId + "\"\n";
            File.WriteAllText(gymfilePath, gymfileContent);
            Debug.Log("[iOSPostBuild] Gymfile'a DEVELOPMENT_TEAM xcargs eklendi.");
        }

        // ── Info.plist ──
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription",
            "Zihin Arenası, sana daha alakalı reklamlar gösterebilmek için reklam verilerini kullanmak istiyor.");
        plist.WriteToFile(plistPath);

        Debug.Log("[iOSPostBuild] Tamamlandi.");
#endif
    }
}
#endif
