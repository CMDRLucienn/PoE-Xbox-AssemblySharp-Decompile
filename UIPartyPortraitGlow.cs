public class UIPartyPortraitGlow : UIParentSelectorListener
{
	private UIWidget m_Widget;

	private PartyMemberAI m_SelectedAi;

	public TweenAlpha AlphaTween;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		if (!m_SelectedAi)
		{
			AlphaTween.enabled = false;
			m_Widget.alpha = 0f;
		}
		else if (GameCursor.CharacterUnderCursor == m_SelectedAi.gameObject && GameCursor.OverrideCharacterUnderCursor != m_SelectedAi.gameObject)
		{
			if (!AlphaTween.enabled)
			{
				AlphaTween.Reset();
				m_Widget.alpha = AlphaTween.from;
				AlphaTween.Play(forward: true);
			}
		}
		else
		{
			AlphaTween.enabled = false;
			m_Widget.alpha = (m_SelectedAi.Selected ? 1f : 0f);
		}
	}

	public override void NotifySelectionChanged(CharacterStats selected)
	{
		m_SelectedAi = (selected ? selected.GetComponent<PartyMemberAI>() : null);
	}
}
