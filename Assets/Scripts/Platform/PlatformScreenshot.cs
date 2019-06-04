using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using System.Collections.Generic;
using System.Collections;
public class PlatformScreenshot : MonoBehaviour
{

    public static event Action screenShotEvent;
    //private static string fullClassName = "top.plusalpha.screenshot.Screenshot";

    // 스크린샷 공유 기능 - 스크린샷은 Unity (C#)에서 찍고, 찍은 내용을 Java쪽으로 넘긴다.
    // 스크린샷 공유를 위한 아래 함수는 Unity 이벤트 핸들러로서 연결되어 있으므로
    // Visual Studio에서 참고(레퍼런스) 체크 시 검사되지 않음
    // 사용되지 않는 것이 아니므로 삭제하지 말 것...
    public void SharePngByteArrayOnUiThread()
    {
        StartCoroutine(TakeScreenShot());
    }
    //이런식으로 랜더링이 끝날때까지 기다린후에 스샷을 읽어야 에러가 안난다.
    IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        Platform.instance.ShareScreenshot(screenshot.EncodeToPNG());

        if (screenShotEvent != null)
            screenShotEvent();
    }
}
