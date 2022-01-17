using System;
using UnityEngine;

[Obsolete("Implement with DamageToStaminaRegen status effect.")]
public class Unbending : GenericAbility
{
	public float StaminaRecoverPercent = 0.5f;

	public float StaminaRecoverTime = 5f;

	public override bool ListenForDamageEvents => true;

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (m_activated && m_ownerHealth != null && m_ownerStats != null)
		{
			float num = args.FloatData[1] - m_ownerHealth.CurrentStamina;
			if (num > 0f)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Stamina;
				statusEffectParams.Value = num;
				statusEffectParams.Duration = StaminaRecoverTime;
				statusEffectParams.IntervalRate = StatusEffectParams.IntervalRateType.Damage;
				statusEffectParams.Apply = StatusEffect.ApplyType.ApplyOverTime;
				statusEffectParams.IsHostile = false;
				m_ownerStats.ApplyStatusEffectImmediate(StatusEffect.Create(Owner, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true));
			}
		}
	}
}
