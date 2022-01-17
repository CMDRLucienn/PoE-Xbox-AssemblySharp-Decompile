using UnityEngine;

public class VolumeAsCategory : MonoBehaviour
{
	public AudioSource Source;

	public MusicManager.SoundCategory Category;

	private float m_ExternalVolume = 1f;

	private bool m_initialized;

	private bool m_hasFocus = true;

	public float ExternalVolume
	{
		get
		{
			return m_ExternalVolume;
		}
		set
		{
			m_ExternalVolume = value;
			UpdateVolume();
		}
	}

	private void Start()
	{
		if (Source == null)
		{
			Source = GetComponent<AudioSource>();
		}
		Init();
	}

	private void OnEnable()
	{
		if (Source == null)
		{
			Source = GetComponent<AudioSource>();
		}
		if (Source != null)
		{
			Source.ignoreListenerVolume = true;
		}
		else
		{
			Debug.LogError("AudioSource is null in OnEnable() for object: " + base.gameObject.name);
		}
		UpdateVolume();
	}

	private void OnDisable()
	{
		if (Source == null)
		{
			Source = GetComponent<AudioSource>();
		}
		if (Source != null)
		{
			Source.ignoreListenerVolume = false;
		}
		else
		{
			Debug.LogError("AudioSource is null in OnDisable() for object: " + base.gameObject.name);
		}
	}

	private void Update()
	{
		Init();
		if (m_hasFocus != GameState.ApplicationIsFocused)
		{
			m_hasFocus = GameState.ApplicationIsFocused;
			UpdateVolume();
		}
	}

	private void OnDestroy()
	{
		if ((bool)Source)
		{
			Source.volume = ExternalVolume;
		}
		if ((bool)MusicManager.Instance)
		{
			MusicManager.Instance.OnVolumeChanged -= VolumeChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Init()
	{
		if ((bool)MusicManager.Instance && !m_initialized)
		{
			m_initialized = true;
			MusicManager.Instance.OnVolumeChanged += VolumeChanged;
		}
		UpdateVolume();
	}

	public void SetCategory(MusicManager.SoundCategory category, float externalVolume)
	{
		Category = category;
		ExternalVolume = externalVolume;
		UpdateVolume();
	}

	public void SetCategory(MusicManager.SoundCategory category)
	{
		Category = category;
		UpdateVolume();
	}

	public void UpdateVolume()
	{
		if (!m_hasFocus)
		{
			if ((bool)Source)
			{
				Source.volume = 0f;
			}
		}
		else if ((bool)MusicManager.Instance)
		{
			VolumeChanged(Category, MusicManager.Instance.GetFinalVolume(Category));
		}
	}

	private void VolumeChanged(MusicManager.SoundCategory cat, float vol)
	{
		if (base.enabled && base.gameObject.activeInHierarchy && (bool)Source && cat == Category)
		{
			Source.volume = vol * ExternalVolume;
		}
	}
}
