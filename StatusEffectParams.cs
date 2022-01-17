using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
[DebuggerDisplay("{GetDebuggerString()} (StatusEffectParams)")]
public class StatusEffectParams : ISerializationCallbackReceiver
{
	public enum IntervalRateType
	{
		None,
		Damage,
		Hazard,
		Footstep
	}

	public enum ParamType
	{
		Tag,
		AffectsStat,
		DmgType,
		Value,
		ExtraValue,
		TrapPrefab,
		EquippablePrefab,
		ConsumablePrefab,
		AttackPrefab,
		AfflictionPrefab,
		RaceType,
		Keyword,
		Duration,
		AbilityPrefab,
		Attribute,
		DefenseType,
		ClassType,
		Count
	}

	private class ListToStringGroupKey
	{
		public AttackBase.TargetType TargetType = AttackBase.TargetType.None;

		public StatusEffect.ModifiedStat SpecialModifiedStat = StatusEffect.ModifiedStat.NoEffect;

		public override bool Equals(object obj)
		{
			if (obj is ListToStringGroupKey)
			{
				ListToStringGroupKey listToStringGroupKey = (ListToStringGroupKey)obj;
				if (listToStringGroupKey.TargetType == TargetType)
				{
					return listToStringGroupKey.SpecialModifiedStat == SpecialModifiedStat;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return TargetType.GetHashCode() + 13 * SpecialModifiedStat.GetHashCode();
		}
	}

	public string Tag;

	public PrerequisiteData[] ApplicationPrerequisites;

	[Tooltip("If checked, this status effect will persist through save/load. Otherwise, this effect will not serialize into the save game.")]
	[HideInInspector]
	public bool Persistent = true;

	[Tooltip("Determines if an attack, generic ability, or affliction is hostile. Hostile means red tooltip.")]
	public bool IsHostile = true;

	[Tooltip("Normally, hostile effects are cleared on death. This overrides that behavior.")]
	public bool KeepOnDeath;

	public StatusEffect.ApplyType Apply;

	public StatusEffect.ModifiedStat AffectsStat = StatusEffect.ModifiedStat.NoEffect;

	public DamagePacket.DamageType DmgType;

	[Tooltip("Used as a container for value getting and setting.")]
	public float Value;

	[Tooltip("Used as a container for extra values in some status effects.")]
	public float ExtraValue;

	public Trap TrapPrefab;

	public Equippable EquippablePrefab;

	public Consumable ConsumablePrefab;

	public AttackBase AttackPrefab;

	public GenericAbility AbilityPrefab;

	public Affliction AfflictionPrefab;

	public CharacterStats.Race RaceType;

	public CharacterStats.Class ClassType;

	public CharacterStats.AttributeScoreType AttributeType = CharacterStats.AttributeScoreType.Count;

	public CharacterStats.DefenseType DefenseType = CharacterStats.DefenseType.None;

	public string Keyword;

	[Tooltip("If DurationOverride is set on the Generic Ability, this value is ignored.")]
	public float Duration;

	[Tooltip("Owners and targets of attacks remove OneHitUse status effects after attacks if the effect is from the Owner.")]
	public bool OneHitUse;

	public bool LastsUntilCombatEnds;

	public bool LastsUntilRest;

	public int MaxRestCycles = 1;

	[Tooltip("How often an aura checks the AOE for valid targets.")]
	public IntervalRateType IntervalRate;

	[Tooltip("After this effect is applied, don't show it in the character sheet/party bar.")]
	public bool HideFromUi;

	public bool ChecksReligion;

	public GameObject OnStartVisualEffect;

	public GameObject OnAppliedVisualEffect;

	public GameObject OnStopVisualEffect;

	public GameObject OnAuraVisualEffect;

	public GameObject OnTriggerVisualEffect;

	public AttackBase.EffectAttachType VisualEffectAttach = AttackBase.EffectAttachType.Root;

	[ExcludeFromSerialization]
	public Texture2D Icon;

	[HideInInspector]
	public bool DontHideFromLog;

	[Tooltip("Do not stop this effect when the ability it is from deactivates. Let it run its course, or be removed by something else.")]
	public bool IgnoreAbilityDeactivation;

	[ExcludeFromSerialization]
	[Tooltip("Delay on when to destroy applied visual effects (default 5 seconds)")]
	public float DestroyVFXDelay = 5f;

	public StatusEffectTrigger TriggerAdjustment = new StatusEffectTrigger();

	[Tooltip("If set, Level Scaling will not be used for the player, only for other characters.")]
	public bool DoNotScalePlayer;

	public StatusEffectLevelScaling LevelScaling = new StatusEffectLevelScaling();

	private Guid m_trapPrefabSerialized = Guid.Empty;

	private Guid m_equippablePrefabSerialized = Guid.Empty;

	private Guid m_attackPrefabSerialized = Guid.Empty;

	private Guid m_onDamageCallbackAbilitySerialized = Guid.Empty;

	[HideInInspector]
	public bool m_deserializeInitialized;

	[Obsolete("This property is only for save-game backwards compatibility.")]
	public float UnadjustedDurationAdd { get; set; }

	public GenericAbility OnDamagedCallbackAbility { get; set; }

	public bool IsOverTime
	{
		get
		{
			if (IntervalRate != 0)
			{
				if (Apply != 0)
				{
					return Apply == StatusEffect.ApplyType.ApplyOverTime;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsInstantApplication
	{
		get
		{
			if (Duration > 0f)
			{
				return Duration < 1f;
			}
			return false;
		}
	}

	public int LevelScalingBaseLevel
	{
		get
		{
			return LevelScaling.BaseLevel;
		}
		set
		{
			LevelScaling.BaseLevel = value;
		}
	}

	public int LevelScalingLevelIncrement
	{
		get
		{
			return LevelScaling.LevelIncrement;
		}
		set
		{
			LevelScaling.LevelIncrement = value;
		}
	}

	public int LevelScalingMaxLevel
	{
		get
		{
			return LevelScaling.MaxLevel;
		}
		set
		{
			LevelScaling.MaxLevel = value;
		}
	}

	public float LevelScalingValueAdjustment
	{
		get
		{
			return LevelScaling.ValueAdjustment;
		}
		set
		{
			LevelScaling.ValueAdjustment = value;
		}
	}

	public float LevelScalingExtraValueAdjustment
	{
		get
		{
			return LevelScaling.ExtraValueAdjustment;
		}
		set
		{
			LevelScaling.ExtraValueAdjustment = value;
		}
	}

	public float LevelScalingDurationAdjustment
	{
		get
		{
			return LevelScaling.DurationAdjustment;
		}
		set
		{
			LevelScaling.DurationAdjustment = value;
		}
	}

	public Guid TrapPrefabSerialized
	{
		get
		{
			if ((bool)TrapPrefab)
			{
				InstanceID component = TrapPrefab.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_trapPrefabSerialized = component.Guid;
				}
			}
			return m_trapPrefabSerialized;
		}
		set
		{
			m_trapPrefabSerialized = value;
		}
	}

	public Trap TrapSerialized2
	{
		get
		{
			return TrapPrefab;
		}
		set
		{
			TrapPrefab = value;
		}
	}

	public Guid EquippablePrefabSerialized
	{
		get
		{
			if ((bool)EquippablePrefab)
			{
				InstanceID component = EquippablePrefab.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_equippablePrefabSerialized = component.Guid;
				}
			}
			return m_equippablePrefabSerialized;
		}
		set
		{
			m_equippablePrefabSerialized = value;
		}
	}

	public Equippable EquippableSerialized2
	{
		get
		{
			return EquippablePrefab;
		}
		set
		{
			EquippablePrefab = value;
		}
	}

	public Guid AttackPrefabSerialized
	{
		get
		{
			if ((bool)AttackPrefab)
			{
				InstanceID component = AttackPrefab.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_attackPrefabSerialized = component.Guid;
				}
			}
			return m_attackPrefabSerialized;
		}
		set
		{
			m_attackPrefabSerialized = value;
		}
	}

	public AttackBase AttackPrefabSerialized2
	{
		get
		{
			return AttackPrefab;
		}
		set
		{
			AttackPrefab = value;
		}
	}

	public Affliction AfflictionPrefabSerialized
	{
		get
		{
			return AfflictionPrefab;
		}
		set
		{
			AfflictionPrefab = value;
		}
	}

	public Guid OnDamageCallbackAbilitySerialized
	{
		get
		{
			if ((bool)OnDamagedCallbackAbility)
			{
				InstanceID component = OnDamagedCallbackAbility.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_onDamageCallbackAbilitySerialized = component.Guid;
				}
			}
			return m_onDamageCallbackAbilitySerialized;
		}
		set
		{
			m_onDamageCallbackAbilitySerialized = value;
		}
	}

	public string Description { get; set; }

	public bool IsCleanedUp { get; private set; }

	public float MergedValue { get; private set; }

	public string GetDebuggerString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(AffectsStat.ToString());
		stringBuilder.Append("(");
		if (!string.IsNullOrEmpty(Tag))
		{
			stringBuilder.Append("Tag:" + Tag + "|");
		}
		if (StatusEffect.UsesDamageTypeParam(AffectsStat))
		{
			stringBuilder.Append(DmgType.ToString() + "|");
		}
		if (StatusEffect.UsesTrapParam(AffectsStat))
		{
			stringBuilder.Append("Trp:" + GameUtilities.GetName(TrapPrefab) + "|");
		}
		if (StatusEffect.UsesEquippableParam(AffectsStat))
		{
			stringBuilder.Append("Eq:" + GameUtilities.GetName(EquippablePrefab) + "|");
		}
		if (StatusEffect.UsesConsumableParam(AffectsStat))
		{
			stringBuilder.Append("Cnsm:" + GameUtilities.GetName(ConsumablePrefab) + "|");
		}
		if (StatusEffect.UsesAttackParam(AffectsStat))
		{
			stringBuilder.Append("Atk:" + GameUtilities.GetName(AttackPrefab) + "|");
		}
		if (StatusEffect.UsesAfflictionParam(AffectsStat))
		{
			stringBuilder.Append("Aff:" + GameUtilities.GetName(AfflictionPrefab) + "|");
		}
		if (StatusEffect.UsesRaceParam(AffectsStat))
		{
			stringBuilder.Append("Rce:" + RaceType.ToString() + "|");
		}
		if (StatusEffect.UsesClassParam(AffectsStat))
		{
			stringBuilder.Append("Cls:" + ClassType.ToString() + "|");
		}
		if (StatusEffect.UsesAttributeParam(AffectsStat))
		{
			stringBuilder.Append(string.Concat("Attr:", AttributeType, "|"));
		}
		if (StatusEffect.UsesDefenseTypeParam(AffectsStat))
		{
			stringBuilder.Append(string.Concat("Def:", DefenseType, "|"));
		}
		if (StatusEffect.UsesKeywordParam(AffectsStat))
		{
			stringBuilder.Append("Key:" + Keyword + "|");
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	public float GetValue(GameObject owner)
	{
		return GetValue(ComponentUtils.GetComponent<CharacterStats>(owner));
	}

	public float GetValue(CharacterStats stats)
	{
		if ((bool)stats)
		{
			float num = Value;
			if (AffectsStat == StatusEffect.ModifiedStat.StaminaByAthletics)
			{
				num += stats.SecondWindAthleticsBonus;
			}
			if (UseLevelScaling(stats))
			{
				num = LevelScaling.AdjustValue(num, stats.ScaledLevel);
			}
			return num;
		}
		return Value;
	}

	public float GetExtraValue(GameObject owner)
	{
		return GetExtraValue(ComponentUtils.GetComponent<CharacterStats>(owner));
	}

	public float GetExtraValue(CharacterStats stats)
	{
		if ((bool)stats && UseLevelScaling(stats))
		{
			return LevelScaling.AdjustExtraValue(ExtraValue, stats.ScaledLevel);
		}
		return ExtraValue;
	}

	public float GetDuration(CharacterStats stats)
	{
		return AdjustDuration(stats, Duration);
	}

	public float AdjustDuration(CharacterStats stats, float duration)
	{
		if ((bool)stats && UseLevelScaling(stats))
		{
			return LevelScaling.AdjustDuration(duration, stats.ScaledLevel);
		}
		return duration;
	}

	private bool UseLevelScaling(CharacterStats character)
	{
		if ((bool)character)
		{
			if (DoNotScalePlayer)
			{
				return !character.GetComponent<Player>();
			}
			return true;
		}
		return false;
	}

	public bool EqualsExceptValues(StatusEffectParams other)
	{
		return EqualsExceptParameter(other, ParamType.Value);
	}

	public bool EqualsExceptParameter(StatusEffectParams other, ParamType except)
	{
		if ((except == ParamType.Tag || string.Equals(Tag, other.Tag)) && IsHostile == other.IsHostile && Apply == other.Apply && (except == ParamType.AffectsStat || AffectsStat == other.AffectsStat) && (except == ParamType.DmgType || !StatusEffect.UsesDamageTypeParam(AffectsStat) || DmgType == other.DmgType) && (except == ParamType.Duration || Duration == other.Duration) && LastsUntilCombatEnds == other.LastsUntilCombatEnds && LastsUntilRest == other.LastsUntilRest && MaxRestCycles == other.MaxRestCycles && IntervalRate == other.IntervalRate && OneHitUse == other.OneHitUse && ChecksReligion == other.ChecksReligion && (except == ParamType.TrapPrefab || !StatusEffect.UsesTrapParam(AffectsStat) || TrapPrefab == other.TrapPrefab) && (except == ParamType.EquippablePrefab || !StatusEffect.UsesEquippableParam(AffectsStat) || EquippablePrefab == other.EquippablePrefab) && (except == ParamType.AttackPrefab || !StatusEffect.UsesAttackParam(AffectsStat) || AttackPrefab == other.AttackPrefab) && (except == ParamType.AbilityPrefab || !StatusEffect.UsesAbilityParam(AffectsStat) || AbilityPrefab == other.AbilityPrefab) && (except == ParamType.AfflictionPrefab || !StatusEffect.UsesAfflictionParam(AffectsStat) || AfflictionPrefab == other.AfflictionPrefab) && (except == ParamType.ConsumablePrefab || !StatusEffect.UsesConsumableParam(AffectsStat) || ConsumablePrefab == other.ConsumablePrefab) && (except == ParamType.RaceType || !StatusEffect.UsesRaceParam(AffectsStat) || RaceType == other.RaceType) && (except == ParamType.ClassType || !StatusEffect.UsesClassParam(AffectsStat) || ClassType == other.ClassType) && (except == ParamType.Keyword || !StatusEffect.UsesKeywordParam(AffectsStat) || Keyword == other.Keyword) && TriggerAdjustment.Equals(other.TriggerAdjustment) && LevelScaling.Empty && other.LevelScaling.Empty && DoNotScalePlayer == other.DoNotScalePlayer && (except == ParamType.Value || !StatusEffect.UsesValueParam(AffectsStat) || Value == other.Value))
		{
			if (except != ParamType.ExtraValue && StatusEffect.UsesExtraValueParam(AffectsStat))
			{
				return ExtraValue == other.ExtraValue;
			}
			return true;
		}
		return false;
	}

	public int CalculateStackingKey()
	{
		string text = AffectsStat.ToString();
		if (StatusEffect.UsesDamageTypeParam(AffectsStat))
		{
			text += DmgType;
		}
		if (AffectsStat == StatusEffect.ModifiedStat.SpellCastBonus)
		{
			text += ExtraValue;
		}
		if (StatusEffect.UsesTrapParam(AffectsStat))
		{
			text += TrapPrefab.ToString();
		}
		if (StatusEffect.UsesEquippableParam(AffectsStat))
		{
			text += EquippablePrefab.ToString();
		}
		if (StatusEffect.UsesConsumableParam(AffectsStat))
		{
			text += ConsumablePrefab.ToString();
		}
		if (StatusEffect.UsesAfflictionParam(AffectsStat))
		{
			text += AfflictionPrefab.Tag;
		}
		if (StatusEffect.UsesRaceParam(AffectsStat))
		{
			text += RaceType;
		}
		if (StatusEffect.UsesAttributeParam(AffectsStat))
		{
			text += AttributeType;
		}
		if (StatusEffect.UsesDefenseTypeParam(AffectsStat))
		{
			text += DefenseType;
		}
		if (StatusEffect.UsesKeywordParam(AffectsStat))
		{
			text += Keyword;
		}
		if (StatusEffect.UsesClassParam(AffectsStat))
		{
			text += ClassType;
		}
		return text.GetHashCode();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (!m_deserializeInitialized)
		{
			Persistent = true;
			m_deserializeInitialized = true;
		}
	}

	public bool CanApply(GameObject owner, GameObject target, GameObject main_target)
	{
		return PrerequisiteData.CheckPrerequisites(owner, target, ApplicationPrerequisites, main_target);
	}

	public bool HasPrereqWhere(Func<PrerequisiteData, bool> where)
	{
		if (ApplicationPrerequisites == null)
		{
			return false;
		}
		return ApplicationPrerequisites.Where(where).Any();
	}

	public void OverrideCleanedValue(float value)
	{
		IsCleanedUp = true;
		MergedValue = value;
	}

	public static void CleanUp<T>(IList<T> effects)
	{
		for (int i = 0; i < effects.Count; i++)
		{
			StatusEffect statusEffect = null;
			StatusEffectParams statusEffectParams;
			if (effects[i] is StatusEffect)
			{
				statusEffect = effects[i] as StatusEffect;
				statusEffectParams = statusEffect.Params;
			}
			else
			{
				if (!(effects[i] is StatusEffectParams))
				{
					continue;
				}
				statusEffectParams = effects[i] as StatusEffectParams;
			}
			statusEffectParams.MergedValue = statusEffectParams.GetValue(statusEffect?.Owner);
			if (statusEffectParams.AffectsStat == StatusEffect.ModifiedStat.GenericMarker)
			{
				continue;
			}
			for (int num = effects.Count - 1; num > i; num--)
			{
				StatusEffect statusEffect2 = null;
				StatusEffectParams statusEffectParams2;
				if (effects[num] is StatusEffect)
				{
					statusEffect2 = effects[num] as StatusEffect;
					statusEffectParams2 = statusEffect2.Params;
				}
				else
				{
					if (!(effects[num] is StatusEffectParams))
					{
						continue;
					}
					statusEffectParams2 = effects[num] as StatusEffectParams;
				}
				if (statusEffectParams2.ChecksReligion || !statusEffectParams2.LevelScaling.Empty)
				{
					continue;
				}
				GameObject owner = statusEffect2?.Owner;
				if (statusEffect2 != null && statusEffect != null)
				{
					if (!statusEffect2.IsSuppressed && statusEffect2.Applied && !statusEffect.IsSuppressed && statusEffect.Applied && statusEffect2.EqualsExceptValues(statusEffect))
					{
						statusEffectParams.MergedValue += statusEffectParams2.GetValue(owner);
						statusEffectParams.IsCleanedUp = true;
						effects.RemoveAt(num);
					}
				}
				else if (statusEffectParams.EqualsExceptValues(statusEffectParams2))
				{
					statusEffectParams.MergedValue += statusEffectParams2.GetValue(owner);
					statusEffectParams.IsCleanedUp = true;
					effects.RemoveAt(num);
				}
			}
		}
	}

	public static string ListToString<T>(IEnumerable<T> effects)
	{
		return ListToString(effects, null, null, null, null, StatusEffectFormatMode.Default, AttackBase.TargetType.All);
	}

	public static string ListToString<T>(IEnumerable<T> effects, CharacterStats source)
	{
		return ListToString(effects, source, null, null, null, StatusEffectFormatMode.Default, AttackBase.TargetType.All);
	}

	public static string ListToString<T>(IEnumerable<T> effect, CharacterStats source, Phrase originPhrase, StatusEffectFormatMode mode)
	{
		return ListToString(effect, source, null, null, originPhrase, mode, AttackBase.TargetType.All);
	}

	public static string ListToString<T>(IEnumerable<T> effects, CharacterStats source, GenericAbility ability)
	{
		return ListToString(effects, source, ability, null, null, StatusEffectFormatMode.Default, AttackBase.TargetType.All);
	}

	public static string ListToString<T>(IEnumerable<T> effects, CharacterStats source, GenericAbility ability, AfflictionParams affliction)
	{
		return ListToString(effects, source, ability, affliction, null, StatusEffectFormatMode.Default, AttackBase.TargetType.All);
	}

	public static string ListToString<T>(IEnumerable<T> effects, CharacterStats source, GenericAbility ability, AttackBase.TargetType targetType)
	{
		return ListToString(effects, source, ability, null, null, StatusEffectFormatMode.Default, targetType);
	}

	public static string ListToString<T>(IEnumerable<T> effects, CharacterStats source, GenericAbility ability, AfflictionParams affliction, Phrase phraseOrigin, StatusEffectFormatMode mode, AttackBase.TargetType targetType)
	{
		if (typeof(T) != typeof(StatusEffectParams) && typeof(T) != typeof(StatusEffect))
		{
			throw new ArgumentException(string.Concat("Collection of type '", typeof(T), "' is not supported. Supported types are StatusEffect, StatusEffectParams."));
		}
		if (!effects.Any())
		{
			return string.Empty;
		}
		bool flag = mode == StatusEffectFormatMode.CharacterSheet || mode == StatusEffectFormatMode.PartyBar;
		bool flag2 = mode == StatusEffectFormatMode.CharacterSheet || mode == StatusEffectFormatMode.PartyBar;
		string text = GUIUtils.Comma();
		bool flag3 = typeof(T) == typeof(StatusEffectParams);
		float num = -1f;
		float num2 = -1f;
		bool flag4 = true;
		bool flag5 = true;
		foreach (T effect in effects)
		{
			object obj = effect;
			if (obj == null)
			{
				continue;
			}
			StatusEffect statusEffect = null;
			StatusEffectParams statusEffectParams;
			if (flag3)
			{
				statusEffectParams = (StatusEffectParams)obj;
			}
			else
			{
				statusEffect = (StatusEffect)obj;
				statusEffectParams = statusEffect.Params;
			}
			if (statusEffectParams.AffectsStat != StatusEffect.ModifiedStat.NoEffect)
			{
				float o_appliedDuration;
				float o_adjustedDuration;
				if (flag)
				{
					flag5 = flag5 && statusEffect.Params.IsInstantApplication;
					o_appliedDuration = statusEffect.TimeLeft;
					o_adjustedDuration = statusEffect.TimeLeft;
				}
				else
				{
					GetEffectValuesForUi(statusEffect, statusEffectParams, source, ability, affliction, mode, out o_appliedDuration, out o_adjustedDuration, out float o_appliedValue, out o_appliedValue);
				}
				if (statusEffect == null || (!statusEffect.IsSuppressed && !statusEffect.IsSuspended))
				{
					flag4 = false;
				}
				if ((statusEffect != null && statusEffect.IsOverTime) || statusEffectParams.IsOverTime)
				{
					num = -1f;
					break;
				}
				if (num < 0f)
				{
					num = o_appliedDuration;
					num2 = o_adjustedDuration;
				}
				else if (o_appliedDuration != num)
				{
					num = -1f;
					break;
				}
			}
		}
		if (flag)
		{
			if (flag5)
			{
				num = 0f;
			}
		}
		else if (num < 1f && num > 0f)
		{
			num = 0f;
		}
		if (flag4)
		{
			num = 0f;
		}
		if (num2 < 0f)
		{
			num2 = num;
		}
		List<T> list = new List<T>();
		list.AddRange(effects);
		StringBuilder stringBuilder = new StringBuilder();
		while (list.Count > 0)
		{
			object obj2 = list[0];
			list.RemoveAt(0);
			if (obj2 != null)
			{
				bool flag6 = false;
				string text2;
				if (flag3)
				{
					text2 = ((StatusEffectParams)obj2).GetString(null, source, ability, affliction, mode, num < 0f, targetType, list);
				}
				else
				{
					StatusEffect statusEffect2 = (StatusEffect)obj2;
					text2 = statusEffect2.Params.GetString(statusEffect2, source, ability, affliction, mode, num < 0f, targetType, list);
					flag6 = statusEffect2.IsSuppressed;
				}
				if (flag2 && flag6)
				{
					text2 = "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.DarkDisabled) + "]" + text2 + "[-]";
				}
				if (!string.IsNullOrEmpty(text2))
				{
					stringBuilder.Append(text2);
					stringBuilder.Append(text);
				}
			}
		}
		if (stringBuilder.Length >= text.Length)
		{
			stringBuilder = stringBuilder.Remove(stringBuilder.Length - text.Length);
		}
		if (stringBuilder.Length > 0)
		{
			if (num > 0f)
			{
				if (mode == StatusEffectFormatMode.PartyBar)
				{
					return stringBuilder.ToString() + GUIUtils.Format(1731, FormatDuration(num, num, mode));
				}
				return GUIUtils.Format(1665, stringBuilder.ToString(), FormatDuration(num, num2, mode));
			}
			return stringBuilder.ToString();
		}
		return "";
	}

	public static void ListToStringEffects<T>(IEnumerable<T> effects, CharacterStats source, GenericAbility ability, AttackBase attack, AfflictionParams affliction, Phrase phraseOrigin, StatusEffectFormatMode mode, AttackBase.FormattableTarget targetFormat, string targetQualifiers, AttackBase.TargetType externalTarget, StringEffects stringEffects)
	{
		if (!effects.Any())
		{
			return;
		}
		AttackBase.TargetType mainTarget = AttackBase.TargetType.Self;
		if ((bool)attack)
		{
			if (attack.ValidPrimaryTargets == AttackBase.TargetType.OwnAnimalCompanion)
			{
				mainTarget = AttackBase.TargetType.OwnAnimalCompanion;
			}
			else if (!attack.ApplyToSelfOnly)
			{
				mainTarget = AttackBase.TargetType.All;
			}
		}
		foreach (IGrouping<ListToStringGroupKey, T> item in effects.GroupBy(delegate(T t)
		{
			StatusEffectParams statusEffectParams2 = ((!(t is StatusEffect)) ? (t as StatusEffectParams) : (t as StatusEffect).Params);
			if (statusEffectParams2 == null)
			{
				return new ListToStringGroupKey();
			}
			AttackBase.TargetType targetType = externalTarget;
			for (int i = 0; i < statusEffectParams2.ApplicationPrerequisites.Length; i++)
			{
				targetType = TargetTypeUtils.LayerTargetTypes(targetType, TargetTypeUtils.PrerequisiteToTarget(statusEffectParams2.ApplicationPrerequisites[i].Type, mainTarget));
			}
			ListToStringGroupKey listToStringGroupKey = new ListToStringGroupKey
			{
				TargetType = targetType
			};
			if (StatusEffect.ModifiedStatHasSpecialTargetString(statusEffectParams2.AffectsStat))
			{
				listToStringGroupKey.SpecialModifiedStat = statusEffectParams2.AffectsStat;
			}
			return listToStringGroupKey;
		}))
		{
			bool hostile = false;
			foreach (T item2 in item)
			{
				StatusEffectParams statusEffectParams = ((!(item2 is StatusEffect)) ? (item2 as StatusEffectParams) : (item2 as StatusEffect).Params);
				if (statusEffectParams != null && statusEffectParams.IsHostile)
				{
					hostile = true;
					break;
				}
				AttackBase.FormattableTarget formattableTarget = (attack ? attack.GetMainTargetString(ability) : ((!ability) ? targetFormat : ability.GetSelfTarget()));
				if (StatusEffect.ModifiedStatHasSpecialTargetString(statusEffectParams.AffectsStat))
				{
					GetEffectValuesForUi(item2 as StatusEffect, statusEffectParams, source, ability, affliction, mode, out var o_appliedDuration, out var o_adjustedDuration, out var _, out var _);
					if (o_appliedDuration >= 1f)
					{
						string text = GUIUtils.Format(1731, statusEffectParams.FormatBaseDuration(source, o_appliedDuration, o_adjustedDuration));
						formattableTarget = new AttackBase.FormattableTarget(formattableTarget.StandaloneString + text, formattableTarget.QualifiedString + text);
					}
					AttackBase.FormattableTarget extraFormatParameter = formattableTarget;
					formattableTarget = StatusEffect.GetModifiedStatSpecialTargetString(statusEffectParams.AffectsStat);
					formattableTarget.ExtraFormatParameter = extraFormatParameter;
				}
				if (StatusEffect.EffectLaunchesAttack(statusEffectParams.AffectsStat) && (bool)statusEffectParams.AttackPrefab)
				{
					if (TargetTypeUtils.LayerTargetTypes(statusEffectParams.AttackPrefab.ValidTargets, item.Key.TargetType) != AttackBase.TargetType.None)
					{
						statusEffectParams.AttackPrefab.UICleanStatusEffects();
						statusEffectParams.AttackPrefab.AddEffects(formattableTarget, null, source ? source.gameObject : null, 1f, stringEffects);
					}
				}
				else if (statusEffectParams.AffectsStat == StatusEffect.ModifiedStat.ApplyAttackEffects && (bool)statusEffectParams.AttackPrefab)
				{
					statusEffectParams.AttackPrefab.UICleanStatusEffects();
					ListToStringEffects(statusEffectParams.AttackPrefab.CleanedUpStatusEffects, source, null, attack, null, null, StatusEffectFormatMode.InspectWindow, formattableTarget, targetQualifiers, AttackBase.TargetType.All, stringEffects);
				}
			}
			string text2 = ListToString(item, source, ability, affliction, phraseOrigin, mode, item.Key.TargetType);
			if (!string.IsNullOrEmpty(text2))
			{
				string text3 = targetFormat.GetText(item.Key.TargetType);
				if (!string.IsNullOrEmpty(targetQualifiers))
				{
					text3 += GUIUtils.Format(1731, targetQualifiers);
				}
				AttackBase.AddStringEffect(text3, new AttackBase.AttackEffect(text2, attack, hostile), stringEffects);
			}
		}
	}

	public static string FormatDuration(float applied, float adjusted, StatusEffectFormatMode mode)
	{
		string text = GUIUtils.Seconds(adjusted);
		if (adjusted != applied && mode == StatusEffectFormatMode.InspectWindow)
		{
			text = AttackBase.FormatBase(text, GUIUtils.Format(1555, GUIUtils.Seconds(applied)), applied < adjusted);
		}
		return text;
	}

	private static void GetEffectValuesForUi(StatusEffect effect, StatusEffectParams param, CharacterStats owner, GenericAbility ability, AfflictionParams affliction, StatusEffectFormatMode mode, out float o_appliedDuration, out float o_adjustedDuration, out float o_appliedValue, out float o_adjustedValue)
	{
		float num = (((bool)ability && ability.DurationOverride > 0f) ? ability.DurationOverride : ((affliction != null && affliction.Duration > 0f) ? affliction.Duration : ((effect == null) ? param.Duration : (effect.Duration - effect.TemporaryDurationAdjustment))));
		if (effect != null)
		{
			num += effect.UnadjustedDurationAdd;
		}
		float num2 = StatusEffect.EstimateDurationForUI(param, effect, owner, ability, affliction);
		float num3 = (param.IsCleanedUp ? param.MergedValue : param.Value);
		float num4 = (param.IsCleanedUp ? param.MergedValue : param.GetValue(owner));
		float num5 = 1f;
		bool flag = true;
		bool flag2 = (StatusEffect.IsScaledMultiplierStatic(param.AffectsStat) ? (param.Value == 1f) : (param.Value == 0f));
		if (mode == StatusEffectFormatMode.InspectWindow || (flag2 && !param.TriggerAdjustment.Ineffective))
		{
			if (!param.TriggerAdjustment.IneffectiveValue)
			{
				if (StatusEffect.IsScaledMultiplierStatic(param.AffectsStat))
				{
					num3 *= param.TriggerAdjustment.ValueAdjustment;
					num4 *= param.TriggerAdjustment.ValueAdjustment;
				}
				else
				{
					num3 += param.TriggerAdjustment.ValueAdjustment;
					num4 += param.TriggerAdjustment.ValueAdjustment;
				}
			}
		}
		else if (effect != null && effect.Applied)
		{
			num4 = effect.CurrentAppliedValueForUi;
			num = effect.Duration;
			flag = false;
		}
		if (effect != null)
		{
			if (flag && effect.AfflictionOrigin == null && effect.IsDamageDealing)
			{
				num5 += effect.Scale - 1f;
			}
			num2 = effect.CalculateDuration(owner ? owner.gameObject : null, ignoreTemporaryAdjustment: true);
		}
		if (flag && param.ChecksReligion)
		{
			num5 += Religion.Instance.GetCurrentBonusMultiplier(owner, ability) - 1f;
		}
		if ((bool)owner && (param.AffectsStat == StatusEffect.ModifiedStat.Damage || param.AffectsStat == StatusEffect.ModifiedStat.DisengagementDamage || param.AffectsStat == StatusEffect.ModifiedStat.BonusUnarmedDamage || param.AffectsStat == StatusEffect.ModifiedStat.BonusMeleeDamage || param.AffectsStat == StatusEffect.ModifiedStat.ReviveAndAddStamina || param.AffectsStat == StatusEffect.ModifiedStat.TransferDamageToStamina || param.AffectsStat == StatusEffect.ModifiedStat.StaminaByAthletics || param.AffectsStat == StatusEffect.ModifiedStat.DamageToSummon) && (!ability || !ability.IgnoreCharacterStats))
		{
			num5 += owner.StatDamageHealMultiplier - 1f;
		}
		if (param.AffectsStat == StatusEffect.ModifiedStat.Health || param.AffectsStat == StatusEffect.ModifiedStat.Stamina)
		{
			Health health = ((effect != null && (bool)effect.Target) ? effect.Target.GetComponent<Health>() : null);
			if ((bool)health && num3 > 0f)
			{
				num5 += health.GetHealingMultiplier(owner) - 1f;
			}
			else if ((bool)owner)
			{
				num5 += owner.StatDamageHealMultiplier - 1f;
			}
		}
		if (StatusEffect.StatNotRevokedParam(param.AffectsStat) && param.IntervalRate == IntervalRateType.None && param.Apply != StatusEffect.ApplyType.ApplyAtEnd)
		{
			num = (num2 = 0.5f);
		}
		num4 *= num5;
		o_appliedDuration = num;
		o_adjustedDuration = num2;
		o_appliedValue = num3;
		o_adjustedValue = num4;
	}

	public string GetString()
	{
		return GetString<StatusEffectParams>(null, null, showTime: true, null);
	}

	public string GetString(StatusEffect effect)
	{
		return GetString<StatusEffectParams>(effect, null, showTime: true, null);
	}

	public string GetString(StatusEffect effect, CharacterStats owner)
	{
		return GetString<StatusEffectParams>(effect, owner, showTime: true, null);
	}

	public string GetString(StatusEffect effect, CharacterStats owner, GenericAbility ability)
	{
		return GetString<StatusEffectParams>(effect, owner, ability, null, StatusEffectFormatMode.Default, showTime: true, AttackBase.TargetType.All, null);
	}

	public string GetString<T>(StatusEffect effect, CharacterStats owner, GenericAbility ability, IList<T> otherEffects)
	{
		return GetString(effect, owner, ability, null, StatusEffectFormatMode.Default, showTime: true, AttackBase.TargetType.All, otherEffects);
	}

	public string GetString(StatusEffect effect, CharacterStats owner, bool showTime)
	{
		return GetString<StatusEffectParams>(effect, owner, null, null, StatusEffectFormatMode.Default, showTime, AttackBase.TargetType.All, null);
	}

	public string GetString<T>(StatusEffect effect, CharacterStats owner, bool showTime, IList<T> otherEffects)
	{
		return GetString(effect, owner, null, null, StatusEffectFormatMode.Default, showTime, AttackBase.TargetType.All, otherEffects);
	}

	public string GetString<T>(StatusEffect effect, CharacterStats owner, GenericAbility ability, AfflictionParams affliction, StatusEffectFormatMode mode, bool showTime, AttackBase.TargetType targetType, IList<T> otherEffects)
	{
		if (AffectsStat == StatusEffect.ModifiedStat.NoEffect)
		{
			return "";
		}
		if (effect != null && effect.Owner != null && owner == null)
		{
			owner = effect.Owner.GetComponent<CharacterStats>();
		}
		if (!ability && effect != null)
		{
			ability = effect.AbilityOrigin;
		}
		string text = "{0}";
		GetEffectValuesForUi(effect, this, owner, ability, affliction, mode, out var o_appliedDuration, out var o_adjustedDuration, out var o_appliedValue, out var o_adjustedValue);
		float num = effect?.Interval ?? StatusEffect.GetInterval(this);
		bool flag = !TriggerAdjustment.Ineffective && TriggerAdjustment.DurationAdjustment > 0f && TriggerAdjustment.DurationAdjustment < 1f && Duration < 1f;
		if ((IntervalRate != 0 || effect == null || !effect.IsAura) && (effect == null || (!effect.IsSuppressed && !effect.IsSuspended)))
		{
			if (((effect != null && effect.IsDOT) || IsOverTime) && (o_appliedDuration >= 1f || o_appliedDuration < float.Epsilon) && !flag)
			{
				bool flag2 = false;
				if (((bool)ability && ability.Passive) || o_appliedDuration == 0f)
				{
					flag2 = true;
				}
				else if (StatusEffect.StatNotRevokedParam(AffectsStat) && Apply == StatusEffect.ApplyType.ApplyOnTick)
				{
					o_appliedValue *= Mathf.Max(1f, 1f + o_appliedDuration / num);
					o_adjustedValue *= Mathf.Max(1f, 1f + o_adjustedDuration / num);
				}
				string text2 = FormatBaseDuration(owner, o_appliedDuration, o_adjustedDuration);
				if (flag2)
				{
					text = GUIUtils.Format(1419, text, GUIUtils.Format(211, num.ToString("#0.0#")));
					if (o_appliedDuration > float.Epsilon)
					{
						text = GUIUtils.Format(1665, text, text2);
					}
				}
				else
				{
					text = GUIUtils.Format(1402, text, text2);
				}
			}
			else if (Apply == StatusEffect.ApplyType.ApplyAtEnd)
			{
				string text3 = FormatBaseDuration(owner, o_appliedDuration, o_adjustedDuration);
				text = GUIUtils.Format(1547, text, text3);
			}
			else if (showTime && o_appliedDuration >= 1f)
			{
				string text4 = (((mode != StatusEffectFormatMode.CharacterSheet && mode != StatusEffectFormatMode.PartyBar) || effect == null) ? FormatDuration(o_appliedDuration, o_adjustedDuration, mode) : GUIUtils.Seconds(effect.TimeLeft));
				if (!string.IsNullOrEmpty(text4))
				{
					text = GUIUtils.Format((Apply == StatusEffect.ApplyType.ApplyAtEnd) ? 1547 : 1665, text, text4);
				}
			}
		}
		Gender gender = CharacterStats.GetGender(owner);
		DatabaseString statDisplayString = StatusEffect.GetStatDisplayString(AffectsStat);
		string text5 = ((statDisplayString != null) ? statDisplayString.GetText(gender) : "");
		if (AffectsStat == StatusEffect.ModifiedStat.DamageThreshhold)
		{
			if (DmgType == DamagePacket.DamageType.All)
			{
				text5 = GUIUtils.GetText(1483, gender);
			}
			else if (DmgType == DamagePacket.DamageType.None)
			{
				return "";
			}
		}
		else if (AffectsStat == StatusEffect.ModifiedStat.TransferDT)
		{
			if (DmgType == DamagePacket.DamageType.All)
			{
				text5 = GUIUtils.GetText(1484, gender);
			}
			else if (DmgType == DamagePacket.DamageType.None)
			{
				return "";
			}
		}
		else if (AffectsStat == StatusEffect.ModifiedStat.BonusDamageMult)
		{
			if (DmgType == DamagePacket.DamageType.All)
			{
				text5 = GUIUtils.GetText(1144, gender);
			}
			else if (DmgType == DamagePacket.DamageType.None)
			{
				return "";
			}
		}
		else if (AffectsStat == StatusEffect.ModifiedStat.DamageShield)
		{
			if (DmgType == DamagePacket.DamageType.All)
			{
				text5 = GUIUtils.GetText(2159, gender);
			}
			else if (DmgType == DamagePacket.DamageType.None)
			{
				return "";
			}
		}
		string text6 = "";
		switch (AffectsStat)
		{
		case StatusEffect.ModifiedStat.Deflection:
		case StatusEffect.ModifiedStat.Fortitude:
		case StatusEffect.ModifiedStat.Reflex:
		case StatusEffect.ModifiedStat.Will:
		case StatusEffect.ModifiedStat.AllDefense:
		case StatusEffect.ModifiedStat.AllDefensesExceptDeflection:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.FloorToInt(v))));
			break;
		case StatusEffect.ModifiedStat.DamageThreshhold:
		{
			string paramNamesAndRemove3 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.DmgType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove3);
			break;
		}
		case StatusEffect.ModifiedStat.BonusDamageMult:
		case StatusEffect.ModifiedStat.IncomingDamageMult:
		{
			string paramNamesAndRemove16 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.DmgType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.MultiplierAsPercentBonus(v)), paramNamesAndRemove16);
			break;
		}
		case StatusEffect.ModifiedStat.Disintegrate:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.0"), GUIUtils.GetDamageTypeString(DamagePacket.DamageType.Raw));
			break;
		case StatusEffect.ModifiedStat.Damage:
			if (mode == StatusEffectFormatMode.CombatLog && DmgType != DamagePacket.DamageType.Raw && effect != null && effect.Target != null)
			{
				CharacterStats component = effect.Target.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					float b = component.AdjustDamageByDTDR(o_adjustedValue, DmgType, null, owner ? owner.gameObject : null, 0.25f);
					string text13 = Mathf.Max(0f, b).ToString("#0.0") + " " + GUIUtils.GetDamageTypeString(DmgType);
					float num4 = component.CalcDT(DmgType, isVeilPiercing: false) * 0.25f;
					string text14 = (float.IsPositiveInfinity(num4) ? GUIUtils.GetText(2187) : Mathf.Max(0f, num4).ToString("#0.0"));
					string text15 = GUIUtils.GetText(1622) + ":" + text14;
					text6 = o_adjustedValue.ToString("#0.0") + " - " + text15 + " = " + text13 + ".";
					break;
				}
			}
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.0"), GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.DamageBasedOnInverseStamina:
		case StatusEffect.ModifiedStat.DOTOnHit:
		case StatusEffect.ModifiedStat.TransferDT:
		case StatusEffect.ModifiedStat.TransferDamageToStamina:
		case StatusEffect.ModifiedStat.DamageAttackerOnImplementLaunch:
		case StatusEffect.ModifiedStat.DamageToSummon:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.0"), GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.DamageShield:
		{
			if (Value >= 99999f)
			{
				text6 = GUIUtils.GetText(2279);
				break;
			}
			string text11 = FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.#");
			if (effect != null && mode != StatusEffectFormatMode.CombatLog && mode != StatusEffectFormatMode.InspectWindow)
			{
				text11 = GUIUtils.Format(451, effect.m_damageToAbsorb.ToString("#0"), text11);
			}
			text6 = StringUtility.Format(text5, text11, GUIUtils.GetDamageTypeString(DmgType));
			break;
		}
		case StatusEffect.ModifiedStat.DamageByKeywordCount:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.#"), GUIUtils.GetDamageTypeString(DmgType), GetParamNamesAndRemove(this, effect, otherEffects, ParamType.Keyword));
			break;
		case StatusEffect.ModifiedStat.AddDamageTypeImmunity:
			text6 = StringUtility.Format(text5, GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.DamageToStamina:
		{
			string paramNamesAndRemove5 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.DmgType);
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatPercentValue(o_appliedValue, o_adjustedValue)), paramNamesAndRemove5);
			break;
		}
		case StatusEffect.ModifiedStat.TransferDamageToCaster:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => Mathf.RoundToInt((1f - v) * 100f).ToString())), GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.BonusDamage:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "0#")), GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.AccuracyByWeaponType:
		case StatusEffect.ModifiedStat.ExtraProjectilesByWeaponType:
		{
			string paramNamesAndRemove14 = GetParamNamesAndRemove(this, otherEffects, ParamType.EquippablePrefab, delegate(StatusEffectParams sp)
			{
				Weapon weapon = sp.EquippablePrefab as Weapon;
				return (!weapon) ? "*NULL*" : GUIUtils.GetWeaponTypeIDs(weapon.WeaponType);
			});
			text6 = StringUtility.Format(text5, FormatIntBonus(o_appliedValue, o_adjustedValue), paramNamesAndRemove14);
			break;
		}
		case StatusEffect.ModifiedStat.LaunchAttack:
		case StatusEffect.ModifiedStat.LaunchAttackWithRollingBonus:
			if (mode != StatusEffectFormatMode.InspectWindow)
			{
				if ((bool)AttackPrefab)
				{
					if (TargetTypeUtils.LayerTargetTypes(AttackPrefab.ValidTargets, targetType) == AttackBase.TargetType.None)
					{
						text6 = "";
						break;
					}
					StringEffects stringEffects = new StringEffects();
					AttackPrefab.AddEffects(AttackPrefab.GetMainTargetString(ability), ability, owner ? owner.gameObject : null, 1f, stringEffects);
					text6 = AttackBase.StringEffects(stringEffects, mode == StatusEffectFormatMode.TalentModification);
				}
				break;
			}
			return "";
		case StatusEffect.ModifiedStat.NonTargetable:
		case StatusEffect.ModifiedStat.NonMobile:
		case StatusEffect.ModifiedStat.KnockedDown:
		case StatusEffect.ModifiedStat.NonEngageable:
		case StatusEffect.ModifiedStat.Stunned:
		case StatusEffect.ModifiedStat.MeleeAttackAllOnPath:
		case StatusEffect.ModifiedStat.MarkedPrey:
		case StatusEffect.ModifiedStat.SuspendHostileEffects:
		case StatusEffect.ModifiedStat.ImmuneToEngageStop:
		case StatusEffect.ModifiedStat.SuspendBeneficialEffects:
		case StatusEffect.ModifiedStat.RedirectMeleeAttacks:
		case StatusEffect.ModifiedStat.ImprovedFlanking:
		case StatusEffect.ModifiedStat.SwapFaction:
		case StatusEffect.ModifiedStat.AttackOnMeleeHit:
		case StatusEffect.ModifiedStat.MinorSpellReflection:
		case StatusEffect.ModifiedStat.CanStunOnCrit:
		case StatusEffect.ModifiedStat.Confused:
		case StatusEffect.ModifiedStat.SpellReflection:
		case StatusEffect.ModifiedStat.DisableSpellcasting:
		case StatusEffect.ModifiedStat.PreventDeath:
		case StatusEffect.ModifiedStat.CanKnockDownOnCrit:
		case StatusEffect.ModifiedStat.RangedGrazeReflection:
		case StatusEffect.ModifiedStat.StopAnimation:
		case StatusEffect.ModifiedStat.Invisible:
		case StatusEffect.ModifiedStat.HidesHealthStamina:
		case StatusEffect.ModifiedStat.ShieldDeflectionExtendToReflex:
		case StatusEffect.ModifiedStat.BreakAllEngagement:
		case StatusEffect.ModifiedStat.CantUseFoodDrinkDrugs:
		case StatusEffect.ModifiedStat.NegateNextRecovery:
		case StatusEffect.ModifiedStat.DamageAlwaysMinimumAgainstCCD:
			text6 = text5;
			break;
		case StatusEffect.ModifiedStat.AttackSpeed:
		case StatusEffect.ModifiedStat.MeleeAttackDistanceMult:
		case StatusEffect.ModifiedStat.RangedAttackDistanceMult:
		case StatusEffect.ModifiedStat.BeamDamageMult:
		case StatusEffect.ModifiedStat.ReloadSpeed:
		case StatusEffect.ModifiedStat.StaminaRechargeRateMult:
		case StatusEffect.ModifiedStat.BonusDamageMultOnLowStaminaTarget:
		case StatusEffect.ModifiedStat.HostileEffectDurationMult:
		case StatusEffect.ModifiedStat.DamageToDOT:
		case StatusEffect.ModifiedStat.BonusDamageMultIfTargetHasDOT:
		case StatusEffect.ModifiedStat.HostileAOEDamageMultiplier:
		case StatusEffect.ModifiedStat.BonusDamageMultOnSameEnemy:
		case StatusEffect.ModifiedStat.BonusMeleeWeaponDamageMult:
		case StatusEffect.ModifiedStat.BonusRangedWeaponDamageMult:
		case StatusEffect.ModifiedStat.RateOfFireMult:
		case StatusEffect.ModifiedStat.DOTTickMult:
		case StatusEffect.ModifiedStat.BonusDamageMultOnKDSFTarget:
		case StatusEffect.ModifiedStat.StunDurationMult:
		case StatusEffect.ModifiedStat.KnockDownDurationMult:
		case StatusEffect.ModifiedStat.BonusArmorDtMultAtLowHealth:
		case StatusEffect.ModifiedStat.SpellDamageMult:
		case StatusEffect.ModifiedStat.FinishingBlowDamageMult:
		case StatusEffect.ModifiedStat.ZealousAuraAoEMult:
		case StatusEffect.ModifiedStat.NegMoveTickMult:
		case StatusEffect.ModifiedStat.BonusDamageMultOnFlankedTarget:
		case StatusEffect.ModifiedStat.FocusGainMult:
		case StatusEffect.ModifiedStat.PoisonTickMult:
		case StatusEffect.ModifiedStat.DiseaseTickMult:
		case StatusEffect.ModifiedStat.StalkersLinkDamageMult:
		case StatusEffect.ModifiedStat.ChanterPhraseAoEMult:
		case StatusEffect.ModifiedStat.BonusHealMult:
		case StatusEffect.ModifiedStat.IncomingCritDamageMult:
		case StatusEffect.ModifiedStat.AoEMult:
		case StatusEffect.ModifiedStat.FrenzyDurationMult:
		case StatusEffect.ModifiedStat.ProneDurationMult:
		case StatusEffect.ModifiedStat.WildstrikeDamageMult:
		case StatusEffect.ModifiedStat.MaxStaminaMult:
		case StatusEffect.ModifiedStat.BonusRangedWeaponCloseEnemyDamageMult:
		case StatusEffect.ModifiedStat.BonusDamageMultWithImplements:
		case StatusEffect.ModifiedStat.WeapMinDamageMult:
		case StatusEffect.ModifiedStat.BonusTwoHandedMeleeWeaponDamageMult:
		case StatusEffect.ModifiedStat.BonusMeleeDamageMult:
		case StatusEffect.ModifiedStat.BonusHealingGivenMult:
		case StatusEffect.ModifiedStat.BonusArmorDtMult:
		case StatusEffect.ModifiedStat.PhraseRecitationLengthMult:
		case StatusEffect.ModifiedStat.DrugDurationMult:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.MultiplierAsPercentBonus(v)));
			break;
		case StatusEffect.ModifiedStat.DamagePlusDot:
		case StatusEffect.ModifiedStat.PostDtDamagePlusDot:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatPercentValue(o_appliedValue, o_adjustedValue))) + GUIUtils.Format(1731, GUIUtils.Seconds(GetExtraValue(owner)));
			break;
		case StatusEffect.ModifiedStat.CanStun:
		case StatusEffect.ModifiedStat.SneakAttackOnNearDead:
		case StatusEffect.ModifiedStat.CanCripple:
		case StatusEffect.ModifiedStat.ReapplyDamage:
		case StatusEffect.ModifiedStat.ReapplyDamageToNearbyEnemies:
		case StatusEffect.ModifiedStat.GainStaminaWhenHits:
		case StatusEffect.ModifiedStat.DamageToStaminaRegen:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatPercentValue(o_appliedValue, o_adjustedValue)));
			break;
		case StatusEffect.ModifiedStat.RangedReflection:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, Mathf.RoundToInt(ExtraValue * 100f)));
			break;
		case StatusEffect.ModifiedStat.BonusCritChanceOnSameEnemy:
		case StatusEffect.ModifiedStat.EnemyCritToHitPercent:
		case StatusEffect.ModifiedStat.EnemyDeflectReflexHitToGrazePercent:
		case StatusEffect.ModifiedStat.EnemyFortitudeWillHitToGrazePercent:
		case StatusEffect.ModifiedStat.BonusGrazeToHitPercent:
		case StatusEffect.ModifiedStat.BonusGrazeToMissPercent:
		case StatusEffect.ModifiedStat.BonusCritToHitPercent:
		case StatusEffect.ModifiedStat.BonusMissToGrazePercent:
		case StatusEffect.ModifiedStat.BonusHitToCritPercent:
		case StatusEffect.ModifiedStat.BonusHitToGrazePercent:
		case StatusEffect.ModifiedStat.EnemyReflexGrazeToMissPercent:
		case StatusEffect.ModifiedStat.StaminaPercent:
		case StatusEffect.ModifiedStat.EnemyHitToGrazePercent:
		case StatusEffect.ModifiedStat.BonusGrazeToHitRatioMeleeOneHand:
		case StatusEffect.ModifiedStat.HealthPercent:
		case StatusEffect.ModifiedStat.EnemyGrazeToMissPercent:
		case StatusEffect.ModifiedStat.EnemyReflexHitToGrazePercent:
		case StatusEffect.ModifiedStat.BonusHitToCritPercentAll:
		case StatusEffect.ModifiedStat.BonusHitToCritRatioMeleeOneHand:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt(v * 100f)))));
			break;
		case StatusEffect.ModifiedStat.ConsumableDurationMult:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt((v - 1f) * 100f)))));
			break;
		case StatusEffect.ModifiedStat.RangedMovingRecoveryReductionPct:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt((0f - v) * 100f)))));
			break;
		case StatusEffect.ModifiedStat.TransferAttackSpeed:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatPercentValue(o_appliedValue, o_adjustedValue)));
			break;
		case StatusEffect.ModifiedStat.BonusPotionEffectOrDurationPercent:
		case StatusEffect.ModifiedStat.MeleeAttackSpeedPercent:
		case StatusEffect.ModifiedStat.RangedAttackSpeedPercent:
		case StatusEffect.ModifiedStat.TrapBonusDamageOrDurationPercent:
		case StatusEffect.ModifiedStat.BonusHitToCritPercentEnemyBelow10Percent:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt(v)))));
			break;
		case StatusEffect.ModifiedStat.BonusMeleeDamageFromWounds:
		case StatusEffect.ModifiedStat.BonusDamageProc:
		case StatusEffect.ModifiedStat.BonusDamageByTypePercent:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt(v)))), GUIUtils.GetDamageTypeString(DmgType));
			break;
		case StatusEffect.ModifiedStat.ArmorSpeedFactorAdj:
		case StatusEffect.ModifiedStat.SingleWeaponSpeedFactorAdj:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(Mathf.RoundToInt((0f - v) * 100f)))));
			break;
		case StatusEffect.ModifiedStat.MeleeDamageRangePctIncreaseToMin:
		case StatusEffect.ModifiedStat.DualWieldAttackSpeedPercent:
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0")));
			break;
		case StatusEffect.ModifiedStat.DrainResolveForDeflection:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.#"), TextUtils.NumberBonus(GetExtraValue(owner)));
			break;
		case StatusEffect.ModifiedStat.DropTrap:
			text6 = StringUtility.Format(text5, TrapPrefab.GetDisplayName());
			break;
		case StatusEffect.ModifiedStat.TransferBeneficialTime:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, GUIUtils.Seconds));
			break;
		case StatusEffect.ModifiedStat.StasisShield:
			text6 = ((!(Value >= 99999f)) ? StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.#")) : GUIUtils.GetText(1870));
			break;
		case StatusEffect.ModifiedStat.TransferStamina:
		case StatusEffect.ModifiedStat.DTBypass:
		case StatusEffect.ModifiedStat.TransferRandomAttribute:
		case StatusEffect.ModifiedStat.Fatigue:
		case StatusEffect.ModifiedStat.DelayUnconsciousness:
		case StatusEffect.ModifiedStat.ReviveAndAddStamina:
		case StatusEffect.ModifiedStat.TransferStaminaReversed:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0.#"));
			break;
		case StatusEffect.ModifiedStat.TransferAttribute:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0"), GUIUtils.GetAttributeScoreTypeString(AttributeType));
			break;
		case StatusEffect.ModifiedStat.StealSpell:
			if (effect != null && effect.Spells != null && effect.Spells.Count > 0)
			{
				text6 = StringUtility.Format(GUIUtils.GetText(2274), TextUtils.FuncJoin((GenericAbility spell) => (!spell) ? "*null*" : spell.Name(), effect.Spells, GUIUtils.Comma()), CharacterStats.NameColored(effect.Target));
				break;
			}
			if (ExtraValue != 1f)
			{
				text5 = GUIUtils.GetText(2273);
			}
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0"), GetExtraValue(owner));
			break;
		case StatusEffect.ModifiedStat.SummonWeapon:
		case StatusEffect.ModifiedStat.SummonSecondaryWeapon:
		{
			string text12 = (EquippablePrefab ? $"[url=item://{EquippablePrefab.name}]{EquippablePrefab.Name}[/url]" : "*NULL*");
			text6 = StringUtility.Format(text5, text12);
			break;
		}
		case StatusEffect.ModifiedStat.ResistAffliction:
		{
			string paramNamesAndRemove9 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.AfflictionPrefab);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove9);
			float extraValue2 = GetExtraValue(owner);
			if (extraValue2 != 0f)
			{
				text6 = text6 + GUIUtils.Comma() + GUIUtils.Format(1690, GUIUtils.Format(211, TextUtils.NumberBonus(extraValue2, "#0.#")), paramNamesAndRemove9);
			}
			break;
		}
		case StatusEffect.ModifiedStat.ShortenAfflictionDuration:
		case StatusEffect.ModifiedStat.ShortenAfflictionDurationOngoing:
		{
			string paramNamesAndRemove8 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.AfflictionPrefab);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => GUIUtils.Format(211, TextUtils.NumberBonus(v, "#0.#"))), paramNamesAndRemove8);
			break;
		}
		case StatusEffect.ModifiedStat.AccuracyBonusForAttackersWithAffliction:
		{
			string paramNamesAndRemove7 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.AfflictionPrefab);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove7);
			break;
		}
		case StatusEffect.ModifiedStat.AddAfflictionImmunity:
		case StatusEffect.ModifiedStat.RemoveAffliction:
		{
			string paramNamesAndRemove6 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.AfflictionPrefab);
			text6 = StringUtility.Format(text5, paramNamesAndRemove6, paramNamesAndRemove6);
			break;
		}
		case StatusEffect.ModifiedStat.ResistKeyword:
		{
			string paramNamesAndRemove4 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.Keyword);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove4);
			float extraValue = GetExtraValue(owner);
			if (extraValue != 0f)
			{
				text6 = text6 + GUIUtils.Comma() + GUIUtils.Format(1690, GUIUtils.Format(211, TextUtils.NumberBonus(extraValue, "#0.#")), paramNamesAndRemove4);
			}
			break;
		}
		case StatusEffect.ModifiedStat.AfflictionShield:
		{
			string text9 = FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0");
			if (effect != null && mode != StatusEffectFormatMode.CombatLog && mode != StatusEffectFormatMode.InspectWindow)
			{
				text9 = GUIUtils.Format(451, o_appliedValue - (float)effect.m_generalCounter, o_appliedValue);
			}
			string paramNamesAndRemove2 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.AfflictionPrefab);
			text6 = StringUtility.Format(text5, text9, paramNamesAndRemove2);
			break;
		}
		case StatusEffect.ModifiedStat.KeywordImmunity:
		case StatusEffect.ModifiedStat.RemoveAllEffectsByKeyword:
		{
			string paramNamesAndRemove = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.Keyword);
			text6 = StringUtility.Format(text5, paramNamesAndRemove);
			break;
		}
		case StatusEffect.ModifiedStat.BonusAccuracyOnSameEnemyAsExtraObject:
		{
			string text7 = FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#"));
			string text8 = ((mode != StatusEffectFormatMode.InspectWindow && effect != null && !(effect.ExtraObject == null)) ? CharacterStats.Name(effect.ExtraObject) : GUIUtils.GetText(1509));
			text6 = StringUtility.Format(text5, text7, text8);
			break;
		}
		case StatusEffect.ModifiedStat.GrantFocusToExtraObject:
		{
			string text16 = GUIUtils.Format(1277, FormatPercentValue(o_appliedValue, o_adjustedValue));
			string text17 = ((mode != StatusEffectFormatMode.InspectWindow && effect != null && !(effect.ExtraObject == null)) ? CharacterStats.Name(effect.ExtraObject) : GUIUtils.GetText(1611));
			text6 = StringUtility.Format(text5, text16, text17);
			break;
		}
		case StatusEffect.ModifiedStat.BonusAccuracyAtLowStamina:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), GUIUtils.Format(1277, Mathf.RoundToInt(GetExtraValue(owner) * 100f)));
			break;
		case StatusEffect.ModifiedStat.BonusDamageMultAtLowStamina:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.MultiplierAsPercentBonus(v)), GUIUtils.Format(1277, Mathf.RoundToInt(GetExtraValue(owner) * 100f)));
			break;
		case StatusEffect.ModifiedStat.AccuracyByRace:
		{
			string paramNamesAndRemove15 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.RaceType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove15);
			break;
		}
		case StatusEffect.ModifiedStat.DamageMultByRace:
		{
			string paramNamesAndRemove13 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.RaceType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.MultiplierAsPercentBonus(v)), paramNamesAndRemove13);
			break;
		}
		case StatusEffect.ModifiedStat.AccuracyByClass:
		{
			string paramNamesAndRemove12 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.ClassType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")), paramNamesAndRemove12);
			break;
		}
		case StatusEffect.ModifiedStat.DamageMultByClass:
		{
			string paramNamesAndRemove11 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.ClassType);
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.MultiplierAsPercentBonus(v)), paramNamesAndRemove11);
			break;
		}
		case StatusEffect.ModifiedStat.BonusDamageByRacePercent:
		{
			string paramNamesAndRemove10 = GetParamNamesAndRemove(this, effect, otherEffects, ParamType.RaceType);
			text6 = StringUtility.Format(text5, GUIUtils.Format(1277, FormatIntBonus(o_appliedValue, o_adjustedValue)), paramNamesAndRemove10);
			break;
		}
		case StatusEffect.ModifiedStat.SpellCastBonus:
			text6 = StringUtility.Format(text5, FormatIntBonus(o_appliedValue, o_adjustedValue), Ordinal.Get((int)GetExtraValue(owner)));
			break;
		case StatusEffect.ModifiedStat.CallbackOnDamaged:
		case StatusEffect.ModifiedStat.CallbackAfterAttack:
			text6 = Description;
			break;
		case StatusEffect.ModifiedStat.GenericMarker:
		{
			int num3 = (int)GetValue(owner);
			if (num3 >= 0)
			{
				text6 = GUIUtils.GetText(num3);
				break;
			}
			if (effect != null)
			{
				return effect.BundleName;
			}
			text6 = "";
			break;
		}
		case StatusEffect.ModifiedStat.ApplyPulsedAOE:
			if ((bool)AttackPrefab)
			{
				text6 = AttackPrefab.GetMainTargetString(ability).GetText(AttackPrefab.ValidTargets);
				break;
			}
			UIDebug.Instance.LogOnScreenWarning("Status Effect on " + owner.name + " is ApplyPulsedAOE but has no AttackPrefab.", UIDebug.Department.Design, 10f);
			return "";
		case StatusEffect.ModifiedStat.SummonConsumable:
		{
			string text10 = (ConsumablePrefab ? $"[url=item://{ConsumablePrefab.name}]{ConsumablePrefab.Name}[/url]" : "*NULL*");
			text6 = ((o_adjustedValue != 0f) ? StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, "#0"), text10) : GUIUtils.Format(2203, o_adjustedValue.ToString("#0"), text10));
			break;
		}
		case StatusEffect.ModifiedStat.ApplyFinishingBlowDamage:
		case StatusEffect.ModifiedStat.VerticalLaunch:
		case StatusEffect.ModifiedStat.AttackOnHitWithMelee:
			return "";
		case StatusEffect.ModifiedStat.Push:
		{
			float num2 = o_appliedValue;
			if (num2 < 0f)
			{
				num2 *= -1f;
				text5 = GUIUtils.GetText(1901);
			}
			text6 = StringUtility.Format(text5, GUIUtils.Format(1533, o_appliedValue.ToString("#0.#")));
			break;
		}
		case StatusEffect.ModifiedStat.ProhibitEnemyEngagementByLevel:
			text6 = ((o_appliedValue != -1f) ? ((o_appliedValue != 1f) ? ((!(o_appliedValue < 0f)) ? ("*UnsupportedValue:" + o_appliedValue + "*") : StringUtility.Format(text5, o_appliedValue * -1f)) : ("*UnsupportedValue:" + o_appliedValue + "*")) : GUIUtils.GetText(2151));
			break;
		case StatusEffect.ModifiedStat.GrantAbility:
			text6 = AbilityPrefab.Name();
			break;
		case StatusEffect.ModifiedStat.MindwebEffect:
			text6 = StringTableManager.GetText(DatabaseString.StringTableType.Abilities, 1695);
			break;
		case StatusEffect.ModifiedStat.RestoreSpiritshiftUses:
			if (o_appliedValue == 1f)
			{
				text5 = GUIUtils.GetText(2391);
			}
			text6 = StringUtility.Format(text5, o_appliedValue.ToString("#0"), StringTableManager.GetText(DatabaseString.StringTableType.Abilities, 1508));
			break;
		case StatusEffect.ModifiedStat.ApplyAffliction:
			if ((bool)AfflictionPrefab && AfflictionPrefab.HideFromUI)
			{
				return "";
			}
			text6 = (AfflictionPrefab ? AfflictionPrefab.Name() : "*null*");
			if (o_appliedValue >= 1f)
			{
				text6 = GUIUtils.Format(1665, text6, GUIUtils.Seconds(o_appliedValue));
			}
			break;
		default:
			text6 = StringUtility.Format(text5, FormatBaseValue(owner, o_appliedValue, o_adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0.#")));
			break;
		}
		if (TriggerAdjustment.Type != 0)
		{
			text6 = TriggerAdjustment.GetString(text6);
		}
		text6 = StringUtility.Format(text, text6);
		if (effect != null && (effect.IsSuppressed || effect.IsSuspended))
		{
			return "[" + NGUITools.EncodeColor(UIGlobalColor.Instance.DarkDisabled) + "]" + text6 + GUIUtils.Format(1731, GUIUtils.GetText(379)) + "[-]";
		}
		return text6;
	}

	private string FormatBaseValue(CharacterStats character, float appliedValue, float adjustedValue, Func<float, string> format)
	{
		string text = "";
		if (ChecksReligion && (bool)character && character.gameObject == GameState.s_playerCharacter.gameObject)
		{
			text = GUIUtils.Comma() + GUIUtils.GetText(2453);
		}
		if (LevelScaling.ValueAdjustment != 0f && UseLevelScaling(character))
		{
			return AttackBase.FormatBase(format(adjustedValue) + "*", GUIUtils.Format(1555, format(appliedValue)) + GUIUtils.Comma() + LevelScaling.GetValueString() + text, Math.Abs(appliedValue) <= Math.Abs(adjustedValue));
		}
		if (!string.IsNullOrEmpty(text))
		{
			return AttackBase.FormatBase(format(adjustedValue), GUIUtils.Format(1555, format(appliedValue)) + text, Math.Abs(appliedValue) <= Math.Abs(adjustedValue));
		}
		return TextUtils.FormatBase(appliedValue, adjustedValue, format);
	}

	private string FormatBaseValue(CharacterStats character, float appliedValue, float adjustedValue, string format)
	{
		string text = "";
		if (ChecksReligion && (bool)character && character.gameObject == GameState.s_playerCharacter.gameObject)
		{
			text = GUIUtils.Comma() + GUIUtils.GetText(2453);
		}
		if (LevelScaling.ValueAdjustment != 0f && UseLevelScaling(character))
		{
			return AttackBase.FormatBase(adjustedValue.ToString(format) + "*", GUIUtils.Format(1555, appliedValue.ToString(format)) + GUIUtils.Comma() + LevelScaling.GetValueString() + text, Math.Abs(appliedValue) <= Math.Abs(adjustedValue));
		}
		if (!string.IsNullOrEmpty(text))
		{
			return AttackBase.FormatBase(adjustedValue.ToString(format), GUIUtils.Format(1555, appliedValue.ToString(format)) + text, Math.Abs(appliedValue) <= Math.Abs(adjustedValue));
		}
		return TextUtils.FormatBase(appliedValue, adjustedValue, format);
	}

	private string FormatBaseDuration(CharacterStats character, float appliedValue, float adjustedValue)
	{
		if (LevelScaling.DurationAdjustment != 0f && UseLevelScaling(character))
		{
			return AttackBase.FormatBase(GUIUtils.Seconds(adjustedValue) + "*", GUIUtils.Format(1555, GUIUtils.Seconds(appliedValue)) + GUIUtils.Comma() + LevelScaling.GetDurationString(), Math.Abs(appliedValue) <= Math.Abs(adjustedValue));
		}
		return TextUtils.FormatBase(appliedValue, adjustedValue, GUIUtils.Seconds);
	}

	private static string FormatIntBonus(float appliedValue, float adjustedValue)
	{
		return TextUtils.FormatBase(appliedValue, adjustedValue, (float v) => TextUtils.NumberBonus(v, "#0"));
	}

	private static string FormatPercentValue(float appliedValue, float adjustedValue)
	{
		return TextUtils.FormatBase(appliedValue, adjustedValue, (float v) => Mathf.RoundToInt(v * 100f).ToString());
	}

	private static string GetParamNamesAndRemove<T>(StatusEffectParams effectParams, StatusEffect effect, IList<T> otherEffects, ParamType parameter)
	{
		return GetParamNamesAndRemove(((object)effect) ?? ((object)effectParams), otherEffects, parameter, (StatusEffectParams sp) => sp.ParamToString(parameter));
	}

	private static string GetParamNamesAndRemove<T>(object root, IList<T> otherEffects, ParamType parameter, Func<StatusEffectParams, string> toString)
	{
		if (typeof(T) != typeof(StatusEffectParams) && typeof(T) != typeof(StatusEffect))
		{
			throw new ArgumentException(string.Concat("Collection of type '", typeof(T), "' is not supported. Supported types are StatusEffect, StatusEffectParams."));
		}
		StatusEffectParams statusEffectParams = root as StatusEffectParams;
		StatusEffect statusEffect = root as StatusEffect;
		if (statusEffectParams == null && statusEffect != null)
		{
			statusEffectParams = statusEffect.Params;
		}
		if (!StatusEffect.UsesParamOfType(statusEffectParams.AffectsStat, parameter))
		{
			throw new ArgumentException(string.Concat("Status effect '", statusEffectParams.AffectsStat, "' doesn't use the '", parameter, "' parameter."));
		}
		bool flag = typeof(T) == typeof(StatusEffectParams);
		string text = toString(statusEffectParams);
		if (otherEffects == null)
		{
			return text;
		}
		for (int num = otherEffects.Count - 1; num >= 0; num--)
		{
			StatusEffectParams statusEffectParams2 = ((!flag) ? ((StatusEffect)(object)otherEffects[num]).Params : ((StatusEffectParams)(object)otherEffects[num]));
			if ((statusEffect == null || !((object)otherEffects[num] is StatusEffect statusEffect2) || statusEffect2.IsSuppressed == statusEffect.IsSuppressed) && StatusEffect.UsesParamOfType(statusEffectParams.AffectsStat, parameter) && statusEffectParams.EqualsExceptParameter(statusEffectParams2, parameter))
			{
				text = text + GUIUtils.Comma() + toString(statusEffectParams2);
				otherEffects.RemoveAt(num);
			}
		}
		return text;
	}

	public string ParamToString(ParamType parameter)
	{
		switch (parameter)
		{
		case ParamType.Tag:
		case ParamType.DmgType:
			return GUIUtils.GetDamageTypeString(DmgType);
		case ParamType.Value:
			return Value.ToString();
		case ParamType.ExtraValue:
			return ExtraValue.ToString();
		case ParamType.TrapPrefab:
			return TrapPrefab.GetDisplayName();
		case ParamType.EquippablePrefab:
			return EquippablePrefab.Name;
		case ParamType.ConsumablePrefab:
			return ConsumablePrefab.Name;
		case ParamType.AfflictionPrefab:
			return Affliction.Name(AfflictionPrefab);
		case ParamType.RaceType:
			return GUIUtils.GetRaceString(RaceType, Gender.Neuter);
		case ParamType.ClassType:
			return GUIUtils.GetPluralClassString(ClassType, Gender.Neuter);
		case ParamType.Keyword:
		{
			DatabaseString adjective = KeywordData.GetAdjective(Keyword);
			if (adjective != null)
			{
				return adjective.GetText();
			}
			return "*KeywordError*";
		}
		default:
			throw new NotImplementedException(string.Concat("Parameter '", parameter, "' isn't supported."));
		}
	}

	public float EstimateAccuracyBonusForUi(AttackBase attack)
	{
		if (AffectsStat == StatusEffect.ModifiedStat.Accuracy || (AffectsStat == StatusEffect.ModifiedStat.MeleeAccuracy && attack is AttackMelee) || (AffectsStat == StatusEffect.ModifiedStat.RangedAccuracy && attack is AttackRanged))
		{
			return GetValue(attack.Owner);
		}
		if (AffectsStat == StatusEffect.ModifiedStat.MeleeWeaponAccuracy && attack is AttackMelee && attack.IsAutoAttack())
		{
			return GetValue(attack.Owner);
		}
		if (AffectsStat == StatusEffect.ModifiedStat.Perception)
		{
			return CharacterStats.GetStatBonusAccuracyRelative((int)GetValue(attack.Owner));
		}
		return 0f;
	}

	public void AdjustDamageForUi(GameObject character, DamageInfo damage)
	{
		damage.DamageAdd(GetDamageBonusForUi(character, damage.Attack));
		damage.DamageMult(GetDamageMultiplierForUi(character, damage.Attack));
	}

	public float GetDamageBonusForUi(GameObject character, AttackBase attack)
	{
		switch (AffectsStat)
		{
		case StatusEffect.ModifiedStat.BonusUnarmedDamage:
		{
			AttackMelee attackMelee = attack as AttackMelee;
			if ((bool)attackMelee && attackMelee.Unarmed)
			{
				return GetValue(character);
			}
			break;
		}
		case StatusEffect.ModifiedStat.BonusMeleeDamage:
			if (attack is AttackMelee)
			{
				return GetValue(character);
			}
			break;
		}
		return 0f;
	}

	public float GetDamageMultiplierForUi(GameObject character, AttackBase attack)
	{
		switch (AffectsStat)
		{
		case StatusEffect.ModifiedStat.BonusDamageMult:
			return GetValue(character);
		case StatusEffect.ModifiedStat.BeamDamageMult:
			if (attack is AttackBeam)
			{
				return GetValue(character);
			}
			break;
		case StatusEffect.ModifiedStat.Might:
			return CharacterStats.GetStatDamageHealMultiplierRelative((int)GetValue(character));
		case StatusEffect.ModifiedStat.BonusMeleeWeaponDamageMult:
		case StatusEffect.ModifiedStat.BonusMeleeDamageMult:
			if (attack is AttackMelee && attack.IsAutoAttack())
			{
				return GetValue(character);
			}
			break;
		case StatusEffect.ModifiedStat.BonusRangedWeaponDamageMult:
			if (!(attack is AttackMelee))
			{
				return GetValue(character);
			}
			break;
		}
		return 1f;
	}

	public StatusEffectParams()
	{
	}

	public StatusEffectParams(Phrase.PhraseData pdata, float baseLinger)
		: this()
	{
		IsHostile = pdata.IsHostile;
		Apply = pdata.Apply;
		AffectsStat = pdata.AffectsStat;
		DmgType = pdata.DmgType;
		Value = pdata.Value;
		ExtraValue = pdata.ExtraValue;
		IntervalRate = pdata.IntervalRate;
		OnStartVisualEffect = pdata.OnStartVisualEffect;
		OnAppliedVisualEffect = pdata.OnAppliedVisualEffect;
		OnStopVisualEffect = pdata.OnStopVisualEffect;
		VisualEffectAttach = pdata.VisualEffectAttach;
		Icon = pdata.Icon;
		TrapPrefab = pdata.TrapPrefab;
		AfflictionPrefab = pdata.AfflictionPrefab;
		Persistent = true;
		Duration = baseLinger;
	}
}
