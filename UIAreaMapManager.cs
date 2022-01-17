using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAreaMapManager : UIHudWindow
{
	public UITexture m_mapTexture;

	public UIPanel m_mapAreaPanel;

	public UISprite m_mapBevel;

	public GameObject PointOfInterestPrefab;

	public UIPointOfInterestVisualData PotentialCompanionPointOfInterestVisualData;

	private List<GameObject> m_pointsOfInterest;

	private PE_StreamTileManager m_streamTileManager;

	private LevelInfo m_levelInfo;

	private FogOfWar m_fogOfWar;

	public UITexture m_fogTextureA;

	public UISprite m_CameraBoundsSprite;

	private Vector3 m_worldOrigin;

	private Vector3 m_worldWidth;

	private Vector3 m_worldHeight;

	private Vector3 m_mapBoundsX;

	private Vector3 m_mapBoundsY;

	private Vector3 m_mapBoundsOrigin;

	private float m_levelWidth;

	private float m_levelHeight;

	private float m_areaMapWidth;

	private float m_areaMapHeight;

	private Vector3 m_PreviousCameraPosition;

	private static float s_AreaMapMaxWidth = 1084f;

	private static float s_AreaMapMaxHeight = 610f;

	private RenderTexture m_RenderTexture;

	private Camera m_areaMapCamera;

	private RenderTexture m_emptyFogTexture;

	private RenderTexture m_fogRenderTexture;

	private Material m_fogMaterial;

	private bool m_realtimeAreaMapImage;

	private int m_CachedShaderProp_ReduceFogIntensity;

	private int m_CachedMatProp_FogTexA;

	private int m_CachedMatProp_FogTexOffset;

	private int m_CachedMatProp_FogColor;

	private static float overrideZoom = 0f;

	public static UIAreaMapManager Instance { get; private set; }

	public float AreaMapWidthFillPercentage => m_areaMapWidth / s_AreaMapMaxWidth;

	public float AreaMapHeightFillPercentage => m_areaMapHeight / s_AreaMapMaxHeight;

	public override int CyclePosition => 3;

	private void Awake()
	{
		Instance = this;
		m_CachedMatProp_FogTexA = Shader.PropertyToID("_FogTexA");
		m_CachedMatProp_FogTexOffset = Shader.PropertyToID("_FogTexOffset");
		m_CachedMatProp_FogColor = Shader.PropertyToID("_FogColor");
		m_CachedShaderProp_ReduceFogIntensity = Shader.PropertyToID("_ReduceFogOfWarIntensity");
	}

	public void SetRenderSize()
	{
		Rect rect = new Rect(0f, 0f, s_AreaMapMaxWidth, s_AreaMapMaxHeight);
		if (m_RenderTexture == null || !m_RenderTexture.IsCreated() || rect.width != (float)m_RenderTexture.width || rect.height != (float)m_RenderTexture.height)
		{
			if ((bool)m_RenderTexture)
			{
				m_RenderTexture.Release();
			}
			m_RenderTexture = new RenderTexture((int)rect.width, (int)rect.height, 32);
			if ((bool)m_fogOfWar)
			{
				if ((bool)m_emptyFogTexture)
				{
					m_emptyFogTexture.Release();
				}
				if ((bool)m_fogRenderTexture)
				{
					m_fogRenderTexture.Release();
				}
				m_emptyFogTexture = new RenderTexture(m_fogOfWar.FogTextureA.width, m_fogOfWar.FogTextureA.height, 32);
				m_fogRenderTexture = new RenderTexture(m_fogOfWar.FogTextureA.width, m_fogOfWar.FogTextureA.height, 32);
				Shader shader = Shader.Find("Trenton/PE_FogOfWar");
				m_fogMaterial = new Material(shader);
			}
		}
		m_areaMapCamera = Camera.main;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if (m_pointsOfInterest != null)
		{
			UnsubscribePointsOfInterest();
			m_pointsOfInterest.Clear();
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void SetCameraBounds()
	{
		if ((bool)CameraControl.Instance)
		{
			Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
			Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane));
			Vector3 vector3 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane)) - vector;
			Vector3 vector4 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelHeight, Camera.main.nearClipPlane)) - vector;
			float magnitude = vector3.magnitude;
			float magnitude2 = vector4.magnitude;
			WorldToMap(new Vector3((vector2.x + vector.x) / 2f, (vector2.y + vector.y) / 2f, (vector2.z + vector.z) / 2f), out var x, out var y);
			m_CameraBoundsSprite.transform.localPosition = new Vector3((x - m_levelWidth / 2f) * m_areaMapWidth / m_levelWidth, (y - m_levelHeight / 2f) * m_areaMapHeight / m_levelHeight, -3f);
			m_CameraBoundsSprite.transform.localScale = new Vector3(magnitude * m_areaMapWidth / m_levelWidth, magnitude2 * m_areaMapHeight / m_levelHeight, 1f);
		}
		m_PreviousCameraPosition = Camera.main.transform.localPosition;
	}

	private void Update()
	{
		if (!base.IsVisible)
		{
			return;
		}
		if (!m_RenderTexture.IsCreated() || !m_emptyFogTexture.IsCreated() || !m_fogRenderTexture.IsCreated())
		{
			SetRenderSize();
			if (m_mapTexture != null)
			{
				m_mapTexture.mainTexture = m_RenderTexture;
				m_mapTexture.MakePixelPerfect();
				m_mapTexture.transform.localScale = new Vector3(s_AreaMapMaxWidth, s_AreaMapMaxHeight, 1f);
			}
			UpdateAreaMapRender();
		}
		if (m_PreviousCameraPosition != Camera.main.transform.localPosition)
		{
			SetCameraBounds();
		}
		UpdateFog();
		UpdatePointsOfInterest();
	}

	private void UpdateFog()
	{
		if (m_fogOfWar == null || m_realtimeAreaMapImage)
		{
			return;
		}
		Shader.SetGlobalInt(m_CachedShaderProp_ReduceFogIntensity, 1);
		if (m_levelInfo != null && m_streamTileManager != null)
		{
			m_fogOfWar.RefreshFogTexture();
			m_fogMaterial.SetTexture(m_CachedMatProp_FogTexA, m_fogOfWar.FogTextureA);
			m_fogMaterial.SetVector(m_CachedMatProp_FogTexOffset, new Vector4(0f, 0f, 1f, 1f));
			m_fogMaterial.SetVector(m_CachedMatProp_FogColor, m_fogOfWar.FogColor);
			if (m_fogTextureA.material == null)
			{
				Shader shader = Shader.Find("Unlit/Transparent Colored");
				m_fogTextureA.material = new Material(shader);
				m_fogTextureA.color = Color.white;
			}
			RenderTexture active = RenderTexture.active;
			Graphics.SetRenderTarget(m_emptyFogTexture);
			GL.Clear(clearDepth: true, clearColor: true, Color.clear);
			Graphics.SetRenderTarget(active);
			Graphics.Blit(m_emptyFogTexture, m_fogRenderTexture, m_fogMaterial);
			m_fogTextureA.mainTexture = m_fogRenderTexture;
			m_fogTextureA.transform.localScale = new Vector3(Mathf.Ceil(m_areaMapWidth), Mathf.Ceil(m_areaMapHeight), m_fogTextureA.transform.localScale.z);
		}
		Shader.SetGlobalInt(m_CachedShaderProp_ReduceFogIntensity, 0);
	}

	private void UpdatePointsOfInterest()
	{
		if (m_fogOfWar == null || m_pointsOfInterest == null || m_levelInfo == null)
		{
			return;
		}
		for (int i = 0; i < m_pointsOfInterest.Count; i++)
		{
			GameObject gameObject = m_pointsOfInterest[i];
			UIPointOfInterestNode component = gameObject.GetComponent<UIPointOfInterestNode>();
			if (m_fogOfWar.PointRevealed(component.PointOfInterestReference.transform.position))
			{
				gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
			if (gameObject.activeInHierarchy)
			{
				WorldToMap(component.PointOfInterestReference.transform.position, out var x, out var y);
				Vector3 localPosition = component.transform.localPosition;
				localPosition.x = (x - m_levelWidth / 2f) * m_areaMapWidth / m_levelWidth;
				localPosition.y = (y - m_levelHeight / 2f) * m_areaMapHeight / m_levelHeight;
				localPosition.z = -4f;
				gameObject.transform.localPosition = localPosition;
			}
		}
	}

	protected override bool Hide(bool forced)
	{
		if (m_realtimeAreaMapImage)
		{
			UICamera uICamera = UICamera.FindCameraForLayer(base.gameObject.layer);
			if ((bool)uICamera)
			{
				uICamera.OnPreRenderCallbacks -= UpdateAreaMapRender;
			}
		}
		return base.Hide(forced);
	}

	public static Vector3 GetCameraPlaneRayIntersectionPosition(Plane cameraPlane, Ray theRay)
	{
		if (!cameraPlane.Raycast(theRay, out var enter))
		{
			Debug.LogError("CameraControl::GetCameraPlaneRayIntersectionPosition() - failed to find intersection with camera plane");
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	public void WorldToMap(Vector3 position, out float x, out float y)
	{
		Vector3 lhs = position - m_worldOrigin;
		Vector3 mapBoundsX = m_mapBoundsX;
		Vector3 mapBoundsY = m_mapBoundsY;
		mapBoundsX.Normalize();
		mapBoundsY.Normalize();
		float num = Vector3.Dot(lhs, mapBoundsX);
		float num2 = Vector3.Dot(lhs, mapBoundsY);
		x = num;
		y = num2;
	}

	private void UpdateAreaMapRender(object sender, EventArgs args)
	{
		UpdateAreaMapRender();
	}

	public void UpdateAreaMapRender()
	{
		if (FogOfWarRender.Instance == null)
		{
			return;
		}
		List<Camera> list = new List<Camera>();
		list.AddRange(m_areaMapCamera.GetComponentsInChildren<Camera>(includeInactive: true));
		list.Sort(PE_GameRender.CameraDepthComparer.Instance);
		foreach (Camera item in list)
		{
			if (item.targetTexture == null && item.name != "Camera_DynamicNormals" && item.name != "Camera_DynamicDepth" && item.name != "Camera_DynamicAlbedo")
			{
				item.targetTexture = m_RenderTexture;
			}
		}
		Vector3 localPosition = m_areaMapCamera.gameObject.transform.localPosition;
		m_areaMapCamera.gameObject.transform.localPosition = m_mapBoundsOrigin + m_mapBoundsX / 2f + m_mapBoundsY / 2f;
		float val = s_AreaMapMaxWidth / s_AreaMapMaxHeight;
		float val2 = m_levelWidth / m_levelHeight;
		float num = m_levelInfo.m_CameraOrthographicSize / 2f;
		num /= Math.Min(val, val2);
		if (overrideZoom != 0f)
		{
			num = overrideZoom;
		}
		List<float> list2 = new List<float>();
		foreach (Camera item2 in list)
		{
			list2.Add(item2.orthographicSize);
			item2.orthographicSize = num;
		}
		PE_StreamTileManager.Instance.ForceAllTilesVisible = true;
		PE_GameRender.Instance.DoUpdate(updateCameraControl: false);
		CullEnvironmentalVfx.EnableAll();
		AreaMapVisibility.StartRender();
		Shader.SetGlobalInt("_DisableDynamicShadows", 1);
		Shader.SetGlobalInt("_FlipYForRenderTextureUV", 1);
		Shader.SetGlobalInt("_ReduceFogOfWarIntensity", 1);
		foreach (Camera item3 in list)
		{
			if (!item3.gameObject.activeInHierarchy || !item3.enabled || (!m_realtimeAreaMapImage && FogOfWarRender.Instance.GetComponent<Camera>() == item3))
			{
				continue;
			}
			if (!m_realtimeAreaMapImage && item3.name == "DynamicPass")
			{
				item3.cullingMask &= ~(1 << LayerUtility.FindLayerValue("Dynamics"));
				item3.cullingMask &= ~(1 << LayerUtility.FindLayerValue("InGameUI"));
			}
			if ((m_realtimeAreaMapImage || (!(item3.name == "DynamicShadowBuffer") && !(item3.name == "GUI") && !(item3.name == "InGameUIPass") && !(item3.name == "PostProcess") && !(item3.name == "ScreenDynamicShadowPass") && !(item3.name == "DynamicOcclusionPass"))) && !item3.name.Contains("WatcherFatigueCamera") && !item3.name.Contains("SoulMemoryCamera") && !item3.name.Contains("GammaCamera"))
			{
				item3.Render();
				if (!m_realtimeAreaMapImage && item3.name == "DynamicPass")
				{
					item3.cullingMask |= 1 << LayerUtility.FindLayerValue("Dynamics");
					item3.cullingMask |= 1 << LayerUtility.FindLayerValue("InGameUI");
				}
			}
		}
		Shader.SetGlobalInt("_DisableDynamicShadows", 0);
		Shader.SetGlobalInt("_FlipYForRenderTextureUV", 0);
		Shader.SetGlobalInt("_ReduceFogOfWarIntensity", 0);
		AreaMapVisibility.EndRender();
		for (int i = 0; i < list.Count; i++)
		{
			list[i].orthographicSize = list2[i];
		}
		PE_StreamTileManager.Instance.ForceAllTilesVisible = false;
		m_areaMapCamera.gameObject.transform.localPosition = localPosition;
		PE_GameRender.Instance.DoUpdate(updateCameraControl: false);
		foreach (Camera item4 in list)
		{
			if (item4.targetTexture == m_RenderTexture)
			{
				item4.targetTexture = null;
				item4.ResetAspect();
			}
		}
	}

	private void UnsubscribePointsOfInterest()
	{
		if (m_pointsOfInterest == null)
		{
			return;
		}
		foreach (GameObject item in m_pointsOfInterest)
		{
			if (item == null)
			{
				continue;
			}
			UIPointOfInterestNode component = item.GetComponent<UIPointOfInterestNode>();
			if (!(component == null) && !(component.PointOfInterestReference == null))
			{
				Health component2 = component.PointOfInterestReference.GetComponent<Health>();
				if (component2 != null)
				{
					component2.OnDeath -= OnPOIDeath;
				}
			}
		}
	}

	private void OnPOIDeath(GameObject objSource, GameEventArgs args)
	{
		PlacePointsOfInterest(LevelInfo.Instance);
	}

	private void PlacePointsOfInterest(LevelInfo levelInfo)
	{
		if (m_pointsOfInterest == null)
		{
			m_pointsOfInterest = new List<GameObject>();
		}
		UnsubscribePointsOfInterest();
		foreach (GameObject item in m_pointsOfInterest)
		{
			if (item != null)
			{
				NGUITools.Destroy(item);
			}
		}
		m_pointsOfInterest.Clear();
		PointOfInterest[] array = UnityEngine.Object.FindObjectsOfType<PointOfInterest>();
		foreach (PointOfInterest pointOfInterest in array)
		{
			Health component = pointOfInterest.GetComponent<Health>();
			if (component != null)
			{
				if (component.Dead)
				{
					continue;
				}
				component.OnDeath += OnPOIDeath;
			}
			UIPointOfInterestVisualData visuals = pointOfInterest.Visuals;
			if ((bool)pointOfInterest.Visuals && pointOfInterest.Visuals.Type == UIPointOfInterestVisualData.PointOfInterestType.Companion)
			{
				PartyMemberAI component2 = pointOfInterest.GetComponent<PartyMemberAI>();
				if (!component2 || !component2.enabled)
				{
					visuals = PotentialCompanionPointOfInterestVisualData;
				}
			}
			GameObject gameObject = NGUITools.AddChild(m_mapAreaPanel.gameObject, PointOfInterestPrefab);
			UIPointOfInterestNode component3 = gameObject.GetComponent<UIPointOfInterestNode>();
			component3.PointOfInterestReference = pointOfInterest;
			component3.Visuals = visuals;
			WorldToMap(pointOfInterest.transform.localPosition, out var x, out var y);
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.x = (x - m_levelWidth / 2f) * m_areaMapWidth / m_levelWidth;
			localPosition.y = (y - m_levelHeight / 2f) * m_areaMapHeight / m_levelHeight;
			localPosition.z = -4f;
			component3.SetUpIcons(pointOfInterest, localPosition);
			m_pointsOfInterest.Add(gameObject);
		}
	}

	protected override void Show()
	{
		if (FogOfWarRender.Instance == null)
		{
			UIDebug.Instance.LogOnScreenWarning("No active FogOfWarRenderer found. Please ensure the FogOfWarCamera is active. Please send to Design / Area Art to fix.", UIDebug.Department.Design, 10f);
		}
		if (m_realtimeAreaMapImage)
		{
			UICamera uICamera = UICamera.FindCameraForLayer(base.gameObject.layer);
			if ((bool)uICamera)
			{
				uICamera.OnPreRenderCallbacks += UpdateAreaMapRender;
			}
		}
		if (m_fogOfWar == null)
		{
			m_fogOfWar = FogOfWar.Instance;
		}
		SetRenderSize();
		m_levelInfo = LevelInfo.Instance;
		m_streamTileManager = PE_StreamTileManager.Instance;
		CalculateAreaMapSize();
		if (m_levelInfo != null && m_streamTileManager != null)
		{
			if (m_mapTexture != null)
			{
				if (m_mapTexture.material == null)
				{
					Shader shader = Shader.Find("Unlit/Transparent Colored");
					if (shader != null)
					{
						m_mapTexture.material = new Material(shader);
					}
					m_mapTexture.color = Color.white;
				}
				m_mapTexture.mainTexture = m_RenderTexture;
				m_mapTexture.MakePixelPerfect();
				m_mapTexture.transform.localScale = new Vector3(s_AreaMapMaxWidth, s_AreaMapMaxHeight, 1f);
				PlacePointsOfInterest(m_levelInfo);
			}
			UpdateFog();
			SetCameraBounds();
		}
		UIAreaMapSetter.Instance.SetName();
		if (!m_realtimeAreaMapImage)
		{
			UpdateAreaMapRender();
		}
	}

	private void CalculateAreaMapSize()
	{
		m_worldOrigin = m_levelInfo.m_backgroundQuadOrigin;
		m_worldWidth = m_levelInfo.m_backgroundQuadAxisX * m_levelInfo.m_backgroundQuadWidth;
		m_worldHeight = m_levelInfo.m_backgroundQuadAxisY * m_levelInfo.m_backgroundQuadHeight;
		Vector3 origin = m_worldOrigin + m_worldWidth;
		Vector3 origin2 = m_worldOrigin + m_worldHeight;
		Camera main = Camera.main;
		Plane cameraPlane = default(Plane);
		cameraPlane.SetNormalAndPosition(m_levelInfo.m_CameraRotation * Vector3.forward, m_levelInfo.m_CameraPos);
		Ray theRay = new Ray(m_worldOrigin, -main.transform.forward);
		Ray theRay2 = new Ray(origin, -main.transform.forward);
		Ray theRay3 = new Ray(origin2, -main.transform.forward);
		m_mapBoundsOrigin = GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay);
		Vector3 cameraPlaneRayIntersectionPosition = GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay2);
		Vector3 cameraPlaneRayIntersectionPosition2 = GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay3);
		m_mapBoundsX = cameraPlaneRayIntersectionPosition - m_mapBoundsOrigin;
		m_mapBoundsY = cameraPlaneRayIntersectionPosition2 - m_mapBoundsOrigin;
		m_levelWidth = m_mapBoundsX.magnitude * 1f;
		m_levelHeight = m_mapBoundsY.magnitude * 1f;
		float num = s_AreaMapMaxWidth / s_AreaMapMaxHeight;
		if (m_levelWidth / m_levelHeight > num)
		{
			m_areaMapWidth = s_AreaMapMaxWidth;
			m_areaMapHeight = m_levelHeight * s_AreaMapMaxWidth / m_levelWidth;
		}
		else
		{
			m_areaMapWidth = m_levelWidth * s_AreaMapMaxHeight / m_levelHeight;
			m_areaMapHeight = s_AreaMapMaxHeight;
		}
		if ((bool)m_mapBevel)
		{
			m_mapBevel.transform.localScale = new Vector3(m_areaMapWidth + 1f, m_areaMapHeight + 1f, 1f);
		}
	}
}
