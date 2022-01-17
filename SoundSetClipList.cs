using System;
using UnityEngine.Serialization;

[Serializable]
public class SoundSetClipList
{
	[ResourcesAudioClipProperty]
	[FormerlySerializedAs("strClips")]
	public string[] m_clips = new string[1];
}
