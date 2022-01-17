using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UICharacterCreationStringGetter : UICharacterCreationElement
{
	public enum StringDataSource
	{
		SEX,
		RACE,
		SUBRACE,
		CLASS,
		CULTURE,
		VOICE,
		NAME,
		SEX_DESC,
		RACE_DESC,
		RACE_DESC2,
		SUBRACE_DESC,
		CLASS_DESC,
		CLASS_DESC2,
		CULTURE_DESC,
		CULTURE_DESC2,
		BACKGROUND,
		BACKGROUND_DESC,
		BACKGROUND_DESC2,
		ABILITY,
		ABILITY_DESC,
		ABILITY_DESC2,
		ABILITIES_ALL,
		TALENT,
		TALENT_DESC,
		TALENT_DESC2,
		TALENT_ALL,
		ATTRIBUTE,
		ATTRIBUTE_DESC,
		ATTRIBUTE_DESC2,
		DEITY,
		DEITY_DESC,
		PALADIN_ORDER,
		PALADIN_ORDER_DESC,
		PALADIN_ORDER_DESC2,
		SKILL,
		SKILL_DESC,
		SKILL_DESC2,
		SKILL_DELTAS,
		DESC2_NONE,
		SPELL_MASTERY,
		SPELL_MASTERY_DESC,
		SPELL_MASTERY_DESC2
	}

	private UILabel m_Label;

	private UICapitularLabel m_CapLabel;

	public UIPuckScrollbar TextScrollBar;

	private string[] m_TwoPartStringSplitter = new string[3] { "\n\n", "\r\n\r\n", "\n\r\n\r" };

	private UITable m_ParentLayoutTable;

	public StringDataSource DataSource;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Start()
	{
		base.Start();
		m_ParentLayoutTable = NGUITools.FindInParents<UITable>(base.gameObject);
		if (m_ParentLayoutTable != null)
		{
			if (StringTableManager.CurrentLanguage != null && StringTableManager.CurrentLanguage.Charset != 0)
			{
				m_ParentLayoutTable.transform.localPosition = Vector3.zero;
			}
			else
			{
				m_ParentLayoutTable.transform.localPosition = new Vector3(0f, 40f, 0f);
			}
		}
	}

	public void SetText(string text)
	{
		if (m_CapLabel != null)
		{
			m_CapLabel.text = text;
		}
		else if (m_Label != null)
		{
			m_Label.text = text;
		}
		if ((bool)m_ParentLayoutTable)
		{
			m_ParentLayoutTable.Reposition();
		}
		if ((bool)TextScrollBar)
		{
			TextScrollBar.ResetForNewContent();
		}
	}

	public bool IsDescriptionText2()
	{
		switch (DataSource)
		{
		case StringDataSource.RACE_DESC2:
		case StringDataSource.CLASS_DESC2:
		case StringDataSource.CULTURE_DESC2:
		case StringDataSource.BACKGROUND_DESC2:
		case StringDataSource.ABILITY_DESC2:
		case StringDataSource.TALENT_DESC2:
		case StringDataSource.ATTRIBUTE_DESC2:
		case StringDataSource.PALADIN_ORDER_DESC2:
		case StringDataSource.SKILL_DESC2:
		case StringDataSource.DESC2_NONE:
		case StringDataSource.SPELL_MASTERY_DESC2:
			return true;
		default:
			return false;
		}
	}

	public bool IsDescriptionText()
	{
		switch (DataSource)
		{
		case StringDataSource.SEX_DESC:
		case StringDataSource.RACE_DESC:
		case StringDataSource.SUBRACE_DESC:
		case StringDataSource.CLASS_DESC:
		case StringDataSource.CULTURE_DESC:
		case StringDataSource.BACKGROUND_DESC:
		case StringDataSource.ABILITY_DESC:
		case StringDataSource.TALENT_DESC:
		case StringDataSource.ATTRIBUTE_DESC:
		case StringDataSource.DEITY_DESC:
		case StringDataSource.PALADIN_ORDER_DESC:
		case StringDataSource.SKILL_DESC:
		case StringDataSource.SPELL_MASTERY_DESC:
			return true;
		default:
			return false;
		}
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		if (!m_CapLabel)
		{
			m_CapLabel = GetComponent<UICapitularLabel>();
		}
		UICharacterCreationManager.Character activeCharacter = UICharacterCreationManager.Instance.ActiveCharacter;
		switch (DataSource)
		{
		case StringDataSource.SEX:
			SetText(GUIUtils.GetGenderString(activeCharacter.Gender));
			break;
		case StringDataSource.RACE:
			SetText(GUIUtils.GetRaceString(activeCharacter.Race, activeCharacter.Gender));
			break;
		case StringDataSource.SUBRACE:
			if (activeCharacter.Subrace == CharacterStats.Subrace.Undefined)
			{
				SetText(GUIUtils.GetRaceString(activeCharacter.Race, activeCharacter.Gender));
			}
			else
			{
				SetText(GUIUtils.GetSubraceString(activeCharacter.Subrace, activeCharacter.Gender));
			}
			break;
		case StringDataSource.CLASS:
		{
			string text3 = GUIUtils.GetClassString(activeCharacter.Class, activeCharacter.Gender);
			if (activeCharacter.Class == CharacterStats.Class.Paladin && activeCharacter.PaladinOrder != 0)
			{
				text3 = text3 + " - " + GUIUtils.GetPaladinOrderString(activeCharacter.PaladinOrder, activeCharacter.Gender);
			}
			else if (activeCharacter.Class == CharacterStats.Class.Priest && activeCharacter.Deity != 0)
			{
				text3 = text3 + " - " + GUIUtils.GetDeityString(activeCharacter.Deity);
			}
			SetText(text3);
			break;
		}
		case StringDataSource.CULTURE:
			SetText(GUIUtils.GetCultureString(UICharacterCreationEnumSetter.s_PendingCulture, activeCharacter.Gender));
			break;
		case StringDataSource.BACKGROUND:
		{
			string text4 = GUIUtils.GetCultureString(activeCharacter.Culture, activeCharacter.Gender);
			if (activeCharacter.Background != 0)
			{
				text4 = text4 + " - " + GUIUtils.GetBackgroundString(activeCharacter.Background, activeCharacter.Gender);
			}
			SetText(text4);
			break;
		}
		case StringDataSource.VOICE:
			if (activeCharacter.VoiceSet != null)
			{
				SetText(activeCharacter.VoiceSet.DisplayName.GetText());
			}
			else
			{
				SetText("");
			}
			break;
		case StringDataSource.NAME:
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(activeCharacter.Name))
			{
				stringBuilder.Append(activeCharacter.Name);
				stringBuilder.Append(" - ");
			}
			stringBuilder.Append(GUIUtils.Format(activeCharacter.Gender, 1764, UICharacterCreationManager.Instance.EndingLevel, GUIUtils.GetClassString(activeCharacter.Class, activeCharacter.Gender)));
			SetText(stringBuilder.ToString());
			break;
		}
		case StringDataSource.SEX_DESC:
			SetText(GUIUtils.GetGenderDescriptionString(activeCharacter.Gender));
			break;
		case StringDataSource.RACE_DESC:
			SetText(GUIUtils.GetRaceDescriptionString(activeCharacter.Race, activeCharacter.Gender));
			break;
		case StringDataSource.RACE_DESC2:
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int n = 0; n < 6; n++)
			{
				int num4 = CharacterStats.RaceAbilityAdjustment[(int)activeCharacter.Race, n];
				if (num4 > 0)
				{
					stringBuilder.AppendFormat("{0}: +{1}\n", GUIUtils.GetAttributeScoreTypeString((CharacterStats.AttributeScoreType)n), num4);
				}
				else if (num4 < 0)
				{
					stringBuilder.AppendFormat("{0}: {1}\n", GUIUtils.GetAttributeScoreTypeString((CharacterStats.AttributeScoreType)n), num4);
				}
			}
			SetText(stringBuilder.ToString());
			break;
		}
		case StringDataSource.SUBRACE_DESC:
			SetText(GUIUtils.GetSubraceDescriptionString(activeCharacter.Subrace, activeCharacter.Gender));
			break;
		case StringDataSource.CLASS_DESC:
			SetText(GUIUtils.GetClassDescriptionString(activeCharacter.Class, activeCharacter.Gender));
			break;
		case StringDataSource.CLASS_DESC2:
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int l = 0; l < 6; l++)
			{
				int num3 = CharacterStats.ClassSkillAdjustment[(int)activeCharacter.Class, l];
				if (num3 != 0)
				{
					stringBuilder.AppendFormat("{0}: {1}\n", GUIUtils.GetSkillTypeString((CharacterStats.SkillType)l), TextUtils.NumberBonus(num3));
				}
			}
			stringBuilder.AppendLine();
			object component = new UIDataManagerHelper.CharacterStats
			{
				CharacterClass = activeCharacter.Class
			};
			DataManager.AdjustFromData(ref component, "CharacterStats");
			UIDataManagerHelper.CharacterStats characterStats = component as UIDataManagerHelper.CharacterStats;
			stringBuilder.AppendFormat("{0}: {1} + {2}/{3} ({4})\n", GUIUtils.GetText(1498, activeCharacter.Gender), characterStats.MaxStamina, characterStats.HealthStaminaPerLevel, GUIUtils.GetText(373), GUIUtils.GetRelativeRatingString(UIDataManagerHelper.GetRelativeMaxStamina(characterStats.MaxStamina)));
			stringBuilder.AppendFormat("{0}: {1} * {2} ({3})\n", GUIUtils.GetText(1469, activeCharacter.Gender), characterStats.ClassHealthMultiplier, GUIUtils.GetText(1498, activeCharacter.Gender), GUIUtils.GetRelativeRatingString(UIDataManagerHelper.GetRelativeHealthMultiplier(characterStats.ClassHealthMultiplier)));
			stringBuilder.AppendFormat("{0}: {1} + {2}/{3} ({4})\n", GUIUtils.GetText(369, activeCharacter.Gender), characterStats.MeleeAccuracyBonus, AttackData.Instance.AccuracyPerLevel, GUIUtils.GetText(373), GUIUtils.GetRelativeRatingString(UIDataManagerHelper.GetRelativeMeleeAccuracyBonus(characterStats.MeleeAccuracyBonus)));
			stringBuilder.AppendFormat("{0}: {1} ({2})\n", GUIUtils.GetText(40), characterStats.BaseDeflection, GUIUtils.GetRelativeRatingString(UIDataManagerHelper.GetRelativeBaseDeflection(characterStats.BaseDeflection)));
			SetText(stringBuilder.ToString());
			break;
		}
		case StringDataSource.CULTURE_DESC:
			SetText(GUIUtils.GetCultureDescriptionString(UICharacterCreationEnumSetter.s_PendingCulture, activeCharacter.Gender));
			break;
		case StringDataSource.CULTURE_DESC2:
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < 6; j++)
			{
				int num2 = CharacterStats.CultureAbilityAdjustment[(int)UICharacterCreationEnumSetter.s_PendingCulture, j];
				if (num2 != 0)
				{
					stringBuilder.AppendFormat("{0}: {1}\n", GUIUtils.GetAttributeScoreTypeString((CharacterStats.AttributeScoreType)j), TextUtils.NumberBonus(num2));
				}
			}
			SetText(stringBuilder.ToString());
			break;
		}
		case StringDataSource.BACKGROUND_DESC:
			SetText(GUIUtils.GetBackgroundDescriptionString(activeCharacter.Background, activeCharacter.Gender));
			break;
		case StringDataSource.BACKGROUND_DESC2:
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 6; i++)
			{
				int num = CharacterStats.BackgroundSkillAdjustment[(int)activeCharacter.Background, i];
				if (num != 0)
				{
					stringBuilder.AppendFormat("{0}: {1}\n", GUIUtils.GetSkillTypeString((CharacterStats.SkillType)i), TextUtils.NumberBonus(num));
				}
			}
			SetText(stringBuilder.ToString());
			break;
		}
		case StringDataSource.ABILITY:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.LastSelectedAbility != null)
				{
					SetText(AbilityProgressionTable.GetAbilityName(currentSpellMasterySelectionState.LastSelectedAbility.Ability));
				}
				else
				{
					SetText(currentSpellMasterySelectionState.CategoryName);
				}
			}
			break;
		}
		case StringDataSource.ABILITY_DESC:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.LastSelectedAbility != null)
				{
					string abilityDesc2 = AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.LastSelectedAbility.Ability, UICharacterCreationManager.Instance.TargetCharacter, currentSpellMasterySelectionState.LastSelectedAbility.ActivationObject);
					SetText(SplitTextBlock(abilityDesc2, 0));
				}
				else
				{
					SetText(currentSpellMasterySelectionState.PointUnlockDescription);
				}
			}
			break;
		}
		case StringDataSource.ABILITY_DESC2:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.LastSelectedAbility != null)
				{
					string abilityDesc4 = AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.LastSelectedAbility.Ability, UICharacterCreationManager.Instance.TargetCharacter, currentSpellMasterySelectionState.LastSelectedAbility.ActivationObject);
					SetText(SplitTextBlock(abilityDesc4, 1));
				}
				else
				{
					SetText(string.Empty);
				}
			}
			break;
		}
		case StringDataSource.ABILITIES_ALL:
		{
			SetText("");
			bool flag = UICharacterCreationManager.Instance.AbilitySelectionStates != null && UICharacterCreationManager.Instance.AbilitySelectionStates.Count > 0;
			List<GameObject> selectedAndGrantedAbilities = UICharacterCreationManager.Instance.GetSelectedAndGrantedAbilities();
			GenericAbility genericAbility = null;
			for (int k = 0; k < selectedAndGrantedAbilities.Count; k++)
			{
				GameObject gameObject = selectedAndGrantedAbilities[k];
				if (gameObject != null)
				{
					genericAbility = AbilityProgressionTable.GetGenericAbility(gameObject);
					if (genericAbility == null || !genericAbility.HideFromUi)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				SetText(GUIUtils.GetText(1449));
			}
			break;
		}
		case StringDataSource.TALENT:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.SelectedAbilities.Count > 0)
				{
					SetText(AbilityProgressionTable.GetAbilityName(currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].Ability));
				}
				else
				{
					SetText(currentSpellMasterySelectionState.CategoryName);
				}
			}
			break;
		}
		case StringDataSource.TALENT_DESC:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.SelectedAbilities.Count > 0)
				{
					SetText(SplitTextBlock(AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].Ability, UICharacterCreationManager.Instance.TargetCharacter, currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].ActivationObject), 0));
				}
				else
				{
					SetText(currentSpellMasterySelectionState.PointUnlockDescription);
				}
			}
			break;
		}
		case StringDataSource.TALENT_DESC2:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentSpellMasterySelectionState == null)
			{
				break;
			}
			if (currentSpellMasterySelectionState.SelectedAbilities.Count > 0)
			{
				string value = SplitTextBlock(AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].Ability, UICharacterCreationManager.Instance.TargetCharacter, currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].ActivationObject), 1);
				if (!string.IsNullOrEmpty(value))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(value);
					for (int m = 0; m < 6; m++)
					{
						int skillAdjustment = AbilityProgressionTable.GetGenericTalent(currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].Ability).GetSkillAdjustment((CharacterStats.SkillType)m);
						if (skillAdjustment > 0)
						{
							stringBuilder.AppendFormat("{0}: +{1}\n", GUIUtils.GetSkillTypeString((CharacterStats.SkillType)m), skillAdjustment);
						}
					}
					SetText(stringBuilder.ToString());
				}
				else
				{
					SetText(string.Empty);
				}
			}
			else
			{
				SetText(string.Empty);
			}
			break;
		}
		case StringDataSource.TALENT_ALL:
			SetText("");
			if (UICharacterCreationManager.Instance.TalentSelectionStates.Count == 0)
			{
				SetText(GUIUtils.GetText(1450));
			}
			break;
		case StringDataSource.PALADIN_ORDER:
			if (activeCharacter.PaladinOrder == Religion.PaladinOrder.None)
			{
				SetText("");
			}
			else
			{
				SetText(GUIUtils.GetPaladinOrderString(activeCharacter.PaladinOrder, activeCharacter.Gender));
			}
			break;
		case StringDataSource.DEITY:
			if (activeCharacter.Deity == Religion.Deity.None)
			{
				SetText("");
			}
			else
			{
				SetText(GUIUtils.GetDeityString(activeCharacter.Deity));
			}
			break;
		case StringDataSource.PALADIN_ORDER_DESC:
		{
			string text2 = GUIUtils.GetPaladinOrderDescriptionString(activeCharacter.PaladinOrder, activeCharacter.Gender);
			if ((bool)Religion.Instance)
			{
				text2 += "\n\n";
				string dispositionsString3 = Religion.Instance.GetDispositionsString(activeCharacter.PaladinOrder, positive: true);
				string dispositionsString4 = Religion.Instance.GetDispositionsString(activeCharacter.PaladinOrder, positive: false);
				text2 = text2 + GUIUtils.Format(1877, dispositionsString3) + "\n" + GUIUtils.Format(1878, dispositionsString4);
			}
			SetText(text2.Trim());
			break;
		}
		case StringDataSource.DEITY_DESC:
		{
			string text = GUIUtils.GetDeityDescriptionString(activeCharacter.Deity);
			if ((bool)Religion.Instance)
			{
				text += "\n\n";
				string dispositionsString = Religion.Instance.GetDispositionsString(activeCharacter.Deity, positive: true);
				string dispositionsString2 = Religion.Instance.GetDispositionsString(activeCharacter.Deity, positive: false);
				text = text + GUIUtils.Format(1877, dispositionsString) + "\n" + GUIUtils.Format(1878, dispositionsString2);
			}
			SetText(text.Trim());
			break;
		}
		case StringDataSource.SKILL_DELTAS:
		{
			string[] array = new string[10]
			{
				GUIUtils.GetSkillTypeString(CharacterStats.SkillType.Stealth),
				activeCharacter.SkillRankDeltas[0].ToString(),
				GUIUtils.GetSkillTypeString(CharacterStats.SkillType.Athletics),
				activeCharacter.SkillRankDeltas[1].ToString(),
				GUIUtils.GetSkillTypeString(CharacterStats.SkillType.Lore),
				activeCharacter.SkillRankDeltas[2].ToString(),
				GUIUtils.GetSkillTypeString(CharacterStats.SkillType.Mechanics),
				activeCharacter.SkillRankDeltas[3].ToString(),
				GUIUtils.GetSkillTypeString(CharacterStats.SkillType.Survival),
				activeCharacter.SkillRankDeltas[4].ToString()
			};
			object[] param = array;
			SetText(StringUtility.Format("{0}: +{1}    {2}: +{3}\n{4}: +{5}    {6}: +{7}\n{8}: +{9}", param));
			break;
		}
		case StringDataSource.DESC2_NONE:
			SetText(string.Empty);
			break;
		case StringDataSource.SPELL_MASTERY:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.SelectedAbilities.Count > 0)
				{
					SetText(AbilityProgressionTable.GetAbilityName(currentSpellMasterySelectionState.SelectedAbilities[currentSpellMasterySelectionState.SelectedAbilities.Count - 1].Ability));
				}
				else
				{
					SetText(GUIUtils.GetText(2252));
				}
			}
			break;
		}
		case StringDataSource.SPELL_MASTERY_DESC:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.LastSelectedAbility != null)
				{
					string abilityDesc3 = AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.LastSelectedAbility.Ability, UICharacterCreationManager.Instance.TargetCharacter, AbilityProgressionTable.AbilityActivationObject.Self);
					SetText(SplitTextBlock(abilityDesc3, 0));
				}
				else
				{
					SetText(GUIUtils.GetText(2257));
				}
			}
			break;
		}
		case StringDataSource.SPELL_MASTERY_DESC2:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (currentSpellMasterySelectionState != null)
			{
				if (currentSpellMasterySelectionState.LastSelectedAbility != null)
				{
					string abilityDesc = AbilityProgressionTable.GetAbilityDesc(currentSpellMasterySelectionState.LastSelectedAbility.Ability, UICharacterCreationManager.Instance.TargetCharacter, AbilityProgressionTable.AbilityActivationObject.Self);
					SetText(SplitTextBlock(abilityDesc, 1));
				}
				else
				{
					SetText(string.Empty);
				}
			}
			break;
		}
		case StringDataSource.ATTRIBUTE:
		case StringDataSource.ATTRIBUTE_DESC:
		case StringDataSource.ATTRIBUTE_DESC2:
		case StringDataSource.PALADIN_ORDER_DESC2:
		case StringDataSource.SKILL:
		case StringDataSource.SKILL_DESC:
		case StringDataSource.SKILL_DESC2:
			break;
		}
	}

	private string SplitTextBlock(string textBlock, int splitIndex)
	{
		if (splitIndex < 0)
		{
			return string.Empty;
		}
		string[] array = textBlock.Split(m_TwoPartStringSplitter, 2, StringSplitOptions.RemoveEmptyEntries);
		if (splitIndex < array.Length)
		{
			return array[splitIndex];
		}
		return string.Empty;
	}
}
