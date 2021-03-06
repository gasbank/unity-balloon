﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Stage : MonoBehaviour {
    [SerializeField] Material backgroundMaterial = null;
    [SerializeField] int tutorialTemplate = 0;
    [SerializeField] GameObject checkpointPrefab = null;
    [SerializeField] FinishLine finishLine = null;
    [SerializeField] GameObject hotairBalloonPrefab = null;
    [SerializeField] bool hideGizmos = false;
    [SerializeField] float stageWidth = 17.0f;
    [SerializeField] LimitCubeGroup limitCubeGroup = null;
    [SerializeField] int autoGeneratedStageNumber = 0;
    private BackgroundCanvas backgroundCanvas;

    public int AutoGeneratedStageNumber {
        get => autoGeneratedStageNumber;
        set {
            autoGeneratedStageNumber = value;
            var stageGenerator = GetComponent<StageGenerator>();
            if (stageGenerator != null) {
                stageGenerator.GenerateStageWithSeed(autoGeneratedStageNumber);
            }
        }
    }
    
    public int TutorialTemplate => tutorialTemplate;
    public float TotalStageLength { get; private set; }

    void OnValidate() {
        // finishLine = GameObject.FindObjectOfType<FinishLine>();
        // if (finishLine != null) {
        //     TotalStageLength = finishLine.transform.position.y;
        // }
        UpdateLimitCubeGroupReference();
    }

    private void UpdateLimitCubeGroupReference() {
        limitCubeGroup = GameObject.Find("Limit Cube Group")?.GetComponent<LimitCubeGroup>() ?? null;
    }

    void Awake() {
        if (Application.isPlaying == false) {
            return;
        }

        UpdateLimitCubeGroupReference();

        Bootstrap.GetStageNumber(gameObject.name, out PlatformAds.stageNumber);

        backgroundCanvas = GameObject.FindObjectOfType<BackgroundCanvas>();
        if (backgroundCanvas != null) {
            backgroundCanvas.SetImageMaterial(backgroundMaterial);
        }
        UpdateFinishLineReference();
        RespawnCheckpointLines();

        // 체크포인트 기능 지원
        // 스테이지에 있는 Hotair Balloon을 삭제하고
        // 이어서해야할 위치에 만든다.
        DestroyStageAroundHotairBalloonConditional();
        RespawnHotairBalloonConditional();

        UpdateLimitCubeGroup();
    }

    void RespawnCheckpointLines() {
        if (finishLine != null) {
            foreach (var cl in GameObject.FindObjectsOfType<CheckpointLine>()) {
                Destroy(cl.gameObject);
            }
            TotalStageLength = finishLine.transform.position.y;
            if (Application.isPlaying) {
                var checkpointInterval = 0.25f;
                for (var i = 1; i <= 3; i++) {
                    var checkpointRatio = i * checkpointInterval;
                    var checkpointLine = Instantiate(checkpointPrefab, Vector3.up * TotalStageLength * checkpointRatio, Quaternion.identity).GetComponent<CheckpointLine>();
                    checkpointLine.CheckpointText = string.Format("{0:F0}% ==", checkpointRatio * 100);
                }
            }
        }
    }

    public IEnumerator PostProcessOnStageSpawn() {
        yield return new WaitForEndOfFrame();
        DestroyStageAroundHotairBalloonConditional();
        UpdateFinishLineReference();
        RespawnCheckpointLines();
        yield return new WaitForEndOfFrame();
        RespawnHotairBalloonConditional();
    }

    // 이어서하게 되는 체크포인트 부근의 오브젝트는 모두 삭제한다.
    void DestroyStageAroundHotairBalloonConditional() {
        if (HotairBalloon.InitialPositionY != 0) {
            var allRbs = transform.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in allRbs) {
                if (Mathf.Abs(rb.position.y - HotairBalloon.InitialPositionY) < 10) {
                    SushiDebug.Log($"{rb.gameObject.name} is destroyed since it is close to checkpoint.");
                    Destroy(rb.gameObject);
                }
            }
        }
    }

    // 이어서하게 되는 체크포인트로 새롭게 HotairBalloon을 생성시킨다. 기존 것은 삭제된다.
    void RespawnHotairBalloonConditional() {
        if (HotairBalloon.InitialPositionY != 0) {
            var hotairBalloon = GameObject.FindObjectOfType<HotairBalloon>();
            Destroy(hotairBalloon.gameObject);
            Instantiate(hotairBalloonPrefab, Vector3.up * HotairBalloon.InitialPositionY, Quaternion.identity);
            var hotairBalloonCamera = GameObject.FindObjectOfType<HotairBalloonCamera>();
            hotairBalloonCamera.UpdateHotairBalloon();
            var balloonInterface = GameObject.FindObjectOfType<BalloonInterface>();
            balloonInterface.UpdateReferences();
            var balloonHandleSlider = GameObject.FindObjectOfType<BalloonHandleSlider>();
            balloonHandleSlider.UpdateReferences();

            backgroundCanvas.UpdateReferences();

            var stageDuration = GameObject.FindObjectOfType<StageDuration>();
            if (stageDuration != null) {
                stageDuration.UpdateReferences();
            }

            var tutorial = GameObject.FindObjectOfType<Tutorial>();
            if (tutorial != null) {
                tutorial.UpdateReferences();
            }

            var finishLine = GameObject.FindObjectOfType<FinishLine>();
            if (finishLine != null) {
                finishLine.UpdateReferences();
            }
        }
    }

    void UpdateFinishLineReference() {
        finishLine = GameObject.FindObjectOfType<FinishLine>();
    }

    void Update() {
        UpdateLimitCubeGroup();
    }

    private void UpdateLimitCubeGroup() {
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

    [ContextMenu("Align All Children")]
    void AlignAllChildren() {
        foreach (Transform t in transform) {
            var x = Math.Round(2 * t.position.x, MidpointRounding.AwayFromZero) / 2;
            var y = Math.Round(2 * t.position.y, MidpointRounding.AwayFromZero) / 2;
            t.position = new Vector3((float)x, (float)y, 0);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
}
