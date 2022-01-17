public static class RecordAggregator
{
	public enum PartyStat
	{
		ELAPSED_TIME,
		TRAVEL_TIME,
		TIME_IN_COMBAT,
		TOTAL_ENEMIES_DEFEATED,
		MOST_POWERFUL_ENEMY_DEFEATED,
		MOST_TIME_IN_PARTY,
		MOST_ENEMIES_DEFEATED,
		MOST_TOTAL_DAMAGE_DONE,
		HIGHEST_SINGLE_TARGET_DAMAGE_HIT,
		MOST_CRITICAL_HITS,
		MOST_HITS,
		MOST_DAMAGE_TAKEN,
		MOST_TIMES_KNOCKED_OUT,
		COUNT
	}

	public enum PersonalStat
	{
		TIME_IN_PARTY,
		PERSONAL_TIME_IN_COMBAT,
		TOTAL_ENEMIES_DEFEATED,
		MOST_POWERFUL_ENEMY_DEFEATED,
		TOTAL_DAMAGE_DONE,
		HIGHEST_SINGLE_TARGET_DAMAGE_HIT,
		TOTAL_CRITICAL_HITS,
		TOTAL_HITS,
		TOTAL_DAMAGE_TAKEN,
		TOTAL_TIMES_KNOCKED_OUT,
		COUNT
	}

	private static int[] PartyStatStrings = new int[13]
	{
		380, 381, 382, 383, 384, 385, 386, 387, 388, 716,
		717, 718, 719
	};

	private static int[] PersonalStatStrings = new int[10] { 762, 382, 763, 384, 764, 388, 765, 766, 767, 768 };

	public static string GetPartyStatValue(PartyStat stat)
	{
		return stat switch
		{
			PartyStat.ELAPSED_TIME => (WorldTime.Instance.CurrentTime - WorldTime.Instance.AdventureStart).ToString(), 
			PartyStat.TRAVEL_TIME => WorldTime.Instance.TimeSpentTravelling.ToString(), 
			PartyStat.TIME_IN_COMBAT => WorldTime.Instance.TimeInCombat.ToString(), 
			_ => RecordKeeper.Instance.GetStatValue(stat), 
		};
	}

	public static string GetPartyStatLine(PartyStat stat)
	{
		return "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.DullGreen) + "]" + GetPartyStatTag(stat) + "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.White) + "]: " + GetPartyStatValue(stat);
	}

	public static string GetPartyStatTag(PartyStat stat)
	{
		return GUIUtils.GetText(PartyStatStrings[(int)stat]);
	}

	public static string GetPersonalStatValue(PersonalStat stat, PartyMemberAI selected)
	{
		return RecordKeeper.Instance.GetStatValue(stat, selected);
	}

	public static string GetPersonalStatLine(PersonalStat stat, PartyMemberAI selected)
	{
		return "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.DullGreen) + "]" + GetPersonalStatTag(stat, CharacterStats.GetGender(selected)) + "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.White) + "]: " + GetPersonalStatValue(stat, selected);
	}

	public static string GetPersonalStatTag(PersonalStat stat, Gender gender)
	{
		return GUIUtils.GetText(PersonalStatStrings[(int)stat], gender);
	}
}
