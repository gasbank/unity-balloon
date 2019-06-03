using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindFanRotation : MonoBehaviour {
    [SerializeField] WindRegion windRegion = null;
    [SerializeField] float rotationSpeed = 10.0f;
    void Update() {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed * windRegion.Power * windRegion.CapacityRatio);
    }
}
