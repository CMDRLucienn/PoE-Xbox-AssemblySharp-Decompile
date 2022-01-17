using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AI.Achievement;
using AI.Player;
using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("AI/Party Member")]
public class PartyMemberAI : AIController
{
	[Serializable]
	public class DetectionObject
	{
		public Guid m_obj;

		public float m_time;

		public Guid Object
		{
			get
			{
				return m_obj;
			}
			set
			{
				m_obj = value;
			}
		}

		public float Time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
			}
		}

		public DetectionObject()
		{
		}

		public DetectionObject(Guid obj, float time)
		{
			m_obj = obj;
			m_time = time;
		}
	}

	private static Vector3[] s_resolveOffsets = new Vector3[8]
	{
		new Vector3(1.5f, 0f, 1.5f),
		new Vector3(1.5f, 0f, 0f),
		new Vector3(1.5f, 0f, -1.5f),
		new Vector3(0f, 0f, 1.5f),
		new Vector3(0f, 0f, -1.5f),
		new Vector3(-1.5f, 0f, 1.5f),
		new Vector3(-1.5f, 0f, 0f),
		new Vector3(-1.5f, 0f, -1.5f)
	};

	public static bool DebugParty = false;

	public static readonly int MaxAdventurers = 8;

	public const int MAX_PRIMARY_PARTY_MEMBERS = 6;

	public const int ANIMAL_COMPANION_START_INDEX = 6;

	public const int ANIMAL_COMPANION_END_INDEX = 12;

	public const int SUMMONED_CREATURE_START_INDEX = 12;

	public const int MAX_SECONDARY_SLOTS = 4;

	public const int MAX_PARTY_MEMBERS = 30;

	public const float LOS_DISTANCE = 13f;

	public const int INVESTIGATION_THRESHOLD = 100;

	public const int COMBAT_THRESHOLD = 200;

	protected static GameObject[] s_selectedPartyMembers = new GameObject[30];

	protected static List<GameObject> s_selectedPartyMembersList = new List<GameObject>(30);

	public static PartyMemberAI[] PartyMembers = new PartyMemberAI[30];

	private static List<GameObject> s_bestMembers = new List<GameObject>();

	private static float s_enemySpottedTimer = -1f;

	[HideInInspector]
	public PartyMemberSpellList InstructionSet;

	public float CooldownBetweenSpells;

	protected List<SpellCastData> m_instructions = new List<SpellCastData>();

	protected SpellList.InstructionSelectionType m_instructionSelectionType;

	protected TargetPreference m_defaultTargetPreference;

	protected CasterTargetScanner m_casterTargetScanner = new CasterTargetScanner();

	private SoundSetComponent m_SoundSet;

	[Persistent]
	public int FormationStyle;

	[Persistent]
	public int AssignedSlot = -1;

	[Persistent]
	public bool Secondary;

	public int DesiredFormationIndex = -1;

	private bool m_IsInSlot;

	[Persistent]
	[HideInInspector]
	public bool AddedThroughScript;

	private PartyMemberStats m_StatTracker;

	private bool m_isSelected;

	private bool m_dragSelected;

	private bool m_gotUsabilityEvent;

	private static int m_selectionCount = 0;

	private static int m_lastSelectedSlot = 0;

	private bool m_enemySpotted;

	private FogOfWar.Revealer m_revealer;

	private DestinationCircle m_DestinationCircle;

	private GameObject m_TargetCircle;

	private AlphaControl m_alphaControl;

	private AIState m_destinationCircleState;

	private Vector3 m_destinationCirclePosition;

	private Vector3 m_desiredFormationPosition;

	[HideInInspector]
	public Inventory Inventory;

	[HideInInspector]
	public QuickbarInventory QuickbarInventory;

	private CharacterHotkeyBindings m_Hotkeys;

	public static bool SafeEnableDisable = false;

	private bool m_DestSuspended;

	public bool IsAdventurer
	{
		get
		{
			if (!GetComponent<CompanionInstanceID>())
			{
				return !GetComponent<Player>();
			}
			return false;
		}
	}

	public override List<SpellCastData> Instructions => m_instructions;

	public override SpellList.InstructionSelectionType InstructionSelectionType => m_instructionSelectionType;

	public override TargetPreference DefaultTargetPreference => m_defaultTargetPreference;

	private PartyAISettings AISettings
	{
		get
		{
			PartyAISettings component = GetComponent<PartyAISettings>();
			if ((bool)component)
			{
				return component;
			}
			return base.gameObject.AddComponent<PartyAISettings>();
		}
	}

	public bool UseInstructionSet
	{
		get
		{
			return AISettings.UseInstructionSet;
		}
		set
		{
			AISettings.UseInstructionSet = value;
		}
	}

	public bool UsePerRestAbilitiesInInstructionSet
	{
		get
		{
			return AISettings.UsePerRestAbilitiesInInstructionSet;
		}
		set
		{
			AISettings.UsePerRestAbilitiesInInstructionSet = value;
		}
	}

	public int InstructionSetIndex
	{
		get
		{
			return AISettings.InstructionSetIndex;
		}
		set
		{
			AISettings.InstructionSetIndex = value;
		}
	}

	public CasterTargetScanner CasterTargetScanner => m_casterTargetScanner;

	[HideInInspector]
	public SoundSet SoundSet
	{
		get
		{
			if (m_SoundSet == null)
			{
				return null;
			}
			return m_SoundSet.SoundSet;
		}
		set
		{
			if (m_SoundSet == null)
			{
				m_SoundSet = GetComponent<SoundSetComponent>();
			}
			if (m_SoundSet != null)
			{
				m_SoundSet.SetSoundSet(value);
			}
		}
	}

	public static IEnumerable<PartyMemberAI> OnlyPrimaryPartyMembers => PartyMembers.Where((PartyMemberAI p) => (bool)p && !p.Summoner && !p.Secondary);

	public static int NumPrimaryPartyMembers => PartyMembers.Count((PartyMemberAI p) => (bool)p && !p.Summoner && !p.Secondary);

	[Persistent]
	[HideInInspector]
	public bool IsInSlot
	{
		get
		{
			return m_IsInSlot;
		}
		set
		{
			m_IsInSlot = value;
		}
	}

	private EternityTimeInterval TimeInParty
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker && value != null)
			{
				m_StatTracker.TimeInParty = value;
			}
		}
	}

	private EternityTimeInterval TimeInCombat
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker && value != null)
			{
				m_StatTracker.TimeInCombat = value;
			}
		}
	}

	private float m_TotalDamageDone
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.TotalDamageDone = value;
			}
		}
	}

	private float m_MaxSingleTargetDamage
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.MaxSingleTargetDamage = value;
			}
		}
	}

	private int m_EnemiesDefeated
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.EnemiesDefeated = value;
			}
		}
	}

	private float m_MaxGroupDamage
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.MaxGroupDamage = value;
			}
		}
	}

	private int m_CriticalHits
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.CriticalHits = value;
			}
		}
	}

	private int m_TotalHits
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.TotalHits = value;
			}
		}
	}

	private float m_DamageTaken
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.DamageTaken = value;
			}
		}
	}

	private int m_TimesKOed
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.TimesKOed = value;
			}
		}
	}

	private int m_MostPowerfulEnemyLevel
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.MostPowerfulEnemyLevel = value;
			}
		}
	}

	private DatabaseString m_MostPowerfulEnemyName
	{
		set
		{
			CheckStatComponent();
			if ((bool)m_StatTracker)
			{
				m_StatTracker.MostPowerfulEnemyName = value;
			}
		}
	}

	[Persistent]
	public bool IsActiveInParty
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public bool IsControllable
	{
		get
		{
			if ((!m_health || (!m_health.Dead && !m_health.Unconscious)) && base.gameObject.activeInHierarchy)
			{
				return base.enabled;
			}
			return false;
		}
	}

	public GenericAbility QueuedAbility { get; set; }

	private Dictionary<KeyControl, int> AbilityHotkeys
	{
		set
		{
			CharacterHotkeyBindings.Get(base.gameObject).AbilityHotkeys = value;
		}
	}

	public AIState DestinationCircleState
	{
		get
		{
			return m_destinationCircleState;
		}
		set
		{
			m_destinationCircleState = value;
		}
	}

	public Vector3 DestinationCirclePosition
	{
		get
		{
			return m_destinationCirclePosition;
		}
		set
		{
			m_destinationCirclePosition = value;
		}
	}

	public Vector3 DesiredRotationPosition
	{
		get
		{
			return m_desiredFormationPosition;
		}
		set
		{
			m_desiredFormationPosition = value;
		}
	}

	public bool Selected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			if (value == m_isSelected || (value && (!IsControllable || IsFactionSwapped())))
			{
				return;
			}
			if (value)
			{
				if (!m_isSelected)
				{
					m_selectionCount++;
				}
				m_lastSelectedSlot = Slot;
				SelectedPartyMembers[Slot] = base.gameObject;
			}
			else if (m_isSelected)
			{
				m_selectionCount--;
			}
			if (!value)
			{
				SelectedPartyMembers[Slot] = null;
			}
			m_isSelected = value;
			if (!m_isSelected)
			{
				HighlightCharacter component = GetComponent<HighlightCharacter>();
				if ((bool)component)
				{
					component.LassoSelected = false;
				}
			}
			if (this.OnSelectionChanged != null)
			{
				this.OnSelectionChanged(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public bool DragSelected
	{
		get
		{
			return m_dragSelected;
		}
		set
		{
			m_dragSelected = value;
			Selected = value;
		}
	}

	public int Slot
	{
		get
		{
			return AssignedSlot;
		}
		set
		{
			if (value >= 30)
			{
				return;
			}
			if (value < 0)
			{
				value = GetDesiredSlot();
			}
			if (value < 0)
			{
				Debug.LogError("Party Member AI: Could not assign a slot to this object.", this);
				return;
			}
			if (AssignedSlot >= 0 && PartyMembers[AssignedSlot] == this)
			{
				PartyMembers[AssignedSlot] = null;
			}
			if (PartyMembers[value] != null)
			{
				Debug.LogError(string.Concat(base.name, " was assigned ", PartyMembers[value], "'s slot! Reassigning."), base.gameObject);
				value = GetDesiredSlot();
				if (PartyMembers[value] != null)
				{
					Debug.LogError("PROGRAMMER ERROR!! GetDesiredSlot isn't working correctly!" + base.name + " got assigned an in-use slot!", base.gameObject);
				}
			}
			PartyMembers[value] = this;
			if (Selected)
			{
				if (s_selectedPartyMembers[AssignedSlot] == base.gameObject)
				{
					s_selectedPartyMembers[AssignedSlot] = null;
				}
				s_selectedPartyMembers[value] = base.gameObject;
			}
			m_IsInSlot = true;
			AssignedSlot = value;
			if (!Secondary)
			{
				foreach (GameObject summonedCreature in base.SummonedCreatureList)
				{
					if (summonedCreature != null)
					{
						PartyMemberAI component = summonedCreature.GetComponent<PartyMemberAI>();
						if ((bool)component)
						{
							component.Slot = -1;
						}
					}
				}
				if (AssignedSlot >= 0 && AssignedSlot < 6)
				{
					SpecialCharacterInstanceID.Add(base.gameObject, SpecialCharacterInstanceID.s_slotGuids[AssignedSlot]);
				}
			}
			if (AssignedSlot < 12)
			{
				RecalculateFormationIndexOrder();
			}
		}
	}

	public static GameObject[] SelectedPartyMembers => s_selectedPartyMembers;

	public static int NextAvailablePrimarySlot
	{
		get
		{
			for (int i = 0; i < 6; i++)
			{
				if (PartyMembers[i] == null)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int MyAnimalSlot => Slot + 6;

	public override float PerceptionDistance
	{
		get
		{
			if (!IsControllable)
			{
				return 0f;
			}
			return base.PerceptionDistance;
		}
	}

	public GenericAbility CurrentAbility
	{
		get
		{
			GenericAbility result = null;
			AIState currentState = m_ai.CurrentState;
			if (currentState != null)
			{
				result = currentState.CurrentAbility;
			}
			return result;
		}
	}

	public event EventHandler OnSelectionChanged;

	public static event EventHandler OnPartyMembersChanged;

	public static event EventHandler OnAnySelectionChanged;

	public static event GameInputEventHandle OnPartyMemberPermaDeath;

	private static void OnSubSelectionChanged(object sender, EventArgs e)
	{
		try
		{
			if (PartyMemberAI.OnAnySelectionChanged != null)
			{
				PartyMemberAI.OnAnySelectionChanged(sender, e);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void NotifyPartyMemberPermaDeath(PartyMemberAI partyMember)
	{
		if ((bool)partyMember && PartyMemberAI.OnPartyMemberPermaDeath != null)
		{
			PartyMemberAI.OnPartyMemberPermaDeath(partyMember.gameObject, null);
		}
	}

	public static void Reset()
	{
		s_selectedPartyMembers = new GameObject[30];
		PartyMembers = new PartyMemberAI[30];
		s_selectedPartyMembersList.Clear();
		m_selectionCount = 0;
		m_lastSelectedSlot = 0;
		SafeEnableDisable = false;
		for (int i = 0; i < SpecialCharacterInstanceID.s_skillCheckGuids.Length; i++)
		{
			InstanceID.RemoveSpecialObjectID(SpecialCharacterInstanceID.GetSkillCheckGuid(i));
		}
		PartyMemberAI.OnPartyMembersChanged = null;
	}

	public static void SelectAll()
	{
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if ((bool)partyMemberAI)
			{
				partyMemberAI.DragSelected = true;
				HighlightCharacter component = partyMemberAI.GetComponent<HighlightCharacter>();
				if ((bool)component)
				{
					component.LassoSelected = false;
				}
			}
		}
	}

	public static void PopAllStates()
	{
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!partyMemberAI)
			{
				continue;
			}
			partyMemberAI.StateManager.ClearQueuedStates();
			if (!partyMemberAI.StateManager.IsExecutingDefaultState())
			{
				AIState currentState = partyMemberAI.StateManager.CurrentState;
				if (currentState != null && currentState.Priority < 6)
				{
					partyMemberAI.StateManager.PopAllStates();
				}
			}
			partyMemberAI.HideDestination();
			partyMemberAI.HideDestinationTarget();
		}
	}

	private void CheckStatComponent()
	{
		if (!m_StatTracker)
		{
			m_StatTracker = GetComponent<PartyMemberStats>();
		}
		if (!m_StatTracker)
		{
			m_StatTracker = base.gameObject.AddComponent<PartyMemberStats>();
		}
		if ((bool)m_StatTracker)
		{
			m_StatTracker.enabled = base.enabled;
		}
	}

	public void BindHotkey(KeyControl hotkey, GenericAbility ability)
	{
		if (!m_Hotkeys)
		{
			m_Hotkeys = CharacterHotkeyBindings.Get(base.gameObject);
		}
		m_Hotkeys.BindHotkey(hotkey, ability);
	}

	public KeyControl GetHotkeyFor(GenericAbility ability)
	{
		if (!m_Hotkeys)
		{
			m_Hotkeys = CharacterHotkeyBindings.Get(base.gameObject);
		}
		return m_Hotkeys.GetHotkeyFor(ability);
	}

	public override void InitAI()
	{
		if (m_ai == null)
		{
			m_ai = AIStateManager.StateManagerPool.Allocate();
			m_ai.Owner = base.gameObject;
			m_ai.AIController = this;
		}
		InitMover();
		m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<AI.Player.Wait>());
		if (InstructionSet == null)
		{
			InitInstructionSet();
		}
	}

	public void SetInstructionSetIndex(int index)
	{
		if (index >= 0)
		{
			InstructionSetIndex = index;
			InitInstructionSet();
		}
	}

	public void ClearInstructionSet()
	{
		InstructionSetIndex = -1;
		InitInstructionSet();
	}

	public override void Awake()
	{
		base.Awake();
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	public override void Start()
	{
		base.Start();
		CheckStatComponent();
		CharacterStats component = GetComponent<CharacterStats>();
		if (component == null)
		{
			Debug.LogError("PartyMemberAI has no CharacterStats (required for stat tracking).");
		}
		else
		{
			component.OnDamageFinal += StatsOnHit;
		}
		if (!m_health)
		{
			m_health = GetComponent<Health>();
		}
		if (m_health == null)
		{
			Debug.LogError("PartyMemberAI has no Health (required for stat tracking).");
		}
		else
		{
			m_health.OnKill += StatsOnKill;
			m_health.OnUnconscious += StatsOnUnconscious;
			m_health.OnDamaged += StatsOnDamaged;
		}
		if (Inventory == null)
		{
			Inventory = GetComponent<Inventory>();
		}
		if (Inventory == null)
		{
			Inventory = base.gameObject.AddComponent<Inventory>();
		}
		Inventory.AttachedCharacter = component;
		if (QuickbarInventory == null)
		{
			QuickbarInventory = GetComponent<QuickbarInventory>();
		}
		if (QuickbarInventory == null)
		{
			QuickbarInventory = base.gameObject.AddComponent<QuickbarInventory>();
		}
		GameState.PersistAcrossSceneLoadsTracked(base.gameObject);
		Persistence component2 = GetComponent<Persistence>();
		if ((bool)component2)
		{
			component2.UnloadsBetweenLevels = false;
		}
		base.gameObject.name = base.gameObject.name + "_" + Slot;
		m_DestinationCircle = UnityEngine.Object.Instantiate(InGameHUD.Instance.DestinationCircle).GetComponent<DestinationCircle>();
		m_DestinationCircle.Set(this);
		GameState.PersistAcrossSceneLoadsTracked(m_DestinationCircle.gameObject);
		m_TargetCircle = UnityEngine.Object.Instantiate(InGameHUD.Instance.TargetCircle);
		GameState.PersistAcrossSceneLoadsTracked(m_TargetCircle);
		m_TargetCircle.GetComponent<TargetCircle>().Set(this);
		m_TargetCircle.SetActive(value: false);
		if (m_mover != null)
		{
			m_TargetCircle.transform.localScale *= m_mover.Radius;
			m_mover.OnFrozenChanged -= OnFrozenMovementChanged;
			m_mover.OnFrozenChanged += OnFrozenMovementChanged;
		}
		CreateFogRevealer();
		m_SoundSet = GetComponent<SoundSetComponent>();
		if (m_SoundSet == null)
		{
			m_SoundSet = base.gameObject.AddComponent<SoundSetComponent>();
		}
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		VerifyStartPosition();
	}

	private void StatsOnHit(GameObject source, CombatEventArgs args)
	{
		CheckStatComponent();
		if ((bool)m_StatTracker)
		{
			m_StatTracker.NotifyHit(source, args);
		}
	}

	private void StatsOnKill(GameObject me, GameEventArgs args)
	{
		CheckStatComponent();
		if ((bool)m_StatTracker)
		{
			m_StatTracker.NotifyKill(me, args);
		}
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumEnemiesKilled);
		}
	}

	private void StatsOnUnconscious(GameObject me, GameEventArgs args)
	{
		CheckStatComponent();
		if ((bool)m_StatTracker)
		{
			m_StatTracker.NotifyUnconscious(me, args);
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember.gameObject == me)
			{
				if ((bool)AchievementTracker.Instance)
				{
					AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumPartyMemberKnockouts);
				}
				break;
			}
		}
	}

	private void StatsOnDamaged(GameObject me, GameEventArgs args)
	{
		CheckStatComponent();
		if ((bool)m_StatTracker)
		{
			m_StatTracker.NotifyDamaged(me, args);
		}
	}

	public override void OnEnable()
	{
		if (!SafeEnableDisable)
		{
			base.OnEnable();
			if (base.Mover != null)
			{
				base.Mover.AIController = this;
			}
		}
		CheckStatComponent();
		if (m_StatTracker != null)
		{
			m_StatTracker.enabled = true;
		}
		CharacterStats component = GetComponent<CharacterStats>();
		if ((bool)component)
		{
			component.IsPartyMember = true;
			for (CharacterStats.DefenseType defenseType = CharacterStats.DefenseType.Deflect; defenseType < CharacterStats.DefenseType.Count; defenseType++)
			{
				component.RevealDefense(defenseType);
			}
			component.RevealDT(DamagePacket.DamageType.All);
		}
		OnSelectionChanged += OnSubSelectionChanged;
	}

	public override void OnDisable()
	{
		if (base.gameObject == null)
		{
			return;
		}
		OnSelectionChanged -= OnSubSelectionChanged;
		if (!SafeEnableDisable)
		{
			CharacterStats component = GetComponent<CharacterStats>();
			if ((bool)component)
			{
				component.IsPartyMember = false;
			}
			base.OnDisable();
		}
		CheckStatComponent();
		if (m_StatTracker != null)
		{
			m_StatTracker.enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		if (m_revealer != null && (bool)FogOfWar.Instance)
		{
			FogOfWar.Instance.RemoveRevealer(m_revealer);
			m_revealer = null;
		}
		if (m_TargetCircle != null)
		{
			GameState.DestroyTrackedObject(m_TargetCircle);
		}
		if (m_DestinationCircle != null)
		{
			GameState.DestroyTrackedObject(m_DestinationCircle.gameObject);
		}
		if (m_mover != null)
		{
			m_mover.OnFrozenChanged -= OnFrozenMovementChanged;
		}
		CharacterStats component = GetComponent<CharacterStats>();
		if ((bool)component)
		{
			component.OnDamageFinal -= StatsOnHit;
		}
		if ((bool)m_health)
		{
			m_health.OnKill -= StatsOnKill;
			m_health.OnUnconscious -= StatsOnUnconscious;
			m_health.OnDamaged -= StatsOnDamaged;
			m_health.OnRevived -= OnPartyMemberRevive;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Restored()
	{
		Persistence component = GetComponent<Persistence>();
		if ((bool)component && string.IsNullOrEmpty(component.Prefab))
		{
			PersistenceManager.RemoveObject(component);
			GameUtilities.Destroy(base.gameObject);
			return;
		}
		Faction component2 = GetComponent<Faction>();
		if (base.enabled && (bool)component2 && !component2.CurrentTeamInstance)
		{
			component2.CurrentTeamInstance = ReputationManager.Instance.PlayerTeamPrefab;
		}
		InitInstructionSet();
		if (IsActiveInParty)
		{
			if (Secondary)
			{
				AddSummonToActiveParty(base.gameObject, base.Summoner, base.SummonType, fromScript: false);
			}
			else
			{
				AddToActiveParty(base.gameObject, fromScript: false);
			}
		}
	}

	public void VerifyStartPosition()
	{
		if (GameUtilities.IsPositionOnNavMesh(base.transform.position))
		{
			return;
		}
		if (StartPoint.s_ChosenStartPoint != null)
		{
			StartPoint.s_ChosenStartPoint.SpawnPartyHere();
			Debug.LogError("Error: Party Member not found on Nav Mesh on Load. Moving Party Member to chosen Start Position.");
			return;
		}
		SceneTransition sceneTransition = StartPoint.FindClosestSceneTransition(base.transform.position);
		if ((bool)sceneTransition)
		{
			NavMesh.SamplePosition(sceneTransition.transform.position, out var hit, 100f, -1);
			base.transform.position = GameUtilities.NearestUnoccupiedLocation(hit.position, 1f, 12f, GetComponent<Mover>());
			Debug.LogError("Error: Party Member not found on Nav Mesh on Load. Moving Party Member to nearest Scene Transition.");
		}
		else
		{
			NavMesh.SamplePosition(base.transform.position, out var hit2, 100f, -1);
			base.transform.position = GameUtilities.NearestUnoccupiedLocation(hit2.position, 1f, 12f, GetComponent<Mover>());
			Debug.LogError("Error: Party Member not found on Nav Mesh on Load. Moving Party Member to nearest Nav Position.");
		}
	}

	public static string GetPartyDebugOutput()
	{
		StringBuilder stringBuilder = new StringBuilder("-- Party Debug --");
		for (int i = 0; i < PartyMembers.Length; i++)
		{
			if (!(PartyMembers[i] != null))
			{
				continue;
			}
			stringBuilder.AppendLine();
			stringBuilder.Append(" ");
			stringBuilder.Append(i);
			stringBuilder.Append(" ");
			stringBuilder.Append(PartyMembers[i].gameObject.name);
			stringBuilder.Append(", ");
			stringBuilder.Append(PartyMembers[i].Slot);
			stringBuilder.Append(", ");
			stringBuilder.Append(PartyMembers[i].DesiredFormationIndex);
			if ((bool)PartyMembers[i].Summoner)
			{
				stringBuilder.Append(" s: ");
				stringBuilder.Append(PartyMembers[i].Summoner.name);
			}
			foreach (GameObject summonedCreature in PartyMembers[i].SummonedCreatureList)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("       ");
				stringBuilder.Append(summonedCreature.name);
			}
		}
		return stringBuilder.ToString();
	}

	public override void Update()
	{
		if (m_mover != null && m_mover.AIController == null)
		{
			m_mover.AIController = this;
		}
		if (m_instructionTimer > 0f)
		{
			m_instructionTimer -= Time.deltaTime;
		}
		if (m_instructions != null)
		{
			for (int i = 0; i < m_instructions.Count; i++)
			{
				m_instructions[i].Update();
			}
		}
		if (GameState.s_playerCharacter != null && base.gameObject == GameState.s_playerCharacter.gameObject && DebugParty)
		{
			UIDebug.Instance.SetText("Party Debug", GetPartyDebugOutput(), Color.cyan);
			UIDebug.Instance.SetTextPosition("Party Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
		}
		if (m_destinationCircleState != null)
		{
			if (base.StateManager.IsStateInStack(m_destinationCircleState))
			{
				ShowDestination(m_destinationCirclePosition);
			}
			else
			{
				m_destinationCircleState = null;
				HideDestination();
			}
		}
		if (GameState.s_playerCharacter != null && GameState.s_playerCharacter.RotatingFormation && Selected)
		{
			ShowDestinationTarget(m_desiredFormationPosition);
		}
		else
		{
			HideDestinationTarget();
		}
		if (m_revealer != null)
		{
			m_revealer.WorldPos = base.gameObject.transform.position;
			m_revealer.RequiresRefresh = false;
		}
		else
		{
			CreateFogRevealer();
		}
		if (GameState.Paused)
		{
			CheckForNullEngagements();
			if (m_ai != null)
			{
				m_ai.Update();
			}
		}
		else
		{
			if (m_ai == null)
			{
				return;
			}
			if (GameState.Option.AutoPause.IsEventSet(AutoPauseOptions.PauseEvent.EnemySpotted))
			{
				UpdateEnemySpotted();
			}
			if (QueuedAbility != null && QueuedAbility.Ready)
			{
				AIState currentState = m_ai.CurrentState;
				Consumable component = QueuedAbility.GetComponent<Consumable>();
				if (component != null && component.IsFoodDrugOrPotion)
				{
					ConsumePotion consumePotion = m_ai.QueuedState as ConsumePotion;
					if (!(currentState is ConsumePotion) && (consumePotion == null || currentState.Priority < 1))
					{
						ConsumePotion consumePotion2 = AIStateManager.StatePool.Allocate<ConsumePotion>();
						base.StateManager.PushState(consumePotion2);
						consumePotion2.Ability = QueuedAbility;
						consumePotion2.ConsumeAnimation = component.AnimationVariation;
						AttackBase primaryAttack = GetPrimaryAttack();
						if (!(primaryAttack is AttackMelee) || !(primaryAttack as AttackMelee).Unarmed)
						{
							consumePotion2.HiddenObjects = primaryAttack.GetComponentsInChildren<Renderer>();
						}
					}
				}
				else
				{
					AI.Achievement.Attack attack = currentState as AI.Achievement.Attack;
					TargetedAttack targetedAttack = currentState as TargetedAttack;
					if (QueuedAbility.Passive || (attack == null && targetedAttack == null))
					{
						QueuedAbility.Activate(currentState.Owner);
					}
					else if (targetedAttack == null || (!QueuedAbility.UsePrimaryAttack && !QueuedAbility.UseFullAttack))
					{
						Ability ability = AIStateManager.StatePool.Allocate<Ability>();
						ability.QueuedAbility = QueuedAbility;
						if (attack != null)
						{
							if (attack.CanCancel)
							{
								attack.OnCancel();
								base.StateManager.PopCurrentState();
								base.StateManager.PushState(ability);
							}
							else
							{
								base.StateManager.QueueStateAtTop(ability);
							}
						}
						else
						{
							base.StateManager.PushState(ability);
						}
					}
				}
				QueuedAbility = null;
			}
			base.Update();
			if (!GameState.IsLoading && !(GameState.s_playerCharacter == null))
			{
				if (m_alphaControl != null && m_alphaControl.Alpha < float.Epsilon)
				{
					m_alphaControl.Alpha = 1f;
				}
				if (m_mover != null && !GameState.InCombat)
				{
					float newValue = (Stealth.IsInStealthMode(base.gameObject) ? 2f : 4f);
					m_mover.UseCustomSpeed(newValue);
				}
			}
		}
	}

	protected override string BuildDebugText(string text)
	{
		return base.BuildDebugText(text + "Slot: " + Slot);
	}

	public static void EnsurePartyMemberSelected()
	{
		for (int i = 0; i < 30; i++)
		{
			if (PartyMembers[i] != null && PartyMembers[i].IsControllable)
			{
				PartyMembers[i].Selected = true;
				if (PartyMembers[i].Selected)
				{
					break;
				}
			}
		}
	}

	public void ProcessSelection()
	{
		if (IsFactionSwapped())
		{
			if (Selected)
			{
				Selected = false;
			}
			return;
		}
		if (m_selectionCount == 0 && Slot == m_lastSelectedSlot)
		{
			Selected = true;
			if (!Selected)
			{
				m_lastSelectedSlot = ((Slot == 0) ? 1 : 0);
			}
		}
		if (GameInput.GetControlUp(MappedControl.SELECT))
		{
			if (m_gotUsabilityEvent)
			{
				m_gotUsabilityEvent = false;
				return;
			}
			bool flag = GameInput.GetControl(MappedControl.MULTISELECT) || GameInput.GetControl(MappedControl.MULTISELECT_NEGATIVE);
			if (GameCursor.CharacterUnderCursor == base.gameObject)
			{
				if (Selected && flag && m_selectionCount > 1)
				{
					Selected = false;
				}
				else
				{
					if (flag)
					{
						Selected = true;
					}
					else
					{
						ExclusiveSelect();
					}
					if (Selected && !base.IsUnconscious)
					{
						SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.Selected, SoundSet.s_VeryShortVODelay, forceInterrupt: false);
					}
				}
			}
		}
		if (SelectedPartyMembers.Length > 1)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.MULTIPLE_CHARACTERS_SELECTED);
		}
		if (!IsControllable)
		{
			Selected = false;
		}
	}

	public void SuspendDestination()
	{
		if (m_TargetCircle != null)
		{
			m_TargetCircle.SetActive(value: false);
		}
		if (m_DestinationCircle != null)
		{
			m_DestSuspended = m_DestinationCircle.Visible;
			m_DestinationCircle.Visible = false;
		}
	}

	public void ReinstateDestination()
	{
		if (m_DestinationCircle != null && m_destinationCircleState != null && base.StateManager.IsStateInStack(m_destinationCircleState))
		{
			m_DestinationCircle.Visible = m_DestSuspended;
		}
	}

	protected void ShowDestination(Vector3 position)
	{
		if (m_DestinationCircle != null && InGameHUD.Instance.ShowHUD)
		{
			position += new Vector3(0f, 0.05f, 0f);
			if (position != m_DestinationCircle.transform.position)
			{
				m_DestinationCircle.StartAt(position);
			}
		}
	}

	public void HideDestination()
	{
		if (m_DestinationCircle != null)
		{
			m_DestinationCircle.Visible = false;
		}
	}

	protected void ShowDestinationTarget(Vector3 position)
	{
		if (m_TargetCircle != null && InGameHUD.Instance.ShowHUD)
		{
			m_TargetCircle.SetActive(value: true);
			m_TargetCircle.transform.position = position + new Vector3(0f, 0.05f, 0f);
			m_TargetCircle.transform.rotation = GameState.s_playerCharacter.FormationRotation;
		}
	}

	public void HideDestinationTarget()
	{
		if (m_TargetCircle != null)
		{
			m_TargetCircle.SetActive(value: false);
		}
	}

	public Vector3 CalculateFormationPosition(Vector3 destination, bool ignoreSelection, out int selectedSlot)
	{
		SortedList sortedList = new SortedList();
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && (partyMemberAI.Selected || ignoreSelection) && partyMemberAI.DesiredFormationIndex >= 0)
			{
				sortedList.Add(partyMemberAI.DesiredFormationIndex, partyMemberAI);
			}
		}
		if (DesiredFormationIndex < 0)
		{
			if (base.Summoner != null)
			{
				return base.Summoner.GetComponent<PartyMemberAI>().CalculateFormationPosition(destination, ignoreSelection, out selectedSlot);
			}
			selectedSlot = 0;
			return destination;
		}
		if (sortedList.Count < 2)
		{
			selectedSlot = 0;
			return destination;
		}
		int num = Slot;
		if (!ignoreSelection)
		{
			for (int j = 0; j < sortedList.Count; j++)
			{
				if (sortedList.GetByIndex(j).Equals(this))
				{
					num = j;
					break;
				}
			}
		}
		selectedSlot = num;
		Vector3 vector = FormationData.Instance.GetFormation(FormationStyle)[num];
		Vector3 vector2;
		if (GameState.s_playerCharacter != null && GameState.s_playerCharacter.RotatingFormation && GameState.s_playerCharacter.FormationRotated)
		{
			vector2 = GameState.s_playerCharacter.FormationRotation * vector;
		}
		else
		{
			int selectedLeaderSlot = GetSelectedLeaderSlot();
			Vector3 position = SelectedPartyMembers[selectedLeaderSlot].transform.position;
			Vector3 vector3 = destination;
			position.y = (vector3.y = 0f);
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, vector3 - position);
			vector2 = quaternion * vector;
			if ((bool)GameState.s_playerCharacter)
			{
				GameState.s_playerCharacter.FormationRotated = true;
				GameState.s_playerCharacter.FormationRotation = quaternion;
			}
		}
		return vector2 + destination;
	}

	public void ResolveDesiredRotationPosition(Vector3 formationRotationPickPosition, List<PartyMemberAI> anchoredPartyMembers)
	{
		if (NavMesh.Raycast(formationRotationPickPosition, m_desiredFormationPosition, out var hit, int.MaxValue))
		{
			m_desiredFormationPosition = hit.position;
		}
		bool flag = false;
		float radius = m_mover.Radius;
		foreach (PartyMemberAI anchoredPartyMember in anchoredPartyMembers)
		{
			float num = anchoredPartyMember.m_mover.Radius + radius;
			if ((m_desiredFormationPosition - anchoredPartyMember.m_desiredFormationPosition).sqrMagnitude < num * num)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		float num2 = float.MaxValue;
		Vector3 desiredFormationPosition = m_desiredFormationPosition;
		foreach (PartyMemberAI anchoredPartyMember2 in anchoredPartyMembers)
		{
			Vector3[] array = s_resolveOffsets;
			foreach (Vector3 vector in array)
			{
				Vector3 vector2 = anchoredPartyMember2.m_desiredFormationPosition + vector * radius;
				flag = false;
				float sqrMagnitude = (m_desiredFormationPosition - vector2).sqrMagnitude;
				if (sqrMagnitude > num2)
				{
					continue;
				}
				foreach (PartyMemberAI anchoredPartyMember3 in anchoredPartyMembers)
				{
					if (!(anchoredPartyMember3 == anchoredPartyMember2))
					{
						float num3 = anchoredPartyMember3.m_mover.Radius + radius;
						if ((vector2 - anchoredPartyMember3.m_desiredFormationPosition).sqrMagnitude < num3 * num3)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag && GameUtilities.IsPositionOnNavMesh(vector2))
				{
					desiredFormationPosition = vector2;
					num2 = sqrMagnitude;
				}
			}
		}
		m_desiredFormationPosition = desiredFormationPosition;
	}

	public void AdjustDestinationPosition(Vector3 pickCenter, ref Vector3 destination)
	{
		float radius = m_mover.Radius;
		bool flag = false;
		int num = 0;
		do
		{
			num++;
			flag = false;
			for (int i = 0; i < Slot; i++)
			{
				PartyMemberAI partyMemberAI = PartyMembers[i];
				if (!(partyMemberAI == null) && !(partyMemberAI == this) && !(partyMemberAI.m_mover == null))
				{
					Vector3 a = partyMemberAI.m_mover.transform.position;
					if (partyMemberAI.Selected)
					{
						a = partyMemberAI.CalculateFormationPosition(pickCenter, ignoreSelection: false, out var _);
					}
					else if (partyMemberAI.m_mover.HasGoal)
					{
						a = partyMemberAI.m_mover.Goal;
					}
					if (Vector3.Distance(a, destination) < radius * 2f && (destination - base.transform.position).sqrMagnitude > float.Epsilon)
					{
						Vector3 vector = (base.transform.position - destination).normalized * radius * 2f;
						destination += vector;
						flag = true;
					}
				}
			}
		}
		while (flag && num < 2);
	}

	public void CreateFogRevealer()
	{
		if (FogOfWar.Instance != null)
		{
			if (m_revealer != null)
			{
				FogOfWar.Instance.RemoveRevealer(m_revealer);
			}
			m_revealer = FogOfWar.Instance.AddRevealer(triggersBoxColliders: true, 13f, base.transform.position, null, revealOnly: false, respectLOS: true);
			m_revealer.RequiresRefresh = true;
		}
		else
		{
			m_revealer = null;
		}
	}

	public static int GetSelectedLeaderSlot()
	{
		for (int i = 0; i < 30; i++)
		{
			if ((bool)s_selectedPartyMembers[i])
			{
				return i;
			}
		}
		return 0;
	}

	public static int NumSelectedMembers()
	{
		int num = 0;
		for (int i = 0; i < s_selectedPartyMembers.Length; i++)
		{
			if (s_selectedPartyMembers[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public static int NumMembers()
	{
		int num = 0;
		for (int i = 0; i < PartyMembers.Length; i++)
		{
			if (PartyMembers[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public static GameObject GetBestMember(Func<GameObject, IComparable> orderBy, Vector3 position, bool selectedOnly)
	{
		s_bestMembers.Clear();
		if (selectedOnly)
		{
			for (int i = 0; i < 6; i++)
			{
				if (SelectedPartyMembers[i] != null)
				{
					s_bestMembers.Add(SelectedPartyMembers[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < 6; j++)
			{
				if (PartyMembers[j] != null)
				{
					s_bestMembers.Add(PartyMembers[j].gameObject);
				}
			}
		}
		IComparable max = s_bestMembers.Max(orderBy);
		IEnumerable<GameObject> source = s_bestMembers.Where((GameObject go) => orderBy(go).CompareTo(max) == 0);
		switch (source.Count())
		{
		case 0:
		{
			PartyMemberAI closestPrimaryMember = GetClosestPrimaryMember(position, selectedOnly);
			if (closestPrimaryMember != null)
			{
				return closestPrimaryMember.gameObject;
			}
			return null;
		}
		case 1:
			return source.First();
		default:
			return source.OrderBy((GameObject go) => (position - go.transform.position).sqrMagnitude).First();
		}
	}

	public static PartyMemberAI GetClosestMember(Vector3 position, bool selectedOnly)
	{
		PartyMemberAI result = null;
		float num = float.MaxValue;
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && (!selectedOnly || partyMemberAI.Selected) && partyMemberAI.SummonType == AISummonType.NotSummoned && (selectedOnly || partyMemberAI.Selected || !partyMemberAI.InCombat) && partyMemberAI.IsControllable)
			{
				float sqrMagnitude = (position - partyMemberAI.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = partyMemberAI;
				}
			}
		}
		return result;
	}

	public static PartyMemberAI GetClosestPrimaryMember(Vector3 position, bool selectedOnly)
	{
		PartyMemberAI result = null;
		float num = float.MaxValue;
		for (int i = 0; i < 6; i++)
		{
			PartyMemberAI partyMemberAI = PartyMembers[i];
			if (!(partyMemberAI == null) && (!selectedOnly || partyMemberAI.Selected) && partyMemberAI.SummonType == AISummonType.NotSummoned && (selectedOnly || partyMemberAI.Selected || !partyMemberAI.InCombat) && partyMemberAI.IsControllable)
			{
				float sqrMagnitude = (position - partyMemberAI.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = partyMemberAI;
				}
			}
		}
		return result;
	}

	public override void OnEvent(GameEventArgs args)
	{
		base.OnEvent(args);
		if (args.Type == GameEventType.UsableClicked)
		{
			m_gotUsabilityEvent = true;
		}
	}

	public void ExclusiveSelect()
	{
		Selected = true;
		if (!Selected)
		{
			return;
		}
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				partyMemberAI.Selected = partyMemberAI == this;
			}
		}
	}

	public AttackBase GetPrimaryAttack()
	{
		Equipment component = base.StateManager.Owner.GetComponent<Equipment>();
		if (component != null)
		{
			AttackBase primaryAttack = component.PrimaryAttack;
			if (primaryAttack != null)
			{
				return primaryAttack;
			}
		}
		if (GetSecondaryAttack() == null)
		{
			return base.StateManager.Owner.GetComponent<AttackBase>();
		}
		return null;
	}

	public AttackBase GetSecondaryAttack()
	{
		Equipment component = base.StateManager.Owner.GetComponent<Equipment>();
		if (component != null)
		{
			return component.SecondaryAttack;
		}
		return null;
	}

	public void SwapWith(int otherSlot)
	{
		if (otherSlot < 0 || otherSlot >= PartyMembers.Length || otherSlot == AssignedSlot)
		{
			return;
		}
		int assignedSlot = AssignedSlot;
		PartyMembers[otherSlot].AssignedSlot = AssignedSlot;
		PartyMembers[assignedSlot] = PartyMembers[otherSlot];
		PartyMembers[otherSlot] = this;
		GameObject gameObject = s_selectedPartyMembers[assignedSlot];
		s_selectedPartyMembers[assignedSlot] = s_selectedPartyMembers[otherSlot];
		s_selectedPartyMembers[otherSlot] = gameObject;
		AssignedSlot = otherSlot;
		for (int i = 6; i < 30; i += 6)
		{
			int num = i + assignedSlot;
			int num2 = i + otherSlot;
			if ((bool)PartyMembers[num2])
			{
				PartyMembers[num2].AssignedSlot = num;
			}
			if ((bool)PartyMembers[num])
			{
				PartyMembers[num].AssignedSlot = num2;
			}
			PartyMemberAI partyMemberAI = PartyMembers[num];
			PartyMembers[num] = PartyMembers[num2];
			PartyMembers[num2] = partyMemberAI;
			GameObject gameObject2 = s_selectedPartyMembers[num];
			s_selectedPartyMembers[num] = s_selectedPartyMembers[num2];
			s_selectedPartyMembers[num2] = gameObject2;
		}
		if (AssignedSlot >= 0 && AssignedSlot < 6)
		{
			SpecialCharacterInstanceID.Add(base.gameObject, SpecialCharacterInstanceID.s_slotGuids[AssignedSlot]);
		}
		RecalculateFormationIndexOrder();
	}

	public void SwapWith(PartyMemberAI other)
	{
		int otherSlot = -1;
		for (int i = 0; i < PartyMembers.Length; i++)
		{
			if (PartyMembers[i] == other)
			{
				otherSlot = i;
				break;
			}
		}
		SwapWith(otherSlot);
	}

	public static void Swap(PartyMemberAI first, PartyMemberAI second)
	{
		first.SwapWith(second);
	}

	public static bool IsInPartyList(PartyMemberAI partyMemberAI)
	{
		if (partyMemberAI == null)
		{
			return false;
		}
		for (int i = 0; i < 6; i++)
		{
			if (PartyMembers[i] == partyMemberAI)
			{
				return true;
			}
		}
		return false;
	}

	public static void CompressPartyMembers()
	{
		int num = -1;
		for (int i = 0; i < 6; i++)
		{
			if (PartyMembers[i] != null)
			{
				if (num >= 0)
				{
					PartyMembers[i].Slot = num;
					i = num;
					num = -1;
				}
			}
			else if (num < 0)
			{
				num = i;
			}
		}
		RecalculateFormationIndexOrder();
	}

	private int GetDesiredSlot()
	{
		int num = -1;
		if (Secondary)
		{
			if (base.Summoner == null)
			{
				base.Summoner = GameState.s_playerCharacter.gameObject;
			}
			PartyMemberAI component = base.Summoner.GetComponent<PartyMemberAI>();
			if (base.SummonType == AISummonType.AnimalCompanion)
			{
				return component.MyAnimalSlot;
			}
			return component.GetNextAvailableSummonSlot(this);
		}
		return NextAvailablePrimarySlot;
	}

	public static List<GameObject> GetSelectedPartyMembers()
	{
		s_selectedPartyMembersList.Clear();
		GameObject[] array = s_selectedPartyMembers;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				s_selectedPartyMembersList.Add(gameObject);
			}
		}
		return s_selectedPartyMembersList;
	}

	public static bool IsPrimaryPartyMemberSelected()
	{
		for (int i = 0; i < 6; i++)
		{
			if (s_selectedPartyMembers[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsSelectedPartyMember(GameObject obj)
	{
		for (int i = 0; i < 6; i++)
		{
			if (s_selectedPartyMembers[i] == obj)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPartyMemberUnconscious()
	{
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null && partyMemberAI.IsUnconsciousButNotDead)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNextAvailableSummonSlot(PartyMemberAI summon)
	{
		for (int i = 12 + Slot; i < 30; i += 6)
		{
			if (PartyMembers[i] == summon)
			{
				return i;
			}
		}
		for (int j = 12 + Slot; j < 30; j += 6)
		{
			if (PartyMembers[j] == null)
			{
				return j;
			}
		}
		return -1;
	}

	protected override void HandleCombatEnd(object sender, EventArgs e)
	{
		if (m_ai != null)
		{
			if (m_ai.CurrentState is AI.Achievement.Attack attack && attack.CanUserInterrupt())
			{
				attack.OnCancel();
				m_ai.PopState(attack);
			}
			base.HandleCombatEnd(sender, e);
		}
	}

	private static void OnPartyMemberRevive(GameObject myObject, GameEventArgs args)
	{
		GameState.GameOver = false;
		GameState.PartyDead = false;
	}

	public static PartyMemberAI AddToActiveParty(GameObject newPartyMember, bool fromScript)
	{
		for (int i = 0; i < PartyMembers.Length; i++)
		{
			if (PartyMembers[i] != null && PartyMembers[i].gameObject == newPartyMember)
			{
				return null;
			}
		}
		Health component = newPartyMember.GetComponent<Health>();
		if ((bool)component)
		{
			component.ShouldDecay = false;
			component.NeverGib = true;
			component.OnRevived += OnPartyMemberRevive;
		}
		int num = NextAvailablePrimarySlot;
		bool flag = false;
		PartyMemberAI partyMemberAI = newPartyMember.GetComponent<PartyMemberAI>();
		if (!partyMemberAI)
		{
			if (num < 0)
			{
				flag = true;
			}
			partyMemberAI = newPartyMember.AddComponent<PartyMemberAI>();
			partyMemberAI.AssignedSlot = num;
		}
		else
		{
			if (num < 0)
			{
				flag = true;
			}
			if (partyMemberAI.AssignedSlot >= 0)
			{
				num = partyMemberAI.AssignedSlot;
			}
			else
			{
				partyMemberAI.AssignedSlot = num;
			}
			partyMemberAI.enabled = true;
		}
		Persistence component2 = newPartyMember.GetComponent<Persistence>();
		if ((bool)component2)
		{
			component2.Mobile = true;
		}
		Equipment component3 = newPartyMember.GetComponent<Equipment>();
		if ((bool)component3)
		{
			component3.m_shouldSaveEquipment = true;
		}
		List<GameObject> list = new List<GameObject>();
		AIPackageController component4 = newPartyMember.GetComponent<AIPackageController>();
		if ((bool)component4)
		{
			list.AddRange(component4.SummonedCreatureList.ToArray());
			partyMemberAI.Summoner = component4.Summoner;
			partyMemberAI.SummonType = component4.SummonType;
			PersistenceManager.ClearComponentPacket(component4.gameObject, typeof(AIPackageController));
			GameUtilities.DestroyImmediate(component4);
		}
		if (!flag)
		{
			partyMemberAI.Slot = num;
			partyMemberAI.Selected = true;
		}
		Faction component5 = newPartyMember.GetComponent<Faction>();
		if ((bool)component5)
		{
			Faction component6 = GameState.s_playerCharacter.GetComponent<Faction>();
			component5.CurrentTeamInstance = component6.CurrentTeam;
		}
		UIOffscreenObjectManager.AddPointer(newPartyMember);
		partyMemberAI.m_alphaControl = newPartyMember.GetComponent<AlphaControl>();
		if (partyMemberAI.m_alphaControl == null)
		{
			partyMemberAI.m_alphaControl = newPartyMember.AddComponent<AlphaControl>();
		}
		partyMemberAI.m_alphaControl.Alpha = 1f;
		Inventory inventory = newPartyMember.GetComponent<Inventory>();
		if (inventory == null)
		{
			inventory = newPartyMember.AddComponent<Inventory>();
		}
		inventory.SetPartyMemberInventory(enabled: true);
		partyMemberAI.InterruptAnimationForCutscene();
		if (!partyMemberAI.AddedThroughScript && fromScript)
		{
			partyMemberAI.AddedThroughScript = fromScript;
		}
		CharacterStats component7 = newPartyMember.GetComponent<CharacterStats>();
		if ((bool)component7 && !GameState.IsLoading)
		{
			component7.ClearEffectFromAffliction("Fatigue");
			Console.AddMessage(Console.Format(GUIUtils.GetText(406, CharacterStats.GetGender(component7)), CharacterStats.NameColored(component7)));
			component7.AddSecondWind();
		}
		for (int j = 0; j < list.Count; j++)
		{
			GameObject gameObject = list[j];
			if (!flag && gameObject != null)
			{
				AddSummonToActiveParty(gameObject, newPartyMember, AISummonType.AnimalCompanion, fromScript);
			}
		}
		PresetProgression component8 = newPartyMember.GetComponent<PresetProgression>();
		if ((bool)component8)
		{
			if (!component8.PresetProgressionHandled() && (bool)AchievementTracker.Instance)
			{
				if ((bool)component2 && component2.ExportPackage == Persistence.AssetBundleExportPackage.X2)
				{
					AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumPX2PrimaryCompanionsGained);
				}
				else if ((bool)component2 && component2.ExportPackage == Persistence.AssetBundleExportPackage.X1)
				{
					AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumPX1PrimaryCompanionsGained);
				}
				else
				{
					AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumBaseGamePrimaryCompanionsGained);
				}
			}
			component8.HandlePresetProgression(forceFromStartingLevel: false);
		}
		if (!newPartyMember.GetComponent<Stealth>())
		{
			newPartyMember.AddComponent<Stealth>();
		}
		if (PartyMemberAI.OnPartyMembersChanged != null)
		{
			PartyMemberAI.OnPartyMembersChanged(newPartyMember, EventArgs.Empty);
		}
		if (flag)
		{
			UIPartyManager.Instance.AddingNewPartyMember = true;
			GameState.Stronghold.StoreCompanion(partyMemberAI.gameObject);
			UIWindowManager.Instance.SuspendFor(UIPartyManager.Instance);
			UIPartyManager.Instance.ShowWindow();
			UIPartyManager.Instance.AddingNewPartyMember = false;
		}
		PartyMemberAI component9 = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
		if ((bool)component9)
		{
			partyMemberAI.FormationStyle = component9.FormationStyle;
		}
		return partyMemberAI;
	}

	public static void AddSummonToActiveParty(GameObject newPartyMember, GameObject parentSummoner, AISummonType summonType, bool fromScript)
	{
		for (int i = 0; i < PartyMembers.Length; i++)
		{
			if (PartyMembers[i] != null && PartyMembers[i].gameObject == newPartyMember)
			{
				return;
			}
		}
		CharacterStats component = parentSummoner.GetComponent<CharacterStats>();
		Persistence component2 = newPartyMember.GetComponent<Persistence>();
		if ((bool)component2)
		{
			component2.Mobile = true;
		}
		else
		{
			component2 = newPartyMember.AddComponent<Persistence>();
			component2.Mobile = true;
			if (summonType == AISummonType.AnimalCompanion && (bool)component)
			{
				for (int j = 0; j < component.Abilities.Count; j++)
				{
					Summon component3 = component.Abilities[j].GetComponent<Summon>();
					if ((bool)component3 && component3.SummonType == AISummonType.AnimalCompanion)
					{
						component2.Prefab = component3.SummonFileList[0];
					}
				}
			}
		}
		Equipment component4 = newPartyMember.GetComponent<Equipment>();
		if ((bool)component4)
		{
			component4.m_shouldSaveEquipment = true;
		}
		AIPackageController component5 = newPartyMember.GetComponent<AIPackageController>();
		Health component6 = newPartyMember.GetComponent<Health>();
		if ((bool)component6)
		{
			component6.ShouldDecay = false;
		}
		int num = -1;
		PartyMemberAI partyMemberAI = newPartyMember.GetComponent<PartyMemberAI>();
		if (!partyMemberAI)
		{
			partyMemberAI = newPartyMember.AddComponent<PartyMemberAI>();
		}
		PartyMemberAI component7 = parentSummoner.GetComponent<PartyMemberAI>();
		if ((bool)component7 && !component7.SummonedCreatureList.Contains(newPartyMember))
		{
			component7.SummonedCreatureList.Add(newPartyMember);
		}
		partyMemberAI.Summoner = parentSummoner;
		partyMemberAI.SummonType = summonType;
		partyMemberAI.AssignedSlot = num;
		if (component5 != null)
		{
			PersistenceManager.ClearComponentPacket(component5.gameObject, typeof(AIPackageController));
			GameUtilities.DestroyImmediate(component5);
		}
		partyMemberAI.Secondary = true;
		partyMemberAI.Slot = num;
		partyMemberAI.enabled = true;
		if (summonType != AISummonType.AnimalCompanion)
		{
			partyMemberAI.Selected = component7.Selected;
		}
		else
		{
			partyMemberAI.Selected = true;
		}
		if (!partyMemberAI.AddedThroughScript && fromScript)
		{
			partyMemberAI.AddedThroughScript = fromScript;
		}
		Faction component8 = newPartyMember.GetComponent<Faction>();
		if ((bool)component8)
		{
			Faction component9 = parentSummoner.GetComponent<Faction>();
			component8.CurrentTeamInstance = component9.CurrentTeam;
		}
		if (newPartyMember.GetComponent<Inventory>() == null)
		{
			newPartyMember.AddComponent<Inventory>();
		}
		partyMemberAI.m_alphaControl = newPartyMember.GetComponent<AlphaControl>();
		if (partyMemberAI.m_alphaControl == null)
		{
			partyMemberAI.m_alphaControl = newPartyMember.AddComponent<AlphaControl>();
		}
		partyMemberAI.m_alphaControl.Alpha = 1f;
		if (summonType == AISummonType.AnimalCompanion)
		{
			UIOffscreenObjectManager.AddPointer(newPartyMember);
		}
		PresetProgression component10 = newPartyMember.GetComponent<PresetProgression>();
		if ((bool)component10)
		{
			component10.HandlePresetProgression(forceFromStartingLevel: false);
		}
		CharacterStats component11 = newPartyMember.GetComponent<CharacterStats>();
		if (parentSummoner != null && summonType == AISummonType.AnimalCompanion)
		{
			component11.LevelUpToLevel(component.Level);
		}
		if ((bool)component11 && !GameState.IsLoading)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetText(406, CharacterStats.GetGender(component11)), CharacterStats.NameColored(component11)));
		}
		if (!GameState.IsLoading && (bool)component11 && component11.CharacterClass == CharacterStats.Class.AnimalCompanion)
		{
			component11.AddNewClassAbilities();
		}
		if (!newPartyMember.GetComponent<Stealth>())
		{
			newPartyMember.AddComponent<Stealth>();
		}
		PartyMemberAI component12 = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
		if ((bool)component12)
		{
			partyMemberAI.FormationStyle = component12.FormationStyle;
		}
	}

	public static void RemoveFromActiveParty(PartyMemberAI partyMemberAI, bool purgePersistencePacket)
	{
		if (partyMemberAI == null)
		{
			Debug.LogError("RemoveFromActiveParty(): NULL PartyMemberAI passed in as parameter. Cannot continue.");
			return;
		}
		GameObject gameObject = partyMemberAI.gameObject;
		GameState.MarkTrackedObjectForDelayedDestroy(gameObject);
		CharacterStats component = gameObject.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(425), CharacterStats.NameColored(component)));
			component.RemoveAbility(component.FindAbilityInstance("Second_Wind"));
		}
		partyMemberAI.Selected = false;
		if (partyMemberAI.Slot >= 0 && partyMemberAI.Slot < 60)
		{
			PartyMembers[partyMemberAI.Slot] = null;
		}
		partyMemberAI.m_IsInSlot = false;
		partyMemberAI.AssignedSlot = -1;
		Health component2 = gameObject.GetComponent<Health>();
		if ((bool)component2)
		{
			component2.ShouldDecay = true;
			if (!component2.Dead)
			{
				component2.NeverGib = false;
			}
			component2.OnRevived -= OnPartyMemberRevive;
		}
		AIPackageController aIPackageController = gameObject.GetComponent<AIPackageController>();
		if (aIPackageController != null)
		{
			aIPackageController.enabled = true;
		}
		else
		{
			aIPackageController = gameObject.AddComponent<AIPackageController>();
		}
		aIPackageController.Summoner = partyMemberAI.Summoner;
		aIPackageController.SummonType = partyMemberAI.SummonType;
		aIPackageController.SummonedCreatureList.Clear();
		for (int i = 0; i < partyMemberAI.SummonedCreatureList.Count; i++)
		{
			GameObject gameObject2 = partyMemberAI.SummonedCreatureList[i];
			if (!(gameObject2 == null))
			{
				aIPackageController.SummonedCreatureList.Add(gameObject2);
				PartyMemberAI component3 = gameObject2.GetComponent<PartyMemberAI>();
				if ((bool)component3)
				{
					RemoveFromActiveParty(component3, purgePersistencePacket: true);
				}
			}
		}
		partyMemberAI.SummonedCreatureList.Clear();
		if (purgePersistencePacket)
		{
			PersistenceManager.ClearComponentPacket(gameObject, typeof(PartyMemberAI));
		}
		GameUtilities.DestroyImmediate(partyMemberAI);
		UIOffscreenObjectManager.RemovePointer(gameObject);
		Inventory component4 = gameObject.GetComponent<Inventory>();
		if (component4 != null)
		{
			component4.SetPartyMemberInventory(enabled: false);
		}
		Faction component5 = gameObject.GetComponent<Faction>();
		if ((bool)component5)
		{
			Team team = Team.Create();
			team.ScriptTag = gameObject.name;
			component5.CurrentTeamInstance = team;
		}
		Stealth component6 = gameObject.GetComponent<Stealth>();
		if ((bool)component6)
		{
			GameUtilities.DestroyComponent(component6);
		}
		CompressPartyMembers();
		if (PartyMemberAI.OnPartyMembersChanged != null)
		{
			PartyMemberAI.OnPartyMembersChanged(gameObject, EventArgs.Empty);
		}
	}

	private void OnFrozenMovementChanged(GameObject target, bool isFrozen)
	{
		if (!(target != base.gameObject) && !(SoundSet == null) && (bool)m_stats && base.enabled && IsActiveInParty && isFrozen)
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(target, SoundSet.SoundAction.Immobilized, SoundSet.s_MediumVODelay, forceInterrupt: false);
		}
	}

	private static void RecalculateFormationIndexOrder()
	{
		int num = 0;
		for (int i = 0; i < 30; i++)
		{
			if (PartyMembers[i] != null)
			{
				if (i < 12)
				{
					PartyMembers[i].DesiredFormationIndex = num;
					num++;
				}
				else
				{
					PartyMembers[i].DesiredFormationIndex = -1;
				}
			}
		}
	}

	public static PartyMemberAI GetPartyMemberAtFormationIndex(int index)
	{
		for (int i = 0; i < 12; i++)
		{
			if (PartyMembers[i] != null && PartyMembers[i].DesiredFormationIndex == index)
			{
				return PartyMembers[i];
			}
		}
		return null;
	}

	public static void UpdateEnemySpottedTimer()
	{
		if (!GameState.Paused && s_enemySpottedTimer > float.Epsilon)
		{
			s_enemySpottedTimer -= Time.deltaTime;
		}
	}

	public void ReloadIfNecessary()
	{
		AttackFirearm attackFirearm = GetPrimaryAttack() as AttackFirearm;
		if (attackFirearm != null && attackFirearm.BaseIsReady() && attackFirearm.RequiresReload)
		{
			AI.Player.ReloadWeapon state = AIStateManager.StatePool.Allocate<AI.Player.ReloadWeapon>();
			base.StateManager.PushState(state);
			return;
		}
		AttackFirearm attackFirearm2 = GetSecondaryAttack() as AttackFirearm;
		if (attackFirearm2 != null && attackFirearm2.BaseIsReady() && attackFirearm2.RequiresReload)
		{
			AI.Player.ReloadWeapon state2 = AIStateManager.StatePool.Allocate<AI.Player.ReloadWeapon>();
			base.StateManager.PushState(state2);
		}
	}

	public void UpdateStateAfterWeaponSetChange(bool becauseSummoningWeapon)
	{
		if (base.StateManager != null)
		{
			AIState currentState = base.StateManager.CurrentState;
			if (currentState is AI.Achievement.ReloadWeapon || currentState is AI.Player.ReloadWeapon)
			{
				currentState.BaseAbort();
			}
			else if (currentState is AI.Achievement.Attack && !becauseSummoningWeapon)
			{
				AI.Achievement.Attack obj = currentState as AI.Achievement.Attack;
				obj.InWeaponChange = true;
				currentState.BaseCancel();
				currentState.BaseAbort();
				obj.InWeaponChange = false;
			}
			for (AI.Achievement.ReloadWeapon reloadWeapon = base.StateManager.FindState(typeof(AI.Achievement.ReloadWeapon)) as AI.Achievement.ReloadWeapon; reloadWeapon != null; reloadWeapon = base.StateManager.FindState(typeof(AI.Achievement.ReloadWeapon)) as AI.Achievement.ReloadWeapon)
			{
				base.StateManager.PopState(reloadWeapon);
			}
			for (AI.Player.ReloadWeapon reloadWeapon2 = base.StateManager.FindState(typeof(AI.Player.ReloadWeapon)) as AI.Player.ReloadWeapon; reloadWeapon2 != null; reloadWeapon2 = base.StateManager.FindState(typeof(AI.Player.ReloadWeapon)) as AI.Player.ReloadWeapon)
			{
				base.StateManager.PopState(reloadWeapon2);
			}
			AI.Achievement.Attack attack = base.StateManager.FindState(typeof(AI.Achievement.Attack)) as AI.Achievement.Attack;
			while (attack != null && !becauseSummoningWeapon)
			{
				base.StateManager.PopState(attack);
				attack = base.StateManager.FindState(typeof(AI.Achievement.Attack)) as AI.Achievement.Attack;
			}
			if (base.StateManager.FindState(typeof(AI.Player.Attack)) is AI.Player.Attack attack2)
			{
				attack2.QueueWeaponSetChange();
			}
		}
	}

	public void TriggerAbility(GameEventArgs args, GenericAbility weaponAbility)
	{
		if (!(args.GenericData[0].ToString() == "activate") || !(args.GameObjectData[1] == base.gameObject))
		{
			return;
		}
		AIState currentState = m_ai.CurrentState;
		if (currentState is AI.Achievement.ReloadWeapon reloadWeapon)
		{
			reloadWeapon.InterruptReload();
		}
		GenericAbility genericAbility = args.GameObjectData[0].GetComponent<GenericAbility>();
		AttackBase attackBase = null;
		attackBase = ((!(genericAbility != null)) ? args.GameObjectData[0].GetComponent<AttackBase>() : genericAbility.gameObject.GetComponent<AttackBase>());
		if (currentState is Dead || currentState is Unconscious)
		{
			if (attackBase != null)
			{
				Debug.LogError(attackBase.name + " is trying to play an animation on a dead/unconscious party member.", base.gameObject);
			}
			else if (genericAbility != null)
			{
				genericAbility.Activate();
			}
		}
		else if (attackBase != null)
		{
			bool movementRestricted = false;
			bool fullAttack = false;
			StatusEffect[] statusEffects = null;
			int animVariation = -1;
			AttackBase weaponAttack = null;
			if (args.GenericData.Length > 1)
			{
				if (args.GenericData[1].ToString() == "restricted")
				{
					movementRestricted = true;
				}
				else if (args.GenericData[1].ToString() == "full")
				{
					fullAttack = true;
				}
				if (args.GenericData.Length > 2)
				{
					statusEffects = (StatusEffect[])args.GenericData[2];
				}
				if (args.GenericData.Length > 3)
				{
					animVariation = (int)args.GenericData[3];
				}
				if (args.GenericData.Length > 4)
				{
					weaponAttack = args.GenericData[4] as AttackBase;
				}
			}
			if (genericAbility == null)
			{
				genericAbility = weaponAbility;
			}
			GameState.s_playerCharacter.StartCasting(genericAbility, attackBase, movementRestricted, fullAttack, statusEffects, weaponAttack, animVariation, this);
		}
		else if (GameInput.GetControl(MappedControl.QUEUE))
		{
			Ability ability = AIStateManager.StatePool.Allocate<Ability>();
			ability.QueuedAbility = genericAbility;
			base.StateManager.QueueState(ability);
		}
		else
		{
			QueuedAbility = genericAbility;
		}
	}

	public override bool BeingKited()
	{
		return false;
	}

	public override float GetCooldownBetweenSpells()
	{
		return CooldownBetweenSpells;
	}

	public void InitInstructionSet()
	{
		InstructionSet = null;
		m_instructions.Clear();
		CharacterStats component = GetComponent<CharacterStats>();
		if (!component || !CharacterStats.IsPlayableClass(component.CharacterClass))
		{
			return;
		}
		int num = 0;
		PartyMemberInstructionSetList instructionSetList = PartyMemberInstructionSetList.InstructionSetList;
		if (instructionSetList == null)
		{
			return;
		}
		InstructionSet = instructionSetList.GetInstructionSet(component.CharacterClass, InstructionSetIndex);
		if (InstructionSet == null)
		{
			return;
		}
		m_instructionSelectionType = InstructionSet.SelectionType;
		m_defaultTargetPreference = InstructionSet.DefaultTargetPreference.Clone() as TargetPreference;
		CooldownBetweenSpells = InstructionSet.CooldownBetweenSpells;
		SpellCastData[] spells = InstructionSet.Spells;
		foreach (SpellCastData spellCastData in spells)
		{
			if (spellCastData != null)
			{
				num += spellCastData.CastingPriority;
				SpellCastData item = spellCastData.Clone() as SpellCastData;
				m_instructions.Add(item);
			}
		}
		float num2 = num;
		foreach (SpellCastData instruction in m_instructions)
		{
			instruction.Odds = (float)instruction.CastingPriority / num2;
		}
		m_instructions.Sort();
	}

	private void UpdateEnemySpotted()
	{
		if (GameState.InCombat || GameState.IsLoading || !IsControllable)
		{
			return;
		}
		Faction component = base.StateManager.Owner.GetComponent<Faction>();
		if (component == null)
		{
			return;
		}
		bool flag = false;
		GameObject target = null;
		float num = 361f;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (faction.IsHostile(component))
			{
				AIController aIController = GameUtilities.FindActiveAIController(faction.gameObject);
				if (!(aIController == null) && !aIController.IsPet && !aIController.IsInvisible && !aIController.IsDead && !aIController.IsUnconscious && GameUtilities.V3SqrDistance2D(base.StateManager.Owner.transform.position, faction.gameObject.transform.position) < num && FogOfWar.Instance.FogValue(faction.gameObject.transform.position) < 0.7f && GameUtilities.LineofSight(base.StateManager.Owner.transform.position, aIController.gameObject, 1f))
				{
					flag = true;
					target = aIController.gameObject;
					break;
				}
			}
		}
		if (flag == m_enemySpotted)
		{
			return;
		}
		m_enemySpotted = flag;
		if (!m_enemySpotted)
		{
			return;
		}
		PartyMemberAI[] partyMembers = PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null && partyMemberAI.m_enemySpotted && partyMemberAI != this)
			{
				return;
			}
		}
		if (s_enemySpottedTimer <= 0f)
		{
			GameState.AutoPause(AutoPauseOptions.PauseEvent.EnemySpotted, target, null);
			s_enemySpottedTimer = 7f;
		}
	}

	public bool AutoPickNearbyEnemy(AI.Player.Attack attackState)
	{
		if (!GameState.InCombat)
		{
			return false;
		}
		if (attackState != null && (bool)attackState.AttackToUse && attackState.AttackToUse.HasForcedTarget)
		{
			GameObject forcedTarget = attackState.AttackToUse.ForcedTarget;
			if ((bool)forcedTarget)
			{
				UpdateAttackStateTarget(attackState, forcedTarget);
				return true;
			}
			return false;
		}
		float num = 0f;
		switch (GetAutoAttackAggression())
		{
		case AggressionType.Aggressive:
			num = 20f;
			break;
		case AggressionType.Defensive:
		{
			AttackBase primaryAttack2 = AIController.GetPrimaryAttack(m_ai.Owner);
			if (primaryAttack2 == null)
			{
				return false;
			}
			num = ((!(primaryAttack2 is AttackMelee)) ? 9f : 6f);
			break;
		}
		case AggressionType.DefendMyself:
		{
			AttackBase primaryAttack = AIController.GetPrimaryAttack(m_ai.Owner);
			if (primaryAttack == null)
			{
				return false;
			}
			num = ((!(primaryAttack is AttackMelee)) ? 6f : 3f);
			break;
		}
		case AggressionType.Passive:
			return false;
		}
		num *= num;
		float num2 = float.MaxValue;
		GameObject gameObject = null;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (!faction.IsHostile(m_ai.Owner.gameObject))
			{
				continue;
			}
			Health component = faction.GetComponent<Health>();
			if (component == null || component.Dead || !component.gameObject.activeInHierarchy)
			{
				continue;
			}
			AIController aIController = GameUtilities.FindActiveAIController(faction.gameObject);
			if (aIController == null || (bool)faction.GetComponent<PartyMemberAI>() || aIController.IsInvisible)
			{
				continue;
			}
			float num3 = GameUtilities.V3SqrDistance2D(m_ai.Owner.transform.position, faction.transform.position);
			if (!(num3 > num) && (!(FogOfWar.Instance != null) || FogOfWar.PointVisibleInFog(faction.transform.position)) && aIController.InCombat && GameUtilities.LineofSight(m_ai.Owner.transform.position, faction.transform.position, 1f, includeDynamics: false))
			{
				float num4 = 0f;
				float num5 = Mathf.Sqrt(num3);
				num4 += num5 / PerceptionDistance * 5f;
				num4 += component.CurrentStamina / component.MaxStamina * 1f;
				num4 += ((aIController.CurrentTarget == m_ai.Owner) ? 0f : 2.5f);
				if (num4 < num2)
				{
					num2 = num4;
					gameObject = faction.gameObject;
				}
			}
		}
		if (gameObject != null)
		{
			UpdateAttackStateTarget(attackState, gameObject);
		}
		return gameObject != null;
	}

	private void UpdateAttackStateTarget(AI.Player.Attack attackState, GameObject bestObj)
	{
		if (bestObj == null)
		{
			Debug.LogError("Tried to PartyMemberAI::UpdateAttackStateTarget a null object.");
			return;
		}
		if (attackState == null)
		{
			attackState = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
			m_ai.PushState(attackState);
		}
		attackState.IsAutoAttack = true;
		attackState.Target = bestObj;
		attackState.TargetMover = bestObj.GetComponent<Mover>();
		Faction component = bestObj.GetComponent<Faction>();
		if (component != null)
		{
			attackState.TargetTeam = component.CurrentTeamInstance;
		}
	}
}
