using System;

[Serializable]
public class UIAudioEntry
{
	public string Name = "Entry";

	public UIAudioList.UIAudioType UIType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = UIType.ToString();
	}
}
