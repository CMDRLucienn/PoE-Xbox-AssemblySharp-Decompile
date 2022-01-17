public class UICharacterCreationPointsLefts : UICharacterCreationElement
{
	private UILabel m_Label;

	public bool ShowString;

	public bool ShowUnspent;

	public override void SignalValueChanged(ValueType type)
	{
		if (type != ValueType.Attribute && type != ValueType.All)
		{
			return;
		}
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		int remainingAttributePoints = UICharacterCreationManager.Instance.GetRemainingAttributePoints(base.Owner.Character);
		if (ShowUnspent)
		{
			m_Label.text = remainingAttributePoints.ToString();
		}
		else if (ShowString)
		{
			if (remainingAttributePoints == 1)
			{
				m_Label.text = GUIUtils.GetText(338);
			}
			else
			{
				m_Label.text = GUIUtils.GetText(337);
			}
		}
	}
}
