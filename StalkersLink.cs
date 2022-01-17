using UnityEngine;

public class StalkersLink : GenericAbility
{
	public float AccuracyBonus = 20f;

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (args.Attacker == null || args.Victim == null)
		{
			return;
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(args.Attacker);
		if (gameObject == null)
		{
			return;
		}
		AIController component = gameObject.GetComponent<AIController>();
		if (component != null && component.EngagedEnemies.Contains(args.Victim))
		{
			CharacterStats component2 = args.Attacker.GetComponent<CharacterStats>();
			if ((bool)component2)
			{
				component2.ClearEffectFromAbility(this);
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Accuracy;
				statusEffectParams.Value = AccuracyBonus;
				statusEffectParams.OneHitUse = true;
				statusEffectParams.IsHostile = false;
				component2.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true));
				StatusEffectParams statusEffectParams2 = new StatusEffectParams();
				statusEffectParams2.AffectsStat = StatusEffect.ModifiedStat.BonusDamageMult;
				statusEffectParams2.Value = component2.StalkersLinkDamageMult;
				statusEffectParams2.DmgType = DamagePacket.DamageType.All;
				statusEffectParams2.OneHitUse = true;
				statusEffectParams2.IsHostile = false;
				component2.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams2, AbilityType.Ability, null, deleteOnClear: true));
			}
		}
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(GUIUtils.Format(1890, TextUtils.NumberBonus(AccuracyBonus)), base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
