using UnityEngine;

public class UIHideWindowButton : UIIsButton
{
	public UIHudWindow Window;

	private void Start()
	{
		UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
		if ((bool)component)
		{
			component.ButtonUpSound = UIAudioList.UIAudioType.None;
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnClick()
	{
		if (base.enabled)
		{
			SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
			if (!Window.HideWindow())
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ButtonUp);
			}
		}
	}
}
