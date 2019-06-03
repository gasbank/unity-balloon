using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMaterial : MonoBehaviour {
    [SerializeField] Renderer planeRenderer = null;
    [SerializeField] WindRegion windRegion = null;

    void Awake() {
        planeRenderer.material = Instantiate(planeRenderer.material);
    }

    void Update() {
        planeRenderer.material.SetFloat("_Center", 1.0f - Mathf.Repeat(Time.time * windRegion.Power / 10, 1.0f));
        planeRenderer.material.SetFloat("_Alpha", windRegion.CapacityRatio);
    }
}
