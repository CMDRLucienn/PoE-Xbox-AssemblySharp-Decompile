using System;

[Serializable]
public class UIEquipAudioEntry
{
	public string Name = "Entry";

	public Item.UIEquipSoundType ItemType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = ItemType.ToString();
	}
}
