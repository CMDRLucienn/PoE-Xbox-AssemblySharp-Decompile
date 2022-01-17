using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStoreRecruitPage : MonoBehaviour
{
	public UIStoreRecruitRow RootRow;

	private List<UIStoreRecruitRow> m_Rows;

	public UIGrid RowGrid;

	public GameObject NoneAvailable;

	public GameObject Backgrounds;

	public static int CostOf(int level)
	{
		return level * EconomyManager.Instance.AdventurerCostMultiplier;
	}

	private void OnEnable()
	{
		HideUnavailable();
		Backgrounds.SetActive(value: true);
	}

	private void OnDisable()
	{
		Backgrounds.SetActive(value: false);
	}

	private void Start()
	{
		InitRows();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void InitRows()
	{
		if (m_Rows == null)
		{
			m_Rows = new List<UIStoreRecruitRow>();
			RootRow.Set(1);
			RootRow.name = "001.Recruit";
			m_Rows.Add(RootRow);
			for (int i = 2; i <= CharacterStats.PlayerLevelCap; i++)
			{
				UIStoreRecruitRow component = NGUITools.AddChild(RootRow.transform.parent.gameObject, RootRow.gameObject).GetComponent<UIStoreRecruitRow>();
				component.name = i.ToString("000") + ".Recruit";
				component.Set(i);
				m_Rows.Add(component);
			}
			HideUnavailable();
			RowGrid.Reposition();
		}
	}

	private void HideUnavailable()
	{
		InitRows();
		int num = 1;
		if ((bool)GameState.s_playerCharacter)
		{
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				num = component.ScaledLevel;
			}
		}
		foreach (UIStoreRecruitRow row in m_Rows)
		{
			row.HideIfAbove(num - 1);
		}
		NoneAvailable.SetActive(!m_Rows.Where((UIStoreRecruitRow rc) => rc.gameObject.activeSelf).Any());
		RowGrid.Reposition();
	}
}
