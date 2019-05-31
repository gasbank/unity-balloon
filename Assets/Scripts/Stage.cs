using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour {
    [SerializeField] Material backgroundMaterial = null;
    [SerializeField] bool tutorial = false;

    public bool Tutorial => tutorial;

    void Awake() {
        var backgroundCanvas = GameObject.FindObjectOfType<BackgroundCanvas>();
        backgroundCanvas.SetImageMaterial(backgroundMaterial);
    }
}
