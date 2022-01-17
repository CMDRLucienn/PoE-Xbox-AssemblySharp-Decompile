using UnityEngine;

public class UICharacterSheetSkillLine : UICharacterSheetContentLine
{
	public UILabel Name;

	public UILabel Value;

	public UILabel Bonus;

	public GameObject Collider;

	public CharacterStats.SkillType Skill;

	private static int[] m_BonusTexts = new int[6] { -1, 2259, 1657, 1658, 2258, -1 };

	public override void Load(CharacterStats stats)
	{
		int value = stats.CalculateSkill(Skill);
		Bonus.text = GetBonusTextLine(Skill, stats.CalculateSkill(Skill));
		switch (Skill)
		{
		case CharacterStats.SkillType.Stealth:
			Load(CharacterStats.SkillType.Stealth, value);
			break;
		case CharacterStats.SkillType.Athletics:
			Load(CharacterStats.SkillType.Athletics, value);
			break;
		case CharacterStats.SkillType.Lore:
			Load(CharacterStats.SkillType.Lore, value);
			break;
		case CharacterStats.SkillType.Mechanics:
			Load(CharacterStats.SkillType.Mechanics, value);
			break;
		case CharacterStats.SkillType.Survival:
			Load(CharacterStats.SkillType.Survival, value);
			break;
		case CharacterStats.SkillType.Crafting:
			Load(CharacterStats.SkillType.Crafting, value);
			break;
		}
	}

	private static string GetPercentBonus(float amt)
	{
		return TextUtils.NumberBonus(100f * (amt - 1f), "####0");
	}

	private static string GetBonus(float amt)
	{
		return TextUtils.NumberBonus(amt, "####0");
	}

	public static string GetBonusTextLine(CharacterStats.SkillType skill, int skillValue)
	{
		string result = "";
		switch (skill)
		{
		case CharacterStats.SkillType.Stealth:
			result = "";
			break;
		case CharacterStats.SkillType.Athletics:
			result = ((skillValue <= 0) ? "" : GUIUtils.FormatWithLinks(m_BonusTexts[(int)skill], 20f + CharacterStats.GetSecondWindAthleticsBonus(skillValue)));
			break;
		case CharacterStats.SkillType.Lore:
			result = GUIUtils.FormatWithLinks(1657, Consumable.GetMaxUsableScrollLevel(skillValue));
			break;
		case CharacterStats.SkillType.Mechanics:
			result = GUIUtils.FormatWithLinks(m_BonusTexts[(int)skill], skillValue, skillValue, TextUtils.NumberBonus(CharacterStats.GetStatTrapAccuracyBonus(skillValue)));
			break;
		case CharacterStats.SkillType.Survival:
			result = ((skillValue <= 0) ? "" : GUIUtils.FormatWithLinks(m_BonusTexts[(int)skill], skillValue));
			break;
		case CharacterStats.SkillType.Crafting:
			result = "";
			break;
		}
		return result;
	}

	private void Load(CharacterStats.SkillType type, int value)
	{
		Name.text = GUIUtils.GetSkillTypeString(Skill);
		Value.text = value.ToString();
		UIStatBreakdownTrigger uIStatBreakdownTrigger = Name.GetComponent<UIStatBreakdownTrigger>();
		if (uIStatBreakdownTrigger == null)
		{
			uIStatBreakdownTrigger = Name.gameObject.AddComponent<UIStatBreakdownTrigger>();
		}
		uIStatBreakdownTrigger.ModifiedStat = StatusEffect.SkillTypeToModifiedStat(type);
	}
}
