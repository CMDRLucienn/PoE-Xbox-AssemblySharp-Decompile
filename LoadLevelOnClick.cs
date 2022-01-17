using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelOnClick : MonoBehaviour
{
	public string levelName;

	private void OnClick()
	{
		Debug.Log("\n");
		Debug.Log("------- BEGIN LEVEL LOAD INITIATED --------        Pending level = " + levelName + ".");
		SceneManager.LoadScene(levelName);
	}
}
