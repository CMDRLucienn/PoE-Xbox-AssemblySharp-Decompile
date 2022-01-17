using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProjectileAudioFader : MonoBehaviour
{
	public void StartFade(float time)
	{
		StartCoroutine(FadeSound(time));
	}

	private IEnumerator FadeSound(float time)
	{
		float timeF = time;
		while (timeF > 0f)
		{
			if (timeF <= 0.01f)
			{
				if (base.gameObject.GetComponent<AudioSource>() != null)
				{
					base.gameObject.GetComponent<AudioSource>().volume = 0f;
				}
				GameUtilities.DestroyImmediate(base.gameObject);
			}
			else
			{
				if (base.gameObject != null && base.gameObject.GetComponent<AudioSource>() != null)
				{
					base.gameObject.GetComponent<AudioSource>().volume *= timeF;
				}
				timeF -= 0.01f;
			}
			yield return new WaitForSeconds(0.01f);
		}
	}
}
