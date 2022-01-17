using System;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAppearanceList : ScriptableObject
{
	[Serializable]
	public class DefaultAppearanceSet
	{
		public CharacterStats.Subrace Subrace;

		public Gender Gender = Gender.Neuter;

		public int HairVariation = 1;

		public int FacialHairVariation = 1;

		public int HeadVariation = 1;

		public int PrimaryColorIndex;

		public int SecondaryColorIndex;

		public int HairColorIndex;

		public int SkinColorIndex;
	}

	public List<DefaultAppearanceSet> DefaultAppearanceSets;

	public static DefaultAppearanceSet GetMatchingAppearanceSet(CharacterStats.Subrace subrace, Gender gender)
	{
		return (Resources.Load("Data/Lists/DefaultAppearanceList") as DefaultAppearanceList).GetMatchingAppearanceSetInternal(subrace, gender);
	}

	private DefaultAppearanceSet GetMatchingAppearanceSetInternal(CharacterStats.Subrace subrace, Gender gender)
	{
		foreach (DefaultAppearanceSet defaultAppearanceSet in DefaultAppearanceSets)
		{
			if ((defaultAppearanceSet.Gender == Gender.Neuter || defaultAppearanceSet.Gender == gender) && defaultAppearanceSet.Subrace == subrace)
			{
				return defaultAppearanceSet;
			}
		}
		return null;
	}
}
