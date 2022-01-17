public class UICharacterCreationAbilityPointGetter : UICharacterCreationElement
{
	public enum PointType
	{
		ABILITY,
		TALENT,
		ABILITY_MASTERY
	}

	private UILabel m_Label;

	public PointType Point;

	public override void SignalValueChanged(ValueType type)
	{
		if (type != ValueType.Ability && type != ValueType.Talent && type != ValueType.All)
		{
			return;
		}
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		if (Point == PointType.ABILITY)
		{
			if (UICharacterCreationManager.Instance.AbilitySelectionStates.Count > 0 && UICharacterCreationManager.Instance.AbilitySelectionStateIndex >= 0)
			{
				UICharacterCreationManager.AbilitySelectionState currentAbilitySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
				m_Label.text = (currentAbilitySelectionState.Points - currentAbilitySelectionState.SelectedAbilities.Count).ToString();
			}
		}
		else if (Point == PointType.TALENT)
		{
			if (UICharacterCreationManager.Instance.TalentSelectionStates.Count > 0 && UICharacterCreationManager.Instance.TalentSelectionStateIndex >= 0)
			{
				UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
				m_Label.text = (currentTalentSelectionState.Points - currentTalentSelectionState.SelectedAbilities.Count).ToString();
			}
		}
		else if (Point == PointType.ABILITY_MASTERY && UICharacterCreationManager.Instance.SpellMasterySelectionStates.Count > 0 && UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex >= 0)
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			m_Label.text = (currentSpellMasterySelectionState.Points - currentSpellMasterySelectionState.SelectedAbilities.Count).ToString();
		}
	}
}
