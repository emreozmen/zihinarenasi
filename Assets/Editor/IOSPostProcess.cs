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

        string plistPath = Path.Combine(pathToBuildProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        plist.root.SetString(
            "NSUserTrackingUsageDescription",
            "Reklamları kişiselleştirmek için cihaz tanımlayıcınıza erişmek istiyoruz."
        );

        File.WriteAllText(plistPath, plist.WriteToString());
        Debug.Log("[IOSPostProcess] NSUserTrackingUsageDescription Info.plist'e eklendi.");
    }
}
