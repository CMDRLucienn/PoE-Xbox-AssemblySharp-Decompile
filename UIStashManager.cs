public class UIStashManager : UIHudWindow
{
	public UIInventoryFilter.ItemFilterType ShowOnTab = UIInventoryFilter.ItemFilterType.WEAPONS;

	public UIInventoryStashGrid StashGrid;

	public static UIStashManager Instance { get; private set; }

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

	protected override void Show()
	{
		StashGrid.CurrentTab = ShowOnTab;
		if (!UIInventoryManager.Instance.WindowActive())
		{
			UIInventoryManager.Instance.ShowWindow();
		}
	}

	protected override bool Hide(bool forced)
	{
		ShowOnTab = UIInventoryFilter.ItemFilterType.WEAPONS;
		if ((bool)UIInventoryManager.Instance.DraggedSource && UIInventoryManager.Instance.DraggedSource.Owner == StashGrid.ItemGrid)
		{
			StashGrid.ItemGrid.Put(UIInventoryManager.Instance.DraggedItem, null);
			UIInventoryManager.Instance.FinishDrag();
		}
		return base.Hide(forced);
	}
}
