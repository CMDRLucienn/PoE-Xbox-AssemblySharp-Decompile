using System;
using System.Collections.Generic;
using System.Text;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class StrongholdEvent
{
	public enum Type
	{
		CollectTaxes,
		SpawnAdventure,
		BuildUpgrade,
		SpawnIngredients,
		PayHirelings,
		RandomEvent,
		Kidnapped,
		Escorting,
		SupplicantEffectsWearOff,
		Attack,
		VisitorKilled,
		Count
	}

	private Type m_type;

	private int m_randomValue1;

	private int m_randomValue2;

	private object m_data;

	private int m_dataInt = -1;

	private StrongholdUpgrade.Type m_eventUpgradeType = StrongholdUpgrade.Type.None;

	private StrongholdVisitorSerializeData m_eventVisitor = new StrongholdVisitorSerializeData();

	private string m_eventAttackName = string.Empty;

	private StoredCharacterInfo m_companion;

	private Guid m_serializedCompanion = Guid.Empty;

	private StoredCharacterInfo m_abandonedCompanion;

	private Guid m_serializedAbanondedCompanion = Guid.Empty;

	private static int[,] s_copper_range_by_roll = new int[16, 3]
	{
		{ 10, 0, 150 },
		{ 20, 150, 300 },
		{ 30, 400, 500 },
		{ 40, 600, 700 },
		{ 50, 700, 850 },
		{ 60, 850, 950 },
		{ 70, 1000, 1200 },
		{ 80, 1200, 1500 },
		{ 90, 1500, 1800 },
		{ 100, 1800, 2200 },
		{ 110, 2200, 2600 },
		{ 120, 2600, 3000 },
		{ 130, 3000, 3800 },
		{ 140, 3800, 4600 },
		{ 150, 4600, 5200 },
		{ 2147483647, 5200, 7500 }
	};

	public float Time { get; set; }

	public Type EventType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public int RandomValue1
	{
		get
		{
			return m_randomValue1;
		}
		set
		{
			m_randomValue1 = value;
		}
	}

	public int RandomValue2
	{
		get
		{
			return m_randomValue2;
		}
		set
		{
			m_randomValue2 = value;
		}
	}

	[ExcludeFromSerialization]
	public object EventData
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public int EventDataInt
	{
		get
		{
			return m_dataInt;
		}
		set
		{
			m_dataInt = value;
		}
	}

	public StrongholdUpgrade.Type EventUpgradeTypeSerialized
	{
		get
		{
			if (EventData != null && EventData is StrongholdUpgrade.Type)
			{
				m_eventUpgradeType = (StrongholdUpgrade.Type)EventData;
			}
			return m_eventUpgradeType;
		}
		set
		{
			m_eventUpgradeType = value;
		}
	}

	public StrongholdVisitorSerializeData EventVisitorNameSerialized
	{
		get
		{
			if (EventData != null && EventData is StrongholdVisitor)
			{
				m_eventVisitor = new StrongholdVisitorSerializeData();
				m_eventVisitor.Tag = (EventData as StrongholdVisitor).Tag;
				m_eventVisitor.AssociatedPrisoner = (EventData as StrongholdVisitor).AssociatedPrisoner;
				m_eventVisitor.TimeToLeave = (EventData as StrongholdVisitor).TimeToLeave;
			}
			return m_eventVisitor;
		}
		set
		{
			m_eventVisitor = value;
		}
	}

	public string EventAttackNameSerialized
	{
		get
		{
			if (EventData != null && EventData is StrongholdAttack)
			{
				m_eventAttackName = (EventData as StrongholdAttack).Tag;
			}
			return m_eventAttackName;
		}
		set
		{
			m_eventAttackName = value;
		}
	}

	[ExcludeFromSerialization]
	public StoredCharacterInfo EventCompanion
	{
		get
		{
			return m_companion;
		}
		set
		{
			m_companion = value;
		}
	}

	public Guid SerializedCompanion
	{
		get
		{
			if (EventCompanion != null)
			{
				m_serializedCompanion = EventCompanion.GUID;
			}
			return m_serializedCompanion;
		}
		set
		{
			m_serializedCompanion = value;
		}
	}

	[ExcludeFromSerialization]
	public StoredCharacterInfo EventAbandonedCompanion
	{
		get
		{
			return m_abandonedCompanion;
		}
		set
		{
			m_abandonedCompanion = value;
		}
	}

	public Guid SerializedAbandonedCompanion
	{
		get
		{
			if (EventAbandonedCompanion != null)
			{
				m_serializedAbanondedCompanion = EventAbandonedCompanion.GUID;
			}
			return m_serializedAbanondedCompanion;
		}
		set
		{
			m_serializedAbanondedCompanion = value;
		}
	}

	public static bool UsesTurns(Type type)
	{
		if (type == Type.CollectTaxes || type == Type.SpawnAdventure || type == Type.SpawnIngredients || type == Type.VisitorKilled)
		{
			return true;
		}
		return false;
	}

	public static bool UsesGameTimeUnits(Type type)
	{
		return !UsesTurns(type);
	}

	public static StrongholdEvent Create(float time, Type type, object data)
	{
		StrongholdEvent strongholdEvent = new StrongholdEvent();
		strongholdEvent.Time = time;
		strongholdEvent.m_type = type;
		strongholdEvent.m_data = data;
		strongholdEvent.m_companion = null;
		strongholdEvent.m_abandonedCompanion = null;
		switch (type)
		{
		case Type.CollectTaxes:
			strongholdEvent.m_randomValue1 = OEIRandom.Range(1, 50);
			strongholdEvent.m_randomValue2 = OEIRandom.Range(1, 35);
			break;
		case Type.SpawnAdventure:
			strongholdEvent.m_randomValue1 = OEIRandom.Range(1, 50);
			strongholdEvent.m_randomValue2 = OEIRandom.Range(1, 50);
			break;
		case Type.RandomEvent:
			strongholdEvent.m_randomValue1 = OEIRandom.Range(1, 100);
			strongholdEvent.m_randomValue2 = OEIRandom.Range(1, 100);
			break;
		}
		return strongholdEvent;
	}

	public void ProcessEvent(Player player, Stronghold stronghold)
	{
		switch (m_type)
		{
		case Type.CollectTaxes:
			ProcessCollectTaxes(player, stronghold);
			break;
		case Type.SpawnAdventure:
			ProcessSpawnAdventure(player, stronghold);
			break;
		case Type.BuildUpgrade:
			ProcessBuildUpgrade(player, stronghold);
			break;
		case Type.SpawnIngredients:
			ProcessSpawnIngredients(player, stronghold);
			break;
		case Type.PayHirelings:
			ProcessPayHirelings(player, stronghold);
			break;
		case Type.RandomEvent:
			ProcessRandomEvent(player, stronghold);
			break;
		case Type.Kidnapped:
			ProcessKidnapped(player, stronghold);
			break;
		case Type.Escorting:
			ProcessEscorting(player, stronghold);
			break;
		case Type.SupplicantEffectsWearOff:
			ProcessSupplicantEffectsWearOff(player, stronghold);
			break;
		case Type.Attack:
			ProcessAttack(player, stronghold);
			break;
		}
	}

	private void ProcessCollectTaxes(Player player, Stronghold stronghold)
	{
		int prestige = stronghold.GetPrestige();
		int security = stronghold.GetSecurity();
		int num = CopperByRoll(m_randomValue1 + prestige);
		int num2 = CopperByRoll(m_randomValue2 + prestige - security);
		int num3 = num - num2;
		int num4 = 0;
		if (stronghold.IsErlTaxActive)
		{
			num4 = Mathf.FloorToInt(stronghold.ErlTaxRatio * (float)num3);
			num3 -= num4;
		}
		StringBuilder stringBuilder = new StringBuilder(Stronghold.Format(17));
		if (num3 > 0)
		{
			if (num2 > 0)
			{
				stringBuilder.Append(" " + Stronghold.Format(18, GUIUtils.Format(466, num2)));
			}
			if (num4 > 0)
			{
				stringBuilder.Append(" " + Stronghold.Format(278, GUIUtils.Format(466, num4)));
			}
			if (num3 > stronghold.Debt)
			{
				num3 -= stronghold.Debt;
				if (stronghold.Debt > 0)
				{
					stringBuilder.Append(" " + Stronghold.Format(20));
				}
				stringBuilder.Append(" " + Stronghold.Format(19, GUIUtils.Format(466, num3)));
				stronghold.Debt = 0;
				PlayerInventory component = player.GetComponent<PlayerInventory>();
				if ((bool)component)
				{
					component.currencyTotalValue.v += num3;
				}
			}
			else
			{
				stronghold.Debt -= num3;
				stringBuilder.Append(" " + Stronghold.Format(21, GUIUtils.Format(466, stronghold.Debt)));
			}
		}
		else
		{
			stringBuilder.Append(" " + Stronghold.Format(22));
		}
		stronghold.LogTurnEvent(stringBuilder.ToString(), Stronghold.NotificationType.Positive);
		stronghold.AddEvent(Type.CollectTaxes, null, stronghold.CollectTaxesTurnCount);
	}

	private int CopperByRoll(int roll)
	{
		for (int i = 0; i < s_copper_range_by_roll.GetLength(0); i++)
		{
			if (roll <= s_copper_range_by_roll[i, 0])
			{
				return OEIRandom.Range(s_copper_range_by_roll[i, 1], s_copper_range_by_roll[i, 2]);
			}
		}
		return 0;
	}

	private void ProcessSpawnAdventure(Player player, Stronghold stronghold)
	{
		try
		{
			if (stronghold.HasUpgrade(StrongholdUpgrade.Type.MainKeep))
			{
				int num = m_randomValue1 + m_randomValue2 + stronghold.GetPrestige();
				StrongholdAdventure.Type type = StrongholdAdventure.Type.None;
				type = ((num > 25) ? ((num <= 50) ? StrongholdAdventure.Type.Minor : ((num <= 75) ? StrongholdAdventure.Type.Average : ((num <= 100) ? StrongholdAdventure.Type.Major : ((num > 125) ? StrongholdAdventure.Type.Legendary : StrongholdAdventure.Type.Grand)))) : StrongholdAdventure.Type.None);
				stronghold.AddAdventure(type);
			}
		}
		finally
		{
			stronghold.AddEvent(Type.SpawnAdventure, null, stronghold.SpawnAdventureTurnCount);
		}
	}

	private void ProcessBuildUpgrade(Player player, Stronghold stronghold)
	{
		stronghold.CompleteBuildingUpgrade((StrongholdUpgrade.Type)m_data);
	}

	public static void ProcessSpawnIngredients(Player player, Stronghold stronghold)
	{
		stronghold.SpawnIngredients();
		stronghold.AddEvent(Type.SpawnIngredients, null, stronghold.SpawnIngredientsTurnCount);
	}

	public static void ProcessPayHirelings(Player player, Stronghold stronghold)
	{
		stronghold.ProcessPayCycle();
		stronghold.AddEvent(Type.PayHirelings, null, stronghold.PayHirelingsDayCount);
	}

	private void ProcessRandomEvent(Player player, Stronghold stronghold)
	{
		stronghold.AddEvent(Type.RandomEvent, null, OEIRandom.Range(1, stronghold.RandomEventMaxDayCount));
		bool flag = false;
		int num = stronghold.GetPrestige() - (30 - stronghold.GetSecurity());
		while (!flag)
		{
			int num2 = m_randomValue1 + num;
			if (num2 <= 30)
			{
				flag = true;
			}
			else if (num2 <= 40)
			{
				flag = HandlePrisonBreak(player, stronghold);
			}
			else if (num2 <= 50)
			{
				flag = HandleKidnapping(player, stronghold);
			}
			else if (num2 <= 60)
			{
				flag = HandleBadVisitor(player, stronghold);
			}
			else if (num2 <= 70)
			{
				flag = HandleAttackNotification(player, stronghold);
			}
			else if (num2 <= 80)
			{
				flag = HandleGuestHireling(player, stronghold);
			}
			else if (num2 <= 90)
			{
				flag = HandlePrisonerRequest(player, stronghold);
			}
			else if (num2 > 100)
			{
				flag = ((num2 > 110) ? ((num2 > 120) ? (num2 > 130 || HandleRareItemOffer(player, stronghold)) : HandlePrestigiousVisitor(player, stronghold)) : HandleSupplicant(player, stronghold));
			}
			m_randomValue1 = OEIRandom.Range(1, 100);
			num--;
		}
	}

	public bool HandlePrisonBreak(Player player, Stronghold stronghold)
	{
		if (!stronghold.HasUpgrade(StrongholdUpgrade.Type.Dungeons))
		{
			return false;
		}
		CharacterDatabaseString randomPrisonerName = stronghold.GetRandomPrisonerName();
		if (randomPrisonerName == null)
		{
			return false;
		}
		if (m_randomValue1 - stronghold.GetSecurity() * 2 <= 0)
		{
			stronghold.LogTimeEvent(Stronghold.Format(282, randomPrisonerName.GetText()), Stronghold.NotificationType.Positive);
			return true;
		}
		stronghold.RemovePrisoner(randomPrisonerName);
		stronghold.LogTimeEvent(Stronghold.Format(23, randomPrisonerName.GetText()), Stronghold.NotificationType.Negative);
		return true;
	}

	public static bool HandleKidnapping(Player player, Stronghold stronghold)
	{
		StrongholdVisitor randomExistingVisitor = stronghold.GetRandomExistingVisitor(StrongholdVisitor.Type.PrestigiousVisitor, onlyKidnappable: true);
		if (randomExistingVisitor == null)
		{
			return false;
		}
		stronghold.RemoveVisitor(randomExistingVisitor);
		stronghold.Prestige += randomExistingVisitor.KidnapPrestigeAdjustment;
		stronghold.AddEvent(Type.Kidnapped, randomExistingVisitor, randomExistingVisitor.KidnapDuration);
		stronghold.LogTimeEvent(StrongholdUtils.Format(CharacterStats.GetGender(randomExistingVisitor.VisitorPrefab), 24, randomExistingVisitor.Name), Stronghold.NotificationType.Negative);
		return true;
	}

	private void ProcessKidnapped(Player player, Stronghold stronghold)
	{
		StrongholdVisitor strongholdVisitor = (StrongholdVisitor)m_data;
		if (strongholdVisitor != null)
		{
			stronghold.Prestige -= strongholdVisitor.KidnapPrestigeAdjustment;
			if (m_companion != null)
			{
				stronghold.AddVisitor(strongholdVisitor);
				stronghold.LogTimeEvent(StrongholdUtils.Format(CharacterStats.GetGender(strongholdVisitor.VisitorPrefab), 26, strongholdVisitor.Name, m_companion.DisplayName), Stronghold.NotificationType.Positive);
			}
			else
			{
				stronghold.AddVisitorToDeadList(strongholdVisitor);
				stronghold.Prestige += strongholdVisitor.KilledPrestigeAdjustment;
				stronghold.LogTimeEvent(StrongholdUtils.Format(CharacterStats.GetGender(strongholdVisitor.VisitorPrefab), 25, strongholdVisitor.Name), Stronghold.NotificationType.Negative);
				stronghold.AddEvent(Type.VisitorKilled, strongholdVisitor, int.MaxValue);
			}
		}
	}

	public static bool HandleBadVisitor(Player player, Stronghold stronghold)
	{
		return stronghold.SafeAddVisitor(stronghold.GetRandomNewVisitor(StrongholdVisitor.Type.BadVisitor), thwarted: false, almostThwarted: false);
	}

	public static void DebugForceAttack(Player player, Stronghold stronghold, int attackIndex)
	{
		if (attackIndex < 0 || attackIndex >= Stronghold.Instance.Attacks.Length)
		{
			Console.AddMessage("ERROR: Stronghold has no attack '" + attackIndex + "'.", Color.red);
		}
		else
		{
			HandleAttackNotificationHelper(player, stronghold, Stronghold.Instance.Attacks[attackIndex]);
		}
	}

	public static bool HandleAttackNotification(Player player, Stronghold stronghold)
	{
		if (stronghold.HasEvent(Type.Attack))
		{
			return false;
		}
		StrongholdAttack attack = null;
		if (!stronghold.PickAttack(ref attack))
		{
			Stronghold.Instance.LogTimeEvent(Stronghold.Format(66), Stronghold.NotificationType.Negative);
			return true;
		}
		return HandleAttackNotificationHelper(player, stronghold, attack);
	}

	private static bool HandleAttackNotificationHelper(Player player, Stronghold stronghold, StrongholdAttack attack)
	{
		if (attack == null)
		{
			return false;
		}
		int num = OEIRandom.Range(stronghold.AttackEventMinDayCount, stronghold.AttackEventMaxDayCount);
		stronghold.AddEvent(Type.Attack, attack, num);
		stronghold.LogTimeEvent(Stronghold.Format(28, attack.Name.GetText(), new EternityTimeInterval((int)(stronghold.DaysToGTU(num) * (float)WorldTime.Instance.SecondsPerDay)).FormatNonZero(2)), Stronghold.NotificationType.Negative);
		if (!string.IsNullOrEmpty(attack.ScheduledGlobalVariableName))
		{
			GlobalVariables.Instance.SetVariable(attack.ScheduledGlobalVariableName, 1);
		}
		return true;
	}

	private bool HandleGuestHireling(Player player, Stronghold stronghold)
	{
		if (stronghold.GuestHirelingAvailable != null)
		{
			return false;
		}
		List<StrongholdGuestHireling> list = new List<StrongholdGuestHireling>();
		StrongholdGuestHireling[] guestHirelings = stronghold.GuestHirelings;
		foreach (StrongholdGuestHireling strongholdGuestHireling in guestHirelings)
		{
			if (!stronghold.HasHireling(strongholdGuestHireling) && stronghold.GetPrestige() >= strongholdGuestHireling.MinimumPrestige && (strongholdGuestHireling.CanHireGlobalVariableName == null || strongholdGuestHireling.CanHireGlobalVariableName.Length == 0 || GlobalVariables.Instance.GetVariable(strongholdGuestHireling.CanHireGlobalVariableName) > 0))
			{
				list.Add(strongholdGuestHireling);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		StrongholdGuestHireling strongholdGuestHireling2 = list[OEIRandom.Index(list.Count)];
		if (IsThwarted(stronghold))
		{
			stronghold.LogTimeEvent(StrongholdUtils.Format(39, strongholdGuestHireling2.Name), Stronghold.NotificationType.Negative);
			return true;
		}
		stronghold.GuestHirelingAvailable = strongholdGuestHireling2;
		stronghold.GuestHirelingTimeLeft = WorldTime.Instance.SecondsPerDay;
		stronghold.LogVisitorArrival(strongholdGuestHireling2.HirelingPrefab, StrongholdUtils.Format(CharacterStats.GetGender(strongholdGuestHireling2.HirelingPrefab), 29, strongholdGuestHireling2.Name), Stronghold.NotificationType.Positive, IsAlmostThwarted(stronghold));
		return true;
	}

	private bool HandlePrisonerRequest(Player player, Stronghold stronghold)
	{
		CharacterDatabaseString randomPrisonerName = stronghold.GetRandomPrisonerName();
		if (randomPrisonerName == null)
		{
			return false;
		}
		StrongholdVisitor randomNewVisitor = stronghold.GetRandomNewVisitor(StrongholdVisitor.Type.PrisonerRequest);
		if (randomNewVisitor == null)
		{
			return false;
		}
		if (IsThwarted(stronghold))
		{
			stronghold.LogTimeEvent(StrongholdUtils.Format(39, randomNewVisitor.Name), Stronghold.NotificationType.Negative);
			return true;
		}
		randomNewVisitor.AssociatedPrisoner = randomPrisonerName;
		stronghold.AddVisitor(randomNewVisitor);
		stronghold.LogVisitorArrival(randomNewVisitor.VisitorPrefab, randomNewVisitor.FormatArrivalString(), Stronghold.NotificationType.Positive, IsAlmostThwarted(stronghold));
		return true;
	}

	private void ProcessEscorting(Player player, Stronghold stronghold)
	{
		StrongholdVisitor strongholdVisitor = (StrongholdVisitor)m_data;
		if (strongholdVisitor == null)
		{
			return;
		}
		bool flag = EventDataInt >= 0 && EventDataInt < strongholdVisitor.SpecialEscorts.Length;
		if (m_companion != null || flag)
		{
			string fstring = ((EventDataInt < 0 || EventDataInt >= strongholdVisitor.SpecialEscorts.Length || !strongholdVisitor.SpecialEscorts[EventDataInt].CompleteMessage.IsValidString) ? StrongholdUtils.GetText(31, m_companion ? m_companion.Gender : Gender.Neuter) : strongholdVisitor.SpecialEscorts[EventDataInt].CompleteMessage.GetText(m_companion ? m_companion.Gender : Gender.Neuter));
			stronghold.LogTimeEvent(StringUtility.Format(fstring, m_companion ? m_companion.DisplayName : "*null*", strongholdVisitor.Name), Stronghold.NotificationType.Positive);
			return;
		}
		stronghold.AddVisitor(strongholdVisitor);
		if (m_abandonedCompanion != null)
		{
			stronghold.LogTimeEvent(StrongholdUtils.Format(m_abandonedCompanion.Gender, 32, m_abandonedCompanion.DisplayName, strongholdVisitor.Name), Stronghold.NotificationType.None);
		}
	}

	private bool IsAlmostThwarted(Stronghold stronghold)
	{
		if (m_randomValue2 - stronghold.GetSecurity() <= 33)
		{
			return true;
		}
		return false;
	}

	private bool IsThwarted(Stronghold stronghold)
	{
		int num = m_randomValue2 - stronghold.GetSecurity();
		if (num <= 33)
		{
			return false;
		}
		if (num <= 66)
		{
			return false;
		}
		return true;
	}

	private bool HandleSupplicant(Player player, Stronghold stronghold)
	{
		return stronghold.SafeAddVisitor(stronghold.GetRandomNewVisitor(StrongholdVisitor.Type.Supplicant), thwarted: false, almostThwarted: false);
	}

	private void ProcessSupplicantEffectsWearOff(Player player, Stronghold stronghold)
	{
		StrongholdVisitor strongholdVisitor = (StrongholdVisitor)m_data;
		if (strongholdVisitor != null)
		{
			stronghold.Prestige -= strongholdVisitor.SupplicantPrestigeAdjustment;
			Reputation.Axis axis = strongholdVisitor.SupplicantReputationAdjustment.axis;
			axis = ((axis != Reputation.Axis.Negative) ? Reputation.Axis.Negative : Reputation.Axis.Positive);
			ReputationManager.Instance.AddReputation(strongholdVisitor.SupplicantReputationAdjustment.factionName, axis, strongholdVisitor.SupplicantReputationAdjustment.strength);
			stronghold.LogTimeEvent(Stronghold.Format(34, strongholdVisitor.Name), Stronghold.NotificationType.Positive);
		}
	}

	private void ProcessAttack(Player player, Stronghold stronghold)
	{
		((StrongholdAttack)m_data)?.AutoResolve(stronghold);
	}

	private int TotalDefenseLevels(Stronghold stronghold)
	{
		return 0 + stronghold.UpgradeTotalDefense() + stronghold.HirelingTotalDefense() + stronghold.CompanionTotalDefense();
	}

	private bool HandlePrestigiousVisitor(Player player, Stronghold stronghold)
	{
		return stronghold.SafeAddVisitor(stronghold.GetRandomNewVisitor(StrongholdVisitor.Type.PrestigiousVisitor), IsThwarted(stronghold), IsAlmostThwarted(stronghold));
	}

	private bool HandleRareItemOffer(Player player, Stronghold stronghold)
	{
		return stronghold.SafeAddVisitor(stronghold.GetRandomNewVisitor(StrongholdVisitor.Type.RareItemMerchant), IsThwarted(stronghold), IsAlmostThwarted(stronghold));
	}
}
