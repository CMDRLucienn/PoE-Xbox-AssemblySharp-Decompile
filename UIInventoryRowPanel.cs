using UnityEngine;

public class UIInventoryRowPanel : UIParentSelectorListener
{
	private UIInventoryPlayerRow[] m_Rows;

	public UIInventoryPlayerRow RootRow;

	public UIGrid RowGrid;

	public UIDraggablePanel DragPanel;

	public UISprite SelectShadow;

	public string[] BackgroundSprites;

	protected override void Start()
	{
		base.Start();
		Init();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Init()
	{
		if (m_Rows != null)
		{
			return;
		}
		if (!DragPanel)
		{
			DragPanel = GetComponent<UIDraggablePanel>();
		}
		if ((bool)DragPanel && !DragPanel.enabled)
		{
			DragPanel = null;
		}
		if ((bool)DragPanel)
		{
			DragPanel.ResetPosition();
		}
		m_Rows = new UIInventoryPlayerRow[6];
		m_Rows[0] = RootRow;
		for (int i = 0; i < m_Rows.Length; i++)
		{
			if (m_Rows[i] == null)
			{
				m_Rows[i] = NGUITools.AddChild(RootRow.transform.parent.gameObject, RootRow.gameObject).GetComponent<UIInventoryPlayerRow>();
			}
			m_Rows[i].gameObject.SetActive(value: true);
		}
		UIInventoryPlayerRow[] rows = m_Rows;
		for (int j = 0; j < rows.Length; j++)
		{
			rows[j].ItemGrid.Allocate();
		}
		RowGrid.Reposition();
		if ((bool)DragPanel)
		{
			DragPanel.ResetPosition();
		}
	}

	public void ReloadGrids()
	{
		UIInventoryPlayerRow[] rows = m_Rows;
		for (int i = 0; i < rows.Length; i++)
		{
			rows[i].ItemGrid.Reload();
		}
	}

	public void ReloadParty()
	{
		Init();
		if ((bool)DragPanel)
		{
			DragPanel.ResetPosition();
		}
		int i = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			m_Rows[i].SetPartyMember(onlyPrimaryPartyMember);
			m_Rows[i].gameObject.SetActive(value: true);
			i++;
		}
		for (; i < m_Rows.Length; i++)
		{
			m_Rows[i].gameObject.SetActive(value: false);
		}
		RowGrid.Reposition();
		if (ParentSelector != null)
		{
			NotifySelectionChanged(ParentSelector.SelectedCharacter);
		}
		if ((bool)DragPanel)
		{
			DragPanel.ResetPosition();
		}
	}

	public UIInventoryPlayerRow RowForSelectedPartyMember()
	{
		if (!ParentSelector.SelectedCharacter)
		{
			return null;
		}
		return RowForPartyMember(ParentSelector.SelectedCharacter.GetComponent<PartyMemberAI>());
	}

	public UIInventoryPlayerRow RowForPartyMember(PartyMemberAI partyMember)
	{
		for (int i = 0; i < m_Rows.Length; i++)
		{
			if (m_Rows[i].PartyMember == partyMember)
			{
				return m_Rows[i];
			}
		}
		return null;
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		Init();
		if (stats == null || m_Rows == null)
		{
			return;
		}
		PartyMemberAI component = stats.GetComponent<PartyMemberAI>();
		if (component == null || !component.gameObject.activeInHierarchy)
		{
			return;
		}
		for (int i = 0; i < m_Rows.Length; i++)
		{
			UIInventoryPlayerRow uIInventoryPlayerRow = m_Rows[i];
			if (uIInventoryPlayerRow == null)
			{
				continue;
			}
			if (uIInventoryPlayerRow.PartyMember == component)
			{
				if ((bool)SelectShadow)
				{
					SelectShadow.alpha = 1f;
					SelectShadow.transform.localPosition = new Vector3(SelectShadow.transform.localPosition.x, uIInventoryPlayerRow.transform.localPosition.y - (float)uIInventoryPlayerRow.Height, SelectShadow.transform.localPosition.z);
				}
				uIInventoryPlayerRow.Selected = true;
			}
			else
			{
				uIInventoryPlayerRow.Selected = false;
			}
		}
	}
}
