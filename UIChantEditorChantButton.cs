using System;
using UnityEngine;

public class UIChantEditorChantButton : MonoBehaviour
{
	public UILabel NumberLabel;

	public Collider Collider;

	private UIImageButtonRevised m_highlighter;

	[HideInInspector]
	public UIChantEditorChant Owner;

	private int m_Chant;

	public int Chant
	{
		get
		{
			return m_Chant;
		}
		set
		{
			m_Chant = value;
			NumberLabel.text = TextUtils.IndexToAlphabet(m_Chant);
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
		Owner.Select();
	}

	public void ForceHighlight(bool setting)
	{
		Init();
		m_highlighter.SetOverrideHighlighted(setting);
	}
}
