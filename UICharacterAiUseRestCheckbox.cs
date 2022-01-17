using System;
using UnityEngine;

[RequireComponent(typeof(UICheckbox))]
public class UICharacterAiUseRestCheckbox : UIParentSelectorListener
{
	[Tooltip("Control whether this checkbox should propagate changes back to the AI itself.")]
	public bool Set = true;

	private PartyMemberAI m_SelectedAi;

	private UICheckbox m_Checkbox;

	public bool Setting => m_Checkbox.isChecked;

	private void Awake()
	{
		m_Checkbox = GetComponent<UICheckbox>();
		UICheckbox checkbox = m_Checkbox;
		checkbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(checkbox.onStateChange, new UICheckbox.OnStateChange(OnCheckStateChange));
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if (!stats)
		{
			m_SelectedAi = null;
			return;
		}
		m_SelectedAi = stats.GetComponent<PartyMemberAI>();
		m_Checkbox.SetNoCallback(m_SelectedAi.UsePerRestAbilitiesInInstructionSet);
	}

	private void OnCheckStateChange(GameObject sender, bool state)
	{
		if (Set && (bool)m_SelectedAi)
		{
			m_SelectedAi.UsePerRestAbilitiesInInstructionSet = state;
		}
	}
}
