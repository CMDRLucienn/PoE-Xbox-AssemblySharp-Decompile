using System;
using UnityEngine;

public class UIStoreStorePage : UIParentSelectorListener
{
	public UIInventoryItemGrid StoreGrid;

	public UILabel StoreEmptyLabel;

	public UIInventoryRowPanel PlayerPanel;

	public UIDraggablePanel PlayerPanelPanel;

	public UIInventoryStashGrid StashGrid;

	public UIDraggablePanel StorePanelPanel;

	public UIInventoryItemReciever StoreReciever;

	public UIInventoryItemGrid ToBuyGrid;

	public UIInventoryItemGrid ToSellGrid;

	public UIMultiSpriteImageButton DoTradeButton;

	public UILabel TransferValueLabel;

	public UIWidget MoneyToShopkeep;

	public UIWidget MoneyToPlayer;

	public Color TransferValidColor;

	public Color TransferInvalidColor;

	private Inventory ToSellItems;

	private Inventory ToBuyItems;

	private PlayerInventory m_PlayerInventory;

	private Store m_Store;

	public GameObject Backgrounds;

	public PartyInventoryType ViewingPage { get; private set; }

	public bool TradeIsPending
	{
		get
		{
			if (ToBuyGrid.Inventory.Empty())
			{
				return !ToSellGrid.Inventory.Empty();
			}
			return true;
		}
	}

	public event Action<PartyInventoryType> OnViewingChanged;

	private void OnEnable()
	{
		Backgrounds.SetActive(value: true);
		PlayerPanel.ReloadParty();
	}

	private void OnDisable()
	{
		Backgrounds.SetActive(value: false);
	}

