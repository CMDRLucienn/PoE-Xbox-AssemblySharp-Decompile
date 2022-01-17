using System;
using UnityEngine;

public class FlankingAbility : GenericAbility
{
	public float CombatStartTime = 2f;

	private float m_time_since_combat_start;

	protected override void OnDestroy()
	{
		GameState.OnCombatStart -= CombatStarted;
		base.OnDestroy();
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			DurationOverride = 0.1f;
			GameState.OnCombatStart += CombatStarted;
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		OnIncomingDamage(args.Damage, args.Victim);
	}

	protected override void Update()
	{
		m_time_since_combat_start += Time.deltaTime;
		base.Update();
	}

	private void CombatStarted(object sender, EventArgs e)
	{
		m_time_since_combat_start = 0f;
	}

	public bool CanSneakAttackEnemy(GameObject creature, AttackBase attack)
	{
		if (attack.AbilityOrigin != null && (!attack.AbilityOrigin.UseFullAttack || !attack.AbilityOrigin.UsePrimaryAttack))
		{
			return false;
		}
		if (m_time_since_combat_start <= CombatStartTime || !GameState.InCombat)
		{
			return true;
		}
		if (m_ownerStats != null && m_ownerStats.InvisibilityState > 0)
		{
			return true;
		}
		return HasSneakAttackAffliction(creature);
	}

	public bool CanSneakAttackEnemy(GameObject creature, DamageInfo damage)
	{
		if (damage.IsMiss)
		{
			return false;
		}
		return CanSneakAttackEnemy(creature, damage.Attack);
	}

	public static bool HasSneakAttackAffliction(GameObject creature)
	{
		if (AttackData.Instance.SneakAttackAfflictions == null)
		{
			return false;
		}
		CharacterStats component = creature.GetComponent<CharacterStats>();
		if (component != null)
		{
			Affliction[] sneakAttackAfflictions = AttackData.Instance.SneakAttackAfflictions;
			foreach (Affliction aff in sneakAttackAfflictions)
			{
				if (component.HasStatusEffectFromAffliction(aff))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static StatusEffect GetSneakAttackAffliction(GameObject creature)
	{
		if (AttackData.Instance.SneakAttackAfflictions == null)
		{
			return null;
		}
		CharacterStats component = creature.GetComponent<CharacterStats>();
		if (component != null)
		{
			Affliction[] sneakAttackAfflictions = AttackData.Instance.SneakAttackAfflictions;
			foreach (Affliction aff in sneakAttackAfflictions)
			{
				StatusEffect statusEffectFromAffliction = component.GetStatusEffectFromAffliction(aff);
				if (statusEffectFromAffliction != null)
				{
					return statusEffectFromAffliction;
				}
			}
		}
		return null;
	}

	protected override void ActivateStatusEffects()
	{
	}

	private void OnIncomingDamage(DamageInfo damage, GameObject enemy)
	{
		if (CanSneakAttackEnemy(enemy, damage))
		{
			ApplyEffectsImmediately();
			damage.AddAttackEffect(GUIUtils.GetTextWithLinks(DisplayName.ToString()));
		}
	}

	private void ApplyEffectsImmediately()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			Debug.LogError("Owner has no character stats component in FlankingAbility.ApplyEffectsImmediately.");
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			component.ApplyStatusEffectImmediate(effect);
		}
	}

	protected override string FormatAbilityStringEffects(StringEffects stringEffects)
	{
		string text = "";
		for (int i = 0; i < AttackData.Instance.SneakAttackAfflictions.Length; i++)
		{
			if (i > 0)
			{
				text += GUIUtils.Comma();
			}
			text += AttackData.Instance.SneakAttackAfflictions[i].Name();
		}
		return GUIUtils.Format(2180, base.FormatAbilityStringEffects(stringEffects), text, GUIUtils.Format(211, CombatStartTime.ToString("#0.#")));
	}
}
