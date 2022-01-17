using UnityEngine;

public class UIAudioTrigger : MonoBehaviour
{
	public UIAudioList.UIAudioType OnPressDown;

	public UIAudioList.UIAudioType OnPressUp;

	public UIAudioList.UIAudioType OnHoverSound;

	public UIAudioList.UIAudioType OnDragSound;

	public UIAudioList.UIAudioType OnDropSound;

	private void OnPress(bool state)
	{
		if (state)
		{
			GlobalAudioPlayer.SPlay(OnPressDown);
		}
		else
		{
			GlobalAudioPlayer.SPlay(OnPressUp);
		}
	}

	private void OnHover(bool state)
	{
		if (state)
		{
			GlobalAudioPlayer.SPlay(OnHoverSound);
		}
	}

	private void OnDrag()
	{
		GlobalAudioPlayer.SPlay(OnDragSound);
	}

	private void OnDrop()
	{
		GlobalAudioPlayer.SPlay(OnDropSound);
	}
}
