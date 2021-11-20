using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MessagePack.LZ4;
using MiniJSON;
using UnityEngine;
using UnityEngine.UI;
using Dict = System.Collections.Generic.Dictionary<string, object>;

public class BalloonLogViewer : MonoBehaviour
{
    [SerializeField]
    int curPage;

    [SerializeField]
    int entryCountPerPage = 20;

    [SerializeField]
    GameObject loadRemoteLogButton;

    IBalloonLogSource logSource;

    [SerializeField]
    Text logText;

    [SerializeField]
    int pages;

    [SerializeField]
    Text pageText;

    [SerializeField]
    int totalEntryCount;

    void OnEnable()
    {
        logSource = BalloonLogManager.instance;
        UpdateToPage(0);
        loadRemoteLogButton.SetActive(BalloonSpawner.instance.cheatMode);
    }

    void UpdateToPage(int page)
    {
        if (gameObject.activeSelf == false) return;

        FlushAndUpdateTotalPages();
        curPage = pages > 0 ? Mathf.Clamp(page, 0, pages - 1) : 0;
        RefreshCurrentPage();
    }

    void RefreshCurrentPage()
    {
        if (pages > 0)
        {
            var sb = new StringBuilder();
            var logEntryList = logSource.Read(curPage * entryCountPerPage, entryCountPerPage);
            //SushiDebug.Log($"log Entry List: {logEntryList.Count}");
            foreach (var logEntry in logEntryList)
                sb.AppendLine(string.Format("{0} {1,-20} {2,6:n0} {3:n0}",
                    new DateTime(logEntry.ticks, DateTimeKind.Utc).ToLocalTime().ToString("MM/dd HH:mm:ss"),
                    (BalloonLogEntry.Type) logEntry.type,
                    logEntry.arg1,
                    logEntry.arg2));
            logText.text = sb.ToString();
        }
        else
        {
            logText.text = "EMPTY";
        }

        RefreshPageText();
    }

    void FlushAndUpdateTotalPages()
    {
        logSource.Flush();
        totalEntryCount = (int) logSource.Count();
        pages = totalEntryCount / entryCountPerPage + (totalEntryCount % entryCountPerPage == 0 ? 0 : 1);
    }

    public void GoToFirstPage()
    {
        UpdateToPage(0);
    }

    public void GoToPrevPage()
    {
        UpdateToPage(curPage - 1);
    }

    public void GoToNextPage()
    {
        UpdateToPage(curPage + 1);
    }

    public void GoToLastPage()
    {
        UpdateToPage(pages - 1);
    }

    void RefreshPageText()
    {
        if (pages > 0)
            pageText.text = $"{curPage + 1}/{pages}";
        else
            pageText.text = "--/--";
    }

    public void Refresh()
    {
        if (gameObject.activeSelf == false) return;

        FlushAndUpdateTotalPages();
        curPage = pages > 0 ? pages - 1 : 0;
        RefreshCurrentPage();
    }

    public void LoadRemoteLog()
    {
        ConfirmPopup.instance.OpenInputFieldPopup("Play Log Error Device ID", async () =>
        {
            ConfirmPopup.instance.Close();
            var remoteLogSource = new BalloonRemoteLogSource();
            await remoteLogSource.LoadPlayLogAsync(ConfirmPopup.instance.InputFieldText);
            logSource = remoteLogSource;
            UpdateToPage(0);
        }, () => { ConfirmPopup.instance.Close(); }, "Remote Log", ConfirmPopup.Header.Normal, "", "");
    }

    public interface IBalloonLogSource
    {
        List<BalloonLogEntry> Read(int logEntryStartIndex, int count);
        long Count();
        void Flush();
    }

    class BalloonRemoteLogSource : IBalloonLogSource
    {
        MemoryStream readLogStream;

        public void Flush()
        {
        }

        public long Count()
        {
            var dummyLogEntryBytes = BalloonLogManager.GetLogEntryBytes(BalloonLogEntry.Type.DummyLogRecord, 0, 0);
            return readLogStream.Length / dummyLogEntryBytes.Length;
        }

        public List<BalloonLogEntry> Read(int logEntryStartIndex, int count)
        {
            var logEntryList = new List<BalloonLogEntry>();
            var dummyLogEntryBytes = BalloonLogManager.GetLogEntryBytes(BalloonLogEntry.Type.DummyLogRecord, 0, 0);
            readLogStream.Seek(logEntryStartIndex * dummyLogEntryBytes.Length, SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            var offset = 0;
            for (var i = 0; i < readByteCount / dummyLogEntryBytes.Length; i++)
            {
                logEntryList.Add(new BalloonLogEntry
                {
                    ticks = BitConverter.ToInt64(bytes, offset + 0),
                    type = BitConverter.ToInt32(bytes, offset + 0 + 8),
                    arg1 = BitConverter.ToInt32(bytes, offset + 0 + 8 + 4),
                    arg2 = BitConverter.ToInt64(bytes, offset + 0 + 8 + 4 + 4)
                });
                offset += dummyLogEntryBytes.Length;
            }

            return logEntryList;
        }

        public async Task LoadPlayLogAsync(string playLogCode)
        {
            var saveDbUrl = ConfigPopup.BaseUrl + "/playLog";
            ProgressMessage.instance.Open("Loading play log");
            playLogCode = playLogCode.Trim();
            var url = string.Format("{0}/{1}", saveDbUrl, playLogCode);
            SushiDebug.LogFormat("URL: {0}", url);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var getTask = await httpClient.GetAsync(url);
                    if (getTask.IsSuccessStatusCode)
                    {
                        var text = await getTask.Content.ReadAsStringAsync();
                        var userPlayLogDataRoot = Json.Deserialize(text) as Dict;
                        var userPlayLogDataFields = userPlayLogDataRoot["fields"] as Dict;
                        var userPlayLogDataFieldsSaveData = userPlayLogDataFields["playLogData"] as Dict;
                        var userPlayLogDataFieldsSaveDataStringValue =
                            userPlayLogDataFieldsSaveData["bytesValue"] as string;
                        var userPlayLogUncompressedSizeData =
                            userPlayLogDataFields["playLogUncompressedSizeData"] as Dict;
                        var userPlayLogUncompressedSizeDataIntegerValue =
                            int.Parse(userPlayLogUncompressedSizeData["integerValue"] as string);

                        var playLogDataBase64 = userPlayLogDataFieldsSaveDataStringValue;
                        var playLogData = Convert.FromBase64String(playLogDataBase64);
                        var playLogUncompressedData = new byte[userPlayLogUncompressedSizeDataIntegerValue];
                        LZ4Codec.Decode(playLogData, 0, playLogData.Length, playLogUncompressedData, 0,
                            playLogUncompressedData.Length);

                        readLogStream = new MemoryStream(playLogUncompressedData);

                        SushiDebug.LogFormat("Play Log Data Base64 ({0} bytes): {1}",
                            playLogDataBase64 != null ? playLogDataBase64.Length : 0, playLogDataBase64);
                        SushiDebug.LogFormat("Play Log Data ({0} bytes - compresseed)", playLogData.Length);
                    }
                    else
                    {
                        Debug.LogError($"Loading play log failed - status code {getTask.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Play log upload exception: {e}");
            }
            finally
            {
                // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
                ProgressMessage.instance.Close();
            }
        }
    }
}