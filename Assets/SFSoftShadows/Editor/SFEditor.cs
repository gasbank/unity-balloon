// Super Fast Soft Lighting. Copyright 2015 Howling Moon Software, LLP

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class SFAbstractEditor : Editor {
	protected delegate void ValidateBlock(SerializedProperty property);
	
	protected void PropertyField(string name, ValidateBlock validate = null){
		var property = this.serializedObject.FindProperty(name);
		
		EditorGUI.BeginChangeCheck(); {
			EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
			if(GUI.changed && validate != null) validate(property);
		} EditorGUI.EndChangeCheck();
	}

	protected void PropertySliderField(string name){
		var property = this.serializedObject.FindProperty(name);

		EditorGUI.BeginChangeCheck(); {
			EditorGUILayout.Slider(property, 0f, 1f);
		} EditorGUI.EndChangeCheck();
	}
}

[CustomEditor(typeof(SFLight))]
[CanEditMultipleObjects]
public class SFLightEditor : SFAbstractEditor {
	public override void OnInspectorGUI(){
		this.serializedObject.Update();
		
		PropertyField("_radius", (p) => p.floatValue = Mathf.Max(0.0f, p.floatValue));
		PropertyField("_intensity", (p) => p.floatValue = Mathf.Max(0.0f, p.floatValue));
		PropertyField("_color");
		PropertyField("_cookieTexture");
		PropertyField("_shadowLayers");
		PropertyField("_parallaxLight");

		this.serializedObject.ApplyModifiedProperties();
	}
	
	private void OnSceneGUI(){
		var light = this.target as SFLight;
		
		var size = light.GetComponent<RectTransform>().sizeDelta;
		var max = Mathf.Min(size.x, size.y);
		
		Handles.matrix = light._ModelMatrix(true);
		Handles.color = Color.yellow;
		light.radius = Mathf.Min(max, Handles.RadiusHandle(Quaternion.identity, Vector3.zero, light.radius));

		Handles.matrix *= light._CookieMatrix();
		Handles.color = Color.white;
		Handles.DrawSolidRectangleWithOutline(new Vector3[]{
			new Vector3(-1.0f, -1.0f, 0.0f),
			new Vector3(-1.0f,  1.0f, 0.0f),
			new Vector3( 1.0f,  1.0f, 0.0f),
			new Vector3( 1.0f, -1.0f, 0.0f),
		}, new Color(0.5f, 0.5f, 0.5f, 0.1f), Color.white);
		
		if(GUI.changed) EditorUtility.SetDirty(target);
	}
}

[CustomEditor(typeof(SFPolygon))]
[CanEditMultipleObjects]
public class SFPolygonEditor : SFAbstractEditor {
	public static bool inEditMode = false;

	public void OnDisable(){
		// OnDisable for editor scripts is essentially used when you change focus.
		inEditMode = false;
	}

	private static Vector2 ClosestHelper(Vector2 p, Vector2 a, Vector2 b){
		var delta = a - b;
		var t = Mathf.Clamp01(Vector2.Dot(delta, p - b)/delta.sqrMagnitude);
		return b + delta*t;
	}

	protected Vector3 ClosestPoint(Transform t, List<Vector2> verts, out int index){
//		// Duplicate the verts list and make it into a loop.
//		var loop = new List<Vector2>(verts);
//		loop.Add(verts[0]);
//		Vector3[] arr = loop.Select<Vector2, Vector3>((arg) => (Vector3)arg).ToArray();

		var mouse = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
		var p = t.InverseTransformPoint(mouse);

		float closest = Mathf.Infinity;
		index = 0;

		for(int i=1; i<verts.Count; i++){
			var a = verts[i - 1];
			var b = verts[i];

			var d = HandleUtility.DistancePointToLineSegment(p, a, b);
			if(d < closest){
				closest = d;
				index = i;
			}
		}

		return ClosestHelper(p, verts[index - 1], verts[index]);
	}
	
