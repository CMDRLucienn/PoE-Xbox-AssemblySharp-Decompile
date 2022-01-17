using System;

[Serializable]
public class UIUseAudioEntry
{
	public string Name = "Entry";

	public Item.UIDragDropSoundType ItemType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = ItemType.ToString();
	}
}
