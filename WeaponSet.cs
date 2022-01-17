using System;
using System.Collections;
using System.Collections.Generic;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class WeaponSet : IEnumerable<Equippable>, IEnumerable, ITooltipContent
{
	[ExcludeFromSerialization]
	public Equippable PrimaryWeapon;

	[ExcludeFromSerialization]
	public Equippable SecondaryWeapon;

	public WeaponSet()
	{
	}

	public WeaponSet(Equippable primaryWeapon, Equippable secondaryWeapon)
	{
		PrimaryWeapon = primaryWeapon;
		SecondaryWeapon = secondaryWeapon;
	}

	public Equippable GetItemInSlot(Equippable.EquipmentSlot slot)
	{
		switch (slot)
		{
		case Equippable.EquipmentSlot.PrimaryWeapon:
		case Equippable.EquipmentSlot.PrimaryWeapon2:
			return PrimaryWeapon;
		case Equippable.EquipmentSlot.SecondaryWeapon:
		case Equippable.EquipmentSlot.SecondaryWeapon2:
			return SecondaryWeapon;
		default:
			Debug.LogError("WeaponSet doesn't have slot '" + slot.ToString() + "'");
			return null;
		}
	}

	public bool Empty()
	{
		if (PrimaryWeapon == null)
		{
			return SecondaryWeapon == null;
		}
		return false;
	}

	public IEnumerator<Equippable> GetEnumerator()
	{
		yield return PrimaryWeapon;
		yield return SecondaryWeapon;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static bool IsNullOrEmpty(WeaponSet ws)
	{
		return ws?.Empty() ?? true;
	}

	public string GetTooltipContent(GameObject owner)
	{
		if ((bool)PrimaryWeapon && (bool)SecondaryWeapon)
		{
			return PrimaryWeapon.Name + "\n" + SecondaryWeapon.Name;
		}
		if ((bool)PrimaryWeapon)
		{
			return PrimaryWeapon.Name;
		}
		if ((bool)SecondaryWeapon)
		{
			return SecondaryWeapon.Name;
		}
		return GUIUtils.GetText(1280, CharacterStats.GetGender(owner));
	}

	public string GetTooltipName(GameObject owner)
	{
		return GUIUtils.GetText(1279);
	}

	public Texture GetTooltipIcon()
	{
		if ((bool)PrimaryWeapon)
		{
			return PrimaryWeapon.GetIconTexture();
		}
		if ((bool)SecondaryWeapon)
		{
			return SecondaryWeapon.GetIconTexture();
		}
		return null;
	}
}
