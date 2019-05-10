using UnityEngine;
using System.Collections;

public class SimpleAnimation : MonoBehaviour {

	public float width = 8.0f;
	public float speed = 1.0f;
	private Vector3 pos;

	// Use this for initialization
	void Start () {
		pos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = pos + Vector3.right * Mathf.Sin (Time.time * speed) * width;
	}
}
