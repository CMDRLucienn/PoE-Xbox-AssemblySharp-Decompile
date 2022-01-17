using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class UIPartyMemberPortraitGetter : UIParentSelectorListener
{
	public bool LargePortrait = true;

	private Portrait m_CurrentPortrait;

	private UITexture m_Texture;

	private UIClippedTexture m_ClipComp;

	private void Awake()
	{
		m_Texture = GetComponent<UITexture>();
		m_ClipComp = GetComponent<UIClippedTexture>();
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if ((bool)m_CurrentPortrait)
		{
			m_CurrentPortrait.OnPortraitChanged -= OnPortraitChanged;
		}
		m_CurrentPortrait = (stats ? stats.GetComponent<Portrait>() : null);
		OnPortraitChanged();
		if ((bool)m_CurrentPortrait)
		{
			m_CurrentPortrait.OnPortraitChanged += OnPortraitChanged;
		}
	}

	private void OnPortraitChanged()
	{
		if ((bool)m_CurrentPortrait)
		{
			m_Texture.mainTexture = (LargePortrait ? m_CurrentPortrait.TextureLarge : m_CurrentPortrait.TextureSmall);
			if ((bool)m_ClipComp)
			{
				m_ClipComp.OnTextureChanged();
			}
		}
	}
}
