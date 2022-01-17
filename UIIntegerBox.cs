using System;
using UnityEngine;

public class UIIntegerBox : MonoBehaviour
{
	public delegate void ValueChanged(int value);

	public UILabel Label;

	public GameObject DragCollider;

	public GameObject UpCollider;

	public GameObject DownCollider;

	public float DragInterval = 20f;

	private int m_DragBuffer;

	public int Max = 100;

	public int Min = 1;

	private int m_DisplayMultiplier = 1;

	private int m_Value;

	[HideInInspector]
	public int DisplayMultiplier
	{
		get
		{
			return m_DisplayMultiplier;
		}
		set
		{
			m_DisplayMultiplier = value;
			Value = Value;
		}
	}

	public int Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			int value2 = m_Value;
			m_Value = Mathf.Clamp(value, Min, Max);
			Label.text = (m_Value * DisplayMultiplier).ToString();
			if (value2 != m_Value && this.OnValueChanged != null)
			{
				this.OnValueChanged(m_Value);
			}
		}
	}

	public event ValueChanged OnValueChanged;

	private void Start()
	{
		Value = Min;
		if ((bool)DragCollider)
		{
			UIEventListener uIEventListener = UIEventListener.Get(DragCollider);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragged));
		}
		if ((bool)UpCollider)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(UpCollider);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnUp));
		}
		if ((bool)DownCollider)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(DownCollider);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnDown));
		}
	}

	private void OnDragged(GameObject sender, Vector2 delta)
	{
		m_DragBuffer += (int)delta.x;
		if ((float)Mathf.Abs(m_DragBuffer) >= DragInterval)
		{
			Value += (int)Mathf.Sign(m_DragBuffer);
			m_DragBuffer = 0;
		}
	}

	private void OnUp(GameObject sender)
	{
		Value++;
	}

	private void OnDown(GameObject sender)
	{
		Value--;
	}
}
