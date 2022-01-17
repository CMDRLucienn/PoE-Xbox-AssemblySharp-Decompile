using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCursor : MonoBehaviour
{
	public enum CursorType
	{
		None,
		Normal,
		Walk,
		NoWalk,
		RotateFormation,
		Attack,
		OpenDoor,
		CloseDoor,
		LockedDoor,
		AreaTransition,
		Examine,
		Talk,
		Interact,
		CastAbility,
		CastAbilityInvalid,
		ArrowUp,
		ArrowRight,
		ArrowDown,
		ArrowLeft,
		ArrowUpRight,
		ArrowDownRight,
		ArrowUpLeft,
		ArrowDownLeft,
		Deprecated1,
		DoubleArrow_L_R,
		DoubleArrow_U_D,
		DoubleArrow_DL_UR,
		DoubleArrow_UL_DR,
		Disarm,
		CastAbilityNoLOS,
		CastAbilityFar,
		Stealing,
		Loot,
		NormalHeld,
		WalkHeld,
		SpecialAttack,
		StealingLocked,
		Disengage,
		DisengageHeld,
		AttackAdvantage,
		SelectionAdd,
		SelectionSubtract,
		SelectionAddHeld,
		SelectionSubtractHeld,
		DuplicateItem
	}

	[Serializable]
	public class CursorSet
	{
		public Texture2D CastAbilityFrame;

		public Texture2D CastAbilityInvalidFrame;

		public Texture2D CastAbilityMoveFrame;

		public Texture2D CastAbilityNoLosFrame;

		public Texture2D CastAbilityIconMask;

		public Texture2D InvisibleCursor;

		public Texture2D NormalCursor;

		public Texture2D NormalHeldCursor;

		public Texture2D WalkCursor;

		public Texture2D WalkHeldCursor;

		public Texture2D DisengageCursor;

		public Texture2D DisengageHeldCursor;

		public Texture2D NoWalkCursor;

		public Texture2D RotateFormationCursor;

		public Texture2D AttackCursor;

		public Texture2D SpecialAttackCursor;

		public Texture2D OpenDoorCursor;

		public Texture2D CloseDoorCursor;

		public Texture2D LockedDoorCursor;

		public Texture2D AreaTransition;

		public Texture2D ExamineCursor;

		public Texture2D TalkCursor;

		public Texture2D InteractCursor;

		public Texture2D CastAbility;

		public Texture2D CastAbilityInvalid;

		public Texture2D ArrowUp;

		public Texture2D ArrowRight;

		public Texture2D ArrowDown;

		public Texture2D ArrowLeft;

		public Texture2D ArrowUpRight;

		public Texture2D ArrowDownRight;

		public Texture2D ArrowUpLeft;

		public Texture2D ArrowDownLeft;

		public Texture2D DoubleArrow_L_R;

		public Texture2D DoubleArrow_U_D;

		public Texture2D DoubleArrow_DL_UR;

		public Texture2D DoubleArrow_UL_DR;

		public Texture2D DisarmCursor;

		public Texture2D CastAbilityNoLOS;

		public Texture2D CastAbilityFar;

		public Texture2D Stealing;

		public Texture2D StealingLocked;

		public Texture2D LootCursor;

		public Texture2D AttackAdvantageCursor;

		public Texture2D SelectionAddCursor;

		public Texture2D SelectionSubtractCursor;

		public Texture2D SelectionAddHeldCursor;

		public Texture2D SelectionSubtractHeldCursor;

		public Texture2D DuplicateItemCursor;

		public Color[] CastAbilityFrameData { get; set; }

		public Color[] CastAbilityInvalidFrameData { get; set; }

		public Color[] CastAbilityMoveFrameData { get; set; }

		public Color[] CastAbilityNoLosFrameData { get; set; }

		public Color[] CastAbilityIconMaskData { get; set; }
	}

	public CursorSet PrefabCursors;

	private static CursorType m_activeCursor = CursorType.None;

	private static CursorType m_desiredCursor = CursorType.None;

	private static CursorType m_uiCursor = CursorType.None;

	private static Texture2D m_castCursor = null;

	private static Texture2D m_castCursorInvalid = null;

	private static Texture2D m_castCursorMove = null;

	private static Texture2D m_castCursorLoS = null;

	private static bool m_castCursorValid = false;

	private static Color[] m_castCursorBuffer;

	private static int m_frameCount = 0;

	private static GameObject m_genericUnderCursor = null;

	private static GameObject m_characterUnderCursor = null;

	private static PE_Collider2D m_colliderUnderCursor = null;

	private static Vector2 m_centerHotSpot = new Vector2(16f, 16f);

	private List<WeakReference> m_ObjectsHidingCursor = new List<WeakReference>();

	private bool m_ShowCursor = true;

	private static bool m_showDebug = false;

	public static GameObject UiObjectUnderCursor;

	public static GameObject OverrideCharacterUnderCursor;

	public static GameCursor Instance { get; private set; }

	public static CursorType CursorOverride { get; set; }

	public bool DisableCursor { get; private set; }

	public bool ShowCursor
	{
		get
		{
			return m_ShowCursor;
		}
		private set
		{
			m_ShowCursor = value;
			DisableCursor = !value;
			if (!m_ShowCursor)
			{
				Cursor.SetCursor(PrefabCursors.InvisibleCursor, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				m_activeCursor = CursorType.None;
			}
		}
	}

	public static bool ShowDebug
	{
		get
		{
			return m_showDebug;
		}
		set
		{
			m_showDebug = value;
			if (!m_showDebug)
			{
				UIDebug.Instance.RemoveText("Cursor Debug");
			}
		}
	}

	public static bool LockCursor
	{
		get
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				return true;
			}
			return false;
		}
		set
		{
			Cursor.lockState = (value ? CursorLockMode.Locked : CursorLockMode.None);
			WinCursor.Clip(state: true);
		}
	}

	public static bool ActiveCursorIsTargeting => IsTargetCursor(ActiveCursor);

	public static CursorType ActiveCursor => m_activeCursor;

	public static CursorType UiCursor
	{
		get
		{
			return m_uiCursor;
		}
		set
		{
			m_uiCursor = value;
		}
	}

	public static CursorType DesiredCursor
	{
		get
		{
			if (m_uiCursor != 0)
			{
				if (IsAttackCursor(m_desiredCursor))
				{
					return m_desiredCursor;
				}
				if (IsCastCursor(m_desiredCursor))
				{
					return CursorType.CastAbilityInvalid;
				}
				return m_uiCursor;
			}
			return m_desiredCursor;
		}
		set
		{
			m_desiredCursor = value;
		}
	}

	public static Usable UnusableUnderCursor { get; set; }

	public static GameObject CharacterUnderCursor
	{
		get
		{
			if ((bool)OverrideCharacterUnderCursor)
			{
				return OverrideCharacterUnderCursor;
			}
			return m_characterUnderCursor;
		}
		set
		{
			m_characterUnderCursor = value;
		}
	}

	public static GameObject GenericUnderCursor
	{
		get
		{
			if ((bool)m_genericUnderCursor)
			{
				return m_genericUnderCursor;
			}
			return null;
		}
		set
		{
			m_genericUnderCursor = value;
			if ((bool)m_genericUnderCursor)
			{
				ColliderUnderCursor = m_genericUnderCursor.GetComponent<PE_Collider2D>();
			}
			else
			{
				ColliderUnderCursor = null;
			}
		}
	}

	public static GameObject ObjectUnderCursor
	{
		get
		{
			if ((bool)CharacterUnderCursor)
			{
				return CharacterUnderCursor;
			}
			return GenericUnderCursor;
		}
	}

	public static PE_Collider2D ColliderUnderCursor
	{
		get
		{
			return m_colliderUnderCursor;
		}
		set
		{
			m_colliderUnderCursor = value;
		}
	}

	public static Vector3 WorldPickPosition { get; set; }

	public static bool IsTargetCursor(CursorType type)
	{
		if (!IsAttackCursor(type))
		{
			return IsCastCursor(type);
		}
		return true;
	}

	public static bool IsAttackCursor(CursorType type)
	{
		if (type != CursorType.Attack && type != CursorType.SpecialAttack)
		{
			return type == CursorType.AttackAdvantage;
		}
		return true;
	}

	public static bool IsCastCursor(CursorType type)
	{
		if (type != CursorType.CastAbility && type != CursorType.CastAbilityInvalid && type != CursorType.CastAbilityNoLOS && type != CursorType.CastAbilityFar)
		{
			return type == CursorType.SpecialAttack;
		}
		return true;
	}

	public static bool IsInteractCursor(CursorType type)
	{
		if (type != CursorType.Examine && type != CursorType.DuplicateItem && type != CursorType.Interact && type != CursorType.Talk && type != CursorType.Stealing && type != CursorType.StealingLocked && type != CursorType.CloseDoor && type != CursorType.OpenDoor && type != CursorType.LockedDoor && type != CursorType.Loot)
		{
			return type == CursorType.Disarm;
		}
		return true;
	}

	public void SetShowCursor(object sender, bool state)
	{
		if (!state)
		{
			for (int num = m_ObjectsHidingCursor.Count - 1; num >= 0; num--)
			{
				if (m_ObjectsHidingCursor[num].Target == sender)
				{
					return;
				}
			}
			m_ObjectsHidingCursor.Add(new WeakReference(sender));
		}
		else
		{
			for (int num2 = m_ObjectsHidingCursor.Count - 1; num2 >= 0; num2--)
			{
				if (m_ObjectsHidingCursor[num2].Target == sender)
				{
					m_ObjectsHidingCursor.RemoveAt(num2);
				}
			}
		}
		ShowCursor = m_ObjectsHidingCursor.Count == 0;
	}

	public void ResetShowCursor()
	{
		m_ObjectsHidingCursor.Clear();
		ShowCursor = true;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GameCursor' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		Cursor.lockState = CursorLockMode.None;
		m_activeCursor = CursorType.None;
		m_desiredCursor = CursorType.Normal;
		m_uiCursor = CursorType.None;
		CursorOverride = CursorType.None;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ResetShowCursor();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public string GetCursorDebugOutput()
	{
		string text = "-- Mouse Cursor Debug --";
		text = text + "\nMouse Screen Pos: x = " + Input.mousePosition.x + ", y = " + Input.mousePosition.y + ", z = " + Input.mousePosition.z;
		text = text + "\nMouse Delta: x = " + GameInput.MouseDelta.x + ", y = " + GameInput.MouseDelta.y + ", z = " + GameInput.MouseDelta.z;
		text = text + "\nMouse World Pos: x = " + WorldPickPosition.x + ", y = " + WorldPickPosition.y + ", z = " + WorldPickPosition.z;
		text = text + "\nCursor: " + DesiredCursor.ToString() + ", UI Cursor: " + UiCursor;
		text = text + "\nGeneric Object Under Cursor: " + (GenericUnderCursor ? GenericUnderCursor.name : "Null");
		text = text + "\nCharacter Under Cursor: " + (CharacterUnderCursor ? CharacterUnderCursor.name : "Null");
		text = text + "\nUI Under Cursor: " + (UiObjectUnderCursor ? UiObjectUnderCursor.name : "Null");
		text = text + "\nCollider Under Cursor: " + (ColliderUnderCursor ? ColliderUnderCursor.name : "Null");
		return text + "\nUnusable Under Cursor: " + (UnusableUnderCursor ? UnusableUnderCursor.name : "Null");
	}

	private void DrawDebugCursorInfo()
	{
		UIDebug.Instance.SetText("Cursor Debug", GetCursorDebugOutput(), Color.cyan);
		UIDebug.Instance.SetTextPosition("Cursor Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
	}

	private void ValidateObjectsUnderCursors()
	{
		if (m_genericUnderCursor != null && !m_genericUnderCursor.activeInHierarchy)
		{
			m_genericUnderCursor = null;
		}
		if (m_characterUnderCursor != null && !m_characterUnderCursor.activeInHierarchy)
		{
			m_characterUnderCursor = null;
		}
		if (m_colliderUnderCursor != null && !m_colliderUnderCursor.enabled)
		{
			m_colliderUnderCursor = null;
		}
		if (UiObjectUnderCursor != null && !UiObjectUnderCursor.activeInHierarchy)
		{
			UiObjectUnderCursor = null;
		}
		if (OverrideCharacterUnderCursor != null && !OverrideCharacterUnderCursor.activeInHierarchy)
		{
			OverrideCharacterUnderCursor = null;
		}
		if (UnusableUnderCursor != null && !UnusableUnderCursor.gameObject.activeInHierarchy)
		{
			UnusableUnderCursor = null;
		}
	}

	private void Update()
	{
		if (PrefabCursors == null)
		{
			return;
		}
		if ((bool)UnusableUnderCursor && UnusableUnderCursor.IsUsable)
		{
			GenericUnderCursor = UnusableUnderCursor.gameObject;
			UnusableUnderCursor.NotifyMouseOver(state: true);
			UnusableUnderCursor = null;
		}
		ValidateObjectsUnderCursors();
		if (ShowDebug)
		{
			DrawDebugCursorInfo();
		}
		for (int num = m_ObjectsHidingCursor.Count - 1; num >= 0; num--)
		{
			if (m_ObjectsHidingCursor[num].Target == null || !m_ObjectsHidingCursor[num].IsAlive)
			{
				m_ObjectsHidingCursor.RemoveAt(num);
			}
		}
		ShowCursor = m_ObjectsHidingCursor.Count == 0;
		if ((bool)UIGlobalInventory.Instance && UIGlobalInventory.Instance.DraggingItem)
		{
			ShowCursor = false;
			DisableCursor = false;
		}
		else if ((bool)UIWindowManager.Instance && UIWindowManager.Instance.AnyWindowShowing())
		{
			ShowCursor = true;
		}
		if (m_frameCount < 2)
		{
			m_frameCount++;
			return;
		}
		CursorType desiredCursor = DesiredCursor;
		desiredCursor = HandleDownState(desiredCursor, CursorType.Normal, CursorType.NormalHeld);
		desiredCursor = HandleDownState(desiredCursor, CursorType.Walk, CursorType.WalkHeld);
		desiredCursor = HandleDownState(desiredCursor, CursorType.Disengage, CursorType.DisengageHeld);
		desiredCursor = HandleDownState(desiredCursor, CursorType.SelectionSubtract, CursorType.SelectionSubtractHeld);
		desiredCursor = HandleDownState(desiredCursor, CursorType.SelectionAdd, CursorType.SelectionAddHeld);
		if (!ShowCursor || m_activeCursor == desiredCursor)
		{
			return;
		}
		switch (desiredCursor)
		{
		case CursorType.Normal:
		case CursorType.Deprecated1:
			Cursor.SetCursor(PrefabCursors.NormalCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.NormalHeld:
			Cursor.SetCursor(PrefabCursors.NormalHeldCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.Walk:
			Cursor.SetCursor(PrefabCursors.WalkCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.WalkHeld:
			Cursor.SetCursor(PrefabCursors.WalkHeldCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.Disengage:
			Cursor.SetCursor(PrefabCursors.DisengageCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.DisengageHeld:
			Cursor.SetCursor(PrefabCursors.DisengageHeldCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.NoWalk:
			Cursor.SetCursor(PrefabCursors.NoWalkCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.RotateFormation:
			Cursor.SetCursor(PrefabCursors.RotateFormationCursor, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.Attack:
			Cursor.SetCursor(PrefabCursors.AttackCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.SpecialAttack:
			if (m_castCursorValid && (bool)m_castCursor)
			{
				Cursor.SetCursor(m_castCursor, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(PrefabCursors.SpecialAttackCursor, Vector2.zero, CursorMode.Auto);
			}
			break;
		case CursorType.StealingLocked:
			Cursor.SetCursor(PrefabCursors.StealingLocked, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.OpenDoor:
			Cursor.SetCursor(PrefabCursors.OpenDoorCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.CloseDoor:
			Cursor.SetCursor(PrefabCursors.CloseDoorCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.LockedDoor:
			Cursor.SetCursor(PrefabCursors.LockedDoorCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.AreaTransition:
			Cursor.SetCursor(PrefabCursors.AreaTransition, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.Examine:
			Cursor.SetCursor(PrefabCursors.ExamineCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.Talk:
			Cursor.SetCursor(PrefabCursors.TalkCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.Interact:
			Cursor.SetCursor(PrefabCursors.InteractCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.CastAbility:
			if (m_castCursorValid && (bool)m_castCursor)
			{
				Cursor.SetCursor(m_castCursor, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(PrefabCursors.CastAbility, Vector2.zero, CursorMode.Auto);
			}
			break;
		case CursorType.CastAbilityInvalid:
			if (m_castCursorValid && (bool)m_castCursorInvalid)
			{
				Cursor.SetCursor(m_castCursorInvalid, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(PrefabCursors.CastAbilityInvalid, Vector2.zero, CursorMode.Auto);
			}
			break;
		case CursorType.DoubleArrow_U_D:
			Cursor.SetCursor(PrefabCursors.DoubleArrow_U_D, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.DoubleArrow_L_R:
			Cursor.SetCursor(PrefabCursors.DoubleArrow_L_R, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.DoubleArrow_DL_UR:
			Cursor.SetCursor(PrefabCursors.DoubleArrow_DL_UR, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.DoubleArrow_UL_DR:
			Cursor.SetCursor(PrefabCursors.DoubleArrow_UL_DR, m_centerHotSpot, CursorMode.Auto);
			break;
		case CursorType.Disarm:
			Cursor.SetCursor(PrefabCursors.DisarmCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.CastAbilityNoLOS:
			if (m_castCursorValid && (bool)m_castCursorLoS)
			{
				Cursor.SetCursor(m_castCursorLoS, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(PrefabCursors.CastAbilityNoLOS, Vector2.zero, CursorMode.Auto);
			}
			break;
		case CursorType.CastAbilityFar:
			if (m_castCursorValid && (bool)m_castCursorMove)
			{
				Cursor.SetCursor(m_castCursorMove, Vector2.zero, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(PrefabCursors.CastAbilityFar, Vector2.zero, CursorMode.Auto);
			}
			break;
		case CursorType.Stealing:
			Cursor.SetCursor(PrefabCursors.Stealing, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.Loot:
			Cursor.SetCursor(PrefabCursors.LootCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.AttackAdvantage:
			Cursor.SetCursor(PrefabCursors.AttackAdvantageCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.SelectionAdd:
			Cursor.SetCursor(PrefabCursors.SelectionAddCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.SelectionSubtract:
			Cursor.SetCursor(PrefabCursors.SelectionSubtractCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.SelectionAddHeld:
			Cursor.SetCursor(PrefabCursors.SelectionAddHeldCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.SelectionSubtractHeld:
			Cursor.SetCursor(PrefabCursors.SelectionSubtractHeldCursor, Vector2.zero, CursorMode.Auto);
			break;
		case CursorType.DuplicateItem:
			Cursor.SetCursor(PrefabCursors.DuplicateItemCursor, Vector2.zero, CursorMode.Auto);
			break;
		default:
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			break;
		}
		m_activeCursor = desiredCursor;
	}

	private void LateUpdate()
	{
		UiCursor = CursorType.None;
		DesiredCursor = CursorType.Normal;
	}

	private CursorType HandleDownState(CursorType current, CursorType up, CursorType down)
	{
		if (current == up || current == down)
		{
			if (GameInput.GetMouseButton(0, setHandled: false))
			{
				return down;
			}
			return up;
		}
		return current;
	}

	private static void NewCastingCursor(ref Texture2D texture, Texture2D frame)
	{
		if ((bool)texture)
		{
			GameUtilities.Destroy(texture);
		}
		if ((bool)frame)
		{
			texture = new Texture2D(frame.width, frame.height, TextureFormat.RGBA32, mipChain: false);
		}
	}

	public static void BeginCasting(GenericAbility abil)
	{
		NewCastingCursor(ref m_castCursor, Instance.PrefabCursors.CastAbilityFrame);
		NewCastingCursor(ref m_castCursorInvalid, Instance.PrefabCursors.CastAbilityInvalidFrame);
		NewCastingCursor(ref m_castCursorMove, Instance.PrefabCursors.CastAbilityMoveFrame);
		NewCastingCursor(ref m_castCursorLoS, Instance.PrefabCursors.CastAbilityNoLosFrame);
		if (Instance.PrefabCursors.CastAbilityFrameData == null && (bool)Instance.PrefabCursors.CastAbilityFrame)
		{
			Instance.PrefabCursors.CastAbilityFrameData = Instance.PrefabCursors.CastAbilityFrame.GetPixels();
		}
		if (Instance.PrefabCursors.CastAbilityInvalidFrameData == null && (bool)Instance.PrefabCursors.CastAbilityInvalidFrame)
		{
			Instance.PrefabCursors.CastAbilityInvalidFrameData = Instance.PrefabCursors.CastAbilityInvalidFrame.GetPixels();
		}
		if (Instance.PrefabCursors.CastAbilityMoveFrameData == null && (bool)Instance.PrefabCursors.CastAbilityMoveFrame)
		{
			Instance.PrefabCursors.CastAbilityMoveFrameData = Instance.PrefabCursors.CastAbilityMoveFrame.GetPixels();
		}
		if (Instance.PrefabCursors.CastAbilityNoLosFrameData == null && (bool)Instance.PrefabCursors.CastAbilityNoLosFrame)
		{
			Instance.PrefabCursors.CastAbilityNoLosFrameData = Instance.PrefabCursors.CastAbilityNoLosFrame.GetPixels();
		}
		if (Instance.PrefabCursors.CastAbilityIconMaskData == null && (bool)Instance.PrefabCursors.CastAbilityIconMask)
		{
			Instance.PrefabCursors.CastAbilityIconMaskData = Instance.PrefabCursors.CastAbilityIconMask.GetPixels();
		}
		if (m_castCursorBuffer == null || m_castCursorBuffer.Length != Instance.PrefabCursors.CastAbilityFrameData.Length)
		{
			m_castCursorBuffer = new Color[Instance.PrefabCursors.CastAbilityFrameData.Length];
		}
		Instance.PrefabCursors.CastAbilityFrameData.CopyTo(m_castCursorBuffer, 0);
		try
		{
			Instance.CreateCastCursorForIcon(Instance.PrefabCursors.CastAbilityFrameData, abil.Icon, m_castCursor, 1f);
			Instance.CreateCastCursorForIcon(Instance.PrefabCursors.CastAbilityInvalidFrameData, abil.Icon, m_castCursorInvalid, 0.4f);
			Instance.CreateCastCursorForIcon(Instance.PrefabCursors.CastAbilityNoLosFrameData, abil.Icon, m_castCursorLoS, 0.4f);
			Instance.CreateCastCursorForIcon(Instance.PrefabCursors.CastAbilityMoveFrameData, abil.Icon, m_castCursorMove, 1f);
			m_activeCursor = CursorType.Normal;
			m_castCursorValid = true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			m_castCursorValid = false;
		}
	}

	private void CreateCastCursorForIcon(Color[] frameColorBuffer, Texture2D abilityIcon, Texture2D outputTexture, float iconSaturation)
	{
		if (!outputTexture || frameColorBuffer == null || !abilityIcon)
		{
			return;
		}
		if (m_castCursorBuffer == null || m_castCursorBuffer.Length != frameColorBuffer.Length)
		{
			m_castCursorBuffer = new Color[frameColorBuffer.Length];
		}
		frameColorBuffer.CopyTo(m_castCursorBuffer, 0);
		int width = Instance.PrefabCursors.CastAbilityIconMask.width;
		int height = Instance.PrefabCursors.CastAbilityIconMask.height;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				int num = i + j * width;
				if (!(Instance.PrefabCursors.CastAbilityIconMaskData[num].r <= 0f))
				{
					Color color = abilityIcon.GetPixelBilinear((float)i / (float)width, (float)j / (float)height);
					if (iconSaturation != 1f)
					{
						HSBColor hSBColor = new HSBColor(color);
						hSBColor.s *= iconSaturation;
						hSBColor.b *= iconSaturation;
						color = hSBColor.ToColor();
					}
					Color color2 = frameColorBuffer[num];
					color.a = Instance.PrefabCursors.CastAbilityIconMaskData[num].r;
					color.r = color2.r * color2.a + color.r * (1f - color2.a);
					color.g = color2.g * color2.a + color.g * (1f - color2.a);
					color.b = color2.b * color2.a + color.b * (1f - color2.a);
					color.a = 1f - (1f - color2.a) * (1f - color.a);
					m_castCursorBuffer[num] = color;
				}
			}
		}
		outputTexture.SetPixels(m_castCursorBuffer);
		outputTexture.Apply();
	}

	public static void EndCasting()
	{
		m_castCursorValid = false;
	}
}
