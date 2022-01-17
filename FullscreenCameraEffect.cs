using System.Collections;
using UnityEngine;

public class FullscreenCameraEffect : MonoBehaviour
{
	public string[] FadeComponents;

	public AudioSource AudioSource;

	public AudioClip StartingAudio;

	public ClipBankSet RandomAudio;

	public float MinRandomAudioDelay;

	public float MaxRandomAudioDelay;

	public float AudioFadeTime;

	private bool mOn;

	private void Start()
	{
		StartCoroutine(PlayRandomAudio());
	}

	private void Update()
	{
		if ((bool)AudioSource && AudioSource.isPlaying)
		{
			if (mOn)
			{
				AudioSource.volume = Mathf.Min(1f, AudioSource.volume + Time.unscaledDeltaTime * AudioFadeTime);
			}
			else
			{
				AudioSource.volume = Mathf.Max(0f, AudioSource.volume - Time.unscaledDeltaTime * AudioFadeTime);
			}
		}
	}

	private IEnumerator PlayRandomAudio()
	{
		while (true)
		{
			if (mOn)
			{
				float volume;
				float pitch;
				AudioClip clip = RandomAudio.GetClip(forbidImmediateRepeat: true, out volume, out pitch);
				if (Time.timeScale > 0f)
				{
					AudioSource.PlayOneShot(clip, volume * AudioSource.volume);
				}
				float delayTime = OEIRandom.RangeInclusive(MinRandomAudioDelay, MaxRandomAudioDelay);
				float startTime = Time.realtimeSinceStartup;
				while (Time.realtimeSinceStartup - startTime < delayTime)
				{
					yield return null;
				}
			}
			else
			{
				yield return null;
			}
		}
	}

	public void FadeIn()
	{
		string[] fadeComponents = FadeComponents;
		foreach (string type in fadeComponents)
		{
			Component component = GetComponent(type);
			if ((bool)component && component as MonoBehaviour != null)
			{
				(component as MonoBehaviour).Invoke("FadeIn", 0f);
			}
		}
		if (!mOn)
		{
			GetComponent<Animator>().SetTrigger("Fade");
		}
		if ((bool)AudioSource && !AudioSource.isPlaying)
		{
			AudioSource.Play();
		}
		if ((bool)StartingAudio)
		{
			AudioSource.PlayOneShot(StartingAudio, 1f);
		}
		mOn = true;
	}

	public void FadeOut()
	{
		string[] fadeComponents = FadeComponents;
		foreach (string type in fadeComponents)
		{
			Component component = GetComponent(type);
			if ((bool)component && component as MonoBehaviour != null)
			{
				(component as MonoBehaviour).Invoke("FadeOut", 0f);
			}
		}
		if (mOn)
		{
			GetComponent<Animator>().SetTrigger("Fade");
		}
		mOn = false;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
