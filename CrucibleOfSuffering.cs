using UnityEngine;

public class CrucibleOfSuffering : GenericAbility
{
	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnClearStatusEffect += ClearStatusEffect;
			}
			m_permanent = true;
		}
	}

	private void ClearStatusEffect(GameObject sender, StatusEffect effect)
	{
		if (effect.Params.IsHostile)
		{
			ApplyStatusEffectsNow();
		}
	}

	public void ApplyStatusEffectsNow()
	{
		if (m_ownerStats == null)
		{
			return;
		}
		bool flag = false;
		foreach (StatusEffect effect in m_effects)
		{
			effect.Params.DontHideFromLog = true;
			if (m_ownerStats.ClearEffect(effect))
			{
				flag = true;
			}
			m_ownerStats.ApplyStatusEffectImmediate(effect);
		}
		if (!flag)
		{
			AttackBase.PostAttackMessages(m_ownerStats.gameObject, Owner, new DamageInfo(Owner, CharacterStats.DefenseType.None, this), m_effects, primaryAttack: true);
		}
	}

	protected override void ActivateStatusEffects()
	{
	}
}
