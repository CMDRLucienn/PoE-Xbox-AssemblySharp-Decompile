using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIDropdownMenu))]
public class UIDropdownCampBonus : UIParentSelectorListener
{
	private UIDropdownMenu m_dropdown;

	private CharacterStats m_loadedStats;

	private List<object> m_OptionBuilder = new List<object>();

	private void Awake()
	{
		m_dropdown = GetComponent<UIDropdownMenu>();
		m_dropdown.OnDropdownOptionChanged += OnDropdownChanged;
	}

	public override void NotifySelectionChanged(CharacterStats character)
	{
		m_loadedStats = character;
		if (!m_loadedStats)
		{
			return;
		}
		int num = m_loadedStats.CalculateSkill(CharacterStats.SkillType.Survival);
		m_OptionBuilder.Clear();
		for (int i = 0; i < AfflictionData.Instance.SurvivalCampEffects.CyclePeriod; i++)
		{
			m_OptionBuilder.Add(AfflictionData.Instance.SurvivalCampEffects.GetBestBonusByIndex(i, -1, num));
			m_dropdown.SetOptionEnabled(m_OptionBuilder.Count - 1, AfflictionData.Instance.SurvivalCampEffects.IsBonusValid(i, -1, num));
			if (!AfflictionData.Instance.SurvivalCampEffects.HasSubBonuses(i))
			{
				continue;
			}
			CampEffectSubBonus[] subBonuses = AfflictionData.Instance.SurvivalCampEffects.GetSubBonuses(AfflictionData.Instance.SurvivalCampEffects.GetMaximumIndex(i, num));
			if (subBonuses != null)
			{
				for (int j = 0; j < subBonuses.Length; j++)
				{
					m_OptionBuilder.Add(subBonuses[j]);
					m_dropdown.SetOptionEnabled(m_OptionBuilder.Count - 1, AfflictionData.Instance.SurvivalCampEffects.IsBonusValid(i, j, num));
				}
			}
		}
		m_dropdown.Options = m_OptionBuilder.ToArray();
		m_OptionBuilder.Clear();
		if (m_loadedStats.LastSelectedSurvivalBonus < 0 && num > 0)
		{
			m_loadedStats.LastSelectedSurvivalBonus = 0;
		}
		m_dropdown.SelectedItem = AfflictionData.Instance.SurvivalCampEffects.GetBestBonusByIndex(m_loadedStats.LastSelectedSurvivalBonus, m_loadedStats.LastSelectedSurvivalSubBonus, num);
		if (m_dropdown.SelectedItem == null)
		{
			m_loadedStats.LastSelectedSurvivalBonus = -1;
			m_loadedStats.LastSelectedSurvivalSubBonus = -1;
		}
	}

	private void OnDropdownChanged(object selected)
	{
		if ((bool)m_loadedStats)
		{
			Affliction affliction = selected as Affliction;
			if (affliction == null)
			{
				affliction = ((CampEffectSubBonus)selected).Affliction;
			}
			m_loadedStats.LastSelectedSurvivalBonus = AfflictionData.Instance.SurvivalCampEffects.IndexOf(affliction);
			m_loadedStats.LastSelectedSurvivalSubBonus = AfflictionData.Instance.SurvivalCampEffects.SubIndexOf(affliction);
		}
	}
}
