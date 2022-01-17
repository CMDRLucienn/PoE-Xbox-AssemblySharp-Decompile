using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MovieManager : MonoBehaviour
{
	public delegate void MovieStopped();

	public UITexture PlayTexture;

	public AudioSource PlayAudio;

	private bool m_AudioOverride;

	private static MovieManager s_Instance;

	private VideoClip m_LoadedMovie;

	private AudioClip m_LoadedAudio;

	private VideoPlayer videoPlayer;

	private VideoSource videoSource;

	private bool m_Skippable;

	public bool Fade = true;

	private float m_MovieVolume;

	private bool m_PlayStarted;

	public static MovieManager Instance => s_Instance;

	public event MovieStopped OnMovieStopped;

	private void Awake()
	{
		s_Instance = this;
		videoPlayer = base.gameObject.AddComponent<VideoPlayer>();
		videoPlayer.source = VideoSource.VideoClip;
		videoPlayer.clip = m_LoadedMovie;
		videoPlayer.renderMode = VideoRenderMode.APIOnly;
		videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
		videoPlayer.targetMaterialProperty = "_MainTex";
		videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		videoPlayer.playOnAwake = true;
	}

	private void Start()
	{
		if ((bool)PlayAudio)
		{
			PlayAudio.ignoreListenerVolume = true;
		}
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!m_LoadedMovie)
		{
			return;
		}
		PlayTexture.mainTexture = videoPlayer.texture;
		if (videoPlayer.isPlaying && PlayTexture.color != Color.white)
		{
			PlayTexture.color = Color.white;
		}
		if (m_PlayStarted && m_LoadedMovie.frameCount == (ulong)videoPlayer.frame && !videoPlayer.isLooping)
		{
			StopMovie();
		}
		else if ((bool)GameInput.Instance && GameInput.Instance.LastKeyUp.KeyCode != 0 && m_Skippable)
		{
			StopMovie();
		}
		if ((bool)PlayAudio)
		{
			if (!GameState.ApplicationIsFocused)
			{
				PlayAudio.volume = 0f;
			}
			else
			{
				PlayAudio.volume = m_MovieVolume;
			}
		}
	}

	public bool IsMoviePlaying()
	{
		if ((bool)m_LoadedMovie && videoPlayer.isPlaying)
		{
			return m_PlayStarted;
		}
		return false;
	}

	public float GetMovieDuration()
	{
		float result = 0f;
		if (m_LoadedMovie != null)
		{
			result = (float)((double)m_LoadedMovie.frameCount / m_LoadedMovie.frameRate);
		}
		return result;
	}

	private void OnStartFadeEnded()
	{
		StartMovieAudioPlayback();
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnStartFadeEnded));
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0f);
		MusicManager.Instance.MusicEnabled = false;
		MusicManager.Instance.SFXEnabled = false;
	}

	private void OnCloseFadeEnded()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnCloseFadeEnded));
		MusicManager.Instance.MusicEnabled = true;
		MusicManager.Instance.SFXEnabled = true;
	}

	public void StopMovie()
	{
		if (m_LoadedMovie != null)
		{
			if ((bool)FadeManager.Instance && Fade)
			{
				MusicManager.Instance.MusicEnabled = true;
				MusicManager.Instance.SFXEnabled = true;
				FadeManager instance = FadeManager.Instance;
				instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnStartFadeEnded));
				FadeManager instance2 = FadeManager.Instance;
				instance2.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance2.OnFadeEnded, new FadeManager.OnFadeEnd(OnCloseFadeEnded));
				FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 1f);
			}
			videoPlayer?.Stop();
			Resources.UnloadAsset(m_LoadedMovie);
		}
		if ((bool)videoPlayer)
		{
			videoPlayer.clip = null;
		}
		m_LoadedMovie = null;
		m_LoadedAudio = null;
		if (!m_AudioOverride && (bool)PlayAudio)
		{
			PlayAudio.time = 0f;
			PlayAudio.Stop();
			PlayAudio.clip = null;
			PlayAudio.enabled = false;
		}
		PlayTexture.color = Color.black;
		PlayTexture.enabled = false;
		m_PlayStarted = false;
		if (this.OnMovieStopped != null)
		{
			this.OnMovieStopped();
		}
	}

	public void PlayMovieAtPath(string path, bool skippable, AudioClip audio = null)
	{
		m_Skippable = skippable;
		m_LoadedMovie = Resources.Load(path) as VideoClip;
		if (m_LoadedMovie == null)
		{
			throw new FileNotFoundException("The specified movie asset could not be loaded.", path);
		}
		m_AudioOverride = audio;
		m_LoadedAudio = audio;
		if ((bool)PlayAudio)
		{
			PlayAudio.clip = m_LoadedAudio;
			PlayAudio.time = 0f;
		}
		m_MovieVolume = MusicManager.Instance.FinalSfxVolume;
		if (!videoPlayer.isPlaying)
		{
			PlayTexture.color = Color.black;
		}
		videoPlayer.clip = m_LoadedMovie;
		PlayTexture.mainTexture = videoPlayer.texture;
		if ((bool)FadeManager.Instance && Fade)
		{
			if ((bool)PlayAudio)
			{
				PlayAudio.volume = MusicManager.Instance.FinalSfxVolume;
			}
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnStartFadeEnded));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0.5f, AudioFadeMode.MusicAndFx);
		}
		else
		{
			StartMovieAudioPlayback();
		}
	}

	public void Loop(bool state)
	{
		if ((bool)videoPlayer && (bool)m_LoadedMovie)
		{
			videoPlayer.isLooping = state;
		}
		if ((bool)PlayAudio)
		{
			PlayAudio.loop = state;
		}
	}

	private void StartMovieAudioPlayback()
	{
		m_PlayStarted = true;
		if (videoPlayer.clip != null && m_LoadedMovie != null)
		{
			PlayTexture.enabled = true;
			videoPlayer.sendFrameReadyEvents = true;
			videoPlayer.Play();
		}
		if ((bool)PlayAudio)
		{
			PlayAudio.enabled = true;
			PlayAudio.volume = m_MovieVolume;
			PlayAudio.Play();
		}
	}
}
