public class CampingSupplies : Item
{
	public static int StackMaximum
	{
		get
        {
            int result = 1;
            GameDifficulty difficulty = GameState.Instance.Difficulty;

            switch (IEModOptions.MaxCampingSupplies)
            {
                case IEModOptions.MaxCampingSuppliesOptions.Default:
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
                            UIDebug.Instance.LogOnceOnlyWarning("Please set StackMaximum in CampingSupplies class for difficulty '" + difficulty + "'.", UIDebug.Department.Programming, 10f);
                            break;
                    }
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Normal_8:
                    result = 8;
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Double_16:
                    result = 16;
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Triple_24:
                    result = 24;
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Quadra_32:
                    result = 32;
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Sixty_four_64:
                    result = 64;
                    break;
                case IEModOptions.MaxCampingSuppliesOptions.Ninety_nine_99:
                case IEModOptions.MaxCampingSuppliesOptions.Disabled:
                    result = 99;
                    break;
            }
            return result;
        }
    }
}
