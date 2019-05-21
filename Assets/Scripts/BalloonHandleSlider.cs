using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalloonHandleSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] Slider slider = null;
    [SerializeField] float startPosition = 0;
    [SerializeField] bool controlled = false;
    [SerializeField] float horizontal = 0;
    [SerializeField] bool leftButton;
    [SerializeField] bool rightButton;
    [SerializeField] SliderInterface sliderInterface = null;
    [SerializeField] RectTransform rt = null;
    [SerializeField] CanvasScaler canvasScaler = null;

    public bool Controlled { get => controlled; private set => controlled = value; }
    public float Horizontal => horizontal;

    public bool LeftButton { get => leftButton; set => leftButton = value; }
    public bool RightButton { get => rightButton; set => rightButton = value; }

    void Awake() {
        rt = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        startPosition = slider.normalizedValue;
        Controlled = true;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out var positionInParentRect)) {
            sliderInterface.Rt.anchoredPosition = positionInParentRect;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        Controlled = false;
    }

    void Update() {
        float v = 0;
        if (LeftButton) {
            v += -1;
        }
        if (RightButton) {
            v += 1;
        }
        horizontal = v + (Controlled ? Mathf.Clamp(8 * (slider.normalizedValue - startPosition), -1, 1) : 0);

        if (sliderInterface != null) {
            sliderInterface.Value = Horizontal;
        }
    }
}
