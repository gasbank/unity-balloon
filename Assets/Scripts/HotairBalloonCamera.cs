using UnityEngine;

public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;

    void LateUpdate() {
        transform.position = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
    }
}
