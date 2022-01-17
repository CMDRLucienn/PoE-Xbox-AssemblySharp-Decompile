using System.Collections.Generic;
using UnityEngine;

public class Detectable : MonoBehaviour
{
	public static List<Detectable> ActiveDetectables = new List<Detectable>();

	public int Difficulty;

	public bool StartsDetected;

	[Persistent]
	private bool m_detected;

	private ScaledContent m_CachedScaler;

	public bool Detected => m_detected;

	public int GetDifficulty()
	{
		int num = Difficulty;
		if ((bool)m_CachedScaler)
		{
			float scaleMultiplicative = DifficultyScaling.Instance.GetScaleMultiplicative(m_CachedScaler, (DifficultyScaling.ScaleData scaledata) => scaledata.DetectableDifficultyMult);
			num = Mathf.CeilToInt((float)num * scaleMultiplicative);
		}
		return num;
	}

	private void Awake()
	{
		m_CachedScaler = GetComponent<ScaledContent>();
		if (StartsDetected)
		{
			m_detected = true;
		}
	}

	private void OnEnable()
	{
		ActiveDetectables.Add(this);
	}

	private void OnDisable()
	{
		ActiveDetectables.Remove(this);
	}

	public void IgnoreDetection()
	{
		m_detected = true;
	}

	public void Detect(GameObject character)
	{
		if (!m_detected)
		{
			BroadcastDetection(character);
			m_detected = true;
		}
	}

	public void Hide()
	{
		m_detected = false;
	}

	private void BroadcastDetection(GameObject finder)
	{
		GameState.AutoPause(AutoPauseOptions.PauseEvent.HiddenObjectFound, finder, base.gameObject);
		object[] components = base.gameObject.GetComponents<MonoBehaviour>();
		object[] array = components;
		if (array != null)
		{
			components = array;
			foreach (object obj in components)
			{
				if (obj is iCanBeDetected)
				{
					(obj as iCanBeDetected).OnDetection();
				}
			}
		}
		if ((bool)GetComponent<Trap>())
		{
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.TrapDiscovered);
		}
		else
		{
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.HiddenDiscovered);
		}
		Console.AddMessage(GUIUtils.Format(1859, CharacterStats.Name(finder)), Color.green);
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnDetected);
		}
	}
}
