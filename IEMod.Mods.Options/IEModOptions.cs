// IEMod.Mods.Options.IEModOptions
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.Mods.Options.IEModOptions")]
public static class IEModOptions
{
	[XmlInclude(typeof(Vector3))]
	[PatchedByType("IEMod.Mods.Options.IEModOptions/LayoutOptions")]
	public class LayoutOptions
	{
		public bool BuffsSideLeft;

		public Vector3 FormationPosition;

		public Vector3 PartyBarPosition;

		public Vector3 PartySolidHudPosition;

		public Vector3 HudPosition;

		public Vector3 AbilitiesBarPosition;

		public Vector3 LeftHudBarPosition;

		public Vector3 RightHudBarPosition;

		public Vector3 ClockPosition;

		public Vector3 LogPosition;

		public Vector3 CustomizeButtonPosition;

		public bool HudHorizontal;

		public bool HudTextureHidden;

		public bool LogButtonsLeft;

		public bool ButtonsBackground;

		public bool UsingCustomTextures;

		public bool PortraitHighlightsDisabled;

		public bool PartyBarHorizontal;

		public bool TooltipOffset;

		public string FramePath;

		[PatchedByMember("IEMod.Mods.Options.IEModOptions/LayoutOptions IEMod.Mods.Options.IEModOptions/LayoutOptions::Clone()")]
		public LayoutOptions Clone()
		{
			return (LayoutOptions)MemberwiseClone();
		}

		[PatchedByMember("System.Boolean IEMod.Mods.Options.IEModOptions/LayoutOptions::Equals(System.Object)")]
		public override bool Equals(object obj)
		{
			if (!(obj is LayoutOptions))
			{
				return false;
			}
			LayoutOptions layoutOptions = obj as LayoutOptions;
			return AbilitiesBarPosition == layoutOptions.AbilitiesBarPosition && BuffsSideLeft == layoutOptions.BuffsSideLeft && ButtonsBackground == layoutOptions.ButtonsBackground && ClockPosition == layoutOptions.ClockPosition && CustomizeButtonPosition == layoutOptions.CustomizeButtonPosition && FormationPosition == layoutOptions.FormationPosition && FramePath == layoutOptions.FramePath && HudHorizontal == layoutOptions.HudHorizontal && HudPosition == layoutOptions.HudPosition && HudTextureHidden == layoutOptions.HudTextureHidden && LeftHudBarPosition == layoutOptions.LeftHudBarPosition && LogButtonsLeft == layoutOptions.LogButtonsLeft && LogPosition == layoutOptions.LogPosition && PartyBarHorizontal == layoutOptions.PartyBarHorizontal && PartyBarPosition == layoutOptions.PartyBarPosition && PartySolidHudPosition == layoutOptions.PartySolidHudPosition && PortraitHighlightsDisabled == layoutOptions.PortraitHighlightsDisabled && RightHudBarPosition == layoutOptions.RightHudBarPosition && TooltipOffset == layoutOptions.TooltipOffset && UsingCustomTextures == layoutOptions.UsingCustomTextures;
		}

		[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions/LayoutOptions::.ctor()")]
		public LayoutOptions()
		{
		}
	}

	[PatchedByType("IEMod.Mods.Options.IEModOptions/AutoSaveSetting")]
	[NewType(null, null)]
	public enum AutoSaveSetting
	{
		[Description("Save after every area transition (standard)")]
		Default,
		[Description("Save after area transitions, but only once per 15 minutes")]
		SaveAfter15,
		[Description("Save after area transitions, but only once per 30 minutes")]
		SaveAfter30,
		[Description("Save before every area transition")]
		SaveBefore,
		[Description("Save before area transitions, but only once per 15 minutes")]
		SaveBefore15,
		[Description("Save before area transitions, but only once per 30 minutes")]
		SaveBefore30,
		[Description("Disable autosave")]
		DisableAutosave
	}

	[PatchedByType("IEMod.Mods.Options.IEModOptions/NerfedXpTable")]
	[NewType(null, null)]
	public enum NerfedXpTable
	{
		[Description("Disabled")]
		Disabled,
		[Description("25% increase: 1250,3750,...82,500")]
		Increase25,
		[Description("33% increase: 1330,3990,...87,780")]
		Increase33,
		[Description("50% increase: 1500,4500,...99,000")]
		Increase50,
		[Description("Square progression: 1000,4000,...121,000")]
		Square
	}

