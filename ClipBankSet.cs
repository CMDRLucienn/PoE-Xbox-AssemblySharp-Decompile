using System;
using UnityEngine;

[Serializable]
public class ClipBankSet
{
	public string name;

	public ClipBankClip[] clips = new ClipBankClip[1];

	[NonSerialized]
	private float m_TotalProbability;

	[NonSerialized]
	private bool m_ProbDirty = true;

	[NonSerialized]
	private ClipBankClip m_LastPlayed;

	public ClipBankSet()
	{
	}

	public ClipBankSet(string newName)
	{
		name = newName;
	}

	public void LoadAudio()
	{
		for (int i = 0; i < clips.Length; i++)
		{
			clips[i].Load();
		}
	}

	public void UnloadAudio()
	{
		for (int i = 0; i < clips.Length; i++)
		{
			clips[i].Unload();
		}
	}

	public void Recalculate()
	{
		m_ProbDirty = false;
		m_TotalProbability = 0f;
		for (int i = 0; i < clips.Length; i++)
		{
			m_TotalProbability += clips[i].relativeFreq;
		}
		if (m_TotalProbability == 0f && clips.Length != 0)
		{
			Debug.LogError(name + " clip set has clips but their probability is set to 0!");
		}
	}

	public AudioClip GetClip(bool forbidImmediateRepeat, out float volume, out float pitch)
	{
		volume = 1f;
		pitch = 1f;
		if (clips.Length == 0)
		{
			return null;
		}
		if (m_ProbDirty)
		{
			Recalculate();
		}
		if (m_TotalProbability == 0f)
		{
			return null;
		}
		forbidImmediateRepeat = forbidImmediateRepeat && clips.Length > 1;
		float num = m_TotalProbability;
		if (forbidImmediateRepeat && m_LastPlayed != null)
		{
			num -= m_LastPlayed.relativeFreq;
		}
		float num2 = num * (float)AudioBank.s_AudioRandom.NextDouble();
		float num3 = 0f;
		ClipBankClip[] array = clips;
		foreach (ClipBankClip clipBankClip in array)
		{
			if (!forbidImmediateRepeat || clipBankClip != m_LastPlayed)
			{
				num3 += clipBankClip.relativeFreq;
				if (num3 >= num2)
				{
					m_LastPlayed = clipBankClip;
					volume = OEIRandom.RangeInclusive(clipBankClip.MinVolume, clipBankClip.MaxVolume);
					pitch = OEIRandom.RangeInclusive(clipBankClip.MinPitch, clipBankClip.MaxPitch);
					return clipBankClip.clip;
				}
			}
		}
		Debug.LogError("Reached the end of AudioBank.GetClip (coding error.)");
		m_LastPlayed = null;
		return null;
	}

	public AudioClip GetClip(bool forbidImmediateRepeat, out float spatialBlend, out float volume, out float pitch)
	{
		volume = 1f;
		pitch = 1f;
		spatialBlend = 0f;
		if (clips.Length == 0)
		{
			return null;
		}
		if (m_ProbDirty)
		{
			Recalculate();
		}
		if (m_TotalProbability == 0f)
		{
			return null;
		}
		forbidImmediateRepeat = forbidImmediateRepeat && clips.Length > 1;
		float num = m_TotalProbability;
		if (forbidImmediateRepeat && m_LastPlayed != null)
		{
			num -= m_LastPlayed.relativeFreq;
		}
		float num2 = num * (float)AudioBank.s_AudioRandom.NextDouble();
		float num3 = 0f;
		ClipBankClip[] array = clips;
		foreach (ClipBankClip clipBankClip in array)
		{
			if (!forbidImmediateRepeat || clipBankClip != m_LastPlayed)
			{
				num3 += clipBankClip.relativeFreq;
				if (num3 >= num2)
				{
					m_LastPlayed = clipBankClip;
					volume = OEIRandom.RangeInclusive(clipBankClip.MinVolume, clipBankClip.MaxVolume);
					pitch = OEIRandom.RangeInclusive(clipBankClip.MinPitch, clipBankClip.MaxPitch);
					spatialBlend = clipBankClip.spatialBlend;
					return clipBankClip.clip;
				}
			}
		}
		Debug.LogError("Reached the end of AudioBank.GetClip (coding error.)");
		m_LastPlayed = null;
		return null;
	}
}
