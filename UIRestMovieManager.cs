using UnityEngine;

public class UIRestMovieManager : UIHudWindow
{
	public MovieManager MovieMan;

	[ResourcesImageProperty]
	public string InnMovie;

	public AudioClip InnAudio;

	[ResourcesImageProperty]
	public string CampMovie;

	public AudioClip CampAudio;

	[ResourcesImageProperty]
	public string WatcherMovie;

	public AudioClip WatcherAudio;

	private AudioSource audioSource;

	[HideInInspector]
	public RestMovieMode ForMode = RestMovieMode.Camp;

	private bool m_Initialized;

	public static UIRestMovieManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Initialize();
	}

	private void Initialize()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			try
			{
				audioSource = GetComponent<AudioSource>();
				audioSource.spatialBlend = 0f;
				audioSource.reverbZoneMix = 0f;
			}
			catch
			{
			}
			MovieMan.OnMovieStopped += OnMovieStopped;
		}
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)MovieMan)
		{
			MovieMan.OnMovieStopped -= OnMovieStopped;
		}
		base.OnDestroy();
	}

	private void OnMovieStopped()
	{
		HideWindow();
	}

	private void Play(string str, AudioClip audio)
	{
		Initialize();
		if (!string.IsNullOrEmpty(str))
		{
			MovieMan.PlayMovieAtPath(str, skippable: true, audio);
		}
		else
		{
			HideWindow();
		}
	}

	protected override void Show()
	{
		switch (ForMode)
		{
		case RestMovieMode.Inn:
			Play(InnMovie, InnAudio);
			break;
		case RestMovieMode.Camp:
			Play(CampMovie, CampAudio);
			break;
		case RestMovieMode.Watcher:
			Play(WatcherMovie, WatcherAudio);
			break;
		default:
			HideWindow();
			ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnRestFinished);
			break;
		}
	}

	protected override bool Hide(bool forced)
	{
		ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnRestFinished);
		return base.Hide(forced);
	}
}
