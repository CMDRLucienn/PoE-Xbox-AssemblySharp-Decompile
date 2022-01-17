using UnityEngine;

public static class MirrorCharacterUtils
{
	public enum MirrorType
	{
		Cutscene,
		Image,
		NewCharacter,
		Paperdoll
	}

	public static GameObject MirrorCharacter(GameObject srcCharacter, MirrorType type)
	{
		GameObject gameObject = new GameObject(srcCharacter.name + "(Cloned)");
		gameObject.transform.position = srcCharacter.transform.position;
		gameObject.transform.rotation = srcCharacter.transform.rotation;
		Equipment equipment = null;
		CharacterStats characterStats = null;
		NPCAppearance nPCAppearance = null;
		Mover dest = null;
		Faction dest2 = null;
		PartyMemberAI partyMemberAI = null;
		Portrait portrait = null;
		equipment = gameObject.AddComponent<Equipment>();
		characterStats = gameObject.AddComponent<CharacterStats>();
		nPCAppearance = gameObject.AddComponent<NPCAppearance>();
		gameObject.AddComponent<Health>();
		portrait = gameObject.AddComponent<Portrait>();
		AlphaControl alphaControl = gameObject.AddComponent<AlphaControl>();
		alphaControl.Alpha = 0f;
		alphaControl.MaxAlpha = 0.5f;
		alphaControl.FadeIn(1f);
		if (type != MirrorType.Image)
		{
			dest = gameObject.AddComponent<Mover>();
		}
		if (type != MirrorType.Image)
		{
			dest2 = gameObject.AddComponent<Faction>();
		}
		if (type == MirrorType.NewCharacter)
		{
			partyMemberAI = gameObject.AddComponent<PartyMemberAI>();
			if ((bool)srcCharacter.GetComponent<PartyMemberAI>())
			{
				partyMemberAI.Secondary = true;
			}
		}
		if (type != MirrorType.NewCharacter)
		{
			gameObject.AddComponent(typeof(AIControllerDummy));
		}
		else
		{
			gameObject.AddComponent(typeof(AIController));
		}
		int layer = LayerUtility.FindLayerValue("Dynamics");
		GameUtilities.RecursiveSetLayer(gameObject, layer);
		nPCAppearance.layer = layer;
		CopyMover(dest, srcCharacter.GetComponent<Mover>());
		CopyAppearance(nPCAppearance, srcCharacter.GetComponent<NPCAppearance>());
		CopyStats(characterStats, srcCharacter.GetComponent<CharacterStats>());
		CopyEquipment(equipment.DefaultEquippedItems, srcCharacter.GetComponent<Equipment>().CurrentItems, includeWeapons: true);
		CopyFaction(dest2, srcCharacter.GetComponent<Faction>());
		CopyPortrait(portrait, srcCharacter.GetComponent<Portrait>());
		Animator component = gameObject.GetComponent<Animator>();
		if (component != null && type == MirrorType.Image)
		{
			component.applyRootMotion = false;
		}
		return gameObject;
	}

	public static void MirrorAppearance(GameObject dest, GameObject src)
	{
		Equipment component = dest.GetComponent<Equipment>();
		NPCAppearance component2 = dest.GetComponent<NPCAppearance>();
		Mover component3 = dest.GetComponent<Mover>();
		Faction component4 = dest.GetComponent<Faction>();
		Portrait component5 = dest.GetComponent<Portrait>();
		int layer = LayerUtility.FindLayerValue("Dynamics");
		GameUtilities.RecursiveSetLayer(dest, layer);
		component2.layer = layer;
		CopyMover(component3, src.GetComponent<Mover>());
		CopyAppearance(component2, src.GetComponent<NPCAppearance>());
		CopyEquipment(component.DefaultEquippedItems, src.GetComponent<Equipment>().CurrentItems, includeWeapons: false);
		CopyFaction(component4, src.GetComponent<Faction>());
		CopyPortrait(component5, src.GetComponent<Portrait>());
	}

