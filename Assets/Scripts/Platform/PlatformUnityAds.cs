using UnityEngine;
using UnityEngine.Advertisements;

public class PlatformUnityAds {
    private static ShopProductEntry shopProductEntry;
    private static ShopProductData shopProductData;
    
    public static void TryShowRewardedAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData) {
        TryShowAd("rewardedVideo", shopProductEntry, shopProductData, new ShowOptions { resultCallback = HandleShowResult_RewardedVideo });
    }

    public static void TryShowInterstitialAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData, int stageNumber) {
        PlatformAds.stageNumber = stageNumber;
        TryShowAd("video", shopProductEntry, shopProductData, new ShowOptions { resultCallback = HandleShowResult_Video });
    }

    private static void TryShowAd(string placementId, ShopProductEntry shopProductEntry, ShopProductData shopProductData, ShowOptions options) {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            ConfirmPopup.instance.Open(string.Format("\\광고를 보기 위해서는 인터넷 연결이 필요합니다.".Localized()));
        } else if (Advertisement.IsReady(placementId)) {
            PlatformUnityAds.shopProductEntry = shopProductEntry;
            PlatformUnityAds.shopProductData = shopProductData;
            Advertisement.Show(placementId, options);
        } else {
            Debug.LogError("Ad not ready! - " + placementId);
            ShowAdsErrorPopup();
        }
    }

    public static void HandleShowResult_RewardedVideo(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                PlatformAds.HandleRewarded_RewardedVideo(shopProductEntry, shopProductData, PlatformAds.AdsType.UnityAds);
                break;
            case ShowResult.Skipped:
                SushiDebug.Log("The ad was skipped before reaching the end. (Rewarded Video)");
                ConfirmPopup.instance.Open(string.Format("\\광고 시청 도중 중단됐습니다. (Rewarded Video)".Localized()));
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                ShowAdsErrorPopup();
                break;
        }
    }

    public static void HandleShowResult_Video(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
            case ShowResult.Skipped:
                PlatformAds.HandleRewarded_Video(shopProductEntry, shopProductData, PlatformAds.AdsType.UnityAds);
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                ShowAdsErrorPopup();
                break;
        }
    }

    static void ShowAdsErrorPopup() {
        ConfirmPopup.instance.Open(string.Format("\\$Unity Ads 광고 볼 수 없는 사유 설명$".Localized()));
    }
}
