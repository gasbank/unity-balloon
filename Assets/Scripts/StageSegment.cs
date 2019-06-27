using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class StageSegment : MonoBehaviour {
    [SerializeField] Bounds segmentBounds;

    public float Height => segmentBounds.extents.y * 2;

    void Update() {
        if (Application.isPlaying == false) {
            segmentBounds = new Bounds();
            foreach (var c in GetComponentsInChildren<Collider>()) {
                segmentBounds.Encapsulate(c.bounds);
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
