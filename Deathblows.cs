using UnityEngine;

public class Deathblows : GenericAbility
{
	public Affliction[] TriggeringAffliction;

	protected override void Init()
	{
		if (!m_initialized)
		{
			DurationOverride = 0.1f;
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		OnIncomingDamage(args.Damage, args.Victim);
	}

	protected bool CanDeathblowEnemy(GameObject creature)
	{
		int num = 0;
		if (creature != null)
		{
			CharacterStats component = creature.GetComponent<CharacterStats>();
			if (component != null)
			{
				Affliction[] triggeringAffliction = TriggeringAffliction;
				foreach (Affliction aff in triggeringAffliction)
				{
					if (component.HasStatusEffectFromAffliction(aff))
					{
						num++;
					}
				}
			}
		}
		if (num >= 2)
		{
			return true;
		}
		return false;
	}

	protected override void ActivateStatusEffects()
	{
	}

	private void OnIncomingDamage(DamageInfo damage, GameObject enemy)
	{
		if (CanDeathblowEnemy(enemy))
		{
			damage.AddAttackEffect(GenericAbility.Name(this));
			ApplyEffectsImmediately();
		}
	}

	private void ApplyEffectsImmediately()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			component.ApplyStatusEffectImmediate(effect);
		}
	}
}