	private void PathGUI(SFPolygon poly, int pathIndex){
		bool dirty = false;
		var path = poly.GetPath(pathIndex);
		
		bool invalidVerts = true;
		for(int i = 0; i < path.Length && invalidVerts; i++){
			invalidVerts &= (path[i] == Vector2.zero);
		}
		
		if(invalidVerts){
			Debug.Log("Invalid verts, attempting to set up from collider.");
			// copy in collider verts if possible.
			poly._CopyFromCollider();
			
			path = poly.GetPath(pathIndex);
			dirty = true;
		}
		
		var verts = new List<Vector2>(path);
		
		// Copy of verts that is looped if the polygon is looped.
		var looped = new List<Vector2>(verts);
		if(poly.looped) looped.Add(looped[0]);
		
		Handles.matrix = poly.transform.localToWorldMatrix;
		
		if(!inEditMode){
			Handles.color = new Color(1f, 1f, 0f, 0.4f);
		} else {
			SetupUndo("edited SFPolygon");
			
			var removePressed = ((Event.current.modifiers & (EventModifiers.Command | EventModifiers.Control)) != 0);
			var addPressed = ((Event.current.modifiers & (EventModifiers.Shift)) != 0);
			
			if(removePressed){
				if(verts.Count > 2){
					Handles.color = Color.red;
				
					for(int i = 0; i < verts.Count; i++){
						var handleSize = 0.05f * HandleUtility.GetHandleSize (verts [i]);
						if(Handles.Button (verts [i], Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap)){
							verts.RemoveAt(i);
							dirty = true;
							break;
						}
					}
				}
			} else if(addPressed){
				int insertIndex = 0;
				Vector3 insertPosition = ClosestPoint(poly.transform, looped, out insertIndex);
				var s = HandleUtility.GetHandleSize(insertPosition) * 0.05f;
				
				// Draw the existing vertexes
				Handles.color = Color.white;
				for(int i = 0; i < verts.Count; i++){
					Handles.DotHandleCap(0, verts [i], Quaternion.identity, s, EventType.Layout);
				}
				
				// Draw the insert handle
				Handles.color = Color.green;
				if(Handles.Button (insertPosition, Quaternion.identity, s, s, Handles.DotHandleCap)){
					verts.Insert(insertIndex, (Vector2)insertPosition);
					dirty = true;
				}
				
				HandleUtility.Repaint();
			} else {
				// Move an existing vertex
				Handles.color = Color.white;
				for(int i = 0; i < verts.Count; i++){
					Vector3 v = verts [i];
					Vector2 delta = DotHandle (v) - (Vector2)v;
					if(delta != Vector2.zero){
						verts [i] = (Vector2)v + delta;
						dirty = true;
					}
				}
			}
			
			Handles.color = Color.white;
		}
		
		Handles.DrawPolyLine(looped.Select(v => (Vector3)v).ToArray());
		
		if(dirty){
			// Unity 5.3 and later only
			Undo.RecordObject (poly, "Move Shadow Geometry");
			poly.SetPath(pathIndex, verts.ToArray());
		}
	}
	
	protected void OnSceneGUI(){
		var poly = target as SFPolygon;
		if(poly == null) return;
		
		var activePath = poly.activePath;
		if(activePath >= 0){
			PathGUI(poly, activePath);
		} else {
			var pathCount = poly.pathCount;
			for(int p = 0; p < pathCount; p++) PathGUI(poly, p);
		}
	}
	
	protected Vector2 CircleHandle(Vector3 pos){
		float size = HandleUtility.GetHandleSize(pos)*0.2f;
		return Handles.FreeMoveHandle(pos, Quaternion.identity, size, Vector3.zero, Handles.CircleHandleCap);
	}

	protected Vector2 DotHandle(Vector3 pos, float size = 0.05f){
		float s = HandleUtility.GetHandleSize(pos)*size;
		return Handles.FreeMoveHandle(pos, Quaternion.identity, s, Vector3.zero, Handles.DotHandleCap);
	}

	protected void SetupUndo(string message){
		if(Input.GetMouseButtonDown(0)){
			Undo.RecordObject(target, message);
		}
	}

	public override void OnInspectorGUI(){
		this.serializedObject.Update();
		SFPolygon poly = this.target as SFPolygon;

		EditorGUILayout.HelpBox("When editing the shadow geometry, shift+click to add new points. Command+click or control+click to remove point.", MessageType.Info);
		
		// PropertyField("pathCount", (p) => p.intValue = Math.Max(0, p.intValue));
		PropertyField("_activePath", (p) => p.intValue = Math.Min(Math.Max(-1, p.intValue), poly.pathCount - 1));
		poly.pathCount = Math.Max(1, EditorGUILayout.DelayedIntField("Path Count", poly.pathCount));
		
		GUILayout.BeginHorizontal("box"); {
			if(GUILayout.Button("<")){
				foreach(SFPolygon t in this.targets){
					t.activePath = Math.Max(0, t.activePath - 1);
					EditorUtility.SetDirty(t);
				}
			}
			
			if(GUILayout.Button(">")){
				foreach(SFPolygon t in this.targets){
					t.activePath = Math.Min(t.pathCount - 1, t.activePath + 1);
					EditorUtility.SetDirty(t);
				}
			}
			
			if(GUILayout.Button("All Paths")){
				foreach(SFPolygon t in this.targets){
					t.activePath = -1;
					EditorUtility.SetDirty(t);
				}
				SFPolygonEditor.inEditMode = false;
			}
			
			if(poly.GetComponent<PolygonCollider2D>() || poly.GetComponent<BoxCollider2D>()){
				Undo.RecordObjects(this.targets, "Copy from Collider");
				if(GUILayout.Button("Copy from Collider")){
					foreach(SFPolygon t in this.targets){
						t._CopyFromCollider();
						EditorUtility.SetDirty(t);
					}
				}
			}
		} GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal("box"); {
			if(GUILayout.Button(SFPolygonEditor.inEditMode ? "Stop Editing" : "Edit Path")){
				SFPolygonEditor.inEditMode = !SFPolygonEditor.inEditMode;
				EditorUtility.SetDirty(target);
			}
			
			if(GUILayout.Button("Flip Inside-Out")){
				Undo.RecordObjects(this.targets, "Flip Inside-Out");
				foreach(SFPolygon t in this.targets){
					t._FlipInsideOut(t.activePath);
					EditorUtility.SetDirty(t);
				}
			}
		} GUILayout.EndHorizontal ();
		
		PropertyField("_looped");
		PropertyField("_shadowLayers");
		PropertyField("_lightPenetration", (p) => p.floatValue = Mathf.Max(0.0f, p.floatValue));
		PropertyField("_opacity", (p) => p.floatValue = Mathf.Clamp01(p.floatValue));

		this.serializedObject.ApplyModifiedProperties();
	}
}

