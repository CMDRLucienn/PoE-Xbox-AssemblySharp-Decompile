using UnityEngine;

public class DynamicsShadowScript : MonoBehaviour
{
	public Shader m_DynamicsShadowShader;

	public RenderTexture m_ShadowTexture;

	private int m_screen_width;

	private int m_screen_height;

	private void CreateMaskTexture(int width, int height)
	{
		if (m_ShadowTexture == null)
		{
			m_ShadowTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		}
		else
		{
			if (m_ShadowTexture.IsCreated())
			{
				m_ShadowTexture.Release();
			}
			m_ShadowTexture.width = width;
			m_ShadowTexture.height = height;
		}
		m_screen_width = width;
		m_screen_height = height;
		m_ShadowTexture.SetGlobalShaderProperty("_DynamicsShadowMap");
		_ = GetComponent<Camera>() != null;
	}

	private void Start()
	{
		GetComponent<Camera>().SetReplacementShader(m_DynamicsShadowShader, "");
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
