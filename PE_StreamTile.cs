using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class PE_StreamTile : MonoBehaviour
{
	private int m_tileX;

	private int m_tileY;

	private int m_numFramesSinceLastUsed;

	private PE_StreamTileManager m_tileManager;

	private MeshFilter m_meshFilter;

	private Renderer m_renderer;

	private Material m_material;

	private Material m_backgroundPassMaterial;

	private Material m_normalMapPassMaterial;

	private Material m_dynamicShadowPassMaterial;

	private Material m_mrtMaterial;

	private Texture2D m_backgroundTexture;

	private Texture2D m_heightTexture;

	private Texture2D m_normalsTexture;

	private Texture2D m_albedoTexture;

	private Vector3[] m_verts;

	private Vector3[] m_vertsCopy;

	private AssetBundleRequest m_loadRequest_backgroundTexture;

	private AssetBundleRequest m_loadRequest_heightTexture;

	private AssetBundleRequest m_loadRequest_normalsTexture;

	private AssetBundleRequest m_loadRequest_albedoTexture;

	public bool m_texturesAreLoaded;

	private void DestroyObj(Object t)
	{
		GameUtilities.Destroy(t);
	}

	private void FreeResources()
	{
		if (m_material != null)
		{
			DestroyObj(m_material);
		}
		if (m_backgroundPassMaterial != null)
		{
			DestroyObj(m_backgroundPassMaterial);
		}
		if (m_normalMapPassMaterial != null)
		{
			DestroyObj(m_normalMapPassMaterial);
		}
		if (m_dynamicShadowPassMaterial != null)
		{
			DestroyObj(m_dynamicShadowPassMaterial);
		}
		if (m_mrtMaterial != null)
		{
			DestroyObj(m_mrtMaterial);
		}
		if (m_meshFilter != null && m_meshFilter.sharedMesh != null)
		{
			DestroyObj(m_meshFilter.sharedMesh);
		}
	}

	public void OnDestroy()
	{
		FreeResources();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public Vector3[] GetVerts()
	{
		return m_vertsCopy;
	}

	public int GetTileX()
	{
		return m_tileX;
	}

	public int GetTileY()
	{
		return m_tileY;
	}

	public void SetNumFramesSinceLastUsed(int numFramesSinceLastUsed)
	{
		m_numFramesSinceLastUsed = numFramesSinceLastUsed;
	}

	public int GetNumFramesSinceLastUsed()
	{
		return m_numFramesSinceLastUsed;
	}

	public void IncrementNumFramesSinceLastUsed()
	{
		m_numFramesSinceLastUsed++;
	}

	public Material GetMRTMaterial()
	{
		return m_mrtMaterial;
	}

	public void ApplyShaderSettingsForMRT()
	{
		m_mrtMaterial.SetTexture("_MainTex", m_backgroundTexture);
		m_mrtMaterial.SetTexture("_DepthTex", m_heightTexture);
		m_mrtMaterial.SetTexture("_NormalTex", m_normalsTexture);
		m_mrtMaterial.SetTexture("_AlbedoSpecTex", m_albedoTexture);
	}

	public void SwapTexture(PE_GameRender.ScreenTextureType textureType)
	{
		if ((bool)m_material && (bool)m_backgroundTexture)
		{
			m_renderer.material = m_material;
			switch (textureType)
			{
			case PE_GameRender.ScreenTextureType.None:
				m_backgroundPassMaterial.mainTexture = m_backgroundTexture;
				m_renderer.material = m_backgroundPassMaterial;
				break;
			case PE_GameRender.ScreenTextureType.Depth:
				m_material.mainTexture = m_heightTexture;
				break;
			case PE_GameRender.ScreenTextureType.Normals:
				m_normalMapPassMaterial.mainTexture = m_normalsTexture;
				m_renderer.material = m_normalMapPassMaterial;
				break;
			case PE_GameRender.ScreenTextureType.AlbedoSpec:
				m_material.mainTexture = m_albedoTexture;
				break;
			case PE_GameRender.ScreenTextureType.DynamicShadow:
				m_renderer.material = m_dynamicShadowPassMaterial;
				break;
			default:
				Debug.LogError("PE_StreamTile::SwapTexture() - need to handle this texture type");
				break;
			}
		}
	}

	public void InitializeTile(PE_StreamTileManager manager)
	{
		m_tileManager = manager;
		m_tileX = -1;
		m_tileY = -1;
		m_material = new Material(Shader.Find("Trenton/PE_UnlitZwriteOff"));
		m_backgroundPassMaterial = new Material(Shader.Find("Trenton/PE_BackgroundPass"));
		m_normalMapPassMaterial = new Material(Shader.Find("Trenton/NormalMapPass"));
		m_dynamicShadowPassMaterial = new Material(Shader.Find("Trenton/PE_DynamicShadowScreenPass"));
		m_mrtMaterial = new Material(Shader.Find("Trenton/PE_BackgroundPassMRT"));
		m_renderer = base.gameObject.GetComponent<MeshRenderer>();
		m_renderer.shadowCastingMode = ShadowCastingMode.Off;
		m_renderer.receiveShadows = false;
	}

	public void SetTileCoords(int tileX, int tileY)
	{
		m_tileX = tileX;
		m_tileY = tileY;
		LevelInfo levelInfo = m_tileManager.GetLevelInfo();
		m_numFramesSinceLastUsed = 0;
		if (m_verts == null || m_verts.Length < 4)
		{
			m_verts = new Vector3[4];
		}
		if (m_vertsCopy == null || m_vertsCopy.Length < 4)
		{
			m_vertsCopy = new Vector3[4];
		}
		m_verts[0] = ComputeTileVertexWorldPosition(tileX, tileY);
		m_verts[1] = ComputeTileVertexWorldPosition(tileX + 1, tileY);
		m_verts[2] = ComputeTileVertexWorldPosition(tileX, tileY + 1);
		m_verts[3] = ComputeTileVertexWorldPosition(tileX + 1, tileY + 1);
		m_vertsCopy[0] = m_verts[0];
		m_vertsCopy[1] = m_verts[1];
		m_vertsCopy[2] = m_verts[2];
		m_vertsCopy[3] = m_verts[3];
		m_meshFilter = GetComponent<MeshFilter>();
		Mesh sharedMesh = m_meshFilter.sharedMesh;
		sharedMesh.vertices = m_verts;
		sharedMesh.RecalculateBounds();
		m_texturesAreLoaded = false;
		if (m_tileManager.UseAsyncLoading())
		{
			StartCoroutine(LoadTexturesAsync(levelInfo.GetLevelName(), levelInfo));
		}
		else
		{
			LoadTextures(m_tileManager.GetAssetBundle(), levelInfo.m_LevelDataPath, levelInfo.GetLevelName(), levelInfo);
		}
	}

	public void DoVisibleUpdate()
	{
	}

	public void ReleaseTile()
	{
		StopAllCoroutines();
		m_texturesAreLoaded = false;
		FreeResources();
		m_backgroundTexture = null;
		m_heightTexture = null;
		m_normalsTexture = null;
		m_albedoTexture = null;
		m_loadRequest_backgroundTexture = null;
		m_loadRequest_heightTexture = null;
		m_loadRequest_normalsTexture = null;
		m_loadRequest_albedoTexture = null;
		base.gameObject.transform.parent = null;
		GameUtilities.DestroyImmediate(base.gameObject);
	}

	private void OnBecameVisible()
	{
		if (base.enabled && (bool)GetComponent<Renderer>() && GetComponent<Renderer>().enabled)
		{
			PE_GameRender instance = PE_GameRender.Instance;
			if ((bool)instance)
			{
				instance.RegisterVisibleStreamTile(base.gameObject);
			}
		}
	}

	private void OnBecameInvisible()
	{
		PE_GameRender instance = PE_GameRender.Instance;
		if ((bool)instance)
		{
			instance.UnregisterVisibleStreamTile(base.gameObject);
		}
	}

	private void Start()
	{
		m_verts = new Vector3[4];
	}

	private void Update()
	{
		if (null == m_renderer)
		{
			m_renderer = GetComponent<Renderer>();
		}
		if (m_backgroundTexture != null)
		{
			FilterMode filterMode = FilterMode.Bilinear;
			m_backgroundTexture.filterMode = filterMode;
			if ((bool)m_albedoTexture)
			{
				m_albedoTexture.filterMode = filterMode;
			}
			if ((bool)m_normalsTexture)
			{
				m_normalsTexture.filterMode = filterMode;
			}
			if ((bool)m_heightTexture)
			{
				m_heightTexture.filterMode = FilterMode.Point;
			}
		}
	}

	private Vector3 ComputeTileVertexWorldPosition(int vertCoordX, int vertCoordY)
	{
		LevelInfo levelInfo = m_tileManager.GetLevelInfo();
		Vector3 tilePositionX = m_tileManager.GetTilePositionX(vertCoordX);
		Vector3 tilePositionY = m_tileManager.GetTilePositionY(vertCoordY);
		return levelInfo.m_backgroundQuadOrigin + tilePositionX + tilePositionY;
	}

	private IEnumerator LoadTexturesAsync(string levelName, LevelInfo levelInfo)
	{
		while (!m_tileManager.IsBundleLoaded)
		{
			yield return null;
		}
		string base_texturename = levelName + "-R" + m_tileY.ToString("D3") + "_C" + m_tileX.ToString("D3");
		string textureName = "BKG_" + base_texturename;
		m_loadRequest_backgroundTexture = m_tileManager.LoadAsyncTexture(textureName);
		yield return m_loadRequest_backgroundTexture;
		m_backgroundTexture = m_loadRequest_backgroundTexture.asset as Texture2D;
		float mipMapBias = -2f;
		PE_GameRender instance = PE_GameRender.Instance;
		if ((bool)instance)
		{
			mipMapBias = instance.mipMapBias_background;
		}
		if (m_backgroundTexture != null)
		{
			m_backgroundTexture.filterMode = FilterMode.Trilinear;
			m_backgroundTexture.mipMapBias = mipMapBias;
		}
		m_renderer.material = m_material;
		m_material.mainTexture = m_backgroundTexture;
		m_loadRequest_backgroundTexture = null;
		string textureName2 = "HGT_" + base_texturename;
		m_loadRequest_heightTexture = m_tileManager.LoadAsyncTexture(textureName2);
		yield return m_loadRequest_heightTexture;
		m_heightTexture = m_loadRequest_heightTexture.asset as Texture2D;
		m_loadRequest_heightTexture = null;
		m_texturesAreLoaded = true;
		string textureName3 = "NM_" + base_texturename;
		m_loadRequest_normalsTexture = m_tileManager.LoadAsyncTexture(textureName3);
		yield return m_loadRequest_normalsTexture;
		m_normalsTexture = m_loadRequest_normalsTexture.asset as Texture2D;
		m_loadRequest_normalsTexture = null;
		yield return m_loadRequest_albedoTexture;
		string textureName4 = "AS_" + base_texturename;
		m_loadRequest_albedoTexture = m_tileManager.LoadAsyncTexture(textureName4);
		m_albedoTexture = m_loadRequest_albedoTexture.asset as Texture2D;
		m_loadRequest_albedoTexture = null;
		yield return null;
	}

	public void LoadTextures(AssetBundle bundle, string levelPath, string levelName, LevelInfo levelInfo)
	{
		string text = levelName + "-R" + m_tileY.ToString("D3") + "_C" + m_tileX.ToString("D3");
		string fileName = "BKG_" + text;
		m_backgroundTexture = m_tileManager.LoadTexture(bundle, levelPath, "Textures/", fileName, levelInfo.m_ColorImgExt);
		string fileName2 = "HGT_" + text;
		m_heightTexture = m_tileManager.LoadTexture(bundle, levelPath, "Heightmaps/", fileName2, levelInfo.m_HeightImgExt);
		string fileName3 = "NM_" + text;
		m_normalsTexture = m_tileManager.LoadTexture(bundle, levelPath, "Normalmaps/", fileName3, levelInfo.m_NormalMapExt);
		string fileName4 = "AS_" + text;
		m_albedoTexture = m_tileManager.LoadTexture(bundle, levelPath, "AlbedoSpec/", fileName4, levelInfo.m_AlbedoMapExt);
		m_texturesAreLoaded = true;
		m_renderer.material = m_material;
		m_material.mainTexture = m_backgroundTexture;
		float mipMapBias = -2f;
		PE_GameRender instance = PE_GameRender.Instance;
		if ((bool)instance)
		{
			mipMapBias = instance.mipMapBias_background;
		}
		if (m_backgroundTexture != null)
		{
			m_backgroundTexture.filterMode = FilterMode.Point;
			m_backgroundTexture.mipMapBias = mipMapBias;
		}
	}
}
