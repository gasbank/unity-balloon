using System;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class StageSegment : MonoBehaviour
{
    [SerializeField]
    Bounds segmentBounds;

    public float Height => segmentBounds.extents.y * 2;

    void Update()
    {
        if (Application.isPlaying == false)
        {
            var newSegmentBounds = new Bounds();
            foreach (var c in GetComponentsInChildren<Collider>()) newSegmentBounds.Encapsulate(c.bounds);
            if (segmentBounds != newSegmentBounds)
            {
                segmentBounds = newSegmentBounds;
#if UNITY_EDITOR
                EditorUtility.SetDirty(gameObject);
#endif
            }
        }
    }

    [ContextMenu("Align All Children")]
    void AlignAllChildren()
    {
        foreach (Transform t in transform)
        {
            var x = Math.Round(2 * t.position.x, MidpointRounding.AwayFromZero) / 2;
            var y = Math.Round(2 * t.position.y, MidpointRounding.AwayFromZero) / 2;
            t.position = new Vector3((float) x, (float) y, 0);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
#endif
    }
}