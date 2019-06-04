using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;
using UnityEngine.Events;

public class PlatformAndroid : IPlatformBase {
    private static string GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY = "__google_login_failed_flag";
#if UNITY_ANDROID
    private static string NOTIFICATION_MANAGER_FULL_CLASS_NAME = "top.plusalpha.notification.NotificationManager";
    // 버그 신고 기능과 스크린샷 기능은 기능상은 다르지만, 라이브러리를 따로 추가하지
    // 않고 구현했으므로 같은 이름을 쓴다.
    private static string SCREENSHOT_AND_REPORT_FULL_CLASS_NAME = "top.plusalpha.screenshot.Screenshot";
#endif
    public bool CheckLoadSavePrecondition(string progressMessage, UnityAction onNotLoggedIn, UnityAction onAbort) {
        if (!Social.localUser.authenticated) {
            ConfirmPopup.instance.OpenYesNoPopup(Platform.GetText("platform_google_login_required_popup"), onNotLoggedIn, onAbort);
            return false;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable) {
            ConfirmPopup.instance.Open(TextHelper.GetText("platform_load_require_internet_popup"));
            return false;
        }

        if (string.IsNullOrEmpty(progressMessage) == false) {
            ProgressMessage.instance.Open(progressMessage);
        }
        return true;
    }

    public static void Open(ISavedGameClient savedGameClient, bool useAutomaticResolution, ConflictCallback conflictCallback, System.Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {
#if !NO_GPGS
        if (useAutomaticResolution) {
            savedGameClient.OpenWithAutomaticConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                ConflictResolutionStrategy.UseLongestPlaytime,
                completedCallback);
        } else {
            savedGameClient.OpenWithManualConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                true,
                conflictCallback,
                completedCallback);
        }
#endif
    }

    public void GetCloudLastSavedMetadataAsync(System.Action<CloudMetadata> onPeekResult) {
        if (!Social.localUser.authenticated) {
            SushiDebug.LogFormat("GetCloudSavedAccountData: not authenticated");
            onPeekResult(CloudMetadata.Invalid);
            return;
        }

#if !NO_GPGS
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                (status, game) => {
                    if (status == SavedGameRequestStatus.Success) {
                        // handle reading or writing of saved game.

                        SushiDebug.LogFormat("GetCloudSavedAccountData: Save game open (read) success! Filename: {0}", game.Filename);

                        savedGameClient.ReadBinaryData(game, (status2, data2) => {
                            if (status == SavedGameRequestStatus.Success) {
                                // handle processing the byte array data
                                SushiDebug.LogFormat("GetCloudSavedAccountData success! - Data size: {0} bytes", data2.Length);
                                try {
                                    var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(data2);
                                    var cloudMetadata = new CloudMetadata {
                                        level = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_KEY),
                                        levelExp = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_EXP_KEY),
                                        gem = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_GEM_KEY),
                                        riceRate = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_RICE_RATE_KEY),
                                        saveDate = PlatformSaveUtil.GetInt64FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.SAVE_DATE_KEY),
                                    };
                                    onPeekResult(cloudMetadata);
                                } catch {
                                    SushiDebug.LogFormat("GetCloudSavedAccountData: Exception at deserialization");
                                    onPeekResult(CloudMetadata.Invalid);
                                }
                            } else {
                                SushiDebug.LogFormat("GetCloudSavedAccountData: ReadBinaryData error! - {0}", status2);
                                onPeekResult(CloudMetadata.Invalid);
                            }
                        });
                    } else {
                        PlatformSaveUtil.LogCloudLoadSaveError(string.Format("GetCloudSavedAccountData: OpenWithAutomaticConflictResolution error! - {0}", status));
                        onPeekResult(CloudMetadata.Invalid);
                    }
                });
        } else {
            PlatformSaveUtil.LogCloudLoadSaveError(string.Format("GetCloudSavedAccountData: savedGameClient null"));
            onPeekResult(CloudMetadata.Invalid);
        }
#endif
    }

    public void ExecuteCloudLoad() {
#if !NO_GPGS
        PlatformSaveUtil.ShowLoadProgressPopup();

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                OnSavedGameOpenedAndRead);
        } else {
            // handle error
            PlatformSaveUtil.ShowLoadErrorPopup("OnClick_cloudSave: savedGameClient null");
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadFailure, 0, 2);
        }
