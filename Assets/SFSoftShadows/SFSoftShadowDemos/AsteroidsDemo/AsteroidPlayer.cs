using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using Random=UnityEngine.Random;

public class AsteroidPlayer : MonoBehaviour {

	public GameObject bulletPrefab;

	public float timeToSpawn;
	public float turnSpeed = 90f;
	public float maxSpeed = 5f;
	public float accel = 1.0f;

	private float hue;

	private Vector2 vel;

	public SFLight headlight;
	public SFLight engineGlow;
	public Color engineBaseGlow;
	private float headlightIntensity;
	private float engineGlowIntensity;
	public float bulletIntensity;

	public SFRenderer sfRenderer;

	private bool headlightOn = true;

	private void Start(){
		headlightIntensity = headlight.intensity;
		if (engineGlow != null) {
			engineGlowIntensity = engineGlow.intensity;
		}
	}

	private void Update(){
		// Player movement
		float horiz = Input.GetAxis("Horizontal");
		float vert = Input.GetAxis("Vertical");

		this.transform.Rotate(Vector3.back, horiz * Time.deltaTime * turnSpeed);

		// Apply accel:
		vel += Time.deltaTime * vert * accel * (Vector2) transform.up;

		// clamp velocity:
		vel = Mathf.Min(maxSpeed, vel.magnitude) * vel.normalized;

		this.transform.position += (Vector3) vel * Time.deltaTime;

		if(Input.GetKeyDown(KeyCode.Space)){
			Fire();
		}

		// bounce the player off the bounds
		if(Mathf.Abs(transform.position.x) > 10f){
			vel.x = -vel.x;
		}
		if(Mathf.Abs(transform.position.y) > 10f){
			vel.y = -vel.y;
		}

		if(Input.GetKeyDown(KeyCode.G)){
			sfRenderer.enabled = !sfRenderer.enabled;
		}

		// headlights!
		if(Input.GetKeyDown(KeyCode.F)){
			headlightOn = !headlightOn;
		}

		headlight.intensity = Mathf.Clamp(headlight.intensity + (headlightOn ? 1f : -1f) * 30f * Time.deltaTime, 0f, headlightIntensity);

		if (engineGlow != null) {
			engineGlow.intensity = engineGlowIntensity * Mathf.Abs (vert) * (Mathf.PerlinNoise (0f, Time.time * 20f) / 4f + 0.75f);
			engineGlow.color = Color.HSVToRGB (0.08f + 0.07f * Mathf.PerlinNoise (Time.time * 5f, 0f), 1f, 1f);
		}
	}


	public void Fire(){
		GameObject go = (GameObject) Instantiate(bulletPrefab, transform.position, Quaternion.identity);
		AsteroidBullet b = go.GetComponent<AsteroidBullet>();
		b.vel = maxSpeed * 3.0f * (Vector2) transform.up;

		SFLight bulletLight = go.GetComponent<SFLight>();

		bulletLight.intensity = bulletIntensity;
		bulletLight.color = Color.HSVToRGB(hue, 1f, 1f);
		b.sr.color  = bulletLight.color;

		hue = (hue + 0.15f) % 1.0f;

		Destroy(go, 5.0f);
	}

}
