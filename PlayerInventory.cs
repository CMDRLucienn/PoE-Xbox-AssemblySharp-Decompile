using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Equipment))]
public class PlayerInventory : Inventory
{
	private struct LogItemKey
	{
		public string destination;

		public string itemname;

		public LogItemKey(string destination, string itemname)
		{
			this.destination = destination;
			this.itemname = itemname;
		}

		public override bool Equals(object obj)
		{
			if (obj is LogItemKey logItemKey)
			{
				if (logItemKey.destination == destination)
				{
					return logItemKey.itemname == itemname;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return destination.GetHashCode() + 11 * itemname.GetHashCode();
		}
	}

	[Persistent]
	public CurrencyValue currencyTotalValue = new CurrencyValue();

	[Persistent]
	public int campingSupplies;

	private Inventory m_prioritizedInventory;

	private bool m_Stashed;

	private static Dictionary<LogItemKey, int> s_ItemsToLog = new Dictionary<LogItemKey, int>();

	private static int s_LogItemsDisabled = 0;

	public QuestInventory QuestInventory { get; private set; }

	public CraftingInventory CraftingInventory { get; private set; }

	public StashInventory StashInventory { get; private set; }

	public int CampingSuppliesTotal
	{
		get
		{
			return campingSupplies;
		}
		set
		{
			campingSupplies = Mathf.Clamp(value, 0, CampingSupplies.StackMaximum);
		}
	}

	public override void Start()
	{
		base.Start();
		GameState.OnDifficultyChanged = (GameState.DifficultyDelegate)Delegate.Combine(GameState.OnDifficultyChanged, new GameState.DifficultyDelegate(OnDifficultyChanged));
		Initialize();
	}

	private void OnDestroy()
	{
		GameState.OnDifficultyChanged = (GameState.DifficultyDelegate)Delegate.Remove(GameState.OnDifficultyChanged, new GameState.DifficultyDelegate(OnDifficultyChanged));
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Update()
	{
		if (s_LogItemsDisabled <= 0 && s_ItemsToLog.Count > 0)
		{
			try
			{
				foreach (KeyValuePair<LogItemKey, int> item in s_ItemsToLog)
				{
					string text = item.Key.itemname;
					if (item.Value != 1)
					{
						text = GUIUtils.FormatWithLinks(1625, item.Value, text);
					}
					text = GUIUtils.Format(1728, text);
					Console.AddMessage(GUIUtils.FormatWithLinks(299, text, item.Key.destination), Scripts.ConsoleNotifyColor, Console.ConsoleState.Both);
				}
			}
			finally
			{
				s_ItemsToLog.Clear();
			}
		}
		base.Update();
	}

	private void OnDifficultyChanged(GameDifficulty newDiff)
	{
		CampingSuppliesTotal = CampingSuppliesTotal;
	}

	private void Initialize()
	{
		if (!QuestInventory)
		{
			QuestInventory = GetComponent<QuestInventory>();
			if (!QuestInventory)
			{
				QuestInventory = base.gameObject.AddComponent<QuestInventory>();
			}
		}
		if (!CraftingInventory)
		{
			CraftingInventory = GetComponent<CraftingInventory>();
			if (!CraftingInventory)
			{
				CraftingInventory = base.gameObject.AddComponent<CraftingInventory>();
			}
		}
		if (!StashInventory)
		{
			StashInventory = GetComponent<StashInventory>();
			if (!StashInventory)
			{
				StashInventory = base.gameObject.AddComponent<StashInventory>();
			}
		}
	}

	public int AddItemAndLog(Item newItem, int addCount, GameObject targetInventory)
	{
		m_Stashed = false;
		m_prioritizedInventory = null;
		if (targetInventory != null)
		{
			m_prioritizedInventory = targetInventory.GetComponent<Inventory>();
			if (m_prioritizedInventory == this)
			{
				m_prioritizedInventory = null;
			}
		}
		int num = AddItem(newItem, addCount);
		Scripts.LogItemGet(newItem, addCount - num, m_Stashed);
		return num;
	}

	public override int AddItem(Item newItem, int addCount, int forceSlot, bool original)
	{
		if (newItem == null || addCount <= 0)
		{
			return 0;
		}
		Initialize();
		if (newItem is Currency)
		{
			AddCurrency((float)addCount * newItem.GetValue());
			CallObtainedScripts(newItem);
			return 0;
		}
		if (newItem is CampingSupplies)
		{
			int num = CampingSuppliesTotal + addCount - CampingSupplies.StackMaximum;
			if (num < 0)
			{
				num = 0;
			}
			if (num != addCount)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(1807), Color.green);
				CallObtainedScripts(newItem);
			}
			CampingSuppliesTotal += addCount;
			return num;
		}
		if (newItem.IsQuestItem)
		{
			QuestInventory.AddItem(newItem, addCount);
			return 0;
		}
		if (newItem.IsRedirectIngredient)
		{
			CraftingInventory.AddItem(newItem, addCount);
			return 0;
		}
		BaseInventory.m_EnableInventoryVoiceCues = false;
		if (m_prioritizedInventory != null)
		{
			addCount = m_prioritizedInventory.AddItem(newItem, addCount);
		}
		if (addCount > 0)
		{
			addCount = base.AddItem(newItem, addCount, forceSlot, original: false);
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (addCount <= 0)
			{
				break;
			}
			Inventory component = onlyPrimaryPartyMember.GetComponent<Inventory>();
			if (!(component is PlayerInventory) && (bool)component && component != m_prioritizedInventory)
			{
				addCount = component.AddItem(newItem, addCount);
			}
		}
		if (addCount > 0)
		{
			if (m_prioritizedInventory != null)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(m_prioritizedInventory.gameObject, SoundSet.SoundAction.InventoryFull, SoundSet.s_MediumVODelay, forceInterrupt: true);
			}
			else
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InventoryFull, SoundSet.s_MediumVODelay, forceInterrupt: true);
			}
			addCount = StashInventory.AddItem(newItem, addCount);
			m_Stashed = true;
		}
		BaseInventory.m_EnableInventoryVoiceCues = true;
		m_prioritizedInventory = null;
		return addCount;
	}

	public override Item RemoveItem(Item item, int removeCount)
	{
		if (item == null || removeCount <= 0)
		{
			return null;
		}
		Initialize();
		if (item is Currency)
		{
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.RemoveGold);
			}
			if (RemoveCurrency(item.GetValue(), removeCount) == removeCount)
			{
				return null;
			}
			return InstantiateStoredItem(item.Prefab);
		}
		if (item is CampingSupplies)
		{
			CampingSuppliesTotal -= removeCount;
			return InstantiateStoredItem(item.Prefab);
		}
		if (item.IsQuestItem)
		{
			return QuestInventory.RemoveItem(item, removeCount);
		}
		if (item.IsRedirectIngredient)
		{
			return CraftingInventory.RemoveItem(item, removeCount);
		}
		return base.RemoveItem(item, removeCount);
	}

	public override bool CanPutItem(InventoryItem item, bool isSwap = false)
	{
		if (item == null || item.baseItem == null)
		{
			return true;
		}
		if (item.baseItem is Currency)
		{
			return true;
		}
		if (item.baseItem is CampingSupplies)
		{
			return Mathf.Max(CampingSuppliesTotal + item.stackSize - CampingSupplies.StackMaximum, 0) == 0;
		}
		if (item.baseItem.IsQuestItem)
		{
			return QuestInventory.CanPutItem(item);
		}
		if (item.baseItem.IsRedirectIngredient)
		{
			return CraftingInventory.CanPutItem(item);
		}
		if (base.CanPutItem(item, isSwap))
		{
			return true;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember)
			{
				Inventory component = onlyPrimaryPartyMember.GetComponent<Inventory>();
				if (!(component is PlayerInventory) && component.CanPutItem(item))
				{
					return true;
				}
			}
		}
		return StashInventory.CanPutItem(item);
	}

	public override bool PutItem(InventoryItem item, int slot)
	{
		if (item == null || item.baseItem == null)
		{
			return true;
		}
		Initialize();
		if (item.baseItem is Currency)
		{
			AddCurrency((float)item.stackSize * item.baseItem.GetValue());
			CallObtainedScripts(item.baseItem);
			GameUtilities.Destroy(item.baseItem.gameObject);
			return true;
		}
		if (item.baseItem is CampingSupplies)
		{
			int num = CampingSuppliesTotal + item.stackSize - CampingSupplies.StackMaximum;
			if (num < 0)
			{
				num = 0;
			}
			if (num != item.stackSize)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(1807), Color.green);
				CallObtainedScripts(item.baseItem);
			}
			CampingSuppliesTotal += item.stackSize - num;
			item.SetStackSize(num);
			if (num == 0)
			{
				GameUtilities.Destroy(item.baseItem.gameObject);
				return true;
			}
			return false;
		}
		if (item.baseItem.IsQuestItem)
		{
			return QuestInventory.PutItem(item, slot);
		}
		if (item.baseItem.IsRedirectIngredient)
		{
			return CraftingInventory.PutItem(item, slot);
		}
		if (base.PutItem(item, slot))
		{
			return true;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember)
			{
				Inventory component = onlyPrimaryPartyMember.GetComponent<Inventory>();
				if (!(component is PlayerInventory) && component.PutItem(item, slot))
				{
					return true;
				}
			}
		}
		return StashInventory.PutItem(item, slot);
	}

	public override int DestroyItem(Item item, int destroyCount)
	{
		if (item == null || destroyCount <= 0)
		{
			return 0;
		}
		Initialize();
		if (item is Currency)
		{
			destroyCount = RemoveCurrency(item.GetValue(), destroyCount);
		}
		else if (!(item is CampingSupplies))
		{
			destroyCount = (item.IsQuestItem ? QuestInventory.DestroyItem(item, destroyCount) : ((!item.IsRedirectIngredient) ? base.DestroyItem(item, destroyCount) : CraftingInventory.DestroyItem(item, destroyCount)));
		}
		else
		{
			int num = destroyCount - CampingSuppliesTotal;
			if (num < 0)
			{
				num = 0;
			}
			CampingSuppliesTotal -= destroyCount;
			destroyCount = num;
		}
		return destroyCount;
	}

	public override int DestroyItem(string itemName, int destroyCount)
	{
		if (destroyCount <= 0)
		{
			return 0;
		}
		Initialize();
		int destroyCount2 = destroyCount;
		destroyCount2 = base.DestroyItem(itemName, destroyCount2);
		destroyCount2 = QuestInventory.DestroyItem(itemName, destroyCount2);
		return CraftingInventory.DestroyItem(itemName, destroyCount2);
	}

	public void AddCurrency(float amount)
	{
		if ((bool)GlobalAudioPlayer.Instance)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ReceiveGold);
		}
		currencyTotalValue.v += amount;
	}

	public int RemoveCurrency(float itemWorth, int stackSize)
	{
		float num = (float)stackSize * itemWorth;
		if (num <= currencyTotalValue.v)
		{
			currencyTotalValue.v -= num;
			stackSize = 0;
		}
		else
		{
			Debug.LogWarning("Prevented attempt to remove more currency than inventory contained.");
			int num2 = 0;
			if (itemWorth > 0f)
			{
				num2 = (int)(currencyTotalValue.v / itemWorth);
			}
			num = (float)stackSize * itemWorth;
			currencyTotalValue.v -= num;
			stackSize -= num2;
		}
		return stackSize;
	}

	public override int ItemCount(Item item)
	{
		Initialize();
		if (item == null)
		{
			return 0;
		}
		if (item is Currency)
		{
			if (item.GetValue() == 0f)
			{
				return 0;
			}
			return (int)(currencyTotalValue.v / item.GetValue());
		}
		if (item is CampingSupplies)
		{
			return CampingSuppliesTotal;
		}
		int num = base.ItemCount(item);
		if ((bool)QuestInventory)
		{
			num += QuestInventory.ItemCount(item);
		}
		if ((bool)CraftingInventory)
		{
			num += CraftingInventory.ItemCount(item);
		}
		return num;
	}

	public override int ItemCount(string itemName)
	{
		Initialize();
		return base.ItemCount(itemName) + QuestInventory.ItemCount(itemName) + CraftingInventory.ItemCount(itemName) + StashInventory.ItemCount(itemName);
	}

	public static void LogItemGet(string destination, Item item, int quantity)
	{
		if ((bool)item)
		{
			LogItemKey key = new LogItemKey(destination, item.Name);
			if (s_ItemsToLog.ContainsKey(key))
			{
				s_ItemsToLog[key] += quantity;
			}
			else
			{
				s_ItemsToLog[key] = quantity;
			}
		}
	}

	public static void PauseItemLogging()
	{
		s_LogItemsDisabled++;
	}

	public static void ResumeItemLogging()
	{
		s_LogItemsDisabled--;
		if (s_LogItemsDisabled < 0)
		{
			UIDebug.Instance.LogOnScreenWarning("Unbalanced item log disable (PlayerInventory.ResumeItemLogging)", UIDebug.Department.Programming, 10f);
			s_LogItemsDisabled = 0;
		}
	}
}
