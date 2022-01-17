using System;
using UnityEngine;

[Serializable]
public class VOBankClip
{
	[ResourcesAudioClipProperty]
	public string clip;

	[Range(0f, 1f)]
	public float MinVolume = 1f;

	[Range(0f, 1f)]
	public float MaxVolume = 1f;

	[Tooltip("What percentage of the time will this voice line trigger for the action")]
	[Range(0f, 1f)]
	public float PlayFrequency = 1f;

	public float RandomVolume => OEIRandom.RangeInclusive(MinVolume, MaxVolume);
}