#endif
    }

    public void ExecuteCloudSave() {
#if !NO_GPGS
        SaveLoadManager.Save(BalloonSpawner.instance, ConfigPopup.instance, Sound.instance, Data.instance, SaveLoadManager.SaveReason.BeforeCloudSave);
        PlatformSaveUtil.ShowSaveProgressPopup();

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndWriteConflictResolve,
                OnSavedGameOpenedAndWrite);
        } else {
            PlatformSaveUtil.ShowSaveErrorPopup("OnClick_cloudSave: savedGameClient null");
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveFailure, 0, 1);
        }
#endif
    }

    public void Login(System.Action<bool, string> onAuthResult) {
        Social.localUser.Authenticate((b, reason) => {
            // 구글 로그인 성공/실패 유무에 따른 플래그 업데이트
            PlayerPrefs.SetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, b ? 0 : 1);
            PlayerPrefs.Save();
            onAuthResult(b, reason);
        });
    }

    public void Logout() {
        throw new System.NotImplementedException();
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("ReportBugByMailSaveFileOnUiThread", reportPopupTitle, mailTo, subject, text, saveData);
        } else {
            Debug.LogErrorFormat("ReportBugByMailSaveFileOnUiThread: AndroidJavaClass name {0} not found!", SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        }
#endif
    }

    public void ShareScreenshot(byte[] pngData) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SharePngByteArrayOnUiThread", pngData);
        } else {
            Debug.LogErrorFormat("SharePngByteArrayOnUiThread: AndroidJavaClass name {0} not found!", SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        }
#endif
    }

    private void OnSavedGameOpenedAndWriteConflictResolve(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData) {
        resolver.ChooseMetadata(unmerged);
    }

    private void OnSavedGameOpenedAndReadConflictResolve(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData) {
        resolver.ChooseMetadata(original);
    }

    public void OnSavedGameOpenedAndWrite(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.

            SushiDebug.LogFormat("OnSavedGameOpenedAndWrite: Save game open (write) success! Filename: {0}", game.Filename);

            SerializeAndSaveGame(game);
        } else {
            // handle error
            PlatformSaveUtil.ShowSaveErrorPopup(string.Format("OnSavedGameOpenedAndWrite: Save game open (write) failed! - {0}", status));
            //rootCanvasGroup.interactable = true;
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveFailure, 0, 2);
        }
    }

    private void SerializeAndSaveGame(ISavedGameMetadata game) {
        var savedData = PlatformSaveUtil.SerializeSaveData();
        var played = System.TimeSpan.FromSeconds(BalloonSpawner.instance.playTimeSec);// System.TimeSpan.Zero;//NetworkTime.GetNetworkTime() - NetworkTime.BaseDateTime;
        SaveGame(game, savedData, played);
    }

    void SaveGame(ISavedGameMetadata game, byte[] savedData, System.TimeSpan totalPlaytime) {
#if !NO_GPGS
        var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(savedData);
        var accountLevel = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_KEY);
        var accountLevelExp = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_EXP_KEY);
        var accountGem = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_GEM_KEY);

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedDescription(string.Format("Level {0} / Exp {1} / Gem {2}", accountLevel, accountLevelExp, accountGem));
        builder = builder.WithUpdatedPlayedTime(totalPlaytime);
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
#endif
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.
            PlatformSaveUtil.ShowSaveResultPopup();
        } else {
            // handle error
            PlatformSaveUtil.ShowSaveErrorPopup(string.Format("OnSavedGameWritten: OnSavedGameWritten failed! - {0}", status));
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveFailure, 0, 3);
        }

        //rootCanvasGroup.interactable = true;
    }

    public void OnSavedGameOpenedAndRead(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.

            SushiDebug.LogFormat("Save game open (read) success! Filename: {0}", game.Filename);

            LoadGameData(game);
        } else {
            // handle error
            PlatformSaveUtil.ShowLoadErrorPopup("OnSavedGameOpenedAndRead: status != SavedGameRequestStatus.Success");
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadFailure, 0, 3);
        }
    }
    public void LoadGameData(ISavedGameMetadata game) {
#if !NO_GPGS
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
#endif
    }
    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data) {
        if (status == SavedGameRequestStatus.Success) {
            // handle processing the byte array data
            SushiDebug.LogFormat("OnSavedGameDataRead success! - Data size: {0} bytes", data.Length);

            var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(data);

            PlatformSaveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
        } else {
            // handle error
            PlatformSaveUtil.ShowLoadErrorPopup("OnSavedGameDataRead: status == SavedGameRequestStatus.Success");
            BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadFailure, 0, 4);
        }
    }

    public void PreAuthenticate() {
#if !NO_GPGS
        var config = new PlayGamesClientConfiguration.Builder()
                    .EnableSavedGames()
                    .Build();

        PlayGamesPlatform.InitializeInstance(config);
        //PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
    }

    public bool LoginFailedLastTime() {
        return PlayerPrefs.GetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, 0) != 0;
    }

    static int SendNotification(System.TimeSpan delay, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
        int id = new System.Random().Next();
        return SendNotification(id, (int)delay.TotalSeconds * 1000, title, message, bgColor, sound, vibrate, lights, bigIcon, smallIcon);
    }

    static int SendNotification(int id, System.TimeSpan delay, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
        return SendNotification(id, (int)delay.TotalSeconds * 1000, title, message, bgColor, sound, vibrate, lights, bigIcon, smallIcon);
    }

    static int SendNotification(int id, long delayMs, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SetNotification", id, delayMs, title, message, message,
                sound ? 1 : 0, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, smallIcon,
        bgColor.r * 65536 + bgColor.g * 256 + bgColor.b, Application.identifier);
        } else {
            Debug.LogErrorFormat("SendNotification: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return id;
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon) {
        SendNotification(
            System.TimeSpan.FromMilliseconds(afterMs),
            title,
            body,
            new Color32(0x7f, 0x7f, 0x7f, 255),
            true,
            true,
            true,
            largeIcon,
            "icon1024_2_gray");
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon) {
        ClearAllNotifications();

        SetRepeatingNotificationAtMillis(
            (int)RepeatingLocalNotificationId.AT_0900,
            GetNextHourOfDayInMillis(9), // 09:00
            GetDayInterval(),
            title,
            body,
            new Color32(239, 58, 38, 255),
            true,
            true,
            true,
            largeIcon,
            "icon1024_2_gray");
            
        SushiDebug.Log("RegisterAllRepeatingNotifications");
    }

    enum RepeatingLocalNotificationId {
        AT_0900 = 1,
        AT_1200 = 2,
        AT_1800 = 3,
        AT_0000_TEST = 4,
    }

    public void ClearAllNotifications() {
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_0900);
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_1200);
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_1800);

        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_0900);
        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_1200);
        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_1800);
    }

    public void OnCloudSaveResult(string result) {
        throw new System.NotImplementedException();
    }

    public void OnCloudLoadResult(string result, byte[] data) {
        throw new System.NotImplementedException();
    }

    static void CancelPendingNotification(int id) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("CancelPendingNotification", id);
        }
