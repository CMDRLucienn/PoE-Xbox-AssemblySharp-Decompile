using UnityEngine;

public class CampEffectList : ScriptableObject
{
	[Tooltip("List of survival bonuses indexed by the survival level that unlocks them (neglecting 0).")]
	public Affliction[] BonusesByLevel;

	[Tooltip("If a particular bonus should have a sub-list, include that here. This disables the top-level bonus.")]
	public CampEffectSubBonuses[] SubBonuses;

	[Tooltip("How often the bonus list repeats itself with upgraded versions.")]
	public int CyclePeriod = 6;

	public CampEffectSubBonus[] GetSubBonuses(int index)
	{
		int num = index % CyclePeriod;
		int num2 = index / CyclePeriod;
		index = num + num2 * CyclePeriod;
		for (int i = 0; i < SubBonuses.Length; i++)
		{
			if (SubBonuses[i].OverrideIndex == index)
			{
				return SubBonuses[i].SubBonuses;
			}
		}
		return null;
	}

	public bool HasSubBonuses(int index)
	{
		return GetSubBonuses(index) != null;
	}

	public Affliction GetBestBonusByIndex(int index, int subindex, int survival)
	{
		int num = index % CyclePeriod;
		int max = (BonusesByLevel.Length - 1 - num) / CyclePeriod;
		int num2 = Mathf.Clamp(Mathf.FloorToInt((float)(survival - 1 - num) / (float)CyclePeriod), 0, max) * CyclePeriod + num;
		if (subindex >= 0)
		{
			for (int i = 0; i < SubBonuses.Length; i++)
			{
				if (SubBonuses[i].OverrideIndex == index && SubBonuses[i].SubBonuses.Length > subindex)
				{
					return SubBonuses[i].SubBonuses[subindex].Affliction;
				}
			}
		}
		return BonusesByLevel[num2];
	}

	public bool IsBonusValid(int index, int subindex, int survival)
	{
		return GetMaximumIndex(index, survival) >= 0;
	}

	public int IndexOf(Affliction affliction)
	{
		for (int i = 0; i < BonusesByLevel.Length; i++)
		{
			if ((object)affliction == BonusesByLevel[i])
			{
				return i;
			}
		}
		for (int j = 0; j < SubBonuses.Length; j++)
		{
			for (int k = 0; k < SubBonuses[j].SubBonuses.Length; k++)
			{
				if ((object)SubBonuses[j].SubBonuses[k].Affliction == affliction)
				{
					return SubBonuses[j].OverrideIndex;
				}
			}
		}
		return -1;
	}

	public int SubIndexOf(Affliction affliction)
	{
		for (int i = 0; i < SubBonuses.Length; i++)
		{
			for (int j = 0; j < SubBonuses[i].SubBonuses.Length; j++)
			{
				if ((object)SubBonuses[i].SubBonuses[j].Affliction == affliction)
				{
					return j;
				}
			}
		}
		return -1;
	}

	public int GetMaximumIndex(int cycle, int survival)
	{
		cycle %= CyclePeriod;
		int num = Mathf.FloorToInt((float)(survival - 1 - cycle) / (float)CyclePeriod);
		return cycle + num * CyclePeriod;
	}
}
