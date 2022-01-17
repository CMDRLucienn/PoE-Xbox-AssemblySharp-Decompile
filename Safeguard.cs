using UnityEngine;

public class Safeguard : GenericSpell
{
	public float SafeguardDuration = 30f;

	public float StaminaTriggerPercent = 0.5f;

	public AttackAOE AoeProneAttack;

	private StatusEffect m_safeEffect;

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * StaminaTriggerPercent;
			if (args.FloatData[1] >= num && m_ownerHealth.CurrentStamina < num)
			{
				ActivateSafeguard();
			}
		}
	}

	public override void Activate(GameObject target)
	{
		base.Activate(target);
		if (!(m_ownerStats != null))
		{
			return;
		}
		StatusEffectParams statusEffectParams = new StatusEffectParams();
		statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.CallbackOnDamaged;
		statusEffectParams.OnDamagedCallbackAbility = this;
		statusEffectParams.Description = GenericAbility.Name(this);
		statusEffectParams.Duration = SafeguardDuration;
		statusEffectParams.IsHostile = false;
		m_safeEffect = StatusEffect.Create(Owner, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true);
		m_ownerStats.ApplyStatusEffectImmediate(m_safeEffect);
		if (m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * StaminaTriggerPercent;
			if (m_ownerHealth.CurrentStamina < num)
			{
				ActivateSafeguard();
			}
		}
	}

	private void ActivateSafeguard()
	{
		if ((bool)Owner && FogOfWar.PointVisibleInFog(Owner.transform.position))
		{
			ReportActivation(overridePassive: true);
		}
		ApplyStatusEffectsNow();
		PerformAOEAttack();
		if (m_ownerStats != null && m_safeEffect != null)
		{
			m_ownerStats.ClearEffect(m_safeEffect);
		}
	}

	private void ApplyStatusEffectsNow()
	{
		if (m_ownerStats == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			m_ownerStats.ApplyStatusEffectImmediate(effect);
		}
	}

	protected override void ActivateStatusEffects()
	{
	}

	private void PerformAOEAttack()
	{
		AttackAOE attackAOE = Object.Instantiate(AoeProneAttack);
		attackAOE.Owner = Owner;
		attackAOE.transform.parent = Owner.transform;
		attackAOE.SkipAnimation = true;
		attackAOE.AbilityOrigin = this;
		attackAOE.OnImpactShared(null, Owner.transform.position, Owner);
	}
}
