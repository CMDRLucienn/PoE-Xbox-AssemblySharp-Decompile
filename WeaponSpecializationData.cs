using System;
using UnityEngine;

public class WeaponSpecializationData : MonoBehaviour
{
	public enum WeaponType
	{
		Arbalest,
		Arquebus,
		BattleAxe,
		Blunderbuss,
		Club,
		Crossbow,
		Dagger,
		Estoc,
		Flail,
		GreatSword,
		Hatchet,
		HuntingBow,
		Mace,
		MorningStar,
		Pike,
		Pistol,
		Pollaxe,
		Quarterstaff,
		Rapier,
		Rod,
		Sabre,
		Stiletto,
		Sceptre,
		Spear,
		Sword,
		Wand,
		WarBow,
		WarHammer,
		Unarmed,
		Count
	}

	public enum Category
	{
		Adventurer,
		Knight,
		Noble,
		Peasant,
		Ruffian,
		Soldier,
		Count
	}

	public WeaponType[] Adventurer;

	public WeaponType[] Knight;

	public WeaponType[] Noble;

	public WeaponType[] Peasant;

	public WeaponType[] Ruffian;

	public WeaponType[] Soldier;

	public static WeaponSpecializationData Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'WeaponSpecializationData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static bool IsWeaponTypeOnList(WeaponType type, Category category)
	{
		WeaponType[] array = (new WeaponType[6][] { Instance.Adventurer, Instance.Knight, Instance.Noble, Instance.Peasant, Instance.Ruffian, Instance.Soldier })[(int)category];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetWeaponTypeList(Category category)
	{
		WeaponType[] array = null;
		switch (category)
		{
		case Category.Adventurer:
			array = Instance.Adventurer;
			break;
		case Category.Knight:
			array = Instance.Knight;
			break;
		case Category.Noble:
			array = Instance.Noble;
			break;
		case Category.Peasant:
			array = Instance.Peasant;
			break;
		case Category.Ruffian:
			array = Instance.Ruffian;
			break;
		case Category.Soldier:
			array = Instance.Soldier;
			break;
		}
		if (array != null)
		{
			return TextUtils.FuncJoin((WeaponType wt) => GUIUtils.GetWeaponTypeIDs(wt), array, ", ");
		}
		return "";
	}

	public static void AddWeaponSpecialization(CharacterStats attacker, DamageInfo damage)
	{
		if (!attacker)
		{
			return;
		}
		if (damage == null)
		{
			throw new ArgumentNullException("damage");
		}
		if (damage.IsMiss)
		{
			return;
		}
		AttackBase attack = damage.Attack;
		Weapon weapon = (attack ? attack.GetComponent<Weapon>() : null);
		if (!weapon)
		{
			return;
		}
		if (weapon.UniversalType)
		{
			float[] array = new float[6];
			for (int i = 0; i < attacker.ActiveAbilities.Count; i++)
			{
				WeaponSpecialization weaponSpecialization = attacker.ActiveAbilities[i] as WeaponSpecialization;
				if ((bool)weaponSpecialization)
				{
					array[(int)weaponSpecialization.SpecializationCategory] += weaponSpecialization.BonusDamageMult - 1f;
				}
			}
			float num = 0f;
			for (int j = 0; j < array.Length; j++)
			{
				num = Mathf.Max(num, array[j] + 1f);
			}
			damage.DamageMult(num);
			return;
		}
		for (int k = 0; k < attacker.ActiveAbilities.Count; k++)
		{
			WeaponSpecialization weaponSpecialization2 = attacker.ActiveAbilities[k] as WeaponSpecialization;
			if ((bool)weaponSpecialization2 && WeaponSpecialization.WeaponSpecializationApplies(attack, weaponSpecialization2.SpecializationCategory))
			{
				damage.DamageMult(weaponSpecialization2.BonusDamageMult);
			}
		}
	}
}
