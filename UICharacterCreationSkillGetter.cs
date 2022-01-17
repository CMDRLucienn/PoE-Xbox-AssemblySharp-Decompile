public class UICharacterCreationSkillGetter : UICharacterCreationElement
{
	public enum SkillPointType
	{
		Level,
		TotalPoints,
		DeltaPoints,
		PointsToSpend,
		PointsToNextRank
	}

	private UILabel m_Label;

	public CharacterStats.SkillType SkillSource;

	public SkillPointType PointType;

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Skill || type == ValueType.All)
		{
			if (!m_Label)
			{
				m_Label = GetComponent<UILabel>();
			}
			UICharacterCreationManager.Character character = base.Owner.Character;
			int num = character.SkillValues[(int)SkillSource] + character.SkillValueDeltas[(int)SkillSource];
			switch (PointType)
			{
			case SkillPointType.Level:
				m_Label.text = (CharacterStats.CalculateSkillLevelViaPoints(num) + CharacterStats.ClassSkillAdjustment[(int)character.Class, (int)SkillSource] + CharacterStats.BackgroundSkillAdjustment[(int)character.Background, (int)SkillSource]).ToString();
				break;
			case SkillPointType.TotalPoints:
				m_Label.text = num + "/" + CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num) + 1);
				break;
			case SkillPointType.DeltaPoints:
				m_Label.text = character.SkillValueDeltas[(int)SkillSource].ToString();
				break;
			case SkillPointType.PointsToSpend:
				m_Label.text = character.SkillPointsToSpend.ToString();
				break;
			case SkillPointType.PointsToNextRank:
				m_Label.text = (CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num) + 1) - num).ToString();
				break;
			}
		}
	}
}
