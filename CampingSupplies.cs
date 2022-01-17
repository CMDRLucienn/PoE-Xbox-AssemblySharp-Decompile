public class CampingSupplies : Item
{
	public static int StackMaximum
	{
		get
		{
			int result = 1;
			GameDifficulty difficulty = GameState.Instance.Difficulty;
			switch (difficulty)
			{
			case GameDifficulty.Easy:
				result = 6;
				break;
			case GameDifficulty.Normal:
				result = 4;
				break;
			case GameDifficulty.Hard:
			case GameDifficulty.PathOfTheDamned:
				result = 2;
				break;
			case GameDifficulty.StoryTime:
				result = 99;
				break;
			default:
				UIDebug.Instance.LogOnceOnlyWarning(string.Concat("Please set StackMaximum in CampingSupplies class for difficulty '", difficulty, "'."), UIDebug.Department.Programming, 10f);
				break;
			}
			return result;
		}
	}
}
