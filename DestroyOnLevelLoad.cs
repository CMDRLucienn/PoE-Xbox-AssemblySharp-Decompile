using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnLevelLoad : MonoBehaviour
{
	public bool InGameLevelOnly;

	public bool FadePanel;

	public float FadeTime = 1f;

	private bool m_loadTriggered;

	private void Start()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
		GameState.PersistAcrossSceneLoadsUntracked(base.gameObject);
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
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
		if (!GameState.SceneIsTransitionScene(scene) && !GameState.SceneIsStartingScene(scene) && !InGameLevelOnly)
		{
			OnLevelLoaded(null, null);
		}
	}

	private void Update()
	{
		if (!m_loadTriggered || !FadePanel)
		{
			return;
		}
		UIPanel component = GetComponent<UIPanel>();
		if ((bool)component)
		{
			component.alpha -= Time.deltaTime * FadeTime;
			if (component.alpha <= 0f)
			{
				GameUtilities.Destroy(base.gameObject);
			}
		}
		else
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		m_loadTriggered = true;
		if (!FadePanel)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}
}
