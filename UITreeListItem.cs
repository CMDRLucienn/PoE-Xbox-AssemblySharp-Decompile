using System;
using System.Collections.Generic;
using UnityEngine;

public class UITreeListItem : MonoBehaviour
{
	public GameObject BaseChild;

	public UITreeListItem UseBaseChildOf;

	public Transform IndentationOffset;

	public float IndentationAmount;

	public UILabel DisplayLabel;

	public UIIsButton ExpandableIndicator;

	public UIIsButton Button;

	public Color LabelNeutralColor;

	public Color LabelSelectedColor;

	public Color LabelDisabledColor;

	public UITable ChildLayout;

	private List<UITreeListItem> m_Children = new List<UITreeListItem>();

	private List<UITreeListItem> m_ChildrenPool = new List<UITreeListItem>();

	private bool m_Selected;

	private bool m_Expanded = true;

	private UITreeList m_TopParent;

	private UITreeListItem m_Parent;

	private bool m_VisualDisabled;

	public ITreeListContent Data { get; private set; }

	private UITreeList TopParent
	{
		get
		{
			if (!m_TopParent)
			{
				m_TopParent = GetComponentInParent<UITreeList>();
			}
			return m_TopParent;
		}
	}

	private UITreeListItem Parent
	{
		get
		{
			if (!m_Parent)
			{
				m_Parent = base.transform.parent.GetComponentInParent<UITreeListItem>();
			}
			return m_Parent;
		}
	}

	public bool HasChildren => m_Children.Count > 0;

	public bool Selectable
	{
		get
		{
			if (HasChildren)
			{
				return Data is BestiaryParent;
			}
			return true;
		}
	}

	public bool Enabled
	{
		get
		{
			if ((bool)Button)
			{
				return Button.enabled;
			}
			return false;
		}
		set
		{
			if ((bool)Button)
			{
				Button.enabled = value;
			}
			if ((bool)ExpandableIndicator)
			{
				ExpandableIndicator.enabled = value;
			}
			UpdateColor();
		}
	}

	public event UIEventListener.VoidDelegate OnClick;

