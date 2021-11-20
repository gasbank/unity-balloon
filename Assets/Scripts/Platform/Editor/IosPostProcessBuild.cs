using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using System.IO;
#endif

public static class DisableBitcode
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_IOS
             //localization
            NativeLocale.AddLocalizedStringsIOS(path, Path.Combine(Application.dataPath, "NativeLocale/iOS"));
            SushiDebug.Log("DEBUG_________Application datapath:"+Application.dataPath);
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            // Facebook SDK가 Bitcode 미지원하므로 이 플래그를 꺼야 빌드가 된다.
            string target = pbxProject.TargetGuidByName("Unity-iPhone");            
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            // 로컬 알림 관련해서 아래 프레임워크가 추가 되어야 한다.
            pbxProject.AddFrameworkToProject(target, "UserNotifications.framework", false);
			
            pbxProject.AddCapability(target, PBXCapabilityType.iCloud);
            pbxProject.AddCapability(target, PBXCapabilityType.GameCenter);
            pbxProject.AddCapability(target, PBXCapabilityType.InAppPurchase);

            pbxProject.WriteToFile (projectPath);

            var plistPath = Path.Combine (path, "Info.plist");
            var plist = new PlistDocument ();
            plist.ReadFromFile (plistPath);
            // 수출 관련 규정 플래그 추가 (AppStore 제출 시 필요하다고 안내하고 있음)
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            // 스크린샷을 앨범에 저장하고자 할 때 필요한 권한을 요청하는 팝업 설정 (지정하지 않으면 크래시)
            plist.root.SetString("NSPhotoLibraryUsageDescription", "Screenshot Save");
            plist.root.SetString("NSPhotoLibraryAddUsageDescription", "Screenshot Save");
            // 최신 AdMob SDK에서는 아래 항목이 필수다...헛헛헛...
            plist.root.SetString("GADApplicationIdentifier", "ca-app-pub-5072035175916776~2508482457");
   
            plist.WriteToFile (plistPath);

            // Copy entitlements file
            System.IO.File.Copy("balloon.entitlements", path + "/balloon.entitlements", true);
#endif
            // https://stackoverflow.com/questions/55419956/how-to-fix-pod-does-not-support-provisioning-profiles-in-azure-devops-build
            var podfileAppend = @"
post_install do |installer|
    installer.pods_project.targets.each do |target|
        target.build_configurations.each do |config|
            config.build_settings['CODE_SIGN_STYLE'] = ""Automatic"";
        end
    end
end
";
            File.AppendAllText($"{path}/Podfile", podfileAppend);
        }
    }
}