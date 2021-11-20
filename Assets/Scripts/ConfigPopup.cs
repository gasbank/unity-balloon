using System;
using UnityEngine;
using UnityEngine.UI;
using Dict = System.Collections.Generic.Dictionary<string, object>;

[DisallowMultipleComponent]
public class ConfigPopup : MonoBehaviour
{
    public static ConfigPopup instance;
    public static readonly string BaseUrl = "https://firestore.googleapis.com/xxxxxxx";
    static readonly string ServiceDbUrl = BaseUrl + "/service";
    public static string noticeDbPostfix = "";

    [SerializeField]
    Toggle alwaysOnToggle;

    [SerializeField]
    Toggle bigNumberNotationToggle;

    [SerializeField]
    Toggle fastAutoMergerOnlyModeToggle;

    [SerializeField]
    Toggle hideSpawnEffectToggle;

    //[SerializeField] GameObject fastAutoMergerOnlyMode = null;
    [SerializeField]
    Toggle lowLevelBalloonRemovalToggle;

    //[SerializeField] Toggle bgmToggle = null;
    //[SerializeField] Toggle sfxToggle = null;
    //[SerializeField] Toggle gatherStoredMaxSfxToggle = null;
    [SerializeField]
    Toggle notchToggle;

    [SerializeField]
    Toggle performanceModeToggle;
    //[SerializeField] GameObject lowLevelBalloonRemoval = null;
    //[SerializeField] Toggle languageCodeKoToggle = null;
    //[SerializeField] Toggle languageCodeJaToggle = null;
    //[SerializeField] Toggle languageCodeChToggle = null;
    //[SerializeField] Toggle languageCodeTwToggle = null;
    //[SerializeField] Dropdown languageDropdown = null;

    //[SerializeField] TopNotchOffsetGroup[] topNotchOffsetGroupList = null;
    //[SerializeField] Animator topAnimator = null;
    //[SerializeField] Text userPseudoIdText = null;
    //[SerializeField] Text appMetaInfoText = null;
    //[SerializeField] List<GameObject> configButtonGroupEtc = null;
    public bool IsNotchOn
    {
        get => notchToggle.isOn;
        set => notchToggle.isOn = value;
    }

    public bool IsPerformanceModeOn
    {
        get => performanceModeToggle.isOn;
        set => performanceModeToggle.isOn = value;
    }

    public bool IsAlwaysOnOn
    {
        get => alwaysOnToggle.isOn;
        set => alwaysOnToggle.isOn = value;
    }

    public bool IsBigNumberNotationOn
    {
        get => bigNumberNotationToggle.isOn;
        set => bigNumberNotationToggle.isOn = value;
    }

    public bool IsHideSpawnEffectOn
    {
        get => hideSpawnEffectToggle.isOn;
        set => hideSpawnEffectToggle.isOn = value;
    }

    public bool IsFastAutoMergerOnlyModeOn
    {
        get => fastAutoMergerOnlyModeToggle.isOn;
        set => fastAutoMergerOnlyModeToggle.isOn = value;
    }

    public bool IsLowLevelBalloonRemovalOn
    {
        get => lowLevelBalloonRemovalToggle.isOn;
        set => lowLevelBalloonRemovalToggle.isOn = value;
    }

    public string ServiceId => string.Format("{0:D3}-{1:D3}", (BalloonSpawner.instance.userPseudoId / 1000).ToInt(),
        (BalloonSpawner.instance.userPseudoId % 1000).ToInt());

    public static string NoticeDbUrl => BaseUrl + "/notice" + noticeDbPostfix;

    //[SerializeField] GameObject configButtonNewImage = null;
    //[SerializeField] GameObject noticeButtonNewImage = null;
    public bool EtcGroupVisible =>
        Application.systemLanguage == SystemLanguage.Korean || BalloonSpawner.instance.cheatMode;

    public string GetAppMetaInfo()
    {
        var appMetaInfo = Resources.Load<AppMetaInfo>("App Meta Info");
        var platformVersionCode = "UNKNOWN";

        if (appMetaInfo != null)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    platformVersionCode = appMetaInfo.androidBundleVersionCode.ToString();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platformVersionCode = appMetaInfo.iosBuildNumber;
                    break;
                default:
                    platformVersionCode = $"{appMetaInfo.androidBundleVersionCode},{appMetaInfo.iosBuildNumber}";
                    break;
            }

            return
                $"v{Application.version}#{appMetaInfo.buildNumber} {appMetaInfo.buildStartDateTime} [{platformVersionCode}]";
        }

        return $"v{Application.version} [{platformVersionCode}]";
    }

    public string GetUserId()
    {
        return string.Format("ID: {0}-{1:D3}", ServiceId, BalloonSpawner.instance.lastConsumedServiceIndex.ToInt());
    }

    public void OpenCommunity()
    {
        Application.OpenURL("https://cafe.naver.com/balloontycoon");
    }

    public void RequestUserReview()
    {
        Platform.instance.RequestUserReview();
    }

    internal void EnableLanguageKo()
    {
        throw new NotImplementedException();
    }

    internal void EnableLanguageCh()
    {
        throw new NotImplementedException();
    }

    internal void EnableLanguageTw()
    {
        throw new NotImplementedException();
    }

    internal void EnableLanguageJa()
    {
        throw new NotImplementedException();
    }
}