using UnityEngine;

public class PlayTweenOnStart : MonoBehaviour
{
	public UITweener Tween;

	private void Start()
	{
		if (!Tween)
		{
			Tween = GetComponent<UITweener>();
		}
		if ((bool)Tween)
		{
			Tween.Play(forward: true);
		}
	}
}
