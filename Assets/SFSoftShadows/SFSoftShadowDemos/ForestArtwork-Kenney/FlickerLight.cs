using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour {

	public float flickerAmt = 0.15f;
	public float lightStrength = 0.8f;
	public float flickerSpeed = 0.4f;

	public Color baseLight;

	public SFLight shadowLight;

	private float timeOffset;

	public void Start(){
		timeOffset = Random.value * 7.0f;
	}


	void Update () {
		float t = (Time.time + timeOffset) * flickerSpeed;
		float str = Mathf.Sin (t * 7f) * flickerAmt + Mathf.Sin (t * 3f) * flickerAmt + lightStrength;
		shadowLight.color = new Color (baseLight.r * str, baseLight.g * str, baseLight.b * str, 1f);
	}
}
