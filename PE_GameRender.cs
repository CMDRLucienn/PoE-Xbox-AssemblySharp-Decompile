using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;

[ExecuteInEditMode]
public class PE_GameRender : MonoBehaviour
{
	public class CameraDepthComparer : IComparer<Camera>
	{
		private static CameraDepthComparer s_instance;

		public static CameraDepthComparer Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new CameraDepthComparer();
				}
				return s_instance;
			}
		}

		public int Compare(Camera a, Camera b)
		{
			if (a == null || b == null)
			{
				return 0;
			}
			if (a.depth > b.depth)
			{
				return 1;
			}
			if (a.depth == b.depth)
			{
				return 0;
			}
			return -1;
		}
	}

	public enum ScreenTextureType
	{
		None,
		Depth,
		Normals,
		AlbedoSpec,
		DynamicShadow,
		DynamicShadowBuffer,
		DynamicDepth,
		MAX
	}

	public static Vector4 s_cameraFrustumRay00 = Vector4.zero;

	public static Vector4 s_cameraFrustumRay10 = Vector4.zero;

	public static Vector4 s_cameraFrustumRay01 = Vector4.zero;

	public static Vector4 s_cameraFrustumRay11 = Vector4.zero;

	public static Vector3 s_cameraDirection = Vector3.zero;

	public RenderTexture m_screenTexture_background;

	public RenderTexture m_screenTexture_depth;

	public RenderTexture m_screenTexture_depth_backgroundOnly;

	public RenderTexture m_screenTexture_normals;

	public RenderTexture m_screenTexture_albedoSpec;

	public RenderTexture m_screenTexture_dynamicShadow;

	public Texture2D m_defaultNormalTexture;

	public Texture2D m_defaultWhiteTexture;

	public Texture2D m_defaultBlackTexture;

	public Texture2D m_defaultGrayTexture;

	public Texture2D m_ambientMap;

	public Texture2D m_shadowMap;

	public Texture3D m_identityColorGradeLut;

	public Vector3 m_fauxViewPos000;

	public Vector3 m_fauxViewPos001;

	public Vector3 m_fauxViewPos100;

	public Vector3 m_fauxViewPos101;

	public Vector3 m_fauxViewPos010;

	public Vector3 m_fauxViewPos011;

	public Vector3 m_fauxViewPos110;

	public Vector3 m_fauxViewPos111;

	public bool m_blockoutMode;

	public Material m_depthMaterial;

	public Material m_shadowDepthMaterial;

	public Material m_occlusionMaterial;

	public Shader m_dynamicDepthShader;

	public Shader m_dynamicAlbedoShader;

	public Shader m_dynamicNormalShader;

	public float mipMapBias_characterDiffuse = -1f;

	public float mipMapBias_characterNormal = -1f;

	public float mipMapBias_background = -2f;

	private int m_screen_width;

	private int m_screen_height;

	private bool m_ccNeedsReset;

	private ArrayList m_visibleStreamTileObjectList;

	private ArrayList m_visibleDynamicMeshList;

	private CameraControl m_cameraControl;

	private PE_StreamTileManager m_streamTileManager;

	private PE_DynamicShadowBuffer m_dynamicShadowBuffer;

	private PE_DayNightRender m_dayNightRender;

	private PE_LightEnvironment m_lightEnvironment;

	private PE_DeferredLightPass m_deferredLightPass;

	private SyncCameraOrthoSettings m_syncCameraOrthoSettings;

	private RenderBuffer[] m_mrtBuffers;

	private Material m_bufferCopyMaterial;

	private Light m_directionalLight;

	private Camera m_camera_dynamicAlbedo;

	private Camera m_camera_dynamicDepth;

	private Camera m_camera_dynamicNormal;

	private Camera m_camera_dynamicShadowBuffer;

	private LevelInfo m_levelInfo;

	public static PE_GameRender Instance = null;

	public static bool IsEditorNotPlayer()
	{
		if (Application.isEditor)
		{
			return !Application.isPlaying;
		}
		return false;
	}

	public PE_DayNightRender GetDayNightRender()
	{
		return m_dayNightRender;
	}

	public PE_LightEnvironment GetLightEnvironment()
	{
		return m_lightEnvironment;
	}

	public PE_DeferredLightPass GetDeferredLightPass()
	{
		return m_deferredLightPass;
	}

	public PE_StreamTileManager GetStreamTileManager()
	{
		return m_streamTileManager;
	}

	public SyncCameraOrthoSettings GetSyncCameraOrthoSettings()
	{
		return m_syncCameraOrthoSettings;
	}

	public RenderTexture GetScreenTextureType(ScreenTextureType type)
	{
		switch (type)
		{
		case ScreenTextureType.Depth:
			return m_screenTexture_depth;
		case ScreenTextureType.Normals:
			return m_screenTexture_normals;
		case ScreenTextureType.AlbedoSpec:
			return m_screenTexture_albedoSpec;
		case ScreenTextureType.DynamicShadow:
			return m_screenTexture_dynamicShadow;
		default:
			Debug.LogError("PE_GameRender::GetScreenTextureType() - Invalid texture type");
			return null;
		}
	}

	public void SetScreenTextureMode(ScreenTextureType textureType, bool setStreamTiles, bool setDynamics)
	{
		if (!setStreamTiles)
		{
			return;
		}
		foreach (GameObject visibleStreamTileObject in m_visibleStreamTileObjectList)
		{
			PE_StreamTile[] componentsInChildren = visibleStreamTileObject.GetComponentsInChildren<PE_StreamTile>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SwapTexture(textureType);
			}
		}
	}

	public void RegisterVisibleStreamTile(GameObject game_obj)
	{
		if (IsEditorNotPlayer() && m_visibleStreamTileObjectList == null)
		{
			m_visibleStreamTileObjectList = new ArrayList();
		}
		bool flag = false;
		foreach (GameObject visibleStreamTileObject in m_visibleStreamTileObjectList)
		{
			if (visibleStreamTileObject == game_obj)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_visibleStreamTileObjectList.Add(game_obj);
		}
	}

	public void UnregisterVisibleStreamTile(GameObject game_obj)
	{
		if (m_visibleStreamTileObjectList != null)
		{
			m_visibleStreamTileObjectList.Remove(game_obj);
		}
	}

	[Obsolete]
	public void RegisterVisibleDynamicMesh(PE_DynamicMeshComponent dynamicMeshComponent)
	{
		if (IsEditorNotPlayer() && m_visibleDynamicMeshList == null)
		{
			m_visibleDynamicMeshList = new ArrayList();
		}
		bool flag = false;
		foreach (PE_DynamicMeshComponent visibleDynamicMesh in m_visibleDynamicMeshList)
		{
			if (visibleDynamicMesh == dynamicMeshComponent)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_visibleDynamicMeshList.Add(dynamicMeshComponent);
		}
	}

	[Obsolete]
	public void UnregisterVisibleDynamicMesh(PE_DynamicMeshComponent dynamicMeshComponent)
	{
		m_visibleDynamicMeshList.Remove(dynamicMeshComponent);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (IsEditorNotPlayer() || m_blockoutMode)
		{
			Graphics.Blit(source, destination);
			return;
		}
		PE_StreamTile[] tilesInFrustum = m_streamTileManager.GetTilesInFrustum();
		int numTilesInFrustum = m_streamTileManager.GetNumTilesInFrustum();
		if (!m_screenTexture_depth.IsCreated())
		{
			CreateRenderTargets(m_screen_width, m_screen_height);
			UpdateShaderFauxViewVectors();
			UpdateCameraForDynamicGBuffer(m_camera_dynamicAlbedo, ScreenTextureType.AlbedoSpec);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicDepth, ScreenTextureType.Depth);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicNormal, ScreenTextureType.Normals);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicShadowBuffer, ScreenTextureType.DynamicShadow);
		}
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(m_mrtBuffers, m_screenTexture_depth.depthBuffer);
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		for (int i = 0; i < numTilesInFrustum; i++)
		{
			GL.Begin(7);
			PE_StreamTile obj = tilesInFrustum[i];
			Vector3[] verts = obj.GetVerts();
			obj.ApplyShaderSettingsForMRT();
			obj.GetMRTMaterial().SetPass(0);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(verts[0].x, verts[0].y, verts[0].z);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(verts[1].x, verts[1].y, verts[1].z);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(verts[3].x, verts[3].y, verts[3].z);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(verts[2].x, verts[2].y, verts[2].z);
			GL.End();
		}
		RenderTexture.active = active;
		Graphics.Blit(m_screenTexture_depth_backgroundOnly, m_screenTexture_depth);
		Camera component = base.gameObject.GetComponent<Camera>();
		CopyMainCameraSettingsToGBufferCamera(m_camera_dynamicAlbedo, component);
		CopyMainCameraSettingsToGBufferCamera(m_camera_dynamicDepth, component);
		CopyMainCameraSettingsToGBufferCamera(m_camera_dynamicNormal, component);
		m_camera_dynamicAlbedo.RenderWithShader(m_dynamicAlbedoShader, string.Empty);
		m_camera_dynamicNormal.RenderWithShader(m_dynamicNormalShader, string.Empty);
		m_camera_dynamicDepth.RenderWithShader(m_dynamicDepthShader, string.Empty);
		m_bufferCopyMaterial.SetTexture("_MainTex", m_screenTexture_background);
		Graphics.Blit(m_screenTexture_background, destination, m_bufferCopyMaterial, 0);
	}

	private void Awake()
	{
		Instance = this;
		if (!GetComponent<NotifyPostRender>())
		{
			base.gameObject.AddComponent<NotifyPostRender>();
		}
		m_visibleStreamTileObjectList = new ArrayList();
		m_visibleDynamicMeshList = new ArrayList();
		m_defaultNormalTexture = CreateDefaultNormalTexture();
		m_defaultWhiteTexture = CreateDefaultWhiteTexture();
		m_defaultBlackTexture = CreateDefaultBlackTexture();
		m_defaultGrayTexture = CreateDefaultGrayTexture();
		m_identityColorGradeLut = CreateIdentityColorGradeLut();
	}

	private void DestroyObj(UnityEngine.Object t)
	{
		GameUtilities.Destroy(t);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if (m_screenTexture_depth != null)
		{
			m_screenTexture_depth.Release();
		}
		if (m_screenTexture_depth_backgroundOnly != null)
		{
			m_screenTexture_depth_backgroundOnly.Release();
		}
		if (m_screenTexture_normals != null)
		{
			m_screenTexture_normals.Release();
		}
		if (m_screenTexture_albedoSpec != null)
		{
			m_screenTexture_albedoSpec.Release();
		}
		if (m_screenTexture_dynamicShadow != null)
		{
			m_screenTexture_dynamicShadow.Release();
		}
		if (m_screenTexture_background != null)
		{
			m_screenTexture_background.Release();
		}
		if (m_depthMaterial != null)
		{
			DestroyObj(m_depthMaterial);
		}
		if (m_shadowDepthMaterial != null)
		{
			DestroyObj(m_shadowDepthMaterial);
		}
		if (m_occlusionMaterial != null)
		{
			DestroyObj(m_occlusionMaterial);
		}
		if (m_bufferCopyMaterial != null)
		{
			DestroyObj(m_bufferCopyMaterial);
		}
		if (m_defaultNormalTexture != null)
		{
			DestroyObj(m_defaultNormalTexture);
		}
		if (m_defaultWhiteTexture != null)
		{
			DestroyObj(m_defaultWhiteTexture);
		}
		if (m_defaultBlackTexture != null)
		{
			DestroyObj(m_defaultBlackTexture);
		}
		if (m_defaultGrayTexture != null)
		{
			DestroyObj(m_defaultGrayTexture);
		}
		if (m_identityColorGradeLut != null)
		{
			DestroyObj(m_identityColorGradeLut);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		SetupCameras();
		RenderSettings.ambientLight = Color.black;
		CreateRenderTargets(Screen.width, Screen.height);
		UpdateShaderFauxViewVectors();
		m_dynamicDepthShader = Shader.Find("Trenton/PE_DynamicMeshDepth");
		m_dynamicAlbedoShader = Shader.Find("Trenton/PE_DynamicMeshAlbedoSpec");
		m_dynamicNormalShader = Shader.Find("Trenton/PE_DynamicMeshNormals");
		m_depthMaterial = new Material(m_dynamicDepthShader);
		m_shadowDepthMaterial = new Material(Shader.Find("Trenton/PE_DynamicShadowDepth"));
		m_occlusionMaterial = new Material(Shader.Find("Trenton/PE_OcclusionEffect"));
		m_bufferCopyMaterial = new Material(Shader.Find("Trenton/PE_UnlitZwriteOff"));
		m_cameraControl = CameraControl.Instance;
		if (m_blockoutMode)
		{
			m_streamTileManager = null;
			m_dynamicShadowBuffer = null;
		}
		else
		{
			m_streamTileManager = PE_StreamTileManager.Instance;
			PE_DynamicShadowBuffer[] array = UnityEngine.Object.FindObjectsOfType(typeof(PE_DynamicShadowBuffer)) as PE_DynamicShadowBuffer[];
			if (array.Length != 1)
			{
				Debug.LogError("PE_GameRender::Start() - Should be exactly one PE_DynamicShadowBuffer in scene");
				m_dynamicShadowBuffer = null;
			}
			else
			{
				m_dynamicShadowBuffer = array[0];
			}
			PE_DeferredLightPass[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(PE_DeferredLightPass)) as PE_DeferredLightPass[];
			if (array2.Length != 1)
			{
				Debug.LogError("PE_GameRender::Start() - Should be exactly one PE_DeferredLightPass in scene");
				m_deferredLightPass = null;
			}
			else
			{
				m_deferredLightPass = array2[0];
			}
			SyncCameraOrthoSettings[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(SyncCameraOrthoSettings)) as SyncCameraOrthoSettings[];
			if (array3.Length != 1)
			{
				Debug.LogError("PE_GameRender::Start() - Should be exactly one SyncCameraOrthoSettings in scene");
				m_syncCameraOrthoSettings = null;
			}
			else
			{
				m_syncCameraOrthoSettings = array3[0];
			}
			if (null != m_streamTileManager)
			{
				m_camera_dynamicAlbedo = CreateCameraForDynamicGBuffer("Camera_DynamicAlbedo", ScreenTextureType.AlbedoSpec);
				m_camera_dynamicDepth = CreateCameraForDynamicGBuffer("Camera_DynamicDepth", ScreenTextureType.Depth);
				m_camera_dynamicNormal = CreateCameraForDynamicGBuffer("Camera_DynamicNormals", ScreenTextureType.Normals);
			}
		}
		m_camera_dynamicShadowBuffer = null;
		ScreenTextureScript_ScreenShadowPass[] array4 = UnityEngine.Object.FindObjectsOfType(typeof(ScreenTextureScript_ScreenShadowPass)) as ScreenTextureScript_ScreenShadowPass[];
		if (array4.Length > 1)
		{
			Debug.LogError("PE_GameRender::Start() - Should be no more than one ScreenTextureScript_ScreenShadowPass in scene");
			m_camera_dynamicShadowBuffer = null;
		}
		else if (array4.Length == 1)
		{
			m_camera_dynamicShadowBuffer = array4[0].gameObject.GetComponent<Camera>();
		}
		m_dayNightRender = null;
		PE_DayNightRender[] array5 = UnityEngine.Object.FindObjectsOfType(typeof(PE_DayNightRender)) as PE_DayNightRender[];
		if (array5.Length > 1)
		{
			Debug.LogError("PE_GameRender::Start() - Should be no more than one PE_DayNightRender in scene");
			m_dayNightRender = null;
		}
		else if (array5.Length == 1)
		{
			m_dayNightRender = array5[0];
		}
		PE_LightEnvironment[] array6 = UnityEngine.Object.FindObjectsOfType(typeof(PE_LightEnvironment)) as PE_LightEnvironment[];
		if (array6.Length != 1)
		{
			Debug.LogWarning("PE_GameRender::Start() - No light environments found in scene");
			m_lightEnvironment = null;
		}
		else
		{
			m_lightEnvironment = array6[0];
		}
		if (m_ambientMap == null)
		{
			m_ambientMap = m_defaultGrayTexture;
		}
		if (m_shadowMap == null)
		{
			m_shadowMap = m_defaultWhiteTexture;
		}
		UpdateCameraForDynamicGBuffer(m_camera_dynamicShadowBuffer, ScreenTextureType.DynamicShadow);
	}

	private void Update()
	{
		DoUpdate(updateCameraControl: true);
	}

	public void DoUpdate(bool updateCameraControl)
	{
		float deltaTime = Time.deltaTime;
		LevelInfo levelInfo = FindLevelInfo();
		int width = Screen.width;
		int height = Screen.height;
		if (m_ccNeedsReset)
		{
			m_cameraControl.ResetAtEdges();
			m_ccNeedsReset = false;
		}
		if (width != m_screen_width || height != m_screen_height)
		{
			CreateRenderTargets(width, height);
			UpdateShaderFauxViewVectors();
			UpdateCameraForDynamicGBuffer(m_camera_dynamicAlbedo, ScreenTextureType.AlbedoSpec);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicDepth, ScreenTextureType.Depth);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicNormal, ScreenTextureType.Normals);
			UpdateCameraForDynamicGBuffer(m_camera_dynamicShadowBuffer, ScreenTextureType.DynamicShadow);
			m_ccNeedsReset = true;
		}
		if (updateCameraControl)
		{
			m_cameraControl.DoUpdate();
		}
		if (null == m_directionalLight)
		{
			m_directionalLight = FindSceneDirectionalLight();
		}
		Vector4 value;
		Vector4 value2;
		if (null != m_directionalLight)
		{
			Color color = m_directionalLight.color;
			value = new Vector4(color.r, color.g, color.b, 1f) * m_directionalLight.intensity;
			Vector3 vector = -m_directionalLight.transform.forward;
			value2 = new Vector4(vector.x, vector.y, vector.z, 0f);
		}
		else
		{
			value = Color.white;
			value2 = new Vector4(0f, -1f, 0f, 0f);
		}
		Shader.SetGlobalVector("Trenton_Light0_Color", value);
		Shader.SetGlobalVector("Trenton_Light0_Direction", value2);
		if ((bool)m_streamTileManager)
		{
			m_streamTileManager.DoUpdate();
		}
		if ((bool)m_dynamicShadowBuffer)
		{
			m_dynamicShadowBuffer.DoUpdate();
		}
		if (Application.isPlaying && (bool)m_dayNightRender)
		{
			m_dayNightRender.DoUpdate(deltaTime);
		}
		else
		{
			PE_DayNightRender.ApplyIdentityShaderSettings(m_identityColorGradeLut);
		}
		if ((bool)m_deferredLightPass)
		{
			m_deferredLightPass.DoUpdate(deltaTime, levelInfo);
		}
		ShaderSetEnvironment(m_lightEnvironment);
		if (Application.isPlaying)
		{
			UpdateAmbientMapValues();
			Shader.SetGlobalTexture("Trenton_ShadowMap", m_shadowMap);
			Shader.SetGlobalTexture("Trenton_AmbientMap", m_ambientMap);
		}
		if (null != Camera.main)
		{
			Shader.SetGlobalMatrix("Trenton_CameraToWorld", Camera.main.GetComponent<Camera>().worldToCameraMatrix.inverse);
		}
	}

	public void UpdateAmbientMapValues()
	{
		Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
		Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane)) - vector;
		Vector3 vector3 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelHeight, Camera.main.nearClipPlane)) - vector;
		float magnitude = m_cameraControl.CamNearWorldBoundsX.magnitude;
		float magnitude2 = m_cameraControl.CamNearWorldBoundsY.magnitude;
		Vector3 camNearWorldBoundsX = m_cameraControl.CamNearWorldBoundsX;
		Vector3 camNearWorldBoundsY = m_cameraControl.CamNearWorldBoundsY;
		camNearWorldBoundsX.Normalize();
		camNearWorldBoundsY.Normalize();
		float x = Vector3.Dot(camNearWorldBoundsX, vector - m_cameraControl.CamNearWorldBoundsOrigin) / magnitude;
		float y = Vector3.Dot(camNearWorldBoundsY, vector - m_cameraControl.CamNearWorldBoundsOrigin + vector3) / magnitude2;
		float z = vector2.magnitude / magnitude;
		float num = vector3.magnitude / magnitude2;
		Shader.SetGlobalVector("Trenton_AmbientShadowMapOffsets", new Vector4(x, y, z, 0f - num));
	}

	public void ShaderSetEnvironment(PE_LightEnvironment lightEnvironment)
	{
		if (lightEnvironment == null)
		{
			lightEnvironment = m_lightEnvironment;
		}
		if ((bool)lightEnvironment)
		{
			Shader.SetGlobalTexture("Trenton_EnvironmentMap", lightEnvironment.environmentMap);
			Shader.SetGlobalTexture("Trenton_AnisoEnvironmentMap", lightEnvironment.anisotropicEnvironmentMap);
			Shader.SetGlobalColor("Trenton_Ambient_High", lightEnvironment.ambientUpper);
			Shader.SetGlobalColor("Trenton_Ambient_Low", lightEnvironment.ambientLower);
		}
		else
		{
			Shader.SetGlobalColor("Trenton_Ambient_High", Color.black);
			Shader.SetGlobalColor("Trenton_Ambient_Low", Color.black);
		}
	}

	private void CreateRenderTargets(int width, int height)
	{
		m_screen_width = width;
		m_screen_height = height;
		CreateRenderTarget(ref m_screenTexture_depth, width, height, 24, "_ScreenDepthTex", FilterMode.Bilinear);
		CreateRenderTarget(ref m_screenTexture_depth_backgroundOnly, width, height, 0, "_ScreenDepthBackgroundOnlyTex", FilterMode.Bilinear);
		CreateRenderTarget(ref m_screenTexture_normals, width, height, 24, "_ScreenNormalsTex", FilterMode.Bilinear);
		CreateRenderTarget(ref m_screenTexture_albedoSpec, width, height, 24, "_ScreenAlbedoSpecTex", FilterMode.Bilinear);
		CreateRenderTarget(ref m_screenTexture_dynamicShadow, width, height, 0, "_ScreenShadow", FilterMode.Bilinear);
		CreateRenderTarget(ref m_screenTexture_background, width, height, 24, "_ScreenBackgroundTex", FilterMode.Bilinear);
		m_mrtBuffers = new RenderBuffer[4];
		m_mrtBuffers[0] = m_screenTexture_background.colorBuffer;
		m_mrtBuffers[1] = m_screenTexture_depth_backgroundOnly.colorBuffer;
		m_mrtBuffers[2] = m_screenTexture_normals.colorBuffer;
		m_mrtBuffers[3] = m_screenTexture_albedoSpec.colorBuffer;
	}

	private void CreateRenderTarget(ref RenderTexture renderTexture, int width, int height, int depthBits, string name, FilterMode filterMode)
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
		}
		renderTexture = new RenderTexture(width, height, depthBits, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		renderTexture.filterMode = filterMode;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.useMipMap = false;
		renderTexture.name = name;
		renderTexture.SetGlobalShaderProperty(name);
	}

	private GameObject FindChildWithName(GameObject parentGO, string childName)
	{
		foreach (Transform item in parentGO.transform)
		{
			if (childName == item.name)
			{
				return item.gameObject;
			}
		}
		return null;
	}

	private Camera CreateCameraForDynamicGBuffer(string gameObjectName, ScreenTextureType screenTextureType)
	{
		GameObject gameObject = FindChildWithName(base.gameObject, gameObjectName);
		while (gameObject != null)
		{
			GameUtilities.DestroyImmediate(gameObject);
			gameObject = FindChildWithName(base.gameObject, gameObjectName);
		}
		GameObject gameObject2 = new GameObject(gameObjectName);
		gameObject2.transform.parent = base.gameObject.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.targetTexture = GetScreenTextureType(screenTextureType);
		camera.cullingMask = (1 << LayerMask.NameToLayer("Dynamics")) | (1 << LayerMask.NameToLayer("Dynamics No Shadow")) | (1 << LayerMask.NameToLayer("Doors")) | (1 << LayerMask.NameToLayer("Dynamics No Occlusion")) | (1 << LayerMask.NameToLayer("Dynamics No Shadow No Occlusion"));
		camera.clearFlags = CameraClearFlags.Depth;
		camera.backgroundColor = Color.black;
		camera.orthographic = true;
		gameObject2.AddComponent<ScreenTextureScript>().textureType = screenTextureType;
		gameObject2.SetActive(value: false);
		return camera;
	}

	private void UpdateCameraForDynamicGBuffer(Camera cam, ScreenTextureType screenTextureType)
	{
		cam.targetTexture = GetScreenTextureType(screenTextureType);
	}

	private void CopyMainCameraSettingsToGBufferCamera(Camera destCamera, Camera mainCamera)
	{
		destCamera.orthographic = mainCamera.orthographic;
		destCamera.orthographicSize = mainCamera.orthographicSize;
	}

	private void UpdateShaderFauxViewVectors()
	{
		Camera component = base.gameObject.GetComponent<Camera>();
		component.orthographic = false;
		component.fieldOfView = 60f;
		m_fauxViewPos000 = component.ViewportToWorldPoint(new Vector3(0f, 0f, component.nearClipPlane));
		m_fauxViewPos001 = component.ViewportToWorldPoint(new Vector3(0f, 0f, component.farClipPlane));
		m_fauxViewPos100 = component.ViewportToWorldPoint(new Vector3(1f, 0f, component.nearClipPlane));
		m_fauxViewPos101 = component.ViewportToWorldPoint(new Vector3(1f, 0f, component.farClipPlane));
		m_fauxViewPos010 = component.ViewportToWorldPoint(new Vector3(0f, 1f, component.nearClipPlane));
		m_fauxViewPos011 = component.ViewportToWorldPoint(new Vector3(0f, 1f, component.farClipPlane));
		m_fauxViewPos110 = component.ViewportToWorldPoint(new Vector3(1f, 1f, component.nearClipPlane));
		m_fauxViewPos111 = component.ViewportToWorldPoint(new Vector3(1f, 1f, component.farClipPlane));
		Vector3 vector = m_fauxViewPos001 - m_fauxViewPos000;
		Vector3 vector2 = m_fauxViewPos101 - m_fauxViewPos100;
		Vector3 vector3 = m_fauxViewPos011 - m_fauxViewPos010;
		Vector3 vector4 = m_fauxViewPos111 - m_fauxViewPos110;
		Shader.SetGlobalVector("Trenton_FauxViewVector00", vector.normalized);
		Shader.SetGlobalVector("Trenton_FauxViewVector10", vector2.normalized);
		Shader.SetGlobalVector("Trenton_FauxViewVector01", vector3.normalized);
		Shader.SetGlobalVector("Trenton_FauxViewVector11", vector4.normalized);
		component.orthographic = true;
	}

	private LevelInfo FindLevelInfo()
	{
		if (m_levelInfo == null)
		{
			if (!Application.isPlaying)
			{
				LevelInfo[] array = UnityEngine.Object.FindObjectsOfType(typeof(LevelInfo)) as LevelInfo[];
				if (array == null && array.Length != 1)
				{
					Debug.LogError("PE_GameRender::Start() - Should be exactly 1 LevelInfo object per scene.");
					return null;
				}
				m_levelInfo = array[0];
			}
			else
			{
				m_levelInfo = LevelInfo.Instance;
			}
		}
		return m_levelInfo;
	}

	public static Light FindSceneDirectionalLight()
	{
		PE_DirectionalLight[] array = (PE_DirectionalLight[])UnityEngine.Object.FindObjectsOfType(typeof(PE_DirectionalLight));
		foreach (PE_DirectionalLight pE_DirectionalLight in array)
		{
			if (pE_DirectionalLight.LightType == PE_DirectionalLight.DirectionLightType.Main)
			{
				return pE_DirectionalLight.GetComponent<Light>();
			}
		}
		Light[] array2 = (Light[])UnityEngine.Object.FindObjectsOfType(typeof(Light));
		foreach (Light light in array2)
		{
			if (light.type == LightType.Directional)
			{
				light.gameObject.AddComponent<PE_DirectionalLight>().LightType = PE_DirectionalLight.DirectionLightType.Main;
				return light;
			}
		}
		Debug.LogWarning("PE_GameRender::FindSceneDirectionalLight - Could not find a directional light, creating a default one.");
		GameObject obj = new GameObject("DirectionLight");
		Light light2 = obj.AddComponent<Light>();
		PE_DirectionalLight pE_DirectionalLight2 = obj.AddComponent<PE_DirectionalLight>();
		light2.type = LightType.Directional;
		pE_DirectionalLight2.LightType = PE_DirectionalLight.DirectionLightType.Main;
		return light2;
	}

	private Texture2D CreateDefaultNormalTexture()
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		texture2D.SetPixel(0, 0, color);
		texture2D.SetPixel(1, 0, color);
		texture2D.SetPixel(0, 1, color);
		texture2D.SetPixel(1, 1, color);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D CreateDefaultWhiteTexture()
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		texture2D.SetPixel(0, 0, Color.white);
		texture2D.SetPixel(1, 0, Color.white);
		texture2D.SetPixel(0, 1, Color.white);
		texture2D.SetPixel(1, 1, Color.white);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D CreateDefaultBlackTexture()
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		texture2D.SetPixel(0, 0, Color.black);
		texture2D.SetPixel(1, 0, Color.black);
		texture2D.SetPixel(0, 1, Color.black);
		texture2D.SetPixel(1, 1, Color.black);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D CreateDefaultGrayTexture()
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		texture2D.SetPixel(0, 0, Color.gray);
		texture2D.SetPixel(1, 0, Color.gray);
		texture2D.SetPixel(0, 1, Color.gray);
		texture2D.SetPixel(1, 1, Color.gray);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture3D CreateIdentityColorGradeLut()
	{
		int num = 16;
		Color[] array = new Color[num * num * num];
		float num2 = 1f / (1f * (float)num - 1f);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					array[i + j * num + k * num * num] = new Color((float)i * 1f * num2, (float)j * 1f * num2, (float)k * 1f * num2, 1f);
				}
			}
		}
		Texture3D texture3D = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
		texture3D.SetPixels(array);
		texture3D.Apply();
		return texture3D;
	}

	private void OnDrawGizmos()
	{
		DrawBackgroundQuadGizmo();
	}

	private void DrawBackgroundQuadGizmo()
	{
		LevelInfo levelInfo = FindLevelInfo();
		Gizmos.color = Color.blue;
		Vector3 backgroundQuadOrigin = levelInfo.m_backgroundQuadOrigin;
		Vector3 vector = backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisX * levelInfo.m_backgroundQuadWidth;
		Vector3 vector2 = vector + levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
		Vector3 vector3 = backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
		Gizmos.DrawLine(backgroundQuadOrigin, vector);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, backgroundQuadOrigin);
	}

	private void SetupCameras()
	{
		GameObject gameObject = base.gameObject;
		Camera camera;
		if (gameObject.transform.Find("InGameUIPass") == null)
		{
			GameObject obj = new GameObject("InGameUIPass");
			obj.transform.parent = gameObject.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			camera = obj.AddComponent<Camera>();
		}
		else
		{
			camera = gameObject.transform.Find("InGameUIPass").GetComponent<Camera>();
		}
		camera.cullingMask = 1 << LayerMask.NameToLayer("InGameUI");
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.orthographic = true;
		camera.depth = 83f;
		camera.farClipPlane = 500f;
		if (Application.isPlaying)
		{
			camera.gameObject.AddComponent<AntiAliasing>().enabled = GameState.Option.Quality > 0;
		}
		camera.gameObject.SetActive(value: true);
		if (m_blockoutMode)
		{
			return;
		}
		if (gameObject.transform.Find("DynamicOcclusionPass") == null)
		{
			GameObject obj2 = new GameObject("DynamicOcclusionPass");
			obj2.transform.parent = gameObject.transform;
			obj2.transform.localPosition = Vector3.zero;
			obj2.transform.localRotation = Quaternion.identity;
			obj2.transform.localScale = Vector3.one;
			Camera camera2 = obj2.AddComponent<Camera>();
			camera2.cullingMask = (1 << LayerMask.NameToLayer("Dynamics")) | (1 << LayerMask.NameToLayer("Dynamics No Shadow"));
			camera2.clearFlags = CameraClearFlags.Depth;
			camera2.orthographic = true;
			camera2.depth = 29f;
			obj2.AddComponent<ScreenTextureScript_Occlusion>();
		}
		if (gameObject.transform.Find("FogOfWarCamera") == null)
		{
			GameObject obj3 = new GameObject("FogOfWarCamera");
			obj3.transform.parent = gameObject.transform;
			obj3.transform.localPosition = Vector3.zero;
			obj3.transform.localRotation = Quaternion.identity;
			obj3.transform.localScale = Vector3.one;
			Camera camera3 = obj3.AddComponent<Camera>();
			camera3.cullingMask = 0;
			camera3.clearFlags = CameraClearFlags.Nothing;
			camera3.orthographic = true;
			camera3.depth = 91f;
			obj3.AddComponent<FogOfWarRender>();
		}
		Transform transform = gameObject.transform.Find("DynamicPass");
		if (transform != null)
		{
			Camera component = transform.gameObject.GetComponent<Camera>();
			if ((bool)component)
			{
				component.cullingMask = (1 << LayerMask.NameToLayer("Dynamics")) | (1 << LayerMask.NameToLayer("Dynamics No Shadow")) | (1 << LayerMask.NameToLayer("Doors")) | (1 << LayerMask.NameToLayer("Dynamics No Occlusion")) | (1 << LayerMask.NameToLayer("Dynamics No Shadow No Occlusion"));
			}
		}
		SyncCameraOrthoSettings syncCameraOrthoSettings = SyncCameraOrthoSettings.Instance;
		if (syncCameraOrthoSettings == null)
		{
			syncCameraOrthoSettings = UnityEngine.Object.FindObjectOfType<SyncCameraOrthoSettings>();
		}
		syncCameraOrthoSettings.RefreshCameras();
	}

	public static void SetAntiAliasing(bool value)
	{
		AntiAliasing antiAliasing = UnityEngine.Object.FindObjectOfType<AntiAliasing>();
		if (!(antiAliasing == null) && antiAliasing.enabled != value)
		{
			antiAliasing.enabled = value;
		}
	}
}
