using System;
using UnityEngine;

[Serializable]
public class SpellCastData : IComparable<SpellCastData>
{
	public enum Instruction
	{
		Cast,
		UseWeapon,
		CastAtWaypoint,
		UseConsumable,
		Flee
	}

	public enum CastTarget
	{
		CurrentEnemy,
		AllyLowestStamina,
		NearestEnemy,
		NearestAlly,
		CloseAlliesCenter,
		CloseEnemiesCenter,
		Self,
		AllyUnconscious,
		PreferredAlly,
		FarthestEnemyWithinRange,
		FarthestAllyWithinRange,
		OwnAnimalCompanion
	}

	public enum ConditionalTargetType
	{
		Enemy,
		Ally,
		Self,
		AllyOrSelf
	}

	public enum ConditionType
	{
		None,
		StaminaPercentage,
		HealthPercentage,
		NumTargetsInAOE,
		InAttackRange,
		Engaged,
		Engaging,
		HasHostileEffect,
		Stunned,
		MoveRateEffect,
		AllyCount,
		EnemyCount,
		Distance,
		DifficultyIsEasy,
		DifficultyIsNormal,
		DifficultyIsHard,
		DifficultyIsNormalOrHard,
		DifficultyIsEasyOrNormal,
		Px1HighLevelScaling,
		EngagedByAnimalCompanion,
		HasHostileEffectWithDuration,
		HasSummonedWeapon,
		SummonCount,
		MoreThanOneTargetInAOEWithStaminaPercentage,
		MoreThanTwoTargetsInAOEWithStaminaPercentage,
		IsImmuneToAnyAfflictionFromAbility,
		ProneOrUnconscious,
		TimeInCombat,
		DifficultyIsStoryTime
	}

	public enum Operator
	{
		Equals,
		GreaterThan,
		LessThan
	}

	public string DebugName = "Spell";

	public Instruction CastInstruction;

	public CastTarget Target;

	public TargetPreference TargetPreference;

	[Range(0f, 100f)]
	public int CastingPriority = 50;

	public GenericAbility Spell;

	public Waypoint Waypoint;

	public Consumable Item;

	public float CooldownTime;

	public int MaxCastCount;

	public bool PerRestAbility;

	public bool IgnoreFallbackToDefaultTargetPreference;

	public ConditionalData[] Conditionals;

	private float m_cooldown;

	private int m_castCount;

	private float m_odds;

	private bool m_handledInternally;

	public bool Ready
	{
		get
		{
			if (MaxCastCount > 0 && m_castCount >= MaxCastCount)
			{
				return false;
			}
			bool flag = m_cooldown <= 0f;
			if (Spell != null)
			{
				flag = flag && Spell.ReadyIgnoreRecovery;
			}
			return flag;
		}
	}

	public bool InCooldown => m_cooldown > 0f;

	public float Odds
	{
		get
		{
			return m_odds;
		}
		set
		{
			m_odds = value;
		}
	}

	public bool HandledInternally
	{
		get
		{
			return m_handledInternally;
		}
		set
		{
			m_handledInternally = value;
		}
	}

	public int CastCount => m_castCount;

	public void SetName()
	{
		if (Spell != null)
		{
			DebugName = GenericAbility.Name(Spell);
		}
		else
		{
			DebugName = CastInstruction.ToString();
		}
	}

	public void Update()
	{
		if (m_cooldown > 0f)
		{
			m_cooldown -= Time.deltaTime;
		}
	}

	public int CompareTo(SpellCastData other)
	{
		return other.CastingPriority - CastingPriority;
	}

	public void StartTimer()
	{
		m_cooldown = CooldownTime;
	}

	public void IncrementCastCount()
	{
		m_castCount++;
	}

	public void StartNoValidTargetTimer()
	{
		if (CastInstruction != Instruction.Flee)
		{
			m_cooldown = 4f;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
