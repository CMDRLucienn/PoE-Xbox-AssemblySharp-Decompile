using System;
using UnityEngine;

[Obsolete("Implement with StatusEffect.TriggerAdjustment.")]
public class Blooded : GenericAbility
{
	public override bool ListenForDamageEvents => true;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			if (m_ownerHealth != null && m_ownerHealth.CurrentStamina / m_ownerHealth.MaxStamina < 0.5f)
			{
				ActivateTrait();
			}
		}
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * 0.5f;
			if (args.FloatData[1] >= num && m_ownerHealth.CurrentStamina < num)
			{
				ActivateTrait();
			}
		}
	}

	public override void HandleOnHealed(GameObject myObject, GameEventArgs args)
	{
		if (m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * 0.5f;
			if (args.FloatData[1] < num && m_ownerHealth.CurrentStamina >= num)
			{
				DeactivateTrait();
			}
		}
	}

	protected void ActivateTrait()
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

	protected void DeactivateTrait()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			component.ClearEffect(effect);
		}
	}
}
