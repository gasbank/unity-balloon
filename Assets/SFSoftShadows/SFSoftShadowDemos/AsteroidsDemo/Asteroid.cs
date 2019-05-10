using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Random=UnityEngine.Random;

public class Asteroid : MonoBehaviour {

	private float spin;
	private RectTransform rt;
	public Vector2 vel;

	public float speed = 3f;

	public void Start(){
		spin = (Random.value > 0.5f ? -1f : 1f) * Random.Range(10f, 20f);
		vel = Random.insideUnitCircle.normalized * speed;
	}

	public void FixedUpdate(){
		transform.rotation *= Quaternion.AngleAxis(spin * Time.deltaTime, Vector3.forward);
		Vector3 pos = transform.position  + (Vector3) vel * Time.deltaTime;

		if(Mathf.Abs(pos.x) > 10f){
			vel.x = -vel.x;
		}
		if(Mathf.Abs(pos.y) > 10f){
			vel.y = -vel.y;
		}
		transform.position = pos;
	}

}
