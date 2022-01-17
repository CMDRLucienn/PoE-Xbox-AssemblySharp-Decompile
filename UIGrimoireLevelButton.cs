using System;
using UnityEngine;

public class UIGrimoireLevelButton : MonoBehaviour
{
	public UILabel NumberLabel;

	public Collider Collider;

	private UIImageButtonRevised m_highlighter;

	private int m_Level = 1;

	public int Level
	{
		get
		{
			return m_Level;
		}
		set
		{
			m_Level = value;
			NumberLabel.text = RomanNumeral.Convert(m_Level);
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		Init();
	}

	private void Init()
	{
		if (!(m_highlighter != null))
		{
			m_highlighter = Collider.GetComponent<UIImageButtonRevised>();
		}
	}

	private void OnChildClick(GameObject sender)
	{
		UIGrimoireManager.Instance.LevelButtons.ChangeLevel(Level);
	}

	public void ForceHighlight(bool setting)
	{
		Init();
		m_highlighter.SetOverrideHighlighted(setting);
	}
}
