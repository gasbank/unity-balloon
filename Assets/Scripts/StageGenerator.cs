using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class StageGenerator : MonoBehaviour {
    [SerializeField] StageSegment[] stageSegmentPrefabs = null;
    [SerializeField] bool rebuild = false;

    void OnValidate() {
        rebuild = true;
    }
    
    void Update() {
#if UNITY_EDITOR
        var isPrefab = PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab;
        if (Application.isPlaying == false && isPrefab == false && rebuild) {
            Debug.Log("Rebuilding stage...");
            foreach (var t in transform.Cast<Transform>().ToArray()) {
                DestroyImmediate(t.gameObject);
            }
            if (stageSegmentPrefabs != null) {
                var yOffset = 0.0f;
                foreach (var segmentPrefab in stageSegmentPrefabs) {
                    if (segmentPrefab != null) {
                        var segmentObject = PrefabUtility.InstantiatePrefab(segmentPrefab.gameObject) as GameObject;
                        segmentObject.transform.position = Vector3.up * yOffset;
                        segmentObject.transform.rotation = Quaternion.identity;
                        segmentObject.transform.parent = transform;
                        var segment = segmentObject.GetComponent<StageSegment>();
                        //var segment = Instantiate(segmentPrefab, Vector3.up * yOffset, Quaternion.identity, transform).GetComponent<StageSegment>();
                        segment.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        yOffset += segment.Height;
                    }
                }
            }
            rebuild = false;
        }
#endif
    }
}
