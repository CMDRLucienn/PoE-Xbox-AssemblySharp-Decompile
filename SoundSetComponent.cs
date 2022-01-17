using UnityEngine;

public class SoundSetComponent : MonoBehaviour
{
	public SoundSet SoundSet;

	private bool m_requiresDestroy;

	[Persistent]
	private string SoundSetSerialized
	{
		get
		{
			if (SoundSet == null)
			{
				return string.Empty;
			}
			string result = string.Empty;
			if (GetComponent<CompanionInstanceID>() == null)
			{
				result = SoundSet.name.Replace("(Clone)", string.Empty);
			}
			return result;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && (SoundSet == null || SoundSet.name.CompareTo(value) != 0))
			{
				PlayerVoiceSetList playerVoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
				if ((bool)playerVoiceSetList)
				{
					SetSoundSet(playerVoiceSetList.GetSoundSet(value));
				}
			}
		}
	}

	public void SetSoundSet(SoundSet s)
	{
		if (SoundSet != null && SoundSet != s && m_requiresDestroy)
		{
			GameUtilities.Destroy(SoundSet);
		}
		SoundSet = s;
		m_requiresDestroy = true;
	}

	private void Update()
	{
		if (SoundSet != null)
		{
			SoundSet.MyUpdate(Time.unscaledDeltaTime);
		}
	}

	private void OnDestroy()
	{
		if (m_requiresDestroy && SoundSet != null)
		{
			GameUtilities.Destroy(SoundSet);
			m_requiresDestroy = false;
		}
	}

	public bool PlaySound(SoundSet.SoundAction action)
	{
		return SoundSet.PlaySound(base.gameObject, action);
	}

	public bool PlaySound(SoundSet.SoundAction action, int idx)
	{
		return SoundSet.PlaySound(base.gameObject, action, idx);
	}

	public bool PlaySound(SoundSet.SoundAction action, int idx, bool skipIfConversing)
	{
		return SoundSet.PlaySound(base.gameObject, action, idx, skipIfConversing);
	}

	public bool PlaySound(SoundSet.SoundAction action, int idx, bool skipIfConversing, bool ignoreListenerVolume)
	{
		return SoundSet.PlaySound(base.gameObject, action, idx, skipIfConversing, ignoreListenerVolume);
	}
}
