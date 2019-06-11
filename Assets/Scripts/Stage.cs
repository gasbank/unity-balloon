using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour {
    [SerializeField] Material backgroundMaterial = null;
    [SerializeField] bool tutorial = false;
    [SerializeField] GameObject checkpointPrefab = null;
    [SerializeField] FinishLine finishLine = null;

    public bool Tutorial => tutorial;

    void Awake() {
        var backgroundCanvas = GameObject.FindObjectOfType<BackgroundCanvas>();
        backgroundCanvas.SetImageMaterial(backgroundMaterial);
        finishLine = GameObject.FindObjectOfType<FinishLine>();
        var totalStageLength = finishLine.transform.position.y;
        var checkpointInterval = 0.25f;
        for (var i = 1; i <= 3; i++) {
            var checkpointRatio = i * checkpointInterval;
            var checkpointLine = Instantiate(checkpointPrefab, Vector3.up * totalStageLength * checkpointRatio, Quaternion.identity).GetComponent<CheckpointLine>();
            checkpointLine.CheckpointText = string.Format("========== {0:F0}% ==========", checkpointRatio * 100);
        }
    }
}
