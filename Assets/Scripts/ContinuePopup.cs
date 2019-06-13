using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class ContinuePopup : MonoBehaviour {
    [SerializeField] Slider stageProgressSlider = null;
    [SerializeField] Text stageName = null;
    [SerializeField] Text waitRemainTime = null;
    [SerializeField] Slider waitTimeSlider = null;
    [SerializeField] CanvasGroup noThanksCanvasGroup = null;
    [SerializeField] float totalWaitTime = 5.0f;
    [SerializeField] float noThanksAppearTime = 2.0f;
    [SerializeField] Subcanvas subcanvas = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] Stage stage = null;
    
    float WaitTimeValue {
        get => waitTimeSlider.value;
        set => waitTimeSlider.value = value;
    }

    public bool IsOpen => subcanvas.IsOpen;

    public void Open() => subcanvas.Open();

    // void OnValidate() {
    //     subcanvas = GetComponent<Subcanvas>();
    // }

    void OnEnable() {
        OpenPopup();
    }

    void OpenPopup() {
        hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
        stage = GameObject.FindObjectOfType<Stage>();
        stageName.text = Bootstrap.CurrentStageName;
        if (PlatformIapManager.instance.NoAdsPurchased) {
            waitTimeSlider.minValue = 0;
            waitTimeSlider.maxValue = totalWaitTime;
            waitTimeSlider.value = 0;
            noThanksCanvasGroup.interactable = true;
            noThanksCanvasGroup.alpha = 1;
        } else {
            waitTimeSlider.minValue = 0;
            waitTimeSlider.maxValue = totalWaitTime;
            waitTimeSlider.value = totalWaitTime;
            noThanksCanvasGroup.interactable = false;
            noThanksCanvasGroup.alpha = 0;
        }
        waitRemainTime.text = "";
        stageProgressSlider.value = (stage != null && hotairBalloon != null) ? (hotairBalloon.highestY / stage.TotalStageLength) : 0;
    }

    void ClosePopup() {

    }

    void Update() {
        if (subcanvas.IsOpen == false) {
            return;
        }

        if (PlatformIapManager.instance.NoAdsPurchased == false) {
            WaitTimeValue = Mathf.Clamp(WaitTimeValue - Time.deltaTime, waitTimeSlider.minValue, waitTimeSlider.maxValue);
            waitRemainTime.text = WaitTimeValue.ToString("F0");
            if (WaitTimeValue <= 0) {
                OnNoThanksButton();
                subcanvas.Close();
            }
            if (noThanksCanvasGroup.interactable == false && totalWaitTime - WaitTimeValue > noThanksAppearTime) {
                noThanksCanvasGroup.interactable = true;
                noThanksCanvasGroup.alpha = 1.0f;
            }
        }
    }
    
    public void OnContinueButton() {
        if (PlatformIapManager.instance.NoAdsPurchased) {
            PlatformAds.HandleRewarded_RewardedVideo(null, null, PlatformAds.AdsType.AdMob);
        } else {
            // 에디터에서 테스트하기 쉽도록 에디터에서는 Unity Ads를,
            // 실제 기기에서는 Google AdMob을 쓴다.
            if (Application.isEditor) {
                PlatformUnityAds.TryShowRewardedAd(null, null);
            } else {
                PlatformAdMobAds.TryShowRewardedAd(null, null);
            }
        }
    }

    public void OnNoThanksButton() {
        if (PlatformIapManager.instance.NoAdsPurchased) {
            PlatformAds.HandleRewarded_Video(null, null, PlatformAds.AdsType.AdMob);
        } else {
            // 에디터에서 테스트하기 쉽도록 에디터에서는 Unity Ads를,
            // 실제 기기에서는 Google AdMob을 쓴다.
            if (Application.isEditor) {
                PlatformUnityAds.TryShowInterstitialAd(null, null, Bootstrap.CurrentStageNumber);
            } else {
                PlatformAdMobAds.TryShowInterstitialAd(null, null, Bootstrap.CurrentStageNumber);
            }
        }
    }
}
