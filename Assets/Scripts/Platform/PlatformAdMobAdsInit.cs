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
        bannerView.LoadAd(new());
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
                HandleFailedToLoad();
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
            
            PlatformAds.ExecuteInterstitialReward(null, null, PlatformAds.AdsType.AdMob);
        };

        interstitial.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " + "with error : " + error);

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

    public void TryShowRewardedAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData)
    {
#if GOOGLE_MOBILE_ADS
        // Get singleton reward based video ad reference.
        var rewardBasedVideo = Ad;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ConfirmPopup.instance.Open(string.Format("\\인터넷 연결이 필요합니다.".Localized()));
        }
        else if (rewardBasedVideo.CanShowAd())
        {
            this.shopProductEntry = shopProductEntry;
            this.shopProductData = shopProductData;
            BalloonDebug.LogFormat("ShowRewardedAd: shopProductEntry: {0}", shopProductEntry);
            BalloonDebug.LogFormat("ShowRewardedAd: shopProductData: {0}", shopProductData);
            rewardBasedVideo.Show(reward =>
            {
#if GOOGLE_MOBILE_ADS
                PlatformAds.ExecuteRewardedVideoReward(shopProductEntry, shopProductData, PlatformAds.AdsType.AdMob);
#endif
            });
        }
        else
        {
            Debug.LogError("Ad not ready!");
            ConfirmPopup.instance.Open(string.Format("\\광고가 아직 준비되지 않았습니다.".Localized()));
        }
#endif
    }

    public void TryShowInterstitialAd(ShopProductEntry shopProductEntry, ShopProductData shopProductData, int stageNumber)
    {
        PlatformAds.stageNumber = stageNumber;
#if GOOGLE_MOBILE_ADS
        if (interstitial.CanShowAd())
        {
            interstitial.Show();
        }
        else
        {
            PlatformAds.ExecuteInterstitialReward(null, null, PlatformAds.AdsType.AdMob);
        }
#endif
    }

    void HandleFailedToLoad()
    {
        Debug.LogError("HandleFailedToLoad");
        // 유저에게 광고를 못불러왔다는 걸 굳이 게임 시작할 때 보여줄 필요는 없지...
        //ShortMessage.instance.ShowLocalized("광고 불러오기를 실패했습니다.");
    }
#if GOOGLE_MOBILE_ADS
    ShopProductEntry shopProductEntry;
    ShopProductData shopProductData;
#endif
}