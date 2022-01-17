using UnityEngine;

public class OccludedDynamicsScript : MonoBehaviour
{
	public Shader OccludedDynamicsShader;

	private void Start()
	{
		GetComponent<Camera>().SetReplacementShader(OccludedDynamicsShader, "");
	}
}
