using UnityEngine;

[ClassTooltip("Grants the owner the specified <Bonus Damage Mult> when attacking with a weapon that is in the specified <Specialization Category> or is marked 'Universal'. Only one type of Weapon Specialization can ever apply to a single attack.")]
public class WeaponSpecialization : GenericAbility
{
	public WeaponSpecializationData.Category SpecializationCategory;

	public float BonusDamageMult = 1.15f;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	public static bool WeaponSpecializationApplies(AttackBase attack, WeaponSpecializationData.Category category)
	{
		if (attack == null)
		{
			return false;
		}
		AttackMelee attackMelee = attack as AttackMelee;
		if ((bool)attackMelee && attackMelee.Unarmed && WeaponSpecializationData.IsWeaponTypeOnList(WeaponSpecializationData.WeaponType.Unarmed, category))
		{
			return true;
		}
		Weapon component = attack.GetComponent<Weapon>();
		if ((bool)component && (component.UniversalType || WeaponSpecializationData.IsWeaponTypeOnList(component.WeaponType, category)))
		{
			return true;
		}
		return false;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string weaponTypeList = WeaponSpecializationData.GetWeaponTypeList(SpecializationCategory);
		string text = GUIUtils.Format(1144, TextUtils.MultiplierAsPercentBonus(BonusDamageMult));
		if (!string.IsNullOrEmpty(weaponTypeList))
		{
			text += GUIUtils.Format(1731, weaponTypeList);
		}
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(text, base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
