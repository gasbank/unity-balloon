using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {
    [SerializeField] CanvasGroup holdToThrustVertically = null;
    [SerializeField] CanvasGroup dragToSteerHorizontally = null;
    [SerializeField] CanvasGroup takeGas = null;
    [SerializeField] CanvasGroup hitRepulsive = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] int tutorialTemplate = 1;

    public int TutorialTemplate {
        get => tutorialTemplate;
        set => tutorialTemplate = value;
    }

    void Start() {
        UpdateReferences();
        hotairBalloon.VerticallyStationary = true;
    }

    public void UpdateReferences() {
        hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
    }

    void Update() {
        if (TutorialTemplate == 1) {
            if (hotairBalloon.IsTitleVisible == false) {
                if (hotairBalloon.IsGameOver == false) {
                    holdToThrustVertically.alpha = Mathf.Clamp(12.0f - hotairBalloon.Y, 0, 0.8f);
                    dragToSteerHorizontally.alpha = Mathf.Clamp(1.0f - Mathf.Abs((hotairBalloon.Y - 20.0f) / 10.0f), 0, 0.8f);
                    takeGas.alpha = Mathf.Clamp(1.0f - Mathf.Abs((hotairBalloon.Y - 40.0f) / 10.0f), 0, 0.8f);
                    hitRepulsive.alpha = 0;
                } else {
                    holdToThrustVertically.alpha = 0;
                    dragToSteerHorizontally.alpha = 0;
                    takeGas.alpha = 0;
                    hitRepulsive.alpha = 0;
                }
            }
        } else {
            if (hotairBalloon.IsTitleVisible == false) {
                if (hotairBalloon.IsGameOver == false) {
                    holdToThrustVertically.alpha = 0;
                    dragToSteerHorizontally.alpha = 0;
                    takeGas.alpha = 0;
                    hitRepulsive.alpha = Mathf.Clamp(12.0f - hotairBalloon.Y, 0, 0.8f);
                } else {
                    holdToThrustVertically.alpha = 0;
                    dragToSteerHorizontally.alpha = 0;
                    takeGas.alpha = 0;
                    hitRepulsive.alpha = 0;
                }
            }
        }
    }
}
