using UnityEngine;

[ClassTooltip("Applies its Status Effects to the owner after he kills two enemies, then goes on cooldown until the effects expire.")]
public class Bloodlust : GenericAbility
{
	private int m_deathCounter;

	private float m_lustTimer;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			Health component = Owner.GetComponent<Health>();
			if (component != null)
			{
				component.OnKill += HandleHealthOnKill;
			}
			m_permanent = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_lustTimer > 0f)
		{
			m_lustTimer -= Time.deltaTime;
		}
	}

	private void HandleHealthOnKill(GameObject myObject, GameEventArgs args)
	{
		if (m_lustTimer > 0f)
		{
			return;
		}
		m_deathCounter++;
		if (m_deathCounter < 2 || m_ownerStats == null)
		{
			return;
		}
		StatusEffectParams[] statusEffects = StatusEffects;
		foreach (StatusEffectParams param in statusEffects)
		{
			StatusEffect statusEffect = StatusEffect.Create(base.gameObject, this, param, AbilityType.Ability, null, deleteOnClear: true);
			m_ownerStats.ApplyStatusEffect(statusEffect);
			float duration = statusEffect.Duration;
			if (duration > 0f && m_lustTimer <= duration)
			{
				m_lustTimer = duration;
			}
		}
		m_deathCounter = 0;
	}

	protected override void ActivateStatusEffects()
	{
	}
}
