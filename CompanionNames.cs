public static class CompanionNames
{
	public enum Companions
	{
		Invalid,
		Eder,
		Priest,
		Caroc,
		Aloth,
		Kana,
		Sagani,
		Pallegina,
		Mother,
		Hiravias,
		Calisca,
		Heodan,
		Monk,
		Maneha,
		Px2TalkingItem
	}

	public static string[] s_companionNames = new string[14]
	{
		"c_eder", "c_priest", "c_caroc", "c_aloth", "c_kana", "c_sagani", "c_pallegina", "c_mother", "c_hiravias", "c_calisca",
		"c_heodan", "c_monk", "c_maneha", "o_talking_item_00"
	};

	public static Companions Parse(string name)
	{
		for (int i = 0; i < s_companionNames.Length; i++)
		{
			if (s_companionNames[i] == name)
			{
				return (Companions)(i + 1);
			}
		}
		return Companions.Invalid;
	}
}
