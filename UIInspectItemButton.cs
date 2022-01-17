public class UIInspectItemButton : UIIsButton
{
	private void OnClick()
	{
		if (base.enabled)
		{
			InGameHUD.Instance.EnterInspectMode();
		}
	}
}
