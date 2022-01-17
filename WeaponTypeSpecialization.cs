using System.Collections.Generic;
using UnityEngine;

public class WeaponTypeSpecialization : GenericAbility
{
	[Tooltip("Type of weapon you are specializing in")]
	public WeaponSpecializationData.WeaponType[] type;

	[Tooltip("Accuracy bonus for weapons of this type.")]
	public float BonusAccuracy = 10f;

	[Tooltip("If set, Universal weapons do not automatically qualify for this bonus.")]
	public bool DisallowUniversal;

	private HashSet<WeaponSpecializationData.WeaponType> typesAvailable = new HashSet<WeaponSpecializationData.WeaponType>();

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
			typesAvailable.UnionWith(type);
		}
	}

	protected override void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnAttackLaunch(source, args);
		if (WeaponTypeApplies(args.Attacker, args.Damage.Attack))
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
			}
		}
	}

	public override DamageInfo UIGetBonusAccuracyOnAttack(GameObject source, DamageInfo info)
	{
		if (WeaponTypeApplies(source, info.Attack))
		{
			info.AccuracyRating += (int)BonusAccuracy;
		}
		return base.UIGetBonusAccuracyOnAttack(source, info);
	}

	public bool WeaponTypeApplies(GameObject attacker, AttackBase attack)
	{
		if (attacker == null || attack == null)
		{
			return false;
		}
		AttackMelee attackMelee = attack as AttackMelee;
		if ((bool)attackMelee && attackMelee.Unarmed)
		{
			return typesAvailable.Contains(WeaponSpecializationData.WeaponType.Unarmed);
		}
		Equipment component = attacker.GetComponent<Equipment>();
		if (component != null)
		{
			Equippable equippable = null;
			if (component.CurrentItems != null)
			{
				if (component.PrimaryAttack == attack)
				{
					equippable = component.CurrentItems.PrimaryWeapon;
				}
				else if (component.SecondaryAttack == attack)
				{
					equippable = component.CurrentItems.SecondaryWeapon;
				}
			}
			Weapon weapon = equippable as Weapon;
			if ((bool)weapon)
			{
				if (DisallowUniversal || !weapon.UniversalType)
				{
					return typesAvailable.Contains(weapon.WeaponType);
				}
				return true;
			}
		}
		return false;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = TextUtils.FuncJoin((WeaponSpecializationData.WeaponType ws) => GUIUtils.GetWeaponTypeIDs(ws), type, ", ");
		string text2 = GUIUtils.Format(1404, TextUtils.NumberBonus(BonusAccuracy));
		if (!string.IsNullOrEmpty(text))
		{
			text2 += GUIUtils.Format(1731, text);
		}
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(text2, base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
