using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class SaveLoadManager
{
    public enum SaveReason
    {
        Quit,
        Pause,
        BeforeCloudSave
    }

    public static readonly string localSaveFileName = "save.dat";

    // 총 maxSaveDataSlot개의 저장 슬롯이 있고, 이를 돌려가며 쓴다.
    public static readonly int maxSaveDataSlot = 9;
    static readonly string saveDataSlotKey = "Save Data Slot";
    static readonly int LatestVersion = 2;

    static byte[] lastSaveDataArray;
    public static string SaveFileName => GetSaveLoadFilePathName(GetSaveSlot() + 1);
    public static string LoadFileName => GetSaveLoadFilePathName(GetSaveSlot());

    public static int PositiveMod(int x, int m)
    {
        return (x % m + m) % m;
    }

    static string GetSaveLoadFilePathName(int saveDataSlot)
    {
        return Path.Combine(Application.persistentDataPath, GetSaveLoadFileNameOnly(saveDataSlot));
    }

    public static string GetSaveLoadFileNameOnly(int saveDataSlot)
    {
        saveDataSlot = PositiveMod(saveDataSlot, maxSaveDataSlot);
        // 하위 호환성을 위해 0인 경우 기존 이름을 쓴다.
        return saveDataSlot == 0 ? localSaveFileName : $"save{saveDataSlot}.dat";
    }

    static int GetSaveSlot()
    {
        return PlayerPrefs.GetInt(saveDataSlotKey, 0);
    }

    // 저장 슬롯 증가 (성공적인 저장 후 항상 1씩 증가되어야 함)
    public static void IncreaseSaveDataSlotAndWrite()
    {
        var oldSaveDataSlot = GetSaveSlot();
        var newSaveDataSlot = PositiveMod(oldSaveDataSlot + 1, maxSaveDataSlot);
        BalloonDebug.Log($"Increase save data slot from {oldSaveDataSlot} to {newSaveDataSlot}...");
        PlayerPrefs.SetInt(saveDataSlotKey, newSaveDataSlot);
        PlayerPrefs.Save();
        BalloonDebug.Log($"Increase save data slot from {oldSaveDataSlot} to {newSaveDataSlot}... OKAY");
    }

    // 저장 슬롯 감소 (불러오기 실패 후 항상 1씩 감소되어야 함)
    public static void DecreaseSaveDataSlotAndWrite()
    {
        var oldSaveDataSlot = GetSaveSlot();
        var newSaveDataSlot = PositiveMod(oldSaveDataSlot - 1, maxSaveDataSlot);
        BalloonDebug.Log($"Decrease save data slot from {oldSaveDataSlot} to {newSaveDataSlot}...");
        PlayerPrefs.SetInt(saveDataSlotKey, newSaveDataSlot);
        PlayerPrefs.Save();
        BalloonDebug.Log($"Decrease save data slot from {oldSaveDataSlot} to {newSaveDataSlot}... OKAY");
    }

    static void ResetSaveDataSlotAndWrite()
    {
        lastSaveDataArray = null;
        PlayerPrefs.SetInt(saveDataSlotKey, 0);
        PlayerPrefs.Save();
    }

    internal static void DeleteSaveFileAndReloadScene()
    {
        // From MSDN: If the file to be deleted does not exist, no exception is thrown.
        BalloonDebug.Log("DeleteSaveFileAndReloadScene");
        DeleteAllSaveFiles();

        SceneManager.LoadScene("Splash");
    }

    public static void DeleteAllSaveFiles()
    {
        for (var i = 0; i < maxSaveDataSlot; i++) File.Delete(GetSaveLoadFilePathName(i));
        ResetSaveDataSlotAndWrite();
    }

    public static bool Save(BalloonSpawner spawner, ConfigPopup configPopup, BalloonSound sound, Data data,
        SaveReason sr)
    {
        // 에디터에서 간혹 게임 플레이 시작할 때 Load도 호출되기도 전에 Save가 먼저 호출되기도 한다.
        // (OnApplicationPause 통해서)
        // 실제 기기에서도 이럴 수 있나? 이러면 망인데...
        // 그래서 플래그를 하나 추가한다. 이 플래그는 로드가 제대로 한번 됐을 때 true로 변경된다.
        if (spawner.loadedAtLeastOnce == false)
        {
            Debug.LogWarning(
                "****** Save() called before first Load(). There might be an error during Load(). Save() will be skipped to prevent losing your save data.");
            return false;
        }

        var balloonSaveData2 = new BalloonSaveData2();
        BalloonDebug.LogFormat("Saving...");
        balloonSaveData2.version = LatestVersion;

        return SaveBalloonSaveData2(balloonSaveData2);
    }

    public static bool SaveBalloonSaveData2(BalloonSaveData2 balloonSaveData2)
    {
        var binFormatter = new BinaryFormatter();
        var memStream = new MemoryStream();
        //BalloonDebug.LogFormat("Start Saving JSON Data: {0}", JsonUtility.ToJson(balloonSaveData2));
        binFormatter.Serialize(memStream, balloonSaveData2);
        var saveDataArray = memStream.ToArray();
        BalloonDebug.LogFormat("Saving path: {0}", SaveFileName);
        if (lastSaveDataArray != null && lastSaveDataArray.SequenceEqual(saveDataArray))
            BalloonDebug.LogFormat("Saving skipped since there is no difference made compared to last time saved.");
        else
            try
            {
                // 진짜 쓰자!!
                File.WriteAllBytes(SaveFileName, saveDataArray);

                // 마지막 저장 데이터 갱신
                lastSaveDataArray = saveDataArray;
                BalloonDebug.Log($"{SaveFileName} Saved. (written to disk)");

                // 유저 서비스를 위해 필요할 수도 있으니까 개발 중일 때는 base64 인코딩 버전 세이브 파일도 저장한다.
                // 실서비스 버전에서는 불필요한 기능이다.
                if (Application.isEditor)
                {
                    var base64Path = SaveFileName + ".base64.txt";
                    BalloonDebug.LogFormat("Saving path (base64): {0}", base64Path);
                    File.WriteAllText(base64Path, Convert.ToBase64String(saveDataArray));
                    BalloonDebug.Log($"{base64Path} Saved. (written to disk)");
                }

                IncreaseSaveDataSlotAndWrite();
                BalloonLogManager.Add(BalloonLogEntry.Type.GameSaved, 0, 0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Writing to disk failed!!!");
                ConfirmPopup.instance.Open("Writing to disk failed!!!");
                BalloonLogManager.Add(BalloonLogEntry.Type.GameSaveFailure, 0, 0);
                return false;
            }

        return true;
    }

    public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        if (val.CompareTo(max) > 0) return max;
        return val;
    }

    public static void Load(BalloonSpawner spawner)
    {
        // 모든 세이브 슬롯에 대해 로드를 성공 할 때까지 시도한다.
        var exceptionList = new List<Exception>();

        for (var i = 0; i < maxSaveDataSlot; i++)
            try
            {
                if (LoadInternal(spawner))
                {
                    // 저장 파일 중 하나는 제대로 읽히긴 했다.
                    if (i != 0) // 그런데 한번 이상 실패했다면 에러 메시지는 보여준다.
                        Debug.LogError($"Save data rolled back {i} time(s)...!!!");
                    // 게임은 속행하면 된다. 롤백이 됐건 안됐건 읽긴 읽었다...
                    return;
                }

                // 뭔가 예외 발생에 의한 실패는 아니지만 실패일 수도 있다.
                // 어쩄든 실패긴 실패.
                // 이전 슬롯으로 넘어간다.
                exceptionList.Add(new Exception("Balloon Save Data Load Exception"));
                DecreaseSaveDataSlotAndWrite();
            }
            catch (NotSupportedBalloonSaveDataVersionException e)
            {
                // 지원되지 않는 저장 파일 버전
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
            }
            catch (SaveFileNotFoundException e)
            {
                // 세이브 파일 자체가 없네?
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
            }
            catch (Exception e)
            {
                // 세이브 파일 읽는 도중 알 수 없는 예외 발
                // 무언가 크게 잘못됐어...
                // 큰일이다~~
                // 이전 슬롯으로 넘어간다.
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
                BalloonLogManager.Add(BalloonLogEntry.Type.GameLoadFailure, 0, GetSaveSlot());
            }

        if (exceptionList.All(e => e.GetType() == typeof(SaveFileNotFoundException)))
        {
            // 세이브 파일이 하나도 없다.
            // 신규 유저다~~~ 풍악을 울려라~~~~~
            ProcessNewUser(spawner, exceptionList[0]);
        }
        else if (exceptionList.Any(e => e.GetType() == typeof(NotSupportedBalloonSaveDataVersionException)))
        {
            var exception = (NotSupportedBalloonSaveDataVersionException) exceptionList.FirstOrDefault(e =>
                e.GetType() == typeof(NotSupportedBalloonSaveDataVersionException));
            // 새 버전으로 업그레이드하면 해결되는 문제다.
            ProcessCriticalLoadErrorPrelude(exceptionList);
            ProcessUpdateNeededError(exception.SaveFileVersion);
        }
        else
        {
            // W.T.F.
            Debug.LogError("All save files cannot be loaded....T.T");
            var st = ProcessCriticalLoadErrorPrelude(exceptionList);
            ProcessCriticalLoadError(exceptionList, st);
        }
    }

    static bool LoadInternal(BalloonSpawner spawner)
    {
        var balloonSaveData2 = LoadBalloonSaveData2();

        // 세이브 데이터 자체에 오류가 있는 케이스이다.
        if (balloonSaveData2.version < 1) return false;

        // 최신 버전 데이터로 마이그레이션
        MigrateBalloonSaveData2(balloonSaveData2);

        if (balloonSaveData2.version == LatestVersion)
        {
            // GOOD!
        }
        else if (balloonSaveData2.version > LatestVersion)
        {
            // 저장 파일 버전이 더 높다? 아마도 최신 버전에서 저장한 클라우드 저장 파일을 예전 버전 클라이언트에서 클라우드 불러오기 한 듯
            throw new NotSupportedBalloonSaveDataVersionException(balloonSaveData2.version);
        }
        else
        {
            throw new Exception(
                $"[CRITICAL ERROR] Latest version {LatestVersion} not match save file latest version field {balloonSaveData2.version}!!!");
        }

        //BalloonDebug.LogFormat("Start Loading JSON Data: {0}", JsonUtility.ToJson(balloonSaveData2));


        // 인앱 상품 구매 내역 디버그 정보
        BalloonDebug.Log("=== Purchased Begin ===");
        if (balloonSaveData2.purchasedProductDict != null)
            foreach (var kv in balloonSaveData2.purchasedProductDict)
                BalloonDebug.Log($"PURCHASED (THANK YOU!!!): {kv.Key} = {kv.Value}");
        BalloonDebug.Log("=== Purchased End ===");

        BalloonSpawner.instance.loadedAtLeastOnce = true;
        BalloonLogManager.Add(BalloonLogEntry.Type.GameLoaded, BalloonSpawner.instance.LastBalloonLevel,
            BalloonSpawner.instance.Gem);
        return true;
    }

    //balloonSaveData2.purchasedProductDict = spawner.purchasedProductDict;
    public static void MigrateBalloonSaveData2(BalloonSaveData2 balloonSaveData2)
    {
        if (balloonSaveData2 == null) throw new ArgumentNullException("balloonSaveData2");

        var defaultBalloonSaveData2 = new BalloonSaveData2();

        // Version 1 --> 2
        if (balloonSaveData2.version == 1)
        {
            BalloonDebug.LogFormat("Upgrading save file version from {0} to {1}", balloonSaveData2.version,
                balloonSaveData2.version + 1);
            balloonSaveData2.version++;
        }
    }

    public static BalloonSaveData2 LoadBalloonSaveData2()
    {
        var memStream = new MemoryStream();
        var binFormatter = new BinaryFormatter();
        //binFormatter.Binder = new BalloonSaveDataSerializationBinder();
        BalloonDebug.Log($"Reading the save file {LoadFileName}...");
        try
        {
            var saveDataArray = File.ReadAllBytes(LoadFileName);
            BalloonDebug.Log($"Loaded on memory. ({saveDataArray.Length:n0} bytes)");
            memStream.Write(saveDataArray, 0, saveDataArray.Length);
        }
        catch (FileNotFoundException)
        {
            throw new SaveFileNotFoundException();
        }
        catch (IsolatedStorageException)
        {
            throw new SaveFileNotFoundException();
        }

        // 처음부터 읽어야 한다.
        memStream.Position = 0;
        // 최신 세이브 데이터인걸 먼저 확인 해 본다.
        var balloonSaveData2 = binFormatter.Deserialize(memStream) as BalloonSaveData2;
        return balloonSaveData2;
    }

    static string ProcessCriticalLoadErrorPrelude(List<Exception> exceptionList)
    {
        Debug.LogErrorFormat("Load: Unknown exception thrown: {0}", exceptionList[0]);
        var t = new StackTrace();
        Debug.LogErrorFormat(t.ToString());
        return t.ToString();
    }

    public static void ProcessCriticalLoadError(List<Exception> exceptionList, string st)
    {
        BalloonLogManager.Add(BalloonLogEntry.Type.GameCriticalError, 0, 0);
        ChangeLanguageBySystemLanguage();
        ConfirmPopup.instance.OpenTwoButtonPopup(
            "\\BalloonRider 저장 파일을 불러오는 중 오류가 발생했습니다.\\n더 심각한 데이터 유실을 막기 위해 게임 플레이는 중단됐습니다.\\n문제 해결을 위해서는 게임 진행 상황이 모두 담긴 파일을 업로드해야 합니다.\\n문제 분석 및 수정이 되면 게임을 재개할 수 있습니다.\\n업로드를 진행하시겠습니까?\\n\\n(인터넷 연결이 필요합니다.)"
                .Localized(), () => UploadSaveFileAsync(exceptionList, st, false),
            () => AskAgainToReportSaveData(exceptionList, st), "\\중대한 오류 발생".Localized(), "\\예".Localized(),
            "\\아니요".Localized());
    }

    public static void ProcessUpdateNeededError(int saveFileVersion)
    {
        ChangeLanguageBySystemLanguage();
        ConfirmPopup.instance.Open(
            string.Format(
                @"\BalloonRider 업데이트가 필요합니다.\n\n확인을 눌러 업데이트하세요!\n\n저장 파일 버전: {0}\n클라이언트 지원 버전: {1}".Localized(),
                saveFileVersion, LatestVersion), () =>
            {
                // 앱 상세 페이지로 보낸다.
                Platform.instance.RequestUserReview();
            });
    }

    static void ChangeLanguageBySystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                ConfigPopup.instance.EnableLanguageKo();
                break;
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
                ConfigPopup.instance.EnableLanguageCh();
                break;
            case SystemLanguage.ChineseTraditional:
                ConfigPopup.instance.EnableLanguageTw();
                break;
            case SystemLanguage.Japanese:
                ConfigPopup.instance.EnableLanguageJa();
                break;
            default:
                ConfigPopup.instance.EnableLanguageKo();
                break;
        }
    }

    public static void EnterRecoveryCode(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        ConfirmPopup.instance.OpenInputFieldPopup("\\안내 받은 복구 코드를 입력해 주십시오.".Localized(), () =>
        {
            ConfirmPopup.instance.Close();
            ErrorReporter.instance.ProcessRecoveryCode(exceptionList, st, ConfirmPopup.instance.InputFieldText);
        }, () =>
        {
            if (notCriticalError == false)
                ProcessCriticalLoadError(exceptionList, st);
            else
                ConfirmPopup.instance.Close();
        }, "\\복구 코드".Localized(), ConfirmPopup.Header.Normal, "", "");
    }

    static void AskAgainToReportSaveData(List<Exception> exceptionList, string st)
    {
        ConfirmPopup.instance.OpenTwoButtonPopup(
            @"\진행 상황을 업로드하지 않으면 더이상 게임을 진행할 수 없습니다.\n\n업로드가 불가한 증상의 경우 네이버 공식 카페로 이동하여 해결 방법에 대해 문의해 주시기 바랍니다."
                .Localized(), () =>
            {
                ConfigPopup.instance.OpenCommunity();
                ProcessCriticalLoadError(exceptionList, st);
            }, () => EnterRecoveryCode(exceptionList, st, false), "\\중대한 오류 발생".Localized(), "\\공식 카페 이동".Localized(),
            "\\복구 코드 입력".Localized());
    }

    static async void UploadSaveFileAsync(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        ConfirmPopup.instance.Close();
        await ErrorReporter.instance.UploadSaveFileIncidentAsync(exceptionList, st, notCriticalError);
    }

    static void ProcessNewUser(BalloonSpawner spawner, Exception e)
    {
        BalloonDebug.LogFormat("Load: Save file not found: {0}", e.ToString());
        ResetData(spawner);
        BalloonDebug.Log("Your OS language is " + Application.systemLanguage);
        ChangeLanguageBySystemLanguage();
        ShowFirstInstallWelcomePopup();
        BalloonDebug.Log("loadedAtLeastOnce set to true");
        spawner.loadedAtLeastOnce = true;
    }

    static void ShowFirstInstallWelcomePopup()
    {
    }

    static int NewUserPseudoId()
    {
        return Random.Range(100000, 1000000);
    }

    static void ResetData(BalloonSpawner spawner)
    {
        BalloonLogManager.Add(BalloonLogEntry.Type.GemToZero, 0, 0);

        if (SystemInfo.deviceModel.IndexOf("iPhone") >= 0)
        {
            var screenRatio = 1.0 * Screen.height / (1.0 * Screen.width);
            if (2.1 < screenRatio && screenRatio < 2.2)
                ConfigPopup.instance.IsNotchOn = true;
            else
                ConfigPopup.instance.IsNotchOn = false;
        }
        else
        {
            ConfigPopup.instance.IsNotchOn = false;
        }

        BalloonLogManager.Add(BalloonLogEntry.Type.GameReset, 0, 0);
    }
}