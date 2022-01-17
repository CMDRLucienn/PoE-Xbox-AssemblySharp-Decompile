using UnityEngine;

public class Timer : MonoBehaviour
{
	[Persistent]
	public float Delay;

	[Persistent]
	private bool m_Running;

	private ScriptEvent m_ScriptEvent;

	private void Start()
	{
		m_ScriptEvent = GetComponent<ScriptEvent>();
	}

	private void Update()
	{
		if (!m_Running || GameState.IsLoading)
		{
			return;
		}
		Delay -= Time.deltaTime;
		if (Delay <= 0f)
		{
			if ((bool)m_ScriptEvent)
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnTimerFinished);
			}
			m_Running = false;
		}
	}

	public void StartTimer()
	{
		m_Running = true;
	}

	public void StopTimer()
	{
		m_Running = false;
	}
}
