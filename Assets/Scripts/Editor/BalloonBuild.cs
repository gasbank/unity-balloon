using UnityEngine;
using UnityEditor;
using System.Linq;

class BalloonBuild
{
    static string[] Scenes
    {
        get
        {
            return new string[] {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/Stage 01.unity",
                "Assets/Scenes/Stage 02.unity",
                "Assets/Scenes/Stage 03.unity",
                "Assets/Scenes/Stage 04.unity",
                "Assets/Scenes/Stage 05.unity",
                "Assets/Scenes/Stage 06.unity",
                "Assets/Scenes/Stage 07.unity",
                "Assets/Scenes/Stage 08.unity",
                "Assets/Scenes/Stage 09.unity",
                "Assets/Scenes/Stage 10.unity",
                "Assets/Scenes/Stage 11.unity",
                "Assets/Scenes/Stage 12.unity",
                "Assets/Scenes/Stage 13.unity",
                "Assets/Scenes/Stage 14.unity",
                "Assets/Scenes/Stage 15.unity",
                "Assets/Scenes/Stage 16.unity",
                "Assets/Scenes/Stage 17.unity",
                "Assets/Scenes/Stage 18.unity",
                "Assets/Scenes/Stage 19.unity",
                "Assets/Scenes/Stage 20.unity",
                "Assets/Scenes/Ending.unity",
                "Assets/Scenes/Stage Selection.unity",
            };
        }
    }

    static void PerformAndroidBuild() {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = Scenes;
        options.target = BuildTarget.Android;
        options.locationPathName = "./balloon.apk";
        // BALLOON_DEBUG 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (System.Environment.GetEnvironmentVariable("BALLOON_DEV_BUILD") != "1") {
            RemovingBalloonDebugDefine(BuildTargetGroup.Android);
        }
        var cmdArgs = System.Environment.GetCommandLineArgs().ToList();
        if (ProcessAndroidKeystorePassArg(options, cmdArgs)) {
            ProcessBuildNumber(cmdArgs);
            BuildPipeline.BuildPlayer(options);
        }
    }

    private static bool ProcessAndroidKeystorePassArg(BuildPlayerOptions options, System.Collections.Generic.List<string> cmdArgs) {
        var keystorePassIdx = cmdArgs.FindIndex(e => e == "-keystorePass");
        var keystorePass = "";
        if (keystorePassIdx >= 0) {
            keystorePass = cmdArgs[keystorePassIdx + 1];
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystorePass;
            return true;
        } else {
            Debug.LogError("-keystorePass argument should be provided to build Android APK.");
        }
        return false;
    }

    public static AppMetaInfo ProcessBuildNumber(System.Collections.Generic.List<string> cmdArgs) {
        var buildNumberIdx = cmdArgs.FindIndex(e => e == "-buildNumber");
        var buildNumber = "<?>";
        if (buildNumberIdx >= 0) {
            buildNumber = cmdArgs[buildNumberIdx + 1];
        }
        var appMetaInfo = AppMetaInfoEditor.CreateAppMetaInfoAsset();
        appMetaInfo.buildNumber = buildNumber;
        appMetaInfo.buildStartDateTime = System.DateTime.Now.ToShortDateString();
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

    static void PerformIosBuild() {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = Scenes;
        options.target = BuildTarget.iOS;
        options.locationPathName = "./build";
        // BALLOON_DEBUG 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (System.Environment.GetEnvironmentVariable("BALLOON_DEV_BUILD") != "1") {
            RemovingBalloonDebugDefine(BuildTargetGroup.iOS);
        }
        PlayerSettings.iOS.appleDeveloperTeamID = "TG9MHV97AH";
        var cmdArgs = System.Environment.GetCommandLineArgs().ToList();
        ProcessBuildNumber(cmdArgs);
        BuildPipeline.BuildPlayer(options);
    }

    private static void RemovingBalloonDebugDefine(BuildTargetGroup buildTargetGroup) {
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";", scriptingDefineSymbols.Split(';').Where(e => e != "BALLOON_DEBUG" && e != "BALLOON_ADMIN"));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}