	protected override void Start()
	{
		Init();
		base.Start();
		ToSellGrid.OnPostReload += OnTransactionChanged;
		ToBuyGrid.OnPostReload += OnTransactionChanged;
		StoreGrid.OnPreReload += OnStoreWillReload;
		UIMultiSpriteImageButton doTradeButton = DoTradeButton;
		doTradeButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(doTradeButton.onClick, new UIEventListener.VoidDelegate(OnDoTrade));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)ToSellGrid)
		{
			ToSellGrid.OnPostReload -= OnTransactionChanged;
		}
		if ((bool)ToBuyGrid)
		{
			ToBuyGrid.OnPostReload -= OnTransactionChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnTransactionChanged(UIInventoryItemGrid sender)
	{
		UpdateTransferValue();
	}

	private void OnStoreWillReload(UIInventoryItemGrid sender)
	{
		if ((bool)m_Store)
		{
			m_Store.Sort(BaseInventory.CompareItemsForShop);
		}
	}

	private void OnDoTrade(GameObject sender)
	{
		FinalizeTransaction();
		UpdateTransferValue();
	}

	private void OnPageChanged(UIStorePageType page)
	{
	}

	private void UpdateTransferValue()
	{
		if (!m_Store)
		{
			return;
		}
		TransferValueLabel.gameObject.SetActive(TradeIsPending);
		float num = 0f;
		foreach (InventoryItem item in ToBuyGrid.Inventory)
		{
			num -= item.GetBuyValue(m_Store);
		}
		foreach (InventoryItem item2 in ToSellGrid.Inventory)
		{
			num += item2.GetSellValue(m_Store);
		}
		Init();
		TransferValueLabel.text = GUIUtils.Format(466, Mathf.Abs(Mathf.FloorToInt(num)));
		if (0f - num > m_PlayerInventory.currencyTotalValue.v)
		{
			TransferValueLabel.color = TransferInvalidColor;
		}
		else
		{
			TransferValueLabel.color = TransferValidColor;
		}
		DoTradeButton.enabled = TradeIsPending;
		MoneyToShopkeep.gameObject.SetActive(num < 0f);
		MoneyToPlayer.gameObject.SetActive(num > 0f);
	}

	public void StartTransaction()
	{
		Init();
		CancelTransaction();
		ToSellItems.MaxItems = int.MaxValue;
		ToSellGrid.LoadInventory(ToSellItems);
		ToBuyItems.MaxItems = int.MaxValue;
		ToBuyGrid.LoadInventory(ToBuyItems);
		UpdateTransferValue();
	}

	private void OnDialogEnd(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			if (owner.CheckboxActive)
			{
				GameState.Option.SetOption(GameOption.BoolOption.DONT_WARN_STORE_TOO_POOR, setting: true);
			}
			FinalizeTransaction();
		}
	}

	public void FinalizeTransaction()
	{
		if (!m_Store)
		{
			return;
		}
		float num = m_Store.currencyStoreBank;
		float num2 = m_PlayerInventory.currencyTotalValue;
		for (int num3 = ToSellItems.ItemList.Count - 1; num3 >= 0; num3--)
		{
			InventoryItem inventoryItem = ToSellItems.ItemList[num3];
			if (m_Store.CanPutItem(inventoryItem))
			{
				float sellValue = inventoryItem.GetSellValue(m_Store);
				num -= sellValue;
				num2 += sellValue;
			}
		}
		bool flag = true;
		if (ToBuyItems.ItemList.Count <= 0)
		{
			flag = false;
		}
		for (int num4 = ToBuyItems.ItemList.Count - 1; num4 >= 0; num4--)
		{
			InventoryItem inventoryItem2 = ToBuyItems.ItemList[num4];
			if (!(inventoryItem2.baseItem is CampingSupplies))
			{
				flag = false;
			}
			if ((inventoryItem2.baseItem is CampingSupplies && m_PlayerInventory.CampingSuppliesTotal < CampingSupplies.StackMaximum) || m_PlayerInventory.CanPutItem(inventoryItem2))
			{
				float buyValue = inventoryItem2.GetBuyValue(m_Store);
				num2 -= buyValue;
				num += buyValue;
			}
		}
		if (num2 < 0f)
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
			return;
		}
		if (flag && m_PlayerInventory.CampingSuppliesTotal == CampingSupplies.StackMaximum)
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(2122));
			return;
		}
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.Trade);
		for (int num5 = ToSellItems.ItemList.Count - 1; num5 >= 0; num5--)
		{
			InventoryItem inventoryItem3 = ToSellItems.ItemList[num5];
			if (m_Store.PutItem(inventoryItem3))
			{
				ToSellItems.TakeItem(inventoryItem3);
				float sellValue2 = inventoryItem3.GetSellValue(m_Store);
				m_Store.currencyStoreBank.v -= sellValue2;
				m_PlayerInventory.currencyTotalValue.v += sellValue2;
			}
		}
		for (int num6 = ToBuyItems.ItemList.Count - 1; num6 >= 0; num6--)
		{
			InventoryItem inventoryItem4 = ToBuyItems.ItemList[num6];
			float num7 = inventoryItem4.GetBuyValue(m_Store);
			if (PartyHelper.PutItem(inventoryItem4, ParentSelector.SelectedCharacter.gameObject))
			{
				ToBuyItems.TakeItem(inventoryItem4);
				if ((bool)inventoryItem4.baseItem.GetComponent<Grimoire>())
				{
					TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.GRIMOIRE_LOOTED);
				}
			}
			else
			{
				num7 -= inventoryItem4.GetBuyValue(m_Store);
			}
			m_PlayerInventory.currencyTotalValue.v -= num7;
			m_Store.currencyStoreBank.v += num7;
		}
		ToBuyGrid.Reload();
		ToSellGrid.Reload();
		StoreGrid.Reload();
		PlayerPanel.ReloadGrids();
		StoreEmptyLabel.alpha = ((m_Store.ItemList.Count <= 0) ? 1 : 0);
	}

	public void CancelTransaction()
	{
		if (!m_Store)
		{
			return;
		}
		foreach (InventoryItem item in ToBuyItems.ItemList)
		{
			if (!m_Store.PutItem(item))
			{
				Debug.LogError("Failed to Put item " + item.baseItem.name + " back into store inventory. The item was lost!");
			}
		}
		foreach (InventoryItem item2 in ToSellItems.ItemList)
		{
			if (!m_PlayerInventory.PutItem(item2))
			{
				Debug.LogError("Failed to Put item " + item2.baseItem.name + " back into player inventory. The item was lost!");
			}
		}
		ToBuyItems.ItemList.Clear();
		ToSellItems.ItemList.Clear();
	}

	private void Init()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			m_PlayerInventory = GameState.s_playerCharacter.Inventory;
		}
		if (!(ToSellItems != null))
		{
			UIStoreManager.Instance.OnPageChanged += OnPageChanged;
			ToSellItems = base.gameObject.AddComponent<Inventory>();
			ToBuyItems = base.gameObject.AddComponent<Inventory>();
		}
	}

	public void ChangePlayerViewing(PartyInventoryType target)
	{
		ViewingPage = target;
		PlayerPanelPanel.ResetPosition();
		PlayerPanel.gameObject.SetActive(target == PartyInventoryType.Party);
		StashGrid.gameObject.SetActive(target != PartyInventoryType.Party);
		if (this.OnViewingChanged != null)
		{
			this.OnViewingChanged(target);
		}
		PlayerPanelPanel.ResetPosition();
	}

	public void Set(Store store)
	{
		m_Store = store;
		StoreGrid.LoadInventory(m_Store);
		StoreReciever.SetReciever(m_Store);
		StoreEmptyLabel.alpha = ((m_Store.ItemList.Count <= 0) ? 1 : 0);
	}

	public void Show()
	{
		StartTransaction();
		StoreGrid.Reload();
		StorePanelPanel.ResetPosition();
		ChangePlayerViewing(PartyInventoryType.Party);
		StoreEmptyLabel.alpha = ((!m_Store || m_Store.ItemList.Count <= 0) ? 1 : 0);
	}
}
