using UnityEngine;

public class WeaponFocus : GenericAbility
{
	public WeaponSpecializationData.Category SpecializationCategory;

	public float BonusAccuracy = 10f;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnAttackLaunch(source, args);
		if (WeaponSpecialization.WeaponSpecializationApplies(args.Damage.Attack, SpecializationCategory) && !args.Damage.WeaponFocusApplied)
		{
			CharacterStats component = args.Attacker.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.Accuracy;
				statusEffectParams.Value = BonusAccuracy;
				statusEffectParams.OneHitUse = true;
				statusEffectParams.IsHostile = false;
				component.ApplyStatusEffectImmediate(StatusEffect.Create(args.Attacker, this, statusEffectParams, AbilityType.Talent, null, deleteOnClear: true));
				args.Damage.WeaponFocusApplied = true;
			}
		}
	}

	public override DamageInfo UIGetBonusAccuracyOnAttack(GameObject source, DamageInfo info)
	{
		if (WeaponSpecialization.WeaponSpecializationApplies(info.Attack, SpecializationCategory) && !info.WeaponFocusApplied)
		{
			info.AccuracyRating += (int)BonusAccuracy;
			info.WeaponFocusApplied = true;
		}
		return base.UIGetBonusAccuracyOnAttack(source, info);
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string weaponTypeList = WeaponSpecializationData.GetWeaponTypeList(SpecializationCategory);
		string text = GUIUtils.Format(1404, TextUtils.NumberBonus(BonusAccuracy));
		if (!string.IsNullOrEmpty(weaponTypeList))
		{
			text += GUIUtils.Format(1731, weaponTypeList);
		}
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(text, base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
