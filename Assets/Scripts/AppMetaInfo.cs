using UnityEngine;

public class AppMetaInfo : ScriptableObject {
    public string buildNumber;
    public string buildStartDateTime;
    public int androidBundleVersionCode; // Only for Android
    public string iosBuildNumber; // Only for iOS

    public string GetAppMetaInfo() {
        var appMetaInfo = Resources.Load<AppMetaInfo>("App Meta Info");
        var platformVersionCode = "UNKNOWN";

        if (appMetaInfo != null) {
            switch (Application.platform) {
                case RuntimePlatform.Android: platformVersionCode = appMetaInfo.androidBundleVersionCode.ToString(); break;
                case RuntimePlatform.IPhonePlayer: platformVersionCode = appMetaInfo.iosBuildNumber; break;
                default: platformVersionCode = $"{appMetaInfo.androidBundleVersionCode},{appMetaInfo.iosBuildNumber}"; break;
            }
            return $"v{Application.version}#{appMetaInfo.buildNumber} {appMetaInfo.buildStartDateTime} [{platformVersionCode}]";
        } else {
            return $"v{Application.version} [{platformVersionCode}]";
        }
    }
}
