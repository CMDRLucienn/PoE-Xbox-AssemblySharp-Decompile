using UnityEngine;

public class ScreenTextureScript_Occlusion : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Camera>().enabled = true;
		GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_OcclusionEffect"), string.Empty);
	}
}
