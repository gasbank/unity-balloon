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
}
