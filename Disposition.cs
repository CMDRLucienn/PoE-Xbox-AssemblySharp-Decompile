using System;
using System.Collections.Generic;

[Serializable]
public class Disposition
{
	public enum Axis
	{
		Benevolent,
		Cruel,
		Clever,
		Stoic,
		Aggressive,
		Diplomatic,
		Passionate,
		Rational,
		Honest,
		Deceptive
	}

	public enum Strength
	{
		Minor = 1,
		Average = 3,
		Major = 7
	}

	public enum Rank
	{
		None,
		Rank1,
		Rank2,
		Rank3
	}

	public const int MAX_RANK = 4;

	public int[] BenevolentRanks = new int[4] { 1, 25, 50, 75 };

	public int[] CruelRanks = new int[4] { 1, 25, 50, 75 };

	public int[] CleverRanks = new int[4] { 1, 25, 50, 75 };

	public int[] StoicRanks = new int[4] { 1, 25, 50, 75 };

	public int[] AggressiveRanks = new int[4] { 1, 25, 50, 75 };

	public int[] DiplomaticRanks = new int[4] { 1, 25, 50, 75 };

	public int[] PassionateRanks = new int[4] { 1, 25, 50, 75 };

	public int[] RationalRanks = new int[4] { 1, 25, 50, 75 };

	public int[] HonestRanks = new int[4] { 1, 25, 50, 75 };

	public int[] DeceptiveRanks = new int[4] { 1, 25, 50, 75 };

	public int[] m_dispositions { get; set; }

	public List<int[]> m_pointsPerRank { get; set; }

	public Disposition()
	{
		m_dispositions = new int[Enum.GetValues(typeof(Axis)).Length];
		m_pointsPerRank = new List<int[]>();
	}

	public void Start()
	{
		m_pointsPerRank.Add(BenevolentRanks);
		m_pointsPerRank.Add(CruelRanks);
		m_pointsPerRank.Add(CleverRanks);
		m_pointsPerRank.Add(StoicRanks);
		m_pointsPerRank.Add(AggressiveRanks);
		m_pointsPerRank.Add(DiplomaticRanks);
		m_pointsPerRank.Add(PassionateRanks);
		m_pointsPerRank.Add(RationalRanks);
		m_pointsPerRank.Add(HonestRanks);
		m_pointsPerRank.Add(DeceptiveRanks);
	}

	public void ChangeDisposition(Axis axis, Strength strength)
	{
		int rank = GetRank(axis);
		m_dispositions[(int)axis] += (int)strength;
		int rank2 = GetRank(axis);
		if (rank < 3 && rank2 >= 3 && (bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumDispositionsAtLevel);
		}
		TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.DISPOSITION_GAINED);
	}

	public int GetRank(Axis axis)
	{
		if (m_pointsPerRank.Count == 0)
		{
			return 0;
		}
		int[] array = m_pointsPerRank[(int)axis];
		if (array == null || m_dispositions == null || m_dispositions[(int)axis] == 0)
		{
			return 0;
		}
		int result = 0;
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (m_dispositions[(int)axis] >= array[num])
			{
				result = num + 1;
				break;
			}
		}
		return result;
	}
}
