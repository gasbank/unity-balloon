using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class StageGenerator : MonoBehaviour {
    [SerializeField] StageSegment[] stageSegmentPrefabs = null;
    
    void Update() {
#if UNITY_EDITOR
        var isPrefab = UnityEditor.PrefabUtility.GetPrefabAssetType(gameObject) != UnityEditor.PrefabAssetType.NotAPrefab;
        if (Application.isPlaying == false && isPrefab == false && UnityEditor.EditorUtility.IsDirty(gameObject)) {
            foreach (var t in transform.Cast<Transform>().ToArray()) {
                DestroyImmediate(t.gameObject);
            }
            if (stageSegmentPrefabs != null) {
                var yOffset = 0.0f;
                foreach (var segmentPrefab in stageSegmentPrefabs) {
                    if (segmentPrefab != null) {
                        var segment = Instantiate(segmentPrefab, Vector3.up * yOffset, Quaternion.identity, transform).GetComponent<StageSegment>();
                        segment.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        yOffset += segment.Height;
                    }
                }
            }
        }
#endif
    }
}
