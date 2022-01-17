using System;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
	public delegate void HandleInput();

	public float KeyRepeatDelay = 0.25f;

	public float KeyRepeatRate = 15f;

	public float HoldTime = 0.08f;

	protected static bool[] s_handledUp;

	protected static bool[] s_handledDown;

	protected static float[] s_holdTimer;

	protected static float[] s_holdRepeatTimer;

	private static Dictionary<int, int> s_keyMap;

	public float DoubleClickTime = 0.25f;

	public float DoublePressTime = 0.25f;

	protected static float s_doublePressTimer;

	protected static Vector2 s_doublePressPosition;

	protected static KeyCode s_doublePressKey;

	private static Vector3 s_lastMouse;

	private static bool s_leftIsRight;

	private static int s_NumberPressedKeypad;

	private static int s_NumberPressedAlpha;

	protected static Vector3 s_pickLocation;

	private static Rigidbody s_hitObj;

	public static bool DisableInput;

	public static bool SelectDead;

	private static bool s_cancelTargetedAttack;

	private static bool s_targetedAttackActive;

	public KeyControl LastKeyUp;

	public static GameInput Instance { get; private set; }

	public float KeyRepeatTime => 1f / KeyRepeatRate;

	public static bool BlockAllKeys { get; protected set; }

	public static int NumberPressed
	{
		get
		{
			if (s_NumberPressedAlpha < 0)
			{
				return s_NumberPressedKeypad;
			}
			return s_NumberPressedAlpha;
		}
	}

	public static int AlphaNumberPressed => s_NumberPressedAlpha;

	public static int KeypadNumberPressed => s_NumberPressedKeypad;

	public static Vector3 MousePosition => Input.mousePosition;

	public static Vector3 MouseDelta => Input.mousePosition - s_lastMouse;

	public static Vector3 WorldMousePosition => s_pickLocation;

	public event HandleInput OnHandleInput;

	static GameInput()
	{
		s_keyMap = new Dictionary<int, int>();
		s_leftIsRight = false;
		s_NumberPressedKeypad = -1;
		s_NumberPressedAlpha = -1;
		s_pickLocation = Vector3.zero;
		s_hitObj = null;
		DisableInput = false;
		SelectDead = false;
		s_targetedAttackActive = false;
		Array values = Enum.GetValues(typeof(KeyCode));
		s_handledUp = new bool[values.Length];
		s_handledDown = new bool[values.Length];
		s_holdTimer = new float[values.Length];
		s_holdRepeatTimer = new float[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			if (!s_keyMap.ContainsKey((int)values.GetValue(i)))
			{
				s_keyMap.Add((int)values.GetValue(i), i);
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		_ = s_leftIsRight;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.KeyUp || Event.current.type == EventType.MouseUp)
		{
			LastKeyUp.KeyCode = Event.current.keyCode;
			if (Event.current.type == EventType.MouseUp)
			{
				LastKeyUp.KeyCode = (KeyCode)(323 + Event.current.button);
			}
			LastKeyUp.AltKey = Event.current.alt;
			LastKeyUp.CtrlKey = Event.current.control;
			LastKeyUp.ShiftKey = Event.current.shift;
		}
	}

	private void Update()
	{
		if (Camera.main == null)
		{
			return;
		}
		if (BlockAllKeys)
		{
			HandleAllKeys();
		}
		if (s_targetedAttackActive)
		{
			s_cancelTargetedAttack = GetKeyDown(KeyCode.Escape, setHandled: true);
		}
		s_NumberPressedAlpha = -1;
		s_NumberPressedKeypad = -1;
		for (KeyCode keyCode = KeyCode.Alpha0; keyCode <= KeyCode.Alpha9; keyCode++)
		{
			if (Input.GetKeyDown(keyCode))
			{
				s_NumberPressedAlpha = (int)(keyCode - 48);
			}
		}
		for (KeyCode keyCode2 = KeyCode.Keypad0; keyCode2 <= KeyCode.Keypad9; keyCode2++)
		{
			if (Input.GetKeyDown(keyCode2))
			{
				s_NumberPressedKeypad = (int)(keyCode2 - 256);
			}
		}
		for (KeyCode keyCode3 = KeyCode.Mouse0; keyCode3 <= KeyCode.Mouse6; keyCode3++)
		{
			if (Input.GetMouseButtonUp((int)(keyCode3 - 323)))
			{
				LastKeyUp.KeyCode = keyCode3;
				LastKeyUp.AltKey = GetAltkey();
				LastKeyUp.ShiftKey = GetShiftkey();
				LastKeyUp.CtrlKey = GetControlkey();
			}
		}
		Vector3 mousePosition = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(mousePosition);
		int layerMask = 1 << LayerMask.NameToLayer("Walkable");
		s_hitObj = null;
		float enter;
		if (Physics.Raycast(ray, out var hitInfo, 1000f, layerMask))
		{
			if (hitInfo.rigidbody != null)
			{
				s_pickLocation = hitInfo.rigidbody.transform.position;
				s_hitObj = hitInfo.rigidbody;
			}
			else
			{
				s_pickLocation = hitInfo.point;
				s_hitObj = null;
			}
		}
		else if (FogOfWar.Instance != null && new Plane(Vector3.up, FogOfWar.Instance.GetBackgroundQuadOrigin()).Raycast(ray, out enter))
		{
			s_pickLocation = ray.origin + ray.direction * enter;
		}
		GameCursor.UiObjectUnderCursor = null;
		if (UICamera.Raycast(mousePosition, out var hit))
		{
			GameCursor.UiObjectUnderCursor = hit.collider.gameObject;
		}
		GameCursor.CharacterUnderCursor = null;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (faction == null || (!SelectDead && !faction.DrawSelectionCircle))
			{
				continue;
			}
			float num = 0.5f;
			Mover component = faction.GetComponent<Mover>();
			if ((bool)component)
			{
				num = component.Radius;
			}
			Plane plane = new Plane(Vector3.up, faction.gameObject.transform.position);
			Vector3 vector = s_pickLocation;
			if (plane.Raycast(ray, out var enter2))
			{
				vector = ray.origin + ray.direction * enter2;
			}
			Vector3 vector2 = faction.gameObject.transform.position - vector;
			Health component2 = faction.gameObject.GetComponent<Health>();
			bool flag = true;
			if (SelectDead)
			{
				flag = component2 != null && component2.ShowDead;
			}
			else if ((bool)component2)
			{
				flag = !component2.ShowDead;
			}
			if (vector2.sqrMagnitude < float.Epsilon && (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(faction.gameObject.transform.position)) && flag && s_hitObj != null)
			{
				AIController component3 = faction.gameObject.GetComponent<AIController>();
				if (component3 == null || !component3.IsInvisible || component3 is PartyMemberAI)
				{
					GameCursor.CharacterUnderCursor = faction.gameObject;
					break;
				}
			}
			vector2.Normalize();
			Vector3 vector3 = Camera.main.WorldToScreenPoint(faction.transform.position);
			vector3.z = 0f;
			Vector3 vector4 = Camera.main.WorldToScreenPoint(faction.transform.position + vector2 * num);
			vector4.z = 0f;
			float sqrMagnitude = (vector4 - vector3).sqrMagnitude;
			if ((vector3 - mousePosition).sqrMagnitude < sqrMagnitude && (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(faction.gameObject.transform.position)) && flag)
			{
				AIController component4 = faction.gameObject.GetComponent<AIController>();
				if (component4 == null || !component4.IsInvisible || component4 is PartyMemberAI)
				{
					GameCursor.CharacterUnderCursor = faction.gameObject;
					break;
				}
			}
		}
		GameCursor.WorldPickPosition = s_pickLocation;
		if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftCommand))
		{
			s_leftIsRight = true;
		}
		Dictionary<int, int>.Enumerator enumerator = s_keyMap.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int value = enumerator.Current.Value;
				if (GetKey((KeyCode)enumerator.Current.Key))
				{
					s_holdTimer[value] += Time.unscaledDeltaTime;
					if (s_holdTimer[value] >= KeyRepeatDelay)
					{
						if (s_holdRepeatTimer[value] >= KeyRepeatTime)
						{
							s_holdRepeatTimer[value] = 0f;
						}
						s_holdRepeatTimer[value] += Time.unscaledDeltaTime;
					}
				}
				else
				{
					s_holdRepeatTimer[value] = 0f;
					s_holdTimer[value] = 0f;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		if (s_doublePressTimer > 0f)
		{
			s_doublePressTimer -= Time.unscaledDeltaTime;
			if (s_doublePressTimer < 0f)
			{
				s_doublePressTimer = 0f;
			}
		}
		if (LastKeyUp.KeyCode != 0)
		{
			if (s_doublePressTimer > 0f && LastKeyUp.KeyCode == s_doublePressKey)
			{
				s_doublePressTimer = -1f;
			}
			else
			{
				s_doublePressTimer = DoubleClickTime;
				s_doublePressPosition = MousePosition;
				s_doublePressKey = LastKeyUp.KeyCode;
			}
		}
		if (this.OnHandleInput != null)
		{
			this.OnHandleInput();
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < s_handledUp.Length; i++)
		{
			s_handledUp[i] = false;
		}
		for (int j = 0; j < s_handledDown.Length; j++)
		{
			s_handledDown[j] = false;
		}
		if (s_doublePressTimer < 0f)
		{
			s_doublePressTimer = 0f;
		}
		s_lastMouse = Input.mousePosition;
		if (s_cancelTargetedAttack)
		{
			s_targetedAttackActive = false;
		}
		s_cancelTargetedAttack = false;
		LastKeyUp.KeyCode = KeyCode.None;
		LastKeyUp.ShiftKey = (LastKeyUp.CtrlKey = (LastKeyUp.AltKey = false));
	}

	public static void StartTargetedAttack()
	{
		s_targetedAttackActive = true;
	}

	public static void EndTargetedAttack()
	{
		s_targetedAttackActive = false;
	}

	public static bool GetCancelTargetedAttack()
	{
		return s_cancelTargetedAttack;
	}

	public static bool GetControlUp(MappedControl control)
	{
		return GetControlUp(control, handle: false);
	}

	public static bool GetControlDown(MappedControl control)
	{
		return GetControlDown(control, handle: false);
	}

	public static bool GetControlUpWithoutModifiers(MappedControl control)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlUpWithoutModifiers(list[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlDownWithoutModifiers(MappedControl control)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlDownWithoutModifiers(list[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlDownWithoutModifiersWithRepeat(MappedControl control)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlDownWithoutModifiersWithRepeat(list[i], handle: false))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlUp(MappedControl control, bool handle)
	{
		for (int num = MappedInput.IgnoreModifiers.Length - 1; num >= 0; num--)
		{
			if (MappedInput.IgnoreModifiers[num] == control)
			{
				return GetControlUpWithoutModifiers(control);
			}
		}
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlUp(list[i], handle))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlDown(MappedControl control, bool handle)
	{
		for (int num = MappedInput.IgnoreModifiers.Length - 1; num >= 0; num--)
		{
			if (MappedInput.IgnoreModifiers[num] == control)
			{
				return GetControlDownWithoutModifiers(control);
			}
		}
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlDown(list[i], handle))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlHeld(MappedControl control)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlHeld(list[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlDownWithRepeat(MappedControl control, bool handle)
	{
		for (int num = MappedInput.IgnoreModifiers.Length - 1; num >= 0; num--)
		{
			if (MappedInput.IgnoreModifiers[num] == control)
			{
				return GetControlDownWithoutModifiers(control);
			}
		}
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControlDownWithRepeat(list[i], handle))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControl(MappedControl control)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControl(list[i], ignoreHandle: false, ignoreModifiers: false))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControl(MappedControl control, bool ignoreHandle, bool ignoreModifiers)
	{
		List<KeyControl> list = GameState.Controls.Controls[(int)control];
		for (int i = 0; i < list.Count; i++)
		{
			if (GetControl(list[i], ignoreHandle, ignoreModifiers))
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetControlUp(KeyControl control, bool handle)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledUp[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		if (Input.GetKeyUp(control.KeyCode) && GetModifiersValid(control))
		{
			if (handle)
			{
				s_handledUp[s_keyMap[(int)control.KeyCode]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetControlDown(KeyControl control, bool handle)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledDown[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		if (Input.GetKeyDown(control.KeyCode) && GetModifiersValid(control))
		{
			if (handle)
			{
				s_handledDown[s_keyMap[(int)control.KeyCode]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetControlHeld(KeyControl control)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_holdTimer[s_keyMap[(int)control.KeyCode]] >= Instance.HoldTime && GetModifiersValid(control))
		{
			return true;
		}
		return false;
	}

	public static bool GetControlDownWithRepeat(KeyControl control, bool handle)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledDown[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		bool flag = s_holdRepeatTimer[s_keyMap[(int)control.KeyCode]] >= Instance.KeyRepeatTime;
		if ((Input.GetKeyDown(control.KeyCode) || flag) && GetModifiersValid(control))
		{
			if (handle)
			{
				s_handledDown[s_keyMap[(int)control.KeyCode]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetControlUpWithoutModifiers(KeyControl control)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledUp[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		return Input.GetKeyUp(control.KeyCode);
	}

	public static bool GetControlDownWithoutModifiers(KeyControl control)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledDown[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		return Input.GetKeyDown(control.KeyCode);
	}

	public static bool GetControlDownWithoutModifiersWithRepeat(KeyControl control, bool handle)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledDown[s_keyMap[(int)control.KeyCode]])
		{
			return false;
		}
		if (!Input.GetKeyDown(control.KeyCode))
		{
			return s_holdRepeatTimer[s_keyMap[(int)control.KeyCode]] >= Instance.KeyRepeatTime;
		}
		return true;
	}

	public static bool GetControl(KeyControl control, bool ignoreHandle, bool ignoreModifiers)
	{
		if (DisableInput)
		{
			return false;
		}
		bool flag = s_handledUp[s_keyMap[(int)control.KeyCode]] || s_handledDown[s_keyMap[(int)control.KeyCode]];
		if (!ignoreHandle && s_keyMap.Count > 0 && flag)
		{
			return false;
		}
		if (control.KeyCode == KeyCode.None && control.HasModifiers())
		{
			return GetModifiersValid(control);
		}
		if (Input.GetKey(control.KeyCode))
		{
			if (!ignoreModifiers)
			{
				return GetModifiersValid(control);
			}
			return true;
		}
		return false;
	}

	private static bool GetModifiersValid(KeyControl control)
	{
		if (control.ShiftKey == GetShiftkey() && control.CtrlKey == GetControlkey())
		{
			return control.AltKey == GetAltkey();
		}
		return false;
	}

	public static bool GetDoublePressed(KeyCode key, bool handle)
	{
		if (DisableInput)
		{
			return false;
		}
		bool flag = key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
		if (flag && (s_doublePressPosition - (Vector2)MousePosition).magnitude > 10f)
		{
			return false;
		}
		if (s_doublePressKey == key && s_doublePressTimer < 0f)
		{
			if (handle)
			{
				s_doublePressTimer = 0f;
				if (flag)
				{
					HandleAllClicks();
				}
			}
			return true;
		}
		return false;
	}

	public static bool GetHeld(KeyCode key)
	{
		if (DisableInput)
		{
			return false;
		}
		return s_holdTimer[s_keyMap[(int)key]] >= Instance.HoldTime;
	}

	public static bool GetAnyMouseButtonUp(bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		for (KeyCode keyCode = KeyCode.Mouse0; keyCode <= KeyCode.Mouse6; keyCode++)
		{
			if (!s_handledUp[s_keyMap[(int)keyCode]] && Input.GetMouseButtonUp((int)(keyCode - 323)))
			{
				if (setHandled)
				{
					s_handledUp[s_keyMap[(int)keyCode]] = true;
				}
				return true;
			}
		}
		return false;
	}

	public static bool GetAnyMouseButtonDown(bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		for (KeyCode keyCode = KeyCode.Mouse0; keyCode <= KeyCode.Mouse6; keyCode++)
		{
			if (!s_handledUp[s_keyMap[(int)keyCode]] && Input.GetMouseButtonDown((int)(keyCode - 323)))
			{
				if (setHandled)
				{
					s_handledUp[s_keyMap[(int)keyCode]] = true;
				}
				return true;
			}
		}
		return false;
	}

	public static bool GetMouseButtonUp(int button, bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_handledUp[s_keyMap[323 + button]])
		{
			return false;
		}
		if (Input.GetMouseButtonUp(button))
		{
			if (setHandled)
			{
				s_handledUp[s_keyMap[323 + button]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetMouseButtonDown(int button, bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_handledDown[s_keyMap[323 + button]])
		{
			return false;
		}
		if (Input.GetMouseButtonDown(button))
		{
			if (setHandled)
			{
				s_handledDown[s_keyMap[323 + button]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetMouseButton(int button, bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		return Input.GetMouseButton(button);
	}

	public static bool GetKeyUp(KeyCode key)
	{
		return GetKeyUp(key, setHandled: false);
	}

	public static bool GetKeyUp(KeyCode key, bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledUp[s_keyMap[(int)key]])
		{
			return false;
		}
		if (Input.GetKeyUp(key))
		{
			if (setHandled)
			{
				s_handledUp[s_keyMap[(int)key]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetKeyDown(KeyCode key)
	{
		return GetKeyDown(key, setHandled: false);
	}

	public static bool GetKeyDown(KeyCode key, bool setHandled)
	{
		if (DisableInput)
		{
			return false;
		}
		if (s_keyMap.Count > 0 && s_handledDown[s_keyMap[(int)key]])
		{
			return false;
		}
		if (Input.GetKeyDown(key))
		{
			if (setHandled)
			{
				s_handledDown[s_keyMap[(int)key]] = true;
			}
			return true;
		}
		return false;
	}

	public static bool GetKey(KeyCode key)
	{
		return Input.GetKey(key);
	}

	public static bool GetControlkey()
	{
		if (!Input.GetKey(KeyCode.LeftControl))
		{
			return Input.GetKey(KeyCode.RightControl);
		}
		return true;
	}

	public static bool GetShiftkey()
	{
		if (!Input.GetKey(KeyCode.LeftShift))
		{
			return Input.GetKey(KeyCode.RightShift);
		}
		return true;
	}

	public static bool GetAltkey()
	{
		if (!Input.GetKey(KeyCode.LeftAlt))
		{
			return Input.GetKey(KeyCode.RightAlt);
		}
		return true;
	}

	public static bool ControlIsBound(KeyControl control)
	{
		List<KeyControl>[] controls = GameState.Controls.Controls;
		for (int i = 0; i < controls.Length; i++)
		{
			foreach (KeyControl item in controls[i])
			{
				if (item.Equals(control))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void HandleAllKeys()
	{
		for (int i = 0; i < s_handledUp.Length; i++)
		{
			s_handledUp[i] = true;
		}
		for (int j = 0; j < s_handledDown.Length; j++)
		{
			s_handledDown[j] = true;
		}
	}

	public static void BeginBlockAllKeys()
	{
		BlockAllKeys = true;
	}

	public static void EndBlockAllKeys()
	{
		BlockAllKeys = false;
	}

	public static void HandleAllKeysExcept(MappedControl control)
	{
		for (int i = 0; i < s_handledUp.Length; i++)
		{
			bool flag = true;
			List<KeyControl> list = GameState.Controls.Controls[(int)control];
			for (int j = 0; j < list.Count; j++)
			{
				if (i == s_keyMap[(int)list[j].KeyCode])
				{
					flag = false;
				}
			}
			if (flag)
			{
				s_handledUp[i] = true;
				s_handledDown[i] = true;
			}
		}
	}

	public static void HandleAllKeysExcept(KeyCode key)
	{
		for (int i = 0; i < s_handledUp.Length; i++)
		{
			if (i != s_keyMap[(int)key])
			{
				s_handledUp[i] = true;
				s_handledDown[i] = true;
			}
		}
	}

	public static void HandleAllKeysExcept(KeyCode key1, KeyCode key2)
	{
		for (int i = 0; i < s_handledUp.Length; i++)
		{
			if (i != s_keyMap[(int)key1] && i != s_keyMap[(int)key2])
			{
				s_handledUp[i] = true;
				s_handledDown[i] = true;
			}
		}
	}

	public static void HandleAllClicks()
	{
		for (KeyCode keyCode = KeyCode.Mouse0; keyCode <= KeyCode.Mouse6; keyCode++)
		{
			s_handledUp[s_keyMap[(int)keyCode]] = true;
			s_handledDown[s_keyMap[(int)keyCode]] = true;
		}
	}

	public static bool LmbAvailable()
	{
		return !s_handledUp[s_keyMap[323]];
	}

	private void OnDrawGizmos()
	{
		if (!(s_hitObj == null) && !(s_hitObj.GetComponent<Collider>() == null))
		{
			Gizmos.color = Color.magenta;
			if (s_hitObj.GetComponent<Collider>() is BoxCollider)
			{
				Gizmos.DrawSphere(s_hitObj.transform.position, 0.2f);
			}
			else if (s_hitObj.GetComponent<Collider>() is SphereCollider)
			{
				Gizmos.DrawWireSphere(s_hitObj.transform.position, (s_hitObj.GetComponent<Collider>() as SphereCollider).radius);
			}
		}
	}
}