	[NewType(null, null)]
	[PatchedByType("IEMod.Mods.Options.IEModOptions/PerEncounterSpells")]
	public enum PerEncounterSpells
	{
		[Description("No Change")]
		NoChange,
		[Description("Per-encounter spells gained at levels 9 and 12")]
		Levels_9_12,
		[Description("Per-encounter spells gained at levels 6, 9 and 12")]
		Levels_6_9_12,
		[Description("Per-encounter spells gained at levels 8, 10, 12, and 14")]
		Levels_8_10_12_14,
		[Description("Per-encounter spells gained at levels 6, 9, 12 and 14")]
		Levels_6_9_12_14,
		[Description("Per-encounter spells gained at levels 6, 8, 10, 12 and 14")]
		Levels_6_8_10_12_14,
		[Description("Per-encounter spells gained at levels 4, 6, 8, 10, 12 and 14")]
		Levels_4_6_8_10_12_14,
		[Description("Per-encounter spells gained at levels 4, 8, 12 and 16")]
		Levels_4_8_12_16,
		[Description("All spells per-encounter")]
		AllPerEncounter,
		[Description("All spells per-rest")]
		AllPerRest
	}

	[NewType(null, null)]
	[PatchedByType("IEMod.Mods.Options.IEModOptions/CipherStartingFocus")]
	public enum CipherStartingFocus
	{
		[Description("No Change (1/4 Max Focus)")]
		Quarter,
		[Description("1/2 Max Focus")]
		Half,
		[Description("3/4 Max Focus")]
		ThreeQuarter,
		[Description("Max Focus")]
		Max,
		[Description("No Focus")]
		None
	}

	[PatchedByType("IEMod.Mods.Options.IEModOptions/ExtraSpellsInGrimoire")]
	[NewType(null, null)]
	public enum ExtraSpellsInGrimoire
	{
		[Description("No extra preparation slots.")]
		None,
		[Description("One (1) extra preparation slot.")]
		One,
		[Description("Two (2) extra preparation slots.")]
		Two,
		[Description("Three (3) extra preparation slots.")]
		Three
	}

	[NewType(null, null)]
	[PatchedByType("IEMod.Mods.Options.IEModOptions/FastSneakOptions")]
	public enum FastSneakOptions
	{
		[Description("Normal")]
		Normal,
		[Description("Fast Scouting, Party LoS")]
		FastScoutingAllLOS,
		[Description("Fast Scouting, Individual LoS")]
		FastScoutingSingleLOS
	}

	[NewType(null, null)]
	[PatchedByType("IEMod.Mods.Options.IEModOptions/MaxAdventurersOptions")]
	public enum MaxAdventurersOptions
	{
		[Description("Normal (8)")]
		Normal_8,
		[Description("Double (16)")]
		Double_16,
		[Description("Triple (24)")]
		Triple_24,
		[Description("Quadra (32)")]
		Quadra_32,
		[Description("One hundred twenty eight (128)")]
		OneHundredTwentyEight
	}

	[PatchedByType("IEMod.Mods.Options.IEModOptions/MaxCampingSuppliesOptions")]
	[NewType(null, null)]
	public enum MaxCampingSuppliesOptions
	{
		[Description("No change (based on the difficulty level)")]
		Default,
		[Description("Default (8)")]
		Normal_8,
		[Description("Double (16)")]
		Double_16,
		[Description("Triple (24)")]
		Triple_24,
		[Description("Quadra (32)")]
		Quadra_32,
		[Description("Sixty-four (64)")]
		Sixty_four_64,
		[Description("Ninety-nine (99)")]
		Ninety_nine_99,
		[Description("Disable check and use")]
		Disabled
	}

	private static bool _enableCustomUi;

	public static XmlNullable<float> SelectionCircleWidth = null;

	public static float DefaultZoom;

	public static LayoutOptions Layout = new LayoutOptions();

	[Description("Display selection circles for neutral NPCs at all times.")]
	[Label("Always show circles")]
	public static bool AlwaysShowCircles = false;

	[XmlElement]
	[Label("One tooltip at a time")]
	[Description("When holding down TAB, displays only one tooltip - for the hovered character.")]
	public static bool OneTooltip = false;

	[Label("Blue selection circles")]
	[Description("Make selection circles for neutral NPCs blue. \n(colorblind mode must be disabled)")]
	public static bool BlueCircles;

