using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartFeverButton : MonoBehaviour {
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Awake() {
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
    }

    public void StartFever() {
        hotairBalloon.StartFever();
    }
}
