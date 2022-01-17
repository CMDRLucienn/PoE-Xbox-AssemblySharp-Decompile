using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMultiSpriteImageButton : UIIsButton
{
	private List<UIIsButton> m_ContainedButtons;

	public GameObject SizeReference;

	private bool m_Pressed;

	private bool m_Hovered;

	private bool m_ForcedDown;

	private bool m_ForcedHighlight;

	private UIAbilityBarButton m_abilityBarButton;

	public UIEventListener.VoidDelegate onClick;

	public UIEventListener.VoidDelegate onRightClick;

	public UIEventListener.ObjectDelegate onDrop;

	public UIEventListener.VectorDelegate onDrag;

	public UIEventListener.BoolDelegate onPress;

	public UIEventListener.BoolDelegate onHover;

	public UIEventListener.BoolDelegate onTooltip;

	public UIAudioList.UIAudioType ButtonDownSound;

	public UIAudioList.UIAudioType ButtonUpSound;

	public UIAudioList.UIAudioType HoverSound;

	public Vector3 ClickLabelOffset;

	private Vector3 m_LabelPosInitial;

	public UILabel Label;

	public GameObject Collider;

	private bool m_IsNotifying;

	private Consumable m_abilityBarButtonConsumable
	{
		get
		{
			if ((bool)m_abilityBarButton && m_abilityBarButton.TargetItem != null && (bool)m_abilityBarButton.TargetItem.baseItem)
			{
				return m_abilityBarButton.TargetItem.baseItem.GetComponent<Consumable>();
			}
			return null;
		}
	}

	private void OnEnable()
	{
		FindChildren();
		foreach (UIIsButton containedButton in m_ContainedButtons)
		{
			if ((bool)containedButton)
			{
				containedButton.enabled = true;
			}
		}
		InitConsumableCheck();
	}

	private void OnDisable()
	{
		FindChildren();
		foreach (UIIsButton containedButton in m_ContainedButtons)
		{
			if ((bool)containedButton)
			{
				containedButton.enabled = false;
			}
		}
	}

	private void Awake()
	{
		if (!Label)
		{
			Label = GetComponentInChildren<UILabel>();
		}
		if (Label != null)
		{
			m_LabelPosInitial = Label.transform.localPosition;
			if (SizeReference != null)
			{
				Label.lineWidth = (int)SizeReference.transform.localScale.x;
			}
		}
	}

	private void Start()
	{
		FindChildren();
		InitConsumableCheck();
		if (!Collider)
		{
			Transform transform = base.transform.Find("Collider");
			if ((bool)transform)
			{
				Collider = transform.gameObject;
			}
		}
		if (!Collider)
		{
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(base.transform);
			Collider = new GameObject("Collider");
			Collider.layer = base.gameObject.layer;
			Collider.transform.parent = base.transform;
			Collider.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
			Collider.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, -2f);
			Collider.AddComponent<BoxCollider>();
			UINoClick uINoClick = Collider.AddComponent<UINoClick>();
			Collider.AddComponent<UIDragPanelContents>();
			uINoClick.BlockScrolling = false;
		}
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnChildHover));
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnChildPress));
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		uIEventListener.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(uIEventListener.onDrop, new UIEventListener.ObjectDelegate(OnChildDrop));
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnChildDrag));
	}

	private void InitConsumableCheck()
	{
		if (m_abilityBarButton == null)
		{
			m_abilityBarButton = GetComponent<UIAbilityBarButton>();
		}
	}

	public void ReFindChildren()
	{
		m_ContainedButtons = null;
		FindChildren();
	}

	private void FindChildren()
	{
		if (m_ContainedButtons == null)
		{
			m_ContainedButtons = new List<UIIsButton>();
		}
		if (m_ContainedButtons.Count == 0)
		{
			FindChildren(base.transform);
		}
	}

	private void FindChildren(Transform t)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			if ((bool)t.GetChild(i).GetComponent<UIMultiSpriteImageButton>())
			{
				continue;
			}
			UIIsButton[] components = t.GetChild(i).GetComponents<UIIsButton>();
			UIIsButton[] array = components;
			for (int j = 0; j < array.Length; j++)
			{
				UIImageButtonRevised uIImageButtonRevised = array[j] as UIImageButtonRevised;
				if ((bool)uIImageButtonRevised)
				{
					if (ButtonDownSound == UIAudioList.UIAudioType.None)
					{
						ButtonDownSound = uIImageButtonRevised.ButtonDownSound;
					}
					if (ButtonUpSound == UIAudioList.UIAudioType.None)
					{
						ButtonUpSound = uIImageButtonRevised.ButtonUpSound;
					}
					if (HoverSound == UIAudioList.UIAudioType.None)
					{
						HoverSound = uIImageButtonRevised.HoverSound;
					}
					uIImageButtonRevised.ButtonDownSound = (uIImageButtonRevised.ButtonUpSound = (uIImageButtonRevised.HoverSound = UIAudioList.UIAudioType.None));
				}
			}
			m_ContainedButtons.AddRange(components);
			FindChildren(t.GetChild(i));
		}
	}

	private void OnChildDrop(GameObject go, GameObject dragged)
	{
		if (base.enabled && onDrop != null)
		{
			onDrop(base.gameObject, dragged);
		}
	}

	private void OnChildDrag(GameObject go, Vector2 v)
	{
		if (base.enabled && onDrag != null)
		{
			onDrag(base.gameObject, v);
		}
	}

	private void OnChildClick(GameObject go)
	{
		if (base.enabled)
		{
			NotifyChildren(go, "OnClick", null);
			if (onClick != null)
			{
				onClick(base.gameObject);
			}
			base.gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnChildRightClick(GameObject go)
	{
		if (base.enabled)
		{
			NotifyChildren(go, "OnRightClick", null);
			if (onRightClick != null)
			{
				onRightClick(base.gameObject);
			}
			base.gameObject.SendMessage("OnRightClick", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnChildHover(GameObject go, bool over)
	{
		if (base.enabled)
		{
			if (over && !m_Hovered && (bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(HoverSound);
			}
			m_Hovered = over;
			NotifyChildren(go, "OnHover", over);
			if (onHover != null)
			{
				onHover(base.gameObject, over);
			}
			base.gameObject.SendMessage("OnHover", over, SendMessageOptions.DontRequireReceiver);
			UpdatePressed();
		}
	}

	private void OnChildPress(GameObject go, bool down)
	{
		if (!base.enabled)
		{
			return;
		}
		InitConsumableCheck();
		if ((bool)GlobalAudioPlayer.Instance)
		{
			if ((bool)m_abilityBarButtonConsumable)
			{
				if (down)
				{
					if (m_abilityBarButton == null || !m_abilityBarButton.Disabled)
					{
						GlobalAudioPlayer.Instance.Play(m_abilityBarButtonConsumable, GlobalAudioPlayer.UIInventoryAction.UseItem);
					}
					else
					{
						GlobalAudioPlayer.Instance.Play(ButtonDownSound, preventOverlap: false);
					}
				}
				else
				{
					GlobalAudioPlayer.Instance.Play(ButtonUpSound, preventOverlap: false);
				}
			}
			else if (down)
			{
				GlobalAudioPlayer.Instance.Play(ButtonDownSound, preventOverlap: false);
			}
			else
			{
				GlobalAudioPlayer.Instance.Play(ButtonUpSound, preventOverlap: false);
			}
		}
		m_Pressed = down;
		NotifyChildren(go, "OnPress", down);
		if (onPress != null)
		{
			onPress(base.gameObject, down);
		}
		base.gameObject.SendMessage("OnPress", down, SendMessageOptions.DontRequireReceiver);
		UpdatePressed();
	}

	private void OnChildTooltip(GameObject go, bool over)
	{
		if (base.enabled)
		{
			NotifyChildren(go, "OnTooltip", over);
			if (onTooltip != null)
			{
				onTooltip(base.gameObject, over);
			}
			base.gameObject.SendMessage("OnTooltip", over, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void NotifyChildren(GameObject except, string fn, object param)
	{
		if (m_IsNotifying || !base.enabled)
		{
			return;
		}
		m_IsNotifying = true;
		FindChildren();
		foreach (UIIsButton containedButton in m_ContainedButtons)
		{
			if (containedButton.gameObject != except)
			{
				UICamera.Notify(containedButton.gameObject, fn, param);
			}
		}
		m_IsNotifying = false;
	}

	public void ForceDown(bool state)
	{
		m_ForcedDown = state;
		UpdatePressed();
	}

	public void ForceHighlight(bool state)
	{
		FindChildren();
		m_ForcedHighlight = state;
		UpdatePressed();
	}

	public void SetText(string text)
	{
		Label.text = text;
	}

	private void UpdatePressed()
	{
		if (Label != null)
		{
			UIImageButtonRevised component = Label.GetComponent<UIImageButtonRevised>();
			if (!component || component.RepositionSprite)
			{
				if (m_Pressed || m_ForcedDown)
				{
					Label.transform.localPosition = m_LabelPosInitial + ClickLabelOffset;
				}
				else
				{
					Label.transform.localPosition = m_LabelPosInitial;
				}
			}
		}
		FindChildren();
		for (int i = 0; i < m_ContainedButtons.Count; i++)
		{
			UIIsButton uIIsButton = m_ContainedButtons[i];
			if (uIIsButton is UIImageButtonRevised)
			{
				UIImageButtonRevised obj = uIIsButton as UIImageButtonRevised;
				obj.SetOverridePressed(m_Pressed || m_ForcedDown);
				obj.SetOverrideHighlighted(m_Hovered || m_ForcedHighlight);
			}
		}
	}
}
