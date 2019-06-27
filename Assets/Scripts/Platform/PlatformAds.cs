using UnityEngine;

public class PlatformAds {
    public enum AdsType {
        AdMob,
        UnityAds,
    }

    public static int stageNumber;

    public static void HandleRewarded_RewardedVideo(ShopProductEntry shopProductEntry, ShopProductData shopProductData, AdsType adsType) {
        SushiDebug.Log("The ad was successfully shown. (Rewarded Video)");
        SushiDebug.LogFormat("HandleShowResult: shopProductEntry: {0}", shopProductEntry);
        SushiDebug.LogFormat("HandleShowResult: shopProductData: {0}", shopProductData);
        // 체크포인트부터 이어서 플레이한다.
        // HotairBalloon.initialPositionY를 리셋하지 않으면
        // 이어서 되도록 해 놓았다.
        Bootstrap.ReloadCurrentScene();
    }

    public static void HandleRewarded_Video(ShopProductEntry shopProductEntry, ShopProductData shopProductData, AdsType adsType) {
        SushiDebug.Log("The ad was successfully shown. (Video)");
        SushiDebug.LogFormat("HandleShowResult: shopProductEntry: {0}", shopProductEntry);
        SushiDebug.LogFormat("HandleShowResult: shopProductData: {0}", shopProductData);
        // 이번 스테이지를 처음부터 새로 시작한다.
        //HotairBalloon.InitialPositionY = 0;
        //Bootstrap.LoadStageScene(stageNumber);
    }
}
