using UnityEngine;

public class UICharacterHideIfPartyMember : UIParentSelectorListener
{
	[Tooltip("If set, hide if not a party member.")]
	public bool Invert;

	private UIWidget m_Widget;

	private UIPanel m_Panel;

	private float m_Alpha = 1f;

	private PartyMemberAI m_PartyAI;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Panel = GetComponent<UIPanel>();
		m_Alpha = (m_Widget ? m_Widget.alpha : (m_Panel ? m_Panel.alpha : 1f));
	}

	private void Update()
	{
		bool flag = (bool)m_PartyAI && m_PartyAI.IsActiveInParty;
		if ((bool)m_Widget)
		{
			m_Widget.alpha = ((flag ^ Invert) ? 0f : m_Alpha);
		}
		if ((bool)m_Panel)
		{
			m_Panel.alpha = ((flag ^ Invert) ? 0f : m_Alpha);
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_PartyAI = (stats ? stats.GetComponent<PartyMemberAI>() : null);
	}
}
