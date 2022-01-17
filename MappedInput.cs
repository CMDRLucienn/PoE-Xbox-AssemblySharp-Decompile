using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MappedInput
{
	public static GUIDatabaseString[] CategoryNames;

	private static GUIDatabaseString[] m_ControlNames;

	public static bool[] MouseForbidden;

	public static MappedControl[][] CategorizedControls;

	private static MappedControl[] ReadOnlyControls;

	public static MappedControl[][] OverlapAllowed;

	public static MappedControl[][] ExclusiveGroups;

	public static KeyCode[] Forbidden;

	public static MappedControl[] IgnoreModifiers;

	public static ControlMapping DefaultMapping;

	public static string GetControlName(MappedControl control)
	{
		return m_ControlNames[(int)control].GetText();
	}

	static MappedInput()
	{
		CategoryNames = new GUIDatabaseString[5]
		{
			new GUIDatabaseString(760),
			new GUIDatabaseString(168),
			new GUIDatabaseString(1472),
			new GUIDatabaseString(1473),
			new GUIDatabaseString(365)
		};
		m_ControlNames = new GUIDatabaseString[76]
		{
			new GUIDatabaseString(),
			new GUIDatabaseString(843),
			new GUIDatabaseString(844),
			new GUIDatabaseString(845),
			new GUIDatabaseString(846),
			new GUIDatabaseString(847),
			new GUIDatabaseString(848),
			new GUIDatabaseString(849),
			new GUIDatabaseString(850),
			new GUIDatabaseString(851),
			new GUIDatabaseString(852),
			new GUIDatabaseString(853),
			new GUIDatabaseString(854),
			new GUIDatabaseString(855),
			new GUIDatabaseString(856),
			new GUIDatabaseString(857),
			new GUIDatabaseString(858),
			new GUIDatabaseString(859),
			new GUIDatabaseString(869),
			new GUIDatabaseString(860),
			new GUIDatabaseString(861),
			new GUIDatabaseString(862),
			new GUIDatabaseString(863),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(864),
			new GUIDatabaseString(865),
			new GUIDatabaseString(866),
			new GUIDatabaseString(867),
			new GUIDatabaseString(868),
			new GUIDatabaseString(59),
			new GUIDatabaseString(870),
			new GUIDatabaseString(871),
			new GUIDatabaseString(872),
			new GUIDatabaseString(873),
			new GUIDatabaseString(874),
			new GUIDatabaseString(1556),
			new GUIDatabaseString(),
			new GUIDatabaseString(877),
			new GUIDatabaseString(878),
			new GUIDatabaseString(879),
			new GUIDatabaseString(880),
			new GUIDatabaseString(881),
			new GUIDatabaseString(882),
			new GUIDatabaseString(1758),
			new GUIDatabaseString(1529),
			new GUIDatabaseString(1530),
			new GUIDatabaseString(1527),
			new GUIDatabaseString(1528),
			new GUIDatabaseString(1576),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(),
			new GUIDatabaseString(1808),
			new GUIDatabaseString(1930),
			new GUIDatabaseString(2053),
			new GUIDatabaseString(2280),
			new GUIDatabaseString(2294),
			new GUIDatabaseString(2283),
			new GUIDatabaseString(2284),
			new GUIDatabaseString(2285),
			new GUIDatabaseString(2286),
			new GUIDatabaseString(2287),
			new GUIDatabaseString(2288),
			new GUIDatabaseString(2289),
			new GUIDatabaseString(843),
			new GUIDatabaseString(2281)
		};
		MouseForbidden = new bool[77];
		CategorizedControls = new MappedControl[5][]
		{
			new MappedControl[11]
			{
				MappedControl.SELECT,
				MappedControl.MOVE,
				MappedControl.INTERACT,
				MappedControl.ATTACK,
				MappedControl.ATTACK_CURSOR,
				MappedControl.CANCEL_ACTION,
				MappedControl.SELECT_ALL,
				MappedControl.ROTATE_FORMATION,
				MappedControl.STEALTH_TOGGLE,
				MappedControl.STEALTH_ON,
				MappedControl.STEALTH_OFF
			},
			new MappedControl[19]
			{
				MappedControl.CAMP,
				MappedControl.HIGHLIGHT_HOLD,
				MappedControl.HIGHLIGHT_TOGGLE,
				MappedControl.SLOW_TOGGLE,
				MappedControl.FAST_TOGGLE,
				MappedControl.RESTORE_SPEED,
				MappedControl.GAME_SPEED_CYCLE,
				MappedControl.PAUSE,
				MappedControl.QUICKSAVE,
				MappedControl.QUICKLOAD,
				MappedControl.PAN_CAMERA_LEFT,
				MappedControl.PAN_CAMERA_RIGHT,
				MappedControl.PAN_CAMERA_UP,
				MappedControl.PAN_CAMERA_DOWN,
				MappedControl.PAN_CAMERA,
				MappedControl.ZOOM_IN,
				MappedControl.ZOOM_OUT,
				MappedControl.RESET_ZOOM,
				MappedControl.FOLLOW_CAM
			},
			new MappedControl[10]
			{
				MappedControl.TOGGLE_HUD,
				MappedControl.NEXT_TAB,
				MappedControl.TOGGLE_CONSOLE,
				MappedControl.PREVIOUS_COMPANION,
				MappedControl.NEXT_COMPANION,
				MappedControl.NEXT_WEAPON_SET,
				MappedControl.TAKE_ALL,
				MappedControl.UI_UP,
				MappedControl.UI_DOWN,
				MappedControl.UI_SELECT
			},
			new MappedControl[13]
			{
				MappedControl.MENU_CYCLE_LEFT,
				MappedControl.MENU_CYCLE_RIGHT,
				MappedControl.CHARACTER_SHEET,
				MappedControl.FORMATIONS,
				MappedControl.INVENTORY,
				MappedControl.JOURNAL,
				MappedControl.PARTY_MANAGER,
				MappedControl.SAVE,
				MappedControl.LOAD,
				MappedControl.STRONGHOLD,
				MappedControl.AREAMAP,
				MappedControl.EDIT_SPELLS,
				MappedControl.OPTIONS
			},
			new MappedControl[5]
			{
				MappedControl.NEXT_ABILITY,
				MappedControl.PREVIOUS_ABILITY,
				MappedControl.UP_ABILITY,
				MappedControl.DOWN_ABILITY,
				MappedControl.CAST_SELECTED_ABILITY
			}
		};
		OverlapAllowed = new MappedControl[1][] { new MappedControl[4]
		{
			MappedControl.SELECT,
			MappedControl.MOVE,
			MappedControl.INTERACT,
			MappedControl.ATTACK
		} };
		ExclusiveGroups = new MappedControl[3][]
		{
			new MappedControl[8]
			{
				MappedControl.CONV_CONTINUE,
				MappedControl.CLOSE_WINDOW,
				MappedControl.MB_CONFIRM,
				MappedControl.MB_CANCEL,
				MappedControl.NEXT_TAB,
				MappedControl.UI_UP,
				MappedControl.UI_DOWN,
				MappedControl.UI_SELECT
			},
			new MappedControl[36]
			{
				MappedControl.SELECT,
				MappedControl.MULTISELECT,
				MappedControl.MULTISELECT_NEGATIVE,
				MappedControl.MOVE,
				MappedControl.INTERACT,
				MappedControl.ATTACK,
				MappedControl.ATTACK_CURSOR,
				MappedControl.CANCEL_ACTION,
				MappedControl.SELECT_ALL,
				MappedControl.ROTATE_FORMATION,
				MappedControl.STEALTH_TOGGLE,
				MappedControl.STEALTH_ON,
				MappedControl.STEALTH_OFF,
				MappedControl.HIGHLIGHT_HOLD,
				MappedControl.HIGHLIGHT_TOGGLE,
				MappedControl.SLOW_TOGGLE,
				MappedControl.FAST_TOGGLE,
				MappedControl.RESTORE_SPEED,
				MappedControl.PAUSE,
				MappedControl.TOGGLE_HUD,
				MappedControl.PAN_CAMERA_LEFT,
				MappedControl.PAN_CAMERA_RIGHT,
				MappedControl.PAN_CAMERA_UP,
				MappedControl.PAN_CAMERA_DOWN,
				MappedControl.PAN_CAMERA,
				MappedControl.QUEUE,
				MappedControl.ZOOM_IN,
				MappedControl.ZOOM_OUT,
				MappedControl.RESET_ZOOM,
				MappedControl.FOLLOW_CAM,
				MappedControl.GAME_SPEED_CYCLE,
				MappedControl.NEXT_ABILITY,
				MappedControl.PREVIOUS_ABILITY,
				MappedControl.UP_ABILITY,
				MappedControl.DOWN_ABILITY,
				MappedControl.CAST_SELECTED_ABILITY
			},
			new MappedControl[1] { MappedControl.TAKE_ALL }
		};
		Forbidden = new KeyCode[5]
		{
			KeyCode.SysReq,
			KeyCode.LeftWindows,
			KeyCode.RightWindows,
			KeyCode.LeftCommand,
			KeyCode.RightCommand
		};
		IgnoreModifiers = new MappedControl[5]
		{
			MappedControl.SELECT,
			MappedControl.MOVE,
			MappedControl.INTERACT,
			MappedControl.ATTACK,
			MappedControl.ROTATE_FORMATION
		};
		IEnumerable<MappedControl> enumerable = CategorizedControls[0];
		for (int i = 1; i < CategorizedControls.Length; i++)
		{
			enumerable = enumerable.Concat(CategorizedControls[i]);
		}
		ReadOnlyControls = Enum.GetValues(typeof(MappedControl)).Cast<MappedControl>().Except(enumerable)
			.ToArray();
		for (int j = 29; j <= 40; j++)
		{
			MouseForbidden[j] = true;
		}
		DefaultMapping = new ControlMapping();
		DefaultMapping.AddControl(MappedControl.SELECT, KeyCode.Mouse0);
		DefaultMapping.AddCtrlControl(MappedControl.MULTISELECT_NEGATIVE, KeyCode.None);
		DefaultMapping.AddShiftControl(MappedControl.MULTISELECT, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.MOVE, KeyCode.Mouse0);
		DefaultMapping.AddControl(MappedControl.INTERACT, KeyCode.Mouse0);
		DefaultMapping.AddControl(MappedControl.ATTACK, KeyCode.Mouse0);
		DefaultMapping.AddControl(MappedControl.ATTACK_CURSOR, KeyCode.A);
		DefaultMapping.AddControl(MappedControl.CANCEL_ACTION, KeyCode.X);
		DefaultMapping.AddControl(MappedControl.SELECT_ALL, KeyCode.Backspace);
		DefaultMapping.AddControl(MappedControl.ROTATE_FORMATION, KeyCode.Mouse1);
		DefaultMapping.AddControl(MappedControl.STEALTH_TOGGLE, KeyCode.LeftAlt, KeyCode.RightAlt);
		DefaultMapping.AddControl(MappedControl.STEALTH_ON, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.STEALTH_OFF, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.CAMP, KeyCode.None);
		DefaultMapping.AddShiftControl(MappedControl.QUEUE, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.HIGHLIGHT_HOLD, KeyCode.Tab);
		DefaultMapping.AddControl(MappedControl.HIGHLIGHT_TOGGLE, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.SLOW_TOGGLE, KeyCode.S);
		DefaultMapping.AddControl(MappedControl.FAST_TOGGLE, KeyCode.D);
		DefaultMapping.AddControl(MappedControl.GAME_SPEED_CYCLE, KeyCode.Keypad0);
		DefaultMapping.AddControl(MappedControl.RESTORE_SPEED, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.PAUSE, KeyCode.Space);
		DefaultMapping.AddControl(MappedControl.QUICKSAVE, KeyCode.F5);
		DefaultMapping.AddControl(MappedControl.QUICKLOAD, KeyCode.F8);
		DefaultMapping.AddCtrlControl(MappedControl.TOGGLE_HUD, KeyCode.H);
		DefaultMapping.AddControl(MappedControl.CONV_CONTINUE, KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter);
		DefaultMapping.AddControl(MappedControl.CLOSE_WINDOW, KeyCode.Escape);
		DefaultMapping.AddControl(MappedControl.MB_CONFIRM, KeyCode.Return, KeyCode.Space);
		DefaultMapping.AddControl(MappedControl.MB_CANCEL, KeyCode.Escape);
		DefaultMapping.AddControl(MappedControl.NEXT_TAB, KeyCode.Tab);
		DefaultMapping.AddControl(MappedControl.TOGGLE_CONSOLE, KeyCode.BackQuote);
		DefaultMapping.AddControl(MappedControl.TAKE_ALL, KeyCode.E);
		DefaultMapping.AddControl(MappedControl.TAKE_ALL, KeyCode.Return);
		DefaultMapping.AddControl(MappedControl.CHARACTER_SHEET, KeyCode.C);
		DefaultMapping.AddControl(MappedControl.FORMATIONS, KeyCode.F);
		DefaultMapping.AddControl(MappedControl.INVENTORY, KeyCode.I);
		DefaultMapping.AddControl(MappedControl.JOURNAL, KeyCode.J);
		DefaultMapping.AddControl(MappedControl.PARTY_MANAGER, KeyCode.P);
		DefaultMapping.AddControl(MappedControl.SAVE, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.LOAD, KeyCode.None);
		DefaultMapping.AddControl(MappedControl.STRONGHOLD, KeyCode.H);
		DefaultMapping.AddControl(MappedControl.AREAMAP, KeyCode.M);
		DefaultMapping.AddControl(MappedControl.EDIT_SPELLS, KeyCode.G);
		DefaultMapping.AddControl(MappedControl.OPTIONS, KeyCode.Escape);
		DefaultMapping.AddControl(MappedControl.PAN_CAMERA_LEFT, KeyCode.LeftArrow);
		DefaultMapping.AddControl(MappedControl.PAN_CAMERA_RIGHT, KeyCode.RightArrow);
		DefaultMapping.AddControl(MappedControl.PAN_CAMERA_UP, KeyCode.UpArrow);
		DefaultMapping.AddControl(MappedControl.PAN_CAMERA_DOWN, KeyCode.DownArrow);
		DefaultMapping.AddControl(MappedControl.PAN_CAMERA, KeyCode.Mouse2);
		DefaultMapping.AddControl(MappedControl.ZOOM_IN, KeyCode.Equals);
		DefaultMapping.AddControl(MappedControl.ZOOM_OUT, KeyCode.Minus);
		DefaultMapping.AddControl(MappedControl.NEXT_COMPANION, KeyCode.RightBracket);
		DefaultMapping.AddControl(MappedControl.PREVIOUS_COMPANION, KeyCode.LeftBracket);
		DefaultMapping.AddControl(MappedControl.FOLLOW_CAM, KeyCode.Period);
		DefaultMapping.AddControl(MappedControl.NEXT_ABILITY, KeyCode.KeypadMultiply);
		DefaultMapping.AddControl(MappedControl.PREVIOUS_ABILITY, KeyCode.Delete);
		DefaultMapping.AddControl(MappedControl.UP_ABILITY, KeyCode.KeypadMinus);
		DefaultMapping.AddControl(MappedControl.DOWN_ABILITY, KeyCode.KeypadPlus);
		DefaultMapping.AddControl(MappedControl.CAST_SELECTED_ABILITY, KeyCode.Return);
		DefaultMapping.AddControl(MappedControl.MENU_CYCLE_LEFT, KeyCode.Semicolon);
		DefaultMapping.AddControl(MappedControl.MENU_CYCLE_RIGHT, KeyCode.Quote);
		DefaultMapping.AddControl(MappedControl.UI_UP, KeyCode.KeypadMinus);
		DefaultMapping.AddControl(MappedControl.UI_DOWN, KeyCode.KeypadPlus);
		DefaultMapping.AddControl(MappedControl.UI_SELECT, KeyCode.Return);
	}

	public static bool IsOverlapAllowed(MappedControl a, MappedControl b)
	{
		if (ReadOnlyControls.Contains(a) || ReadOnlyControls.Contains(b))
		{
			return true;
		}
		for (int i = 0; i < OverlapAllowed.Length; i++)
		{
			if (OverlapAllowed[i].Contains(a) && OverlapAllowed[i].Contains(b))
			{
				return true;
			}
		}
		for (int j = 0; j < ExclusiveGroups.Length; j++)
		{
			for (int k = j + 1; k < ExclusiveGroups.Length; k++)
			{
				if (ExclusiveGroups[j].Contains(a) && ExclusiveGroups[k].Contains(b))
				{
					return true;
				}
				if (ExclusiveGroups[j].Contains(b) && ExclusiveGroups[k].Contains(a))
				{
					return true;
				}
			}
		}
		return false;
	}
}
