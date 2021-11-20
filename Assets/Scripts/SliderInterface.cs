﻿using UnityEngine;
using UnityEngine.UI;

public class SliderInterface : MonoBehaviour
{
    [SerializeField]
    RectTransform directionFillLeftImage;

    [SerializeField]
    RectTransform directionFillRightImage;

    [SerializeField]
    Slider slider;

    [SerializeField]
    float valueVelocity;

    [field: SerializeField]
    public RectTransform Rt { get; set; }

    public float Value
    {
        get => slider.value;
        set => slider.value = value;
    }

    void Awake()
    {
        Rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Value >= 0)
        {
            directionFillLeftImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            directionFillRightImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Value * 150);
        }
        else
        {
            directionFillLeftImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, -Value * 150);
            directionFillRightImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        }
    }

    void LateUpdate()
    {
        if (Input.GetAxis("Horizontal") != 0
        ) // Value를 아마도 HotairBalloon에서도 업데이트해서 덮어쓰니까 그냥 여기선 LateUpdate로 하자. 개발용 임시 방편.
            Value = Mathf.SmoothDamp(Value, Input.GetAxis("Horizontal"), ref valueVelocity, 0.1f);
    }
}