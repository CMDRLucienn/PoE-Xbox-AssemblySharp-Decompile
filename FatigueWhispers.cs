using System.Collections;
using UnityEngine;

public class FatigueWhispers : MonoBehaviour
{
	public ClipBankSet RandomWhispers;

	public AudioClip LoopingWhisperSound;

	public float MinWhisperDelay;

	public float MaxWhisperDelay;

	public float MinorFatigueVolume = 0.4f;

	public float MajorFatigueVolume = 0.75f;

	public float CriticalFatigueVolume = 1f;

	public float VolumeChangeRate = 1.2f;

	[Persistent]
	private float m_DesiredVolume;

	private CharacterStats.FatigueLevel m_CurrentFatigueLevel;

	private AudioSource m_AudioSource;

	private bool m_Initialized;

	[Persistent]
	private bool m_VolumeIsOverridden;

	private bool m_CanHearWhispers;

	public static FatigueWhispers Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FatigueWhispers' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		GameResources.OnPreloadGame += PreLoadGame;
	}

	private void OnDestroy()
	{
		GameResources.OnPreloadGame -= PreLoadGame;
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		if (!LoopingWhisperSound)
		{
			base.enabled = false;
			return;
		}
		m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		m_AudioSource.volume = m_DesiredVolume;
		m_AudioSource.clip = LoopingWhisperSound;
		m_AudioSource.playOnAwake = false;
	}

	private void PreLoadGame()
	{
		m_VolumeIsOverridden = false;
	}

	public void Restored()
	{
		m_Initialized = false;
		if ((bool)GameState.s_playerCharacter)
		{
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if (component != null)
			{
				m_CurrentFatigueLevel = component.GetFatigueLevel();
			}
		}
	}

	private void Update()
	{
		if (!m_Initialized && (bool)GameState.s_playerCharacter)
		{
			if (GameState.s_playerCharacter == null)
			{
				return;
			}
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if (component == null)
			{
				return;
			}
			component.OnFatigueLevelChanged += OnFatigueChanged;
			m_CanHearWhispers = GameGlobalVariables.IsPlayerWatcher();
			m_Initialized = true;
		}
		if (m_Initialized && !m_CanHearWhispers && GameGlobalVariables.IsPlayerWatcher())
		{
			m_CanHearWhispers = true;
			if (GameState.s_playerCharacter != null)
			{
				CharacterStats component2 = GameState.s_playerCharacter.GetComponent<CharacterStats>();
				if (component2 != null)
				{
					SetDesiredVolume(component2.GetFatigueLevel());
				}
			}
		}
		float num = (float)((Time.deltaTime == 0f) ? 0.03 : ((double)Time.deltaTime));
		if (m_AudioSource != null && !Mathf.Approximately(m_AudioSource.volume, m_DesiredVolume))
		{
			if ((double)Mathf.Abs(m_AudioSource.volume - m_DesiredVolume) < 0.01)
			{
				m_AudioSource.volume = m_DesiredVolume;
			}
			else
			{
				m_AudioSource.volume = Mathf.Lerp(m_AudioSource.volume, m_DesiredVolume, num * VolumeChangeRate);
			}
			if (Mathf.Approximately(m_AudioSource.volume, 0f))
			{
				m_AudioSource.Stop();
				StopAllCoroutines();
			}
		}
	}

	private void OnFatigueChanged(CharacterStats.FatigueLevel newLevel)
	{
		if (!m_VolumeIsOverridden && newLevel != m_CurrentFatigueLevel)
		{
			SetDesiredVolume(newLevel);
		}
	}

	private void SetDesiredVolume(CharacterStats.FatigueLevel fatigueLevel)
	{
		if (m_CanHearWhispers)
		{
			float desiredVolume = 0f;
			switch (fatigueLevel)
			{
			case CharacterStats.FatigueLevel.Minor:
				desiredVolume = MinorFatigueVolume;
				break;
			case CharacterStats.FatigueLevel.Major:
				desiredVolume = MajorFatigueVolume;
				break;
			case CharacterStats.FatigueLevel.Critical:
				desiredVolume = CriticalFatigueVolume;
				break;
			}
			if (!m_AudioSource.isPlaying)
			{
				m_AudioSource.loop = true;
				m_AudioSource.volume = 0f;
				m_AudioSource.Play();
				StartCoroutine(PlayRandomWhispers());
			}
			m_DesiredVolume = desiredVolume;
		}
		else
		{
			m_DesiredVolume = 0f;
		}
		m_CurrentFatigueLevel = fatigueLevel;
	}

	private IEnumerator PlayRandomWhispers()
	{
		while (true)
		{
			float volume;
			float pitch;
			AudioClip clip = RandomWhispers.GetClip(forbidImmediateRepeat: true, out volume, out pitch);
			if (Time.timeScale > 0f)
			{
				GlobalAudioPlayer.Instance.PlayOneShot(m_AudioSource, clip, volume * m_AudioSource.volume);
			}
			float delayTime = OEIRandom.RangeInclusive(MinWhisperDelay, MaxWhisperDelay);
			float startTime = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - startTime < delayTime)
			{
				yield return null;
			}
		}
	}

	public void SetVolumeOverride(float volume)
	{
		if (!m_AudioSource)
		{
			return;
		}
		if (volume >= 0f)
		{
			if (!m_AudioSource.isPlaying)
			{
				m_AudioSource.loop = true;
				m_AudioSource.volume = 0f;
				m_AudioSource.Play();
				StartCoroutine(PlayRandomWhispers());
			}
			m_DesiredVolume = volume;
			m_VolumeIsOverridden = true;
		}
		else
		{
			ReleaseVolumeOverride();
		}
	}

	public void ReleaseVolumeOverride()
	{
		if ((bool)m_AudioSource && m_VolumeIsOverridden)
		{
			m_VolumeIsOverridden = false;
			SetDesiredVolume(GameState.s_playerCharacter.GetComponent<CharacterStats>().GetFatigueLevel());
		}
	}
}
