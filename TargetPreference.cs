using System;

[Serializable]
public class TargetPreference
{
	public enum TargetPreferenceType
	{
		None,
		HighestStamina,
		LowestStamina,
		LowestDamageThreshold,
		HighestDefense,
		LowestDefense,
		HighestDamageInflictor,
		CurrentEngager,
		SneakAttackVulnerable,
		LowNumberOfEngagers,
		Spellcasters,
		AfflictedBy,
		BehindAttacker,
		FastestClass,
		EngagedByAnimalCompanion,
		Engaged
	}

	public enum AllowedMovementToTargetType
	{
		MoveWithinRange,
		WillNotMove,
		BreakEngagement
	}

	public enum AfflictedByType
	{
		None,
		FrightenedOrTerrified,
		SickenedOrWeakened,
		HobbledOrStuck,
		DazedOrConfused,
		ParalyzedOrPetrified,
		CharmedOrDominated,
		ProneOrUnconscious
	}

	public TargetPreferenceType PreferenceType;

	public AllowedMovementToTargetType AllowedMovementToTarget;

	public DamagePacket.DamageType DamageType = DamagePacket.DamageType.None;

	public CharacterStats.DefenseType DefenseType = CharacterStats.DefenseType.None;

	public AfflictedByType AfflictedBy;

	public object Clone()
	{
		return MemberwiseClone();
	}
}
