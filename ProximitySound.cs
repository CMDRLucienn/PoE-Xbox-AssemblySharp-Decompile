using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximitySound : MonoBehaviour
{
	private AudioSource m_audioSource;

	private float m_maxDistance;

	private float m_defaultVolume;

	private float m_currentVolume;

	private float m_fadeTime;

	private float m_fadeStartVolume;

	private float m_fadeTargetVolume;

	public float MaxDistance => m_maxDistance;

	private void Awake()
	{
		m_audioSource = base.gameObject.GetComponent<AudioSource>();
		if (m_audioSource != null)
		{
			m_maxDistance = m_audioSource.maxDistance;
			m_defaultVolume = m_audioSource.volume;
		}
		if (ProximitySoundManager.Instance != null)
		{
			m_maxDistance += ProximitySoundManager.Instance.EnabledDistanceBuffer;
			ProximitySoundManager.Instance.AddProximitySound(this);
		}
	}

	public void FadeIn()
	{
		m_fadeStartVolume = m_currentVolume;
		m_fadeTargetVolume = m_defaultVolume;
		m_fadeTime = ProximitySoundManager.Instance.FadeDuration;
	}

	public void FadeOut()
	{
		m_fadeStartVolume = m_currentVolume;
		m_fadeTargetVolume = 0f;
		m_fadeTime = ProximitySoundManager.Instance.FadeDuration;
	}

	public void UpdateFade()
	{
		m_fadeTime -= Time.unscaledDeltaTime;
		if (m_fadeTime < Mathf.Epsilon)
		{
			m_fadeTime = 0f;
		}
		if (m_audioSource != null)
		{
			float volume = m_fadeStartVolume + (m_fadeTargetVolume - m_fadeStartVolume) * (1f - m_fadeTime / ProximitySoundManager.Instance.FadeDuration);
			m_audioSource.volume = volume;
		}
	}

	public bool IsFading()
	{
		return m_fadeTime > Mathf.Epsilon;
	}

	public void EnableSound()
	{
		if (m_audioSource != null)
		{
			m_audioSource.enabled = true;
			if (!m_audioSource.isPlaying)
			{
				m_audioSource.PlayDelayed(0.01f);
			}
		}
	}

	public void DisableSound()
	{
		if (m_audioSource != null)
		{
			m_audioSource.enabled = false;
		}
	}
}