	[Description("Blue selection circles become cyan, like in IE games. (requires exit to main menu)")]
	[Label("IE-Like blue")]
	public static bool BlueCirclesBG;

	[Description("Allows looting containers during combat, transfering items between party members, as well as equipping and unequipping all gear, except body armor.")]
	[Label("Unlock combat inventory/loot")]
	public static bool UnlockCombatInv;

	[Description("Some backer names can be immersion breaking, so this mod replaces them with random fantasy names based on their race and gender. Takes effect after reloading or transitionning.")]
	
	[Label("Fantasy names for backers")]
	public static bool FixBackerNames;

	public static bool SaveBeforeTransition;

	public static int SaveInterval;
	
	[Label("Fix moving recovery rate")]
	[Description("This mod removes additional recovery rate penalty for moving characters.")]
	public static bool RemoveMovingRecovery;

	[Description("This mod makes Scouting Mode move at normal running speed (instead of walking speed).  Note: when enemies are visible, your scouting movement speed is reduced to walking speed")]
	[Label("Fast scouting mode")]
	
	public static FastSneakOptions FastSneak;

	[Label("Improved AI")]
	[Description("Some improvements to the combat AI.")]
	
	public static bool ImprovedAI = false;

	[Label("Nerfed XP table")]
	[Description("Increases experience needed. Note: You may need to use ChangeClass to de-level if enabling/increasing this setting midgame.")]
	
	public static NerfedXpTable NerfedXPTableSetting;

	[Description("Random loot will change on every reload. (Loot is set when opening a container.)")]
	
	[Label("Loot shuffler")]
	public static bool LootShuffler;

	[Description("Holding control when toggling fast or slow mode will use more extreme speeds.")]
	[Label("Game speed mod")]
	
	public static bool GameSpeedMod;

	[Description("Allows most spells, abilities, items, and consumables to function outside of combat. (Warning: this can significantly affect game balance, and will cause bugs. Be careful while saving with combat-only effects active.)")]
	[Label("Remove Combat-Only Restrictions")]
	
	public static bool CombatOnlyMod;

	[Description("Modifies which levels of Wizard, Priest and Druid spells are treated as per-encounter.")]
	
	[Label("Per-Encounter Spells Mod")]
	public static PerEncounterSpells PerEncounterSpellsSetting;

	[Description("Modifies the amount of Focus Ciphers begin combat with.")]
	
	[Label("Cipher Base Focus")]
	public static CipherStartingFocus CipherStartingFocusSetting;
	
	[Label("NPC Disposition Fix")]
	[Description("Applies disposition-based bonuses to NPC paladins and priests. Patches in favored and disfavored dispositions for Pallegina's order.")]
	public static bool NPCDispositionFix;

	[Label("Pallegina's Favored Dispositions")]
	
	[Description("Favored dispositions for Pallegina's Order - Brotherhood of the Five Suns.")]
	public static Disposition.Axis PalleginaFavored1;
	
	public static Disposition.Axis PalleginaFavored2;

	[Description("Disables talking to backer NPCs.")]
	[Label("Disable Backer Dialogs")]
	public static bool DisableBackerDialogs;

	[Description("Minimizes the interaction and visibility of tombstones.")]
	[Label("Minimize tombstones")]
	public static bool MinimizeTombstones;

	[Label("Pallegina's Disfavored Dispositions")]
	[Description("Disfavored dispositions for Pallegina's Order - Brotherhood of the Five Suns.")]
	public static Disposition.Axis PalleginaDisfavored1;

	public static Disposition.Axis PalleginaDisfavored2;

	[Description("Disable Friendly Fire")]
	[Label("Disable friendly fire")]
	public static bool DisableFriendlyFire;

	[Description("Calculates bonus spells per rest based on a caster's Intellect. The first bonus spell for a given spell level is gained at an Intellect score of 14 + [Spell Level].  Another bonus spell for that level is added for every 4 additional Intellect points.")]
	[Label("Bonus spells")]
	public static bool BonusSpellsPerDay;

	[Description("Enemies that have temporarily switched to your side are still considered hostile against your abilities, like area attacks. Doesn't change how turned allies are targeted.")]
	[Label("Target turned enemies")]
	public static bool TargetTurnedEnemies;

	[Description("Adds extra spell slots to a wizard's grimoire.  If you change this option in game, you will need to return to the main menu and reload before the change takes effect.")]
	[Label("Extra Wizard Preparation Slots")]
	public static ExtraSpellsInGrimoire ExtraWizardSpells;