	private void OnEnable()
	{
		if ((bool)TopParent && TopParent.SelectedItem == this)
		{
			TopParent.SelectionPointer.gameObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)TopParent && TopParent.SelectedItem == this)
		{
			TopParent.SelectionPointer.gameObject.SetActive(value: false);
		}
	}

	private void Awake()
	{
		if ((bool)BaseChild && BaseChild != base.gameObject)
		{
			BaseChild.SetActive(value: false);
		}
	}

	private void Start()
	{
		UIMultiSpriteImageButton component = GetComponent<UIMultiSpriteImageButton>();
		if ((bool)component)
		{
			component.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(component.onClick, new UIEventListener.VoidDelegate(OnItemClick));
		}
		else if ((bool)Button)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Button);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemClick));
		}
		if ((bool)ExpandableIndicator)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(ExpandableIndicator);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnExpandClick));
		}
	}

	private void OnExpandClick(GameObject sender)
	{
		ToggleExpanded();
	}

	private void OnItemClick(GameObject sender)
	{
		if (this.OnClick != null)
		{
			this.OnClick(base.gameObject);
		}
		else if (Selectable)
		{
			TopParent.SelectedItem = this;
		}
		else
		{
			ToggleExpanded();
		}
	}

	public void Unload()
	{
		Data = null;
		this.OnClick = null;
	}

	public void Load(ITreeListContent content, bool defaultExpand)
	{
		Data = content;
		m_VisualDisabled = false;
		Enabled = true;
		if (content is ITreeListContentWithChildren children)
		{
			SetChildren(children);
		}
		else
		{
			ClearChildren();
		}
		if ((bool)ExpandableIndicator)
		{
			ExpandableIndicator.gameObject.SetActive(HasChildren);
		}
		if (defaultExpand)
		{
			Expand();
		}
		else
		{
			Contract();
		}
		ReloadVisualsFromData();
	}

	public void Navigate(int move)
	{
		if ((bool)Parent)
		{
			Parent.Navigate(this, move);
		}
	}

	public void Navigate(UITreeListItem childStart, int move)
	{
		int num = m_Children.IndexOf(childStart);
		if (num < 0)
		{
			Debug.LogError("Tried to UITreeListItem.Navigate from an item that wasn't a child.");
			return;
		}
		while (move != 0)
		{
			int num2 = (int)Mathf.Sign(move);
			num += num2;
			if (num < 0 || num >= m_Children.Count)
			{
				return;
			}
			if (m_Children[num].Selectable)
			{
				move -= num2;
			}
		}
		TopParent.SelectedItem = m_Children[num];
	}

	protected virtual void ReloadVisualsFromData()
	{
		if ((bool)DisplayLabel)
		{
			if (Data != null)
			{
				DisplayLabel.text = Data.GetTreeListDisplayName();
			}
			else
			{
				DisplayLabel.text = "";
			}
		}
	}

	public void SetVisualDisabled(bool state)
	{
		m_VisualDisabled = state;
		UpdateColor();
	}

	public UITreeListItem AddChild(ITreeListContent itemContent)
	{
		return AddChild(itemContent, defaultExpand: true);
	}

	public UITreeListItem AddChild(ITreeListContent itemContent, bool defaultExpand)
	{
		UITreeListItem freeChild = GetFreeChild();
		freeChild.gameObject.SetActive(value: true);
		freeChild.gameObject.name = m_Children.Count.ToString("00000");
		if ((bool)freeChild)
		{
			m_Children.Add(freeChild);
			freeChild.Load(itemContent, defaultExpand);
		}
		else
		{
			Debug.LogError("UITreeListItem child is not a UITreeListItem.");
		}
		return freeChild;
	}

	public UITreeListItem AddChild(string str, bool disabled = true)
	{
		UITreeListItem freeChild = GetFreeChild();
		freeChild.gameObject.SetActive(value: true);
		freeChild.gameObject.name = m_Children.Count.ToString("00000");
		if ((bool)freeChild)
		{
			m_Children.Add(freeChild);
			freeChild.Load(null, defaultExpand: false);
			freeChild.DisplayLabel.text = str;
			freeChild.Enabled = !disabled;
		}
		else
		{
			Debug.LogError("UITreeListItem child is not a UITreeListItem.");
		}
		return freeChild;
	}

	private UITreeListItem GetFreeChild()
	{
		if (m_ChildrenPool.Count > 0)
		{
			UITreeListItem component = m_ChildrenPool[m_ChildrenPool.Count - 1].gameObject.GetComponent<UITreeListItem>();
			component.Unload();
			m_ChildrenPool.RemoveAt(m_ChildrenPool.Count - 1);
			return component;
		}
		UITreeListItem component2 = NGUITools.AddChild(ChildLayout.gameObject, UseBaseChildOf ? UseBaseChildOf.BaseChild : BaseChild).GetComponent<UITreeListItem>();
		if ((bool)component2.IndentationOffset)
		{
			component2.IndentationOffset.transform.localPosition = new Vector3(IndentationAmount, 0f, 0f);
		}
		return component2;
	}

	private void SetChildren(ITreeListContentWithChildren contents)
	{
		ClearChildren();
		contents.LoadTreeListChildren(this);
		ChildLayout.Reposition();
	}

	public void RepositionRecursive()
	{
		foreach (UITreeListItem child in m_Children)
		{
			if ((bool)child)
			{
				child.RepositionRecursive();
			}
		}
		if ((bool)ChildLayout)
		{
			ChildLayout.Reposition();
		}
	}

	protected void RepositionUpward()
	{
		if ((bool)ChildLayout)
		{
			ChildLayout.Reposition();
		}
		if ((bool)Parent)
		{
			Parent.RepositionUpward();
		}
	}

	private void ClearChildren()
	{
		for (int i = 0; i < m_Children.Count; i++)
		{
			m_Children[i].gameObject.SetActive(value: false);
		}
		m_ChildrenPool.AddRange(m_Children);
		m_Children.Clear();
	}

	public void ToggleExpanded()
	{
		if (m_Expanded)
		{
			Contract();
		}
		else
		{
			Expand();
		}
	}

	public void Contract()
	{
		foreach (UITreeListItem child in m_Children)
		{
			if ((bool)child)
			{
				child.gameObject.SetActive(value: false);
			}
		}
		m_Expanded = false;
		RepositionUpward();
	}

	public void Expand(bool reposition = true)
	{
		foreach (UITreeListItem child in m_Children)
		{
			if ((bool)child)
			{
				child.gameObject.SetActive(value: true);
			}
		}
		m_Expanded = true;
		RepositionUpward();
	}

	public void ExpandUpward()
	{
		Expand(reposition: false);
		if ((bool)ChildLayout)
		{
			ChildLayout.Reposition();
		}
		if ((bool)Parent)
		{
			Parent.ExpandUpward();
		}
	}

	public bool SelectItem(ITreeListContent item)
	{
		if (Data == item && Enabled)
		{
			TopParent.SelectedItem = this;
			return true;
		}
		foreach (UITreeListItem child in m_Children)
		{
			if ((bool)child && child.SelectItem(item))
			{
				return true;
			}
		}
		return false;
	}

	public void NotifySelect()
	{
		m_Selected = true;
		UpdateColor();
	}

	public void NotifyDeselect()
	{
		m_Selected = false;
		UpdateColor();
	}

	private void UpdateColor()
	{
		if (!DisplayLabel)
		{
			return;
		}
		UIImageButtonRevised component = DisplayLabel.GetComponent<UIImageButtonRevised>();
		if ((bool)component)
		{
			if (m_Selected)
			{
				component.SetNeutralColor(LabelSelectedColor);
			}
			else if (Enabled && !m_VisualDisabled)
			{
				component.SetNeutralColor(LabelNeutralColor);
			}
			else
			{
				component.SetNeutralColor(LabelDisabledColor);
			}
			component.UpdateImage();
		}
	}
}
