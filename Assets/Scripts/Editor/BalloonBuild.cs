using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.Build.Reporting;

static class BalloonBuild
{
    static string[] Scenes
    {
        get
        {
            return new[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/Stage.unity",
                "Assets/Scenes/Ending.unity",
#if BALLOON_ADMIN
                "Assets/Scenes/Stage Selection.unity",

                "Assets/Scenes/Test Scenes/Iap Test Scene.unity",
                "Assets/Scenes/Test Scenes/Stage 02 - Test.unity",
                "Assets/Scenes/Test Scenes/Stage Boost Test.unity",
                "Assets/Scenes/Test Scenes/Stage Fever Gauge.unity",
                "Assets/Scenes/Test Scenes/Stage Game Over Immediately.unity",
                "Assets/Scenes/Test Scenes/Stage Red Block (Rechargeable).unity",
                "Assets/Scenes/Test Scenes/Stage Shading.unity",
                "Assets/Scenes/Test Scenes/Stage Slider Interface.unity",
                "Assets/Scenes/Test Scenes/Stage Wind Region (Rechargeable).unity",
                "Assets/Scenes/Test Scenes/Stage Yellow Block Spawn.unity",
                "Assets/Scenes/Test Scenes/Test.unity",
                "Assets/Scenes/Test Scenes/Yellow Block Catalog.unity",
#endif
            };
        }
    }

    [MenuItem("Balloon/Perform Android Build (Mono)")]
    [UsedImplicitly]
    public static void PerformAndroidBuildMono()
    {
        Environment.SetEnvironmentVariable("BALLOON_DEV_BUILD", "1");
        var locationPathName = "./balloon-mono.apk";
        if (PerformAndroidBuildInternal(false, false, locationPathName))
        {
            EditorUtility.RevealInFinder(locationPathName);
        }
    }

    [MenuItem("Balloon/Perform Android Build (IL2CPP)")]
    [UsedImplicitly]
    public static void PerformAndroidBuildIl2Cpp()
    {
        Environment.SetEnvironmentVariable("BALLOON_DEV_BUILD", "1");
        Environment.SetEnvironmentVariable("BALLOON_SKIP_ARMV7", "1");
        var locationPathName = "./balloon-il2cpp.apk";
        if (PerformAndroidBuildInternal(true, false, locationPathName))
        {
            EditorUtility.RevealInFinder(locationPathName);
        }
    }

    [UsedImplicitly]
    public static void PerformAndroidBuild()
    {
        PerformAndroidBuildInternal(true);
    }

    static bool PerformAndroidBuildInternal(bool il2Cpp, bool run = false, string locationPathName = "./balloon.apk")
    {
        var isReleaseBuild = Environment.GetEnvironmentVariable("BALLOON_DEV_BUILD") != "1";
        var skipArmV7 = Environment.GetEnvironmentVariable("BALLOON_SKIP_ARMV7") == "1";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes, target = BuildTarget.Android, locationPathName = locationPathName
        };
        if (run)
        {
            options.options |= BuildOptions.AutoRunPlayer;
        }

        // Split APKs 옵션 켠다. 개발중에는 끄고 싶을 때도 있을 것
        if (il2Cpp)
        {
            // 자동 빌드는 IL2CPP로 
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            // Split APK
            PlayerSettings.Android.buildApkPerCpuArchitecture = true;
            // 개발중일때는 ARM64만 빌드하자. 빠르니까...
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            if (isReleaseBuild || skipArmV7 == false)
            {
                PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARMv7;
            }
        }
        else
        {
            // 개발 기기에서 바로 보고 싶을 땐 mono로 보자
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            // Split Apk 필요 없다
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            // 개발중일때는 ARM64만 빌드하자. 빠르니까...
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }

        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // 디버그 관련 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (isReleaseBuild)
        {
            RemovingBalloonDebugDefine(BuildTargetGroup.Android);
        }

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        if (ProcessAndroidKeystorePassArg(cmdArgs))
        {
            ProcessBuildNumber(cmdArgs);
            var buildReport = BuildPipeline.BuildPlayer(options);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(-1);
                }

