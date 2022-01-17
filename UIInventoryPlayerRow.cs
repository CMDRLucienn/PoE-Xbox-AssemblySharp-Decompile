using System;
using UnityEngine;

public class UIInventoryPlayerRow : UIParentSelectorListener, ISelectACharacter
{
	private PartyMemberAI m_PartyMember;

	private Inventory m_PartyMemberInventory;

	public UIInventoryItemGrid ItemGrid;

	public UIWidget CoinSelectedSprite;

	public UIWidget SelectedSprite;

	public UITexture Portrait;

	private UIImageButtonRevised m_PortraitMouseOver;

	public UISprite PortraitFrame;

	public const int SelectedDepthChange = 10;

	public int ManualHeight = 40;

	public const float HOVER_SELECTION_DELAY = 0.5f;

	private float m_WaitToSelectTimer;

	private bool m_Selected;

	public PartyMemberAI PartyMember => m_PartyMember;

	public CharacterStats SelectedCharacter { get; private set; }

	public bool Selected
	{
		get
		{
			return m_Selected;
		}
		set
		{
			if (m_Selected != value)
			{
				m_Selected = value;
				if ((bool)m_PortraitMouseOver)
				{
					m_PortraitMouseOver.SetOverrideHighlighted(value);
				}
			}
			RefreshSelected();
		}
	}

	public int Height => ManualHeight;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Awake()
	{
		m_PortraitMouseOver = Portrait.GetComponent<UIImageButtonRevised>();
	}

	protected override void Start()
	{
		base.Start();
		UIEventListener uIEventListener = UIEventListener.Get(Portrait.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnPortraitClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(Portrait.gameObject);
		uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnPortraitTooltip));
		UIEventListener uIEventListener3 = UIEventListener.Get(Portrait.gameObject);
		uIEventListener3.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onHover, new UIEventListener.BoolDelegate(OnPortraitHover));
	}

	private void Update()
	{
		if (m_WaitToSelectTimer > 0f)
		{
			m_WaitToSelectTimer -= Time.unscaledDeltaTime;
			if (m_WaitToSelectTimer <= 0f && UIGlobalInventory.Instance.RealDrag && UIGlobalInventory.Instance.DraggingItem)
			{
				m_WaitToSelectTimer = 0f;
				OnPortraitClick(base.gameObject);
			}
		}
	}

	public override void NotifySelectionChanged(CharacterStats selected)
	{
		Selected = selected == SelectedCharacter;
	}

	private void OnPortraitClick(GameObject go)
	{
		if (ParentSelector is ISelectACharacterMutable)
		{
			((ISelectACharacterMutable)ParentSelector).SelectedCharacter = SelectedCharacter;
		}
	}

	private void OnPortraitHover(GameObject sender, bool over)
	{
		if (over && UIGlobalInventory.Instance.RealDrag && UIGlobalInventory.Instance.DraggingItem)
		{
			m_WaitToSelectTimer = 0.5f;
		}
		else
		{
			m_WaitToSelectTimer = 0f;
		}
	}

	private void OnPortraitTooltip(GameObject sender, bool over)
	{
		if (over)
		{
			UIActionBarTooltip.GlobalShow(Portrait, CharacterStats.Name(m_PartyMember));
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	public void SetPartyMember(PartyMemberAI partymember)
	{
		m_PartyMember = partymember;
		SelectedCharacter = partymember.GetComponent<CharacterStats>();
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
		if (partymember != null)
		{
			m_PartyMemberInventory = partymember.Inventory;
			if (!m_PartyMemberInventory)
			{
				m_PartyMemberInventory = partymember.GetComponent<Inventory>();
			}
			if (m_PartyMemberInventory == null)
			{
				Debug.LogError("Party member '" + partymember.gameObject.name + "' has no inventory component.");
			}
			else
			{
				ItemGrid.LoadInventory(m_PartyMemberInventory);
			}
		}
		else
		{
			m_PartyMemberInventory = null;
		}
	}

	public void RefreshSelected()
	{
		if ((bool)SelectedSprite)
		{
			SelectedSprite.alpha = (m_Selected ? 1f : 0f);
		}
		if ((bool)CoinSelectedSprite)
		{
			CoinSelectedSprite.alpha = (m_Selected ? 1f : 0f);
		}
		if ((bool)m_PortraitMouseOver)
		{
			m_PortraitMouseOver.SetOverrideHighlighted(m_Selected);
		}
	}
}