#endif
    }

    static void ClearAlreadyNotified(int id) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("ClearAlreadyNotified", id);
        }
#endif
    }

    static int SetRepeatingNotificationAtMillis(int id, long atMillis, long timeoutMs, string title, string message, Color32 bgColor, bool sound, bool vibrate, bool lights, string bigIcon, string smallIcon) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SetRepeatingNotificationAtMillis", id, atMillis, title, message, message, timeoutMs,
                sound ? 1 : 0, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, smallIcon,
                bgColor.r * 65536 + bgColor.g * 256 + bgColor.b, Application.identifier);
        }
        return id;
#else
        return 0;
#endif
    }

    static long GetNextHourOfDayInMillis(int hourOfDay) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            return pluginClass.CallStatic<long>("GetNextHourOfDayInMillis", hourOfDay);
        } else {
            Debug.LogErrorFormat("GetNextHourOfDayInMillis: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return 0;
    }

    static long GetDayInterval() {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            return pluginClass.CallStatic<long>("GetDayInterval");
        } else {
            Debug.LogErrorFormat("GetDayInterval: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return 0;
    }

    public void RequestUserReview() {
        Application.OpenURL("market://details?id=top.plusalpha.balloon");
    }

    public string GetAccountTypeText() {
        return TextHelper.GetText("platform_account_google");
    }
}