                return false;
            }

            // 빌드 성공!
            return true;
        }

        // 키가 없어서 실패!
        return false;
    }

    static bool ProcessAndroidKeystorePassArg(List<string> cmdArgs)
    {
        // 이미 채워져있다면 더 할 게 없다.
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) == false
            && string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass) == false)
        {
            return true;
        }

        var keystorePassIdx = cmdArgs.FindIndex(e => e == "-keystorePass");
        string keystorePass;
        if (keystorePassIdx >= 0)
        {
            keystorePass = cmdArgs[keystorePassIdx + 1];
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystorePass;
            return true;
        }
        else
        {
            keystorePass = Environment.GetEnvironmentVariable("BALLOON_KEYSTORE_PASS");
            if (string.IsNullOrEmpty(keystorePass))
            {
                try
                {
                    keystorePass = File.ReadAllText(".balloon_keystore_pass").Trim();
                    PlayerSettings.Android.keystorePass = keystorePass;
                    PlayerSettings.Android.keyaliasPass = keystorePass;
                    return true;
                }
                catch
                {
                    Debug.LogError(
                        "-keystorePass argument or '.balloon_keystore_pass' file should be provided to build Android APK.");
                }
            }
            else
            {
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasPass = keystorePass;
                return true;
            }
        }

        return false;
    }

    public static AppMetaInfo ProcessBuildNumber(List<string> cmdArgs)
    {
        var buildNumberIdx = cmdArgs.FindIndex(e => e == "-buildNumber");
        var buildNumber = AppMetaInfo.TemporaryBuildNumber;
        if (buildNumberIdx >= 0)
        {
            buildNumber = cmdArgs[buildNumberIdx + 1];
        }

        var appMetaInfo = AppMetaInfoEditor.CreateAppMetaInfoAsset();
        appMetaInfo.buildNumber = buildNumber;
        appMetaInfo.buildStartDateTime = DateTime.Now.ToShortDateString();
#if UNITY_ANDROID
        appMetaInfo.androidBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
#else
        appMetaInfo.androidBundleVersionCode = -1;
#endif
#if UNITY_IOS
        appMetaInfo.iosBuildNumber = PlayerSettings.iOS.buildNumber;
#else
        appMetaInfo.iosBuildNumber = "INVALID";
#endif
        EditorUtility.SetDirty(appMetaInfo);
        AssetDatabase.SaveAssets();
        return appMetaInfo;
    }

    [UsedImplicitly]
    public static void PerformIosAdHocBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("BALLOON_IOS_AD_HOC_PROFILE_ID"));
    }

    [UsedImplicitly]
    public static void PerformIosAppStoreBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("BALLOON_IOS_APP_STORE_PROFILE_ID"));
    }

    static void PerformIosDistributionBuild(string profileId)
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes, target = BuildTarget.iOS, locationPathName = "./build"
        };
        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // 디버그 관련 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (Environment.GetEnvironmentVariable("BALLOON_DEV_BUILD") != "1")
        {
            RemovingBalloonDebugDefine(BuildTargetGroup.iOS);
        }

        PlayerSettings.iOS.appleDeveloperTeamID = "TG9MHV97AH";
        if (string.IsNullOrEmpty(profileId))
        {
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        }
        else
        {
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileID = profileId;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;
        }

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        ProcessBuildNumber(cmdArgs);
        var buildReport = BuildPipeline.BuildPlayer(options);
        if (buildReport.summary.result != BuildResult.Succeeded && Application.isBatchMode)
        {
            EditorApplication.Exit(-1);
        }
    }

    static void RemovingBalloonDebugDefine(BuildTargetGroup buildTargetGroup)
    {
        var symbolsToBeRemovedList = new List<string>
        {
            "BALLOON_DEBUG",
            "CONDITIONAL_DEBUG",
            "BALLOON_ADMIN"
        };
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";",
            scriptingDefineSymbols.Split(';').Where(e => symbolsToBeRemovedList.Contains(e) == false));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}