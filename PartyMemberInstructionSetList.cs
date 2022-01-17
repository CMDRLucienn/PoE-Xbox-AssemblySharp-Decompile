using System;
using UnityEngine;

[Serializable]
public class PartyMemberInstructionSetList : ScriptableObject
{
	public PartyMemberClassSpellList[] SpellLists;

	private static PartyMemberInstructionSetList s_InstructionSetList;

	public PartyMemberClassSpellList this[int index]
	{
		get
		{
			return SpellLists[index];
		}
		set
		{
			SpellLists[index] = value;
		}
	}

	public int Length => SpellLists.Length;

	public static PartyMemberInstructionSetList InstructionSetList
	{
		get
		{
			if (s_InstructionSetList == null)
			{
				s_InstructionSetList = GameResources.LoadPrefab<PartyMemberInstructionSetList>("classbasedinstructionsets", instantiate: false);
			}
			return s_InstructionSetList;
		}
	}

	public PartyMemberClassSpellList GetClassSpellList(CharacterStats.Class characterClass)
	{
		for (int i = 0; i < SpellLists.Length; i++)
		{
			if (SpellLists[i].Class == characterClass)
			{
				return SpellLists[i];
			}
		}
		return null;
	}

	public PartyMemberSpellList GetInstructionSet(CharacterStats.Class characterClass, int index)
	{
		if (index < 0)
		{
			return null;
		}
		PartyMemberClassSpellList classSpellList = GetClassSpellList(characterClass);
		if (classSpellList != null && index < classSpellList.SpellLists.Length)
		{
			return classSpellList.SpellLists[index];
		}
		return null;
	}
}
