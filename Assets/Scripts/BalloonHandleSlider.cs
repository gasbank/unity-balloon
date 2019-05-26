using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalloonHandleSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] Slider slider = null;
    [SerializeField] float startPosition = 0;
    [SerializeField] bool controlled = false;
    [SerializeField] float horizontal = 0;
    [SerializeField] bool leftButton;
    [SerializeField] bool rightButton;
    [SerializeField] SliderInterface sliderInterface = null;
    [SerializeField] GameObject sliderInterfaceBody = null;
    [SerializeField] RectTransform rt = null;
    [SerializeField] CanvasScaler canvasScaler = null;
    [SerializeField] RectTransform feverSwipeStartPosition = null;
    [SerializeField] RectTransform feverSwipeTargetPosition = null;
    [SerializeField] float feverSwipeDistance = 150;
    [SerializeField] float feverSwipeMaxSpeed = 0.15f;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] RawImage feverSwipeArrow = null;
    [SerializeField] StageCommon stageCommon = null;

    public bool Controlled { get => controlled; private set => controlled = value; }
    public float Horizontal => horizontal;

    public bool LeftButton { get => leftButton; set => leftButton = value; }
    public bool RightButton { get => rightButton; set => rightButton = value; }

    Vector2 dragStartPosition;

    void Awake() {
        rt = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
        hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        stageCommon = GameObject.FindObjectOfType<StageCommon>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        startPosition = slider.normalizedValue;
        Controlled = true;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out var positionInParentRect)) {
            sliderInterface.Rt.anchoredPosition = positionInParentRect;

            feverSwipeStartPosition.anchoredPosition = Vector2.zero;
            feverSwipeTargetPosition.anchoredPosition = feverSwipeStartPosition.anchoredPosition + Vector2.up * feverSwipeDistance;
        }
        OnStartStage();
    }

    public void OnStartStage() {
        sliderInterfaceBody.SetActive(true);
        stageCommon.DeactivateTitleGroup();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out dragStartPosition);
    }

    public void OnDrag(PointerEventData eventData) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out var dragPosition);
        feverSwipeStartPosition.anchoredPosition = Vector2.up * Mathf.Max(0, (dragPosition.y - dragStartPosition.y));
    }

    public void OnEndDrag(PointerEventData eventData) {
        
    }

    public void OnPointerUp(PointerEventData eventData) {
        Controlled = false;
    }

    static bool keyboardControlled = false;

    void Update() {
        float v = 0;
        if (LeftButton) {
            v += -1;
        }
        if (RightButton) {
            v += 1;
        }
        horizontal = v + (Controlled ? Mathf.Clamp(8 * (slider.normalizedValue - startPosition), -1, 1) : 0);

        if (keyboardControlled == false) {
            keyboardControlled = Input.GetAxis("Horizontal") != 0;
        }

        if (sliderInterface != null && keyboardControlled == false) {
            sliderInterface.Value = Horizontal;
        }

        Vector2 feverSwipeVelocity = Vector2.zero;
        feverSwipeTargetPosition.anchoredPosition = Vector2.SmoothDamp(feverSwipeTargetPosition.anchoredPosition, feverSwipeStartPosition.anchoredPosition + Vector2.up * feverSwipeDistance, ref feverSwipeVelocity, feverSwipeMaxSpeed);

        if (feverSwipeStartPosition.anchoredPosition.y > feverSwipeTargetPosition.anchoredPosition.y || Input.GetKeyDown(KeyCode.UpArrow)) {
            hotairBalloon.StartFever();
        }

        feverSwipeArrow.gameObject.SetActive(hotairBalloon.CanStartFever);
        if (hotairBalloon.CanStartFever) {
            var feverSwipeArrowUvRect = feverSwipeArrow.uvRect;
            feverSwipeArrowUvRect.y = -Time.time;
            feverSwipeArrow.uvRect = feverSwipeArrowUvRect;
        }
    }
}
