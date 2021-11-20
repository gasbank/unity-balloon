using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalloonHandleSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    static bool keyboardControlled;

    [SerializeField]
    CanvasScaler canvasScaler;

    [SerializeField]
    GameObject configButton;

    Vector2 dragStartPosition;

    [SerializeField]
    RawImage feverSwipeArrow;

    [SerializeField]
    float feverSwipeDistance = 150;

    [SerializeField]
    float feverSwipeMaxSpeed = 0.15f;

    [SerializeField]
    RectTransform feverSwipeStartPosition;

    [SerializeField]
    RectTransform feverSwipeTargetPosition;

    [SerializeField]
    HotairBalloon hotairBalloon;

    [SerializeField]
    GameObject iapGroup;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    Slider slider;

    [SerializeField]
    SliderInterface sliderInterface;

    [SerializeField]
    GameObject sliderInterfaceBody;

    [SerializeField]
    StageCommon stageCommon;

    [SerializeField]
    float startPosition;

    [field: SerializeField]
    public bool Controlled { get; private set; }

    [field: SerializeField]
    public float Horizontal { get; set; }

    [field: SerializeField]
    public bool LeftButton { get; set; }

    [field: SerializeField]
    public bool RightButton { get; set; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out dragStartPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out var dragPosition);
        feverSwipeStartPosition.anchoredPosition = Vector2.up * Mathf.Max(0, dragPosition.y - dragStartPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = slider.normalizedValue;
        Controlled = true;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null,
            out var positionInParentRect))
        {
            sliderInterface.Rt.anchoredPosition = positionInParentRect;

            feverSwipeStartPosition.anchoredPosition = Vector2.zero;
            feverSwipeTargetPosition.anchoredPosition =
                feverSwipeStartPosition.anchoredPosition + Vector2.up * feverSwipeDistance;
        }

        OnStartStage();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Controlled = false;
    }

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
        UpdateReferences();
        stageCommon = FindObjectOfType<StageCommon>();
    }

    public void UpdateReferences()
    {
        hotairBalloon = FindObjectOfType<HotairBalloon>();
    }

    public void OnStartStage()
    {
        sliderInterfaceBody.SetActive(true);
        stageCommon.DeactivateTitleGroup();
        iapGroup.SetActive(false);
        configButton.SetActive(false);
    }

    void Update()
    {
        float v = 0;
        if (LeftButton) v += -1;
        if (RightButton) v += 1;
        Horizontal = v + (Controlled ? Mathf.Clamp(8 * (slider.normalizedValue - startPosition), -1, 1) : 0);

        if (keyboardControlled == false) keyboardControlled = Input.GetAxis("Horizontal") != 0;

        if (sliderInterface != null && keyboardControlled == false) sliderInterface.Value = Horizontal;

        var feverSwipeVelocity = Vector2.zero;
        feverSwipeTargetPosition.anchoredPosition = Vector2.SmoothDamp(feverSwipeTargetPosition.anchoredPosition,
            feverSwipeStartPosition.anchoredPosition + Vector2.up * feverSwipeDistance, ref feverSwipeVelocity,
            feverSwipeMaxSpeed);

        if (feverSwipeStartPosition.anchoredPosition.y > feverSwipeTargetPosition.anchoredPosition.y ||
            Input.GetKeyDown(KeyCode.UpArrow)) hotairBalloon.StartFever();

        if (feverSwipeArrow != null && hotairBalloon != null)
            feverSwipeArrow.gameObject.SetActive(hotairBalloon.CanStartFever);

        if (hotairBalloon != null && hotairBalloon.CanStartFever)
        {
            var feverSwipeArrowUvRect = feverSwipeArrow.uvRect;
            feverSwipeArrowUvRect.y = -Time.time;
            feverSwipeArrow.uvRect = feverSwipeArrowUvRect;
        }
    }
}