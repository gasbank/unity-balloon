using System;
using System.Collections;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
#if !NO_GPGS
using GooglePlayGames.BasicApi.SavedGame;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
using RemoteSaveDictionary = System.Collections.Generic.Dictionary<string, byte[]>;

public class PlatformSaveUtil
{
    public static readonly string remoteSaveFileName = "default-remote-save";
    public static readonly string ACCOUNT_LEVEL_KEY = "__accountLevel";
    public static readonly string ACCOUNT_LEVEL_EXP_KEY = "__accountLevelExp";
    public static readonly string ACCOUNT_GEM_KEY = "__accountGem";
    public static readonly string ACCOUNT_RICE_RATE_KEY = "__accountRiceRate";
    public static readonly string SAVE_DATE_KEY = "__saveDate";

    static readonly string firebaseBaseUrl = "https://xxxxxxxxxxx-xxxxx.firebaseio.com";

    //private static readonly string couponUrlFormat = firebaseBaseUrl + "/coupons/{0}.json";
    public static readonly string noticeUrlFormat = firebaseBaseUrl + "/notice.json";
    public static readonly string saveUrlFormat = "https://xxxxxxxxxx-xxxxxxx.firebaseio.com/saves/{0}.json";
    public static string RemoteSaveFileForEditor => Application.persistentDataPath + "/" + remoteSaveFileName;

    public static byte[] SerializeSaveData()
    {
        var dict = new RemoteSaveDictionary();
        // 직전 단계에서 SaveLoadManager.SaveFileName에다가 썼다.
        // 쓰면서 save slot을 1 증가시켰으니까, 직전에 쓴 파일을 읽어오고 싶으면
        // SaveLoadManager.LoadFileName을 써야 한다.
        // 저장하는 키는 'save.dat'로 고정이다. (하위호환성)
        var localSaveFileName = SaveLoadManager.GetSaveLoadFileNameOnly(0);
        BalloonDebug.Log($"Saving '{SaveLoadManager.LoadFileName}' to a dict key '{localSaveFileName}'");
        dict[localSaveFileName] = File.ReadAllBytes(SaveLoadManager.LoadFileName);
        dict[ACCOUNT_LEVEL_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountLevel);
        dict[ACCOUNT_LEVEL_EXP_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountLevelExp);
        dict[ACCOUNT_GEM_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountGem);
        dict[ACCOUNT_RICE_RATE_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountRiceRate);
        dict[SAVE_DATE_KEY] = BitConverter.GetBytes(DateTime.Now.Ticks);
        var binFormatter = new BinaryFormatter();
        var memStream = new MemoryStream();
        binFormatter.Serialize(memStream, dict);
        return memStream.ToArray();
    }

    public static RemoteSaveDictionary DeserializeSaveData(byte[] byteArr)
    {
        var memStream = new MemoryStream();
        var binFormatter = new BinaryFormatter();
        memStream.Write(byteArr, 0, byteArr.Length);
        memStream.Position = 0;
        return binFormatter.Deserialize(memStream) as RemoteSaveDictionary;
    }

#if !NO_GPGS
    static void OnSavedGameOpenedAndWriteConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(unmerged);
    }

    static void OnSavedGameOpenedAndReadConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(original);
    }
