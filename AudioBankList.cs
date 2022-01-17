using UnityEngine;

public class AudioBankList : ScriptableObject
{
	public ClipBankSet[] bank;

	private int m_refCount;

	public void RegisterUse()
	{
		if (m_refCount <= 0)
		{
			for (int i = 0; i < bank.Length; i++)
			{
				bank[i].LoadAudio();
			}
		}
		m_refCount++;
	}

	public void UnregisterUse()
	{
		m_refCount--;
		if (m_refCount < 0)
		{
			Debug.LogWarning("Refcount of AudioBankList \"" + base.name + "\" went below zero. Attempted to unload already unloaded AudioBankList");
		}
		if (m_refCount <= 0)
		{
			for (int i = 0; i < bank.Length; i++)
			{
				bank[i].UnloadAudio();
			}
		}
	}
}
