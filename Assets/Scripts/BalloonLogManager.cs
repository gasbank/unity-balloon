using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

public class BalloonLogManager : MonoBehaviour, BalloonLogViewer.IBalloonLogSource {
    public static BalloonLogManager instance;
    FileStream writeLogStream;
    string LogFilePath { get { return Path.Combine(Application.persistentDataPath, "balloon.log"); } }
    [SerializeField] BalloonLogViewer logViewer = null;

    void Awake() {
        instance = this;
        writeLogStream = File.Open(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
    }

    void OnApplicationPause(bool pause) {
        writeLogStream.Flush(true);
    }

    void OnDestroy() {
        FinalizeLogStream();
    }

    void OnApplicationQuit() {
        FinalizeLogStream();
    }

    private void FinalizeLogStream() {
        if (writeLogStream != null) {
            Flush();
            writeLogStream.Close();
            writeLogStream.Dispose();
            writeLogStream = null;
        }
    }

    public void Flush() {
        if (writeLogStream != null) {
            writeLogStream.Flush(true);
        }
    }

    public static void Add(BalloonLogEntry.Type type, int arg1, long arg2) {
        try {
            if (instance != null && instance.writeLogStream != null) {
                byte[] logBytes = GetLogEntryBytes(type, arg1, arg2);
                instance.writeLogStream.Write(logBytes, 0, logBytes.Length);
                if (instance.logViewer != null) {
                    instance.logViewer.Refresh();
                }
            }
        } catch {
        }
    }

    public static byte[] GetLogEntryBytes(BalloonLogEntry.Type type, int arg1, long arg2) {
        return BitConverter.GetBytes(DateTime.UtcNow.Ticks)
                        .Concat(BitConverter.GetBytes((int)type))
                        .Concat(BitConverter.GetBytes(arg1))
                        .Concat(BitConverter.GetBytes(arg2)).ToArray();
    }

    FileStream OpenReadLogStream() {
        return File.Open(instance.LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public long Count() {
        var dummyLogEntryBytes = GetLogEntryBytes(BalloonLogEntry.Type.DummyLogRecord, 0, 0);
        using (var readLogStream = OpenReadLogStream()) {
            return readLogStream.Length / dummyLogEntryBytes.Length;
        }
    }

    public List<BalloonLogEntry> Read(int logEntryStartIndex, int count) {
        List<BalloonLogEntry> logEntryList = new List<BalloonLogEntry>();
        using (var readLogStream = OpenReadLogStream()) {
            var dummyLogEntryBytes = GetLogEntryBytes(BalloonLogEntry.Type.DummyLogRecord, 0, 0);
            readLogStream.Seek(logEntryStartIndex * dummyLogEntryBytes.Length, SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            var offset = 0;
            for (int i = 0; i < readByteCount / dummyLogEntryBytes.Length; i++) {
                logEntryList.Add(new BalloonLogEntry {
                    ticks = BitConverter.ToInt64(bytes, offset + 0),
                    type = BitConverter.ToInt32(bytes, offset + 0 + 8),
                    arg1 = BitConverter.ToInt32(bytes, offset + 0 + 8 + 4),
                    arg2 = BitConverter.ToInt64(bytes, offset + 0 + 8 + 4 + 4),
                });
                offset += dummyLogEntryBytes.Length;
            }
        }
        return logEntryList;
    }

    [System.Serializable]
    class PlayLogFile {
        [System.Serializable]
        public class Fields {
            [System.Serializable]
            public class BytesValueData {
                public string bytesValue;
            }
            [System.Serializable]
            public class TimestampValueData {
                public string timestampValue;
            }
            [System.Serializable]
            public class StringValueData {
                public string stringValue;
            }
            [System.Serializable]
            public class IntegerValueData {
                public long integerValue;
            }
            public BytesValueData saveData = new BytesValueData();
            public BytesValueData playLogData = new BytesValueData();
            public IntegerValueData playLogUncompressedSizeData = new IntegerValueData();
            public TimestampValueData uploadDate = new TimestampValueData();
            public StringValueData appMetaInfo = new StringValueData();
            public StringValueData userId = new StringValueData();
            public StringValueData gemStr = new StringValueData();
            public StringValueData socialUserName = new StringValueData();
        }
        public Fields fields = new Fields();
    }

    public async Task UploadPlayLogAsync(byte[] playLogBytes, int uncompressedBytesLength) {
        var uploadPlayLogFileDbUrl = ConfigPopup.BaseUrl + "/playLog";
        ProgressMessage.instance.Open("\\클라우드 저장 중...".Localized());

        var errorDeviceId = ErrorReporter.instance.GetOrCreateErrorDeviceId();
        var url = string.Format("{0}/{1}", uploadPlayLogFileDbUrl, errorDeviceId);
        var saveFile = new PlayLogFile();
        var uploadDate = DateTime.UtcNow;
        try { saveFile.fields.uploadDate.timestampValue = uploadDate.ToString("yyyy-MM-ddTHH:mm:ssZ"); } catch { saveFile.fields.uploadDate.timestampValue = "ERROR"; }
        try { saveFile.fields.appMetaInfo.stringValue = ConfigPopup.instance.GetAppMetaInfo(); } catch { saveFile.fields.appMetaInfo.stringValue = "ERROR"; }
        try { saveFile.fields.userId.stringValue = ConfigPopup.instance.GetUserId(); } catch { saveFile.fields.userId.stringValue = "ERROR"; }
        try { saveFile.fields.saveData.bytesValue = System.Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName)); } catch { saveFile.fields.saveData.bytesValue = "ERROR"; }
        try { saveFile.fields.playLogData.bytesValue = System.Convert.ToBase64String(playLogBytes); } catch { saveFile.fields.playLogData.bytesValue = "ERROR"; }
        try { saveFile.fields.playLogUncompressedSizeData.integerValue = uncompressedBytesLength; } catch { saveFile.fields.playLogUncompressedSizeData.integerValue = 0; }
        try { saveFile.fields.gemStr.stringValue = BalloonSpawner.instance.Gem.ToString("n0"); } catch { saveFile.fields.gemStr.stringValue = "ERROR"; }
        try { saveFile.fields.socialUserName.stringValue = Social.localUser.userName; } catch { saveFile.fields.socialUserName.stringValue = "ERROR"; }
        
        try {
            using (var httpClient = new HttpClient()) {
                var patchData = JsonUtility.ToJson(saveFile);
                using (var patchContent = new StringContent(patchData)) {
                    SushiDebug.Log($"HttpClient PATCH TO {url}...");

                    // PATCH 시작하고 기다린다.
                    var patchTask = await httpClient.PatchAsync(new Uri(url), patchContent);

                    SushiDebug.Log($"HttpClient Result: {patchTask.ReasonPhrase}");

                    if (patchTask.IsSuccessStatusCode) {
                        SushiDebug.Log("Play log uploaded successfully.");
                        //var msg = string.Format("\\업로드가 성공적으로 완료됐습니다.\\n\\n업로드 코드: {0}\\n용량: {1:n0}바이트\\nTS: {2}\\n\\n<color=brown>본 화면의 스크린샷을 찍어 공식 카페에 버그 신고를 부탁 드립니다.</color>\\n\\n업로드된 데이터를 분석 후, 카페를 통해 이후 진행을 안내드리겠습니다.\\n\\n공식 카페로 이동하거나, 안내 받은 복구 코드를 입력하세요.".Localized(), errorDeviceId, patchData.Length, saveFile.fields.uploadDate.timestampValue);
                        //ConfirmPopup.instance.OpenTwoButtonPopup(msg, () => ConfigPopup.instance.OpenCommunity(), () => SaveLoadManager.EnterRecoveryCode(exceptionList, st), "\\업로드 완료".Localized(), "\\공식 카페 이동".Localized(), "\\복구 코드 입력".Localized());
                    } else {
                        Debug.LogError($"Play log upload failed: status code={patchTask.StatusCode}");
                    }
                }
            }
        } catch (Exception e) {
            Debug.LogError($"Play log upload exception: {e.ToString()}");
        } finally {
            // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
            ProgressMessage.instance.Close();
        }
	}
	
	public static async Task DumpAndUploadPlayLog() {
        var count = 10000;
        SushiDebug.Log($"Dumping log for last {count:n0} log entries...");
        var dummyLogEntryBytes = GetLogEntryBytes(BalloonLogEntry.Type.DummyLogRecord, 0, 0);
        using (var readLogStream = instance.OpenReadLogStream()) {
            readLogStream.Seek(Math.Max(0, readLogStream.Length - count * dummyLogEntryBytes.Length), SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            SushiDebug.Log($"{readByteCount:n0} bytes ({(readByteCount / dummyLogEntryBytes.Length):n0} log entries) read.");
            if (readByteCount % dummyLogEntryBytes.Length != 0) {
                Debug.LogError($"Log dump failed! readByteCount={readByteCount:n0}, logEntrySize={dummyLogEntryBytes.Length:n0}. Abort!");
                return;
            }
            var maxOutBytesLength = MessagePack.LZ4.LZ4Codec.MaximumOutputLength(readByteCount);
            SushiDebug.Log($"Maximum compressed log: {maxOutBytesLength:n0} bytes");
            var outBytes = new byte[maxOutBytesLength];
            var outBytesLength = MessagePack.LZ4.LZ4Codec.Encode(bytes, 0, readByteCount, outBytes, 0, outBytes.Length);
            SushiDebug.Log($"Compressed log size: {outBytesLength:n0} bytes");
            outBytes = outBytes.Take(outBytesLength).ToArray();
            await instance.UploadPlayLogAsync(outBytes, readByteCount);
        }
    }
}
