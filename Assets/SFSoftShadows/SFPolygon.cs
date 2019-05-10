// Super Fast Soft Lighting. Copyright 2015 Howling Moon Software, LLP

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SFPolygon : MonoBehaviour {
	private Transform _t;
	public Matrix4x4 _GetMatrix(){
		if (!_t) _t = this.transform;
		return _t.localToWorldMatrix;
	}
	
	private void PathBounds(Vector2[] path, int i0, ref float l, ref float b, ref float r, ref float t){
		for(var i = i0; i < path.Length; i++){
			var v = path[i];
			l = Mathf.Min(v.x, l);
			r = Mathf.Max(v.x, r);
			b = Mathf.Min(v.y, b);
			t = Mathf.Max(v.y, t);
		}
	}

	private Rect _bounds;
	public void _UpdateBounds(){
		float l, b, r, t;
		
		if(_activePath > 0){
			var path = GetPath(_activePath);
			var v0 = path[0];
			l = r = v0.x;
			b = t = v0.y;
			PathBounds(_verts, 1, ref l, ref b, ref r, ref t);
		} else {
			var v0 = _verts[0];
			l = r = v0.x;
			b = t = v0.y;
			PathBounds(_verts, 1, ref l, ref b, ref r, ref t);
			
			var pathCount = this.pathCount;
			for(int p = 1; p < pathCount; p++){
				PathBounds(GetPath(p), 0, ref l, ref b, ref r, ref t);
			}
		}
		
		_bounds = Rect.MinMaxRect(l, b, r, t);
	}

	private Rect _worldBounds;
	public Rect _WorldBounds{ get {return _worldBounds;}}

	public void _CacheWorldBounds(){
		if (!_t) _t = this.transform;
		_worldBounds = SFRenderer._TransformRect(_t.localToWorldMatrix, _bounds);
	}
	
	// Stores the first path.
	// Unfortunate backwards compatibility thing for old serialized scenes.
	// Has made a real mess of the code.
	[SerializeField]
	private Vector2[] _verts = new Vector2[3];
	// Stores more paths.
	[SerializeField]
	private Vector2[][] _paths = null;
	
	public int _activePath = 0;
	public bool _looped;
	public LayerMask _shadowLayers = ~0;
	public float _lightPenetration = 0.0f;
	public float _opacity = 1.0f;
	
	// Sorry, this property is a disaster due to backwards compat for old projects. :-\
	public int pathCount {
		get {return (_paths == null ? 1 : _paths.Length + 1);}
		set {
			int len = value - 1;
			if(value == this.pathCount){
				return;
			} else if(value < 1){
				Debug.LogError("pathCount must be positive.");
				return;
			} else if(value == 1){
				_paths = null;
			} else if(_paths == null){
				// _paths didn't exist before. Fill with the 0 path (_verts).
				_paths = new Vector2[len][];
				for(int i = 0; i < _paths.Length; i++) _paths[i] = _verts;
			} else {
				var paths = _paths;
				_paths = new Vector2[len][];
				
				if(len > paths.Length){
					int i = 0;
					// Copy existing paths.
					for(; i < paths.Length; i++) _paths[i] = paths[i];
					// Fill remaining with path 0.
					for(; i < len; i++) _paths[i] = _verts;
				} else {
					for(int i = 0; i < len; i++) _paths[i] = paths[i];
				}
			}
			
			_UpdateBounds();
		}
	}
	
	public Vector2[] verts {
		get {return _verts;}
		set {_verts = value;}
	}
	
	public Vector2[] GetPath(int index){return (index == 0 ? _verts : _paths[index - 1]);}
	public void SetPath(int index, Vector2[] path){SetPathRaw(index, path); _UpdateBounds();}
	private void SetPathRaw(int index, Vector2[] path){
		if(index == 0){
			_verts = path;
		} else {
			_paths[index - 1] = path;
		}
	}
	
	public int activePath {get {return _activePath;} set {_activePath = value;}}
	public bool looped {get {return _looped;} set {_looped = value;}}
	public LayerMask shadowLayers {get {return _shadowLayers;} set {_shadowLayers = value;}}
	public float lightPenetration {get {return _lightPenetration;} set {_lightPenetration = value;}}
	public float opacity {get {return _opacity;} set {_opacity = value;}}

	public static List<SFPolygon> _polygons = new List<SFPolygon>();
	private void OnEnable(){_polygons.Add(this);}
	private void OnDisable(){_polygons.Remove(this);}

	private void Start(){
		_UpdateBounds();
	}
	
	public void CopyFromCollider(Collider2D collider){
		var poly = collider as PolygonCollider2D;
		var box = collider as BoxCollider2D;
		if(poly){
			this.looped = true;
			var count = this.pathCount = poly.pathCount;
			
			for(int p = 0; p < count; p++){
				var path = poly.GetPath(p);
				for(int i = 0; i < path.Length; i++) path[i] = path[i] + poly.offset;
				System.Array.Reverse(path);
				this.SetPathRaw(p, path);
			}
			
			_UpdateBounds();
		} else if(box){
			SetBoxVerts(box.offset - 0.5f*box.size, box.offset + 0.5f*box.size);
		} else {
			Debug.LogWarning("CopyFromCollider() only works with polygon and box colliders.");
		}
	}
	
	public void _CopyFromCollider(){
		Collider2D collider = GetComponent<Collider2D>();
		if(collider){
			CopyFromCollider(collider);
		} else {
			Debug.LogWarning("GameObject has no polygon or box collider. Adding default SFPolygon shape instead.");
			SetBoxVerts(-Vector2.one, Vector2.one);
		}
	}
	
	private void SetBoxVerts(Vector2 min, Vector2 max){
		this.looped = true;
		this.pathCount = 1;
		this.verts = new Vector2[] {
			new Vector2(max.x, max.y),
			new Vector2(max.x, min.y),
			new Vector2(min.x, min.y),
			new Vector2(min.x, max.y)
		};
	}

	public void _FlipInsideOut(int index){
		if(index == -1){
			var pathCount = this.pathCount;
			for(int i = 0; i < pathCount; i++) System.Array.Reverse(GetPath(i));
		} else {
			System.Array.Reverse(GetPath(index));
		}
	}
}
