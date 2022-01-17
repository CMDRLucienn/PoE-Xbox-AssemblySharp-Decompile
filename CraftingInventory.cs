public class CraftingInventory : BaseInventory
{
	public override bool InfiniteStacking => true;

	public override int AddItem(Item newItem, int addCount, int forceSlot, bool original)
	{
		PlayerInventory.LogItemGet(GUIUtils.GetText(1021), newItem, addCount);
		return base.AddItem(newItem, addCount, forceSlot, original: false);
	}

	public override bool PutItem(InventoryItem item, int slot)
	{
		PlayerInventory.LogItemGet(GUIUtils.GetText(1021), item.baseItem, item.stackSize);
		return base.PutItem(item, slot);
	}
}
