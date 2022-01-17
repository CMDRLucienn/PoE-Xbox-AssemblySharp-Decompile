using System;
using UnityEngine;

public abstract class UIStrongholdExpandableItem : UIStrongholdActionItem
{
	public UILabel NameLabel;

	public UILabel DescLabel;

	public UIWidget ExpandButton;

	public GameObject ExpandCollider;

	public GameObject BottomAnchored;

	private UIStrongholdParchmentSizer m_ParentSizer;

	private bool m_Expanded;

	public bool Expanded
	{
		get
		{
			return m_Expanded;
		}
		set
		{
			UITable componentInParent = GetComponentInParent<UITable>();
			m_Expanded = value;
			if (m_Expanded)
			{
				DescLabel.maxLineCount = 0;
				ExpandButton.transform.localScale = new Vector3(ExpandButton.transform.localScale.x, 0f - Mathf.Abs(ExpandButton.transform.localScale.y), 1f);
				if ((bool)componentInParent)
				{
					componentInParent.repositionNow = true;
				}
			}
			else
			{
				DescLabel.maxLineCount = 3;
				ExpandButton.transform.localScale = new Vector3(ExpandButton.transform.localScale.x, Mathf.Abs(ExpandButton.transform.localScale.y), 1f);
				if ((bool)componentInParent)
				{
					componentInParent.repositionNow = true;
				}
			}
			float b = DescLabel.transform.localPosition.y - DescLabel.transform.localScale.y * DescLabel.relativeSize.y;
			BottomAnchored.transform.localPosition = new Vector3(BottomAnchored.transform.localPosition.x, Mathf.Min(-114f, b), BottomAnchored.transform.localPosition.z);
			if ((bool)m_ParentSizer)
			{
				m_ParentSizer.UpdateParchmentSize();
				m_ParentSizer.ParchmentNeedsReposition = 2;
			}
		}
	}

	protected virtual void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(ExpandCollider);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnExpandClicked));
		m_ParentSizer = GetComponentInParent<UIStrongholdParchmentSizer>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Expanded = false;
	}

	private void OnExpandClicked(GameObject sender)
	{
		Expanded = !Expanded;
	}

	protected void SetDescriptionText(string text)
	{
		DescLabel.text = text;
		UIDynamicFontSize.Guarantee(DescLabel.gameObject);
		int num = 0;
		text = DescLabel.WrapText(DescLabel.text, out var _, ignoreLineCount: true);
		for (int num2 = text.Length - 1; num2 >= 0; num2--)
		{
			if (text[num2] == '\n')
			{
				num++;
			}
		}
		bool active = num >= 3;
		ExpandButton.gameObject.SetActive(active);
		ExpandCollider.SetActive(active);
	}
}
