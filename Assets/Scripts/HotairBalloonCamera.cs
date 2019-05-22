using UnityEngine;

public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            var balloonCameraTarget = GameObject.Find("Hotair Balloon/Balloon/Balloon Camera Target");
            if (balloonCameraTarget != null) {
                followTarget = balloonCameraTarget.transform;
            }

            hotairBalloon = GameObject.Find("Hotair Balloon").GetComponent<HotairBalloon>();
        }
    }

    void LateUpdate() {
        if (followTarget != null && hotairBalloon.IsGameOver == false && hotairBalloon.IsStageFinished == false) {
            transform.position = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
        }
    }
}
