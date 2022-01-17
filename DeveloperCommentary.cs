using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DeveloperCommentary : MonoBehaviour
{
	public AudioClip CommentaryTrack;

	public GUIDatabaseString DeveloperName;

	public GUIDatabaseString DeveloperTitle;

	public Texture2D DeveloperPortrait;

	private VolumeAsCategory m_volumeAsCategory;

	private bool m_HasStarted;

	private AudioSource m_source;

	public bool IsFinished
	{
		get
		{
			if (m_HasStarted)
			{
				return !m_source.isPlaying;
			}
			return false;
		}
	}

	private void Awake()
	{
		m_volumeAsCategory = base.gameObject.AddComponent<VolumeAsCategory>();
		m_volumeAsCategory.Category = MusicManager.SoundCategory.VOICE;
		m_source = GetComponent<AudioSource>();
		m_source.ignoreListenerPause = true;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Queue()
	{
		if (GameState.Option.DeveloperCommentary)
		{
			UIDeveloperCommentary.Instance.QueueCommentary(this);
		}
	}

	public void PlayAudio()
	{
		m_source.clip = CommentaryTrack;
		m_source.spatialBlend = 0f;
		if ((bool)m_volumeAsCategory)
		{
			m_volumeAsCategory.Source = m_source;
			m_volumeAsCategory.UpdateVolume();
		}
		GlobalAudioPlayer.Play(m_source);
		m_HasStarted = true;
	}

	public void StopAudio()
	{
		m_source.Stop();
	}
}
