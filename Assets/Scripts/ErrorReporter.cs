using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using Random = System.Random;

public class ErrorReporter : MonoBehaviour
{
    public static ErrorReporter instance;

    // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
    static readonly Random random = new Random();
    internal static readonly string ERROR_DEVICE_ID_KEY = "errorDeviceId";

    public static string RandomString(int length)
    {
        const string chars = "ACEFGHJKMNQRTUVWXY346"; // L, I, 1, 7, D, 0, O, 2, Z, P, 9, B, 8, S, 5 등 혼란을 주는 글씨는 뺐다.
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string NewErrorPseudoId()
    {
        return string.Format("E{0}-{1}", RandomString(3), RandomString(3));
    }

    public string GetOrCreateErrorDeviceId()
    {
        // PlayerPrefs에서 기기 고유 ID를 가져온다. (게임 지우면 리셋됨. 없으면 생성)
        var errorDeviceId = PlayerPrefs.GetString(ERROR_DEVICE_ID_KEY, NewErrorPseudoId());
        // 생성됐을 수도 있으니까 한번 저장
        PlayerPrefs.SetString(ERROR_DEVICE_ID_KEY, errorDeviceId);
        PlayerPrefs.Save();
        return errorDeviceId;
    }

    public async Task UploadSaveFileIncidentAsync(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        var uploadSaveFileDbUrl = ConfigPopup.BaseUrl + "/save";
        ProgressMessage.instance.Open("\\저장 파일 문제 업로드 중...".Localized());

        var errorDeviceId = GetOrCreateErrorDeviceId();
        var url = string.Format("{0}/{1}", uploadSaveFileDbUrl, errorDeviceId);
        var saveFile = new ErrorFile();
        var uploadDate = DateTime.UtcNow;
        try
        {
            saveFile.fields.uploadDate.timestampValue = uploadDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        catch
        {
        }

        try
        {
            saveFile.fields.stackTraceMessage.stringValue = string.Join("///", exceptionList.Select(e => e.ToString()));
        }
        catch
        {
        }

        try
        {
            saveFile.fields.appMetaInfo.stringValue = ConfigPopup.instance.GetAppMetaInfo();
        }
        catch
        {
        }

        try
        {
            saveFile.fields.userId.stringValue = ConfigPopup.instance.GetUserId();
        }
        catch
        {
        }

        try
        {
            try
            {
                saveFile.fields.saveData.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData1.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData1.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData2.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData2.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData3.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData3.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData4.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData4.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData5.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData5.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData6.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData6.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
            try
            {
                saveFile.fields.saveData7.bytesValue =
                    Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
            }
            catch
            {
                saveFile.fields.saveData7.bytesValue = "";
            }

            SaveLoadManager.DecreaseSaveDataSlotAndWrite();
        }
        catch (Exception e)
        {
            // 문제가 있는 파일을 업로드하는 것 조차 실패했다. 이건 수가 없네...
            ProgressMessage.instance.Close();
            ConfirmPopup.instance.Open(string.Format("SAVE FILE UPLOAD FAILED: {0}", e), () =>
            {
                if (notCriticalError == false)
                    Application.Quit();
                else
                    ConfirmPopup.instance.Close();
            });
            return;
        }

        try
        {
            using (var httpClient = new HttpClient())
            {
                var patchData = JsonUtility.ToJson(saveFile);
                using (var patchContent = new StringContent(patchData))
                {
                    BalloonDebug.Log($"HttpClient PATCH TO {url}...");

                    // PATCH 시작하고 기다린다.
                    var patchTask = await httpClient.PatchAsync(new Uri(url), patchContent);

                    BalloonDebug.Log($"HttpClient Result: {patchTask.ReasonPhrase}");

                    if (patchTask.IsSuccessStatusCode)
                    {
                        var msg = string.Format(
                            "\\업로드가 성공적으로 완료됐습니다.\\n\\n업로드 코드: {0}\\n용량: {1:n0}바이트\\nTS: {2}\\n\\n<color=brown>본 화면의 스크린샷을 찍어 공식 카페에 버그 신고를 부탁 드립니다.</color>\\n\\n업로드된 데이터를 분석 후, 카페를 통해 이후 진행을 안내드리겠습니다.\\n\\n공식 카페로 이동하거나, 안내 받은 복구 코드를 입력하세요."
                                .Localized(), errorDeviceId, patchData.Length,
                            saveFile.fields.uploadDate.timestampValue);
                        if (notCriticalError == false)
                            ConfirmPopup.instance.OpenTwoButtonPopup(msg, () => ConfigPopup.instance.OpenCommunity(),
                                () => SaveLoadManager.EnterRecoveryCode(exceptionList, st, notCriticalError),
                                "\\업로드 완료".Localized(), "\\공식 카페 이동".Localized(), "\\복구 코드 입력".Localized());
                        else
                            ConfirmPopup.instance.OpenTwoButtonPopup(msg, () => ConfirmPopup.instance.Close(),
                                () => SaveLoadManager.EnterRecoveryCode(exceptionList, st, notCriticalError),
                                "\\업로드 완료".Localized(), "\\닫기".Localized(), "\\복구 코드 입력".Localized());
                    }
                    else
                    {
                        ShortMessage.instance.Show(string.Format("{0}", patchTask.ReasonPhrase));
                        if (notCriticalError == false) // 다시 안내 팝업 보여주도록 한다.
                            SaveLoadManager.ProcessCriticalLoadError(exceptionList, st);
                        else
                            ConfirmPopup.instance.Open(string.Format("SAVE FILE UPLOAD FAILED: {0}",
                                patchTask.ReasonPhrase));
                    }
                }
            }
        }
        catch (Exception e)
        {
            ConfirmPopup.instance.Open(e.Message, () =>
            {
                if (notCriticalError == false) // 다시 안내 팝업 보여주도록 한다.
                    SaveLoadManager.ProcessCriticalLoadError(exceptionList, st);
                else
                    ConfirmPopup.instance.Close();
            });
        }
        finally
        {
            // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
            ProgressMessage.instance.Close();
        }
    }

    internal void ProcessRecoveryCode(List<Exception> exceptionList, string st, string recoveryCode)
    {
        StartCoroutine(ProcessRecoveryCodeCoro(exceptionList, st, recoveryCode));
    }

    public IEnumerator ProcessRecoveryCodeCoro(List<Exception> exceptionList, string st, string recoveryCode)
    {
        var recoveryDbUrl = ConfigPopup.BaseUrl + "/recovery";
        ProgressMessage.instance.Open("\\복구 코드 확인중...".Localized());
        recoveryCode = recoveryCode.Trim();
        // 복구 코드를 특별히 입력하지 않았을 경우에는 Error Device ID가 기본으로 쓰인다.
        if (string.IsNullOrEmpty(recoveryCode))
        {
            recoveryCode = GetOrCreateErrorDeviceId();
        }
        else if (recoveryCode == "deleteall")
        {
            // 파일 삭제하고 새 게임 시작하는 개발자용 복구 코드
            SaveLoadManager.DeleteSaveFileAndReloadScene();
            yield break;
        }

        var url = string.Format("{0}/{1}", recoveryDbUrl, recoveryCode);
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            ProgressMessage.instance.Close();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                ShortMessage.instance.ShowLocalized("\\복구 정보 수신에 실패했습니다.".Localized());
            }
            else
            {
                try
                {
                    //BalloonDebug.LogFormat("URL Text: {0}", request.downloadHandler.text);
                    var recoveryDataRoot = Json.Deserialize(request.downloadHandler.text) as Dict;
                    foreach (var kv in recoveryDataRoot)
                    {
                        //BalloonDebug.LogFormat("root key: {0}", kv.Key);
                    }

                    var recoveryData = recoveryDataRoot["fields"] as Dictionary<string, object>;
                    foreach (var kv in recoveryData)
                    {
                        //BalloonDebug.LogFormat("fields key: {0}", kv.Key);
                    }

                    //BalloonDebug.LogFormat("serviceData = {0}", serviceData);
                    foreach (var recovery in recoveryData)
                    {
                        var recoveryIndex = 0;
                        var recoveryIndexParsed = int.TryParse(recovery.Key, out recoveryIndex);
                        // 이미 받았거나 이상한 항목은 스킵
                        if (recoveryIndexParsed == false) continue;
                        var fields = (Dict) ((Dict) ((Dict) recovery.Value)["mapValue"])["fields"];
                        var isValidErrorDeviceId = false;
                        var saveDataBase64 = "";
                        byte[] saveData = null;
                        var recoveryErrorDeviceId = "";
                        //var serviceValue = service.Value as 
                        foreach (var recoveryItem in fields)
                            if (recoveryItem.Key == "errorDeviceId")
                            {
                                recoveryErrorDeviceId = ((Dict) recoveryItem.Value)["stringValue"] as string;
                                if (recoveryErrorDeviceId == GetOrCreateErrorDeviceId()) isValidErrorDeviceId = true;
                            }
                            else if (recoveryItem.Key == "saveData")
                            {
                                saveDataBase64 = ((Dict) recoveryItem.Value)["stringValue"] as string;
                                saveData = Convert.FromBase64String(saveDataBase64);
                            }

                        BalloonDebug.LogFormat("Error Device ID: {0}", GetOrCreateErrorDeviceId());
                        BalloonDebug.LogFormat("Recovery Error Device ID: {0}", recoveryErrorDeviceId);
                        BalloonDebug.LogFormat("Save Data Base64 ({0} bytes): {1}",
                            saveDataBase64 != null ? saveDataBase64.Length : 0, saveDataBase64);

                        if (isValidErrorDeviceId && saveData != null && saveData.Length > 0)
                        {
                            // 복구 성공!!
                            // 새로운 세이브 파일 쓰고, 다시 Splash 신 로드
                            BalloonDebug.LogFormat("Writing recovery save data {0} bytes", saveData.Length);
                            File.WriteAllBytes(SaveLoadManager.SaveFileName, saveData);
                            // 일반적인 저장 경로가 아니고 파일을 직접 만들어낸 것이라서 수동으로 저장 슬롯 인덱스 증가시켜 줘야
                            // 다음에 직전에 저장한 슬롯의 저장 데이터를 불러온다.
                            SaveLoadManager.IncreaseSaveDataSlotAndWrite();
                            SceneManager.LoadScene("Splash");
                            break;
                        }
                    }
                }
                catch
                {
                    // 딱히 할 수 있는 게 없다
                }

                // 여기까지 왔으면 복구가 제대로 안됐다는 뜻이다.
                ConfirmPopup.instance.Open(
                    string.Format("\\복구 코드가 잘못됐거나, 복구 데이터가 존재하지 않습니다.\\n\\n확인을 눌러 처음 화면으로 돌아갑니다.".Localized()),
                    () => SaveLoadManager.ProcessCriticalLoadError(exceptionList, st));
            }
        }
    }

    // 유저가 응급 제출한 오류가 있는 세이브 파일을 개발자가 재현해 볼 때 사용한다.
    internal void ProcessUserSaveCode(string userSaveCode)
    {
        StartCoroutine(ProcessUserSaveCodeCoro(userSaveCode));
    }

    public IEnumerator ProcessUserSaveCodeCoro(string userSaveCode)
    {
        var saveDbUrl = ConfigPopup.BaseUrl + "/save";
        ProgressMessage.instance.Open("\\유저 세이브 코드 확인중...".Localized());
        userSaveCode = userSaveCode.Trim();
        var url = string.Format("{0}/{1}", saveDbUrl, userSaveCode);
        BalloonDebug.LogFormat("URL: {0}", url);
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            ProgressMessage.instance.Close();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                ShortMessage.instance.ShowLocalized("\\복구 정보 수신에 실패했습니다.");
            }
            else
            {
                try
                {
                    var userSaveDataRoot = Json.Deserialize(request.downloadHandler.text) as Dict;
                    var userSaveDataFields = userSaveDataRoot["fields"] as Dict;
                    // userSaveDataFields는 정렬되어 있지 않다. saveData, saveData2, saveData3, ... 순으로
                    // 로드 시도하기 위해서 필터링 및 정렬한다.
                    foreach (var fieldName in userSaveDataFields.Keys.Where(e => e.StartsWith("saveData"))
                        .OrderBy(e => e))
                    {
                        BalloonDebug.Log($"Checking save data field name '{fieldName}'...");
                        var userSaveDataFieldsSaveData = userSaveDataFields[fieldName] as Dict;
                        if (userSaveDataFieldsSaveData.Keys.Count > 0)
                        {
                            var userSaveDataFieldsSaveDataStringValue =
                                (userSaveDataFieldsSaveData.ContainsKey("bytesValue")
                                    ? userSaveDataFieldsSaveData["bytesValue"]
                                    : userSaveDataFieldsSaveData["stringValue"]) as string;

                            var saveDataBase64 = userSaveDataFieldsSaveDataStringValue;
                            var saveData = Convert.FromBase64String(saveDataBase64);

                            BalloonDebug.LogFormat("Save Data Base64 ({0} bytes): {1}",
                                saveDataBase64 != null ? saveDataBase64.Length : 0, saveDataBase64);

                            if (saveData.Length > 0)
                            {
                                BalloonDebug.LogFormat("Writing recovery save data {0} bytes", saveData.Length);
                                File.WriteAllBytes(SaveLoadManager.SaveFileName, saveData);
                                // 일반적인 저장 경로가 아니고 파일을 직접 만들어낸 것이라서 수동으로 저장 슬롯 인덱스 증가시켜 줘야
                                // 다음에 직전에 저장한 슬롯의 저장 데이터를 불러온다.
                                SaveLoadManager.IncreaseSaveDataSlotAndWrite();
                                SceneManager.LoadScene("Splash");
                                yield break;
                            }

                            BalloonDebug.Log($"Save data field name '{fieldName}' is empty!");
                        }
                        else
                        {
                            BalloonDebug.Log($"Save data field name '{fieldName}' is empty!");
                        }
                    }
                }
                catch (Exception e)
                {
                    // 딱히 할 수 있는 게 없다
                    Debug.LogException(e);
                }

                // 여기까지 왔으면 복구가 제대로 안됐다는 뜻이다.
                ConfirmPopup.instance.Open(
                    string.Format("\\유저 세이브 코드가 잘못됐거나, 복구 데이터가 존재하지 않습니다.\\n\\n확인을 눌러 처음 화면으로 돌아갑니다.".Localized()));
            }
        }
    }

    [Serializable]
    class ErrorFile
    {
        public Fields fields = new Fields();

        [Serializable]
        public class Fields
        {
            public StringValueData appMetaInfo = new StringValueData();
            public BytesValueData saveData = new BytesValueData();
            public BytesValueData saveData1 = new BytesValueData();
            public BytesValueData saveData2 = new BytesValueData();
            public BytesValueData saveData3 = new BytesValueData();
            public BytesValueData saveData4 = new BytesValueData();
            public BytesValueData saveData5 = new BytesValueData();
            public BytesValueData saveData6 = new BytesValueData();
            public BytesValueData saveData7 = new BytesValueData();
            public StringValueData stackTraceMessage = new StringValueData();
            public TimestampValueData uploadDate = new TimestampValueData();
            public StringValueData userId = new StringValueData();

            [Serializable]
            public class BytesValueData
            {
                public string bytesValue;
            }

            [Serializable]
            public class TimestampValueData
            {
                public string timestampValue;
            }

            [Serializable]
            public class StringValueData
            {
                public string stringValue;
            }
        }
    }
}