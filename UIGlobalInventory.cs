using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UIGlobalInventory : MonoBehaviour
{
	public delegate void DraggingChanged(bool dragging);

	private UIDraggedItem m_DragItemView;

	private float m_DragStartTime;

	public float MinDragTime = 0.018f;

	public UILabel MessageLabel;

	public GameObject MessageObject;

	private UIAnchor m_MessageAnchor;

	private float m_MessageTimer;

	private UIInventoryGridItem m_DraggedSource;

	private InventoryItem m_DraggedItem;

	private GameObject m_DraggedOwnerStart;

	private List<UIInventoryGridItem> m_SelectedSlots = new List<UIInventoryGridItem>();

	private List<UIInventoryGridItem> m_EnabledSlots = new List<UIInventoryGridItem>();

	private bool m_SelectChanged;

	private bool m_IsCanceling;

	public static UIGlobalInventory Instance
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		private set;
	}

	public bool RealDrag { get; private set; }

	public UIInventoryGridItem DraggedSource => m_DraggedSource;

	public InventoryItem DraggedItem => m_DraggedItem;

	public bool DraggingItem => m_DraggedItem != null;

	public bool MultiDrag { get; private set; }

	public event DraggingChanged OnDraggingChanged;

	private void Awake()
	{
		Instance = this;
		m_DragItemView = UnityEngine.Object.FindObjectOfType<UIDraggedItem>();
		if ((bool)m_DragItemView)
		{
			FinishDrag();
		}
		m_MessageAnchor = MessageObject.GetComponent<UIAnchor>();
		GameResources.OnPreSaveGame += OnPreSaveGame;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameResources.OnPreSaveGame -= OnPreSaveGame;
	}

	private void OnPreSaveGame()
	{
		CancelDrag();
	}

	public void HandleInput()
	{
	}

	private void LateUpdate()
	{
		if (m_MessageTimer > 0f)
		{
			m_MessageTimer -= TimeController.sUnscaledDelta;
			if (m_MessageTimer <= 0f)
			{
				MessageObject.SetActive(value: false);
			}
		}
		if (m_DraggedItem != null && m_DraggedItem.baseItem == null)
		{
			FinishDrag();
		}
		else if (GameInput.GetMouseButtonUp(0, setHandled: false) && RealDrag && TryDragDrop())
		{
			CancelDrag();
		}
		if (!GameInput.GetControlkey() && GameInput.GetMouseButtonUp(0, setHandled: false) && !m_SelectChanged)
		{
			UnselectAll();
		}
	}

	public void Activated(UIInventoryGridItem item)
	{
		m_EnabledSlots.Add(item);
	}

	public void Deactivated(UIInventoryGridItem item)
	{
		m_EnabledSlots.Remove(item);
	}

	public void Select(UIInventoryGridItem item, bool multi = false)
	{
		if (!multi && !GameInput.GetControlkey())
		{
			UnselectAll();
		}
		item.Selected = true;
		if (!m_SelectedSlots.Contains(item))
		{
			m_SelectedSlots.Add(item);
		}
		m_SelectChanged = true;
	}

	public void Unselect(UIInventoryGridItem item)
	{
		item.Selected = false;
		m_SelectedSlots.Remove(item);
		m_SelectChanged = true;
	}

	public void UnselectAll()
	{
		foreach (UIInventoryGridItem selectedSlot in m_SelectedSlots)
		{
			selectedSlot.Selected = false;
		}
		m_SelectedSlots.Clear();
	}

	public void FinishMultidragAt(UIInventoryItemZone zone)
	{
		for (int num = m_SelectedSlots.Count - 1; num >= 0; num--)
		{
			if (m_SelectedSlots[num].TryPutIn(zone))
			{
				m_SelectedSlots[num].Selected = false;
				m_SelectedSlots.RemoveAt(num);
			}
		}
		FinishDrag();
	}

	public void FinishMultidragAt(BaseInventory inventory, Func<UIInventoryGridItem, bool> valid)
	{
		for (int num = m_SelectedSlots.Count - 1; num >= 0; num--)
		{
			if (valid(m_SelectedSlots[num]) && inventory.PutItem(m_SelectedSlots[num].RemoveItem()))
			{
				m_SelectedSlots[num].Selected = false;
				m_SelectedSlots.RemoveAt(num);
			}
		}
		FinishDrag();
	}

	public bool TryDragDrop()
	{
		if (Time.realtimeSinceStartup - m_DragStartTime >= MinDragTime)
		{
			return true;
		}
		RealDrag = false;
		return false;
	}

	public bool DragOwnerIsSame()
	{
		return m_DraggedOwnerStart == m_DraggedSource.Owner.OwnerGameObject;
	}

	public void BeginDrag(UIInventoryGridItem item, bool realDrag, int qty = -1)
	{
		MultiDrag = false;
		if (qty == 0)
		{
			return;
		}
		UIAbilityTooltip.GlobalHide();
		if (item != null && item.InvItem != null)
		{
			m_DragStartTime = Time.realtimeSinceStartup;
			if (item.Selected && m_SelectedSlots.Count > 1)
			{
				MultiDrag = true;
			}
			else
			{
				if (qty < 0)
				{
					qty = item.InvItem.stackSize;
				}
				if (qty >= item.InvItem.stackSize)
				{
					m_DraggedItem = item.RemoveItem();
				}
				else
				{
					m_DraggedItem = new InventoryItem();
					m_DraggedItem.baseItem = GameResources.Instantiate<Item>(item.InvItem.baseItem.Prefab);
					m_DraggedItem.baseItem.Prefab = item.InvItem.baseItem.Prefab;
					m_DraggedItem.SetStackSize(qty);
					item.InvItem.SetStackSize(item.InvItem.stackSize - qty);
				}
				if ((bool)m_DraggedItem.baseItem)
				{
					m_DraggedItem.baseItem.Location = Item.ItemLocation.Dragged;
				}
				if (this.OnDraggingChanged != null)
				{
					this.OnDraggingChanged(DraggingItem);
				}
				if (m_DraggedItem != null)
				{
					RealDrag = realDrag;
					m_DraggedSource = item;
					m_DraggedOwnerStart = m_DraggedSource.Owner.OwnerGameObject;
					Unselect(item);
					item.RefreshIcon();
					m_DragItemView.SetVisible(isVisible: true);
					if ((bool)m_DraggedItem.baseItem)
					{
						m_DragItemView.LoadInventoryItem(m_DraggedItem);
					}
					else
					{
						FinishDrag();
					}
				}
			}
		}
		if (!MultiDrag)
		{
			UnselectAll();
		}
	}

	public void CancelDrag()
	{
		if (m_IsCanceling)
		{
			UnityEngine.Debug.LogError("Failed to cancel item drag.");
			return;
		}
		m_IsCanceling = true;
		if (MultiDrag)
		{
			MultiDrag = false;
		}
		else if (DraggingItem)
		{
			m_DraggedSource.TryDropItem();
		}
		m_IsCanceling = false;
	}

	public void DestroyItem(Item item, int qty)
	{
		if (DraggedItem != null && DraggedItem.baseItem == item)
		{
			if (DraggedItem.stackSize <= qty)
			{
				PersistenceManager.RemoveObject(DraggedItem.BaseItem.GetComponent<Persistence>());
				GameUtilities.Destroy(DraggedItem.baseItem.gameObject);
				FinishDrag();
			}
			else
			{
				DraggedItem.SetStackSize(DraggedItem.stackSize - qty);
			}
		}
	}

	public void FinishDrag()
	{
		m_DraggedItem = null;
		if (this.OnDraggingChanged != null)
		{
			this.OnDraggingChanged(DraggingItem);
		}
		m_DraggedSource = null;
		if (m_DragItemView != null)
		{
			m_DragItemView.SetVisible(isVisible: false);
		}
		RealDrag = false;
		MultiDrag = false;
	}

	public void PostMessage(string message, UIWidget anchor, float duration)
	{
		if (string.IsNullOrEmpty(message))
		{
			MessageObject.SetActive(value: false);
			return;
		}
		MessageObject.SetActive(value: true);
		MessageLabel.text = message;
		m_MessageAnchor.widgetContainer = anchor;
		m_MessageTimer = duration;
	}

	public void PostMessage(string message, UIWidget anchor)
	{
		PostMessage(message, anchor, 4f);
	}

	public void HideMessage()
	{
		PostMessage("", null);
	}
}
