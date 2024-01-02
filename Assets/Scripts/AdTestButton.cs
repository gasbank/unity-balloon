using UnityEngine;

public class AdTestButton : MonoBehaviour
{
    public void ShowContinueAd_AdMob()
    {
        PlatformAdMobAds.TryShowRewardedAd(null, null);
    }

    public void ShowInterstitialAd_AdMob()
    {
        PlatformAdMobAds.TryShowInterstitialAd(null, null, 1);
    }
}