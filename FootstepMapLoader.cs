using System;
using System.Collections.Generic;
using UnityEngine;

public class FootstepMapLoader : MonoBehaviour
{
	private Dictionary<Color, GroundMaterial> m_materialColorMap = new Dictionary<Color, GroundMaterial>();

	private Texture2D m_mapTexture;

	private GroundMaterial[,] m_materialBuffer;

	private Vector3 m_mapBoundsX;

	private Vector3 m_mapBoundsY;

	private Vector3 m_mapBoundsOrigin;

	private Vector3 m_worldOrigin;

	private Vector3 m_worldWidth;

	private Vector3 m_worldHeight;

	private Dictionary<Color, int> used = new Dictionary<Color, int>();

	public static FootstepMapLoader Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FootstepMapLoader' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		m_materialColorMap.Add(Color.red, GroundMaterial.Dirt);
		m_materialColorMap.Add(Color.green, GroundMaterial.Stone);
		m_materialColorMap.Add(new Color(1f, 1f, 0f), GroundMaterial.Wood);
		m_materialColorMap.Add(Color.blue, GroundMaterial.Water);
		m_materialColorMap.Add(new Color(0f, 1f, 1f), GroundMaterial.Rug);
		m_materialColorMap.Add(new Color(1f, 0f, 1f), GroundMaterial.Snow);
		m_materialColorMap.Add(new Color(0.5019608f, 0f, 0.5019608f), GroundMaterial.Ice);
		GameState.OnLevelLoaded += GameState_OnLevelLoaded;
	}

	private void OnDestroy()
	{
		m_mapTexture = null;
		m_materialBuffer = null;
		m_materialColorMap = null;
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelLoaded -= GameState_OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void GameState_OnLevelLoaded(object sender, EventArgs e)
	{
		LevelInfo instance = LevelInfo.Instance;
		PE_StreamTileManager instance2 = PE_StreamTileManager.Instance;
		if (!(instance != null) || !(instance2 != null))
		{
			return;
		}
		try
		{
			string text = "BKGSM_" + instance.GetLevelName();
			Texture2D texture2D = Resources.Load<Texture2D>("Art/Maps/" + text + "_fs");
			if (texture2D != null)
			{
				SetFootstepTexture(texture2D, instance);
				return;
			}
			m_mapTexture = null;
			m_materialBuffer = null;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	private void SetFootstepTexture(Texture2D texture, LevelInfo levelInfo)
	{
		m_mapTexture = texture;
		m_materialBuffer = new GroundMaterial[texture.width, texture.height];
		CreateMaterialBuffer();
		m_worldOrigin = levelInfo.m_backgroundQuadOrigin;
		m_worldWidth = levelInfo.m_backgroundQuadAxisX * levelInfo.m_backgroundQuadWidth;
		m_worldHeight = levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
		Vector3 origin = m_worldOrigin + m_worldWidth;
		Vector3 origin2 = m_worldOrigin + m_worldHeight;
		Camera main = Camera.main;
		Plane cameraPlane = default(Plane);
		cameraPlane.SetNormalAndPosition(levelInfo.m_CameraRotation * Vector3.forward, levelInfo.m_CameraPos);
		Ray theRay = new Ray(m_worldOrigin, -main.transform.forward);
		Ray theRay2 = new Ray(origin, -main.transform.forward);
		Ray theRay3 = new Ray(origin2, -main.transform.forward);
		m_mapBoundsOrigin = UIAreaMapManager.GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay);
		Vector3 cameraPlaneRayIntersectionPosition = UIAreaMapManager.GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay2);
		Vector3 cameraPlaneRayIntersectionPosition2 = UIAreaMapManager.GetCameraPlaneRayIntersectionPosition(cameraPlane, theRay3);
		m_mapBoundsX = cameraPlaneRayIntersectionPosition - m_mapBoundsOrigin;
		m_mapBoundsY = cameraPlaneRayIntersectionPosition2 - m_mapBoundsOrigin;
	}

	private void CreateMaterialBuffer()
	{
		for (int i = 0; i < m_materialBuffer.GetLength(0); i++)
		{
			for (int j = 0; j < m_materialBuffer.GetLength(1); j++)
			{
				Color pixel = m_mapTexture.GetPixel(i, j);
				if (m_materialColorMap.ContainsKey(pixel))
				{
					m_materialBuffer[i, j] = m_materialColorMap[pixel];
					if (!used.ContainsKey(pixel))
					{
						used.Add(pixel, 1);
					}
					else
					{
						used[pixel] += 1;
					}
				}
				else if (GameState.Instance.CurrentMap != null)
				{
					m_materialBuffer[i, j] = GameState.Instance.CurrentMap.DefaultSoundMaterial;
				}
			}
		}
	}

	private void WorldToMap(Vector3 position, out int x, out int y)
	{
		Vector3 lhs = position - m_worldOrigin;
		Vector3 mapBoundsX = m_mapBoundsX;
		Vector3 mapBoundsY = m_mapBoundsY;
		mapBoundsX.Normalize();
		mapBoundsY.Normalize();
		float num = Vector3.Dot(lhs, mapBoundsX);
		float num2 = Vector3.Dot(lhs, mapBoundsY);
		x = (int)(num * 1f);
		y = (int)(num2 * 1f);
	}

	public GroundMaterial GetMaterialAtPoint(Vector3 worldPos)
	{
		if (GameState.Instance.CurrentMap == null)
		{
			return GroundMaterial.Default;
		}
		if (m_materialBuffer == null)
		{
			return GameState.Instance.CurrentMap.DefaultSoundMaterial;
		}
		int x = 0;
		int y = 0;
		WorldToMap(worldPos, out x, out y);
		float num = m_mapBoundsX.magnitude * 1f;
		float num2 = m_mapBoundsY.magnitude * 1f;
		if (num == 0f || num2 == 0f)
		{
			return GroundMaterial.Default;
		}
		float num3 = m_materialBuffer.GetLength(0);
		float num4 = m_materialBuffer.GetLength(1);
		float num5 = (float)x * (num3 / num);
		float num6 = (float)y * (num4 / num2);
		if (num5 >= (float)m_materialBuffer.GetLength(0) || num6 >= (float)m_materialBuffer.GetLength(1) || num5 < 0f || num6 < 0f)
		{
			return GameState.Instance.CurrentMap.DefaultSoundMaterial;
		}
		return m_materialBuffer[(int)num5, (int)num6];
	}
}
