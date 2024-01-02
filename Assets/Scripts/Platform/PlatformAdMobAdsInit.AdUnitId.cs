partial class PlatformAdMobAdsInit
{
    static string RewardedAdUnitId
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

    static string InterstitialAdUnitId
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

    static string BannerAdUnitId
    {
        get
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
            return adUnitId;
        }
    }
}