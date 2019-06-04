using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using System.Linq;
using UnityEditor;
using System.IO;
using System;
using BigInteger = System.Numerics.BigInteger;

[DisallowMultipleComponent]
public class ConfigPopup : MonoBehaviour {
    public static ConfigPopup instance;

    [SerializeField] Toggle bgmToggle = null;
    [SerializeField] Toggle sfxToggle = null;
    [SerializeField] Toggle gatherStoredMaxSfxToggle = null;
    [SerializeField] Toggle notchToggle = null;
    [SerializeField] Toggle performanceModeToggle = null;
    [SerializeField] Toggle alwaysOnToggle = null;
    [SerializeField] Toggle bigNumberNotationToggle = null;
    [SerializeField] Toggle hideSpawnEffectToggle = null;
    [SerializeField] Toggle fastAutoMergerOnlyModeToggle = null;
    [SerializeField] GameObject fastAutoMergerOnlyMode = null;
    [SerializeField] Toggle lowLevelBalloonRemovalToggle = null;
    [SerializeField] GameObject lowLevelBalloonRemoval = null;
    [SerializeField] Toggle languageCodeKoToggle = null;
    [SerializeField] Toggle languageCodeJaToggle = null;
    [SerializeField] Toggle languageCodeChToggle = null;
    [SerializeField] Toggle languageCodeTwToggle = null;
    [SerializeField] Dropdown languageDropdown = null;

    [SerializeField] TopNotchOffsetGroup[] topNotchOffsetGroupList = null;
    [SerializeField] Animator topAnimator = null;
    [SerializeField] Text userPseudoIdText = null;
    [SerializeField] Text appMetaInfoText = null;
    [SerializeField] List<GameObject> configButtonGroupEtc = null;
    public bool IsNotchOn { get { return notchToggle.isOn; } set { notchToggle.isOn = value; } }
    public bool IsPerformanceModeOn { get { return performanceModeToggle.isOn; } set { performanceModeToggle.isOn = value; } }
    public bool IsAlwaysOnOn { get { return alwaysOnToggle.isOn; } set { alwaysOnToggle.isOn = value; } }
    public bool IsBigNumberNotationOn { get { return bigNumberNotationToggle.isOn; } set { bigNumberNotationToggle.isOn = value; } }
    public bool IsHideSpawnEffectOn { get { return hideSpawnEffectToggle.isOn; } set { hideSpawnEffectToggle.isOn = value; } }
    public bool IsFastAutoMergerOnlyModeOn { get { return fastAutoMergerOnlyModeToggle.isOn; } set { fastAutoMergerOnlyModeToggle.isOn = value; } }
    public bool IsLowLevelBalloonRemovalOn { get { return lowLevelBalloonRemovalToggle.isOn; } set { lowLevelBalloonRemovalToggle.isOn = value; } }
    public string ServiceId { get { return string.Format("{0:D3}-{1:D3}", (BalloonSpawner.instance.userPseudoId / 1000).ToInt(), (BalloonSpawner.instance.userPseudoId % 1000).ToInt()); } }
    public static readonly string BaseUrl = "https://firestore.googleapis.com/xxxxxxx";
    static readonly string ServiceDbUrl = BaseUrl + "/service";
    public static string NoticeDbUrl { get { return BaseUrl + "/notice" + noticeDbPostfix; } }
    public static string noticeDbPostfix = "";
    [SerializeField] GameObject configButtonNewImage = null;
    [SerializeField] GameObject noticeButtonNewImage = null;
    public bool EtcGroupVisible { get { return Application.systemLanguage == SystemLanguage.Korean || BalloonSpawner.instance.cheatMode; } }

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

    public string GetUserId() {
        return string.Format("ID: {0}-{1:D3}", ServiceId, BalloonSpawner.instance.lastConsumedServiceIndex.ToInt());
    }

    public void OpenCommunity() {
        Application.OpenURL("https://cafe.naver.com/balloontycoon");
    }

    public void RequestUserReview() {
        Platform.instance.RequestUserReview();
    }

    internal void EnableLanguageKo() {
        throw new NotImplementedException();
    }

    internal void EnableLanguageCh() {
        throw new NotImplementedException();
    }

    internal void EnableLanguageTw() {
        throw new NotImplementedException();
    }

    internal void EnableLanguageJa() {
        throw new NotImplementedException();
    }
}
