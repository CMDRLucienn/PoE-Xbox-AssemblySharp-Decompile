using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIInventoryItemGrid : UIInventoryItemZone
{
	public delegate void ItemGridDelegate(UIInventoryItemGrid sender);

	public int Capacity;

	public int MinimumSize;

	public int SizeFactorOf = 1;

	public int PageSize;

	public bool IgnoreItemSlots;

	public bool KeepExtraSlot;

	public bool FilterIsHide;

	private int m_MaxCount;

	private int m_CurrentPage;

	public UIInventoryGridItem RootTile;

	public GameObject RootContent;

	private UIInventoryGridItem[] m_Tiles;

	public bool AllowOrder = true;

	public bool CompressOnChange;

	public bool RecycleWidgets;

	private UIDraggablePanel m_PanelParent;

	private UIGrid m_Grid;

	public Func<InventoryItem, bool> InclusionFilter;

	public UIEventListener.VoidDelegate OnItemClicked;

	public UIEventListener.FloatDelegate OnItemScrolled;

	public Comparison<InventoryItem> SortFunc;

	private bool m_NeedsReload;

	private int NeededSlots
	{
		get
		{
			if (Capacity == 0)
			{
				return SizeFactorOf * Mathf.CeilToInt((float)Mathf.Clamp(Inventory ? (ApplicableInventory.Count() + (KeepExtraSlot ? 1 : 0)) : 0, MinimumSize, GetPageSize) / (float)SizeFactorOf);
			}
			return Capacity;
		}
	}

	private int GetPageSize
	{
		get
		{
			if (PageSize <= 0)
			{
				return int.MaxValue;
			}
			return PageSize;
		}
	}

	public int PageCount => Mathf.Max(1, m_MaxCount);

	public int CurrentPage
	{
		get
		{
			return m_CurrentPage;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, PageCount - 1);
			if (PageSize > 0 && m_CurrentPage != num)
			{
				m_CurrentPage = num;
				Reload();
			}
		}
	}

	public BaseInventory Inventory { get; private set; }

	public IEnumerable<InventoryItem> ApplicableInventory
	{
		get
		{
			IEnumerable<InventoryItem> enumerable = Inventory;
			if (InclusionFilter != null)
			{
				enumerable = enumerable.Where(InclusionFilter);
			}
			if (FilterIsHide)
			{
				enumerable = enumerable.Where((InventoryItem ii) => UIInventoryFilterManager.Accepts(ii.baseItem));
			}
			if (SortFunc != null)
			{
				List<InventoryItem> list = new List<InventoryItem>(enumerable);
				list.Sort(SortFunc);
				enumerable = list;
			}
			return enumerable;
		}
	}

	public override bool IsQuickslotInventory => Inventory is QuickbarInventory;

	public override bool CareAboutIndividualSlots => AllowOrder;

	protected int MaxVisibleItems
	{
		get
		{
			if ((bool)m_PanelParent && (bool)m_PanelParent.panel && m_PanelParent.panel.clipping != 0)
			{
				int num = Mathf.CeilToInt(m_PanelParent.panel.clipRange.w / m_Grid.cellHeight);
				if (m_Grid.arrangement == UIGrid.Arrangement.Horizontal)
				{
					return num * m_Grid.maxPerLine;
				}
				return num;
			}
			return m_Tiles.Length;
		}
	}

	public override bool InfiniteStacking => Inventory.InfiniteStacking;

	public event ItemGridDelegate OnPreReload;

	public event ItemGridDelegate OnPostReload;

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			return;
		}
		if (!m_Grid)
		{
			m_Grid = GetComponent<UIGrid>();
		}
		if (!m_Grid)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		for (int i = 1; i < MinimumSize; i++)
		{
			float num = i;
			float num2 = 0f;
			if (m_Grid.maxPerLine > 0)
			{
				num = i % m_Grid.maxPerLine;
				num2 = Mathf.Floor(i / m_Grid.maxPerLine);
			}
			num *= m_Grid.cellWidth;
			num2 *= 0f - m_Grid.cellHeight;
			Vector3 from = new Vector3(num - m_Grid.cellWidth / 2f, num2 - m_Grid.cellHeight / 2f, 0f);
			Vector3 to = new Vector3(num + m_Grid.cellWidth / 2f, num2 + m_Grid.cellHeight / 2f, 0f);
			Gizmos.DrawLine(from, new Vector3(to.x, from.y));
			Gizmos.DrawLine(new Vector3(to.x, from.y), to);
			Gizmos.DrawLine(from, new Vector3(from.x, to.y));
			Gizmos.DrawLine(new Vector3(from.x, to.y), to);
		}
	}

	private void Awake()
	{
		if (SizeFactorOf <= 0)
		{
			SizeFactorOf = 1;
		}
		if ((bool)RootTile)
		{
			RootTile.gameObject.SetActive(value: false);
		}
		if ((bool)RootContent)
		{
			RootContent.gameObject.SetActive(value: false);
		}
		m_PanelParent = GetComponentInParent<UIDraggablePanel>();
	}

	private void Start()
	{
		Allocate();
		UIInventoryFilterManager.FilterChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Combine(UIInventoryFilterManager.FilterChanged, new UIInventoryFilterManager.OnFiltersChanged(OnFilterChanged));
	}

	private void OnEnable()
	{
		Reload();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
		UIInventoryFilterManager.FilterChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Remove(UIInventoryFilterManager.FilterChanged, new UIInventoryFilterManager.OnFiltersChanged(OnFilterChanged));
	}

	private void Update()
	{
		if (m_NeedsReload)
		{
			ExecuteReload();
		}
		if (!RecycleWidgets)
		{
			return;
		}
		int num = Mathf.FloorToInt(m_PanelParent.transform.localPosition.y / m_Grid.cellHeight);
		if (num >= m_Tiles.Length)
		{
			num = m_Tiles.Length - MaxVisibleItems;
		}
		num = Math.Max(num, 0);
		int num2 = num + MaxVisibleItems - 1;
		int i = num;
		bool flag = false;
		for (int j = 0; j < m_Tiles.Length; j++)
		{
			if (i >= m_Tiles.Length)
			{
				break;
			}
			if ((j < num || j > num2) && (bool)m_Tiles[j].WidgetOwner)
			{
				for (; i < m_Tiles.Length && (bool)m_Tiles[i].WidgetOwner; i++)
				{
				}
				if (i < m_Tiles.Length)
				{
					m_Tiles[i].TransplantWidgets(m_Tiles[j]);
					i++;
					flag = true;
				}
			}
		}
		if (flag && (bool)m_PanelParent)
		{
			m_PanelParent.panel.Refresh();
		}
	}

	private void LateUpdate()
	{
		if (m_NeedsReload)
		{
			ExecuteReload();
		}
	}

	private void OnFilterChanged(UIInventoryFilter.ItemFilterType mask)
	{
		if (FilterIsHide)
		{
			Reload();
		}
	}

	public void ChangeGridItems(Action<UIInventoryGridItem> action)
	{
		Allocate();
		UIInventoryGridItem[] tiles = m_Tiles;
		foreach (UIInventoryGridItem obj in tiles)
		{
			action(obj);
		}
		action(RootTile);
	}

	public UIInventoryGridItem FirstEmptyGridItem()
	{
		int num = Inventory.FirstFreeSlot();
		if (num >= 0)
		{
			return m_Tiles[num];
		}
		return null;
	}

	public void LootAll(BaseInventory into)
	{
		UIInventoryGridItem[] tiles = m_Tiles;
		foreach (UIInventoryGridItem uIInventoryGridItem in tiles)
		{
			if ((bool)uIInventoryGridItem)
			{
				uIInventoryGridItem.Loot(into);
				if (GlobalAudioPlayer.Instance != null)
				{
					GlobalAudioPlayer.Instance.AllowPlayingOfTakeSound = false;
				}
			}
		}
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.AllowPlayingOfTakeSound = true;
		}
		Reload();
	}

	public void SetGridItemClickDestination(UIInventoryItemZone clickDestination)
	{
		if (m_Tiles != null)
		{
			for (int i = 0; i < m_Tiles.Length; i++)
			{
				m_Tiles[i].ClickSendsTo = clickDestination;
			}
		}
	}

	public void SetGridItemSendsToPlayer(bool newSendsToPlayer)
	{
		if (m_Tiles != null)
		{
			for (int i = 0; i < m_Tiles.Length; i++)
			{
				m_Tiles[i].ClickSendsToPlayer = newSendsToPlayer;
			}
		}
	}

	protected override InventoryItem DoTake(InventoryItem item)
	{
		TryPlayTakeSound(item);
		Inventory.TakeItem(item);
		return item;
	}

	protected override bool DoCanPut(InventoryItem item, UIInventoryGridItem where)
	{
		return Inventory.CanPutItem(item, (bool)where && !where.Empty);
	}

	protected override bool DoPut(InventoryItem item, UIInventoryGridItem where)
	{
		TryPlayPutSound(item);
		if (AllowOrder && (bool)where)
		{
			item.uiSlot = where.Slot;
		}
		else
		{
			item.uiSlot = Inventory.FirstFreeSlot();
		}
		return Inventory.PutItem(item, item.uiSlot);
	}

	protected override Item DoRemove(Item item, int quantity)
	{
		return Inventory.RemoveItem(item, quantity);
	}

	protected override int DoAdd(Item item, int quantity, UIInventoryGridItem where)
	{
		quantity = ((!where) ? Inventory.AddItem(item, quantity) : Inventory.AddItem(item, quantity, where.Slot));
		return quantity;
	}

	private void TryPlayTakeSound(InventoryItem item)
	{
		if (GlobalAudioPlayer.Instance != null && GlobalAudioPlayer.Instance.AllowPlayingOfTakeSound)
		{
			GlobalAudioPlayer.Instance.Play(item.baseItem, GlobalAudioPlayer.UIInventoryAction.PickUpItem);
		}
	}

	private void TryPlayPutSound(InventoryItem item)
	{
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(item.baseItem, GlobalAudioPlayer.UIInventoryAction.DropItem);
		}
	}

	public int AddItem(Item item, int qty, int forceSlot)
	{
		return Inventory.AddItem(item, qty, forceSlot);
	}

	public void LoadInventory(BaseInventory inventory)
	{
		if (!(inventory == null))
		{
			if (Inventory != null)
			{
				Inventory.OnChanged -= OnInventoryChanged;
			}
			Inventory = inventory;
			base.OwnerGameObject = inventory.gameObject;
			Reload();
			Inventory.OnChanged += OnInventoryChanged;
		}
	}

	private void OnInventoryChanged(BaseInventory sender)
	{
		Reload();
	}

	public override void Reload()
	{
		m_NeedsReload = true;
	}

	private void ExecuteReload()
	{
		if (this.OnPreReload != null)
		{
			this.OnPreReload(this);
		}
		m_NeedsReload = false;
		if (!AllowOrder || CompressOnChange)
		{
			Inventory.CompressSlots();
		}
		Allocate();
		UIInventoryGridItem.SetRefreshBlock(block: true);
		for (int i = 0; i < m_Tiles.Length; i++)
		{
			m_Tiles[i].UnsetItem();
			if ((bool)Inventory && !IgnoreItemSlots && i >= Inventory.MaxItems && Inventory.MaxItems >= 0)
			{
				m_Tiles[i].Block();
			}
			else
			{
				m_Tiles[i].Unblock();
			}
		}
		if ((bool)Inventory)
		{
			int num = 0;
			IEnumerable<InventoryItem> applicableInventory = ApplicableInventory;
			foreach (InventoryItem item in applicableInventory)
			{
				if (!IgnoreItemSlots)
				{
					num = item.uiSlot;
					if (num < 0 || num >= m_Tiles.Length)
					{
						Debug.LogWarning("UI slot out of bounds. Reordering");
						int num3 = (item.uiSlot = Inventory.FirstFreeSlot());
						num = num3;
						if (num < 0 || num >= m_Tiles.Length)
						{
							Debug.LogError("ItemGrid: Too many items for grid.", base.gameObject);
							continue;
						}
					}
				}
				int num4 = num - PageSize * CurrentPage;
				if (num4 >= 0 && num4 < m_Tiles.Length)
				{
					m_Tiles[num4].SetItem(item);
				}
				num++;
			}
			m_MaxCount = (Inventory ? Mathf.CeilToInt((float)applicableInventory.Count() / (float)PageSize) : 0);
		}
		else
		{
			m_MaxCount = 1;
		}
		if (Capacity == 0)
		{
			for (int j = NeededSlots; j < m_Tiles.Length; j++)
			{
				if (m_Tiles[j].InvItem == null || m_Tiles[j].InvItem.baseItem == null)
				{
					m_Tiles[j].gameObject.SetActive(value: false);
				}
			}
			if ((bool)m_Grid)
			{
				m_Grid.Reposition();
			}
		}
		UIInventoryGridItem.SetRefreshBlock(block: false);
		UIInventoryGridItem[] tiles = m_Tiles;
		for (int num3 = 0; num3 < tiles.Length; num3++)
		{
			tiles[num3].RefreshIcon();
		}
		CurrentPage = CurrentPage;
		if (this.OnPostReload != null)
		{
			this.OnPostReload(this);
		}
	}

	public void Allocate()
	{
		if (!m_Grid)
		{
			m_Grid = GetComponent<UIGrid>();
		}
		if (m_Tiles == null)
		{
			m_Tiles = new UIInventoryGridItem[NeededSlots];
		}
		else if (Capacity == 0)
		{
			UIInventoryGridItem[] array = new UIInventoryGridItem[Mathf.Max(m_Tiles.Length, NeededSlots)];
			m_Tiles.CopyTo(array, 0);
			m_Tiles = array;
		}
		for (int i = 0; i < m_Tiles.Length; i++)
		{
			bool flag = RootTile.transform.parent.childCount - 2 < MaxVisibleItems;
			if (m_Tiles[i] == null)
			{
				m_Tiles[i] = NGUITools.AddChild(RootTile.transform.parent.gameObject, RootTile.gameObject).GetComponent<UIInventoryGridItem>();
				UIInventoryGridItem obj = m_Tiles[i];
				obj.OnItemPut = (ItemSwapDelegate)Delegate.Combine(obj.OnItemPut, new ItemSwapDelegate(ChildPutItem));
				UIInventoryGridItem obj2 = m_Tiles[i];
				obj2.OnItemTake = (ItemSwapDelegate)Delegate.Combine(obj2.OnItemTake, new ItemSwapDelegate(ChildTakeItem));
				UIInventoryGridItem obj3 = m_Tiles[i];
				obj3.OnClicked = (UIEventListener.VoidDelegate)Delegate.Combine(obj3.OnClicked, new UIEventListener.VoidDelegate(OnChildClicked));
				UIInventoryGridItem obj4 = m_Tiles[i];
				obj4.OnScrolled = (UIEventListener.FloatDelegate)Delegate.Combine(obj4.OnScrolled, new UIEventListener.FloatDelegate(OnChildScrolled));
				if ((bool)RootContent && flag)
				{
					GameObject gameObject = NGUITools.AddChild(m_Tiles[i].gameObject, RootContent);
					gameObject.SetActive(value: true);
					m_Tiles[i].WidgetOwner = gameObject;
					m_Tiles[i].Icon = gameObject.transform.Find("Icon").GetComponent<UITexture>();
					m_Tiles[i].Background = gameObject.transform.Find("Background").gameObject;
					m_Tiles[i].NonDragBackground = gameObject.transform.Find("NonDragBackground").gameObject;
					m_Tiles[i].QuantityLabel = gameObject.transform.Find("Quantity").GetComponent<UILabel>();
				}
			}
			m_Tiles[i].gameObject.SetActive(value: true);
			m_Tiles[i].Init(flag);
			m_Tiles[i].SetSlotNumber(i);
		}
		RootTile.gameObject.SetActive(value: false);
		if ((bool)m_Grid)
		{
			m_Grid.Reposition();
		}
	}

	private void ChildTakeItem(Item baseItem, int quantity)
	{
		if (OnItemTake != null)
		{
			OnItemTake(baseItem, quantity);
		}
	}

	private void ChildPutItem(Item baseItem, int quantity)
	{
		if (OnItemPut != null)
		{
			OnItemPut(baseItem, quantity);
		}
	}

	private void OnChildClicked(GameObject sender)
	{
		if (OnItemClicked != null)
		{
			OnItemClicked(sender);
		}
	}

	private void OnChildScrolled(GameObject sender, float delta)
	{
		if (OnItemScrolled != null)
		{
			OnItemScrolled(sender, delta);
		}
	}
}
