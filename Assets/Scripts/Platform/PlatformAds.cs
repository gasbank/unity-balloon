using UnityEngine;

public class PlatformAds {
    public enum AdsType {
        AdMob,
        UnityAds,
    }

    public static void HandleRewarded(ShopProductEntry shopProductEntry, ShopProductData shopProductData, AdsType adsType) {
        SushiDebug.Log("The ad was successfully shown.");
        SushiDebug.LogFormat("HandleShowResult: shopProductEntry: {0}", shopProductEntry);
        SushiDebug.LogFormat("HandleShowResult: shopProductData: {0}", shopProductData);
    }
}