#endif

    public static void LogCloudLoadSaveError(string message)
    {
        Debug.LogError(message);
    }

    public static void CancelStartLoginForSave()
    {
        ConfirmPopup.instance.Open(TextHelper.GetText("platform_save_cancelled_popup"));
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveFailure, 0, 0);
    }

    public static void ShowSaveProgressPopup()
    {
        ProgressMessage.instance.Open(TextHelper.GetText("platform_saving"));
    }

    public static void ShowLoadProgressPopup()
    {
        ProgressMessage.instance.Open(TextHelper.GetText("platform_loading"));
    }

    public static void ShowSaveErrorPopup(string text)
    {
        LogCloudLoadSaveError(text);

        ProgressMessage.instance.Close();
        ConfirmPopup.instance.Open(text);
    }

    public static void LoadDataAndLoadSplashScene(RemoteSaveDictionary dict)
    {
        // 모든 저장 파일을 지운다.
        SaveLoadManager.DeleteAllSaveFiles();
        // 그 다음 쓴다.
        foreach (var fileName in dict)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName.Key);
            BalloonDebug.Log(
                $"LoadDataAndLoadSplashScene: gd key = {fileName.Key}, length = {fileName.Value.Length:n0}, writing to = {filePath}");
            File.WriteAllBytes(filePath, fileName.Value);
        }

        BalloonDebug.Log("LoadCloudDataAndSave");
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadEnd, 0, 0);
        SceneManager.LoadScene("Splash");
    }

    public static void CancelStartLoginForLoad()
    {
        ConfirmPopup.instance.Open(TextHelper.GetText("platform_load_cancelled_popup"));
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadFailure, 0, 0);
    }

    public static void NoDataToLoad()
    {
        ConfirmPopup.instance.Open(TextHelper.GetText("platform_cloud_load_fail"));
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudLoadFailure, 0, 1);
    }

    public static void ShowLoadErrorPopup(string text)
    {
        LogCloudLoadSaveError(text);

        ProgressMessage.instance.Close();
        ConfirmPopup.instance.Open(text);
    }

    public static void ShowPeekProgressPopup()
    {
        ProgressMessage.instance.Open(TextHelper.GetText("platform_check_last_saved"));
    }

    public static void StartLoginAndDoSomething(Action something)
    {
        // 유저가 직접 로그인 시도한 것이기 때문에 과거의 로그인 실패 여부는 따지지 않는다.
        ProgressMessage.instance.Open(TextHelper.GetText("platform_logging_in"));
        Platform.StartAuthAsync((b, reason) =>
        {
            ProgressMessage.instance.Close();
            if (b)
            {
                something();
            }
            else
            {
                Debug.LogErrorFormat("Login failed - reason: {0}", reason);

                ConfirmPopup.instance.Open(Platform.GetText("platform_login_failed_popup") + "\n\n" + reason);
            }
        });
    }

    public static void StartLoginAndLoad()
    {
        StartLoginAndDoSomething(Platform.CloudLoad);
    }

    public static void StartLoginAndSave()
    {
        // 유저가 직접 로그인 시도한 것이기 때문에 과거의 로그인 실패 여부는 따지지 않는다.
        ProgressMessage.instance.Open(TextHelper.GetText("platform_logging_in"));
        Platform.StartAuthAsync((b, reason) =>
        {
            ProgressMessage.instance.Close();
            if (b)
            {
                Platform.CloudSave();
            }
            else
            {
                Debug.LogErrorFormat("Login failed - reason: {0}", reason);

                ConfirmPopup.instance.Open(Platform.GetText("platform_login_failed_popup") + "\n\n" + reason);
            }
        });
    }

    public static void ShowSaveResultPopup()
    {
        ProgressMessage.instance.Close();
        ConfirmPopup.instance.Open(TextHelper.GetText("platform_saved_popup"));
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCloudSaveEnd, 0, 0);
    }

    public static IEnumerator ReportCorruptSaveFileSendCoroutine(string guid, string reportJsonStr)
    {
        using (var request = UnityWebRequest.Put(string.Format(saveUrlFormat, guid), reportJsonStr))
        {
            yield return request.SendWebRequest();
        }

        BalloonDebug.LogFormat("ReportCorruptSaveFile report finished. GUID {0}", guid);
    }

    public static int GetInt32FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key)
    {
        byte[] b;
        if (remoteSaveDict.TryGetValue(key, out b))
            return BitConverter.ToInt32(remoteSaveDict[key], 0);
        return -1;
    }

    public static long GetInt64FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key)
    {
        byte[] b;
        if (remoteSaveDict.TryGetValue(key, out b))
            return BitConverter.ToInt64(remoteSaveDict[key], 0);
        return -1;
    }

    public static BigInteger GetBigIntegerFromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key)
    {
        byte[] b;
        if (remoteSaveDict.TryGetValue(key, out b))
            return new BigInteger(remoteSaveDict[key]);
        return -1;
    }
}