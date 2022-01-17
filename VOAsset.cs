using UnityEngine;

public class VOAsset : ScriptableObject
{
	public DatabaseString StringID = new DatabaseString();

	public VOBankClip VOClip;

	public SoundSet.SoundAction VOAction = SoundSet.SoundAction.Invalid;

	public SoundSet.SoundAction[] AdditionalActions = new SoundSet.SoundAction[0];
}
