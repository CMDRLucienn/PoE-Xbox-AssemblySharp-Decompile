using UnityEngine;

public class ScriptedInteractionAssets : ScriptableObject
{
	[Tooltip("List of images that can be used by the scripted interaction.")]
	public Texture[] Images;

	[Tooltip("List of Audio Clips that will be automatically played with the image at the same index is used.")]
	public AudioClip[] AudioClips;

	[Tooltip("List of Audio Clips that can be played arbitrarily by a script during the interaction.")]
	public AudioClip[] ScriptAudioClips;

	[Tooltip("List of music tracks that can be played arbitrarily by a script during the interaction.")]
	public AudioClip[] ScriptMusic;
}
