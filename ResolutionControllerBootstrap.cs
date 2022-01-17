using UnityEngine;

public class ResolutionControllerBootstrap : MonoBehaviour
{
	private void Awake()
	{
		GameObject obj = new GameObject();
		obj.name = "ResolutionController";
		obj.AddComponent<ResolutionController>();
		Object.DontDestroyOnLoad(obj);
	}
}
