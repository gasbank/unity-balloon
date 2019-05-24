using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {
    [SerializeField] CanvasGroup holdToThrustVertically = null;
    [SerializeField] CanvasGroup dragToSteerHorizontally = null;
    [SerializeField] CanvasGroup takeGas = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Start() {
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        hotairBalloon.VerticallyStationary = true;
    }

    void Update() {
        if (hotairBalloon.IsTitleVisible == false) {
            if (hotairBalloon.IsGameOver == false) {
                holdToThrustVertically.alpha = Mathf.Clamp(12.0f - hotairBalloon.Y, 0, 0.8f);
                dragToSteerHorizontally.alpha = Mathf.Clamp(1.0f - Mathf.Abs((hotairBalloon.Y - 20.0f) / 10.0f), 0, 0.8f);
                takeGas.alpha = Mathf.Clamp(1.0f - Mathf.Abs((hotairBalloon.Y - 40.0f) / 10.0f), 0, 0.8f);
            } else {
                holdToThrustVertically.alpha = 0;
                dragToSteerHorizontally.alpha = 0;
                takeGas.alpha = 0;
            }
        }
    }
}
