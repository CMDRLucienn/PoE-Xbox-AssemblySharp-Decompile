using System;
using UnityEngine;

public class UILootPartyIcon : MonoBehaviour, ISelectACharacter
{
	public GameObject PortraitContainer;

	public UISprite Background;

	public Collider Collider;

	public UIInventoryItemReciever Reciever;

	public UIMultiSpriteImageButton Button;

	private bool m_Selected;

	public PartyMemberAI PartyMember { get; private set; }

	public CharacterStats SelectedCharacter
	{
		get
		{
			if (!PartyMember)
			{
				return null;
			}
			return PartyMember.GetComponent<CharacterStats>();
		}
	}

	public int Width => (int)Collider.transform.localScale.x;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Start()
	{
		if ((bool)Collider)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Collider);
			uIEventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
		}
	}

	private void OnDoubleClick(GameObject sender)
	{
		if (UILootManager.Instance.WindowActive())
		{
			BaseInventory component = SelectedCharacter.GetComponent<Inventory>();
			if ((bool)component)
			{
				UILootManager.Instance.TakeAll(component);
			}
		}
	}

	public void Select(bool value)
	{
		m_Selected = value;
		Button.ForceHighlight(m_Selected);
		Background.spriteName = (m_Selected ? "sel_circle_selected" : "sel_circle");
		PortraitContainer.transform.localScale = (m_Selected ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one);
	}

	public void LoadPartyMember(PartyMemberAI pai)
	{
		PartyMember = pai;
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
		if (PartyMember == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		if ((bool)Reciever)
		{
			Reciever.Reciever = pai.Inventory;
		}
	}

	public void NotifyReload()
	{
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
	}
}
