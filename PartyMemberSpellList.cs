using System;

[Serializable]
public class PartyMemberSpellList : SpellList
{
	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public DatabaseString Description = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public float CooldownBetweenSpells;

	public override string ToString()
	{
		return DisplayName.GetText();
	}
}
