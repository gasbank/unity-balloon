using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderInterface : MonoBehaviour {
    [SerializeField] Slider slider = null;
    [SerializeField] RectTransform directionFillLeftImage = null;
    [SerializeField] RectTransform directionFillRightImage = null;
    [SerializeField] RectTransform rt = null;

    public RectTransform Rt => rt;
    public float Value {
        get => slider.value;
        set => slider.value = value;
    }

    void Awake() {
        rt = GetComponent<RectTransform>();
    }

    void Update() {
        if (slider.value >= 0) {
            directionFillLeftImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            directionFillRightImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slider.value * 150);
        } else {
            directionFillLeftImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, -slider.value * 150);
            directionFillRightImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        }
    }
}
