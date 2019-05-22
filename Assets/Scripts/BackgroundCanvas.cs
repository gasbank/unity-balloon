﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundCanvas : MonoBehaviour {
    [SerializeField] Canvas canvas = null;
    [SerializeField] Image image = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] Color colorTop;
    [SerializeField] Color colorMid;
    [SerializeField] Color colorBot;
    [SerializeField] float colorChangeScale = 200;
    
    void Awake() {
        canvas.worldCamera = Camera.main;
        image.material = Instantiate(image.material);
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        colorTop = image.material.GetColor("_ColorTop");
        colorMid = image.material.GetColor("_ColorMid");
        colorBot = image.material.GetColor("_ColorBot");
    }

    void SetMaterialDarker(string name, Color color) {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        image.material.SetColor(name, Color.HSVToRGB(h, s, v - hotairBalloon.Y / colorChangeScale));
    }

    void Update() {
        SetMaterialDarker("_ColorTop", colorTop);
        SetMaterialDarker("_ColorMid", colorMid);
        SetMaterialDarker("_ColorBot", colorBot);
    }
}