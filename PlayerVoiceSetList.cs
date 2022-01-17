using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVoiceSetList : ScriptableObject
{
	[Flags]
	public enum VoiceSetPriority
	{
		Gender = 0x20,
		Subrace = 0x10,
		Race = 8,
		Culture = 4,
		Class = 2
	}

	[Serializable]
	public class VoiceSetPreferedData
	{
		public SoundSet PlayerSoundSet;

		public Gender PreferredGender = Gender.Neuter;

		public CharacterStats.Subrace PreferredSubrace;

		public CharacterStats.Race PreferredRace;

		public CharacterStats.Culture PreferredCulture;

		public CharacterStats.Class PreferredClass;
	}

	public class SetPriorityValue : IComparable<SetPriorityValue>
	{
		public SoundSet PlayerSoundSet;

		public int PriorityValue;

		public SetPriorityValue(SoundSet soundSet, int value)
		{
			PlayerSoundSet = soundSet;
			PriorityValue = value;
		}

		public int CompareTo(SetPriorityValue other)
		{
			if (PriorityValue < other.PriorityValue)
			{
				return 1;
			}
			if (PriorityValue > other.PriorityValue)
			{
				return -1;
			}
			return 0;
		}
	}

	public static string DefaultPlayerSoundSetList = "DefaultPlayerVoiceSetList";

	public List<VoiceSetPreferedData> SoundSets;

	public SoundSet[] GetPrioritySortedVoiceSets(Gender gender, CharacterStats.Subrace subrace, CharacterStats.Race race, CharacterStats.Culture culture, CharacterStats.Class playerClass)
	{
		List<SetPriorityValue> list = new List<SetPriorityValue>();
		foreach (VoiceSetPreferedData soundSet in SoundSets)
		{
			SetPriorityValue setPriorityValue = new SetPriorityValue(soundSet.PlayerSoundSet, 0);
			if (soundSet.PreferredGender == Gender.Neuter)
			{
				setPriorityValue.PriorityValue -= 1000;
			}
			if (soundSet.PreferredGender == gender)
			{
				setPriorityValue.PriorityValue += 32;
			}
			if (soundSet.PreferredSubrace == subrace)
			{
				setPriorityValue.PriorityValue += 16;
			}
			if (soundSet.PreferredRace == race)
			{
				setPriorityValue.PriorityValue += 8;
			}
			if (soundSet.PreferredCulture == culture)
			{
				setPriorityValue.PriorityValue += 4;
			}
			if (soundSet.PreferredClass == playerClass)
			{
				setPriorityValue.PriorityValue += 2;
			}
			list.Add(setPriorityValue);
		}
		list.Sort();
		SoundSet[] array = new SoundSet[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i].PlayerSoundSet;
		}
		return array;
	}

	public SoundSet[] GetPrioritySortedVoiceSets(CharacterStats stats)
	{
		return GetPrioritySortedVoiceSets(stats.Gender, stats.CharacterSubrace, stats.CharacterRace, stats.CharacterCulture, stats.CharacterClass);
	}

	public SoundSet GetSoundSet(string name)
	{
		foreach (VoiceSetPreferedData soundSet in SoundSets)
		{
			if (soundSet.PlayerSoundSet.name.CompareTo(name.Replace("(Clone)", "")) == 0)
			{
				return UnityEngine.Object.Instantiate(soundSet.PlayerSoundSet);
			}
		}
		return null;
	}
}
