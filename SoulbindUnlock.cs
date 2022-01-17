using System;
using UnityEngine;

[Serializable]
public class SoulbindUnlock
{
	public string DesignNote;

	[Tooltip("If the bound character isn't of this class, the unlock will be skipped.")]
	public CharacterStats.Class RequiresClass;

	[Tooltip("The stat to look at to unlock this ability.")]
	public SoulbindUnlockType UnlockStatType;

	[Tooltip("Prerequisites applied to slain or damaged enemies to determine if they advance this unlock.")]
	public PrerequisiteData[] UnlockPrerequisites;

	[Tooltip("If set, only damage or kills made by critical hits will be counted.")]
	public bool PrereqCriticalHit;

	[Tooltip("If not NONE, only damage or kills made with specified damage type will be counted.")]
	public DamagePacket.DamageType PrereqDamageType = DamagePacket.DamageType.None;

	public Affliction AfflictionValue;

	public CharacterStats.AttributeScoreType AttributeValue;

	[Tooltip("How much of the Unlock Stat Type is needed for the unlock, or the value of the global variable.")]
	public int UnlockStatLevel;

	[GlobalVariableString]
	[Tooltip("A boolean global variable to check to advance this unlock.")]
	public string UnlockGlobalVar;

	public ItemsDatabaseString LoreToAdd = new ItemsDatabaseString();

	public ItemsDatabaseString OverrideRequirementText = new ItemsDatabaseString();

	[Tooltip("These mods are added to the item when this unlock is achieved.")]
	public ItemMod[] ModsToApply;

	[Tooltip("These mods are removed from the item when this unlock is achieved.")]
	public ItemMod[] ModsToRemove;

	public Texture2D OverrideIconTexture;

	public Texture2D OverrideIconLargeTexture;

	[ResourcesImageProperty]
	public string OverridePencilSketch;

	public Equippable OverrideAppearanceWith;

	public bool CanBeUnlockedDegenerately
	{
		get
		{
			if (UnlockStatType != SoulbindUnlockType.Damage && UnlockStatType != 0)
			{
				return false;
			}
			if (UnlockPrerequisites.Length == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsValidForClass(CharacterStats.Class cclass)
	{
		if (RequiresClass != 0)
		{
			return cclass == RequiresClass;
		}
		return true;
	}

	public bool CheckDamageTypePrerequisite(DamageInfo damage)
	{
		if (PrereqDamageType != DamagePacket.DamageType.None && PrereqDamageType != DamagePacket.DamageType.All && (damage == null || damage.DamageType != PrereqDamageType))
		{
			return false;
		}
		return true;
	}

	public bool CheckDamageTypePrerequisite(DamagePacket.DamageType damageType)
	{
		if (PrereqDamageType != DamagePacket.DamageType.None && PrereqDamageType != DamagePacket.DamageType.All && damageType != PrereqDamageType)
		{
			return false;
		}
		return true;
	}

	public bool CheckOtherPrerequisites(GameObject owner, GameObject target, DamageInfo damage)
	{
		if (PrereqCriticalHit && (damage == null || !damage.IsCriticalHit))
		{
			return false;
		}
		return PrerequisiteData.CheckPrerequisites(owner, target, UnlockPrerequisites, target);
	}

	public bool CheckPrerequisites(GameObject owner, GameObject target, DamageInfo damage)
	{
		if (!CheckDamageTypePrerequisite(damage?.DamageType ?? DamagePacket.DamageType.None))
		{
			return false;
		}
		return CheckOtherPrerequisites(owner, target, damage);
	}
}
