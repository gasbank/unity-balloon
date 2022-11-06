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
#if DEV_BUILD
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

    [UsedImplicitly]
    public static void PerformAndroidBuild()
    {
        var isReleaseBuild = Environment.GetEnvironmentVariable("DEV_BUILD") != "1";
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes, target = BuildTarget.Android, locationPathName = "build/Balloon.apk"
        };
        
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
            }
        }
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
            keystorePass = Environment.GetEnvironmentVariable("keystore_pass");
            if (string.IsNullOrEmpty(keystorePass))
            {
                try
                {
                    keystorePass = File.ReadAllText(".keystore_pass").Trim();
                    PlayerSettings.Android.keystorePass = keystorePass;
                    PlayerSettings.Android.keyaliasPass = keystorePass;
                    return true;
                }
                catch
                {
                    Debug.LogError(
                        "-keystorePass argument or '.keystore_pass' file should be provided to build Android APK.");
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
    public static void PerformIosBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("IOS_PROFILE_GUID"));
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
        if (Environment.GetEnvironmentVariable("DEV_BUILD") != "1")
        {
            RemovingBalloonDebugDefine(BuildTargetGroup.iOS);
        }

        PlayerSettings.iOS.appleDeveloperTeamID = Environment.GetEnvironmentVariable("IOS_TEAM_ID");
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
        
        // 자동 빌드니까 당연히 Device SDK 사용해야겠지?
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

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
            "CONDITIONAL_DEBUG",
            "DEV_BUILD"
        };
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";",
            scriptingDefineSymbols.Split(';').Where(e => symbolsToBeRemovedList.Contains(e) == false));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}