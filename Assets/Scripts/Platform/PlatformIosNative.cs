using System;
using System.Collections;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformIosNative : MonoBehaviour
{
#if UNITY_IOS
	[DllImport ("__Internal")]
	public static extern void saveToCloudPrivate(string playerID, string data, string loginErrorTitle, string loginErrorMessage, string confirmMessage);
	[DllImport ("__Internal")]
    public static extern void loadFromCloudPrivate(string playerID, string loginErrorTitle, string loginErrorMessage, string confirmMessage);
	[DllImport ("__Internal")]
    public static extern void clearAllNotifications();
	[DllImport ("__Internal")]
    public static extern void sendMail(string title, string body, string recipient, string attachment);
#endif
    // Use this for initialization
    void Start()
    {
#if UNITY_IOS
		UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
#if !UNITY_EDITOR
		saveToCloudPrivate("testplayerid", "heheheh~~~", "test login error title", "test login error message", "test confirm message");
		loadFromCloudPrivate("testplayerid", "test login error title", "test login error message", "test confirm message");
#endif
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == false)
        {
#if UNITY_IOS && !UNITY_EDITOR
			UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
			UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
			clearAllNotifications();
#endif
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
#if UNITY_IOS && !UNITY_EDITOR
			UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
			UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
			clearAllNotifications();
#endif
        }
    }

    public void ScheduleNotification()
    {
        var notificationDate0000 = DateTime.Today;
        var now = DateTime.Now;
        var notificationDate1200 = notificationDate0000.AddHours(12);
        var notificationDate1800 = notificationDate0000.AddHours(18);
        if (notificationDate1200 < now) notificationDate1200 = notificationDate1200.AddDays(1);
        if (notificationDate1800 < now) notificationDate1800 = notificationDate1800.AddDays(1);
        SushiDebug.LogFormat("Notification Time 1: {0}", notificationDate1200);
        SushiDebug.LogFormat("Notification Time 2: {0}", notificationDate1800);
#if UNITY_IOS
		SushiDebug.Log("Schedule Local Notification");
		UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
		UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
		#if !UNITY_EDITOR
		clearAllNotifications();
		#endif

		UnityEngine.iOS.LocalNotification notification1200 = new UnityEngine.iOS.LocalNotification();
		notification1200.fireDate = notificationDate1200;
		notification1200.alertBody = "Message";
		notification1200.alertAction = "Action";
		notification1200.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		notification1200.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notification1200);

		UnityEngine.iOS.LocalNotification notification1800 = new UnityEngine.iOS.LocalNotification();
		notification1800.fireDate = notificationDate1800;
		notification1800.alertBody = "Message";
		notification1800.alertAction = "Action";
		notification1800.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		notification1800.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notification1800);
#endif
    }

    void OnIosSaveResult(string result)
    {
        SushiDebug.LogFormat("OnIosSaveResult: {0}", result);
    }

    void OnIosLoadResult(string result)
    {
        SushiDebug.LogFormat("OnIosLoadResult: {0}", result);
    }

    public void CaptureScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoro());
    }

    readonly string filename = "ss3.png";

    IEnumerator CaptureScreenshotCoro()
    {
        if (Application.isEditor)
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/" + filename);
        else
            ScreenCapture.CaptureScreenshot(filename);
        SushiDebug.LogFormat("Captured!");
        var info = new FileInfo(Application.persistentDataPath + "/" + filename);
        while (info == null || info.Exists == false)
        {
            info = new FileInfo(Application.persistentDataPath + "/" + filename);
            yield return null;
        }

        SushiDebug.LogFormat("Screenshot saved successfully! Size={0}, Path={1}", info.Length, info.FullName);
#if !UNITY_EDITOR
		NativeShare.Share("body", info.FullName, null, "subject", "image/png", true, "Select sharing app");
#endif
    }

    public void SendBugReportMail()
    {
#if UNITY_IOS && !UNITY_EDITOR
		sendMail("Title", "Body", "gasbank@gmail.com", Application.persistentDataPath + "/" + filename);
#endif
    }
}