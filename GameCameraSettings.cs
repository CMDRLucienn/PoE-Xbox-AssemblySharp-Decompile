using UnityEngine;

public class GameCameraSettings : MonoBehaviour
{
	public bool EnableShaderOcclusion = true;

	public Texture2D AmbientMapOverride;

	[HideInInspector]
	public PE_LightEnvironment LightEnvironment;

	private PE_GameRender gameRender;

	private void Start()
	{
		gameRender = PE_GameRender.Instance;
	}

	private void OnPreRender()
	{
		Shader.SetGlobalFloat("_EnableTrentonOcclusion", EnableShaderOcclusion ? 1f : 0f);
		if ((bool)LightEnvironment)
		{
			gameRender.ShaderSetEnvironment(LightEnvironment);
		}
		if ((bool)AmbientMapOverride)
		{
			Shader.SetGlobalTexture("Trenton_AmbientMap", AmbientMapOverride);
		}
	}

	private void OnPostRender()
	{
		if ((!Application.isEditor || Application.isPlaying) && !PE_GameRender.Instance.m_blockoutMode)
		{
			Shader.SetGlobalFloat("_EnableTrentonOcclusion", 1f);
		}
		else
		{
			Shader.SetGlobalFloat("_EnableTrentonOcclusion", 0f);
		}
		gameRender.ShaderSetEnvironment(null);
	}
}
