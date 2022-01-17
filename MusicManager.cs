using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
	public enum FadeType
	{
		FadeOutStart,
		FadeOutPauseStart,
		FadeOutFadeIn,
		FadeOutPauseFadeIn,
		LinearCrossFade,
		LogarithmicCrossFade
	}

	public class FadeParams
	{
		[Tooltip("The type of fade to use when transitioning to new music.")]
		public FadeType FadeType = FadeType.FadeOutPauseFadeIn;

		[Tooltip("The length in seconds to fade out new music.")]
		public float FadeOutDuration = 1f;

		[Tooltip("The length in seconds to fade in new music.")]
		public float FadeInDuration = 1f;

		[Tooltip("The length in seconds to wait between ending fading out and starting fade in of new music.")]
		public float PauseDuration = 0.5f;

		public override string ToString()
		{
			return string.Concat("{ FadeType='", FadeType, "'; FadeOutDuration=", FadeOutDuration, "; FadeInDuration=", FadeInDuration, "; PauseDuration=", PauseDuration, "}");
		}
	}

	private class AudioSourceVolume
	{
		public AudioSource AudioSource;

		public float OriginalVolume = 1f;

		public float StartVolume;
	}

	public delegate void VolumeChanged(SoundCategory cat, float volume);

	public enum SoundCategory
	{
		MASTER,
		MUSIC,
		EFFECTS,
		VOICE,
		COUNT
	}

	public static bool OnscreenDebug;

	[Tooltip("Specifies how long (in seconds) after you exit combat, that the area music be played again.")]
	public float ReplayCombatMusicThreshold = 300f;

	[Tooltip("Specifies how long (in seconds) to wait before restarting area music when the track finishes.")]
	public float LoopAreaMusicDelay = 10f;

	[Tooltip("If transitioning to a map with Play Music On Enter, use this delay instead of Loop Area Music Delay.")]
	public float PlayOnEnterDelay = 1f;

	[Tooltip("The type of fade to use when transitioning to area music.")]
	public FadeType AreaFadeType = FadeType.FadeOutPauseFadeIn;

	[Tooltip("The length in seconds to fade out area music.")]
	public float AreaFadeOutDuration = 1f;

	[Tooltip("The length in seconds to fade in area music.")]
	public float AreaFadeInDuration = 1f;

	[Tooltip("The length in seconds to wait between ending fading out and starting fade in of area music.")]
	public float AreaPauseDuration = 1f;

	[Tooltip("The type of fade to use when transitioning to combat music.")]
	public FadeType CombatFadeType = FadeType.FadeOutPauseFadeIn;

	[Tooltip("The length in seconds to fade out combat music.")]
	public float CombatFadeOutDuration = 1f;

	[Tooltip("The length in seconds to fade in combat music.")]
	public float CombatFadeInDuration = 1f;

	[Tooltip("The length in seconds to wait between ending fading out and starting fade in of combat music.")]
	public float CombatPauseDuration = 1f;

	private float m_Volume = 1f;

	private float m_crossFadeTimer;

	private FadeParams m_fadeParams = new FadeParams();

	private float m_combinedFadeDuration;

	private bool m_fadeOutComplete;

	private float m_allFadeTimer;

	private float m_allFadeDuration = 1f;

	private AudioClip m_pendingClip;

	private bool m_pendingLoops;

	private FadeParams m_pendingFadeParams;

	private int m_pendingScriptedMusicIndex = -1;

	private bool m_pendingScriptedMusicLoop;

	private FadeParams m_areaFadeParams = new FadeParams();

	private FadeParams m_combatFadeParams = new FadeParams();

	private bool m_musicEnabled = true;

	private bool m_sfxEnabled = true;

	[Persistent]
	private int m_scriptedMusicIndex = -1;

	[Persistent]
	private bool m_scriptedMusicLoop;

	private string m_scriptedMusicName = string.Empty;

	[Persistent]
	private bool m_enableLoopCooldown = true;

	private int m_activeSource;

	private AudioSource[] m_AudioSource = new AudioSource[2];

	private VolumeAsCategory[] m_VolumeControllers = new VolumeAsCategory[2];

	private AreaMusic m_AreaMusic;

	private string m_AreaMusicName;

	private float m_loopDelayTimer;

	[Persistent]
	private bool m_blockCombatMusic;

	private float m_combatEndedTimer = 301f;

	private bool m_transitionMusicSetOnLoop;

	private bool m_resetWhenFaded;

	private bool m_fadeLastLevelsMusic;

	private bool m_useFastTransition;

	private List<AudioSourceVolume> m_audioSourceVolumes = new List<AudioSourceVolume>();

	[Tooltip("Similar to AreaMusic.ScriptedMusic, but accessible across all scenes.")]
	public string[] GlobalScriptedMusic;

	private float[] m_OldFinalVolumes = new float[4];

	public static MusicManager Instance { get; private set; }

	public bool MusicEnabled
	{
		get
		{
			return m_musicEnabled;
		}
		set
		{
			m_musicEnabled = value;
		}
	}

	public bool SFXEnabled
	{
		get
		{
			return m_sfxEnabled;
		}
		set
		{
			m_sfxEnabled = value;
		}
	}

	public float FinalMusicVolume => GetFinalVolume(SoundCategory.MUSIC);

	public float FinalSfxVolume => GetFinalVolume(SoundCategory.EFFECTS);

	public event VolumeChanged OnVolumeChanged;

	public void Reset()
	{
		int num = 0;
		while (m_AudioSource != null && num < m_AudioSource.Length)
		{
			if ((bool)m_AudioSource[num])
			{
				m_AudioSource[num].clip = null;
			}
			num++;
		}
		m_AreaMusic = null;
		m_AreaMusicName = "";
		m_pendingClip = null;
		m_resetWhenFaded = false;
		m_transitionMusicSetOnLoop = false;
		m_blockCombatMusic = false;
		m_activeSource = 0;
		m_fadeLastLevelsMusic = false;
		m_enableLoopCooldown = true;
		m_audioSourceVolumes.Clear();
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'MusicManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		GameState.OnLevelLoaded += LevelLoad;
		GameState.OnCombatEnd += CombatEnded;
		GameState.OnCombatStart += CombatStarted;
		GameState.OnLevelUnload += OnLevelUnload;
		m_areaFadeParams.FadeType = AreaFadeType;
		m_areaFadeParams.FadeOutDuration = AreaFadeOutDuration;
		m_areaFadeParams.FadeInDuration = AreaFadeInDuration;
		m_areaFadeParams.PauseDuration = AreaPauseDuration;
		m_combatFadeParams.FadeType = CombatFadeType;
		m_combatFadeParams.FadeOutDuration = CombatFadeOutDuration;
		m_combatFadeParams.FadeInDuration = CombatFadeInDuration;
		m_combatFadeParams.PauseDuration = CombatPauseDuration;
		m_AudioSource[0] = GetComponent<AudioSource>();
		m_Volume = m_AudioSource[0].volume;
		m_AudioSource[0].priority = 0;
		m_AudioSource[1] = base.gameObject.AddComponent<AudioSource>();
		Copy2DAudioSourceSettings(m_AudioSource[0], m_AudioSource[1]);
		for (int i = 0; i < m_AudioSource.Length; i++)
		{
			m_AudioSource[i].ignoreListenerPause = true;
			m_AudioSource[i].spatialBlend = 0f;
			m_VolumeControllers[i] = base.gameObject.AddComponent<VolumeAsCategory>();
			m_VolumeControllers[i].Source = m_AudioSource[i];
			m_VolumeControllers[i].SetCategory(SoundCategory.MUSIC);
		}
	}

	public void Restored()
	{
		m_pendingScriptedMusicIndex = m_scriptedMusicIndex;
		m_pendingScriptedMusicLoop = m_scriptedMusicLoop;
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		m_enableLoopCooldown = true;
		m_blockCombatMusic = false;
		m_audioSourceVolumes.Clear();
		if ((bool)GameState.Instance && GameState.Instance.CurrentMap != null && GameState.Instance.CurrentMap.StopMusicOnExit)
		{
			m_fadeLastLevelsMusic = true;
		}
	}

	private void Copy2DAudioSourceSettings(AudioSource from, AudioSource to)
	{
		to.bypassEffects = from.bypassEffects;
		to.bypassListenerEffects = from.bypassListenerEffects;
		to.bypassReverbZones = from.bypassReverbZones;
		to.playOnAwake = from.playOnAwake;
		to.loop = from.loop;
		to.priority = from.priority;
		to.volume = from.volume;
		to.pitch = from.pitch;
		to.panStereo = from.panStereo;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelLoaded -= LevelLoad;
		GameState.OnCombatEnd -= CombatEnded;
		GameState.OnCombatStart -= CombatStarted;
		GameState.OnLevelUnload -= OnLevelUnload;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private float GetDeltaTime()
	{
		if (Time.unscaledDeltaTime < 0.05f)
		{
			return Time.unscaledDeltaTime;
		}
		return 0.05f;
	}

	public void StopAllMusic()
	{
		m_blockCombatMusic = false;
		for (int i = 0; i < m_AudioSource.Length; i++)
		{
			if ((bool)m_AudioSource[i])
			{
				m_AudioSource[i].Stop();
			}
		}
		m_combinedFadeDuration = 0f;
		m_fadeOutComplete = true;
	}

	private void SetAllFadeTimer(float fadeTimer)
	{
		m_allFadeTimer = fadeTimer;
		m_allFadeDuration = m_allFadeTimer;
		m_combinedFadeDuration = 0f;
		m_fadeOutComplete = true;
	}

	public void FadeOutAreaMusic(bool resetWhenFaded)
	{
		if (m_AudioSource.Length > m_activeSource && m_AudioSource[m_activeSource] != null)
		{
			SetAllFadeTimer(1f);
			m_resetWhenFaded = resetWhenFaded;
		}
	}

	public void FadeAmbientAudioIn(float fadeTime)
	{
		AmbientSound[] array = UnityEngine.Object.FindObjectsOfType<AmbientSound>();
		if (array == null || array.Length == 0)
		{
			return;
		}
		AmbientSound[] array2 = array;
		foreach (AmbientSound ambientSound in array2)
		{
			if (ambientSound.gameObject.activeInHierarchy)
			{
				FadeAllAudioSourcesIn(ambientSound.gameObject, fadeTime);
			}
		}
	}

	public void FadeAmbientAudioOut(float fadeTime)
	{
		AmbientSound[] array = UnityEngine.Object.FindObjectsOfType<AmbientSound>();
		if (array == null || array.Length == 0)
		{
			return;
		}
		AmbientSound[] array2 = array;
		foreach (AmbientSound ambientSound in array2)
		{
			if (ambientSound.gameObject.activeInHierarchy)
			{
				FadeAllAudioSourcesOut(ambientSound.gameObject, fadeTime);
			}
		}
	}

	public void FadeAllAudioSourcesIn(GameObject parent, float fadeTime)
	{
		AudioSource[] componentsInChildren = parent.GetComponentsInChildren<AudioSource>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		List<AudioSourceVolume> list = new List<AudioSourceVolume>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			AudioSource audioSource = componentsInChildren[i];
			if (audioSource == m_AudioSource[0] || audioSource == m_AudioSource[1])
			{
				continue;
			}
			AudioSourceVolume audioSourceVolume = null;
			for (int j = 0; j < m_audioSourceVolumes.Count; j++)
			{
				if (m_audioSourceVolumes[j].AudioSource == audioSource)
				{
					audioSourceVolume = m_audioSourceVolumes[j];
					break;
				}
			}
			if (audioSourceVolume == null)
			{
				audioSourceVolume = new AudioSourceVolume();
				audioSourceVolume.AudioSource = componentsInChildren[i];
				if (audioSource.volume > 0f)
				{
					audioSourceVolume.OriginalVolume = audioSource.volume;
				}
				else
				{
					audioSourceVolume.OriginalVolume = 1f;
				}
				m_audioSourceVolumes.Add(audioSourceVolume);
			}
			audioSourceVolume.StartVolume = audioSourceVolume.AudioSource.volume;
			list.Add(audioSourceVolume);
		}
		StartCoroutine(UpdateFadeIn(fadeTime, list));
	}

	public void FadeAllAudioSourcesOut(GameObject parent, float fadeTime)
	{
		AudioSource[] componentsInChildren = parent.GetComponentsInChildren<AudioSource>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		List<AudioSourceVolume> list = new List<AudioSourceVolume>();
		foreach (AudioSource audioSource in componentsInChildren)
		{
			AudioSourceVolume audioSourceVolume = null;
			for (int j = 0; j < m_audioSourceVolumes.Count; j++)
			{
				if (m_audioSourceVolumes[j].AudioSource == audioSource)
				{
					audioSourceVolume = m_audioSourceVolumes[j];
					break;
				}
			}
			if (audioSourceVolume == null)
			{
				audioSourceVolume = new AudioSourceVolume();
				audioSourceVolume.AudioSource = audioSource;
				audioSourceVolume.OriginalVolume = audioSource.volume;
				m_audioSourceVolumes.Add(audioSourceVolume);
			}
			audioSourceVolume.StartVolume = audioSourceVolume.AudioSource.volume;
			list.Add(audioSourceVolume);
		}
		StartCoroutine(UpdateFadeOut(fadeTime, list));
	}

	private IEnumerator UpdateFadeIn(float fadeTime, List<AudioSourceVolume> audioSourceVolumes)
	{
		if (fadeTime <= 0f)
		{
			fadeTime = 0.1f;
		}
		float timer = 0f;
		while (timer < fadeTime)
		{
			foreach (AudioSourceVolume audioSourceVolume in audioSourceVolumes)
			{
				if (audioSourceVolume.AudioSource != null)
				{
					audioSourceVolume.AudioSource.volume = timer / fadeTime * audioSourceVolume.OriginalVolume;
				}
			}
			timer += GetDeltaTime();
			yield return null;
		}
		foreach (AudioSourceVolume audioSourceVolume2 in audioSourceVolumes)
		{
			if (audioSourceVolume2.AudioSource != null)
			{
				audioSourceVolume2.AudioSource.volume = audioSourceVolume2.OriginalVolume;
			}
		}
	}

	private IEnumerator UpdateFadeOut(float fadeTime, List<AudioSourceVolume> audioSourceVolumes)
	{
		if (fadeTime <= 0f)
		{
			fadeTime = 0.1f;
		}
		float timer = fadeTime;
		while (timer > 0f)
		{
			foreach (AudioSourceVolume audioSourceVolume in audioSourceVolumes)
			{
				if (audioSourceVolume.AudioSource != null)
				{
					audioSourceVolume.AudioSource.volume = timer / fadeTime * audioSourceVolume.StartVolume;
				}
			}
			timer -= GetDeltaTime();
			yield return null;
		}
		foreach (AudioSourceVolume audioSourceVolume2 in audioSourceVolumes)
		{
			if (audioSourceVolume2.AudioSource != null)
			{
				audioSourceVolume2.AudioSource.volume = 0f;
			}
		}
	}

	public void PlayScriptedMusic(string clipName, bool blockCombatMusic, FadeType fadeType, float fadeOutDuration, float fadeInDuration, float pauseDuration, bool loop)
	{
		if (m_AreaMusic == null)
		{
			InitMusic();
		}
		if (!m_AreaMusic || m_AreaMusic.ScriptedMusic == null)
		{
			Debug.LogError("PlayerScriptedMusic Error: m_AreaMusic or m_AreaMusic.ScriptedMusic is null!");
			return;
		}
		for (int i = 0; i < m_AreaMusic.ScriptedMusic.Length; i++)
		{
			if (m_AreaMusic.ScriptedMusic[i].ToLowerInvariant().EndsWith(clipName.ToLowerInvariant()))
			{
				if (string.Compare(m_scriptedMusicName, m_AreaMusic.ScriptedMusic[i].ToLowerInvariant(), ignoreCase: true) != 0)
				{
					m_fadeParams.FadeType = fadeType;
					m_fadeParams.FadeOutDuration = fadeOutDuration;
					m_fadeParams.FadeInDuration = fadeInDuration;
					m_fadeParams.PauseDuration = pauseDuration;
					PlayMusic(m_AreaMusic.ScriptedMusicClips[i], m_fadeParams, loop);
				}
				m_blockCombatMusic = blockCombatMusic;
				m_scriptedMusicIndex = i;
				m_scriptedMusicLoop = loop;
				m_scriptedMusicName = m_AreaMusic.ScriptedMusic[i].ToLowerInvariant();
				m_transitionMusicSetOnLoop = false;
				m_allFadeTimer = 0f;
				break;
			}
		}
	}

	public void ResumeScriptedOrNormalMusic(bool resumeActiveSource)
	{
		int num = m_activeSource;
		if (!resumeActiveSource)
		{
			num = 1 - m_activeSource;
		}
		if (!(m_AreaMusic != null))
		{
			return;
		}
		if (m_combatEndedTimer > ReplayCombatMusicThreshold || m_scriptedMusicIndex >= 0)
		{
			if (m_scriptedMusicIndex >= 0)
			{
				if (m_AreaMusic.ScriptedMusicClips.Length > m_scriptedMusicIndex && (m_AudioSource[num] == null || m_AreaMusic.ScriptedMusicClips[m_scriptedMusicIndex] != m_AudioSource[num].clip || !m_AudioSource[num].isPlaying))
				{
					PlayMusic(m_AreaMusic.ScriptedMusicClips[m_scriptedMusicIndex], m_areaFadeParams, m_scriptedMusicLoop);
					m_crossFadeTimer = 0f;
				}
				else
				{
					m_combinedFadeDuration = 0f;
				}
				return;
			}
			if (m_transitionMusicSetOnLoop)
			{
				UpdateMusicSetToCurrentArea(startMusic: false);
			}
			if (m_AudioSource[num] == null || m_AreaMusic.NormalTrack != m_AudioSource[num].clip || !m_AudioSource[num].isPlaying)
			{
				PlayMusic(m_AreaMusic.NormalTrack, m_areaFadeParams, loop: false);
				m_crossFadeTimer = 0f;
			}
			else
			{
				m_combinedFadeDuration = 0f;
			}
		}
		else if (m_AudioSource[num] != null)
		{
			if (m_transitionMusicSetOnLoop)
			{
				UpdateMusicSetToCurrentArea(startMusic: false);
			}
			if (m_AudioSource[num] == null || m_AreaMusic.NormalTrack != m_AudioSource[num].clip || !m_AudioSource[num].isPlaying)
			{
				m_crossFadeTimer = 0f;
				PlayMusic(m_AreaMusic.NormalTrack, m_areaFadeParams, loop: false);
			}
			else
			{
				m_combinedFadeDuration = 0f;
			}
		}
	}

	public void PlayNormalMusic()
	{
		m_scriptedMusicIndex = -1;
		m_scriptedMusicLoop = false;
		m_scriptedMusicName = string.Empty;
		m_blockCombatMusic = false;
		PlayMusic(m_AreaMusic.NormalTrack, m_areaFadeParams, loop: false);
	}

	public void EndScriptedMusic()
	{
		if (m_scriptedMusicIndex >= 0)
		{
			PlayNormalMusic();
		}
		else
		{
			ResumeScriptedOrNormalMusic(resumeActiveSource: true);
		}
	}

	public void PlayMusic(AudioClip clip, FadeParams fadeParams)
	{
		PlayMusic(clip, fadeParams, 0f, loop: false);
	}

	public void PlayMusic(AudioClip clip, FadeParams fadeParams, float seekTime)
	{
		PlayMusic(clip, fadeParams, seekTime, loop: false);
	}

	public void PlayMusic(AudioClip clip, FadeParams fadeParams, bool loop)
	{
		PlayMusic(clip, fadeParams, 0f, loop);
	}

	public void PlayNormalMusicForPending()
	{
		m_scriptedMusicIndex = -1;
		m_scriptedMusicLoop = false;
		m_scriptedMusicName = string.Empty;
		m_blockCombatMusic = false;
		m_pendingClip = m_AreaMusic.NormalTrack;
		m_pendingFadeParams = m_areaFadeParams;
		m_pendingLoops = false;
	}

	public void PlayMusic(AudioClip clip, FadeParams fadeParams, float seekTime, bool loop)
	{
		if (clip == null)
		{
			return;
		}
		if (clip.loadState != AudioDataLoadState.Loaded && clip.loadType != AudioClipLoadType.Streaming)
		{
			m_pendingClip = clip;
			m_pendingLoops = loop;
			m_pendingFadeParams = fadeParams;
			return;
		}
		m_pendingClip = null;
		m_pendingLoops = false;
		m_pendingFadeParams = null;
		m_activeSource = 1 - m_activeSource;
		if (m_AudioSource[m_activeSource] != null && m_AudioSource[m_activeSource].isPlaying && m_AudioSource[1 - m_activeSource] != null && !m_AudioSource[1 - m_activeSource].isPlaying)
		{
			m_activeSource = 1 - m_activeSource;
		}
		if (m_AudioSource[m_activeSource] == null)
		{
			m_AudioSource[m_activeSource] = base.gameObject.AddComponent<AudioSource>();
			m_AudioSource[m_activeSource].ignoreListenerVolume = true;
			Debug.LogWarning("Audio source " + m_activeSource + " had to be initialized on the fly!", base.gameObject);
		}
		m_AudioSource[m_activeSource].clip = clip;
		if (clip == null || seekTime >= clip.length)
		{
			seekTime = 0f;
		}
		m_AudioSource[m_activeSource].time = seekTime;
		m_AudioSource[m_activeSource].loop = loop;
		m_loopDelayTimer = LoopAreaMusicDelay;
		InitFading(fadeParams.FadeType, fadeParams.FadeOutDuration, fadeParams.FadeInDuration, fadeParams.PauseDuration);
		if (m_useFastTransition)
		{
			m_loopDelayTimer = PlayOnEnterDelay;
			m_useFastTransition = false;
		}
	}

	private void InitFading(FadeType fadeType, float fadeOutDuration, float fadeInDuration, float pauseDuration)
	{
		if (fadeOutDuration < 0f || fadeInDuration < 0f || pauseDuration < 0f)
		{
			m_fadeParams.FadeType = fadeType;
			m_fadeParams.FadeOutDuration = 0f;
			m_fadeParams.FadeInDuration = 0f;
			m_fadeParams.PauseDuration = 0f;
			m_combinedFadeDuration = 0f;
			return;
		}
		m_fadeParams.FadeType = fadeType;
		m_fadeParams.FadeOutDuration = fadeOutDuration;
		m_fadeParams.FadeInDuration = fadeInDuration;
		m_fadeParams.PauseDuration = pauseDuration;
		m_crossFadeTimer = 0f;
		m_fadeOutComplete = false;
		switch (fadeType)
		{
		case FadeType.FadeOutStart:
			m_combinedFadeDuration = m_fadeParams.FadeOutDuration;
			break;
		case FadeType.FadeOutFadeIn:
		case FadeType.LinearCrossFade:
		case FadeType.LogarithmicCrossFade:
			m_combinedFadeDuration = m_fadeParams.FadeOutDuration + m_fadeParams.FadeInDuration;
			break;
		case FadeType.FadeOutPauseStart:
			m_combinedFadeDuration = m_fadeParams.FadeOutDuration + m_fadeParams.PauseDuration;
			break;
		case FadeType.FadeOutPauseFadeIn:
			m_combinedFadeDuration = m_fadeParams.FadeOutDuration + m_fadeParams.FadeInDuration + m_fadeParams.PauseDuration;
			break;
		}
		if (m_AudioSource[1 - m_activeSource] == null || !m_AudioSource[1 - m_activeSource].isPlaying || (m_VolumeControllers[1 - m_activeSource] != null && m_VolumeControllers[1 - m_activeSource].ExternalVolume < 0.15f))
		{
			m_crossFadeTimer = m_fadeParams.FadeOutDuration;
		}
		if (m_combinedFadeDuration <= Mathf.Epsilon)
		{
			m_crossFadeTimer = 1f;
		}
		m_allFadeTimer = 0f;
	}

	public void PlayCombatMusic(AreaMusic areaMusic, bool loop)
	{
		if (areaMusic.CombatTrack != null)
		{
			PlayMusic(areaMusic.CombatTrack, m_combatFadeParams, loop: true);
		}
	}

	public void RollBackMusic()
	{
		m_blockCombatMusic = false;
		int num = 1 - m_activeSource;
		if (m_AudioSource.Length > num && m_AudioSource[num] != null && m_AudioSource[num].clip != null)
		{
			PlayMusic(m_AudioSource[num].clip, m_fadeParams);
		}
	}

	private void CombatStarted(object sender, EventArgs e)
	{
		if (!m_blockCombatMusic && !GameState.IsInTrapTriggeredCombat && (bool)m_AreaMusic && m_AudioSource[m_activeSource] != null && m_AudioSource[m_activeSource].clip != m_AreaMusic.CombatTrack)
		{
			if (m_scriptedMusicIndex == -1)
			{
				m_AreaMusic.NormalSeekTime = m_AudioSource[m_activeSource].time;
			}
			PlayCombatMusic(m_AreaMusic, loop: true);
		}
	}

	private void CombatEnded(object sender, EventArgs e)
	{
		if (!m_blockCombatMusic && !GameState.IsInTrapTriggeredCombat)
		{
			ResumeScriptedOrNormalMusic(resumeActiveSource: false);
		}
	}

	private void LevelLoad(object sender, EventArgs e)
	{
		m_resetWhenFaded = false;
		m_scriptedMusicIndex = -1;
		m_scriptedMusicLoop = false;
		m_scriptedMusicName = string.Empty;
		if (m_pendingScriptedMusicIndex >= 0)
		{
			m_scriptedMusicIndex = m_pendingScriptedMusicIndex;
			m_scriptedMusicLoop = m_pendingScriptedMusicLoop;
			m_pendingScriptedMusicIndex = -1;
			m_pendingScriptedMusicLoop = false;
		}
		if (GameState.Instance.CurrentMap == null)
		{
			return;
		}
		if (m_AreaMusic == null)
		{
			InitMusic();
		}
		if (GameState.Instance.CurrentMap != null && GameState.Instance.CurrentMap.PlayMusicOnEnter)
		{
			SetAllFadeTimer(1.5f);
			m_transitionMusicSetOnLoop = true;
			m_useFastTransition = true;
		}
		else if (m_AreaMusic != null && !GameState.LoadedGame && !m_fadeLastLevelsMusic)
		{
			m_fadeLastLevelsMusic = false;
			if (GameState.Instance.CurrentMap.MusicSet != null && (!GameState.Instance.CurrentMap.PlayMusicOnEnter || m_AreaMusicName == GameState.Instance.CurrentMap.MusicSet.name))
			{
				if (m_scriptedMusicIndex >= 0)
				{
					PlayNormalMusicForPending();
				}
				if (m_AreaMusicName != GameState.Instance.CurrentMap.MusicSet.name)
				{
					m_transitionMusicSetOnLoop = true;
				}
				return;
			}
		}
		else if (m_AreaMusic != null && GameState.NumSceneLoads == 0)
		{
			if (m_scriptedMusicIndex < 0)
			{
				PlayNormalMusicForPending();
			}
			else if (m_AreaMusic.ScriptedMusicClips.Length <= m_scriptedMusicIndex)
			{
				m_scriptedMusicIndex = -1;
				PlayNormalMusicForPending();
			}
			else
			{
				PlayMusic(m_AreaMusic.ScriptedMusicClips[m_scriptedMusicIndex], m_areaFadeParams, m_scriptedMusicLoop);
			}
			SetAllFadeTimer(0f);
		}
		else
		{
			SetAllFadeTimer(1.5f);
			m_transitionMusicSetOnLoop = true;
		}
		m_fadeLastLevelsMusic = false;
	}

	private void InitMusic()
	{
		if (GameState.Instance.CurrentMap.MusicSet != null)
		{
			m_AreaMusic = UnityEngine.Object.Instantiate(GameState.Instance.CurrentMap.MusicSet);
			m_AreaMusicName = GameState.Instance.CurrentMap.MusicSet.name;
			if ((bool)m_AreaMusic)
			{
				m_AreaMusic.LoadMusic();
			}
			else
			{
				Debug.Log("Area music null for " + GameState.Instance.CurrentMap.SceneName);
			}
			ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnLevelMusicLoaded);
		}
		else
		{
			m_AreaMusic = null;
			Debug.LogWarning(GameState.Instance.CurrentMap.SceneName + " is missing it's area music.");
		}
	}

	public void UnloadMusic()
	{
		if (m_AreaMusic != null)
		{
			m_AreaMusic.UnloadMusic();
			GameUtilities.Destroy(m_AreaMusic);
			m_AreaMusic = null;
			m_AreaMusicName = "";
		}
	}

	private void UpdateMusicSetToCurrentArea(bool startMusic)
	{
		m_resetWhenFaded = false;
		m_transitionMusicSetOnLoop = false;
		int scriptedMusicIndex = m_scriptedMusicIndex;
		UnloadMusic();
		InitMusic();
		if ((bool)m_AreaMusic && (bool)m_AreaMusic.NormalTrack && GameState.Instance.CurrentMap.PlayMusicOnEnter && startMusic && (m_scriptedMusicIndex < 0 || m_scriptedMusicIndex != scriptedMusicIndex))
		{
			ResumeScriptedOrNormalMusic(resumeActiveSource: true);
		}
	}

	private void Update()
	{
		if (OnscreenDebug)
		{
			DrawDebugInfo();
		}
		if (m_AudioSource == null || m_AudioSource.Length == 0)
		{
			return;
		}
		if (m_combatEndedTimer <= ReplayCombatMusicThreshold)
		{
			m_combatEndedTimer += GetDeltaTime();
		}
		if (m_crossFadeTimer < m_combinedFadeDuration || !m_fadeOutComplete)
		{
			UpdateCrossFade();
		}
		else if (m_allFadeTimer > 0f)
		{
			VolumeAsCategory[] volumeControllers = m_VolumeControllers;
			foreach (VolumeAsCategory volumeAsCategory in volumeControllers)
			{
				if (volumeAsCategory != null)
				{
					volumeAsCategory.ExternalVolume = m_allFadeTimer / m_allFadeDuration;
				}
			}
			m_allFadeTimer -= GetDeltaTime();
			if (m_allFadeTimer <= 0f)
			{
				AudioSource[] audioSource = m_AudioSource;
				foreach (AudioSource audioSource2 in audioSource)
				{
					if (audioSource2 != null && audioSource2.isPlaying)
					{
						audioSource2.Stop();
					}
				}
				if (m_resetWhenFaded)
				{
					Reset();
				}
				else if (m_transitionMusicSetOnLoop)
				{
					UpdateMusicSetToCurrentArea(startMusic: true);
				}
			}
		}
		AudioListener.volume = FinalSfxVolume;
		for (SoundCategory soundCategory = SoundCategory.MASTER; soundCategory < SoundCategory.COUNT; soundCategory++)
		{
			float finalVolume = GetFinalVolume(soundCategory);
			if (Math.Abs(finalVolume - m_OldFinalVolumes[(int)soundCategory]) > 0.001f)
			{
				m_OldFinalVolumes[(int)soundCategory] = finalVolume;
				if (this.OnVolumeChanged != null)
				{
					this.OnVolumeChanged(soundCategory, finalVolume);
				}
			}
		}
		if (m_pendingClip != null && (m_pendingClip.loadType == AudioClipLoadType.Streaming || m_pendingClip.loadState == AudioDataLoadState.Loaded))
		{
			PlayMusic(m_pendingClip, m_pendingFadeParams, m_pendingLoops);
		}
		if (GameState.IsLoading || !(m_AudioSource[m_activeSource] != null) || m_AudioSource[m_activeSource].loop || m_AudioSource[m_activeSource].isPlaying)
		{
			return;
		}
		if (m_loopDelayTimer > 0f && m_enableLoopCooldown)
		{
			m_loopDelayTimer -= GetDeltaTime();
			return;
		}
		m_loopDelayTimer = 0f;
		m_blockCombatMusic = false;
		if (m_scriptedMusicIndex >= 0)
		{
			if (m_AreaMusic == null)
			{
				Debug.LogError("Areamusic is null when returning to normal music");
			}
			else if (m_AreaMusic.ScriptedMusicClips == null)
			{
				Debug.LogError("ScriptedMusicClips array is null when returning to normal music");
			}
			else if (m_AreaMusic.ScriptedMusicClips.Length <= m_scriptedMusicIndex)
			{
				m_scriptedMusicIndex = -1;
				PlayNormalMusic();
			}
			else
			{
				PlayMusic(m_AreaMusic.ScriptedMusicClips[m_scriptedMusicIndex], m_areaFadeParams, m_scriptedMusicLoop);
			}
		}
		else if (m_transitionMusicSetOnLoop)
		{
			UpdateMusicSetToCurrentArea(startMusic: true);
		}
		else if (m_AreaMusic != null)
		{
			PlayMusic(m_AreaMusic.NormalTrack, m_areaFadeParams, loop: false);
		}
	}

	private void DrawDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("===== MusicManager =====");
		stringBuilder.AppendLine("m_activeSource: " + m_activeSource);
		stringBuilder.AppendLine("m_scriptedMusicIndex: " + m_scriptedMusicIndex);
		stringBuilder.AppendLine("has m_pendingClip: " + (bool)m_pendingClip);
		stringBuilder.AppendLine("m_allFadeTimer: " + m_allFadeTimer);
		stringBuilder.AppendLine("m_allFadeDuration: " + m_allFadeDuration);
		stringBuilder.AppendLine("m_crossFadeTimer: " + m_crossFadeTimer);
		stringBuilder.AppendLine("m_fadeParams: " + m_fadeParams);
		stringBuilder.AppendLine("m_combinedFadeDuration: " + m_combinedFadeDuration);
		stringBuilder.AppendLine("m_fadeOutComplete: " + m_fadeOutComplete);
		UIDebug.Instance.SetText("MusicManager Debug", stringBuilder.ToString(), Color.cyan);
		UIDebug.Instance.SetTextPosition("MusicManager Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
	}

	private void UpdateCrossFade()
	{
		float alphaFadeOut = 0f;
		float alphaFadeIn = 0f;
		m_crossFadeTimer += GetDeltaTime();
		if (m_crossFadeTimer > m_combinedFadeDuration || m_combinedFadeDuration < Mathf.Epsilon)
		{
			m_crossFadeTimer = m_combinedFadeDuration;
			alphaFadeOut = 0f;
			alphaFadeIn = 1f;
		}
		else
		{
			GetCrossFadeAlphas(out alphaFadeOut, out alphaFadeIn);
		}
		if (alphaFadeOut < float.Epsilon)
		{
			StopFadedTrack();
		}
		if (m_VolumeControllers[1 - m_activeSource] != null)
		{
			m_VolumeControllers[1 - m_activeSource].ExternalVolume = alphaFadeOut;
		}
		if (!(m_AudioSource[m_activeSource] != null))
		{
			return;
		}
		if (alphaFadeIn > Mathf.Epsilon && !m_AudioSource[m_activeSource].isPlaying)
		{
			try
			{
				if ((bool)m_AudioSource[m_activeSource].clip)
				{
					Debug.Log("Playing Music Clip: '" + m_AudioSource[m_activeSource].clip.name + "'.\n");
					m_AudioSource[m_activeSource].Play();
				}
				else
				{
					Debug.LogWarning("Attempting to play music audio source with no clip.\n");
				}
			}
			catch (Exception ex)
			{
				if ((bool)m_AudioSource[m_activeSource].clip)
				{
					Debug.LogError("Can't play music: '" + m_AudioSource[m_activeSource].clip.name + "'\n" + ex.Message);
				}
				else
				{
					Debug.LogError("Can't play music audio source with no clip.\n" + ex.Message);
				}
			}
		}
		if (m_VolumeControllers[m_activeSource] != null)
		{
			m_VolumeControllers[m_activeSource].ExternalVolume = alphaFadeIn;
		}
	}

	private void GetCrossFadeAlphas(out float alphaFadeOut, out float alphaFadeIn)
	{
		alphaFadeOut = 0f;
		alphaFadeIn = 0f;
		switch (m_fadeParams.FadeType)
		{
		case FadeType.FadeOutStart:
			if (m_crossFadeTimer < m_combinedFadeDuration)
			{
				alphaFadeOut = 1f - m_crossFadeTimer / m_combinedFadeDuration;
				alphaFadeIn = 0f;
			}
			else
			{
				alphaFadeOut = 0f;
				alphaFadeIn = 1f;
			}
			break;
		case FadeType.FadeOutPauseStart:
			if (m_crossFadeTimer < m_fadeParams.FadeOutDuration)
			{
				alphaFadeOut = 1f - m_crossFadeTimer / m_fadeParams.FadeOutDuration;
				alphaFadeIn = 0f;
			}
			else if (m_crossFadeTimer < m_combinedFadeDuration)
			{
				alphaFadeOut = 0f;
				alphaFadeIn = 0f;
			}
			else
			{
				alphaFadeOut = 0f;
				alphaFadeIn = 1f;
			}
			break;
		case FadeType.FadeOutFadeIn:
			if (m_crossFadeTimer < m_fadeParams.FadeOutDuration)
			{
				alphaFadeOut = 1f - m_crossFadeTimer / m_fadeParams.FadeOutDuration;
				alphaFadeIn = 0f;
			}
			else
			{
				alphaFadeOut = 0f;
				alphaFadeIn = (m_crossFadeTimer - m_fadeParams.FadeOutDuration) / m_fadeParams.FadeInDuration;
			}
			break;
		case FadeType.FadeOutPauseFadeIn:
			if (m_crossFadeTimer < m_fadeParams.FadeOutDuration)
			{
				alphaFadeOut = 1f - m_crossFadeTimer / m_fadeParams.FadeOutDuration;
				alphaFadeIn = 0f;
			}
			else if (m_crossFadeTimer < m_fadeParams.FadeOutDuration + m_fadeParams.PauseDuration)
			{
				alphaFadeOut = 0f;
				alphaFadeIn = 0f;
			}
			else
			{
				alphaFadeOut = 0f;
				alphaFadeIn = (m_crossFadeTimer - m_fadeParams.FadeOutDuration - m_fadeParams.PauseDuration) / m_fadeParams.FadeInDuration;
			}
			break;
		case FadeType.LinearCrossFade:
			alphaFadeOut = 1f - m_crossFadeTimer / m_fadeParams.FadeOutDuration;
			alphaFadeIn = m_crossFadeTimer / m_fadeParams.FadeInDuration;
			break;
		case FadeType.LogarithmicCrossFade:
			alphaFadeOut = m_crossFadeTimer / m_fadeParams.FadeOutDuration;
			alphaFadeOut = 1f - alphaFadeOut * alphaFadeOut;
			alphaFadeIn = m_crossFadeTimer / m_fadeParams.FadeInDuration;
			alphaFadeIn *= alphaFadeIn;
			break;
		}
		alphaFadeIn = Mathf.Clamp01(alphaFadeIn);
		alphaFadeOut = Mathf.Clamp01(alphaFadeOut);
	}

	private void StopFadedTrack()
	{
		if (!m_fadeOutComplete)
		{
			if (m_AudioSource[1 - m_activeSource] != null)
			{
				m_AudioSource[1 - m_activeSource].Stop();
			}
			m_fadeOutComplete = true;
		}
	}

	public void EnableLoopCooldown()
	{
		m_enableLoopCooldown = true;
	}

	public void DisableLoopCooldown()
	{
		m_enableLoopCooldown = false;
	}

	private bool IsSourcePlaying()
	{
		if (m_AudioSource != null)
		{
			AudioSource[] audioSource = m_AudioSource;
			foreach (AudioSource audioSource2 in audioSource)
			{
				if (audioSource2 != null && audioSource2.isPlaying)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetFinalVolume(SoundCategory category)
	{
		if (!GameState.ApplicationIsFocused)
		{
			return 0f;
		}
		switch (category)
		{
		case SoundCategory.MUSIC:
			if (!MusicEnabled)
			{
				return 0f;
			}
			if ((bool)FadeManager.Instance && FadeManager.Instance.FadeMusic && FadeManager.Instance.IsFadeActive())
			{
				return m_Volume * GameState.Option.GetVolume(category) * (1f - FadeManager.Instance.FadeValue);
			}
			if ((bool)UIOptionsManager.Instance && UIOptionsManager.Instance.WindowActive())
			{
				return m_Volume * UIOptionsManager.Instance.GetVolumeSetting(category);
			}
			return m_Volume * GameState.Option.GetVolume(category);
		case SoundCategory.EFFECTS:
			if (!SFXEnabled)
			{
				return 0f;
			}
			break;
		}
		if ((bool)UIOptionsManager.Instance && UIOptionsManager.Instance.WindowActive())
		{
			return UIOptionsManager.Instance.GetVolumeSetting(category);
		}
		if (GameState.IsLoading)
		{
			return 0f;
		}
		float num = 1f;
		if ((bool)FadeManager.Instance && FadeManager.Instance.IsFadeActive() && (FadeManager.Instance.CurrentFadeType == FadeManager.FadeType.AreaTransition || FadeManager.Instance.FadeAudio))
		{
			num = 1f - FadeManager.Instance.FadeValue;
		}
		return GameState.Option.GetVolume(category) * num;
	}
}
