using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class PlatformLog : MonoBehaviour {
    private static string LOGGLY_URL = "http://xxxx";

    //Register the HandleLog function on scene start to fire on debug.log events
    public void OnEnable()
    {
        //Application.logMessageReceived += HandleLog;
    }

    //Remove callback when object goes out of scope
    public void OnDisable()
    {
        //Application.logMessageReceived -= HandleLog;
    }

    //Capture debug.log output, send logs to Loggly
    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        var loggingForm = new WWWForm();
        // 'stackTrace' 변수는 기기 빌드에서는 비어 있으므로 직접 구한다.
        System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
        loggingForm.AddField("localTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
        loggingForm.AddField("stackTrace", trace.ToString());
        loggingForm.AddField("message", logString);
        loggingForm.AddField("deviceModel", SystemInfo.deviceModel);
        loggingForm.AddField("isEditor", Application.isEditor.ToString());
        loggingForm.AddField("logType", type.ToString());
        loggingForm.AddField("applicationVersion", Application.version);
        StartCoroutine(SendData(loggingForm));
    }

    public IEnumerator SendData(WWWForm form)
    {   
        using (UnityWebRequest request = UnityWebRequest.Post(LOGGLY_URL, form))
        {
            yield return request.SendWebRequest();
        }
    }
}
