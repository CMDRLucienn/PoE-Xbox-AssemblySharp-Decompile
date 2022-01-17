using System;
using UnityEngine;

public class UICharacterCustomizeLauncher : UIParentSelectorListener
{
	public UITexture TexturePortrait;

	public UISprite SpriteCustomizeIcon;

	public UISprite SpriteCustomAiIcon;

	public UIPanel Panel;

	private UIPanel m_ParentPanel;

	private bool m_CanCustomize;

	private bool m_AiHovered;

	private bool m_CustomizeHovered;

	private bool m_PortraitHovered;

	private void Awake()
	{
		m_ParentPanel = GetComponentInParent<UIPanel>();
		if ((bool)TexturePortrait)
		{
			UIEventListener uIEventListener = UIEventListener.Get(TexturePortrait);
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHoverPortrait));
			TexturePortrait.color = Color.black;
		}
		if ((bool)SpriteCustomizeIcon)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(SpriteCustomizeIcon);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnCustomize));
			UIEventListener uIEventListener3 = UIEventListener.Get(SpriteCustomizeIcon);
			uIEventListener3.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onHover, new UIEventListener.BoolDelegate(OnHoverCustomize));
		}
		if ((bool)SpriteCustomAiIcon)
		{
			UIEventListener uIEventListener4 = UIEventListener.Get(SpriteCustomAiIcon);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnCustomAi));
			UIEventListener uIEventListener5 = UIEventListener.Get(SpriteCustomAiIcon);
			uIEventListener5.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener5.onHover, new UIEventListener.BoolDelegate(OnHoverAi));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		bool flag = m_AiHovered || m_CustomizeHovered || m_PortraitHovered;
		_ = (bool)TexturePortrait;
		if ((bool)Panel)
		{
			Panel.alpha = (flag ? 1f : 0.5f) * (m_ParentPanel ? m_ParentPanel.alpha : 1f);
		}
		if ((bool)SpriteCustomizeIcon)
		{
			SpriteCustomizeIcon.gameObject.SetActive(m_CanCustomize);
		}
	}

	public override void NotifySelectionChanged(CharacterStats newSelection)
	{
		CompanionInstanceID component = newSelection.GetComponent<CompanionInstanceID>();
		m_CanCustomize = component == null;
		if ((bool)SpriteCustomizeIcon)
		{
			TweenAlpha component2 = SpriteCustomizeIcon.GetComponent<TweenAlpha>();
			if ((bool)component2)
			{
				component2.enabled = false;
			}
			SpriteCustomizeIcon.alpha = 0f;
		}
		if ((bool)TexturePortrait)
		{
			TweenColor component3 = SpriteCustomizeIcon.GetComponent<TweenColor>();
			if ((bool)component3)
			{
				component3.enabled = false;
			}
			TexturePortrait.color = Color.black;
		}
	}

	private void OnHoverAi(GameObject go, bool hover)
	{
		m_AiHovered = hover;
	}

	private void OnHoverCustomize(GameObject go, bool hover)
	{
		m_CustomizeHovered = hover;
	}

	private void OnHoverPortrait(GameObject go, bool hover)
	{
		m_PortraitHovered = hover;
	}

	public void OnCustomize(GameObject go)
	{
		if (m_CanCustomize && ParentSelector != null && ParentSelector.SelectedCharacter != null)
		{
			UICharacterCustomizeManager.Instance.LoadCharacter(ParentSelector.SelectedCharacter);
			UICharacterCustomizeManager.Instance.ShowWindow();
		}
	}

	public void OnCustomAi(GameObject go)
	{
		if (ParentSelector != null && ParentSelector.SelectedCharacter != null)
		{
			UIAiCustomizerManager.Instance.SelectedCharacter = ParentSelector.SelectedCharacter;
			UIAiCustomizerManager.Instance.ShowWindow();
		}
	}
}
