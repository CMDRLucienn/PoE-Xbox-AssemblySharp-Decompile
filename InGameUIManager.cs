using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class InGameUIManager : MonoBehaviour
{
	public string testScene = "Prototype_Interior_01";

	public InGameUILayout Hud;

	private void Awake()
	{
		Hud.gameObject.SetActive(value: true);
		if (Application.isPlaying)
		{
			GameUtilities.CreateInGameGlobalPrefabObject();
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			GameState.PersistAcrossSceneLoadsTracked(base.gameObject);
			if (LevelInfo.Instance == null)
			{
				SceneManager.LoadScene(testScene);
			}
			UIDynamicFontSize.ReloadAllFonts();
		}
		else
		{
			StringTableManager.Init();
		}
	}
}
