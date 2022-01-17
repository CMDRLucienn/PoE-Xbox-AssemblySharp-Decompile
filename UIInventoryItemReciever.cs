using System;
using UnityEngine;

public class UIInventoryItemReciever : UIInventoryItemZone
{
	private BaseInventory m_Reciever;

	public static bool RecievedThisFrame;

	public UIInventoryGridItem SampleBox;

	public bool RecieverIsStash;

	public AccessLevel RestrictBy = AccessLevel.InField;

	public bool IgnoreEquippable;

	public BaseInventory Reciever
	{
		get
		{
			if (RecieverIsStash)
			{
				return GameState.s_playerCharacter.GetComponent<StashInventory>();
			}
			return m_Reciever;
		}
		set
		{
			m_Reciever = value;
			RecieverIsStash = false;
		}
	}

	private void Start()
	{
		if ((bool)SampleBox)
		{
			SampleBox.Init();
		}
		UIEventListener uIEventListener = UIEventListener.Get(this);
		uIEventListener.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(uIEventListener.onDrop, new UIEventListener.ObjectDelegate(OnDrop2));
	}

	private void OnClick()
	{
		DoPut();
	}

	private void OnDrop(GameObject source)
	{
		if (UIGlobalInventory.Instance.TryDragDrop())
		{
			DoPut();
		}
	}

	private void OnDrop2(GameObject go, GameObject source)
	{
		if (UIGlobalInventory.Instance.TryDragDrop())
		{
			DoPut();
		}
	}

	private void LateUpdate()
	{
		RecievedThisFrame = false;
	}

	private void DoPut()
	{
		if (Reciever == null || UIInventoryItemZone.GetCurrentAccessLevel() < RestrictBy)
		{
			return;
		}
		string error;
		if (UIGlobalInventory.Instance.MultiDrag)
		{
			UIGlobalInventory.Instance.FinishMultidragAt(Reciever, ItemValidHere);
			RecievedThisFrame = true;
		}
		else if (UIInventoryManager.Instance.DraggingItem && ItemValidHere(UIInventoryManager.Instance.DraggedItem, UIInventoryManager.Instance.DraggedSource, out error))
		{
			RecievedThisFrame = true;
			int num = Reciever.AddItem(UIInventoryManager.Instance.DraggedItem.baseItem, UIInventoryManager.Instance.DraggedItem.stackSize, -1);
			if (num > 0)
			{
				UIInventoryManager.Instance.DraggedItem.stackSize = num;
				UIInventoryManager.Instance.CancelDrag();
			}
			else
			{
				TryPlayPutSound(UIInventoryManager.Instance.DraggedItem);
				UIInventoryManager.Instance.FinishDrag();
			}
		}
	}

	private void TryPlayPutSound(InventoryItem item)
	{
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(item.baseItem, GlobalAudioPlayer.UIInventoryAction.DropItem);
		}
	}

	public bool ItemValidHere(UIInventoryGridItem from)
	{
		string error;
		return ItemValidHere(from.InvItem, from, out error);
	}

	public bool ItemValidHere(InventoryItem item, UIInventoryGridItem from, out string error)
	{
		error = "";
		if (!UIInventoryGridItem.ItemTransferValid(item, from, SampleBox, out error))
		{
			return false;
		}
		if (IgnoreEquippable && (bool)from && from.InvItem != null && (bool)from.InvItem.baseItem && (bool)from.InvItem.baseItem.GetComponent<Equippable>())
		{
			return false;
		}
		return true;
	}

	public void SetReciever(BaseInventory r)
	{
		m_Reciever = r;
	}

	protected override InventoryItem DoTake(InventoryItem from)
	{
		return Reciever.TakeItem(from);
	}

	protected override bool DoCanPut(InventoryItem item, UIInventoryGridItem where)
	{
		return Reciever.CanPutItem(item);
	}

	protected override bool DoPut(InventoryItem item, UIInventoryGridItem where)
	{
		return Reciever.PutItem(item);
	}

	protected override Item DoRemove(Item item, int quantity)
	{
		return Reciever.RemoveItem(item, quantity);
	}

	protected override int DoAdd(Item item, int quantity, UIInventoryGridItem where)
	{
		return Reciever.AddItem(item, quantity);
	}

	public override void Reload()
	{
	}
}
