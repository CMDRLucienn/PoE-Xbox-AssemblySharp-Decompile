using System.Linq;

public static class UIDataManagerHelper
{
	public class CharacterStats
	{
		public global::CharacterStats.Class CharacterClass;

		public int BaseDeflection;

		public int BaseFortitude;

		public int BaseReflexes;

		public int BaseWill;

		public int MeleeAccuracyBonus;

		public int RangedAccuracyBonus;

		public int MaxHealth;

		public int MaxStamina;

		public int HealthStaminaPerLevel;

		public float ClassHealthMultiplier;
	}

	private static CharacterStats[] data;

	static UIDataManagerHelper()
	{
		data = new CharacterStats[10];
		for (int i = 0; i < data.Length; i++)
		{
			object component = new CharacterStats
			{
				CharacterClass = (global::CharacterStats.Class)(i + 1)
			};
			DataManager.AdjustFromData(ref component, "CharacterStats");
			data[i] = (CharacterStats)component;
		}
	}

	public static GUIUtils.RelativeRating GetRelativeMaxStamina(int stamina)
	{
		int num = data.Min((CharacterStats cs) => cs.MaxStamina);
		int num2 = data.Max((CharacterStats cs) => cs.MaxStamina);
		return GetRelativeRating((float)(stamina - num) / ((float)num2 - (float)num));
	}

	public static GUIUtils.RelativeRating GetRelativeHealthMultiplier(float multiplier)
	{
		if (multiplier <= 3f)
		{
			return GUIUtils.RelativeRating.VERY_LOW;
		}
		if (multiplier <= 4f)
		{
			return GUIUtils.RelativeRating.LOW;
		}
		if (multiplier <= 5f)
		{
			return GUIUtils.RelativeRating.HIGH;
		}
		return GUIUtils.RelativeRating.VERY_HIGH;
	}

	public static GUIUtils.RelativeRating GetRelativeMeleeAccuracyBonus(int bonus)
	{
		int num = data.Min((CharacterStats cs) => cs.MeleeAccuracyBonus);
		int num2 = data.Max((CharacterStats cs) => cs.MeleeAccuracyBonus);
		return GetRelativeRating((float)(bonus - num) / ((float)num2 - (float)num));
	}

	public static GUIUtils.RelativeRating GetRelativeBaseDeflection(int baseDeflection)
	{
		int num = data.Min((CharacterStats cs) => cs.BaseDeflection);
		int num2 = data.Max((CharacterStats cs) => cs.BaseDeflection);
		return GetRelativeRating((float)(baseDeflection - num) / ((float)num2 - (float)num));
	}

	public static GUIUtils.RelativeRating GetRelativeRating(float ratio)
	{
		if ((double)ratio < 0.2)
		{
			return GUIUtils.RelativeRating.VERY_LOW;
		}
		if ((double)ratio < 0.4)
		{
			return GUIUtils.RelativeRating.LOW;
		}
		if ((double)ratio < 0.6)
		{
			return GUIUtils.RelativeRating.AVERAGE;
		}
		if ((double)ratio < 0.8)
		{
			return GUIUtils.RelativeRating.HIGH;
		}
		return GUIUtils.RelativeRating.VERY_HIGH;
	}
}
