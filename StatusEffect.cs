using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AI;
using AI.Achievement;
using ModifiedStatAttributes;
using Polenter.Serialization;
using UnityEngine;

[AddComponentMenu("Toolbox/Status Effect")]
[DebuggerDisplay("{GetDebuggerString()} (StatusEffect)")]
public class StatusEffect : INotifyPropertyChanged
{
	public class ModifiedStatMetadata
	{
		public bool[] UsesParameters;

		public bool NotRevoked;

		public UsageType UsageType;

		public DatabaseString DisplayString;

		public bool Obsolete;

		public string ObsoleteMessage;

		public ModifiedStatMetadata()
		{
			UsesParameters = new bool[17];
		}
	}

	public enum ModifiedStat
	{
		[Tooltip("Increases the target's maximum health by Value.")]
		[DisplayString(1102)]
		MaxHealth = 0,
		[Tooltip("Increases the target's maximum endurance by Value.")]
		[DisplayString(1103)]
		MaxStamina = 1,
		[Tooltip("Restores the target's health by Value.")]
		[DisplayString(1104)]
		[NotRevoked]
		Health = 2,
		[Tooltip("Restores the target's stamina by Value.")]
		[DisplayString(1105)]
		[NotRevoked]
		Stamina = 3,
		[Tooltip("Adds Value to the target's accuracy with Melee attacks.")]
		[DisplayString(1106)]
		MeleeAccuracy = 4,
		[Tooltip("Adds Value to the target's accuracy with Ranged attacks.")]
		[DisplayString(1107)]
		RangedAccuracy = 5,
		[Tooltip("Adds Value to the target's Deflection.")]
		[DisplayString(1108)]
		Deflection = 6,
		[Tooltip("Adds Value to the target's Fortitude.")]
		[DisplayString(1109)]
		Fortitude = 7,
		[Tooltip("Adds Value to the target's Reflex.")]
		[DisplayString(1110)]
		Reflex = 8,
		[Tooltip("Adds Value to the target's Will.")]
		[DisplayString(1111)]
		Will = 9,
		[Tooltip("Increases the target's stamina regen rate by Value.")]
		[DisplayString(1112)]
		StaminaRechargeRate = 10,
		[Tooltip("Multiplies the target's attack speed by Value.")]
		[DisplayString(1113)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		AttackSpeed = 11,
		[Tooltip("Adds Value to the target's Stealth skill.")]
		[DisplayString(1114)]
		Stealth = 12,
		[Tooltip("The target gains a Value percent (0-100) proc of the specified DmgType.")]
		[DisplayString(1115)]
		[UsesDmgTypeParam]
		BonusDamage = 13,
		[Tooltip("Adds Value to the target's DT of the specified DmgType.")]
		[DisplayString(1116)]
		[UsesDmgTypeParam]
		DamageThreshhold = 14,
		[Tooltip("Adds Value to the target's minimum damage.")]
		[DisplayString(1117)]
		DamageMinimum = 15,
		[Tooltip("Sets the target's movement speed to their base plus Value.")]
		[DisplayString(1118)]
		MovementRate = 16,
		[Tooltip("The target cannot be targeted with attacks.")]
		[DisplayString(1119)]
		[IgnoresValue]
		NonTargetable = 17,
		[Tooltip("The target cannot move.")]
		[DisplayString(1120)]
		[IgnoresValue]
		NonMobile = 18,
		[Tooltip("The target is knocked prone. NOTE: prefer the Prone affliction over this effect.")]
		[DisplayString(1121)]
		[IgnoresValue]
		KnockedDown = 19,
		[Tooltip("Adjusts the number of enemies the target can engage by Value.")]
		[DisplayString(1122)]
		EngagedEnemyCount = 20,
		[Tooltip("Increases the target's engagement range by Value.")]
		[DisplayString(1123)]
		EngagementRadius = 21,
		[Tooltip("Increases the target's accuracy on disengagement attacks by Value.")]
		[DisplayString(1124)]
		DisengagementAccuracy = 22,
		[Tooltip("The target does Value extra damage on disengagement attacks.")]
		[DisplayString(1125)]
		DisengagementDamage = 23,
		[Tooltip("The target cannot be engaged.")]
		[DisplayString(1126)]
		[IgnoresValue]
		NonEngageable = 24,
		[Tooltip("Applies Value damage of the specified DmgType directly to the target.")]
		[DisplayString(1127)]
		[UsesDmgTypeParam]
		[NotRevoked]
		Damage = 25,
		[Tooltip("The target is stunned. NOTE: prefer the Stunned affliction over this effect.")]
		[DisplayString(1128)]
		[IgnoresValue]
		Stunned = 26,
		[Tooltip("The target does Value bonus damage with Unarmed attacks.")]
		[DisplayString(1129)]
		BonusUnarmedDamage = 27,
		[Tooltip("Set's the target's attack distance multiplier for Melee attacks to value.")]
		[DisplayString(1130)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		MeleeAttackDistanceMult = 28,
		[Tooltip("Set's the target's attack distance multiplier for Ranged attacks to value.")]
		[DisplayString(1131)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		RangedAttackDistanceMult = 29,
		[Tooltip("Attack every enemy between attacker and target, when attacker is using melee.")]
		[DisplayString(1132)]
		[IgnoresValue]
		MeleeAttackAllOnPath = 30,
		[Tooltip("Attacker does bonus damage on every melee attack, with the bonus proportional to how many wounds are on him.")]
		[DisplayString(1133)]
		[UsesDmgTypeParam]
		BonusMeleeDamageFromWounds = 31,
		[Tooltip("Extra Damage Threshold from any worn armor.")]
		[DisplayString(1134)]
		BonusDTFromArmor = 32,
		[Tooltip("Adjustment to the Miss Threshold for melee attacks.")]
		[Obsolete("Hit types no longer use percent chances.")]
		MeleeMissThresholdDelta_DO_NOT_USE = 33,
		[Tooltip("Percentage of the full damage range used to increase the minimum damage of melee attacks.")]
		[DisplayString(1135)]
		MeleeDamageRangePctIncreaseToMin = 34,
		[Tooltip("The target's attacks have Value chance (0.0-1.0) to stun his enemy.")]
		[DisplayString(1136)]
		CanStun = 35,
		[Tooltip("Grants sneak attack bonuses to the target when enemy is at Value percent (0.0-1.0) or less health.")]
		[DisplayString(1137)]
		SneakAttackOnNearDead = 36,
		[Tooltip("Adjustment to the Crit Threshold for the target's melee attacks.")]
		[Obsolete("Hit types no longer use percent chances.")]
		MeleeCritThresholdDelta_DO_NOT_USE = 37,
		[Tooltip("Adjustment to the Crit Threshold for the target's ranged attacks.")]
		[Obsolete("Hit types no longer use percent chances.")]
		RangedCritThresholdDelta_DO_NOT_USE = 38,
		[Tooltip("All attacks made by the target can cripple their targets.")]
		[DisplayString(1138)]
		[IgnoresValue]
		CanCripple = 39,
		[Tooltip("Marks the target (effects of the mark are dictated by other abilities).")]
		[DisplayString(1139)]
		[IgnoresValue]
		MarkedPrey = 40,
		[Tooltip("Prevents all existing hostile effects on the target from ticking.")]
		[DisplayString(1140)]
		[IgnoresValue]
		SuspendHostileEffects = 41,
		[Tooltip("The target does Value damage with Melee attacks.")]
		[DisplayString(1141)]
		BonusMeleeDamage = 42,
		[Tooltip("The target will not stop moving when an engagement attack occurs.")]
		[DisplayString(1142)]
		[IgnoresValue]
		ImmuneToEngageStop = 43,
		[Tooltip("Multiplicative adjustment to health loss percentage.")]
		[DisplayString(1143)]
		[Obsolete]
		[ValueUsage(UsageType.ScaledMultiplier)]
		HealthLossPctMult_DO_NOT_USE = 44,
		[Tooltip("Any damage done by the target of type DmgType is multiplied by Value.")]
		[DisplayString(1115)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[UsesDmgTypeParam]
		BonusDamageMult = 45,
		[Tooltip("The target gains Value focus when they successfully hit with a weapon.")]
		[DisplayString(1145)]
		FocusWhenHits = 46,
		[Tooltip("Damage done by the target's Beam attacks is multiplied by Value.")]
		[DisplayString(1146)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BeamDamageMult = 47,
		[Tooltip("The target gains Value Resolve, the attacker gains ExtraValue Deflection.")]
		[DisplayString(1147)]
		[UsesExtraValueParam]
		DrainResolveForDeflection = 48,
		[Tooltip("Remembers damage applied to the target and does it again, multiplied by Value, to the target every interval.")]
		[DisplayString(1148)]
		ReapplyDamage = 49,
		[Tooltip("Remembers damage applied to the target and does it again, multiplied by Value, to nearby enemies every interval.")]
		[DisplayString(1149)]
		ReapplyDamageToNearbyEnemies = 50,
		[Tooltip("Multiplies the length of the target's reload animations by Value.")]
		[DisplayString(1150)]
		ReloadSpeed = 51,
		[Tooltip("The target drops a copy of the TrapPrefab every interval. Value is the maximum number of traps. If ExtraValue is non-zero, the traps are cleared with the effect.")]
		[DisplayString(1151)]
		[UsesTrapParam]
		[UsesExtraValueParam]
		DropTrap = 52,
		[Tooltip("Absorbs up to Value damage to the target, and stuns the target.")]
		[DisplayString(1152)]
		StasisShield = 53,
		[Tooltip("Prevents any existing non-hostile effects on the target from ticking.")]
		[DisplayString(1153)]
		[IgnoresValue]
		SuspendBeneficialEffects = 54,
		[Tooltip("Does Value damage of type DmgType to the target, multiplied by the ratio of its Stamina it has lost.")]
		[DisplayString(1154)]
		[UsesDmgTypeParam]
		DamageBasedOnInverseStamina = 55,
		[Tooltip("Adds Value to the target's Resolve.")]
		[DisplayString(1155)]
		Resolve = 56,
		[Tooltip("Adds Value to the target's Might.")]
		[DisplayString(1156)]
		Might = 57,
		[Tooltip("Adds Value to the target's Dexterity.")]
		[DisplayString(1157)]
		Dexterity = 58,
		[Tooltip("Adds Value to the target's Intellect.")]
		[DisplayString(1158)]
		Intellect = 59,
		[Tooltip("Summons a copy of EquippablePrefab into the target's primary weapon slot.")]
		[DisplayString(1159)]
		[IgnoresValue]
		[UsesEquippableParam]
		SummonWeapon = 60,
		[Tooltip("Reduces the target's stamina by Value and adds it to the caster.")]
		[DisplayString(1160)]
		TransferStamina = 61,
		[Tooltip("Multiplies the target's stamina recharge rate by Value.")]
		[DisplayString(1161)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		StaminaRechargeRateMult = 62,
		[Tooltip("Adds Value to the target's accuracy when he is attacking Vessels.")]
		[DisplayString(1162)]
		VesselAccuracy = 63,
		[Tooltip("Adds Value to the target's accuracy when he is attacking Beasts.")]
		[DisplayString(1163)]
		BeastAccuracy = 64,
		[Tooltip("Adds Value to the target's accuracy when he is attacking Wilders.")]
		[DisplayString(1164)]
		WilderAccuracy = 65,
		[Tooltip("The target gains Value defense against any attack that inflicts Stun.")]
		[DisplayString(1165)]
		[DesignerObsolete("Use ResistAffliction instead.")]
		StunDefense = 66,
		[Tooltip("The target gains Value defense against any attack that inflicts Knockdown.")]
		[DisplayString(1166)]
		[DesignerObsolete("Use ResistAffliction instead.")]
		KnockdownDefense = 67,
		[Tooltip("The target gains Value defense against any attack with the Poison keyword.")]
		[DisplayString(1167)]
		[DesignerObsolete("Use ResistKeyword instead.")]
		PoisonDefense = 68,
		[Tooltip("The target gains Value defense against any attack with the Disease keyword.")]
		[DisplayString(1168)]
		[DesignerObsolete("Use ResistKeyword instead.")]
		DiseaseDefense = 69,
		[Tooltip("The target gains Value to accuracy, deflection, and reflex against distant enemies.")]
		[DisplayString(1169)]
		DistantEnemyBonus = 70,
		[Tooltip("Multiplies the target's damage by Value when his target has low stamina.")]
		[DisplayString(1170)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultOnLowStaminaTarget = 71,
		[Tooltip("Increases target's critical hit chance by Value when he is attacking the same target as a friend.")]
		[DisplayString(1171)]
		BonusCritChanceOnSameEnemy = 72,
		[Tooltip("The target's nearest ally attacking the same enemy gets Value bonus accuracy.")]
		[DisplayString(1172)]
		BonusAccuracyForNearestAllyOnSameEnemy = 73,
		[Tooltip("Crits against the target have Value (0.0-1.0) chance to be converted to Hits.")]
		[DisplayString(1173)]
		EnemyCritToHitPercent = 74,
		[Tooltip("Multiplies the duration of all current and new hostile effects on the target by Value.")]
		[DisplayString(1174)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		HostileEffectDurationMult = 75,
		[Tooltip("Changes Value (0.0-1.0) percentage of Deflection and Reflex hits against the target to grazes.")]
		[DisplayString(1175)]
		EnemyDeflectReflexHitToGrazePercent = 76,
		[Tooltip("Changes Value (0.0-1.0) percentage of Fortitude and Will hits against the target to grazes.")]
		[DisplayString(1176)]
		EnemyFortitudeWillHitToGrazePercent = 77,
		[Tooltip("Adds Value bounces (in a straight line) to the target's ranged attacks that have no bounces.")]
		[DisplayString(1177)]
		ExtraStraightBounces = 78,
		[Tooltip("All damage dealt by the target is multiplied by Value and converted to a DOT.")]
		[DisplayString(1178)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		DamageToDOT = 79,
		[Tooltip("Damage dealt by the target to enemies with any DOTs is multiplied by Value.")]
		[DisplayString(1179)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultIfTargetHasDOT = 80,
		[Tooltip("Melee attacks against the target are redirected to another target near the attacker.")]
		[DisplayString(1180)]
		[IgnoresValue]
		RedirectMeleeAttacks = 81,
		[Tooltip("All damage done by the target's AOE attacks is multiplied by Value.")]
		[DisplayString(1181)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		HostileAOEDamageMultiplier = 82,
		[Tooltip("The target is allowed to sneak attack stunned, prone, or paralyzed opponents.")]
		[DisplayString(1182)]
		[IgnoresValue]
		ImprovedFlanking = 83,
		[Tooltip("Adds Value to the target's DT bypass on all attacks.")]
		[DisplayString(1183)]
		DTBypass = 84,
		[Tooltip("Caster copies up to ExtraValue GenericSpells from the target's grimoire or ability list, with spell level no higher than Value.")]
		[DisplayString(1184)]
		[UsesExtraValueParam]
		StealSpell = 85,
		[Tooltip("Makes hostiles into friendlies and vice versa (player loses control of affected characters).")]
		[DisplayString(1185)]
		[IgnoresValue]
		SwapFaction = 86,
		[Tooltip("When the target is hit by a melee attack, AttackPrefab is launched at the attacker.")]
		[IgnoresValue]
		[UsesAttackParam]
		AttackOnMeleeHit = 87,
		[Tooltip("Hostile spells of level 3 or lower cast at the target are reflected back to their caster.")]
		[DisplayString(1187)]
		[IgnoresValue]
		MinorSpellReflection = 88,
		[Tooltip("Adds Value to the target's Athletics.")]
		[DisplayString(1188)]
		Athletics = 89,
		[Tooltip("Adds Value to the target's Lore.")]
		[DisplayString(1189)]
		Lore = 90,
		[Tooltip("Adds Value to the target's Mechanics.")]
		[DisplayString(1190)]
		Mechanics = 91,
		[Tooltip("Adds Value to the target's Survival.")]
		[DisplayString(1191)]
		Survival = 92,
		[Tooltip("Adds Value to the target's Crafting.")]
		[DisplayString(1192)]
		Crafting = 93,
		[Tooltip("Adds Value to the target's defense against any attack with a Push effect.")]
		[DisplayString(1193)]
		PushDefense = 94,
		[Tooltip("Adds Value to the target's defense while he is stunned.")]
		[DisplayString(1194)]
		WhileStunnedDefense = 95,
		[Tooltip("Adds Value to the target's defense while he is knocked down.")]
		[DisplayString(1195)]
		WhileKnockeddownDefense = 96,
		[Tooltip("Adds Value to the target's accuracy when he attacks the same target as a friend.")]
		[DisplayString(1196)]
		BonusAccuracyOnSameEnemy = 97,
		[Tooltip("Damage dealt by the target to same target as a friend is multiplied by Value.")]
		[DisplayString(1197)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultOnSameEnemy = 98,
		[Tooltip("Adds Value to the target's Constitution.")]
		[DisplayString(1198)]
		Constitution = 99,
		[Tooltip("Adds Value to the target's Perception.")]
		[DisplayString(1199)]
		Perception = 100,
		[Tooltip("Adds Value to the target's critical hit damage multiplier.")]
		[DisplayString(1200)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		CritHitMultiplierBonus = 101,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon grazes to hits.")]
		[DisplayString(1201)]
		BonusGrazeToHitPercent = 102,
		[Tooltip("The target's critical hits make a Fortitude attack to inflict stun for 2 seconds.")]
		[DisplayString(1202)]
		[IgnoresValue]
		CanStunOnCrit = 103,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon grazes to misses.")]
		[DisplayString(1203)]
		BonusGrazeToMissPercent = 104,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon crits to hits.")]
		[DisplayString(1204)]
		BonusCritToHitPercent = 105,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon misses to grazes.")]
		[DisplayString(1205)]
		BonusMissToGrazePercent = 106,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon hits to crits.")]
		[DisplayString(1206)]
		BonusHitToCritPercent = 107,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's weapon hits to grazes.")]
		[DisplayString(1207)]
		BonusHitToGrazePercent = 108,
		[Tooltip("When the target hits, he inflicts a proc of Value percent (100 = 100%) with type DmgType.")]
		[DisplayString(1208)]
		[UsesDmgTypeParam]
		BonusDamageProc = 109,
		[Tooltip("The target will act randomly.")]
		[DisplayString(1209)]
		[IgnoresValue]
		Confused = 110,
		[Tooltip("Multiplies damage done by the target's melee weapon and unarmed attacks by Value.")]
		[DisplayString(1210)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusMeleeWeaponDamageMult = 111,
		[Tooltip("Multiplies damage done by the target's ranged weapon attacks by Value.")]
		[DisplayString(1211)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusRangedWeaponDamageMult = 112,
		[Tooltip("Multiplies the target's attack speed with Ranged attacks by Value.")]
		[DisplayString(1212)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		RateOfFireMult = 113,
		[Tooltip("Apply the status effects and afflictions in AttackPrefab to whatever the target hits.")]
		[DisplayString(1213)]
		[IgnoresValue]
		[UsesAttackParam]
		ApplyAttackEffects = 114,
		[Tooltip("Changes Value percentage (0.0-1.0) of Reflex attacks that graze against the target to misses.")]
		[DisplayString(1214)]
		EnemyReflexGrazeToMissPercent = 115,
		[Tooltip("The target regains Value percentage (0.0-1.0) of his stamina.")]
		[DisplayString(1215)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[NotRevoked]
		StaminaPercent = 116,
		[Tooltip("The target must be engaged by Value additional enemies to be able to be flanked.")]
		[DisplayString(1216)]
		EnemiesNeededToFlankAdj = 117,
		[Tooltip("Adds Value to the target's Concentration.")]
		[DisplayString(1217)]
		ConcentrationBonus = 118,
		[Tooltip("When the target is hit, he takes an additional Value damage of type DmgType per tick for ExtraValue seconds.")]
		[DisplayString(1218)]
		[UsesDmgTypeParam]
		[UsesExtraValueParam]
		DOTOnHit = 119,
		[Tooltip("Hostile spells of level 5 or lower cast at the target are reflected back to their caster.")]
		[DisplayString(1219)]
		[IgnoresValue]
		SpellReflection = 120,
		[Tooltip("The target cannot cast spells or switch grimoires.")]
		[DisplayString(1220)]
		[IgnoresValue]
		DisableSpellcasting = 121,
		[Tooltip("The target gains Value defense against any attack that inflicts the AfflictionPrefab. Duration of existing afflictions is reduced by ExtraValue seconds.")]
		[DisplayString(1221)]
		[UsesExtraValueParam]
		[UsesAfflictionParam]
		ResistAffliction = 122,
		[Tooltip("The target's health cannot drop below 1.")]
		[DisplayString(1222)]
		[IgnoresValue]
		PreventDeath = 123,
		[Tooltip("Changes the duration of all non-hostile effects on the target by Value.")]
		[DisplayString(1223)]
		AdjustDurationBeneficialEffects = 124,
		[Tooltip("The tick rate of DOTs on the target is multiplied by Value (higher numbers are faster).")]
		[DisplayString(1224)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		DOTTickMult = 125,
		[Tooltip("Changes the duration of all hostile effects on the target by Value.")]
		[DisplayString(1225)]
		AdjustDurationHostileEffects = 126,
		[Tooltip("The target gains Value defense against any attack with the Keyword. Duration of existing effects is reduced by ExtraValue seconds.")]
		[DisplayString(1226)]
		[UsesExtraValueParam]
		[UsesKeywordParam]
		ResistKeyword = 127,
		[Tooltip("The target loses Value DT of the specified type, and the caster gains Value DT of the specified type.")]
		[DisplayString(1227)]
		[UsesDmgTypeParam]
		TransferDT = 128,
		[Tooltip("The target loses Value from a random attribute, and the caster gains Value to the same attribute.")]
		[DisplayString(1228)]
		TransferRandomAttribute = 129,
		[Tooltip("Applies Value raw damage to the target. If it kills the target, the target is destroyed.")]
		[DisplayString(1127)]
		[NotRevoked]
		Disintegrate = 130,
		[Tooltip("The target gains Value accuracy when attacking the same target as (Code) m_extraObject.")]
		[DisplayString(1230)]
		BonusAccuracyOnSameEnemyAsExtraObject = 131,
		[Tooltip("Summons a copy of the target, equipped with the weapon in EquippablePrefab.")]
		[DisplayString(1231)]
		[IgnoresValue]
		[UsesEquippableParam]
		[Obsolete("Use a Summon attack instead.")]
		Duplicate_DEPRECATED = 132,
		[Tooltip("The target regains stamina equal to the final damage done, times Value, when he hits a victim.")]
		[DisplayString(1232)]
		GainStaminaWhenHits = 133,
		[Tooltip("The target's critical hits make a Fortitude attack to inflict knock down for Value seconds.")]
		[DisplayString(1233)]
		CanKnockDownOnCrit = 134,
		[Tooltip("The target gains Value accuracy while his stamina is below ExtraValue percent (0.0-1.0).")]
		[DisplayString(1234)]
		[UsesExtraValueParam]
		BonusAccuracyAtLowStamina = 135,
		[Tooltip("The target's damage is multiplied by Value while his stamina is below ExtraValue percent (0.0-1.0).")]
		[DisplayString(1235)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultAtLowStamina = 136,
		[Tooltip("The target's damage is multiplied by Value when attacking a target that is knocked down, stunned or flanked.")]
		[DisplayString(1236)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultOnKDSFTarget = 137,
		[Tooltip("Damage rolled by the target is also applied as a DOT of type DmgType (NONE = same type as attack), where Value is multiplier for the damage and ExtraValue is duration.")]
		[DisplayString(1237)]
		[UsesExtraValueParam]
		[UsesDmgTypeParam]
		DamagePlusDot = 138,
		[Tooltip("Ranged attacks against the target defended by Deflection or Reflex that graze are reflected back at the attacker with Value bonus accuracy.")]
		[DisplayString(1238)]
		RangedGrazeReflection = 139,
		[Tooltip("Changes Value percentage (0.0-1.0) of hits against the target to grazes.")]
		[DisplayString(1239)]
		EnemyHitToGrazePercent = 140,
		[Tooltip("Multiplies the duration of Stuns that affect the target by Value.")]
		[DisplayString(1240)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		StunDurationMult = 141,
		[Tooltip("Multiplies the duration of Knockdowns that affect the target by Value.")]
		[DisplayString(1241)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		KnockDownDurationMult = 142,
		[Tooltip("The target's Armor DT is multiplied by Value while his health is below 50%.")]
		[DisplayString(1242)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusArmorDtMultAtLowHealth = 143,
		[Tooltip("The target gains Value accuracy against enemies of RaceType.")]
		[DisplayString(1243)]
		[UsesRaceParam]
		AccuracyByRace = 144,
		[Tooltip("The target's damage against enemies of RaceType is multiplied by Value.")]
		[DisplayString(1244)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[UsesRaceParam]
		DamageMultByRace = 145,
		[Tooltip("The target's fatigue is increased by Value levels.")]
		[DisplayString(1245)]
		Fatigue = 146,
		[Tooltip("Does nothing, but is reported as Value meters of bonus weapon reach.")]
		[DisplayString(1246)]
		DUMMY_EFFECT_IncreasedWeaponReach = 147,
		[Tooltip("The target's damage against Primordials is multiplied by Value.")]
		[DisplayString(1247)]
		PrimordialAccuracy = 148,
		[Tooltip("Stops the target's animation.")]
		[DisplayString(1248)]
		[IgnoresValue]
		StopAnimation = 149,
		[Tooltip("The target is immune to AfflictionPrefab (existing effects from that affliction are cleared).")]
		[DisplayString(1249)]
		[IgnoresValue]
		[UsesAfflictionParam]
		AddAfflictionImmunity = 150,
		[Tooltip("The target is not rendered.")]
		[DisplayString(1250)]
		[IgnoresValue]
		Invisible = 151,
		[Tooltip("Adjusts the delay before Monk wounds appear on the target by Value seconds.")]
		[DisplayString(1251)]
		WoundDelay = 152,
		[Tooltip("Damage of type DmgType done by the target's spells is multiplied by Value.")]
		[DisplayString(1252)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[UsesDmgTypeParam]
		SpellDamageMult = 153,
		[Tooltip("Damage done by the target's Finishing Blows is multiplied by Value.")]
		[DisplayString(1253)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		FinishingBlowDamageMult = 154,
		[Tooltip("The area of the target's Zealous Auras is multiplied by Value.")]
		[DisplayString(1254)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		ZealousAuraAoEMult = 155,
		[Tooltip("If the target would become unconscious, it is delayed by 3 seconds.")]
		[DisplayString(1255)]
		[IgnoresValue]
		DelayUnconsciousness = 156,
		[Tooltip("Multiplies the tick rate of negative movement effects on the target by Value (higher numbers tick faster).")]
		[DisplayString(1256)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		NegMoveTickMult = 157,
		[Tooltip("The target's damage against Flanked targets is multiplied by Value.")]
		[DisplayString(1257)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultOnFlankedTarget = 158,
		[Tooltip("All focus gained by the target is multiplied by Value.")]
		[DisplayString(1258)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		FocusGainMult = 159,
		[Tooltip("The target gains Value defense against disengagement attacks.")]
		[DisplayString(1259)]
		DisengagementDefense = 160,
		[Tooltip("The target gains Value defense against spells.")]
		[DisplayString(1260)]
		SpellDefense = 161,
		[Tooltip("The target gains Value Deflection against ranged attacks.")]
		[DisplayString(1261)]
		RangedDeflection = 162,
		[Tooltip("The target's per-rest abilities with more than three uses per rest have Value extra uses.")]
		[DisplayString(1262)]
		BonusUsesPerRestPastThree = 163,
		[Tooltip("Multiplies the tick rate of Poison effects on the target by Value (higher numbers tick faster).")]
		[DisplayString(1263)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		PoisonTickMult = 164,
		[Tooltip("Multiplies the tick rate of Disease effects on the target by Value (higher numbers tick faster).")]
		[DisplayString(1264)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		DiseaseTickMult = 165,
		[Tooltip("Multiplies damage done by the target's Stalker's Link by Value.")]
		[DisplayString(1265)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		StalkersLinkDamageMult = 166,
		[Tooltip("Value percentage (0.0-1.0) of damage of type DmgType done to the target is regained as healing.")]
		[DisplayString(1266)]
		[UsesDmgTypeParam]
		DamageToStamina = 167,
		[Tooltip("The area of the target's Phrases is multiplied by Value.")]
		[DisplayString(1267)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		ChanterPhraseAoEMult = 168,
		[Tooltip("Multiplies all healing done to the target by Value.")]
		[DisplayString(2256)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusHealMult = 169,
		[Tooltip("Damage of type DmgType done to the target by critical hits is multiplied by Value.")]
		[DisplayString(1269)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[UsesDmgTypeParam]
		IncomingCritDamageMult = 170,
		[Tooltip("The target has Value extra uses of ExtraValue-level spells.")]
		[DisplayString(1270)]
		[UsesExtraValueParam]
		SpellCastBonus = 171,
		[Tooltip("Multiplies the area of the target's AoE attacks by Value.")]
		[DisplayString(1271)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		AoEMult = 172,
		[Tooltip("Multiplies the duration of any effect from Frenzy on the target by Value.")]
		[DisplayString(1272)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		FrenzyDurationMult = 173,
		[Tooltip("Multiplies the duration of any effect from Prone on the target by Value.")]
		[DisplayString(1273)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		ProneDurationMult = 174,
		[Tooltip("Multiplies the damage done by the target's Wildstrike by Value.")]
		[DisplayString(1274)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		WildstrikeDamageMult = 175,
		[Tooltip("If the target is unconscious, he revives and is healed for Value stamina.")]
		[DisplayString(1275)]
		ReviveAndAddStamina = 176,
		[Tooltip("When the target is hit for damage of type DmgType, he recieves a HoT effect for Value percentage (0.0-1.0) of the damage with duration ExtraValue seconds.")]
		[DisplayString(1276)]
		[UsesDmgTypeParam]
		[UsesExtraValueParam]
		DamageToStaminaRegen = 177,
		[Tooltip("Launches the AttackPrefab at the target.")]
		[DisplayString(1318)]
		[IgnoresValue]
		[UsesAttackParam]
		LaunchAttack = 178,
		[Tooltip("The target's current health and stamina are hidden from the player. ExtraValue changes the color (0=red, 1=purple).")]
		[DisplayString(1314)]
		[IgnoresValue]
		[UsesExtraValueParam]
		HidesHealthStamina = 179,
		[Tooltip("The target gains Value to all defenses.")]
		[DisplayString(1316)]
		AllDefense = 180,
		[Tooltip("The target's maximum stamina is multiplied by Value.")]
		[DisplayString(1317)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		MaxStaminaMult = 181,
		[Tooltip("CODE: Calls back an ability when the target is damaged. Description overrides string display.")]
		[IgnoresValue]
		CallbackOnDamaged = 182,
		[Tooltip("Damage dealt by the target when he has this effect is adjusted by his Finishing Blow ability.")]
		[DisplayString(1696)]
		[IgnoresValue]
		ApplyFinishingBlowDamage = 183,
		[Tooltip("Does nothing. Used to apply additional VFX.")]
		[IgnoresValue]
		NoEffect = 184,
		[Tooltip("Adds Value to the target's melee and ranged accuracy.")]
		[DisplayString(1404)]
		Accuracy = 185,
		[Tooltip("The target takes Value damage of the type DmgType, and the caster regains Value stamina.")]
		[DisplayString(1532)]
		[NotRevoked]
		[UsesDmgTypeParam]
		TransferDamageToStamina = 186,
		[Tooltip("CODE: Calls back the ability origin when the target finishes an attack. Description overrides string display.")]
		[IgnoresValue]
		CallbackAfterAttack = 187,
		[Tooltip("Damage dealt to the target of type DmgType is multiplied by Value.")]
		[DisplayString(1584)]
		[UsesDmgTypeParam]
		IncomingDamageMult = 188,
		[Tooltip("The player gains Value copper when a stronghold turn is processed.")]
		[DisplayString(294)]
		GivePlayerBonusMoneyViaStrongholdTurn = 189,
		[Tooltip("Adds Value to the armor speed factor of the target (which cannot exceed 1.0).")]
		[DisplayString(1585)]
		ArmorSpeedFactorAdj = 190,
		[Tooltip("The target gains Value accuracy with ranged weapons against distant enemies.")]
		[DisplayString(1694)]
		DistantEnemyWeaponAccuracyBonus = 191,
		[Tooltip("Damage dealt by the target with ranged weapons to nearby enemies is multiplied by Value.")]
		[DisplayString(1695)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusRangedWeaponCloseEnemyDamageMult = 192,
		[Tooltip("Adds Value percent (0-100) to the target's attack speed if the target is dual-wielding (weapon in each hand or unarmed), not including abilities.")]
		[DisplayString(1627)]
		DualWieldAttackSpeedPercent = 193,
		[Tooltip("While the target is using a shield, it gains Value Deflection.")]
		[DisplayString(1628)]
		BonusShieldDeflection = 194,
		[Tooltip("The target's shield Deflection bonus also applies to Reflex.")]
		[DisplayString(1629)]
		ShieldDeflectionExtendToReflex = 195,
		[Tooltip("Adds Value wounds to the target. If ExtraValue is non-zero, does an appropriate amount of raw damage to the target.")]
		[DisplayString(1671)]
		[UsesExtraValueParam]
		ApplyWounds = 196,
		[Tooltip("Damage dealt by the target with implements is multiplied by Value.")]
		[DisplayString(1672)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusDamageMultWithImplements = 197,
		[Tooltip("The target takes Value damage of type DmgType when he attacks with an implement.")]
		[DisplayString(1673)]
		[UsesDmgTypeParam]
		DamageAttackerOnImplementLaunch = 198,
		[Tooltip("Reduces the recovery penalty while moving by Value percentage (0.0-1.0) when the target is using a ranged weapon.")]
		[DisplayString(1674)]
		RangedMovingRecoveryReductionPct = 199,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's grazes to hits while he is wielding only a one-handed melee weapon.")]
		[DisplayString(1675)]
		BonusGrazeToHitRatioMeleeOneHand = 200,
		[Tooltip("The target gains Value Deflection while using a two-handed weapon.")]
		[DisplayString(1676)]
		TwoHandedDeflectionBonus = 201,
		[Tooltip("Damage dealt by the target against the specified RaceType is multiplied by Value percentage (0-100).")]
		[DisplayString(1677)]
		[UsesRaceParam]
		BonusDamageByRacePercent = 202,
		[Tooltip("The target gains Value Interrupt.")]
		[DisplayString(1776)]
		InterruptBonus = 203,
		[Tooltip("The duration of over-time potion effects on the target is multiplied by Value percent (0-100), or the value of non-over-time effects.")]
		[DisplayString(1679)]
		BonusPotionEffectOrDurationPercent = 204,
		[Tooltip("The target gains Value defense when he an his animal companion are hit by the same attack.")]
		[DisplayString(1680)]
		ExtraSimultaneousHitDefenseBonus = 205,
		[Tooltip("The target gains Value available weapon sets.")]
		[DisplayString(1681)]
		BonusWeaponSets = 206,
		[Tooltip("The target gains Value available quick item slots.")]
		[DisplayString(1682)]
		BonusQuickSlots = 207,
		[Tooltip("Multiplies the target's melee weapon attack speed by Value percent (0-100).")]
		[DisplayString(1683)]
		MeleeAttackSpeedPercent = 208,
		[Tooltip("Multiplies the target's ranged weapon attack speed by Value percent (0-100).")]
		[DisplayString(1684)]
		RangedAttackSpeedPercent = 209,
		[Tooltip("The target's traps gain Value accuracy.")]
		[DisplayString(1685)]
		TrapAccuracy = 210,
		[Tooltip("When the target deals damage of type DmgType, it is multiplied by Value percent (0-100). Doesn't support ALL or NONE.")]
		[DisplayString(1115)]
		[UsesDmgTypeParam]
		BonusDamageByTypePercent = 211,
		[Tooltip("The target's melee attacks gain Value DT bypass.")]
		[DisplayString(1686)]
		MeleeDTBypass = 212,
		[Tooltip("The target's ranged attacks gain Value DT bypass.")]
		[DisplayString(1687)]
		RangedDTBYpass = 213,
		[Tooltip("The target's grimoire cooldown is increased by Value seconds.")]
		[DisplayString(1688)]
		GrimoireCooldownBonus = 214,
		[Tooltip("The target's weapon switch cooldown is increased by Value seconds.")]
		[DisplayString(1689)]
		WeaponSwitchCooldownBonus = 215,
		[Tooltip("The durations of current afflictions of type AfflictionPrefab on the target have Value seconds added.")]
		[DisplayString(1690)]
		[UsesAfflictionParam]
		[NotRevoked]
		ShortenAfflictionDuration = 216,
		[Tooltip("The target's maximum focus is increased by Value.")]
		[DisplayString(1691)]
		MaxFocus = 217,
		[Tooltip("The target's traps have their damage or status effect duration multiplied by Value percent (0-100)")]
		[DisplayString(1692)]
		TrapBonusDamageOrDurationPercent = 218,
		[Tooltip("Converts Value percent (0-100) of the target's hits to crits against enemies with <10% stamina.")]
		[DisplayString(1693)]
		BonusHitToCritPercentEnemyBelow10Percent = 219,
		[Tooltip("The target gains Value to all defenses except Deflection.")]
		[DisplayString(1739)]
		AllDefensesExceptDeflection = 220,
		[Tooltip("Applies the PulsedAOE in AttackPrefab to the target. If Value is 0, the target becomes the owner.")]
		[UsesAttackParam]
		ApplyPulsedAOE = 221,
		[Tooltip("The target's minimum weapon damage is multiplied by Value.")]
		[DisplayString(1117)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		WeapMinDamageMult = 222,
		[Tooltip("Breaks any existing engagements with the target.")]
		[DisplayString(1789)]
		[IgnoresValue]
		BreakAllEngagement = 223,
		[Tooltip("The target gains Value Deflection, except against veil-piercing attacks.")]
		[DisplayString(1108)]
		VeilDeflection = 224,
		[Tooltip("The target's damage with two-handed melee weapons is multiplied by Value.")]
		[DisplayString(1144)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusTwoHandedMeleeWeaponDamageMult = 225,
		[Tooltip("The target's damage with melee attacks is multiplied by Value.")]
		[DisplayString(1210)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[DesignerObsolete("Use BonusMeleeWeaponDamageMult instead.")]
		BonusMeleeDamageMult = 226,
		[Tooltip("The target's critical hit damage multiplier is increased by Value against enemies with <10% stamina.")]
		[DisplayString(1952)]
		BonusCritHitMultiplierEnemyBelow10Percent = 227,
		[Tooltip("The target gains Value accuracy with Unarmed attacks.")]
		[DisplayString(1949)]
		UnarmedAccuracy = 228,
		[Tooltip("The target gains Value DT from each Monk wound he has.")]
		[DisplayString(2198)]
		BonusDTFromWounds = 229,
		[Tooltip("The target loses Value time from each beneficial effect. The total time (multiplied by ExtraValue) is spread evenly among the caster's beneficial effects.")]
		[DisplayString(2199)]
		[UsesExtraValueParam]
		TransferBeneficialTime = 230,
		[Tooltip("The target gains Value accuracy with weapons of the same Type as EquippablePrefab.")]
		[DisplayString(2200)]
		[UsesEquippableParam]
		AccuracyByWeaponType = 231,
		[Tooltip("The target shoots Value extra projectiles with weapons of the same Type as EquippablePrefab.")]
		[DisplayString(2201)]
		[UsesEquippableParam]
		ExtraProjectilesByWeaponType = 232,
		[Tooltip("Summons up to Value instances of ConsumablePrefab (0 means as many as possible) into the target's free quickslots.")]
		[DisplayString(2202)]
		[UsesConsumableParam]
		SummonConsumable = 233,
		[Tooltip("Damage dealt to the target is multiplied by Value. The difference is dealt back to the attacker with type DmgType.")]
		[DisplayString(2204)]
		[UsesDmgTypeParam]
		TransferDamageToCaster = 234,
		[Tooltip("Removes Value percent of the target's attack speed, and grants the caster an equivalent bonus.")]
		[DisplayString(2205)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		TransferAttackSpeed = 235,
		[Tooltip("The target takes Value damage of type DmgType, and if he dies, the Summon in AttackPrefab is executed.")]
		[DisplayString(1127)]
		[UsesAttackParam]
		[UsesDmgTypeParam]
		DamageToSummon = 236,
		[Tooltip("The target dies.")]
		[DisplayString(2043)]
		[IgnoresValue]
		[NotRevoked]
		Destroy = 237,
		[Tooltip("Launches the AttackPrefab on the target. It recieves Value extra accuracy and ExtraValue bonus damage each time it ticks.")]
		[DisplayString(2206)]
		[UsesExtraValueParam]
		[UsesAttackParam]
		LaunchAttackWithRollingBonus = 238,
		[Tooltip("The target can't be engaged by enemies whose Level is less than his level plus Value.")]
		[DisplayString(2152)]
		ProhibitEnemyEngagementByLevel = 239,
		[Tooltip("Absorbs up to Value points of damage of the specified Dmg Type that would be dealt to the target.")]
		[DisplayString(2260)]
		[UsesDmgTypeParam]
		DamageShield = 240,
		[Tooltip("The target regains Value percent (0.0-1.0) of his maximum health.")]
		[DisplayString(1104)]
		[NotRevoked]
		[ValueUsage(UsageType.ScaledMultiplier)]
		HealthPercent = 241,
		[Tooltip("Damage dealt by the target is also applied as a DOT of type DmgType (NONE = same type as attack), where Value is multiplier for the damage and ExtraValue is duration.")]
		[DisplayString(1237)]
		[UsesExtraValueParam]
		[UsesDmgTypeParam]
		PostDtDamagePlusDot = 242,
		[Tooltip("Ranged attacks against the target defended by Deflection or Reflex have an ExtraValue percent (0.0-1.0) chance to be reflected back at the attacker, with Value bonus accuracy.")]
		[DisplayString(2190)]
		[UsesExtraValueParam]
		RangedReflection = 243,
		[Tooltip("Adds Value to the single-weapon speed factor of the target (which cannot exceed 1.0).")]
		[DisplayString(2194)]
		SingleWeaponSpeedFactorAdj = 244,
		[Tooltip("The target is immune to effects with the specified Keyword.")]
		[DisplayString(2227)]
		[UsesKeywordParam]
		[IgnoresValue]
		KeywordImmunity = 245,
		[Tooltip("The target cannot consume or equip 'Ingestible' consumable items.")]
		[DisplayString(2228)]
		[IgnoresValue]
		CantUseFoodDrinkDrugs = 246,
		[Tooltip("The target is immune to damage of the specified Dmg Type.")]
		[DisplayString(2238)]
		[IgnoresValue]
		[UsesDmgTypeParam]
		AddDamageTypeImmunity = 247,
		[Tooltip("Negates the recovery of the target's next attack to complete.")]
		[DisplayString(2239)]
		[IgnoresValue]
		[NotRevoked]
		NegateNextRecovery = 248,
		[Tooltip("Similar to NoEffect, but is displayed on the target. Value is the display string id (GUI table). ExtraValue is maximum stack size.")]
		[UsesExtraValueParam]
		GenericMarker = 249,
		[Tooltip("Does Value damage of the specified Dmg Type to the target for each status effect on him with the Keyword. If ExtraValue is nonzero, only effects caused by the caster count.")]
		[DisplayString(2241)]
		[NotRevoked]
		[UsesDmgTypeParam]
		[UsesKeywordParam]
		[UsesExtraValueParam]
		DamageByKeywordCount = 250,
		[Tooltip("Removes all effects on the target with the specified Keyword. If ExtraValue is nonzero, only effects caused by the caster count.")]
		[DisplayString(2242)]
		[NotRevoked]
		[IgnoresValue]
		[UsesKeywordParam]
		[UsesExtraValueParam]
		RemoveAllEffectsByKeyword = 251,
		[Tooltip("Summons a copy of EquippablePrefab into the target's secondary weapon slot.")]
		[DisplayString(1159)]
		[IgnoresValue]
		[UsesEquippableParam]
		SummonSecondaryWeapon = 252,
		[Tooltip("The (Code) m_extraObject gains focus damage dealt by the target (multiplied by the Value).")]
		[DisplayString(2247)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		GrantFocusToExtraObject = 253,
		[Tooltip("The target is programmatically launched Value meters into the air.")]
		[NotRevoked]
		VerticalLaunch = 254,
		[Tooltip("Grazes against the target have Value (0.0-1.0) chance to be converted to Misses.")]
		[DisplayString(2255)]
		EnemyGrazeToMissPercent = 255,
		[Tooltip("Multiplies all healing done by the target by Value.")]
		[DisplayString(1268)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusHealingGivenMult = 256,
		[Tooltip("Restores (Value + 5 * n) stamina to the target, where n is the target's Athletics skill.")]
		[DisplayString(1105)]
		[NotRevoked]
		StaminaByAthletics = 257,
		[Tooltip("When the target hits someone with a melee attack, also hits that character with the specified AttackPrefab.")]
		[DisplayString(1318)]
		[IgnoresValue]
		[UsesAttackParam]
		AttackOnHitWithMelee = 258,
		[Tooltip("Attackers with the specified Affliction get Value bonus accuracy when attacking the target.")]
		[DisplayString(2304)]
		[UsesAfflictionParam]
		AccuracyBonusForAttackersWithAffliction = 259,
		[Tooltip("Removes all instances of the specified affliction from the target.")]
		[DisplayString(2305)]
		[IgnoresValue]
		[UsesAfflictionParam]
		[NotRevoked]
		RemoveAffliction = 260,
		[Tooltip("The target's Armor DT is multiplied by Value.")]
		[DisplayString(1134)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		BonusArmorDtMult = 261,
		[Tooltip("Grants the target the specified ability.")]
		[IgnoresValue]
		[UsesAbilityParam]
		GrantAbility = 262,
		[Tooltip("Sets the target's base attribute score in the specified Attribute to Value.")]
		[UsesAttributeParam]
		SetBaseAttribute = 263,
		[Tooltip("Sets the target's base score in the specified Defense to Value.")]
		[UsesDefenseTypeParam]
		SetBaseDefense = 264,
		[Tooltip("Effect placed on creatures affected by the Mindweb ability.")]
		MindwebEffect = 265,
		[Tooltip("The duration of the Recitation of the target's phrases is multiplied by Value (1.0 = no change).")]
		[DisplayString(2311)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		PhraseRecitationLengthMult = 266,
		[Tooltip("The target always does minimum damage when attacking a Confused, Charmed, or Dominated enemy. Additionall,y the damage is multiplied by Value.")]
		[DisplayString(2314)]
		DamageAlwaysMinimumAgainstCCD = 267,
		[Tooltip("When the target consumes drugs, the effect duration is multiplied by Value.")]
		[DisplayString(2323)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		DrugDurationMult = 268,
		[Tooltip("Reduces the casters's stamina by Value and adds it to the target. If ExtraValue>0, it's allowed to revive unconscious characters.")]
		[DisplayString(2326)]
		[UsesExtraValueParam]
		TransferStaminaReversed = 269,
		[Tooltip("The target loses Value from the specified attribute, and the caster gains Value to the same attribute.")]
		[DisplayString(2328)]
		[UsesAttributeParam]
		TransferAttribute = 270,
		[Tooltip("Restores Value uses of each of the target's Spiritshift abilities.")]
		[DisplayString(2392)]
		[NotRevoked]
		RestoreSpiritshiftUses = 271,
		[Tooltip("Applies the specified Affliction to the target with a duration of Value seconds.")]
		[UsesAfflictionParam]
		ApplyAffliction = 272,
		[Tooltip("The target gains Value accuracy against enemies of ClassType.")]
		[DisplayString(1243)]
		[UsesClassParam]
		AccuracyByClass = 273,
		[Tooltip("The target's damage against enemies of ClassType is multiplied by Value.")]
		[DisplayString(1244)]
		[ValueUsage(UsageType.ScaledMultiplier)]
		[UsesClassParam]
		DamageMultByClass = 274,
		[Tooltip("Grants immunity to up to Value applications of the specified Affliction that would be put on the target (0=infinite, but please consider AddAfflictionImmunity instead).")]
		[DisplayString(2434)]
		[UsesAfflictionParam]
		AfflictionShield = 275,
		[Tooltip("Multiplier the duration of consumable affects put on the target.")]
		[DisplayString(1659)]
		ConsumableDurationMult = 276,
		[Tooltip("The target cannot use any active abilities.")]
		[IgnoresValue]
		DisableAbilityUse = 277,
		[Tooltip("The durations of new afflictions of type AfflictionPrefab on the target have Value seconds added.")]
		[DisplayString(1690)]
		[UsesAfflictionParam]
		ShortenAfflictionDurationOngoing = 278,
		[Tooltip("Adds Value to the target's accuracy with Melee weapon (non-spell) attacks.")]
		[DisplayString(1106)]
		MeleeWeaponAccuracy = 279,
		[Tooltip("Changes Value percentage (0.0-1.0) of Reflex attacks that hit against the target to grazes.")]
		[DisplayString(2456)]
		EnemyReflexHitToGrazePercent = 280,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's hits (weapon or ability) to crits.")]
		[DisplayString(1206)]
		BonusHitToCritPercentAll = 281,
		[Tooltip("Changes Value percentage (0.0-1.0) of the target's grazes to hits while he is wielding only a one-handed melee weapon.")]
		[DisplayString(2455)]
		BonusHitToCritRatioMeleeOneHand = 282,
		[Tooltip("Restores the target's stamina by Value, ignoring any stat adjustments.")]
		[DisplayString(1105)]
		[NotRevoked]
		RawStamina = 283,
		[Tooltip("Pushes the target away from the caster up to Value meters, with speed ExtraValue.")]
		[DisplayString(1614)]
		[UsesExtraValueParam]
		Push = 10000
	}

	public struct HitTypeModifier
	{
		public ModifiedStat ModifiedStat;

		public HitType From;

		public HitType To;

		public HitTypeModifier(ModifiedStat stat, HitType from, HitType to)
		{
			ModifiedStat = stat;
			From = from;
			To = to;
		}
	}

	public enum ApplyType
	{
		ApplyOnTick,
		ApplyOverTime,
		ApplyAtEnd
	}

	public enum NonstackingType
	{
		ActiveBonus,
		ItemBonus,
		ActivePenalty,
		ItemPenalty
	}

	private static AttackBase.FormattableTarget TARGET_ATTACKONHITWITHMELEE;

	private static AttackBase.FormattableTarget TARGET_ATTACKONMELEEHIT;

	public static HitTypeModifier[] HitTypeStats;

	protected StatusEffectParams m_params;

	private List<PrerequisiteData> m_paramPrereqSerialized = new List<PrerequisiteData>();

	protected GameObject m_source;

	private Guid m_sourceSerialized = Guid.Empty;

	protected GameObject m_owner;

	protected CharacterStats m_ownerStats;

	protected GameObject m_target;

	protected CharacterStats m_targetStats;

	private Guid m_targetSerialized = Guid.Empty;

	protected int m_bundleId = -1;

	protected List<GameObject> m_appliedFX = new List<GameObject>();

	protected Dictionary<GameObject, StatusEffect> m_auraEffectApplied = new Dictionary<GameObject, StatusEffect>();

	protected Dictionary<GameObject, GameObject> m_auraTargetsApplied = new Dictionary<GameObject, GameObject>();

	private List<GameObject> m_tempAuraTargetKeys = new List<GameObject>();

	public bool m_is_checking_melee_path;

	public AttackBase[] ApplyAttackEffectsOnlyForAttacks;

	public static float SwapTeamTimerMax;

	private GameObject m_extraObject;

	private Guid m_extraObjectSerialized = Guid.Empty;

	private GenericAbility.AbilityType m_abilityType;

	private GenericAbility m_abilityOrigin;

	private Guid m_abilityOriginSerialized = Guid.Empty;

	private Equippable m_equipmentOrigin;

	private Guid m_equipmentOriginSerialized = Guid.Empty;

	private Phrase m_phraseOrigin;

	private Guid m_phraseOriginSerialized = Guid.Empty;

	private Affliction m_afflictionOrigin;

	private Guid m_afflictionOriginSerialized = Guid.Empty;

	private string m_afflictionKeyword;

	private float m_durationAfterBreak;

	private bool m_ticksAfterBreak;

	private float m_increasePerTick;

	private float m_friendlyRadius;

	private bool m_listenersAttached;

	private Guid m_ownerSerialized = Guid.Empty;

	private bool m_restored;

	private bool m_restoredMethodCalled;

	private static Dictionary<int, ModifiedStatMetadata> StatEnumMetadata;

	private string[] m_restoredFXNames;

	[Persistent]
	public List<PrerequisiteData> ParamPrereqSerialized
	{
		get
		{
			if (m_params != null && m_params.ApplicationPrerequisites != null)
			{
				m_paramPrereqSerialized.Clear();
				m_paramPrereqSerialized.AddRange(m_params.ApplicationPrerequisites);
			}
			return m_paramPrereqSerialized;
		}
		set
		{
			m_paramPrereqSerialized = value;
		}
	}

	public GameObject Source
	{
		get
		{
			return m_source;
		}
		set
		{
			m_source = value;
		}
	}

	[Persistent]
	public Guid SourceSerialized
	{
		get
		{
			if ((bool)Source)
			{
				InstanceID component = Source.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_sourceSerialized = component.Guid;
				}
			}
			return m_sourceSerialized;
		}
		set
		{
			m_sourceSerialized = value;
		}
	}

	[Persistent]
	public uint m_id { get; set; }

	[Persistent]
	public int m_stackingKey { get; set; }

	[Persistent]
	public Equippable.EquipmentSlot m_slot { get; set; }

	[Persistent]
	public bool m_applied { get; set; }

	[Persistent]
	public float m_duration { get; set; }

	[Persistent]
	public float m_durationOverride { get; set; }

	[Persistent]
	public bool m_needsDurationCalculated { get; set; }

	[Persistent]
	public float m_timeActive { get; set; }

	[Persistent]
	public bool RemovingEffect { get; set; }

	[Persistent]
	public float m_intervalTimer { get; set; }

	[Persistent]
	public uint m_intervalCount { get; set; }

	public GameObject Target => m_target;

	[Persistent]
	public Guid TargetSerialized
	{
		get
		{
			if ((bool)m_target)
			{
				InstanceID component = m_target.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_targetSerialized = component.Guid;
				}
			}
			return m_targetSerialized;
		}
		set
		{
			m_targetSerialized = value;
		}
	}

	[Persistent]
	public bool m_deleteOnClear { get; set; }

	[Persistent]
	public bool m_effect_is_on_main_target { get; set; }

	[Persistent]
	public uint m_generalCounter { get; set; }

	[Persistent]
	public int BundleId
	{
		get
		{
			return m_bundleId;
		}
		set
		{
			m_bundleId = value;
		}
	}

	public IEnumerable<GameObject> AuraEffectsAppliedCharacters => m_auraEffectApplied.Keys;

	public Dictionary<GameObject, StatusEffect> UiAuraEffectsApplied => m_auraEffectApplied;

	public float m_scale { get; set; }

	public float m_durationScale { get; set; }

	public float m_religiousScale { get; set; }

	public uint m_suspensionCount { get; set; }

	public bool m_suppressed { get; set; }

	public float m_timeApplied { get; set; }

	public int m_numRestCycles { get; set; }

	public List<GenericAbility> Spells { get; private set; }

	public List<GenericAbility> AbilitiesGrantedToTarget { get; private set; }

	public List<Trap> Traps { get; private set; }

	private Team m_cachedTeam { get; set; }

	public Team CachedTeam => m_cachedTeam;

	private float m_swapTeamTimer { get; set; }

	[ExcludeFromSerialization]
	public GameObject ExtraObject
	{
		get
		{
			return m_extraObject;
		}
		set
		{
			m_extraObject = value;
		}
	}

	[Persistent]
	public Guid ExtraObjectSerialized
	{
		get
		{
			if ((bool)m_extraObject)
			{
				InstanceID component = m_extraObject.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_extraObjectSerialized = component.Guid;
				}
			}
			return m_extraObjectSerialized;
		}
		set
		{
			m_extraObjectSerialized = value;
		}
	}

	public GenericAbility.AbilityType AbilityType
	{
		get
		{
			return m_abilityType;
		}
		set
		{
			m_abilityType = value;
		}
	}

	[ExcludeFromSerialization]
	public GenericAbility AbilityOrigin
	{
		get
		{
			return m_abilityOrigin;
		}
		set
		{
			m_abilityOrigin = value;
		}
	}

	[Persistent]
	public Guid AbilityOriginSerialized
	{
		get
		{
			if ((bool)AbilityOrigin)
			{
				InstanceID component = AbilityOrigin.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_abilityOriginSerialized = component.Guid;
				}
			}
			return m_abilityOriginSerialized;
		}
		set
		{
			m_abilityOriginSerialized = value;
		}
	}

	public Equippable EquipmentOrigin
	{
		get
		{
			return m_equipmentOrigin;
		}
		set
		{
			m_equipmentOrigin = value;
		}
	}

	[Persistent]
	public Guid EquipmentOriginSerialized
	{
		get
		{
			if ((bool)EquipmentOrigin)
			{
				InstanceID component = EquipmentOrigin.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_equipmentOriginSerialized = component.Guid;
				}
			}
			return m_equipmentOriginSerialized;
		}
		set
		{
			m_equipmentOriginSerialized = value;
		}
	}

	public Phrase PhraseOrigin
	{
		get
		{
			return m_phraseOrigin;
		}
		set
		{
			m_phraseOrigin = value;
		}
	}

	[Persistent]
	public Guid PhraseOriginSerialized
	{
		get
		{
			if ((bool)PhraseOrigin)
			{
				InstanceID component = PhraseOrigin.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_phraseOriginSerialized = component.Guid;
				}
			}
			return m_phraseOriginSerialized;
		}
		set
		{
			m_phraseOriginSerialized = value;
		}
	}

	public Affliction AfflictionOrigin
	{
		get
		{
			return m_afflictionOrigin;
		}
		set
		{
			m_afflictionOrigin = value;
		}
	}

	[Persistent]
	public Guid AfflictionOriginSerialized
	{
		get
		{
			if ((bool)AfflictionOrigin)
			{
				InstanceID component = AfflictionOrigin.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_afflictionOriginSerialized = component.Guid;
				}
			}
			return m_afflictionOriginSerialized;
		}
		set
		{
			m_afflictionOriginSerialized = value;
		}
	}

	public string AfflictionKeyword
	{
		get
		{
			return m_afflictionKeyword;
		}
		set
		{
			m_afflictionKeyword = value.Trim().ToLowerInvariant();
		}
	}

	public MonoBehaviour Origin
	{
		get
		{
			if ((bool)EquipmentOrigin)
			{
				return EquipmentOrigin;
			}
			if ((bool)PhraseOrigin)
			{
				return PhraseOrigin;
			}
			if ((bool)AfflictionOrigin)
			{
				return AfflictionOrigin;
			}
			if ((bool)AbilityOrigin)
			{
				return AbilityOrigin;
			}
			return m_ownerStats;
		}
	}

	public float m_damageToReapply { get; set; }

	public float m_damageToAbsorb { get; set; }

	public int m_triggerCount { get; set; }

	private bool m_triggerCallbackSet { get; set; }

	private bool m_visualEffectsAttached { get; set; }

	public float DurationAfterBreak
	{
		get
		{
			return m_durationAfterBreak;
		}
		set
		{
			m_durationAfterBreak = value;
		}
	}

	public bool TicksAfterBreak
	{
		get
		{
			return m_ticksAfterBreak;
		}
		set
		{
			m_ticksAfterBreak = value;
		}
	}

	public float IncreasePerTick
	{
		get
		{
			return m_increasePerTick;
		}
		set
		{
			m_increasePerTick = value;
		}
	}

	public float Interval => GetInterval(this);

	public float AdjustedFriendlyRadius
	{
		get
		{
			float num = m_friendlyRadius;
			if (Owner != null)
			{
				CharacterStats component = Owner.GetComponent<CharacterStats>();
				if (component != null)
				{
					num *= component.StatEffectRadiusMultiplier;
				}
			}
			if (m_abilityOrigin != null)
			{
				num *= m_abilityOrigin.RadiusMultiplier;
			}
			if (num > 0f && m_triggerCallbackSet && m_triggerCount > 0)
			{
				num += m_params.TriggerAdjustment.RadiusAdjustment * (float)m_triggerCount;
			}
			return num;
		}
	}

	public float FriendlyRadius
	{
		get
		{
			return m_friendlyRadius;
		}
		set
		{
			m_friendlyRadius = value;
		}
	}

	public bool IsAura => m_friendlyRadius > 0f;

	public bool IsFromAura { get; set; }

	[Persistent]
	public bool m_forceStackable { get; set; }

	[ExcludeFromSerialization]
	public GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
			if ((bool)m_owner)
			{
				m_ownerStats = m_owner.GetComponent<CharacterStats>();
			}
		}
	}

	[Persistent]
	public Guid OwnerSerialized
	{
		get
		{
			if ((bool)Owner)
			{
				InstanceID component = Owner.GetComponent<InstanceID>();
				if ((bool)component)
				{
					m_ownerSerialized = component.Guid;
				}
			}
			return m_ownerSerialized;
		}
		set
		{
			m_ownerSerialized = value;
		}
	}

	public bool Stackable
	{
		get
		{
			if (Params.AffectsStat == ModifiedStat.SummonConsumable)
			{
				return true;
			}
			if (Params.AffectsStat == ModifiedStat.NoEffect)
			{
				return true;
			}
			if (m_forceStackable)
			{
				return true;
			}
			if (IsDamageDealing)
			{
				return true;
			}
			if (m_abilityOrigin != null && m_abilityOrigin.Passive && !(m_abilityOrigin is TriggeredOnKillAbility))
			{
				return true;
			}
			if (AbilityType == GenericAbility.AbilityType.WeaponOrShield || AbilityType == GenericAbility.AbilityType.Talent)
			{
				return true;
			}
			if (AfflictionOrigin != null && AfflictionOrigin.FromResting)
			{
				return true;
			}
			return false;
		}
	}

	public bool Exclusive
	{
		get
		{
			if (Params.AffectsStat == ModifiedStat.SummonWeapon || Params.AffectsStat == ModifiedStat.StasisShield || Params.AffectsStat == ModifiedStat.SwapFaction || Params.AffectsStat == ModifiedStat.SummonSecondaryWeapon)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsSuspended => m_suspensionCount != 0;

	public bool IsSuppressed => m_suppressed;

	public bool Applied
	{
		[DebuggerStepThrough]
		get
		{
			return m_applied;
		}
		set
		{
			m_applied = value;
		}
	}

	public bool HasBeenApplied
	{
		get
		{
			if (!(m_timeActive > 0f))
			{
				if (m_applied)
				{
					return Duration == 0f;
				}
				return false;
			}
			return true;
		}
	}

	public bool Expired
	{
		get
		{
			if (Duration == 0f || TimeLeft > 0f || DurationAfterBreak > 0f)
			{
				return false;
			}
			return true;
		}
	}

	public float TimeActive => m_timeActive;

	public float TimeApplied
	{
		[DebuggerStepThrough]
		get
		{
			return m_timeApplied;
		}
	}

	public Equippable.EquipmentSlot Slot
	{
		get
		{
			return m_slot;
		}
		set
		{
			m_slot = value;
		}
	}

	[Persistent]
	public StatusEffectParams Params
	{
		[DebuggerStepThrough]
		get
		{
			return m_params;
		}
		set
		{
			m_params = value;
			if (m_params != null && m_params.OnAppliedVisualEffect != null && m_target != null)
			{
				ApplyLoopingEffect(m_params.OnAppliedVisualEffect);
			}
		}
	}

	public float CurrentAppliedValue => GetCurrentAppliedValue(ParamsValue());

	public float CurrentAppliedValueForUi
	{
		get
		{
			if (!Params.IsCleanedUp)
			{
				return CurrentAppliedValue;
			}
			return GetCurrentAppliedValue(Params.MergedValue);
		}
	}

	public bool HasTriggerActivation
	{
		get
		{
			if (StatNotRevokedParam(Params.AffectsStat))
			{
				return false;
			}
			if (Params.TriggerAdjustment.Ineffective)
			{
				return false;
			}
			if (UsesValueParam(Params.AffectsStat) && ((IsScaledMultiplier && ParamsValue() == 1f) || (!IsScaledMultiplier && ParamsValue() == 0f)))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsTriggerActivated
	{
		get
		{
			if (!HasTriggerActivation)
			{
				return false;
			}
			if ((IsScaledMultiplier && CurrentAppliedValue != 1f) || (!IsScaledMultiplier && CurrentAppliedValue != 0f))
			{
				return true;
			}
			return false;
		}
	}

	public bool HideBecauseUntriggered
	{
		get
		{
			if ((bool)AbilityOrigin && AbilityOrigin.Passive && HasTriggerActivation)
			{
				return !IsTriggerActivated;
			}
			return false;
		}
	}

	public bool AppliedTriggered
	{
		get
		{
			if (Applied)
			{
				if (HasTriggerActivation)
				{
					return IsTriggerActivated;
				}
				return true;
			}
			return false;
		}
	}

	[Persistent]
	public float Duration
	{
		get
		{
			return m_duration + TemporaryDurationAdjustment;
		}
		set
		{
			m_duration = value;
			if (!Applied)
			{
				return;
			}
			if (Params.AffectsStat == ModifiedStat.Stunned)
			{
				UpdateAiStateDuration<Stunned>();
			}
			else if (Params.AffectsStat == ModifiedStat.KnockedDown)
			{
				UpdateAiStateDuration<KnockedDown>();
			}
			else if (Params.AffectsStat == ModifiedStat.StasisShield)
			{
				UpdateAiStateDuration<Stunned>();
			}
			else if (Params.AffectsStat == ModifiedStat.Confused)
			{
				Confusion confusion = (Target ? Target.GetComponent<Confusion>() : null);
				if ((bool)confusion)
				{
					confusion.Duration = Duration;
				}
			}
			else if (Params.AffectsStat == ModifiedStat.StopAnimation)
			{
				UpdateAiStateDuration<Paralyzed>();
			}
		}
	}

	public float TemporaryDurationAdjustment { get; set; }

	[Persistent]
	public float UnadjustedDurationAdd { get; set; }

	public bool LastsUntilCombatEnds => Params.LastsUntilCombatEnds;

	public bool LastsUntilRest => Params.LastsUntilRest;

	public float TimeLeft
	{
		get
		{
			if (Duration == 0f)
			{
				return 0f;
			}
			return Duration - m_timeActive;
		}
	}

	public float Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			m_scale = value;
		}
	}

	public string HitStrength
	{
		get
		{
			if (Scale < 1f)
			{
				return GUIUtils.GetText(55);
			}
			if (Scale > 1f)
			{
				return GUIUtils.GetText(56);
			}
			return "";
		}
	}

	public NonstackingType NonstackingEffectType
	{
		get
		{
			bool flag = false;
			bool flag2 = false;
			if (m_abilityOrigin != null && !m_abilityOrigin.Passive)
			{
				flag = true;
			}
			if (m_phraseOrigin != null)
			{
				flag = true;
			}
			if (IsScaledMultiplier)
			{
				if (ParamsValue() >= 1f)
				{
					flag2 = true;
				}
			}
			else if (ParamsValue() >= 0f)
			{
				flag2 = true;
			}
			if (flag)
			{
				if (flag2)
				{
					return NonstackingType.ActiveBonus;
				}
				return NonstackingType.ActivePenalty;
			}
			if (flag2)
			{
				return NonstackingType.ItemBonus;
			}
			return NonstackingType.ItemPenalty;
		}
	}

	public bool IsDamageDealing
	{
		get
		{
			if (Params.Value < 0f && (Params.AffectsStat == ModifiedStat.Health || Params.AffectsStat == ModifiedStat.Stamina || Params.AffectsStat == ModifiedStat.RawStamina || Params.AffectsStat == ModifiedStat.StaminaPercent || Params.AffectsStat == ModifiedStat.HealthPercent))
			{
				return true;
			}
			if (Params.Value > 0f && (Params.AffectsStat == ModifiedStat.Damage || Params.AffectsStat == ModifiedStat.Disintegrate || Params.AffectsStat == ModifiedStat.DamageToSummon))
			{
				return true;
			}
			if (Params.AffectsStat != ModifiedStat.TransferStamina)
			{
				return Params.AffectsStat == ModifiedStat.TransferDamageToStamina;
			}
			return true;
		}
	}

	public bool IsOverTime
	{
		get
		{
			if (Interval > 0f && !IsAura)
			{
				if (Params.Apply != 0)
				{
					return Params.Apply == ApplyType.ApplyOverTime;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsDOT
	{
		get
		{
			if (IsOverTime)
			{
				return IsDamageDealing;
			}
			return false;
		}
	}

	public bool IsScaledMultiplier => IsScaledMultiplierStatic(Params.AffectsStat);

	public bool IsNegativeMovementEffect
	{
		get
		{
			if (ParamsValue() < 0f && Params.AffectsStat == ModifiedStat.MovementRate)
			{
				return true;
			}
			if (Params.AffectsStat == ModifiedStat.NonMobile || Params.AffectsStat == ModifiedStat.StopAnimation)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsRecoveryTimePausingEffect
	{
		get
		{
			if (Params.AffectsStat == ModifiedStat.Stunned || Params.AffectsStat == ModifiedStat.KnockedDown || Params.AffectsStat == ModifiedStat.StopAnimation)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsPoisonEffect
	{
		get
		{
			if (m_abilityOrigin != null && m_abilityOrigin.Attack != null && m_abilityOrigin.Attack.HasKeyword("poison"))
			{
				return true;
			}
			if (m_afflictionOrigin != null && m_afflictionKeyword != null && m_afflictionKeyword.Equals("poison", StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsDiseaseEffect
	{
		get
		{
			if (m_abilityOrigin != null && m_abilityOrigin.Attack != null && m_abilityOrigin.Attack.HasKeyword("disease"))
			{
				return true;
			}
			if (m_afflictionOrigin != null && m_afflictionKeyword != null && m_afflictionKeyword.Equals("disease", StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}
	}

	[Persistent]
	public string[] AppliedFXNames
	{
		get
		{
			m_restoredFXNames = new string[m_appliedFX.Count];
			for (int i = 0; i < m_appliedFX.Count; i++)
			{
				m_restoredFXNames[i] = m_appliedFX[i].name.Replace("(Clone)", "");
			}
			return m_restoredFXNames;
		}
		set
		{
			m_restoredFXNames = value;
		}
	}

	public HashSet<GameObject> ForcedAuraTargets => null;

	[Obsolete]
	public bool DeleteOnClear
	{
		get
		{
			return m_deleteOnClear;
		}
		set
		{
			m_deleteOnClear = value;
		}
	}

	public uint EffectID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public string BundleName
	{
		get
		{
			if (AfflictionOrigin != null)
			{
				return AfflictionOrigin.DisplayName.ToString();
			}
			if (AbilityOrigin != null)
			{
				return AbilityOrigin.Name();
			}
			if (EquipmentOrigin != null)
			{
				return EquipmentOrigin.Name;
			}
			if (PhraseOrigin != null)
			{
				return PhraseOrigin.DisplayName.ToString();
			}
			return null;
		}
	}

	[Persistent]
	public StatusEffectTrigger.TriggerType TriggerType
	{
		get
		{
			return Params.TriggerAdjustment.Type;
		}
		set
		{
			Params.TriggerAdjustment.Type = value;
		}
	}

	[Persistent]
	public float TriggerValue
	{
		get
		{
			return Params.TriggerAdjustment.TriggerValue;
		}
		set
		{
			Params.TriggerAdjustment.TriggerValue = value;
		}
	}

	[Persistent]
	public float TriggerValueAdjustment
	{
		get
		{
			return Params.TriggerAdjustment.ValueAdjustment;
		}
		set
		{
			Params.TriggerAdjustment.ValueAdjustment = value;
		}
	}

	[Persistent]
	public float TriggerDurationAdjustment
	{
		get
		{
			return Params.TriggerAdjustment.DurationAdjustment;
		}
		set
		{
			Params.TriggerAdjustment.DurationAdjustment = value;
		}
	}

	[Persistent]
	public float TriggerRadiusAdjustment
	{
		get
		{
			return Params.TriggerAdjustment.RadiusAdjustment;
		}
		set
		{
			Params.TriggerAdjustment.RadiusAdjustment = value;
		}
	}

	[Persistent]
	public int TriggerMaxTriggerCount
	{
		get
		{
			return Params.TriggerAdjustment.MaxTriggerCount;
		}
		set
		{
			Params.TriggerAdjustment.MaxTriggerCount = value;
		}
	}

	[Persistent]
	public bool TriggerRemoveEffectAtMax
	{
		get
		{
			return Params.TriggerAdjustment.RemoveEffectAtMax;
		}
		set
		{
			Params.TriggerAdjustment.RemoveEffectAtMax = value;
		}
	}

	[Persistent]
	public bool TriggerResetTriggerOnEffectPulse
	{
		get
		{
			return Params.TriggerAdjustment.ResetTriggerOnEffectPulse;
		}
		set
		{
			Params.TriggerAdjustment.ResetTriggerOnEffectPulse = value;
		}
	}

	[Persistent]
	public bool TriggerResetTriggerOnEffectEnd
	{
		get
		{
			return Params.TriggerAdjustment.ResetTriggerOnEffectEnd;
		}
		set
		{
			Params.TriggerAdjustment.ResetTriggerOnEffectEnd = value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public string GetDebuggerString()
	{
		if ((bool)Origin)
		{
			return Origin.name + "->" + Params.GetDebuggerString();
		}
		return "?->" + Params.GetDebuggerString();
	}

	static StatusEffect()
	{
		TARGET_ATTACKONHITWITHMELEE = new AttackBase.FormattableTarget(2308, 2306);
		TARGET_ATTACKONMELEEHIT = new AttackBase.FormattableTarget(2309, 2307);
		HitTypeStats = new HitTypeModifier[16]
		{
			new HitTypeModifier(ModifiedStat.EnemyCritToHitPercent, HitType.CRIT, HitType.HIT),
			new HitTypeModifier(ModifiedStat.EnemyHitToGrazePercent, HitType.HIT, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.EnemyReflexGrazeToMissPercent, HitType.GRAZE, HitType.MISS),
			new HitTypeModifier(ModifiedStat.EnemyReflexHitToGrazePercent, HitType.HIT, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.EnemyDeflectReflexHitToGrazePercent, HitType.HIT, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.EnemyFortitudeWillHitToGrazePercent, HitType.HIT, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.BonusGrazeToHitPercent, HitType.GRAZE, HitType.HIT),
			new HitTypeModifier(ModifiedStat.BonusCritToHitPercent, HitType.CRIT, HitType.HIT),
			new HitTypeModifier(ModifiedStat.BonusMissToGrazePercent, HitType.MISS, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.BonusHitToCritPercent, HitType.HIT, HitType.CRIT),
			new HitTypeModifier(ModifiedStat.BonusHitToCritPercentAll, HitType.HIT, HitType.CRIT),
			new HitTypeModifier(ModifiedStat.BonusHitToGrazePercent, HitType.HIT, HitType.GRAZE),
			new HitTypeModifier(ModifiedStat.BonusGrazeToHitRatioMeleeOneHand, HitType.GRAZE, HitType.HIT),
			new HitTypeModifier(ModifiedStat.BonusHitToCritRatioMeleeOneHand, HitType.HIT, HitType.CRIT),
			new HitTypeModifier(ModifiedStat.BonusHitToCritPercentEnemyBelow10Percent, HitType.HIT, HitType.CRIT),
			new HitTypeModifier(ModifiedStat.EnemyGrazeToMissPercent, HitType.GRAZE, HitType.MISS)
		};
		SwapTeamTimerMax = 2f;
		Type typeFromHandle = typeof(ModifiedStat);
		Array values = Enum.GetValues(typeFromHandle);
		StatEnumMetadata = new Dictionary<int, ModifiedStatMetadata>();
		for (int i = 0; i < values.Length; i++)
		{
			ModifiedStatMetadata modifiedStatMetadata = new ModifiedStatMetadata();
			MemberInfo[] member = typeFromHandle.GetMember(values.GetValue(i).ToString());
			modifiedStatMetadata.UsesParameters[2] = member.First().GetCustomAttributes(typeof(UsesDmgTypeParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[3] = !member.First().GetCustomAttributes(typeof(IgnoresValueAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[4] = member.First().GetCustomAttributes(typeof(UsesExtraValueParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[5] = member.First().GetCustomAttributes(typeof(UsesTrapParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[6] = member.First().GetCustomAttributes(typeof(UsesEquippableParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[7] = member.First().GetCustomAttributes(typeof(UsesConsumableParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[8] = member.First().GetCustomAttributes(typeof(UsesAttackParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[13] = member.First().GetCustomAttributes(typeof(UsesAbilityParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[9] = member.First().GetCustomAttributes(typeof(UsesAfflictionParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[10] = member.First().GetCustomAttributes(typeof(UsesRaceParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[14] = member.First().GetCustomAttributes(typeof(UsesAttributeParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[15] = member.First().GetCustomAttributes(typeof(UsesDefenseTypeParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[11] = member.First().GetCustomAttributes(typeof(UsesKeywordParamAttribute), inherit: false).Any();
			modifiedStatMetadata.UsesParameters[16] = member.First().GetCustomAttributes(typeof(UsesClassParamAttribute), inherit: false).Any();
			modifiedStatMetadata.NotRevoked = member.First().GetCustomAttributes(typeof(NotRevokedAttribute), inherit: false).Any();
			if (member.First().GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).FirstOrDefault() is ObsoleteAttribute obsoleteAttribute)
			{
				modifiedStatMetadata.Obsolete = true;
				modifiedStatMetadata.ObsoleteMessage = obsoleteAttribute.Message;
			}
			else if (member.First().GetCustomAttributes(typeof(DesignerObsoleteAttribute), inherit: false).FirstOrDefault() is DesignerObsoleteAttribute designerObsoleteAttribute)
			{
				modifiedStatMetadata.Obsolete = true;
				modifiedStatMetadata.ObsoleteMessage = designerObsoleteAttribute.Message;
			}
			object[] customAttributes = member.First().GetCustomAttributes(typeof(DisplayStringAttribute), inherit: false);
			if (customAttributes.Length != 0 && customAttributes[0] is DisplayStringAttribute displayStringAttribute)
			{
				modifiedStatMetadata.DisplayString = displayStringAttribute.String;
			}
			object[] customAttributes2 = member.First().GetCustomAttributes(typeof(ValueUsageAttribute), inherit: false);
			if (customAttributes2.Any())
			{
				modifiedStatMetadata.UsageType = (customAttributes2.First() as ValueUsageAttribute).m_Usage;
			}
			StatEnumMetadata[(int)values.GetValue(i)] = modifiedStatMetadata;
		}
	}

	public static bool ModifiedStatHasSpecialTargetString(ModifiedStat stat)
	{
		if (stat != ModifiedStat.AttackOnHitWithMelee && stat != ModifiedStat.ApplyAttackEffects)
		{
			return stat == ModifiedStat.AttackOnMeleeHit;
		}
		return true;
	}

	public static AttackBase.FormattableTarget GetModifiedStatSpecialTargetString(ModifiedStat stat)
	{
		switch (stat)
		{
		case ModifiedStat.ApplyAttackEffects:
		case ModifiedStat.AttackOnHitWithMelee:
			return TARGET_ATTACKONHITWITHMELEE;
		case ModifiedStat.AttackOnMeleeHit:
			return TARGET_ATTACKONMELEEHIT;
		default:
			return null;
		}
	}

	public static ModifiedStat AttributeTypeToModifiedStat(CharacterStats.AttributeScoreType type)
	{
		return type switch
		{
			CharacterStats.AttributeScoreType.Constitution => ModifiedStat.Constitution, 
			CharacterStats.AttributeScoreType.Dexterity => ModifiedStat.Dexterity, 
			CharacterStats.AttributeScoreType.Intellect => ModifiedStat.Intellect, 
			CharacterStats.AttributeScoreType.Might => ModifiedStat.Might, 
			CharacterStats.AttributeScoreType.Perception => ModifiedStat.Perception, 
			CharacterStats.AttributeScoreType.Resolve => ModifiedStat.Resolve, 
			_ => ModifiedStat.NoEffect, 
		};
	}

	public static ModifiedStat SkillTypeToModifiedStat(CharacterStats.SkillType skill)
	{
		return skill switch
		{
			CharacterStats.SkillType.Athletics => ModifiedStat.Athletics, 
			CharacterStats.SkillType.Crafting => ModifiedStat.Crafting, 
			CharacterStats.SkillType.Lore => ModifiedStat.Lore, 
			CharacterStats.SkillType.Mechanics => ModifiedStat.Mechanics, 
			CharacterStats.SkillType.Stealth => ModifiedStat.Stealth, 
			CharacterStats.SkillType.Survival => ModifiedStat.Survival, 
			_ => ModifiedStat.NoEffect, 
		};
	}

	public static CharacterStats.AttributeScoreType ModifiedStatToAttributeScoreType(ModifiedStat stat)
	{
		return stat switch
		{
			ModifiedStat.Constitution => CharacterStats.AttributeScoreType.Constitution, 
			ModifiedStat.Dexterity => CharacterStats.AttributeScoreType.Dexterity, 
			ModifiedStat.Intellect => CharacterStats.AttributeScoreType.Intellect, 
			ModifiedStat.Might => CharacterStats.AttributeScoreType.Might, 
			ModifiedStat.Perception => CharacterStats.AttributeScoreType.Perception, 
			ModifiedStat.Resolve => CharacterStats.AttributeScoreType.Resolve, 
			_ => CharacterStats.AttributeScoreType.Count, 
		};
	}

	public static CharacterStats.SkillType ModifiedStatToSkillType(ModifiedStat stat)
	{
		return stat switch
		{
			ModifiedStat.Athletics => CharacterStats.SkillType.Athletics, 
			ModifiedStat.Crafting => CharacterStats.SkillType.Crafting, 
			ModifiedStat.Lore => CharacterStats.SkillType.Lore, 
			ModifiedStat.Mechanics => CharacterStats.SkillType.Mechanics, 
			ModifiedStat.Stealth => CharacterStats.SkillType.Stealth, 
			ModifiedStat.Survival => CharacterStats.SkillType.Survival, 
			_ => CharacterStats.SkillType.Count, 
		};
	}

	public static CharacterStats.DefenseType ModifiedStatToDefenseType(ModifiedStat stat)
	{
		return stat switch
		{
			ModifiedStat.Deflection => CharacterStats.DefenseType.Deflect, 
			ModifiedStat.Reflex => CharacterStats.DefenseType.Reflex, 
			ModifiedStat.Fortitude => CharacterStats.DefenseType.Fortitude, 
			ModifiedStat.Will => CharacterStats.DefenseType.Will, 
			_ => CharacterStats.DefenseType.None, 
		};
	}

	public static ModifiedStat DefenseTypeToModifiedStat(CharacterStats.DefenseType defense)
	{
		return defense switch
		{
			CharacterStats.DefenseType.Deflect => ModifiedStat.Deflection, 
			CharacterStats.DefenseType.Reflex => ModifiedStat.Reflex, 
			CharacterStats.DefenseType.Fortitude => ModifiedStat.Fortitude, 
			CharacterStats.DefenseType.Will => ModifiedStat.Will, 
			_ => ModifiedStat.NoEffect, 
		};
	}

	public StatusEffect()
	{
		m_generalCounter = (uint)OEIRandom.Range(0, 5);
		m_scale = 1f;
		m_religiousScale = 1f;
		m_durationScale = 1f;
		m_slot = Equippable.EquipmentSlot.None;
		m_needsDurationCalculated = true;
	}

	public bool EqualsExceptValues(StatusEffect other)
	{
		if (Params.EqualsExceptValues(other.Params))
		{
			return Origin == other.Origin;
		}
		return false;
	}

	public bool AnyOriginIs(MonoBehaviour mono)
	{
		if (!(mono == EquipmentOrigin) && !(mono == PhraseOrigin) && !(mono == AfflictionOrigin))
		{
			return mono == AbilityOrigin;
		}
		return true;
	}

	public static float GetInterval(StatusEffect Effect)
	{
		if (AttackData.Instance != null)
		{
			if (Effect.Params.IntervalRate == StatusEffectParams.IntervalRateType.Damage)
			{
				return AttackData.Instance.DamageIntervalRate;
			}
			if (Effect.Params.IntervalRate == StatusEffectParams.IntervalRateType.Hazard)
			{
				return AttackData.Instance.HazardIntervalRate;
			}
			if (Effect.Params.IntervalRate == StatusEffectParams.IntervalRateType.Footstep)
			{
				return 0.33f;
			}
			if (Effect.IsAura)
			{
				return 1f;
			}
		}
		return 0f;
	}

	public static float GetInterval(StatusEffectParams Params)
	{
		if (AttackData.Instance != null)
		{
			if (Params.IntervalRate == StatusEffectParams.IntervalRateType.Damage)
			{
				return AttackData.Instance.DamageIntervalRate;
			}
			if (Params.IntervalRate == StatusEffectParams.IntervalRateType.Hazard)
			{
				return AttackData.Instance.HazardIntervalRate;
			}
			if (Params.IntervalRate == StatusEffectParams.IntervalRateType.Footstep)
			{
				return 0.33f;
			}
		}
		return 0f;
	}

	public float DotExpectedDamage(GameObject target)
	{
		Health component = target.GetComponent<Health>();
		if (Params.AffectsStat == ModifiedStat.Damage || Params.AffectsStat == ModifiedStat.DamageBasedOnInverseStamina || Params.AffectsStat == ModifiedStat.Disintegrate || Params.AffectsStat == ModifiedStat.DamageToSummon)
		{
			float num = 1f;
			if (Params.AffectsStat == ModifiedStat.DamageBasedOnInverseStamina)
			{
				num = 2f - component.CurrentStamina / component.MaxStamina;
			}
			long num2 = Mathf.FloorToInt(TimeLeft / Interval) - m_intervalCount;
			if (IncreasePerTick > 0f)
			{
				return ParamsValue() * num * ((float)(num2 * (num2 + 1)) / 2f) * IncreasePerTick;
			}
			return ParamsValue() * num * (float)num2;
		}
		return 0f;
	}

	public static List<List<StatusEffect>> FilterAfflictions(List<StatusEffect> effectList)
	{
		List<List<StatusEffect>> list = new List<List<StatusEffect>>();
		for (int num = effectList.Count - 1; num >= 0; num--)
		{
			StatusEffect statusEffect = effectList[num];
			if ((bool)statusEffect.AfflictionOrigin)
			{
				List<StatusEffect> list2 = null;
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i][0].AfflictionOrigin == statusEffect.AfflictionOrigin)
					{
						list2 = list[i];
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2 = new List<StatusEffect>();
					list.Add(list2);
				}
				list2.Add(statusEffect);
				effectList.Remove(statusEffect);
			}
		}
		return list;
	}

	public static List<StatusEffect> BundleEffects(List<StatusEffect> effectList, out float longestDuration)
	{
		longestDuration = 0f;
		if (effectList.Count == 0)
		{
			return null;
		}
		int num = effectList.Count - 1;
		while (num >= 0 && effectList[num].Params.AffectsStat == ModifiedStat.NoEffect)
		{
			effectList.RemoveAt(num);
			num--;
		}
		if (num < 0)
		{
			return null;
		}
		List<StatusEffect> list = new List<StatusEffect>();
		list.Add(effectList[num]);
		effectList.RemoveAt(num);
		num--;
		longestDuration = 0f;
		while (num >= 0)
		{
			StatusEffect statusEffect = effectList[num];
			if (statusEffect.Params.AffectsStat == ModifiedStat.NoEffect)
			{
				effectList.Remove(statusEffect);
			}
			else if (statusEffect.BundlesWith(list[0]))
			{
				list.Add(statusEffect);
				effectList.Remove(statusEffect);
				if (!statusEffect.Params.IsInstantApplication && statusEffect.TimeLeft > longestDuration)
				{
					longestDuration = statusEffect.TimeLeft;
				}
			}
			num--;
		}
		return list;
	}

	protected StatusEffect CreateChild(GameObject owner, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear)
	{
		return CreateChild(owner, param, abType, hitInfo, deleteOnClear, 0f);
	}

	public StatusEffect CreateChild(GameObject owner, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear, float durationOverride)
	{
		StatusEffect statusEffect = Create(owner, param, abType, hitInfo, deleteOnClear, durationOverride);
		statusEffect.AbilityOrigin = AbilityOrigin;
		statusEffect.AfflictionOrigin = AfflictionOrigin;
		statusEffect.EquipmentOrigin = EquipmentOrigin;
		statusEffect.PhraseOrigin = PhraseOrigin;
		if (hitInfo == null)
		{
			statusEffect.m_durationScale = m_durationScale;
		}
		return statusEffect;
	}

	public static StatusEffect Create(GameObject owner, GenericAbility origin, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear)
	{
		StatusEffect statusEffect = Create(owner, param, abType, hitInfo, deleteOnClear);
		statusEffect.AbilityOrigin = origin;
		return statusEffect;
	}

	public static StatusEffect Create(GameObject owner, Equippable origin, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear)
	{
		StatusEffect statusEffect = Create(owner, param, abType, hitInfo, deleteOnClear);
		statusEffect.EquipmentOrigin = origin;
		return statusEffect;
	}

	public static StatusEffect Create(GameObject owner, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear)
	{
		return Create(owner, param, abType, hitInfo, deleteOnClear, 0f);
	}

	public static StatusEffect Create(GameObject owner, StatusEffectParams param, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear, float durationOverride)
	{
		StatusEffect statusEffect = new StatusEffect();
		GameObject gameObject2 = (statusEffect.Owner = GameUtilities.FindParentWithComponent<CharacterStats>(owner));
		statusEffect.Params = param;
		statusEffect.AbilityType = abType;
		statusEffect.AbilityOrigin = owner.GetComponent<GenericAbility>();
		statusEffect.IsFromAura = false;
		statusEffect.SetHitInfo(hitInfo);
		statusEffect.m_needsDurationCalculated = true;
		statusEffect.m_durationOverride = durationOverride;
		statusEffect.SetDuration(null);
		statusEffect.m_timeApplied = WorldTime.Instance.RealWorldPlayTime;
		if (param.Apply == ApplyType.ApplyOverTime)
		{
			statusEffect.m_intervalTimer = statusEffect.Interval;
		}
		GenericCipherAbility component = owner.GetComponent<GenericCipherAbility>();
		if (component != null)
		{
			statusEffect.DurationAfterBreak = component.DurationAfterBreak;
			statusEffect.TicksAfterBreak = component.TicksAfterBreak;
			statusEffect.IncreasePerTick = component.IncreasePerTick;
		}
		statusEffect.CreateStackingKey();
		ModifiedStat affectsStat = statusEffect.Params.AffectsStat;
		if (affectsStat == ModifiedStat.LaunchAttackWithRollingBonus)
		{
			statusEffect.m_generalCounter = 0u;
		}
		return statusEffect;
	}

	public void CheckForErrors()
	{
		string text = (Origin ? Origin.name : "Null Origin");
		if (Params.AffectsStat == ModifiedStat.MaxHealth && Params.Value == 0f)
		{
			UnityEngine.Debug.LogError("Status effect from " + text + " is set for MaxHealth, Value:0. Did you mean NoEffect?");
		}
		if (!Params.TriggerAdjustment.IneffectiveValue && Params.TriggerAdjustment.MaxTriggerCount == 0)
		{
			UnityEngine.Debug.LogError("Status effect from " + text + " has a TriggerAdjustment with a MaxTriggerCount of 0. It will never trigger.");
		}
		if (Params.OnAppliedVisualEffect != null && Params.OnTriggerVisualEffect != null)
		{
			UnityEngine.Debug.LogError("Status effect from " + text + " has both OnAppliedVisualEffect and OnTriggerVisualEffect. You can only have one.");
		}
		if ((AbilityOrigin == null || (!AbilityOrigin.Modal && !AbilityOrigin.CombatOnly && !AbilityOrigin.Passive)) && Params.Duration == 0f && m_durationOverride <= 0f && !Params.OneHitUse)
		{
			bool flag = false;
			if (AbilityOrigin != null && (bool)AbilityOrigin.GetComponent<Summon>())
			{
				flag = true;
			}
			if (Params.AffectsStat == ModifiedStat.NoEffect && !flag && Params.Persistent)
			{
				UnityEngine.Debug.LogError("Status effect from " + text + " with NoEffect has an infinite duration.");
			}
			if (Params.AffectsStat == ModifiedStat.MaxHealth && Params.Value == 0f)
			{
				UnityEngine.Debug.LogError("Status effect from " + text + " is set for MaxHealth:0 and has an infinite duration.");
			}
		}
		Params.TriggerAdjustment.CheckForErrors(text);
		if (Params.ApplicationPrerequisites != null)
		{
			PrerequisiteData[] applicationPrerequisites = Params.ApplicationPrerequisites;
			for (int i = 0; i < applicationPrerequisites.Length; i++)
			{
				applicationPrerequisites[i].CheckForErrors(text);
			}
		}
	}

	public void SetHitInfo(DamageInfo hitInfo)
	{
		Scale = 1f;
		m_durationScale = 1f;
		if (hitInfo != null)
		{
			if (hitInfo.IsCriticalHit)
			{
				Scale = CharacterStats.CritMultiplier;
				m_durationScale = 1.5f;
			}
			if (hitInfo.IsGraze)
			{
				Scale = CharacterStats.GrazeMultiplier;
				m_durationScale = 0.5f;
			}
			if (hitInfo.IsPlainHit)
			{
				Scale = CharacterStats.HitMultiplier;
			}
		}
	}

	public void Restored()
	{
		if (m_restored)
		{
			return;
		}
		m_restored = true;
		m_restoredMethodCalled = true;
		m_params.ApplicationPrerequisites = m_paramPrereqSerialized.ToArray();
		GameObject objectByID = InstanceID.GetObjectByID(m_params.TrapPrefabSerialized);
		if ((bool)objectByID && m_restoredMethodCalled)
		{
			m_params.TrapPrefab = objectByID.GetComponent<Trap>();
		}
		GameObject objectByID2 = InstanceID.GetObjectByID(m_params.EquippablePrefabSerialized);
		if ((bool)objectByID2)
		{
			m_params.EquippablePrefab = objectByID2.GetComponent<Equippable>();
		}
		GameObject objectByID3 = InstanceID.GetObjectByID(m_params.AttackPrefabSerialized);
		if ((bool)objectByID3)
		{
			m_params.AttackPrefab = objectByID3.GetComponent<AttackBase>();
		}
		GameObject objectByID4 = InstanceID.GetObjectByID(m_params.OnDamageCallbackAbilitySerialized);
		if ((bool)objectByID4)
		{
			m_params.OnDamagedCallbackAbility = objectByID4.GetComponent<GenericAbility>();
		}
		GameObject objectByID5 = InstanceID.GetObjectByID(m_equipmentOriginSerialized);
		if ((bool)objectByID5)
		{
			m_equipmentOrigin = objectByID5.GetComponent<Equippable>();
		}
		GameObject objectByID6 = InstanceID.GetObjectByID(m_afflictionOriginSerialized);
		if ((bool)objectByID6)
		{
			m_afflictionOrigin = objectByID6.GetComponent<Affliction>();
		}
		GameObject objectByID7 = InstanceID.GetObjectByID(m_phraseOriginSerialized);
		if ((bool)objectByID7)
		{
			m_phraseOrigin = objectByID7.GetComponent<Phrase>();
		}
		m_extraObject = InstanceID.GetObjectByID(m_extraObjectSerialized);
		GameObject objectByID8 = InstanceID.GetObjectByID(m_abilityOriginSerialized);
		if ((bool)objectByID8)
		{
			AbilityOrigin = objectByID8.GetComponent<GenericAbility>();
		}
		if (Params.Duration == 0f && (m_equipmentOriginSerialized != Guid.Empty || m_abilityOriginSerialized != Guid.Empty || m_afflictionOriginSerialized != Guid.Empty || m_phraseOriginSerialized != Guid.Empty) && !m_equipmentOrigin && !m_abilityOrigin && !m_afflictionOrigin && !m_phraseOrigin)
		{
			Duration = 0.01f;
		}
		m_target = InstanceID.GetObjectByID(m_targetSerialized);
		m_source = InstanceID.GetObjectByID(m_sourceSerialized);
		m_owner = InstanceID.GetObjectByID(m_ownerSerialized);
		if ((bool)m_owner)
		{
			m_ownerStats = m_owner.GetComponent<CharacterStats>();
		}
		if ((bool)m_target)
		{
			m_targetStats = m_target.GetComponent<CharacterStats>();
			AddTriggerCallback();
		}
		if (m_restoredFXNames != null && m_restoredFXNames.Length != 0 && !(Params.OnAppliedVisualEffect != null) && !(Params.OnTriggerVisualEffect != null))
		{
			if ((bool)EquipmentOrigin)
			{
				StatusEffectParams[] statusEffects = EquipmentOrigin.StatusEffects;
				foreach (StatusEffectParams statusEffectParams in statusEffects)
				{
					if (statusEffectParams.OnAppliedVisualEffect != null && statusEffectParams.OnAppliedVisualEffect.name == m_restoredFXNames[0])
					{
						Params.OnAppliedVisualEffect = statusEffectParams.OnAppliedVisualEffect;
						break;
					}
					if (statusEffectParams.OnTriggerVisualEffect != null && statusEffectParams.OnTriggerVisualEffect.name == m_restoredFXNames[0])
					{
						Params.OnTriggerVisualEffect = statusEffectParams.OnTriggerVisualEffect;
						break;
					}
				}
				if (Params.OnAppliedVisualEffect == null && Params.OnTriggerVisualEffect == null)
				{
					foreach (ItemModComponent attachedItemMod in EquipmentOrigin.AttachedItemMods)
					{
						statusEffects = attachedItemMod.Mod.StatusEffectsOnEquip;
						foreach (StatusEffectParams statusEffectParams2 in statusEffects)
						{
							if (statusEffectParams2.OnAppliedVisualEffect != null && statusEffectParams2.OnAppliedVisualEffect.name == m_restoredFXNames[0])
							{
								Params.OnAppliedVisualEffect = statusEffectParams2.OnAppliedVisualEffect;
								break;
							}
							if (statusEffectParams2.OnTriggerVisualEffect != null && statusEffectParams2.OnTriggerVisualEffect.name == m_restoredFXNames[0])
							{
								Params.OnTriggerVisualEffect = statusEffectParams2.OnTriggerVisualEffect;
								break;
							}
						}
					}
				}
			}
			else if (AbilityOrigin != null)
			{
				StatusEffectParams[] statusEffects = AbilityOrigin.StatusEffects;
				foreach (StatusEffectParams statusEffectParams3 in statusEffects)
				{
					if (statusEffectParams3.OnTriggerVisualEffect != null && statusEffectParams3.OnTriggerVisualEffect.name == m_restoredFXNames[0])
					{
						Params.OnTriggerVisualEffect = statusEffectParams3.OnTriggerVisualEffect;
						break;
					}
				}
			}
			ApplyLoopingEffect(m_params.OnAppliedVisualEffect);
			ApplyLoopingEffect(m_params.OnTriggerVisualEffect);
		}
		ResubscribeListeners();
	}

	private void ResubscribeListeners()
	{
		if (!m_listenersAttached && m_targetStats != null)
		{
			switch (Params.AffectsStat)
			{
			case ModifiedStat.ResistAffliction:
				m_targetStats.OnDefenseAdjustment += AdjustDefenseAffliction;
				m_listenersAttached = true;
				break;
			case ModifiedStat.ResistKeyword:
				m_targetStats.OnDefenseAdjustment += AdjustDefenseKeyword;
				m_listenersAttached = true;
				break;
			case ModifiedStat.KeywordImmunity:
				m_targetStats.OnCheckImmunity += AdjustDefenseKeywordImmune;
				m_listenersAttached = true;
				break;
			}
		}
	}

	private void RetrofixAuraRadius()
	{
		if (!(m_friendlyRadius > 0f))
		{
			return;
		}
		if ((bool)m_abilityOrigin)
		{
			m_friendlyRadius = m_abilityOrigin.FriendlyRadius;
		}
		else
		{
			if (!m_equipmentOrigin)
			{
				return;
			}
			foreach (ItemModComponent attachedItemMod in m_equipmentOrigin.AttachedItemMods)
			{
				if (attachedItemMod.Mod.FriendlyRadius > 0f)
				{
					m_friendlyRadius = attachedItemMod.Mod.FriendlyRadius;
				}
			}
		}
	}

	public void Update()
	{
		UpdateHelper(Time.deltaTime);
	}

	public void TimeJumpUpdate(float seconds)
	{
		if (!(seconds <= 0f))
		{
			UpdateHelper(seconds);
		}
	}

	private void UpdateHelper(float seconds)
	{
		if (m_target == null || IsSuspended)
		{
			return;
		}
		if (m_intervalTimer > 0f)
		{
			float num = seconds;
			CharacterStats characterStats = m_targetStats;
			if (IsFromAura)
			{
				characterStats = m_ownerStats;
			}
			if (characterStats != null)
			{
				if (IsDOT)
				{
					num *= characterStats.DOTTickMult;
				}
				if (IsNegativeMovementEffect)
				{
					num *= characterStats.NegMoveTickMult;
				}
				if (IsPoisonEffect)
				{
					num *= characterStats.PoisonTickMult;
				}
				if (IsDiseaseEffect)
				{
					num *= characterStats.DiseaseTickMult;
				}
			}
			m_intervalTimer -= num;
		}
		m_timeActive += seconds;
		if (!m_suppressed && (m_applied || IsAura))
		{
			UpdateActiveEffect(seconds);
		}
		if (Duration > 0f && m_timeActive > Duration && DurationAfterBreak <= 0f)
		{
			UpdateFinalTick();
			ClearEffect(m_target);
		}
		else
		{
			if (!(m_cachedTeam != null) || !(m_targetStats != null))
			{
				return;
			}
			m_swapTeamTimer -= seconds;
			if (!(m_swapTeamTimer <= 0f))
			{
				return;
			}
			m_swapTeamTimer = SwapTeamTimerMax;
			AIController aIController = GameUtilities.FindActiveAIController(m_target);
			if (aIController != null && aIController.CurrentTarget == null && !aIController.IsConfused)
			{
				if (AfflictionOrigin != null)
				{
					m_targetStats.ClearEffectFromAffliction(AfflictionOrigin);
				}
				else
				{
					ClearEffect(m_target);
				}
			}
		}
	}

	private void UpdateActiveEffect(float seconds)
	{
		if (m_triggerCallbackSet && Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.Timer && m_triggerCount < Params.TriggerAdjustment.MaxTriggerCount && m_timeActive >= Params.TriggerAdjustment.TriggerValue * (float)(m_triggerCount + 1))
		{
			OnTrigger();
		}
		if ((!(Duration > 0f) && !(Interval > 0f)) || !(m_target != null) || !(m_intervalTimer <= 0f))
		{
			return;
		}
		float num = ((Duration > 0f) ? Duration : 1f);
		float adjustedFriendlyRadius = AdjustedFriendlyRadius;
		bool flag = adjustedFriendlyRadius > 0f;
		if (Duration > 0f && m_timeActive >= Duration)
		{
			if (!(DurationAfterBreak <= 0f))
			{
				return;
			}
			if (Params.Apply == ApplyType.ApplyAtEnd)
			{
				ApplyEffect(m_target, ParamsValue());
				if (flag)
				{
					ApplyAura(ParamsValue());
				}
			}
			UpdateFinalTick();
			ClearEffect(m_target);
		}
		else
		{
			if (!(m_intervalTimer <= 0f))
			{
				return;
			}
			m_intervalTimer = Interval;
			if (Params.Apply != ApplyType.ApplyAtEnd)
			{
				if (flag)
				{
					Vector3 vector = ((m_target == null || IsFromAura) ? Owner.transform.position : m_target.transform.position);
					m_tempAuraTargetKeys.Clear();
					m_tempAuraTargetKeys.AddRange(m_auraTargetsApplied.Keys);
					for (int i = 0; i < m_tempAuraTargetKeys.Count; i++)
					{
						GameObject gameObject = m_tempAuraTargetKeys[i];
						if (!(gameObject == null) && (gameObject.transform.position - vector).sqrMagnitude > adjustedFriendlyRadius * adjustedFriendlyRadius)
						{
							RemoveAura(gameObject);
						}
					}
				}
				if (Interval > 0f && Params.Apply != ApplyType.ApplyOverTime)
				{
					if (!flag || !m_effect_is_on_main_target)
					{
						ApplyEffect(m_target, ParamsValue());
					}
					if (flag)
					{
						ApplyAura(ParamsValue());
					}
				}
				else
				{
					float num2 = seconds;
					if (Interval > 0f)
					{
						num2 = Interval;
					}
					float num3 = ParamsValue() * Mathf.Min(1f, num2 / num);
					ApplyEffect(m_target, num3);
					if (flag)
					{
						ApplyAura(num3);
					}
				}
			}
			m_intervalCount++;
		}
	}

	private void UpdateFinalTick()
	{
		if (IsAura)
		{
			return;
		}
		float interval = Interval;
		if (!(interval > 0f))
		{
			return;
		}
		if (Params.Apply == ApplyType.ApplyAtEnd)
		{
			float num = interval - m_intervalTimer;
			if (num >= 1f)
			{
				float num2 = num / interval;
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				float appliedValue = num2 * ParamsValue();
				ApplyEffect(m_target, appliedValue);
			}
		}
		else if (Params.Apply == ApplyType.ApplyOverTime)
		{
			float appliedValue2 = Mathf.Min(1f, (interval - m_intervalTimer) / Duration) * ParamsValue();
			ApplyEffect(m_target, appliedValue2);
		}
		else if (Params.Apply == ApplyType.ApplyOnTick)
		{
			float appliedValue3 = Mathf.Min(1f, 1f - m_intervalTimer / interval) * ParamsValue();
			ApplyEffect(m_target, appliedValue3);
		}
	}

	private void CreateStackingKey()
	{
		m_stackingKey = Params.CalculateStackingKey();
	}

	public int GetStackingKey()
	{
		return m_stackingKey;
	}

	private void ApplyAura(float amount)
	{
		if (m_target == null)
		{
			return;
		}
		RetrofixAuraRadius();
		foreach (GameObject auraTarget in GetAuraTargets(m_target, AdjustedFriendlyRadius))
		{
			if (!m_auraTargetsApplied.ContainsKey(auraTarget) && !m_auraEffectApplied.ContainsKey(auraTarget))
			{
				AddAura(auraTarget, amount);
			}
		}
	}

	public static IEnumerable<GameObject> GetAuraTargets(GameObject emitter, float adjustedRadius)
	{
		float adjustedRadiusSq = adjustedRadius * adjustedRadius;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			GameObject gameObject = faction.gameObject;
			if (faction == null || gameObject == null || gameObject == emitter)
			{
				continue;
			}
			Health component = faction.GetComponent<Health>();
			if (!(component != null) || component.Targetable)
			{
				float sqrMagnitude = (faction.transform.position - emitter.transform.position).sqrMagnitude;
				if (faction.GetRelationship(emitter) == Faction.Relationship.Friendly && sqrMagnitude < adjustedRadiusSq)
				{
					yield return gameObject;
				}
			}
		}
	}

	public void AddAura(GameObject obj, float amount)
	{
		if (this != null && !(obj == null))
		{
			CharacterStats component = obj.GetComponent<CharacterStats>();
			if (component != null)
			{
				StatusEffect statusEffect = CreateChild(m_owner, Params, AbilityType, null, deleteOnClear: true, 0f);
				statusEffect.FriendlyRadius = 0f;
				statusEffect.IsFromAura = true;
				component.ApplyStatusEffect(statusEffect);
				m_auraEffectApplied.Add(obj, statusEffect);
			}
			Transform transform = AttackBase.GetTransform(obj, Params.VisualEffectAttach);
			GameObject value = GameUtilities.LaunchLoopingEffect(m_params.OnAuraVisualEffect, transform.position, transform.rotation, 1f, transform, m_abilityOrigin);
			m_auraTargetsApplied.Add(obj, value);
			Health component2 = obj.GetComponent<Health>();
			if (component2 != null)
			{
				component2.OnDeath += HandleAuraOnDeathAndDestroy;
			}
		}
	}

	public void RemoveAura(GameObject obj)
	{
		if (!(obj == null))
		{
			CharacterStats component = obj.GetComponent<CharacterStats>();
			if ((bool)component && m_auraEffectApplied.ContainsKey(obj))
			{
				component.ClearEffect(m_auraEffectApplied[obj]);
				m_auraEffectApplied.Remove(obj);
			}
			GameUtilities.Destroy(m_auraTargetsApplied[obj]);
			m_auraTargetsApplied.Remove(obj);
			Health component2 = obj.GetComponent<Health>();
			if (component2 != null)
			{
				component2.OnDeath -= HandleAuraOnDeathAndDestroy;
			}
		}
	}

	private void HandleAuraOnDeathAndDestroy(GameObject obj, GameEventArgs args)
	{
		if (m_auraTargetsApplied.ContainsKey(obj))
		{
			RemoveAura(obj);
		}
	}

	private void SetDuration(GameObject target)
	{
		Duration = CalculateDuration(target, ignoreTemporaryAdjustment: false);
	}

	public float CalculateDuration(GameObject target, bool ignoreTemporaryAdjustment)
	{
		float num = 1f;
		float num2 = ((!(m_durationOverride > 0f)) ? Params.Duration : m_durationOverride);
		num2 *= m_durationScale;
		if (target != null)
		{
			CharacterStats component = target.GetComponent<CharacterStats>();
			if (component != null)
			{
				if (Params.AffectsStat == ModifiedStat.KnockedDown)
				{
					num += component.GetStatusEffectValueMultiplier(ModifiedStat.KnockDownDurationMult) - 1f;
				}
				else if (Params.AffectsStat == ModifiedStat.Stunned)
				{
					num += component.GetStatusEffectValueMultiplier(ModifiedStat.StunDurationMult) - 1f;
				}
				if (Params.IsHostile)
				{
					num += component.HostileEffectDurationMultiplier - 1f;
				}
				if (component.IsPartyMember && GameState.Instance.IsDifficultyStoryTime && (bool)m_ownerStats && !m_ownerStats.IsPartyMember)
				{
					num2 *= 0.5f;
				}
				for (int i = 0; i < component.ActiveStatusEffects.Count; i++)
				{
					if (component.ActiveStatusEffects[i].Params.AffectsStat == ModifiedStat.ShortenAfflictionDurationOngoing && AfflictionOrigin == component.ActiveStatusEffects[i].Params.AfflictionPrefab)
					{
						num2 = Mathf.Max(0.05f, num2 + component.ActiveStatusEffects[i].ParamsValue());
					}
				}
			}
		}
		if ((bool)m_ownerStats)
		{
			num2 = Params.AdjustDuration(m_ownerStats, num2);
			num += GetEffectDurationMultiplier(m_ownerStats, m_abilityOrigin, m_afflictionOrigin) - 1f;
		}
		num2 = num2 * num + UnadjustedDurationAdd;
		if (!ignoreTemporaryAdjustment)
		{
			num2 += TemporaryDurationAdjustment;
		}
		return num2;
	}

	public static float EstimateDurationForUI(StatusEffectParams param, StatusEffect effect, CharacterStats ownerStats, GenericAbility abilityOrigin, AfflictionParams afflictionOrigin)
	{
		float num = 1f;
		float num2 = effect?.Duration ?? param.Duration;
		if (afflictionOrigin != null && afflictionOrigin.Duration > 0f)
		{
			num2 = afflictionOrigin.Duration;
		}
		if ((bool)abilityOrigin && abilityOrigin.DurationOverride > 0f)
		{
			num2 = abilityOrigin.DurationOverride;
		}
		if ((bool)ownerStats)
		{
			num2 = param.AdjustDuration(ownerStats, num2);
			num += GetEffectDurationMultiplier(ownerStats, abilityOrigin, afflictionOrigin?.AfflictionPrefab) - 1f;
		}
		float num3 = effect?.UnadjustedDurationAdd ?? 0f;
		return num2 * num + num3;
	}

	private static float GetEffectDurationMultiplier(CharacterStats casterStats, GenericAbility abilityOrigin, Affliction afflictionOrigin)
	{
		float num = 1f;
		if (!casterStats || ((bool)abilityOrigin && abilityOrigin.IgnoreCharacterStats))
		{
			return num;
		}
		if (AfflictionData.Instance != null && afflictionOrigin == AfflictionData.Prone)
		{
			num += casterStats.GetStatusEffectValueMultiplier(ModifiedStat.ProneDurationMult) - 1f;
		}
		Consumable.ConsumableType consumableType = Consumable.ConsumableType.None;
		if ((bool)abilityOrigin && abilityOrigin.EffectType == GenericAbility.AbilityType.Consumable)
		{
			Consumable component = abilityOrigin.GetComponent<Consumable>();
			if ((bool)component)
			{
				consumableType = component.Type;
				num += casterStats.GetStatusEffectValueMultiplier(ModifiedStat.ConsumableDurationMult) - 1f;
			}
		}
		switch (consumableType)
		{
		case Consumable.ConsumableType.Potion:
			num += casterStats.PotionEffectiveness - 1f;
			break;
		case Consumable.ConsumableType.Drug:
			num += casterStats.GetStatusEffectValueMultiplier(ModifiedStat.DrugDurationMult) - 1f;
			break;
		default:
			num += casterStats.StatEffectDurationMultiplier - 1f;
			if (abilityOrigin is Frenzy)
			{
				num += casterStats.GetStatusEffectValueMultiplier(ModifiedStat.FrenzyDurationMult) - 1f;
			}
			break;
		case Consumable.ConsumableType.Ingestible:
			break;
		}
		return num;
	}

	public virtual void ApplyEffect(GameObject target)
	{
		m_target = target;
		if (m_target != null)
		{
			m_targetStats = m_target.GetComponent<CharacterStats>();
		}
		bool applied = m_applied;
		if (m_needsDurationCalculated)
		{
			SetDuration(target);
			m_needsDurationCalculated = false;
		}
		if (Params.Apply == ApplyType.ApplyAtEnd)
		{
			if (Duration <= 0f || Interval <= 0f)
			{
				UnityEngine.Debug.Log("Apply-at-end status effect has no duration or interval!", target);
			}
			m_applied = true;
		}
		else if ((Params.Apply == ApplyType.ApplyOverTime || Params.IntervalRate != 0) && !HasBeenApplied)
		{
			m_applied = true;
		}
		else
		{
			ApplyEffect(target, ParamsValue());
			if (IsAura)
			{
				ApplyAura(ParamsValue());
			}
		}
		if (m_applied && !applied && target != null)
		{
			Transform transform = AttackBase.GetTransform(m_target, Params.VisualEffectAttach);
			GameUtilities.LaunchEffect(m_params.OnStartVisualEffect, 1f, transform, m_abilityOrigin);
			ApplyLoopingEffect(m_params.OnAppliedVisualEffect);
			AddTriggerCallback();
		}
	}

	private void ApplyLoopingEffect(GameObject effectBase)
	{
		if (effectBase == null)
		{
			return;
		}
		Transform transform = AttackBase.GetTransform(m_target, Params.VisualEffectAttach);
		if (Params.VisualEffectAttach >= AttackBase.EffectAttachType.Fx_Bone_01 && Params.VisualEffectAttach <= AttackBase.EffectAttachType.Fx_Bone_10)
		{
			if (m_visualEffectsAttached)
			{
				return;
			}
			m_visualEffectsAttached = true;
			Equipment component = m_target.GetComponent<Equipment>();
			if (component != null)
			{
				EquipmentSet currentItems = component.CurrentItems;
				if (currentItems != null && currentItems.PrimaryWeapon != null)
				{
					AnimationBoneMapper component2 = m_target.GetComponent<AnimationBoneMapper>();
					if (component2 != null)
					{
						for (AttackBase.EffectAttachType effectAttachType = AttackBase.EffectAttachType.Fx_Bone_01; effectAttachType <= AttackBase.EffectAttachType.Fx_Bone_10; effectAttachType++)
						{
							transform = null;
							if (component2.HasBone(currentItems.PrimaryWeapon.gameObject, effectAttachType))
							{
								transform = component2[currentItems.PrimaryWeapon.gameObject, effectAttachType];
							}
							if (transform != null)
							{
								GameObject gameObject = GameUtilities.LaunchLoopingEffect(effectBase, transform.position, transform.rotation, 1f, transform, m_abilityOrigin);
								if (gameObject != null)
								{
									m_appliedFX.Add(gameObject);
								}
							}
						}
					}
				}
			}
		}
		else
		{
			GameObject gameObject2 = GameUtilities.LaunchLoopingEffect(effectBase, transform.position, transform.rotation, 1f, transform, m_abilityOrigin);
			if (gameObject2 != null)
			{
				m_appliedFX.Add(gameObject2);
			}
		}
		AlphaControl alphaControl = (m_target ? m_target.GetComponent<AlphaControl>() : null);
		if ((bool)alphaControl)
		{
			alphaControl.Refresh();
		}
	}

	private void RemoveLoopingEffects()
	{
		if (m_appliedFX.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in m_appliedFX)
		{
			GameUtilities.ShutDownLoopingEffect(item);
			GameUtilities.Destroy(item, Params.DestroyVFXDelay);
		}
		m_appliedFX.Clear();
		m_visualEffectsAttached = false;
	}

	private void DestroyAbilities()
	{
		if (Spells != null)
		{
			for (int i = 0; i < Spells.Count; i++)
			{
				if ((bool)m_ownerStats)
				{
					m_ownerStats.ActiveAbilities.Remove(Spells[i]);
				}
				if ((bool)Spells[i])
				{
					Spells[i].ForceDeactivate(m_owner);
					GameUtilities.Destroy(Spells[i].gameObject);
				}
			}
			Spells.Clear();
		}
		if (AbilitiesGrantedToTarget == null)
		{
			return;
		}
		for (int j = 0; j < AbilitiesGrantedToTarget.Count; j++)
		{
			if ((bool)m_targetStats)
			{
				m_targetStats.ActiveAbilities.Remove(AbilitiesGrantedToTarget[j]);
			}
			if ((bool)AbilitiesGrantedToTarget[j])
			{
				AbilitiesGrantedToTarget[j].ForceDeactivate(m_target);
				GameUtilities.Destroy(AbilitiesGrantedToTarget[j].gameObject);
			}
		}
		AbilitiesGrantedToTarget.Clear();
	}

	public bool CanApply(CharacterStats target)
	{
		if (Params.AffectsStat == ModifiedStat.GenericMarker && ParamsExtraValue() > 0f)
		{
			int num = 0;
			for (int i = 0; i < target.ActiveStatusEffects.Count; i++)
			{
				if (target.ActiveStatusEffects[i].Params.AffectsStat == ModifiedStat.GenericMarker && target.ActiveStatusEffects[i].Params.Value == Params.Value)
				{
					num++;
				}
			}
			if ((float)num >= ParamsExtraValue())
			{
				return false;
			}
		}
		if (!IsAura)
		{
			return Params.CanApply((AbilityOrigin != null) ? AbilityOrigin.gameObject : Owner, target.gameObject, IsFromAura ? Owner : target.gameObject);
		}
		return true;
	}

	private bool ApplyEffect(GameObject target, float appliedValue)
	{
		m_restored = true;
		if (m_target == null)
		{
			m_target = target;
			if (m_target != null)
			{
				m_targetStats = m_target.GetComponent<CharacterStats>();
			}
		}
		if (!Params.CanApply((AbilityOrigin != null) ? AbilityOrigin.gameObject : Owner, target, IsFromAura ? Owner : m_target))
		{
			return false;
		}
		bool flag = Params.Apply == ApplyType.ApplyOverTime || Params.Apply == ApplyType.ApplyAtEnd || Params.IntervalRate != StatusEffectParams.IntervalRateType.None;
		if (m_applied && !flag && target == m_target)
		{
			return false;
		}
		if (IsSuppressed)
		{
			return false;
		}
		if (m_triggerCount > 0 && Params.TriggerAdjustment != null)
		{
			appliedValue = ((!IsScaledMultiplier) ? (appliedValue + Params.TriggerAdjustment.ValueAdjustment * (float)m_triggerCount) : (appliedValue * (float)Math.Pow(Params.TriggerAdjustment.ValueAdjustment, m_triggerCount)));
		}
		if (!ApplyEffectHelper(target, appliedValue))
		{
			return false;
		}
		m_applied = true;
		if (m_target == target)
		{
			m_effect_is_on_main_target = true;
		}
		return true;
	}

	private bool ApplyEffectHelper(GameObject target, float appliedValue)
	{
		if (target == null)
		{
			return false;
		}
		CharacterStats characterStats = m_targetStats;
		if (characterStats == null || target != m_target)
		{
			characterStats = target.GetComponent<CharacterStats>();
		}
		Health component = target.GetComponent<Health>();
		Mover component2 = target.GetComponent<Mover>();
		Equipment component3 = target.GetComponent<Equipment>();
		if (characterStats == null || component == null)
		{
			UnityEngine.Debug.LogError(target.name + " doesn't have a health and/or CharacterStats component. Unable to apply status effect!");
			return false;
		}
		if (AfflictionOrigin == null && IsDamageDealing)
		{
			if (IsScaledMultiplier)
			{
				appliedValue -= 1f;
				appliedValue *= Scale;
				appliedValue += 1f;
			}
			else if (!IsDOT)
			{
				appliedValue *= Scale;
			}
		}
		if (Params.ChecksReligion)
		{
			m_religiousScale = Religion.Instance.GetCurrentBonusMultiplier(Owner, m_abilityOrigin);
		}
		if (IsScaledMultiplier)
		{
			appliedValue -= 1f;
			appliedValue *= m_religiousScale;
			appliedValue += 1f;
		}
		else
		{
			appliedValue *= m_religiousScale;
		}
		if ((bool)AbilityOrigin && AbilityOrigin.EffectType == GenericAbility.AbilityType.Consumable && !IsOverTime)
		{
			Consumable component4 = AbilityOrigin.GetComponent<Consumable>();
			if ((bool)component4 && component4.Type == Consumable.ConsumableType.Potion)
			{
				appliedValue *= characterStats.PotionEffectiveness;
			}
		}
		if (Owner != null)
		{
			Persistence component5 = Owner.GetComponent<Persistence>();
			if ((bool)component5 && !component5.Mobile && (GameState.LoadedGame || GameState.IsRestoredLevel))
			{
				ResubscribeListeners();
				return true;
			}
		}
		if (IsModifiedStatObsolete(Params.AffectsStat))
		{
			UnityEngine.Debug.LogError(string.Concat("ModifiedStat '", Params.AffectsStat, "' is obsolete. Message: '", GetModifiedStatObsoleteMessage(Params.AffectsStat), "'."), Origin);
		}
		switch (Params.AffectsStat)
		{
		case ModifiedStat.MaxHealth:
			if (!component.Dead)
			{
				characterStats.MaxHealth += appliedValue;
				component.ApplyHealthChangeDirectly(0f, m_owner);
			}
			break;
		case ModifiedStat.Stamina:
		case ModifiedStat.RawStamina:
			if (!component.Dead)
			{
				float num17 = appliedValue;
				if (num17 > 0f && (Params.Apply == ApplyType.ApplyOverTime || (Params.Apply == ApplyType.ApplyOnTick && Duration >= 1f)))
				{
					num17 *= characterStats.StaminaRechargeMult;
				}
				component.ApplyStaminaChangeDirectly(num17, m_owner, applyIfDead: false, Params.AffectsStat == ModifiedStat.Stamina);
			}
			break;
		case ModifiedStat.TransferStamina:
		{
			Health health2 = (m_owner ? m_owner.GetComponent<Health>() : null);
			if (health2 != null)
			{
				health2.TransferStaminaFrom(m_owner, component, appliedValue, applyIfDead: false);
			}
			break;
		}
		case ModifiedStat.TransferStaminaReversed:
		{
			Health health = (m_owner ? m_owner.GetComponent<Health>() : null);
			if (health != null)
			{
				component.TransferStaminaFrom(m_owner, health, appliedValue, ParamsExtraValue() > 0f);
			}
			break;
		}
		case ModifiedStat.Damage:
			if (!component.Unconscious && !component.Dead)
			{
				if (IncreasePerTick > 0f)
				{
					component.ApplyDamageDirectly(appliedValue * IncreasePerTick * (float)m_intervalCount, Params.DmgType, Owner, this);
				}
				else
				{
					component.ApplyDamageDirectly(appliedValue, Params.DmgType, Owner, this);
				}
			}
			break;
		case ModifiedStat.DamageByKeywordCount:
			if (!component.Unconscious && !component.Dead)
			{
				int num14 = characterStats.CountStatusEffects(Params.Keyword, (ParamsExtraValue() != 0f) ? Owner : null);
				float num15 = ((!(IncreasePerTick > 0f)) ? (appliedValue * (float)num14) : (appliedValue * IncreasePerTick * (float)m_intervalCount * (float)num14));
				component.ApplyDamageDirectly(num15, Params.DmgType, Owner, this);
				component.ReportDamage(CharacterStats.NameColored(target), num15, Owner, this);
			}
			break;
		case ModifiedStat.Health:
			if (!component.Dead)
			{
				component.ApplyHealthChangeDirectly(appliedValue, m_owner, this, applyIfDead: true);
			}
			break;
		case ModifiedStat.MaxStamina:
			characterStats.StaminaBonus += appliedValue;
			if (!component.Unconscious)
			{
				component.CurrentStamina += appliedValue;
			}
			break;
		case ModifiedStat.StaminaRechargeRate:
			characterStats.StaminaRechargeBonus += appliedValue;
			break;
		case ModifiedStat.AttackSpeed:
			characterStats.AttackSpeedMultiplier *= appliedValue;
			break;
		case ModifiedStat.Stealth:
			characterStats.StealthBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusDamage:
			if (Params.DmgType < DamagePacket.DamageType.Count)
			{
				characterStats.BonusDamage[(int)Params.DmgType] += appliedValue;
			}
			else if (Params.DmgType == DamagePacket.DamageType.All)
			{
				for (int l = 0; l < 7; l++)
				{
					characterStats.BonusDamage[l] += appliedValue;
				}
			}
			break;
		case ModifiedStat.DamageMinimum:
			characterStats.DamageMinBonus += appliedValue;
			break;
		case ModifiedStat.MovementRate:
			if ((bool)component2)
			{
				component2.SetRunSpeed(component2.RunSpeed + appliedValue);
			}
			break;
		case ModifiedStat.NonTargetable:
			component.Targetable = false;
			break;
		case ModifiedStat.NonMobile:
			if ((bool)component2)
			{
				component2.Frozen = true;
			}
			break;
		case ModifiedStat.KnockedDown:
			SendOnEventToAi(GameEventType.KnockedDown, target);
			break;
		case ModifiedStat.Stunned:
			SendOnEventToAi(GameEventType.Stunned, target);
			break;
		case ModifiedStat.EngagedEnemyCount:
			characterStats.EngageableEnemyCount += (int)appliedValue;
			if (characterStats.EngageableEnemyCount < 0)
			{
				characterStats.EngageableEnemyCount = 0;
			}
			if ((int)appliedValue < 0)
			{
				AIController aIController5 = GameUtilities.FindActiveAIController(target);
				if (aIController5 != null)
				{
					aIController5.CancelAllEngagements();
					aIController5.UpdateEngagement(Owner, AIController.GetPrimaryAttack(target));
				}
			}
			break;
		case ModifiedStat.EngagementRadius:
			characterStats.EngagementDistanceBonus += appliedValue;
			break;
		case ModifiedStat.DisengagementAccuracy:
			characterStats.DisengagementAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.MeleeAttackDistanceMult:
			characterStats.MeleeAttackDistanceMultiplier = appliedValue;
			break;
		case ModifiedStat.RangedAttackDistanceMult:
			characterStats.RangedAttackDistanceMultiplier = appliedValue;
			break;
		case ModifiedStat.BonusDTFromArmor:
			characterStats.BonusDTFromArmor += appliedValue;
			break;
		case ModifiedStat.MeleeDamageRangePctIncreaseToMin:
			characterStats.MeleeDamageRangePctIncreaseToMin += appliedValue;
			break;
		case ModifiedStat.SuspendHostileEffects:
			characterStats.SuspendEffects(CharacterStats.EffectType.Hostile);
			break;
		case ModifiedStat.ImmuneToEngageStop:
			characterStats.ImmuneToEngageStop = true;
			break;
		case ModifiedStat.DrainResolveForDeflection:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Resolve, (int)appliedValue);
			if (m_owner != null && m_ownerStats != null)
			{
				StatusEffectParams statusEffectParams2 = new StatusEffectParams();
				statusEffectParams2.AffectsStat = ModifiedStat.Deflection;
				statusEffectParams2.Value = ParamsExtraValue();
				statusEffectParams2.Duration = Params.Duration;
				statusEffectParams2.IsHostile = false;
				StatusEffect statusEffect3 = CreateChild(m_owner, statusEffectParams2, AbilityType, null, deleteOnClear: true);
				statusEffect3.DurationAfterBreak = DurationAfterBreak;
				m_ownerStats.ApplyStatusEffectImmediate(statusEffect3);
			}
			break;
		case ModifiedStat.ReapplyDamage:
			if (m_intervalCount != 0)
			{
				float num16 = m_damageToReapply * (ParamsValue() + IncreasePerTick * (float)(m_intervalCount - 1));
				PostDamageMessage(Owner, Target, null, num16, DamagePacket.DamageType.Raw);
				component.ApplyDamageDirectly(num16, DamagePacket.DamageType.Raw, Owner, this);
				m_damageToReapply = 0f;
			}
			else
			{
				component.OnDamaged += HandleOnDamagedForReapply;
				m_damageToReapply = 0f;
			}
			break;
		case ModifiedStat.ReapplyDamageToNearbyEnemies:
			if (m_intervalCount != 0)
			{
				float num13 = m_damageToReapply * (ParamsValue() + IncreasePerTick * (float)(m_intervalCount - 1));
				GameObject[] array = GameUtilities.CreaturesInRange(target.transform.position, 3f, target, includeUnconscious: true);
				if (array != null)
				{
					GameObject[] array2 = array;
					foreach (GameObject gameObject in array2)
					{
						Health component19 = gameObject.GetComponent<Health>();
						if (component19 != null)
						{
							PostDamageMessage(Owner, gameObject, null, num13, DamagePacket.DamageType.Raw);
							component19.ApplyDamageDirectly(num13, DamagePacket.DamageType.Raw, Owner, this);
						}
					}
				}
				m_damageToReapply = 0f;
			}
			else
			{
				component.OnDamaged += HandleOnDamagedForReapply;
				m_damageToReapply = 0f;
			}
			break;
		case ModifiedStat.ReloadSpeed:
			characterStats.ReloadSpeedMultiplier *= appliedValue;
			break;
		case ModifiedStat.DropTrap:
		{
			if (!(component2 != null))
			{
				break;
			}
			Vector3 lastPosition = component2.LastPosition;
			if (!(lastPosition != target.transform.position))
			{
				break;
			}
			if (Traps == null)
			{
				Traps = new List<Trap>();
			}
			if (((float)Traps.Count >= ParamsValue() || ParamsValue() == 0f) && Traps.Count > 0)
			{
				if ((bool)Traps[0])
				{
					Traps[0].DestroyTrap();
				}
				Traps.RemoveAt(0);
			}
			Traps.Add(HazardAttack.PlaceTrap(Params.TrapPrefab, lastPosition, target));
			break;
		}
		case ModifiedStat.StasisShield:
			if (m_damageToAbsorb == 0f)
			{
				characterStats.OnPreDamageApplied += HandleStatsOnPreDamageAppliedForAbsorb;
				m_damageToAbsorb = appliedValue;
				SendOnEventToAi(GameEventType.Stunned, target);
			}
			break;
		case ModifiedStat.DamageShield:
			if (m_damageToAbsorb == 0f)
			{
				characterStats.OnPreDamageApplied += HandleStatsOnPreDamageAppliedForAbsorb;
				m_damageToAbsorb = appliedValue;
			}
			break;
		case ModifiedStat.AfflictionShield:
			m_generalCounter = 0u;
			break;
		case ModifiedStat.SuspendBeneficialEffects:
			characterStats.SuspendEffects(CharacterStats.EffectType.Beneficial);
			break;
		case ModifiedStat.DamageBasedOnInverseStamina:
			if (!component.Dead)
			{
				float num8 = 2f - component.CurrentStamina / component.MaxStamina;
				if (IncreasePerTick > 0f)
				{
					component.ApplyDamageDirectly(appliedValue * num8 * IncreasePerTick * (float)m_intervalCount, Params.DmgType, Owner, this);
				}
				else
				{
					component.ApplyDamageDirectly(appliedValue * num8, Params.DmgType, Owner, this);
				}
			}
			break;
		case ModifiedStat.Resolve:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Resolve, (int)appliedValue);
			break;
		case ModifiedStat.Might:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Might, (int)appliedValue);
			break;
		case ModifiedStat.Dexterity:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Dexterity, (int)appliedValue);
			break;
		case ModifiedStat.Intellect:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Intellect, (int)appliedValue);
			break;
		case ModifiedStat.SummonWeapon:
			if (component3 != null)
			{
				component3.PushSummonedWeapon(Params.EquippablePrefab, Equippable.EquipmentSlot.PrimaryWeapon, this);
				PartyMemberAI component11 = target.GetComponent<PartyMemberAI>();
				if (component11 != null)
				{
					component11.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: true);
				}
			}
			break;
		case ModifiedStat.SummonSecondaryWeapon:
			if (component3 != null)
			{
				component3.PushSummonedWeapon(Params.EquippablePrefab, Equippable.EquipmentSlot.SecondaryWeapon, this);
				PartyMemberAI component6 = target.GetComponent<PartyMemberAI>();
				if (component6 != null)
				{
					component6.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: true);
				}
			}
			break;
		case ModifiedStat.StaminaRechargeRateMult:
			characterStats.StaminaRechargeMult *= appliedValue;
			break;
		case ModifiedStat.VesselAccuracy:
			characterStats.VesselAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.BeastAccuracy:
			characterStats.BeastAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.WilderAccuracy:
			characterStats.WilderAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.StunDefense:
			characterStats.StunDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.KnockdownDefense:
			characterStats.KnockdownDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.PoisonDefense:
			characterStats.PoisonDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.DiseaseDefense:
			characterStats.DiseaseDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusAccuracyForNearestAllyOnSameEnemy:
			characterStats.NearestAllyWithSharedTargetAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.EnemyCritToHitPercent:
			characterStats.EnemyCritToHitPercent += appliedValue;
			break;
		case ModifiedStat.HostileEffectDurationMult:
			characterStats.AdjustEffectDurations(CharacterStats.EffectType.Hostile, appliedValue);
			characterStats.HostileEffectDurationMultiplier *= appliedValue;
			break;
		case ModifiedStat.EnemyDeflectReflexHitToGrazePercent:
			characterStats.EnemyDeflectReflexHitToGrazePercent += appliedValue;
			break;
		case ModifiedStat.EnemyFortitudeWillHitToGrazePercent:
			characterStats.EnemyFortitudeWillHitToGrazePercent += appliedValue;
			break;
		case ModifiedStat.ExtraStraightBounces:
			characterStats.ExtraStraightBounces += (int)appliedValue;
			break;
		case ModifiedStat.RedirectMeleeAttacks:
			characterStats.RedirectMeleeAttacks = true;
			break;
		case ModifiedStat.HostileAOEDamageMultiplier:
			characterStats.HostileAOEDamageMultiplier *= appliedValue;
			break;
		case ModifiedStat.ImprovedFlanking:
			characterStats.ImprovedFlanking++;
			break;
		case ModifiedStat.DTBypass:
			characterStats.DTBypass += (int)appliedValue;
			break;
		case ModifiedStat.StealSpell:
		{
			if (!m_ownerStats)
			{
				break;
			}
			DestroyAbilities();
			int num10 = (int)ParamsValue();
			List<GenericSpell> list2 = new List<GenericSpell>();
			if ((bool)component3 && component3.CurrentItems != null && (bool)component3.CurrentItems.Grimoire)
			{
				Grimoire component17 = component3.CurrentItems.Grimoire.GetComponent<Grimoire>();
				if (component17 != null)
				{
					component17.FindNewSpells(list2, m_ownerStats, num10);
				}
			}
			if ((bool)characterStats)
			{
				characterStats.FindNewSpells(list2, m_ownerStats, num10);
			}
			if (Spells == null)
			{
				Spells = new List<GenericAbility>();
			}
			List<GenericSpell> list3 = new List<GenericSpell>();
			for (int num11 = list2.Count - 1; num11 >= 0; num11--)
			{
				if (list2[num11].SpellLevel > num10 - 3)
				{
					list3.Add(list2[num11]);
					list2.RemoveAt(num11);
				}
			}
			int num12 = (int)ParamsExtraValue();
			while ((list2.Count > 0 || list3.Count > 0) && Spells.Count < num12)
			{
				List<GenericSpell> obj2 = ((list3.Count > 0) ? list3 : list2);
				int index = OEIRandom.Index(obj2.Count);
				GenericSpell genericSpell = obj2[index];
				obj2.RemoveAt(index);
				if ((bool)genericSpell)
				{
					GenericSpell genericSpell2 = (GenericSpell)m_ownerStats.InstantiateAbility(genericSpell, GenericAbility.AbilityType.Spell);
					Persistence component18 = genericSpell2.GetComponent<Persistence>();
					if ((bool)component18)
					{
						GameUtilities.Destroy(component18);
					}
					genericSpell2.IsFree = true;
					genericSpell2.NeedsGrimoire = false;
					genericSpell2.ProhibitFromGrimoire = true;
					genericSpell2.StatusEffectGrantingSpell = this;
					Spells.Add(genericSpell2);
				}
			}
			break;
		}
		case ModifiedStat.GrantAbility:
		{
			if (AbilitiesGrantedToTarget == null)
			{
				AbilitiesGrantedToTarget = new List<GenericAbility>();
			}
			CharacterStats characterStats3 = (target ? target.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats3)
			{
				GenericAbility item = characterStats3.InstantiateAbility(Params.AbilityPrefab, GenericAbility.AbilityType.Ability);
				AbilitiesGrantedToTarget.Add(item);
			}
			break;
		}
		case ModifiedStat.SwapFaction:
		{
			AIController aIController3 = GameUtilities.FindActiveAIController(target);
			PartyMemberAI partyMemberAI = aIController3 as PartyMemberAI;
			AIController aIController4 = null;
			if ((bool)partyMemberAI)
			{
				partyMemberAI.Selected = false;
				AIPackageController aIPackageController = target.GetComponent<AIPackageController>();
				if (aIPackageController == null)
				{
					aIPackageController = target.AddComponent<AIPackageController>();
					aIPackageController.InitAI();
				}
				else if (aIPackageController.StateManager != null)
				{
					aIPackageController.StateManager.PopAllStates();
					if (aIPackageController.StateManager.DefaultState == null)
					{
						aIPackageController.InitAI();
					}
				}
				aIController4 = aIPackageController;
				partyMemberAI.TransferSummonedCreatures(aIPackageController);
				partyMemberAI.enabled = false;
				aIPackageController.enabled = true;
				aIPackageController.SummonType = partyMemberAI.SummonType;
			}
			if (aIController3 != null)
			{
				AIState currentState = aIController3.StateManager.CurrentState;
				if (currentState.Priority < 3)
				{
					aIController3.SafePopAllStates();
				}
				else
				{
					aIController3.StateManager.ClearQueuedStates();
					if (aIController4 != null)
					{
						aIController3.StateManager.TransferCurrentState(aIController4.StateManager);
					}
				}
				if (!component.Dead && !component.Unconscious && !(currentState is KnockedDown))
				{
					aIController3.InterruptAnimationForCutscene();
				}
				ChanterTrait chanterTrait = characterStats.GetChanterTrait();
				if (chanterTrait != null && chanterTrait.IsChanting() && chanterTrait.Chant != null)
				{
					chanterTrait.Chant.InterruptChant();
				}
			}
			Faction component9 = target.GetComponent<Faction>();
			Faction component10 = m_owner.GetComponent<Faction>();
			if (component9 != null && component10 != null)
			{
				m_cachedTeam = component9.CurrentTeamInstance;
				m_swapTeamTimer = SwapTeamTimerMax;
				component9.ModifyToMatch(component10);
			}
			break;
		}
		case ModifiedStat.Athletics:
			characterStats.AthleticsBonus += (int)appliedValue;
			break;
		case ModifiedStat.Lore:
			characterStats.LoreBonus += (int)appliedValue;
			break;
		case ModifiedStat.Mechanics:
			characterStats.MechanicsBonus += (int)appliedValue;
			break;
		case ModifiedStat.Survival:
			characterStats.SurvivalBonus += (int)appliedValue;
			break;
		case ModifiedStat.Crafting:
			characterStats.CraftingBonus += (int)appliedValue;
			break;
		case ModifiedStat.PushDefense:
			characterStats.PushDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.WhileStunnedDefense:
			characterStats.WhileStunnedDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.WhileKnockeddownDefense:
			characterStats.WhileKnockeddownDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.Constitution:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Constitution, (int)appliedValue);
			break;
		case ModifiedStat.Perception:
			characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Perception, (int)appliedValue);
			break;
		case ModifiedStat.CritHitMultiplierBonus:
			characterStats.CritHitDamageMultiplierBonus += appliedValue;
			break;
		case ModifiedStat.BonusGrazeToHitPercent:
			characterStats.BonusGrazeToHitPercent += appliedValue;
			break;
		case ModifiedStat.BonusGrazeToMissPercent:
			characterStats.BonusGrazeToMissPercent += appliedValue;
			break;
		case ModifiedStat.BonusCritToHitPercent:
			characterStats.BonusCritToHitPercent += appliedValue;
			break;
		case ModifiedStat.BonusMissToGrazePercent:
			characterStats.BonusMissToGrazePercent += appliedValue;
			break;
		case ModifiedStat.BonusHitToCritPercent:
			characterStats.BonusHitToCritPercent += appliedValue;
			break;
		case ModifiedStat.BonusHitToCritPercentAll:
			characterStats.BonusHitToCritPercentAll += appliedValue;
			break;
		case ModifiedStat.BonusHitToGrazePercent:
			characterStats.BonusHitToGrazePercent += appliedValue;
			break;
		case ModifiedStat.Confused:
		{
			if (!(m_timeActive < float.Epsilon))
			{
				break;
			}
			AIController aIController2 = GameUtilities.FindActiveAIController(target);
			if (aIController2 != null && m_cachedTeam == null)
			{
				if (aIController2 is PartyMemberAI && PartyMemberAI.NumPrimaryPartyMembers == 1 && Duration > SwapTeamTimerMax)
				{
					Duration = SwapTeamTimerMax;
				}
				SendOnEventToAi(GameEventType.Confused, target);
			}
			break;
		}
		case ModifiedStat.RateOfFireMult:
			characterStats.RateOfFireMultiplier *= appliedValue;
			break;
		case ModifiedStat.ApplyAttackEffects:
		{
			if (!(Params.AttackPrefab != null) || !(Params.AttackPrefab.AbilityOrigin != null))
			{
				break;
			}
			m_generalCounter = 0u;
			AIController aIController = GameUtilities.FindActiveAIController(m_owner);
			if (aIController != null && aIController.IsPerformingSecondPartOfFullAttack())
			{
				foreach (StatusEffect activeStatusEffect in Params.AttackPrefab.AbilityOrigin.ActiveStatusEffects)
				{
					bool oneHitUse = activeStatusEffect.Params.OneHitUse;
					activeStatusEffect.Params.OneHitUse = true;
					m_ownerStats.ApplyStatusEffectImmediate(activeStatusEffect);
					activeStatusEffect.Params.OneHitUse = oneHitUse;
				}
			}
			else
			{
				Params.AttackPrefab.AbilityOrigin.Activate();
				m_generalCounter = 1u;
			}
			break;
		}
		case ModifiedStat.EnemyReflexGrazeToMissPercent:
			characterStats.EnemyReflexGrazeToMissPercent += appliedValue;
			break;
		case ModifiedStat.EnemyReflexHitToGrazePercent:
			characterStats.EnemyReflexHitToGrazePercent += appliedValue;
			break;
		case ModifiedStat.StaminaPercent:
			component.ApplyStaminaChangeDirectly(appliedValue * component.MaxStamina, m_owner, applyIfDead: false);
			break;
		case ModifiedStat.EnemiesNeededToFlankAdj:
			characterStats.EnemiesNeededToFlank += (int)appliedValue;
			characterStats.CheckToRemoveFlanked();
			characterStats.CheckToAddFlankedAll();
			break;
		case ModifiedStat.ResistAffliction:
			if (!m_listenersAttached)
			{
				characterStats.OnDefenseAdjustment += AdjustDefenseAffliction;
				characterStats.AdjustStatusEffectDurationsFromAffliction(Params.AfflictionPrefab, ParamsExtraValue());
				m_listenersAttached = true;
			}
			break;
		case ModifiedStat.PreventDeath:
			characterStats.DeathPrevented++;
			break;
		case ModifiedStat.AdjustDurationBeneficialEffects:
			characterStats.AdjustStatusEffectDurations(CharacterStats.EffectType.Beneficial, ParamsValue(), this);
			break;
		case ModifiedStat.DOTTickMult:
			characterStats.DOTTickMult *= appliedValue;
			break;
		case ModifiedStat.AdjustDurationHostileEffects:
			characterStats.AdjustStatusEffectDurations(CharacterStats.EffectType.Hostile, ParamsValue(), this);
			break;
		case ModifiedStat.ResistKeyword:
			if (!m_listenersAttached)
			{
				characterStats.OnDefenseAdjustment += AdjustDefenseKeyword;
				characterStats.AdjustStatusEffectDurationsFromKeyword(Params.Keyword, ParamsExtraValue());
				m_listenersAttached = true;
			}
			break;
		case ModifiedStat.KeywordImmunity:
			if (!m_listenersAttached)
			{
				characterStats.OnCheckImmunity += AdjustDefenseKeywordImmune;
				characterStats.ClearStatusEffectsByKeyword(Params.Keyword);
				m_listenersAttached = true;
			}
			break;
		case ModifiedStat.TransferDT:
			if (!(m_owner != null) || !(m_ownerStats != null))
			{
				break;
			}
			m_ownerStats.DamageThreshhold[(int)Params.DmgType] += appliedValue;
			characterStats.DamageThreshhold[(int)Params.DmgType] -= appliedValue;
			if (Params.DmgType == DamagePacket.DamageType.All)
			{
				for (int m = 0; m < 7; m++)
				{
					m_ownerStats.DamageThreshhold[m] += appliedValue;
					characterStats.DamageThreshhold[m] -= appliedValue;
				}
			}
			break;
		case ModifiedStat.TransferRandomAttribute:
			characterStats.AddAttributeBonus((CharacterStats.AttributeScoreType)m_generalCounter, -(int)appliedValue);
			if ((bool)m_ownerStats)
			{
				m_ownerStats.AddAttributeBonus((CharacterStats.AttributeScoreType)m_generalCounter, (int)appliedValue);
			}
			break;
		case ModifiedStat.TransferAttribute:
			characterStats.AddAttributeBonus(Params.AttributeType, -(int)appliedValue);
			if ((bool)m_ownerStats)
			{
				m_ownerStats.AddAttributeBonus(Params.AttributeType, (int)appliedValue);
			}
			break;
		case ModifiedStat.Disintegrate:
			if (component.Dead)
			{
				break;
			}
			if (IncreasePerTick > 0f)
			{
				component.ApplyDamageDirectly(appliedValue * IncreasePerTick * (float)m_intervalCount, DamagePacket.DamageType.Raw, Owner, this);
			}
			else
			{
				component.ApplyDamageDirectly(appliedValue, DamagePacket.DamageType.Raw, Owner, this);
			}
			if (component.Dead && target != GameState.s_playerCharacter.gameObject)
			{
				PartyMemberAI component14 = target.GetComponent<PartyMemberAI>();
				if (component14 != null && component14.enabled)
				{
					GameUtilities.KillAnimalCompanion(component14.gameObject);
					PartyMemberAI.RemoveFromActiveParty(component14, purgePersistencePacket: true);
				}
				Loot component15 = target.GetComponent<Loot>();
				if ((bool)component15)
				{
					component15.UseBodyAsLootBag = false;
					component15.DropAllItems();
				}
				Persistence component16 = target.GetComponent<Persistence>();
				if ((bool)component16)
				{
					component16.SetForDestroy();
				}
				GameUtilities.Destroy(target, 0.1f);
			}
			break;
		case ModifiedStat.BonusAccuracyAtLowStamina:
			if (component.StaminaPercentage <= ParamsExtraValue())
			{
				m_generalCounter = 1u;
			}
			else
			{
				m_generalCounter = 0u;
			}
			break;
		case ModifiedStat.EnemyHitToGrazePercent:
			characterStats.EnemyHitToGrazePercent += appliedValue;
			break;
		case ModifiedStat.Fatigue:
			characterStats.AdjustFatigueLevel(Mathf.RoundToInt(appliedValue));
			break;
		case ModifiedStat.PrimordialAccuracy:
			characterStats.PrimordialAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.StopAnimation:
			SendOnEventToAi(GameEventType.Paralyzed, target);
			break;
		case ModifiedStat.AddAfflictionImmunity:
			if (Params.AfflictionPrefab != null)
			{
				characterStats.AfflictionImmunities.Add(Params.AfflictionPrefab);
				characterStats.ClearEffectFromAffliction(Params.AfflictionPrefab);
			}
			break;
		case ModifiedStat.RemoveAffliction:
			characterStats.ClearEffectFromAffliction(Params.AfflictionPrefab);
			break;
		case ModifiedStat.Invisible:
			characterStats.InvisibilityState++;
			if (characterStats.InvisibilityState == 1)
			{
				AIController component13 = target.GetComponent<AIController>();
				if ((bool)component13 && component13.StateManager != null)
				{
					component13.SafePopAllStates();
					component13.CancelAllEngagements();
				}
			}
			break;
		case ModifiedStat.WoundDelay:
			characterStats.WoundDelay += appliedValue;
			break;
		case ModifiedStat.FinishingBlowDamageMult:
			characterStats.FinishingBlowDamageMult *= appliedValue;
			break;
		case ModifiedStat.ZealousAuraAoEMult:
			characterStats.ZealousAuraRadiusMult *= Mathf.Sqrt(appliedValue);
			break;
		case ModifiedStat.DelayUnconsciousness:
			characterStats.UnconsciousnessDelayed++;
			break;
		case ModifiedStat.NegMoveTickMult:
			characterStats.NegMoveTickMult *= appliedValue;
			break;
		case ModifiedStat.FocusGainMult:
			characterStats.FocusGainMult *= appliedValue;
			break;
		case ModifiedStat.DisengagementDefense:
			characterStats.DisengagementDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.SpellDefense:
			characterStats.SpellDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.RangedDeflection:
			characterStats.RangedDeflectionBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusUsesPerRestPastThree:
			characterStats.BonusUsesPerRestPastThree += (int)appliedValue;
			break;
		case ModifiedStat.PoisonTickMult:
			characterStats.PoisonTickMult *= appliedValue;
			break;
		case ModifiedStat.DiseaseTickMult:
			characterStats.DiseaseTickMult *= appliedValue;
			break;
		case ModifiedStat.StalkersLinkDamageMult:
			characterStats.StalkersLinkDamageMult *= appliedValue;
			break;
		case ModifiedStat.ChanterPhraseAoEMult:
			characterStats.ChanterPhraseRadiusMult *= Mathf.Sqrt(appliedValue);
			break;
		case ModifiedStat.BonusHealMult:
			characterStats.BonusHealMult *= appliedValue;
			break;
		case ModifiedStat.BonusHealingGivenMult:
			characterStats.BonusHealingGivenMult *= appliedValue;
			break;
		case ModifiedStat.SpellCastBonus:
		{
			int num9 = (int)ParamsExtraValue() - 1;
			if (num9 >= 0 && num9 < 8)
			{
				characterStats.SpellCastBonus[num9] += (int)appliedValue;
			}
			break;
		}
		case ModifiedStat.AoEMult:
			characterStats.AoERadiusMult *= Mathf.Sqrt(appliedValue);
			break;
		case ModifiedStat.WildstrikeDamageMult:
			characterStats.WildstrikeDamageMult += appliedValue - 1f;
			break;
		case ModifiedStat.ReviveAndAddStamina:
			if (!component.Dead)
			{
				if (component.Unconscious)
				{
					component.OnRevive();
				}
				component.ApplyStaminaChangeDirectly(appliedValue, m_owner, applyIfDead: false);
				component.ReportStamina(CharacterStats.NameColored(target), appliedValue, m_owner, this);
			}
			break;
		case ModifiedStat.LaunchAttack:
			LaunchAttack(target, 0, 1f);
			break;
		case ModifiedStat.MaxStaminaMult:
			characterStats.MaxStaminaMultiplier *= appliedValue;
			break;
		case ModifiedStat.CallbackOnDamaged:
			if (Params.OnDamagedCallbackAbility != null)
			{
				component.OnDamaged += Params.OnDamagedCallbackAbility.HandleOnDamaged;
			}
			break;
		case ModifiedStat.TransferDamageToStamina:
			if (m_owner != null)
			{
				Health component12 = m_owner.GetComponent<Health>();
				if (component12 != null)
				{
					if (IncreasePerTick > 0f)
					{
						component12.ApplyStaminaChangeDirectly(appliedValue * IncreasePerTick * (float)m_intervalCount, m_owner, applyIfDead: false);
					}
					else
					{
						component12.ApplyStaminaChangeDirectly(appliedValue, m_owner, applyIfDead: false);
					}
				}
			}
			if (Params.DmgType < DamagePacket.DamageType.Count || Params.DmgType == DamagePacket.DamageType.Raw)
			{
				if (IncreasePerTick > 0f)
				{
					component.ApplyDamageDirectly(appliedValue * IncreasePerTick * (float)m_intervalCount, Params.DmgType, Owner, this);
				}
				else
				{
					component.ApplyDamageDirectly(appliedValue, Params.DmgType, Owner, this);
				}
			}
			else if (Params.DmgType == DamagePacket.DamageType.All)
			{
				UnityEngine.Debug.LogError(Params.AffectsStat.ToString() + " is using DamageType of All, which is not supported!");
			}
			break;
		case ModifiedStat.CallbackAfterAttack:
			if (m_abilityOrigin != null)
			{
				characterStats.OnPostDamageDealt += m_abilityOrigin.HandleStatsOnPostDamageDealtCallback;
			}
			break;
		case ModifiedStat.GivePlayerBonusMoneyViaStrongholdTurn:
			if ((bool)Stronghold.Instance)
			{
				Stronghold.Instance.BonusTurnMoney += (int)appliedValue;
			}
			break;
		case ModifiedStat.ArmorSpeedFactorAdj:
			characterStats.ArmorSpeedFactorAdj += appliedValue;
			break;
		case ModifiedStat.SingleWeaponSpeedFactorAdj:
			characterStats.SingleWeaponSpeedFactorAdj += appliedValue;
			break;
		case ModifiedStat.BonusRangedWeaponCloseEnemyDamageMult:
			characterStats.BonusRangedWeaponCloseEnemyDamageMult *= appliedValue;
			break;
		case ModifiedStat.DualWieldAttackSpeedPercent:
			characterStats.DualWieldAttackSpeedMultiplier += appliedValue / 100f;
			break;
		case ModifiedStat.BonusShieldDeflection:
			characterStats.BonusShieldDeflection += (int)appliedValue;
			break;
		case ModifiedStat.ApplyWounds:
		{
			if (component.Unconscious || component.Dead)
			{
				break;
			}
			WoundsTrait woundsTrait = (WoundsTrait)characterStats.FindWoundsTrait();
			if (!(woundsTrait != null))
			{
				break;
			}
			float num6 = ParamsValue();
			float num7 = ParamsExtraValue();
			for (int j = 0; (float)j < num6; j++)
			{
				if (num7 > 0f)
				{
					float amount = woundsTrait.DamageNeededToWound();
					component.ApplyDamageDirectly(amount, DamagePacket.DamageType.Raw, Owner, this);
				}
				else
				{
					woundsTrait.AddNewWound();
				}
			}
			break;
		}
		case ModifiedStat.RangedMovingRecoveryReductionPct:
			characterStats.RangedMovingRecoveryReductionPct += appliedValue;
			break;
		case ModifiedStat.BonusGrazeToHitRatioMeleeOneHand:
			characterStats.BonusGrazeToHitPercentMeleeOneHanded += appliedValue;
			break;
		case ModifiedStat.BonusHitToCritRatioMeleeOneHand:
			characterStats.BonusHitToCritPercentMeleeOneHanded += appliedValue;
			break;
		case ModifiedStat.TwoHandedDeflectionBonus:
			characterStats.TwoHandedDeflectionBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusDamageByRacePercent:
			if ((int)Params.RaceType < characterStats.BonusDamagePerRace.Length)
			{
				characterStats.BonusDamagePerRace[(int)Params.RaceType] += appliedValue;
			}
			break;
		case ModifiedStat.BonusPotionEffectOrDurationPercent:
			characterStats.PotionEffectiveness += appliedValue / 100f;
			break;
		case ModifiedStat.ExtraSimultaneousHitDefenseBonus:
			characterStats.ExtraSimultaneousHitDefenseBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusWeaponSets:
			characterStats.BonusWeaponSets += (int)appliedValue;
			break;
		case ModifiedStat.BonusQuickSlots:
			characterStats.BonusQuickSlots += (int)appliedValue;
			break;
		case ModifiedStat.MeleeAttackSpeedPercent:
			characterStats.MeleeAttackSpeedMultiplier += appliedValue / 100f;
			break;
		case ModifiedStat.RangedAttackSpeedPercent:
			characterStats.RangedAttackSpeedMultiplier += appliedValue / 100f;
			break;
		case ModifiedStat.TrapAccuracy:
			characterStats.TrapAccuracyBonus += (int)appliedValue;
			break;
		case ModifiedStat.BonusDamageByTypePercent:
			if ((int)Params.DmgType < characterStats.BonusDamagePerType.Length)
			{
				characterStats.BonusDamagePerType[(int)Params.DmgType] += appliedValue;
			}
			break;
		case ModifiedStat.MeleeDTBypass:
			characterStats.MeleeDTBypass += appliedValue;
			break;
		case ModifiedStat.RangedDTBYpass:
			characterStats.RangedDTBypass += appliedValue;
			break;
		case ModifiedStat.GrimoireCooldownBonus:
			characterStats.GrimoireCooldownBonus += appliedValue;
			break;
		case ModifiedStat.WeaponSwitchCooldownBonus:
			characterStats.WeaponSwitchCooldownBonus += appliedValue;
			break;
		case ModifiedStat.ShortenAfflictionDuration:
			characterStats.AdjustStatusEffectDurationsFromAffliction(Params.AfflictionPrefab, appliedValue);
			break;
		case ModifiedStat.MaxFocus:
			characterStats.MaxFocusBonus += appliedValue;
			break;
		case ModifiedStat.TrapBonusDamageOrDurationPercent:
			characterStats.TrapDamageOrDurationMult += appliedValue / 100f;
			break;
		case ModifiedStat.BonusHitToCritPercentEnemyBelow10Percent:
			characterStats.BonusHitToCritPercentEnemyBelow10Percent += appliedValue / 100f;
			break;
		case ModifiedStat.ApplyPulsedAOE:
		{
			AttackPulsedAOE attackPulsedAOE = UnityEngine.Object.Instantiate(Params.AttackPrefab) as AttackPulsedAOE;
			if (ParamsValue() == 0f)
			{
				attackPulsedAOE.Owner = target;
			}
			else
			{
				attackPulsedAOE.Owner = Owner;
			}
			attackPulsedAOE.transform.parent = target.transform;
			if (AbilityOrigin != null)
			{
				attackPulsedAOE.ParentAttack = AbilityOrigin.Attack;
			}
			attackPulsedAOE.SkipAnimation = true;
			attackPulsedAOE.Launch(target, AbilityOrigin);
			break;
		}
		case ModifiedStat.WeapMinDamageMult:
			characterStats.WeaponDamageMinMult *= appliedValue;
			break;
		case ModifiedStat.NonEngageable:
		case ModifiedStat.BreakAllEngagement:
			AIController.BreakAllEngagements(target);
			break;
		case ModifiedStat.VeilDeflection:
			characterStats.VeilDeflectionBonus += (int)appliedValue;
			break;
		case ModifiedStat.MinorSpellReflection:
			m_generalCounter = 0u;
			break;
		case ModifiedStat.SpellReflection:
			m_generalCounter = 0u;
			break;
		case ModifiedStat.BonusCritHitMultiplierEnemyBelow10Percent:
			characterStats.CritHitDamageMultiplierBonusEnemyBelow10Percent += appliedValue;
			break;
		case ModifiedStat.Push:
			AttackBase.PushEnemyHelper(m_target, m_owner.transform.position, appliedValue, ParamsExtraValue());
			break;
		case ModifiedStat.TransferBeneficialTime:
		{
			if (!(m_owner != null))
			{
				break;
			}
			CharacterStats characterStats2 = m_ownerStats;
			Trap component8 = m_owner.GetComponent<Trap>();
			if (component8 != null && component8.Owner != null)
			{
				characterStats2 = component8.Owner.GetComponent<CharacterStats>();
			}
			if (characterStats2 != null)
			{
				float num5 = 0f - characterStats.AdjustBeneficialEffectTime(0f - appliedValue);
				if (num5 > 0f)
				{
					num5 *= ParamsExtraValue();
					characterStats2.SpreadBeneficialEffectTime(num5);
				}
			}
			break;
		}
		case ModifiedStat.SummonConsumable:
		{
			QuickbarInventory component7 = target.GetComponent<QuickbarInventory>();
			if (component7 != null && Params.ConsumablePrefab != null)
			{
				int num4 = (int)ParamsValue();
				if (num4 == 0)
				{
					num4 = characterStats.MaxQuickSlots;
				}
				component7.AddItem(Params.ConsumablePrefab, num4);
			}
			break;
		}
		case ModifiedStat.TransferAttackSpeed:
		{
			characterStats.AttackSpeedMultiplier *= 1f - ParamsValue();
			if (!(m_ownerStats != null))
			{
				break;
			}
			StatusEffect statusEffect = null;
			foreach (StatusEffect item2 in m_ownerStats.FindStatusEffectsOfType(ModifiedStat.AttackSpeed))
			{
				if (item2.Owner == Owner && item2.AbilityOrigin == AbilityOrigin)
				{
					statusEffect = item2;
					break;
				}
			}
			if (statusEffect == null)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = ModifiedStat.AttackSpeed;
				statusEffectParams.Value = 1f + ParamsValue();
				statusEffectParams.Duration = Params.Duration;
				statusEffectParams.LastsUntilCombatEnds = true;
				statusEffectParams.IsHostile = false;
				StatusEffect statusEffect2 = CreateChild(m_owner, statusEffectParams, AbilityType, null, deleteOnClear: true);
				statusEffect2.AbilityOrigin = AbilityOrigin;
				m_ownerStats.ApplyStatusEffectImmediate(statusEffect2);
			}
			else
			{
				m_ownerStats.AdjustStatusEffectDuration(statusEffect, Duration);
			}
			break;
		}
		case ModifiedStat.DamageToSummon:
			if (!component.Unconscious && !component.Dead)
			{
				if (IncreasePerTick > 0f)
				{
					component.ApplyDamageDirectly(appliedValue * IncreasePerTick * (float)m_intervalCount, Params.DmgType, Owner, this);
				}
				else
				{
					component.ApplyDamageDirectly(appliedValue, Params.DmgType, Owner, this);
				}
				if (m_owner != null && Params.AttackPrefab != null && Params.AttackPrefab is Summon && characterStats.CharacterRace != CharacterStats.Race.Spirit && (component.Unconscious || component.Dead))
				{
					Summon obj = UnityEngine.Object.Instantiate(Params.AttackPrefab) as Summon;
					obj.Owner = m_owner;
					obj.SkipAnimation = true;
					obj.DestroyAfterSummonEnds = true;
					obj.ForceInit();
					obj.Launch(target);
				}
			}
			break;
		case ModifiedStat.Destroy:
			if (!component.Unconscious && !component.Dead)
			{
				component.DestroyKill(Owner);
			}
			break;
		case ModifiedStat.LaunchAttackWithRollingBonus:
		{
			int bonusAcc = (int)(appliedValue * (float)m_generalCounter);
			float damageMult = 1f + (ParamsExtraValue() - 1f) * (float)m_generalCounter;
			LaunchAttack(target, bonusAcc, damageMult);
			m_generalCounter++;
			break;
		}
		case ModifiedStat.HealthPercent:
			if (!component.Dead)
			{
				component.ApplyHealthChangeDirectly(appliedValue * component.MaxHealth, m_owner, this, applyIfDead: true);
			}
			break;
		case ModifiedStat.NegateNextRecovery:
			characterStats.NegateNextRecovery = true;
			break;
		case ModifiedStat.RemoveAllEffectsByKeyword:
		{
			List<StatusEffect> list = new List<StatusEffect>();
			bool flag = ParamsExtraValue() == 0f;
			for (int num3 = characterStats.ActiveStatusEffects.Count - 1; num3 >= 0; num3--)
			{
				if ((flag || characterStats.ActiveStatusEffects[num3].Owner == Owner) && string.Compare(characterStats.ActiveStatusEffects[num3].Params.Tag, Params.Keyword, ignoreCase: true) == 0)
				{
					list.Add(characterStats.ActiveStatusEffects[num3]);
				}
			}
			characterStats.ClearEffectRange(list);
			break;
		}
		case ModifiedStat.VerticalLaunch:
		{
			AnimationController animationController = (target ? target.GetComponent<AnimationController>() : null);
			if ((bool)animationController)
			{
				float num2 = ParamsValue();
				float velocity = Mathf.Sqrt(80f * num2);
				animationController.VerticalLaunch(velocity);
			}
			break;
		}
		case ModifiedStat.EnemyGrazeToMissPercent:
			characterStats.EnemyGrazeToMissPercent += appliedValue;
			break;
		case ModifiedStat.StaminaByAthletics:
			if (!component.Dead)
			{
				float num = appliedValue;
				if (num > 0f && (Params.Apply == ApplyType.ApplyOverTime || (Params.Apply == ApplyType.ApplyOnTick && Duration >= 1f)))
				{
					num *= characterStats.StaminaRechargeMult;
				}
				component.ApplyStaminaChangeDirectly(num, m_owner, applyIfDead: false);
			}
			break;
		case ModifiedStat.RestoreSpiritshiftUses:
		{
			for (int i = 0; i < characterStats.ActiveAbilities.Count; i++)
			{
				if (characterStats.ActiveAbilities[i] is Spiritshift)
				{
					characterStats.ActiveAbilities[i].GiveUses((int)appliedValue);
				}
			}
			break;
		}
		case ModifiedStat.ApplyAffliction:
			characterStats.ApplyAffliction(Params.AfflictionPrefab, m_owner, AbilityType, null, deleteOnClear: true, ParamsValue(), null);
			break;
		case ModifiedStat.GenericMarker:
			if ((bool)m_ownerStats)
			{
				m_ownerStats.SetInflictedGenericMarker(Params.Tag);
			}
			break;
		case ModifiedStat.AttackOnHitWithMelee:
			m_generalCounter = 0u;
			break;
		}
		return true;
	}

	private AttackBase LaunchAttack(GameObject target, int bonusAcc, float damageMult)
	{
		if (m_owner != null && Params.AttackPrefab != null)
		{
			AttackBase attackBase = UnityEngine.Object.Instantiate(Params.AttackPrefab);
			attackBase.Owner = m_owner;
			attackBase.transform.parent = m_owner.transform;
			attackBase.SkipAnimation = true;
			attackBase.SkipAbilityModAttackStatusEffects = true;
			attackBase.TriggeringAbility = m_abilityOrigin;
			attackBase.EquippableOrigin = EquipmentOrigin;
			attackBase.DestroyAfterImpact = true;
			attackBase.AccuracyBonus += bonusAcc;
			attackBase.DamageData.Minimum *= damageMult;
			attackBase.DamageData.Maximum *= damageMult;
			AttackAOE attackAOE = attackBase as AttackAOE;
			if ((bool)attackAOE)
			{
				attackAOE.ShowImpactEffect(target.transform.position);
				attackAOE.OnImpactShared(null, m_owner.transform.position, null);
			}
			else
			{
				attackBase.Launch(target, AbilityOrigin);
			}
			return attackBase;
		}
		return null;
	}

	public void ClearEffect(GameObject target)
	{
		ClearEffect(target, triggerEffects: true);
	}

	public void ClearEffect(GameObject target, bool triggerEffects)
	{
		try
		{
			if (m_target == target)
			{
				m_tempAuraTargetKeys.Clear();
				m_tempAuraTargetKeys.AddRange(m_auraTargetsApplied.Keys);
				for (int i = 0; i < m_tempAuraTargetKeys.Count; i++)
				{
					RemoveAura(m_tempAuraTargetKeys[i]);
				}
			}
			if (!m_applied)
			{
				return;
			}
			if (m_target == target && m_triggerCount == 1 && Params.TriggerAdjustment.ResetTriggerOnEffectEnd)
			{
				OffTrigger();
				return;
			}
			RemoveLoopingEffects();
			if (target == null)
			{
				return;
			}
			if (triggerEffects)
			{
				Transform transform = AttackBase.GetTransform(target, Params.VisualEffectAttach);
				GameUtilities.LaunchEffect(m_params.OnStopVisualEffect, 1f, transform, m_abilityOrigin);
			}
			if (AfflictionOrigin != null && !AfflictionOrigin.Material.Empty)
			{
				AfflictionOrigin.Material.Restore(target);
			}
			if (AbilityOrigin != null && !MaterialReplacement.IsNullOrEmpty(AbilityOrigin.SelfMaterialReplacement) && Target == Owner)
			{
				AbilityOrigin.DeactivateMaterialReplacement();
			}
			CharacterStats characterStats = m_targetStats;
			if (target != m_target || characterStats == null)
			{
				characterStats = target.GetComponent<CharacterStats>();
			}
			Health component = target.GetComponent<Health>();
			Mover component2 = target.GetComponent<Mover>();
			if (m_abilityOrigin != null)
			{
				m_abilityOrigin.HandleOnMyEffectRemoved();
			}
			if (characterStats == null || component == null)
			{
				UnityEngine.Debug.LogError(target.name + " doesn't have a health and/or CharacterStats component. Unable to remove status effect!");
				return;
			}
			float currentAppliedValue = CurrentAppliedValue;
			if (m_target != target || m_effect_is_on_main_target)
			{
				switch (Params.AffectsStat)
				{
				case ModifiedStat.MaxHealth:
					characterStats.MaxHealth -= currentAppliedValue;
					if (component.CurrentHealth > component.MaxHealth)
					{
						component.CurrentHealth = component.MaxHealth;
					}
					break;
				case ModifiedStat.MaxStamina:
					characterStats.StaminaBonus -= currentAppliedValue;
					if (component.CurrentStamina > component.MaxStamina)
					{
						component.CurrentStamina = component.MaxStamina;
					}
					break;
				case ModifiedStat.StaminaRechargeRate:
					characterStats.StaminaRechargeBonus -= currentAppliedValue;
					break;
				case ModifiedStat.AttackSpeed:
					characterStats.AttackSpeedMultiplier /= currentAppliedValue;
					if ((characterStats.AttackSpeedMultiplier >= 1f) & (characterStats.AttackSpeedMultiplier <= 1f))
					{
						characterStats.AttackSpeedMultiplier = 1f;
					}
					break;
				case ModifiedStat.Stealth:
					characterStats.StealthBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusDamage:
					if (Params.DmgType < DamagePacket.DamageType.Count)
					{
						characterStats.BonusDamage[(int)Params.DmgType] -= currentAppliedValue;
					}
					else if (Params.DmgType == DamagePacket.DamageType.All)
					{
						for (int l = 0; l < 7; l++)
						{
							characterStats.BonusDamage[l] -= currentAppliedValue;
						}
					}
					break;
				case ModifiedStat.DamageMinimum:
					characterStats.DamageMinBonus -= currentAppliedValue;
					break;
				case ModifiedStat.MovementRate:
					if ((bool)component2)
					{
						component2.SetRunSpeed(component2.RunSpeed - currentAppliedValue);
					}
					break;
				case ModifiedStat.NonTargetable:
					component.Targetable = true;
					break;
				case ModifiedStat.NonMobile:
					if ((bool)component2)
					{
						component2.Frozen = false;
					}
					break;
				case ModifiedStat.KnockedDown:
					SendOffEventToAi(GameEventType.KnockedDown, target);
					break;
				case ModifiedStat.Stunned:
					SendOffEventToAi(GameEventType.Stunned, target);
					break;
				case ModifiedStat.EngagedEnemyCount:
					characterStats.EngageableEnemyCount -= (int)currentAppliedValue;
					if (characterStats.EngageableEnemyCount < 0)
					{
						characterStats.EngageableEnemyCount = 0;
					}
					if ((int)currentAppliedValue > 0)
					{
						AIController aIController3 = GameUtilities.FindActiveAIController(target);
						if (aIController3 != null && aIController3.gameObject.activeInHierarchy)
						{
							aIController3.CancelAllEngagements();
							aIController3.UpdateEngagement(Owner, AIController.GetPrimaryAttack(target));
						}
					}
					break;
				case ModifiedStat.EngagementRadius:
					characterStats.EngagementDistanceBonus -= currentAppliedValue;
					break;
				case ModifiedStat.DisengagementAccuracy:
					characterStats.DisengagementAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.MeleeAttackDistanceMult:
					characterStats.MeleeAttackDistanceMultiplier = 1f;
					break;
				case ModifiedStat.RangedAttackDistanceMult:
					characterStats.RangedAttackDistanceMultiplier = 1f;
					break;
				case ModifiedStat.BonusDTFromArmor:
					characterStats.BonusDTFromArmor -= currentAppliedValue;
					break;
				case ModifiedStat.MeleeDamageRangePctIncreaseToMin:
					characterStats.MeleeDamageRangePctIncreaseToMin -= currentAppliedValue;
					break;
				case ModifiedStat.SuspendHostileEffects:
					characterStats.UnsuspendEffects(CharacterStats.EffectType.Hostile, m_timeApplied);
					break;
				case ModifiedStat.ImmuneToEngageStop:
					characterStats.ImmuneToEngageStop = false;
					break;
				case ModifiedStat.DrainResolveForDeflection:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Resolve, -(int)currentAppliedValue);
					break;
				case ModifiedStat.ReapplyDamage:
				case ModifiedStat.ReapplyDamageToNearbyEnemies:
					component.OnDamaged -= HandleOnDamagedForReapply;
					break;
				case ModifiedStat.ReloadSpeed:
					characterStats.ReloadSpeedMultiplier /= currentAppliedValue;
					if ((characterStats.ReloadSpeedMultiplier >= 1f) & (characterStats.ReloadSpeedMultiplier <= 1f))
					{
						characterStats.ReloadSpeedMultiplier = 1f;
					}
					break;
				case ModifiedStat.StasisShield:
					characterStats.OnPreDamageApplied -= HandleStatsOnPreDamageAppliedForAbsorb;
					SendOffEventToAi(GameEventType.Stunned, target);
					break;
				case ModifiedStat.DamageShield:
					characterStats.OnPreDamageApplied -= HandleStatsOnPreDamageAppliedForAbsorb;
					break;
				case ModifiedStat.SuspendBeneficialEffects:
					characterStats.UnsuspendEffects(CharacterStats.EffectType.Beneficial, m_timeApplied);
					break;
				case ModifiedStat.Resolve:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Resolve, -(int)currentAppliedValue);
					break;
				case ModifiedStat.Might:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Might, -(int)currentAppliedValue);
					break;
				case ModifiedStat.Dexterity:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Dexterity, -(int)currentAppliedValue);
					break;
				case ModifiedStat.Intellect:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Intellect, -(int)currentAppliedValue);
					break;
				case ModifiedStat.SummonWeapon:
				{
					Equipment component6 = target.GetComponent<Equipment>();
					if ((bool)component6 && component6.PopSummonedWeapon(Equippable.EquipmentSlot.PrimaryWeapon))
					{
						PartyMemberAI component7 = target.GetComponent<PartyMemberAI>();
						if (component7 != null)
						{
							component7.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: true);
						}
					}
					break;
				}
				case ModifiedStat.SummonSecondaryWeapon:
				{
					Equipment component4 = target.GetComponent<Equipment>();
					if ((bool)component4 && component4.PopSummonedWeapon(Equippable.EquipmentSlot.SecondaryWeapon))
					{
						PartyMemberAI component5 = target.GetComponent<PartyMemberAI>();
						if (component5 != null)
						{
							component5.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: true);
						}
					}
					break;
				}
				case ModifiedStat.StaminaRechargeRateMult:
					if (currentAppliedValue != 0f)
					{
						characterStats.StaminaRechargeMult /= currentAppliedValue;
					}
					break;
				case ModifiedStat.VesselAccuracy:
					characterStats.VesselAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BeastAccuracy:
					characterStats.BeastAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.WilderAccuracy:
					characterStats.WilderAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.StunDefense:
					characterStats.StunDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.KnockdownDefense:
					characterStats.KnockdownDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.PoisonDefense:
					characterStats.PoisonDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.DiseaseDefense:
					characterStats.DiseaseDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusAccuracyForNearestAllyOnSameEnemy:
					characterStats.NearestAllyWithSharedTargetAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.EnemyCritToHitPercent:
					characterStats.EnemyCritToHitPercent -= currentAppliedValue;
					break;
				case ModifiedStat.HostileEffectDurationMult:
					characterStats.HostileEffectDurationMultiplier /= currentAppliedValue;
					break;
				case ModifiedStat.EnemyDeflectReflexHitToGrazePercent:
					characterStats.EnemyDeflectReflexHitToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.EnemyFortitudeWillHitToGrazePercent:
					characterStats.EnemyFortitudeWillHitToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.ExtraStraightBounces:
					characterStats.ExtraStraightBounces -= (int)currentAppliedValue;
					break;
				case ModifiedStat.RedirectMeleeAttacks:
					characterStats.RedirectMeleeAttacks = false;
					break;
				case ModifiedStat.HostileAOEDamageMultiplier:
					characterStats.HostileAOEDamageMultiplier /= currentAppliedValue;
					break;
				case ModifiedStat.ImprovedFlanking:
					characterStats.ImprovedFlanking--;
					break;
				case ModifiedStat.DTBypass:
					characterStats.DTBypass -= (int)currentAppliedValue;
					break;
				case ModifiedStat.StealSpell:
					DestroyAbilities();
					break;
				case ModifiedStat.GrantAbility:
					DestroyAbilities();
					break;
				case ModifiedStat.SwapFaction:
				{
					AIController aIController = GameUtilities.FindActiveAIController(target);
					PartyMemberAI component8 = target.GetComponent<PartyMemberAI>();
					AIController aIController2 = null;
					if (aIController != null && aIController.IsConfused)
					{
						aIController.RemoveConfusion();
					}
					Faction component9 = target.GetComponent<Faction>();
					if (component9 != null)
					{
						component9.CurrentTeamInstance = m_cachedTeam;
						m_cachedTeam = null;
					}
					if ((bool)component8)
					{
						if (component8.StateManager != null)
						{
							component8.StateManager.PopAllStates();
						}
						AIPackageController component10 = target.GetComponent<AIPackageController>();
						if (component10 != null)
						{
							component10.TransferSummonedCreatures(component8);
							if (component10.StateManager != null)
							{
								component10.SafePopAllStates();
							}
							component10.enabled = false;
							aIController2 = component10;
						}
						component8.enabled = true;
					}
					if (!(aIController != null) || aIController.StateManager == null)
					{
						break;
					}
					AIState currentState = aIController.StateManager.CurrentState;
					if (currentState == null)
					{
						break;
					}
					if (currentState.Priority < 3)
					{
						aIController.SafePopAllStates();
						break;
					}
					aIController.StateManager.ClearQueuedStates();
					if (aIController2 != null && component8 != null && currentState.Priority > 3)
					{
						aIController2.StateManager.TransferCurrentState(component8.StateManager);
					}
					break;
				}
				case ModifiedStat.Athletics:
					characterStats.AthleticsBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.Lore:
					characterStats.LoreBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.Mechanics:
					characterStats.MechanicsBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.Survival:
					characterStats.SurvivalBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.Crafting:
					characterStats.CraftingBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.PushDefense:
					characterStats.PushDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.WhileStunnedDefense:
					characterStats.WhileStunnedDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.WhileKnockeddownDefense:
					characterStats.WhileKnockeddownDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.Constitution:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Constitution, -(int)currentAppliedValue);
					break;
				case ModifiedStat.Perception:
					characterStats.AddAttributeBonus(CharacterStats.AttributeScoreType.Perception, -(int)currentAppliedValue);
					break;
				case ModifiedStat.CritHitMultiplierBonus:
					characterStats.CritHitDamageMultiplierBonus -= currentAppliedValue;
					break;
				case ModifiedStat.BonusGrazeToHitPercent:
					characterStats.BonusGrazeToHitPercent -= currentAppliedValue;
					break;
				case ModifiedStat.BonusGrazeToMissPercent:
					characterStats.BonusGrazeToMissPercent -= currentAppliedValue;
					break;
				case ModifiedStat.BonusCritToHitPercent:
					characterStats.BonusCritToHitPercent -= currentAppliedValue;
					break;
				case ModifiedStat.BonusMissToGrazePercent:
					characterStats.BonusMissToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.BonusHitToCritPercent:
					characterStats.BonusHitToCritPercent -= currentAppliedValue;
					break;
				case ModifiedStat.BonusHitToCritPercentAll:
					characterStats.BonusHitToCritPercentAll -= currentAppliedValue;
					break;
				case ModifiedStat.BonusHitToGrazePercent:
					characterStats.BonusHitToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.Confused:
					SendOffEventToAi(GameEventType.Confused, target);
					break;
				case ModifiedStat.RateOfFireMult:
					characterStats.RateOfFireMultiplier /= currentAppliedValue;
					if ((characterStats.RateOfFireMultiplier >= 1f) & (characterStats.RateOfFireMultiplier <= 1f))
					{
						characterStats.RateOfFireMultiplier = 1f;
					}
					break;
				case ModifiedStat.EnemyReflexGrazeToMissPercent:
					characterStats.EnemyReflexGrazeToMissPercent -= currentAppliedValue;
					break;
				case ModifiedStat.EnemyReflexHitToGrazePercent:
					characterStats.EnemyReflexHitToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.EnemiesNeededToFlankAdj:
					characterStats.EnemiesNeededToFlank -= (int)currentAppliedValue;
					characterStats.CheckToRemoveFlanked();
					characterStats.CheckToAddFlankedAll();
					break;
				case ModifiedStat.ResistAffliction:
					if (m_listenersAttached)
					{
						characterStats.OnDefenseAdjustment -= AdjustDefenseAffliction;
						m_listenersAttached = false;
					}
					break;
				case ModifiedStat.PreventDeath:
					characterStats.DeathPrevented--;
					break;
				case ModifiedStat.DOTTickMult:
					characterStats.DOTTickMult /= currentAppliedValue;
					break;
				case ModifiedStat.ResistKeyword:
					if (m_listenersAttached)
					{
						characterStats.OnDefenseAdjustment -= AdjustDefenseKeyword;
						m_listenersAttached = false;
					}
					break;
				case ModifiedStat.KeywordImmunity:
					if (m_listenersAttached)
					{
						characterStats.OnCheckImmunity -= AdjustDefenseKeywordImmune;
						m_listenersAttached = false;
					}
					break;
				case ModifiedStat.TransferDT:
					if (!(m_owner != null) || !(m_ownerStats != null))
					{
						break;
					}
					m_ownerStats.DamageThreshhold[(int)Params.DmgType] -= currentAppliedValue;
					characterStats.DamageThreshhold[(int)Params.DmgType] += currentAppliedValue;
					if (Params.DmgType == DamagePacket.DamageType.All)
					{
						for (int k = 0; k < 7; k++)
						{
							m_ownerStats.DamageThreshhold[k] -= currentAppliedValue;
							characterStats.DamageThreshhold[k] += currentAppliedValue;
						}
					}
					break;
				case ModifiedStat.TransferRandomAttribute:
					characterStats.AddAttributeBonus((CharacterStats.AttributeScoreType)m_generalCounter, (int)currentAppliedValue);
					if ((bool)m_ownerStats)
					{
						m_ownerStats.AddAttributeBonus((CharacterStats.AttributeScoreType)m_generalCounter, -(int)currentAppliedValue);
					}
					break;
				case ModifiedStat.TransferAttribute:
					characterStats.AddAttributeBonus(Params.AttributeType, (int)currentAppliedValue);
					if ((bool)m_ownerStats)
					{
						m_ownerStats.AddAttributeBonus(Params.AttributeType, -(int)currentAppliedValue);
					}
					break;
				case ModifiedStat.EnemyHitToGrazePercent:
					characterStats.EnemyHitToGrazePercent -= currentAppliedValue;
					break;
				case ModifiedStat.PrimordialAccuracy:
					characterStats.PrimordialAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.StopAnimation:
					SendOffEventToAi(GameEventType.Paralyzed, target);
					break;
				case ModifiedStat.AddAfflictionImmunity:
					if (Params.AfflictionPrefab != null)
					{
						characterStats.AfflictionImmunities.Remove(Params.AfflictionPrefab);
					}
					break;
				case ModifiedStat.Invisible:
					characterStats.InvisibilityState--;
					break;
				case ModifiedStat.WoundDelay:
					characterStats.WoundDelay -= currentAppliedValue;
					break;
				case ModifiedStat.FinishingBlowDamageMult:
					characterStats.FinishingBlowDamageMult /= currentAppliedValue;
					break;
				case ModifiedStat.ZealousAuraAoEMult:
					characterStats.ZealousAuraRadiusMult /= Mathf.Sqrt(currentAppliedValue);
					break;
				case ModifiedStat.DelayUnconsciousness:
					characterStats.UnconsciousnessDelayed--;
					break;
				case ModifiedStat.NegMoveTickMult:
					characterStats.NegMoveTickMult /= currentAppliedValue;
					break;
				case ModifiedStat.FocusGainMult:
					characterStats.FocusGainMult /= currentAppliedValue;
					break;
				case ModifiedStat.DisengagementDefense:
					characterStats.DisengagementDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.SpellDefense:
					characterStats.SpellDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.RangedDeflection:
					characterStats.RangedDeflectionBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusUsesPerRestPastThree:
					characterStats.BonusUsesPerRestPastThree -= (int)currentAppliedValue;
					break;
				case ModifiedStat.PoisonTickMult:
					characterStats.PoisonTickMult /= currentAppliedValue;
					break;
				case ModifiedStat.DiseaseTickMult:
					characterStats.DiseaseTickMult /= currentAppliedValue;
					break;
				case ModifiedStat.StalkersLinkDamageMult:
					characterStats.StalkersLinkDamageMult /= currentAppliedValue;
					break;
				case ModifiedStat.ChanterPhraseAoEMult:
					characterStats.ChanterPhraseRadiusMult /= Mathf.Sqrt(currentAppliedValue);
					break;
				case ModifiedStat.BonusHealMult:
					characterStats.BonusHealMult /= currentAppliedValue;
					break;
				case ModifiedStat.BonusHealingGivenMult:
					characterStats.BonusHealingGivenMult /= currentAppliedValue;
					break;
				case ModifiedStat.SpellCastBonus:
				{
					int num3 = (int)ParamsExtraValue() - 1;
					if (num3 >= 0 && num3 < 8)
					{
						characterStats.SpellCastBonus[num3] -= (int)currentAppliedValue;
					}
					break;
				}
				case ModifiedStat.AoEMult:
					characterStats.AoERadiusMult /= Mathf.Sqrt(currentAppliedValue);
					break;
				case ModifiedStat.WildstrikeDamageMult:
					characterStats.WildstrikeDamageMult -= currentAppliedValue - 1f;
					break;
				case ModifiedStat.MaxStaminaMult:
					characterStats.MaxStaminaMultiplier /= currentAppliedValue;
					break;
				case ModifiedStat.CallbackOnDamaged:
					if (Params.OnDamagedCallbackAbility != null)
					{
						component.OnDamaged -= Params.OnDamagedCallbackAbility.HandleOnDamaged;
					}
					break;
				case ModifiedStat.CallbackAfterAttack:
					if (m_abilityOrigin != null)
					{
						characterStats.OnPostDamageDealt -= m_abilityOrigin.HandleStatsOnPostDamageDealtCallback;
						m_abilityOrigin.HandleStatsOnPostDamageDealtCallbackComplete();
					}
					break;
				case ModifiedStat.GivePlayerBonusMoneyViaStrongholdTurn:
					if ((bool)Stronghold.Instance)
					{
						Stronghold.Instance.BonusTurnMoney -= (int)currentAppliedValue;
					}
					break;
				case ModifiedStat.ArmorSpeedFactorAdj:
					characterStats.ArmorSpeedFactorAdj -= currentAppliedValue;
					break;
				case ModifiedStat.SingleWeaponSpeedFactorAdj:
					characterStats.SingleWeaponSpeedFactorAdj -= currentAppliedValue;
					break;
				case ModifiedStat.BonusRangedWeaponCloseEnemyDamageMult:
					characterStats.BonusRangedWeaponCloseEnemyDamageMult /= currentAppliedValue;
					if ((characterStats.BonusRangedWeaponCloseEnemyDamageMult >= 1f) & (characterStats.BonusRangedWeaponCloseEnemyDamageMult <= 1f))
					{
						characterStats.BonusRangedWeaponCloseEnemyDamageMult = 1f;
					}
					break;
				case ModifiedStat.DualWieldAttackSpeedPercent:
					characterStats.DualWieldAttackSpeedMultiplier -= currentAppliedValue / 100f;
					if ((characterStats.DualWieldAttackSpeedMultiplier >= 1f) & (characterStats.DualWieldAttackSpeedMultiplier <= 1f))
					{
						characterStats.DualWieldAttackSpeedMultiplier = 1f;
					}
					break;
				case ModifiedStat.BonusShieldDeflection:
					characterStats.BonusShieldDeflection -= (int)currentAppliedValue;
					break;
				case ModifiedStat.RangedMovingRecoveryReductionPct:
					characterStats.RangedMovingRecoveryReductionPct -= currentAppliedValue;
					break;
				case ModifiedStat.BonusGrazeToHitRatioMeleeOneHand:
					characterStats.BonusGrazeToHitPercentMeleeOneHanded -= currentAppliedValue;
					break;
				case ModifiedStat.BonusHitToCritRatioMeleeOneHand:
					characterStats.BonusHitToCritPercentMeleeOneHanded -= currentAppliedValue;
					break;
				case ModifiedStat.TwoHandedDeflectionBonus:
					characterStats.TwoHandedDeflectionBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusDamageByRacePercent:
					if ((int)Params.RaceType < characterStats.BonusDamagePerRace.Length)
					{
						characterStats.BonusDamagePerRace[(int)Params.RaceType] -= currentAppliedValue;
					}
					break;
				case ModifiedStat.BonusPotionEffectOrDurationPercent:
					characterStats.PotionEffectiveness -= currentAppliedValue / 100f;
					characterStats.PotionEffectiveness = CleanFloatingPoint(characterStats.PotionEffectiveness, 1f);
					break;
				case ModifiedStat.ExtraSimultaneousHitDefenseBonus:
					characterStats.ExtraSimultaneousHitDefenseBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusWeaponSets:
					characterStats.BonusWeaponSets -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusQuickSlots:
					characterStats.BonusQuickSlots -= (int)currentAppliedValue;
					break;
				case ModifiedStat.MeleeAttackSpeedPercent:
					characterStats.MeleeAttackSpeedMultiplier -= currentAppliedValue / 100f;
					characterStats.MeleeAttackSpeedMultiplier = CleanFloatingPoint(characterStats.MeleeAttackSpeedMultiplier, 1f);
					break;
				case ModifiedStat.RangedAttackSpeedPercent:
					characterStats.RangedAttackSpeedMultiplier -= currentAppliedValue / 100f;
					characterStats.RangedAttackSpeedMultiplier = CleanFloatingPoint(characterStats.RangedAttackSpeedMultiplier, 1f);
					break;
				case ModifiedStat.TrapAccuracy:
					characterStats.TrapAccuracyBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusDamageByTypePercent:
					if ((int)Params.DmgType < characterStats.BonusDamagePerType.Length)
					{
						characterStats.BonusDamagePerType[(int)Params.DmgType] -= currentAppliedValue;
					}
					break;
				case ModifiedStat.MeleeDTBypass:
					characterStats.MeleeDTBypass -= currentAppliedValue;
					break;
				case ModifiedStat.RangedDTBYpass:
					characterStats.RangedDTBypass -= currentAppliedValue;
					break;
				case ModifiedStat.GrimoireCooldownBonus:
					characterStats.GrimoireCooldownBonus -= currentAppliedValue;
					break;
				case ModifiedStat.WeaponSwitchCooldownBonus:
					characterStats.WeaponSwitchCooldownBonus -= currentAppliedValue;
					break;
				case ModifiedStat.MaxFocus:
					characterStats.MaxFocusBonus -= currentAppliedValue;
					break;
				case ModifiedStat.TrapBonusDamageOrDurationPercent:
					characterStats.TrapDamageOrDurationMult -= currentAppliedValue / 100f;
					characterStats.TrapDamageOrDurationMult = CleanFloatingPoint(characterStats.TrapDamageOrDurationMult, 1f);
					break;
				case ModifiedStat.BonusHitToCritPercentEnemyBelow10Percent:
					characterStats.BonusHitToCritPercentEnemyBelow10Percent -= currentAppliedValue / 100f;
					characterStats.BonusHitToCritPercentEnemyBelow10Percent = CleanFloatingPoint(characterStats.BonusHitToCritPercentEnemyBelow10Percent, 0f);
					break;
				case ModifiedStat.WeapMinDamageMult:
					characterStats.WeaponDamageMinMult /= currentAppliedValue;
					break;
				case ModifiedStat.VeilDeflection:
					characterStats.VeilDeflectionBonus -= (int)currentAppliedValue;
					break;
				case ModifiedStat.BonusCritHitMultiplierEnemyBelow10Percent:
					characterStats.CritHitDamageMultiplierBonusEnemyBelow10Percent -= currentAppliedValue;
					break;
				case ModifiedStat.SummonConsumable:
				{
					QuickbarInventory component3 = target.GetComponent<QuickbarInventory>();
					if (!(component3 != null) || !(Params.ConsumablePrefab != null))
					{
						break;
					}
					bool flag = false;
					for (int j = 0; j < characterStats.ActiveStatusEffects.Count; j++)
					{
						StatusEffect statusEffect = characterStats.ActiveStatusEffects[j];
						if (statusEffect != this && statusEffect.Params == Params && statusEffect.Applied)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int num2 = (int)ParamsValue();
						if (num2 == 0)
						{
							num2 = characterStats.MaxQuickSlots;
						}
						component3.DestroyItem(Params.ConsumablePrefab, num2);
					}
					break;
				}
				case ModifiedStat.TransferAttackSpeed:
					characterStats.AttackSpeedMultiplier /= 1f - ParamsValue();
					if ((characterStats.AttackSpeedMultiplier >= 1f) & (characterStats.AttackSpeedMultiplier <= 1f))
					{
						characterStats.AttackSpeedMultiplier = 1f;
					}
					break;
				case ModifiedStat.EnemyGrazeToMissPercent:
					characterStats.EnemyGrazeToMissPercent -= currentAppliedValue;
					break;
				case ModifiedStat.DropTrap:
				{
					if (Traps == null || ParamsExtraValue() == 0f)
					{
						break;
					}
					for (int num = Traps.Count - 1; num >= 0; num--)
					{
						if ((bool)Traps[num])
						{
							Traps[num].DestroyTrap();
						}
					}
					Traps.Clear();
					break;
				}
				}
			}
			if (m_target == target)
			{
				m_applied = false;
				m_effect_is_on_main_target = false;
				RemoveTriggerCallback();
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			if ((bool)UIDebug.Instance)
			{
				UIDebug.Instance.LogOnScreenWarning("Exception in StatusEffect.ClearEffect(), please fix! Attempting to gracefully continue.", UIDebug.Department.Programming, 10f);
			}
		}
	}

	private float CleanFloatingPoint(float numberToClean, float originalNumber)
	{
		if (numberToClean >= originalNumber - float.Epsilon && numberToClean <= originalNumber + float.Epsilon)
		{
			numberToClean = originalNumber;
		}
		return numberToClean;
	}

	public void RemoveEffect()
	{
		if (m_timeActive <= 0f)
		{
			m_timeActive = 0.01f;
		}
		Duration = m_timeActive;
		RemovingEffect = true;
	}

	public int AdjustDefense(GameObject attacker, GameObject self, CharacterStats.DefenseType defenseType)
	{
		if (!m_applied)
		{
			return 0;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.Deflection:
			if (defenseType == CharacterStats.DefenseType.Deflect)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.Reflex:
			if (defenseType == CharacterStats.DefenseType.Reflex)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.Fortitude:
			if (defenseType == CharacterStats.DefenseType.Fortitude)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.Will:
			if (defenseType == CharacterStats.DefenseType.Will)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.AllDefense:
			return (int)currentAppliedValue;
		case ModifiedStat.AllDefensesExceptDeflection:
			if (defenseType == CharacterStats.DefenseType.Fortitude || defenseType == CharacterStats.DefenseType.Will || defenseType == CharacterStats.DefenseType.Reflex)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.DistantEnemyBonus:
			if (defenseType == CharacterStats.DefenseType.Deflect || defenseType == CharacterStats.DefenseType.Reflex)
			{
				CharacterStats characterStats = (self ? self.GetComponent<CharacterStats>() : null);
				if ((bool)characterStats && (bool)attacker && characterStats.IsEnemyDistant(attacker))
				{
					return (int)currentAppliedValue;
				}
			}
			break;
		}
		return 0;
	}

	public int AdjustAccuracy(GameObject self, GameObject enemy, AttackBase attack)
	{
		if (!m_applied)
		{
			return 0;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.Accuracy:
			return (int)currentAppliedValue;
		case ModifiedStat.MeleeAccuracy:
			if (attack is AttackMelee)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.MeleeWeaponAccuracy:
			if (attack is AttackMelee && attack.IsAutoAttack())
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.RangedAccuracy:
			if (attack is AttackRanged || attack is AttackBeam)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.UnarmedAccuracy:
		{
			AttackMelee attackMelee = attack as AttackMelee;
			if ((bool)attackMelee && attackMelee.Unarmed)
			{
				return (int)currentAppliedValue;
			}
			break;
		}
		case ModifiedStat.DistantEnemyWeaponAccuracyBonus:
			if (attack is AttackRanged && !attack.AbilityOrigin)
			{
				CharacterStats characterStats = (self ? self.GetComponent<CharacterStats>() : null);
				if ((bool)characterStats && (bool)enemy && characterStats.IsEnemyDistant(enemy))
				{
					return (int)currentAppliedValue;
				}
			}
			break;
		case ModifiedStat.BonusAccuracyAtLowStamina:
			if (m_generalCounter == 1)
			{
				return (int)currentAppliedValue;
			}
			break;
		case ModifiedStat.AccuracyByClass:
			if ((bool)enemy)
			{
				CharacterStats component2 = enemy.GetComponent<CharacterStats>();
				if ((bool)component2 && component2.CharacterClass == Params.ClassType)
				{
					return (int)currentAppliedValue;
				}
			}
			break;
		case ModifiedStat.DistantEnemyBonus:
			if ((bool)self && (bool)enemy)
			{
				CharacterStats component3 = self.GetComponent<CharacterStats>();
				if ((bool)component3 && component3.IsEnemyDistant(enemy))
				{
					return (int)currentAppliedValue;
				}
			}
			break;
		case ModifiedStat.AccuracyByWeaponType:
			if (attack != null && Params.EquippablePrefab != null && Params.EquippablePrefab is Weapon)
			{
				Weapon component = attack.gameObject.GetComponent<Weapon>();
				if (component != null && component.WeaponType == (Params.EquippablePrefab as Weapon).WeaponType)
				{
					return (int)currentAppliedValue;
				}
			}
			break;
		}
		return 0;
	}

	public float AdjustDamageThreshold(GameObject self, DamagePacket.DamageType damageType, bool ignoreArmor = false)
	{
		if (!m_applied)
		{
			return 0f;
		}
		float num = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.DamageThreshhold:
		{
			Armor armor = null;
			if (Slot != Equippable.EquipmentSlot.None && (bool)EquipmentOrigin)
			{
				armor = EquipmentOrigin.GetComponent<Armor>();
			}
			if ((bool)armor && !ignoreArmor)
			{
				num = armor.AdjustForDamageType(num, damageType);
			}
			if (Params.DmgType == DamagePacket.DamageType.All)
			{
				return num;
			}
			if (Params.DmgType == damageType)
			{
				return num;
			}
			break;
		}
		case ModifiedStat.BonusDTFromWounds:
		{
			CharacterStats component = self.GetComponent<CharacterStats>();
			GenericAbility genericAbility = (component ? component.FindWoundsTrait() : null);
			if ((bool)genericAbility)
			{
				return num * (float)component.CountStatusEffects(genericAbility.StatusEffects[0].Tag);
			}
			break;
		}
		case ModifiedStat.AddDamageTypeImmunity:
			if (Params.DmgType == DamagePacket.DamageType.All)
			{
				return float.PositiveInfinity;
			}
			if (Params.DmgType == damageType)
			{
				return float.PositiveInfinity;
			}
			break;
		}
		return 0f;
	}

	public float AdjustDamage(GameObject self, GameObject enemy, AttackBase attack)
	{
		if (!m_applied)
		{
			return 0f;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.BonusUnarmedDamage:
		{
			AttackMelee attackMelee = attack as AttackMelee;
			if ((bool)attackMelee && attackMelee.Unarmed)
			{
				return currentAppliedValue;
			}
			break;
		}
		case ModifiedStat.BonusMeleeDamage:
			if (attack is AttackMelee)
			{
				return currentAppliedValue;
			}
			break;
		case ModifiedStat.DisengagementDamage:
			if (attack.IsDisengagementAttack)
			{
				return currentAppliedValue;
			}
			break;
		}
		return 0f;
	}

	public float AdjustDamageMultiplier(GameObject self, GameObject enemy, AttackBase attack)
	{
		if (!m_applied)
		{
			return 1f;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.BonusMeleeWeaponDamageMult:
		case ModifiedStat.BonusMeleeDamageMult:
			if (attack is AttackMelee && attack.IsAutoAttack())
			{
				return currentAppliedValue;
			}
			break;
		case ModifiedStat.BonusRangedWeaponDamageMult:
			if (attack is AttackRanged && attack.IsAutoAttack())
			{
				return currentAppliedValue;
			}
			break;
		case ModifiedStat.BonusTwoHandedMeleeWeaponDamageMult:
			if (attack is AttackMelee)
			{
				Weapon component2 = attack.GetComponent<Weapon>();
				if ((bool)component2 && component2.BothPrimaryAndSecondarySlot)
				{
					return currentAppliedValue;
				}
			}
			break;
		case ModifiedStat.DamageMultByClass:
			if ((bool)enemy)
			{
				CharacterStats component = enemy.GetComponent<CharacterStats>();
				if ((bool)component && component.CharacterClass == Params.ClassType)
				{
					return currentAppliedValue;
				}
			}
			break;
		}
		return 1f;
	}

	public int AdjustAttackerAccuracy(GameObject attacker, GameObject enemy, AttackBase attack)
	{
		if (!m_applied)
		{
			return 0;
		}
		float currentAppliedValue = CurrentAppliedValue;
		ModifiedStat affectsStat = Params.AffectsStat;
		if (affectsStat == ModifiedStat.AccuracyBonusForAttackersWithAffliction)
		{
			CharacterStats characterStats = (attacker ? attacker.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats && characterStats.HasStatusEffectFromAffliction(Params.AfflictionPrefab))
			{
				return (int)currentAppliedValue;
			}
		}
		return 0;
	}

	public void WhenAttacked(GameObject attacker, GameObject enemy, AttackBase attack)
	{
		if (m_applied && !(attacker == enemy) && !(enemy == null))
		{
			_ = Params.AffectsStat;
		}
	}

	public void WhenLaunchesAttack(GameObject attacker, GameObject enemy, AttackBase attack)
	{
		if (!m_applied || attacker == enemy || enemy == null)
		{
			return;
		}
		float num = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.SneakAttackOnNearDead:
		{
			Health component10 = enemy.GetComponent<Health>();
			if (!component10 || !(component10.CurrentHealth / component10.MaxHealth <= num))
			{
				break;
			}
			CharacterStats component11 = attacker.GetComponent<CharacterStats>();
			if ((bool)component11)
			{
				StatusEffectParams statusEffectParams6 = new StatusEffectParams();
				statusEffectParams6.AffectsStat = ModifiedStat.BonusDamage;
				statusEffectParams6.DmgType = DamagePacket.DamageType.Crush;
				Equipment component12 = attacker.GetComponent<Equipment>();
				if (component12 != null && component12.PrimaryAttack != null)
				{
					statusEffectParams6.DmgType = component12.PrimaryAttack.DamageData.Type;
				}
				statusEffectParams6.Value = 10f;
				statusEffectParams6.OneHitUse = true;
				statusEffectParams6.IsHostile = false;
				component11.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams6, AbilityType, null, deleteOnClear: true));
				StatusEffectParams statusEffectParams7 = new StatusEffectParams();
				statusEffectParams7.AffectsStat = ModifiedStat.MeleeAccuracy;
				statusEffectParams7.Value = 10f;
				statusEffectParams7.OneHitUse = true;
				statusEffectParams7.IsHostile = false;
				component11.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams7, AbilityType, null, deleteOnClear: true));
			}
			break;
		}
		case ModifiedStat.BonusCritChanceOnSameEnemy:
		{
			CharacterStats component9 = attacker.GetComponent<CharacterStats>();
			if (!(component9 != null))
			{
				break;
			}
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI3 in partyMembers)
			{
				if (!(partyMemberAI3 == null) && !(partyMemberAI3.gameObject == attacker) && partyMemberAI3.CurrentTarget == enemy)
				{
					StatusEffectParams statusEffectParams5 = new StatusEffectParams();
					statusEffectParams5.AffectsStat = ModifiedStat.BonusHitToCritPercentAll;
					statusEffectParams5.Value = num;
					statusEffectParams5.OneHitUse = true;
					statusEffectParams5.IsHostile = false;
					component9.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams5, AbilityType, null, deleteOnClear: true));
					break;
				}
			}
			break;
		}
		case ModifiedStat.BonusAccuracyOnSameEnemy:
		{
			CharacterStats component5 = attacker.GetComponent<CharacterStats>();
			if (!(component5 != null))
			{
				break;
			}
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI2 in partyMembers)
			{
				if (!(partyMemberAI2 == null) && !(partyMemberAI2.gameObject == attacker) && partyMemberAI2.CurrentTarget == enemy)
				{
					StatusEffectParams statusEffectParams2 = new StatusEffectParams();
					statusEffectParams2.AffectsStat = ModifiedStat.Accuracy;
					statusEffectParams2.Value = num;
					statusEffectParams2.OneHitUse = true;
					statusEffectParams2.IsHostile = false;
					component5.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams2, AbilityType, null, deleteOnClear: true));
					break;
				}
			}
			break;
		}
		case ModifiedStat.BonusAccuracyOnSameEnemyAsExtraObject:
		{
			if (!(m_extraObject != null))
			{
				break;
			}
			CharacterStats component6 = attacker.GetComponent<CharacterStats>();
			if (component6 != null)
			{
				AIController aIController = GameUtilities.FindActiveAIController(m_extraObject);
				if (aIController != null && aIController.CurrentTarget == enemy)
				{
					StatusEffectParams statusEffectParams3 = new StatusEffectParams();
					statusEffectParams3.AffectsStat = ModifiedStat.Accuracy;
					statusEffectParams3.Value = num;
					statusEffectParams3.OneHitUse = true;
					statusEffectParams3.IsHostile = false;
					component6.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams3, AbilityType, null, deleteOnClear: true));
				}
			}
			break;
		}
		case ModifiedStat.BonusDamageMultOnSameEnemy:
		{
			CharacterStats component4 = attacker.GetComponent<CharacterStats>();
			if (!(component4 != null))
			{
				break;
			}
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (!(partyMemberAI == null) && !(partyMemberAI.gameObject == attacker) && partyMemberAI.CurrentTarget == enemy)
				{
					StatusEffectParams statusEffectParams = new StatusEffectParams();
					statusEffectParams.AffectsStat = ModifiedStat.BonusDamageMult;
					statusEffectParams.Value = num;
					statusEffectParams.OneHitUse = true;
					statusEffectParams.IsHostile = false;
					component4.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams, AbilityType, null, deleteOnClear: true));
					break;
				}
			}
			break;
		}
		case ModifiedStat.AccuracyByRace:
		{
			CharacterStats component7 = enemy.GetComponent<CharacterStats>();
			if (component7 != null && component7.CharacterRace == Params.RaceType)
			{
				CharacterStats component8 = attacker.GetComponent<CharacterStats>();
				if (component8 != null)
				{
					StatusEffectParams statusEffectParams4 = new StatusEffectParams();
					statusEffectParams4.AffectsStat = ModifiedStat.Accuracy;
					statusEffectParams4.Value = num;
					statusEffectParams4.OneHitUse = true;
					statusEffectParams4.IsHostile = false;
					component8.ApplyStatusEffectImmediate(CreateChild(attacker, statusEffectParams4, AbilityType, null, deleteOnClear: true));
				}
			}
			break;
		}
		case ModifiedStat.DamageAttackerOnImplementLaunch:
		{
			if (!(attack != null))
			{
				break;
			}
			Weapon component = attack.GetComponent<Weapon>();
			if (!(component != null) || !component.IsImplement)
			{
				break;
			}
			Health component2 = attacker.GetComponent<Health>();
			if (component2.Unconscious || component2.Dead)
			{
				break;
			}
			if (attacker == Owner)
			{
				CharacterStats component3 = attacker.GetComponent<CharacterStats>();
				if (component3 != null && component3.StatDamageHealMultiplier > 0f)
				{
					num /= component3.StatDamageHealMultiplier;
				}
			}
			component2.ApplyDamageDirectly(num, Params.DmgType, Owner, this);
			break;
		}
		}
	}

	public void WhenHits(GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (!m_applied || attacker == enemy)
		{
			return;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.MeleeAttackAllOnPath:
			if (damage.Attack is AttackMelee && !m_is_checking_melee_path)
			{
				m_is_checking_melee_path = true;
				damage.Attack.HandleBeam(attacker, enemy, Vector3.zero);
				m_is_checking_melee_path = false;
			}
			break;
		case ModifiedStat.BonusMeleeDamageFromWounds:
		{
			if (!(damage.Attack is AttackMelee))
			{
				break;
			}
			CharacterStats component10 = attacker.GetComponent<CharacterStats>();
			if (component10 != null)
			{
				int num = 0;
				GenericAbility genericAbility = component10.FindWoundsTrait();
				if (genericAbility != null)
				{
					num = component10.CountStatusEffects(genericAbility.StatusEffects[0].Tag);
				}
				if (num > 0)
				{
					DamagePacket.DamageProcType item = new DamagePacket.DamageProcType(Params.DmgType, (float)num * currentAppliedValue);
					damage.Damage.DamageProc.Add(item);
				}
			}
			break;
		}
		case ModifiedStat.CanStun:
			if (OEIRandom.FloatValue() < currentAppliedValue)
			{
				CharacterStats component = enemy.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					component.ApplyAffliction(AfflictionData.Instance.StunnedPrefab, Owner, AbilityType, damage, deleteOnClear: true, 5f, null);
				}
			}
			break;
		case ModifiedStat.CanStunOnCrit:
		{
			if (!damage.IsCriticalHit)
			{
				break;
			}
			CharacterStats component15 = attacker.GetComponent<CharacterStats>();
			if (!component15)
			{
				break;
			}
			DamageInfo damageInfo3 = component15.ComputeSecondaryAttack(damage.Attack, enemy, CharacterStats.DefenseType.Fortitude);
			List<StatusEffect> appliedEffects2 = new List<StatusEffect>();
			if (damageInfo3.HitType != 0)
			{
				CharacterStats component16 = enemy.GetComponent<CharacterStats>();
				if ((bool)component16)
				{
					component16.ApplyAffliction(AfflictionData.Instance.StunnedPrefab, Owner, AbilityType, damageInfo3, deleteOnClear: true, 2f, appliedEffects2);
				}
			}
			AttackBase.PostAttackMessagesSecondaryEffect(enemy, damageInfo3, appliedEffects2);
			break;
		}
		case ModifiedStat.CanKnockDownOnCrit:
		{
			if (!damage.IsCriticalHit)
			{
				break;
			}
			CharacterStats component5 = attacker.GetComponent<CharacterStats>();
			if (!component5)
			{
				break;
			}
			DamageInfo damageInfo = component5.ComputeSecondaryAttack(damage.Attack, enemy, CharacterStats.DefenseType.Fortitude);
			List<StatusEffect> appliedEffects = new List<StatusEffect>();
			if (damageInfo.HitType != 0)
			{
				CharacterStats component6 = enemy.GetComponent<CharacterStats>();
				if ((bool)component6)
				{
					component6.ApplyAffliction(AfflictionData.Prone, attacker, AbilityType, damageInfo, deleteOnClear: true, ParamsValue(), appliedEffects);
				}
			}
			AttackBase.PostAttackMessagesSecondaryEffect(enemy, damageInfo, appliedEffects);
			break;
		}
		case ModifiedStat.CanCripple:
			if (OEIRandom.FloatValue() < currentAppliedValue)
			{
				CharacterStats component14 = enemy.GetComponent<CharacterStats>();
				if ((bool)component14)
				{
					StatusEffectParams statusEffectParams2 = new StatusEffectParams();
					statusEffectParams2.AffectsStat = ModifiedStat.MovementRate;
					statusEffectParams2.Duration = 30f;
					statusEffectParams2.Value = -3f;
					statusEffectParams2.IsHostile = true;
					component14.ApplyStatusEffect(CreateChild(attacker, statusEffectParams2, AbilityType, damage, deleteOnClear: true));
				}
			}
			break;
		case ModifiedStat.BonusDamageMult:
			if (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		case ModifiedStat.FocusWhenHits:
			if (damage.Attack != null && damage.Attack.AbilityOrigin == null)
			{
				CharacterStats component4 = attacker.GetComponent<CharacterStats>();
				if (component4 != null)
				{
					component4.Focus += currentAppliedValue * component4.FocusGainMult;
				}
			}
			break;
		case ModifiedStat.BeamDamageMult:
			if (damage.Attack is AttackBeam)
			{
				for (uint num3 = 0u; num3 <= m_intervalCount; num3++)
				{
					damage.DamageMult(currentAppliedValue);
				}
			}
			break;
		case ModifiedStat.BonusDamageMultOnLowStaminaTarget:
		{
			Health component7 = enemy.GetComponent<Health>();
			if (component7 != null && component7.CurrentStamina / component7.MaxStamina < 0.25f)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
		case ModifiedStat.DamageToDOT:
		{
			CharacterStats component9 = enemy.GetComponent<CharacterStats>();
			if ((bool)component9)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.Apply = ApplyType.ApplyOverTime;
				statusEffectParams.AffectsStat = ModifiedStat.Damage;
				statusEffectParams.IntervalRate = StatusEffectParams.IntervalRateType.Damage;
				statusEffectParams.Duration = 30f;
				statusEffectParams.Value = damage.DamageAmount * currentAppliedValue;
				statusEffectParams.IsHostile = true;
				component9.ApplyStatusEffect(CreateChild(attacker, statusEffectParams, AbilityType, damage, deleteOnClear: true));
				damage.DamageMult(0f);
			}
			break;
		}
		case ModifiedStat.BonusDamageMultIfTargetHasDOT:
		{
			CharacterStats component11 = enemy.GetComponent<CharacterStats>();
			if (component11 != null && component11.CountDOTs() > 0)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
		case ModifiedStat.BonusDamageProc:
		{
			if (!damage.Attack || !damage.Attack.IsAutoAttack())
			{
				break;
			}
			float num2 = currentAppliedValue;
			if (m_abilityOrigin != null && m_abilityOrigin is Wildstrike)
			{
				CharacterStats component12 = attacker.GetComponent<CharacterStats>();
				if (component12 != null)
				{
					num2 += (component12.WildstrikeDamageMult - 1f) * 100f;
				}
			}
			DamagePacket.DamageProcType item2 = new DamagePacket.DamageProcType(Params.DmgType, num2);
			damage.Damage.DamageProc.Add(item2);
			break;
		}
		case ModifiedStat.ApplyAttackEffects:
		{
			if ((ApplyAttackEffectsOnlyForAttacks != null && !ApplyAttackEffectsOnlyForAttacks.Contains(damage.Attack)) || !(Params.AttackPrefab != null))
			{
				break;
			}
			Params.AttackPrefab.PushEnemy(enemy, Params.AttackPrefab.PushDistance, m_owner.transform.position, null, null);
			List<StatusEffect> list = new List<StatusEffect>();
			List<StatusEffect> list2 = new List<StatusEffect>();
			Params.AttackPrefab.Owner = attacker;
			Params.AttackPrefab.ApplyStatusEffects(enemy, damage, deleteOnClear: true, list);
			DamageInfo damageInfo2 = Params.AttackPrefab.ApplyAfflictions(enemy, damage, deleteOnClear: true, list, list2);
			if (Params.AttackPrefab.ExtraAOE != null)
			{
				AttackAOE attackAOE = UnityEngine.Object.Instantiate(Params.AttackPrefab.ExtraAOE);
				attackAOE.DestroyAfterImpact = true;
				attackAOE.Owner = attacker;
				attackAOE.transform.parent = attacker.transform;
				attackAOE.SkipAnimation = true;
				attackAOE.AbilityOrigin = m_abilityOrigin;
				attackAOE.ShowImpactEffect(enemy.transform.position);
				GameObject excludedObject = null;
				if (attackAOE.ExcludeTarget)
				{
					excludedObject = enemy;
				}
				attackAOE.OnImpactShared(attacker, enemy.transform.position, excludedObject);
			}
			GameUtilities.LaunchEffect(Params.AttackPrefab.OnHitAttackerVisualEffect, 1f, attacker.transform, Params.AttackPrefab.AbilityOrigin);
			GameUtilities.LaunchEffect(Params.AttackPrefab.OnHitGroundVisualEffect, 1f, enemy.transform.position, Params.AttackPrefab.AbilityOrigin);
			Transform hitTransform = Params.AttackPrefab.GetHitTransform(enemy);
			GameUtilities.LaunchEffect(Params.AttackPrefab.OnHitVisualEffect, 1f, hitTransform, Params.AttackPrefab.AbilityOrigin);
			if (Params.AttackPrefab.AbilityOrigin != null)
			{
				Params.AttackPrefab.AbilityOrigin.Deactivate(attacker);
				m_generalCounter = 0u;
			}
			damage.PostponedDisplayEffects = list;
			if (damageInfo2 != null && damageInfo2 != damage)
			{
				AttackBase.PostAttackMessagesSecondaryEffect(enemy, damageInfo2, list2);
			}
			break;
		}
		case ModifiedStat.BonusDamageMultAtLowStamina:
			if (attacker.GetComponent<Health>().StaminaPercentage <= ParamsExtraValue())
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		case ModifiedStat.BonusDamageMultOnKDSFTarget:
		{
			bool flag = false;
			CharacterStats component2 = enemy.GetComponent<CharacterStats>();
			if (component2 != null)
			{
				flag = component2.HasStatusEffectOfType(ModifiedStat.KnockedDown) || component2.HasStatusEffectOfType(ModifiedStat.Stunned) || component2.HasStatusEffectFromAffliction(AfflictionData.Flanked);
			}
			if (flag)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
		case ModifiedStat.DamageMultByRace:
		{
			CharacterStats component17 = enemy.GetComponent<CharacterStats>();
			if (component17 != null && component17.CharacterRace == Params.RaceType)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
		case ModifiedStat.SpellDamageMult:
			if (damage.Attack != null && damage.Attack.AbilityOrigin != null && damage.Attack.AbilityOrigin is GenericSpell && (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type))
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		case ModifiedStat.BonusDamageMultOnFlankedTarget:
		{
			CharacterStats component13 = enemy.GetComponent<CharacterStats>();
			if ((component13 != null) & component13.HasStatusEffectFromAffliction(AfflictionData.Flanked))
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
		case ModifiedStat.ApplyFinishingBlowDamage:
		{
			bool flag2 = false;
			CharacterStats component8 = attacker.GetComponent<CharacterStats>();
			if (component8 != null)
			{
				foreach (GenericAbility activeAbility in component8.ActiveAbilities)
				{
					FinishingBlow finishingBlow = activeAbility as FinishingBlow;
					if ((bool)finishingBlow)
					{
						finishingBlow.AdjustDamage(damage, enemy);
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				FinishingBlow finishingBlow2 = AbilityOrigin as FinishingBlow;
				if ((bool)finishingBlow2)
				{
					finishingBlow2.AdjustDamage(damage, enemy);
					flag2 = true;
				}
			}
			break;
		}
		case ModifiedStat.BonusDamageMultWithImplements:
			if (damage.Attack != null)
			{
				Weapon component3 = damage.Attack.GetComponent<Weapon>();
				if (component3 != null && component3.IsImplement)
				{
					damage.DamageMult(currentAppliedValue);
				}
			}
			break;
		case ModifiedStat.AttackOnHitWithMelee:
			if (m_generalCounter == 0 && damage.Attack is AttackMelee)
			{
				try
				{
					m_generalCounter = 1u;
					LaunchAttack(enemy, 0, 1f);
					break;
				}
				finally
				{
					m_generalCounter = 0u;
				}
			}
			break;
		}
	}

	public void WhenMisses(GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (m_applied && !(attacker == enemy))
		{
			ModifiedStat affectsStat = Params.AffectsStat;
			if (affectsStat == ModifiedStat.ApplyAttackEffects && Params.AttackPrefab != null && Params.AttackPrefab.AbilityOrigin != null)
			{
				GameUtilities.LaunchEffect(Params.AttackPrefab.OnMissVisualEffect, 1f, enemy.transform, Params.AttackPrefab.AbilityOrigin);
				Params.AttackPrefab.AbilityOrigin.Deactivate(attacker);
				m_generalCounter = 0u;
			}
		}
	}

	public void WhenInterrupted(GameObject attacker)
	{
		if (m_applied)
		{
			ModifiedStat affectsStat = Params.AffectsStat;
			if (affectsStat == ModifiedStat.ApplyAttackEffects && Params.AttackPrefab != null && Params.AttackPrefab.AbilityOrigin != null && m_generalCounter != 0)
			{
				Params.AttackPrefab.AbilityOrigin.RestoreCooldown();
				m_generalCounter = 0u;
			}
		}
	}

	public void WhenHit(GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (!m_applied || attacker == enemy)
		{
			return;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.AttackOnMeleeHit:
			if (damage.Attack != null && damage.AttackIsHostile && damage.Attack is AttackMelee && !damage.Attack.LaunchingDirectlyToImpact && m_params.AttackPrefab != null && attacker != null)
			{
				LaunchAttack(attacker, 0, 1f);
			}
			break;
		case ModifiedStat.MinorSpellReflection:
			SpellReflect(3, attacker, enemy, damage);
			break;
		case ModifiedStat.SpellReflection:
			SpellReflect(5, attacker, enemy, damage);
			break;
		case ModifiedStat.DOTOnHit:
			if (enemy != null && damage.DamageAmount > 0f)
			{
				CharacterStats component = enemy.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					StatusEffectParams statusEffectParams = new StatusEffectParams();
					statusEffectParams.Apply = ApplyType.ApplyOnTick;
					statusEffectParams.AffectsStat = ModifiedStat.Damage;
					statusEffectParams.DmgType = Params.DmgType;
					statusEffectParams.IntervalRate = StatusEffectParams.IntervalRateType.Damage;
					statusEffectParams.Duration = ParamsExtraValue();
					statusEffectParams.Value = currentAppliedValue;
					statusEffectParams.IsHostile = true;
					component.ApplyStatusEffect(CreateChild(Owner, statusEffectParams, AbilityType, damage, deleteOnClear: true));
				}
			}
			break;
		case ModifiedStat.RangedGrazeReflection:
			RangedReflect(HitType.GRAZE, (int)currentAppliedValue, attacker, enemy, damage);
			break;
		case ModifiedStat.RangedReflection:
			if (OEIRandom.FloatValue() < ParamsExtraValue())
			{
				RangedReflect(HitType.CRIT, (int)currentAppliedValue, attacker, enemy, damage);
			}
			break;
		case ModifiedStat.DamageToStamina:
			if (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type)
			{
				Health component2 = enemy.GetComponent<Health>();
				if (component2 != null && damage.DamageAmount > 0f)
				{
					float amount = damage.DamageAmount * currentAppliedValue;
					component2.AddStamina(amount);
					component2.ReportStamina(CharacterStats.Name(enemy), amount, Origin ? Origin.gameObject : null, this);
				}
			}
			break;
		case ModifiedStat.IncomingCritDamageMult:
			if (damage.IsCriticalHit && (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type))
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		case ModifiedStat.IncomingDamageMult:
			if (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type)
			{
				damage.DamageMult(currentAppliedValue);
			}
			break;
		}
	}

	public void WhenTakesDamage(GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (!m_applied || attacker == enemy)
		{
			return;
		}
		switch (Params.AffectsStat)
		{
		case ModifiedStat.TransferDamageToCaster:
			if (Owner != null && damage.FinalAdjustedDamage > 0f)
			{
				Health component3 = Owner.GetComponent<Health>();
				if (component3 != null && !component3.Unconscious && !component3.Dead)
				{
					float num = damage.FinalAdjustedDamage * (1f - ParamsValue());
					damage.FinalAdjustedDamage *= ParamsValue();
					damage.DTAdjustedDamage *= ParamsValue();
					component3.ApplyDamageDirectly(num, Params.DmgType, null, this);
					PostDamageMessage(attacker, Owner, damage.Attack, num, damage.DamageType);
				}
			}
			break;
		case ModifiedStat.DamageToStaminaRegen:
			if (Params.DmgType == DamagePacket.DamageType.All || Params.DmgType == damage.Damage.Type)
			{
				Health component = enemy.GetComponent<Health>();
				CharacterStats component2 = enemy.GetComponent<CharacterStats>();
				if (component2 != null && component != null && damage.DTAdjustedDamage > 0f)
				{
					StatusEffectParams statusEffectParams = new StatusEffectParams();
					statusEffectParams.AffectsStat = ModifiedStat.RawStamina;
					statusEffectParams.Value = damage.DTAdjustedDamage * ParamsValue();
					statusEffectParams.Duration = ParamsExtraValue();
					statusEffectParams.IntervalRate = StatusEffectParams.IntervalRateType.Damage;
					statusEffectParams.Apply = ApplyType.ApplyOverTime;
					statusEffectParams.IsHostile = false;
					StatusEffect statusEffect = CreateChild(enemy, statusEffectParams, AbilityType, damage, deleteOnClear: true);
					statusEffect.m_forceStackable = true;
					component2.ApplyStatusEffect(statusEffect);
				}
			}
			break;
		}
	}

	protected void PostDamageMessage(GameObject attacker, GameObject defender, AttackBase attack, float damage, DamagePacket.DamageType damageType)
	{
		if (damage != 0f)
		{
			string text = damage.ToString((damage < 1f) ? "#0.0" : "#0");
			if (damageType != DamagePacket.DamageType.All && damageType != DamagePacket.DamageType.None)
			{
				text = text + " " + GUIUtils.GetDamageTypeString(damageType);
			}
			Console.AddBatchedMessage(GUIUtils.Format(123, CharacterStats.NameColored(defender), text, CharacterStats.NameColored(attacker)) + GUIUtils.Format(1731, BundleName), AttackBase.GetMessageColor(attacker ? attacker.GetComponent<Faction>() : null, defender.GetComponent<Faction>()), new DamageInfo(defender, damage, attack)
			{
				Target = defender
			});
		}
	}

	public void WhenInflictsDamage(GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (!m_applied || attacker == enemy)
		{
			return;
		}
		float currentAppliedValue = CurrentAppliedValue;
		switch (Params.AffectsStat)
		{
		case ModifiedStat.DamagePlusDot:
		case ModifiedStat.PostDtDamagePlusDot:
		{
			CharacterStats component2 = enemy.GetComponent<CharacterStats>();
			if ((bool)component2)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.Apply = ApplyType.ApplyOverTime;
				statusEffectParams.AffectsStat = ModifiedStat.Damage;
				if (Params.DmgType == DamagePacket.DamageType.None)
				{
					statusEffectParams.DmgType = damage.Damage.Type;
				}
				else
				{
					statusEffectParams.DmgType = Params.DmgType;
				}
				statusEffectParams.IntervalRate = StatusEffectParams.IntervalRateType.Damage;
				statusEffectParams.Duration = ParamsExtraValue();
				if (Params.AffectsStat == ModifiedStat.PostDtDamagePlusDot)
				{
					statusEffectParams.Value = damage.DTAdjustedDamage;
				}
				else
				{
					statusEffectParams.Value = damage.DamageAmount;
				}
				statusEffectParams.Value *= currentAppliedValue;
				statusEffectParams.IsHostile = true;
				StatusEffect statusEffect = CreateChild(attacker, statusEffectParams, AbilityType, damage, deleteOnClear: true);
				component2.ApplyStatusEffectImmediate(statusEffect);
				DamageInfo damageInfo = new DamageInfo(enemy, CharacterStats.DefenseType.None, AbilityOrigin);
				damageInfo.Attack = damage.Attack;
				StatusEffect[] appliedEffects = new StatusEffect[1] { statusEffect };
				AttackBase.PostAttackMessages(enemy, attacker, damageInfo, appliedEffects, primaryAttack: false);
			}
			break;
		}
		case ModifiedStat.GainStaminaWhenHits:
		{
			Health component3 = attacker.GetComponent<Health>();
			if (component3 != null)
			{
				float amount = damage.FinalAdjustedDamage * currentAppliedValue;
				component3.AddStamina(amount);
				component3.ReportStamina(CharacterStats.Name(attacker), amount, Origin ? Origin.gameObject : null, this);
			}
			break;
		}
		case ModifiedStat.GrantFocusToExtraObject:
		{
			if (!ExtraObject)
			{
				break;
			}
			CharacterStats component = ExtraObject.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				FocusTrait focusTrait = component.FindFocusTrait();
				if ((bool)focusTrait)
				{
					focusTrait.AddFocus(damage.FinalAdjustedDamage * ParamsValue());
				}
			}
			break;
		}
		}
	}

	private void SpellReflect(int spellLevel, GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (damage == null || damage.Attack == null || damage.Attack is AttackAOE)
		{
			return;
		}
		GenericAbility abilityOrigin = damage.Attack.AbilityOrigin;
		if (!(abilityOrigin != null) || !(attacker != null) || !(enemy != null) || !(abilityOrigin is GenericSpell) || damage.Attack is AttackAOE || (damage.Attack is AttackRanged && (damage.Attack as AttackRanged).MultiHitRay) || !damage.AttackIsHostile)
		{
			return;
		}
		GenericSpell genericSpell = abilityOrigin as GenericSpell;
		if (genericSpell.SpellLevel > spellLevel)
		{
			return;
		}
		m_generalCounter += (uint)genericSpell.SpellLevel;
		if ((float)m_generalCounter <= ParamsValue())
		{
			CharacterStats component = enemy.GetComponent<CharacterStats>();
			if (component != null)
			{
				DamageInfo damageInfo = component.ComputeSecondaryAttack(damage.Attack, attacker, CharacterStats.DefenseType.Will);
				AttackBase.PostAttackMessages(attacker, enemy, damageInfo, null, primaryAttack: true);
				if (damageInfo.HitType != 0)
				{
					damage.IsMiss = true;
					damage.IsGraze = false;
					damage.IsCriticalHit = false;
					damage.DamageMult(0f);
					damage.HitTypeChangeAbility = AbilityOrigin;
					int bounceCount = damage.Attack.IncrementBounceCount(damage.Self);
					if (damage.Attack is AttackRanged)
					{
						Transform hitTransform = damage.Attack.GetHitTransform(attacker);
						(damage.Attack as AttackRanged).ProjectileLaunch(enemy.transform.position, hitTransform.position, attacker, bounceCount, canHitOwner: true);
					}
					else
					{
						damage.Attack.StartCoroutine(damage.Attack.OnImpactDelay(0.5f, damage.Attack.gameObject, attacker, bounceCount));
					}
				}
			}
		}
		if ((float)m_generalCounter >= ParamsValue())
		{
			RemoveEffect();
		}
	}

	private void RangedReflect(HitType maxHitType, int accuracyAdjustment, GameObject attacker, GameObject enemy, DamageInfo damage)
	{
		if (!(damage.Attack is AttackAOE) && damage.HitType <= maxHitType && (damage.DefendedBy == CharacterStats.DefenseType.Deflect || damage.DefendedBy == CharacterStats.DefenseType.Reflex) && attacker != null && enemy != null && damage.Attack is AttackRanged && !(damage.Attack is AttackAOE))
		{
			damage.IsMiss = true;
			damage.IsGraze = false;
			damage.IsCriticalHit = false;
			damage.DamageMult(0f);
			AttackRanged obj = damage.Attack as AttackRanged;
			obj.TemporaryAccuracyBonus += accuracyAdjustment;
			obj.ProjectileLaunch(desiredDestination: damage.Attack.GetHitTransform(attacker).position, launchPos: enemy.transform.position, enemy: attacker, bounceCount: 0, canHitOwner: true);
		}
	}

	public void IncrementGeneralCounterForDestroy()
	{
		m_generalCounter++;
		if (ParamsValue() != 0f && (float)m_generalCounter >= ParamsValue())
		{
			RemoveEffect();
		}
	}

	private void AdjustDefenseAffliction(CharacterStats.DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref int defense)
	{
		if (attack != null && (isSecondary || attack.SecondaryDefense == CharacterStats.DefenseType.None) && attack.HasAffliction(Params.AfflictionPrefab))
		{
			defense += (int)ParamsValue();
		}
	}

	private void AdjustDefenseKeyword(CharacterStats.DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref int defense)
	{
		if (attack != null)
		{
			if (attack.HasKeyword(Params.Keyword))
			{
				defense += (int)ParamsValue();
			}
			else if ((isSecondary || attack.SecondaryDefense == CharacterStats.DefenseType.None) && attack.HasAfflictionWithKeyword(Params.Keyword))
			{
				defense += (int)ParamsValue();
			}
		}
	}

	private void AdjustDefenseKeywordImmune(CharacterStats.DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref bool isImmune)
	{
		if (attack != null && (string.Compare(Params.Keyword, "ground", ignoreCase: true) != 0 || !m_ownerStats || !m_ownerStats.HasStatusEffectOfType(ModifiedStat.KnockedDown)))
		{
			if (attack.HasKeyword(Params.Keyword))
			{
				isImmune = true;
			}
			else if ((isSecondary || attack.SecondaryDefense == CharacterStats.DefenseType.None) && attack.HasAfflictionWithKeyword(Params.Keyword))
			{
				isImmune = true;
			}
		}
	}

	private void HandleDestroyOnDeath(GameObject obj, GameEventArgs args)
	{
		RemoveEffect();
		GameUtilities.Destroy(obj, 10f);
	}

	private void SendOnEventToAi(GameEventType eventType, GameObject target)
	{
		AIController aIController = GameUtilities.FindActiveAIController(target);
		if (aIController != null)
		{
			GameEventArgs gameEventArgs = new GameEventArgs();
			gameEventArgs.Type = eventType;
			gameEventArgs.FloatData = new float[1];
			gameEventArgs.FloatData[0] = TimeLeft;
			gameEventArgs.IntData = new int[1];
			gameEventArgs.IntData[0] = 1;
			gameEventArgs.GameObjectData = new GameObject[1];
			gameEventArgs.GameObjectData[0] = m_owner;
			aIController.OnEvent(gameEventArgs);
		}
	}

	private void SendOffEventToAi(GameEventType eventType, GameObject target)
	{
		AIController aIController = GameUtilities.FindActiveAIController(target);
		if (aIController != null)
		{
			GameEventArgs gameEventArgs = new GameEventArgs();
			gameEventArgs.Type = eventType;
			gameEventArgs.IntData = new int[1];
			gameEventArgs.IntData[0] = 0;
			aIController.OnEvent(gameEventArgs);
		}
	}

	private void UpdateAiStateDuration<T>() where T : IStateWithDuration
	{
		if (!Target)
		{
			return;
		}
		AIController aIController = GameUtilities.FindActiveAIController(Target);
		if (aIController != null)
		{
			IStateWithDuration stateWithDuration = (IStateWithDuration)aIController.StateManager.FindState(typeof(T));
			if (stateWithDuration != null)
			{
				stateWithDuration.Duration = TimeLeft;
			}
		}
	}

	public void Reset()
	{
		ClearEffect(m_target, triggerEffects: false);
		m_timeActive = 0f;
		TemporaryDurationAdjustment = 0f;
		m_intervalTimer = 0f;
		m_intervalCount = 0u;
		m_auraTargetsApplied.Clear();
		m_auraEffectApplied.Clear();
		m_triggerCount = 0;
		m_needsDurationCalculated = true;
		IsFromAura = false;
		if (!(AbilityOrigin != null) || !(AbilityOrigin.GetComponent<Consumable>() != null))
		{
			return;
		}
		GenericAbility abilityOrigin = AbilityOrigin;
		bool flag = false;
		if ((bool)m_ownerStats)
		{
			for (int i = 0; i < m_ownerStats.ActiveStatusEffects.Count; i++)
			{
				if (flag)
				{
					break;
				}
				if (m_ownerStats.ActiveStatusEffects[i].AbilityOrigin == abilityOrigin)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			GameUtilities.Destroy(abilityOrigin.gameObject, 1f);
			AbilityOrigin = null;
		}
	}

	public bool HasBiggerValueThan(StatusEffect eff)
	{
		NonstackingType nonstackingEffectType = NonstackingEffectType;
		if (nonstackingEffectType == NonstackingType.ActiveBonus || nonstackingEffectType == NonstackingType.ItemBonus)
		{
			if (CurrentAppliedValue > eff.CurrentAppliedValue)
			{
				return true;
			}
		}
		else if (CurrentAppliedValue < eff.CurrentAppliedValue)
		{
			return true;
		}
		return false;
	}

	public bool Suspend()
	{
		if (Params.AffectsStat == ModifiedStat.SuspendBeneficialEffects || Params.AffectsStat == ModifiedStat.SuspendHostileEffects)
		{
			return false;
		}
		if (!IsSuspended)
		{
			ClearEffect(m_target);
		}
		m_suspensionCount++;
		return true;
	}

	public bool Unsuspend()
	{
		if (IsSuspended)
		{
			m_suspensionCount--;
			if (m_suspensionCount == 0)
			{
				ApplyEffect(m_target);
			}
			return true;
		}
		return false;
	}

	public void Suppress()
	{
		if (!IsSuppressed)
		{
			ClearEffect(m_target);
			m_suppressed = true;
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs("m_suppressed"));
			}
		}
	}

	public void Unsuppress()
	{
		if (IsSuppressed)
		{
			m_suppressed = false;
			ApplyEffect(m_target);
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs("m_suppressed"));
			}
		}
	}

	public bool Suppresses(StatusEffect eff, bool suppress_if_tied)
	{
		bool result = false;
		if (eff.Params.AffectsStat == ModifiedStat.GenericMarker)
		{
			return false;
		}
		if (AfflictionOrigin != null && eff.AfflictionOrigin != null && AfflictionOrigin.OverridesAffliction(eff.AfflictionOrigin))
		{
			result = true;
		}
		if (!Stackable && !eff.Stackable && GetStackingKey() == eff.GetStackingKey() && NonstackingEffectType == eff.NonstackingEffectType)
		{
			if (HasBiggerValueThan(eff))
			{
				result = true;
			}
			else if (CurrentAppliedValue == eff.CurrentAppliedValue)
			{
				if (TimeLeft > eff.TimeLeft)
				{
					result = true;
				}
				else if (TimeLeft == eff.TimeLeft && suppress_if_tied)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public void FocusBreak()
	{
		if (!(DurationAfterBreak <= 0f))
		{
			Duration = DurationAfterBreak;
			DurationAfterBreak = 0f;
			m_timeActive = 0f;
			if (TicksAfterBreak)
			{
				m_intervalTimer = Interval;
			}
			else
			{
				m_intervalTimer = Duration;
			}
		}
	}

	public void AddTriggerCallback()
	{
		if (m_triggerCallbackSet || Params.TriggerAdjustment == null || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.None)
		{
			return;
		}
		if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnMiss || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnTargetOfWillAttack || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnDamage || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnHitOrCriticallyHit || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnUnconscious)
		{
			Health component = m_target.GetComponent<Health>();
			if (component != null)
			{
				component.OnDamaged += HandleOnDamaged;
				if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove)
				{
					component.OnHealed += HandleOnHealed;
				}
				m_triggerCallbackSet = true;
			}
			if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnUnconscious)
			{
				HandleOnDamaged(null, null);
			}
			if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove)
			{
				HandleOnHealed(null, null);
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.Timer)
		{
			m_triggerCallbackSet = true;
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnMove)
		{
			Mover component2 = m_target.GetComponent<Mover>();
			if (component2 != null)
			{
				component2.OnMovementStarted += HandleOnMovementStarted;
				component2.OnMovementStopped += HandleOnMovementStopped;
				m_triggerCallbackSet = true;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnKill)
		{
			Health component3 = m_target.GetComponent<Health>();
			if (component3 != null)
			{
				component3.OnKill += HandleOnKill;
				m_triggerCallbackSet = true;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnWeaponChange)
		{
			Equipment component4 = m_target.GetComponent<Equipment>();
			if (component4 != null)
			{
				component4.OnEquipmentChanged += HandleOnWeaponChanged;
				m_triggerCallbackSet = true;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnWeaponChangeToNonSummoned)
		{
			Equipment component5 = m_target.GetComponent<Equipment>();
			if (component5 != null)
			{
				component5.OnEquipmentChanged += HandleOnWeaponChangedToNonSummoned;
				m_triggerCallbackSet = true;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileGriefed)
		{
			CharacterStats component6 = m_target.GetComponent<CharacterStats>();
			if (component6 != null)
			{
				component6.OnGriefStateChanged += HandleGriefStatusChanged;
				m_triggerCallbackSet = true;
				HandleGriefStatusChanged(component6.gameObject, component6.HasStatusEffectFromAffliction(AttackData.Instance.BondedGriefAffliction));
			}
		}
		else
		{
			UnityEngine.Debug.Log("Unhandled trigger callback");
		}
	}

	public void RemoveTriggerCallback()
	{
		if (!m_triggerCallbackSet)
		{
			return;
		}
		if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnMiss || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnTargetOfWillAttack || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnDamage || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnHitOrCriticallyHit || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnUnconscious)
		{
			Health component = m_target.GetComponent<Health>();
			if (component != null)
			{
				component.OnDamaged -= HandleOnDamaged;
				if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove)
				{
					component.OnHealed -= HandleOnHealed;
				}
				m_triggerCallbackSet = false;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.Timer)
		{
			m_triggerCallbackSet = false;
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnMove)
		{
			Mover component2 = m_target.GetComponent<Mover>();
			if (component2 != null)
			{
				component2.OnMovementStarted -= HandleOnMovementStarted;
				component2.OnMovementStopped -= HandleOnMovementStopped;
				m_triggerCallbackSet = false;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnKill)
		{
			Health component3 = m_target.GetComponent<Health>();
			if (component3 != null)
			{
				component3.OnKill -= HandleOnKill;
				m_triggerCallbackSet = false;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnWeaponChange)
		{
			Equipment component4 = m_target.GetComponent<Equipment>();
			if (component4 != null)
			{
				component4.OnEquipmentChanged -= HandleOnWeaponChanged;
				m_triggerCallbackSet = false;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnWeaponChangeToNonSummoned)
		{
			Equipment component5 = m_target.GetComponent<Equipment>();
			if (component5 != null)
			{
				component5.OnEquipmentChanged -= HandleOnWeaponChangedToNonSummoned;
				m_triggerCallbackSet = false;
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileGriefed)
		{
			CharacterStats component6 = m_target.GetComponent<CharacterStats>();
			if (component6 != null)
			{
				component6.OnGriefStateChanged -= HandleGriefStatusChanged;
				m_triggerCallbackSet = false;
			}
		}
	}

	public void DeactivateAttackPrefabAbility(GameObject attacker)
	{
		if (Params != null && Params.AttackPrefab != null && Params.AttackPrefab.AbilityOrigin != null)
		{
			Params.AttackPrefab.AbilityOrigin.Deactivate(attacker);
		}
	}

	public void HandleClearingEffectOnResting()
	{
		if (Params.LastsUntilRest)
		{
			m_numRestCycles++;
			if (m_numRestCycles >= Params.MaxRestCycles)
			{
				RemoveEffect();
			}
		}
	}

	private void HandleGriefStatusChanged(GameObject sender, bool state)
	{
		if (state)
		{
			if (m_triggerCount == 0)
			{
				OnTrigger();
			}
		}
		else if (m_triggerCount > 0)
		{
			OffTrigger();
		}
	}

	private void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnMiss)
		{
			DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
			if (damageInfo != null && damageInfo.IsMiss)
			{
				OnTrigger();
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnStaminaPercentBelow || (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow && m_triggerCount == 0) || (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove && m_triggerCount == 1))
		{
			Health component = m_target.GetComponent<Health>();
			if (component != null && component.CurrentStamina / component.MaxStamina < Params.TriggerAdjustment.TriggerValue)
			{
				if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove)
				{
					OffTrigger();
				}
				else
				{
					OnTrigger();
				}
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnUnconscious)
		{
			Health component2 = m_target.GetComponent<Health>();
			if ((bool)component2 && component2.Unconscious)
			{
				OnTrigger();
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnTargetOfWillAttack)
		{
			DamageInfo damageInfo2 = (DamageInfo)args.GenericData[0];
			if (damageInfo2 != null && damageInfo2.DefendedBy == CharacterStats.DefenseType.Will)
			{
				OnTrigger();
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnDamage)
		{
			DamageInfo damageInfo3 = (DamageInfo)args.GenericData[0];
			if (damageInfo3 == null || (!damageInfo3.IsMiss && damageInfo3.MaxDamage > 0f))
			{
				OnTrigger();
			}
		}
		else if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnHitOrCriticallyHit)
		{
			DamageInfo damageInfo4 = (DamageInfo)args.GenericData[0];
			if (damageInfo4 != null && damageInfo4.AttackIsHostile && (damageInfo4.IsCriticalHit || damageInfo4.IsPlainHit))
			{
				OnTrigger();
			}
		}
	}

	private void HandleOnHealed(GameObject myObject, GameEventArgs args)
	{
		if ((Params.TriggerAdjustment.Type != StatusEffectTrigger.TriggerType.WhileStaminaPercentBelow || m_triggerCount != 1) && (Params.TriggerAdjustment.Type != StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove || m_triggerCount != 0))
		{
			return;
		}
		Health component = m_target.GetComponent<Health>();
		if (component != null && component.CurrentStamina / component.MaxStamina >= Params.TriggerAdjustment.TriggerValue)
		{
			if (Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.WhileStaminaPercentAbove)
			{
				OnTrigger();
			}
			else
			{
				OffTrigger();
			}
		}
	}

	private void HandleOnMovementStarted(object sender, EventArgs e)
	{
		OnTrigger();
	}

	private void HandleOnMovementStopped(object sender, EventArgs e)
	{
		OffTrigger();
	}

	private void HandleOnKill(object sender, EventArgs e)
	{
		OnTrigger();
	}

	private void HandleOnWeaponChanged(Equippable.EquipmentSlot slot, Equippable oldEq, Equippable newEq, bool isSwappingSummonedWeapon, bool enforceRecoveryPenalty)
	{
		if (slot == Equippable.EquipmentSlot.PrimaryWeapon || slot == Equippable.EquipmentSlot.SecondaryWeapon)
		{
			OnTrigger();
		}
	}

	private void HandleOnWeaponChangedToNonSummoned(Equippable.EquipmentSlot slot, Equippable oldEq, Equippable newEq, bool isSwappingSummonedWeapon, bool enforceRecoveryPenalty)
	{
		if (!isSwappingSummonedWeapon && (slot == Equippable.EquipmentSlot.PrimaryWeapon || slot == Equippable.EquipmentSlot.SecondaryWeapon))
		{
			OnTrigger();
		}
	}

	public void OnTrigger()
	{
		if (m_triggerCount >= Params.TriggerAdjustment.MaxTriggerCount)
		{
			return;
		}
		if (Params.TriggerAdjustment.ValueAdjustment != 0f && Interval == 0f)
		{
			ApplyEffectHelper(m_target, Params.TriggerAdjustment.ValueAdjustment);
		}
		if (Params.TriggerAdjustment.DurationAdjustment != 0f)
		{
			if (Duration == 0f)
			{
				ResetTimer();
			}
			Duration += Params.TriggerAdjustment.DurationAdjustment;
		}
		ApplyLoopingEffect(Params.OnTriggerVisualEffect);
		if (!Params.TriggerAdjustment.ResetTriggerOnEffectPulse)
		{
			m_triggerCount++;
			if ((bool)AbilityOrigin && HasTriggerActivation)
			{
				AbilityOrigin.EffectTriggeredThisFrame = true;
			}
			if (m_triggerCount == Params.TriggerAdjustment.MaxTriggerCount && Params.TriggerAdjustment.RemoveEffectAtMax)
			{
				RemoveEffect();
			}
		}
	}

	public void OffTrigger()
	{
		if (m_triggerCount == 0)
		{
			return;
		}
		if (Params.TriggerAdjustment.ValueAdjustment != 0f && Interval == 0f)
		{
			if (IsScaledMultiplier)
			{
				ApplyEffectHelper(m_target, 1f / Params.TriggerAdjustment.ValueAdjustment);
			}
			else
			{
				ApplyEffectHelper(m_target, 0f - Params.TriggerAdjustment.ValueAdjustment);
			}
		}
		if (Params.TriggerAdjustment.DurationAdjustment != 0f)
		{
			Duration -= Params.TriggerAdjustment.DurationAdjustment;
		}
		RemoveLoopingEffects();
		m_triggerCount--;
		if ((bool)AbilityOrigin && HasTriggerActivation)
		{
			AbilityOrigin.EffectUntriggeredThisFrame = true;
		}
	}

	private void HandleOnDamagedForReapply(GameObject myObject, GameEventArgs args)
	{
		m_damageToReapply += args.FloatData[0];
	}

	private void HandleStatsOnPreDamageAppliedForAbsorb(GameObject source, CombatEventArgs args)
	{
		if ((Params.AffectsStat != ModifiedStat.DamageShield || Params.DmgType == DamagePacket.DamageType.All || args.Damage.DamageType == Params.DmgType) && args.Damage.DamageAmount != 0f)
		{
			if (args.Damage.DamageAmount < m_damageToAbsorb)
			{
				Console.AddBatchedMessage(Console.Format(GUIUtils.GetTextWithLinks(1330), BundleName, args.Damage.DamageAmount.ToString("#0")), Color.white, new Console.BatchedAttackData(args.Damage));
				m_damageToAbsorb -= args.Damage.DamageAmount;
				args.Damage.DamageMult(0f);
				args.Damage.AttackAbsorbed = true;
			}
			else
			{
				Console.AddBatchedMessage(Console.Format(GUIUtils.GetTextWithLinks(1331), BundleName, m_damageToAbsorb.ToString("#0")), Color.white, new Console.BatchedAttackData(args.Damage));
				args.Damage.DamageAdd(0f - m_damageToAbsorb);
				m_damageToAbsorb = 0f;
				RemoveEffect();
			}
		}
	}

	public void ResetTimer()
	{
		m_timeActive = 0f;
	}

	public float ParamsValue()
	{
		return Params.GetValue(m_ownerStats);
	}

	public float ParamsExtraValue()
	{
		return Params.GetExtraValue(m_ownerStats);
	}

	protected float GetCurrentAppliedValue(float baseValue)
	{
		float num = 0f;
		if (IsScaledMultiplier)
		{
			num = 1f;
		}
		if (Params.TriggerAdjustment != null)
		{
			num = ((!IsScaledMultiplier) ? (Params.TriggerAdjustment.ValueAdjustment * (float)m_triggerCount) : ((float)Math.Pow(Params.TriggerAdjustment.ValueAdjustment, m_triggerCount)));
		}
		float num2 = 0f;
		num2 = ((!IsScaledMultiplier) ? (baseValue + num) : (baseValue * num));
		if (AfflictionOrigin == null && IsDamageDealing)
		{
			if (IsScaledMultiplier)
			{
				num2 -= 1f;
				num2 *= Scale;
				num2 += 1f;
			}
			else if (!IsDOT)
			{
				num2 *= Scale;
			}
		}
		if (Params.ChecksReligion)
		{
			m_religiousScale = Religion.Instance.GetCurrentBonusMultiplier(Owner, m_abilityOrigin);
		}
		if (IsScaledMultiplier)
		{
			num2 -= 1f;
			num2 *= m_religiousScale;
			num2 += 1f;
		}
		else
		{
			num2 *= m_religiousScale;
		}
		if ((bool)m_targetStats && (bool)AbilityOrigin && AbilityOrigin.EffectType == GenericAbility.AbilityType.Consumable && !IsOverTime)
		{
			Consumable component = AbilityOrigin.GetComponent<Consumable>();
			if ((bool)component && component.Type == Consumable.ConsumableType.Potion)
			{
				num2 *= m_targetStats.PotionEffectiveness;
			}
		}
		return num2;
	}

	public static bool IsScaledMultiplierStatic(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].UsageType == UsageType.ScaledMultiplier;
	}

	public static bool IsAdditiveBonus(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].UsageType == UsageType.Additive;
	}

	public static DatabaseString GetStatDisplayString(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].DisplayString;
	}

	public static bool UsesParamOfType(ModifiedStat stat, StatusEffectParams.ParamType valueId)
	{
		return StatEnumMetadata[(int)stat].UsesParameters[(int)valueId];
	}

	public static bool UsesDamageTypeParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.DmgType);
	}

	public static bool UsesValueParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.Value);
	}

	public static bool UsesExtraValueParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.ExtraValue);
	}

	public static bool UsesTrapParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.TrapPrefab);
	}

	public static bool UsesEquippableParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.EquippablePrefab);
	}

	public static bool UsesConsumableParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.ConsumablePrefab);
	}

	public static bool UsesAttackParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.AttackPrefab);
	}

	public static bool UsesAbilityParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.AbilityPrefab);
	}

	public static bool UsesAfflictionParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.AfflictionPrefab);
	}

	public static bool UsesRaceParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.RaceType);
	}

	public static bool UsesAttributeParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.Attribute);
	}

	public static bool UsesDefenseTypeParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.DefenseType);
	}

	public static bool UsesKeywordParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.Keyword);
	}

	public static bool UsesClassParam(ModifiedStat stat)
	{
		return UsesParamOfType(stat, StatusEffectParams.ParamType.ClassType);
	}

	public static bool StatNotRevokedParam(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].NotRevoked;
	}

	public static bool IsModifiedStatObsolete(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].Obsolete;
	}

	public static string GetModifiedStatObsoleteMessage(ModifiedStat stat)
	{
		return StatEnumMetadata[(int)stat].ObsoleteMessage;
	}

	public static bool EffectLaunchesAttack(ModifiedStat stat)
	{
		if (stat != ModifiedStat.LaunchAttack && stat != ModifiedStat.LaunchAttackWithRollingBonus && stat != ModifiedStat.ApplyPulsedAOE && stat != ModifiedStat.DamageToSummon && stat != ModifiedStat.AttackOnMeleeHit)
		{
			return stat == ModifiedStat.AttackOnHitWithMelee;
		}
		return true;
	}

	public bool BundlesWith(StatusEffect other)
	{
		if (Origin != null && Origin != other.Origin)
		{
			return false;
		}
		if ((BundleName == null || !BundleName.Equals(other.BundleName)) && (BundleId < 0 || BundleId != other.BundleId))
		{
			return false;
		}
		return true;
	}

	public Team GetCachedTeam()
	{
		return m_cachedTeam;
	}

	public string GetDisplayName()
	{
		string text = BundleName;
		if (string.IsNullOrEmpty(text))
		{
			if (AfflictionOrigin != null)
			{
				return AfflictionOrigin.DisplayName.GetText();
			}
			if (AbilityOrigin != null)
			{
				return AbilityOrigin.Name();
			}
			if (EquipmentOrigin != null)
			{
				return EquipmentOrigin.Name;
			}
			if (PhraseOrigin != null)
			{
				return PhraseOrigin.DisplayName.ToString();
			}
			if (Owner != null)
			{
				text = GUIUtils.Format(1313, CharacterStats.Name(Owner));
			}
		}
		return text;
	}

	public Texture2D GetDisplayIcon()
	{
		if (PhraseOrigin != null && PhraseOrigin.Icon != null)
		{
			return PhraseOrigin.Icon;
		}
		if (AfflictionOrigin != null && AfflictionOrigin.Icon != null)
		{
			return AfflictionOrigin.Icon;
		}
		if (AbilityOrigin != null && (bool)AbilityOrigin.Icon)
		{
			return AbilityOrigin.Icon;
		}
		if (AbilityOrigin != null && (bool)m_abilityOrigin.GetComponent<Item>())
		{
			return m_abilityOrigin.GetComponent<Item>().IconTexture;
		}
		if (EquipmentOrigin != null && EquipmentOrigin.IconTexture != null)
		{
			return EquipmentOrigin.IconTexture;
		}
		if (!string.IsNullOrEmpty(Params.Keyword))
		{
			return KeywordData.GetIcon(Params.Keyword);
		}
		if ((bool)Params.Icon)
		{
			return Params.Icon;
		}
		return null;
	}
}
