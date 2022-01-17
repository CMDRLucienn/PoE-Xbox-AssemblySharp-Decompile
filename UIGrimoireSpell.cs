using System;
using UnityEngine;

public class UIGrimoireSpell : UIParentSelectorListener
{
	public UITexture Icon;

	public UILabel Label;

	public UISprite Frame;

	public UIWidget AdditiveHighlight;

	public UISprite StatusIndicator;

	private UIIsButton m_StatusButton;

	private GenericSpell m_Spell;

	public UIMultiSpriteImageButton MainButton;

	private bool m_Disabled;

	private bool m_Initted;

	public GenericSpell Spell => m_Spell;

	public bool Disabled
	{
		get
		{
			if (!MainButton || MainButton.enabled)
			{
				return m_Disabled;
			}
			return true;
		}
		set
		{
			SetDisabled(value);
		}
	}

	protected override void Start()
	{
		base.Start();
		Init();
		MainButton = GetComponent<UIMultiSpriteImageButton>();
		if ((bool)MainButton)
		{
			UIMultiSpriteImageButton mainButton = MainButton;
			mainButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(mainButton.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			UIMultiSpriteImageButton mainButton2 = MainButton;
			mainButton2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(mainButton2.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		}
		else if ((bool)Icon)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Icon);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			UIEventListener uIEventListener2 = UIEventListener.Get(Icon);
			uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		}
		if ((bool)StatusIndicator)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(StatusIndicator);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnIndicatorClick));
			m_StatusButton = StatusIndicator.GetComponent<UIIsButton>();
		}
		UIImageButtonRevised[] componentsInChildren = GetComponentsInChildren<UIImageButtonRevised>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IsInspectable = true;
		}
	}

	private void Init()
	{
		if (!m_Initted)
		{
			m_Initted = true;
			FindParent();
			Icon.material = null;
			UIClippedTexture component = Icon.GetComponent<UIClippedTexture>();
			if ((bool)component)
			{
				component.OnTextureChanged();
			}
		}
	}

	private void OnIndicatorClick(GameObject sender)
	{
		UIItemInspectManager.QueryLearnSpell(ParentSelector.SelectedCharacter, Spell);
	}

	private void OnChildClick(GameObject sender)
	{
		if (InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.Inspect)
		{
			if (UIGrimoireManager.Instance.CanEditGrimoire)
			{
				UIItemInspectManager.ExamineLearn(m_Spell, UIGrimoireManager.Instance.SelectedCharacter.gameObject);
			}
			else
			{
				UIItemInspectManager.Examine(m_Spell, UIGrimoireManager.Instance.SelectedCharacter.gameObject);
			}
		}
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		if (over)
		{
			UIAbilityTooltip.GlobalShow(Icon, m_Spell);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	public void SetSpell(GenericSpell spell)
	{
		Init();
		if (spell == null)
		{
			Hide();
			return;
		}
		if ((bool)m_StatusButton)
		{
			m_StatusButton.enabled = UIGrimoireManager.Instance.CanEditGrimoire;
		}
		if ((bool)StatusIndicator)
		{
			if (ParentSelector != null && (bool)ParentSelector.SelectedCharacter && ParentSelector.SelectedCharacter.CharacterClass == CharacterStats.Class.Wizard)
			{
				StatusIndicator.gameObject.SetActive(!ParentSelector.SelectedCharacter.FindAbilityInstance(spell));
			}
			else
			{
				StatusIndicator.gameObject.SetActive(value: false);
			}
		}
		m_Spell = spell;
		base.gameObject.name = spell.name;
		if ((bool)Icon)
		{
			Icon.mainTexture = spell.Icon;
		}
		if ((bool)Label)
		{
			Label.text = GenericAbility.Name(spell);
		}
	}

	public void Show()
	{
		SetVisibility(val: true);
	}

	public void Hide()
	{
		SetVisibility(val: false);
	}

	public void SetVisibility(bool val)
	{
		if ((bool)StatusIndicator && !val)
		{
			StatusIndicator.gameObject.SetActive(value: false);
		}
		if ((bool)Icon)
		{
			Icon.gameObject.SetActive(val);
		}
		if ((bool)Label)
		{
			Label.gameObject.SetActive(val);
		}
	}

	public void SetSelected(bool val)
	{
	}

	public void SetHighlighted(bool val)
	{
		if ((bool)AdditiveHighlight)
		{
			AdditiveHighlight.alpha = (val ? 1 : 0);
			if ((bool)Frame)
			{
				Frame.alpha = 0.5f * (1f - AdditiveHighlight.alpha);
			}
		}
	}

	public void SetDisabled(bool val)
	{
		m_Disabled = val;
		if (val)
		{
			if ((bool)Icon)
			{
				Icon.material.SetFloat("_Saturation", 0.2f);
			}
			if ((bool)Label)
			{
				Label.color = new Color(0.3f, 0.3f, 0.3f);
			}
		}
		else
		{
			if ((bool)Icon)
			{
				Icon.material.SetFloat("_Saturation", 1f);
			}
			if ((bool)Label)
			{
				Label.color = Color.black;
			}
		}
	}
}
