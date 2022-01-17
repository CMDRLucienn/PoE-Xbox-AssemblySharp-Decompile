using System.Collections.Generic;
using UnityEngine;

public class UIStoreRespecPage : MonoBehaviour
{
	public UIStoreRespecRow RootRow;

	private List<UIStoreRespecRow> m_Rows;

	public UIGrid RowGrid;

	public GameObject NoneAvailable;

	public GameObject Backgrounds;

	public static int CostOf(int level)
	{
		return level * EconomyManager.Instance.RespecCostMultiplier;
	}

	private void OnEnable()
	{
		Backgrounds.SetActive(value: true);
		RefreshItems();
	}

	private void OnDisable()
	{
		Backgrounds.SetActive(value: false);
	}

	private void Start()
	{
		RefreshItems();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void RefreshItems()
	{
		InitRows();
		int num = 1;
		int num2 = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if ((bool)partyMemberAI && PartyMemberAI.IsInPartyList(partyMemberAI))
			{
				UIStoreRespecRow uIStoreRespecRow;
				if (num2 >= m_Rows.Count)
				{
					uIStoreRespecRow = NGUITools.AddChild(RootRow.transform.parent.gameObject, RootRow.gameObject).GetComponent<UIStoreRespecRow>();
					uIStoreRespecRow.name = num.ToString("000") + ".Respec";
					m_Rows.Add(uIStoreRespecRow);
				}
				else
				{
					uIStoreRespecRow = m_Rows[num2];
				}
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				uIStoreRespecRow.Set(component);
				uIStoreRespecRow.gameObject.SetActive(value: true);
				num++;
				num2++;
				if (component.Level == 1 && component.GetComponent<CompanionInstanceID>() != null)
				{
					uIStoreRespecRow.gameObject.SetActive(value: false);
				}
			}
		}
		for (int j = num2; j < m_Rows.Count; j++)
		{
			m_Rows[j].gameObject.SetActive(value: false);
		}
		RowGrid.Reposition();
	}

	private void InitRows()
	{
		if (m_Rows == null)
		{
			m_Rows = new List<UIStoreRespecRow>();
			RootRow.gameObject.SetActive(value: false);
		}
	}
}
