using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformCallbackHandler : MonoBehaviour
{
    // Unity API의 한계로 함수 인자는 string 하나만 쓸 수 있다.
    void OnIosSaveResult(string result)
    {
        BalloonDebug.LogFormat("PlatformCallbackHandler.OnIosSaveResult: {0}", result != null ? result : "(null)");
        Platform.instance.OnCloudSaveResult(result);
    }

    // Unity API의 한계로 함수 인자는 string 하나만 쓸 수 있다.
    void OnIosLoadResult(string result)
    {
        if (result.StartsWith("*****ERROR***** "))
        {
            Platform.instance.OnCloudLoadResult(result, null);
        }
        else
        {
            BalloonDebug.LogFormat("PlatformCallbackHandler.OnIosLoadResult: {0}", result != null ? result : "(null)");
            byte[] loadedDataBytes = null;
            if (result != null)
                try
                {
                    loadedDataBytes = Convert.FromBase64String(result);
                }
                catch
                {
                    loadedDataBytes = null;
                }

            Platform.instance.OnCloudLoadResult("OK", loadedDataBytes);
        }
    }
}