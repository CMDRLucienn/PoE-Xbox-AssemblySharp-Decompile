public class UIISelectReposition : UIParentSelectorListener
{
	private int m_NeedsReposition = 1;

	private void LateUpdate()
	{
		if (m_NeedsReposition <= 0)
		{
			return;
		}
		m_NeedsReposition--;
		if (m_NeedsReposition == 0)
		{
			UITable component = GetComponent<UITable>();
			if ((bool)component)
			{
				component.repositionNow = true;
			}
			UIGrid component2 = GetComponent<UIGrid>();
			if ((bool)component2)
			{
				component2.repositionNow = true;
			}
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_NeedsReposition = 2;
	}
}
