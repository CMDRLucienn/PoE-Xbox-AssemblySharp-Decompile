using System;
using UnityEngine;

[Serializable]
public class StrongholdUpgrade
{
	public enum Type
	{
		Barbican,
		WestCurtainWall,
		SouthCurtainWall,
		Bailey,
		MainKeep,
		Barracks,
		Dungeons,
		BeastVault,
		Towers,
		Library,
		CraftHall,
		AritficersHall,
		Forum,
		MerchantStalls,
		CurioShop,
		TrainingGrounds,
		Chapel,
		HedgeMaze,
		BotanicalGarden,
		RoadRepairs,
		WardensLodge,
		WoodlandTrails,
		Bedding,
		AdditionalStorage,
		Hearth,
		Lab,
		CourtyardPool,
		EasternBarbican,
		Count,
		None
	}

	public enum InteriorLocation
	{
		None,
		House_Lower,
		House_Upper,
		Hall,
		Dungeon,
		Library,
		Barracks
	}

	public Type UpgradeType;

	public InteriorLocation InteriorUpgradeLocation;

	public int Cost;

	public int TimeToBuild;

	public int Level;

	public int PrestigeAdjustment;

	public int SecurityAdjustment;

	public CharacterStats.AttributeScoreType Attribute;

	public int AttributeAdjustment;

	public CharacterStats.SkillType Skill;

	public int SkillAdjustment;

	public Affliction Boon;

	public RegeneratingItem[] IngredientList;

	public Vector2[] Tiles;

	public bool FullMapInteriorUpgrade = true;

	public Vector2[] InteriorTiles;

	public int UiOrderNumber;

	public GUIDatabaseString Name;

	public GUIDatabaseString Description;

	public Texture2D Icon;

	public Type Prerequisite = Type.None;

	public bool Destructible = true;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the upgrade is built and back to 0 when it is destroyed.")]
	public string UpgradeCompletedGlobalVariableName;
}