	private static MaxAdventurersOptions _maxAdventurersCount;

	[Label("Max Camping Supplies")]
	[Description("Number of purchasable Camping Supplies. You can set the maximum value or you can disable checking methods. (Rest without any CS.)")]
	public static MaxCampingSuppliesOptions MaxCampingSupplies;

	private static Dictionary<string, FieldInfo> _fieldCache;

	[Label("Disable engagement")]
	[Description("Engagement begone. You need to exit to main menu and reload to reenable engagement.")]
	public static bool DisableEngagement;

	[Label("Hide Anticlass Spells from Ability Bar")]
	[Description("Hides Anticlass spells from showing up on the ability bar and adds them to a spell submenu of an existing class.  For example, if you add Druid spells to your Wizard, the added Druid spells will show up under the Wizard spells submenu of the appropriate level.")]
	public static bool HideAnticlassSpells;

	[Label("Hide Weapon Effects")]
	[Description("Hides all of the special effects added to the weapons like the lashes and the glows.")]
	public static bool HideWeaponEffects;

	[Description("Applies the NPC stats located in Managed/iemod/customStats/custom after loading a map")]
	[Label("Custom NPC Stats")]
	public static bool AutoLoadCustomStats;

	[Description("Enables the use of the classic cheat keys and a few more, look at the readme for the available options")]
	[Label("Enable Cheat Keys")]
	public static bool EnableCheatKeys;

	[Description("Gives the chanter the necessary amount of phrases to cast his highest level invocation when combat starts")]
	[Label("Chanter Base Phrases")]
	public static bool ChanterPhraseCount;

	[Description("Unlocks every inventory slot for all characters. Clipping is expected for helmets on Godlike characters since it was never meant to happen. Also allows multiple pets if you equip them on different companions.")]
	[Label("Unlock All Inventory Slots")]
	public static bool AllInventorySlots;

	[Label("Make Cape/Cloak invisible")]
	[Description("Removes the visuals from the capes and cloaks")]
	public static bool CapesHidden;

	[Description("Number between 0-1 that makes the fog lighter or thicker")]
	[Label("Fog Thickness")]
	public static float FogOpacity;

	[Description("Takes away the timer on Spiritshift, allows toggle instead")]
	[Label("Spiritshift Toggle")]
	public static bool SpiritshiftToggleable;

	private static AutoSaveSetting _autoSaveSetting;

	private static Dictionary<string, PropertyInfo> _propertyCache;

