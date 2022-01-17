using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class PrerequisiteData
{
	public PrerequisiteType Type;

	public string Tag;

	public float Value;

	public CharacterStats.Race RaceValue = CharacterStats.Race.Human;

	public CharacterStats.Class ClassValue;

	public CharacterStats.SkillType SkillValue = CharacterStats.SkillType.Count;

	public Affliction AfflictionPrefab;

	public bool IsConsumed;

	[Tooltip("If set, the ability will hide from the UI bar when this prereq fails.")]
	public bool HideIfFailed;

	public void CheckForErrors(string owner)
	{
		if ((Type == PrerequisiteType.StaminaPercentAtLeast || Type == PrerequisiteType.StaminaPercentBelow) && (Value < 0f || Value > 1f))
		{
			UIDebug.Instance.LogOnScreenWarning(string.Concat("Status effect from ", owner, " has prereq of type '", Type, "' but its Value '", Value, "' is out of range (0.0 - 1.0)."), UIDebug.Department.Design, 10f);
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is PrerequisiteData prerequisiteData)
		{
			if (prerequisiteData.Type == Type && (!PrerequisiteMetadata.UsesTagParam(Type) || prerequisiteData.Tag == Tag) && (!PrerequisiteMetadata.UsesValueParam(Type) || prerequisiteData.Value == Value) && (!PrerequisiteMetadata.UsesRaceValueParam(Type) || prerequisiteData.RaceValue == RaceValue) && (!PrerequisiteMetadata.UsesClassValueParam(Type) || prerequisiteData.ClassValue == ClassValue) && (!PrerequisiteMetadata.UsesSkillValueParam(Type) || prerequisiteData.SkillValue == SkillValue) && (!PrerequisiteMetadata.UsesAfflictionParam(Type) || prerequisiteData.AfflictionPrefab == AfflictionPrefab))
			{
				return prerequisiteData.IsConsumed == IsConsumed;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode();
	}

	public static string GetQualifierString(PrerequisiteData[] prereqs)
	{
		if (prereqs == null)
		{
			return GUIUtils.GetText(2056);
		}
		string text = GUIUtils.GetText(2056);
		for (int i = 0; i < prereqs.Length; i++)
		{
			string abilityPrereqBaseQualifierString = GUIUtils.GetAbilityPrereqBaseQualifierString(prereqs[i].Type);
			if (!string.IsNullOrEmpty(abilityPrereqBaseQualifierString))
			{
				text = abilityPrereqBaseQualifierString;
				continue;
			}
			switch (prereqs[i].Type)
			{
			case PrerequisiteType.IsRace:
				if (prereqs[i].RaceValue != 0)
				{
					text = GUIUtils.GetRaceString(prereqs[i].RaceValue, Gender.Neuter);
				}
				break;
			case PrerequisiteType.IsClass:
				if (prereqs[i].ClassValue != 0)
				{
					text = GUIUtils.GetPluralClassString(prereqs[i].ClassValue, Gender.Neuter);
					if (prereqs[i].ClassValue <= CharacterStats.Class.Chanter)
					{
						text = GUIUtils.Format(2186, text);
					}
				}
				break;
			case PrerequisiteType.IsSummonedCreature:
				text = GUIUtils.GetText(2254);
				break;
			case PrerequisiteType.IsDragonOrDrake:
				text = GUIUtils.GetText(2452);
				break;
			}
		}
		for (int j = 0; j < prereqs.Length; j++)
		{
			string abilityPrereqQualifierString = GUIUtils.GetAbilityPrereqQualifierString(prereqs[j].Type);
			if (!string.IsNullOrEmpty(abilityPrereqQualifierString))
			{
				switch (prereqs[j].Type)
				{
				case PrerequisiteType.LevelAtLeast:
					text = StringUtility.Format(abilityPrereqQualifierString, text, prereqs[j].Value);
					break;
				case PrerequisiteType.HasAffliction:
					text = StringUtility.Format(abilityPrereqQualifierString, prereqs[j].AfflictionPrefab ? prereqs[j].AfflictionPrefab.Name() : "*null*", text);
					break;
				case PrerequisiteType.StaminaAmountAtLeast:
				case PrerequisiteType.StaminaAmountBelow:
					text = StringUtility.Format(abilityPrereqQualifierString, text, prereqs[j].Value);
					break;
				case PrerequisiteType.StaminaPercentBelow:
				case PrerequisiteType.StaminaPercentAtLeast:
					text = StringUtility.Format(abilityPrereqQualifierString, text, GUIUtils.Format(1277, (prereqs[j].Value * 100f).ToString("#0")));
					break;
				default:
					text = StringUtility.Format(abilityPrereqQualifierString, text);
					break;
				}
			}
		}
		return text;
	}

	public string GetString()
	{
		string abilityPrereqString = GUIUtils.GetAbilityPrereqString(Type);
		switch (Type)
		{
		case PrerequisiteType.StatusEffectCount:
		case PrerequisiteType.StatusEffectCountFromOwner:
			if (Value == 1f)
			{
				return StringUtility.Format(abilityPrereqString, Value, KeywordData.GetNoun(Tag));
			}
			return StringUtility.Format(abilityPrereqString, Value, KeywordData.GetNounPlural(Tag));
		case PrerequisiteType.SummonsCountNotGreaterThan:
			return "";
		case PrerequisiteType.EquipmentUnlocked:
			return "";
		case PrerequisiteType.EnemiesInAttackRange:
		case PrerequisiteType.AlliesInFriendlyRadius:
		case PrerequisiteType.CasterPhraseCount:
		case PrerequisiteType.StaminaAmountAtLeast:
		case PrerequisiteType.StaminaAmountBelow:
		case PrerequisiteType.LevelAtLeast:
			return StringUtility.Format(abilityPrereqString, Value);
		case PrerequisiteType.StaminaPercentBelow:
		case PrerequisiteType.StaminaPercentAtLeast:
			return StringUtility.Format(abilityPrereqString, GUIUtils.Format(1277, Mathf.CeilToInt(Value * 100f)));
		case PrerequisiteType.CombatTimeAtLeast:
			return StringUtility.Format(abilityPrereqString, GUIUtils.Format(211, Value.ToString("#0.#")));
		case PrerequisiteType.IsRace:
			return StringUtility.Format(abilityPrereqString, GUIUtils.GetRaceString(RaceValue, Gender.Neuter));
		case PrerequisiteType.IsClass:
			return StringUtility.Format(abilityPrereqString, GUIUtils.GetClassString(ClassValue, Gender.Neuter));
		case PrerequisiteType.SkillAtLeast:
			return StringUtility.Format(abilityPrereqString, GUIUtils.GetSkillTypeString(SkillValue), Value.ToString("#0"));
		case PrerequisiteType.IsOnMap:
			return "";
		default:
			return abilityPrereqString;
		}
	}

	public static bool CheckPrerequisites(GameObject gameObject, GameObject target, PrerequisiteData[] prereqs, GameObject main_target)
	{
		PrerequisiteType failed_type;
		return CheckPrerequisites(gameObject, target, prereqs, main_target, out failed_type);
	}

	public static bool CheckPrerequisites(GameObject gameObject, GameObject target, PrerequisiteData[] prereqs, GameObject main_target, out PrerequisiteType failed_type)
	{
		failed_type = PrerequisiteType.StatusEffectCount;
		if (gameObject == null)
		{
			return false;
		}
		if (prereqs == null)
		{
			return true;
		}
		foreach (PrerequisiteData prerequisiteData in prereqs)
		{
			if (!prerequisiteData.Check(gameObject, target, main_target))
			{
				failed_type = prerequisiteData.Type;
				return false;
			}
		}
		return true;
	}

	public static bool CheckVisibilityPrerequisites(GameObject gameObject, GameObject target, PrerequisiteData[] prereqs, GameObject main_target)
	{
		if (gameObject == null)
		{
			return false;
		}
		if (prereqs == null)
		{
			return true;
		}
		foreach (PrerequisiteData prerequisiteData in prereqs)
		{
			if (prerequisiteData.HideIfFailed && !prerequisiteData.Check(gameObject, target, main_target))
			{
				return false;
			}
		}
		return true;
	}

	public bool Check(GameObject gameObject, GameObject target, GameObject main_target)
	{
		GameObject gameObject2 = GameUtilities.FindParentWithComponent<CharacterStats>(gameObject);
		if (gameObject2 == null)
		{
			gameObject2 = gameObject;
		}
		switch (Type)
		{
		case PrerequisiteType.AlwaysFalse:
			return false;
		case PrerequisiteType.StatusEffectCount:
		{
			CharacterStats component9 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component9)
			{
				return (float)component9.CountStatusEffects(Tag) >= Value;
			}
			return false;
		}
		case PrerequisiteType.StatusEffectCountFromOwner:
		{
			CharacterStats component11 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component11)
			{
				return (float)component11.CountStatusEffects(Tag, gameObject2) >= Value;
			}
			return false;
		}
		case PrerequisiteType.SummonsCountNotGreaterThan:
		{
			PartyMemberAI component30 = ComponentUtils.GetComponent<PartyMemberAI>(target);
			AIPackageController component31 = ComponentUtils.GetComponent<AIPackageController>(target);
			AIController aIController = null;
			if ((bool)component31)
			{
				aIController = component31;
			}
			else if ((bool)component30)
			{
				aIController = component30;
			}
			int num5 = 0;
			if (aIController != null)
			{
				for (int num6 = aIController.SummonedCreatureList.Count - 1; num6 >= 0; num6--)
				{
					GameObject gameObject5 = aIController.SummonedCreatureList[num6];
					if (gameObject5 != null)
					{
						AIController component32 = gameObject5.GetComponent<AIController>();
						if (component32 != null && component32.SummonType == AIController.AISummonType.Summoned)
						{
							num5++;
						}
					}
				}
			}
			return (float)num5 <= Value;
		}
		case PrerequisiteType.EnemiesInAttackRange:
		{
			float num2 = AttackRange(gameObject);
			if (num2 > 0f && (bool)main_target)
			{
				GameObject[] array2 = GameUtilities.CreaturesInRange(main_target.transform.position, num2, main_target, includeUnconscious: false);
				if (array2 == null || (float)array2.Length < Value)
				{
					return false;
				}
			}
			break;
		}
		case PrerequisiteType.AlliesInFriendlyRadius:
		{
			float num7 = FriendlyRadius(gameObject);
			if (num7 > 0f && (bool)main_target)
			{
				GameObject[] array5 = GameUtilities.FriendsInRange(main_target.transform.position, num7, main_target, includeUnconscious: false);
				if (array5 == null || (float)array5.Length < Value)
				{
					return false;
				}
			}
			break;
		}
		case PrerequisiteType.MainTargetOnly:
			return target == main_target;
		case PrerequisiteType.ExcludeMainTarget:
			return target != main_target;
		case PrerequisiteType.NoAlliesInFriendlyRadius:
		{
			float num = FriendlyRadius(gameObject);
			if (num > 0f && (bool)main_target)
			{
				GameObject[] array = GameUtilities.FriendsInRange(main_target.transform.position, num, main_target, includeUnconscious: false);
				if (array != null)
				{
					return array.Length == 0;
				}
				return true;
			}
			break;
		}
		case PrerequisiteType.ClosestAllyWithSameTarget:
		{
			float num3 = FriendlyRadius(gameObject);
			AIController component23 = ComponentUtils.GetComponent<AIController>(main_target);
			if (!(num3 > 0f) || !(component23 != null))
			{
				break;
			}
			GameObject gameObject3 = null;
			float num4 = 0f;
			GameObject[] array3 = GameUtilities.FriendsInRange(main_target.transform.position, num3, main_target, includeUnconscious: false);
			if (array3 != null)
			{
				GameObject[] array4 = array3;
				foreach (GameObject gameObject4 in array4)
				{
					if (gameObject4 == null || gameObject4 == main_target)
					{
						continue;
					}
					AIController component24 = gameObject4.GetComponent<AIController>();
					if (!(component24 == null) && !(component24.CurrentTarget != component23.CurrentTarget))
					{
						float sqrMagnitude = (gameObject4.transform.position - main_target.transform.position).sqrMagnitude;
						if (gameObject3 == null || sqrMagnitude < num4)
						{
							gameObject3 = gameObject4;
							num4 = sqrMagnitude;
						}
					}
				}
			}
			return gameObject3 == target;
		}
		case PrerequisiteType.CasterIsChanting:
		{
			CharacterStats component37 = ComponentUtils.GetComponent<CharacterStats>(target);
			ChanterTrait chanterTrait2 = (component37 ? component37.GetChanterTrait() : null);
			if ((bool)chanterTrait2)
			{
				return chanterTrait2.IsChanting();
			}
			return false;
		}
		case PrerequisiteType.CasterPhraseCount:
		{
			CharacterStats component22 = ComponentUtils.GetComponent<CharacterStats>(target);
			ChanterTrait chanterTrait = (component22 ? component22.GetChanterTrait() : null);
			if ((bool)chanterTrait)
			{
				return (float)chanterTrait.PhraseCount >= Value;
			}
			return false;
		}
		case PrerequisiteType.Friendly:
		{
			Faction component12 = ComponentUtils.GetComponent<Faction>(gameObject2);
			if ((bool)component12)
			{
				return component12.GetRelationship(target) == Faction.Relationship.Friendly;
			}
			return true;
		}
		case PrerequisiteType.Hostile:
		{
			Faction component33 = ComponentUtils.GetComponent<Faction>(gameObject2);
			if ((bool)component33 && component33.GetRelationship(target) != Faction.Relationship.Hostile)
			{
				component33 = ComponentUtils.GetComponent<Faction>(target);
				if ((bool)component33 && component33.GetRelationship(gameObject2) != Faction.Relationship.Hostile)
				{
					return false;
				}
			}
			break;
		}
		case PrerequisiteType.StaminaPercentBelow:
		{
			Health component29 = ComponentUtils.GetComponent<Health>(target);
			if ((bool)component29)
			{
				return component29.StaminaPercentage < Value;
			}
			return false;
		}
		case PrerequisiteType.UsingRangedWeapon:
		{
			Equipment component19 = ComponentUtils.GetComponent<Equipment>(target);
			if ((bool)component19)
			{
				return component19.PrimaryAttack is AttackRanged;
			}
			return false;
		}
		case PrerequisiteType.Vessel:
		{
			CharacterStats component16 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component16)
			{
				return component16.CharacterRace == CharacterStats.Race.Vessel;
			}
			return false;
		}
		case PrerequisiteType.UsingMeleeWeapon:
		{
			Equipment component5 = ComponentUtils.GetComponent<Equipment>(target);
			if ((bool)component5)
			{
				Weapon component6 = component5.PrimaryAttack.GetComponent<Weapon>();
				if ((bool)component6 && component6.WeaponType == WeaponSpecializationData.WeaponType.Unarmed)
				{
					return true;
				}
				return component5.PrimaryAttack is AttackMelee;
			}
			return false;
		}
		case PrerequisiteType.StaminaAmountAtLeast:
		{
			Health component36 = ComponentUtils.GetComponent<Health>(target);
			if ((bool)component36)
			{
				return component36.CurrentStamina >= Value;
			}
			return false;
		}
		case PrerequisiteType.StaminaAmountBelow:
		{
			Health component34 = ComponentUtils.GetComponent<Health>(target);
			if ((bool)component34)
			{
				return component34.CurrentStamina < Value;
			}
			return false;
		}
		case PrerequisiteType.FocusBelowMax:
		{
			CharacterStats component25 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component25)
			{
				return component25.Focus < component25.MaxFocus;
			}
			return false;
		}
		case PrerequisiteType.IsRace:
		{
			CharacterStats component26 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component26)
			{
				return component26.CharacterRace == RaceValue;
			}
			return false;
		}
		case PrerequisiteType.HasGrimoire:
		{
			Equipment component17 = ComponentUtils.GetComponent<Equipment>(target);
			if ((bool)component17 && component17.CurrentItems != null)
			{
				return component17.CurrentItems.Grimoire;
			}
			return false;
		}
		case PrerequisiteType.StaminaPercentAtLeast:
		{
			Health component14 = ComponentUtils.GetComponent<Health>(target);
			if ((bool)component14)
			{
				return component14.StaminaPercentage >= Value;
			}
			return false;
		}
		case PrerequisiteType.EquipmentUnlocked:
		{
			CharacterStats component8 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component8)
			{
				return !component8.IsEquipmentLocked;
			}
			return false;
		}
		case PrerequisiteType.IsDragonOrDrake:
		{
			CharacterStats component4 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component4)
			{
				return CharacterStats.ClassIsDragonOrDrake(component4.CharacterClass);
			}
			return false;
		}
		case PrerequisiteType.CombatTimeAtLeast:
			return GameState.InCombatDuration >= Value;
		case PrerequisiteType.IsClass:
		{
			CharacterStats component2 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component2)
			{
				return component2.CharacterClass == ClassValue;
			}
			return false;
		}
		case PrerequisiteType.LevelAtLeast:
		{
			CharacterStats component35 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component35)
			{
				return (float)component35.ScaledLevel >= Value;
			}
			return false;
		}
		case PrerequisiteType.LevelLowerThanUser:
		{
			CharacterStats component27 = ComponentUtils.GetComponent<CharacterStats>(target);
			CharacterStats component28 = ComponentUtils.GetComponent<CharacterStats>(gameObject2);
			if ((bool)component27)
			{
				return component27.ScaledLevel < (component28 ? component28.ScaledLevel : 0);
			}
			return false;
		}
		case PrerequisiteType.LevelHigherOrEqualToUser:
		{
			CharacterStats component20 = ComponentUtils.GetComponent<CharacterStats>(target);
			CharacterStats component21 = ComponentUtils.GetComponent<CharacterStats>(gameObject2);
			if ((bool)component20)
			{
				return component20.ScaledLevel >= (component21 ? component21.ScaledLevel : 0);
			}
			return false;
		}
		case PrerequisiteType.HasKeyword:
		{
			CharacterStats component18 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component18)
			{
				return component18.HasKeyword(Tag);
			}
			return false;
		}
		case PrerequisiteType.IsOnMap:
			return string.Compare(GameState.Instance.CurrentMap.SceneName, Tag, ignoreCase: true, CultureInfo.InvariantCulture) == 0;
		case PrerequisiteType.IsSummonedCreature:
		{
			AIController component15 = ComponentUtils.GetComponent<AIController>(target);
			if ((bool)component15)
			{
				return component15.SummonType == AIController.AISummonType.Summoned;
			}
			return false;
		}
		case PrerequisiteType.SkillAtLeast:
		{
			CharacterStats component13 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component13)
			{
				return (float)component13.CalculateSkill(SkillValue) >= Value;
			}
			return false;
		}
		case PrerequisiteType.HasAffliction:
		{
			CharacterStats component10 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component10)
			{
				return component10.HasStatusEffectFromAffliction(AfflictionPrefab);
			}
			return false;
		}
		case PrerequisiteType.IsBackerNpc:
			return ComponentUtils.GetComponent<BackerContent>(target);
		case PrerequisiteType.IsKithMale:
		{
			CharacterStats component7 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component7 && component7.Gender == Gender.Male)
			{
				return CharacterStats.IsKithRace(component7.CharacterRace);
			}
			return false;
		}
		case PrerequisiteType.IsKithFemale:
		{
			CharacterStats component3 = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component3 && component3.Gender == Gender.Female)
			{
				return CharacterStats.IsKithRace(component3.CharacterRace);
			}
			return false;
		}
		case PrerequisiteType.HasInflictedMarker:
		{
			CharacterStats component = ComponentUtils.GetComponent<CharacterStats>(target);
			if ((bool)component)
			{
				return component.GetInflictedGenericMarker(Tag);
			}
			return false;
		}
		default:
			Debug.LogError(string.Concat("The prereq type '", Type, "' is not supported."));
			break;
		}
		return true;
	}

	private static float AttackRange(GameObject gameObject)
	{
		float result = 0f;
		AttackBase component = gameObject.GetComponent<AttackBase>();
		if ((bool)component)
		{
			result = component.TotalAttackDistance;
		}
		return result;
	}

	private static float FriendlyRadius(GameObject gameObject)
	{
		float result = 0f;
		GenericAbility component = gameObject.GetComponent<GenericAbility>();
		if ((bool)component)
		{
			result = component.AdjustedFriendlyRadius;
		}
		return result;
	}
}
