using System;
using System.Text;
using UnityEngine;

public class UICharacterCreationSkillTooltip : UICharacterCreationElement
{
	public CharacterStats.SkillType Skill;

	private static UICharacterCreationSkillTooltip LastUpdated;

	protected override void OnDestroy()
	{
		if (LastUpdated == this)
		{
			LastUpdated = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (this == LastUpdated)
		{
			SetText();
		}
	}

	private void SetText()
	{
		UICharacterCreationStringGetter[] array = UnityEngine.Object.FindObjectsOfType<UICharacterCreationStringGetter>();
		foreach (UICharacterCreationStringGetter uICharacterCreationStringGetter in array)
		{
			if (!uICharacterCreationStringGetter)
			{
				continue;
			}
			if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.SKILL_DESC)
			{
				int skillValue = UICharacterCreationManager.Instance.TargetCharacter.GetComponent<CharacterStats>().CalculateSkillBasedOnChange(Skill, base.Owner.Character.SkillValueDeltas[(int)Skill]);
				string skillTypeDescriptionString = GUIUtils.GetSkillTypeDescriptionString(Skill);
				UICharacterCreationManager.Character activeCharacter = UICharacterCreationManager.Instance.ActiveCharacter;
				StringBuilder stringBuilder = new StringBuilder(skillTypeDescriptionString);
				string bonusTextLine = UICharacterSheetSkillLine.GetBonusTextLine(Skill, skillValue);
				if (!string.IsNullOrEmpty(bonusTextLine))
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(bonusTextLine);
				}
				stringBuilder.AppendFormat("\n\n{0}: {1}", GUIUtils.GetSkillTypeString(Skill), CharacterStats.CalculateSkillLevelViaPoints(activeCharacter.SkillValues[(int)Skill]));
				int num = CharacterStats.ClassSkillAdjustment[(int)activeCharacter.Class, (int)Skill];
				int num2 = CharacterStats.BackgroundSkillAdjustment[(int)activeCharacter.Background, (int)Skill];
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
					stringBuilder.Append(GUIUtils.GetClassString(activeCharacter.Class, activeCharacter.Gender));
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
					stringBuilder.Append(GUIUtils.GetBackgroundString(activeCharacter.Background, activeCharacter.Gender));
					stringBuilder.Append(")");
				}
				if (num != 0 || num2 != 0)
				{
					stringBuilder.AppendFormat(" = {0}", CharacterStats.CalculateSkillLevelViaPoints(activeCharacter.SkillValues[(int)Skill]) + num + num2);
				}
				uICharacterCreationStringGetter.SetText(stringBuilder.ToString());
			}
			else if (uICharacterCreationStringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.SKILL)
			{
				uICharacterCreationStringGetter.SetText(GUIUtils.GetSkillTypeString(Skill));
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
