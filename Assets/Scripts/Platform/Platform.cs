using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Platform {
    private static IPlatformBase platform;
    public static IPlatformBase instance {
        get {
            if (platform == null) {
                if (Application.isEditor) {
                    platform = new PlatformEditor();
                } else if (Application.platform == RuntimePlatform.Android) {
                    platform = new PlatformAndroid();
                }
#if UNITY_IOS
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    platform = new PlatformIos();
                }
#endif
                else {
                    Debug.LogErrorFormat("Platform: Unknown/not supported platform detected: {0}", Application.platform);
                    // Fallback to editor
                    platform = new PlatformEditor();
                }
            }
            return platform;
        }
    }

    internal static string GetText(string v) {
        return string.Format(TextHelper.GetText(v), Platform.instance.GetAccountTypeText());
    }

    public static void StartAuthAsync(System.Action<bool, string> onAuthResult) {
        instance.PreAuthenticate();
        instance.Login(onAuthResult);
    }

    public static void CloudLoad() {
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadBegin, 0, 0);
        if (instance.CheckLoadSavePrecondition(TextHelper.GetText("platform_loading"), PlatformSaveUtil.StartLoginAndLoad, PlatformSaveUtil.CancelStartLoginForLoad) == false) {
            return;
        }

        instance.GetCloudLastSavedMetadataAsync((cloudMetadata) => {
            ProgressMessage.instance.Close();

            SushiDebug.LogFormat("prevAccountLevel = {0}", cloudMetadata.level);
            SushiDebug.LogFormat("prevAccountLevelExp = {0}", cloudMetadata.levelExp);
            SushiDebug.LogFormat("prevAccountGem = {0}", cloudMetadata.gem);
            SushiDebug.LogFormat("prevAccountRiceRate = {0}", cloudMetadata.riceRate);
            SushiDebug.LogFormat("prevSaveDate = {0}", cloudMetadata.saveDate);

            if (cloudMetadata.level >= 0 && cloudMetadata.levelExp >= 0 && cloudMetadata.saveDate >= 0) {
                var overwriteConfirmMsg = TextHelper.GetText("platform_load_confirm_popup",
                    /* {0} */0,
                    /* {1} */cloudMetadata.levelExp.Postfixed(),
                    /* {2} */cloudMetadata.gem >= 0 ? cloudMetadata.gem.Postfixed() : "???",
                    /* {3} */cloudMetadata.riceRate >= 0 ? cloudMetadata.riceRate.Postfixed() : "???",
                    /* {4} */new System.DateTime(cloudMetadata.saveDate),
                    /* {5} */0,
                    /* {6} */0,
                    /* {7} */0,
                    /* {8} */0,
                    /* {9} */System.DateTime.Now);

                ConfirmPopup.instance.OpenYesNoPopup(overwriteConfirmMsg, () => {
                    // 로드하려는 데이터가 현재 플레이하는 것보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = cloudMetadata.level < ResourceManager.instance.accountLevel
                        || cloudMetadata.levelExp < ResourceManager.instance.accountLevelExp
                        || cloudMetadata.gem < ResourceManager.instance.accountGem
                        || cloudMetadata.riceRate < ResourceManager.instance.accountRiceRate;

                    if (rollback) {
                        var msgAgain = TextHelper.GetText("platform_load_confirm_popup_rollback_alert") + "\n\n" + overwriteConfirmMsg;
                        ConfirmPopup.instance.OpenYesNoPopup(msgAgain, instance.ExecuteCloudLoad, PlatformSaveUtil.CancelStartLoginForLoad);
                    } else {
                        instance.ExecuteCloudLoad();
                    }
                }, PlatformSaveUtil.CancelStartLoginForLoad);
            } else {
                PlatformSaveUtil.NoDataToLoad();
            }
        });
    }

    public static void CloudSave() {
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveBegin, 0, 0);
        if (instance.CheckLoadSavePrecondition(TextHelper.GetText("platform_saving"), PlatformSaveUtil.StartLoginAndSave, PlatformSaveUtil.CancelStartLoginForSave) == false) {
            return;
        }

        instance.GetCloudLastSavedMetadataAsync((cloudMetadata) => {
            ProgressMessage.instance.Close();

            SushiDebug.LogFormat("prevAccountLevel = {0}", cloudMetadata.level);
            SushiDebug.LogFormat("prevAccountLevelExp = {0}", cloudMetadata.levelExp);
            SushiDebug.LogFormat("prevAccountGem = {0}", cloudMetadata.gem);
            SushiDebug.LogFormat("prevAccountRiceRate = {0}", cloudMetadata.riceRate);
            SushiDebug.LogFormat("prevSaveDate = {0}", cloudMetadata.saveDate);

            if (cloudMetadata.level >= 0 && cloudMetadata.levelExp >= 0 && cloudMetadata.saveDate >= 0) {
                var overwriteConfirmMsg = TextHelper.GetText("platform_save_confirm_popup",
                    /* {0} */0,
                    /* {1} */cloudMetadata.levelExp.Postfixed(),
                    /* {2} */cloudMetadata.gem >= 0 ? cloudMetadata.gem.Postfixed() : "???",
                    /* {3} */cloudMetadata.riceRate >= 0 ? cloudMetadata.riceRate.Postfixed() : "???",
                    /* {4} */new System.DateTime(cloudMetadata.saveDate),
                    /* {5} */0,
                    /* {6} */ResourceManager.instance.accountLevelExp.Postfixed(),
                    /* {7} */ResourceManager.instance.accountGem.Postfixed(),
                    /* {8} */ResourceManager.instance.accountRiceRate.Postfixed(),
                    /* {9} */System.DateTime.Now);

                ConfirmPopup.instance.OpenYesNoPopup(overwriteConfirmMsg, () => {
                    // 현재 플레이 상황이 덮어쓰려는 저장 데이터보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = cloudMetadata.level > ResourceManager.instance.accountLevel
                        || cloudMetadata.levelExp > ResourceManager.instance.accountLevelExp
                        || cloudMetadata.gem > ResourceManager.instance.accountGem
                        || cloudMetadata.riceRate > ResourceManager.instance.accountRiceRate;

                    if (rollback) {
                        var msgAgain = TextHelper.GetText("platform_save_confirm_popup_rollback_alert") + "\n\n" + overwriteConfirmMsg;
                        ConfirmPopup.instance.OpenYesNoPopup(msgAgain, instance.ExecuteCloudSave, PlatformSaveUtil.CancelStartLoginForSave);
                    } else {
                        instance.ExecuteCloudSave();
                    }


                }, PlatformSaveUtil.CancelStartLoginForSave);
            } else {
                instance.ExecuteCloudSave();
            }
        });
    }
}
