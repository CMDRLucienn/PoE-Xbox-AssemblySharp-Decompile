using System;
using UnityEngine;

[Serializable]
public class StatusEffectTrigger
{
	public enum TriggerType
	{
		None,
		OnMiss,
		OnStaminaPercentBelow,
		Timer,
		OnTargetOfWillAttack,
		OnMove,
		OnDamage,
		[Tooltip("Triggers when the target makes a kill.")]
		OnKill,
		[Tooltip("Triggers when stamina drops below a percent, and turns off when it is above that percent.")]
		WhileStaminaPercentBelow,
		[Tooltip("Triggers when stamina is above a percent, and turns off when it is below that percent.")]
		WhileStaminaPercentAbove,
		[Tooltip("Triggers when target changes either weapon (primary or secondary).")]
		OnWeaponChange,
		[Tooltip("Triggers when target changes either weapon (primary or secondary) to a non-summoned weapon.")]
		OnWeaponChangeToNonSummoned,
		[Tooltip("Triggers when the target is hit or critically hit by another creature.")]
		OnHitOrCriticallyHit,
		[Tooltip("Triggers when the target becomes unconscious.")]
		OnUnconscious,
		[Tooltip("Triggers when the target recieves Grief, and turns off when he loses it.")]
		WhileGriefed
	}

	public TriggerType Type;

	public float TriggerValue;

	public float ValueAdjustment;

	public float DurationAdjustment;

	public float RadiusAdjustment;

	public int MaxTriggerCount;

	public bool RemoveEffectAtMax;

	public bool ResetTriggerOnEffectPulse;

	public bool ResetTriggerOnEffectEnd;

	public bool Ineffective
	{
		get
		{
			if (!IneffectiveValue)
			{
				return MaxTriggerCount == 0;
			}
			return true;
		}
	}

	public bool IneffectiveValue
	{
		get
		{
			if (ValueAdjustment == 0f && DurationAdjustment == 0f && RadiusAdjustment == 0f)
			{
				return !RemoveEffectAtMax;
			}
			return false;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is StatusEffectTrigger statusEffectTrigger)
		{
			if (Type == statusEffectTrigger.Type && TriggerValue == statusEffectTrigger.TriggerValue && ValueAdjustment == statusEffectTrigger.ValueAdjustment && DurationAdjustment == statusEffectTrigger.DurationAdjustment && RadiusAdjustment == statusEffectTrigger.RadiusAdjustment && MaxTriggerCount == statusEffectTrigger.MaxTriggerCount && RemoveEffectAtMax == statusEffectTrigger.RemoveEffectAtMax && ResetTriggerOnEffectEnd == statusEffectTrigger.ResetTriggerOnEffectEnd)
			{
				return ResetTriggerOnEffectPulse == statusEffectTrigger.ResetTriggerOnEffectPulse;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode();
	}

	public string GetString(string effect)
	{
		string fstring = GUIUtils.GetStatusEffectTriggerTypeString(Type);
		if (Ineffective)
		{
			return effect;
		}
		if (RemoveEffectAtMax)
		{
			switch (Type)
			{
			case TriggerType.OnDamage:
				fstring = GUIUtils.GetText(1761);
				break;
			case TriggerType.OnWeaponChange:
				fstring = GUIUtils.GetText(1760);
				break;
			case TriggerType.OnWeaponChangeToNonSummoned:
				fstring = GUIUtils.GetText(1760);
				break;
			case TriggerType.OnHitOrCriticallyHit:
				fstring = GUIUtils.GetText(2218);
				break;
			}
		}
		if (DurationAdjustment < 0f || DurationAdjustment >= 1f)
		{
			effect = GUIUtils.Format(1665, effect, GUIUtils.Format(211, TextUtils.NumberBonus(DurationAdjustment)));
		}
		switch (Type)
		{
		case TriggerType.OnMiss:
		case TriggerType.OnTargetOfWillAttack:
		case TriggerType.OnMove:
		case TriggerType.OnDamage:
		case TriggerType.OnKill:
		case TriggerType.OnWeaponChange:
		case TriggerType.OnWeaponChangeToNonSummoned:
		case TriggerType.OnHitOrCriticallyHit:
		case TriggerType.OnUnconscious:
			return StringUtility.Format(fstring, effect);
		case TriggerType.Timer:
			return StringUtility.Format(fstring, effect, GUIUtils.Format(211, TriggerValue));
		case TriggerType.OnStaminaPercentBelow:
		case TriggerType.WhileStaminaPercentBelow:
		case TriggerType.WhileStaminaPercentAbove:
			return StringUtility.Format(fstring, effect, GUIUtils.Format(1277, (TriggerValue * 100f).ToString("#0")));
		default:
			return effect;
		}
	}

	public void CheckForErrors(string origin)
	{
		if ((Type == TriggerType.OnStaminaPercentBelow || Type == TriggerType.WhileStaminaPercentAbove || Type == TriggerType.WhileStaminaPercentBelow) && (TriggerValue < 0f || TriggerValue > 1f))
		{
			UIDebug.Instance.LogOnceOnlyWarning("Status Effect on " + origin + " is checking StaminaPercent with an invalid TriggerValue. The value should be between 0 and 1 inclusive.", UIDebug.Department.Design, 10f);
		}
	}
}
