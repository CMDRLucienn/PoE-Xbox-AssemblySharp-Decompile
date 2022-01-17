public class UIItemsPopupManager : UIHudWindow
{
	public UIInventoryItemGrid ItemGrid;

	public UILabel TitleLabel;

	public static UIItemsPopupManager Instance { get; private set; }

	public void ShowQuestItems()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			QuestInventory component = GameState.s_playerCharacter.GetComponent<QuestInventory>();
			component.ResetNew();
			SetInventory(component);
			if (!UIInventoryManager.Instance.WindowActive())
			{
				UIInventoryManager.Instance.ShowWindow();
			}
			ShowWindow();
			ItemGrid.ChangeGridItems(delegate(UIInventoryGridItem item)
			{
				item.Locked = true;
			});
		}
	}

	public void ShowIngredients()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			SetInventory(GameState.s_playerCharacter.GetComponent<CraftingInventory>());
			if (!UIInventoryManager.Instance.WindowActive())
			{
				UIInventoryManager.Instance.ShowWindow();
			}
			ShowWindow();
			ItemGrid.ChangeGridItems(delegate(UIInventoryGridItem item)
			{
				item.Locked = true;
			});
		}
	}

	public void SetInventory(BaseInventory value)
	{
		if ((bool)value)
		{
			ItemGrid.LoadInventory(value);
			if (value is QuestInventory)
			{
				TitleLabel.text = GUIUtils.GetText(993);
			}
			else if (value is CraftingInventory)
			{
				TitleLabel.text = GUIUtils.GetText(1021);
			}
			else if (value is StashInventory)
			{
				TitleLabel.text = GUIUtils.GetText(563);
			}
			else
			{
				TitleLabel.text = GUIUtils.GetText(1020);
			}
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override bool Hide(bool forced)
	{
		if ((bool)UIInventoryManager.Instance.DraggedSource && UIInventoryManager.Instance.DraggedSource.Owner == ItemGrid)
		{
			ItemGrid.Put(UIInventoryManager.Instance.DraggedItem, null);
			UIInventoryManager.Instance.FinishDrag();
		}
		return base.Hide(forced);
	}
}
