using UnityEngine;

[ExecuteInEditMode]
public class HotairBalloonCamera : MonoBehaviour {
    [SerializeField] Transform followTarget = null;
    [SerializeField] HotairBalloon hotairBalloon = null;
    [SerializeField] Vector3 followVelocity;
    [SerializeField] float followSmoothMaxTime = 0.35f;
    [SerializeField] Camera cam = null;
    [SerializeField] GameObject limitCubeRight = null;

     void OnValidate() {
        // if (gameObject.scene.rootCount != 0) {
        //     UpdateHotairBalloon();
        // }
        limitCubeRight = GameObject.Find("Limit Cube Group/Limit Cube (Right)");
    }

    void Start() {
        UpdateHotairBalloon();
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
        if (followTarget != null && hotairBalloon != null) {
            var followTargetPosition = new Vector3(transform.position.x, followTarget.position.y, transform.position.z);
            var followSmoothTime = hotairBalloon.InFeverGaugeNotEmpty ? hotairBalloon.FeverGaugeRatio * followSmoothMaxTime : 0;
            transform.position = Vector3.SmoothDamp(transform.position, followTargetPosition, ref followVelocity, followSmoothTime);
        }

        //GeometryUtility.

        var fovYDeg = cam.fieldOfView;
        var fovXDeg = GetFovXDegFromFovYDeg(fovYDeg, cam.aspect);

        //SushiDebug.Log($"fovYDeg = {fovYDeg}, fovXDeg = {fovXDeg}, aspect = {cam.aspect}");

        if (limitCubeRight != null) {
            var l = limitCubeRight.GetComponent<Collider>().bounds.min.x;//limitCubeRight.transform.position.x;
            var d = Mathf.Abs(limitCubeRight.transform.position.z - transform.position.z);
            var targetFovXDeg = 2 * Mathf.Atan(l / d) * Mathf.Rad2Deg;
            var targetFovYDeg = GetFovYDegFromFovXDeg(targetFovXDeg, cam.aspect);
            cam.fieldOfView = targetFovYDeg;
        }
    }

    public static float GetFovXDegFromFovYDeg(float fovYDeg, float aspect) {
        return 2 * Mathf.Atan(Mathf.Tan(fovYDeg * Mathf.Deg2Rad / 2) * aspect) * Mathf.Rad2Deg;
    }

    public static float GetFovYDegFromFovXDeg(float fovXDeg, float aspect) {
        return 2 * Mathf.Atan(Mathf.Tan(fovXDeg * Mathf.Deg2Rad / 2) / aspect) * Mathf.Rad2Deg;
    }
}
