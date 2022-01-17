using PrerequisiteTypeAttributes;
using UnityEngine;

public enum PrerequisiteType
{
	[Tooltip("Valid only if the target has at least the specified number of status effects with the tag.")]
	[UsesTagParam]
	[UsesValueParam]
	StatusEffectCount,
	[UsesValueParam]
	SummonsCountNotGreaterThan,
	[UsesValueParam]
	EnemiesInAttackRange,
	[UsesValueParam]
	AlliesInFriendlyRadius,
	MainTargetOnly,
	ExcludeMainTarget,
	NoAlliesInFriendlyRadius,
	ClosestAllyWithSameTarget,
	CasterIsChanting,
	[UsesValueParam]
	CasterPhraseCount,
	Friendly,
	Hostile,
	[UsesValueParam]
	StaminaPercentBelow,
	AlwaysFalse,
	UsingRangedWeapon,
	Vessel,
	UsingMeleeWeapon,
	[UsesValueParam]
	StaminaAmountAtLeast,
	[UsesValueParam]
	StaminaAmountBelow,
	FocusBelowMax,
	[UsesRaceValueParam]
	IsRace,
	HasGrimoire,
	[UsesValueParam]
	StaminaPercentAtLeast,
	EquipmentUnlocked,
	IsDragonOrDrake,
	[Tooltip("Valid if combat has been active for at least Value seconds.")]
	[UsesValueParam]
	CombatTimeAtLeast,
	[Tooltip("Valid if target is of class ClassValue.")]
	[UsesClassValueParam]
	IsClass,
	[Tooltip("Valid if the target is at least level Value.")]
	[UsesValueParam]
	LevelAtLeast,
	[Tooltip("Valid if the target is a lower level than user.")]
	LevelLowerThanUser,
	[Tooltip("Valid if the target is the same or a higher level than user.")]
	LevelHigherOrEqualToUser,
	[Tooltip("Valid if the target's CharacterStats has the keyword specified by Tag.")]
	[UsesTagParam]
	HasKeyword,
	[Tooltip("Valid only if the caster is on the map with the specified scene name.")]
	[UsesTagParam]
	IsOnMap,
	[Tooltip("Valid only if the target is a summoned creature.")]
	IsSummonedCreature,
	[Tooltip("Valid only if the target has at least the specified skill level.")]
	[UsesValueParam]
	[UsesSkillValueParam]
	SkillAtLeast,
	[Tooltip("Valid only if the target has the specified affliction.")]
	[UsesAfflictionParam]
	HasAffliction,
	[Tooltip("Valid only if the target is a backer NPC.")]
	IsBackerNpc,
	[Tooltip("Vaild only if the target is a male NPC.")]
	IsKithMale,
	[Tooltip("Vaild only if the target is a female NPC.")]
	IsKithFemale,
	[Tooltip("Valid only if the target has at least the specified number of status effects with the tag and caused by the caster.")]
	[UsesTagParam]
	[UsesValueParam]
	StatusEffectCountFromOwner,
	[Tooltip("Valid only if the target has a currently-active GenericMarker with the specified tag active on someone.")]
	[UsesTagParam]
	HasInflictedMarker
}
