using UnityEngine;
using System.Collections;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

[DisallowMultipleComponent]
public class PlatformAdMobAdsInit : MonoBehaviour {
#if GOOGLE_MOBILE_ADS
    private RewardBasedVideoAd rewardBasedVideo;
    public static InterstitialAd interstitial;
    bool shouldBeRewarded;
    private BannerView bannerView;

    public void Start() {
        SushiDebug.Log("PlatformAdMobAdsInit.Start()");
#if UNITY_ANDROID
        string appId = "ca-app-pub-5072035175916776~9742483955";
#elif UNITY_IOS
        string appId = "ca-app-pub-5072035175916776~2508482457";
#else
        string appId = "unexpected_platform";
#endif
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        RequestRewardBasedVideo();
        RequestInterstitial();

        // Called when an ad request has successfully loaded.
        interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        RequestBanner();
    }

    void HandleRewardBasedVideoLoaded(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoLoadedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoLoadedCoro(EventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoLoaded event received");
    }

    void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoFailedToLoadCoro(args));
    }

    IEnumerator HandleRewardBasedVideoFailedToLoadCoro(AdFailedToLoadEventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
        PlatformAdMobAds.HandleFailedToLoad();
    }

    IEnumerator HandleRewarded() {
        yield return null;
        PlatformAdMobAds.HandleRewarded();
    }

    void HandleRewardBasedVideoOpened(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoOpenedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoOpenedCoro(EventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoOpened event received");
        shouldBeRewarded = false;
        Sound.instance.StopTimeAndMuteAudioMixer();
    }

    void HandleRewardBasedVideoStarted(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoStartedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoStartedCoro(EventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoStarted event received");
    }

    void HandleRewardBasedVideoClosed(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoClosedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoClosedCoro(EventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoClosed event received");
        RequestRewardBasedVideo();
        if (shouldBeRewarded) {
            // Workaround for processing result in main thread
            StartCoroutine(HandleRewarded());
        }
        Sound.instance.ResumeToNormalTimeAndResumeAudioMixer();
    }

    void HandleRewardBasedVideoRewarded(object sender, Reward args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoRewardedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoRewardedCoro(Reward args) {
        yield return null;
        string type = args.Type;
        double amount = args.Amount;
        SushiDebug.Log("HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
        shouldBeRewarded = true;
    }

    void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoLeftApplicationCoro(args));
    }

    IEnumerator HandleRewardBasedVideoLeftApplicationCoro(EventArgs args) {
        yield return null;
        SushiDebug.Log("HandleRewardBasedVideoLeftApplication event received");
    }

    void OnDestroy() {
        // Called when an ad request has successfully loaded.
        if (rewardBasedVideo != null) {
            rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardBasedVideo.OnAdStarted -= HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
            // Called when the ad click caused the user to leave the application.
            rewardBasedVideo.OnAdLeavingApplication -= HandleRewardBasedVideoLeftApplication;
        }

        if (interstitial != null) {
            // Called when an ad request has successfully loaded.
            interstitial.OnAdLoaded -= HandleOnAdLoaded;
            // Called when an ad request failed to load.
            interstitial.OnAdFailedToLoad -= HandleOnAdFailedToLoad;
            // Called when an ad is shown.
            interstitial.OnAdOpening -= HandleOnAdOpened;
            // Called when the ad is closed.
            interstitial.OnAdClosed -= HandleOnAdClosed;
            // Called when the ad click caused the user to leave the application.
            interstitial.OnAdLeavingApplication -= HandleOnAdLeavingApplication;

            interstitial.Destroy();
        }
    }

    AdRequest DefaultAdRequest => new AdRequest.Builder()
        .AddTestDevice("F626104A61B1DF52DAFC0B5BE7F72B00") // Galaxy S6 Edge
        .Build();

    void RequestRewardBasedVideo() {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-5072035175916776/5803238943";
#elif UNITY_IOS
        string adUnitId = "ca-app-pub-5072035175916776/9016505898";
#else
        string adUnitId = "unexpected_platform";
#endif
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(DefaultAdRequest, adUnitId);
    }

    void RequestInterstitial() {
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
            string adUnitId = "unexpected_platform";
#endif
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);

        // Load the interstitial with the request.
        interstitial.LoadAd(DefaultAdRequest);
    }

    public void HandleOnAdLoaded(object sender, EventArgs args) {
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args) {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args) {
        // Workaround for processing result in main thread
        StartCoroutine(HandleAdClosedCoro());
    }

    IEnumerator HandleAdClosedCoro() {
        yield return null;
        SushiDebug.Log("HandleAdClosedCoro event received");
        PlatformAds.HandleRewarded_Video(null, null, PlatformAds.AdsType.AdMob);
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args) {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }

    private void RequestBanner() {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
    }
#endif // #if GOOGLE_MOBILE_ADS
}
