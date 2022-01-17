using System;
using System.Text;
using UnityEngine;

public class UICharacterCreationAttributeToolTip : UICharacterCreationElement
{
	public CharacterStats.AttributeScoreType Attribute;

	private static StringBuilder stringBuilder = new StringBuilder();

	private static UICharacterCreationAttributeToolTip LastUpdated = null;

	private static int[] m_BonusTexts = new int[6] { 372, 367, 1582, 371, 368, 370 };

	protected override void OnDestroy()
	{
		if (LastUpdated == this)
		{
			LastUpdated = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public string GetDescription1Text()
	{
		string text = string.Empty;
		if (!GameState.Mode.Expert)
		{
			CharacterStats.AttributeScoreType[] array = UICharacterCreationEnumSetter.GodTierAttributes[(int)base.Owner.Character.Class];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == Attribute)
				{
					text = GUIUtils.GetText(1726);
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				array = UICharacterCreationEnumSetter.ProTierAttributes[(int)base.Owner.Character.Class];
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == Attribute)
					{
						text = GUIUtils.GetText(1727);
						break;
					}
				}
			}
		}
		stringBuilder.Length = 0;
		stringBuilder.Append(GUIUtils.GetAttributeScoreDescriptionString(Attribute));
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.Append("\n\n");
			stringBuilder.Append(StringUtility.Format(text, GUIUtils.GetClassString(base.Owner.Character.Class, base.Owner.Character.Gender).ToUpper()));
		}
		return stringBuilder.ToString();
	}

	public string GetDescription2Text()
	{
		UICharacterCreationManager.Character character = base.Owner.Character;
		stringBuilder.Length = 0;
		stringBuilder.Append(GUIUtils.GetAttributeScoreTypeString(Attribute));
		stringBuilder.AppendFormat(": {0}", character.BaseStats[(int)Attribute]);
		int num = CharacterStats.RaceAbilityAdjustment[(int)character.Race, (int)Attribute];
		int num2 = CharacterStats.CultureAbilityAdjustment[(int)character.Culture, (int)Attribute];
		if (num != 0)
		{
			if (num > 0)
			{
				stringBuilder.AppendFormat(" + {0} (", num);
			}
			else
			{
				stringBuilder.AppendFormat(" - {0} (", Math.Abs(num));
			}
			stringBuilder.Append(GUIUtils.GetRaceString(character.Race, character.Gender));
			stringBuilder.Append(")");
		}
		if (num2 != 0)
		{
			if (num2 > 0)
			{
				stringBuilder.AppendFormat(" + {0} (", num2);
			}
			else
			{
				stringBuilder.AppendFormat(" - {0} (", Math.Abs(num2));
			}
			stringBuilder.Append(GUIUtils.GetCultureString(character.Culture, character.Gender));
			stringBuilder.Append(")");
		}
		if (num != 0 || num2 != 0)
		{
			stringBuilder.AppendFormat(" = {0}", character.BaseStats[(int)Attribute] + num + num2);
		}
		stringBuilder.Append("\n\n");
		int num3 = character.BaseStats[(int)Attribute] + num + num2;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		switch (Attribute)
		{
		case CharacterStats.AttributeScoreType.Might:
			num4 = CharacterStats.GetStatDamageHealMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Constitution:
			num4 = CharacterStats.GetStatHealthStaminaMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Dexterity:
			num4 = CharacterStats.GetStatAttackSpeedMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Perception:
			num4 = CharacterStats.ComputeBaseInterrupt(num3);
			num5 = CharacterStats.GetStatBonusDeflection(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = false;
			flag2 = false;
			flag3 = false;
			break;
		case CharacterStats.AttributeScoreType.Intellect:
			num4 = CharacterStats.GetStatEffectRadiusMultiplier(num3);
			num5 = CharacterStats.GetStatEffectDurationMultiplier(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = true;
			flag3 = false;
			break;
		case CharacterStats.AttributeScoreType.Resolve:
			num4 = CharacterStats.ComputeBaseConcentration(num3);
			num5 = CharacterStats.GetStatBonusDeflection(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = false;
			flag2 = false;
			flag3 = false;
			break;
		}
		string text = (flag ? TextUtils.NumberBonus(100f * (num4 - 1f), "####0") : TextUtils.NumberBonus(num4, "####0"));
		string text2 = (flag2 ? TextUtils.NumberBonus(100f * (num5 - 1f), "####0") : TextUtils.NumberBonus(num5, "####0"));
		string text3 = (flag3 ? TextUtils.NumberBonus(100f * (num6 - 1f), "####0") : TextUtils.NumberBonus(num6, "####0"));
		stringBuilder.Append(GUIUtils.Format(m_BonusTexts[(int)Attribute], text, text2, text3));
		return stringBuilder.ToString();
	}

	public string GetTooltipContent()
	{
		string text = "";
		if (!GameState.Mode.Expert)
		{
			CharacterStats.AttributeScoreType[] array = UICharacterCreationEnumSetter.GodTierAttributes[(int)base.Owner.Character.Class];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == Attribute)
				{
					text = GUIUtils.GetText(1726);
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				array = UICharacterCreationEnumSetter.ProTierAttributes[(int)base.Owner.Character.Class];
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == Attribute)
					{
						text = GUIUtils.GetText(1727);
						break;
					}
				}
			}
		}
		stringBuilder.Remove(0, stringBuilder.Length);
		UICharacterCreationManager.Character character = base.Owner.Character;
		stringBuilder.Append(GUIUtils.GetAttributeScoreDescriptionString(Attribute));
		stringBuilder.Append("\n\n");
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.Append(StringUtility.Format(text, GUIUtils.GetClassString(base.Owner.Character.Class, base.Owner.Character.Gender).ToUpper()));
			stringBuilder.Append("\n\n");
		}
		stringBuilder.Append(GUIUtils.GetAttributeScoreTypeString(Attribute));
		stringBuilder.AppendFormat(": {0}", character.BaseStats[(int)Attribute]);
		int num = CharacterStats.RaceAbilityAdjustment[(int)character.Race, (int)Attribute];
		int num2 = CharacterStats.CultureAbilityAdjustment[(int)character.Culture, (int)Attribute];
		if (num != 0)
		{
			if (num > 0)
			{
				stringBuilder.AppendFormat(" + {0} (", num);
			}
			else
			{
				stringBuilder.AppendFormat(" - {0} (", Math.Abs(num));
			}
			stringBuilder.Append(GUIUtils.GetRaceString(character.Race, character.Gender));
			stringBuilder.Append(")");
		}
		if (num2 != 0)
		{
			if (num2 > 0)
			{
				stringBuilder.AppendFormat(" + {0} (", num2);
			}
			else
			{
				stringBuilder.AppendFormat(" - {0} (", Math.Abs(num2));
			}
			stringBuilder.Append(GUIUtils.GetCultureString(character.Culture, character.Gender));
			stringBuilder.Append(")");
		}
		if (num != 0 || num2 != 0)
		{
			stringBuilder.AppendFormat(" = {0}", character.BaseStats[(int)Attribute] + num + num2);
		}
		stringBuilder.Append("\n\n");
		int num3 = character.BaseStats[(int)Attribute] + num + num2;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		switch (Attribute)
		{
		case CharacterStats.AttributeScoreType.Might:
			num4 = CharacterStats.GetStatDamageHealMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Constitution:
			num4 = CharacterStats.GetStatHealthStaminaMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Dexterity:
			num4 = CharacterStats.GetStatAttackSpeedMultiplier(num3);
			num5 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = false;
			break;
		case CharacterStats.AttributeScoreType.Perception:
			num4 = CharacterStats.ComputeBaseInterrupt(num3);
			num5 = CharacterStats.GetStatBonusDeflection(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = false;
			flag2 = false;
			flag3 = false;
			break;
		case CharacterStats.AttributeScoreType.Intellect:
			num4 = CharacterStats.GetStatEffectRadiusMultiplier(num3);
			num5 = CharacterStats.GetStatEffectDurationMultiplier(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = true;
			flag2 = true;
			flag3 = false;
			break;
		case CharacterStats.AttributeScoreType.Resolve:
			num4 = CharacterStats.ComputeBaseConcentration(num3);
			num5 = CharacterStats.GetStatBonusDeflection(num3);
			num6 = CharacterStats.GetStatDefenseTypeBonus(num3);
			flag = false;
			flag2 = false;
			flag3 = false;
			break;
		}
		string text2 = (flag ? TextUtils.NumberBonus(100f * (num4 - 1f), "####0") : TextUtils.NumberBonus(num4, "####0"));
		string text3 = (flag2 ? TextUtils.NumberBonus(100f * (num5 - 1f), "####0") : TextUtils.NumberBonus(num5, "####0"));
		string text4 = (flag3 ? TextUtils.NumberBonus(100f * (num6 - 1f), "####0") : TextUtils.NumberBonus(num6, "####0"));
		stringBuilder.Append(GUIUtils.Format(m_BonusTexts[(int)Attribute], text2, text3, text4));
		return stringBuilder.ToString();
	}

	public override void SignalValueChanged(ValueType type)
	{
		if ((type == ValueType.Attribute || type == ValueType.All) && this == LastUpdated)
		{
			SetText();
		}
	}

	private void SetText()
	{
		UICharacterCreationStringGetter[] array = UnityEngine.Object.FindObjectsOfType<UICharacterCreationStringGetter>();
		foreach (UICharacterCreationStringGetter uICharacterCreationStringGetter in array)
		{
			if ((bool)uICharacterCreationStringGetter)
			{
				if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC2)
				{
					uICharacterCreationStringGetter.SetText(GetDescription2Text());
				}
				else if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC)
				{
					uICharacterCreationStringGetter.SetText(GetDescription1Text());
				}
				else if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE)
				{
					uICharacterCreationStringGetter.SetText(GUIUtils.GetAttributeScoreTypeString(Attribute));
				}
			}
		}
	}

	private void UnSetText()
	{
		UICharacterCreationStringGetter[] array = UnityEngine.Object.FindObjectsOfType<UICharacterCreationStringGetter>();
		foreach (UICharacterCreationStringGetter uICharacterCreationStringGetter in array)
		{
			if ((bool)uICharacterCreationStringGetter)
			{
				if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC2)
				{
					uICharacterCreationStringGetter.SetText(string.Empty);
				}
				else if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC)
				{
					uICharacterCreationStringGetter.SetText(GUIUtils.GetText(1809));
				}
				else if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE)
				{
					uICharacterCreationStringGetter.SetText(GUIUtils.GetText(306));
				}
			}
		}
	}

	private void OnHover(bool isOver)
	{
		if (isOver)
		{
			SetText();
			LastUpdated = this;
		}
	}
}
