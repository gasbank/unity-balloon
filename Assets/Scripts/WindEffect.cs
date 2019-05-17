using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEffect : MonoBehaviour {
    [SerializeField] float speed = 100.0f;
    [SerializeField] new Renderer renderer = null;

    void Awake() {
        renderer.material = Instantiate(renderer.material);
    }

    void Update() {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
        renderer.material.mainTextureOffset += Vector2.up * Time.deltaTime * speed;
    }
}
