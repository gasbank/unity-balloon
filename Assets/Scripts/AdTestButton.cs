using UnityEngine;

public class AdTestButton : MonoBehaviour
{
    public void ShowContinueAd_AdMob()
    {
        PlatformAdMobAdsInit.instance.TryShowRewardedAd(null, null);
    }

    public void ShowInterstitialAd_AdMob()
    {
        PlatformAdMobAdsInit.instance.TryShowInterstitialAd(null, null, 1);
    }
}