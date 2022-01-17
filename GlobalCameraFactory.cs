using UnityEngine;

public class GlobalCameraFactory : MonoBehaviour
{
	public GameObject MainCamera;

	public static GlobalCameraFactory Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GlobalCameraFactory' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}
}
