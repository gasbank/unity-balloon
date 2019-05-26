using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BalloonInterface : MonoBehaviour {
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] Camera mainCamera = null;
    [SerializeField] RectTransform feverRingRt = null;
    [SerializeField] RectTransform feverRingParentRt = null;
    [SerializeField] RectTransform feverRingOuterRt = null;
    [SerializeField] RectTransform feverRingOuterParentRt = null;
    [SerializeField] Image oilImage = null;
    [SerializeField] Image feverRingImageLeft = null;
    [SerializeField] Image feverRingImageRight = null;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            UpdateReferences();
        }
    }

    private void UpdateReferences() {
        hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
        mainCamera = Camera.main;
        feverRingParentRt = feverRingRt.transform.parent.GetComponent<RectTransform>();
        feverRingOuterParentRt = feverRingOuterRt.transform.parent.GetComponent<RectTransform>();
    }

    void Update() {
        if (Application.isPlaying == false) {
            UpdateInterfaces();
        }
    }

    void FixedUpdate() {
        UpdateInterfaces();
    }

    void LateUpdate() {
        UpdateInterfaces();
    }

    private void UpdateInterfaces() {
        if (hotairBalloon != null && mainCamera != null && feverRingParentRt != null && feverRingOuterParentRt != null) {
            var feverRingScreenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, hotairBalloon.FeverRingPosition);
            var feverRingOuterScreenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, hotairBalloon.FeverRingOuterPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(feverRingParentRt, feverRingScreenPoint, null, out var feverRingLocalPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(feverRingOuterParentRt, feverRingOuterScreenPoint, null, out var feverRingOuterLocalPoint);
            feverRingRt.anchoredPosition = feverRingLocalPoint;
            var radiusInLocalPointCoordinates = (feverRingOuterLocalPoint - feverRingLocalPoint).magnitude;
            feverRingRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, radiusInLocalPointCoordinates * 2);
            feverRingRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, radiusInLocalPointCoordinates * 2);
            feverRingOuterRt.anchoredPosition = feverRingOuterLocalPoint;

            oilImage.fillAmount = hotairBalloon.RemainOilAmountRatio;
            feverRingImageLeft.fillAmount = 0.5f * hotairBalloon.FeverGaugeRatio;
            feverRingImageRight.fillAmount = 0.5f * hotairBalloon.FeverGaugeRatio;
        }
    }
}