	public static void CopyAppearance(NPCAppearance dest, NPCAppearance src)
	{
		if (!(src == null) && !(dest == null))
		{
			dest.skinColor = src.skinColor;
			dest.hasHair = src.hasHair;
			dest.hasFacialHair = src.hasFacialHair;
			dest.hairColor = src.hairColor;
			dest.primaryColor = src.primaryColor;
			dest.secondaryColor = src.secondaryColor;
			dest.facialHairAppearance.modelVariation = src.facialHairAppearance.modelVariation;
			dest.facialHairAppearance.materialVariation = src.facialHairAppearance.materialVariation;
			dest.facialHairAppearance.specialOverride = src.facialHairAppearance.specialOverride;
			dest.facialHairAppearance.bodyPiece = AppearancePiece.BodyPiece.Facialhair;
			dest.hairAppearance.modelVariation = src.hairAppearance.modelVariation;
			dest.hairAppearance.materialVariation = src.hairAppearance.materialVariation;
			dest.hairAppearance.specialOverride = src.hairAppearance.specialOverride;
			dest.hairAppearance.bodyPiece = AppearancePiece.BodyPiece.Hair;
			dest.headAppearance.modelVariation = src.headAppearance.modelVariation;
			dest.headAppearance.materialVariation = src.headAppearance.materialVariation;
			dest.headAppearance.specialOverride = src.headAppearance.specialOverride;
			dest.headAppearance.bodyPiece = AppearancePiece.BodyPiece.Head;
			dest.nudeModelOverride = src.nudeModelOverride;
			dest.skinOverride = src.skinOverride;
			dest.ignoreDefaultNudeLegModel = src.ignoreDefaultNudeLegModel;
			dest.HideHelmet = src.HideHelmet;
			dest.gender = src.gender;
			dest.race = src.race;
			dest.subrace = src.subrace;
			dest.racialBodyType = src.racialBodyType;
		}
	}

	public static void CopyStats(CharacterStats dest, CharacterStats src)
	{
		if (!(src == null) && !(dest == null))
		{
			dest.Gender = src.Gender;
			dest.CharacterRace = src.CharacterRace;
			dest.CharacterSubrace = src.CharacterSubrace;
			dest.RacialBodyType = src.RacialBodyType;
			dest.CharacterClass = src.CharacterClass;
			dest.BaseDexterity = src.BaseDexterity;
			dest.BaseMight = src.BaseMight;
			dest.BaseResolve = src.BaseResolve;
			dest.BaseIntellect = src.BaseIntellect;
		}
	}

	public static void CopyPortrait(Portrait dest, Portrait src)
	{
		if (!(src == null) && !(dest == null))
		{
			dest.TextureSmallPath = src.TextureSmallPath;
			dest.TextureLargePath = src.TextureLargePath;
			dest.TextureLarge = src.TextureLarge;
			dest.TextureSmall = src.TextureSmall;
		}
	}

