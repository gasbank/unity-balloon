using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class SFSample : MonoBehaviour {
	private Material _material;
	
	public Vector2 _samplePosition = Vector2.zero;
	public bool _lineSample = false;

	public Vector2 samplePosition {
		get {return _samplePosition;}
		set {
			_samplePosition = value;
			if(_material) _material.SetVector("_SamplePosition", _samplePosition);
		}
	}
	
	public bool lineSample {
		get {return _lineSample;}
		set {
			_lineSample = value;
			if(_material){
				if(value){
					_material.EnableKeyword("LINESAMPLE_ON");
					_material.DisableKeyword("FIXEDSAMPLEPOINT_ON");
				} else {
					_material.DisableKeyword("LINESAMPLE_ON");
					_material.EnableKeyword("FIXEDSAMPLEPOINT_ON");
				}
			}
		}
	}

	private void Start(){
		Renderer r = this.GetComponent<Renderer> ();
		var old = r.sharedMaterial;

		if(old == null || old.shader.name != "Sprites/SFSoftShadow"){
			Debug.LogError("SFSample requires the attached renderer to be using the Sprites/SFSoftShadow shader.");
			return;
		}

		_material = new Material(old);
		r.material = _material;
		_material.SetFloat("_SoftHardMix", old.GetFloat("_SoftHardMix"));

		// Force the properties to be updated.
		this.samplePosition = _samplePosition;
		this.lineSample = _lineSample;
	}

	private void OnDrawGizmosSelected(){
		Gizmos.DrawIcon(transform.TransformPoint(_samplePosition), "SFDotGizmo.psd");
	}
}
