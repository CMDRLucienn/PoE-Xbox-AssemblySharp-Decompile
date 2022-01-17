public class UICharacterCreationAttributeGetter : UICharacterCreationElement
{
	private UILabel m_Label;

	public CharacterStats.AttributeScoreType DataSource;

	public bool RaceAdjustment;

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Attribute || type == ValueType.Race || type == ValueType.Culture || type == ValueType.All)
		{
			if (!m_Label)
			{
				m_Label = GetComponent<UILabel>();
			}
			UICharacterCreationManager.Character character = base.Owner.Character;
			if (RaceAdjustment)
			{
				int value = CharacterStats.RaceAbilityAdjustment[(int)character.Race, (int)DataSource];
				m_Label.text = TextUtils.NumberBonus(value);
			}
			else
			{
				int num = CharacterStats.RaceAbilityAdjustment[(int)character.Race, (int)DataSource] + CharacterStats.CultureAbilityAdjustment[(int)UICharacterCreationEnumSetter.s_PendingCulture, (int)DataSource];
				m_Label.text = (character.BaseStats[(int)DataSource] + num).ToString();
			}
		}
	}
}
