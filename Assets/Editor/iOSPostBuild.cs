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

        // ── Firebase SPM fix: xcconfig dosyasi olustur ──
        // Cloud Build, pbxproj'daki DEVELOPMENT_TEAM satirlarini sed ile siler.
        // Ama xcconfig dosyasina dokunamaz. Xcode oradan okur.
        // baseConfigurationReference satiri DEVELOPMENT_TEAM icermez → sed silemez.
        string xcconfigFileName = "CloudBuildSPMFix.xcconfig";
        string xcconfigFilePath = Path.Combine(path, xcconfigFileName);
        File.WriteAllText(xcconfigFilePath,
            "// Firebase SPM signing fix - Cloud Build sed bu dosyaya dokunamaz\n" +
            "DEVELOPMENT_TEAM = " + DevelopmentTeamId + "\n");

        string xcconfigGuid = proj.AddFile(xcconfigFileName, xcconfigFileName, PBXSourceTree.Source);

        // Framework icin signing kapat
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_REQUIRED", "NO");
        proj.SetBuildProperty(frameworkTargetGuid, "CODE_SIGNING_ALLOWED",  "NO");

        // Projeyi diske yaz
        proj.WriteToFile(projPath);

        // ── pbxproj'a xcconfig referansini elle ekle ──
        // SetBaseReferenceForBuildConfig Unity 6'da yok, dogrudan text manipulation yapiyoruz.
        // "baseConfigurationReference = GUID;" satiri DEVELOPMENT_TEAM icermez → sed bunu silmez.
        string[] targetGuids = { mainTargetGuid, frameworkTargetGuid };
        string[] configNames = { "Release", "Debug", "ReleaseForRunning", "ReleaseForProfiling" };

        string projContent = File.ReadAllText(projPath);
        bool modified = false;

        foreach (string tGuid in targetGuids)
        {
            foreach (string cfgName in configNames)
            {
                string cfgGuid = proj.BuildConfigByName(tGuid, cfgName);
                if (string.IsNullOrEmpty(cfgGuid)) continue;

                // Bu config'in section'ini bul: "GUID /* cfgName */ = {"
                string sectionHeader = cfgGuid + " /* " + cfgName + " */ = {";
                int secStart = projContent.IndexOf(sectionHeader);
                if (secStart < 0) continue;

                // Section bitis: ilk "};"
                int secEnd = projContent.IndexOf("};", secStart);
                if (secEnd < 0) continue;

                // Zaten xcconfig referansi varsa atla
                string section = projContent.Substring(secStart, secEnd - secStart);
                if (section.Contains("baseConfigurationReference")) continue;

                // "isa = XCBuildConfiguration;" sonrasina ekle
                string isaMarker = "isa = XCBuildConfiguration;";
                int isaIdx = projContent.IndexOf(isaMarker, secStart);
                if (isaIdx < 0 || isaIdx > secEnd) continue;

                string insertion = "\n\t\tbaseConfigurationReference = "
                                 + xcconfigGuid
                                 + " /* " + xcconfigFileName + " */;";
                projContent = projContent.Insert(isaIdx + isaMarker.Length, insertion);
                modified = true;
            }
        }

        if (modified)
        {
            File.WriteAllText(projPath, projContent);
            Debug.Log("[iOSPostBuild] xcconfig base referans eklendi (Firebase SPM DEVELOPMENT_TEAM fix).");
        }

        // ── Info.plist ──
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription",
            "Zihin Arenasi, sana daha alakali reklamlar gosterebilmek icin reklam verilerini kullanmak istiyor.");
        plist.WriteToFile(plistPath);

        Debug.Log("[iOSPostBuild] Tamamlandi.");
#endif
    }
}
#endif
