using UnityEngine;

public class BackStabAbility : GenericAbility
{
	[Tooltip("Enemies with distance less than or equal to ValidRange are viable backstab targets.")]
	public float ValidRange = 3f;

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

	protected virtual bool CanSneakAttackEnemy(GameObject creature, DamageInfo damage)
	{
		if (GameUtilities.ObjectDistance2D(creature, Owner) > ValidRange)
		{
			return false;
		}
		if (damage.Attack.AbilityOrigin != null && (!damage.Attack.AbilityOrigin.UseFullAttack || !damage.Attack.AbilityOrigin.UsePrimaryAttack))
		{
			return false;
		}
		if (damage.IsMiss)
		{
			return false;
		}
		if (!(m_ownerStats != null) || m_ownerStats.InvisibilityState <= 0)
		{
			return damage.Attack.IsStealthAttack;
		}
		return true;
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
		if (m_ownerStats == null)
		{
			Debug.LogError("m_ownerStats is null in BackStabAbility.ApplyEffectsImmediately.");
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			m_ownerStats.ApplyStatusEffectImmediate(effect);
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		return (GUIUtils.Format(444, GUIUtils.Format(1533, ValidRange)) + "\n" + base.GetAdditionalEffects(stringEffects, mode, ability, character)).Trim();
	}
}
