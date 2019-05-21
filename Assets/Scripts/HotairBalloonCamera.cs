using UnityEngine;

public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            var balloonCameraTarget = GameObject.Find("Hotair Balloon/Balloon/Balloon Camera Target");
            if (balloonCameraTarget != null) {
                followTarget = balloonCameraTarget.transform;
            }
        }
    }

    void LateUpdate() {
        if (followTarget != null) {
            transform.position = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
        }
    }
}
