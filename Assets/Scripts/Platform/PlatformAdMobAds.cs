﻿using System;
using UnityEngine;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

public class PlatformAdMobAds {
#if GOOGLE_MOBILE_ADS
    private static ShopProductEntry shopProductEntry;
    private static ShopProductData shopProductData;
#endif

    public static void TryShowRewardedAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData) {
#if GOOGLE_MOBILE_ADS
        // Get singleton reward based video ad reference.
        var rewardBasedVideo = RewardBasedVideoAd.Instance;

        if (Application.internetReachability == NetworkReachability.NotReachable) {
            ConfirmPopup.instance.Open(string.Format("\\광고를 보기 위해서는 인터넷 연결이 필요합니다.".Localized()));
        } else if (rewardBasedVideo.IsLoaded()) {
            PlatformAdMobAds.shopProductEntry = shopProductEntry;
            PlatformAdMobAds.shopProductData = shopProductData;
            SushiDebug.LogFormat("ShowRewardedAd: shopProductEntry: {0}", shopProductEntry);
            SushiDebug.LogFormat("ShowRewardedAd: shopProductData: {0}", shopProductData);
            rewardBasedVideo.Show();
        } else {
            Debug.LogError("Ad not ready!");

            // AdMob 광고 보기 실패 시에는 Unity Ads를 폴백으로 쓴다.
            //PlatformUnityAds.TryShowRewardedAd(shopProductEntry, shopProductData);

            ShowAdsErrorPopup();
        }
#endif
    }

    public static void HandleRewarded() {
#if GOOGLE_MOBILE_ADS        
        PlatformAds.HandleRewarded(shopProductEntry, shopProductData, PlatformAds.AdsType.AdMob);
#endif
    }

    internal static void HandleFailedToLoad() {
        Debug.LogError("HandleFailedToLoad");
        // 유저에게 광고를 못불러왔다는 걸 굳이 게임 시작할 때 보여줄 필요는 없지...
        //ShortMessage.instance.ShowLocalized("광고 불러오기를 실패했습니다.");
    }

    static void ShowAdsErrorPopup() {
        ConfirmPopup.instance.Open(string.Format("\\$AdMob 광고 볼 수 없는 사유 설명$".Localized()));
    }
}
