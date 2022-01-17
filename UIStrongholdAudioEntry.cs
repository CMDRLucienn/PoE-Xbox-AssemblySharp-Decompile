using System;

[Serializable]
public class UIStrongholdAudioEntry
{
	public string Name = "Entry";

	public Stronghold.UIActionSoundType ActionType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = ActionType.ToString();
	}
}
