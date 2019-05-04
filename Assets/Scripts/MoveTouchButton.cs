using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] BalloonHandleSlider slider = null;
    [SerializeField] bool rightOrLeft = false;

    public void OnPointerDown(PointerEventData eventData) {
        if (rightOrLeft) {
            slider.RightButton = true;
        } else {
            slider.LeftButton = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (rightOrLeft) {
            slider.RightButton = false;
        } else {
            slider.LeftButton = false;
        }
    }
}
