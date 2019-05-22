using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {
    [SerializeField] CanvasGroup holdToThrustVertically = null;
    [SerializeField] CanvasGroup dragToSteerHorizontally = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Start() {
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        hotairBalloon.VerticallyStationary = true;
    }

    void Update() {
        holdToThrustVertically.alpha = Mathf.Clamp(12.0f - hotairBalloon.Y, 0, 0.8f);
        dragToSteerHorizontally.alpha = Mathf.Clamp(1.0f - Mathf.Abs((hotairBalloon.Y - 20.0f) / 10.0f), 0, 0.8f);
    }
}
