using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioBank : MonoBehaviour
{
	public static System.Random s_AudioRandom = new System.Random();

	public AudioBankList bankFromList;

	public ClipBankSet[] bankList = new ClipBankSet[1];

	public bool DebugLog;

	public bool forbidOverlap;

	public bool forbidImRepeat = true;

	private AudioSource m_Source;

	private static GameObject m_defaultSource = null;

	private static Transform s_AudioParentTransform;

	public static GameObject DefaultSource => m_defaultSource;

	public static Transform AudioBankHierarchyParent
	{
		get
		{
			if (s_AudioParentTransform == null)
			{
				s_AudioParentTransform = new GameObject
				{
					name = "AudioParent"
				}.transform;
			}
			return s_AudioParentTransform;
		}
	}

	private void Awake()
	{
		m_Source = GetComponent<AudioSource>();
		if (m_defaultSource == null)
		{
			m_defaultSource = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Audio/DefaultAudioSource", typeof(GameObject)));
			m_defaultSource.gameObject.transform.parent = AudioBankHierarchyParent;
		}
	}

	private void Start()
	{
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			if (GetComponent<AudioFootsteps>() == null)
			{
				component.OnEventFootstep += anim_OnEventFootstep;
			}
			component.OnEventAudio += anim_OnEventAudio;
		}
	}

	private void anim_OnEventAudio(object sender, EventArgs e)
	{
		PlayFrom(sender.ToString());
	}

	private void anim_OnEventFootstep(object sender, EventArgs e)
	{
		string empty = string.Empty;
		PlayFrom("footstep" + empty);
	}

	public bool PlayFrom(string entry)
	{
		if ((bool)m_Source && m_Source.isPlaying)
		{
			return true;
		}
		if (GameState.IsLoading || FadeManager.Instance.FadeValue == 1f)
		{
			return false;
		}
		ClipBankSet entry2 = GetEntry(entry);
		PlayFrom(entry2);
		return false;
	}

	public void PlayFrom(ClipBankSet set)
	{
		if (set == null)
		{
			return;
		}
		float spatialBlend;
		float volume;
		float pitch;
		AudioClip clip = set.GetClip(forbidImRepeat, out spatialBlend, out volume, out pitch);
		if (!(clip != null))
		{
			return;
		}
		if ((bool)m_Source)
		{
			if (forbidOverlap)
			{
				m_Source.Stop();
				m_Source.volume = volume;
				m_Source.pitch = pitch;
				m_Source.clip = clip;
				m_Source.spatialBlend = spatialBlend;
				m_Source.dopplerLevel = 0f;
				GlobalAudioPlayer.Play(m_Source);
			}
			else
			{
				PlayClipAtPoint(clip, m_Source.transform.position, spatialBlend, volume);
			}
		}
		else
		{
			Debug.LogError("AudioBank found no AudioSource component - on object '" + base.gameObject.name + "'.");
		}
	}

	protected ClipBankSet GetEntry(string name)
	{
		if (DebugLog)
		{
			string message = (base.gameObject.name = " playing '" + name + "'");
			Debug.Log(message, base.gameObject);
		}
		ClipBankSet[] bank = bankList;
		if (bankFromList != null)
		{
			bank = bankFromList.bank;
		}
		if (string.IsNullOrEmpty(name) && bank.Length != 0)
		{
			return bank[0];
		}
		int i = 0;
		for (int num = bank.Length; i < num; i++)
		{
			if (bank[i] != null && bank[i].name.Equals(name))
			{
				return bank[i];
			}
		}
		if (DebugLog)
		{
			Debug.LogWarning("AudioBank for '" + base.gameObject.name + "' has no ClipBankSet named '" + name + "'.", base.gameObject);
		}
		return null;
	}

	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float spatialBlend, float volume)
	{
		return PlayClipAtPoint(clip, position, spatialBlend, volume, ignoreListenerVolume: false);
	}

	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float spatialBlend, float volume, bool ignoreListenerVolume)
	{
		if (m_defaultSource == null || clip == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_defaultSource);
		gameObject.name = "Audio_" + clip.name;
		gameObject.transform.parent = AudioBankHierarchyParent;
		gameObject.transform.position = position;
		AudioSource component = gameObject.GetComponent<AudioSource>();
		component.clip = clip;
		component.volume = volume;
		component.ignoreListenerVolume = ignoreListenerVolume;
		component.spatialBlend = spatialBlend;
		GlobalAudioPlayer.Play(component);
		float num = Time.timeScale;
		if (num < float.Epsilon)
		{
			num = 2f;
		}
		GameUtilities.Destroy(gameObject, clip.length * num);
		return component;
	}
}
