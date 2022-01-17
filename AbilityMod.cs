using System;
using UnityEngine;

[Serializable]
public class AbilityMod
{
	public enum AbilityModType
	{
		AdditionalUse,
		AddAbilityStatusEffects,
		AddAttackStatusEffects,
		AttackAccuracyBonus,
		WoundThresholdAdjustment,
		NegativeReligiousTraitMultiplier,
		FinishingBlowDamagePercentAdjustment,
		AttackDTBypass,
		AttackSpeedMultiplier,
		ReplaceParticleFX,
		AddAttackStatusEffectOnCasterOnly
	}

	[Serializable]
	public class ReplaceObjectParams
	{
		public GameObject Existing;

		public GameObject ReplaceWith;
	}

	public AbilityModType Type;

	public float Value;

	public StatusEffectParams[] StatusEffects;

	public ReplaceObjectParams[] ReplaceObjects;

	public GenericAbility.AbilityType SourceType { get; set; }

	public Equippable EquipmentOrigin { get; set; }

	public string GetString(GameObject owner)
	{
		CharacterStats source = (owner ? owner.GetComponent<CharacterStats>() : null);
		switch (Type)
		{
		case AbilityModType.AdditionalUse:
			return GUIUtils.Format(448, TextUtils.NumberBonus(Value, "#0"));
		case AbilityModType.AddAbilityStatusEffects:
		case AbilityModType.AddAttackStatusEffects:
		case AbilityModType.AddAttackStatusEffectOnCasterOnly:
			return StatusEffectParams.ListToString(StatusEffects, source, null, null, null, StatusEffectFormatMode.TalentModification, AttackBase.TargetType.All);
		case AbilityModType.AttackAccuracyBonus:
			return GUIUtils.Format(1404, TextUtils.NumberBonus(Value, "#0"));
		case AbilityModType.WoundThresholdAdjustment:
			return GUIUtils.Format(1898, TextUtils.NumberBonus(Value, "#0"));
		case AbilityModType.NegativeReligiousTraitMultiplier:
			return "";
		case AbilityModType.FinishingBlowDamagePercentAdjustment:
			return GUIUtils.Format(1253, GUIUtils.Format(1277, TextUtils.NumberBonus(Value * 100f, "#0")));
		case AbilityModType.AttackDTBypass:
			return GUIUtils.Format(1183, TextUtils.NumberBonus(Value, "#0.0"));
		case AbilityModType.AttackSpeedMultiplier:
			return GUIUtils.Format(1113, TextUtils.MultiplierAsPercentBonus(Value));
		case AbilityModType.ReplaceParticleFX:
			return "";
		default:
			return "";
		}
	}
}
