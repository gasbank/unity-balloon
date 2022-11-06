using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DataUpdater : MonoBehaviour
{
    static string BalloonBytesPath => Path.Combine(Application.persistentDataPath, "Balloon.bytes");
    static string BalloonBytesETagPath => Path.Combine(Application.persistentDataPath, "Balloon.bytes.etag");
    static string BalloonBytesHashPath => Path.Combine(Application.persistentDataPath, "Balloon.bytes.hash");

    // https://stackoverflow.com/questions/17292366/hashing-with-sha1-algorithm-in-c-sharp
    public static string Sha1Hash(byte[] input)
    {
        using (var sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(input);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }

    public static string Sha1Hash(Stream input)
    {
        using (var sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(input);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }

    static string GetCachedEtag()
    {
        try
        {
            return File.ReadAllText(BalloonBytesETagPath);
        }
        catch
        {
            return "";
        }
    }

    public static Stream GetBalloonDataStream()
    {
        try
        {
            var stream = new MemoryStream(File.ReadAllBytes(BalloonBytesPath));
            SushiDebug.Log("GetBalloonDataStream(): returning cached remote datasheet");
            return stream;
        }
        catch (Exception e)
        {
            SushiDebug.LogFormat("Something went wrong while accessing cached remote datasheet: {0}", e.ToString());
            DeleteAllCaches();
            return GetBuiltInBalloonDataStream();
        }
    }

    public static void DeleteAllCaches()
    {
        SushiDebug.LogFormat("Deleting cache file {0}...", BalloonBytesPath);
        File.Delete(BalloonBytesPath);
        SushiDebug.LogFormat("Deleting cache file {0}...", BalloonBytesETagPath);
        File.Delete(BalloonBytesETagPath);
        SushiDebug.LogFormat("Deleting cache file {0}...", BalloonBytesHashPath);
        File.Delete(BalloonBytesETagPath);
    }

    public static Stream GetBuiltInBalloonDataStream()
    {
        var asset = Resources.Load("Data/Balloon") as TextAsset;
        var stream = new MemoryStream(asset.bytes);
        SushiDebug.Log("GetBalloonDataStream(): returning built-in datasheet");
        return stream;
    }

    public static IEnumerator GetCachedOrDownloadBalloonData(Action<float> onProgress, Action<bool> onSuccess,
        Action<string> onNetworkFail, Action onCacheCorrupt)
    {
        var getUri = string.Format("https://s3.ap-northeast-2.amazonaws.com/balloontycoon/{0}/Balloon.bytes",
            Application.version);
        SushiDebug.LogFormat("HTTP GET: {0}", getUri);
        using (var www = UnityWebRequest.Get(getUri))
        {
            www.redirectLimit = 1;
            www.SetRequestHeader("If-None-Match", GetCachedEtag());
            www.disposeDownloadHandlerOnDispose = true;
            www.downloadHandler =
                new DownloadHandlerBuffer(); // DownloadHandlerFile(Path.Combine(Application.persistentDataPath, "Balloon.bytes"));

            var downloadAsync = www.SendWebRequest();
            while (downloadAsync.isDone == false && www.result != UnityWebRequest.Result.ConnectionError)
            {
                SushiDebug.LogFormat("Downloading...{0:f0}%", downloadAsync.progress * 100.0f);
                onProgress(downloadAsync.progress);
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                SushiDebug.Log(www.error);
                onNetworkFail(www.error);
            }
            else
            {
                SushiDebug.LogFormat("www.responseCode = {0}", www.responseCode);
                SushiDebug.LogFormat("Data length: {0}", www.downloadHandler.data.Length);
                if (www.responseCode == 304)
                {
                    SushiDebug.LogFormat("Cached file is UP-TO-DATE.");
                    try
                    {
                        var cachedBytes = File.ReadAllBytes(BalloonBytesPath);
                        if (Sha1Hash(cachedBytes) == File.ReadAllText(BalloonBytesHashPath))
                            onSuccess(false);
                        else
                            onCacheCorrupt();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        onCacheCorrupt();
                    }
                }
                else if (www.responseCode == 200)
                {
                    SushiDebug.LogFormat("Successfully downloaded.");
                    File.WriteAllBytes(BalloonBytesPath, www.downloadHandler.data);
                    SushiDebug.LogFormat("Successfully written to {0}", BalloonBytesPath);
                    File.WriteAllText(BalloonBytesETagPath, www.GetResponseHeader("ETag"));
                    SushiDebug.LogFormat("Successfully written to {0}", BalloonBytesETagPath);
                    var downloadedSha1Hash = Sha1Hash(www.downloadHandler.data);
                    File.WriteAllText(BalloonBytesHashPath, downloadedSha1Hash);
                    SushiDebug.LogFormat("Successfully written to {0}", BalloonBytesHashPath);
                    var builtInSha1Hash = Sha1Hash(GetBuiltInBalloonDataStream());
                    onSuccess(downloadedSha1Hash != builtInSha1Hash);
                }
                else
                {
                    Debug.LogError("Unknown response code!");
                    onNetworkFail("Response Code: " + www.responseCode);
                }
            }
        }
    }

    public void OpenDataUpdater()
    {
        ProgressMessage.instance.Open("최신 게임 데이터 확인 중...");
        StartCoroutine(UpdateBalloonDataCoro());
    }

    IEnumerator UpdateBalloonDataCoro()
    {
        yield return GetCachedOrDownloadBalloonData(progress =>
        {
            // 진행 상황 업데이트
            ProgressMessage.instance.Open(string.Format("최신 게임 데이터 확인 중...{0:F0}%", progress * 100));
        }, updated =>
        {
            // 성공 시
            ProgressMessage.instance.Close();
            if (updated)
                ConfirmPopup.instance.Open("최신 게임 데이터를 받았습니다. 게임을 재시작합니다.", () => SceneManager.LoadScene("Splash"));
            else
                ConfirmPopup.instance.Open("현재 최신 게임 데이터입니다.");
        }, errMsg =>
        {
            // 실패 시
            ProgressMessage.instance.Close();
            ConfirmPopup.instance.Open(string.Format("최신 게임 데이터 확인에 실패했습니다.\n\n{0}", errMsg));
        }, () =>
        {
            // ETag 상 다시 받을 필요는 없다고 했는데,
            // 실 기기에 파일이 이상한 경우 여기로 온다.
            ProgressMessage.instance.Close();
            DeleteAllCaches();
            ConfirmPopup.instance.OpenYesNoPopup("게임 데이터에 문제가 있습니다. 다시 받으시겠습니까?", () =>
            {
                ConfirmPopup.instance.Close();
                OpenDataUpdater();
            }, () => ConfirmPopup.instance.Close());
        });
    }
}