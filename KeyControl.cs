using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct KeyControl
{
	public KeyCode KeyCode;

	public bool ShiftKey;

	public bool AltKey;

	public bool CtrlKey;

	private const char serialSeperator = ':';

	public KeyControl(KeyCode keyCode)
	{
		KeyCode = keyCode;
		ShiftKey = false;
		AltKey = false;
		CtrlKey = false;
	}

	public KeyControl(string serialString)
	{
		string[] array = serialString.Split(':');
		int val = 0;
		IntUtils.TryParseInvariant(array[0], out val);
		KeyCode = (KeyCode)val;
		ShiftKey = array[1][0] == '1';
		AltKey = array[1][1] == '1';
		CtrlKey = array[1][2] == '1';
	}

	public KeyControl(KeyControl other)
	{
		KeyCode = other.KeyCode;
		ShiftKey = other.ShiftKey;
		AltKey = other.AltKey;
		CtrlKey = other.CtrlKey;
	}

	public bool HasModifiers()
	{
		if (!ShiftKey && !AltKey)
		{
			return CtrlKey;
		}
		return true;
	}

	public void RemoveModifiers(List<KeyControl> from)
	{
		foreach (KeyControl item in from)
		{
			if (item.KeyCode == KeyCode.None)
			{
				if (item.ShiftKey)
				{
					ShiftKey = false;
				}
				if (item.CtrlKey)
				{
					CtrlKey = false;
				}
				if (item.AltKey)
				{
					AltKey = false;
				}
			}
		}
	}

	public bool Empty()
	{
		if (KeyCode == KeyCode.None)
		{
			return !HasModifiers();
		}
		return false;
	}

	public override string ToString()
	{
		string text = KeyCode.ToString();
		if (AltKey)
		{
			text = "Alt+" + text;
		}
		if (ShiftKey)
		{
			text = "Shift+" + text;
		}
		if (CtrlKey)
		{
			text = "Ctrl+" + text;
		}
		return text;
	}

	public override bool Equals(object obj)
	{
		if (obj is KeyControl keyControl)
		{
			if (KeyCode == keyControl.KeyCode && ShiftKey == keyControl.ShiftKey && AltKey == keyControl.AltKey)
			{
				return CtrlKey == keyControl.CtrlKey;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return KeyCode.GetHashCode() + (ShiftKey ? 7901 : 0) + (AltKey ? 7907 : 0) + (CtrlKey ? 7919 : 0);
	}

	public string GetSerialString()
	{
		return ((int)KeyCode).ToStringInvariant() + ":" + (ShiftKey ? "1" : "0") + (AltKey ? "1" : "0") + (CtrlKey ? "1" : "0");
	}
}
