using UnityEngine;

public class EnableWith : MonoBehaviour
{
	public GameObject Source;

	public MonoBehaviour Target;

	private void Update()
	{
		Target.enabled = Source.activeInHierarchy;
	}
}
