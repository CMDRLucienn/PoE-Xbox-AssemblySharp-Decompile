public class StashInventory : BaseInventory
{
	protected override bool m_RedirectToPlayer => true;

	public override bool InfiniteStacking => true;
}
