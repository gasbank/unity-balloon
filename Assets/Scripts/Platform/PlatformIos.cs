using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PlatformIos : IPlatformBase {
    private static string GAME_CENTER_LOGIN_FAILED_FLAG_PREF_KEY = "__game_center_login_failed_flag";
    private static string GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY = "__game_center_login_disabled_flag";
    System.Action<CloudMetadata> onPeekResultSave;

    public static string LoginErrorTitle { get { return "\\iCloud 로그인 필요".Localized(); } }
    public static string LoginErrorMessage { get { return "\\홈 화면 -> 설정 -> (내 계정) -> iCloud에 로그인 해 주세요. 그 다음 Game Center, iCloud Drive, BalloonRider 항목을 켜고 다시 시도 해 주세요.".Localized(); } }
    public static string ConfirmMessage { get { return  "\\확인".Localized(); } }

    public bool CheckLoadSavePrecondition(string progressMessage, UnityAction onNotLoggedIn, UnityAction onAbort) {
        // Game Center 로그인 시도 회수 제한(3회 연속 로그인 거절)이 있다.
        // 회수 제한을 넘어서면 아무런 응답이 오지 않기 때문에 아예 시도조차 하지 않아야 한다.
        if (!Social.localUser.authenticated && PlayerPrefs.GetInt(GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY, 0) != 0) {
            // 유저가 직접 홈 -> 설정 -> Game Center 로그인을 해야 한다는 것을 알려야된다.
            ConfirmPopup.instance.Open(TextHelper.GetText("platform_game_center_login_required_popup"));
            return false;
        }

        if (!Social.localUser.authenticated) {
            ConfirmPopup.instance.OpenYesNoPopup(Platform.GetText("platform_game_center_login_required_popup"), onNotLoggedIn, onAbort);
            return false;
        }

        // 여기까지 왔으면 Game Center 로그인은 성공한 상태란 뜻이다.
        // Game Center 로그인을 앞으로 다시 시도하도록 한다.
        PlayerPrefs.SetInt(GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY, 0);
        PlayerPrefs.SetInt(GAME_CENTER_LOGIN_FAILED_FLAG_PREF_KEY, 0);

        if (Application.internetReachability == NetworkReachability.NotReachable) {
            ConfirmPopup.instance.Open(TextHelper.GetText("platform_load_require_internet_popup"));
            return false;
        }

        if (string.IsNullOrEmpty(progressMessage) == false) {
            ProgressMessage.instance.Open(progressMessage);
        }
        return true;
    }

    public void GetCloudLastSavedMetadataAsync(System.Action<CloudMetadata> onPeekResult) {
        if (!Social.localUser.authenticated) {
            SushiDebug.LogFormat("GetCloudSavedAccountData: not authenticated");
            if (onPeekResult != null) {
                onPeekResult(CloudMetadata.Invalid);
            }
            return;
        }

        // 아래 함수의 호출 결과는 결과는 PlatformCallbackHandler GameObject의
        // PlatformCallbackHandler.OnIosLoadResult()로 비동기적으로 호출되는 것으로 처리한다.
        // 이를 위해 onPeekResult를 챙겨둔다.
        onPeekResultSave = onPeekResult;
#if UNITY_IOS
        PlatformIosNative.loadFromCloudPrivate(Social.localUser.id, LoginErrorTitle, LoginErrorMessage, ConfirmMessage);
#endif
    }

    public void ExecuteCloudLoad() {
        PlatformSaveUtil.ShowLoadProgressPopup();

        // GetCloudLastSavedMetadataAsync()의 첫 번째 인자가 null이면
        // 게임 데이터 로드로 작동한다.
        GetCloudLastSavedMetadataAsync(null);
    }

    public void ExecuteCloudSave() {
        SaveLoadManager.Save(BalloonSpawner.instance, ConfigPopup.instance, Sound.instance, Data.instance, SaveLoadManager.SaveReason.BeforeCloudSave);
        PlatformSaveUtil.ShowSaveProgressPopup();
#pragma warning disable 219
        var savedData = PlatformSaveUtil.SerializeSaveData();
#pragma warning restore 219
        // 아래 함수의 호출 결과는 결과는 PlatformCallbackHandler GameObject의
        // PlatformCallbackHandler.OnIosSaveResult()로 비동기적으로 호출되는 것으로 처리한다.
#if UNITY_IOS
        PlatformIosNative.saveToCloudPrivate(Social.localUser.id, System.Convert.ToBase64String(savedData), LoginErrorTitle, LoginErrorMessage, ConfirmMessage);
#endif
    }

    public async void Login(System.Action<bool, string> onAuthResult) {
        // iOS에서는 Social.localUser.Authenticate 콜백이 호출이 되지 않을 때도 있다.
        // (유저가 의도적으로 로그인을 3회 거절한 경우, 4회째부터는 콜백이 안온다. ㄷㄷ)
        // 3.5초 타임아웃을 재자.

        var authResultTask = await Task.WhenAny(Task.Run(async () => {
            await Task.Delay(3500);
            return new System.Tuple<bool, string>(false, "TIMEOUT");
        }), Task.Run(async () => {
            var authenticateTask = new TaskCompletionSource<System.Tuple<bool, string>>();
            Social.localUser.Authenticate((b, reason) => {
                authenticateTask.SetResult(new System.Tuple<bool, string>(b, reason));
            });
            return await authenticateTask.Task;
        }));

        // 정상 반환이건 타임아웃이건 둘 중 하나에 대해서만 AuthenticateCallback()을 호출 해 준다.
        var authResult = await authResultTask;
        AuthenticateCallback(onAuthResult, authResult.Item1, authResult.Item2);
    }

    void AuthenticateCallback(System.Action<bool, string> onAuthResult, bool b, string reason) {
        // Game Center 로그인 성공/실패 유무에 따른 플래그 업데이트
        PlayerPrefs.SetInt(GAME_CENTER_LOGIN_FAILED_FLAG_PREF_KEY, b ? 0 : 1);

        SushiDebug.LogFormat("iOS Game Center Login Result: {0} / Reason: {1}", b, reason);

        // 회수 제한 마지막 기회임을 체크해서 다시는 시도하지 않도록 한다.
        // 그런데... reason은 유저가 읽을 수 있도록 시스템 언어 설정에 따라
        // 같은 결과라도 언어별로 값이 다르다. ㄷㄷㄷ
        // 그러므로 GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY 플래그를 올리는 것은
        // 제대로 작동하지 않는다. (언어별로 정상 작동 여부가 달라진다.)
        // 아직은 고치지 않고 그래두 두겠다...
        if (b == false && reason.Contains("canceled") && reason.Contains("disabled")) {
            PlayerPrefs.SetInt(GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY, 1);
        } else if (b == true) {
            PlayerPrefs.SetInt(GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY, 0);
        }
        PlayerPrefs.Save();

        onAuthResult(b, reason);

#if UNITY_IOS
        // 로그인 이후 알림 기능에 대한 동의 팝업을 띄우자...
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);

        // 로그인 성공했다면 업적 알림 기능 켜기
        if (b) {
            UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        }
#endif
    }

    public bool LoginFailedLastTime() {
        // iOS는 마지막 로그인이 실패했건 안했건 무조건 구동 시 로그인 시도 한다.
        return false;
        //return PlayerPrefs.GetInt(GAME_CENTER_LOGIN_FAILED_FLAG_PREF_KEY, 0) != 0;
    }

    public void Logout() {
        throw new System.NotImplementedException();
    }

    public void PreAuthenticate() {
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon) {

        var notificationDate0000 = System.DateTime.Today;
        var now = System.DateTime.Now;
        var notificationDate0900 = notificationDate0000.AddHours(9);
        if (notificationDate0900 < now) {
            notificationDate0900 = notificationDate0900.AddDays(1);
        }

#if UNITY_IOS
        string text = string.Format("{0}\n{1}", title, body);
        SushiDebug.Log("Schedule Local Notification");
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        ClearAllNotifications();

        // 09:00
        UnityEngine.iOS.LocalNotification notification0900 = new UnityEngine.iOS.LocalNotification();
        notification0900.fireDate = notificationDate0900;
        notification0900.alertBody = text;
        notification0900.alertAction = "Action";
        notification0900.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
        notification0900.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notification0900);
