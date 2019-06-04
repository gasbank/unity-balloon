﻿using UnityEngine;
using System.Collections;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

[DisallowMultipleComponent]
public class PlatformAdMobAdsInit : MonoBehaviour {
#if GOOGLE_MOBILE_ADS
    private RewardBasedVideoAd rewardBasedVideo;
    bool shouldBeRewarded;

    public void Start() {
        SushiDebug.Log("PlatformAdMobAdsInit.Start()");
#if UNITY_ANDROID
        string appId = "ca-app-pub-5072035175916776~8524864268";
#elif UNITY_IOS
        string appId = "ca-app-pub-5072035175916776~1063454449";
        // TEST
        //string appId = "ca-app-pub-3940256099942544~3347511713";
#else
        //string appId = "unexpected_platform";
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

    void RequestRewardBasedVideo() {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-5072035175916776/4885002150";
#elif UNITY_IOS
        string adUnitId = "ca-app-pub-5072035175916776/5048653683";
        // TEST
        //string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
        //string adUnitId = "unexpected_platform";
#endif

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(request, adUnitId);
    }
#endif
}
