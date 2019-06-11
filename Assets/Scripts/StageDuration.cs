using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageDuration : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Awake() {
        UpdateReferences();
    }

    public void UpdateReferences() {
        hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
    }

    void Update() {
        if (hotairBalloon != null) {
            text.text = (DateTime.MinValue + TimeSpan.FromSeconds(hotairBalloon.StageElapsedTime)).ToString("mm:ss.fff");
        }
    }
}
