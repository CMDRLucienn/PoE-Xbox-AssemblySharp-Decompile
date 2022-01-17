using System;
using System.Collections.Generic;
using AI.Achievement;
using AI.Player;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
	public Texture2D SelectionImage;

	public float move_distance = 0.1f;

	public float move_interpolation_time = 0.01f;

	public float turn_interpolation_time = 0.01f;

	public float move_cooldown;

	[Tooltip("Specify how often (in seconds) you want idle sounds to play from randomized party members.")]
	public static int idle_threshold = 300;

	[Tooltip("Specify how many repeated clicks on a single character, until a humor sound clip is played.")]
	public static int humor_click_threshold = 6;

	public LayerMask collision_layerMask = -1;

	private int m_walkableLayerBitFlag;

	private bool m_isSelecting;

	private Vector3 m_dragSelectMin = Vector3.zero;

	private Vector3 m_dragSelectMax = Vector3.zero;

	private bool m_trySelect;

	private PartyMemberAI m_initialPartyMemberOnSelect;

	private bool m_isInForceAttackMode;

	private bool m_isCasting;

	private GenericAbility m_castAbility;

	private AttackBase m_castAttack;

	private AttackBase m_castWeaponAttack;

	private bool m_castMovementRestricted;

	private bool m_castFullAttack;

	private StatusEffect[] m_castStatusEffects;

	private int m_castAnimVariation = -1;

	private PartyMemberAI m_castPartyMemberAI;

	private CharacterStats m_stats;

	private List<PartyMemberAI> m_currentPartyMembers;

	private GameObject lastSelectedCharacter;

	private float m_humorTimer;

	private int m_humorCounter;

	private float m_friendlyFireTimer;

	private bool m_playMovementSound = true;

	private float m_movementTrackTime;

	private static float s_humorCooldownTime = 2f;

	private string m_startPoint = string.Empty;

	private StartPoint.PointLocation m_startPointLoc;

	private const float DRAG_BEGIN_MAGNITUDE = 30f;

	private const float QUICKDRAG_BEGIN_MAGNITUDE = 150f;

	private PlayerInventory m_Inventory;

	public bool RotatingFormation { get; set; }

	public bool FormationRotated { get; set; }

	public Vector3 FormationRotationPickPosition { get; private set; }

	public Vector3 FormationRotationDirection { get; private set; }

	public Quaternion FormationRotation { get; set; }

	public bool WantsAttackAdvantageCursor { get; set; }

	[Persistent]
	public Guid SessionID { get; set; }

	[Persistent]
	public int[][] SelectionGroups { get; set; }

	public PlayerInventory Inventory
	{
		get
		{
			if (!m_Inventory)
			{
				m_Inventory = GetComponent<PlayerInventory>();
			}
			return m_Inventory;
		}
	}

	public List<PartyMemberAI> CurrentPartyMembers => m_currentPartyMembers;

	public float FriendlyFireTimer
	{
		get
		{
			return m_friendlyFireTimer;
		}
		set
		{
			m_friendlyFireTimer = value;
		}
	}

	public bool IsDragSelecting => m_isSelecting;

	public bool IsInForceAttackMode
	{
		get
		{
			return m_isInForceAttackMode;
		}
		set
		{
			if (value)
			{
				GameInput.StartTargetedAttack();
			}
			else
			{
				GameInput.EndTargetedAttack();
			}
			m_isInForceAttackMode = value;
		}
	}

	public string StartPointName
	{
		get
		{
			return m_startPoint;
		}
		set
		{
			m_startPoint = value;
		}
	}

	public StartPoint.PointLocation StartPointLink
	{
		get
		{
			return m_startPointLoc;
		}
		set
		{
			m_startPointLoc = value;
		}
	}

	private void Awake()
	{
		SessionID = Guid.NewGuid();
		IsInForceAttackMode = false;
		m_isCasting = false;
		m_castAbility = null;
		m_castAttack = null;
		m_castWeaponAttack = null;
		m_castMovementRestricted = false;
		m_castFullAttack = false;
		m_castStatusEffects = null;
		m_castAnimVariation = -1;
		m_castPartyMemberAI = null;
		m_stats = GetComponent<CharacterStats>();
		m_currentPartyMembers = new List<PartyMemberAI>();
		lastSelectedCharacter = null;
		m_humorTimer = 0f;
		m_humorCounter = 1;
		m_friendlyFireTimer = 0f;
		RotatingFormation = false;
		FormationRotated = false;
		FormationRotationPickPosition = Vector3.zero;
		FormationRotationDirection = Vector3.forward;
		FormationRotation = Quaternion.identity;
		GameState.s_playerCharacter = this;
	}

	private void Start()
	{
		SpecialCharacterInstanceID.Add(base.gameObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Player);
		m_walkableLayerBitFlag = 1 << LayerMask.NameToLayer("Walkable");
		if (SelectionGroups == null)
		{
			SelectionGroups = new int[30][];
			for (int i = 0; i < 6; i++)
			{
				SelectionGroups[i] = new int[1];
				SelectionGroups[i][0] = i;
			}
		}
	}

	private void OnEnable()
	{
		RotatingFormation = false;
		FormationRotated = false;
		GameState.s_playerCharacter = this;
		CachePlayerGender();
	}

	private void CachePlayerGender()
	{
		if ((bool)m_stats)
		{
			StringTableManager.PlayerGender = m_stats.Gender;
			return;
		}
		m_stats = GetComponent<CharacterStats>();
		if ((bool)m_stats)
		{
			StringTableManager.PlayerGender = m_stats.Gender;
		}
	}

	public void Restored()
	{
		CameraControl component = Camera.main.GetComponent<CameraControl>();
		if (component != null)
		{
			component.FocusOnPlayer();
		}
		CachePlayerGender();
	}

	private GameCursor.CursorType GetAttackCursor()
	{
		if (WantsAttackAdvantageCursor)
		{
			return GameCursor.CursorType.AttackAdvantage;
		}
		return GameCursor.CursorType.Attack;
	}

	protected void UpdateCursor()
	{
		if (m_isCasting && m_castAbility != null)
		{
			if (!m_castAbility.ReadyForUI)
			{
				CancelModes(cancelAbility: true);
			}
			if (m_castAbility != null && !m_castAbility.IsValidTarget(GameCursor.CharacterUnderCursor))
			{
				GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
				return;
			}
		}
		if (GameCursor.UiObjectUnderCursor != null)
		{
			GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
			if (m_isCasting)
			{
				GameCursor.DesiredCursor = GetCastingCursor();
			}
			return;
		}
		bool flag = PartyMemberAI.IsPrimaryPartyMemberSelected();
		if (m_isSelecting)
		{
			GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
		}
		else if (IsInForceAttackMode)
		{
			if (GameCursor.CharacterUnderCursor != null)
			{
				Health component = GameCursor.CharacterUnderCursor.GetComponent<Health>();
				PartyMemberAI component2 = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
				if (component == null || !component.CanBeTargeted || (component2 != null && component2.Selected))
				{
					GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
				}
				else
				{
					GameCursor.DesiredCursor = GetAttackCursor();
				}
			}
			else
			{
				GameCursor.DesiredCursor = GetAttackCursor();
			}
		}
		else if (m_isCasting)
		{
			GameCursor.DesiredCursor = GetCastingCursor();
		}
		else if (RotatingFormation)
		{
			GameCursor.DesiredCursor = GameCursor.CursorType.RotateFormation;
		}
		else
		{
			GameObject objectUnderCursor = GameCursor.ObjectUnderCursor;
			if ((bool)objectUnderCursor)
			{
				Faction component3 = objectUnderCursor.GetComponent<Faction>();
				Health component4 = objectUnderCursor.GetComponent<Health>();
				Trap component5 = objectUnderCursor.GetComponent<Trap>();
				Container component6 = objectUnderCursor.GetComponent<Container>();
				PartyMemberAI component7 = objectUnderCursor.GetComponent<PartyMemberAI>();
				if (component3 != null && (component5 == null || component5.Visible) && (!component4 || component4.CurrentHealth > 0f) && component6 == null)
				{
					if (component5 != null)
					{
						if (component3.RelationshipToPlayer == Faction.Relationship.Hostile && !GameCursor.OverrideCharacterUnderCursor && !flag)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
						}
						else if (objectUnderCursor.GetComponent<PE_Collider2D>() != null && component5.CanDisarm)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Disarm;
						}
						else
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
						}
					}
					else if (component3.RelationshipToPlayer == Faction.Relationship.Hostile && !component4.Unconscious && !GameCursor.OverrideCharacterUnderCursor)
					{
						GameCursor.DesiredCursor = GetAttackCursor();
					}
					else if ((bool)component7 && component7.enabled)
					{
						if (GameInput.GetControl(MappedControl.MULTISELECT) || GameInput.GetControl(MappedControl.MULTISELECT_NEGATIVE))
						{
							if (PartyMemberAI.GetSelectedPartyMembers().Contains(objectUnderCursor))
							{
								GameCursor.DesiredCursor = GameCursor.CursorType.SelectionSubtract;
							}
							else
							{
								GameCursor.DesiredCursor = GameCursor.CursorType.SelectionAdd;
							}
						}
					}
					else
					{
						NPCDialogue component8 = objectUnderCursor.GetComponent<NPCDialogue>();
						PartyMemberAI component9 = objectUnderCursor.GetComponent<PartyMemberAI>();
						AIController component10 = objectUnderCursor.GetComponent<AIController>();
						if (flag && (!component10 || (!component10.IsBusy && !component10.IsFactionSwapped())) && (bool)component8 && (component9 == null || !component9.IsInSlot))
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Talk;
						}
						else
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
						}
					}
				}
				else
				{
					if ((bool)objectUnderCursor.GetComponent<TrapTriggerGeneric>())
					{
						GameCursor.DesiredCursor = GameCursor.CursorType.AreaTransition;
						return;
					}
					Door component11 = objectUnderCursor.GetComponent<Door>();
					if ((bool)component11)
					{
						if (component11.CurrentState == OCL.State.Closed)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.OpenDoor;
						}
						else if (component11.CurrentState == OCL.State.Open)
						{
							if (component11.IsAnyMoverIntersectingNavMeshObstacle())
							{
								GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
							}
							else
							{
								GameCursor.DesiredCursor = GameCursor.CursorType.CloseDoor;
							}
						}
						else if (component11.CurrentState == OCL.State.Locked)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.LockedDoor;
						}
						return;
					}
					Container component12 = objectUnderCursor.GetComponent<Container>();
					if (component12 != null)
					{
						if (!flag)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
						}
						else if (component5 != null && !component5.Disarmed && component5.Visible && component5.CanDisarm)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Disarm;
						}
						else
						{
							if (GameState.InCombat)
							{
								return;
							}
							if (component12.CurrentState == OCL.State.Closed)
							{
								if (component12.StealingFactionID != 0)
								{
									GameCursor.DesiredCursor = GameCursor.CursorType.Stealing;
								}
								else
								{
									GameCursor.DesiredCursor = GameCursor.CursorType.Loot;
								}
							}
							else if (component12.CurrentState == OCL.State.Locked)
							{
								if (component12.StealingFactionID != 0)
								{
									GameCursor.DesiredCursor = GameCursor.CursorType.StealingLocked;
								}
								else
								{
									GameCursor.DesiredCursor = GameCursor.CursorType.LockedDoor;
								}
							}
						}
						return;
					}
					if ((bool)objectUnderCursor.GetComponent<AutoLootContainer>() && flag && !GameState.InCombat)
					{
						GameCursor.DesiredCursor = GameCursor.CursorType.Interact;
						return;
					}
					if ((bool)objectUnderCursor.GetComponent<RestInteraction>() && flag && !GameState.InCombat)
					{
						GameCursor.DesiredCursor = GameCursor.CursorType.Interact;
						return;
					}
					if ((bool)objectUnderCursor.GetComponent<Animate>() && flag)
					{
						GameCursor.DesiredCursor = GameCursor.CursorType.Interact;
						return;
					}
					if ((bool)objectUnderCursor.GetComponent<SceneTransition>())
					{
						if (GameCursor.CursorOverride != 0)
						{
							GameCursor.DesiredCursor = GameCursor.CursorOverride;
						}
						else
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
						}
						return;
					}
					ScriptedInteraction component13 = objectUnderCursor.GetComponent<ScriptedInteraction>();
					if ((bool)component13 && (!component13.IsUsable || PartyMemberAI.IsPartyMemberUnconscious()))
					{
						GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
						return;
					}
					if ((bool)objectUnderCursor.GetComponent<PE_Collider2D>() && (component5 == null || component5.Visible))
					{
						if (!flag)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
						}
						else if (GameCursor.CursorOverride != 0)
						{
							GameCursor.DesiredCursor = GameCursor.CursorOverride;
						}
						else
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
						}
						return;
					}
				}
			}
			else if (IsMouseOnWalkMesh())
			{
				if (GameState.InCombat && IsSelectedPartyMemberEngaged())
				{
					GameCursor.DesiredCursor = GameCursor.CursorType.Disengage;
				}
				else
				{
					GameCursor.DesiredCursor = GameCursor.CursorType.Walk;
				}
			}
			else
			{
				GameCursor.DesiredCursor = GameCursor.CursorType.NoWalk;
			}
		}
		if (GameCursor.DesiredCursor == GameCursor.CursorType.Normal && GameInput.GetControl(MappedControl.MULTISELECT))
		{
			GameCursor.DesiredCursor = GameCursor.CursorType.SelectionAdd;
		}
		else if (GameCursor.DesiredCursor == GameCursor.CursorType.Normal && GameInput.GetControl(MappedControl.MULTISELECT_NEGATIVE))
		{
			GameCursor.DesiredCursor = GameCursor.CursorType.SelectionSubtract;
		}
		WantsAttackAdvantageCursor = false;
	}

	public GameCursor.CursorType GetCastingCursor()
	{
		if (m_castAttack == null || m_castPartyMemberAI == null)
		{
			return GameCursor.CursorType.CastAbilityInvalid;
		}
		GameObject gameObject = null;
		if ((bool)GameCursor.OverrideCharacterUnderCursor)
		{
			gameObject = GameCursor.OverrideCharacterUnderCursor;
		}
		else if ((bool)GameCursor.CharacterUnderCursor && m_castAttack.RequiresHitObject)
		{
			gameObject = GameCursor.CharacterUnderCursor;
		}
		Vector3 vector = (gameObject ? gameObject.transform.position : GameInput.WorldMousePosition);
		bool flag = (bool)gameObject || IsMouseOnNavMesh();
		bool flag2 = m_castAttack.IsInRange(m_castPartyMemberAI.gameObject, gameObject, vector);
		if (!m_castAbility && (m_castAttack == m_castPartyMemberAI.GetPrimaryAttack() || m_castAttack == m_castPartyMemberAI.GetSecondaryAttack()))
		{
			return GetAttackCursor();
		}
		if (m_castAttack.RequiresHitObject && (GameCursor.CharacterUnderCursor == null || !m_castPartyMemberAI.IsTargetable(GameCursor.ObjectUnderCursor, m_castAttack)))
		{
			return GameCursor.CursorType.CastAbilityInvalid;
		}
		if (flag)
		{
			if (!(m_castAttack is AttackAOE) && GameCursor.CharacterUnderCursor != null)
			{
				vector = GameCursor.CharacterUnderCursor.transform.position;
			}
			AttackAOE attackAOE = m_castAttack as AttackAOE;
			AttackRanged attackRanged = m_castAttack as AttackRanged;
			if (attackAOE != null && attackAOE.DamageAngleDegrees < 360f)
			{
				return GameCursor.CursorType.CastAbility;
			}
			if (attackRanged != null && !attackRanged.PathsToPos)
			{
				if (m_castAttack.RequiresHitObject)
				{
					if (GameCursor.CharacterUnderCursor == null || !m_castPartyMemberAI.IsTargetable(GameCursor.ObjectUnderCursor, m_castAttack))
					{
						return GameCursor.CursorType.CastAbilityInvalid;
					}
					return GameCursor.CursorType.CastAbility;
				}
				return GameCursor.CursorType.CastAbility;
			}
			if (!GameUtilities.LineofSight(m_castAttack.Owner.transform.position, vector, 1f, includeDynamics: false))
			{
				return GameCursor.CursorType.CastAbilityNoLOS;
			}
			if (!flag2 && m_castAttack.PathsToPos)
			{
				return GameCursor.CursorType.CastAbilityFar;
			}
			if (!flag2)
			{
				return GameCursor.CursorType.NoWalk;
			}
			return GameCursor.CursorType.CastAbility;
		}
		AttackAOE attackAOE2 = m_castAttack as AttackAOE;
		AttackRanged attackRanged2 = m_castAttack as AttackRanged;
		if ((attackAOE2 != null && attackAOE2.DamageAngleDegrees < 360f) || (attackRanged2 != null && !attackRanged2.PathsToPos))
		{
			return GameCursor.CursorType.CastAbility;
		}
		return GameCursor.CursorType.CastAbilityInvalid;
	}

	public bool IsMouseOnWalkMesh()
	{
		if (!Camera.main)
		{
			return false;
		}
		if (Physics.Raycast(Camera.main.ScreenPointToRay(GameInput.MousePosition), out var hitInfo, float.PositiveInfinity, m_walkableLayerBitFlag) && NavMesh.SamplePosition(hitInfo.point, out var _, 1f, -1))
		{
			return true;
		}
		return false;
	}

	public bool IsMouseOnNavMesh()
	{
		if (!Camera.main)
		{
			return false;
		}
		if (Physics.Raycast(Camera.main.ScreenPointToRay(GameInput.MousePosition), out var hitInfo, float.PositiveInfinity, m_walkableLayerBitFlag) && GameUtilities.IsPositionOnNavMesh(hitInfo.point))
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if ((m_isCasting || IsInForceAttackMode) && (GameInput.GetCancelTargetedAttack() || GameInput.GetControlUp(MappedControl.ROTATE_FORMATION)))
		{
			CancelModes(cancelAbility: true);
			return;
		}
		if (GameInput.GetControlDown(MappedControl.SELECT, handle: false) && !GameCursor.IsCastCursor(GameCursor.ActiveCursor) && !IsInForceAttackMode && GameCursor.OverrideCharacterUnderCursor == null)
		{
			m_dragSelectMin = GameInput.MousePosition;
			m_dragSelectMax = m_dragSelectMin;
			m_trySelect = true;
			m_initialPartyMemberOnSelect = null;
			if (GameCursor.CharacterUnderCursor != null)
			{
				PartyMemberAI component = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
				if (component != null && component.gameObject.activeInHierarchy)
				{
					m_initialPartyMemberOnSelect = component;
				}
			}
		}
		if (!GameInput.GetControl(MappedControl.SELECT, ignoreHandle: true, ignoreModifiers: true) || GameInput.GetControlUpWithoutModifiers(MappedControl.SELECT))
		{
			bool flag = false;
			PartyMemberAI partyMemberAI = null;
			if (GameCursor.CharacterUnderCursor != null)
			{
				partyMemberAI = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
			}
			if (m_initialPartyMemberOnSelect != null && partyMemberAI != m_initialPartyMemberOnSelect)
			{
				flag = true;
			}
			if (!m_isSelecting && m_trySelect)
			{
				m_dragSelectMax = GameInput.MousePosition;
				if ((m_dragSelectMax - m_dragSelectMin).magnitude > 150f)
				{
					m_isSelecting = true;
				}
				else if (m_initialPartyMemberOnSelect != null && flag)
				{
					m_isSelecting = true;
				}
			}
			if (m_isSelecting && m_initialPartyMemberOnSelect == null && partyMemberAI != null && partyMemberAI.gameObject.activeInHierarchy)
			{
				m_initialPartyMemberOnSelect = partyMemberAI;
				flag = true;
			}
			m_trySelect = false;
			if (!flag)
			{
				m_initialPartyMemberOnSelect = null;
			}
			if (m_isSelecting)
			{
				PerformSelection(changeSelection: true, performDrag: true, flag);
				GameInput.HandleAllClicks();
				m_isSelecting = false;
			}
			m_initialPartyMemberOnSelect = null;
		}
		else if (m_trySelect)
		{
			m_dragSelectMax = GameInput.MousePosition;
			if (m_isSelecting || (m_dragSelectMax - m_dragSelectMin).magnitude > 30f)
			{
				m_isSelecting = true;
				PerformSelection(changeSelection: false, performDrag: true, characterUnderMouseChanged: false);
			}
			else if (m_initialPartyMemberOnSelect != null)
			{
				PartyMemberAI partyMemberAI2 = null;
				if (GameCursor.CharacterUnderCursor != null)
				{
					partyMemberAI2 = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
				}
				if (partyMemberAI2 != m_initialPartyMemberOnSelect)
				{
					m_isSelecting = true;
				}
				PerformSelection(changeSelection: false, performDrag: false, characterUnderMouseChanged: false);
			}
		}
		if (!RotatingFormation && GameInput.GetControlUp(MappedControl.ROTATE_FORMATION) && (bool)GameCursor.CharacterUnderCursor)
		{
			if (GameCursor.CharacterUnderCursor != null)
			{
				PartyMemberAI component2 = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
				if (component2 != null && component2.gameObject.activeInHierarchy)
				{
					m_initialPartyMemberOnSelect = component2;
				}
			}
			PerformSelection(changeSelection: true, performDrag: false, characterUnderMouseChanged: true);
			GameInput.HandleAllClicks();
			m_isSelecting = false;
			m_trySelect = false;
			m_initialPartyMemberOnSelect = null;
		}
		if ((bool)UIDragSelect.Instance)
		{
			if (IsDragSelecting)
			{
				UIDragSelect.Instance.Show();
				UIDragSelect.Instance.Set(new Rect(m_dragSelectMin.x, m_dragSelectMin.y, m_dragSelectMax.x - m_dragSelectMin.x, m_dragSelectMax.y - m_dragSelectMin.y));
			}
			else
			{
				UIDragSelect.Instance.Hide();
			}
		}
		UpdateCursor();
		if ((bool)GUICastingManager.Instance)
		{
			if (m_castAttack == null && GameCursor.ActiveCursorIsTargeting && (bool)UIAbilityBar.GetSelectedForBars())
			{
				PartyMemberAI selectedAIForBars = UIAbilityBar.GetSelectedAIForBars();
				Equipment component3 = selectedAIForBars.GetComponent<Equipment>();
				GUICastingManager.Instance.UpdateCasting(component3 ? component3.PrimaryAttack : null, null, selectedAIForBars);
			}
			else
			{
				GUICastingManager.Instance.UpdateCasting(m_castAttack, m_castAbility, m_castPartyMemberAI);
			}
		}
		if (UIWindowManager.KeyInputAvailable)
		{
			List<GameObject> selectedPartyMembers = PartyMemberAI.GetSelectedPartyMembers();
			if (selectedPartyMembers.Count > 0)
			{
				if (GameInput.GetControlUp(MappedControl.STEALTH_TOGGLE))
				{
					bool flag2 = Stealth.IsInStealthMode(selectedPartyMembers[0]);
					for (int num = selectedPartyMembers.Count - 1; num >= 0; num--)
					{
						Stealth.SetInStealthMode(selectedPartyMembers[num], !flag2);
					}
					CancelModes(cancelAbility: true);
					GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				}
				else if (GameInput.GetControlUp(MappedControl.STEALTH_ON))
				{
					for (int num2 = selectedPartyMembers.Count - 1; num2 >= 0; num2--)
					{
						Stealth.SetInStealthMode(selectedPartyMembers[num2], inStealth: true);
					}
					CancelModes(cancelAbility: true);
					GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				}
				else if (GameInput.GetControlUp(MappedControl.STEALTH_OFF))
				{
					for (int num3 = selectedPartyMembers.Count - 1; num3 >= 0; num3--)
					{
						Stealth.SetInStealthMode(selectedPartyMembers[num3], inStealth: false);
					}
					CancelModes(cancelAbility: true);
					GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				}
			}
		}
		if (GameInput.GetControlUp(MappedControl.NEXT_WEAPON_SET))
		{
			for (int num4 = PartyMemberAI.SelectedPartyMembers.Length - 1; num4 >= 0; num4--)
			{
				if ((bool)PartyMemberAI.SelectedPartyMembers[num4])
				{
					Equipment component4 = PartyMemberAI.SelectedPartyMembers[num4].GetComponent<Equipment>();
					if ((bool)component4)
					{
						component4.SelectNextWeaponSet();
					}
				}
			}
			if ((bool)UIAbilityBar.Instance)
			{
				UIAbilityBar.Instance.RefreshWeaponSets();
			}
		}
		if (m_isCasting)
		{
			if (!m_castPartyMemberAI.Selected)
			{
				CancelModes(cancelAbility: true);
				return;
			}
			if (!GameInput.GetControlUp(MappedControl.MOVE) && !GameInput.GetControlUp(MappedControl.ATTACK) && !GameInput.GetControlUp(MappedControl.CAST_SELECTED_ABILITY, handle: true))
			{
				return;
			}
			GameInput.HandleAllClicks();
			AttackAOE attackAOE = m_castAttack as AttackAOE;
			AttackRanged attackRanged = m_castAttack as AttackRanged;
			if (!(GameCursor.CharacterUnderCursor == null) || !m_castAttack.RequiresHitObject)
			{
				if (GameCursor.CharacterUnderCursor != null && (attackAOE == null || attackAOE.DamageAngleDegrees >= 360f) && (attackRanged == null || !attackRanged.MultiHitRay))
				{
					bool flag3 = true;
					if (m_castMovementRestricted)
					{
						flag3 = GameUtilities.V3SqrDistance2D(m_castPartyMemberAI.transform.position, GameCursor.CharacterUnderCursor.transform.position) <= m_castAttack.TotalAttackDistance * m_castAttack.TotalAttackDistance;
					}
					if (flag3 && (!m_castAttack.RequiresHitObject || m_castAttack.IsValidPrimaryTarget(GameCursor.CharacterUnderCursor)) && (m_castAbility == null || m_castAbility.IsValidTarget(GameCursor.CharacterUnderCursor)))
					{
						WarnAttackPrevented(m_castPartyMemberAI);
						TargetedAttack targetedAttack = AIStateManager.StatePool.Allocate<TargetedAttack>();
						if (GameInput.GetControl(MappedControl.QUEUE))
						{
							m_castPartyMemberAI.StateManager.QueueState(targetedAttack);
						}
						else
						{
							m_castPartyMemberAI.StateManager.PushState(targetedAttack, clearStack: true);
						}
						if (m_castAttack.RequiresHitObject)
						{
							targetedAttack.Target = GameCursor.CharacterUnderCursor;
							targetedAttack.TargetPos = targetedAttack.Target.transform.position;
						}
						else if ((bool)GameCursor.OverrideCharacterUnderCursor)
						{
							targetedAttack.TargetPos = GameCursor.OverrideCharacterUnderCursor.transform.position;
						}
						else
						{
							targetedAttack.TargetPos = GameInput.WorldMousePosition;
						}
						targetedAttack.AttackToUse = m_castAttack;
						targetedAttack.WeaponAttack = m_castWeaponAttack;
						targetedAttack.FullAttack = m_castFullAttack;
						targetedAttack.StatusEffects = m_castStatusEffects;
						targetedAttack.AnimVariation = m_castAnimVariation;
						targetedAttack.Ability = m_castAbility;
						if (!m_castAttack.RequiresHitObject && !m_castAttack.IsValidPrimaryTarget(GameCursor.CharacterUnderCursor))
						{
							targetedAttack.Target = null;
						}
						CancelModes(cancelAbility: false);
						return;
					}
				}
				else if ((!m_castAttack.RequiresHitObject || (attackRanged != null && attackRanged.MultiHitRay)) && (GameCursor.DesiredCursor == GameCursor.CursorType.CastAbility || GameCursor.DesiredCursor == GameCursor.CursorType.CastAbilityFar || GameCursor.DesiredCursor == GameCursor.CursorType.CastAbilityNoLOS))
				{
					WarnAttackPrevented(m_castPartyMemberAI);
					TargetedAttack targetedAttack2 = AIStateManager.StatePool.Allocate<TargetedAttack>();
					if (GameInput.GetControl(MappedControl.QUEUE))
					{
						m_castPartyMemberAI.StateManager.QueueState(targetedAttack2);
					}
					else
					{
						m_castPartyMemberAI.StateManager.PushState(targetedAttack2, clearStack: true);
					}
					targetedAttack2.AttackToUse = m_castAttack;
					targetedAttack2.WeaponAttack = m_castWeaponAttack;
					targetedAttack2.TargetPos = GameInput.WorldMousePosition;
					targetedAttack2.FullAttack = m_castFullAttack;
					targetedAttack2.StatusEffects = m_castStatusEffects;
					targetedAttack2.AnimVariation = m_castAnimVariation;
					targetedAttack2.Ability = m_castAbility;
					if ((attackAOE != null && attackAOE.DamageAngleDegrees < 360f) || (attackRanged != null && attackRanged.MultiHitRay))
					{
						Vector3 forward = GameInput.WorldMousePosition - m_castPartyMemberAI.StateManager.Owner.transform.position;
						forward.y = 0f;
						forward.Normalize();
						targetedAttack2.Forward = forward;
						if (attackAOE != null)
						{
							targetedAttack2.TargetPos = m_castPartyMemberAI.StateManager.Owner.transform.position;
						}
						if (m_castPartyMemberAI.Mover != null)
						{
							m_castPartyMemberAI.Mover.Stop();
							m_castPartyMemberAI.Mover.enabled = false;
						}
					}
					CancelModes(cancelAbility: false);
					return;
				}
			}
		}
		if (GameInput.GetControlDownWithoutModifiers(MappedControl.ROTATE_FORMATION) || GameInput.GetControlDownWithoutModifiers(MappedControl.MOVE))
		{
			FormationRotationPickPosition = GameInput.WorldMousePosition;
		}
		if (GameInput.GetControlDownWithoutModifiers(MappedControl.ROTATE_FORMATION) && GameCursor.CharacterUnderCursor == null && GameCursor.ActiveCursor != GameCursor.CursorType.NoWalk && IsMouseOnWalkMesh())
		{
			RotatingFormation = true;
			FormationRotated = false;
		}
		if (RotatingFormation)
		{
			Vector3 formationRotationDirection = GameInput.WorldMousePosition - FormationRotationPickPosition;
			formationRotationDirection.y = 0f;
			if (formationRotationDirection.sqrMagnitude > 0.004f)
			{
				FormationRotationDirection = formationRotationDirection;
				FormationRotationDirection.Normalize();
				FormationRotation = Quaternion.FromToRotation(Vector3.forward, FormationRotationDirection);
				FormationRotated = true;
			}
			List<PartyMemberAI> list = new List<PartyMemberAI>();
			List<GameObject> selectedPartyMembers2 = PartyMemberAI.GetSelectedPartyMembers();
			int selectedSlot = 0;
			foreach (GameObject item in selectedPartyMembers2)
			{
				PartyMemberAI component5 = item.GetComponent<PartyMemberAI>();
				if (component5 != null)
				{
					component5.DesiredRotationPosition = component5.CalculateFormationPosition(FormationRotationPickPosition, ignoreSelection: false, out selectedSlot);
					component5.ResolveDesiredRotationPosition(FormationRotationPickPosition, list);
					list.Add(component5);
				}
			}
		}
		if (m_humorTimer > 0f)
		{
			m_humorTimer -= Time.deltaTime;
		}
		if (m_friendlyFireTimer > 0f)
		{
			m_friendlyFireTimer -= Time.deltaTime;
		}
		TryPlayIdleSound();
		IssueCommands();
		if (PartyMemberAI.NumSelectedMembers() <= 0)
		{
			PartyMemberAI.EnsurePartyMemberSelected();
		}
		if (RotatingFormation && !GameInput.GetControl(MappedControl.ROTATE_FORMATION, ignoreHandle: false, ignoreModifiers: true))
		{
			RotatingFormation = false;
		}
	}

	private void WarnAttackPrevented(PartyMemberAI partyMemberAI)
	{
		if (!(partyMemberAI.StateManager.CurrentState is Stunned) && !(partyMemberAI.StateManager.CurrentState is Paralyzed))
		{
			return;
		}
		CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			StatusEffect statusEffect = component.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.StopAnimation) ?? component.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.Stunned);
			if (statusEffect != null)
			{
				UIHealthstringManager.Instance.ShowWarning(statusEffect.BundleName, partyMemberAI.gameObject);
			}
		}
	}

	private void IssueCommands()
	{
		if (m_trySelect || m_isSelecting)
		{
			return;
		}
		List<GameObject> selectedPartyMembers = PartyMemberAI.GetSelectedPartyMembers();
		if (selectedPartyMembers.Count <= 0)
		{
			return;
		}
		if (!RotatingFormation && (bool)GameCursor.CharacterUnderCursor && GameInput.GetControlUp(MappedControl.ATTACK, handle: false))
		{
			Faction component = GameCursor.CharacterUnderCursor.GetComponent<Faction>();
			if (component != null)
			{
				bool flag = component.IsHostile(selectedPartyMembers[0]);
				if ((bool)GameCursor.OverrideCharacterUnderCursor)
				{
					flag = false;
				}
				if (IsInForceAttackMode || flag)
				{
					PartyMemberAI component2 = GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>();
					if (GameCursor.DesiredCursor != GameCursor.CursorType.NoWalk && (component2 == null || !component2.Selected))
					{
						foreach (GameObject item in selectedPartyMembers)
						{
							PartyMemberAI component3 = item.GetComponent<PartyMemberAI>();
							if (!(component3 != null))
							{
								continue;
							}
							WarnAttackPrevented(component3);
							m_stats.IdleTimer = 0f;
							AI.Player.Attack attack = null;
							if (GameInput.GetControl(MappedControl.QUEUE))
							{
								attack = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
								attack.IsAutoAttack = false;
								attack.Target = GameCursor.CharacterUnderCursor;
								component3.StateManager.QueueState(attack);
							}
							else
							{
								AIState currentState = component3.StateManager.CurrentState;
								AI.Player.Attack attack2 = currentState as AI.Player.Attack;
								if (attack2 == null && currentState is AI.Achievement.Attack)
								{
									attack2 = component3.StateManager.QueuedState as AI.Player.Attack;
								}
								if (attack2 != null && attack2.Target == GameCursor.CharacterUnderCursor)
								{
									if (currentState is AI.Player.Attack)
									{
										component3.StateManager.ClearQueuedStates();
									}
									else
									{
										component3.StateManager.ClearQueuedStates(3);
									}
								}
								else
								{
									attack = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
									attack.IsAutoAttack = false;
									attack.Target = GameCursor.CharacterUnderCursor;
									if (currentState is AI.Achievement.ReloadWeapon)
									{
										component3.StateManager.ClearQueuedStates();
										component3.StateManager.QueueState(attack);
									}
									else
									{
										component3.StateManager.PushState(attack, clearStack: true);
									}
								}
							}
							if (component3.Slot == PartyMemberAI.GetSelectedLeaderSlot() && !GameState.Paused)
							{
								SoundSet.TryPlayVoiceEffectWithLocalCooldown(item, SoundSet.SoundAction.Attack, SoundSet.s_VeryShortVODelay, forceInterrupt: true);
							}
						}
					}
					IsInForceAttackMode = false;
					return;
				}
			}
		}
		bool flag2 = false;
		bool controlUp = GameInput.GetControlUp(MappedControl.SELECT, handle: false);
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null)
			{
				continue;
			}
			PartyMemberAI component4 = partyMemberAI.GetComponent<PartyMemberAI>();
			if (!(component4 != null))
			{
				continue;
			}
			component4.ProcessSelection();
			if (!controlUp || !(GameCursor.CharacterUnderCursor == partyMemberAI.gameObject))
			{
				continue;
			}
			flag2 = true;
			m_humorTimer = s_humorCooldownTime;
			if (GameCursor.CharacterUnderCursor == lastSelectedCharacter)
			{
				if (m_humorTimer > 0f)
				{
					m_humorCounter++;
				}
				if (m_humorCounter >= humor_click_threshold)
				{
					if ((bool)component4.SoundSet && !GameState.Paused && !GameState.InCombat && !partyMemberAI.IsUnconscious)
					{
						SoundSet.TryPlayVoiceEffectWithLocalCooldown(component4.gameObject, SoundSet.SoundAction.Humor, 5f, forceInterrupt: true);
					}
					m_humorTimer = 0f;
					m_humorCounter = 1;
				}
			}
			lastSelectedCharacter = GameCursor.CharacterUnderCursor;
		}
		if (selectedPartyMembers.Count <= 0)
		{
			PartyMemberAI.EnsurePartyMemberSelected();
			m_playMovementSound = true;
		}
		else if (flag2)
		{
			m_playMovementSound = true;
			partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI2 in partyMembers)
			{
				if (partyMemberAI2 != null)
				{
					HighlightCharacter component5 = partyMemberAI2.GetComponent<HighlightCharacter>();
					if (component5 != null)
					{
						component5.LassoSelected = false;
					}
				}
			}
		}
		else
		{
			if (GameCursor.DesiredCursor == GameCursor.CursorType.NoWalk || (!RotatingFormation && !IsMouseOnWalkMesh()))
			{
				return;
			}
			if (GameInput.GetControlUp(MappedControl.MOVE))
			{
				flag2 = false;
			}
			bool flag3 = GameCursor.IsInteractCursor(GameCursor.DesiredCursor) || (!RotatingFormation && GameCursor.ColliderUnderCursor != null && GameCursor.ColliderUnderCursor.CanUse);
			bool num = GameCursor.DesiredCursor == GameCursor.CursorType.CastAbilityInvalid || flag3;
			bool flag4 = GameInput.GetControlUp(MappedControl.MOVE) || GameInput.GetControlUp(MappedControl.ROTATE_FORMATION);
			if (!(!num && flag4))
			{
				return;
			}
			float num2 = float.MaxValue;
			GameObject gameObject = null;
			List<PartyMemberAI> list = new List<PartyMemberAI>();
			if (!RotatingFormation)
			{
				FormationRotationPickPosition = GameInput.WorldMousePosition;
			}
			CancelModes(cancelAbility: true);
			int selectedSlot = 0;
			foreach (GameObject item2 in selectedPartyMembers)
			{
				PartyMemberAI component6 = item2.GetComponent<PartyMemberAI>();
				if (!(component6 != null))
				{
					continue;
				}
				component6.DesiredRotationPosition = component6.CalculateFormationPosition(FormationRotationPickPosition, ignoreSelection: false, out selectedSlot);
				component6.ResolveDesiredRotationPosition(FormationRotationPickPosition, list);
				list.Add(component6);
				if (((bool)component6.Mover && component6.Mover.Frozen) || component6.StateManager.CurrentState is Stunned || component6.StateManager.CurrentState is Paralyzed)
				{
					CharacterStats component7 = item2.GetComponent<CharacterStats>();
					if ((bool)component7)
					{
						StatusEffect statusEffect = component7.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.NonMobile) ?? component7.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.StopAnimation) ?? component7.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.Stunned);
						if (statusEffect != null)
						{
							UIHealthstringManager.Instance.ShowWarning(statusEffect.BundleName, item2);
						}
					}
				}
				Move move = null;
				m_stats.IdleTimer = 0f;
				if (GameInput.GetControl(MappedControl.QUEUE))
				{
					move = AIStateManager.StatePool.Allocate<Move>();
					InitializeMoveState(move, component6);
					component6.StateManager.QueueState(move);
					if (component6.StateManager.CurrentState == move)
					{
						component6.DestinationCircleState = move;
						component6.DestinationCirclePosition = component6.DesiredRotationPosition;
					}
				}
				else
				{
					if (!(component6.StateManager.CurrentState is Move move2))
					{
						move = AIStateManager.StatePool.Allocate<Move>();
						InitializeMoveState(move, component6);
						component6.StateManager.PushState(move, clearStack: true);
					}
					else
					{
						move = move2;
						InitializeMoveState(move, component6);
						Mover mover = component6.Mover;
						if (mover != null)
						{
							mover.ClearAllTurnDirections();
						}
						component6.StateManager.ClearQueuedStates();
						if (component6.Mover.enabled)
						{
							move.OnEnter();
						}
					}
					float num3 = GameUtilities.V3Distance2D(item2.transform.position, component6.DesiredRotationPosition);
					if (num3 < num2)
					{
						num2 = num3;
						gameObject = item2;
					}
					component6.DestinationCircleState = move;
					component6.DestinationCirclePosition = component6.DesiredRotationPosition;
				}
				if (component6.Slot == PartyMemberAI.GetSelectedLeaderSlot() && (bool)component6.SoundSet && m_playMovementSound && !GameState.Paused)
				{
					SoundSet.TryPlayVoiceEffectWithLocalCooldown(component6.gameObject, SoundSet.SoundAction.Movement, SoundSet.s_VeryShortVODelay, forceInterrupt: false);
					m_playMovementSound = false;
				}
			}
			if (GameInput.GetControl(MappedControl.QUEUE) || !(gameObject != null) || selectedPartyMembers.Count <= 3)
			{
				return;
			}
			foreach (GameObject item3 in selectedPartyMembers)
			{
				if (item3 == gameObject)
				{
					continue;
				}
				PartyMemberAI component8 = item3.GetComponent<PartyMemberAI>();
				if (!(component8 == null) && (component8.StateManager.LastUpdatedState == null || !component8.StateManager.LastUpdatedState.IsMoving()) && component8.StateManager.CurrentState is Move move3 && !Cutscene.CutsceneActive)
				{
					float num4 = GameUtilities.V3Distance2D(item3.transform.position, move3.Destination);
					float num5 = 5f;
					float num6 = 1.5f;
					float num7 = num5 - num6;
					float num8 = 0.4f;
					if (num4 < num2 + num5 && num4 > num2 + num6)
					{
						WaitForClearPath waitForClearPath = AIStateManager.StatePool.Allocate<WaitForClearPath>();
						waitForClearPath.Blocker = gameObject.GetComponent<Mover>();
						waitForClearPath.BlockerDistance = 0.01f;
						component8.StateManager.PushState(waitForClearPath, clearStack: false);
						waitForClearPath.WaitTimer = (num4 - num2 - num6) / num7 * num8;
					}
				}
			}
		}
	}

	private void InitializeMoveState(Move state, PartyMemberAI partyMemberAI)
	{
		state.Destination = partyMemberAI.DesiredRotationPosition;
		state.Range = 0.05f;
		state.ShowDestinationCircle = true;
	}

	private bool TryPlayPartySound(SoundSet.SoundAction soundAction)
	{
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		for (int i = 0; i < 6; i++)
		{
			PartyMemberAI partyMemberAI = PartyMemberAI.PartyMembers[i];
			if (partyMemberAI != null && (bool)partyMemberAI.SoundSet)
			{
				SoundSetComponent component = partyMemberAI.GetComponent<SoundSetComponent>();
				if ((bool)component && (bool)component.SoundSet && component.SoundSet.VOCooldownRemaining <= 0f)
				{
					list.Add(partyMemberAI);
				}
			}
		}
		if (list.Count > 0)
		{
			int index = OEIRandom.Index(list.Count);
			return SoundSet.TryPlayVoiceEffectWithLocalCooldown(list[index].gameObject, soundAction, 5f, forceInterrupt: false);
		}
		return false;
	}

	private void TryPlayIdleSound()
	{
		if (m_stats.IdleTimer > (float)idle_threshold)
		{
			TryPlayPartySound(SoundSet.SoundAction.Idle);
			m_stats.IdleTimer = 0f;
		}
	}

	public bool IsSelectedPartyMemberEngaged()
	{
		bool flag = false;
		for (int i = 0; i < PartyMemberAI.SelectedPartyMembers.Length; i++)
		{
			if (flag)
			{
				break;
			}
			if ((bool)PartyMemberAI.SelectedPartyMembers[i])
			{
				PartyMemberAI component = PartyMemberAI.SelectedPartyMembers[i].GetComponent<PartyMemberAI>();
				if ((bool)component)
				{
					flag |= component.IsEngaged();
				}
			}
		}
		return flag;
	}

	public void ObjectClicked(Usable usableObject)
	{
		if (GameState.s_playerCharacter.IsActionCursor())
		{
			return;
		}
		if (usableObject is SceneTransition)
		{
			for (int i = 0; i < 30; i++)
			{
				if (!(PartyMemberAI.SelectedPartyMembers[i] != null))
				{
					continue;
				}
				PartyMemberAI component = PartyMemberAI.SelectedPartyMembers[i].GetComponent<PartyMemberAI>();
				if (!component)
				{
					continue;
				}
				UseObject useObject = null;
				if (GameInput.GetControl(MappedControl.QUEUE))
				{
					useObject = AIStateManager.StatePool.Allocate<UseObject>();
					component.StateManager.QueueState(useObject);
				}
				else
				{
					if (IsUsingObject(component, usableObject))
					{
						continue;
					}
					useObject = AIStateManager.StatePool.Allocate<UseObject>();
					component.StateManager.PushState(useObject, clearStack: true);
				}
				useObject.UsableObject = usableObject;
			}
			return;
		}
		PartyMemberAI partyMemberAI = null;
		OCL oCL = usableObject as OCL;
		Trap trap = usableObject as Trap;
		bool flag = PartyMemberAI.IsPrimaryPartyMemberSelected();
		if (usableObject is Container)
		{
			if (!flag)
			{
				return;
			}
			if (oCL == null)
			{
				oCL = usableObject.gameObject.GetComponent<OCL>();
			}
			if (trap == null)
			{
				trap = usableObject.gameObject.GetComponent<Trap>();
			}
		}
		if ((bool)trap && !flag)
		{
			return;
		}
		bool flag2 = (bool)oCL && oCL.CurrentState == OCL.State.Locked && !oCL.PartyHasKey();
		bool flag3 = (bool)trap && !trap.Disarmed;
		if ((bool)trap && !trap.CanDisarm)
		{
			return;
		}
		if (flag2 || flag3)
		{
			GameObject gameObject = null;
			float num = float.MaxValue;
			int num2 = -1;
			GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
			foreach (GameObject gameObject2 in selectedPartyMembers)
			{
				if (gameObject2 == null || !PartyMemberAI.IsSelectedPartyMember(gameObject2))
				{
					continue;
				}
				CharacterStats component2 = gameObject2.GetComponent<CharacterStats>();
				if (component2 == null)
				{
					continue;
				}
				float num3 = GameUtilities.V3SqrDistance2D(usableObject.transform.position, gameObject2.transform.position);
				int num4 = component2.CalculateSkill(CharacterStats.SkillType.Mechanics);
				if (flag3)
				{
					if (num4 < num2)
					{
						continue;
					}
					if (num4 == num2)
					{
						if (num3 < num)
						{
							gameObject = gameObject2;
							num2 = num4;
							num = num3;
						}
					}
					else
					{
						gameObject = gameObject2;
						num2 = num4;
						num = num3;
					}
				}
				else
				{
					if (!oCL || num4 < num2)
					{
						continue;
					}
					if (num4 == num2)
					{
						if (num3 < num)
						{
							gameObject = gameObject2;
							num2 = num4;
							num = num3;
						}
					}
					else
					{
						gameObject = gameObject2;
						num2 = num4;
						num = num3;
					}
				}
			}
			if (gameObject != null)
			{
				partyMemberAI = gameObject.GetComponent<PartyMemberAI>();
			}
		}
		else
		{
			partyMemberAI = PartyMemberAI.GetClosestPrimaryMember(usableObject.transform.position, selectedOnly: true);
		}
		if (!(partyMemberAI != null))
		{
			return;
		}
		UseObject useObject2 = null;
		if (oCL != null && oCL.CurrentState == OCL.State.Locked && (oCL is Door || oCL is Container) && !oCL.MustHaveKey && !oCL.PartyHasKey())
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(partyMemberAI.gameObject, SoundSet.SoundAction.LockPick, SoundSet.s_MediumVODelay, forceInterrupt: true);
		}
		if (GameInput.GetControl(MappedControl.QUEUE))
		{
			useObject2 = AIStateManager.StatePool.Allocate<UseObject>();
			partyMemberAI.StateManager.QueueState(useObject2);
		}
		else
		{
			if (IsUsingObject(partyMemberAI, usableObject))
			{
				return;
			}
			useObject2 = AIStateManager.StatePool.Allocate<UseObject>();
			partyMemberAI.StateManager.PushState(useObject2, clearStack: true);
		}
		useObject2.UsableObject = usableObject;
	}

	private bool IsUsingObject(PartyMemberAI partyMemberAI, Usable useable)
	{
		if (partyMemberAI == null || useable == null)
		{
			return false;
		}
		AIState currentState = partyMemberAI.StateManager.CurrentState;
		if (currentState == null)
		{
			return false;
		}
		UseObject useObject = currentState as UseObject;
		if (useObject == null)
		{
			useObject = currentState.ParentState as UseObject;
		}
		if (useObject != null)
		{
			return useObject.UsableObject == useable;
		}
		return false;
	}

	private bool RectanglesIntersect(Rect a, Rect b)
	{
		if (a.Contains(b.center) || b.Contains(a.center))
		{
			return true;
		}
		if (a.xMin > b.xMax || b.xMin > a.xMax || a.yMin > b.yMax || b.yMin > a.yMax)
		{
			return false;
		}
		return true;
	}

	public void StartCasting(GenericAbility ability, AttackBase attack, bool movementRestricted, bool fullAttack, StatusEffect[] statusEffects, AttackBase weaponAttack, int animVariation, PartyMemberAI partyMemberAI)
	{
		GameObject hitObject;
		if (attack != null && attack.HasForcedTarget)
		{
			AI.Player.Attack attack2 = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
			attack2.AttackToUse = attack;
			attack2.IsAutoAttack = false;
			attack2.WeaponAttack = weaponAttack;
			attack2.Target = attack.ForcedTarget;
			attack2.EffectsOnLaunch = statusEffects;
			if (GameInput.GetControl(MappedControl.QUEUE))
			{
				partyMemberAI.StateManager.QueueState(attack2);
				return;
			}
			if (attack.HasForcedTarget)
			{
				AI.Player.Attack attack3 = partyMemberAI.StateManager.CurrentState as AI.Player.Attack;
				AI.Achievement.Attack attack4 = partyMemberAI.StateManager.CurrentState as AI.Achievement.Attack;
				if ((attack3 != null && attack3.AttackToUse == attack) || (attack4 != null && attack4.Parameters.Attack == attack))
				{
					return;
				}
				AI.Player.Attack attack5 = partyMemberAI.StateManager.FindState(typeof(AI.Player.Attack)) as AI.Player.Attack;
				while (attack5 != null)
				{
					if (attack5.AttackToUse == attack)
					{
						partyMemberAI.StateManager.PopState(attack5);
						attack5 = partyMemberAI.StateManager.FindState(typeof(AI.Player.Attack)) as AI.Player.Attack;
					}
					else
					{
						attack5 = null;
					}
				}
			}
			partyMemberAI.StateManager.PushState(attack2);
		}
		else if (attack.ForceTarget(out hitObject))
		{
			TargetedAttack targetedAttack = AIStateManager.StatePool.Allocate<TargetedAttack>();
			targetedAttack.AttackToUse = attack;
			targetedAttack.WeaponAttack = weaponAttack;
			targetedAttack.Target = hitObject;
			targetedAttack.FullAttack = fullAttack;
			targetedAttack.StatusEffects = statusEffects;
			targetedAttack.AnimVariation = animVariation;
			if (GameInput.GetControl(MappedControl.QUEUE))
			{
				partyMemberAI.StateManager.QueueState(targetedAttack);
			}
			else
			{
				partyMemberAI.StateManager.PushState(targetedAttack);
			}
		}
		else
		{
			m_isCasting = true;
			m_castAbility = ability;
			m_castAttack = attack;
			m_castWeaponAttack = weaponAttack;
			m_castMovementRestricted = movementRestricted;
			m_castFullAttack = fullAttack;
			m_castStatusEffects = statusEffects;
			m_castAnimVariation = animVariation;
			m_castPartyMemberAI = partyMemberAI;
			if (m_castAttack != null && m_castAttack.ValidTargetDead())
			{
				GameInput.SelectDead = true;
			}
			else
			{
				GameInput.SelectDead = false;
			}
			GameCursor.BeginCasting(m_castAbility);
			GameInput.StartTargetedAttack();
		}
	}

	public void CancelModes(bool cancelAbility)
	{
		IsInForceAttackMode = false;
		CancelCasting(cancelAbility);
	}

	private void CancelCasting(bool cancelAbility)
	{
		GameInput.SelectDead = false;
		if (m_isCasting)
		{
			if (cancelAbility && m_castAbility != null)
			{
				m_castAbility.CancelCasting();
			}
			m_isCasting = false;
			m_castAbility = null;
			m_castAttack = null;
			m_castPartyMemberAI = null;
			GameCursor.EndCasting();
			GameInput.EndTargetedAttack();
		}
	}

	private void PerformSelection(bool changeSelection, bool performDrag, bool characterUnderMouseChanged)
	{
		Vector3 dragSelectMin = m_dragSelectMin;
		Vector3 dragSelectMax = m_dragSelectMax;
		dragSelectMin.y = (float)Screen.height - dragSelectMin.y;
		dragSelectMax.y = (float)Screen.height - dragSelectMax.y;
		if (dragSelectMax.x < dragSelectMin.x)
		{
			float x = dragSelectMin.x;
			dragSelectMin.x = dragSelectMax.x;
			dragSelectMax.x = x;
		}
		if (dragSelectMax.y < dragSelectMin.y)
		{
			float y = dragSelectMin.y;
			dragSelectMin.y = dragSelectMax.y;
			dragSelectMax.y = y;
		}
		Rect a = new Rect(dragSelectMin.x, dragSelectMin.y, dragSelectMax.x - dragSelectMin.x, dragSelectMax.y - dragSelectMin.y);
		if (!performDrag)
		{
			a.Set(0f, 0f, 0f, 0f);
		}
		bool control = GameInput.GetControl(MappedControl.MULTISELECT);
		bool control2 = GameInput.GetControl(MappedControl.MULTISELECT_NEGATIVE);
		bool flag = false;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null || !partyMemberAI.gameObject.activeInHierarchy || partyMemberAI.IsFactionSwapped())
			{
				continue;
			}
			Vector3 vector = Camera.main.WorldToScreenPoint(partyMemberAI.transform.position);
			vector.y = (float)Screen.height - vector.y;
			float num = 1f;
			Mover component = partyMemberAI.GetComponent<Mover>();
			if (component != null)
			{
				num = component.Radius;
			}
			Vector3 position = partyMemberAI.transform.position;
			position.x += num;
			position = Camera.main.WorldToScreenPoint(position);
			position.y = (float)Screen.height - position.y;
			float magnitude = (position - vector).magnitude;
			Rect b = new Rect(vector.x - magnitude, vector.y - magnitude, magnitude * 2f, magnitude * 2f);
			bool flag2 = a.Contains(vector) || RectanglesIntersect(a, b) || (characterUnderMouseChanged && partyMemberAI == m_initialPartyMemberOnSelect);
			bool flag3 = (control2 ? (!flag2) : flag2);
			if ((control || control2) && !flag2)
			{
				flag3 = partyMemberAI.Selected;
			}
			if (flag2)
			{
				flag = true;
			}
			HighlightCharacter component2 = partyMemberAI.GetComponent<HighlightCharacter>();
			if (flag3)
			{
				if ((bool)component2)
				{
					component2.LassoSelected = !changeSelection;
					component2.LassoDeselected = false;
				}
			}
			else if ((bool)component2)
			{
				component2.LassoSelected = false;
				if (control2 && flag2)
				{
					component2.LassoDeselected = true;
				}
			}
		}
		if (changeSelection && flag)
		{
			m_playMovementSound = true;
			partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI2 in partyMembers)
			{
				if (!(partyMemberAI2 == null) && partyMemberAI2.gameObject.activeInHierarchy && !partyMemberAI2.IsFactionSwapped())
				{
					Vector3 vector2 = Camera.main.WorldToScreenPoint(partyMemberAI2.transform.position);
					vector2.y = (float)Screen.height - vector2.y;
					float num2 = 1f;
					Mover component3 = partyMemberAI2.GetComponent<Mover>();
					if (component3 != null)
					{
						num2 = component3.Radius;
					}
					Vector3 position2 = partyMemberAI2.transform.position;
					position2.x += num2;
					position2 = Camera.main.WorldToScreenPoint(position2);
					position2.y = (float)Screen.height - position2.y;
					float magnitude2 = (position2 - vector2).magnitude;
					Rect b2 = new Rect(vector2.x - magnitude2, vector2.y - magnitude2, magnitude2 * 2f, magnitude2 * 2f);
					bool flag4 = a.Contains(vector2) || RectanglesIntersect(a, b2) || (characterUnderMouseChanged && partyMemberAI2 == m_initialPartyMemberOnSelect);
					bool dragSelected = (control2 ? (!flag4) : flag4);
					if ((control || control2) && !flag4)
					{
						dragSelected = partyMemberAI2.Selected;
					}
					partyMemberAI2.DragSelected = dragSelected;
					HighlightCharacter component4 = partyMemberAI2.GetComponent<HighlightCharacter>();
					if ((bool)component4)
					{
						component4.LassoSelected = false;
						component4.LassoDeselected = false;
					}
				}
			}
			for (int j = 0; j < 30; j++)
			{
				if (PartyMemberAI.SelectedPartyMembers[j] != null)
				{
					PartyMemberAI component5 = PartyMemberAI.SelectedPartyMembers[j].GetComponent<PartyMemberAI>();
					if ((bool)component5.SoundSet && !component5.IsUnconscious && !GameState.Paused)
					{
						SoundSet.TryPlayVoiceEffectWithLocalCooldown(component5.gameObject, SoundSet.SoundAction.Selected, 5f, forceInterrupt: false);
					}
					break;
				}
			}
		}
		if (PartyMemberAI.NumSelectedMembers() <= 0)
		{
			PartyMemberAI.EnsurePartyMemberSelected();
		}
	}

	public bool IsCasting(PartyMemberAI ai, GenericAbility ability)
	{
		if (m_isCasting && m_castPartyMemberAI == ai)
		{
			return m_castAbility == ability;
		}
		return false;
	}

	public bool IsCasting()
	{
		return m_isCasting;
	}

	public bool IsActionCursor()
	{
		if (!IsCasting())
		{
			return IsInForceAttackMode;
		}
		return true;
	}
}
