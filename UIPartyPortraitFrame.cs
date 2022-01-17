public class UIPartyPortraitFrame : UIParentSelectorListener
{
	private UISprite m_Sprite;

	private PartyMemberAI m_SelectedAi;

	public TweenAlpha AlphaTween;

	public string SelectedSprite;

	public string NotSelectedSprite;

	private void Awake()
	{
		m_Sprite = GetComponent<UISprite>();
	}

	private void Update()
	{
		if (!m_SelectedAi)
		{
			AlphaTween.enabled = false;
			m_Sprite.alpha = 1f;
			m_Sprite.spriteName = NotSelectedSprite;
		}
		if (GameCursor.CharacterUnderCursor == m_SelectedAi.gameObject && GameCursor.OverrideCharacterUnderCursor != m_SelectedAi.gameObject)
		{
			if (!AlphaTween.enabled)
			{
				AlphaTween.Reset();
				m_Sprite.alpha = AlphaTween.from;
				AlphaTween.Play(forward: true);
				m_Sprite.spriteName = SelectedSprite;
			}
		}
		else
		{
			AlphaTween.enabled = false;
			m_Sprite.alpha = 1f;
			m_Sprite.spriteName = (m_SelectedAi.Selected ? SelectedSprite : NotSelectedSprite);
		}
	}

	public override void NotifySelectionChanged(CharacterStats selected)
	{
		m_SelectedAi = (selected ? selected.GetComponent<PartyMemberAI>() : null);
	}
}
