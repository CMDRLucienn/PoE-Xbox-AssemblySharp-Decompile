using UnityEngine;

public class UIHUDMinimizeButton : MonoBehaviour
{
	public GameObject[] Hide;

	public UITweener[] HideTween;

	public UIAnchor ActiveAnchor;

	public UIAnchor HiddenAnchor;

	[HideInInspector]
	public bool MinimizedState;

	private void Start()
	{
		UITweener[] hideTween = HideTween;
		for (int i = 0; i < hideTween.Length; i++)
		{
			hideTween[i].Play(MinimizedState);
		}
		UpdateAnchor();
	}

	private void OnClick()
	{
		MinimizedState = !MinimizedState;
		GameObject[] hide = Hide;
		foreach (GameObject gameObject in hide)
		{
			UIPanel component = gameObject.GetComponent<UIPanel>();
			if ((bool)component)
			{
				component.alpha = ((!MinimizedState) ? 1 : 0);
			}
			else
			{
				gameObject.SetActive(!MinimizedState);
			}
		}
		UITweener[] hideTween = HideTween;
		for (int i = 0; i < hideTween.Length; i++)
		{
			hideTween[i].Play(MinimizedState);
		}
		UpdateAnchor();
	}

	private void UpdateAnchor()
	{
		if ((bool)ActiveAnchor)
		{
			ActiveAnchor.enabled = !MinimizedState;
		}
		if ((bool)HiddenAnchor)
		{
			HiddenAnchor.enabled = MinimizedState;
		}
		if ((bool)ActiveAnchor && ActiveAnchor.enabled)
		{
			ActiveAnchor.Update();
		}
		if ((bool)HiddenAnchor && HiddenAnchor.enabled)
		{
			HiddenAnchor.Update();
		}
	}
}
