using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPartyManagementRoster : MonoBehaviour
{
	public UIPartyManagementIcon RootObject;

	private UIGrid m_Grid;

	public GameObject EmptyNotice;

	private List<UIPartyManagementIcon> m_Icons;

	private int m_Size;

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Icons == null)
		{
			m_Grid = GetComponent<UIGrid>();
			RootObject.gameObject.SetActive(value: false);
			m_Icons = new List<UIPartyManagementIcon>();
		}
	}

	public void Reload()
	{
		Init();
		m_Size = 0;
		foreach (StoredCharacterInfo companion in GameState.Stronghold.GetCompanions())
		{
			if ((bool)companion && !UIPartyManager.Instance.PendingToParty.Contains(companion.gameObject))
			{
				UIPartyManagementIcon icon = GetIcon(m_Size);
				icon.gameObject.SetActive(value: true);
				icon.SetPartyMember(companion.gameObject);
				m_Size++;
			}
		}
		for (int i = 0; i < UIPartyManager.Instance.PendingToBench.Count; i++)
		{
			GameObject go = UIPartyManager.Instance.PendingToBench[i];
			if ((bool)go && PartyMemberAI.OnlyPrimaryPartyMembers.Any((PartyMemberAI pai) => pai.gameObject == go))
			{
				UIPartyManagementIcon icon2 = GetIcon(m_Size);
				icon2.gameObject.SetActive(value: true);
				icon2.SetPartyMember(go);
				m_Size++;
			}
		}
		EmptyNotice.SetActive(m_Size == 0);
		for (int j = m_Size; j < m_Icons.Count; j++)
		{
			m_Icons[j].gameObject.SetActive(value: false);
		}
		m_Grid.repositionNow = true;
	}

	private UIPartyManagementIcon GetIcon(int index)
	{
		if (index < m_Icons.Count)
		{
			return m_Icons[index];
		}
		UIPartyManagementIcon component = NGUITools.AddChild(RootObject.transform.parent.gameObject, RootObject.gameObject).GetComponent<UIPartyManagementIcon>();
		m_Icons.Add(component);
		return component;
	}
}
