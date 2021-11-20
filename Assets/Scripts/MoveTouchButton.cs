using UnityEngine;
using UnityEngine.EventSystems;

public class MoveTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    bool rightOrLeft;

    [SerializeField]
    BalloonHandleSlider slider;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rightOrLeft)
            slider.RightButton = true;
        else
            slider.LeftButton = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rightOrLeft)
            slider.RightButton = false;
        else
            slider.LeftButton = false;
    }
}