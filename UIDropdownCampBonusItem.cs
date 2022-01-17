using System;
using UnityEngine;

public class UIDropdownCampBonusItem : UIDropdownItem
{
	[Tooltip("Widget to anchor this item's tooltip to.")]
	public UIWidget TooltipAnchor;

	private UIMultiSpriteImageButton m_Button;

	private void Awake()
	{
		m_Button = GetComponent<UIMultiSpriteImageButton>();
		UIMultiSpriteImageButton button = m_Button;
		button.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(button.onTooltip, new UIEventListener.BoolDelegate(OnButtonTooltip));
	}

	public override void NotifyContentChanged()
	{
		if (m_Content is Affliction)
		{
			int index = AfflictionData.Instance.SurvivalCampEffects.IndexOf((Affliction)m_Content);
			bool active = !AfflictionData.Instance.SurvivalCampEffects.HasSubBonuses(index);
			m_Button.Collider.SetActive(active);
		}
		if (m_Content is CampEffectSubBonus)
		{
			m_Button.Label.indentFirst = int.MaxValue;
			m_Button.Label.indentAmount = 20;
		}
		else
		{
			m_Button.Label.indentAmount = 0;
		}
	}

	private void OnButtonTooltip(GameObject sender, bool state)
	{
		if (state && m_Content is Affliction)
		{
			UIAbilityTooltip.GlobalShow(TooltipAnchor, (Affliction)m_Content);
		}
		else if (state && m_Content is CampEffectSubBonus)
		{
			UIAbilityTooltip.GlobalShow(TooltipAnchor, ((CampEffectSubBonus)m_Content).Affliction);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}
}
