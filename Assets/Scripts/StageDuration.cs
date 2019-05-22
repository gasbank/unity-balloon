using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageDuration : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Awake() {
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
    }

    void Update() {
        if (hotairBalloon.IsGameOver == false && hotairBalloon.IsStageFinished == false) {
            text.text = (DateTime.MinValue + TimeSpan.FromSeconds(Time.timeSinceLevelLoad)).ToString("mm:ss.fff");
        }
    }
}
