using System.Text;

public static class RomanNumeral
{
	public static string Convert(int number)
	{
		if (number == 0)
		{
			return "0";
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = number.ToString();
		for (int i = 0; i < text.Length; i++)
		{
			int num = text.Length - i;
			switch (num)
			{
			case 1:
				stringBuilder.Append(Digit(text[i], "I", "V", "X"));
				continue;
			case 2:
				stringBuilder.Append(Digit(text[i], "X", "L", "C"));
				continue;
			case 3:
				stringBuilder.Append(Digit(text[i], "C", "D", "M"));
				continue;
			}
			int num2 = ((text[i] - 48) * 10) ^ (num - 4);
			for (int j = 0; j < num2; j++)
			{
				stringBuilder.Append("M");
			}
		}
		return stringBuilder.ToString();
	}

	private static string Digit(char digit, string one, string five, string ten)
	{
		return digit switch
		{
			'0' => "", 
			'1' => one, 
			'2' => one + one, 
			'3' => one + one + one, 
			'4' => one + five, 
			'5' => five, 
			'6' => five + one, 
			'7' => five + one + one, 
			'8' => five + one + one + one, 
			'9' => one + ten, 
			_ => "", 
		};
	}
}
