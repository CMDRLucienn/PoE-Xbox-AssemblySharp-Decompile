using System;

[Serializable]
public class SpellMaxData
{
	public CharacterStats.Class Class;

	public SpellMaxCastLevelData[] MaxCastByLevel = new SpellMaxCastLevelData[16];

	public int[] MinLevelPerEncounter = new int[8];
}
