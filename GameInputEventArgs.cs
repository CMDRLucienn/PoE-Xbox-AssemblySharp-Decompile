using System;
using UnityEngine;

public class GameInputEventArgs : EventArgs
{
	protected bool m_handled;

	public bool PointerDown;

	public bool PointerUp;

	public bool Alt;

	public float HoldTime;

	public Vector3 ScreenCursorPosition;

	public Vector3 WorldCursorPosition;

	public RaycastHit CursorHit;

	public bool Handled
	{
		get
		{
			return m_handled;
		}
		set
		{
			m_handled = value;
		}
	}
}
