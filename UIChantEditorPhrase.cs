using System;
using UnityEngine;

public class UIChantEditorPhrase : MonoBehaviour
{
	public UITexture Icon;

	public UILabel Label;

	public UISprite Frame;

	public UIWidget RecitationBg;

	public UIWidget LingerBg;

	public UIWidget BarBg;

	private float m_SubsequentWidth;

	private Phrase m_Phrase;

	public int PixelsPerSecond = 26;

	private bool m_Disabled;

	public float SubsequentWidth
	{
		get
		{
			return m_SubsequentWidth;
		}
		set
		{
			m_SubsequentWidth = value;
			BarBg.transform.localScale = new Vector3(m_SubsequentWidth + Width - Frame.transform.localScale.x + 2f, BarBg.transform.localScale.y, BarBg.transform.localScale.z);
		}
	}

	public Phrase Phrase => m_Phrase;

	public float Width
	{
		get
		{
			if ((bool)m_Phrase)
			{
				return m_Phrase.Recitation * (float)PixelsPerSecond;
			}
			return 0f;
		}
	}

	public float LingerWidth
	{
		get
		{
			if ((bool)m_Phrase)
			{
				return m_Phrase.CalculateLinger(UIChantEditor.Instance.SelectedCharacter) * (float)PixelsPerSecond;
			}
			return 0f;
		}
	}

	public float EntireWidth
	{
		get
		{
			if ((bool)m_Phrase)
			{
				return (m_Phrase.Recitation + m_Phrase.CalculateLinger(UIChantEditor.Instance.SelectedCharacter)) * (float)PixelsPerSecond;
			}
			return 0f;
		}
	}

	public bool Disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled = value;
			if ((bool)Label)
			{
				if (m_Disabled)
				{
					Label.color = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.DARKDISABLED);
				}
				else
				{
					Label.color = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.DARK);
				}
			}
			if ((bool)Icon && (bool)Icon.material)
			{
				if (m_Disabled)
				{
					Icon.material.SetFloat("_Saturation", 0.2f);
				}
				else
				{
					Icon.material.SetFloat("_Saturation", 1f);
				}
			}
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Icon.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(Icon.gameObject);
		uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
		UIEventListener uIEventListener3 = UIEventListener.Get(Icon.gameObject);
		uIEventListener3.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		UIImageButtonRevised[] componentsInChildren = GetComponentsInChildren<UIImageButtonRevised>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IsInspectable = true;
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnChildClick(GameObject sender)
	{
		if (InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.Inspect)
		{
			UIItemInspectManager.Examine(m_Phrase, UIChantEditor.Instance.SelectedCharacter.gameObject);
		}
	}

	private void OnChildRightClick(GameObject sender)
	{
		UIItemInspectManager.Examine(m_Phrase, UIChantEditor.Instance.SelectedCharacter.gameObject);
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		if (over)
		{
			UIAbilityTooltip.GlobalShow(Icon, m_Phrase);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	public void SetPhrase(Phrase phrase)
	{
		if (phrase == null)
		{
			Hide();
			return;
		}
		m_Phrase = phrase;
		base.gameObject.name = phrase.name;
		if ((bool)Icon)
		{
			Icon.mainTexture = phrase.Icon;
		}
		if ((bool)Label)
		{
			Label.text = phrase.DisplayName.GetText() + " (" + Ordinal.Get(phrase.Level) + ")";
		}
		if ((bool)RecitationBg)
		{
			RecitationBg.transform.localScale = new Vector3(m_Phrase.Recitation * (float)PixelsPerSecond - Frame.transform.localScale.x, RecitationBg.transform.localScale.y, RecitationBg.transform.localScale.z);
		}
		if ((bool)LingerBg)
		{
			LingerBg.transform.localScale = new Vector3(m_Phrase.CalculateLinger(UIChantEditor.Instance.SelectedCharacter) * (float)PixelsPerSecond, LingerBg.transform.localScale.y, LingerBg.transform.localScale.z);
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
		if ((bool)Frame)
		{
			Frame.alpha = (val ? 1f : 0.5f);
		}
	}
}
