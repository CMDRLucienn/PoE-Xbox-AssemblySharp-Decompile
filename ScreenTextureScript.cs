using UnityEngine;

public class ScreenTextureScript : MonoBehaviour
{
	public PE_GameRender.ScreenTextureType textureType;

	private void Start()
	{
		GetComponent<Camera>().enabled = true;
		if (textureType == PE_GameRender.ScreenTextureType.Depth)
		{
			GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_DynamicMeshDepth"), string.Empty);
		}
		else if (textureType == PE_GameRender.ScreenTextureType.AlbedoSpec)
		{
			GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_DynamicAlbedoSpec"), string.Empty);
		}
		else if (textureType == PE_GameRender.ScreenTextureType.Normals)
		{
			GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_DynamicMeshNormals"), string.Empty);
		}
	}

	private void Update()
	{
		if (GetComponent<Camera>() != null && null == GetComponent<Camera>().targetTexture)
		{
			GetComponent<Camera>().targetTexture = PE_GameRender.Instance.GetScreenTextureType(textureType);
		}
	}
}
