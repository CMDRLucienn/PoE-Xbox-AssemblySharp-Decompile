using System;
using UnityEngine;

public class UINewsArticle : MonoBehaviour
{
	public UILabel NewsTitle;

	public UILabel NewsDate;

	public UICapitularLabel NewsDescription;

	public Collider TextCollider;

	public UIAnchor AnchorLine;

	private NewsModel m_Model;

	private bool m_hovered;

	private int m_PrevHoveredURLIndex = -1;

	private const float MIN_LINE_Y = -125f;

	public void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(TextCollider);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnDescriptionClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(TextCollider);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnDescriptionHover));
		AnchorLine.enabled = false;
	}

	public void Update()
	{
		if (m_hovered)
		{
			Vector3 position = UICamera.lastTouchPosition;
			position.z = base.transform.position.z;
			Vector3 position2 = UICamera.mainCamera.ScreenToWorldPoint(position);
			Vector2 localPos = NewsDescription.TextLabel.transform.InverseTransformPoint(position2);
			localPos.Scale(NewsDescription.TextLabel.transform.localScale);
			int characterIndexAtPosition = NewsDescription.TextLabel.GetCharacterIndexAtPosition(localPos);
			int urlIndexAtCharacterIndex = NewsDescription.TextLabel.GetUrlIndexAtCharacterIndex(characterIndexAtPosition);
			if (urlIndexAtCharacterIndex != m_PrevHoveredURLIndex)
			{
				if (urlIndexAtCharacterIndex > 0)
				{
					NewsDescription.TextLabel.text = UIGlossaryEnabledLabel.SetLinkColor(NewsDescription.TextLabel.text, urlIndexAtCharacterIndex, UIGlobalColor.LinkStyle.NEWS, hoverState: true);
					m_PrevHoveredURLIndex = urlIndexAtCharacterIndex;
				}
				else if (m_PrevHoveredURLIndex > 0)
				{
					ParseURLs();
					m_PrevHoveredURLIndex = -1;
				}
			}
		}
		UpdateLineSeparator();
	}

	public void Load(NewsModel newModel)
	{
		m_Model = newModel;
		NewsTitle.text = m_Model.Title.Trim();
		NewsDate.text = m_Model.Date.ToShortDateString();
		NewsDescription.text = m_Model.Description.Trim();
		ParseURLs();
	}

	public void UpdateLineSeparator()
	{
		if (!(AnchorLine == null))
		{
			AnchorLine.Update();
			Vector3 localPosition = AnchorLine.transform.localPosition;
			if (AnchorLine.transform.localPosition.y > -125f)
			{
				localPosition.y = -125f;
				AnchorLine.transform.localPosition = localPosition;
			}
		}
	}

	private void OnDescriptionClick(GameObject source)
	{
		Vector3 position = UICamera.lastTouchPosition;
		position.z = base.transform.position.z;
		Vector3 position2 = UICamera.mainCamera.ScreenToWorldPoint(position);
		Vector2 localPos = NewsDescription.TextLabel.transform.InverseTransformPoint(position2);
		localPos.Scale(NewsDescription.TextLabel.transform.localScale);
		string urlAtPosition = NewsDescription.TextLabel.GetUrlAtPosition(localPos);
		if (!string.IsNullOrEmpty(urlAtPosition))
		{
			Application.OpenURL(urlAtPosition);
		}
	}

	private void OnDescriptionHover(GameObject source, bool isHovering)
	{
		m_hovered = isHovering;
		if (!m_hovered)
		{
			ParseURLs();
		}
	}

	private void ParseURLs()
	{
		string text = NewsDescription.TextLabel.text;
		int num = 0;
		while (true)
		{
			int num2 = text.IndexOf("[url=", num);
			if (num2 < 0)
			{
				break;
			}
			num = num2 + 5;
			text = UIGlossaryEnabledLabel.SetLinkColor(text, num, UIGlobalColor.LinkStyle.NEWS, hoverState: false);
		}
		NewsDescription.TextLabel.text = text;
	}
}
