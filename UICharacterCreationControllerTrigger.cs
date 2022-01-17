public class UICharacterCreationControllerTrigger : UICharacterCreationElement
{
	public UICharacterCreationController Controller;

	public UICharacterCreationController FallbackController;

	private void OnClick()
	{
		if ((bool)Controller && !Controller.ShouldSkip() && UICharacterCreationManager.Instance.GetCurrentController() != Controller)
		{
			Trigger(Controller);
		}
		else if ((bool)FallbackController && !FallbackController.ShouldSkip() && UICharacterCreationManager.Instance.GetCurrentController() != FallbackController)
		{
			Trigger(FallbackController);
		}
	}

	public void Trigger(UICharacterCreationController controller)
	{
		if (UICharacterCreationManager.Instance.GetCurrentController() != controller)
		{
			UICharacterCreationManager.Instance.AdvanceToLatestControllerUpTo(controller);
		}
	}
}
