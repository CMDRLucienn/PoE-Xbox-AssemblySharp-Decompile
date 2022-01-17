public class QuickbarInventory : BaseInventory
{
	private CharacterStats m_Stats;

	public override void Start()
	{
		base.Start();
		m_Stats = GetComponent<CharacterStats>();
	}

	protected override void Update()
	{
		MaxItems = m_Stats.MaxQuickSlots;
		base.Update();
	}
}
