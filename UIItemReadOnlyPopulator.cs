public class UIItemReadOnlyPopulator : UIPopulator
{
	private UIGrid m_Grid;

	private int m_CurrentIndex;

	protected override void Awake()
	{
		m_Grid = GetComponent<UIGrid>();
		base.Awake();
	}

	public void Clear()
	{
		Populate(0);
	}

	public void AddItem(Item item, int quantity)
	{
		ActivateClone(m_CurrentIndex++).GetComponent<UIItemReadOnly>().LoadItem(item, quantity);
		if ((bool)m_Grid)
		{
			m_Grid.Reposition();
		}
	}
}
