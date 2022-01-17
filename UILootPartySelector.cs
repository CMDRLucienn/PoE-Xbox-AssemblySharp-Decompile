using System;
using UnityEngine;

public class UILootPartySelector : UIParentSelectorListener
{
	public UIGrid PartyGrid;

	public GameObject RootPartyMember;

	public GameObject StashButton;

	private UILootPartyIcon[] m_PartyIcons;

	public event UIEventListener.VoidDelegate OnSelectStash;

	public event UIEventListener.VoidDelegate OnSelectCharacter;

	protected override void Start()
	{
		base.Start();
		if ((bool)StashButton)
		{
			UIEventListener uIEventListener = UIEventListener.Get(StashButton);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnStash));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnStash(GameObject sender)
	{
		if (this.OnSelectStash != null)
		{
			this.OnSelectStash(sender);
		}
	}

	private void OnPartyMember(GameObject sender)
	{
		if (this.OnSelectCharacter != null)
		{
			this.OnSelectCharacter(sender);
		}
	}

	public void ReloadParty()
	{
		for (int i = 0; i < m_PartyIcons.Length; i++)
		{
			m_PartyIcons[i].NotifyReload();
		}
	}

	public void LoadParty()
	{
		Init();
		int i = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			m_PartyIcons[i].LoadPartyMember(onlyPrimaryPartyMember);
			i++;
		}
		for (; i < m_PartyIcons.Length; i++)
		{
			m_PartyIcons[i].LoadPartyMember(null);
		}
		PartyGrid.Reposition();
	}

	private void Init()
	{
		if (m_PartyIcons == null)
		{
			m_PartyIcons = new UILootPartyIcon[6];
			m_PartyIcons[0] = RootPartyMember.GetComponent<UILootPartyIcon>();
			RootPartyMember.SetActive(value: true);
			for (int i = 1; i < 6; i++)
			{
				GameObject gameObject = NGUITools.AddChild(RootPartyMember.transform.parent.gameObject, RootPartyMember);
				gameObject.SetActive(value: true);
				m_PartyIcons[i] = gameObject.GetComponent<UILootPartyIcon>();
			}
			UILootPartyIcon[] partyIcons = m_PartyIcons;
			for (int j = 0; j < partyIcons.Length; j++)
			{
				UIEventListener uIEventListener = UIEventListener.Get(partyIcons[j].Collider.gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnPartyMember));
			}
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		UpdateSelection();
	}

	public void UpdateSelection()
	{
		LoadParty();
		UILootManager uILootManager = ParentSelector as UILootManager;
		if ((bool)uILootManager && uILootManager.SelectedStash)
		{
			UILootPartyIcon[] partyIcons = m_PartyIcons;
			for (int i = 0; i < partyIcons.Length; i++)
			{
				partyIcons[i].Select(value: false);
			}
			if ((bool)StashButton)
			{
				StashButton.GetComponent<UIImageButtonRevised>().SetOverrideHighlighted(state: true);
			}
		}
		else if (ParentSelector != null && (bool)ParentSelector.SelectedCharacter)
		{
			UILootPartyIcon[] partyIcons = m_PartyIcons;
			foreach (UILootPartyIcon obj in partyIcons)
			{
				obj.Select(obj.SelectedCharacter == ParentSelector.SelectedCharacter);
			}
			if ((bool)StashButton)
			{
				StashButton.GetComponent<UIImageButtonRevised>().SetOverrideHighlighted(state: false);
			}
		}
	}
}
