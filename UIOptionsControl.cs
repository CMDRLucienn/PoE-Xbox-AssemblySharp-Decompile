using System;
using System.Collections.Generic;
using UnityEngine;

public class UIOptionsControl : MonoBehaviour
{
	private MappedControl m_Control;

	public UILabel Label;

	public UIGrid Grid;

	public UIOptionsKeyControl RootControl;

	private List<UIOptionsKeyControl> m_Controls;

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		UpdateName();
	}

	private void UpdateName()
	{
		Label.text = MappedInput.GetControlName(m_Control);
	}

	private void Init()
	{
		if (m_Controls != null)
		{
			return;
		}
		m_Controls = new List<UIOptionsKeyControl>();
		bool flag = true;
		for (int i = 0; i < UIOptionsControlManager.AltKeyCount; i++)
		{
			UIOptionsKeyControl uIOptionsKeyControl = null;
			if (flag)
			{
				flag = false;
				uIOptionsKeyControl = RootControl;
			}
			else
			{
				uIOptionsKeyControl = NGUITools.AddChild(RootControl.transform.parent.gameObject, RootControl.gameObject).GetComponent<UIOptionsKeyControl>();
			}
			uIOptionsKeyControl.Label.text = "";
			UIEventListener uIEventListener = UIEventListener.Get(uIOptionsKeyControl.Collider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			UIEventListener uIEventListener2 = UIEventListener.Get(uIOptionsKeyControl.Collider.gameObject);
			uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
			m_Controls.Add(uIOptionsKeyControl);
		}
	}

	private void OnChildRightClick(GameObject go)
	{
		if (UIOptionsManager.Instance.WaitToSet)
		{
			return;
		}
		for (int i = 0; i < m_Controls.Count; i++)
		{
			if (go == m_Controls[i].Collider.gameObject)
			{
				UIOptionsManager.Instance.ClearControl(m_Control, i);
				break;
			}
		}
	}

	private void OnChildClick(GameObject go)
	{
		if (UIOptionsManager.Instance.WaitToSet)
		{
			return;
		}
		for (int i = 0; i < m_Controls.Count; i++)
		{
			if (go == m_Controls[i].Collider.gameObject)
			{
				UIOptionsManager.Instance.BeginMapControl(m_Control, i);
				m_Controls[i].BlinkTween.Play(forward: true);
				m_Controls[i].Label.text = GUIUtils.GetText(842);
				break;
			}
		}
	}

	public void Set(MappedControl control)
	{
		m_Control = control;
		UpdateName();
		Reload();
	}

	public void Reload()
	{
		Init();
		List<KeyControl> list = UIOptionsManager.Instance.Controls.Controls[(int)m_Control];
		for (int i = 0; i < m_Controls.Count; i++)
		{
			m_Controls[i].BlinkTween.Reset();
			m_Controls[i].BlinkTween.enabled = false;
			m_Controls[i].Label.color = Color.white;
			if (i < list.Count && !list[i].Empty())
			{
				m_Controls[i].Label.text = list[i].ToString();
			}
			else
			{
				m_Controls[i].Label.text = "";
			}
		}
		Grid.Reposition();
	}
}