[CustomEditor(typeof(SFRenderer))]
[CanEditMultipleObjects]
public class SFRendererEditor : SFAbstractEditor {
	public override void OnInspectorGUI(){
		this.serializedObject.Update();
		PropertyField("_renderInSceneView");

		PropertyField("_linearLightBlending");
		PropertyField("_shadows");
		PropertyField("_ambientLight");
		PropertyField("_exposure", (p) => p.floatValue = Mathf.Max(0.0f, p.floatValue));
		PropertyField("_minLightPenetration", (p) => p.floatValue = Math.Max(0.0f, p.floatValue));
		PropertyField("_shadowCompensation", (p) => p.floatValue = Mathf.Clamp(p.floatValue, 1, 2));
		
		PropertyField("_lightMapScale", (p) => p.floatValue = Mathf.Max(1.0f, p.floatValue));
		PropertyField("_shadowMapScale", (p) => p.floatValue = Mathf.Max(1.0f, p.floatValue));
		
		PropertyField("_fogColor");
		PropertyField("_scatterColor");
		PropertySliderField("_softHardMix");
		
		this.serializedObject.ApplyModifiedProperties();
	}
}

[CustomEditor(typeof(SFSample))]
[CanEditMultipleObjects]
public class SFSampleEditor : SFAbstractEditor {
	public override void OnInspectorGUI(){
		this.serializedObject.Update();
		
		PropertyField("_samplePosition", delegate(SerializedProperty p){
			foreach(var obj in this.serializedObject.targetObjects){
				(obj as SFSample).samplePosition = p.vector2Value;
			}
		});
		PropertyField("_lineSample", delegate(SerializedProperty p){
			foreach(var obj in this.serializedObject.targetObjects){
				(obj as SFSample).lineSample = p.boolValue;
			}
		});
		
		this.serializedObject.ApplyModifiedProperties();
	}

	protected void OnSceneGUI(){
		SFSample sampler = target as SFSample;
		if (sampler == null )
			return;
		
		SetupUndo("edited SFSample");

		Handles.color = Color.white;

		Vector3 v = sampler.samplePosition;
		Handles.matrix = sampler.transform.localToWorldMatrix;

		Vector2 delta = DotHandle (v) - (Vector2)v;
		if(delta != Vector2.zero){
			sampler.samplePosition = (Vector2)v + delta;
			EditorUtility.SetDirty(target);
		}
	}

	protected void SetupUndo(string message){
		if(Input.GetMouseButtonDown(0)){
			Undo.RecordObject(target, message);
		}
	}

	protected Vector2 DotHandle(Vector3 pos, float size = 0.05f){
		float s = HandleUtility.GetHandleSize(pos)*size;
		return Handles.FreeMoveHandle(pos, Quaternion.identity, s, Vector3.zero, Handles.DotHandleCap);
	}
}

public class SFMenus : ScriptableObject {
	public static void AddComponents<T>() where T : Component{
		foreach(var go in Selection.gameObjects){
			Undo.RecordObject(go, "add " + typeof(T).Name);
			go.AddComponent<T>();
		}
	}

	const string ROOT = "Component/SFShadow/";
	
	[MenuItem (ROOT + "SFRenderer", false, 1000)]
	public static void AddRenderer(){
		AddComponents<SFRenderer>();
	}
	
	[MenuItem (ROOT + "SFRenderer", true, 1000)]
	public static bool CheckRenderer(){
		return Selection.activeGameObject;
	}
	
	[MenuItem (ROOT + "SFLight", false, 1100)]
	public static void AddLight(){
		AddComponents<SFLight>();

		var cookie = AssetDatabase.LoadAssetAtPath("Assets/SFSoftShadows/CookieTextures/RadialFalloff.png", typeof(Texture2D)) as Texture2D;
		foreach(var go in Selection.gameObjects){
			go.GetComponent<SFLight>().cookieTexture = cookie;
		}
	}
	
	[MenuItem (ROOT + "SFLight", true, 1100)]
	public static bool CheckLight(){
		return Selection.activeGameObject;
	}
	
	[MenuItem (ROOT + "SFPolygon", false, 1101)]
	public static void AddPolygon(){
		AddComponents<SFPolygon>();
	}
	
	[MenuItem (ROOT + "SFPolygon", true, 1101)]
	public static bool CheckPolygon(){
		return Selection.activeGameObject;
	}
}
