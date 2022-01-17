using System;
using System.Collections.Generic;
using UnityEngine;

public class SyncCameraOrthoSettings : MonoBehaviour
{
	public bool m_useHighResSettings = true;

	private float m_orthoCalc;

	private float m_orthoSize;

	private static float m_zoomLevel = 1f;

	private const float m_highResVertical = 1080f;

	private const float m_lowResVertical = 720f;

	private int m_previousScreenWidth;

	private int m_previousScreenHeight;

	public static bool m_staticWindowSize = true;

	private Camera[] m_syncCameras;

	private LevelInfo m_levelInfo;

	private int ScreenWidth
	{
		get
		{
			if (!m_staticWindowSize)
			{
				return Screen.width;
			}
			return 1920;
		}
	}

	private int ScreenHeight
	{
		get
		{
			if (!m_staticWindowSize)
			{
				return Screen.height;
			}
			return 1080;
		}
	}

	private float AspectRatio => (float)Screen.width / (float)Screen.height;

	public static SyncCameraOrthoSettings Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		LevelInfo[] array = UnityEngine.Object.FindObjectsOfType<LevelInfo>();
		if (array.Length == 0)
		{
			UIDebug.Instance.LogOnScreenWarning("Scene '' has no LevelInfo object.", UIDebug.Department.Art, 10f);
			m_orthoCalc = 12f;
			return;
		}
		if (array.Length > 1)
		{
			UIDebug.Instance.LogOnScreenWarning("Scene '' has multiple LevelInfo objects.", UIDebug.Department.Art, 10f);
			m_orthoCalc = 12f;
			return;
		}
		m_levelInfo = array[0];
		m_previousScreenWidth = ScreenWidth;
		m_previousScreenHeight = ScreenHeight;
		RefreshCameras();
		CalculateOrthoSize();
		SetZoomLevel(GetZoomLevel(), force: false);
	}

	private void Update()
	{
		if (ScreenWidth != m_previousScreenWidth || ScreenHeight != m_previousScreenHeight)
		{
			m_previousScreenWidth = ScreenWidth;
			m_previousScreenHeight = ScreenHeight;
			SetZoomLevel(GetZoomLevel(), force: true);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void RefreshCameras()
	{
		Camera[] componentsInChildren = base.gameObject.GetComponentsInChildren<Camera>(includeInactive: true);
		List<Camera> list = new List<Camera>();
		Camera[] array = componentsInChildren;
		foreach (Camera camera in array)
		{
			if (camera.name != "DynamicShadowBuffer" && camera.name != "UICamera")
			{
				list.Add(camera);
			}
		}
		m_syncCameras = list.ToArray();
	}

	public void SetZoomLevelDelta(float zoomLevelDelta)
	{
		SetZoomLevel(Mathf.Clamp(m_zoomLevel - zoomLevelDelta, GameState.Option.MinZoom, GameState.Option.MaxZoom), force: false);
	}

	public float GetZoomLevel()
	{
		return m_zoomLevel;
	}

	public void ResetZoomLevel()
	{
		SetZoomLevel(1f, force: false);
	}

	public void SetZoomLevel(float zoomLevel, bool force)
	{
		m_zoomLevel = zoomLevel;
		float a = m_zoomLevel * m_orthoCalc * ((float)ScreenHeight / (m_useHighResSettings ? 1080f : 720f));
		float aspectRatio = AspectRatio;
		float num = aspectRatio;
		if (LevelInfo.Instance.m_TotalTexHeight > 0 && aspectRatio > 0f)
		{
			float b = (float)LevelInfo.Instance.m_TotalTexWidth / (float)LevelInfo.Instance.m_TotalTexHeight;
			num = LevelInfo.Instance.m_CameraOrthographicSize / Mathf.Max(aspectRatio, b) * 0.5f;
		}
		a = Mathf.Min(a, num);
		m_zoomLevel = a / (m_orthoCalc * ((float)ScreenHeight / (m_useHighResSettings ? 1080f : 720f)));
		if (force || (a != m_orthoSize && m_syncCameras != null))
		{
			Camera[] syncCameras = m_syncCameras;
			foreach (Camera obj in syncCameras)
			{
				obj.orthographicSize = a;
				obj.orthographic = true;
			}
			m_orthoSize = Mathf.Min(num, a);
		}
	}

	private void CalculateOrthoSize()
	{
		float num = (m_useHighResSettings ? 1080f : 720f);
		float num2 = m_levelInfo.m_backgroundQuadHeight * (num / (float)m_levelInfo.m_TotalTexHeight);
		m_orthoCalc = 0.5f * Mathf.Abs(Mathf.Sin(Camera.main.transform.localEulerAngles.x * ((float)Math.PI / 180f))) * num2;
	}
}
