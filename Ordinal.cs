public static class Ordinal
{
	public static string Get(int number)
	{
		int id = 434;
		if (number < 11 || number > 13)
		{
			switch (number % 10)
			{
			case 1:
				id = 435;
				break;
			case 2:
				id = 436;
				break;
			case 3:
				id = 437;
				break;
			}
		}
		return GUIUtils.Format(id, number);
	}
}
