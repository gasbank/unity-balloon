using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteImage : MonoBehaviour {
    [SerializeField] Image image = null;
    [SerializeField] Button showContinuePopupImmediatelyButton = null;
    [SerializeField] bool continued = false;

    public void SetVignetteColor(Color c) {
        image.color = c;
        if (continued == false) {
            showContinuePopupImmediatelyButton.gameObject.SetActive(c.a != 0);
        }
    }
    
    public void ShowContinuePopupImmediately() {
        var hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
        if (hotairBalloon != null) {
            hotairBalloon.DieImmediately();
        }
        HideShowContinuePopupImmediatelyButton();
    }

    public void HideShowContinuePopupImmediatelyButton() {
        SushiDebug.Log("HideShowContinuePopupImmediatelyButton");
        continued = true;
        showContinuePopupImmediatelyButton.gameObject.SetActive(false);
    }

    internal void ResetShowContinuePopupImmediatelyButton() {
        SushiDebug.Log("ResetShowContinuePopupImmediatelyButton");
        continued = false;
    }
}
