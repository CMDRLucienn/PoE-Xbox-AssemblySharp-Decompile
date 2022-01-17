using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RevealStats
{
	public float Kills = -1f;

	public float Name = -1f;

	public float Picture = -1f;

	public float Level = -1f;

	public float Health = -1f;

	public float Accuracy = -1f;

	public float Might = -1f;

	public float Dexterity = -1f;

	public float Resolve = -1f;

	public float Intellect = -1f;

	public float Constitution = -1f;

	public float Perception = -1f;

	public float Deflection = -1f;

	public float Fortitude = -1f;

	public float Reflexes = -1f;

	public float Will = -1f;

	public float DTs = -1f;

	public float Description = -1f;

	public float SpecialDTs = -1f;

	public float PrimaryDamage = -1f;

	public float SecondaryDamage = -1f;

	public float Abilities = -1f;

	public float Race = -1f;

	public bool HasStat(IndexableStat stat)
	{
		return RevealPointForStat(stat) >= 0f;
	}

	public bool CanSeeStat(RevealStats overrides, IndexableStat stat, float proportionKills)
	{
		if (overrides != null && overrides.HasStat(stat))
		{
			return overrides.CanSeeStat(null, stat, proportionKills);
		}
		float num = (HasStat(stat) ? RevealPointForStat(stat) : 0f);
		return proportionKills >= num;
	}

	public int GetNumUnlocks(RevealStats overrides, int killsToMaster)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (IndexableStat value in Enum.GetValues(typeof(IndexableStat)))
		{
			if (value != IndexableStat.COUNT)
			{
				float num = -1f;
				num = ((overrides == null || !overrides.HasStat(value)) ? RevealPointForStat(value) : overrides.RevealPointForStat(value));
				int num2 = Mathf.CeilToInt(num * (float)killsToMaster);
				if (num2 >= 0)
				{
					hashSet.Add(num2);
				}
			}
		}
		if (hashSet.Contains(0) && hashSet.Contains(1))
		{
			hashSet.Remove(0);
		}
		return hashSet.Count;
	}

	public int GetNumUnlocksAchieved(RevealStats overrides, float proportionKills, int killsToMaster)
	{
		int num = Mathf.RoundToInt(proportionKills * (float)killsToMaster);
		HashSet<int> hashSet = new HashSet<int>();
		foreach (IndexableStat value in Enum.GetValues(typeof(IndexableStat)))
		{
			if (value != IndexableStat.COUNT)
			{
				float num2 = -1f;
				num2 = ((overrides == null || !overrides.HasStat(value)) ? RevealPointForStat(value) : overrides.RevealPointForStat(value));
				int num3 = Mathf.CeilToInt(num2 * (float)killsToMaster);
				if (num3 >= 0 && num >= num3)
				{
					hashSet.Add(num3);
				}
			}
		}
		if (hashSet.Contains(0) && hashSet.Contains(1))
		{
			hashSet.Remove(0);
		}
		return hashSet.Count;
	}

	public float RevealPointForStat(IndexableStat stat)
	{
		return stat switch
		{
			IndexableStat.KILLS => Kills, 
			IndexableStat.NAME => Name, 
			IndexableStat.PICTURE => Picture, 
			IndexableStat.LEVEL => Level, 
			IndexableStat.HEALTH => Health, 
			IndexableStat.ACCURACY => Accuracy, 
			IndexableStat.MIGHT => Might, 
			IndexableStat.DEXTERITY => Dexterity, 
			IndexableStat.RESOLVE => Resolve, 
			IndexableStat.INTELLECT => Intellect, 
			IndexableStat.CONSTITUTION => Constitution, 
			IndexableStat.PERCEPTION => Perception, 
			IndexableStat.DEFLECTION => Deflection, 
			IndexableStat.FORTITUDE => Fortitude, 
			IndexableStat.REFLEXES => Reflexes, 
			IndexableStat.WILL => Will, 
			IndexableStat.DT => DTs, 
			IndexableStat.DESCRIPTION => Description, 
			IndexableStat.SPECIAL_DTS => SpecialDTs, 
			IndexableStat.STAMINA => Health, 
			IndexableStat.DR => DTs, 
			IndexableStat.SPECIAL_DRS => SpecialDTs, 
			IndexableStat.PRIMARYDAMAGE => PrimaryDamage, 
			IndexableStat.SECONDARYDAMAGE => SecondaryDamage, 
			IndexableStat.ABILITIES => Abilities, 
			IndexableStat.RACE => Race, 
			_ => 0f, 
		};
	}
}
