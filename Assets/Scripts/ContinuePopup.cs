﻿using System.Collections;
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
    [SerializeField] CanvasGroup canvasGroup = null;
    [SerializeField] float canvasGroupAlphaTarget = 0;
    [SerializeField] float canvasGroupAlphaVelocity = 0;

    readonly static private float defaultFixedDeltaTime = 0.02f;

    float WaitTimeValue {
        get => waitTimeSlider.value;
        set => waitTimeSlider.value = value;
    }

    public bool IsOpen => subcanvas.IsOpen;

    public void Open() => subcanvas.Open();

    // void OnValidate() {
    //     subcanvas = GetComponent<Subcanvas>();
    // }

    void Awake() {
        RevertToDefaultTimeScale();
    }

    void OnEnable() {
        //OpenPopup();
    }

    void OpenPopup() {
        canvasGroupAlphaTarget = 1;
        canvasGroup.alpha = 0;
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
        var vignetteImage = GameObject.FindObjectOfType<VignetteImage>();
        if (vignetteImage != null) {
            vignetteImage.HideShowContinuePopupImmediatelyButton();
        }
    }

    void ClosePopup() {
        var vignetteImage = GameObject.FindObjectOfType<VignetteImage>();
        if (vignetteImage != null) {
            vignetteImage.ResetShowContinuePopupImmediatelyButton();
        }
    }

    void Update() {
        if (subcanvas.IsOpen == false) {
            return;
        }

        canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, canvasGroupAlphaTarget, ref canvasGroupAlphaVelocity, 0.05f, 2.0f, Time.unscaledDeltaTime);
        //canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, canvasGroupAlphaTarget, Time.deltaTime * 5);
        // 슬로우 효과가 매 프레임마다 Time.timeScale을 덮어쓰는 것 때문에
        // 로직이 복잡해진다. (iOS에서 광고 시청 시 Time.timeScale을 0으로 바꿔도 이 기능으로 0 아닌 다른 값이 됨)
        // 기능 일단 끈다.
        //SetToSlowTimeScale(Mathf.Pow(canvasGroup.alpha, 10.0f));

        if (PlatformIapManager.instance.NoAdsPurchased == false) {
            WaitTimeValue = Mathf.Clamp(WaitTimeValue - Time.deltaTime, waitTimeSlider.minValue, waitTimeSlider.maxValue);
            waitRemainTime.text = WaitTimeValue.ToString("F0");
            if (WaitTimeValue <= 0) {
                OnNoThanksButton();
                // 여기서 Close를 했더니 게임 로직에서 바로 다시 열어버리는 것 같다.
                // 닫지 말자. 어차피 신 전환으로 없어질 것이다.
                //subcanvas.Close();
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
            if (Application.isEditor || Application.isMobilePlatform == false) {
                PlatformUnityAds.TryShowRewardedAd(null, null);
            } else {
                PlatformAdMobAds.TryShowRewardedAd(null, null);
            }
        }
    }

    public void OnNoThanksButton() {
        RevertToDefaultTimeScale();

        if (Bootstrap.CurrentStageNumber <= 0) {
            // 정규 신이 아닌 경우 (테스트 신인 경우), 그냥 그 테스트 신을 다시 로드한다.
            // 이번 스테이지를 처음부터 새로 시작한다.
            HotairBalloon.InitialPositionY = 0;
            Bootstrap.ReloadCurrentScene();
        } else {
            // 에디터에서 테스트하기 쉽도록 에디터에서는 Unity Ads를,
            // 실제 기기에서는 Google AdMob을 쓴다.
            if (PlatformIapManager.instance.NoAdsPurchased) {
                ProceedNoThanksWithoutAds();
            } else {
                //  if (Random.Range(0, 100) < 40) 
                //  {
                //     ProceedNoThanksWithAds();
                //} else    처음에는 혜자로 가야한다. 500~1000다운로드까지는 광고 최소화 하다가 유저풀 모이면 광고 서서히 늘려나간다.
                // {
                ProceedNoThanksWithoutAds();
                //  }
            }
        }
    }

    private static void ProceedNoThanksWithAds() {
        if (Application.isEditor) {
            PlatformUnityAds.TryShowInterstitialAd(null, null, Bootstrap.CurrentStageNumberSafe);
        } else {
            PlatformAdMobAds.TryShowInterstitialAd(null, null, Bootstrap.CurrentStageNumberSafe);
        }
    }

    private static void ProceedNoThanksWithoutAds() {
        PlatformAds.stageNumber = Bootstrap.CurrentStageNumberSafe;
        //PlatformAds.HandleRewarded_Video(null, null, PlatformAds.AdsType.AdMob);
        HotairBalloon.InitialPositionY = 0;
        Bootstrap.LoadStageScene(PlatformAds.stageNumber);
    }

    private void SetToSlowTimeScale(float newTimeScale) {
        SushiDebug.Log("SetToSlowTimeScale() called.");
        Time.timeScale = newTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }

    private void RevertToDefaultTimeScale() {
        SushiDebug.Log("RevertToDefaultTimeScale() called.");
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }

    private void StopTimeScale() {
        SushiDebug.Log("StopTimeScale() called.");
        Time.timeScale = 0.0f;
    }
}
