public static class GameGlobalVariables
{
	public static bool IsPlayerWatcher()
	{
		return GlobalVariables.Instance.GetVariable("bPlayerWatcher") > 0;
	}

	public static bool HasStartedPX2()
	{
		return GlobalVariables.Instance.GetVariable("b_PX2_Started") > 0;
	}

	public static bool HasFinishedPX1()
	{
		return GlobalVariables.Instance.GetVariable("b_PX1_Completed") > 0;
	}
}