#endif
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData) {
        string reportSaveDataPath = Application.persistentDataPath + "/report-save-data";
        System.IO.File.WriteAllBytes(reportSaveDataPath, saveData);
#if UNITY_IOS
        PlatformIosNative.sendMail(subject, text, mailTo, reportSaveDataPath);
#endif
    }

    public void ShareScreenshot(byte[] pngData) {
        string pngDataPath = Application.persistentDataPath + "/screenshot-share.png";
        System.IO.File.WriteAllBytes(pngDataPath, pngData);
        NativeShare.Share("", pngDataPath, null, "", "image/png", true, "Share");
    }

    public void ClearAllNotifications() {
#if UNITY_IOS
        PlatformIosNative.clearAllNotifications();
#endif
    }

    public void OnCloudSaveResult(string result) {
        if (result == "OK") {
            // handle reading or writing of saved game.
            PlatformSaveUtil.ShowSaveResultPopup();
        } else {
            // handle error
            PlatformSaveUtil.ShowSaveErrorPopup(TextHelper.GetText("platform_cloud_save_fail") + "\n\n" + result);
        }
    }

    public void OnCloudLoadResult(string result, byte[] data) {
        if (result == "OK") {
            SushiDebug.LogFormat("OnCloudLoadResult: data length {0} bytes", data != null ? data.Length : 0);
            // 메타데이터 조회의 경우와 실제 세이브 데이터 로딩의 경우를 나눠서 처리
            if (onPeekResultSave != null) {
                SushiDebug.Log("OnCloudLoadResult: onPeekResultSave valid");
                CloudMetadata cloudMetadata = CloudMetadata.Invalid;
                if (data == null || data.Length == 0) {
                } else {
                    try {
                        var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(data);
                        cloudMetadata = new CloudMetadata {
                            level = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_KEY),
                            levelExp = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_EXP_KEY),
                            gem = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_GEM_KEY),
                            riceRate = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_RICE_RATE_KEY),
                            saveDate = PlatformSaveUtil.GetInt64FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.SAVE_DATE_KEY),
                        };
                    } catch {
                        cloudMetadata = CloudMetadata.Invalid;
                    }
                }
                onPeekResultSave(cloudMetadata);
                onPeekResultSave = null;
            } else {
                SushiDebug.Log("OnCloudLoadResult: onPeekResultSave empty. data load...");
                if (data == null || data.Length == 0) {
                    PlatformSaveUtil.ShowLoadErrorPopup("OnCloudLoadResult: Cloud save data corrupted");
                } else {
                    SushiDebug.LogFormat("OnCloudLoadResult: success! - Data size: {0} bytes", data.Length);
                    var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(data);
                    PlatformSaveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
                }
            }
        } else {
            PlatformSaveUtil.ShowSaveErrorPopup(TextHelper.GetText("platform_cloud_load_fail") + "\n\n" + result);
        }
    }

    public void RequestUserReview() {
        Application.OpenURL("itms-apps://itunes.apple.com/app/idxxxxxxxx");
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon) {
#if UNITY_IOS
        UnityEngine.iOS.LocalNotification n = new UnityEngine.iOS.LocalNotification();
        n.fireDate = System.DateTime.Now.AddMilliseconds(afterMs);
        n.alertBody = body;
        n.alertAction = "Action";
        n.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(n);
#endif
    }

    public string GetAccountTypeText() {
        return TextHelper.GetText("platform_account_game_center");
    }
}
