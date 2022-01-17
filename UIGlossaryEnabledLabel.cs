using System;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIGlossaryEnabledLabel : MonoBehaviour
{
	public enum LinkBehavior
	{
		ShowTooltip,
		OpenToPage
	}

	[Tooltip("Whether or not this label needs to be responsible for adding glossary tags when the text changes.")]
	public bool AddUrlTags;

	public bool ColorizeLinks = true;

	public UIGlobalColor.LinkStyle LinkColorSet = UIGlobalColor.LinkStyle.PARCHMENT;

	public LinkBehavior Behavior;

	private string m_previousText;

	private UICamera m_UiCamera;

	private UILabel m_Label;

	private bool m_Hovered;

	private int m_HoveredUrlIndex = -1;

	public GameObject ExtraCollider;

	private void Awake()
	{
		m_UiCamera = UICamera.FindCameraForLayer(base.gameObject.layer);
		m_Label = GetComponent<UILabel>();
		m_Label.OnChanged += OnLabelChanged;
		if (!string.IsNullOrEmpty(m_Label.text))
		{
			OnLabelChanged(m_Label.text);
		}
		if ((bool)GetComponent<Collider>())
		{
			UIEventListener uIEventListener = UIEventListener.Get(GetComponent<Collider>());
			if (uIEventListener != null)
			{
				uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnColliderHover));
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnColliderClick));
				uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnColliderTooltip));
			}
		}
		if ((bool)ExtraCollider)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(ExtraCollider);
			if (uIEventListener2 != null)
			{
				uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnColliderHover));
				uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnColliderClick));
				uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnColliderTooltip));
			}
		}
	}

	private void Update()
	{
		if (!m_Hovered)
		{
			return;
		}
		Vector2 relativeMouseCoords = GetRelativeMouseCoords();
		int characterIndexAtPosition = m_Label.GetCharacterIndexAtPosition(relativeMouseCoords);
		int urlIndexAtCharacterIndex = m_Label.GetUrlIndexAtCharacterIndex(characterIndexAtPosition);
		if (m_HoveredUrlIndex == urlIndexAtCharacterIndex)
		{
			return;
		}
		UIAbilityTooltip.GlobalHide();
		m_UiCamera.ReallowTooltip(base.gameObject);
		if ((bool)ExtraCollider)
		{
			m_UiCamera.ReallowTooltip(ExtraCollider);
		}
		if (ColorizeLinks)
		{
			string text = m_Label.text;
			if (m_HoveredUrlIndex >= 0)
			{
				text = SetLinkColor(text, m_HoveredUrlIndex, LinkColorSet, hoverState: false);
			}
			if (urlIndexAtCharacterIndex >= 0)
			{
				text = SetLinkColor(text, urlIndexAtCharacterIndex, LinkColorSet, hoverState: true);
			}
			SetLabelText(text);
		}
		m_HoveredUrlIndex = urlIndexAtCharacterIndex;
	}

	private void OnLabelChanged(string newText)
	{
		if (!base.enabled)
		{
			return;
		}
		if (newText != m_previousText)
		{
			if (AddUrlTags && Glossary.Instance != null)
			{
				string text = (m_previousText = Glossary.Instance.AddUrlTags(newText, null, GameState.Option.GetOption(GameOption.BoolOption.GLOSSARY_EXACT)));
				m_Label.text = text;
			}
			SetAllUnhighlighted();
		}
		if ((bool)GetComponent<Collider>())
		{
			NGUITools.AddWidgetCollider(base.gameObject);
		}
	}

	private void SetAllUnhighlighted()
	{
		if (!ColorizeLinks)
		{
			return;
		}
		string text = m_Label.text;
		int num = 0;
		while (true)
		{
			int num2 = text.IndexOf("[url=", num);
			if (num2 < 0)
			{
				break;
			}
			num = num2 + 5;
			text = SetLinkColor(text, num, LinkColorSet, hoverState: false);
		}
		SetLabelText(text);
		m_HoveredUrlIndex = -1;
	}

	public static string SetLinkColor(string text, int urlIndex, UIGlobalColor.LinkStyle linkStyle, bool hoverState)
	{
		int num = text.IndexOf(']', urlIndex) + 1;
		int num2 = text.IndexOf("[/url]", num);
		string text2 = text.Substring(num, num2 - num);
		string urlAtCharacterIndex = UILabel.GetUrlAtCharacterIndex(text, num - 1);
		Color c = (urlAtCharacterIndex.StartsWith("buffvalue://") ? UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, linkStyle, hoverState, UIGlobalColor.LinkType.BUFF) : ((!urlAtCharacterIndex.StartsWith("debuffvalue://")) ? UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, linkStyle, hoverState) : UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.LINK, linkStyle, hoverState, UIGlobalColor.LinkType.DEBUFF)));
		string text3 = NGUITools.StripSymbols(text2);
		return text.Remove(num, text2.Length).Insert(num, "[" + NGUITools.EncodeColor(c) + "]" + text3 + "[-]");
	}

	private string GetSelectedUrl()
	{
		Vector2 relativeMouseCoords = GetRelativeMouseCoords();
		string urlAtPosition = m_Label.GetUrlAtPosition(relativeMouseCoords);
		if (!string.IsNullOrEmpty(urlAtPosition))
		{
			return urlAtPosition;
		}
		return null;
	}

	public void OnColliderHover(GameObject obj, bool hovered)
	{
		if (!hovered)
		{
			SetAllUnhighlighted();
			UIAbilityTooltip.GlobalHide();
		}
		m_Hovered = hovered;
	}

	public void OnColliderClick(GameObject obj)
	{
		string selectedUrl = GetSelectedUrl();
		if (!string.IsNullOrEmpty(selectedUrl) && (!selectedUrl.StartsWith("glossary") || Behavior == LinkBehavior.OpenToPage))
		{
			UIHyperlinkManager.FollowLink(selectedUrl);
		}
	}

	public void OnColliderTooltip(GameObject obj, bool over)
	{
		if (!over)
		{
			UIAbilityTooltip.GlobalHide();
			return;
		}
		string selectedUrl = GetSelectedUrl();
		if (!string.IsNullOrEmpty(selectedUrl) && (!selectedUrl.StartsWith("glossary") || Behavior == LinkBehavior.ShowTooltip))
		{
			ITooltipContent tooltip = UIHyperlinkManager.GetTooltip(selectedUrl);
			if (tooltip != null)
			{
				UIAbilityTooltip.GlobalShow(UICamera.lastHit.point, tooltip);
			}
		}
	}

	private Vector2 GetRelativeMouseCoords()
	{
		Vector3 position = UICamera.lastTouchPosition;
		position.z = base.transform.position.z;
		Vector3 position2 = m_UiCamera.cachedCamera.ScreenToWorldPoint(position);
		Vector3 vector = base.transform.InverseTransformPoint(position2);
		vector.Scale(base.transform.localScale);
		return vector;
	}

	private bool ScaledByParentNotUIRoot()
	{
		return base.transform.parent.lossyScale != InGameUILayout.Root.transform.localScale;
	}

	private void SetLabelText(string text)
	{
		m_previousText = text;
		m_Label.text = text;
	}
}
