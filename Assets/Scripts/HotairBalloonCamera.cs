using UnityEngine;

public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] Vector3 followVelocity;
    [SerializeField] float followSmoothMaxTime = 0.35f;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            UpdateHotairBalloon();
        }
    }

    public void UpdateHotairBalloon() {
        hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();

        if (hotairBalloon != null) {
            var balloonCameraTarget = hotairBalloon.transform.Find("Balloon/Balloon Camera Target");
            if (balloonCameraTarget != null) {
                followTarget = balloonCameraTarget.transform;
            }
        }
    }

    void LateUpdate() {
        if (followTarget != null) {
            var followTargetPosition = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
            var followSmoothTime = hotairBalloon.InFeverGaugeNotEmpty ? hotairBalloon.FeverGaugeRatio * followSmoothMaxTime : 0;
            transform.position = Vector3.SmoothDamp(transform.position, followTargetPosition, ref followVelocity, followSmoothTime);
        }
    }
}
