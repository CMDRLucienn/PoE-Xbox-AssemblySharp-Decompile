using System;
using UnityEngine;

public class UIFormationsTabs : MonoBehaviour
{
	public delegate void TabChanged(int tab);

	public UIMultiSpriteImageButton BaseTab;

	private UIMultiSpriteImageButton[] m_Tabs;

	private int[] m_TabDepths;

	public int NumberOfTabs = 2;

	private int m_CurrentTab;

	public TabChanged OnTabChanged;

	private bool m_Initialized;

	public int CurrentTab => m_CurrentTab;

	private void Start()
	{
		if (!m_Initialized)
		{
			Init();
			m_CurrentTab = -1;
			SetTab(0);
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void TabClicked(GameObject sender)
	{
		for (int i = 0; i < m_Tabs.Length; i++)
		{
			if (sender == m_Tabs[i].gameObject)
			{
				SetTab(i);
				break;
			}
		}
	}

	public void SetTab(int index)
	{
		if (!m_Initialized)
		{
			Init();
		}
		if (OnTabChanged != null)
		{
			OnTabChanged(index);
		}
		if (m_CurrentTab >= 0)
		{
			m_Tabs[m_CurrentTab].ForceHighlight(state: false);
		}
		m_CurrentTab = index;
		m_Tabs[m_CurrentTab].ForceHighlight(state: true);
		UpdateTabDepths();
	}

	public void Init()
	{
		m_Tabs = new UIMultiSpriteImageButton[NumberOfTabs];
		m_TabDepths = new int[NumberOfTabs];
		BaseTab.Label.text = 1.ToString();
		UIMultiSpriteImageButton baseTab = BaseTab;
		baseTab.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(baseTab.onClick, new UIEventListener.VoidDelegate(TabClicked));
		BaseTab.gameObject.SetActive(value: true);
		m_Tabs[0] = BaseTab;
		for (int i = 1; i < NumberOfTabs; i++)
		{
			GameObject obj = NGUITools.AddChild(BaseTab.transform.parent.gameObject, BaseTab.gameObject);
			UIMultiSpriteImageButton component = obj.GetComponent<UIMultiSpriteImageButton>();
			component.Label.text = (i + 1).ToString();
			component.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(component.onClick, new UIEventListener.VoidDelegate(TabClicked));
			obj.SetActive(value: true);
			m_Tabs[i] = component;
		}
		UpdateTabDepths();
		GetComponent<UIGrid>().Reposition();
		m_Initialized = true;
	}

	private void UpdateTabDepths()
	{
		SetTabDepth(m_CurrentTab, NumberOfTabs);
		for (int i = m_CurrentTab + 1; i < NumberOfTabs; i++)
		{
			SetTabDepth(i, NumberOfTabs - (i - m_CurrentTab));
		}
		for (int num = m_CurrentTab - 1; num >= 0; num--)
		{
			SetTabDepth(num, NumberOfTabs - (m_CurrentTab - num));
		}
	}

	private void SetTabDepth(int tabIndex, int depth)
	{
		IncreaseTabDepth(tabIndex, depth - m_TabDepths[tabIndex]);
		m_TabDepths[tabIndex] = depth;
	}

	private void IncreaseTabDepth(int tabIndex, int depthIncrease)
	{
		UIWindowManager.IncreaseSpriteDepthRecursive(m_Tabs[tabIndex].gameObject, depthIncrease * 10);
	}
}
