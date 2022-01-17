using System;
using UnityEngine;

[Serializable]
public class StrongholdVisitor
{
	public enum Type
	{
		BadVisitor,
		PrisonerRequest,
		Supplicant,
		PrestigiousVisitor,
		RareItemMerchant,
		Count
	}

	public string Tag = string.Empty;

	public Type VisitorType;

	[Tooltip("If checked, this visitor will never appear at random")]
	public bool AppearOnlyByScript;

	[Tooltip("If set, the visitor won't appear unless this variable is non-zero.")]
	[GlobalVariableString]
	public string AppearanceConditionalVariable;

	[Tooltip("Message to post in the stronghold log when the visitor arrives.")]
	public DatabaseString ArrivalString = new DatabaseString(DatabaseString.StringTableType.Stronghold);

	public bool HasDilemma;

	[Tooltip("A script should set this global to 1 when the visitor's dilemma is resolved.")]
	public GlobalVariableString DilemmaResolutionGlobalVariable;

	[Tooltip("Special arrival string to use when the visitor arrives and their dilemma is not resolved.")]
	public DatabaseString DilemmaArrivalString = new DatabaseString(DatabaseString.StringTableType.Stronghold);

	public int PrestigeAdjustment;

	public int SecurityAdjustment;

	[Tooltip("Length of this visitor's visit, in days. 0 means infinite.")]
	public int VisitDuration;

	public int KidnapPrestigeAdjustment;

	[Tooltip("How long this visitor remains kidnapped, in days. Set to 0 to disable kidnapping.")]
	public int KidnapDuration;

	[Tooltip("Permanent prestige adjustment added when this visitor is killed while kidnapped(the player did not send a rescue party).")]
	public int KilledPrestigeAdjustment;

	[Tooltip("Permanent prestige adjustment added when this visitor is killed while at the stronghold (the player must have done it).")]
	public int OnDeathPrestigeAdjustment;

	[Tooltip("Used for ransoms, pay offs, offers, etc.")]
	public int MoneyValue;

	[Tooltip("Used as an offering to sell or for exchange for a prisoner.")]
	public Item VisitorItem;

	[Tooltip("Duration in days of the default escort used to get rid of this visitor.")]
	public int EscortDuration;

	[Tooltip("A list of special escorts designers can trigger on this visitor.")]
	public StrongholdEscortData[] SpecialEscorts;

	public int SupplicantPrestigeAdjustment;

	public Stronghold.ReputationBonus SupplicantReputationAdjustment;

	[Tooltip("How long this visitor's ignore debuff lasts, in days.")]
	public int SupplicantEffectsDuration;

	public CharacterStats VisitorPrefab;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the visitor shows up and 0 when he leaves.")]
	public string GlobalVariableName;

	public float TimeToLeave { get; set; }

	public CharacterDatabaseString AssociatedPrisoner { get; set; }

	public string Name
	{
		get
		{
			if (VisitorPrefab != null)
			{
				return CharacterStats.Name(VisitorPrefab);
			}
			return GUIUtils.GetStrongholdVisitorString(VisitorType, CharacterStats.GetGender(VisitorPrefab));
		}
	}

	public float GetAppearanceWeight(Stronghold stronghold)
	{
		if (AppearOnlyByScript)
		{
			return 0f;
		}
		if (!string.IsNullOrEmpty(AppearanceConditionalVariable) && GlobalVariables.Instance.GetVariable(AppearanceConditionalVariable) == 0)
		{
			return 0f;
		}
		if (HasUnresolvedDilemma())
		{
			return 1f;
		}
		return stronghold.DilemmaResolvedWeightMultiplier;
	}

	public void HandleLeaving(Stronghold stronghold)
	{
		if (VisitorType == Type.Supplicant)
		{
			stronghold.Prestige += SupplicantPrestigeAdjustment;
			ReputationManager.Instance.AddReputation(SupplicantReputationAdjustment.factionName, SupplicantReputationAdjustment.axis, SupplicantReputationAdjustment.strength);
			stronghold.AddEvent(StrongholdEvent.Type.SupplicantEffectsWearOff, this, SupplicantEffectsDuration);
			if (VisitorPrefab.Gender == Gender.Female)
			{
				stronghold.LogTimeEvent(Stronghold.Format(37, Name), Stronghold.NotificationType.Positive);
			}
			else
			{
				stronghold.LogTimeEvent(Stronghold.Format(36, Name), Stronghold.NotificationType.Positive);
			}
		}
		else
		{
			stronghold.LogTimeEvent(Stronghold.Format(38, Name), Stronghold.NotificationType.Positive);
		}
	}

