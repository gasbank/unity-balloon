using UnityEngine;

public class BalloonOilSprite : MonoBehaviour {
    [SerializeField] Transform cam;

    void Awake() {
        cam = Camera.main.transform;
    }

    void LateUpdate() {
        transform.LookAt(cam);
    }
}
