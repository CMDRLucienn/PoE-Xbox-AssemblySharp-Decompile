using System;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySoundManager : MonoBehaviour
{
	[Tooltip("The length in seconds for a proximity sound to fade in or out.")]
	public float FadeDuration = 0.5f;

	[Tooltip("When checking if a sound is in range of the listener to be enabled, add this tolerance distance.")]
	public float EnabledDistanceBuffer = 3f;

	private Transform m_CachedAudioListenerTransform;

	private List<ProximitySound> m_disabledSounds = new List<ProximitySound>(128);

	private List<ProximitySound> m_enabledSounds = new List<ProximitySound>(128);

	private List<ProximitySound> m_fadingInSounds = new List<ProximitySound>(128);

	private List<ProximitySound> m_fadingOutSounds = new List<ProximitySound>(128);

	public static ProximitySoundManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'ProximitySoundManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		GameState.OnLevelUnload += LevelUnloaded;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= LevelUnloaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!GameState.Paused)
		{
			Vector3 audioListenerPosition = GetAudioListenerPosition();
			UpdateFadingSounds();
			UpdateEnabledSounds(audioListenerPosition);
			UpdateDisabledSounds(audioListenerPosition);
		}
	}

	private void LevelUnloaded(object sender, EventArgs e)
	{
		m_disabledSounds.Clear();
		m_enabledSounds.Clear();
		m_fadingInSounds.Clear();
		m_fadingOutSounds.Clear();
	}

	public void AddProximitySound(ProximitySound sound)
	{
		m_disabledSounds.Add(sound);
	}

	private void UpdateFadingSounds()
	{
		for (int num = m_fadingInSounds.Count - 1; num >= 0; num--)
		{
			ProximitySound proximitySound = m_fadingInSounds[num];
			if (proximitySound != null)
			{
				proximitySound.UpdateFade();
				if (!proximitySound.IsFading())
				{
					m_fadingInSounds.RemoveAt(num);
					if (!m_enabledSounds.Contains(proximitySound))
					{
						m_enabledSounds.Add(proximitySound);
					}
				}
			}
			else
			{
				m_fadingInSounds.RemoveAt(num);
			}
		}
		for (int num2 = m_fadingOutSounds.Count - 1; num2 >= 0; num2--)
		{
			ProximitySound proximitySound2 = m_fadingOutSounds[num2];
			if (proximitySound2 != null)
			{
				proximitySound2.UpdateFade();
				if (!proximitySound2.IsFading())
				{
					m_fadingOutSounds.RemoveAt(num2);
					if (!m_disabledSounds.Contains(proximitySound2))
					{
						proximitySound2.DisableSound();
						m_disabledSounds.Add(proximitySound2);
					}
				}
			}
			else
			{
				m_fadingOutSounds.RemoveAt(num2);
			}
		}
	}

	private void UpdateEnabledSounds(Vector3 listenerPos)
	{
		for (int num = m_enabledSounds.Count - 1; num >= 0; num--)
		{
			ProximitySound proximitySound = m_enabledSounds[num];
			if (proximitySound != null)
			{
				if ((proximitySound.transform.position - listenerPos).sqrMagnitude > proximitySound.MaxDistance * proximitySound.MaxDistance)
				{
					proximitySound.FadeOut();
					m_enabledSounds.RemoveAt(num);
					m_fadingOutSounds.Add(proximitySound);
				}
			}
			else
			{
				m_enabledSounds.RemoveAt(num);
			}
		}
	}

	private void UpdateDisabledSounds(Vector3 listenerPos)
	{
		for (int num = m_disabledSounds.Count - 1; num >= 0; num--)
		{
			ProximitySound proximitySound = m_disabledSounds[num];
			if (proximitySound != null)
			{
				if ((proximitySound.transform.position - listenerPos).sqrMagnitude <= proximitySound.MaxDistance * proximitySound.MaxDistance)
				{
					proximitySound.EnableSound();
					proximitySound.FadeIn();
					m_disabledSounds.RemoveAt(num);
					m_fadingInSounds.Add(proximitySound);
				}
			}
			else
			{
				m_disabledSounds.RemoveAt(num);
			}
		}
	}

	private void CacheAudioListenerTransform()
	{
		if (!(Camera.main == null))
		{
			AudioListener componentInChildren = Camera.main.GetComponentInChildren<AudioListener>();
			if (componentInChildren != null)
			{
				m_CachedAudioListenerTransform = componentInChildren.transform;
			}
		}
	}

	private Vector3 GetAudioListenerPosition()
	{
		if (Camera.main == null)
		{
			return Vector3.zero;
		}
		if (m_CachedAudioListenerTransform == null)
		{
			CacheAudioListenerTransform();
		}
		if (m_CachedAudioListenerTransform == null)
		{
			return Camera.main.transform.position;
		}
		return m_CachedAudioListenerTransform.position;
	}
}