	public bool CanBeThwarted()
	{
		if (VisitorType != Type.RareItemMerchant && VisitorType != Type.PrestigiousVisitor)
		{
			return VisitorType == Type.PrisonerRequest;
		}
		return true;
	}

	public bool IsAffectingStats()
	{
		if (GetPrestigeAdjustment() == 0)
		{
			return GetSecurityAdjustment() != 0;
		}
		return true;
	}

	public int GetPrestigeAdjustment()
	{
		if (HasUnresolvedDilemma())
		{
			return 0;
		}
		return PrestigeAdjustment;
	}

	public int GetSecurityAdjustment()
	{
		if (HasUnresolvedDilemma())
		{
			return 0;
		}
		return SecurityAdjustment;
	}

	public int GetStatAdjustment(Stronghold.StatType stat)
	{
		return stat switch
		{
			Stronghold.StatType.Prestige => GetPrestigeAdjustment(), 
			Stronghold.StatType.Security => GetSecurityAdjustment(), 
			_ => 0, 
		};
	}

	public bool HasUnresolvedDilemma()
	{
		if (HasDilemma)
		{
			return GlobalVariables.Instance.GetVariable(DilemmaResolutionGlobalVariable) <= 0;
		}
		return false;
	}

	public bool HasDilemmaArrivalString()
	{
		if (HasUnresolvedDilemma() && DilemmaArrivalString != null)
		{
			return DilemmaArrivalString.IsValidString;
		}
		return false;
	}

	public bool HasArrivalString()
	{
		return GetArrivalString()?.IsValidString ?? false;
	}

	private DatabaseString GetArrivalString()
	{
		if (HasDilemmaArrivalString())
		{
			return DilemmaArrivalString;
		}
		return ArrivalString;
	}

	public string FormatArrivalString()
	{
		switch (VisitorType)
		{
		case Type.BadVisitor:
			if (HasArrivalString())
			{
				return StringUtility.Format(GetArrivalString().GetText(CharacterStats.GetGender(VisitorPrefab)), Name);
			}
			return StrongholdUtils.Format(CharacterStats.GetGender(VisitorPrefab), 27, Name);
		case Type.Supplicant:
		{
			if (HasArrivalString())
			{
				string factionName = ReputationManager.Instance.GetFactionName(SupplicantReputationAdjustment.factionName);
				return StringUtility.Format(GetArrivalString().GetText(CharacterStats.GetGender(VisitorPrefab)), factionName, GUIUtils.Format(466, MoneyValue));
			}
			string factionName2 = ReputationManager.Instance.GetFactionName(SupplicantReputationAdjustment.factionName);
			return StrongholdUtils.Format(CharacterStats.GetGender(VisitorPrefab), 33, factionName2, GUIUtils.Format(466, MoneyValue));
		}
		case Type.PrestigiousVisitor:
			if (HasArrivalString())
			{
				return StringUtility.Format(GetArrivalString().GetText(CharacterStats.GetGender(VisitorPrefab)), Name);
			}
			return StrongholdUtils.Format(CharacterStats.GetGender(VisitorPrefab), 27, Name);
		case Type.RareItemMerchant:
			if (HasArrivalString())
			{
				return StringUtility.Format(GetArrivalString().GetText(CharacterStats.GetGender(VisitorPrefab)), Name, VisitorItem.Name, GUIUtils.Format(466, MoneyValue));
			}
			return Stronghold.Format(35, Name, VisitorItem.Name, GUIUtils.Format(466, MoneyValue));
		case Type.PrisonerRequest:
			if (HasArrivalString())
			{
				return StringUtility.Format(GetArrivalString().GetText(), Name, AssociatedPrisoner.GetText());
			}
			return Stronghold.Format(30, AssociatedPrisoner.GetText(), GUIUtils.Format(466, MoneyValue), VisitorItem ? VisitorItem.Name : "*ItemError*");
		default:
			return "";
		}
	}

	public Stronghold.NotificationType GetArrivalNotificationType()
	{
		return VisitorType switch
		{
			Type.BadVisitor => Stronghold.NotificationType.Negative, 
			Type.Supplicant => Stronghold.NotificationType.Negative, 
			Type.PrestigiousVisitor => Stronghold.NotificationType.Positive, 
			Type.RareItemMerchant => Stronghold.NotificationType.Positive, 
			_ => Stronghold.NotificationType.None, 
		};
	}
}
