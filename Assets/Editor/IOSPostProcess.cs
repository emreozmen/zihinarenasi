using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class IOSPostProcess
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
    {
        if (target != BuildTarget.iOS) return;

        // 1. Info.plist - NSUserTrackingUsageDescription ekle
        string plistPath = Path.Combine(pathToBuildProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        plist.root.SetString(
            "NSUserTrackingUsageDescription",
            "Reklamları kişiselleştirmek için cihaz tanımlayıcınıza erişmek istiyoruz."
        );
        // Export compliance - şifreleme kullanmıyoruz
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        File.WriteAllText(plistPath, plist.WriteToString());
        Debug.Log("[IOSPostProcess] NSUserTrackingUsageDescription Info.plist'e eklendi.");

        // 2. Xcode - AppTrackingTransparency.framework weak link olarak ekle
        string pbxPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
        var pbx = new PBXProject();
        pbx.ReadFromFile(pbxPath);

        string mainTarget      = pbx.GetUnityMainTargetGuid();
        string frameworkTarget = pbx.GetUnityFrameworkTargetGuid();

        pbx.AddFrameworkToProject(mainTarget,      "AppTrackingTransparency.framework", true);
        pbx.AddFrameworkToProject(frameworkTarget, "AppTrackingTransparency.framework", true);

        pbx.WriteToFile(pbxPath);
        Debug.Log("[IOSPostProcess] AppTrackingTransparency.framework Xcode projesine eklendi.");
    }
}
