#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

[DisallowMultipleComponent]
public partial class PlatformAdMobAdsInit : MonoBehaviour
{
    public static PlatformAdMobAdsInit instance;

    public void StartShowBanner()
    {
#if GOOGLE_MOBILE_ADS
        RequestNewBannerAd();
#endif        
    }
    
    public void HideBanner()
    {
#if GOOGLE_MOBILE_ADS
        bannerView?.Hide();
#endif
    }

#if GOOGLE_MOBILE_ADS
    public RewardedAd Ad { get; private set; }
    public InterstitialAd interstitial { get; private set; }
    BannerView bannerView;
    bool isBannerViewLoaded;

    void Start()
    {
        BalloonDebug.Log("PlatformAdMobAdsInit.Start()");
        if (PlatformIapManager.instance.NoAdsPurchased)
        {
            Debug.Log("PlatformIapManager.NoAdsPurchased = true (thank you!)");
        }
        else
        {
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(_ =>
            {
                var requestConfiguration = new RequestConfiguration();
                MobileAds.SetRequestConfiguration(requestConfiguration);

                // 광고 사운드 끄기
                MobileAds.SetApplicationMuted(true);

                RequestNewRewardedAd();

                RequestNewInterstitialAd();    
            });
        }

#if UNITY_IOS
        var trackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"ATTrackingStatusBinding.GetAuthorizationTrackingStatus()={trackingStatus}");
        switch (trackingStatus)
        {
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                //AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                //AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(false);
                break;
        }
#endif
    }
    void RequestNewBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
        
        bannerView = new(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        
        BalloonDebug.Log("Loading banner ad.");
        bannerView.LoadAd(new ());
    }
    
    void RequestNewInterstitialAd()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }
        
        InterstitialAd.Load(InterstitialAdUnitId, new(), (ad, error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                Debug.LogError("interstitial ad failed to load an ad " + "with error : " + error);
                return;
            }

            BalloonDebug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

            interstitial = ad;

            RegisterReloadHandlerForInterstitial();
        });
    }

    void RequestNewRewardedAd()
    {
        BalloonDebug.Log("RequestNewRewardedAd: Requesting new ad...");

        // 기존 객체가 있으면 명시적으로 없애준다.
        Ad?.Destroy();
        Ad = null;

        RewardedAd.Load(RewardedAdUnitId, new(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError(error);
                PlatformAdMobAds.HandleFailedToLoad();
                return;
            }

            BalloonDebug.Log("A new ad received!");

            Ad = ad;
            
            RegisterEventHandlersForRewardedAd();
        });
    }

    void OnDestroy()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }
    }

    void RegisterReloadHandlerForInterstitial()
    {
        interstitial.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial Ad full screen content closed.");

            RequestNewInterstitialAd();
        };

        interstitial.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);

            RequestNewInterstitialAd();
        };
    }

    void RegisterEventHandlersForRewardedAd()
    {
        // Raised when the ad is estimated to have earned money.
        Ad.OnAdPaid += adValue =>
        {
            BalloonDebug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        // Raised when an impression is recorded for an ad.
        Ad.OnAdImpressionRecorded += () =>
        {
            BalloonDebug.Log("Rewarded ad recorded an impression.");
        };

        // Raised when a click is recorded for an ad.
        Ad.OnAdClicked += () =>
        {
            BalloonDebug.Log("Rewarded ad was clicked.");
        };

        // Raised when an ad opened full screen content.
        Ad.OnAdFullScreenContentOpened += () =>
        {
            BalloonDebug.Log("Rewarded ad full screen content opened.");
        };

        // Raised when the ad closed full screen content.
        Ad.OnAdFullScreenContentClosed += () =>
        {
            BalloonDebug.Log("Rewarded ad full screen content closed.");

            RequestNewRewardedAd();
        };

        // Raised when the ad failed to open full screen content.
        Ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError($"Rewarded ad failed to open full screen content with error : {error}");

            RequestNewRewardedAd();
        };
    }
#endif // #if GOOGLE_MOBILE_ADS
}