using UnityEngine;
using UnityEngine.Advertisements;

public class PlatformUnityAds {
    private static ShopProductEntry shopProductEntry;
    private static ShopProductData shopProductData;

    public static void TryShowRewardedAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData) {
        TryShowAd("rewardedVideo", shopProductEntry, shopProductData);
    }

    public static void TryShowInterstitialAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData) {
        TryShowAd("video", shopProductEntry, shopProductData);
    }

    public static void TryShowAd(string placementId, ShopProductEntry shopProductEntry, ShopProductData shopProductData) {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            ConfirmPopup.instance.Open(string.Format("\\광고를 보기 위해서는 인터넷 연결이 필요합니다.".Localized()));
        } else if (Advertisement.IsReady(placementId)) {
            PlatformUnityAds.shopProductEntry = shopProductEntry;
            PlatformUnityAds.shopProductData = shopProductData;
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show(placementId, options);
        } else {
            Debug.LogError("Ad not ready! - " + placementId);
            ShowAdsErrorPopup();
        }
    }

    public static void HandleShowResult(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                PlatformAds.HandleRewarded(shopProductEntry, shopProductData, PlatformAds.AdsType.UnityAds);
                break;
            case ShowResult.Skipped:
                SushiDebug.Log("The ad was skipped before reaching the end.");
                ConfirmPopup.instance.Open(string.Format("\\광고 시청 도중 중단됐습니다.".Localized()));
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
