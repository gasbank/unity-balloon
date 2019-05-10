using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Random=UnityEngine.Random;

public class AsteroidBullet : MonoBehaviour {

	public Vector2 vel;
	public SpriteRenderer sr;

	public void Update(){
		this.transform.position += (Vector3) vel * Time.deltaTime;
	}

}
