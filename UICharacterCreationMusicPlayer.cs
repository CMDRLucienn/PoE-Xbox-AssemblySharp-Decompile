using System.IO;
using UnityEngine;

public class UICharacterCreationMusicPlayer : MonoBehaviour
{
	public string MusicClip;

	public AudioSource MusicSource;

	private float m_listenerVolumeBefore;

	private AudioClip m_musicClip;

	private float MusicVolume
	{
		get
		{
			if (!MusicManager.Instance)
			{
				return GameState.Option.GetVolume(MusicManager.SoundCategory.MUSIC);
			}
			return MusicManager.Instance.FinalMusicVolume;
		}
	}

	private float SoundVolume
	{
		get
		{
			if (!MusicManager.Instance)
			{
				return GameState.Option.GetVolume(MusicManager.SoundCategory.EFFECTS);
			}
			return MusicManager.Instance.FinalSfxVolume;
		}
	}

	private void Awake()
	{
		string path = Path.Combine("Audio/mus", MusicClip.ToLowerInvariant()).Replace('\\', '/');
		m_musicClip = Resources.Load<AudioClip>(path);
		m_musicClip.name = "mus_global_character_creation";
	}

	private void OnDestroy()
	{
		if (m_musicClip != null)
		{
			Resources.UnloadAsset(m_musicClip);
		}
	}

	private void Update()
	{
		if ((bool)MusicSource && MusicSource.isPlaying)
		{
			MusicSource.volume = MusicVolume;
		}
	}

	public void PlayMusic(UICharacterCreationManager.CharacterCreationType type)
	{
		if (type == UICharacterCreationManager.CharacterCreationType.NewPlayer)
		{
			MusicManager.Instance.FadeOutAreaMusic(resetWhenFaded: false);
			AudioFadeMode audioFadeMode = FadeManager.Instance.AudioFadeMode;
			FadeManager.Instance.AudioFadeMode = AudioFadeMode.None;
			GlobalAudioPlayer.Instance.AudioSource.ignoreListenerVolume = true;
			GlobalAudioPlayer.Instance.AudioSource.volume = SoundVolume;
			MusicSource.clip = m_musicClip;
			MusicSource.volume = MusicVolume;
			MusicSource.loop = true;
			MusicSource.ignoreListenerVolume = true;
			MusicSource.Play();
			m_listenerVolumeBefore = AudioListener.volume;
			AudioListener.volume = 0f;
			FadeManager.Instance.AudioFadeMode = audioFadeMode;
		}
	}

	public void StopMusic(UICharacterCreationManager.CharacterCreationType type)
	{
		if (type == UICharacterCreationManager.CharacterCreationType.NewPlayer)
		{
			GlobalAudioPlayer.Instance.AudioSource.ignoreListenerVolume = false;
			GlobalAudioPlayer.Instance.AudioSource.volume = 1f;
			MusicSource.ignoreListenerVolume = false;
			MusicSource.Stop();
			MusicSource.clip = null;
			AudioListener.volume = m_listenerVolumeBefore;
		}
	}
}
