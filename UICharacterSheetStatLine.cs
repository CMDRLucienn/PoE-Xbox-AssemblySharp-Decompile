using UnityEngine;

public class UICharacterSheetStatLine : UICharacterSheetContentLine
{
	public UILabel Label;

	public UILabel Value;

	public UILabel Bonus;

	public GameObject Collider;

	public CharacterStats.AttributeScoreType Attribute;

	private static int[] m_BonusTexts = new int[6] { 372, 367, 1582, 371, 368, 370 };

	public override void Load(CharacterStats stats)
	{
		switch (Attribute)
		{
		case CharacterStats.AttributeScoreType.Might:
			Load(CharacterStats.AttributeScoreType.Might, stats.Might);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetPercentBonus(stats.StatDamageHealMultiplier), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Might)));
			break;
		case CharacterStats.AttributeScoreType.Constitution:
			Load(CharacterStats.AttributeScoreType.Constitution, stats.Constitution);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetPercentBonus(stats.StatHealthStaminaMultiplier), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Constitution)));
			break;
		case CharacterStats.AttributeScoreType.Dexterity:
			Load(CharacterStats.AttributeScoreType.Dexterity, stats.Dexterity);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetPercentBonus(stats.StatAttackSpeedMultiplier), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Dexterity)));
			break;
		case CharacterStats.AttributeScoreType.Perception:
			Load(CharacterStats.AttributeScoreType.Perception, stats.Perception);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetBonus(stats.ComputeBaseInterrupt()), GetBonus(CharacterStats.GetStatBonusAccuracy(stats.Perception)), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Perception)));
			break;
		case CharacterStats.AttributeScoreType.Intellect:
			Load(CharacterStats.AttributeScoreType.Intellect, stats.Intellect);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetPercentBonus(stats.StatEffectRadiusMultiplier), GetPercentBonus(stats.StatEffectDurationMultiplier), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Intellect)));
			break;
		case CharacterStats.AttributeScoreType.Resolve:
			Load(CharacterStats.AttributeScoreType.Resolve, stats.Resolve);
			Bonus.text = GUIUtils.FormatWithLinks(m_BonusTexts[(int)Attribute], GetBonus(stats.ComputeBaseConcentration()), GetBonus(CharacterStats.GetStatBonusDeflection(stats.Resolve)), GetBonus(CharacterStats.GetStatDefenseTypeBonus(stats.Resolve)));
			break;
		}
	}

	private static string GetPercentBonus(float amt)
	{
		return TextUtils.NumberBonus(100f * (amt - 1f), "####0");
	}

	private static string GetBonus(int amt)
	{
		return TextUtils.NumberBonus(amt);
	}

	private static string GetBonus(float amt)
	{
		return TextUtils.NumberBonus(amt, "####0");
	}

	private void Load(CharacterStats.AttributeScoreType type, int value)
	{
		Label.text = GUIUtils.GetAttributeScoreTypeShortString(type);
		Value.text = value.ToString();
		UIStatBreakdownTrigger uIStatBreakdownTrigger = Label.GetComponent<UIStatBreakdownTrigger>();
		if (uIStatBreakdownTrigger == null)
		{
			uIStatBreakdownTrigger = Label.gameObject.AddComponent<UIStatBreakdownTrigger>();
		}
		uIStatBreakdownTrigger.ModifiedStat = StatusEffect.AttributeTypeToModifiedStat(type);
	}
}
