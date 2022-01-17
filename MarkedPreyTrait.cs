using UnityEngine;

public class MarkedPreyTrait : GenericAbility
{
	public float BonusDamagePercent = 0.2f;

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (args.Attacker == null || args.Victim == null)
		{
			return;
		}
		foreach (StatusEffect item in args.Victim.GetComponent<CharacterStats>().FindStatusEffectsOfType(StatusEffect.ModifiedStat.MarkedPrey))
		{
			if (item.Owner != null && (item.Owner == args.Attacker || item.Owner == GameUtilities.FindMaster(args.Attacker)))
			{
				CharacterStats component = args.Attacker.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					StatusEffectParams statusEffectParams = new StatusEffectParams();
					statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.BonusDamage;
					statusEffectParams.DmgType = args.Damage.Damage.Type;
					statusEffectParams.Value = BonusDamagePercent;
					statusEffectParams.OneHitUse = true;
					statusEffectParams.IsHostile = false;
					component.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true));
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
		string text = GUIUtils.Format(1144, GUIUtils.Format(1277, TextUtils.NumberBonus(BonusDamagePercent)));
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(GUIUtils.Format(1902, text), base.Attack), stringEffects);
		return base.GetMarkedPreyEffects(stringEffects, ability, character);
	}
}
