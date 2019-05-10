// Super Fast Soft Lighting. Copyright 2015 Howling Moon Software, LLP

// #define SF_DEBUG

using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SFRenderer : MonoBehaviour {
	public bool _renderInSceneView = true;

	private void ScenePreRender(Camera camera){
		if(_renderInSceneView && camera.cameraType == CameraType.SceneView){
			OnPreRender();
		}
	}

	private void ScenePostRender(Camera camera){
		if(_renderInSceneView && camera.cameraType == CameraType.SceneView){
			OnPostRender();
		}
	}

	private void OnEnable(){
		if(Application.isEditor){
			Camera.onPreRender += ScenePreRender;
			Camera.onPostRender += ScenePostRender;
		}
	}

	private void OnDisable(){
		if(Application.isEditor){
			Camera.onPreRender -= ScenePreRender;
			Camera.onPostRender -= ScenePostRender;
		}
	}

	private RenderTexture _lightMap;
	private RenderTexture _ShadowMap;

	[Tooltip("Blend the lights in linear space rather than gamma space. Nonlinear blending prevents oversaturation, but can cause draw order artifacts.")]
	public bool _linearLightBlending = true;
	public bool _shadows = true;
	[Tooltip("The global ambient light color- the ambient light is used to light your scene when no lights are affecting part of it. A darker grey, blue, or yellow is often a good place to start. Alpha unused. ")]
	public Color _ambientLight = Color.black;

	[Tooltip("Exposure is a multiplier applied to all lights in this renderer. Use to adjust all your lights at once. Particularly useful if you're using HDR lighting, otherwise it can be used to cause oversaturation.")]
	[FormerlySerializedAs("_globalDynamicRange")]
	public float _exposure = 1.0f;

	[Tooltip("Scale of the render texture for the colored lights. Larger numbers will give you blockier lights, but will run faster. Since lighting tends to " +
		"be pretty diffuse, high numbers like 8 usually look good here. Recommended values are between 8 - 32.")]
	public float _lightMapScale = 8;
	[Tooltip("Scale of the render texture for the colored lights. Larger numbers will give you blockier shadows," +
		" but will run faster. Blocky shadows tend to look worse than blocky lights, so this should usually be lower than the light map scale. Recommended values are between 2 - 8. Less if you have a lot of sharp shadows.")]
	public float _shadowMapScale = 4;

	[Tooltip("How far will light penetrate into each shadow casting object. Makes it look like objects that are casting shadows are illuminated by the lights.")]
	public float _minLightPenetration = 0.2f;

	[Tooltip("Extra darkening to apply to shadows to hide precision artifacts in the seams.")]
	public float _shadowCompensation = 1.01f;

	[Tooltip("The color of the fog color. The alpha controls the fog's strength.")]
	public Color _fogColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	[Tooltip("The scatter color is the color that the fog will glow when it is lit. Alpha is unused. Black disables illumination effects on the fog.")]
	public Color _scatterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
	[Tooltip("What percentage of unshadowed/shadowed light should apply to the fog. At 1.0, your shadows will be fully applied to the scattered light in your fog.")]
	public float _softHardMix = 0.0f;

	public bool linearLightBlending {get {return _linearLightBlending;} set {_linearLightBlending = value;}}
	public bool shadows {get {return _shadows;} set {_shadows = value;}}
	public Color ambientLight {get {return _ambientLight;} set {_ambientLight = value;}}
	public float exposure {get {return _exposure;} set {_exposure = value;}}
	public float lightMapScale {get {return _lightMapScale;} set {_lightMapScale = value;}}
	public float shadowMapScale {get {return _shadowMapScale;} set {_shadowMapScale = value;}}
	public float minLightPenetration {get {return _minLightPenetration;} set {_minLightPenetration = value;}}
	public float shadowCompensation {get {return _shadowCompensation;} set {_shadowCompensation = Mathf.Clamp(value, 1, 2);}}
	public Color fogColor {get {return _fogColor;} set {_fogColor = value;}}
	public Color scatterColor {get {return _scatterColor;} set {_scatterColor = value;}}
	public float softHardMix {get {return _softHardMix;} set {_softHardMix = value;}}

	[Obsolete("Please use SFRenderer.exposure instead.")]
	public float globalIlluminationScale {get {return _exposure;} set {_exposure = value;}}
	[Obsolete("Please use SFRenderer.exposure instead.")]
	public float globalDynamicRange {get {return _exposure;} set {_exposure = value;}}

	private Rect _extents = Rect.MinMaxRect(-1, -1, 1, 1);

	// Set the clip space extents of the lightmap.
	// Useful for extending rendering offscreen for SFSample.
	public Rect extents {
		get {return _extents;}
		set {_extents = value;}
	}

	private Material _shadowMaskMaterial;
	private Material shadowMaskMaterial {
		get {
			if(_shadowMaskMaterial == null){
				_shadowMaskMaterial = new Material(Shader.Find("Hidden/SFSoftShadows/ShadowMask"));
				_shadowMaskMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			return _shadowMaskMaterial;
		}
	}
	
	private Material _linearLightMaterial;
	private Material _softLightMaterial;
	private Material lightMaterial {
		get {
			if(_linearLightMaterial == null){
				_linearLightMaterial = new Material(Shader.Find("Hidden/SFSoftShadows/LightBlendLinear"));
				_linearLightMaterial.hideFlags = HideFlags.HideAndDontSave;

				_softLightMaterial = new Material(Shader.Find("Hidden/SFSoftShadows/LightBlendSoft"));
				_softLightMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			return (_linearLightBlending ? _linearLightMaterial : _softLightMaterial);
		}
	}
	
	private Material _HDRClampMaterial;
	private Material HDRClampMaterial {
		get {
			if(_HDRClampMaterial == null){
				_HDRClampMaterial = new Material(Shader.Find("Hidden/SFSoftShadows/HDRClamp"));
				_HDRClampMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			return _HDRClampMaterial;
		}
	}

	private Material _fogMaterial;
	private Material fogMaterial {
		get {
			if(_fogMaterial == null){
				_fogMaterial = new Material(Shader.Find("Hidden/SFSoftShadows/FogLayer"));
				_fogMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			return _fogMaterial;
		}
	}

	private Mesh _mesh;
	private Mesh sharedMesh {
		get {
			if(_mesh == null){
				_mesh = new Mesh();
				_mesh.MarkDynamic();
				_mesh.hideFlags = HideFlags.HideAndDontSave;
			}

			return _mesh;
		}
	}

	private bool UV_STARTS_AT_TOP;
	private static Matrix4x4 TEXTURE_FLIP_MATRIX = Matrix4x4.Scale(new Vector3(1.0f, -1.0f, 1.0f));

	private RenderTextureFormat lightmapFormat = RenderTextureFormat.ARGB1555;
	private RenderTextureFormat lightmapFormatHDR = RenderTextureFormat.ARGB1555;

	private void Start(){
		var graphicsAPI = SystemInfo.graphicsDeviceType;

		// Consoles or new platforms may need to be added here too.
		UV_STARTS_AT_TOP = (
			graphicsAPI == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9 ||
			graphicsAPI == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
			graphicsAPI == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12 ||
			graphicsAPI == UnityEngine.Rendering.GraphicsDeviceType.Metal
		);

		if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)){
			lightmapFormat = RenderTextureFormat.ARGB32;
		} else if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.BGRA32)){
			lightmapFormat = RenderTextureFormat.BGRA32;
		}

		if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)){
			lightmapFormatHDR = RenderTextureFormat.ARGBHalf;
		} else if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)){
			lightmapFormatHDR = RenderTextureFormat.ARGBFloat;
		}

		Debug.Log("SFSS init: " +
			graphicsAPI + ", " + 
			(UV_STARTS_AT_TOP ? "UV_STARTS_AT_TOP, " : "") +
			lightmapFormat + ", " + 
			lightmapFormatHDR
		);
	}

	private void OnDestroy(){
		if(_mesh) DestroyImmediate(_mesh);
	}

	public static Rect _TransformRect(Matrix4x4 m, Rect r){
		Vector4 c = m.MultiplyPoint3x4(new Vector4(r.x + 0.5f*r.width, r.y + 0.5f*r.height, 0.0f, 1.0f));
		float hw = 0.5f*Mathf.Max(Mathf.Abs(r.width*m[0] + r.height*m[4]), Mathf.Abs(r.width*m[0] - r.height*m[4]));
		float hh = 0.5f*Mathf.Max(Mathf.Abs(r.width*m[1] + r.height*m[5]), Mathf.Abs(r.width*m[1] - r.height*m[5]));
		return new Rect(c.x - hw, c.y - hh, 2.0f*hw, 2.0f*hh);
	}

	private static Vector2 ClampedProjection(Matrix4x4 m, float x, float y){
		var w = Math.Max(0.0f, x*m[3] + y*m[7] + m[15]);
		return new Vector2((x*m[0] + y*m[4] + m[12])/w, (x*m[1] + y*m[5] + m[13])/w);
	}

	// Project a light into pixel space to calculate a scissor rect.
	private static Rect ScissorRect(Matrix4x4 mvp, float w, float h){
		// Project the corners.
		var v0 = ClampedProjection(mvp, -1.0f, -1.0f);
		var v1 = ClampedProjection(mvp,  1.0f, -1.0f);
		var v2 = ClampedProjection(mvp,  1.0f,  1.0f);
		var v3 = ClampedProjection(mvp, -1.0f,  1.0f);

		// Find the bounds.
		var xMin = Mathf.Min(Mathf.Min(v0.x, v1.x), Mathf.Min(v2.x, v3.x));
		var yMin = Mathf.Min(Mathf.Min(v0.y, v1.y), Mathf.Min(v2.y, v3.y));
		var xMax = Mathf.Max(Mathf.Max(v0.x, v1.x), Mathf.Max(v2.x, v3.x));
		var yMax = Mathf.Max(Mathf.Max(v0.y, v1.y), Mathf.Max(v2.y, v3.y));

			#if SF_DEBUG
				_DebugClipRect(Rect.MinMaxRect(
					Mathf.Max(-1.0f, xMin),
					Mathf.Max(-1.0f, yMin),
					Mathf.Min( 1.0f, xMax),
					Mathf.Min( 1.0f, yMax)
				), Color.yellow);
			#endif

		// Convert to a pixel rectangle.
		return Rect.MinMaxRect(
			Mathf.Max(0.0f, Mathf.Floor((0.5f*xMin + 0.5f)*w)),
			Mathf.Max(0.0f, Mathf.Floor((0.5f*yMin + 0.5f)*h)),
			Mathf.Min(w, Mathf.Ceil((0.5f*xMax + 0.5f)*w)),
			Mathf.Min(h, Mathf.Ceil((0.5f*yMax + 0.5f)*h))
		);
	}
	
	// Calculate a clip matrix given the viewport pixel rect and 2/width, 2/height
	private static Matrix4x4 ClipMatrix(Rect r, float dw, float dh){
		float x = r.x*dw - 1.0f;
		float y = r.y*dh - 1.0f;
		return Matrix4x4.Ortho(x, x + r.width*dw, y, y + r.height*dh, -1.0f, 1.0f);
	}
	
	private static void CullPolys(List<SFPolygon> polys, Rect bounds, List<SFPolygon> culledPolygons){
		for(int i = 0; i < polys.Count; i++){
			var poly = polys[i];
			if(bounds.Overlaps(poly._WorldBounds)){
				culledPolygons.Add(poly);
			}
		}
	}

	private Matrix4x4 TextureProjectionMatrix(Matrix4x4 m){
		m.SetRow(2, new Vector4(0, 0, 1, 0));
		var textureMatrix = m.inverse;

		// Viewport and texture coordinates are flipped on DirectX and Metal.
		// Need to flip the projection going in, then flip the texture coordinates coming out.
		if(UV_STARTS_AT_TOP) textureMatrix = TEXTURE_FLIP_MATRIX*textureMatrix*TEXTURE_FLIP_MATRIX;

		return textureMatrix;
	}

	private List<SFPolygon> _perLightCulledPolygons = new List<SFPolygon>();

	private void RenderLightMap(Matrix4x4 viewMatrix, Matrix4x4 projection, Matrix4x4 vpMatrix, RenderTexture target, List<SFLight> lights, List<SFPolygon> polys, Color ambient, bool hdr){
		var w = target.width;
		var h = target.height;

		var CLIP_RECT = new Rect(-1.0f, -1.0f, 2.0f, 2.0f);
		var UNIT_RECT = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		
		Graphics.SetRenderTarget(target);
		GL.Clear(false, true, ambient);
		
		for(int i = 0; i < lights.Count; i++){
			var light = lights[i];
			if(!light.enabled) continue;

			var lightMatrix = light._ModelMatrix(false)*light._CookieMatrix();
			var scissorRect = ScissorRect(vpMatrix*lightMatrix, w, h);
			var clippedProjection = ClipMatrix(scissorRect, 2.0f/w, 2.0f/h)*projection;

			GL.Viewport(scissorRect);
			GL.LoadProjectionMatrix(clippedProjection);
			
			// Draw shadow mask
			if(polys != null && light._shadowLayers != 0){
				CullPolys(polys, light._CalcCullBounds(vpMatrix), _perLightCulledPolygons);
				UnityEngine.Profiling.Profiler.BeginSample("SFSS-BuildShadowMesh");
				var mesh = light._BuildShadowMesh(this.sharedMesh, _perLightCulledPolygons, _minLightPenetration);
				_perLightCulledPolygons.Clear();
				UnityEngine.Profiling.Profiler.EndSample();

				if(mesh != null){
					// Note: DrawMesh apparently not affected by the "GL" transform.
					this.shadowMaskMaterial.SetPass(0);
					Graphics.DrawMeshNow(mesh, light._ModelMatrix(true));
					mesh.Clear();
				}
				
				// Clamp the shadow mask to 0 when rendering to a floating point HDR lightmap.
				if(hdr) Graphics.DrawTexture(CLIP_RECT, Texture2D.blackTexture, this.HDRClampMaterial);
			}

			// Can't use '??' operator because of Unity magic.
			var cookie = light._cookieTexture;
			if(!cookie) cookie = Texture2D.whiteTexture;

			var material = this.lightMaterial;
			if(_linearLightBlending) material.SetFloat("_intensity", light._intensity);

			// Composite the light by drawing a fullscreen, (but scissored) quad with the light's cookie texture projected onto it.
			// Abuse the projection matrix for this since there isn't really a better way to pass the texture's transform.
			GL.LoadProjectionMatrix(TextureProjectionMatrix(clippedProjection*viewMatrix*lightMatrix));
			Graphics.DrawTexture(CLIP_RECT, cookie, UNIT_RECT, 0, 0, 0, 0, light._color, material);
		}
	}

	private RenderTexture GetTexture(Camera cam, Matrix4x4 extensionInv, float downscale){
		var size = extensionInv*cam.pixelRect.size/downscale;
		var format = (cam.allowHDR ? lightmapFormatHDR : lightmapFormat);
		return RenderTexture.GetTemporary((int)size.x, (int)size.y, 0, format);
	}
	
	public static bool _FastCull(Matrix4x4 mvp, Rect bounds){
		var center = bounds.center;
		var extents = 0.5f*bounds.size;

		// Clip space center and extents.
		var c = mvp*(new Vector4(center.x, center.y, 0.0f, 1.0f));
		var ex = extents.x*Mathf.Abs(mvp[0]) + extents.y*Mathf.Abs(mvp[4]);
		var ey = extents.x*Mathf.Abs(mvp[1]) + extents.y*Mathf.Abs(mvp[5]);
		var ez = extents.x*Mathf.Abs(mvp[2]) + extents.y*Mathf.Abs(mvp[6]);
		
		// Check the clip space center against the viewport using a conservative w-value.
		var w = Mathf.Max(0.0f, c.w + extents.x*Mathf.Abs(mvp[3]) + extents.y*Mathf.Abs(mvp[7]));
		return ((Mathf.Abs(c.x) - ex < w) && (Mathf.Abs(c.y) - ey < w) && (Mathf.Abs(c.z) - ez < w));
	}

	private static Rect CullLights(Matrix4x4 vpMatrix, List<SFLight> lights, List<SFLight> culledLights){
		float lCull =  Mathf.Infinity;
		float bCull =  Mathf.Infinity;
		float rCull = -Mathf.Infinity;
		float tCull = -Mathf.Infinity;

		for(int i = 0; i < lights.Count; i++){
			var light = lights[i];

			if(_FastCull(vpMatrix*light._ModelMatrix(false), light._bounds)){
				culledLights.Add(light);

				if(light._shadowLayers != 0){
					var cullRect = light._CalcCullBounds(vpMatrix);
					lCull = Mathf.Min(lCull, cullRect.xMin);
					bCull = Mathf.Min(bCull, cullRect.yMin);
					rCull = Mathf.Max(rCull, cullRect.xMax);
					tCull = Mathf.Max(tCull, cullRect.yMax);
					#if SF_DEBUG
						_DebugRect(cullRect, Color.red);
					#endif
				}
			}
		}

		// Return max culling bounds for polygons.
		return Rect.MinMaxRect(lCull, bCull, rCull, tCull);
	}

	private List<SFLight> _culledLights = new List<SFLight>();
	private List<SFPolygon> _culledPolygons = new List<SFPolygon>();

	private void OnPreRender(){
		var ambientLight = _ambientLight; ambientLight.a = 1f;
		var extensionMatrix = Matrix4x4.Ortho(_extents.xMin, _extents.xMax, _extents.yMin, _extents.yMax, 1, -1);
		var extensionInv = extensionMatrix.inverse;

		var savedColorBuffer = Graphics.activeColorBuffer;
		var savedDepthBuffer = Graphics.activeDepthBuffer;
		var cam = Camera.current;
		var hdr = cam.allowHDR;

		var viewMatrix = cam.worldToCameraMatrix;
		var projection = extensionMatrix*cam.projectionMatrix;
		var vpMatrix = projection*viewMatrix;

		var lights = SFLight._lights;
		var polys = SFPolygon._polygons;

		// Extra slow fallback for editor mode to find and init the objects.
		if(!Application.isPlaying){
			lights = new List<SFLight>(Component.FindObjectsOfType<SFLight>().Where((o) => o.isActiveAndEnabled));
			polys = new List<SFPolygon>(Component.FindObjectsOfType<SFPolygon>().Where((o) => o.isActiveAndEnabled));

			// Force polys to calculate their initial bounds.
			foreach(var p in polys) p._UpdateBounds();
		}

		var polyCullBounds = CullLights(vpMatrix, lights, _culledLights);
		#if SF_DEBUG
			_DebugRect(polyCullBounds, Color.blue);
		#endif

		GL.PushMatrix();

		UnityEngine.Profiling.Profiler.BeginSample("SFSS-RenderLightMap " + _culledLights.Count + " lights");
		_lightMap = GetTexture(cam, extensionInv, _lightMapScale);
		RenderLightMap(viewMatrix, projection, vpMatrix, _lightMap, _culledLights, null, ambientLight, hdr);
		UnityEngine.Profiling.Profiler.EndSample();
		
		if(_shadows){
			UnityEngine.Profiling.Profiler.BeginSample("SFSS-RenderShadowMap");
			for(int i = 0; i < polys.Count; i++) polys[i]._CacheWorldBounds();
			CullPolys(polys, polyCullBounds, _culledPolygons);

			Shader.SetGlobalFloat("_SFShadowCompensation", _shadowCompensation);

			_ShadowMap = GetTexture(cam, extensionInv, _shadowMapScale);
			RenderLightMap(viewMatrix, projection, vpMatrix, _ShadowMap, _culledLights, _culledPolygons, ambientLight, hdr);
			_culledPolygons.Clear();
			UnityEngine.Profiling.Profiler.EndSample();
		}
		GL.PopMatrix();
		
		// Lights is a cached list. Clear it now since we are done with the contents.
		_culledLights.Clear();
		
		Graphics.SetRenderTarget(null);
		GL.Viewport(cam.pixelRect);
		
		Shader.SetGlobalMatrix("_SFProjection", extensionMatrix*Camera.current.projectionMatrix);
		Shader.SetGlobalColor("_SFAmbientLight", ambientLight);
		Shader.SetGlobalFloat("_SFExposure", _exposure);
		Shader.SetGlobalTexture("_SFLightMap", _lightMap);
		Shader.SetGlobalTexture("_SFLightMapWithShadows", _shadows ? _ShadowMap : _lightMap);

		Graphics.SetRenderTarget (savedColorBuffer, savedDepthBuffer);
	}
	
	private void OnPostRender(){
		var fogCheck = _fogColor.a + _scatterColor.r + _scatterColor.g + _scatterColor.b;
		if(fogCheck > 0.0f){
			GL.PushMatrix(); {
				// Unity may or may not apply magic fixes to the projection matrix.
				// Loading an identity projection lets us easily take advantage of any flipping it might do.
				GL.LoadProjectionMatrix(Matrix4x4.identity);

				var scatter = _scatterColor;
				scatter.a = _softHardMix;
				
				var mat = this.fogMaterial;
				mat.SetColor("_FogColor", _fogColor);
				mat.SetColor("_Scatter", scatter);
				mat.SetPass(0);
				
				Graphics.DrawTexture(new Rect(-1.0f, -1.0f, 2.0f, 2.0f), Texture2D.blackTexture, mat);
			} GL.PopMatrix();
		}

		Shader.SetGlobalColor("_SFAmbientLight", Color.white);
		Shader.SetGlobalFloat("_SFExposure", 1.0f);
		Shader.SetGlobalTexture("_SFLightMap", Texture2D.whiteTexture);
		Shader.SetGlobalTexture("_SFLightMapWithShadows", Texture2D.whiteTexture);
		RenderTexture.ReleaseTemporary(_lightMap); _lightMap = null;
		RenderTexture.ReleaseTemporary(_ShadowMap); _ShadowMap = null;
	}

