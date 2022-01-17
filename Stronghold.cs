using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public class Stronghold : MonoBehaviour
{
	[Serializable]
	public class ReputationBonus
	{
		public FactionName factionName;

		public Reputation.Axis axis;

		public Reputation.ChangeStrength strength;

		public override string ToString()
		{
			return StringUtility.Format(GUIUtils.FormatReputationChangeStrength(strength, ReputationManager.Instance.GetFactionName(factionName))) + GUIUtils.Format(1731, GUIUtils.GetReputationAxisString(axis));
		}
	}

	public class Damageables
	{
		public bool isUpgrade;

		public StrongholdUpgrade upgrade;

		public StrongholdHireling hireling;
	}

	public enum UIActionSoundType
	{
		EngageAdventure,
		PurchaseUpgrade,
		DestroyUpgrade,
		HireHireling,
		DismissHireling,
		RansomVisitor,
		RescueVisitor,
		PayOffVisitor,
		EscortVisitor,
		AcceptMoney,
		AcceptItem,
		BuyItem,
		AutoResolveAttack,
		ManualResolveAttack,
		NotifyPositive,
		NotifyNegative,
		NotifyConstructionFinished
	}

	public enum NotificationType
	{
		None,
		Positive,
		Negative,
		ConstructionFinished
	}

	public enum StatType
	{
		None,
		Security,
		Prestige
	}

	public delegate void LogMessageDelegate(NotificationType type, string timestamp, string message);

	public delegate void AdvanceTurnDelegate();

	public delegate void UpgradeStatusChanged(StrongholdUpgrade.Type upgradeType);

	public delegate void HirelingStatusChanged(StrongholdHireling hireling);

	public delegate void AdventureStatusChanged(StrongholdAdventure adventure);

	public delegate void VisitorStatusChanged(StrongholdVisitor visitor);

	public delegate void EventChanged(StrongholdEvent sevent);

	public delegate void DebtChanged();

	public enum WhyCantBuild
	{
		NONE,
		ALREADY_HAS,
		ALREADY_IN_PROGRESS,
		IS_NULL,
		MISSING_PREREQ,
		CANT_AFFORD
	}

	public enum WhyCantHire
	{
		NONE,
		NO_BARRACKS,
		MAX_HIRELINGS,
		ALREADY_HIRED,
		CANT_AFFORD,
		NOT_AVAILABLE
	}

	public enum WindowPane
	{
		Status,
		Actions,
		Upgrades,
		Hirelings,
		Companions
	}

	public LogMessageDelegate OnLogMessage;

	public AdvanceTurnDelegate OnAdvanceTurn;

	public UpgradeStatusChanged OnUpgradeStatusChanged;

	public HirelingStatusChanged OnHirelingStatusChanged;

	public AdventureStatusChanged OnAdventureStatusChanged;

	public VisitorStatusChanged OnVisitorStatusChanged;

	public EventChanged OnEventChanged;

	public DebtChanged OnDebtChanged;

	public Vendor BrighthollowPrefab;

	public float GameDaysMult = 1f;

	public int CollectTaxesTurnCount = 5;

	public int SpawnAdventureTurnCount = 5;

	public int SpawnIngredientsTurnCount = 1;

	public int PayHirelingsDayCount = 5;

	public int RandomEventMaxDayCount = 5;

	public int AttackEventMinDayCount = 3;

	public int AttackEventMaxDayCount = 6;

	public float ErlTaxRatio = 0.15f;

	public int MaxAdventures = 10;

	public StrongholdAdventure[] AdventureTemplates;

	public StrongholdPremadeAdventureList PremadeAdventures;

	public Item[] CurrencyRewards;

	public Item[] MinorItemRewards;

	public Item[] AverageItemRewards;

	public Item[] MajorItemRewards;

	public Item[] GrandItemRewards;

	public ReputationBonus[] MinorReputationRewards;

	public ReputationBonus[] AverageReputationRewards;

	public ReputationBonus[] MajorReputationRewards;

	public ReputationBonus[] GrandReputationRewards;

	public StrongholdUpgrade[] Upgrades;

	public int MaxHirelings = 8;

	public StrongholdHireling[] StandardHirelings;

	public StrongholdGuestHireling[] GuestHirelings;

	public float DilemmaResolvedWeightMultiplier = 0.5f;

	public int MaximumUnresolvedDilemmaStreak = 2;

	public StrongholdVisitor[] Visitors;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when a stronghold attack is in progress and 0 otherwise.")]
	public string AttackGlobalVariableName;

	public StrongholdAttack[] Attacks;

	[Persistent]
	private int m_currentTurn;

	[Persistent]
	private float m_gameUpdateTimer;

	private float m_gameMaxTimer;

	private EternityTimeInterval m_ChunkElapsed = new EternityTimeInterval();

	[Persistent]
	private List<string> m_log = new List<string>(64);

	private const int LOG_MAX_ENTRIES = 64;

	private Dictionary<Guid, StoredCharacterInfo> m_storedCompanions = new Dictionary<Guid, StoredCharacterInfo>();

	public List<Guid> AdventurersSpawnedInMap = new List<Guid>();

	[Persistent]
	private List<StrongholdVisitorArrival> m_VisitorsToArriveDelayed = new List<StrongholdVisitorArrival>();

	[Persistent]
	private int m_CurrentUnresolvedDilemmaStreak;

	private Dictionary<Guid, StoredCharacterInfo> m_storedAnimalCompanions = new Dictionary<Guid, StoredCharacterInfo>();

	[Persistent]
	private List<StrongholdEvent> m_events = new List<StrongholdEvent>();

	[Persistent]
	private List<StrongholdAdventure> m_adventuresAvailable = new List<StrongholdAdventure>();

	[Persistent]
	private List<StrongholdAdventureCompletion> m_adventuresCompleted = new List<StrongholdAdventureCompletion>();

	[Persistent]
	private SortedList<int, StrongholdAdventure> m_adventuresEngaged = new SortedList<int, StrongholdAdventure>();

	[Persistent]
	private List<StrongholdUpgrade.Type> m_upgradesBuilt = new List<StrongholdUpgrade.Type>();

	[Persistent]
	private List<StrongholdHireling> m_hirelingsHired = new List<StrongholdHireling>();

	[Persistent]
	private List<StrongholdPrisonerData> m_prisoners = new List<StrongholdPrisonerData>();

	[Persistent]
	private int m_lastUnshownAdventureCompleted = -1;

	private List<StrongholdVisitor> m_visitors = new List<StrongholdVisitor>();

	private List<StrongholdVisitor> m_visitorsDead = new List<StrongholdVisitor>();

	[Persistent]
	private List<StrongholdUpgrade.Type> m_upgradesSpawned = new List<StrongholdUpgrade.Type>();

	[Persistent]
	private List<StrongholdAdventure> m_adventuresForRewards = new List<StrongholdAdventure>();

	[Persistent]
	private List<Item> m_itemsFromVisitors = new List<Item>();

	[HideInInspector]
	[Persistent]
	public int BonusTurnMoney;

	private bool m_TurnEventLogged;

	private bool m_DayEventLogged;

	[Persistent]
	private bool m_disabled;

	[Persistent]
	private bool m_disallowRareItemMerchants;

	private EternityDateTime m_LastUpdateTime;

	[Persistent]
	private int m_Debt;

	private NotificationType m_PendingNotificationSound;

	private WindowPane m_currentPane;

	public static Stronghold Instance { get; private set; }

	public int HirelingsHired => m_hirelingsHired.Count;

	public int CurrentTurn => m_currentTurn;

	public EternityDateTime CurrentTime => WorldTime.Instance.CurrentTime + m_ChunkElapsed;

	[Persistent]
	public int AvailableTurns { get; private set; }

	[Persistent]
	public bool IsErlTaxActive { get; set; }

	public IList<string> Log => m_log;

	[Persistent]
	public bool[] PremadeAdventuresCompleted { get; set; }

	[Persistent]
	private List<Guid> SerializedStoredGuids
	{
		get
		{
			List<Guid> list = new List<Guid>();
			foreach (StoredCharacterInfo value in m_storedCompanions.Values)
			{
				InstanceID component = value.GetComponent<InstanceID>();
				if ((bool)component)
				{
					list.Add(component.Guid);
				}
			}
			return list;
		}
		set
		{
			m_storedCompanions.Clear();
			foreach (Guid item in value)
			{
				if (item == Guid.Empty)
				{
					continue;
				}
				GameObject objectByID = InstanceID.GetObjectByID(item);
				if ((bool)objectByID)
				{
					StoredCharacterInfo component = objectByID.GetComponent<StoredCharacterInfo>();
					if ((bool)component && !m_storedCompanions.ContainsKey(component.GUID))
					{
						m_storedCompanions.Add(component.GUID, component);
					}
				}
			}
		}
	}

	[Persistent]
	private List<Guid> SerializedStoredAnimalCompanionGuids
	{
		get
		{
			List<Guid> list = new List<Guid>();
			foreach (StoredCharacterInfo value in m_storedAnimalCompanions.Values)
			{
				InstanceID component = value.GetComponent<InstanceID>();
				if ((bool)component)
				{
					list.Add(component.Guid);
				}
			}
			return list;
		}
		set
		{
			m_storedAnimalCompanions.Clear();
			foreach (Guid item in value)
			{
				if (item == Guid.Empty)
				{
					continue;
				}
				GameObject objectByID = InstanceID.GetObjectByID(item);
				if (!objectByID)
				{
					continue;
				}
				StoredCharacterInfo component = objectByID.GetComponent<StoredCharacterInfo>();
				if ((bool)component)
				{
					try
					{
						m_storedAnimalCompanions.Add(component.GUID, component);
					}
					catch (Exception ex)
					{
						Debug.Log(ex.ToString());
					}
				}
			}
		}
	}

	[Persistent]
	private List<StrongholdVisitorSerializeData> SerializedVisitorList
	{
		get
		{
			List<StrongholdVisitorSerializeData> list = new List<StrongholdVisitorSerializeData>();
			for (int i = 0; i < m_visitors.Count; i++)
			{
				StrongholdVisitor strongholdVisitor = m_visitors[i];
				StrongholdVisitorSerializeData strongholdVisitorSerializeData = new StrongholdVisitorSerializeData();
				strongholdVisitorSerializeData.Tag = strongholdVisitor.Tag;
				strongholdVisitorSerializeData.AssociatedPrisoner = strongholdVisitor.AssociatedPrisoner;
				strongholdVisitorSerializeData.TimeToLeave = strongholdVisitor.TimeToLeave;
				list.Add(strongholdVisitorSerializeData);
			}
			return list;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			m_visitors.Clear();
			for (int i = 0; i < value.Count; i++)
			{
				for (int j = 0; j < Visitors.Length; j++)
				{
					if (string.Compare(value[i].Tag, Visitors[j].Tag) == 0 && !m_visitors.Contains(Visitors[j]))
					{
						Visitors[j].TimeToLeave = value[i].TimeToLeave;
						Visitors[j].AssociatedPrisoner = value[i].AssociatedPrisoner;
						m_visitors.Add(Visitors[j]);
					}
				}
			}
		}
	}

	[Persistent]
	private List<StrongholdVisitorSerializeData> SerializedDeadVisitorList
	{
		get
		{
			List<StrongholdVisitorSerializeData> list = new List<StrongholdVisitorSerializeData>();
			for (int i = 0; i < m_visitorsDead.Count; i++)
			{
				StrongholdVisitor strongholdVisitor = m_visitorsDead[i];
				StrongholdVisitorSerializeData strongholdVisitorSerializeData = new StrongholdVisitorSerializeData();
				strongholdVisitorSerializeData.Tag = strongholdVisitor.Tag;
				strongholdVisitorSerializeData.AssociatedPrisoner = strongholdVisitor.AssociatedPrisoner;
				strongholdVisitorSerializeData.TimeToLeave = strongholdVisitor.TimeToLeave;
				list.Add(strongholdVisitorSerializeData);
			}
			return list;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			m_visitorsDead.Clear();
			for (int i = 0; i < value.Count; i++)
			{
				for (int j = 0; j < Visitors.Length; j++)
				{
					if (string.Compare(value[i].Tag, Visitors[j].Name) == 0 && !m_visitorsDead.Contains(Visitors[j]))
					{
						Visitors[j].TimeToLeave = value[i].TimeToLeave;
						Visitors[j].AssociatedPrisoner = value[i].AssociatedPrisoner;
						AddVisitorToDeadList(Visitors[j]);
					}
				}
			}
		}
	}

	public List<StrongholdVisitor> GetVisitors => m_visitors;

	public List<StrongholdAdventure> GetAdventuresAvailable => m_adventuresAvailable;

	public List<StrongholdAdventureCompletion> GetCompleteAdventures => m_adventuresCompleted;

	public SortedList<int, StrongholdAdventure> GetAdventuresEngaged => m_adventuresEngaged;

	public List<StrongholdEvent> GetEvents => m_events;

	[Persistent]
	public int Prestige { get; set; }

	[Persistent]
	public int Security { get; set; }

	public bool Activated { get; private set; }

	public bool Disabled
	{
		get
		{
			return m_disabled;
		}
		set
		{
			m_disabled = value;
		}
	}

	[Persistent]
	public bool SerializedIsActivated
	{
		get
		{
			return Activated;
		}
		set
		{
			if (value)
			{
				ActivateStronghold(restoring: true);
			}
			else
			{
				Activated = false;
			}
		}
	}

	[Persistent]
	public StrongholdGuestHireling GuestHirelingAvailable { get; set; }

	[Persistent]
	public float GuestHirelingTimeLeft { get; set; }

	public int Debt
	{
		get
		{
			return m_Debt;
		}
		set
		{
			m_Debt = value;
			if (OnDebtChanged != null)
			{
				OnDebtChanged();
			}
		}
	}

	[Persistent]
	public int UnviewedEventCount { get; private set; }

	[Persistent]
	public int UnviewedEventCountInternal { get; private set; }

	public int AvailableCP
	{
		get
		{
			Player s_playerCharacter = GameState.s_playerCharacter;
			if (s_playerCharacter == null)
			{
				return 0;
			}
			PlayerInventory component = s_playerCharacter.GetComponent<PlayerInventory>();
			if (component == null)
			{
				return 0;
			}
			return (int)component.currencyTotalValue.v;
		}
		set
		{
			Player s_playerCharacter = GameState.s_playerCharacter;
			if (!(s_playerCharacter == null))
			{
				PlayerInventory component = s_playerCharacter.GetComponent<PlayerInventory>();
				if (!(component == null))
				{
					component.currencyTotalValue.v = value;
				}
			}
		}
	}

	public WindowPane CurrentPane
	{
		get
		{
			return m_currentPane;
		}
		set
		{
			m_currentPane = value;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'Stronghold' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		WorldTime.Instance.OnTimeJump += HandleGameOnTimeJump;
		m_gameMaxTimer = WorldTime.Instance.SecondsPerMinute * WorldTime.Instance.MinutesPerHour;
		m_LastUpdateTime = WorldTime.Instance.CurrentTime;
	}

	private void OnDestroy()
	{
		if (WorldTime.Instance != null)
		{
			WorldTime.Instance.OnTimeJump -= HandleGameOnTimeJump;
		}
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!m_disabled && Activated)
		{
			float num = Time.deltaTime * (float)WorldTime.Instance.GameSecondsPerRealSecond;
			m_gameUpdateTimer += num;
			if (m_gameUpdateTimer >= m_gameMaxTimer || UIStrongholdManager.Instance.WindowActive())
			{
				ProcessTimeAdvancement(m_gameUpdateTimer);
				m_gameUpdateTimer = 0f;
			}
			AdvanceOneTurn();
			if (AvailableTurns == 0)
			{
				PlayNotificationSound();
			}
		}
	}

	public void Restored()
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			StrongholdEvent strongholdEvent = m_events[i];
			if (strongholdEvent == null)
			{
				continue;
			}
			if (m_storedCompanions.ContainsKey(strongholdEvent.SerializedCompanion))
			{
				strongholdEvent.EventCompanion = m_storedCompanions[strongholdEvent.SerializedCompanion];
			}
			if (m_storedCompanions.ContainsKey(strongholdEvent.SerializedAbandonedCompanion))
			{
				strongholdEvent.EventAbandonedCompanion = m_storedCompanions[strongholdEvent.SerializedAbandonedCompanion];
			}
			if (strongholdEvent.EventType == StrongholdEvent.Type.Attack)
			{
				for (int j = 0; j < Attacks.Length; j++)
				{
					if (string.Compare(Attacks[j].Tag, strongholdEvent.EventAttackNameSerialized) == 0)
					{
						strongholdEvent.EventData = Attacks[j];
						break;
					}
				}
				continue;
			}
			if (strongholdEvent.EventType == StrongholdEvent.Type.BuildUpgrade)
			{
				strongholdEvent.EventData = strongholdEvent.EventUpgradeTypeSerialized;
				continue;
			}
			for (int k = 0; k < Visitors.Length; k++)
			{
				if (string.Compare(Visitors[k].Tag, strongholdEvent.EventVisitorNameSerialized.Tag) == 0)
				{
					strongholdEvent.EventData = Visitors[k];
					break;
				}
			}
		}
		for (int num = m_adventuresEngaged.Count - 1; num >= 0; num--)
		{
			StrongholdAdventure strongholdAdventure = m_adventuresEngaged.Values[num];
			if (strongholdAdventure != null)
			{
				if (m_storedCompanions.TryGetValue(strongholdAdventure.SerializedAdventurer, out var value))
				{
					strongholdAdventure.Adventurer = value;
				}
				else
				{
					m_adventuresEngaged.RemoveAt(num);
				}
			}
		}
		for (int l = 0; l < StandardHirelings.Length; l++)
		{
			bool flag = false;
			for (int m = 0; m < m_hirelingsHired.Count; m++)
			{
				if (m_hirelingsHired[m].HiredGlobalVariableName == StandardHirelings[l].HiredGlobalVariableName || m_hirelingsHired[m].SerializedNameId == StandardHirelings[l].NameId)
				{
					StandardHirelings[l].Restore(m_hirelingsHired[m]);
					m_hirelingsHired[m] = StandardHirelings[l];
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				StandardHirelings[l].ResetState();
			}
		}
		for (int n = 0; n < GuestHirelings.Length; n++)
		{
			bool flag2 = false;
			for (int num2 = 0; num2 < m_hirelingsHired.Count; num2++)
			{
				if (m_hirelingsHired[num2].HiredGlobalVariableName == GuestHirelings[n].HiredGlobalVariableName || m_hirelingsHired[num2].SerializedNameId == GuestHirelings[n].NameId)
				{
					GuestHirelings[n].Restore(m_hirelingsHired[num2]);
					m_hirelingsHired[num2] = GuestHirelings[n];
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				GuestHirelings[n].ResetState();
			}
		}
		for (int num3 = m_hirelingsHired.Count - 1; num3 >= 0; num3--)
		{
			if (!m_hirelingsHired[num3].HirelingPrefab && m_hirelingsHired[num3] is StrongholdGuestHireling)
			{
				int minimumPrestige = ((StrongholdGuestHireling)m_hirelingsHired[num3]).MinimumPrestige;
				for (int num4 = 0; num4 < GuestHirelings.Length; num4++)
				{
					bool flag3 = false;
					for (int num5 = 0; num5 < m_hirelingsHired.Count; num5++)
					{
						if (m_hirelingsHired[num5] == GuestHirelings[num4])
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3 && GuestHirelings[num4].MinimumPrestige == minimumPrestige && GlobalVariables.Instance.GetVariable(GuestHirelings[num4].HiredGlobalVariableName) > 0)
					{
						GuestHirelings[num4].Restore(m_hirelingsHired[num3]);
						m_hirelingsHired[num3] = GuestHirelings[num4];
						Debug.Log("Emergency restored a Guest Hireling.");
						break;
					}
				}
				if (m_hirelingsHired[num3].HirelingPrefab == null)
				{
					Debug.LogError("Guest Hireling fix-up failed altogether (this should not happen).");
					m_hirelingsHired.RemoveAt(num3);
				}
			}
		}
		if (GuestHirelingAvailable != null)
		{
			for (int num6 = 0; num6 < GuestHirelings.Length; num6++)
			{
				if (GuestHirelingAvailable.HiredGlobalVariableName == GuestHirelings[num6].HiredGlobalVariableName || GuestHirelingAvailable.SerializedNameId == GuestHirelings[num6].NameId)
				{
					GuestHirelings[num6].Restore(GuestHirelingAvailable);
					GuestHirelingAvailable = GuestHirelings[num6];
					break;
				}
			}
		}
		List<Guid> list = new List<Guid>();
		foreach (KeyValuePair<Guid, StoredCharacterInfo> storedCompanion in m_storedCompanions)
		{
			if (PersistenceManager.GetPacket(storedCompanion.Key) != null)
			{
				continue;
			}
			list.Add(storedCompanion.Key);
			if ((bool)storedCompanion.Value && (bool)storedCompanion.Value.gameObject)
			{
				Persistence component = storedCompanion.Value.gameObject.GetComponent<Persistence>();
				if ((bool)component)
				{
					PersistenceManager.RemoveObject(component);
				}
				GameUtilities.Destroy(component.gameObject);
			}
		}
		foreach (Guid item in list)
		{
			m_storedCompanions.Remove(item);
		}
		FlipTiles();
	}

	private void PlayActionSound(UIActionSoundType soundType)
	{
		GlobalAudioPlayer.SPlay(soundType);
	}

	private void HandleGameOnTimeJump(int gameSeconds, bool isMapTravel, bool isResting)
	{
		if (!m_disabled && Activated)
		{
			ProcessTimeAdvancement(gameSeconds);
		}
	}

	public float DaysToGTU(int days)
	{
		return (float)days * GameDaysMult;
	}

	public void ActivateStronghold(bool restoring)
	{
		TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.STRONGHOLD_UNLOCKED);
		Activated = true;
		if (!restoring)
		{
			m_currentTurn = 0;
			Debt = 0;
			GuestHirelingAvailable = null;
			AddEvent(StrongholdEvent.Type.CollectTaxes, null, CollectTaxesTurnCount);
			AddEvent(StrongholdEvent.Type.SpawnAdventure, null, SpawnAdventureTurnCount);
			AddEvent(StrongholdEvent.Type.SpawnIngredients, null, SpawnIngredientsTurnCount);
			AddEvent(StrongholdEvent.Type.PayHirelings, null, PayHirelingsDayCount);
			AddEvent(StrongholdEvent.Type.RandomEvent, null, OEIRandom.Range(1, RandomEventMaxDayCount));
		}
	}

	public void AddTurnsByObjective(int turns)
	{
		AddTurns(turns);
	}

	public void AddTurns(int turns)
	{
		if (Activated)
		{
			AvailableTurns += turns;
		}
	}

	public void AdvanceOneTurn()
	{
		if (CanAdvanceTurn())
		{
			m_currentTurn++;
			AvailableTurns--;
			m_TurnEventLogged = false;
			ProcessTurnAdvancement();
			if (!m_TurnEventLogged)
			{
				LogTurnEvent(Format(45), NotificationType.None);
			}
			if (OnAdvanceTurn != null)
			{
				OnAdvanceTurn();
			}
		}
	}

	public bool CanAdvanceTurn()
	{
		if (m_disabled)
		{
			return false;
		}
		if (AvailableTurns <= 0)
		{
			return false;
		}
		return true;
	}

	public StrongholdEvent AddEvent(StrongholdEvent.Type type, object data, int time)
	{
		if (!Activated)
		{
			return null;
		}
		time = ((!StrongholdEvent.UsesTurns(type)) ? ((int)(DaysToGTU(time) * (float)WorldTime.Instance.SecondsPerDay)) : (time + CurrentTurn));
		StrongholdEvent strongholdEvent = StrongholdEvent.Create(time, type, data);
		m_events.Add(strongholdEvent);
		if (OnEventChanged != null)
		{
			OnEventChanged(strongholdEvent);
		}
		return strongholdEvent;
	}

	public bool HasEvent(StrongholdEvent.Type type)
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			if (m_events[i].EventType == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool VisitorIsBusy(StrongholdVisitor visitor)
	{
		if (!HasEventWithVisitor(StrongholdEvent.Type.Escorting, visitor))
		{
			return HasEventWithVisitor(StrongholdEvent.Type.Kidnapped, visitor);
		}
		return true;
	}

	public bool HasEventWithVisitor(StrongholdEvent.Type type, StrongholdVisitor visitor)
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			if (m_events[i].EventType == type && (StrongholdVisitor)m_events[i].EventData == visitor)
			{
				return true;
			}
		}
		return false;
	}

	public void FlipTiles()
	{
		StrongholdTileFlipper strongholdTileFlipper = UnityEngine.Object.FindObjectOfType(typeof(StrongholdTileFlipper)) as StrongholdTileFlipper;
		if ((bool)strongholdTileFlipper)
		{
			strongholdTileFlipper.FlipTiles();
		}
	}

	private void ProcessTurnAdvancement()
	{
		FlipTiles();
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter == null)
		{
			return;
		}
		for (int num = m_events.Count - 1; num >= 0; num--)
		{
			if (StrongholdEvent.UsesTurns(m_events[num].EventType) && m_events[num].Time <= (float)m_currentTurn)
			{
				StrongholdEvent strongholdEvent = m_events[num];
				m_events.RemoveAt(num);
				strongholdEvent.ProcessEvent(s_playerCharacter, this);
			}
		}
		while (m_adventuresEngaged.Count > 0 && m_adventuresEngaged.Keys[0] <= m_currentTurn)
		{
			StrongholdAdventure strongholdAdventure = m_adventuresEngaged.Values[0];
			if (strongholdAdventure.PremadeAdventureIndex >= 0)
			{
				if (PremadeAdventuresCompleted == null || strongholdAdventure.PremadeAdventureIndex > PremadeAdventuresCompleted.Length)
				{
					bool[] array = new bool[PremadeAdventures.Adventures.Length];
					if (PremadeAdventuresCompleted != null)
					{
						PremadeAdventuresCompleted.CopyTo(array, 0);
					}
					PremadeAdventuresCompleted = array;
				}
				PremadeAdventuresCompleted[strongholdAdventure.PremadeAdventureIndex] = true;
			}
			m_adventuresEngaged.RemoveAt(0);
			strongholdAdventure.Finish(this);
			m_adventuresForRewards.Add(strongholdAdventure);
			if (OnAdventureStatusChanged != null)
			{
				OnAdventureStatusChanged(strongholdAdventure);
			}
			ProcessTreasuryChanged();
		}
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if ((bool)inventory && BonusTurnMoney > 0)
		{
			inventory.currencyTotalValue.v += BonusTurnMoney;
		}
	}

	private void ProcessTimeAdvancement(float gameSeconds)
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter == null)
		{
			return;
		}
		m_ChunkElapsed = new EternityTimeInterval();
		int num = m_LastUpdateTime.Day;
		while (gameSeconds > m_gameMaxTimer)
		{
			ProcessTimeAdvancementHelper(m_gameMaxTimer, s_playerCharacter);
			gameSeconds -= m_gameMaxTimer;
			m_ChunkElapsed.AddSeconds((int)m_gameMaxTimer);
			int day = (m_LastUpdateTime + m_ChunkElapsed).Day;
			if (num != day)
			{
				if (!m_DayEventLogged)
				{
					LogTimeEvent(Format(45), NotificationType.None);
				}
				m_DayEventLogged = false;
				num = day;
			}
		}
		ProcessTimeAdvancementHelper(gameSeconds, s_playerCharacter);
		m_LastUpdateTime = WorldTime.Instance.CurrentTime;
	}

	private void ProcessTimeAdvancementHelper(float gameSeconds, Player player)
	{
		if (!HasEvent(StrongholdEvent.Type.SpawnAdventure))
		{
			AddEvent(StrongholdEvent.Type.SpawnAdventure, null, SpawnAdventureTurnCount);
		}
		for (int num = m_events.Count - 1; num >= 0; num--)
		{
			if (StrongholdEvent.UsesGameTimeUnits(m_events[num].EventType))
			{
				m_events[num].Time -= gameSeconds;
				if (m_events[num].Time <= 0f)
				{
					StrongholdEvent strongholdEvent = m_events[num];
					m_events.RemoveAt(num);
					strongholdEvent.ProcessEvent(player, this);
				}
			}
		}
		for (int num2 = m_adventuresAvailable.Count - 1; num2 >= 0; num2--)
		{
			m_adventuresAvailable[num2].SerializedOfferExpires -= gameSeconds;
			if (m_adventuresAvailable[num2].SerializedOfferExpires <= 0f)
			{
				LogTurnEvent(Format(0, m_adventuresAvailable[num2].GetTitle(this)), NotificationType.None);
				try
				{
					if (OnAdventureStatusChanged != null)
					{
						OnAdventureStatusChanged(m_adventuresAvailable[num2]);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, this);
				}
				m_adventuresAvailable.RemoveAt(num2);
			}
		}
		for (int num3 = m_visitors.Count - 1; num3 >= 0; num3--)
		{
			m_visitors[num3].TimeToLeave -= gameSeconds;
			if (m_visitors[num3].TimeToLeave <= 0f && m_visitors[num3].VisitDuration > 0)
			{
				StrongholdVisitor strongholdVisitor = m_visitors[num3];
				RemoveVisitor(strongholdVisitor);
				strongholdVisitor.HandleLeaving(this);
			}
		}
		for (int num4 = m_VisitorsToArriveDelayed.Count - 1; num4 >= 0; num4--)
		{
			m_VisitorsToArriveDelayed[num4].TimeToArrive -= gameSeconds;
			if (m_VisitorsToArriveDelayed[num4].TimeToArrive <= 0f)
			{
				StrongholdVisitor visitorByTag = GetVisitorByTag(m_VisitorsToArriveDelayed[num4].Tag);
				if (visitorByTag == null)
				{
					Debug.LogError("Couldn't find delayed visitor with tag '" + m_VisitorsToArriveDelayed[num4].Tag);
					m_VisitorsToArriveDelayed.RemoveAt(num4);
				}
				else
				{
					SafeAddVisitor(visitorByTag, thwarted: false, almostThwarted: false);
				}
			}
		}
		if (GuestHirelingAvailable != null)
		{
			GuestHirelingTimeLeft -= gameSeconds;
			if (GuestHirelingTimeLeft <= 0f)
			{
				LogTimeEvent(StrongholdUtils.Format(CharacterStats.GetGender(GuestHirelingAvailable.HirelingPrefab), 38, GuestHirelingAvailable.Name), NotificationType.Positive);
				GuestHirelingAvailable = null;
			}
		}
	}

	public bool IsPremadeAdventureComplete(int index)
	{
		if (PremadeAdventuresCompleted != null && index >= 0 && index < PremadeAdventuresCompleted.Length)
		{
			return PremadeAdventuresCompleted[index];
		}
		return false;
	}

	public bool IsPremadeAdventureActive(int index)
	{
		for (int i = 0; i < m_adventuresAvailable.Count; i++)
		{
			if (m_adventuresAvailable[i].PremadeAdventureIndex == index)
			{
				return true;
			}
		}
		for (int j = 0; j < m_adventuresEngaged.Count; j++)
		{
			if (m_adventuresEngaged.Values[j].PremadeAdventureIndex == index)
			{
				return true;
			}
		}
		return false;
	}

	public void AddAdventure(StrongholdAdventure.Type type)
	{
		if (m_adventuresAvailable.Count >= MaxAdventures)
		{
			return;
		}
		StrongholdAdventure strongholdAdventure = StrongholdAdventure.Create(type, this);
		if (strongholdAdventure != null)
		{
			m_adventuresAvailable.Add(strongholdAdventure);
			LogTurnEvent(Format(1, strongholdAdventure.GetTitle(this), new EternityTimeInterval((int)strongholdAdventure.SerializedOfferExpires).FormatNonZero(2)), NotificationType.Positive);
			if (OnAdventureStatusChanged != null)
			{
				OnAdventureStatusChanged(strongholdAdventure);
			}
		}
	}

	public void EngageAdventure(int index, GameObject companion)
	{
		StrongholdAdventure adventure = m_adventuresAvailable[index];
		EngageAdventure(adventure, companion);
	}

	public void EngageAdventure(StrongholdAdventure adventure, GameObject companion)
	{
		if (adventure != null)
		{
			m_adventuresAvailable.Remove(adventure);
			adventure.Adventurer = companion.GetComponent<StoredCharacterInfo>();
			m_adventuresEngaged.Add(m_currentTurn + adventure.Duration, adventure);
			PlayActionSound(UIActionSoundType.EngageAdventure);
			if (OnAdventureStatusChanged != null)
			{
				OnAdventureStatusChanged(adventure);
			}
		}
	}

	public void CreateAdventureRewards(Inventory inven)
	{
		for (int num = m_adventuresForRewards.Count - 1; num >= 0; num--)
		{
			if (m_adventuresForRewards[num].CreateItemsAndMoney(inven, this))
			{
				m_adventuresForRewards.RemoveAt(num);
			}
		}
	}

	public int RecordAdventureCompletion(StrongholdAdventure adventure, List<string> rewards)
	{
		StrongholdAdventureCompletion strongholdAdventureCompletion = StrongholdAdventureCompletion.Create(adventure);
		strongholdAdventureCompletion.RewardStrings = rewards;
		m_adventuresCompleted.Add(strongholdAdventureCompletion);
		m_lastUnshownAdventureCompleted = m_adventuresCompleted.Count - 1;
		return m_lastUnshownAdventureCompleted;
	}

	public void TryDisplayLastUnshownAdventureReport()
	{
		if (m_lastUnshownAdventureCompleted >= 0 && m_lastUnshownAdventureCompleted < m_adventuresCompleted.Count)
		{
			UIStrongholdAdventureManager.Instance.ShowAdventure(m_adventuresCompleted[m_lastUnshownAdventureCompleted]);
		}
		m_lastUnshownAdventureCompleted = -1;
	}

	public void AddExperience(int xp)
	{
		int num = xp * StrongholdAdventure.PercentXPFromPlayer / 100;
		foreach (Guid key in m_storedCompanions.Keys)
		{
			StoredCharacterInfo storedCharacterInfo = m_storedCompanions[key];
			if ((bool)storedCharacterInfo)
			{
				int experience = storedCharacterInfo.Experience;
				experience = (storedCharacterInfo.Experience = experience + num);
			}
		}
		for (int i = 0; i < m_adventuresEngaged.Count; i++)
		{
			StrongholdAdventure strongholdAdventure = m_adventuresEngaged.Values[i];
			if (strongholdAdventure != null)
			{
				strongholdAdventure.DeferredXP += xp;
			}
		}
	}

	public object WhyNotAvailable(StoredCharacterInfo companion)
	{
		for (int i = 0; i < m_adventuresEngaged.Count; i++)
		{
			StrongholdAdventure strongholdAdventure = m_adventuresEngaged.Values[i];
			if (strongholdAdventure != null && strongholdAdventure.Adventurer == companion)
			{
				return strongholdAdventure;
			}
		}
		for (int j = 0; j < m_events.Count; j++)
		{
			if (m_events[j].EventCompanion == companion)
			{
				return m_events[j];
			}
		}
		return null;
	}

	public string WhyNotAvailableString(StoredCharacterInfo companion)
	{
		string result = "";
		object obj = WhyNotAvailable(companion);
		if (obj is StrongholdAdventure)
		{
			result = GUIUtils.Format(899, ((StrongholdAdventure)obj).GetTitle(Instance));
		}
		else if (obj is StrongholdEvent)
		{
			StrongholdEvent strongholdEvent = (StrongholdEvent)obj;
			if (strongholdEvent.EventCompanion != null)
			{
				if (strongholdEvent.EventType == StrongholdEvent.Type.Escorting)
				{
					result = ((strongholdEvent.EventDataInt < 0) ? GUIUtils.Format(898, ((StrongholdVisitor)strongholdEvent.EventData).Name) : GUIUtils.Format(899, ((StrongholdVisitor)strongholdEvent.EventData).Name));
				}
				else if (strongholdEvent.EventType == StrongholdEvent.Type.Kidnapped)
				{
					result = GUIUtils.Format(900, ((StrongholdVisitor)strongholdEvent.EventData).Name);
				}
			}
		}
		return result;
	}

	public bool IsAvailable(StoredCharacterInfo companion)
	{
		return WhyNotAvailable(companion) == null;
	}

	public bool IsAvailable(Guid companion)
	{
		if (!m_storedCompanions.ContainsKey(companion))
		{
			return false;
		}
		return WhyNotAvailable(m_storedCompanions[companion]) == null;
	}

	public int GetAdventureEndTurn(StrongholdAdventure adventure)
	{
		foreach (KeyValuePair<int, StrongholdAdventure> item in m_adventuresEngaged)
		{
			if (item.Value == adventure)
			{
				return item.Key;
			}
		}
		return 0;
	}

	public void AbortAndMakeAvailable(StoredCharacterInfo companion)
	{
		for (int i = 0; i < m_adventuresEngaged.Count; i++)
		{
			StrongholdAdventure strongholdAdventure = m_adventuresEngaged.Values[i];
			if (strongholdAdventure != null && strongholdAdventure.Adventurer == companion)
			{
				m_adventuresEngaged.RemoveAt(i);
				if (OnAdventureStatusChanged != null)
				{
					OnAdventureStatusChanged(strongholdAdventure);
				}
				return;
			}
		}
		for (int j = 0; j < m_events.Count; j++)
		{
			if (m_events[j].EventCompanion == companion)
			{
				if (StrongholdEvent.UsesTurns(m_events[j].EventType))
				{
					m_events[j].Time = m_currentTurn;
				}
				else
				{
					m_events[j].Time = WorldTime.Instance.SecondsPerDay;
				}
				m_events[j].EventCompanion = null;
				if (OnEventChanged != null)
				{
					OnEventChanged(m_events[j]);
				}
				break;
			}
		}
	}

	public List<StoredCharacterInfo> GetCompanions()
	{
		List<StoredCharacterInfo> list = new List<StoredCharacterInfo>();
		list.AddRange(m_storedCompanions.Values);
		return list;
	}

	public StoredCharacterInfo FindAvailableCompanion()
	{
		foreach (StoredCharacterInfo value in m_storedCompanions.Values)
		{
			if (IsAvailable(value))
			{
				return value;
			}
		}
		return null;
	}

	public string CompanionName(Guid companion)
	{
		if (m_storedCompanions.ContainsKey(companion))
		{
			return m_storedCompanions[companion].DisplayName;
		}
		return "Companion";
	}

	public int CompanionTotalDefense()
	{
		int num = 0;
		foreach (StoredCharacterInfo value in m_storedCompanions.Values)
		{
			if (IsAvailable(value))
			{
				CharacterStats component = value.GetComponent<CharacterStats>();
				if (component != null)
				{
					num += component.ScaledLevel;
				}
			}
		}
		return num;
	}

	public bool CanBuyUpgrade(StrongholdUpgrade.Type type)
	{
		return WhyCantBuildUpgrade(type) == WhyCantBuild.NONE;
	}

	public WhyCantBuild WhyCantBuildUpgrade(StrongholdUpgrade.Type type)
	{
		if (HasUpgrade(type))
		{
			return WhyCantBuild.ALREADY_HAS;
		}
		if (IsBuildingAnyUpgrade())
		{
			return WhyCantBuild.ALREADY_IN_PROGRESS;
		}
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo == null)
		{
			return WhyCantBuild.IS_NULL;
		}
		if (!HasUpgrade(upgradeInfo.Prerequisite))
		{
			return WhyCantBuild.MISSING_PREREQ;
		}
		if (!CanAfford(upgradeInfo.Cost))
		{
			return WhyCantBuild.CANT_AFFORD;
		}
		return WhyCantBuild.NONE;
	}

	public void BuyUpgrade(StrongholdUpgrade.Type type)
	{
		if (!CanBuyUpgrade(type))
		{
			return;
		}
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo != null)
		{
			AvailableCP -= upgradeInfo.Cost;
			AddEvent(StrongholdEvent.Type.BuildUpgrade, type, upgradeInfo.TimeToBuild);
			LogTimeEvent(Format(283, upgradeInfo.Name.GetText()), NotificationType.None);
			PlayActionSound(UIActionSoundType.PurchaseUpgrade);
			if (OnUpgradeStatusChanged != null)
			{
				OnUpgradeStatusChanged(type);
			}
		}
	}

	public void DebugBuildUpgrade(StrongholdUpgrade.Type type)
	{
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo != null && !HasUpgrade(upgradeInfo.UpgradeType))
		{
			AddEvent(StrongholdEvent.Type.BuildUpgrade, type, 0);
			ProcessTimeAdvancement(0f);
			if (OnUpgradeStatusChanged != null)
			{
				OnUpgradeStatusChanged(type);
			}
		}
	}

	public void CompleteBuildingUpgrade(StrongholdUpgrade.Type type)
	{
		m_upgradesBuilt.Add(type);
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo != null)
		{
			Prestige += upgradeInfo.PrestigeAdjustment;
			Security += upgradeInfo.SecurityAdjustment;
			if (!string.IsNullOrEmpty(upgradeInfo.UpgradeCompletedGlobalVariableName))
			{
				GlobalVariables.Instance.SetVariable(upgradeInfo.UpgradeCompletedGlobalVariableName, 1);
			}
			LogTimeEvent(Format(2, upgradeInfo.Name.GetText()), NotificationType.ConstructionFinished);
			if (OnUpgradeStatusChanged != null)
			{
				OnUpgradeStatusChanged(type);
			}
			if ((bool)AchievementTracker.Instance)
			{
				AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumStrongholdUpgrades, type.ToString());
			}
		}
	}

	public bool HasUpgrade(StrongholdUpgrade.Type type)
	{
		if (type >= StrongholdUpgrade.Type.Count)
		{
			return true;
		}
		return m_upgradesBuilt.Contains(type);
	}

	public bool IsBuildingUpgrade(StrongholdUpgrade.Type type)
	{
		int secondsLeft;
		return IsBuildingUpgrade(type, out secondsLeft);
	}

	public bool IsBuildingAnyUpgrade()
	{
		int secondsLeft;
		return IsBuildingUpgrade(StrongholdUpgrade.Type.None, out secondsLeft);
	}

	public bool IsBuildingUpgrade(StrongholdUpgrade.Type type, out int secondsLeft)
	{
		foreach (StrongholdEvent @event in m_events)
		{
			if (@event.EventType == StrongholdEvent.Type.BuildUpgrade && (type == StrongholdUpgrade.Type.None || (StrongholdUpgrade.Type)@event.EventData == type))
			{
				secondsLeft = (int)@event.Time;
				return true;
			}
		}
		secondsLeft = 0;
		return false;
	}

	public StrongholdUpgrade.Type GetBuildingUpgrade()
	{
		foreach (StrongholdEvent @event in m_events)
		{
			if (@event.EventType == StrongholdEvent.Type.BuildUpgrade)
			{
				return @event.EventUpgradeTypeSerialized;
			}
		}
		return StrongholdUpgrade.Type.None;
	}

	public void DestroyUpgrade(StrongholdUpgrade.Type type)
	{
		if (!m_upgradesBuilt.Remove(type))
		{
			return;
		}
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo != null)
		{
			Prestige -= upgradeInfo.PrestigeAdjustment;
			Security -= upgradeInfo.SecurityAdjustment;
			if (!string.IsNullOrEmpty(upgradeInfo.UpgradeCompletedGlobalVariableName))
			{
				GlobalVariables.Instance.SetVariable(upgradeInfo.UpgradeCompletedGlobalVariableName, 0);
			}
			PlayActionSound(UIActionSoundType.DestroyUpgrade);
			if (OnUpgradeStatusChanged != null)
			{
				OnUpgradeStatusChanged(type);
			}
		}
	}

	public int UpgradeTotalDefense()
	{
		int num = 0;
		foreach (StrongholdUpgrade.Type item in m_upgradesBuilt)
		{
			StrongholdUpgrade upgradeInfo = GetUpgradeInfo(item);
			if (upgradeInfo != null)
			{
				num += upgradeInfo.Level;
			}
		}
		return num;
	}

	public StrongholdUpgrade GetUpgradeInfo(StrongholdUpgrade.Type type)
	{
		StrongholdUpgrade[] upgrades = Upgrades;
		foreach (StrongholdUpgrade strongholdUpgrade in upgrades)
		{
			if (strongholdUpgrade.UpgradeType == type)
			{
				return strongholdUpgrade;
			}
		}
		return null;
	}

	public Affliction[] BoonsAvailable()
	{
		List<Affliction> list = new List<Affliction>();
		foreach (StrongholdUpgrade.Type item in m_upgradesBuilt)
		{
			StrongholdUpgrade upgradeInfo = GetUpgradeInfo(item);
			if (upgradeInfo != null && upgradeInfo.Boon != null)
			{
				list.Add(upgradeInfo.Boon);
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	public void SpawnIngredients()
	{
		bool flag = false;
		foreach (StrongholdUpgrade.Type item in m_upgradesBuilt)
		{
			StrongholdUpgrade upgradeInfo = GetUpgradeInfo(item);
			if (upgradeInfo != null && upgradeInfo.IngredientList != null)
			{
				m_upgradesSpawned.Add(item);
			}
		}
		if (flag)
		{
			LogTurnEvent(Format(3), NotificationType.Positive);
			ProcessTreasuryChanged();
		}
	}

	public void CreateStrongholdSpawnedIngredients(Inventory inven)
	{
		foreach (StrongholdUpgrade.Type item in m_upgradesSpawned)
		{
			if (!CreateSpawnIngredients(inven, item))
			{
				break;
			}
		}
		m_upgradesSpawned.Clear();
	}

	public bool CreateSpawnIngredients(Inventory inven, StrongholdUpgrade.Type type)
	{
		bool flag = true;
		StrongholdUpgrade upgradeInfo = GetUpgradeInfo(type);
		if (upgradeInfo != null && upgradeInfo.IngredientList != null && upgradeInfo.IngredientList.Length != 0)
		{
			int num = 0;
			bool createdSomething = false;
			while (flag && num < 3 && !createdSomething)
			{
				flag = CreateSpawnIngredientsHelper(inven, upgradeInfo.IngredientList, ref createdSomething);
				num++;
			}
			if (!createdSomething)
			{
				int num2 = OEIRandom.Index(upgradeInfo.IngredientList.Length);
				int addCount = OEIRandom.Range(upgradeInfo.IngredientList[num2].stackMin, upgradeInfo.IngredientList[num2].stackMax);
				inven.AddItem(upgradeInfo.IngredientList[num2].baseItem, addCount);
			}
		}
		return flag;
	}

	public bool CreateSpawnIngredientsHelper(Inventory inven, RegeneratingItem[] list, ref bool createdSomething)
	{
		bool result = true;
		createdSomething = false;
		foreach (RegeneratingItem regeneratingItem in list)
		{
			if (OEIRandom.FloatValue() < regeneratingItem.Chance)
			{
				int addCount = OEIRandom.Range(regeneratingItem.stackMin, regeneratingItem.stackMax);
				if (inven.AddItem(regeneratingItem.baseItem, addCount) > 0)
				{
					result = false;
				}
				else
				{
					createdSomething = true;
				}
			}
		}
		return result;
	}

	public bool CanHireHireling(StrongholdHireling hireling)
	{
		return WhyCantHireHireling(hireling) == WhyCantHire.NONE;
	}

	public bool CanSeeHireling(StrongholdHireling hireling)
	{
		return WhyCantSeeHireling(hireling) == WhyCantHire.NONE;
	}

	public WhyCantHire WhyCantHireHireling(StrongholdHireling hireling)
	{
		if (!HasUpgrade(StrongholdUpgrade.Type.Barracks))
		{
			return WhyCantHire.NO_BARRACKS;
		}
		if (m_hirelingsHired.Count >= MaxHirelings)
		{
			return WhyCantHire.MAX_HIRELINGS;
		}
		if (m_hirelingsHired.Contains(hireling))
		{
			return WhyCantHire.ALREADY_HIRED;
		}
		if (!CanAfford(CostToHire(hireling)))
		{
			return WhyCantHire.CANT_AFFORD;
		}
		if (!HirelingAvailable(hireling))
		{
			return WhyCantHire.NOT_AVAILABLE;
		}
		return WhyCantHire.NONE;
	}

	public WhyCantHire WhyCantSeeHireling(StrongholdHireling hireling)
	{
		if (!HasUpgrade(StrongholdUpgrade.Type.Barracks))
		{
			return WhyCantHire.NO_BARRACKS;
		}
		if (!HirelingAvailable(hireling))
		{
			return WhyCantHire.NOT_AVAILABLE;
		}
		return WhyCantHire.NONE;
	}

	public bool HirelingAvailable(StrongholdHireling hireling)
	{
		if (hireling.CanHireGlobalVariableName != null && hireling.CanHireGlobalVariableName.Length != 0)
		{
			return GlobalVariables.Instance.GetVariable(hireling.CanHireGlobalVariableName) != 0;
		}
		return true;
	}

	public bool HasHireling(StrongholdHireling hireling)
	{
		if (FindHireling(hireling.Name) != null)
		{
			return true;
		}
		return false;
	}

	public StrongholdHireling FindHireling(string name)
	{
		foreach (StrongholdHireling item in m_hirelingsHired)
		{
			if (item.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	public int CostToHire(StrongholdHireling hireling)
	{
		int num = DaysRemainingInPayCycle();
		return hireling.CostPerDay * num;
	}

	public void HireHireling(StrongholdHireling hireling)
	{
		if (CanHireHireling(hireling))
		{
			m_hirelingsHired.Add(hireling);
			AvailableCP -= CostToHire(hireling);
			hireling.Paid = true;
			Prestige += hireling.PrestigeAdjustment;
			Security += hireling.SecurityAdjustment;
			GlobalVariables.Instance.SetVariable(hireling.HiredGlobalVariableName, 1);
			if (hireling == GuestHirelingAvailable)
			{
				GuestHirelingAvailable = null;
			}
			PlayActionSound(UIActionSoundType.HireHireling);
			if (OnHirelingStatusChanged != null)
			{
				OnHirelingStatusChanged(hireling);
			}
		}
	}

	public void DismissHireling(StrongholdHireling hireling)
	{
		StrongholdHireling strongholdHireling = FindHireling(hireling.Name);
		if (m_hirelingsHired.Remove(strongholdHireling))
		{
			GlobalVariables.Instance.SetVariable(strongholdHireling.HiredGlobalVariableName, 0);
			if (strongholdHireling.Paid)
			{
				Prestige -= strongholdHireling.PrestigeAdjustment;
				Security -= strongholdHireling.SecurityAdjustment;
			}
			PlayActionSound(UIActionSoundType.DismissHireling);
			if (OnHirelingStatusChanged != null)
			{
				OnHirelingStatusChanged(hireling);
			}
		}
	}

	public void OnHirelingDeath(StrongholdHireling hireling)
	{
		DismissHireling(hireling);
	}

	public int HirelingTotalDefense()
	{
		int num = 0;
		foreach (StrongholdHireling item in m_hirelingsHired)
		{
			if (item.Paid)
			{
				CharacterStats component = item.HirelingPrefab.GetComponent<CharacterStats>();
				if (component != null)
				{
					num += component.ScaledLevel;
				}
			}
		}
		return num;
	}

	public int HirelingTotalStat(StatType stat)
	{
		int num = 0;
		foreach (StrongholdHireling item in m_hirelingsHired)
		{
			if (item.Paid)
			{
				num = ((stat != StatType.Prestige) ? (num + item.SecurityAdjustment) : (num + item.PrestigeAdjustment));
			}
		}
		return num;
	}

	public void RestorePay(StrongholdHireling hireling)
	{
		if (m_hirelingsHired.Contains(hireling) && !hireling.Paid)
		{
			int num = CostToHire(hireling);
			if (!CanAfford(num))
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
				return;
			}
			AvailableCP -= num;
			hireling.Paid = true;
			Prestige += hireling.PrestigeAdjustment;
			Security += hireling.SecurityAdjustment;
		}
	}

	public void ProcessPayCycle()
	{
		if (m_hirelingsHired.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int num2 = m_hirelingsHired.Count - 1; num2 >= 0; num2--)
		{
			StrongholdHireling strongholdHireling = m_hirelingsHired[num2];
			Gender gender = CharacterStats.GetGender(strongholdHireling.HirelingPrefab);
			string text = strongholdHireling.Name;
			if (strongholdHireling.IsLeaving)
			{
				DismissHireling(strongholdHireling);
				stringBuilder.Append(" " + StrongholdUtils.Format(gender, 5, text));
			}
			else
			{
				int num3 = strongholdHireling.CostPerDay * PayHirelingsDayCount;
				if (CanAfford(num3))
				{
					AvailableCP -= num3;
					num += num3;
					if (!strongholdHireling.Paid)
					{
						Prestige += strongholdHireling.PrestigeAdjustment;
						Security += strongholdHireling.SecurityAdjustment;
						stringBuilder.Append(" " + StrongholdUtils.Format(gender, 7, text, GUIUtils.Format(466, num3)));
					}
					strongholdHireling.Paid = true;
					if (strongholdHireling.LeavesAfterFullPayCycle)
					{
						strongholdHireling.IsLeaving = true;
					}
				}
				else
				{
					bool flag = false;
					bool flag2 = false;
					if (strongholdHireling.Paid)
					{
						Prestige -= strongholdHireling.PrestigeAdjustment;
						Security -= strongholdHireling.SecurityAdjustment;
						flag = true;
					}
					strongholdHireling.Paid = false;
					if (strongholdHireling.LeavesAfterFullPayCycle)
					{
						DismissHireling(strongholdHireling);
						flag2 = true;
					}
					if (flag2)
					{
						stringBuilder.Append(" " + StrongholdUtils.Format(gender, 12, text, GUIUtils.Format(466, num3)));
					}
					else if (flag)
					{
						stringBuilder.Append(" " + StrongholdUtils.Format(gender, 9, text, GUIUtils.Format(466, num3)));
					}
					else
					{
						stringBuilder.Append(" " + StrongholdUtils.Format(gender, 8, text, GUIUtils.Format(466, num3)));
					}
				}
			}
		}
		string s = Format(4) + " " + Format(65, GUIUtils.Format(466, num)) + stringBuilder;
		LogTimeEvent(s, NotificationType.Positive);
	}

	public int DaysRemainingInPayCycle()
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			if (m_events[i].EventType == StrongholdEvent.Type.PayHirelings)
			{
				return (int)(m_events[i].Time / (float)WorldTime.Instance.SecondsPerDay) + 1;
			}
		}
		return PayHirelingsDayCount;
	}

	public List<StrongholdPrisonerData> GetPrisoners()
	{
		return m_prisoners;
	}

	private int PrisonerIndex(string name)
	{
		int hashCode = name.GetHashCode();
		for (int i = 0; i < m_prisoners.Count; i++)
		{
			if (m_prisoners[i].PrisonerName.GetHashCode() == hashCode)
			{
				return i;
			}
		}
		return -1;
	}

	private int PrisonerIndex(CharacterDatabaseString name)
	{
		return PrisonerIndex(name.GetText());
	}

	public bool CanAddPrisoner()
	{
		return HasUpgrade(StrongholdUpgrade.Type.Dungeons);
	}

	public void AddPrisoner(GameObject prisoner)
	{
		if (CanAddPrisoner())
		{
			StrongholdPrisoner component = prisoner.GetComponent<StrongholdPrisoner>();
			if (component != null)
			{
				StrongholdPrisonerData strongholdPrisonerData = new StrongholdPrisonerData();
				strongholdPrisonerData.PrisonerName = component.PrisonerName;
				strongholdPrisonerData.PrisonerDescription = component.PrisonerDescription;
				strongholdPrisonerData.GlobalVariableName = component.GlobalVariableName;
				m_prisoners.Add(strongholdPrisonerData);
				GlobalVariables.Instance.SetVariable(component.GlobalVariableName, 1);
				GameUtilities.Destroy(prisoner, 0.1f);
			}
		}
	}

	public void RemovePrisoner(GameObject prisoner)
	{
		StrongholdPrisoner component = prisoner.GetComponent<StrongholdPrisoner>();
		if (component != null)
		{
			RemovePrisoner(component.PrisonerName);
		}
	}

	public void RemovePrisoner(CharacterDatabaseString name)
	{
		int num = PrisonerIndex(name);
		if (num != -1)
		{
			GlobalVariables.Instance.SetVariable(m_prisoners[num].GlobalVariableName, 0);
			m_prisoners.RemoveAt(num);
			if (OnEventChanged != null)
			{
				OnEventChanged(null);
			}
		}
	}

	public bool HasPrisoner(GameObject prisoner)
	{
		StrongholdPrisoner component = prisoner.GetComponent<StrongholdPrisoner>();
		if (component != null)
		{
			return HasPrisoner(component.PrisonerName);
		}
		return false;
	}

	public bool HasPrisoner(CharacterDatabaseString name)
	{
		return PrisonerIndex(name) >= 0;
	}

	public bool HasPrisoner(string name)
	{
		return PrisonerIndex(name) >= 0;
	}

	public CharacterDatabaseString GetRandomPrisonerName()
	{
		int count = m_prisoners.Count;
		if (count == 0)
		{
			return null;
		}
		int index = OEIRandom.Index(count);
		return m_prisoners[index].PrisonerName;
	}

	public int PrisonerCount()
	{
		return m_prisoners.Count;
	}

	public string PrisonerName(int index)
	{
		if (index < m_prisoners.Count)
		{
			return m_prisoners[index].PrisonerName.GetText();
		}
		return null;
	}

	public void SendVisitorDelayed(StrongholdVisitor visitor, float delayHours)
	{
		float num = delayHours * (float)WorldTime.Instance.SecondsPerMinute * (float)WorldTime.Instance.MinutesPerHour;
		for (int num2 = m_VisitorsToArriveDelayed.Count - 1; num2 >= 0; num2--)
		{
			if (m_VisitorsToArriveDelayed[num2].Tag == visitor.Tag)
			{
				m_VisitorsToArriveDelayed[num2].TimeToArrive = num;
				return;
			}
		}
		m_VisitorsToArriveDelayed.Add(new StrongholdVisitorArrival(visitor.Tag, num));
	}

	public bool SafeAddVisitor(StrongholdVisitor visitor, bool thwarted, bool almostThwarted)
	{
		if (visitor == null)
		{
			return false;
		}
		for (int num = m_VisitorsToArriveDelayed.Count - 1; num >= 0; num--)
		{
			if (m_VisitorsToArriveDelayed[num].Tag == visitor.Tag)
			{
				m_VisitorsToArriveDelayed.RemoveAt(num);
			}
		}
		if (IsVisitorDead(visitor))
		{
			return false;
		}
		if (m_visitors.Contains(visitor))
		{
			return false;
		}
		if (visitor.VisitorType == StrongholdVisitor.Type.PrisonerRequest)
		{
			Debug.LogError("Stronghold.SafeAddVisitor does not support PrisonerRequests (visitor '" + visitor.Name + "')");
		}
		if (thwarted)
		{
			LogTimeEvent(Format(39, visitor.Name), NotificationType.Negative);
			return true;
		}
		AddVisitor(visitor);
		if (!visitor.HasUnresolvedDilemma())
		{
			m_CurrentUnresolvedDilemmaStreak++;
		}
		else
		{
			m_CurrentUnresolvedDilemmaStreak = 0;
		}
		LogVisitorArrival(visitor.VisitorPrefab, visitor.FormatArrivalString(), visitor.GetArrivalNotificationType(), almostThwarted);
		return true;
	}

	public void LogVisitorArrival(CharacterStats visitor, string str, NotificationType notificationType, bool almostThwarted)
	{
		if (almostThwarted)
		{
			str = ((!visitor || visitor.Gender != Gender.Female) ? (str + " " + Format(62)) : (str + " " + Format(63)));
		}
		LogTimeEvent(str, notificationType);
	}

	public void AddVisitor(StrongholdVisitor visitor)
	{
		m_visitors.Add(visitor);
		if (visitor.VisitDuration == 0)
		{
			visitor.TimeToLeave = 2.14748365E+09f;
		}
		else
		{
			visitor.TimeToLeave = visitor.VisitDuration * WorldTime.Instance.SecondsPerDay;
		}
		GlobalVariables.Instance.SetVariable(visitor.GlobalVariableName, 1);
		if (OnVisitorStatusChanged != null)
		{
			OnVisitorStatusChanged(visitor);
		}
	}

	public bool RemoveVisitor(StrongholdVisitor visitor)
	{
		if (m_visitors.Remove(visitor))
		{
			GlobalVariables.Instance.SetVariable(visitor.GlobalVariableName, 0);
			if (OnVisitorStatusChanged != null)
			{
				OnVisitorStatusChanged(visitor);
			}
			visitor.AssociatedPrisoner = null;
			return true;
		}
		Debug.LogError("Stronghold tried to RemoveVisitor '" + visitor.Tag + "' but they weren't present.");
		return false;
	}

	public void AddVisitorToDeadList(StrongholdVisitor visitor)
	{
		m_visitorsDead.Add(visitor);
	}

	public void OnVisitorDeath(StrongholdVisitor visitor)
	{
		RemoveVisitor(visitor);
		AddVisitorToDeadList(visitor);
		string text = "";
		if (visitor.OnDeathPrestigeAdjustment != 0)
		{
			Prestige += visitor.OnDeathPrestigeAdjustment;
			text = GUIUtils.Format(1731, visitor.OnDeathPrestigeAdjustment + " " + GUIUtils.GetText(1432));
		}
		LogTimeEvent(Format(15, visitor.Name) + text, NotificationType.Negative);
		if (visitor.VisitorType == StrongholdVisitor.Type.RareItemMerchant)
		{
			m_disallowRareItemMerchants = true;
		}
	}

	public bool HasVisitor(StrongholdVisitor visitor)
	{
		return m_visitors.Contains(visitor);
	}

	public StrongholdVisitor FindVisitor(string name)
	{
		foreach (StrongholdVisitor visitor in m_visitors)
		{
			if (visitor.Name == name)
			{
				return visitor;
			}
		}
		return null;
	}

	public StrongholdVisitor GetVisitorByTag(string tag)
	{
		StrongholdVisitor[] visitors = Visitors;
		foreach (StrongholdVisitor strongholdVisitor in visitors)
		{
			if (strongholdVisitor.Tag == tag)
			{
				return strongholdVisitor;
			}
		}
		return null;
	}

	public bool IsVisitorDead(StrongholdVisitor visitor)
	{
		if (visitor.VisitorType == StrongholdVisitor.Type.RareItemMerchant && m_disallowRareItemMerchants)
		{
			return true;
		}
		return m_visitorsDead.Contains(visitor);
	}

	public StrongholdVisitor GetRandomNewVisitor(StrongholdVisitor.Type type)
	{
		if (!HasUpgrade(StrongholdUpgrade.Type.MainKeep))
		{
			return null;
		}
		if (type == StrongholdVisitor.Type.RareItemMerchant && m_disallowRareItemMerchants)
		{
			return null;
		}
		bool flag = m_CurrentUnresolvedDilemmaStreak >= MaximumUnresolvedDilemmaStreak;
		if (flag)
		{
			bool flag2 = false;
			for (int i = 0; i < Visitors.Length; i++)
			{
				StrongholdVisitor strongholdVisitor = Visitors[i];
				if (strongholdVisitor.VisitorType == type && !m_visitors.Contains(strongholdVisitor) && !m_visitorsDead.Contains(strongholdVisitor) && !VisitorIsBusy(strongholdVisitor) && strongholdVisitor.HasUnresolvedDilemma())
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = false;
			}
		}
		float num = 0f;
		for (int j = 0; j < Visitors.Length; j++)
		{
			StrongholdVisitor strongholdVisitor2 = Visitors[j];
			if (strongholdVisitor2.VisitorType == type && !m_visitors.Contains(strongholdVisitor2) && !m_visitorsDead.Contains(strongholdVisitor2) && !VisitorIsBusy(strongholdVisitor2) && (!flag || strongholdVisitor2.HasUnresolvedDilemma()))
			{
				num += strongholdVisitor2.GetAppearanceWeight(this);
			}
		}
		if (num <= 0f)
		{
			return null;
		}
		float num2 = OEIRandom.Range(0f, num);
		for (int k = 0; k < Visitors.Length; k++)
		{
			StrongholdVisitor strongholdVisitor3 = Visitors[k];
			if (strongholdVisitor3.VisitorType == type && !m_visitors.Contains(strongholdVisitor3) && !m_visitorsDead.Contains(strongholdVisitor3) && !VisitorIsBusy(strongholdVisitor3) && (!flag || strongholdVisitor3.HasUnresolvedDilemma()))
			{
				num2 -= strongholdVisitor3.GetAppearanceWeight(this);
				if (num2 < 0f)
				{
					return strongholdVisitor3;
				}
			}
		}
		UIDebug.Instance.LogOnScreenWarning(string.Concat("Stronghold Programming Error: failed to pick a new visitor even though there were valid ones (", type, ", totalweight=", num.ToString(), ")."), UIDebug.Department.Programming, 10f);
		return null;
	}

	public StrongholdVisitor GetRandomExistingVisitor(StrongholdVisitor.Type type, bool onlyKidnappable)
	{
		int num = 0;
		for (int i = 0; i < m_visitors.Count; i++)
		{
			StrongholdVisitor strongholdVisitor = m_visitors[i];
			if (strongholdVisitor.VisitorType == type && !VisitorIsBusy(strongholdVisitor) && (!onlyKidnappable || strongholdVisitor.KidnapDuration > 0))
			{
				num++;
			}
		}
		if (num == 0)
		{
			return null;
		}
		int num2 = OEIRandom.Index(num);
		for (int j = 0; j < m_visitors.Count; j++)
		{
			StrongholdVisitor strongholdVisitor2 = m_visitors[j];
			if (strongholdVisitor2.VisitorType == type && !VisitorIsBusy(strongholdVisitor2) && (!onlyKidnappable || strongholdVisitor2.KidnapDuration > 0))
			{
				num2--;
				if (num2 <= 0)
				{
					return strongholdVisitor2;
				}
			}
		}
		UIDebug.Instance.LogOnScreenWarning(string.Concat("Stronghold Programming Error: failed to pick an existing visitor even though there were valid ones (", type, ")."), UIDebug.Department.Programming, 10f);
		return null;
	}

	public void RansomVisitor(StrongholdVisitor visitor)
	{
		if (!CanAfford(visitor.MoneyValue))
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
			return;
		}
		for (int i = 0; i < m_events.Count; i++)
		{
			if (m_events[i].EventType == StrongholdEvent.Type.Kidnapped && (StrongholdVisitor)m_events[i].EventData == visitor)
			{
				AvailableCP -= visitor.MoneyValue;
				Prestige -= visitor.KidnapPrestigeAdjustment;
				AddVisitor(visitor);
				StrongholdEvent sevent = m_events[i];
				m_events.RemoveAt(i);
				PlayActionSound(UIActionSoundType.RansomVisitor);
				if (OnEventChanged != null)
				{
					OnEventChanged(sevent);
				}
				break;
			}
		}
	}

	public void RescueVisitor(StrongholdVisitor visitor, StoredCharacterInfo companion)
	{
		if (companion == null)
		{
			Debug.LogError("RescureVisitor wasn't given a stored character!");
		}
		else
		{
			if (!IsAvailable(companion))
			{
				return;
			}
			for (int i = 0; i < m_events.Count; i++)
			{
				if (m_events[i].EventType == StrongholdEvent.Type.Kidnapped && (StrongholdVisitor)m_events[i].EventData == visitor && m_events[i].EventCompanion == null)
				{
					m_events[i].EventCompanion = companion;
					PlayActionSound(UIActionSoundType.RescueVisitor);
					if (OnEventChanged != null)
					{
						OnEventChanged(m_events[i]);
					}
					break;
				}
			}
		}
	}

	public void PayOffVisitor(StrongholdVisitor visitor)
	{
		if (visitor.VisitorType == StrongholdVisitor.Type.BadVisitor || visitor.VisitorType == StrongholdVisitor.Type.Supplicant)
		{
			if (!CanAfford(visitor.MoneyValue))
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
			}
			else if (HasVisitor(visitor))
			{
				PlayActionSound(UIActionSoundType.PayOffVisitor);
				AvailableCP -= visitor.MoneyValue;
				RemoveVisitor(visitor);
			}
		}
	}

	public void EscortVisitor(StrongholdVisitor visitor, StoredCharacterInfo companion, int escortIndex)
	{
		if (((bool)companion && !IsAvailable(companion)) || !HasVisitor(visitor))
		{
			return;
		}
		int time = visitor.EscortDuration;
		if (escortIndex >= 0)
		{
			if (escortIndex < visitor.SpecialEscorts.Length)
			{
				time = visitor.SpecialEscorts[escortIndex].Duration;
			}
			else
			{
				Debug.LogError("Visitor '" + visitor.Name + "' has no escort index '" + escortIndex + "'.");
			}
		}
		RemoveVisitor(visitor);
		StrongholdEvent strongholdEvent = AddEvent(StrongholdEvent.Type.Escorting, visitor, time);
		if (strongholdEvent != null)
		{
			strongholdEvent.EventDataInt = escortIndex;
			strongholdEvent.EventCompanion = companion;
			PlayActionSound(UIActionSoundType.EscortVisitor);
			if (OnEventChanged != null)
			{
				OnEventChanged(strongholdEvent);
			}
		}
	}

	public void AcceptMoney(StrongholdVisitor visitor)
	{
		CharacterDatabaseString characterDatabaseString = ConfirmPrisoner(visitor);
		if (characterDatabaseString != null)
		{
			PlayActionSound(UIActionSoundType.AcceptMoney);
			AvailableCP += visitor.MoneyValue;
			RemovePrisoner(characterDatabaseString);
			RemoveVisitor(visitor);
		}
	}

	public void AcceptItem(StrongholdVisitor visitor)
	{
		CharacterDatabaseString characterDatabaseString = ConfirmPrisoner(visitor);
		if (characterDatabaseString != null)
		{
			PlayActionSound(UIActionSoundType.AcceptItem);
			Accept(visitor.VisitorItem);
			RemovePrisoner(characterDatabaseString);
			RemoveVisitor(visitor);
		}
	}

	public void BuyItem(StrongholdVisitor visitor)
	{
		if (!CanAfford(visitor.MoneyValue))
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
			return;
		}
		AvailableCP -= visitor.MoneyValue;
		Accept(visitor.VisitorItem);
		RemoveVisitor(visitor);
		PlayActionSound(UIActionSoundType.BuyItem);
		AddVisitorToDeadList(visitor);
	}

	private CharacterDatabaseString ConfirmPrisoner(StrongholdVisitor visitor)
	{
		if (visitor.VisitorType != StrongholdVisitor.Type.PrisonerRequest)
		{
			return null;
		}
		if (!HasVisitor(visitor))
		{
			return null;
		}
		CharacterDatabaseString associatedPrisoner = visitor.AssociatedPrisoner;
		if (associatedPrisoner == null)
		{
			return null;
		}
		if (!HasPrisoner(associatedPrisoner))
		{
			return null;
		}
		return associatedPrisoner;
	}

	public void CreateVisitorItems(Inventory inven)
	{
		for (int num = m_itemsFromVisitors.Count - 1; num >= 0; num--)
		{
			if (inven.AddItem(m_itemsFromVisitors[num], 1) == 0)
			{
				m_itemsFromVisitors.RemoveAt(num);
			}
		}
	}

	public bool PickAttack(ref StrongholdAttack attack)
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter == null || Attacks == null)
		{
			attack = null;
			return true;
		}
		List<StrongholdAttack> list = new List<StrongholdAttack>();
		float num = 0f;
		StrongholdAttack[] attacks = Attacks;
		foreach (StrongholdAttack strongholdAttack in attacks)
		{
			if (strongholdAttack.IsValid(s_playerCharacter, this))
			{
				list.Add(strongholdAttack);
				num += strongholdAttack.Weight;
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		float num2 = OEIRandom.Range(0f, num);
		float num3 = 0f;
		foreach (StrongholdAttack item in list)
		{
			num3 += item.Weight;
			if (num2 < num3)
			{
				attack = item;
				return true;
			}
		}
		attack = list[0];
		return true;
	}

	public void AutoResolveAttack()
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter == null)
		{
			return;
		}
		PlayActionSound(UIActionSoundType.AutoResolveAttack);
		for (int i = 0; i < m_events.Count; i++)
		{
			StrongholdEvent strongholdEvent = m_events[i];
			if (strongholdEvent.EventType == StrongholdEvent.Type.Attack)
			{
				m_events.RemoveAt(i);
				strongholdEvent.ProcessEvent(s_playerCharacter, this);
				if (OnEventChanged != null)
				{
					OnEventChanged(strongholdEvent);
				}
				break;
			}
		}
	}

	public void ManualResolveAttack()
	{
		PlayActionSound(UIActionSoundType.ManualResolveAttack);
		for (int i = 0; i < m_events.Count; i++)
		{
			StrongholdEvent strongholdEvent = m_events[i];
			if (strongholdEvent.EventType == StrongholdEvent.Type.Attack)
			{
				m_events.RemoveAt(i);
				((StrongholdAttack)strongholdEvent.EventData)?.ManualResolve(this);
				if (OnEventChanged != null)
				{
					OnEventChanged(strongholdEvent);
				}
				break;
			}
		}
	}

	public EternityTimeInterval PendingAttackTimeLeft()
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			StrongholdEvent strongholdEvent = m_events[i];
			if (strongholdEvent.EventType == StrongholdEvent.Type.Attack)
			{
				if ((StrongholdAttack)strongholdEvent.EventData != null)
				{
					return new EternityTimeInterval((int)strongholdEvent.Time);
				}
				return null;
			}
		}
		return null;
	}

	public StrongholdAttack PendingAttack()
	{
		for (int i = 0; i < m_events.Count; i++)
		{
			StrongholdEvent strongholdEvent = m_events[i];
			if (strongholdEvent.EventType == StrongholdEvent.Type.Attack)
			{
				return (StrongholdAttack)strongholdEvent.EventData;
			}
		}
		return null;
	}

	public SortedList<int, List<Damageables>> GatherDamageables(int maxLevel)
	{
		if (maxLevel < 1)
		{
			return null;
		}
		SortedList<int, List<Damageables>> sortedList = new SortedList<int, List<Damageables>>();
		for (int i = 1; i <= maxLevel; i++)
		{
			List<Damageables> list = new List<Damageables>();
			foreach (StrongholdHireling item in m_hirelingsHired)
			{
				if (item.Paid && item.HirelingPrefab != null)
				{
					CharacterStats component = item.HirelingPrefab.GetComponent<CharacterStats>();
					if (component != null && component.ScaledLevel == i)
					{
						Damageables damageables = new Damageables();
						damageables.isUpgrade = false;
						damageables.hireling = item;
						list.Add(damageables);
					}
				}
			}
			foreach (StrongholdUpgrade.Type item2 in m_upgradesBuilt)
			{
				StrongholdUpgrade upgradeInfo = GetUpgradeInfo(item2);
				if (upgradeInfo != null && upgradeInfo.Level == i && upgradeInfo.Destructible)
				{
					Damageables damageables2 = new Damageables();
					damageables2.isUpgrade = true;
					damageables2.upgrade = upgradeInfo;
					list.Add(damageables2);
				}
			}
			if (list.Count > 0)
			{
				sortedList.Add(i, list);
			}
		}
		if (sortedList.Count == 0)
		{
			return null;
		}
		return sortedList;
	}

	public void View()
	{
		UnviewedEventCount = 0;
		UIHudAlerts.Cancel(UIActionBarOnClick.ActionType.Stronghold);
		UIQuestNotifications.Instance.ClearStronghold();
	}

	public void ViewInternal()
	{
		UnviewedEventCountInternal = 0;
	}

	public void LogTimeEvent(string s, NotificationType notify)
	{
		string text = CurrentTime.Format(StrongholdUtils.GetText(58));
		while (m_log.Count >= 64)
		{
			m_log.RemoveAt(0);
		}
		m_log.Add(text + ": " + s);
		m_DayEventLogged = true;
		if (notify != 0)
		{
			UnviewedEventCount++;
			UnviewedEventCountInternal++;
			UIHudAlerts.Alert(UIActionBarOnClick.ActionType.Stronghold);
		}
		if (OnLogMessage != null)
		{
			OnLogMessage(notify, text, s);
		}
		RegisterNotificationSound(notify);
	}

	public void LogTurnEvent(string s, NotificationType notify)
	{
		string text = Format(57, m_currentTurn);
		while (m_log.Count >= 64)
		{
			m_log.RemoveAt(0);
		}
		m_log.Add(text + ": " + s);
		m_TurnEventLogged = true;
		if (notify != 0)
		{
			UnviewedEventCount++;
			UnviewedEventCountInternal++;
			UIHudAlerts.Alert(UIActionBarOnClick.ActionType.Stronghold);
		}
		if (OnLogMessage != null)
		{
			OnLogMessage(notify, text, s);
		}
		RegisterNotificationSound(notify);
	}

	protected void RegisterNotificationSound(NotificationType notify)
	{
		switch (notify)
		{
		case NotificationType.Positive:
			if (m_PendingNotificationSound == NotificationType.None)
			{
				m_PendingNotificationSound = notify;
			}
			break;
		case NotificationType.Negative:
			m_PendingNotificationSound = notify;
			break;
		case NotificationType.ConstructionFinished:
			if (m_PendingNotificationSound != NotificationType.Negative)
			{
				m_PendingNotificationSound = NotificationType.ConstructionFinished;
			}
			break;
		}
	}

	protected void PlayNotificationSound()
	{
		switch (m_PendingNotificationSound)
		{
		case NotificationType.Positive:
			GlobalAudioPlayer.SPlay(UIActionSoundType.NotifyPositive);
			break;
		case NotificationType.Negative:
			GlobalAudioPlayer.SPlay(UIActionSoundType.NotifyNegative);
			break;
		case NotificationType.ConstructionFinished:
			GlobalAudioPlayer.SPlay(UIActionSoundType.NotifyConstructionFinished);
			break;
		}
		m_PendingNotificationSound = NotificationType.None;
	}

	public int GetSecurity()
	{
		int num = Security;
		for (int i = 0; i < m_visitors.Count; i++)
		{
			num += m_visitors[i].GetSecurityAdjustment();
		}
		return num;
	}

	public int GetPrestige()
	{
		int num = Prestige;
		for (int i = 0; i < m_visitors.Count; i++)
		{
			num += m_visitors[i].GetPrestigeAdjustment();
		}
		return num;
	}

	public int GetStat(StatType stat)
	{
		return stat switch
		{
			StatType.Prestige => GetPrestige(), 
			StatType.Security => GetSecurity(), 
			_ => 0, 
		};
	}

	public bool CanAfford(int amount)
	{
		return AvailableCP >= amount;
	}

	public void Accept(Item item)
	{
		m_itemsFromVisitors.Add(item);
		ProcessTreasuryChanged();
	}

	public void PaydownDebt(int amount)
	{
		if (amount > Debt)
		{
			amount = Debt;
		}
		if (!CanAfford(amount))
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(421));
			return;
		}
		Debt -= amount;
		AvailableCP -= amount;
	}

	private void ProcessTreasuryChanged()
	{
	}

	public void CompanionActivation(Guid guid, bool active)
	{
		GameObject gameObject = RestoreCompanion(guid);
		if (!(gameObject != null))
		{
			return;
		}
		PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
		component.enabled = active;
		if (!active)
		{
			return;
		}
		Mover mover = component.Mover;
		mover.enabled = false;
		int selectedSlot;
		Vector3 vector = component.CalculateFormationPosition(GameState.s_playerCharacter.transform.position, ignoreSelection: true, out selectedSlot);
		if (NavMesh.SamplePosition(vector, out var hit, 100f, -1))
		{
			vector = hit.position;
		}
		component.transform.position = vector;
		component.gameObject.transform.position = vector;
		component.gameObject.transform.rotation = base.transform.rotation;
		mover.enabled = true;
		component.StateManager.AbortStateStack();
		component.InitAI();
		component.gameObject.transform.rotation = base.transform.rotation;
		PartyMemberAI.AddToActiveParty(gameObject, fromScript: false);
		if (component.SummonedCreatureList.Count <= 0)
		{
			return;
		}
		foreach (GameObject summonedCreature in component.SummonedCreatureList)
		{
			AIController component2 = summonedCreature.GetComponent<AIController>();
			if ((bool)component2 && component2.SummonType == AIController.AISummonType.AnimalCompanion)
			{
				Mover component3 = summonedCreature.GetComponent<Mover>();
				if (component3 != null)
				{
					summonedCreature.transform.position = GameUtilities.NearestUnoccupiedLocation(gameObject.transform.position, component3.Radius, 10f, component3);
				}
				else
				{
					summonedCreature.transform.position = GameUtilities.NearestUnoccupiedLocation(gameObject.transform.position, 0.5f, 10f, null);
				}
			}
		}
	}

	public void StoreAnimalCompanion(StoredCharacterInfo companion)
	{
		if (!m_storedAnimalCompanions.ContainsKey(companion.GUID) && !m_storedAnimalCompanions.ContainsKey(companion.GUID))
		{
			m_storedAnimalCompanions.Add(companion.GUID, companion);
		}
	}

	public GameObject RestoreAnimalCompanion(GameObject parent, Guid guid)
	{
		if (!m_storedAnimalCompanions.ContainsKey(guid))
		{
			return null;
		}
		StoredCharacterInfo storedCharacterInfo = m_storedAnimalCompanions[guid];
		if (storedCharacterInfo == null)
		{
			return null;
		}
		GameObject gameObject = storedCharacterInfo.RestoreCharacter(keepPacked: false);
		if ((bool)gameObject)
		{
			m_storedAnimalCompanions.Remove(guid);
			Mover component = gameObject.GetComponent<Mover>();
			if (component != null)
			{
				gameObject.transform.position = GameUtilities.NearestUnoccupiedLocation(parent.transform.position, component.Radius, 10f, component);
			}
			else
			{
				gameObject.transform.position = GameUtilities.NearestUnoccupiedLocation(parent.transform.position, 0.5f, 10f, null);
			}
			Persistence component2 = storedCharacterInfo.GetComponent<Persistence>();
			if ((bool)component2)
			{
				PersistenceManager.RemoveObject(component2);
			}
			GameUtilities.Destroy(storedCharacterInfo.gameObject);
			return gameObject;
		}
		Debug.LogError("Unable to restore animal companion from GUID " + guid.ToString());
		return null;
	}

	public GameObject RestoreAnimalCompanionToNode(GameObject parent, Guid guid)
	{
		if (!m_storedAnimalCompanions.ContainsKey(guid))
		{
			return null;
		}
		StoredCharacterInfo storedCharacterInfo = m_storedAnimalCompanions[guid];
		if (storedCharacterInfo == null)
		{
			return null;
		}
		GameObject gameObject = storedCharacterInfo.RestoreCharacter(keepPacked: true);
		if ((bool)gameObject)
		{
			Mover component = gameObject.GetComponent<Mover>();
			if (component != null)
			{
				gameObject.transform.position = GameUtilities.NearestUnoccupiedLocation(parent.transform.position, component.Radius, 10f, component);
			}
			else
			{
				gameObject.transform.position = GameUtilities.NearestUnoccupiedLocation(parent.transform.position, 0.5f, 10f, null);
			}
			PartyMemberAI component2 = gameObject.GetComponent<PartyMemberAI>();
			if ((bool)component2)
			{
				GameUtilities.DestroyImmediate(component2);
			}
			AIPackageController aIPackageController = gameObject.GetComponent<AIPackageController>();
			if (aIPackageController == null)
			{
				aIPackageController = gameObject.AddComponent<AIPackageController>();
			}
			aIPackageController.InitAI();
			Persistence component3 = gameObject.GetComponent<Persistence>();
			if ((bool)component3)
			{
				GameUtilities.DestroyImmediate(component3);
			}
			return gameObject;
		}
		Debug.LogError("Unable to restore animal companion from GUID " + guid.ToString());
		return null;
	}

	public StoredCharacterInfo StoreCompanion(GameObject companion)
	{
		TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTY_MEMBER_DISMISSED);
		UIOffscreenObjectManager.RemovePointer(companion);
		InstanceID component = companion.GetComponent<InstanceID>();
		if (component == null)
		{
			Debug.LogError("Companion " + companion.name + " doesn't have a instance id! Can't store them in the stronghold!", companion);
			return null;
		}
		if (m_storedCompanions.ContainsKey(component.Guid))
		{
			return m_storedCompanions[component.Guid];
		}
		StoredCharacterInfo storedCharacterInfo = StoredCharacterInfo.ConvertCharacterToStored(companion);
		if (!m_storedCompanions.ContainsKey(storedCharacterInfo.GUID))
		{
			m_storedCompanions.Add(storedCharacterInfo.GUID, storedCharacterInfo);
		}
		CompanionStrongholdNode[] array = UnityEngine.Object.FindObjectsOfType<CompanionStrongholdNode>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].PlaceCompanion();
		}
		return storedCharacterInfo;
	}

	public bool HasStoredCompanion(Guid guid)
	{
		return m_storedCompanions.ContainsKey(guid);
	}

	public StoredCharacterInfo GetStoredCompanion(Guid guid)
	{
		if (m_storedCompanions.ContainsKey(guid))
		{
			return m_storedCompanions[guid];
		}
		return null;
	}

	public GameObject RestoreCompanion(Guid guid)
	{
		if (!m_storedCompanions.ContainsKey(guid))
		{
			return null;
		}
		StoredCharacterInfo storedCharacterInfo = m_storedCompanions[guid];
		if (storedCharacterInfo == null)
		{
			return null;
		}
		CompanionStrongholdNode[] array = UnityEngine.Object.FindObjectsOfType<CompanionStrongholdNode>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].CompanionObj != null))
			{
				continue;
			}
			InstanceID component = array[i].CompanionObj.GetComponent<InstanceID>();
			if (component != null && component.Guid == storedCharacterInfo.GUID)
			{
				array[i].RemoveCompanion();
				if (AdventurersSpawnedInMap.Contains(storedCharacterInfo.GUID))
				{
					AdventurersSpawnedInMap.Remove(storedCharacterInfo.GUID);
				}
			}
		}
		GameObject gameObject = storedCharacterInfo.RestoreCharacter(keepPacked: false);
		if ((bool)gameObject)
		{
			if (storedCharacterInfo.HasRested)
			{
				CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
				if ((bool)component2)
				{
					component2.HandleGameOnResting();
				}
			}
			GameObject objectByID = InstanceID.GetObjectByID(storedCharacterInfo.AnimalCompanionGUID);
			PartyMemberAI component3 = gameObject.GetComponent<PartyMemberAI>();
			if ((bool)component3)
			{
				component3.AssignedSlot = -1;
			}
			m_storedCompanions.Remove(guid);
			GameUtilities.Destroy(storedCharacterInfo.gameObject);
			Persistence component4 = storedCharacterInfo.GetComponent<Persistence>();
			if ((bool)component4)
			{
				PersistenceManager.RemoveObject(component4);
			}
			PartyMemberAI.AddToActiveParty(gameObject, fromScript: true);
			if ((bool)objectByID)
			{
				PartyMemberAI.AddSummonToActiveParty(objectByID, gameObject, AIController.AISummonType.AnimalCompanion, fromScript: true);
			}
			return gameObject;
		}
		Debug.LogError("Unable to restore companion from GUID " + guid.ToString());
		return null;
	}

	public GameObject RestoreCompanionToNode(Guid guid, GameObject point)
	{
		if (!m_storedCompanions.ContainsKey(guid))
		{
			return null;
		}
		StoredCharacterInfo storedCharacterInfo = m_storedCompanions[guid];
		if (storedCharacterInfo == null)
		{
			return null;
		}
		GameObject gameObject = storedCharacterInfo.RestoreCharacter(keepPacked: true);
		if ((bool)gameObject)
		{
			gameObject.transform.position = point.transform.position;
			gameObject.transform.rotation = point.transform.rotation;
			if (storedCharacterInfo.HasRested)
			{
				CharacterStats component = gameObject.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					component.HandleGameOnResting();
				}
			}
			PartyMemberAI component2 = gameObject.GetComponent<PartyMemberAI>();
			if ((bool)component2)
			{
				GameUtilities.DestroyImmediate(component2);
			}
			AIPackageController aIPackageController = gameObject.GetComponent<AIPackageController>();
			if (aIPackageController == null)
			{
				aIPackageController = gameObject.AddComponent<AIPackageController>();
			}
			aIPackageController.Patroller = true;
			aIPackageController.PreferredPatrolPoint = point;
			aIPackageController.InitAI();
			Persistence component3 = gameObject.GetComponent<Persistence>();
			if ((bool)component3)
			{
				GameUtilities.DestroyImmediate(component3);
			}
			Persistence[] componentsInChildren = gameObject.GetComponentsInChildren<Persistence>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameUtilities.DestroyImmediate(componentsInChildren[i]);
			}
			GameObject result = UnityEngine.Object.Instantiate(gameObject);
			GameUtilities.DestroyImmediate(gameObject);
			return result;
		}
		Debug.LogError("Unable to restore companion from GUID " + guid.ToString());
		return null;
	}

	public void DestroyStoredCompanionOnDeath(GameObject obj)
	{
		InstanceID component = obj.GetComponent<InstanceID>();
		if (!component)
		{
			return;
		}
		Guid guid = component.Guid;
		if (!m_storedCompanions.ContainsKey(guid))
		{
			return;
		}
		StoredCharacterInfo storedCharacterInfo = m_storedCompanions[guid];
		if (storedCharacterInfo == null)
		{
			return;
		}
		if (storedCharacterInfo.AnimalCompanionGUID != Guid.Empty && m_storedAnimalCompanions.ContainsKey(storedCharacterInfo.AnimalCompanionGUID))
		{
			StoredCharacterInfo storedCharacterInfo2 = m_storedAnimalCompanions[storedCharacterInfo.AnimalCompanionGUID];
			if (storedCharacterInfo2 != null)
			{
				m_storedAnimalCompanions.Remove(storedCharacterInfo.AnimalCompanionGUID);
				GameUtilities.Destroy(storedCharacterInfo2.gameObject);
				Persistence component2 = storedCharacterInfo2.GetComponent<Persistence>();
				if ((bool)component2)
				{
					PersistenceManager.RemoveObject(component2);
				}
			}
		}
		m_storedCompanions.Remove(guid);
		GameUtilities.Destroy(storedCharacterInfo.gameObject);
		foreach (Guid attachedObject in storedCharacterInfo.AttachedObjects)
		{
			PersistenceManager.RemoveObject(attachedObject, removeEvenIfPacked: true);
		}
		Persistence component3 = storedCharacterInfo.GetComponent<Persistence>();
		if ((bool)component3)
		{
			PersistenceManager.RemoveObject(component3);
		}
	}

	public void DestroyStoredCompanions()
	{
		foreach (StoredCharacterInfo value in m_storedCompanions.Values)
		{
			GameUtilities.DestroyImmediate(value.gameObject);
		}
		foreach (StoredCharacterInfo value2 in m_storedAnimalCompanions.Values)
		{
			GameUtilities.DestroyImmediate(value2.gameObject);
		}
	}

	public static string Format(int stringId, params object[] param)
	{
		return StrongholdUtils.Format(stringId, param);
	}
}
