using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEffect : MonoBehaviour {
    [SerializeField] float speed = 100.0f;
    [SerializeField] Renderer windRenderer = null;

    void Awake() {
        windRenderer.material = Instantiate(windRenderer.material);
    }

    void Update() {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
        windRenderer.material.mainTextureOffset += Vector2.up * Time.deltaTime * speed;
    }
}