#if SF_DEBUG
	static public void _DebugRect(Rect r, Color color, float duration = 0.0f){
		var a = new Vector3(r.xMin, r.yMin);
		var b = new Vector3(r.xMax, r.yMin);
		var c = new Vector3(r.xMax, r.yMax);
		var d = new Vector3(r.xMin, r.yMax);

		Debug.DrawLine(a, b, color, duration);
		Debug.DrawLine(b, c, color, duration);
		Debug.DrawLine(c, d, color, duration);
		Debug.DrawLine(d, a, color, duration);
	}

	static public void _DebugClipRect(Rect r, Color color, float duration = 0.1f){
		if(Camera.current != Camera.main) return;

		var cam = Camera.main;
		var vp_inv = (cam.projectionMatrix*cam.worldToCameraMatrix).inverse;

		var a = vp_inv.MultiplyPoint(new Vector3(r.xMin, r.yMin));
		var b = vp_inv.MultiplyPoint(new Vector3(r.xMax, r.yMin));
		var c = vp_inv.MultiplyPoint(new Vector3(r.xMax, r.yMax));
		var d = vp_inv.MultiplyPoint(new Vector3(r.xMin, r.yMax));

		Debug.DrawLine(a, b, color, duration);
		Debug.DrawLine(b, c, color, duration);
		Debug.DrawLine(c, d, color, duration);
		Debug.DrawLine(d, a, color, duration);
	}
#endif
}