	public static void CopyEquipment(EquipmentSet dest, EquipmentSet src, bool includeWeapons)
	{
		if (src == null || dest == null)
		{
			return;
		}
		for (int i = 0; i < src.Slots.Length; i++)
		{
			if (src.Slots[i].Val != null && i != 9 && i != 10)
			{
				dest.Slots[i].Val = src.Slots[i].Val.Prefab as Equippable;
			}
		}
		if (!includeWeapons)
		{
			return;
		}
		if (src.GetSelectedWeaponSet().PrimaryWeapon != null)
		{
			dest.PrimaryWeapon = src.GetSelectedWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (src.GetSelectedWeaponSet().SecondaryWeapon != null)
		{
			dest.SecondaryWeapon = src.GetSelectedWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		dest.AlternateWeaponSets = new WeaponSet[1];
		dest.AlternateWeaponSets[0] = new WeaponSet();
		if (src.GetFirstAlternateWeaponSet() != null)
		{
			if (src.GetFirstAlternateWeaponSet().PrimaryWeapon != null)
			{
				dest.AlternateWeaponSets[0].PrimaryWeapon = src.GetFirstAlternateWeaponSet().PrimaryWeapon.Prefab as Equippable;
			}
			if (src.GetFirstAlternateWeaponSet().SecondaryWeapon != null)
			{
				dest.AlternateWeaponSets[0].SecondaryWeapon = src.GetFirstAlternateWeaponSet().SecondaryWeapon.Prefab as Equippable;
			}
		}
	}

	public static void CopyMover(Mover dest, Mover src)
	{
		if (!(src == null) && !(dest == null))
		{
			dest.Acceleration = src.Acceleration;
			dest.Radius = src.Radius;
			dest.WalkSpeed = src.WalkSpeed;
			dest.RunSpeed = src.RunSpeed;
			dest.GetComponent<Rigidbody>().mass = src.GetComponent<Rigidbody>().mass;
			dest.GetComponent<Rigidbody>().collisionDetectionMode = src.GetComponent<Rigidbody>().collisionDetectionMode;
			dest.GetComponent<Rigidbody>().isKinematic = src.GetComponent<Rigidbody>().isKinematic;
			dest.GetComponent<Rigidbody>().interpolation = src.GetComponent<Rigidbody>().interpolation;
		}
	}

	public static void CopyFaction(Faction dest, Faction src)
	{
		if (!(src == null) && !(dest == null))
		{
			dest.ModifyToMatch(src);
		}
	}

	public static void LoadEquipment(GameObject character, EquipmentSet equipmentSet)
	{
		bool flag = PE_Paperdoll.IsObjectPaperdoll(character);
		Equipment component = character.GetComponent<Equipment>();
		Equippable equippable = component.UnEquip(component.CurrentItems.PrimaryWeapon);
		if ((bool)equippable)
		{
			GameUtilities.Destroy(equippable.gameObject);
		}
		equippable = component.UnEquip(component.CurrentItems.SecondaryWeapon);
		if ((bool)equippable)
		{
			GameUtilities.Destroy(equippable.gameObject);
		}
		for (int i = 0; i < equipmentSet.Slots.Length; i++)
		{
			if (equipmentSet.Slots[i].Val != null)
			{
				Equippable equippable2 = InstantiateEquippable(equipmentSet.Slots[i].Val.Prefab as Equippable, flag);
				equippable2.Prefab = equipmentSet.Slots[i].Val.Prefab;
				equippable2.gameObject.layer = character.layer;
				equippable = component.Equip(equippable2);
				if ((bool)equippable)
				{
					GameUtilities.Destroy(equippable.gameObject);
				}
			}
			else
			{
				equippable = component.UnEquip((Equippable.EquipmentSlot)i);
				if ((bool)equippable)
				{
					GameUtilities.Destroy(equippable.gameObject);
				}
			}
		}
		if (equipmentSet.PrimaryWeapon != null)
		{
			Equippable equippable3 = InstantiateEquippable(equipmentSet.PrimaryWeapon, flag);
			equippable3.Prefab = equipmentSet.PrimaryWeapon;
			equippable = component.Equip(equippable3, Equippable.EquipmentSlot.PrimaryWeapon, enforceRecoveryPenalty: false);
			if ((bool)equippable)
			{
				GameUtilities.Destroy(equippable.gameObject);
			}
			PaperdollWeaponShaderSwapper.SwapShaders(equippable3.gameObject);
		}
		else
		{
			equippable = component.UnEquip(component.CurrentItems.PrimaryWeapon);
			if ((bool)equippable)
			{
				GameUtilities.Destroy(equippable.gameObject);
			}
		}
		if (equipmentSet.SecondaryWeapon != null)
		{
			Equippable equippable4 = InstantiateEquippable(equipmentSet.SecondaryWeapon, flag, offhand: true);
			equippable4.Prefab = equipmentSet.SecondaryWeapon;
			equippable = component.Equip(equippable4, Equippable.EquipmentSlot.SecondaryWeapon, enforceRecoveryPenalty: false);
			if ((bool)equippable)
			{
				GameUtilities.Destroy(equippable.gameObject);
			}
		}
		else
		{
			equippable = component.UnEquip(component.CurrentItems.SecondaryWeapon);
			if ((bool)equippable)
			{
				GameUtilities.Destroy(equippable.gameObject);
			}
		}
		component.SelectWeaponSet(component.CurrentItems.SelectedWeaponSet, enforceRecoveryPenalty: false);
		if (flag)
		{
			GameUtilities.RecursiveSetLayer(character, PE_Paperdoll.PaperdollLayer);
		}
	}

	private static Equippable InstantiateEquippable(Equippable prefab, bool paperdoll, bool offhand = false)
	{
		PaperdollVariant component = prefab.GetComponent<PaperdollVariant>();
		if (paperdoll && (bool)component)
		{
			Equippable equippable = GameResources.Instantiate<Equippable>(component.VariantPrefab);
			Animator componentInChildren = equippable.GetComponentInChildren<Animator>();
			componentInChildren.SetBool("Offhand", offhand);
			componentInChildren.updateMode = AnimatorUpdateMode.UnscaledTime;
			return equippable;
		}
		return GameResources.Instantiate<Equippable>(prefab);
	}
}
