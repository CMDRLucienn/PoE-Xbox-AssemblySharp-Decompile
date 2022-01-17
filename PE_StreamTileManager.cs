using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class PE_StreamTileManager : MonoBehaviour
{
	private static PE_StreamTileManager s_instance = null;

	public int streamTileGuardband = 1;

	public bool showDebugDisplay;

	public bool enableStreaming;

	private static int MAX_NUM_VISIBLE_FRUSTUM_TILES = 225;

	private PE_StreamTile[] m_tilePool;

	private PE_StreamTile[] m_tilesVisibleInFrustum;

	private int m_numTilesVisibleInFrustum;

	private int m_streamTileMinX;

	private int m_streamTileMaxX;

	private int m_streamTileMinY;

	private int m_streamTileMaxY;

	private int m_cameraFrustumTileMinX;

	private int m_cameraFrustumTileMaxX;

	private int m_cameraFrustumTileMinY;

	private int m_cameraFrustumTileMaxY;

	private AssetBundle m_AssetBundle;

	private Vector3 m_frustumGroundPos00;

	private Vector3 m_frustumGroundPos10;

	private Vector3 m_frustumGroundPos01;

	private Vector3 m_frustumGroundPos11;

	private Vector3 m_frustumGroundPosCenter;

	private GameObject m_streamTileParent;

	public bool ForceAllTilesVisible;

	public static PE_StreamTileManager Instance => s_instance;

	public bool IsBundleLoaded { get; private set; }

	private void Awake()
	{
		s_instance = this;
		IsBundleLoaded = false;
	}

	public PE_StreamTile[] GetTilesInFrustum()
	{
		return m_tilesVisibleInFrustum;
	}

	public int GetNumTilesInFrustum()
	{
		return m_numTilesVisibleInFrustum;
	}

	public PE_StreamTile[] TilePool()
	{
		return m_tilePool;
	}

	public Vector3 GetFrustumGroundPlaneCenter()
	{
		return m_frustumGroundPosCenter;
	}

	public Vector3 GetFrustumGroundPos00()
	{
		return m_frustumGroundPos00;
	}

	public Vector3 GetFrustumGroundPos10()
	{
		return m_frustumGroundPos10;
	}

	public Vector3 GetFrustumGroundPos01()
	{
		return m_frustumGroundPos01;
	}

	public Vector3 GetFrustumGroundPos11()
	{
		return m_frustumGroundPos11;
	}

	public LevelInfo GetLevelInfo()
	{
		if (!Application.isPlaying)
		{
			return GetComponent<LevelInfo>();
		}
		return LevelInfo.Instance;
	}

	public GameObject GetLevelRootObject()
	{
		return base.gameObject;
	}

	public AssetBundle GetAssetBundle()
	{
		return m_AssetBundle;
	}

	public bool UseAsyncLoading()
	{
		if (Application.isEditor)
		{
			_ = Application.isPlaying;
			return false;
		}
		return false;
	}

	public bool LoadFromAssetBundle()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			return false;
		}
		return true;
	}

	public Texture2D LoadTexture(AssetBundle bundle, string levelDataPath, string texturePath, string fileName, string fileExtension)
	{
		if (LoadFromAssetBundle())
		{
			return bundle.LoadAsset(fileName, typeof(Texture2D)) as Texture2D;
		}
		Debug.LogError("PE_StreamTileManager::LoadTexture(). Can only call when isEditor for " + fileName);
		return null;
	}

	public AssetBundleRequest LoadAsyncTexture(string textureName)
	{
		AssetBundleRequest assetBundleRequest = m_AssetBundle.LoadAssetAsync(textureName, typeof(Texture2D));
		if (assetBundleRequest == null)
		{
			Debug.LogError("PE_StreamTileManager::LoadAsyncTexture() - LoadAsync() failed for " + textureName);
		}
		return assetBundleRequest;
	}

	public Vector3 GetTilePositionX(int tileIndex)
	{
		LevelInfo levelInfo = GetLevelInfo();
		if (tileIndex >= levelInfo.m_numStreamTilesX)
		{
			return levelInfo.m_backgroundQuadAxisX * levelInfo.m_backgroundQuadWidth;
		}
		float num = (float)levelInfo.m_TotalTexWidth / (float)levelInfo.m_TileWidth;
		Vector3 vector = levelInfo.m_backgroundQuadAxisX * (levelInfo.m_backgroundQuadWidth / num);
		return tileIndex * vector;
	}

	public Vector3 GetTilePositionY(int tileIndex)
	{
		LevelInfo levelInfo = GetLevelInfo();
		if (tileIndex >= levelInfo.m_numStreamTilesY)
		{
			return levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
		}
		float num = (float)levelInfo.m_TotalTexHeight / (float)levelInfo.m_TileHeight;
		Vector3 vector = levelInfo.m_backgroundQuadAxisY * (levelInfo.m_backgroundQuadHeight / num);
		return tileIndex * vector;
	}

	private void Start()
	{
		if (LoadFromAssetBundle())
		{
			StartCoroutine(LoadAssetBundle());
		}
		m_streamTileMinX = 0;
		m_streamTileMaxX = 0;
		m_streamTileMinY = 0;
		m_streamTileMaxY = 0;
		m_cameraFrustumTileMinX = 0;
		m_cameraFrustumTileMaxX = 0;
		m_cameraFrustumTileMinY = 0;
		m_cameraFrustumTileMaxY = 0;
	}

	private void OnDestroy()
	{
		if ((bool)m_AssetBundle)
		{
			m_AssetBundle.Unload(unloadAllLoadedObjects: true);
		}
		KillTiles();
		if (m_AssetBundle != null)
		{
			m_AssetBundle.Unload(unloadAllLoadedObjects: true);
			GameUtilities.Destroy(m_AssetBundle);
		}
		if (s_instance == this)
		{
			s_instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void KillTiles()
	{
		if (m_tilePool != null)
		{
			int num = m_tilePool.Length;
			for (int i = 0; i < num; i++)
			{
				PE_StreamTile pE_StreamTile = m_tilePool[i];
				m_tilePool[i] = null;
				if (!(pE_StreamTile == null))
				{
					pE_StreamTile.ReleaseTile();
				}
			}
		}
		m_tilePool = new PE_StreamTile[0];
		if ((bool)m_streamTileParent)
		{
			GameUtilities.DestroyImmediate(m_streamTileParent);
			m_streamTileParent = null;
		}
	}

	public void DoUpdate()
	{
		ComputeVisibleStreamTiles();
		UpdateStreamTilePool();
		PE_StreamTile[] tilePool = m_tilePool;
		for (int i = 0; i < tilePool.Length; i++)
		{
			tilePool[i].DoVisibleUpdate();
		}
		if (Application.isPlaying)
		{
			UpdateFrustumVisibleTileList();
		}
		if (showDebugDisplay)
		{
			DebugDrawLevelAxes();
			DebugDrawFrustumGroundPlaneIntersection();
			DebugDrawFauxViewVectors();
		}
	}

	private IEnumerator LoadAssetBundle()
	{
		IsBundleLoaded = false;
		string text = GetLevelRootObject().name;
		string text2 = "assetbundles/ST_" + text;
		text2 = Path.Combine(Application.dataPath, text2.ToLower());
		if (UseAsyncLoading())
		{
			using UnityWebRequest bundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(TextUtils.LiteEscapeUrl("file://" + text2));
			yield return bundleRequest.SendWebRequest();
			if (bundleRequest.isNetworkError || bundleRequest.isHttpError)
			{
				Debug.LogError(bundleRequest.error);
				yield break;
			}
			m_AssetBundle = DownloadHandlerAssetBundle.GetContent(bundleRequest);
			IsBundleLoaded = true;
		}
		else
		{
			m_AssetBundle = AssetBundle.LoadFromFile(text2);
			if (m_AssetBundle == null)
			{
				Debug.LogError("Error trying to load asset bundle " + text2);
			}
			else
			{
				IsBundleLoaded = true;
			}
		}
	}

	public bool FinishedLoading()
	{
		if (m_tilePool == null)
		{
			return false;
		}
		PE_StreamTile[] tilePool = m_tilePool;
		for (int i = 0; i < tilePool.Length; i++)
		{
			if (!tilePool[i].m_texturesAreLoaded)
			{
				return false;
			}
		}
		return true;
	}

	private void ComputeVisibleStreamTiles()
	{
		LevelInfo levelInfo = GetLevelInfo();
		if (PE_GameRender.IsEditorNotPlayer())
		{
			m_streamTileMinX = 0;
			m_streamTileMaxX = levelInfo.m_numStreamTilesX - 1;
			m_streamTileMinY = 0;
			m_streamTileMaxY = levelInfo.m_numStreamTilesY - 1;
			return;
		}
		float num = (float)levelInfo.m_TotalTexWidth / (float)levelInfo.m_TileWidth;
		float num2 = (float)levelInfo.m_TotalTexHeight / (float)levelInfo.m_TileHeight;
		Vector3 normalized = levelInfo.m_backgroundQuadAxisX.normalized;
		Vector3 normalized2 = levelInfo.m_backgroundQuadAxisY.normalized;
		float num3 = Vector3.Dot(levelInfo.m_backgroundQuadOrigin, normalized);
		float num4 = Vector3.Dot(levelInfo.m_backgroundQuadOrigin, normalized2);
		Camera main = Camera.main;
		Ray theRay = main.ScreenPointToRay(Vector3.zero);
		Ray theRay2 = main.ScreenPointToRay(new Vector3(main.pixelWidth, 0f, 0f));
		Ray theRay3 = main.ScreenPointToRay(new Vector3(0f, main.pixelHeight, 0f));
		Ray theRay4 = main.ScreenPointToRay(new Vector3(main.pixelWidth, main.pixelHeight, 0f));
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		float d = 0f;
		Plane groundPlane = new Plane(inNormal, d);
		Vector3 groundPlaneRayIntersectionPosition = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay);
		Vector3 groundPlaneRayIntersectionPosition2 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay2);
		Vector3 groundPlaneRayIntersectionPosition3 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay3);
		Vector3 groundPlaneRayIntersectionPosition4 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay4);
		m_frustumGroundPos00 = groundPlaneRayIntersectionPosition;
		m_frustumGroundPos10 = groundPlaneRayIntersectionPosition2;
		m_frustumGroundPos01 = groundPlaneRayIntersectionPosition3;
		m_frustumGroundPos11 = groundPlaneRayIntersectionPosition4;
		Vector3 a = Vector3.Lerp(m_frustumGroundPos00, m_frustumGroundPos10, 0.5f);
		Vector3 b = Vector3.Lerp(m_frustumGroundPos01, m_frustumGroundPos11, 0.5f);
		m_frustumGroundPosCenter = Vector3.Lerp(a, b, 0.5f);
		float a2 = Vector3.Dot(groundPlaneRayIntersectionPosition, normalized);
		float a3 = Vector3.Dot(groundPlaneRayIntersectionPosition2, normalized);
		float a4 = Vector3.Dot(groundPlaneRayIntersectionPosition3, normalized);
		float b2 = Vector3.Dot(groundPlaneRayIntersectionPosition4, normalized);
		float num5 = Mathf.Min(a2, Mathf.Min(a3, Mathf.Min(a4, b2)));
		m_streamTileMinX = Mathf.FloorToInt((num5 - num3) / levelInfo.m_backgroundQuadWidth * num);
		float num6 = Mathf.Max(a2, Mathf.Max(a3, Mathf.Max(a4, b2)));
		m_streamTileMaxX = Mathf.FloorToInt((num6 - num3) / levelInfo.m_backgroundQuadWidth * num);
		float a5 = Vector3.Dot(groundPlaneRayIntersectionPosition, normalized2);
		float a6 = Vector3.Dot(groundPlaneRayIntersectionPosition2, normalized2);
		float a7 = Vector3.Dot(groundPlaneRayIntersectionPosition3, normalized2);
		float b3 = Vector3.Dot(groundPlaneRayIntersectionPosition4, normalized2);
		float num7 = Mathf.Min(a5, Mathf.Min(a6, Mathf.Min(a7, b3)));
		m_streamTileMinY = Mathf.FloorToInt((num7 - num4) / levelInfo.m_backgroundQuadHeight * num2);
		float num8 = Mathf.Max(a5, Mathf.Max(a6, Mathf.Max(a7, b3)));
		m_streamTileMaxY = Mathf.FloorToInt((num8 - num4) / levelInfo.m_backgroundQuadHeight * num2);
		m_cameraFrustumTileMinX = m_streamTileMinX;
		m_cameraFrustumTileMaxX = m_streamTileMaxX;
		m_cameraFrustumTileMinY = m_streamTileMinY;
		m_cameraFrustumTileMaxY = m_streamTileMaxY;
		m_streamTileMinX = Mathf.Clamp(m_streamTileMinX - streamTileGuardband, 0, levelInfo.m_numStreamTilesX - 1);
		m_streamTileMaxX = Mathf.Clamp(m_streamTileMaxX + streamTileGuardband, 0, levelInfo.m_numStreamTilesX - 1);
		m_streamTileMinY = Mathf.Clamp(m_streamTileMinY - streamTileGuardband, 0, levelInfo.m_numStreamTilesY - 1);
		m_streamTileMaxY = Mathf.Clamp(m_streamTileMaxY + streamTileGuardband, 0, levelInfo.m_numStreamTilesY - 1);
		if (!enableStreaming)
		{
			m_streamTileMinX = 0;
			m_streamTileMaxX = levelInfo.m_numStreamTilesX - 1;
			m_streamTileMinY = 0;
			m_streamTileMaxY = levelInfo.m_numStreamTilesY - 1;
		}
	}

	private void UpdateStreamTilePool()
	{
		int numTiles = (m_streamTileMaxX - m_streamTileMinX + 1) * (m_streamTileMaxY - m_streamTileMinY + 1);
		CreateOrResizeTilePool(numTiles);
		PE_StreamTile[] tilePool = m_tilePool;
		foreach (PE_StreamTile pE_StreamTile in tilePool)
		{
			int tileX = pE_StreamTile.GetTileX();
			int tileY = pE_StreamTile.GetTileY();
			if (tileX >= m_streamTileMinX && tileX <= m_streamTileMaxX && tileY >= m_streamTileMinY && tileY <= m_streamTileMaxY)
			{
				pE_StreamTile.SetNumFramesSinceLastUsed(0);
			}
			else
			{
				pE_StreamTile.IncrementNumFramesSinceLastUsed();
			}
		}
		for (int j = m_streamTileMinX; j <= m_streamTileMaxX; j++)
		{
			for (int k = m_streamTileMinY; k <= m_streamTileMaxY; k++)
			{
				if (!TileExists(j, k))
				{
					int lRUTileIndex = GetLRUTileIndex();
					m_tilePool[lRUTileIndex].SetTileCoords(j, k);
				}
			}
		}
	}

	private void UpdateFrustumVisibleTileList()
	{
		if (m_tilesVisibleInFrustum == null)
		{
			m_tilesVisibleInFrustum = new PE_StreamTile[MAX_NUM_VISIBLE_FRUSTUM_TILES];
		}
		m_numTilesVisibleInFrustum = 0;
		PE_StreamTile[] tilePool = m_tilePool;
		foreach (PE_StreamTile pE_StreamTile in tilePool)
		{
			int tileX = pE_StreamTile.GetTileX();
			int tileY = pE_StreamTile.GetTileY();
			if ((tileX >= m_cameraFrustumTileMinX && tileX <= m_cameraFrustumTileMaxX && tileY >= m_cameraFrustumTileMinY && tileY <= m_cameraFrustumTileMaxY) || ForceAllTilesVisible)
			{
				if (m_numTilesVisibleInFrustum >= m_tilesVisibleInFrustum.Length)
				{
					UIDebug.Instance.LogOnScreenWarning("Maximum Visible StreamTiles Exceeded!", UIDebug.Department.Programming, 10f);
					break;
				}
				m_tilesVisibleInFrustum[m_numTilesVisibleInFrustum++] = pE_StreamTile;
			}
		}
	}

	private bool TileExists(int tileX, int tileY)
	{
		PE_StreamTile[] tilePool = m_tilePool;
		foreach (PE_StreamTile pE_StreamTile in tilePool)
		{
			if (tileX == pE_StreamTile.GetTileX() && tileY == pE_StreamTile.GetTileY())
			{
				return true;
			}
		}
		return false;
	}

	public PE_StreamTile GetTile(int tileX, int tileY)
	{
		if (m_tilePool == null)
		{
			return null;
		}
		PE_StreamTile[] tilePool = m_tilePool;
		foreach (PE_StreamTile pE_StreamTile in tilePool)
		{
			if (tileX == pE_StreamTile.GetTileX() && tileY == pE_StreamTile.GetTileY())
			{
				return pE_StreamTile;
			}
		}
		return null;
	}

	private int GetLRUTileIndex()
	{
		int result = 0;
		int num = 0;
		for (int i = 0; i < m_tilePool.Length; i++)
		{
			PE_StreamTile pE_StreamTile = m_tilePool[i];
			if (pE_StreamTile.GetNumFramesSinceLastUsed() > num)
			{
				result = i;
				num = pE_StreamTile.GetNumFramesSinceLastUsed();
			}
		}
		return result;
	}

	private Vector3 GetGroundPlaneRayIntersectionPosition(Plane groundPlane, Ray theRay)
	{
		float enter = 0f;
		if (!groundPlane.Raycast(theRay, out enter) && enter == 0f)
		{
			Debug.LogError("PE_StreamTileManager::GetGroundPlaneRayIntersectionPosition() - failed to find intersection with high plane");
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	private void CreateOrResizeTilePool(int numTiles)
	{
		if (m_tilePool != null && m_tilePool.Length >= numTiles)
		{
			if (0 <= 0)
			{
				return;
			}
			m_tilePool = null;
		}
		PE_StreamTile[] tilePool = m_tilePool;
		m_tilePool = new PE_StreamTile[numTiles];
		int num = 0;
		if (tilePool != null)
		{
			num = tilePool.Length;
			for (int i = 0; i < tilePool.Length; i++)
			{
				m_tilePool[i] = tilePool[i];
			}
		}
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		array[0] = Vector3.zero;
		array[1] = Vector3.zero;
		array[2] = Vector3.zero;
		array[3] = Vector3.zero;
		array2[0] = Vector2.zero;
		array2[1] = Vector2.right;
		array2[2] = Vector2.up;
		array2[3] = Vector2.one;
		array3[0] = 0;
		array3[1] = 2;
		array3[2] = 1;
		array3[3] = 2;
		array3[4] = 3;
		array3[5] = 1;
		if (m_streamTileParent == null)
		{
			m_streamTileParent = new GameObject();
			m_streamTileParent.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			m_streamTileParent.name = "StreamTileParent_" + base.gameObject.name;
		}
		for (int j = num; j < numTiles; j++)
		{
			string text = j.ToString("D3");
			Mesh mesh = new Mesh();
			mesh.name = "TileMesh_" + text;
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.triangles = array3;
			GameObject gameObject = new GameObject("StreamTile_" + text);
			gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			gameObject.transform.parent = m_streamTileParent.transform;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			gameObject.AddComponent<MeshRenderer>();
			gameObject.isStatic = false;
			gameObject.layer = LayerUtility.FindLayerValue("Background");
			meshFilter.sharedMesh = mesh;
			m_tilePool[j] = gameObject.AddComponent<PE_StreamTile>();
			m_tilePool[j].InitializeTile(this);
		}
	}

	private void DebugDrawLevelAxes()
	{
		LevelInfo levelInfo = GetLevelInfo();
		Vector3 backgroundQuadOrigin = levelInfo.m_backgroundQuadOrigin;
		Vector3 end = backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisX * levelInfo.m_backgroundQuadWidth;
		Debug.DrawLine(backgroundQuadOrigin, end, Color.red, 0f, depthTest: false);
		end = backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
		Debug.DrawLine(backgroundQuadOrigin, end, Color.green, 0f, depthTest: false);
	}

	private void DebugDrawFrustumGroundPlaneIntersection()
	{
		Color magenta = Color.magenta;
		Debug.DrawLine(m_frustumGroundPos00, m_frustumGroundPos10, magenta);
		Debug.DrawLine(m_frustumGroundPos10, m_frustumGroundPos11, magenta);
		Debug.DrawLine(m_frustumGroundPos11, m_frustumGroundPos01, magenta);
		Debug.DrawLine(m_frustumGroundPos01, m_frustumGroundPos00, magenta);
	}

	private void DebugDrawVisibleTile(int tileIndexX, int tileIndexY)
	{
		LevelInfo levelInfo = GetLevelInfo();
		Vector3 tilePositionX = GetTilePositionX(tileIndexX);
		Vector3 tilePositionX2 = GetTilePositionX(tileIndexX + 1);
		Vector3 tilePositionY = GetTilePositionY(tileIndexY);
		Vector3 tilePositionY2 = GetTilePositionY(tileIndexY + 1);
		Vector3 vector = levelInfo.m_backgroundQuadOrigin + tilePositionX + tilePositionY;
		Vector3 vector2 = levelInfo.m_backgroundQuadOrigin + tilePositionX2 + tilePositionY;
		Vector3 vector3 = levelInfo.m_backgroundQuadOrigin + tilePositionX + tilePositionY2;
		Vector3 vector4 = levelInfo.m_backgroundQuadOrigin + tilePositionX2 + tilePositionY2;
		Color yellow = Color.yellow;
		Debug.DrawLine(vector, vector2, yellow);
		Debug.DrawLine(vector2, vector4, yellow);
		Debug.DrawLine(vector4, vector3, yellow);
		Debug.DrawLine(vector3, vector, yellow);
	}

	private void DebugDrawVisibleTiles()
	{
		for (int i = m_streamTileMinY; i <= m_streamTileMaxY; i++)
		{
			for (int j = m_streamTileMinX; j <= m_streamTileMaxX; j++)
			{
				DebugDrawVisibleTile(j, i);
			}
		}
	}

	private void DebugDrawFauxViewVectors()
	{
		Color yellow = Color.yellow;
		PE_GameRender instance = PE_GameRender.Instance;
		Debug.DrawLine(instance.m_fauxViewPos000, instance.m_fauxViewPos001, yellow);
		Debug.DrawLine(instance.m_fauxViewPos100, instance.m_fauxViewPos101, yellow);
		Debug.DrawLine(instance.m_fauxViewPos010, instance.m_fauxViewPos011, yellow);
		Debug.DrawLine(instance.m_fauxViewPos110, instance.m_fauxViewPos111, yellow);
	}
}
