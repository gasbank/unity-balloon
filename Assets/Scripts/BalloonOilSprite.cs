using UnityEngine;

public class BalloonOilSprite : MonoBehaviour {
    [SerializeField] Transform cam;

    void LateUpdate() {
        transform.LookAt(cam);
    }
}
