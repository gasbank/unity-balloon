using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageDuration : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] float stageElapsedTime = 0;

    void Awake() {
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        stageElapsedTime = 0;
    }

    void Update() {
        if (hotairBalloon.IsGameOver == false && hotairBalloon.IsStageFinished == false && hotairBalloon.IsTitleVisible == false) {
            stageElapsedTime += Time.deltaTime;
        }
        text.text = (DateTime.MinValue + TimeSpan.FromSeconds(stageElapsedTime)).ToString("mm:ss.fff");
    }
}
