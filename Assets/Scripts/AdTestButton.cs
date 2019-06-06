using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdTestButton : MonoBehaviour {
    public void ShowContinueAd_UnityAds() {
        PlatformUnityAds.TryShowRewardedAd(null, null);
    }

    public void ShowContinueAd_AdMob() {
        PlatformAdMobAds.TryShowRewardedAd(null, null);
    }

    public void ShowInterstitialAd_UnityAds() {
        PlatformUnityAds.TryShowInterstitialAd(null, null);
    }

    public void ShowInterstitialAd_AdMob() {
        PlatformAdMobAds.TryShowInterstitialAd(null, null);
    }
}
