using System;
using UnityEngine;

[Serializable]
public class ClipBankClip
{
	[Tooltip("")]
	public AudioClip clip;

	[Tooltip("")]
	public string ClipResourcePath;

	[Tooltip("")]
	[Range(0f, 1f)]
	public float spatialBlend = 1f;

	[Tooltip("")]
	public float relativeFreq = 1f;

	[Tooltip("")]
	[Range(0f, 1f)]
	public float MinVolume = 1f;

	[Tooltip("")]
	[Range(0f, 1f)]
	public float MaxVolume = 1f;

	[Tooltip("")]
	public float MinPitch = 1f;

	[Tooltip("")]
	public float MaxPitch = 1f;

	private bool m_IsLoaded;

	public bool IsLoaded
	{
		get
		{
			return m_IsLoaded;
		}
		private set
		{
			m_IsLoaded = value;
		}
	}

	public float RandomVolume => OEIRandom.RangeInclusive(MinVolume, MaxVolume);

	public void Load()
	{
		if (!IsLoaded)
		{
			if (ClipResourcePath != null && ClipResourcePath != "")
			{
				AudioClip audioClip = Resources.Load<AudioClip>(ClipResourcePath);
				if (audioClip != null)
				{
					clip = audioClip;
					IsLoaded = true;
				}
				else
				{
					Debug.LogError("Failed to load AudioCLip \"" + ClipResourcePath + "\"");
				}
			}
		}
		else if (ClipResourcePath != null && ClipResourcePath != "")
		{
			Debug.LogWarning("Attempted to load already loaded AudioClip \"" + ClipResourcePath + "\"");
		}
	}

	public void Unload()
	{
		if (IsLoaded)
		{
			Resources.UnloadAsset(clip);
			clip = null;
			IsLoaded = false;
		}
		else if (ClipResourcePath != null && ClipResourcePath != "")
		{
			Debug.LogWarning("Attempted to unload non-loaded AudioClip \"" + ClipResourcePath + "\"");
		}
	}
}
