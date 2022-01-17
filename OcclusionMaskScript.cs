using UnityEngine;

public class OcclusionMaskScript : MonoBehaviour
{
	public Shader m_OcclusionMaskShader;

	public RenderTexture m_MaskTexture;

	private int m_screen_width;

	private int m_screen_height;

	private void CreateMaskTexture(int width, int height)
	{
		if (m_MaskTexture == null)
		{
			m_MaskTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		}
		else
		{
			if (m_MaskTexture.IsCreated())
			{
				m_MaskTexture.Release();
			}
			m_MaskTexture.width = width;
			m_MaskTexture.height = height;
		}
		m_screen_width = width;
		m_screen_height = height;
		m_MaskTexture.SetGlobalShaderProperty("_OcclusionMask");
		if (GetComponent<Camera>() != null)
		{
			GetComponent<Camera>().targetTexture = m_MaskTexture;
		}
	}

	private void Start()
	{
		GetComponent<Camera>().SetReplacementShader(m_OcclusionMaskShader, "");
		CreateMaskTexture(Screen.width, Screen.height);
	}

	private void Update()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (width != m_screen_width || height != m_screen_height)
		{
			CreateMaskTexture(width, height);
		}
	}
}
