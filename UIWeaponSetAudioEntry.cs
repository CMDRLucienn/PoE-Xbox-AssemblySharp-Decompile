using System;

[Serializable]
public class UIWeaponSetAudioEntry
{
	public string Name = "Entry";

	public WeaponSpecializationData.WeaponType WeaponType;

	public ClipBankSet Clips;

	public void UpdateName()
	{
		Name = WeaponType.ToString();
	}
}
