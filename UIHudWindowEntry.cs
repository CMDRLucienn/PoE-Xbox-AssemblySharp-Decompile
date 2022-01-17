using System;

[Serializable]
public class UIHudWindowEntry
{
	public string Name = "Entry";

	public UIHudWindow.WindowType WindowType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = WindowType.ToString();
	}
}
