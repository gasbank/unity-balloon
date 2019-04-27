using UnityEngine;

public class BalloonLimiter : MonoBehaviour {
    [SerializeField] float widthLimit = 5.0f;
    private void LateUpdate() {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -widthLimit, widthLimit), transform.position.y, transform.position.z);
    }
}
