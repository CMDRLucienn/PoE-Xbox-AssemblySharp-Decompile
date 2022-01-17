public static class UIHudAlerts
{
	public delegate void HudAlertDelegate(UIActionBarOnClick.ActionType alertType);

	public static HudAlertDelegate OnAlertStart;

	public static HudAlertDelegate OnAlertEnd;

	public static void Alert(UIActionBarOnClick.ActionType alertType)
	{
		if (OnAlertStart != null)
		{
			OnAlertStart(alertType);
		}
	}

	public static void Cancel(UIActionBarOnClick.ActionType alertType)
	{
		if (OnAlertEnd != null)
		{
			OnAlertEnd(alertType);
		}
	}
}
