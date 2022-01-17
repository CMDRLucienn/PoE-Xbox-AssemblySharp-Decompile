using UnityEngine;

public class InterstitialData : ScriptableObject
{
	public AreaNotificationsDatabaseString Text = new AreaNotificationsDatabaseString();

	public AudioClip VoiceOver;

	public AudioClip Music;

	public string MusicResourcesPath;

	public Texture Portrait;
}
