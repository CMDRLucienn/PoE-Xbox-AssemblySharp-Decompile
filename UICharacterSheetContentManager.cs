using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UICharacterSheetContentManager : MonoBehaviour
{
	private List<UICharacterSheetContentLine> m_ContentLines;

	private List<UICharacterSheetContentLine> m_CompanionContentLines;

	private UICharacterSheetCompanionContent m_CompanionParent;

	public Color PrefixTagLabelColor;

	private UITable m_Table;

	public UIGrid StatGrid;

	public UIDraggablePanel Panel;

	public UILabel ExperienceLabel;

	public UISlider ExperienceSlider;

	public static string FormatPrefixed(string prefix, string text)
	{
		return "[" + NGUITools.EncodeColor(UICharacterSheetManager.Instance.Content.PrefixTagLabelColor) + "]" + prefix + ":[-] " + text;
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_ContentLines == null)
		{
			m_Table = GetComponent<UITable>();
			UICharacterSheetContentLine[] componentsInChildren = GetComponentsInChildren<UICharacterSheetContentLine>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Initialize();
			}
			m_ContentLines = new List<UICharacterSheetContentLine>();
			m_CompanionContentLines = new List<UICharacterSheetContentLine>();
			m_CompanionParent = GetComponentsInChildren<UICharacterSheetCompanionContent>(includeInactive: true).FirstOrDefault();
			FindContentLines(base.transform, isCompanion: false);
		}
	}

	private void FindContentLines(Transform parent, bool isCompanion)
	{
		UICharacterSheetContentLine component = parent.GetComponent<UICharacterSheetContentLine>();
		if ((bool)component)
		{
			if (isCompanion)
			{
				m_CompanionContentLines.Add(component);
			}
			else
			{
				m_ContentLines.Add(component);
			}
		}
		isCompanion |= (bool)parent.GetComponent<UICharacterSheetCompanionContent>();
		foreach (Transform item in parent)
		{
			FindContentLines(item, isCompanion);
		}
	}

	private void Reposition()
	{
		UITable[] componentsInChildren = GetComponentsInChildren<UITable>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reposition();
		}
		UIGrid[] componentsInChildren2 = GetComponentsInChildren<UIGrid>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].Reposition();
		}
		m_Table.Reposition();
		Panel.RestrictWithinBounds(instant: true);
	}

	public void LoadCharacter(GameObject go)
	{
		Init();
		CharacterStats component = go.GetComponent<CharacterStats>();
		foreach (UICharacterSheetContentLine contentLine in m_ContentLines)
		{
			contentLine.Load(component);
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(go);
		if ((bool)gameObject)
		{
			CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
			if ((bool)component2)
			{
				m_CompanionParent.gameObject.SetActive(value: true);
				foreach (UICharacterSheetContentLine companionContentLine in m_CompanionContentLines)
				{
					companionContentLine.Load(component2);
				}
			}
			else
			{
				m_CompanionParent.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_CompanionParent.gameObject.SetActive(value: false);
		}
		int num = CharacterStats.ExperienceNeededForLevel(component.Level);
		int num2 = CharacterStats.ExperienceNeededForLevel(component.Level + 1);
		string text;
		if (component.Level >= CharacterStats.PlayerLevelCap)
		{
			text = component.Experience + " - " + GUIUtils.GetText(1934);
			ExperienceSlider.sliderValue = 1f;
		}
		else
		{
			text = GUIUtils.Format(451, component.Experience, num2);
			ExperienceSlider.sliderValue = (float)(component.Experience - num) / (float)(num2 - num);
		}
		ExperienceLabel.text = UICharacterSheetContentLine.FormatPrefixed(GUIUtils.GetText(375), text);
		Reposition();
	}

	public static string GetDamageEffectsInverted(CharacterStats stats, AttackBase attack, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Equippable equippable = (attack ? attack.GetComponent<Equippable>() : null);
		Weapon weapon = equippable as Weapon;
		AppendKeyValue(stringBuilder, GameUtilities.GetDisplayName(attack.gameObject), attack.DamageData.GetBaseRangeString(stats), delimiter);
		AppendKeyValueMultiplier(stringBuilder, GameUtilities.GetDisplayName(attack.gameObject), attack.DamageMultiplier, delimiter, colorStyle);
		for (int i = 0; i < stats.ActiveStatusEffects.Count; i++)
		{
			StatusEffect statusEffect = stats.ActiveStatusEffects[i];
			float num = statusEffect.AdjustDamageMultiplier(stats.gameObject, null, attack);
			if (num != 1f)
			{
				AppendStatusEffect(stringBuilder, stats, statusEffect, TextUtils.MultiplierAsPercentBonus(num), delimiter, colorStyle);
			}
		}
		if ((bool)equippable)
		{
			for (int j = 0; j < equippable.AttachedItemMods.Count; j++)
			{
				StatusEffectParams[] statusEffectsOnLaunch = equippable.AttachedItemMods[j].Mod.StatusEffectsOnLaunch;
				for (int k = 0; k < statusEffectsOnLaunch.Length; k++)
				{
					float damageMultiplierForUi = statusEffectsOnLaunch[k].GetDamageMultiplierForUi(stats.gameObject, attack);
					if (damageMultiplierForUi != 1f)
					{
						AppendKeyValueMultiplier(stringBuilder, equippable.AttachedItemMods[j].Mod.DisplayName.GetText(), damageMultiplierForUi, delimiter, colorStyle);
					}
				}
			}
		}
		if ((bool)weapon)
		{
			if (weapon.UniversalType)
			{
				float[] array = new float[6];
				for (int l = 0; l < stats.ActiveAbilities.Count; l++)
				{
					WeaponSpecialization weaponSpecialization = stats.ActiveAbilities[l] as WeaponSpecialization;
					if ((bool)weaponSpecialization)
					{
						array[(int)weaponSpecialization.SpecializationCategory] += weaponSpecialization.BonusDamageMult - 1f;
					}
				}
				float num2 = 0f;
				WeaponSpecializationData.Category category = WeaponSpecializationData.Category.Count;
				for (int m = 0; m < array.Length; m++)
				{
					if (array[m] > num2)
					{
						category = (WeaponSpecializationData.Category)m;
						num2 = array[m];
					}
				}
				for (int n = 0; n < stats.ActiveAbilities.Count; n++)
				{
					WeaponSpecialization weaponSpecialization2 = stats.ActiveAbilities[n] as WeaponSpecialization;
					if ((bool)weaponSpecialization2 && weaponSpecialization2.SpecializationCategory == category)
					{
						AppendKeyValueMultiplier(stringBuilder, weaponSpecialization2.Name(), weaponSpecialization2.BonusDamageMult, delimiter, colorStyle);
					}
				}
			}
			else
			{
				for (int num3 = 0; num3 < stats.ActiveAbilities.Count; num3++)
				{
					WeaponSpecialization weaponSpecialization3 = stats.ActiveAbilities[num3] as WeaponSpecialization;
					if ((bool)weaponSpecialization3 && WeaponSpecialization.WeaponSpecializationApplies(attack, weaponSpecialization3.SpecializationCategory))
					{
						AppendKeyValueMultiplier(stringBuilder, weaponSpecialization3.Name(), weaponSpecialization3.BonusDamageMult, delimiter, colorStyle);
					}
				}
			}
		}
		for (int num4 = 0; num4 < stats.ActiveStatusEffects.Count; num4++)
		{
			StatusEffect statusEffect2 = stats.ActiveStatusEffects[num4];
			float num5 = statusEffect2.AdjustDamage(stats.gameObject, null, attack);
			if (num5 != 0f)
			{
				AppendStatusEffect(stringBuilder, stats, statusEffect2, TextUtils.NumberBonus(num5), delimiter, colorStyle);
			}
		}
		if ((bool)equippable)
		{
			for (int num6 = 0; num6 < equippable.AttachedItemMods.Count; num6++)
			{
				StatusEffectParams[] statusEffectsOnLaunch = equippable.AttachedItemMods[num6].Mod.StatusEffectsOnLaunch;
				for (int k = 0; k < statusEffectsOnLaunch.Length; k++)
				{
					int bonus = (int)statusEffectsOnLaunch[k].GetDamageBonusForUi(stats.gameObject, attack);
					AppendKeyValueBonus(stringBuilder, equippable.AttachedItemMods[num6].Mod.DisplayName.GetText(), bonus, delimiter, colorStyle);
				}
			}
		}
		AppendKeyValueMultiplier(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Might), stats.StatDamageHealMultiplier, delimiter, colorStyle);
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetDamageThresholdEffectsInverted(CharacterStats stats, DamagePacket.DamageType damageType, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Equipment component = stats.GetComponent<Equipment>();
		Armor armor = null;
		if ((bool)component && (bool)component.CurrentItems.Chest)
		{
			armor = component.CurrentItems.Chest.GetComponent<Armor>();
			if ((bool)armor)
			{
				AppendKeyValue(stringBuilder, component.CurrentItems.Chest.Name, armor.GetDamageThreshold(stats.gameObject).ToString("#0"), delimiter);
			}
		}
		for (int i = 0; i < stats.ActiveStatusEffects.Count; i++)
		{
			StatusEffect statusEffect = stats.ActiveStatusEffects[i];
			Armor armor2 = null;
			if (statusEffect.Slot != Equippable.EquipmentSlot.None && (bool)statusEffect.EquipmentOrigin)
			{
				armor2 = statusEffect.EquipmentOrigin.GetComponent<Armor>();
			}
			if ((bool)armor2 && (statusEffect.Params.DmgType == DamagePacket.DamageType.All || statusEffect.Params.DmgType == damageType))
			{
				float num = statusEffect.AdjustDamageThreshold(stats.gameObject, damageType, ignoreArmor: true);
				if (num != 0f)
				{
					AppendStatusEffect(stringBuilder, stats, statusEffect, TextUtils.NumberBonus(num, "#0"), delimiter, colorStyle);
				}
			}
			else if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.BonusDTFromArmor)
			{
				AppendStatusEffect(stringBuilder, stats, statusEffect, TextUtils.NumberBonus(statusEffect.CurrentAppliedValueForUi, "#0"), delimiter, colorStyle);
			}
		}
		if ((bool)armor)
		{
			float multiplier = armor.AdjustForDamageType(1f, damageType);
			string text = GUIUtils.GetText(2361);
			AppendKeyValueMultiplier(stringBuilder, text, multiplier, delimiter, colorStyle);
		}
		for (int j = 0; j < stats.ActiveStatusEffects.Count; j++)
		{
			StatusEffect statusEffect2 = stats.ActiveStatusEffects[j];
			Armor armor3 = null;
			if (statusEffect2.Slot != Equippable.EquipmentSlot.None && (bool)statusEffect2.EquipmentOrigin)
			{
				armor3 = statusEffect2.EquipmentOrigin.GetComponent<Armor>();
			}
			if (!armor3 && (statusEffect2.Params.DmgType == DamagePacket.DamageType.All || statusEffect2.Params.DmgType == damageType))
			{
				float num2 = statusEffect2.AdjustDamageThreshold(stats.gameObject, damageType, ignoreArmor: true);
				if (num2 != 0f)
				{
					AppendStatusEffect(stringBuilder, stats, statusEffect2, TextUtils.NumberBonus(num2, "#0"), delimiter, colorStyle);
				}
			}
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetInterruptEffectsInverted(CharacterStats stats, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendKeyValue(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Perception), stats.ComputeBaseInterrupt().ToString(), delimiter);
		foreach (StatusEffect item in stats.FindStatusEffectsOfType(StatusEffect.ModifiedStat.InterruptBonus))
		{
			AppendStatusEffect(stringBuilder, stats, item, TextUtils.NumberBonus(item.CurrentAppliedValue), delimiter, colorStyle);
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetConcentrationEffectsInverted(CharacterStats stats, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (AttackData.Instance.BaseConcentration != 0f)
		{
			AppendKeyValue(stringBuilder, GUIUtils.GetText(396), AttackData.Instance.BaseConcentration.ToString("#0"), delimiter);
		}
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Resolve), stats.ComputeBaseConcentration(), delimiter, colorStyle);
		foreach (StatusEffect item in stats.FindStatusEffectsOfType(StatusEffect.ModifiedStat.ConcentrationBonus))
		{
			AppendStatusEffect(stringBuilder, stats, item, TextUtils.NumberBonus(item.CurrentAppliedValue), delimiter, colorStyle);
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetAccuracyEffectsInverted(CharacterStats stats, AttackBase attack, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendKeyValueBonus(bonus: (!(attack is AttackRanged)) ? stats.MeleeAccuracyBonus : stats.RangedAccuracyBonus, builder: stringBuilder, key: GUIUtils.GetClassString(stats.CharacterClass, stats.Gender), delimiter: delimiter, colorStyle: colorStyle);
		Item item = (attack ? attack.GetComponent<Item>() : null);
		int num = 0;
		if ((bool)attack)
		{
			num += attack.AccuracyBonusTotal;
		}
		if (!attack || !attack.HasImpactCountRemaining)
		{
			if ((bool)attack)
			{
				num += (int)attack.FindEquipmentLaunchAccuracyBonus();
			}
			for (int i = 0; i < stats.ActiveAbilities.Count; i++)
			{
				if ((bool)stats.ActiveAbilities[i])
				{
					DamageInfo damageInfo = new DamageInfo(null, 0f, attack);
					stats.ActiveAbilities[i].UIGetBonusAccuracyOnAttack(stats.gameObject, damageInfo);
					AppendKeyValueBonus(stringBuilder, stats.ActiveAbilities[i].Name(), damageInfo.AccuracyRating, delimiter, colorStyle);
				}
			}
		}
		AppendKeyValueBonus(stringBuilder, item ? item.Name : GUIUtils.GetText(396), num, delimiter, colorStyle);
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Perception), stats.StatBonusAccuracy, delimiter, colorStyle);
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetText(373), stats.AccuracyBonusFromLevel, delimiter, colorStyle);
		for (int j = 0; j < stats.ActiveStatusEffects.Count; j++)
		{
			StatusEffect statusEffect = stats.ActiveStatusEffects[j];
			int num2 = statusEffect.AdjustAccuracy(stats.gameObject, null, attack);
			if (num2 != 0)
			{
				AppendStatusEffect(stringBuilder, stats, statusEffect, TextUtils.NumberBonus(num2), delimiter, colorStyle);
			}
		}
		Equipment component = stats.GetComponent<Equipment>();
		if ((bool)component)
		{
			Shield equippedShield = component.EquippedShield;
			if ((bool)equippedShield)
			{
				Item component2 = equippedShield.GetComponent<Item>();
				AppendKeyValueBonus(stringBuilder, component2 ? component2.Name : "*null*", equippedShield.AccuracyBonus, delimiter, colorStyle);
			}
			else if (!component.TwoHandedWeapon && !component.DualWielding)
			{
				AppendKeyValueBonus(stringBuilder, GUIUtils.GetText(1806), AttackData.Instance.Single1HWeapNoShieldAccuracyBonus, delimiter, colorStyle);
			}
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetSkillEffectsInverted(CharacterStats stats, CharacterStats.SkillType skill, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendKeyValue(stringBuilder, GUIUtils.GetText(396), stats.CalculateSkillLevel(skill).ToString(), delimiter);
		int bonus = CharacterStats.ClassSkillAdjustment[(int)stats.CharacterClass, (int)skill];
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetClassString(stats.CharacterClass, stats.Gender), bonus, delimiter, colorStyle);
		int bonus2 = CharacterStats.BackgroundSkillAdjustment[(int)stats.CharacterBackground, (int)skill];
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetBackgroundString(stats.CharacterBackground, stats.Gender), bonus2, delimiter, colorStyle);
		foreach (GenericTalent activeTalent in stats.ActiveTalents)
		{
			if (activeTalent.ContainsSkillBonus(skill))
			{
				AppendKeyValueBonus(stringBuilder, activeTalent.Name(stats.gameObject), activeTalent.GetSkillBonus(skill), delimiter, colorStyle);
			}
		}
		StatusEffect.ModifiedStat statType = StatusEffect.SkillTypeToModifiedStat(skill);
		foreach (StatusEffect item in stats.FindStatusEffectsOfType(statType))
		{
			AppendStatusEffect(stringBuilder, stats, item, TextUtils.NumberBonus(item.CurrentAppliedValue), delimiter, colorStyle);
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetAttributeEffectsInverted(CharacterStats stats, CharacterStats.AttributeScoreType attribute, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string key = GUIUtils.GetText(396);
		int num = stats.GetBaseAttribute(attribute);
		if (stats.HasStatusEffectOfType(StatusEffect.ModifiedStat.SetBaseAttribute))
		{
			foreach (StatusEffect item in stats.FindStatusEffectsOfType(StatusEffect.ModifiedStat.SetBaseAttribute))
			{
				if (item.Params.AttributeType == attribute)
				{
					key = item.GetDisplayName();
					num = (int)item.CurrentAppliedValue;
					break;
				}
			}
		}
		AppendKeyValue(stringBuilder, key, num.ToString(), delimiter);
		int bonus = CharacterStats.RaceAbilityAdjustment[(int)stats.CharacterRace, (int)attribute];
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetRaceString(stats.CharacterRace, stats.Gender), bonus, delimiter, colorStyle);
		int bonus2 = CharacterStats.CultureAbilityAdjustment[(int)stats.CharacterCulture, (int)attribute];
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetCultureString(stats.CharacterCulture, stats.Gender), bonus2, delimiter, colorStyle);
		StatusEffect.ModifiedStat modifiedStat = StatusEffect.AttributeTypeToModifiedStat(attribute);
		foreach (StatusEffect item2 in stats.FindStatusEffectsOfType(modifiedStat))
		{
			AppendStatusEffect(stringBuilder, stats, item2, TextUtils.NumberBonus(item2.CurrentAppliedValue), delimiter, colorStyle);
		}
		if (modifiedStat == StatusEffect.ModifiedStat.Resolve)
		{
			foreach (StatusEffect item3 in stats.FindStatusEffectsOfType(StatusEffect.ModifiedStat.DrainResolveForDeflection))
			{
				AppendStatusEffect(stringBuilder, stats, item3, TextUtils.NumberBonus(item3.CurrentAppliedValue), delimiter, colorStyle);
			}
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string GetDefenseEffectsInverted(CharacterStats stats, CharacterStats.DefenseType defense, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendKeyValue(stringBuilder, GUIUtils.GetText(396), stats.GetBaseDefense(defense).ToString(), delimiter);
		AppendKeyValueBonus(stringBuilder, GUIUtils.GetText(373), stats.DefenseBonusFromLevel, delimiter, colorStyle);
		switch (defense)
		{
		case CharacterStats.DefenseType.Deflect:
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Resolve), CharacterStats.GetStatBonusDeflection(stats.Resolve), delimiter, colorStyle);
			break;
		case CharacterStats.DefenseType.Fortitude:
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Might), CharacterStats.GetStatDefenseTypeBonus(stats.Might), delimiter, colorStyle);
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Constitution), CharacterStats.GetStatDefenseTypeBonus(stats.Constitution), delimiter, colorStyle);
			break;
		case CharacterStats.DefenseType.Reflex:
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Dexterity), CharacterStats.GetStatDefenseTypeBonus(stats.Dexterity), delimiter, colorStyle);
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Perception), CharacterStats.GetStatDefenseTypeBonus(stats.Perception), delimiter, colorStyle);
			break;
		case CharacterStats.DefenseType.Will:
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Intellect), CharacterStats.GetStatDefenseTypeBonus(stats.Intellect), delimiter, colorStyle);
			AppendKeyValueBonus(stringBuilder, GUIUtils.GetAttributeScoreTypeString(CharacterStats.AttributeScoreType.Resolve), CharacterStats.GetStatDefenseTypeBonus(stats.Resolve), delimiter, colorStyle);
			break;
		}
		Equipment component = stats.GetComponent<Equipment>();
		if ((bool)component && (bool)component.EquippedShield)
		{
			int bonus = 0;
			switch (defense)
			{
			case CharacterStats.DefenseType.Deflect:
				bonus = stats.GetShieldDeflectBonus(component);
				break;
			case CharacterStats.DefenseType.Reflex:
				bonus = stats.GetShieldReflexBonus(component);
				break;
			}
			Item component2 = component.EquippedShield.GetComponent<Item>();
			AppendKeyValueBonus(stringBuilder, component2 ? component2.Name : "*null*", bonus, delimiter, colorStyle);
		}
		for (int i = 0; i < stats.ActiveStatusEffects.Count; i++)
		{
			StatusEffect statusEffect = stats.ActiveStatusEffects[i];
			int num = statusEffect.AdjustDefense(null, stats.gameObject, defense);
			if (num != 0)
			{
				AppendStatusEffect(stringBuilder, stats, statusEffect, TextUtils.NumberBonus(num), delimiter, colorStyle);
			}
		}
		if (stats.TryGetRedirectDefense(defense, null, null, isSecondary: false, out var defense2, out var origin))
		{
			stringBuilder.Append(GUIUtils.Format(2386, defense2, origin));
			stringBuilder.Append(delimiter);
		}
		if (stringBuilder.Length >= delimiter.Length)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	private static void AppendStatusEffect(StringBuilder builder, CharacterStats stats, StatusEffect e, string value, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		bool flag = false;
		if (e.IsSuppressed)
		{
			builder.Append('[');
			builder.Append(NGUITools.EncodeColor(UIGlobalColor.Instance.DarkDisabled));
			builder.Append(']');
			flag = true;
		}
		else if (colorStyle != 0)
		{
			UIGlobalColor.LinkType linkType = UIGlobalColor.LinkType.NEUTRAL;
			float currentAppliedValue = e.CurrentAppliedValue;
			if (e.IsScaledMultiplier)
			{
				if (currentAppliedValue > 1f)
				{
					linkType = UIGlobalColor.LinkType.BUFF;
				}
				else if (currentAppliedValue < 1f)
				{
					linkType = UIGlobalColor.LinkType.DEBUFF;
				}
			}
			else if (currentAppliedValue > 0f)
			{
				linkType = UIGlobalColor.LinkType.BUFF;
			}
			else if (currentAppliedValue < 0f)
			{
				linkType = UIGlobalColor.LinkType.DEBUFF;
			}
			if (linkType != 0)
			{
				builder.Append('[');
				builder.Append(NGUITools.EncodeColor(UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, colorStyle, hovered: true, linkType)));
				builder.Append(']');
				flag = true;
			}
		}
		builder.Append(value + " " + e.GetDisplayName());
		if ((bool)e.Owner && e.Owner != stats.gameObject)
		{
			builder.Append(" [" + CharacterStats.Name(e.Owner) + "]");
		}
		if (flag)
		{
			builder.Append("[-]");
		}
		builder.Append(delimiter);
	}

	private static void AppendKeyValueBonus(StringBuilder builder, string key, int bonus, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		if (bonus != 0)
		{
			if (bonus > 0 && colorStyle != 0)
			{
				AppendKeyValue(builder, key, TextUtils.NumberBonus(bonus), delimiter, UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, colorStyle, hovered: true, UIGlobalColor.LinkType.BUFF));
			}
			else if (bonus < 0 && colorStyle != 0)
			{
				AppendKeyValue(builder, key, TextUtils.NumberBonus(bonus), delimiter, UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, colorStyle, hovered: true, UIGlobalColor.LinkType.DEBUFF));
			}
			else
			{
				AppendKeyValue(builder, key, TextUtils.NumberBonus(bonus), delimiter);
			}
		}
	}

	private static void AppendKeyValueMultiplier(StringBuilder builder, string key, float multiplier, string delimiter, UIGlobalColor.LinkStyle colorStyle)
	{
		if (multiplier != 1f)
		{
			if (multiplier > 1f && colorStyle != 0)
			{
				AppendKeyValue(builder, key, TextUtils.MultiplierAsPercentBonus(multiplier), delimiter, UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, colorStyle, hovered: true, UIGlobalColor.LinkType.BUFF));
			}
			else if (multiplier < 1f && colorStyle != 0)
			{
				AppendKeyValue(builder, key, TextUtils.MultiplierAsPercentBonus(multiplier), delimiter, UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, colorStyle, hovered: true, UIGlobalColor.LinkType.DEBUFF));
			}
			else
			{
				AppendKeyValue(builder, key, TextUtils.MultiplierAsPercentBonus(multiplier), delimiter);
			}
		}
	}

	private static void AppendKeyValue(StringBuilder builder, string key, string value, string delimiter)
	{
		builder.Append(value);
		builder.Append(' ');
		builder.Append(key);
		builder.Append(delimiter);
	}

	private static void AppendKeyValue(StringBuilder builder, string key, string value, string delimiter, Color color)
	{
		builder.Append('[');
		builder.Append(NGUITools.EncodeColor(color));
		builder.Append(']');
		builder.Append(value);
		builder.Append(' ');
		builder.Append(key);
		builder.Append("[-]");
		builder.Append(delimiter);
	}
}
