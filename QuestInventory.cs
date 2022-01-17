public class QuestInventory : BaseInventory
{
	[Persistent]
	public bool HasNew { get; private set; }

	public void ResetNew()
	{
		HasNew = false;
	}

	public override int AddItem(Item newItem, int addCount, int forceSlot, bool original)
	{
		HasNew = true;
		PlayerInventory.LogItemGet(GUIUtils.GetText(118), newItem, addCount);
		return base.AddItem(newItem, addCount, forceSlot, original: false);
	}

	public override bool PutItem(InventoryItem item, int slot)
	{
		HasNew = true;
		PlayerInventory.LogItemGet(GUIUtils.GetText(118), item.baseItem, item.stackSize);
		return base.PutItem(item, slot);
	}
}