	[Description("Auto save setting")]
	[Label("Autosave setting")]
	public static AutoSaveSetting AutosaveSetting
	{
		[PatchedByMember("IEMod.Mods.Options.IEModOptions/AutoSaveSetting IEMod.Mods.Options.IEModOptions::get_AutosaveSetting()")]
		get
		{
			return _autoSaveSetting;
		}
		[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::set_AutosaveSetting(IEMod.Mods.Options.IEModOptions/AutoSaveSetting)")]
		set
		{
			_autoSaveSetting = value;
			OnAutosaveSettingChanged();
		}
	}

	[Description("Changes the maximum count of adventurers you can hire in tavern.")]
	[Label("Max adventurers you can hire")]
	public static MaxAdventurersOptions MaxAdventurersCount
	{
		[PatchedByMember("IEMod.Mods.Options.IEModOptions/MaxAdventurersOptions IEMod.Mods.Options.IEModOptions::get_MaxAdventurersCount()")]
		get
		{
			return _maxAdventurersCount;
		}
		[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::set_MaxAdventurersCount(IEMod.Mods.Options.IEModOptions/MaxAdventurersOptions)")]
		set
		{
			_maxAdventurersCount = value;
			PartyMemberAI.newUpdateMaxAdventurers();
		}
	}

	public static Dictionary<string, FieldInfo> FieldCache
	{
		[PatchedByMember("System.Collections.Generic.Dictionary`2<System.String,System.Reflection.FieldInfo> IEMod.Mods.Options.IEModOptions::get_FieldCache()")]
		get
		{
			if (_fieldCache == null)
			{
				_fieldCache = (from x in typeof(IEModOptions).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
							   where CustomAttributeExtensions.GetCustomAttribute<SaveAttribute>(x) != null
							   select x).ToDictionary((FieldInfo x) => x.Name, (FieldInfo x) => x);
			}
			return _fieldCache;
		}
	}

	public static Dictionary<string, PropertyInfo> PropertyCache
	{
		[PatchedByMember("System.Collections.Generic.Dictionary`2<System.String,System.Reflection.PropertyInfo> IEMod.Mods.Options.IEModOptions::get_PropertyCache()")]
		get
		{
			if (_propertyCache == null)
			{
				_propertyCache = (from x in typeof(IEModOptions).GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
								  where CustomAttributeExtensions.GetCustomAttribute<SaveAttribute>(x) != null
								  select x).ToDictionary((PropertyInfo x) => x.Name, (PropertyInfo x) => x);
			}
			return _propertyCache;
		}
	}

	[Description("Enables UI customization. You need to exit to main menu and reload to toggle this option properly.")]
	[Label("UI Customization")]
	public static bool EnableCustomUi
	{
		[PatchedByMember("System.Boolean IEMod.Mods.Options.IEModOptions::get_EnableCustomUi()")]
		get
		{
			return _enableCustomUi;
		}
		[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::set_EnableCustomUi(System.Boolean)")]
		set
		{
			_enableCustomUi = value;
		}
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::OnAutosaveSettingChanged()")]
	private static void OnAutosaveSettingChanged()
	{
		switch (AutosaveSetting)
		{
			case AutoSaveSetting.SaveAfter15:
			case AutoSaveSetting.SaveBefore15:
				SaveInterval = 15;
				break;
			case AutoSaveSetting.SaveAfter30:
			case AutoSaveSetting.SaveBefore30:
				SaveInterval = 30;
				break;
			case AutoSaveSetting.Default:
			case AutoSaveSetting.SaveBefore:
				SaveInterval = 0;
				break;
			case AutoSaveSetting.DisableAutosave:
				SaveInterval = -1;
				break;
			default:
				throw IEDebug.Exception(null, $"Invalid AutoSaveSetting: {AutosaveSetting}");
		}
		SaveBeforeTransition = AutosaveSetting.ToString().Contains("Before");
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::LoadFromPrefs()")]
	public static void LoadFromPrefs()
	{
		foreach (FieldInfo value in FieldCache.Values)
		{
			Type fieldType = value.FieldType;
			object @object = PlayerPrefsHelper.GetObject(value.Name, fieldType);
			value.SetValue(null, @object);
		}
		foreach (PropertyInfo value2 in PropertyCache.Values)
		{
			Type propertyType = value2.PropertyType;
			object object2 = PlayerPrefsHelper.GetObject(value2.Name, propertyType);
			value2.SetValue(null, object2, null);
		}
	}

	[PatchedByMember("System.String IEMod.Mods.Options.IEModOptions::GetSettingName(System.String)")]
	public static string GetSettingName(string memberName)
	{
		return memberName;
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::DeleteAllSettings()")]
	public static void DeleteAllSettings()
	{
		foreach (FieldInfo value in FieldCache.Values)
		{
			PlayerPrefs.DeleteKey(GetSettingName(value.Name));
		}
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.IEModOptions::SaveToPrefs()")]
	public static void SaveToPrefs()
	{
		foreach (FieldInfo value3 in FieldCache.Values)
		{
			Type fieldType = value3.FieldType;
			object value = value3.GetValue(null);
			PlayerPrefsHelper.SetObject(GetSettingName(value3.Name), fieldType, value);
		}
		foreach (PropertyInfo value4 in PropertyCache.Values)
		{
			Type propertyType = value4.PropertyType;
			object value2 = value4.GetValue(null, null);
			PlayerPrefsHelper.SetObject(GetSettingName(value4.Name), propertyType, value2);
		}
	}

	[PatchedByMember("System.Boolean IEMod.Mods.Options.IEModOptions::IsIdenticalToPrefs()")]
	public static bool IsIdenticalToPrefs()
	{
		foreach (FieldInfo value3 in FieldCache.Values)
		{
			object value = value3.GetValue(null);
			object @object = PlayerPrefsHelper.GetObject(GetSettingName(value3.Name), value3.FieldType);
			if (!object.Equals(value, @object))
			{
				return false;
			}
		}
		foreach (PropertyInfo value4 in PropertyCache.Values)
		{
			object value2 = value4.GetValue(null, null);
			object object2 = PlayerPrefsHelper.GetObject(GetSettingName(value4.Name), value4.PropertyType);
			if (!object.Equals(value2, object2))
			{
				return false;
			}
		}
		return true;
	}
}
