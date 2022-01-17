using System;

[Serializable]
public class CampEffectSubBonus
{
	public DatabaseString DropdownName;

	public Affliction Affliction;

	public override string ToString()
	{
		return DropdownName.GetText();
	}
}
