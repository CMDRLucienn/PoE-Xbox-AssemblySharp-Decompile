using UnityEngine;

public class DepthPassScript : MonoBehaviour
{
	public Shader m_DepthOnly;

	private void Start()
	{
		GetComponent<Camera>().SetReplacementShader(m_DepthOnly, "");
	}
}
