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
        text.text = (DateTime.MinValue + TimeSpan.FromSeconds(hotairBalloon.StageElapsedTime)).ToString("mm:ss.fff");
    }
}
