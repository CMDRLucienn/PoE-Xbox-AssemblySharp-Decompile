using UnityEngine;

public class SwornEnemyTrait : GenericAbility
{
	public float BonusDamagePercent = 0.2f;

	public float BonusAccuracy = 20f;

	protected override void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnAttackLaunch(source, args);
		if (args.Attacker == null || args.Victim == null)
		{
			return;
		}
		foreach (StatusEffect item in args.Victim.GetComponent<CharacterStats>().FindStatusEffectsOfType(StatusEffect.ModifiedStat.MarkedPrey))
		{
			if (item.Owner == args.Attacker)
			{
				CharacterStats component = args.Attacker.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					StatusEffectParams statusEffectParams = new StatusEffectParams();
					statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.BonusDamageMult;
					statusEffectParams.DmgType = DamagePacket.DamageType.All;
					statusEffectParams.Value = BonusDamagePercent + 1f;
					statusEffectParams.OneHitUse = true;
					statusEffectParams.IsHostile = false;
					component.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true));
					StatusEffectParams statusEffectParams2 = new StatusEffectParams();
					statusEffectParams2.AffectsStat = StatusEffect.ModifiedStat.Accuracy;
					statusEffectParams2.Value = BonusAccuracy;
					statusEffectParams2.OneHitUse = true;
					statusEffectParams.IsHostile = false;
					component.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams2, AbilityType.Ability, null, deleteOnClear: true));
				}
				break;
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

	public override string GetMarkedPreyEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		string text = GUIUtils.Format(1144, TextUtils.MultiplierAsPercentBonus(BonusDamagePercent + 1f)) + ", " + GUIUtils.Format(1404, TextUtils.NumberBonus(BonusAccuracy));
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(GUIUtils.Format(1902, text), base.Attack), stringEffects);
		return base.GetMarkedPreyEffects(stringEffects, ability, character);
	}
}
