using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Stage : MonoBehaviour {
    [SerializeField] Material backgroundMaterial = null;
    [SerializeField] bool tutorial = false;
    [SerializeField] GameObject checkpointPrefab = null;
    [SerializeField] FinishLine finishLine = null;
    [SerializeField] GameObject hotairBalloonPrefab = null;
    [SerializeField] bool hideGizmos = false;
    [SerializeField] float stageWidth = 17.0f;
    [SerializeField] LimitCubeGroup limitCubeGroup = null;
    
    public bool Tutorial => tutorial;
    public float TotalStageLength { get; private set; }

    void OnValidate() {
        // finishLine = GameObject.FindObjectOfType<FinishLine>();
        // if (finishLine != null) {
        //     TotalStageLength = finishLine.transform.position.y;
        // }
        limitCubeGroup = GameObject.Find("Limit Cube Group")?.GetComponent<LimitCubeGroup>() ?? null;
    }

    void Awake() {
        Bootstrap.GetStageNumber(gameObject.name, out PlatformAds.stageNumber);

        var backgroundCanvas = GameObject.FindObjectOfType<BackgroundCanvas>();
        if (backgroundCanvas != null) {
            backgroundCanvas.SetImageMaterial(backgroundMaterial);
        }
        finishLine = GameObject.FindObjectOfType<FinishLine>();
        if (finishLine != null) {
            TotalStageLength = finishLine.transform.position.y;
            var checkpointInterval = 0.25f;
            for (var i = 1; i <= 3; i++) {
                var checkpointRatio = i * checkpointInterval;
                var checkpointLine = Instantiate(checkpointPrefab, Vector3.up * TotalStageLength * checkpointRatio, Quaternion.identity).GetComponent<CheckpointLine>();
                checkpointLine.CheckpointText = string.Format("{0:F0}% ==", checkpointRatio * 100);
            }
        }

        // 체크포인트 기능 지원
        // 스테이지에 있는 Hotair Balloon을 삭제하고
        // 이어서해야할 위치에 만든다.
        var hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
        if (HotairBalloon.initialPositionY != 0) {
            // 이어서하게 되는 체크포인트 부근의 오브젝트는 모두 삭제한다.
            var allRbs = transform.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in allRbs) {
                if (Mathf.Abs(rb.position.y - HotairBalloon.initialPositionY) < 10) {
                    SushiDebug.Log($"{rb.gameObject.name} is destroyed since it is close to checkpoint.");
                    Destroy(rb.gameObject);
                }
            }
            
            Destroy(hotairBalloon.gameObject);
            Instantiate(hotairBalloonPrefab, Vector3.up * HotairBalloon.initialPositionY, Quaternion.identity);
            var hotairBalloonCamera = GameObject.FindObjectOfType<HotairBalloonCamera>();
            hotairBalloonCamera.UpdateHotairBalloon();
            var balloonInterface = GameObject.FindObjectOfType<BalloonInterface>();
            balloonInterface.UpdateReferences();
            var balloonHandleSlider = GameObject.FindObjectOfType<BalloonHandleSlider>();
            balloonHandleSlider.UpdateReferences();


            backgroundCanvas.UpdateReferences();

            var stageDuration = GameObject.FindObjectOfType<StageDuration>();
            stageDuration.UpdateReferences();

            var tutorial = GameObject.FindObjectOfType<Tutorial>();
            if (tutorial != null) {
                tutorial.UpdateReferences();
            }

            var finishLine = GameObject.FindObjectOfType<FinishLine>();
            if (finishLine != null) {
                finishLine.UpdateReferences();
            }
        }

        if (limitCubeGroup != null) {
            limitCubeGroup.SetWidth(stageWidth);
        }
    }

    void Update() {
        if (limitCubeGroup != null) {
            limitCubeGroup.SetWidth(stageWidth);
        }
    }

    void OnDrawGizmos() {
        var finishLineTransform = transform.Find("Finish Line");
        if (hideGizmos == false && finishLineTransform != null) {
            var finishLine = finishLineTransform.GetComponent<FinishLine>();
            if (finishLine != null) {
                var checkpointInterval = 0.25f;
                for (var i = 1; i <= 3; i++) {
                    var checkpointRatio = i * checkpointInterval;
                    Gizmos.color = new Color(1, 1, 0, 0.5f);
                    Gizmos.DrawCube(Vector3.up * finishLine.transform.position.y * checkpointRatio, new Vector3(20, 0.5f, 1));
                }
            }
        }
    }
}
