using UnityEngine;

public class UICharacterCreationSkillBar : UICharacterCreationElement
{
	public float MaxSize;

	public CharacterStats.SkillType SkillSource;

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Skill || type == ValueType.All)
		{
			UICharacterCreationManager.Character character = base.Owner.Character;
			int num = character.SkillValues[(int)SkillSource] + character.SkillValueDeltas[(int)SkillSource];
			int pointsForSkillLevel = CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num));
			int pointsForSkillLevel2 = CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num) + 1);
			int num2 = pointsForSkillLevel2 - pointsForSkillLevel;
			int num3 = pointsForSkillLevel2 - num;
			SetFillPercentage((float)(num2 - num3) / (float)num2);
		}
	}

	public void SetFillPercentage(float percentage)
	{
		base.transform.localScale = new Vector3(percentage * MaxSize, base.transform.localScale.y, base.transform.localScale.z);
	}
}
