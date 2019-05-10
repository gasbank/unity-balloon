using UnityEngine;
using System.Collections;

public class MoonMan : MonoBehaviour {
	public float _force = 10.0f;

	public float _angleMultiplier;
	public float _angleClamp;

	public Transform _sprite;
	public Transform _camera;
	public Transform _light;

	private void Update(){
		if(SystemInfo.supportsAccelerometer){
			Vector3 acceleration = Vector3.zero;
			foreach (AccelerationEvent accEvent in Input.accelerationEvents) {
				acceleration += accEvent.acceleration * accEvent.deltaTime;
			}
			acceleration.z = 0;
			this.GetComponent<Rigidbody2D>().AddForce(acceleration * 90f);
		}else{
			var joy = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			this.GetComponent<Rigidbody2D>().AddForce(joy*_force + Vector2.up * -0.6f);
		}
	}

	private float _angle = 0.0f;

	private void LateUpdate(){
		var t = this.transform;

		var angle = Mathf.Clamp(-_angleMultiplier*this.GetComponent<Rigidbody2D>().velocity.x, -_angleClamp, _angleClamp);
		_angle = Mathf.LerpAngle(angle, _angle, Mathf.Pow(0.1f, Time.deltaTime));
		_sprite.localRotation = Quaternion.AngleAxis(_angle, Vector3.forward);

		var pos = t.position;
		pos.z = _camera.position.z;
		_camera.position = pos;

		var localMouse = t.InverseTransformPoint(_camera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition));
		var lightAngle = Mathf.Atan2(localMouse.y, localMouse.x)*Mathf.Rad2Deg;
		_light.rotation = Quaternion.AngleAxis(lightAngle, Vector3.forward);
	}
}
