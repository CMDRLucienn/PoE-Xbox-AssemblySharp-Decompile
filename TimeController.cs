using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
	public bool ProhibitPause;

	public float NormalTime = 1f;

	public float SlowTime = 0.2f;

	public KeyCode SlowToggle = KeyCode.S;

	public float FastTime = 1.8f;

	public KeyCode FastToggle = KeyCode.D;

	private bool m_PlayerPaused;

	private bool m_UiPaused;

	private List<AudioSource> m_sourcesPaused = new List<AudioSource>();

	private List<float> m_sourcesPausedTime = new List<float>();

	private float m_resumeTime = 1f;

	private float m_TimeScale;

	public static TimeController Instance { get; private set; }

	public static float sUnscaledDelta => Time.unscaledDeltaTime;

	private float TimeScale
	{
		get
		{
			return m_TimeScale;
		}
		set
		{
			m_TimeScale = value;
			if (!GameState.InCombat)
			{
				m_resumeTime = m_TimeScale;
			}
			UpdateTimeScale();
		}
	}

	public static float NotSpedUpDeltaTime
	{
		get
		{
			float num = Time.deltaTime;
			if (Time.timeScale > 1f)
			{
				num /= Time.timeScale;
			}
			return num;
		}
	}

	public float RealtimeSinceStartupThisFrame { get; private set; }

	public bool Slow
	{
		get
		{
			return TimeScale == SlowTime;
		}
		set
		{
			if (value)
			{
				TimeScale = SlowTime;
			}
			else if (TimeScale == SlowTime)
			{
				TimeScale = NormalTime;
			}
			UpdateTimeScale();
		}
	}

	public bool Fast
	{
		get
		{
			return TimeScale == FastTime;
		}
		set
		{
			if (value && !GameState.InCombat)
			{
				TimeScale = FastTime;
			}
			else if (TimeScale == FastTime)
			{
				TimeScale = NormalTime;
			}
			UpdateTimeScale();
		}
	}

	public static bool IsSafeToPause
	{
		get
		{
			if (FadeManager.Instance.FadeValue > 0f)
			{
				return false;
			}
			if (GameState.PlayerSafeMode || GameInput.DisableInput || GameInput.BlockAllKeys)
			{
				return false;
			}
			return true;
		}
	}

	public bool SafePaused
	{
		get
		{
			return Paused;
		}
		set
		{
			if (!value || IsSafeToPause)
			{
				Paused = value;
			}
		}
	}

	public bool Paused
	{
		get
		{
			return Time.timeScale == 0f;
		}
		set
		{
			bool num = m_PlayerPaused || m_UiPaused;
			m_PlayerPaused = value;
			UpdateTimeScale();
			bool flag = m_PlayerPaused || m_UiPaused;
			if (num == flag)
			{
				return;
			}
			if (value)
			{
				AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
				m_sourcesPaused.Clear();
				for (int i = 0; i < array.Length; i++)
				{
					if ((bool)array[i].GetComponent<DeveloperCommentary>() || !array[i].isPlaying)
					{
						continue;
					}
					if (array[i].gameObject.name.Contains("Global"))
					{
						AudioSource[] components = array[i].gameObject.GetComponents<AudioSource>();
						for (int j = 0; j < components.Length; j++)
						{
							if (components[j].clip != null && !components[j].clip.name.StartsWith("mus") && !components[j].clip.name.Contains("_mus_"))
							{
								m_sourcesPausedTime.Add(components[j].time);
								components[j].Pause();
								m_sourcesPaused.Add(components[j]);
							}
						}
					}
					else
					{
						m_sourcesPausedTime.Add(array[i].time);
						array[i].Pause();
						m_sourcesPaused.Add(array[i]);
					}
				}
				return;
			}
			for (int k = 0; k < m_sourcesPaused.Count; k++)
			{
				AudioSource audioSource = m_sourcesPaused[k];
				if (audioSource != null)
				{
					if (!audioSource.enabled)
					{
						audioSource.enabled = true;
					}
					audioSource.time = m_sourcesPausedTime[k];
					GlobalAudioPlayer.Play(audioSource);
				}
			}
			m_sourcesPaused.Clear();
			m_sourcesPausedTime.Clear();
		}
	}

	public bool UiPaused
	{
		get
		{
			return m_UiPaused;
		}
		set
		{
			m_UiPaused = value;
			UpdateTimeScale();
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'TimeController' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		GameState.OnCombatEnd += OnCombatEnd;
	}

	private void Start()
	{
		m_TimeScale = NormalTime;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
		GameState.OnCombatEnd -= OnCombatEnd;
	}

	private void OnCombatEnd(object sender, EventArgs e)
	{
		TimeScale = m_resumeTime;
	}

	private void Update()
	{
		RealtimeSinceStartupThisFrame = Time.realtimeSinceStartup;
		if (GameState.InCombat && TimeScale == FastTime)
		{
			TimeScale = 1f;
		}
		if (!GameState.IsLoading)
		{
			UpdateTimeScale();
		}
		if (!UIWindowManager.KeyInputAvailable)
		{
			return;
		}
		if (GameInput.GetControlDown(MappedControl.RESTORE_SPEED, handle: true))
		{
			TimeScale = NormalTime;
		}
		else if (GameInput.GetControlDown(MappedControl.SLOW_TOGGLE, handle: true))
		{
			ToggleSlow();
		}
		else if (GameInput.GetControlDown(MappedControl.FAST_TOGGLE, handle: true))
		{
			ToggleFast();
		}
		else if (GameInput.GetControlDown(MappedControl.GAME_SPEED_CYCLE, handle: true))
		{
			if (Fast)
			{
				Slow = true;
			}
			else if (Slow)
			{
				Slow = false;
			}
			else if (GameState.InCombat)
			{
				Slow = true;
			}
			else
			{
				Fast = true;
			}
		}
	}

	public void SetNormal()
	{
		if (Paused)
		{
			SafePaused = false;
			TimeScale = NormalTime;
		}
		else
		{
			SafePaused = true;
			UpdateTimeScale();
		}
	}

	public void ToggleSlow()
	{
		if (TimeScale == SlowTime)
		{
			TimeScale = NormalTime;
		}
		else
		{
			TimeScale = SlowTime;
		}
		UpdateTimeScale();
	}

	public void ToggleFast()
	{
		if (TimeScale == FastTime)
		{
			TimeScale = NormalTime;
		}
		else if (!GameState.InCombat)
		{
			TimeScale = FastTime;
		}
		UpdateTimeScale();
	}

	public void AddPausedSource(AudioSource audioSource, float startTime)
	{
		if ((bool)audioSource && !(startTime < 0f) && m_sourcesPaused != null && m_sourcesPausedTime != null)
		{
			m_sourcesPaused.Add(audioSource);
			m_sourcesPausedTime.Add(startTime);
		}
	}

	public bool IsSourcePaused(AudioSource audioSource)
	{
		return m_sourcesPaused.Contains(audioSource);
	}

	private void UpdateTimeScale()
	{
		if ((m_PlayerPaused || m_UiPaused) && !ProhibitPause)
		{
			Time.timeScale = 0f;
		}
		else if (Cutscene.CutsceneActive)
		{
			Time.timeScale = 1f;
		}
		else
		{
			Time.timeScale = TimeScale;
		}
	}
}
