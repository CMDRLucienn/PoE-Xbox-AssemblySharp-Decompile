using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneTransitionSubstitutes : MonoBehaviour
{
	public static UISceneTransitionSubstitutes Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameUtilities.Destroy(base.transform.GetChild(i).gameObject);
		}
	}

	public UIEventListener Create()
	{
		GameObject obj = new GameObject("Substitute");
		obj.transform.parent = base.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.layer = LayerUtility.FindLayerValue("NGUI");
		obj.AddComponent<BoxCollider>();
		return UIEventListener.Get(obj);
	}
}
