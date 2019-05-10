using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {

	void Update () {
		this.transform.rotation = Quaternion.AngleAxis(90.0f*Time.time, Vector3.forward);
	}
}
