using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class StageGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject finishLinePrefab;

    [SerializeField]
    bool rebuild;

    [SerializeField]
    int stageSeed;

    [SerializeField]
    StageSegmentGroup[] stageSegmentGroup;

    [SerializeField]
    StageSegment[] stageSegmentPrefabs;

    void OnValidate()
    {
        if (stageSeed == 0)
            rebuild = true;
        else
            GenerateNewRandomStage();
    }

    void Update()
    {
        RebuildStageConditional();
    }

    void RebuildStageConditional()
    {
        if (Application.isPlaying == false) RebuildStageConditional_Editor();
    }

    void RebuildStage()
    {
        if (rebuild)
        {
            BalloonDebug.Log("Rebuilding stage...");
            foreach (var t in transform.Cast<Transform>().ToArray()) Destroy(t.gameObject);
            var yOffset = 0.0f;
            if (stageSegmentPrefabs != null)
                foreach (var segmentPrefab in stageSegmentPrefabs)
                    if (segmentPrefab != null)
                    {
                        var segmentObject = Instantiate(segmentPrefab.gameObject);
                        segmentObject.transform.position = Vector3.up * yOffset;
                        segmentObject.transform.rotation = Quaternion.identity;
                        segmentObject.transform.parent = transform;
                        var segment = segmentObject.GetComponent<StageSegment>();
                        yOffset += segment.Height;
                    }

            if (finishLinePrefab != null)
            {
                var finishLineObject = Instantiate(finishLinePrefab);
                finishLineObject.transform.position = Vector3.up * yOffset;
                finishLineObject.transform.rotation = Quaternion.identity;
                finishLineObject.transform.parent = transform;
            }

            rebuild = false;
        }
    }

    void RebuildStageConditional_Editor()
    {
#if UNITY_EDITOR
        var isPrefab = PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab;
        if (isPrefab == false && rebuild)
        {
            BalloonDebug.Log("Rebuilding stage... (Editor)");
            foreach (var t in transform.Cast<Transform>().ToArray()) DestroyImmediate(t.gameObject);
            var yOffset = 0.0f;
            if (stageSegmentPrefabs != null)
                foreach (var segmentPrefab in stageSegmentPrefabs)
                    if (segmentPrefab != null)
                    {
                        var segmentObject = PrefabUtility.InstantiatePrefab(segmentPrefab.gameObject) as GameObject;
                        segmentObject.transform.position = Vector3.up * yOffset;
                        segmentObject.transform.rotation = Quaternion.identity;
                        segmentObject.transform.parent = transform;
                        segmentObject.hideFlags = HideFlags.HideInHierarchy;
                        var segment = segmentObject.GetComponent<StageSegment>();
                        yOffset += segment.Height;
                    }

            if (finishLinePrefab != null)
            {
                var finishLineObject = PrefabUtility.InstantiatePrefab(finishLinePrefab) as GameObject;
                finishLineObject.transform.position = Vector3.up * yOffset;
                finishLineObject.transform.rotation = Quaternion.identity;
                finishLineObject.transform.parent = transform;
                finishLineObject.hideFlags = HideFlags.HideInHierarchy;
            }

            rebuild = false;
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

    [ContextMenu("Generate New Random Stage")]
    void GenerateNewRandomStage()
    {
        BalloonDebug.Log($"Generating stage with random seed {stageSeed}");
        var selectedSegments = new List<StageSegment>();
        var oldRandomState = Random.state;
        Random.InitState(stageSeed);
        foreach (var segmentGroup in stageSegmentGroup)
        {
            var segmentGroupNotNull = segmentGroup.stageSegmentPrefabs.Where(e => e != null).ToArray();
            if (segmentGroupNotNull.Length > 0)
                selectedSegments.Add(segmentGroupNotNull[Random.Range(0, segmentGroupNotNull.Length)]);
        }

        stageSegmentPrefabs = selectedSegments.ToArray();
        rebuild = true;
        Random.state = oldRandomState;
    }

    public void GenerateStageWithSeed(int stageSeed)
    {
        this.stageSeed = stageSeed;
        GenerateNewRandomStage();
        RebuildStage();
    }

    [Serializable]
    public class StageSegmentGroup
    {
        public StageSegment[] stageSegmentPrefabs;
    }
}