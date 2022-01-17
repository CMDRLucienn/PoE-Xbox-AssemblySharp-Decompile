using UnityEngine;

[ClassTooltip("When the ability's owner dies, the attached attack will be immediately launched at his location, then this ability's Status Effects will be applied to him.")]
public class TriggeredOnDeathAbility : GenericAbility
{
	public override GameObject Owner
	{
		set
		{
			if (m_owner != null)
			{
				Health component = m_owner.GetComponent<Health>();
				if (component != null)
				{
					component.OnDeath -= HandleHealthOnDeath;
				}
			}
			base.Owner = value;
			if (m_owner != null)
			{
				Health component2 = m_owner.GetComponent<Health>();
				if (component2 != null)
				{
					component2.OnDeath += HandleHealthOnDeath;
				}
			}
		}
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			Passive = true;
			base.Init();
		}
	}

	private void HandleHealthOnDeath(GameObject myObject, GameEventArgs args)
	{
		AttackBase component = GetComponent<AttackBase>();
		if ((bool)component)
		{
			component.SkipAnimation = true;
			component.OnImpact(null, Owner.transform.position);
		}
		CharacterStats component2 = Owner.GetComponent<CharacterStats>();
		if (!(component2 == null))
		{
			ReportActivation(overridePassive: true);
			StatusEffectParams[] statusEffects = StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				StatusEffect effect = StatusEffect.Create(base.gameObject, this, param, AbilityType.Ability, null, deleteOnClear: true);
				component2.ApplyStatusEffect(effect);
			}
		}
	}

	protected override bool ShowNormalActivationMessages()
	{
		return false;
	}

	protected override void ActivateStatusEffects()
	{
	}
}
