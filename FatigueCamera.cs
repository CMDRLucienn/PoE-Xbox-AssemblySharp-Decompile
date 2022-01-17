using UnityEngine;

public class FatigueCamera : MonoBehaviour
{
	private CharacterStats m_playerStats;

	private PE_PostProcessColorLevels m_colorLevels;

	public static void CreateCamera()
	{
		GameObject gameObject = GameResources.LoadPrefab<GameObject>("WatcherFatigueCamera", instantiate: true);
		if ((bool)gameObject)
		{
			Persistence component = gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				GameUtilities.Destroy(component);
			}
			gameObject.transform.parent = Camera.main.gameObject.transform;
		}
	}

	private void Start()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			m_playerStats = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		}
		m_colorLevels = GetComponent<PE_PostProcessColorLevels>();
		m_colorLevels.FadeIn();
	}

	private void Update()
	{
		if ((bool)m_playerStats)
		{
			float b = (float)m_playerStats.CurrentFatigueLevel / 3f;
			m_colorLevels.MaxLerpValue = Mathf.Min(1f, b);
		}
		else if ((bool)GameState.s_playerCharacter)
		{
			m_playerStats = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		}
	}
}
