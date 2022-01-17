using System;
using UnityEngine;

public class UICheckboxAudio : MonoBehaviour
{
	private void Start()
	{
		UICheckbox component = GetComponent<UICheckbox>();
		if ((bool)component)
		{
			component.GetComponent<UIImageButtonRevised>().ButtonDownSound = UIAudioList.UIAudioType.None;
			component.GetComponent<UIImageButtonRevised>().ButtonUpSound = UIAudioList.UIAudioType.None;
			component.onStateChangeUser = (UICheckbox.OnStateChange)Delegate.Combine(component.onStateChangeUser, new UICheckbox.OnStateChange(OnCheckStateChange));
		}
	}

	private void OnDestroy()
	{
		UICheckbox component = GetComponent<UICheckbox>();
		if ((bool)component)
		{
			component.onStateChangeUser = (UICheckbox.OnStateChange)Delegate.Remove(component.onStateChangeUser, new UICheckbox.OnStateChange(OnCheckStateChange));
		}
	}

	private void OnCheckStateChange(GameObject sender, bool state)
	{
		if ((bool)GlobalAudioPlayer.Instance)
		{
			if (state)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Check);
			}
			else
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Uncheck);
			}
		}
	}
}
