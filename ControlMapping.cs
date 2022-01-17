using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ControlMapping
{
	public List<KeyControl>[] Controls = new List<KeyControl>[77];

	private const int HIDDEN_INPUT_VERSION = 2;

	public ControlMapping()
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			Controls[i] = new List<KeyControl>();
		}
	}

	public void Restored()
	{
		int @int = PlayerPrefs.GetInt("hiddenInputVersion", 0);
		if (2 != @int)
		{
			bool[] array = new bool[77];
			MappedControl[][] categorizedControls = MappedInput.CategorizedControls;
			foreach (MappedControl[] array2 in categorizedControls)
			{
				foreach (MappedControl mappedControl in array2)
				{
					array[(int)mappedControl] = true;
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (!array[k])
				{
					CopyFrom((MappedControl)k, MappedInput.DefaultMapping);
				}
			}
			SaveToPrefs();
		}
		if (PlayerPrefs.HasKey("hasSteamControllerDefaults"))
		{
			return;
		}
		for (MappedControl mappedControl2 = MappedControl.NEXT_ABILITY; mappedControl2 <= MappedControl.MENU_CYCLE_RIGHT; mappedControl2++)
		{
			CopyFromSafe(mappedControl2, MappedInput.DefaultMapping);
		}
		for (MappedControl mappedControl3 = MappedControl.NEXT_COMPANION; mappedControl3 <= MappedControl.PREVIOUS_COMPANION; mappedControl3++)
		{
			if (Controls[(int)mappedControl3].Count == 0)
			{
				CopyFromSafe(mappedControl3, MappedInput.DefaultMapping);
			}
		}
		MergeFromSafe(MappedControl.TAKE_ALL, MappedInput.DefaultMapping);
		PlayerPrefs.SetInt("hasSteamControllerDefaults", 1);
	}

	public void AddControl(MappedControl control, KeyCode key)
	{
		Controls[(int)control].Add(new KeyControl(key));
	}

	public bool ControlEmpty(MappedControl c)
	{
		foreach (KeyControl item in Controls[(int)c])
		{
			if (!item.Empty())
			{
				return false;
			}
		}
		return true;
	}

	public void AddControl(MappedControl control, KeyCode key1, KeyCode key2, KeyCode key3)
	{
		Controls[(int)control].Add(new KeyControl(key1));
		Controls[(int)control].Add(new KeyControl(key2));
		Controls[(int)control].Add(new KeyControl(key3));
	}

	public void AddControl(MappedControl control, KeyCode key1, KeyCode key2)
	{
		Controls[(int)control].Add(new KeyControl(key1));
		Controls[(int)control].Add(new KeyControl(key2));
	}

	public void AddShiftControl(MappedControl control, KeyCode key)
	{
		KeyControl item = new KeyControl(key);
		item.ShiftKey = true;
		Controls[(int)control].Add(item);
	}

	public void AddAltControl(MappedControl control, KeyCode key)
	{
		KeyControl item = new KeyControl(key);
		item.AltKey = true;
		Controls[(int)control].Add(item);
	}

	public void AddCtrlControl(MappedControl control, KeyCode key)
	{
		KeyControl item = new KeyControl(key);
		item.CtrlKey = true;
		Controls[(int)control].Add(item);
	}

	public void ClearControls(MappedControl control)
	{
		Controls[(int)control].Clear();
	}

	public ControlMapping Copy()
	{
		ControlMapping controlMapping = new ControlMapping();
		for (int i = 0; i < Controls.Length; i++)
		{
			controlMapping.Controls[i].AddRange(Controls[i]);
		}
		return controlMapping;
	}

	public void CopyFrom(ControlMapping other)
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			Controls[i].Clear();
			Controls[i].AddRange(other.Controls[i]);
		}
	}

	public bool CopyFromSafe(MappedControl ctrl, ControlMapping other)
	{
		if (BindWillConflict(ctrl, other.Controls[(int)ctrl]) == MappedControl.NONE)
		{
			CopyFrom(ctrl, other);
			return true;
		}
		return false;
	}

	public void MergeFromSafe(MappedControl ctrl, ControlMapping other)
	{
		for (int i = 0; i < other.Controls[(int)ctrl].Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < Controls.Length; j++)
			{
				if (flag)
				{
					break;
				}
				if (j == (int)ctrl || MappedInput.IsOverlapAllowed(ctrl, (MappedControl)j))
				{
					continue;
				}
				List<KeyControl> list = Controls[j];
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].Equals(other.Controls[(int)ctrl][i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				while (i >= Controls[(int)ctrl].Count)
				{
					Controls[(int)ctrl].Add(default(KeyControl));
				}
				if (Controls[(int)ctrl][i].Empty())
				{
					Controls[(int)ctrl][i] = other.Controls[(int)ctrl][i];
				}
			}
		}
	}

	public MappedControl CurrentBindIsConflicted(MappedControl ctrl)
	{
		return BindWillConflict(ctrl, Controls[(int)ctrl]);
	}

	public MappedControl BindWillConflict(MappedControl ctrl, List<KeyControl> bind)
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			if (i == (int)ctrl || MappedInput.IsOverlapAllowed(ctrl, (MappedControl)i))
			{
				continue;
			}
			List<KeyControl> list = Controls[i];
			for (int j = 0; j < list.Count; j++)
			{
				for (int k = 0; k < bind.Count; k++)
				{
					if (list[j].Equals(bind[k]))
					{
						return (MappedControl)i;
					}
				}
			}
		}
		return MappedControl.NONE;
	}

	public void CopyFrom(MappedControl ctrl, ControlMapping other)
	{
		Controls[(int)ctrl].Clear();
		Controls[(int)ctrl].AddRange(other.Controls[(int)ctrl]);
	}

	public bool Matches(ControlMapping other)
	{
		for (int i = 0; i < 77; i++)
		{
			if (other.Controls[i].Count != Controls[i].Count)
			{
				return false;
			}
			for (int j = 0; j < Controls[i].Count; j++)
			{
				if (!other.Controls[i][j].Equals(Controls[i][j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public string GetControlString(MappedControl control)
	{
		return TextUtils.FuncJoin((KeyControl kc) => kc.ToString(), Controls[(int)control], ", ");
	}

	public void SaveToPrefs()
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			string text = "";
			foreach (KeyControl item in Controls[i])
			{
				text = text + item.GetSerialString() + "|";
			}
			MappedControl mappedControl = (MappedControl)i;
			PlayerPrefs.SetString("ct_" + mappedControl, text);
		}
		PlayerPrefs.SetInt("hiddenInputVersion", 2);
	}

	public void LoadFromPrefs()
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			char[] separator = new char[1] { '|' };
			MappedControl mappedControl = (MappedControl)i;
			string[] array = PlayerPrefs.GetString("ct_" + mappedControl).Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				Controls[i] = new List<KeyControl>();
				string[] array2 = array;
				foreach (string serialString in array2)
				{
					Controls[i].Add(new KeyControl(serialString));
				}
			}
		}
		if (CurrentBindIsConflicted(MappedControl.PREVIOUS_COMPANION) != 0)
		{
			Controls[50].Clear();
		}
		if (CurrentBindIsConflicted(MappedControl.NEXT_COMPANION) != 0)
		{
			Controls[49].Clear();
		}
	}
}
