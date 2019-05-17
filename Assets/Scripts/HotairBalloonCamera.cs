using UnityEngine;

public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            followTarget = GameObject.Find("Hotair Balloon/Balloon/Balloon Camera Target").transform;
        }
    }

    void LateUpdate() {
        transform.position = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
    }
}
