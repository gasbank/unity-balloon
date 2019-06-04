using System.IO;
using UnityEngine;
using UnityEngine.Events;
using RemoteSaveDictionary = System.Collections.Generic.Dictionary<string, byte[]>;

public class PlatformEditor : IPlatformBase {
    static string RemoteSaveFileForEditor { get { return Application.persistentDataPath + "/" + PlatformSaveUtil.remoteSaveFileName; } }

    public bool CheckLoadSavePrecondition(string progressMessage, UnityAction onNotLoggedIn, UnityAction onAbort) {
        if (string.IsNullOrEmpty(progressMessage) == false) {
            ProgressMessage.instance.Open(progressMessage);
        }
        return true;
    }

    public void ExecuteCloudLoad() {
        PlatformSaveUtil.ShowLoadProgressPopup();

        var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(File.ReadAllBytes(RemoteSaveFileForEditor));

        PlatformSaveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
    }

    public void ExecuteCloudSave() {
        SaveLoadManager.Save(BalloonSpawner.instance, ConfigPopup.instance, Sound.instance, Data.instance, SaveLoadManager.SaveReason.BeforeCloudSave);
        PlatformSaveUtil.ShowSaveProgressPopup();

        var savedData = PlatformSaveUtil.SerializeSaveData();

        using (var f = File.Create(RemoteSaveFileForEditor)) {
            f.Write(savedData, 0, savedData.Length);
        }

        var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(savedData);
        ShowSaveResultPopup(savedData, remoteSaveDict, RemoteSaveFileForEditor);
    }

    public void Login(System.Action<bool, string> onAuthResult) {
        onAuthResult(false, "Not supported platform");
    }

    public void Logout() {
        throw new System.NotImplementedException();
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData) {
        var str = string.Format("버그 메일을 보냅니다. 수신자: {0}, 제목: {1}, 본문: {2}, 세이브데이터크기: {3} bytes", mailTo, subject, text, saveData.Length);
        ConfirmPopup.instance.Open(str);
    }

    public void ShareScreenshot(byte[] pngData) {
        SushiDebug.LogFormat("ShareScreenshot: pngData length {0} bytes", pngData.Length);
        var screenshotFileName = "screenshot.png";
        File.WriteAllBytes(screenshotFileName, pngData);
        SushiDebug.Log($"ShareScreenshot: successfully written to {screenshotFileName}");
    }

    public void GetCloudLastSavedMetadataAsync(System.Action<CloudMetadata> onPeekResult) {
        PlatformSaveUtil.ShowPeekProgressPopup();

        if (File.Exists(RemoteSaveFileForEditor)) {
            var remoteSaveDict = PlatformSaveUtil.DeserializeSaveData(File.ReadAllBytes(RemoteSaveFileForEditor));
            var cloudMetadata = new CloudMetadata {
                level = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_KEY),
                levelExp = PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_EXP_KEY),
                gem = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_GEM_KEY),
                riceRate = PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_RICE_RATE_KEY),
                saveDate = PlatformSaveUtil.GetInt64FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.SAVE_DATE_KEY),
            };
            onPeekResult(cloudMetadata);
        } else {
            onPeekResult(CloudMetadata.Invalid);
        }
    }

    private void ShowSaveResultPopup(byte[] savedData, RemoteSaveDictionary remoteSaveDict, string path) {
        ProgressMessage.instance.Close();
        var text = string.Format("세이브 완료 - age: {0} sec, size: {1} bytes, accountLevel = {2}, accountLevelExp = {3}, accountGem = {4}, savedDate = {5}, path = {6}",
                        TimeChecker.instance.GetLastSavedTimeTotalSeconds(),
                        savedData.Length,
                        PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_KEY),
                        PlatformSaveUtil.GetInt32FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_LEVEL_EXP_KEY),
                        PlatformSaveUtil.GetBigIntegerFromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.ACCOUNT_GEM_KEY),
                        PlatformSaveUtil.GetInt64FromRemoteSaveDict(remoteSaveDict, PlatformSaveUtil.SAVE_DATE_KEY),
                        path
                        );
        ConfirmPopup.instance.Open(text);
    }

    public void PreAuthenticate() {
    }

    public bool LoginFailedLastTime() {
        return true;
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon) {
        //SushiDebug.LogFormat("RegisterAllNotifications: title={0}, body={1}, largeIcon={2}", title, body, largeIcon);
    }

    public void ClearAllNotifications() {
        //SushiDebug.LogFormat("ClearAllNotifications");
    }

    public void OnCloudSaveResult(string result) {
        throw new System.NotImplementedException();
    }

    public void OnCloudLoadResult(string result, byte[] data) {
        throw new System.NotImplementedException();
    }

    public void RequestUserReview() {
        Application.OpenURL("https://daum.net");
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon) {
        SushiDebug.LogFormat("RegisterSingleNotification: title={0}, body={1}, afterMs={2}", title, body, afterMs);
    }

    public string GetAccountTypeText() {
        return TextHelper.GetText("platform_account_editor");
    }
}
