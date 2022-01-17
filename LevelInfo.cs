using UnityEngine;

[ExecuteInEditMode]
public class LevelInfo : MonoBehaviour
{
	public string m_LevelDataPath;

	public string m_ColorImgExt;

	public string m_HeightImgExt;

	public string m_NormalMapExt;

	public string m_AlbedoMapExt;

	public float m_HeightRange_Min;

	public float m_HeightRange_Max = 20f;

	public int m_TotalTexWidth;

	public int m_TotalTexHeight;

	public int m_TileWidth;

	public int m_TileHeight;

	public int m_NumLevelTiles_Row;

	public int m_NumLevelTiles_Col;

	public float m_CameraOrthographicSize;

	public int m_numStreamTilesX;

	public int m_numStreamTilesY;

	public Vector3 m_worldViewport00;

	public Vector3 m_worldViewport10;

	public Vector3 m_worldViewport01;

	public Vector3 m_worldViewport11;

	public Ray m_worldCameraRay00;

	public Ray m_worldCameraRay10;

	public Ray m_worldCameraRay01;

	public Ray m_worldCameraRay11;

	public Vector3 m_backgroundQuadVertex00;

	public Vector3 m_backgroundQuadVertex10;

	public Vector3 m_backgroundQuadVertex01;

	public Vector3 m_backgroundQuadVertex11;

	public Vector3 m_backgroundQuadOrigin;

	public Vector3 m_backgroundQuadAxisX;

	public Vector3 m_backgroundQuadAxisY;

	public float m_backgroundQuadWidth;

	public float m_backgroundQuadHeight;

	public Vector3 m_CameraPos;

	public Quaternion m_CameraRotation;

	public float m_LevelHeight;

	public int m_mayaExporterVersion;

	public int m_streamTileVersion;

	public static LevelInfo Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'LevelInfo' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	public string GetLevelName()
	{
		return base.gameObject.name;
	}

	public void InitializeLevelInfoCameraSettings(Camera inCamera)
	{
		m_CameraPos = inCamera.transform.position;
		m_CameraRotation = inCamera.transform.localRotation;
		float cameraOrthographicSize = m_CameraOrthographicSize;
		inCamera.orthographicSize = cameraOrthographicSize * 0.5f * ((float)m_TotalTexHeight / (float)m_TotalTexWidth);
		inCamera.aspect = (float)m_TotalTexWidth / (float)m_TotalTexHeight;
		inCamera.pixelRect = new Rect((float)m_TotalTexWidth * -0.5f, (float)m_TotalTexHeight * -0.5f, m_TotalTexWidth, m_TotalTexHeight);
		m_worldViewport00 = inCamera.ViewportToWorldPoint(Vector3.zero);
		m_worldViewport10 = inCamera.ViewportToWorldPoint(Vector3.right);
		m_worldViewport01 = inCamera.ViewportToWorldPoint(Vector3.up);
		m_worldViewport11 = inCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
		Vector3 direction = m_CameraRotation * Vector3.forward;
		m_worldCameraRay00 = new Ray(m_worldViewport00, direction);
		m_worldCameraRay10 = new Ray(m_worldViewport10, direction);
		m_worldCameraRay01 = new Ray(m_worldViewport01, direction);
		m_worldCameraRay11 = new Ray(m_worldViewport11, direction);
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		float d = 0f;
		Plane groundPlane = new Plane(inNormal, d);
		m_backgroundQuadVertex00 = GetGroundPlaneRayIntersectionPosition(groundPlane, m_worldCameraRay00);
		m_backgroundQuadVertex10 = GetGroundPlaneRayIntersectionPosition(groundPlane, m_worldCameraRay10);
		m_backgroundQuadVertex01 = GetGroundPlaneRayIntersectionPosition(groundPlane, m_worldCameraRay01);
		m_backgroundQuadVertex11 = GetGroundPlaneRayIntersectionPosition(groundPlane, m_worldCameraRay11);
		Vector3 vector = m_backgroundQuadVertex10 - m_backgroundQuadVertex00;
		m_backgroundQuadWidth = vector.magnitude;
		m_backgroundQuadAxisX = vector.normalized;
		Vector3 vector2 = m_backgroundQuadVertex01 - m_backgroundQuadVertex00;
		m_backgroundQuadHeight = vector2.magnitude;
		m_LevelHeight = vector2.x;
		m_backgroundQuadAxisY = vector2.normalized;
		m_backgroundQuadOrigin = m_backgroundQuadVertex00;
	}

	public Vector3 GetGroundPlaneRayIntersectionPosition(Plane groundPlane, Ray theRay)
	{
		if (!groundPlane.Raycast(theRay, out var enter))
		{
			Debug.LogError("LevelImport::GetGroundPlaneRayIntersectionPosition() - failed to find intersection with ground plane");
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	private void Start()
	{
		Shader.SetGlobalFloat("_HeightRange_Min", m_HeightRange_Min);
		Shader.SetGlobalFloat("_HeightRange_Max", m_HeightRange_Max);
		if ((!Application.isEditor || Application.isPlaying) && PE_GameRender.Instance != null && !PE_GameRender.Instance.m_blockoutMode)
		{
			Shader.SetGlobalFloat("_EnableTrentonOcclusion", 1f);
		}
		else
		{
			Shader.SetGlobalFloat("_EnableTrentonOcclusion", 0f);
		}
	}
}
