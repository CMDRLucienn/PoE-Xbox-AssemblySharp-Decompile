using System.Collections.Generic;
using UnityEngine;

public class CharacterHotkeyBindings : MonoBehaviour
{
	public delegate void HotkeyChanged(KeyControl hotkey, GenericAbility newtarget);

	private const int MASTERED_SPELL_KEY_OFFSET = 1000000;

	[Persistent]
	public Dictionary<KeyControl, int> AbilityHotkeys = new Dictionary<KeyControl, int>();

	public event HotkeyChanged OnHotkeyChanged;

	private void OnDestroy()
	{
		this.OnHotkeyChanged = null;
	}

	public static int GetDictionaryKey(GenericAbility ability)
	{
		if (!ability)
		{
			return 0;
		}
		return ability.DisplayName.StringID + ability.MasteryLevel * 1000000;
	}

	public void BindHotkey(KeyControl hotkey, GenericAbility ability)
	{
		if (ability == null)
		{
			return;
		}
		if (AbilityHotkeys == null)
		{
			AbilityHotkeys = new Dictionary<KeyControl, int>();
		}
		if ((hotkey.KeyCode >= KeyCode.Alpha0 && hotkey.KeyCode <= KeyCode.Alpha9) || (hotkey.KeyCode >= KeyCode.Mouse0 && hotkey.KeyCode <= KeyCode.Mouse1))
		{
			return;
		}
		try
		{
			bool flag = false;
			if (GameInput.ControlIsBound(hotkey))
			{
				if (AbilityHotkeys.ContainsKey(hotkey))
				{
					AbilityHotkeys.Remove(hotkey);
				}
				return;
			}
			List<KeyControl> list = new List<KeyControl>();
			foreach (KeyValuePair<KeyControl, int> abilityHotkey in AbilityHotkeys)
			{
				if (abilityHotkey.Value == GetDictionaryKey(ability))
				{
					list.Add(abilityHotkey.Key);
					if (abilityHotkey.Key.KeyCode == hotkey.KeyCode)
					{
						flag = true;
					}
				}
			}
			foreach (KeyControl item in list)
			{
				AbilityHotkeys.Remove(item);
			}
			if (!flag)
			{
				AbilityHotkeys[hotkey] = GetDictionaryKey(ability);
			}
		}
		finally
		{
			if (this.OnHotkeyChanged != null)
			{
				this.OnHotkeyChanged(hotkey, ability);
			}
		}
	}

	public KeyControl GetHotkeyFor(GenericAbility ability)
	{
		if ((bool)ability && AbilityHotkeys != null)
		{
			foreach (KeyValuePair<KeyControl, int> abilityHotkey in AbilityHotkeys)
			{
				if (abilityHotkey.Value == GetDictionaryKey(ability))
				{
					return abilityHotkey.Key;
				}
			}
		}
		return default(KeyControl);
	}

	public void Activate(KeyControl kc)
	{
		if (AbilityHotkeys == null || !AbilityHotkeys.ContainsKey(kc))
		{
			return;
		}
		CharacterStats component = GetComponent<CharacterStats>();
		if (!component)
		{
			return;
		}
		int num = AbilityHotkeys[kc];
		foreach (GenericAbility activeAbility in component.ActiveAbilities)
		{
			if ((bool)activeAbility && GetDictionaryKey(activeAbility) == num && activeAbility.ReadyForUI)
			{
				activeAbility.TriggerFromUI();
				break;
			}
		}
	}

	public static CharacterHotkeyBindings Get(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		CharacterHotkeyBindings component = go.GetComponent<CharacterHotkeyBindings>();
		if ((bool)component)
		{
			return component;
		}
		return go.AddComponent<CharacterHotkeyBindings>();
	}
}
