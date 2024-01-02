using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

[DisallowMultipleComponent]
public class PlatformAdMobAdsInit : MonoBehaviour
{
    public static PlatformAdMobAdsInit instance;

    public void StartShowBanner()
    {
#if GOOGLE_MOBILE_ADS
        if (bannerView == null) CreateBanner();

        if (isBannerViewLoaded) return;

        // Create an empty ad request.
        var request = new AdRequest.Builder()
            //.AddTestDevice("F626104A61B1DF52DAFC0B5BE7F72B00") // Galaxy S6 Edge
            .Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
        isBannerViewLoaded = true;
#endif
    }

    public void HideBanner()
    {
#if GOOGLE_MOBILE_ADS
        if (bannerView != null) bannerView.Hide();
#endif
    }

#if GOOGLE_MOBILE_ADS
    public RewardedAd rewardBasedVideo;
    public static InterstitialAd interstitial;
    bool shouldBeRewarded;
    BannerView bannerView;
    bool isBannerViewLoaded;

    public void Start()
    {
        BalloonDebug.Log("PlatformAdMobAdsInit.Start()");
        if (PlatformIapManager.instance.NoAdsPurchased)
        {
            Debug.Log("PlatformIapManager.NoAdsPurchased = true (thank you!)");
        }
        else
        {
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(_ => { });

            var testDeviceList = new List<string>
            {
                //"751709a03251817c6a3d7d3f7072ec57" // iPhone 6s
            };
            var requestConfiguration = new RequestConfiguration.Builder().SetTestDeviceIds(testDeviceList).build();
            MobileAds.SetRequestConfiguration(requestConfiguration);

            // 광고 사운드 끄기
            MobileAds.SetApplicationMuted(true);

            // Get singleton reward based video ad reference.
            rewardBasedVideo = new(RewardedAdUnitId);

            // Called when an ad request has successfully loaded.
            rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnUserEarnedReward += HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;

            RequestRewardBasedVideo();

            // Initialize an InterstitialAd.
            interstitial = new(InterstitialAdUnitId);

            // Called when an ad request has successfully loaded.
            interstitial.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            // Called when an ad is shown.
            interstitial.OnAdOpening += HandleOnAdOpened;
            // Called when the ad is closed.
            interstitial.OnAdClosed += HandleOnAdClosed;

            RequestInterstitial();
        }

#if UNITY_IOS
        var trackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"ATTrackingStatusBinding.GetAuthorizationTrackingStatus()={trackingStatus}");
        switch (trackingStatus)
        {
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                //AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                //AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(false);
                break;
        }
#endif
    }

    void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoLoadedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoLoadedCoro(EventArgs args)
    {
        yield return null;
        BalloonDebug.Log("HandleRewardBasedVideoLoaded event received");
    }

    void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoFailedToLoadCoro(args));
    }

    IEnumerator HandleRewardBasedVideoFailedToLoadCoro(AdFailedToLoadEventArgs args)
    {
        var errorStr = args.LoadAdError.ToString();
        yield return null;
        BalloonDebug.Log("HandleRewardBasedVideoFailedToLoad event received with message: " + errorStr);
        PlatformAdMobAds.HandleFailedToLoad();
    }

    IEnumerator HandleRewarded()
    {
        yield return null;
        PlatformAdMobAds.HandleRewarded();
    }

    void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoOpenedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoOpenedCoro(EventArgs args)
    {
        yield return null;
        BalloonDebug.Log("HandleRewardBasedVideoOpened event received");
        shouldBeRewarded = false;
        BalloonSound.instance.StopTimeAndMuteAudioMixer();
    }

    void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoStartedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoStartedCoro(EventArgs args)
    {
        yield return null;
        BalloonDebug.Log("HandleRewardBasedVideoStarted event received");
    }

    void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoClosedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoClosedCoro(EventArgs args)
    {
        yield return null;
        BalloonDebug.Log("HandleRewardBasedVideoClosed event received");
        RequestRewardBasedVideo();
        if (shouldBeRewarded) // Workaround for processing result in main thread
            StartCoroutine(HandleRewarded());
        BalloonSound.instance.ResumeToNormalTimeAndResumeAudioMixer();
    }

    IEnumerator HandleAdOpenedCoro(EventArgs args)
    {
        yield return null;
        BalloonDebug.Log("HandleAdClosedCoro event received");
        BalloonSound.instance.StopTimeAndMuteAudioMixer();
    }

    void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        // Workaround for processing result in main thread
        StartCoroutine(HandleRewardBasedVideoRewardedCoro(args));
    }

    IEnumerator HandleRewardBasedVideoRewardedCoro(Reward args)
    {
        yield return null;
        var type = args.Type;
        var amount = args.Amount;
        BalloonDebug.Log("HandleRewardBasedVideoRewarded event received for " + amount + " " + type);
        shouldBeRewarded = true;
    }

    void OnDestroy()
    {
        // Called when an ad request has successfully loaded.
        if (rewardBasedVideo != null)
        {
            rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnUserEarnedReward -= HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
        }

        if (interstitial != null)
        {
            // Called when an ad request has successfully loaded.
            interstitial.OnAdLoaded -= HandleOnAdLoaded;
            // Called when an ad request failed to load.
            interstitial.OnAdFailedToLoad -= HandleOnAdFailedToLoad;
            // Called when an ad is shown.
            interstitial.OnAdOpening -= HandleOnAdOpened;
            // Called when the ad is closed.
            interstitial.OnAdClosed -= HandleOnAdClosed;

            interstitial.Destroy();
        }

        if (bannerView != null)
        {
            // Called when an ad request has successfully loaded.
            bannerView.OnAdLoaded -= HandleOnBannerAdLoaded;
            // Called when an ad request failed to load.
            bannerView.OnAdFailedToLoad -= HandleOnBannerAdFailedToLoad;
            // Called when an ad is clicked.
            bannerView.OnAdOpening -= HandleOnBannerAdOpened;
            // Called when the user returned from the app after an ad click.
            bannerView.OnAdClosed -= HandleOnBannerAdClosed;

            bannerView.Destroy();
        }
    }

    AdRequest DefaultAdRequest => new AdRequest.Builder()
        //.AddTestDevice("F626104A61B1DF52DAFC0B5BE7F72B00") // Galaxy S6 Edge
        .Build();

    string RewardedAdUnitId
    {
        get
        {
#if TEST_ADS
#if UNITY_ANDROID
            const string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            const string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            const string adUnitId = "unexpected_platform";
#endif
#else
#if UNITY_ANDROID
            const string adUnitId = "ca-app-pub-5072035175916776/5803238943";
#elif UNITY_IOS
            const string  adUnitId = "ca-app-pub-5072035175916776/9016505898";
#else
            const string adUnitId = "unexpected_platform";
#endif
#endif
            return adUnitId;
        }
    }

    void RequestRewardBasedVideo()
    {
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(DefaultAdRequest);
    }

    string InterstitialAdUnitId
    {
        get
        {
#if TEST_ADS
#if UNITY_ANDROID
            const string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
            const string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
            const string adUnitId = "unexpected_platform";
#endif
#else
#if UNITY_ANDROID
            const string adUnitId = "ca-app-pub-5072035175916776/8453453014";
#elif UNITY_IPHONE
            const string adUnitId = "ca-app-pub-5072035175916776/7626260968";
#else
            const string adUnitId = "unexpected_platform";
#endif
#endif
            return adUnitId;
        }
    }

    void RequestInterstitial()
    {
        // Load the interstitial with the request.
        interstitial.LoadAd(DefaultAdRequest);
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        print("HandleFailedToReceiveAd event received with message: " + args.LoadAdError);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        print("HandleOnAdOpened event received");
        // Workaround for processing result in main thread
        StartCoroutine(HandleAdOpenedCoro(args));
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        print("HandleOnAdClosed event received");
        // Workaround for processing result in main thread
        StartCoroutine(HandleAdClosedCoro());
    }

    IEnumerator HandleAdClosedCoro()
    {
        yield return null;
        BalloonDebug.Log("HandleAdClosedCoro event received");
        PlatformAds.HandleRewarded_Video(null, null, PlatformAds.AdsType.AdMob);
        BalloonSound.instance.ResumeToNormalTimeAndResumeAudioMixer();
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        print("HandleAdLeavingApplication event received");
    }

    void CreateBanner()
    {
#if TEST_ADS
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif
#else
#if UNITY_ANDROID
        var adUnitId = "ca-app-pub-5072035175916776/4198513232";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-5072035175916776/5602732856";
#else
        string adUnitId = "unexpected_platform";
#endif
#endif
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Called when an ad request has successfully loaded.
        bannerView.OnAdLoaded += HandleOnBannerAdLoaded;
        // Called when an ad request failed to load.
        bannerView.OnAdFailedToLoad += HandleOnBannerAdFailedToLoad;
        // Called when an ad is clicked.
        bannerView.OnAdOpening += HandleOnBannerAdOpened;
        // Called when the user returned from the app after an ad click.
        bannerView.OnAdClosed += HandleOnBannerAdClosed;

        // 나중에 인앱 결제 상품 현황 보고 광고를 띄울지 말지를 결정해야 한다.
        // 여기서 무조건 띄우지 말자.
        //StartShowBanner();
    }

    public void HandleOnBannerAdLoaded(object sender, EventArgs args)
    {
        print("HandleAdLoaded event received");
    }

    public void HandleOnBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        print("HandleFailedToReceiveAd event received with message: " + args.LoadAdError);
    }

    public void HandleOnBannerAdOpened(object sender, EventArgs args)
    {
        print("HandleAdOpened event received");
    }

    public void HandleOnBannerAdClosed(object sender, EventArgs args)
    {
        print("HandleAdClosed event received");
    }

    public void HandleOnBannerAdLeavingApplication(object sender, EventArgs args)
    {
        print("HandleAdLeavingApplication event received");
    }
#endif // #if GOOGLE_MOBILE_ADS
}