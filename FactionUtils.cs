public static class FactionUtils
{
	private static readonly int[] ReputationRankIDs = new int[4] { 33, 30, 31, 32 };

	private static readonly int[] DispositionAxisIDs = new int[10] { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };

	public static string GetText(int id)
	{
		return StringTableManager.GetText(DatabaseString.StringTableType.Factions, id);
	}

	public static string GetReputationRankString(Reputation.RankType rank)
	{
		if ((int)rank < ReputationRankIDs.Length)
		{
			return GetText(ReputationRankIDs[(int)rank]);
		}
		return string.Empty;
	}

	public static string GetDispositionAxisString(Disposition.Axis axis)
	{
		if ((int)axis < DispositionAxisIDs.Length)
		{
			return GetText(DispositionAxisIDs[(int)axis]);
		}
		return string.Empty;
	}
}
