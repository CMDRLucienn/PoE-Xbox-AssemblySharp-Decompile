using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public class CoreData
	{
		public int Level;

		public Gender Gender;

		public Race Race;

		public Subrace Subrace;

		public Race RacialBodyType;

		public Class Class;

		public Culture Culture;

		public Background Background;

		public Religion.Deity Deity;

		public Religion.PaladinOrder PaladinOrder;

		public bool IsPlayerCharacter;

		public bool IsHiredAdventurer;

		public List<MonoBehaviour> KnownSkills = new List<MonoBehaviour>();

		public int[] BaseStats = new int[6];

		public int[] SkillValues = new int[6];

		public CoreData Copy()
		{
			CoreData coreData = new CoreData();
			coreData.Level = Level;
			coreData.Gender = Gender;
			coreData.Race = Race;
			coreData.Subrace = Subrace;
			coreData.RacialBodyType = RacialBodyType;
			coreData.Class = Class;
			coreData.Culture = Culture;
			coreData.Background = Background;
			coreData.KnownSkills = KnownSkills.ToList();
			coreData.Deity = Deity;
			coreData.PaladinOrder = PaladinOrder;
			coreData.IsPlayerCharacter = IsPlayerCharacter;
			coreData.IsHiredAdventurer = IsHiredAdventurer;
			for (int i = 0; i < BaseStats.Length; i++)
			{
				coreData.BaseStats[i] = BaseStats[i];
			}
			for (int j = 0; j < SkillValues.Length; j++)
			{
				coreData.SkillValues[j] = SkillValues[j];
			}
			return coreData;
		}

		private Phrase GetPhrase(MonoBehaviour ability)
		{
			return ability as Phrase;
		}

		private Recipe GetRecipe(MonoBehaviour ability)
		{
			return ability as Recipe;
		}

		private GenericAbility GetGenericAbility(MonoBehaviour ability)
		{
			return ability as GenericAbility;
		}

		private GenericTalent GetGenericTalent(MonoBehaviour ability)
		{
			return ability as GenericTalent;
		}

		private Phrase GetPhrase(GameObject ability)
		{
			return ability.GetComponent<Phrase>();
		}

		private Recipe GetRecipe(GameObject ability)
		{
			return ability.GetComponent<Recipe>();
		}

		private GenericAbility GetGenericAbility(GameObject ability)
		{
			return ability.GetComponent<GenericAbility>();
		}

		private GenericTalent GetGenericTalent(GameObject ability)
		{
			return ability.GetComponent<GenericTalent>();
		}

		public MonoBehaviour GetKnownSkill(GameObject skill)
		{
			if (skill == null)
			{
				return null;
			}
			MonoBehaviour result = null;
			if (GetGenericAbility(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetGenericAbility(s) != null && GenericAbility.NameComparer.Instance.Equals(GetGenericAbility(s), GetGenericAbility(skill)));
			}
			else if (GetGenericTalent(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetGenericTalent(s) != null && GenericTalent.NameComparer.Instance.Equals(GetGenericTalent(s), GetGenericTalent(skill)));
			}
			else if (GetPhrase(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetPhrase(s) != null && Phrase.NameComparer.Instance.Equals(GetPhrase(s), GetPhrase(skill)));
			}
			else if (GetRecipe(skill) != null)
			{
				return GetRecipe(skill);
			}
			return result;
		}

		public MonoBehaviour GetKnownSkill(MonoBehaviour skill)
		{
			if (skill == null)
			{
				return null;
			}
			MonoBehaviour result = null;
			if (GetGenericAbility(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetGenericAbility(s) != null && GenericAbility.NameComparer.Instance.Equals(GetGenericAbility(s), GetGenericAbility(skill)));
			}
			else if (GetGenericTalent(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetGenericTalent(s) != null && GenericTalent.NameComparer.Instance.Equals(GetGenericTalent(s), GetGenericTalent(skill)));
			}
			else if (GetPhrase(skill) != null)
			{
				result = KnownSkills.Find((MonoBehaviour s) => GetPhrase(s) != null && Phrase.NameComparer.Instance.Equals(GetPhrase(s), GetPhrase(skill)));
			}
			else if (GetRecipe(skill) != null)
			{
				return skill;
			}
			return result;
		}

		public void RemoveKnownSkill(GameObject skill)
		{
			if (!(skill == null))
			{
				MonoBehaviour knownSkill = GetKnownSkill(skill);
				while (knownSkill != null)
				{
					KnownSkills.Remove(knownSkill);
					knownSkill = GetKnownSkill(skill);
				}
			}
		}

		public void RemoveKnownSkill(MonoBehaviour skill)
		{
			if (!(skill == null))
			{
				MonoBehaviour knownSkill = GetKnownSkill(skill);
				while (knownSkill != null)
				{
					KnownSkills.Remove(knownSkill);
					knownSkill = GetKnownSkill(skill);
				}
			}
		}
	}

	public delegate void StatusEffectEvent(GameObject sender, StatusEffect effect);

	public delegate void StatusEffectBoolEvent(GameObject sender, StatusEffect effect, bool isFromAura);

	public delegate void AfflictionEvent(GameObject sender, Affliction affliction);

	public delegate void DefenseEvent(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref int defense);

	public delegate void CheckImmunity(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref bool isImmune);

	public enum FatigueLevel
	{
		None,
		Minor,
		Major,
		Critical
	}

	public delegate void FatigueLevelChanged(FatigueLevel newlevel);

	public delegate void DamageModifier(ref DamageInfo damage, GameObject enemy);

	public enum Race
	{
		Undefined,
		Human,
		Elf,
		Dwarf,
		Godlike,
		Orlan,
		[Obsolete]
		Undead_DO_NOT_USE,
		Aumaua,
		[Obsolete]
		Faunal_DO_NOT_USE,
		[Obsolete]
		Giant_DO_NOT_USE,
		Beast,
		Primordial,
		Spirit,
		Vessel,
		Wilder,
		Count
	}

	public enum Culture
	{
		Undefined,
		Aedyr,
		DeadfireArchipelago,
		IxamitlPlains,
		OldVailia,
		Ruatai,
		TheLivingLands,
		TheWhiteThatWends,
		TheDyrwood,
		TheVailianRepublics,
		Nassitaq,
		EirGlanfath,
		Count
	}

	public enum Subrace
	{
		Undefined,
		Meadow_Human,
		Ocean_Human,
		Savannah_Human,
		Wood_Elf,
		Snow_Elf,
		Mountain_Dwarf,
		Boreal_Dwarf,
		Death_Godlike,
		Fire_Godlike,
		Nature_Godlike,
		Moon_Godlike,
		Hearth_Orlan,
		Wild_Orlan,
		Coastal_Aumaua,
		Island_Aumaua,
		Avian_Godlike,
		Advanced_Construct
	}

	public enum Class
	{
		Undefined,
		Fighter,
		Rogue,
		Priest,
		Wizard,
		Barbarian,
		Ranger,
		Druid,
		Paladin,
		Monk,
		Cipher,
		Chanter,
		Troll,
		Ogre,
		Wolf,
		Spider,
		Ooze,
		Stelgaer,
		Imp,
		DankSpore,
		SwampLurker,
		Eoten,
		Xaurip,
		Vithrack,
		WillOWisp,
		Delemgan,
		Pwgra,
		Wurm,
		Skuldr,
		Drake,
		SkyDragon,
		AdraDragon,
		Blight,
		Animat,
		FleshConstruct,
		Shadow,
		Phantom,
		CeanGwla,
		Skeleton,
		Revenant,
		Gul,
		Dargul,
		Fampyr,
		Wicht,
		Beetle,
		AnimalCompanion,
		WeakEnemy,
		HeraldOfWoedica,
		PriestOfWoedica,
		Lagufaeth,
		Lich,
		Eyeless,
		Kraken,
		PlayerTrap,
		Count
	}

	public enum Background
	{
		Undefined,
		Aristocrat,
		Artist,
		Colonist,
		Dissident,
		Drifter,
		Explorer,
		Hunter,
		Laborer,
		Mercenary,
		Merchant,
		Mystic,
		Philosopher,
		Priest,
		Raider,
		Slave,
		Scholar,
		Scientist,
		Farmer,
		Soldier,
		Midwife,
		Gentry,
		Trapper,
		Count
	}

	public enum AttributeScoreType
	{
		Resolve,
		Might,
		Dexterity,
		Intellect,
		Constitution,
		Perception,
		Count
	}

	public enum DefenseType
	{
		Deflect,
		Fortitude,
		Reflex,
		Will,
		Count,
		None
	}

	public enum PerceptionAdjustment
	{
		Supernatural,
		Keen,
		Normal,
		Poor,
		Oblivious
	}

	public enum NoiseLevelType
	{
		Quiet,
		Loud,
		ExtremelyLoud
	}

	public enum SkillType
	{
		Stealth,
		Athletics,
		Lore,
		Mechanics,
		Survival,
		Crafting,
		Count
	}

	public enum LoreRevealStatus
	{
		Nothing,
		Health,
		HealthDefense,
		HealthDefenseDT,
		Count
	}

	public enum EffectType
	{
		Beneficial,
		Hostile,
		All
	}

	public static bool DebugStats = false;

	public const float MAX_SKILL = 100f;

	public const int MAX_EXPERIENCE = 10000000;

	public const int MAX_LEVEL_ETERNITY = 16;

	public const int MAX_NPC_LEVEL_ETERNITY = 21;

	public const int AVG_ATTRIBUTE = 10;

	public const float MAX_PERCEPTION_DISTANCE = 7f;

	public const float MAX_STEALTH_RADIUS = 4f;

	public const float DEFAULT_WALK_SPEED = 2f;

	public const float DEFAULT_RUN_SPEED = 4f;

	public const float DEFAULT_ANIMAL_COMPANION_WALK_SPEED = 2f;

	public const float DEFAULT_ANIMAL_COMPANION_RUN_SPEED = 5.5f;

	public static float CombatStaminaRechargeRate = 1f;

	public static float NormalStaminaRechargeRate = 10f;

	public static float StaminaRechargeDelay = 3f;

	public static float PiercingDTReduction = 1f;

	public static float ModalRecoveryTime = 3f;

	public static float GrazeThreshhold = 50f;

	public static float HitMultiplier = 1f;

	public static int[,] RaceAbilityAdjustment = new int[15, 6]
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 1, 1, 0, 0, 0, 0 },
		{ 0, 0, 1, 0, 0, 1 },
		{ 0, 2, -1, 0, 1, 0 },
		{ 0, 0, 1, 1, 0, 0 },
		{ 1, -1, 0, 0, 0, 2 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 2, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0 }
	};

	public static int[,] CultureAbilityAdjustment = new int[12, 6]
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0, 0 },
		{ 0, 0, 1, 0, 0, 0 },
		{ 1, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 1, 0, 0 },
		{ 0, 0, 0, 0, 1, 0 },
		{ 0, 1, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 1 },
		{ 1, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 1, 0, 0 },
		{ 0, 0, 0, 0, 0, 1 },
		{ 0, 0, 0, 0, 1, 0 }
	};

	public static int[,] ClassSkillAdjustment = new int[12, 6]
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 1, 1, 0, 1, 0 },
		{ 1, 0, 0, 2, 0, 0 },
		{ 0, 1, 2, 0, 0, 0 },
		{ 0, 0, 2, 1, 0, 0 },
		{ 0, 2, 0, 0, 1, 0 },
		{ 1, 0, 0, 0, 2, 0 },
		{ 0, 0, 1, 0, 2, 0 },
		{ 0, 2, 1, 0, 0, 0 },
		{ 1, 1, 0, 0, 1, 0 },
		{ 1, 0, 1, 1, 0, 0 },
		{ 0, 0, 2, 1, 0, 0 }
	};

	public static int[,] BackgroundSkillAdjustment = new int[23, 6]
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 0, 0, 0, 0, 2, 0 },
		{ 1, 0, 1, 0, 0, 0 },
		{ 1, 0, 0, 1, 0, 0 },
		{ 0, 0, 1, 0, 1, 0 },
		{ 1, 0, 0, 0, 1, 0 },
		{ 0, 1, 0, 1, 0, 0 },
		{ 0, 1, 1, 0, 0, 0 },
		{ 0, 0, 1, 1, 0, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 1, 1, 0, 0, 0, 0 },
		{ 0, 1, 0, 0, 1, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 0, 0, 1, 1, 0, 0 },
		{ 0, 1, 0, 0, 1, 0 },
		{ 0, 1, 1, 0, 0, 0 },
		{ 0, 0, 1, 0, 1, 0 },
		{ 0, 0, 2, 0, 0, 0 },
		{ 1, 0, 0, 1, 0, 0 }
	};

	public CharacterDatabaseString DisplayName = new CharacterDatabaseString();

	[Persistent]
	[HideInInspector]
	public string OverrideName;

	[Persistent]
	public Gender Gender;

	[Persistent]
	public Race CharacterRace;

	[Persistent]
	public Subrace CharacterSubrace;

	[Persistent]
	public Race RacialBodyType;

	[Persistent]
	public Culture CharacterCulture;

	[Persistent]
	public Class CharacterClass;

	[Persistent]
	public Religion.Deity Deity;

	[Persistent]
	public Religion.PaladinOrder PaladinOrder;

	[Persistent]
	public Background CharacterBackground;

	[Tooltip("Comma-delimited list of keywords that apply to this character.")]
	public string Keywords = "";

	private KeywordCollection m_keywords;

	[Persistent]
	[Range(0f, 21f)]
	public int Level = 1;

	private ScaledContent m_ScaledContent;

	[Persistent]
	private int m_NotifiedLevel = 1;

	private int m_experience;

	[Persistent]
	[HideInInspector]
	public int RemainingSkillPoints;

	[Persistent]
	[Range(0f, 100f)]
	public int BaseMight = 10;

	[Persistent]
	[Range(0f, 100f)]
	public int BaseConstitution = 10;

	[Persistent]
	[Range(0f, 100f)]
	public int BaseDexterity = 10;

	[Persistent]
	[Range(0f, 100f)]
	public int BasePerception = 10;

	[Persistent]
	[Range(0f, 100f)]
	public int BaseIntellect = 10;

	[Persistent]
	[Range(0f, 100f)]
	public int BaseResolve = 10;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int ResolveBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int MightBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int DexterityBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int IntellectBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int ConstitutionBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int PerceptionBonus;

	[Persistent]
	public int AthleticsSkill;

	[Persistent]
	public int LoreSkill;

	[Persistent]
	public int MechanicsSkill;

	[Persistent]
	public int SurvivalSkill;

	[Persistent]
	public int StealthSkill;

	[Persistent]
	[HideInInspector]
	public int CraftingSkill;

	[Persistent]
	public int OverrideSurvivalSkillLevel;

	public bool ImmuneToEngagement;

	[Persistent]
	[HideInInspector]
	public int StealthBonus;

	[Persistent]
	[HideInInspector]
	public int AthleticsBonus;

	[Persistent]
	[HideInInspector]
	public int LoreBonus;

	[Persistent]
	[HideInInspector]
	public int MechanicsBonus;

	[Persistent]
	[HideInInspector]
	public int SurvivalBonus;

	[Persistent]
	[HideInInspector]
	public int CraftingBonus;

	public BestiaryReference BestiaryReference;

	[Persistent]
	[HideInInspector]
	public float MaxHealth = 100f;

	[Persistent]
	[HideInInspector]
	public float MaxStamina = 100f;

	[Persistent]
	[HideInInspector]
	public float StaminaBonus;

	[Persistent]
	[HideInInspector]
	public float StaminaRechargeBonus;

	[Persistent]
	[HideInInspector]
	public float StaminaRechargeMult = 1f;

	[Persistent]
	[HideInInspector]
	public int HealthStaminaPerLevel = 8;

	[Persistent]
	[HideInInspector]
	public float ClassHealthMultiplier = 8f;

	[Tooltip("Determines the range a creature can detect a stealth enemy or someone stealing from containers")]
	public PerceptionAdjustment PerceptionType = PerceptionAdjustment.Normal;

	private HashSet<string> m_MarkersAppliedThisCombat = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

	[Persistent]
	private FatigueLevel m_CurrentFatigueLevel;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int MeleeAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int RangedAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int VesselAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BeastAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int WilderAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int PrimordialAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int FlankedAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BaseDeflection = 1;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BaseFortitude = 1;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BaseReflexes = 1;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BaseWill = 1;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int VeilDeflectionBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int KnockdownDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int StunDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int PoisonDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int DiseaseDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int PushDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int WhileKnockeddownDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int WhileStunnedDefenseBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int BonusShieldDeflection;

	[Persistent]
	[HideInInspector]
	public bool EvadeEverything;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int NearestAllyWithSharedTargetAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Tooltip("Increases consumables duration or effectiveness.")]
	public float PotionEffectiveness = 1f;

	[Persistent]
	[HideInInspector]
	[Tooltip("Increased accuracy for traps from items.")]
	[Range(0f, 100f)]
	public int TrapAccuracyBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public float TrapDamageOrDurationMult = 1f;

	[Persistent]
	[Range(0f, 100f)]
	public int EngageableEnemyCount = 1;

	[Persistent]
	[HideInInspector]
	[Range(0f, 10f)]
	public float EngagementDistanceBonus;

	[Persistent]
	[HideInInspector]
	[Range(0f, 100f)]
	public int DisengagementAccuracyBonus = 5;

	[HideInInspector]
	[Persistent]
	public int DisengagementDefenseBonus;

	[Persistent]
	[HideInInspector]
	public bool ImmuneToEngageStop;

	[Persistent]
	[Range(0.01f, 5f)]
	public float AttackSpeedMultiplier = 1f;

	[Persistent]
	[Tooltip("Increases regular attack speed if you are dual wielding not including abilities.")]
	[Range(0.01f, 5f)]
	[HideInInspector]
	public float DualWieldAttackSpeedMultiplier = 1f;

	[Persistent]
	[Tooltip("Increases melee attack speed not including abilities.")]
	[Range(0.01f, 5f)]
	[HideInInspector]
	public float MeleeAttackSpeedMultiplier = 1f;

	[Persistent]
	[Tooltip("Increases ranged attack speed not including abilities.")]
	[Range(0.01f, 5f)]
	[HideInInspector]
	public float RangedAttackSpeedMultiplier = 1f;

	[Persistent]
	[Tooltip("Increases ranged attack speed including abilties.")]
	[Range(0.01f, 5f)]
	public float RateOfFireMultiplier = 1f;

	[Persistent]
	[Range(0.01f, 5f)]
	public float ReloadSpeedMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float MeleeAttackDistanceMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float RangedAttackDistanceMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float DamageMinBonus;

	[Persistent]
	[HideInInspector]
	public float WeaponDamageMinMult = 1f;

	[Persistent]
	[HideInInspector]
	public float BonusRangedWeaponCloseEnemyDamageMult = 1f;

	[Persistent]
	[HideInInspector]
	public float BonusDTFromArmor;

	[Persistent]
	[HideInInspector]
	public float DTBypass;

	[Persistent]
	[HideInInspector]
	[Tooltip("Bonus DT bypass on melee attacks.")]
	public float MeleeDTBypass;

	[Persistent]
	[HideInInspector]
	[Tooltip("Bonus DT bypass on ranged attacks.")]
	public float RangedDTBypass;

	[Persistent]
	[HideInInspector]
	public float MeleeDamageRangePctIncreaseToMin;

	[Persistent]
	[HideInInspector]
	public float EnemyCritToHitPercent;

	[Persistent]
	[HideInInspector]
	public float EnemyHitToGrazePercent;

	[Persistent]
	[HideInInspector]
	public float EnemyReflexGrazeToMissPercent;

	[Persistent]
	[HideInInspector]
	public float EnemyReflexHitToGrazePercent;

	[Persistent]
	[HideInInspector]
	public float EnemyGrazeToMissPercent;

	[Persistent]
	[HideInInspector]
	public float EnemyDeflectReflexHitToGrazePercent;

	[Persistent]
	[HideInInspector]
	public float EnemyFortitudeWillHitToGrazePercent;

	[Persistent]
	[HideInInspector]
	public float BonusGrazeToHitPercent;

	[Persistent]
	[HideInInspector]
	public float BonusGrazeToMissPercent;

	[Persistent]
	[HideInInspector]
	public float BonusGrazeToHitPercentMeleeOneHanded;

	[Persistent]
	[HideInInspector]
	public float BonusHitToCritPercentMeleeOneHanded;

	[Persistent]
	[HideInInspector]
	public float BonusCritToHitPercent;

	[Persistent]
	[HideInInspector]
	public float BonusMissToGrazePercent;

	[Persistent]
	[HideInInspector]
	public float BonusHitToCritPercent;

	[Persistent]
	[HideInInspector]
	public float BonusHitToCritPercentAll;

	[Persistent]
	[HideInInspector]
	public float BonusHitToCritPercentEnemyBelow10Percent;

	[Persistent]
	[HideInInspector]
	public float BonusHitToGrazePercent;

	[Persistent]
	[HideInInspector]
	public int ExtraStraightBounces;

	[Persistent]
	[HideInInspector]
	public float HostileEffectDurationMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float HostileAOEDamageMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float CritHitDamageMultiplierBonus;

	[Persistent]
	[HideInInspector]
	public float CritHitDamageMultiplierBonusEnemyBelow10Percent;

	[Persistent]
	[HideInInspector]
	public int ImprovedFlanking;

	[Persistent]
	[HideInInspector]
	public int AttackerToHitRollOverride = -1;

	[Persistent]
	[HideInInspector]
	public int EnemiesNeededToFlank = 2;

	[Persistent]
	[HideInInspector]
	public int DeathPrevented;

	[Persistent]
	[HideInInspector]
	public float CurrentGrimoireCooldown;

	[Persistent]
	[HideInInspector]
	public float GrimoireCooldownBonus;

	[Persistent]
	[HideInInspector]
	public float WeaponSwitchCooldownBonus;

	[Persistent]
	[HideInInspector]
	public float DOTTickMult = 1f;

	[Persistent]
	[HideInInspector]
	public float WoundDelay;

	[Persistent]
	[HideInInspector]
	public float FinishingBlowDamageMult = 1f;

	[Persistent]
	[HideInInspector]
	public float ZealousAuraRadiusMult = 1f;

	[Persistent]
	[HideInInspector]
	public int UnconsciousnessDelayed;

	[Persistent]
	[HideInInspector]
	public float NegMoveTickMult = 1f;

	[Persistent]
	[HideInInspector]
	public float FocusGainMult = 1f;

	[HideInInspector]
	[Persistent]
	public int SpellDefenseBonus;

	[HideInInspector]
	[Persistent]
	public int RangedDeflectionBonus;

	[HideInInspector]
	[Persistent]
	public int TwoHandedDeflectionBonus;

	[HideInInspector]
	[Persistent]
	public int BonusUsesPerRestPastThree;

	[Persistent]
	[HideInInspector]
	public float PoisonTickMult = 1f;

	[Persistent]
	[HideInInspector]
	public float DiseaseTickMult = 1f;

	[Persistent]
	[HideInInspector]
	public float StalkersLinkDamageMult = 1f;

	[Persistent]
	[HideInInspector]
	public float ChanterPhraseRadiusMult = 1f;

	[Persistent]
	[HideInInspector]
	public float BonusHealMult = 1f;

	[Persistent]
	[HideInInspector]
	public float BonusHealingGivenMult = 1f;

	[Persistent]
	[HideInInspector]
	public float AoERadiusMult = 1f;

	[Persistent]
	[HideInInspector]
	public float WildstrikeDamageMult = 1f;

	[Persistent]
	[HideInInspector]
	public float MaxStaminaMultiplier = 1f;

	[Persistent]
	[HideInInspector]
	public float ArmorSpeedFactorAdj;

	[Persistent]
	[HideInInspector]
	public float SingleWeaponSpeedFactorAdj;

	[Persistent]
	[HideInInspector]
	[Tooltip("Flat increases recovery multiplier when moving with a ranged weapon in the primary slot.")]
	public float RangedMovingRecoveryReductionPct;

	[Persistent]
	[HideInInspector]
	public float ExtraSimultaneousHitDefenseBonus;

	[Persistent]
	[HideInInspector]
	public int BonusWeaponSets;

	[Persistent]
	[HideInInspector]
	public int BonusQuickSlots;

	[HideInInspector]
	public int DefensiveBondBonus;

	[HideInInspector]
	public bool NegateNextRecovery;

	private static HashSet<GameObject> s_EnemiesSpottedInStealth = new HashSet<GameObject>();

	public List<GenericAbility> Abilities = new List<GenericAbility>();

	[Persistent]
	public List<Affliction> AfflictionImmunities = new List<Affliction>();

	[Persistent]
	public List<GenericTalent> Talents = new List<GenericTalent>();

	private List<StatusEffect> m_statusEffects = new List<StatusEffect>();

	private BindingList<GenericAbility> m_abilities = new BindingList<GenericAbility>();

	private BindingList<GenericTalent> m_talents = new BindingList<GenericTalent>();

	private static ObjectPool<Dictionary<int, StatusEffect>> s_statusEffectDictionaryPool = new ObjectPool<Dictionary<int, StatusEffect>>(128);

	private Dictionary<int, Dictionary<int, StatusEffect>> m_stackTracker;

	private bool m_updateTracker = true;

	[Persistent]
	private float[] m_damageThreshhold = new float[10];

	[Persistent]
	private float[] m_bonusDamage = new float[7];

	[Persistent]
	private float[] m_bonusDamagePerType = new float[7];

	[Persistent]
	private float[] m_bonusDamagePerRace = new float[15];

	[Persistent]
	private uint m_availableStatusEffectID;

	[Persistent]
	private NoiseLevelType m_noiseLevel;

	[Persistent]
	private float m_noiseTimer;

	[Persistent]
	private uint m_loreReveal;

	[Persistent]
	private float m_detectTimer;

	[Persistent]
	private int m_redirectMeleeAttacks;

	[Persistent]
	private int[] m_spellCastCount = new int[8];

	[Persistent]
	private int[] m_spellCastBonus = new int[8];

	private const int m_damaged_weapon_accuracy_bonus = -10;

	private const float m_damaged_armor_speed_multiplier = 1.1f;

	private int m_invisState;

	private Stronghold m_stronghold;

	[Persistent]
	private float m_recoveryTimer;

	private float m_totalRecoveryTime;

	private float m_weaponSwitchingTimer;

	private float m_interruptTimer;

	[Persistent]
	private float[] m_modalCooldownTimer = new float[7];

	private float m_idleTimer;

	private static float fatigueSoundDelay = 180f;

	private static bool s_PlayFatigueSoundWhenNotLoading = false;

	protected BestiaryReference m_bestiaryReference;

	protected Mover m_mover;

	protected Equipment m_equipment;

	private Dictionary<int, float> m_trapCooldownTimers = new Dictionary<int, float>();

	public BitArray DefensesKnown = new BitArray(4);

	public BitArray DTsKnown = new BitArray(7);

	private uint m_EquipmentLock;

	[Persistent]
	private int m_LastLevelUpNotified;

	[HideInInspector]
	public List<CharacterStats> Flankers = new List<CharacterStats>();

	public static int PlayerLevelCap => 12 + (GameUtilities.HasPX1() ? 2 : 0) + (GameUtilities.HasPX2() ? 2 : 0);

	public int ScaledLevel
	{
		get
		{
			float num = 1f;
			if ((bool)DifficultyScaling.Instance)
			{
				num = DifficultyScaling.Instance.GetScaleMultiplicative(m_ScaledContent, (DifficultyScaling.ScaleData scaleData) => scaleData.CreatureLevelMult);
			}
			return Mathf.Max(1, Mathf.FloorToInt((float)Level * num * DifficultyLevelMult));
		}
	}

	[Persistent]
	public int Experience
	{
		get
		{
			return m_experience;
		}
		set
		{
			int num = value;
			if (GameState.IsLoading && num == 0 && m_experience > 0 && Level > 1)
			{
				num = m_experience;
			}
			m_experience = Math.Min(num, GetMaxExperienceObtainable());
		}
	}

	[Obsolete("Fatigue is no longer a continuous value. See FatigueLevel.")]
	private float m_fatigueAccrued
	{
		set
		{
			float num = WorldTime.Instance.SecondsPerMinute * WorldTime.Instance.MinutesPerHour;
			if (Math.Abs(value - AfflictionData.Instance.CriticalFatigueHours * num) < 0.1f)
			{
				m_CurrentFatigueLevel = FatigueLevel.Critical;
			}
			else if (Math.Abs(value - AfflictionData.Instance.MajorFatigueHours * num) < 0.1f)
			{
				m_CurrentFatigueLevel = FatigueLevel.Major;
			}
			else if (Math.Abs(value - AfflictionData.Instance.MinorFatigueHours * num) < 0.1f)
			{
				m_CurrentFatigueLevel = FatigueLevel.Minor;
			}
			else
			{
				m_CurrentFatigueLevel = FatigueLevel.None;
			}
		}
	}

	public FatigueLevel CurrentFatigueLevel
	{
		get
		{
			return m_CurrentFatigueLevel;
		}
		set
		{
			if (GameUtilities.IsAnimalCompanion(base.gameObject))
			{
				ClearEffectFromAffliction("Fatigue");
				m_CurrentFatigueLevel = FatigueLevel.None;
				return;
			}
			FatigueLevel currentFatigueLevel = m_CurrentFatigueLevel;
			m_CurrentFatigueLevel = value;
			ClearEffectFromAffliction("Fatigue");
			if (m_CurrentFatigueLevel > FatigueLevel.None)
			{
				ApplyAffliction(AfflictionData.GetFatigueAffliction(m_CurrentFatigueLevel));
			}
			if (m_CurrentFatigueLevel > currentFatigueLevel)
			{
				PlayPartyMemberFatigueSound(GetComponent<PartyMemberAI>());
				if (IsPartyMember)
				{
					TutorialManager.Instance.TriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_GETS_FATIGUED);
					if (currentFatigueLevel == FatigueLevel.None && m_CurrentFatigueLevel > FatigueLevel.None)
					{
						UIHudAlerts.Alert(UIActionBarOnClick.ActionType.Camp);
					}
				}
			}
			if (this.OnFatigueLevelChanged != null)
			{
				this.OnFatigueLevelChanged(m_CurrentFatigueLevel);
			}
		}
	}

	[Persistent]
	public int LastKnownVersion { get; set; }

	public float DifficultyStatBonus
	{
		get
		{
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyPotd && !IsPartyMember)
			{
				return 15f;
			}
			return 0f;
		}
	}

	public float DifficultyHealthStaminaMult
	{
		get
		{
			if (!IsPartyMember && (IsPartyMember || !HasFactionSwapEffect()) && (bool)GameState.Instance)
			{
				if (GameState.Instance.IsDifficultyPotd)
				{
					return 1.25f;
				}
				if (GameState.Instance.IsDifficultyStoryTime)
				{
					return 0.5f;
				}
				return 1f;
			}
			return 1f;
		}
	}

	public float DifficultyLevelMult
	{
		get
		{
			bool flag = IsPartyMember || (!IsPartyMember && HasFactionSwapEffect());
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !flag)
			{
				return 0.75f;
			}
			return 1f;
		}
	}

	public float DifficultyRecoveryTimeMult
	{
		get
		{
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !IsPartyMember)
			{
				return 1.5f;
			}
			return 1f;
		}
	}

	public int DifficultyDisengagementAccuracyBonus
	{
		get
		{
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !IsPartyMember)
			{
				return -25;
			}
			return 0;
		}
	}

	public float DifficultyHitToCritBonusChance
	{
		get
		{
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !IsPartyMember)
			{
				Health component = GetComponent<Health>();
				if ((bool)component)
				{
					if (component.StaminaPercentage < 0.33f)
					{
						return 0.5f;
					}
					if (component.StaminaPercentage < 0.5f)
					{
						return 0.33f;
					}
				}
			}
			return 0f;
		}
	}

	[Persistent]
	private List<StatusEffect> SerializedStatusEffects
	{
		get
		{
			List<StatusEffect> list = new List<StatusEffect>();
			Persistence component = GetComponent<Persistence>();
			if ((bool)component && component.Mobile)
			{
				foreach (StatusEffect statusEffect in m_statusEffects)
				{
					if (statusEffect.Params == null || statusEffect.Params.Persistent)
					{
						list.Add(statusEffect);
					}
				}
				return list;
			}
			return list;
		}
		set
		{
			Persistence component = GetComponent<Persistence>();
			if ((bool)component && component.Mobile)
			{
				m_statusEffects.Clear();
				m_statusEffects.AddRange(value.ToArray());
			}
		}
	}

	[Persistent]
	private List<string> m_serializedTalents
	{
		get
		{
			List<string> list = new List<string>();
			foreach (GenericTalent talent in m_talents)
			{
				if (talent != null)
				{
					list.Add(talent.gameObject.name);
				}
			}
			return list;
		}
		set
		{
			m_talents.Clear();
			foreach (string item in value)
			{
				GameObject gameObject = GameResources.LoadPrefab<GameObject>(item, instantiate: false);
				if ((bool)gameObject)
				{
					m_talents.Add(gameObject.GetComponent<GenericTalent>());
				}
			}
		}
	}

	[Persistent]
	public int LastSelectedSurvivalBonus { get; set; }

	[Persistent]
	public int LastSelectedSurvivalSubBonus { get; set; }

	public bool IsPartyMember { get; set; }

	public bool IsEquipmentLocked => m_EquipmentLock != 0;

	public bool IsImmuneToEngagement
	{
		get
		{
			if (!ImmuneToEngagement)
			{
				return HasStatusEffectOfType(StatusEffect.ModifiedStat.NonEngageable);
			}
			return true;
		}
	}

	public float Health => BaseMaxHealth;

	public float BaseMaxHealth => BaseMaxHealthWithoutStat * StatHealthStaminaMultiplier;

	public float BaseMaxHealthWithoutStat => (MaxHealth + (float)((ScaledLevel - 1) * HealthStaminaPerLevel)) * DifficultyHealthStaminaMult * ClassHealthMultiplier;

	public float StatHealthStaminaMultiplier => GetStatHealthStaminaMultiplier(Constitution);

	public float Stamina => BaseMaxStamina * MaxStaminaMultiplier + StaminaBonus;

	public float BaseMaxStamina => BaseMaxStaminaWithoutStat * StatHealthStaminaMultiplier;

	public float BaseMaxStaminaWithoutStat => (MaxStamina + (float)((ScaledLevel - 1) * HealthStaminaPerLevel)) * DifficultyHealthStaminaMult;

	public float PerceptionDistance => NonStealthPerceptionDistance;

	public float StealthedCharacterSuspicionDistance
	{
		get
		{
			float b = 1f;
			float a = AttackData.GetPerceptionDistance(PerceptionType) / 2f;
			if (m_mover != null)
			{
				b = m_mover.Radius * 2f;
			}
			return Mathf.Max(a, b) * 2f;
		}
	}

	public float NonStealthPerceptionDistance => 11f;

	public float StealthDistance
	{
		get
		{
			float num = 1f;
			if (m_mover != null)
			{
				num = m_mover.Radius * 2f;
			}
			float stealthValue = StealthValue;
			float num2 = GameState.PartyAverageStealthValue();
			float num3;
			if (stealthValue > num2 + 10f)
			{
				num3 = num;
			}
			else if (stealthValue < num2 - 10f)
			{
				num3 = 4f;
			}
			else
			{
				num3 = num2 - stealthValue;
				num3 += 10f;
				num3 /= 20f;
				num3 *= 4f - num;
				num3 += num;
			}
			if (NoiseLevel == NoiseLevelType.Loud)
			{
				num3 *= 2f;
			}
			else if (NoiseLevel == NoiseLevelType.ExtremelyLoud)
			{
				num3 *= 3f;
			}
			return num3;
		}
	}

	public float StealthValue => (float)CalculateSkill(SkillType.Stealth) * 5f;

	public float SecondWindAthleticsBonus => GetSecondWindAthleticsBonus(CalculateSkill(SkillType.Athletics));

	public float TotalStaminaRate
	{
		get
		{
			if (GameState.InCombat)
			{
				return (CombatStaminaRechargeRate + StaminaRechargeBonus) * StaminaRechargeMult;
			}
			return (NormalStaminaRechargeRate + StaminaRechargeBonus) * StaminaRechargeMult;
		}
	}

	public int Resolve => GetAttributeScore(AttributeScoreType.Resolve);

	public int Might => GetAttributeScore(AttributeScoreType.Might);

	public int Dexterity => GetAttributeScore(AttributeScoreType.Dexterity);

	public int Intellect => GetAttributeScore(AttributeScoreType.Intellect);

	public int Constitution => GetAttributeScore(AttributeScoreType.Constitution);

	public int Perception => GetAttributeScore(AttributeScoreType.Perception);

	public IList<GenericAbility> ActiveAbilities => m_abilities;

	public IList<GenericTalent> ActiveTalents => m_talents;

	public int[] SpellCastCount => m_spellCastCount;

	public int[] SpellCastBonus => m_spellCastBonus;

	public float[] DamageThreshhold => m_damageThreshhold;

	public float CriticalHitMultiplier => CritMultiplier + CritHitDamageMultiplierBonus;

	public float StatDamageHealMultiplier => GetStatDamageHealMultiplier(Might);

	public float StatAttackSpeedMultiplier => GetStatAttackSpeedMultiplier(Dexterity);

	public float StatEffectDurationMultiplier => GetStatEffectDurationMultiplier(Intellect);

	public float StatEffectRadiusMultiplier => GetStatEffectRadiusMultiplier(Intellect);

	public float StatRangedAttackDistanceMultiplier => GetStatRangedAttackDistanceMultiplier(Perception);

	public int StatTrapAccuracyBonus => CalculateSkill(SkillType.Mechanics) * 3;

	public int StatBonusAccuracy => GetStatBonusAccuracy(Perception);

	public int DefenseBonusFromLevel => (ScaledLevel - 1) * AttackData.Instance.DefensePerLevel;

	public int AccuracyBonusFromLevel => (ScaledLevel - 1) * AttackData.Instance.AccuracyPerLevel;

	public float[] BonusDamage => m_bonusDamage;

	public float[] BonusDamagePerType => m_bonusDamagePerType;

	public float[] BonusDamagePerRace => m_bonusDamagePerRace;

	public int InventoryMaxSize => 16;

	public int MaxWeaponSets
	{
		get
		{
			if (IEModOptions.AllInventorySlots)
			{
				return 4;
			}
			else
			{
				return 2 + this.BonusWeaponSets;
			}
		}
	}

	public int MaxQuickSlots
	{
		get
		{
			if (IEModOptions.AllInventorySlots)
			{
				return 6;
			}
			else
			{
				return 4 + this.BonusQuickSlots;
			}
		}
	}

	public float Focus
	{
		get
		{
			float result = 0f;
			FocusTrait focusTrait = FindFocusTrait();
			if (focusTrait != null)
			{
				result = focusTrait.Focus;
			}
			return result;
		}
		set
		{
			FocusTrait focusTrait = FindFocusTrait();
			if (focusTrait != null)
			{
				focusTrait.Focus = value;
			}
		}
	}

	public float MaxFocus
	{
		get
		{
			float result = 0f;
			FocusTrait focusTrait = FindFocusTrait();
			if (focusTrait != null)
			{
				result = focusTrait.MaxFocus;
			}
			return result;
		}
	}

	public float MaxFocusBonus
	{
		get
		{
			float result = 0f;
			FocusTrait focusTrait = FindFocusTrait();
			if (focusTrait != null)
			{
				result = focusTrait.MaxFocusBonus;
			}
			return result;
		}
		set
		{
			FocusTrait focusTrait = FindFocusTrait();
			if (focusTrait != null)
			{
				focusTrait.MaxFocusBonus = value;
			}
		}
	}

	public IList<StatusEffect> ActiveStatusEffects => m_statusEffects;

	public uint UniqueStatusEffectID => m_availableStatusEffectID++;

	public NoiseLevelType NoiseLevel
	{
		get
		{
			return m_noiseLevel;
		}
		set
		{
			m_noiseLevel = value;
			m_noiseTimer = 1f;
		}
	}

	public float NoiseLevelRadius => NoiseLevel switch
	{
		NoiseLevelType.ExtremelyLoud => 7f, 
		NoiseLevelType.Loud => 4f, 
		_ => 0f, 
	};

	public uint LoreReveal
	{
		get
		{
			return m_loreReveal;
		}
		set
		{
			m_loreReveal = value;
		}
	}

	public float ChantRadius => AttackData.Instance.ChanterPhraseRadius * ChanterPhraseRadiusMult * StatEffectRadiusMultiplier * AoERadiusMult;

	public bool CanUseAbilities => !HasStatusEffectOfType(StatusEffect.ModifiedStat.DisableAbilityUse);

	public bool CanCastSpells
	{
		get
		{
			if (!EffectDisablesSpellcasting)
			{
				return CurrentGrimoireCooldown <= 0f;
			}
			return false;
		}
	}

	public bool EffectDisablesSpellcasting => HasStatusEffectOfType(StatusEffect.ModifiedStat.DisableSpellcasting);

	public bool RedirectMeleeAttacks
	{
		get
		{
			return m_redirectMeleeAttacks > 0;
		}
		set
		{
			if (value)
			{
				m_redirectMeleeAttacks++;
			}
			else
			{
				m_redirectMeleeAttacks--;
			}
		}
	}

	public float RecoveryTimer
	{
		get
		{
			return m_recoveryTimer;
		}
		set
		{
			m_recoveryTimer = value;
			m_totalRecoveryTime = value;
		}
	}

	public float TotalRecoveryTime => m_totalRecoveryTime;

	public float IdleTimer
	{
		get
		{
			return m_idleTimer;
		}
		set
		{
			m_idleTimer = value;
		}
	}

	public float GetAttackSpeedMultiplier => AttackSpeedMultiplier;

	private bool IsMoving
	{
		get
		{
			if (m_mover != null && m_mover.HasGoal)
			{
				return true;
			}
			return false;
		}
	}

	public float CritThreshhold
	{
		get
		{
			if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !IsPartyMember)
			{
				return AttackData.Instance.StoryTimeMinimumRollToCrit;
			}
			return AttackData.Instance.MinimumRollToCrit;
		}
	}

	public float MinimumRollToGraze => AttackData.Instance.MinimumRollToGraze;

	public static float CritMultiplier => AttackData.Instance.CritDamageMult;

	public static float GrazeMultiplier => AttackData.Instance.GrazeDamageMult;

	public int InvisibilityState
	{
		get
		{
			return m_invisState;
		}
		set
		{
			if (value < 0)
			{
				return;
			}
			if (m_invisState == 0)
			{
				if (value > 0)
				{
					SetInvisible(isInvis: true);
				}
			}
			else if (value == 0)
			{
				SetInvisible(isInvis: false);
			}
			m_invisState = value;
		}
	}

	public bool IsInvisible => m_invisState != 0;

	public bool ImmuneToParticleEffects { get; set; }

	public event CombatEventHandler OnPreApply;

	public event CombatEventHandler OnAttackLaunch;

	public event CombatEventHandler OnAttackHitFrame;

	public event CombatEventHandler OnAttackHits;

	public event CombatEventHandler OnBeamHits;

	public event CombatEventHandler OnPreDamageDealt;

	public event CombatEventHandler OnAddDamage;

	public event CombatEventHandler OnAttackRollCalculated;

	public event CombatEventHandler OnAdjustCritGrazeMiss;

	public event CombatEventHandler OnEffectApply;

	public event CombatEventHandler OnPostDamageDealt;

	public event CombatEventHandler OnDamageFinal;

	public event CombatEventHandler OnPreDamageApplied;

	public event CombatEventHandler OnApplyDamageThreshhold;

	public event CombatEventHandler OnApplyProcs;

	public event CombatEventHandler OnPostDamageApplied;

	public event CombatEventHandler OnDeactivate;

	public event StatusEffectBoolEvent OnAddStatusEffect;

	public event StatusEffectEvent OnClearStatusEffect;

	public event AfflictionEvent OnCausedAffliction;

	public event Action<GameObject, bool> OnGriefStateChanged;

	public event DefenseEvent OnDefenseAdjustment;

	public event CheckImmunity OnCheckImmunity;

	public event EventHandler OnEngagement;

	public event EventHandler OnEngagedByOther;

	public event EventHandler OnEngagementBreak;

	public event EventHandler OnEngagementByOtherBroken;

	public event GameObjectEvent OnBeginFlanking;

	public event GameObjectEvent OnEndFlanking;

	public event EventHandler OnLevelUp;

	public event EventHandler OnResting;

	public static event EventHandler s_OnCharacterStatsStart;

	public event FatigueLevelChanged OnFatigueLevelChanged;

	public ScaledContent AddScaledContentComponent()
	{
		if (!m_ScaledContent)
		{
			m_ScaledContent = base.gameObject.AddComponent<ScaledContent>();
		}
		return m_ScaledContent;
	}

	public void SetInflictedGenericMarker(string tag)
	{
		m_MarkersAppliedThisCombat.Add(tag);
	}

	public bool GetInflictedGenericMarker(string tag)
	{
		return m_MarkersAppliedThisCombat.Contains(tag);
	}

	public float GetDifficultyDamageMultiplier(Health target)
	{
		if ((bool)GameState.Instance && GameState.Instance.IsDifficultyStoryTime && !IsPartyMember)
		{
			if (!target)
			{
				return 1f;
			}
			float staminaPercentage = target.StaminaPercentage;
			if (staminaPercentage < 0.33f)
			{
				return 0.5f;
			}
			if (staminaPercentage < 0.5f)
			{
				return 0.66f;
			}
			return 0.75f;
		}
		return 1f;
	}

	public void BindAbilitiesChanged(ListChangedEventHandler method)
	{
		m_abilities.ListChanged += method;
	}

	public void UnbindAbilitiesChanged(ListChangedEventHandler method)
	{
		m_abilities.ListChanged -= method;
	}

	public void NotifyCausedAffliction(GameObject victim, Affliction afflictionPrefab)
	{
		if (this.OnCausedAffliction != null)
		{
			this.OnCausedAffliction(victim, afflictionPrefab);
		}
	}

	public void NotifyGriefStateChanged(GameObject obj, bool state)
	{
		if (this.OnGriefStateChanged != null)
		{
			this.OnGriefStateChanged(obj, state);
		}
	}

	private int GetSumOfStatusEffectsOfTypeFromAllAbilities(StatusEffect.ModifiedStat stat)
	{
		if (stat == StatusEffect.ModifiedStat.NoEffect)
		{
			return 0;
		}
		float num = 0f;
		foreach (GenericAbility ability in Abilities)
		{
			if (!(ability != null))
			{
				continue;
			}
			StatusEffectParams[] statusEffects = ability.StatusEffects;
			foreach (StatusEffectParams statusEffectParams in statusEffects)
			{
				if (statusEffectParams != null && statusEffectParams.AffectsStat == stat)
				{
					num += statusEffectParams.GetValue(this);
				}
			}
		}
		return (int)num;
	}

	public object GetPropertyByIndex(IndexableStat stat)
	{
		Equipment component = GetComponent<Equipment>();
		switch (stat)
		{
		case IndexableStat.NAME:
			return Name();
		case IndexableStat.LEVEL:
			return ScaledLevel;
		case IndexableStat.HEALTH:
			return Health.ToString("#0");
		case IndexableStat.ACCURACY:
			return CalculateAccuracy(null, null) + GetSumOfStatusEffectsOfTypeFromAllAbilities(StatusEffect.ModifiedStat.Accuracy);
		case IndexableStat.MIGHT:
			return Might;
		case IndexableStat.DEXTERITY:
			return Dexterity;
		case IndexableStat.RESOLVE:
			return Resolve;
		case IndexableStat.INTELLECT:
			return Intellect;
		case IndexableStat.CONSTITUTION:
			return Constitution;
		case IndexableStat.PERCEPTION:
			return Perception;
		case IndexableStat.DEFLECTION:
			return GetDefenseString(DefenseType.Deflect);
		case IndexableStat.FORTITUDE:
			return GetDefenseString(DefenseType.Fortitude);
		case IndexableStat.REFLEXES:
			return GetDefenseString(DefenseType.Reflex);
		case IndexableStat.WILL:
			return GetDefenseString(DefenseType.Will);
		case IndexableStat.DT:
		{
			Equippable itemInSlot4 = component.DefaultEquippedItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if ((bool)itemInSlot4)
			{
				return itemInSlot4.GetComponent<Armor>().CalculateDT(DamagePacket.DamageType.All, 0f, base.gameObject) + (float)GetSumOfStatusEffectsOfTypeFromAllAbilities(StatusEffect.ModifiedStat.DamageThreshhold);
			}
			return 0;
		}
		case IndexableStat.SPECIAL_DTS:
		{
			Equippable itemInSlot3 = component.DefaultEquippedItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if ((bool)itemInSlot3)
			{
				Armor component3 = itemInSlot3.GetComponent<Armor>();
				float num4 = component3.CalculateDT(DamagePacket.DamageType.All, 0f, base.gameObject);
				List<string> list2 = new List<string>();
				for (int j = 0; j < 7; j++)
				{
					float num5 = component3.CalculateDT((DamagePacket.DamageType)j, 0f, base.gameObject);
					if (float.IsPositiveInfinity(num5))
					{
						list2.Add(GUIUtils.GetDamageTypeString((DamagePacket.DamageType)j) + ": " + GUIUtils.GetText(2187));
					}
					else if (num5 != num4)
					{
						list2.Add(GUIUtils.GetDamageTypeString((DamagePacket.DamageType)j) + ": " + num5.ToString("#0"));
					}
				}
				return string.Join(GUIUtils.Comma(), list2.ToArray());
			}
			return "";
		}
		case IndexableStat.DR:
		{
			Equippable itemInSlot2 = component.DefaultEquippedItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if ((bool)itemInSlot2)
			{
				float num3 = itemInSlot2.GetComponent<Armor>().CalculateDR(DamagePacket.DamageType.All);
				return GUIUtils.Format(1277, num3.ToString("#0"));
			}
			return GUIUtils.Format(1277, 0);
		}
		case IndexableStat.SPECIAL_DRS:
		{
			Equippable itemInSlot = component.DefaultEquippedItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if ((bool)itemInSlot)
			{
				Armor component2 = itemInSlot.GetComponent<Armor>();
				float num = component2.CalculateDR(DamagePacket.DamageType.All);
				List<string> list = new List<string>();
				for (int i = 0; i < 7; i++)
				{
					float num2 = component2.CalculateDR((DamagePacket.DamageType)i);
					if (num2 != num)
					{
						list.Add(GUIUtils.GetDamageTypeString((DamagePacket.DamageType)i) + ": " + GUIUtils.Format(1277, num2.ToString("#0")));
					}
				}
				return string.Join(", ", list.ToArray());
			}
			return "";
		}
		case IndexableStat.STAMINA:
			return Stamina.ToString("#0");
		case IndexableStat.PRIMARYDAMAGE:
			throw new NotImplementedException();
		case IndexableStat.SECONDARYDAMAGE:
			throw new NotImplementedException();
		case IndexableStat.ABILITIES:
			throw new NotImplementedException();
		case IndexableStat.RACE:
			return CharacterRace;
		default:
			return null;
		}
	}

	public string GetDefenseString(DefenseType defense)
	{
		bool num = CalculateIsImmune(defense, null, null, isSecondary: false);
		int num2 = CalculateDefense(defense, null, null, isSecondary: false, allowRedirect: false) + GetSumOfStatusEffectsOfTypeFromAllAbilities(StatusEffect.DefenseTypeToModifiedStat(defense));
		if (num)
		{
			return GUIUtils.GetText(2187);
		}
		return num2.ToString();
	}

	public void LockEquipment()
	{
		m_EquipmentLock++;
	}

	public void UnlockEquipment()
	{
		if (m_EquipmentLock != 0)
		{
			m_EquipmentLock--;
		}
	}

	public static IndexableStat DefenseTypeAsStat(DefenseType type)
	{
		return (IndexableStat)(type + 12);
	}

	public static bool IsPlayableClass(Class characterClass)
	{
		if ((uint)(characterClass - 1) <= 10u)
		{
			return true;
		}
		return false;
	}

	public static bool IsKithRace(Race characterRace)
	{
		if ((uint)(characterRace - 1) <= 4u || characterRace == Race.Aumaua)
		{
			return true;
		}
		return false;
	}

	public CoreData GetCopyOfCoreData()
	{
		CoreData coreData = new CoreData();
		coreData.Gender = Gender;
		coreData.Level = Level;
		coreData.Class = CharacterClass;
		coreData.Race = CharacterRace;
		coreData.Subrace = CharacterSubrace;
		coreData.RacialBodyType = RacialBodyType;
		coreData.Culture = CharacterCulture;
		coreData.Background = CharacterBackground;
		coreData.Deity = Deity;
		coreData.PaladinOrder = PaladinOrder;
		coreData.IsPlayerCharacter = GetComponent<Player>() != null;
		coreData.IsHiredAdventurer = !coreData.IsPlayerCharacter && GetComponent<CompanionInstanceID>() == null;
		coreData.BaseStats[2] = BaseDexterity;
		coreData.BaseStats[1] = BaseMight;
		coreData.BaseStats[0] = BaseResolve;
		coreData.BaseStats[3] = BaseIntellect;
		coreData.BaseStats[5] = BasePerception;
		coreData.BaseStats[4] = BaseConstitution;
		coreData.SkillValues[1] = AthleticsSkill;
		coreData.SkillValues[5] = CraftingSkill;
		coreData.SkillValues[2] = LoreSkill;
		coreData.SkillValues[3] = MechanicsSkill;
		coreData.SkillValues[0] = StealthSkill;
		coreData.SkillValues[4] = SurvivalSkill;
		coreData.KnownSkills = new List<MonoBehaviour>();
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			coreData.KnownSkills.Add(activeAbility);
		}
		foreach (GenericTalent activeTalent in ActiveTalents)
		{
			coreData.KnownSkills.Add(activeTalent);
		}
		ChanterTrait chanterTrait = GetChanterTrait();
		if (chanterTrait != null)
		{
			Phrase[] knownPhrases = chanterTrait.GetKnownPhrases();
			foreach (Phrase item in knownPhrases)
			{
				coreData.KnownSkills.Add(item);
			}
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(base.gameObject);
		if ((bool)gameObject)
		{
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			foreach (GenericAbility activeAbility2 in component.ActiveAbilities)
			{
				coreData.KnownSkills.Add(activeAbility2);
			}
			foreach (GenericTalent activeTalent2 in component.ActiveTalents)
			{
				coreData.KnownSkills.Add(activeTalent2);
			}
			ChanterTrait chanterTrait2 = component.GetChanterTrait();
			if (chanterTrait2 != null)
			{
				Phrase[] knownPhrases = chanterTrait2.GetKnownPhrases();
				foreach (Phrase item2 in knownPhrases)
				{
					coreData.KnownSkills.Add(item2);
				}
			}
		}
		return coreData;
	}

	private void Awake()
	{
		m_ScaledContent = GetComponent<ScaledContent>();
		if (CharacterRace != Race.Godlike)
		{
			RacialBodyType = CharacterRace;
		}
		else if (RacialBodyType == Race.Undefined)
		{
			RacialBodyType = Race.Human;
		}
	}

	private void OnEnable()
	{
		ConversationManager.AddObjectToActiveSpeakerGuidCache(new Guid(DisplayName.CharacterGuid), base.gameObject);
	}

	private void Start()
	{
		object component = this;
		if (!GameState.LoadedGame || (GameState.LoadedGame && !GameState.IsLoading) || !GetComponent<PartyMemberAI>())
		{
			DataManager.AdjustFromData(ref component);
		}
		m_keywords = new KeywordCollection(Keywords);
		if (!GameState.LoadedGame)
		{
			for (GenericAbility.ActivationGroup activationGroup = GenericAbility.ActivationGroup.None; activationGroup < GenericAbility.ActivationGroup.Count; activationGroup++)
			{
				m_modalCooldownTimer[(int)activationGroup] = 0f;
			}
		}
		m_bestiaryReference = GetComponent<BestiaryReference>();
		m_stronghold = GameState.Stronghold;
		m_mover = GetComponent<Mover>();
		m_equipment = GetComponent<Equipment>();
		if (!m_bestiaryReference)
		{
			m_stackTracker = new Dictionary<int, Dictionary<int, StatusEffect>>(5);
		}
		if (!m_bestiaryReference)
		{
			Persistence component2 = GetComponent<Persistence>();
			if ((!GameState.LoadedGame && !GameState.IsRestoredLevel) || (component2 != null && !component2.Mobile))
			{
				AddPresetAbilities();
				AddRacialAbilities();
				AddPresetTalents();
			}
		}
		m_NotifiedLevel = Level;
		if (Level > 1 && Experience == 0)
		{
			Experience = ExperienceNeededForLevel(Level);
		}
		if (!m_bestiaryReference)
		{
			GameState.OnCombatEnd += HandleGameUtilitiesOnCombatEnd;
			WorldTime.Instance.OnTimeJump += HandleGameOnTimeJump;
			Health component3 = GetComponent<Health>();
			if ((bool)component3)
			{
				component3.OnDeath += HandleOnDeath;
			}
			if ((bool)m_equipment)
			{
				m_equipment.OnEquipmentChanged += HandleOnEquipmentChanged;
			}
		}
		if (OverrideSurvivalSkillLevel != 0)
		{
			SurvivalSkill = GetPointsForSkillLevel(OverrideSurvivalSkillLevel);
		}
		if (!m_bestiaryReference)
		{
			HandleAttackMeleeAttachment(removeExisting: false);
		}
		GameResources.OnPreSaveGame += OnPreSaveGame;
		if (CharacterStats.s_OnCharacterStatsStart != null)
		{
			CharacterStats.s_OnCharacterStatsStart(this, null);
		}
	}

	private void HandleOnEquipmentChanged(Equippable.EquipmentSlot slot, Equippable oldEq, Equippable newEq, bool swappingSummonedWeapon, bool enforceRecoveryPenalty)
	{
		if (swappingSummonedWeapon || (slot != Equippable.EquipmentSlot.PrimaryWeapon && slot != Equippable.EquipmentSlot.SecondaryWeapon) || !(oldEq != newEq))
		{
			return;
		}
		AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
		if (aIController != null)
		{
			aIController.CancelAllEngagements();
		}
		if (!enforceRecoveryPenalty)
		{
			return;
		}
		float num = Mathf.Max(0f, AttackData.Instance.SwitchWeaponRecoveryTime + WeaponSwitchCooldownBonus);
		if (m_weaponSwitchingTimer <= 0f)
		{
			m_weaponSwitchingTimer = num;
		}
		else
		{
			float weaponSwitchingTimer = num;
			num -= m_weaponSwitchingTimer;
			m_weaponSwitchingTimer = weaponSwitchingTimer;
		}
		num *= DifficultyRecoveryTimeMult;
		if (RecoveryTimer > 0f)
		{
			m_recoveryTimer += num;
			if (m_recoveryTimer > m_totalRecoveryTime)
			{
				m_totalRecoveryTime += num;
			}
		}
		else
		{
			RecoveryTimer = num;
		}
	}

	public void HandleInterruptRecovery(AttackData.InterruptScale interruptScale)
	{
		if (!GameState.InCombat)
		{
			return;
		}
		float num = AttackData.InterruptDuration(interruptScale);
		if (m_interruptTimer <= 0f)
		{
			m_interruptTimer = num;
		}
		else
		{
			float interruptTimer = num;
			num -= m_interruptTimer;
			m_interruptTimer = interruptTimer;
		}
		num *= DifficultyRecoveryTimeMult;
		if (RecoveryTimer > 0f)
		{
			m_recoveryTimer += num;
			if (m_recoveryTimer > m_totalRecoveryTime)
			{
				m_totalRecoveryTime += num;
			}
		}
		else
		{
			RecoveryTimer = num;
		}
		AIController[] components = GetComponents<AIController>();
		foreach (AIController aIController in components)
		{
			if (aIController.enabled)
			{
				aIController.CancelAllEngagementsAndDelayReengagement();
			}
		}
	}

	public void HandleAttackMeleeAttachment(bool removeExisting)
	{
		if (CharacterRace == Race.Undefined)
		{
			return;
		}
		AIController component = GetComponent<AIController>();
		if ((bool)component && component.IsPet)
		{
			return;
		}
		AttackMelee[] components = GetComponents<AttackMelee>();
		AttackMelee attackMelee = null;
		int num = components.Length;
		if (components.Length != 0)
		{
			attackMelee = components[0];
		}
		if (removeExisting)
		{
			for (int i = 0; i < components.Length; i++)
			{
				GameUtilities.Destroy(components[i]);
			}
			attackMelee = null;
			num = 0;
		}
		if (num < 2)
		{
			GameObject gameObject = ((attackMelee != null) ? attackMelee.gameObject : ((CharacterClass != Class.Monk) ? (Resources.Load("Prefabs/DefaultAttacks/Default_Unarmed_Attack") as GameObject) : (Resources.Load("Prefabs/DefaultAttacks/Default_Unarmed_Attack_Monk") as GameObject)));
			AttackMelee component2 = gameObject.GetComponent<AttackMelee>();
			for (int j = 0; j < 2 - num; j++)
			{
				ComponentUtils.CopyComponent((AttackBase)component2, base.gameObject);
			}
		}
	}

	private static int SortAbilities(GenericAbility a1, GenericAbility a2)
	{
		int num = a1.name.CompareTo(a2.name);
		if (num == 0)
		{
			return a1.OverrideName.CompareTo(a2.OverrideName);
		}
		return num;
	}

	public void Restored()
	{
		if (!GameState.LoadedGame && !GameState.IsRestoredLevel)
		{
			return;
		}
		if ((bool)GetComponent<CompanionInstanceID>())
		{
			Persistence component = GetComponent<Persistence>();
			if ((bool)component)
			{
				GameObject gameObject = GameResources.LoadPrefab(component.Prefab, instantiate: false) as GameObject;
				CharacterStats characterStats = (gameObject ? gameObject.GetComponent<CharacterStats>() : null);
				if ((bool)characterStats)
				{
					BaseMight = characterStats.BaseMight;
					BaseConstitution = characterStats.BaseConstitution;
					BaseDexterity = characterStats.BaseDexterity;
					BaseResolve = characterStats.BaseResolve;
					BaseIntellect = characterStats.BaseIntellect;
					BasePerception = characterStats.BasePerception;
				}
			}
		}
		HandleAttackMeleeAttachment(removeExisting: false);
		List<GenericAbility> list = new List<GenericAbility>(base.gameObject.GetComponentsInChildren<GenericAbility>());
		if (list.Count == 0 && !base.gameObject.activeInHierarchy)
		{
			list = new List<GenericAbility>(base.gameObject.GetComponentsInChildren<GenericAbility>(includeInactive: true));
		}
		list.Sort(SortAbilities);
		m_abilities.Clear();
		m_abilities.AddRange(list);
		for (int num = m_abilities.Count - 1; num >= 0; num--)
		{
			if (m_abilities[num] == null)
			{
				m_abilities.RemoveAt(num);
			}
			else if ((bool)m_abilities[num].GetComponent<Persistence>() && m_abilities[num].GetComponent<Persistence>().CreateFromPrefabFailed)
			{
				PersistenceManager.RemoveObject(m_abilities[num].GetComponent<Persistence>());
				GameUtilities.DestroyImmediate(m_abilities[num].gameObject);
				m_abilities.RemoveAt(num);
			}
			else if ((bool)m_abilities[num].GetComponent<Item>() || m_abilities[num].AppliedViaMod)
			{
				m_abilities[num].Restored();
				m_abilities.RemoveAt(num);
			}
			else if (m_abilities[num] is GenericSpell && ((GenericSpell)m_abilities[num]).IsFree)
			{
				m_abilities[num].Restored();
				m_abilities.RemoveAt(num);
			}
			else
			{
				m_abilities[num].Restored();
				if (m_abilities[num] is Spiritshift)
				{
					(m_abilities[num] as Spiritshift).RestoreTempAbilities();
				}
				if (m_abilities[num].DisplayName.StringID == 317 && m_abilities[num] is ZealousAura)
				{
					(m_abilities[num] as ZealousAura).RestoreFixUp();
					m_updateTracker = true;
				}
				if (m_abilities[num].DisplayName.StringID == 30)
				{
					m_abilities[num].FixUpDefender();
				}
			}
		}
		if ((bool)GetComponent<PartyMemberAI>() && LastKnownVersion != ProductConfiguration.Version)
		{
			AddNewAbilities();
		}
		AddSecondWind();
		foreach (GenericAbility ability in m_abilities)
		{
			if (ability.Passive && !ability.Activated)
			{
				ability.Activate();
				ability.UpdateStatusEffectActivation();
			}
		}
		Persistence component2 = GetComponent<Persistence>();
		if ((bool)component2 && component2.Mobile)
		{
			foreach (StatusEffect activeStatusEffect in ActiveStatusEffects)
			{
				activeStatusEffect.Restored();
			}
		}
		ScaledContent component3 = GetComponent<ScaledContent>();
		if ((bool)component2 && !component2.Mobile && (bool)component3)
		{
			int level = Level;
			Level = GetLevelBasedOnExperience();
			if (Level == 1)
			{
				Level = level;
			}
			else if (level != Level)
			{
				float scaleMultiplicative = DifficultyScaling.Instance.GetScaleMultiplicative(component3.Scalers, (DifficultyScaling.ScaleData scaleData) => scaleData.CreatureLevelMult);
				Level = Mathf.CeilToInt((float)Level / scaleMultiplicative);
				Experience = ExperienceNeededForLevel(Level);
			}
		}
		for (int num2 = m_statusEffects.Count - 1; num2 >= 0; num2--)
		{
			StatusEffect statusEffect = m_statusEffects[num2];
			if (statusEffect.IsFromAura)
			{
				statusEffect.ClearEffect(base.gameObject);
				m_statusEffects.RemoveAt(num2);
				continue;
			}
			if ((bool)statusEffect.AbilityOrigin && statusEffect.AbilityOrigin.DisplayName.StringID == 146)
			{
				statusEffect.Params.LastsUntilCombatEnds = true;
			}
			if (statusEffect.LastsUntilCombatEnds || (statusEffect.AbilityOrigin != null && statusEffect.AbilityOrigin.CombatOnly))
			{
				if ((bool)UIDebug.Instance)
				{
					UIDebug.Instance.LogOnScreenWarning("Combat Only status effect being restored from save game! Object = " + statusEffect.GetDisplayName(), UIDebug.Department.Programming, 10f);
				}
				statusEffect.ClearEffect(base.gameObject);
				m_statusEffects.RemoveAt(num2);
				continue;
			}
			if ((bool)statusEffect.AbilityOrigin && statusEffect.AbilityOrigin.Activated && statusEffect.AbilityOrigin.DisplayName.StringID == 30)
			{
				if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.Deflection && statusEffect.Params.Value == 5f)
				{
					statusEffect.Params.Value = -5f;
				}
				else if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.AttackSpeed)
				{
					statusEffect.AbilityOrigin.RemoveStatusEffect(statusEffect.Params, GenericAbility.AbilityType.Ability);
					statusEffect.ClearEffect(base.gameObject);
					m_statusEffects.RemoveAt(num2);
					continue;
				}
			}
			if (statusEffect.Duration == 0f && statusEffect.Interval == 0f && StatusEffect.StatNotRevokedParam(statusEffect.Params.AffectsStat) && statusEffect.Params.Apply == StatusEffect.ApplyType.ApplyOnTick && statusEffect.Applied)
			{
				statusEffect.ClearEffect(base.gameObject);
				m_statusEffects.RemoveAt(num2);
			}
		}
		if (!HasStatusEffectOfType(StatusEffect.ModifiedStat.SuspendHostileEffects) && !HasStatusEffectOfType(StatusEffect.ModifiedStat.SuspendBeneficialEffects))
		{
			ClearAllSuspensions();
		}
		foreach (GenericTalent talent in m_talents)
		{
			if (!(talent != null) || talent.Abilities == null || talent.Type != GenericTalent.TalentType.ModExistingAbility || talent.AbilityMods == null)
			{
				continue;
			}
			GenericAbility[] abilities = talent.Abilities;
			foreach (GenericAbility a in abilities)
			{
				foreach (GenericAbility activeAbility in ActiveAbilities)
				{
					if (GenericAbility.NameComparer.Instance.Equals(a, activeAbility))
					{
						AbilityMod[] abilityMods = talent.AbilityMods;
						foreach (AbilityMod mod in abilityMods)
						{
							activeAbility.AddAbilityMod(mod, GenericAbility.AbilityType.Talent);
						}
					}
				}
			}
		}
		object component4 = this;
		DataManager.AdjustFromData(ref component4);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		float num15 = 1f;
		float num16 = 1f;
		float num17 = 0f;
		float num18 = 1f;
		float num19 = 1f;
		float num20 = 1f;
		AfflictionImmunities.Clear();
		int num21 = 0;
		int num22 = 0;
		float num23 = 0f;
		float num24 = 1f;
		float num25 = 1f;
		float num26 = 0f;
		int num27 = 0;
		int num28 = 1;
		float num29 = 0f;
		float num30 = 1f;
		float num31 = 0f;
		int num32 = 0;
		float num33 = 0f;
		float num34 = 0f;
		float num35 = 0f;
		float num36 = 0f;
		float num37 = 0f;
		int num38 = 2;
		float num39 = 1f;
		int num40 = 0;
		for (int k = 0; k < BonusDamagePerType.Length; k++)
		{
			BonusDamagePerType[k] = 0f;
		}
		for (int l = 0; l < BonusDamagePerRace.Length; l++)
		{
			BonusDamagePerRace[l] = 0f;
		}
		int[] array = new int[8];
		Array.Clear(array, 0, SpellCastBonus.Length);
		for (int num41 = m_statusEffects.Count - 1; num41 >= 0; num41--)
		{
			StatusEffect statusEffect2 = m_statusEffects[num41];
			if (statusEffect2 != null && statusEffect2.Applied)
			{
				switch (statusEffect2.Params.AffectsStat)
				{
				case StatusEffect.ModifiedStat.SpellCastBonus:
				{
					int num42 = (int)statusEffect2.ParamsExtraValue() - 1;
					if (num42 >= 0 && num42 < 8)
					{
						array[num42] += (int)statusEffect2.CurrentAppliedValue;
					}
					break;
				}
				case StatusEffect.ModifiedStat.MaxHealth:
					if (!GetComponent<Health>().Dead)
					{
						MaxHealth += (int)statusEffect2.CurrentAppliedValue;
					}
					break;
				case StatusEffect.ModifiedStat.Might:
					num3 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Resolve:
					num4 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Dexterity:
					num5 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Intellect:
					num6 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Perception:
					num7 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Constitution:
					num8 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.FocusGainMult:
					num15 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.AoEMult:
					num16 *= Mathf.Sqrt(statusEffect2.CurrentAppliedValue);
					break;
				case StatusEffect.ModifiedStat.Athletics:
					num10 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Lore:
					num12 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Mechanics:
					num13 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Survival:
					num11 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.Stealth:
					num9 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.MovementRate:
					num17 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.ProneDurationMult:
					num18 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.MaxFocus:
					num14 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.MaxStaminaMult:
					num19 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.AttackSpeed:
					num20 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.AddAfflictionImmunity:
					AfflictionImmunities.Add(statusEffect2.Params.AfflictionPrefab);
					break;
				case StatusEffect.ModifiedStat.RangedDeflection:
					num21 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.ExtraStraightBounces:
					num22 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusHitToCritPercent:
					num23 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.DualWieldAttackSpeedPercent:
					num24 += statusEffect2.CurrentAppliedValue / 100f;
					break;
				case StatusEffect.ModifiedStat.BonusRangedWeaponCloseEnemyDamageMult:
					num25 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusDamageByTypePercent:
					if ((int)statusEffect2.Params.DmgType < BonusDamagePerType.Length)
					{
						BonusDamagePerType[(int)statusEffect2.Params.DmgType] += statusEffect2.CurrentAppliedValue;
					}
					break;
				case StatusEffect.ModifiedStat.EnemyReflexGrazeToMissPercent:
					num26 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusShieldDeflection:
					num27 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.EngagedEnemyCount:
					num28 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusGrazeToHitPercent:
					num29 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.WeapMinDamageMult:
					num30 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.ArmorSpeedFactorAdj:
					num31 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.DisengagementDefense:
					num32 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusGrazeToHitRatioMeleeOneHand:
					num33 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusHitToCritPercentEnemyBelow10Percent:
					num34 += statusEffect2.CurrentAppliedValue / 100f;
					break;
				case StatusEffect.ModifiedStat.BonusCritHitMultiplierEnemyBelow10Percent:
					num35 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.EnemyCritToHitPercent:
					num36 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.EnemyHitToGrazePercent:
					num37 += statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.EnemiesNeededToFlankAdj:
					num38 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.ReloadSpeed:
					num39 *= statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusAccuracyForNearestAllyOnSameEnemy:
					num40 += (int)statusEffect2.CurrentAppliedValue;
					break;
				case StatusEffect.ModifiedStat.BonusDamageByRacePercent:
					if ((int)statusEffect2.Params.RaceType < BonusDamagePerRace.Length)
					{
						BonusDamagePerRace[(int)statusEffect2.Params.RaceType] += statusEffect2.CurrentAppliedValue;
					}
					break;
				}
			}
		}
		if (m_spellCastBonus.Length < 8)
		{
			Array.Resize(ref m_spellCastBonus, 8);
		}
		if (m_spellCastCount.Length < 8)
		{
			Array.Resize(ref m_spellCastCount, 8);
		}
		for (int m = 0; m < array.Length && m < SpellCastBonus.Length; m++)
		{
			if (array[m] != SpellCastBonus[m])
			{
				SpellCastBonus[m] = array[m];
			}
		}
		if (num3 != MightBonus)
		{
			MightBonus = num3;
		}
		if (num4 != ResolveBonus)
		{
			ResolveBonus = num4;
		}
		if (num5 != DexterityBonus)
		{
			DexterityBonus = num5;
		}
		if (num6 != IntellectBonus)
		{
			IntellectBonus = num6;
		}
		if (num7 != PerceptionBonus)
		{
			PerceptionBonus = num7;
		}
		if (num8 != ConstitutionBonus)
		{
			ConstitutionBonus = num8;
		}
		if (num9 != StealthBonus)
		{
			StealthBonus = num9;
		}
		if (num10 != AthleticsBonus)
		{
			AthleticsBonus = num10;
		}
		if (num11 != SurvivalBonus)
		{
			SurvivalBonus = num11;
		}
		if (num12 != LoreBonus)
		{
			LoreBonus = num12;
		}
		if (num13 != MechanicsBonus)
		{
			MechanicsBonus = num13;
		}
		if (num15 != FocusGainMult)
		{
			FocusGainMult = num15;
		}
		if (num16 != AoERadiusMult)
		{
			AoERadiusMult = num16;
		}
		if ((float)num14 != MaxFocusBonus)
		{
			MaxFocusBonus = num14;
		}
		if (num19 != MaxStaminaMultiplier)
		{
			MaxStaminaMultiplier = num19;
		}
		if (num20 != AttackSpeedMultiplier)
		{
			AttackSpeedMultiplier = num20;
		}
		if (num21 != RangedDeflectionBonus)
		{
			RangedDeflectionBonus = num21;
		}
		if (num22 != ExtraStraightBounces)
		{
			ExtraStraightBounces = num22;
		}
		if (num23 != BonusHitToCritPercent)
		{
			BonusHitToCritPercent = num23;
		}
		if (num24 != DualWieldAttackSpeedMultiplier)
		{
			DualWieldAttackSpeedMultiplier = num24;
		}
		if (num25 != BonusRangedWeaponCloseEnemyDamageMult)
		{
			BonusRangedWeaponCloseEnemyDamageMult = num25;
		}
		if (num26 != EnemyReflexGrazeToMissPercent)
		{
			EnemyReflexGrazeToMissPercent = num26;
		}
		if (num27 != BonusShieldDeflection)
		{
			BonusShieldDeflection = num27;
		}
		if (num28 != EngageableEnemyCount)
		{
			EngageableEnemyCount = num28;
		}
		if (num29 != BonusGrazeToHitPercent)
		{
			BonusGrazeToHitPercent = num29;
		}
		if (num30 != WeaponDamageMinMult)
		{
			WeaponDamageMinMult = num30;
		}
		if (num31 != ArmorSpeedFactorAdj)
		{
			ArmorSpeedFactorAdj = num31;
		}
		if (num32 != DisengagementDefenseBonus)
		{
			DisengagementDefenseBonus = num32;
		}
		if (num33 != BonusGrazeToHitPercentMeleeOneHanded)
		{
			BonusGrazeToHitPercentMeleeOneHanded = num33;
		}
		if (num34 != BonusHitToCritPercentEnemyBelow10Percent)
		{
			BonusHitToCritPercentEnemyBelow10Percent = num34;
		}
		if (num35 != CritHitDamageMultiplierBonusEnemyBelow10Percent)
		{
			CritHitDamageMultiplierBonusEnemyBelow10Percent = num35;
		}
		if (num36 != EnemyCritToHitPercent)
		{
			EnemyCritToHitPercent = num36;
		}
		if (num37 != EnemyHitToGrazePercent)
		{
			EnemyHitToGrazePercent = num37;
		}
		if (num38 != EnemiesNeededToFlank)
		{
			EnemiesNeededToFlank = num38;
		}
		if (num39 != ReloadSpeedMultiplier)
		{
			ReloadSpeedMultiplier = num39;
		}
		if (num40 != NearestAllyWithSharedTargetAccuracyBonus)
		{
			NearestAllyWithSharedTargetAccuracyBonus = num40;
		}
		if (PartyMemberAI.IsInPartyList(GetComponent<PartyMemberAI>()))
		{
			Mover component5 = GetComponent<Mover>();
			if (component5 != null)
			{
				float num43 = 0f;
				float num44 = 0f;
				if (GameUtilities.IsAnimalCompanion(base.gameObject))
				{
					num43 = 5.5f + num17;
					num44 = 2f + num17;
				}
				else
				{
					num43 = 4f + num17;
					num44 = 2f + num17;
				}
				if (num43 != component5.GetRunSpeed())
				{
					component5.SetRunSpeed(num43);
				}
				if (num44 != component5.GetWalkSpeed())
				{
					component5.SetWalkSpeed(num44);
				}
			}
		}
		for (int n = 0; n < 10; n++)
		{
			DamageThreshhold[n] = 0f;
		}
		if (CharacterClass == Class.AnimalCompanion)
		{
			RemoveDuplicateAnimalCompanionAbilities();
		}
		LastKnownVersion = ProductConfiguration.Version;
	}

	public void RemoveDuplicateAnimalCompanionAbilities()
	{
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(CharacterClass.ToString());
		if (abilityProgressionTable == null)
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] abilities = abilityProgressionTable.GetAbilities(this, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.AbilityFilterFlag.OwnedAbilities | AbilityProgressionTable.AbilityFilterFlag.AutoGrantAbilities | AbilityProgressionTable.AbilityFilterFlag.RequirementsNotMet | AbilityProgressionTable.AbilityFilterFlag.RequirementsMet);
		if (abilities == null)
		{
			return;
		}
		for (int i = 0; i < abilities.Length; i++)
		{
			if (abilities[i] == null || abilities[i].Ability == null)
			{
				continue;
			}
			GenericAbility component = abilities[i].Ability.GetComponent<GenericAbility>();
			if (!component)
			{
				continue;
			}
			int abilityInstanceCount = GetAbilityInstanceCount(component);
			if (abilityInstanceCount > 1)
			{
				for (int num = abilityInstanceCount; num > 1; num--)
				{
					RemoveAbility(component);
				}
			}
		}
	}

	public int GetAbilityInstanceCount(GenericAbility abilityPrefab)
	{
		int num = 0;
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			if (GenericAbility.NameComparer.Instance.Equals(activeAbility, abilityPrefab))
			{
				num++;
			}
		}
		return num;
	}

	public void ClearAllProgressionAbilitiesAndTalents(Class tableClass)
	{
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(tableClass.ToString());
		AbilityProgressionTable abilityProgressionTable2 = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
		for (int num = m_talents.Count - 1; num >= 0; num--)
		{
			if (abilityProgressionTable.HasAbility(m_talents[num].gameObject) || abilityProgressionTable2.HasAbility(m_talents[num].gameObject))
			{
				m_talents[num].Remove(base.gameObject);
			}
		}
		int num2;
		for (num2 = m_abilities.Count - 1; num2 >= 0; num2--)
		{
			num2 = Math.Min(num2, m_abilities.Count - 1);
			if (abilityProgressionTable.HasAbility(m_abilities[num2].gameObject) || abilityProgressionTable2.HasAbility(m_abilities[num2].gameObject) || m_abilities[num2] is Chant)
			{
				RemoveAbility(m_abilities[num2]);
			}
			else if (abilityProgressionTable.HasTalentGrantingAbility(m_abilities[num2]) || abilityProgressionTable2.HasTalentGrantingAbility(m_abilities[num2]))
			{
				RemoveAbility(m_abilities[num2]);
			}
		}
	}

	public void ClearAllAbilities()
	{
		for (int i = 0; i < m_abilities.Count; i++)
		{
			m_abilities[i].DeactivateStatusEffects();
			Persistence component = m_abilities[i].GetComponent<Persistence>();
			if ((bool)component)
			{
				PersistenceManager.RemoveObject(component);
			}
			GameUtilities.DestroyImmediate(m_abilities[i].gameObject);
		}
		m_abilities.Clear();
		m_talents.Clear();
	}

	public void RefreshAllAbilities()
	{
		ClearAllProgressionAbilitiesAndTalents(CharacterClass);
		for (int num = m_abilities.Count - 1; num >= 0; num--)
		{
			if (m_abilities[num].MasteryLevel > 0)
			{
				RemoveAbility(m_abilities[num]);
			}
		}
		AddPresetAbilities();
		AddRacialAbilities();
		AddNewClassAbilities();
		AddPresetTalents();
		AddNewTalents();
	}

	public void AddPresetAbilities()
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject) || GetComponent<Health>() == null)
		{
			return;
		}
		for (int i = 0; i < Abilities.Count; i++)
		{
			GenericAbility genericAbility = Abilities[i];
			if ((bool)genericAbility && !ActiveAbilities.Contains(genericAbility, GenericAbility.NameComparer.Instance))
			{
				InstantiateAbility(genericAbility, GenericAbility.AbilityType.Ability);
			}
		}
		AddSecondWind();
	}

	public void AddSecondWind()
	{
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if ((bool)component && component.IsActiveInParty && component.SummonType == AIController.AISummonType.NotSummoned && !FindAbilityInstance("Second_Wind"))
		{
			AbilityProgressionTable.AddAbilityToCharacter("Second_Wind", this);
		}
	}

	public void AddNewClassAbilities(AbilityProgressionTable progressionTable)
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject) || !(progressionTable != null))
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] abilities = progressionTable.GetAbilities(this, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
		AbilityProgressionTable.UnlockableAbility[] array;
		if (abilities != null)
		{
			array = abilities;
			foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility in array)
			{
				if (!(unlockableAbility.Ability == null))
				{
					AbilityProgressionTable.AddAbilityToCharacter(unlockableAbility, this);
				}
			}
		}
		abilities = progressionTable.GetAbilities(this, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoMasterFilterFlags);
		if (abilities == null)
		{
			return;
		}
		array = abilities;
		foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility2 in array)
		{
			if (!(unlockableAbility2.Ability == null))
			{
				GenericAbility component = unlockableAbility2.Ability.GetComponent<GenericAbility>();
				if ((bool)component)
				{
					GenericAbility.MasterAbility(this, component);
				}
			}
		}
	}

	public void AddNewClassAbilities()
	{
		if (!PE_Paperdoll.IsObjectPaperdoll(base.gameObject) && !(GetComponent<Health>() == null))
		{
			AbilityProgressionTable progressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(CharacterClass.ToString());
			AddNewClassAbilities(progressionTable);
		}
	}

	public void AddPresetTalents()
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject))
		{
			return;
		}
		foreach (GenericTalent talent in Talents)
		{
			if (!(talent == null))
			{
				talent.Purchase(base.gameObject);
			}
		}
	}

	public void AddNewTalents(AbilityProgressionTable talentsProgressionTable)
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject) || !(talentsProgressionTable != null))
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] abilities = talentsProgressionTable.GetAbilities(this, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
		if (abilities == null)
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] array = abilities;
		foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility in array)
		{
			if (!(unlockableAbility.Ability == null))
			{
				Debug.Log("Talent \"" + AbilityProgressionTable.GetAbilityName(unlockableAbility.Ability) + "\" has been AutoGranted onto " + Name());
				AbilityProgressionTable.AddAbilityToCharacter(unlockableAbility, this);
			}
		}
	}

	public void AddNewTalents()
	{
		if (!PE_Paperdoll.IsObjectPaperdoll(base.gameObject) && !(GetComponent<Health>() == null))
		{
			AbilityProgressionTable talentsProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
			AddNewTalents(talentsProgressionTable);
		}
	}

	public void AddRacialAbilities()
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject) || GetComponent<Health>() == null)
		{
			return;
		}
		for (int num = m_abilities.Count - 1; num >= 0; num--)
		{
			if (!(m_abilities[num] == null) && m_abilities[num].EffectType == GenericAbility.AbilityType.Racial)
			{
				Persistence component = m_abilities[num].gameObject.GetComponent<Persistence>();
				if ((bool)component)
				{
					ObjectPersistencePacket packet = PersistenceManager.GetPacket(component);
					if (packet != null && packet.Packed)
					{
						return;
					}
				}
				if (m_abilities[num].Activated)
				{
					m_abilities[num].ForceDeactivate(base.gameObject);
				}
				m_abilities[num].HandleStatsOnRemoved();
				if ((bool)component)
				{
					PersistenceManager.RemoveObject(component);
				}
				GameUtilities.DestroyImmediate(m_abilities[num].gameObject);
				m_abilities.RemoveAt(num);
			}
		}
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Racial");
		if (!(abilityProgressionTable != null))
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] abilities = abilityProgressionTable.GetAbilities(this, AbilityProgressionTable.CategoryFlag.Racial, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
		if (abilities == null)
		{
			return;
		}
		AbilityProgressionTable.UnlockableAbility[] array = abilities;
		foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility in array)
		{
			if (!(unlockableAbility.Ability == null))
			{
				AbilityProgressionTable.AddAbilityToCharacter(unlockableAbility, this);
			}
		}
	}

	private void OnDisable()
	{
		ConversationManager.RemoveObjectFromActiveSpeakerGuidCache(new Guid(DisplayName.CharacterGuid), base.gameObject);
		ClearStackTracker();
	}

	private void OnDestroy()
	{
		ClearAllStatusEffects();
		ClearStackTracker();
		if (m_abilities != null)
		{
			m_abilities.Clear();
		}
		if (m_talents != null)
		{
			m_talents.Clear();
		}
		GameState.OnCombatEnd -= HandleGameUtilitiesOnCombatEnd;
		GameResources.OnPreSaveGame -= OnPreSaveGame;
		if (WorldTime.Instance != null)
		{
			WorldTime.Instance.OnTimeJump -= HandleGameOnTimeJump;
		}
		if (base.gameObject != null)
		{
			Health component = GetComponent<Health>();
			if ((bool)component)
			{
				component.OnDeath -= HandleOnDeath;
			}
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnPreSaveGame()
	{
		CleanAllOneHitUseEffects();
	}

	public bool UseAbilityForScript(int nameStringId)
	{
		GenericAbility genericAbility = null;
		for (int i = 0; i < m_abilities.Count; i++)
		{
			if (m_abilities[i].DisplayName.StringID == nameStringId && CanUseAbilityForScript(m_abilities[i]) && (genericAbility == null || (genericAbility.MasteryLevel == 0 && m_abilities[i].MasteryLevel > 0)))
			{
				genericAbility = m_abilities[i];
			}
		}
		if (genericAbility != null)
		{
			genericAbility.ActivateCooldown();
			return true;
		}
		ChanterTrait chanterTrait = GetChanterTrait();
		if ((bool)chanterTrait)
		{
			Phrase[] knownPhrases = chanterTrait.GetKnownPhrases();
			for (int j = 0; j < knownPhrases.Length; j++)
			{
				if (knownPhrases[j].DisplayName.StringID == nameStringId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanUseAbilityForScript(int nameStringId)
	{
		for (int i = 0; i < m_abilities.Count; i++)
		{
			if (m_abilities[i].DisplayName.StringID == nameStringId && CanUseAbilityForScript(m_abilities[i]))
			{
				return true;
			}
		}
		ChanterTrait chanterTrait = GetChanterTrait();
		if ((bool)chanterTrait)
		{
			Phrase[] knownPhrases = chanterTrait.GetKnownPhrases();
			for (int j = 0; j < knownPhrases.Length; j++)
			{
				if (knownPhrases[j].DisplayName.StringID == nameStringId)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CanUseAbilityForScript(GenericAbility ability)
	{
		if (ability.Ready)
		{
			return true;
		}
		GenericAbility.NotReadyValue notReadyValue = GenericAbility.NotReadyValue.AtMaxPer | GenericAbility.NotReadyValue.SpellCastingDisabled;
		if ((ability.WhyNotReady & notReadyValue) == 0)
		{
			return true;
		}
		return false;
	}

	public GenericAbility InstantiateAbility(GenericAbility ability, GenericAbility.AbilityType source)
	{
		if (ability == null)
		{
			Debug.LogWarning(base.name + " is trying to instantiate a null ability!", base.gameObject);
			return null;
		}
		GenericAbility genericAbility = UnityEngine.Object.Instantiate(ability);
		InstanceID component = genericAbility.GetComponent<InstanceID>();
		if ((bool)component)
		{
			component.Guid = Guid.NewGuid();
		}
		genericAbility.transform.parent = base.transform;
		genericAbility.Owner = base.gameObject;
		genericAbility.EffectType = source;
		if (genericAbility is GenericSpell)
		{
			genericAbility.EffectType = GenericAbility.AbilityType.Spell;
			GenericSpell genericSpell = genericAbility as GenericSpell;
			PartyMemberAI component2 = base.gameObject.GetComponent<PartyMemberAI>();
			if (genericSpell.SpellClass == Class.Wizard && component2 != null)
			{
				genericSpell.NeedsGrimoire = true;
			}
			genericSpell.IsFree = false;
		}
		foreach (GenericTalent talent in m_talents)
		{
			if (!(talent == null))
			{
				talent.CheckNewAbility(genericAbility);
			}
		}
		m_abilities.Add(genericAbility);
		List<GenericAbility> list = new List<GenericAbility>(m_abilities);
		list.Sort(SortAbilities);
		m_abilities.Clear();
		m_abilities.AddRange(list);
		genericAbility.HandleStatsOnAdded();
		return genericAbility;
	}

	public GenericAbility FindAbilityInstance(string prefabName)
	{
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			if (activeAbility.name.Replace("(Clone)", "") == prefabName)
			{
				return activeAbility;
			}
		}
		return null;
	}

	public GenericAbility FindAbilityInstance(GenericAbility abilityPrefab)
	{
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			if (GenericAbility.NameComparer.Instance.Equals(activeAbility, abilityPrefab))
			{
				return activeAbility;
			}
		}
		return null;
	}

	public GenericAbility FindMasteredAbilityInstance(GenericAbility abilityPrefab)
	{
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			if (GenericAbility.NameComparer.Instance.Equals(activeAbility, abilityPrefab) && activeAbility.MasteryLevel > 0)
			{
				return activeAbility;
			}
		}
		return null;
	}

	public int GetNumMasteredAbilities()
	{
		int num = 0;
		for (int i = 0; i < m_abilities.Count; i++)
		{
			if (m_abilities[i].MasteryLevel > 0)
			{
				num++;
			}
		}
		return num;
	}

	public static int MaxMasteredAbilitiesAllowed(Class characterClass, int level)
	{
		if (characterClass != Class.Wizard && characterClass != Class.Druid && characterClass != Class.Priest)
		{
			return 0;
		}
		if (level < 9)
		{
			return 0;
		}
		return (level - 9) / 2 + 1;
	}

	public int MaxMasteredAbilitiesAllowed()
	{
		return MaxMasteredAbilitiesAllowed(CharacterClass, Level);
	}

	public bool RemoveAbility(GenericAbility ability)
	{
		if (!ability)
		{
			return false;
		}
		bool result = false;
		int num;
		for (num = ActiveAbilities.Count - 1; num >= 0; num--)
		{
			num = Math.Min(num, ActiveAbilities.Count - 1);
			GenericAbility genericAbility = ActiveAbilities[num];
			if (GenericAbility.NameComparer.Instance.Equals(genericAbility, ability))
			{
				if (genericAbility.Activated)
				{
					genericAbility.ForceDeactivate(genericAbility.Owner);
				}
				ActiveAbilities.Remove(genericAbility);
				genericAbility.HandleStatsOnRemoved();
				Persistence component = genericAbility.gameObject.GetComponent<Persistence>();
				if ((bool)component)
				{
					PersistenceManager.RemoveObject(component);
				}
				GameUtilities.DestroyImmediate(genericAbility.gameObject);
				result = true;
			}
		}
		return result;
	}

	public bool RemoveTalent(GenericTalent talent)
	{
		bool result = false;
		for (int num = ActiveTalents.Count - 1; num >= 0; num--)
		{
			GenericTalent genericTalent = ActiveTalents[num];
			if (GenericTalent.NameComparer.Instance.Equals(genericTalent, talent))
			{
				genericTalent.Remove(base.gameObject);
				Persistence component = genericTalent.gameObject.GetComponent<Persistence>();
				if ((bool)component)
				{
					PersistenceManager.RemoveObject(component);
				}
				GameUtilities.DestroyImmediate(genericTalent.gameObject);
				result = true;
			}
		}
		return result;
	}

	public bool IsImmuneToAffliction(Affliction affliction)
	{
		if (affliction != null)
		{
			return AfflictionImmunities.Contains(affliction);
		}
		return false;
	}

	public bool CanApplyAffliction(Affliction affliction)
	{
		if (IsImmuneToAffliction(affliction) || HasStatusEffectOfType(StatusEffect.ModifiedStat.StasisShield))
		{
			return false;
		}
		if (CheckAfflictionShield(affliction))
		{
			return false;
		}
		return true;
	}

	private bool CheckAfflictionShield(Affliction affliction)
	{
		foreach (StatusEffect item in FindStatusEffectsOfType(StatusEffect.ModifiedStat.AfflictionShield))
		{
			if (item.Params.AfflictionPrefab == affliction)
			{
				item.IncrementGeneralCounterForDestroy();
				return true;
			}
		}
		return false;
	}

	public bool CanApplyStatusEffect(StatusEffect effect)
	{
		if (IsImmuneToAffliction(effect.AfflictionOrigin))
		{
			return false;
		}
		if (HasStatusEffectOfType(StatusEffect.ModifiedStat.StasisShield))
		{
			return false;
		}
		if (!effect.CanApply(this))
		{
			return false;
		}
		return true;
	}

	public void ApplyStatusEffect(StatusEffect effect)
	{
		if (effect != null && CanApplyStatusEffect(effect))
		{
			ApplyStatusEffectHelper(effect);
		}
	}

	public bool ApplyStatusEffectImmediate(StatusEffect effect)
	{
		if (effect == null)
		{
			return false;
		}
		if (CanApplyStatusEffect(effect) && ApplyStatusEffectHelper(effect))
		{
			effect.ApplyEffect(base.gameObject);
			return true;
		}
		return false;
	}

	private bool ApplyStatusEffectHelper(StatusEffect effect)
	{
		if (effect == null)
		{
			return false;
		}
		effect.CheckForErrors();
		if (effect.AfflictionOrigin != null)
		{
			StatusEffect statusEffect = FindExistingStatusEffectFromAffliction(effect.Params, effect.AfflictionOrigin);
			if (statusEffect != null && !statusEffect.RemovingEffect)
			{
				if (statusEffect.Duration > 0f)
				{
					float timeLeft = statusEffect.TimeLeft;
					float num = effect.CalculateDuration(base.gameObject, ignoreTemporaryAdjustment: true);
					if (num > timeLeft)
					{
						AdjustStatusEffectDuration(statusEffect, num - timeLeft, skipOverride: true);
					}
				}
				if (statusEffect != effect)
				{
					effect.Reset();
				}
				return false;
			}
		}
		if (effect.Exclusive)
		{
			ClearStatusEffects(effect.Params.AffectsStat);
		}
		if (m_statusEffects.IndexOf(effect) != -1)
		{
			if (effect.Duration > 0f)
			{
				float timeLeft2 = effect.TimeLeft;
				float num2 = effect.CalculateDuration(base.gameObject, ignoreTemporaryAdjustment: true);
				if (num2 > timeLeft2)
				{
					AdjustStatusEffectDuration(effect, num2 - timeLeft2, skipOverride: true);
					if (effect.Params.TriggerAdjustment != null && effect.Params.TriggerAdjustment.Type == StatusEffectTrigger.TriggerType.OnDamage && effect.Params.TriggerAdjustment.RemoveEffectAtMax)
					{
						while (effect.m_triggerCount > 0)
						{
							effect.OffTrigger();
						}
					}
				}
			}
			return false;
		}
		if (this.OnAddStatusEffect != null)
		{
			this.OnAddStatusEffect(base.gameObject, effect, isFromAura: false);
		}
		effect.EffectID = UniqueStatusEffectID;
		m_statusEffects.Add(effect);
		m_updateTracker = true;
		if ((bool)effect.Owner)
		{
			CharacterStats component = effect.Owner.GetComponent<CharacterStats>();
			if (component != null && component.OnEffectApply != null)
			{
				component.OnEffectApply(effect.Owner, new CombatEventArgs(effect.Owner, base.gameObject, effect));
			}
		}
		else
		{
			Debug.LogError(effect.GetDisplayName() + " doesn't have an owner set! Is it supposed to be " + base.gameObject.name + "?");
		}
		return true;
	}

	public void ApplyAffliction(Affliction aff)
	{
		ApplyAffliction(aff, base.gameObject, GenericAbility.AbilityType.Undefined, null, deleteOnClear: false, 0f, null);
	}

	public void ApplyAffliction(Affliction aff, GameObject attacker, GenericAbility.AbilityType abType, DamageInfo hitInfo, bool deleteOnClear, float durationOverride, List<StatusEffect> appliedEffects)
	{
		if (!(aff != null) || aff.StatusEffects == null)
		{
			return;
		}
		if (IsImmuneToAffliction(aff) || CheckAfflictionShield(aff))
		{
			UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2188), base.gameObject);
			if (PartyHelper.IsPartyMember(attacker))
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(attacker, SoundSet.SoundAction.TargetImmune, SoundSet.s_LongVODelay, forceInterrupt: false);
			}
			return;
		}
		if (aff.Exclusive)
		{
			ClearEffectFromAffliction(aff);
		}
		if (aff.Overrides != null)
		{
			Affliction[] overrides = aff.Overrides;
			foreach (Affliction aff2 in overrides)
			{
				SuppressEffectFromAffliction(aff2);
			}
		}
		StatusEffectParams[] statusEffects = aff.StatusEffects;
		foreach (StatusEffectParams param in statusEffects)
		{
			StatusEffect statusEffect = StatusEffect.Create(attacker, param, abType, hitInfo, deleteOnClear, durationOverride);
			statusEffect.AfflictionOrigin = aff;
			if (ApplyStatusEffectImmediate(statusEffect))
			{
				appliedEffects?.Add(statusEffect);
			}
		}
		if (!aff.Material.Empty)
		{
			aff.Material.Replace(base.gameObject);
		}
		if (aff.DisengageAll)
		{
			AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
			if (aIController != null)
			{
				aIController.CancelAllEngagements();
			}
		}
	}

	public void ClearAllStatusEffects()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num] != null)
			{
				m_statusEffects[num].ClearEffect(base.gameObject, triggerEffects: false);
			}
		}
		m_statusEffects.Clear();
	}

	public bool ClearEffect(StatusEffect effect)
	{
		if (effect == null)
		{
			return false;
		}
		if (m_statusEffects == null)
		{
			return false;
		}
		if (!m_statusEffects.Contains(effect))
		{
			return false;
		}
		effect.ClearEffect(base.gameObject);
		m_statusEffects.Remove(effect);
		m_updateTracker = true;
		if (this.OnClearStatusEffect != null)
		{
			this.OnClearStatusEffect(base.gameObject, effect);
		}
		effect.Reset();
		return true;
	}

	public void ClearEffect(string tag, GameObject caster = null)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			StatusEffect statusEffect = m_statusEffects[num];
			if ((!caster || caster == statusEffect.Owner) && string.Compare(statusEffect.Params.Tag, tag, ignoreCase: true) == 0)
			{
				ClearEffect(statusEffect);
				break;
			}
		}
	}

	public void ClearEffectRange(IEnumerable<StatusEffect> effects)
	{
		foreach (StatusEffect effect in effects)
		{
			ClearEffect(effect);
		}
	}

	public void ClearEffects(StatusEffect.ModifiedStat modifiedStat)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].Params.AffectsStat == modifiedStat)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearEffectsFromAfflictions()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AfflictionOrigin != null)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearEffectFromAffliction(Affliction aff)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AfflictionOrigin == aff)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearEffectFromAffliction(string tag)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AfflictionOrigin != null && string.Compare(m_statusEffects[num].AfflictionOrigin.Tag, tag, ignoreCase: true) == 0)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearEffectFromAbility(GenericAbility ab)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AbilityOrigin == ab)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearRestingAffliction()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AfflictionOrigin != null && m_statusEffects[num].AfflictionOrigin.FromResting)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void SuppressEffectFromAffliction(Affliction aff)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].AfflictionOrigin == aff)
			{
				m_statusEffects[num].Suppress();
			}
		}
	}

	public void ClearEffectInSlot(Equippable.EquipmentSlot slot)
	{
		if (slot == Equippable.EquipmentSlot.None)
		{
			return;
		}
		List<StatusEffect> list = new List<StatusEffect>();
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect.Slot == slot)
			{
				list.Add(statusEffect);
				statusEffect.ClearEffect(base.gameObject);
			}
		}
		foreach (StatusEffect item in list)
		{
			m_statusEffects.Remove(item);
			if (this.OnClearStatusEffect != null)
			{
				this.OnClearStatusEffect(base.gameObject, item);
			}
			item.Reset();
		}
		m_updateTracker = true;
	}

	public void ClearWildstrikeEffects()
	{
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect.AbilityOrigin != null && statusEffect.AbilityOrigin is Wildstrike)
			{
				statusEffect.AbilityOrigin.Deactivate(base.gameObject);
			}
		}
	}

	public void ClearStatusEffects()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			ClearEffect(m_statusEffects[num]);
		}
	}

	public void ClearStatusEffects(string tag)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (string.Compare(m_statusEffects[num].Params.Tag, tag, ignoreCase: true) == 0)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearStatusEffects(StatusEffect.ModifiedStat mStat)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].Params.AffectsStat == mStat)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearStatusEffects(EffectType effectType)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (effectType == EffectType.All || (effectType == EffectType.Hostile && m_statusEffects[num].Params.IsHostile) || (effectType == EffectType.Beneficial && !m_statusEffects[num].Params.IsHostile))
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearStatusEffectsOnDeath()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].Params.IsHostile && !m_statusEffects[num].Params.KeepOnDeath)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void ClearStatusEffectsByKeyword(string keyword)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (HasKeyword(keyword))
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public float AdjustStatusEffectDuration(StatusEffect effect, float DurationAdj)
	{
		return AdjustStatusEffectDuration(effect, DurationAdj, skipOverride: false);
	}

	public float AdjustStatusEffectDuration(StatusEffect effect, float DurationAdj, bool skipOverride)
	{
		float duration = effect.Duration;
		duration += DurationAdj;
		if (duration <= 0f)
		{
			duration = 0.01f;
		}
		float num = duration - effect.Duration;
		effect.TemporaryDurationAdjustment += num;
		return num;
	}

	public void AdjustStatusEffectDurationsFromAffliction(Affliction aff, float DurationAdj)
	{
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect.AfflictionOrigin == aff)
			{
				AdjustStatusEffectDuration(statusEffect, DurationAdj);
			}
		}
	}

	public void AdjustStatusEffectDurationsFromKeyword(string keyword, float DurationAdj)
	{
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect.AbilityOrigin != null)
			{
				AttackBase component = statusEffect.AbilityOrigin.GetComponent<AttackBase>();
				if (component != null && component.HasKeyword(keyword))
				{
					AdjustStatusEffectDuration(statusEffect, DurationAdj);
					continue;
				}
			}
			if (statusEffect.AfflictionOrigin != null && statusEffect.AfflictionKeyword != null && keyword.Equals(statusEffect.AfflictionKeyword, StringComparison.Ordinal))
			{
				AdjustStatusEffectDuration(statusEffect, DurationAdj);
			}
		}
	}

	public void AdjustStatusEffectDurations(EffectType effectType, float DurationAdj, StatusEffect excludedEffect)
	{
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect != excludedEffect && (effectType == EffectType.All || (effectType == EffectType.Hostile && statusEffect.Params.IsHostile) || (effectType == EffectType.Beneficial && !statusEffect.Params.IsHostile)) && statusEffect.Duration > 0f)
			{
				AdjustStatusEffectDuration(statusEffect, DurationAdj);
			}
		}
	}

	public float AdjustBeneficialEffectTime(float DurationAdj)
	{
		float num = 0f;
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect != null && !statusEffect.Params.IsHostile && statusEffect.Duration > 0f)
			{
				num += AdjustStatusEffectDuration(statusEffect, DurationAdj, skipOverride: true);
			}
		}
		return num;
	}

	public void SpreadBeneficialEffectTime(float DurationAdj)
	{
		int num = 0;
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			if (statusEffect != null && !statusEffect.Params.IsHostile && statusEffect.Duration > 0f)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		float durationAdj = DurationAdj / (float)num;
		foreach (StatusEffect statusEffect2 in m_statusEffects)
		{
			if (statusEffect2 != null && !statusEffect2.Params.IsHostile && statusEffect2.Duration > 0f)
			{
				AdjustStatusEffectDuration(statusEffect2, durationAdj);
			}
		}
	}

	public void DeactivateAllAbilities()
	{
		foreach (GenericAbility activeAbility in ActiveAbilities)
		{
			if (activeAbility.Activated)
			{
				activeAbility.Deactivate(activeAbility.Owner);
				activeAbility.HandleStatsOnRemoved();
			}
		}
	}

	private void HandleOnDeath(GameObject myObject, GameEventArgs args)
	{
		ClearStatusEffects(StatusEffect.ModifiedStat.KnockedDown);
		ClearStatusEffectsOnDeath();
	}

	private void HandleGameUtilitiesOnCombatEnd(object sender, EventArgs e)
	{
		try
		{
			m_MarkersAppliedThisCombat.Clear();
			for (int num = m_statusEffects.Count - 1; num >= 0; num--)
			{
				if (m_statusEffects[num].LastsUntilCombatEnds)
				{
					ClearEffect(m_statusEffects[num]);
				}
				else if ((bool)m_statusEffects[num].AbilityOrigin && m_statusEffects[num].AbilityOrigin.CombatOnly)
				{
					ClearEffect(m_statusEffects[num]);
				}
				else if (m_statusEffects[num].IsDOT)
				{
					ClearEffect(m_statusEffects[num]);
				}
			}
			foreach (GenericAbility activeAbility in ActiveAbilities)
			{
				if (activeAbility != null)
				{
					activeAbility.HandleGameUtilitiesOnCombatEnd(sender, e);
				}
			}
			if (base.gameObject != null)
			{
				for (int i = 0; i < 8; i++)
				{
				}
			}

			CharacterStats charStats = base.gameObject.GetComponent<CharacterStats>();
			if (charStats != null)
			{
				if (charStats.CharacterClass == Class.Priest || charStats.CharacterClass == Class.Wizard || charStats.CharacterClass == Class.Druid)
				{
					ResetSpellUsage(charStats);
				}
			}
			PlayPartyFatigueSoundIfAble();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
			if ((bool)UIDebug.Instance)
			{
				UIDebug.Instance.LogOnScreenWarning("Exception in CharacterStats.HandleGameUtilitiesOnCombatEnd! Please Fix!", UIDebug.Department.Programming, 10f);
			}
		}
	}

	public static void ResetStaticData()
	{
		s_EnemiesSpottedInStealth.Clear();
	}

	public void HandleGameOnResting()
	{
		CurrentFatigueLevel = FatigueLevel.None;
		for (int i = 0; i < SpellCastCount.Length; i++)
		{
			SpellCastCount[i] = 0;
		}
		Health component = GetComponent<Health>();
		if (component != null)
		{
			component.HandleGameOnResting();
		}
		foreach (StatusEffect statusEffect in m_statusEffects)
		{
			statusEffect.HandleClearingEffectOnResting();
		}
		for (int j = 0; j < ActiveAbilities.Count; j++)
		{
			ActiveAbilities[j].HandleStuckAbilityDeactivation();
		}
		if (this.OnResting != null)
		{
			this.OnResting(this, null);
		}
	}

	private void HandleGameOnTimeJump(int gameSeconds, bool isMapTravel, bool isResting)
	{
		if (isMapTravel)
		{
			PlayPartyFatigueSoundIfAble();
		}
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			m_statusEffects[i].TimeJumpUpdate(gameSeconds / WorldTime.Instance.GameSecondsPerRealSecond);
		}
	}

	public int CountStatusEffects(string tag, GameObject caster = null)
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect != null && (!caster || caster == statusEffect.Owner) && string.Compare(statusEffect.Params.Tag, tag, ignoreCase: true) == 0)
			{
				num++;
			}
		}
		return num;
	}

	public int CountStatusEffects(GenericAbility ability)
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect != null && statusEffect.AbilityOrigin != null && GenericAbility.NameComparer.Instance.Equals(ability, statusEffect.AbilityOrigin))
			{
				num++;
			}
		}
		return num;
	}

	public int CountDOTs()
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].IsDOT)
			{
				num++;
			}
		}
		return num;
	}

	public int CountHostileEffects()
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].Params.IsHostile)
			{
				num++;
			}
		}
		return num;
	}

	public float GetStatusEffectValueSum(StatusEffect.ModifiedStat statType)
	{
		float num = 0f;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.Params.AffectsStat == statType)
			{
				num += statusEffect.CurrentAppliedValue;
			}
		}
		return num;
	}

	public float GetStatusEffectValueMultiplier(StatusEffect.ModifiedStat statType)
	{
		float num = 1f;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.Params.AffectsStat == statType)
			{
				num += statusEffect.CurrentAppliedValue - 1f;
			}
		}
		return Mathf.Max(0f, num);
	}

	public void GetHostileStatusEffects(List<StatusEffect> hostileStatusEffects)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Params.IsHostile)
			{
				hostileStatusEffects.Add(statusEffect);
			}
		}
	}

	public bool HasStatusEffectOfType(StatusEffect.ModifiedStat statType)
	{
		return GetIndexOfStatusEffectOfType(statType) >= 0;
	}

	public int GetIndexOfStatusEffectOfType(StatusEffect.ModifiedStat statType)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.Params.AffectsStat == statType)
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasStatusEffectThatPausesRecoveryTimer()
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.IsRecoveryTimePausingEffect)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFactionSwapEffect()
	{
		if (!HasStatusEffectOfType(StatusEffect.ModifiedStat.Confused))
		{
			return HasStatusEffectOfType(StatusEffect.ModifiedStat.SwapFaction);
		}
		return true;
	}

	public bool HasStatusEffectFromAffliction(Affliction aff)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.AfflictionOrigin == aff)
			{
				return true;
			}
		}
		return false;
	}

	public StatusEffect GetStatusEffectFromAffliction(Affliction aff)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.AfflictionOrigin == aff)
			{
				return statusEffect;
			}
		}
		return null;
	}

	public IEnumerable<StatusEffect> FindStatusEffectsOfType(StatusEffect.ModifiedStat statType)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.Params.AffectsStat == statType)
			{
				yield return statusEffect;
			}
		}
	}

	public StatusEffect FindFirstStatusEffectOfType(StatusEffect.ModifiedStat statType)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.Params.AffectsStat == statType)
			{
				return statusEffect;
			}
		}
		return null;
	}

	public bool HasStatusEffectWithSearchFunction(Func<StatusEffect, bool> searchFunction)
	{
		if (searchFunction == null || m_statusEffects == null || m_statusEffects.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (searchFunction(m_statusEffects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public StatusEffect FindExistingStatusEffectFromAffliction(StatusEffectParams statType, Affliction aff)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.Applied && statusEffect.AfflictionOrigin == aff && statusEffect.Params.EqualsExceptParameter(statType, StatusEffectParams.ParamType.Count))
			{
				return statusEffect;
			}
		}
		return null;
	}

	public bool HasHitTypeStatusEffects(HitType ifrom, HitType to)
	{
		return FindHitTypeStatusEffects(ifrom, to).Any();
	}

	public IEnumerable<StatusEffect> FindHitTypeStatusEffects(HitType ifrom, HitType to)
	{
		IEnumerable<StatusEffect.ModifiedStat> validModifiers = from htm in StatusEffect.HitTypeStats
			where htm.From == ifrom && htm.To == to
			select htm.ModifiedStat;
		return m_statusEffects.Where((StatusEffect se) => validModifiers.Contains(se.Params.AffectsStat));
	}

	public void SuspendEffects(EffectType effectType)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if ((effectType == EffectType.All || (effectType == EffectType.Hostile && statusEffect.Params.IsHostile) || (effectType == EffectType.Beneficial && !statusEffect.Params.IsHostile && statusEffect.Duration != 0f)) && statusEffect.Suspend())
			{
				m_updateTracker = true;
			}
		}
	}

	public void UnsuspendEffects(EffectType effectType, float time)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.TimeApplied < time && (effectType == EffectType.All || (effectType == EffectType.Hostile && statusEffect.Params.IsHostile) || (effectType == EffectType.Beneficial && !statusEffect.Params.IsHostile)) && statusEffect.Unsuspend())
			{
				m_updateTracker = true;
			}
		}
	}

	public void ClearAllSuspensions()
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			while (statusEffect.IsSuspended)
			{
				statusEffect.Unsuspend();
				m_updateTracker = true;
			}
		}
	}

	public void AdjustEffectDurations(EffectType effectType, float mult)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (effectType == EffectType.All || (effectType == EffectType.Hostile && statusEffect.Params.IsHostile) || (effectType == EffectType.Beneficial && !statusEffect.Params.IsHostile))
			{
				statusEffect.Duration *= mult;
			}
		}
	}

	public StatusEffect FindStatusEffect(uint effectID)
	{
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = m_statusEffects[i];
			if (statusEffect.EffectID == effectID)
			{
				return statusEffect;
			}
		}
		return null;
	}

	public GenericAbility FindWoundsTrait()
	{
		for (int i = 0; i < ActiveAbilities.Count; i++)
		{
			GenericAbility genericAbility = ActiveAbilities[i];
			if (genericAbility is WoundsTrait)
			{
				return genericAbility;
			}
		}
		return null;
	}

	public float TotalStatusEffectWoundDamage()
	{
		float num = 0f;
		GenericAbility genericAbility = FindWoundsTrait();
		if (genericAbility != null)
		{
			for (int i = 0; i < m_statusEffects.Count; i++)
			{
				StatusEffect statusEffect = m_statusEffects[i];
				if (string.Compare(statusEffect.Params.Tag, genericAbility.StatusEffects[0].Tag, ignoreCase: true) == 0)
				{
					num -= statusEffect.ParamsValue();
				}
			}
		}
		return num;
	}

	public void TriggerWhenLaunchesAttack(GameObject enemy, AttackBase attack, Vector3 location)
	{
		if (this.OnAttackLaunch != null)
		{
			DamageInfo info = new DamageInfo(enemy, 0f, attack);
			if (enemy == null)
			{
				this.OnAttackLaunch(base.gameObject, new CombatEventArgs(info, base.gameObject, location));
			}
			else
			{
				this.OnAttackLaunch(base.gameObject, new CombatEventArgs(info, base.gameObject, enemy));
			}
		}
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			m_statusEffects[num].WhenLaunchesAttack(base.gameObject, enemy, attack);
		}
	}

	public void TriggerWhenAttacked(GameObject enemy, AttackBase attack)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			m_statusEffects[num].WhenAttacked(base.gameObject, enemy, attack);
		}
	}

	public void NotifyHitFrame(GameObject enemy, DamageInfo damage)
	{
		if (this.OnAttackHitFrame != null)
		{
			this.OnAttackHitFrame(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
	}

	public void TriggerWhenHits(GameObject enemy, DamageInfo damage)
	{
		if (this.OnAttackHits != null)
		{
			this.OnAttackHits(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenHits(base.gameObject, enemy, damage);
			}
		}
	}

	public void TriggerWhenMisses(GameObject enemy, DamageInfo damage)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenMisses(base.gameObject, enemy, damage);
			}
		}
	}

	public void TriggerWhenInterrupted()
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenInterrupted(base.gameObject);
			}
		}
	}

	public void TriggerWhenHit(GameObject attacker, DamageInfo damage)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenHit(attacker, base.gameObject, damage);
			}
		}
	}

	public void TriggerWhenTakesDamage(GameObject attacker, DamageInfo damage)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenTakesDamage(attacker, base.gameObject, damage);
			}
		}
	}

	public void TriggerWhenBeamHits(GameObject enemy)
	{
		if (this.OnBeamHits != null)
		{
			this.OnBeamHits(base.gameObject, new CombatEventArgs(base.gameObject, enemy));
		}
	}

	public void TriggerWhenInflictsDamage(GameObject enemy, DamageInfo damage)
	{
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (num < m_statusEffects.Count)
			{
				m_statusEffects[num].WhenInflictsDamage(base.gameObject, enemy, damage);
			}
		}
	}

	public void CleanOneHitUseEffects(GameObject owner)
	{
		if (m_statusEffects == null)
		{
			return;
		}
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].Params.OneHitUse && m_statusEffects[num].Owner == owner)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void CleanAllOneHitUseEffects()
	{
		if (m_statusEffects == null)
		{
			return;
		}
		for (int num = m_statusEffects.Count - 1; num >= 0; num--)
		{
			if (m_statusEffects[num].Params.OneHitUse)
			{
				ClearEffect(m_statusEffects[num]);
			}
		}
	}

	public void RevealDefense(DefenseType type)
	{
		if (type != DefenseType.None)
		{
			DefensesKnown.Set((int)type, value: true);
		}
	}

	public void RevealDT(DamagePacket.DamageType type)
	{
		if (type == DamagePacket.DamageType.All)
		{
			for (DamagePacket.DamageType damageType = DamagePacket.DamageType.Slash; damageType < DamagePacket.DamageType.Count; damageType++)
			{
				DTsKnown.Set((int)damageType, value: true);
			}
		}
		else if (type < DamagePacket.DamageType.Count)
		{
			DTsKnown.Set((int)type, value: true);
		}
	}

	public string GetPerceivedDefenseString(DefenseType defense, out BestiaryCertainty known)
	{
		int perceivedDefense = GetPerceivedDefense(defense, out known);
		if (CalculateIsImmune(defense, null, null))
		{
			return GUIUtils.GetText(2187);
		}
		return perceivedDefense.ToString();
	}

	public int GetPerceivedDefense(DefenseType type, out BestiaryCertainty known)
	{
		if (type == DefenseType.None)
		{
			known = BestiaryCertainty.Unknown;
			return 0;
		}
		if ((DefensesKnown.Length > (int)type && DefensesKnown.Get((int)type)) || IsPartyMember)
		{
			known = BestiaryCertainty.Exact;
			return CalculateDefense(type, null, null, isSecondary: false, allowRedirect: false);
		}
		if (BestiaryManager.Instance.CanSeeStat(BestiaryReference, DefenseTypeAsStat(type)))
		{
			known = BestiaryCertainty.Estimated;
			return BestiaryReference.GetComponent<CharacterStats>().CalculateDefense(type, null, null, isSecondary: false, allowRedirect: false);
		}
		known = BestiaryCertainty.Unknown;
		return 0;
	}

	public float GetPerceivedDamThresh(DamagePacket.DamageType type, bool isVeilPiercing, out BestiaryCertainty known)
	{
		bool flag = false;
		if (type < DamagePacket.DamageType.Count)
		{
			flag = DTsKnown.Get((int)type);
		}
		else if (type == DamagePacket.DamageType.All)
		{
			for (DamagePacket.DamageType damageType = DamagePacket.DamageType.Slash; damageType < DamagePacket.DamageType.Count; damageType++)
			{
				flag |= DTsKnown.Get((int)damageType);
			}
		}
		else
		{
			flag = true;
		}
		if (flag || IsPartyMember)
		{
			known = BestiaryCertainty.Exact;
			return CalcDT(type, isVeilPiercing);
		}
		if (BestiaryManager.Instance.CanSeeStat(BestiaryReference, IndexableStat.DT))
		{
			known = BestiaryCertainty.Estimated;
			return BestiaryReference.GetComponent<CharacterStats>().CalcDT(type, isVeilPiercing, prefab: true);
		}
		known = BestiaryCertainty.Unknown;
		return 0f;
	}

	private void Update()
	{
		if ((bool)m_bestiaryReference)
		{
			return;
		}
		NoiseUpdate(Time.deltaTime);
		DetectUpdate(Time.deltaTime);
		TrapCooldownTimerUpdate(Time.deltaTime);
		if (m_weaponSwitchingTimer >= 0f)
		{
			m_weaponSwitchingTimer -= Time.deltaTime;
		}
		if (m_interruptTimer >= 0f)
		{
			m_interruptTimer -= Time.deltaTime;
		}
		if (CurrentGrimoireCooldown > 0f)
		{
			CurrentGrimoireCooldown -= Time.deltaTime;
			if (CurrentGrimoireCooldown < 0f)
			{
				CurrentGrimoireCooldown = 0f;
			}
		}
		if (!HasStatusEffectThatPausesRecoveryTimer())
		{
			float num = 1f;
			if (IsMoving)
			{
				num = AttackData.Instance.MovingRecoveryMult;
				if (m_equipment != null && m_equipment.PrimaryAttack != null && m_equipment.PrimaryAttack is AttackRanged)
				{
					num += RangedMovingRecoveryReductionPct;
				}
			}
			float num2 = Time.deltaTime * num;
			if (m_recoveryTimer > 0f)
			{
				m_recoveryTimer -= num2;
			}
			for (GenericAbility.ActivationGroup activationGroup = GenericAbility.ActivationGroup.None; activationGroup < GenericAbility.ActivationGroup.Count; activationGroup++)
			{
				if (m_modalCooldownTimer[(int)activationGroup] > 0f)
				{
					m_modalCooldownTimer[(int)activationGroup] -= num2;
				}
			}
		}
		for (int num3 = m_statusEffects.Count - 1; num3 >= 0; num3--)
		{
			if (m_statusEffects[num3].Expired)
			{
				StatusEffect statusEffect = m_statusEffects[num3];
				m_statusEffects.RemoveAt(num3);
				m_updateTracker = true;
				if (this.OnClearStatusEffect != null)
				{
					this.OnClearStatusEffect(base.gameObject, statusEffect);
				}
				statusEffect.Reset();
			}
		}
		for (int num4 = m_abilities.Count - 1; num4 >= 0; num4--)
		{
			if (m_abilities[num4] == null)
			{
				m_abilities.RemoveAt(num4);
			}
			else
			{
				GenericAbility genericAbility = m_abilities[num4];
				if (genericAbility.Passive && !genericAbility.Activated && genericAbility.Ready && genericAbility.IsLoaded)
				{
					genericAbility.Activate();
					m_updateTracker = true;
				}
			}
		}
		if (m_updateTracker)
		{
			m_updateTracker = false;
			ClearStackTracker();
			for (int i = 0; i < m_statusEffects.Count; i++)
			{
				StatusEffect statusEffect2 = m_statusEffects[i];
				if (statusEffect2.IsSuspended)
				{
					continue;
				}
				bool isSuppressed = statusEffect2.IsSuppressed;
				bool flag = false;
				for (int j = 0; j < m_statusEffects.Count; j++)
				{
					if (i != j)
					{
						StatusEffect statusEffect3 = m_statusEffects[j];
						if (!statusEffect3.IsSuspended && statusEffect3.Suppresses(statusEffect2, i > j))
						{
							flag = true;
							break;
						}
					}
				}
				if (isSuppressed && !flag)
				{
					statusEffect2.Unsuppress();
				}
				else if (!isSuppressed && flag)
				{
					statusEffect2.Suppress();
				}
			}
		}
		for (int k = 0; k < m_statusEffects.Count; k++)
		{
			StatusEffect statusEffect4 = m_statusEffects[k];
			if (statusEffect4.Stackable)
			{
				if (!statusEffect4.HasBeenApplied)
				{
					statusEffect4.ApplyEffect(base.gameObject);
				}
			}
			else
			{
				if (statusEffect4.IsSuspended || statusEffect4.IsSuppressed)
				{
					continue;
				}
				StatusEffect trackedEffect = GetTrackedEffect(statusEffect4.NonstackingEffectType, statusEffect4.GetStackingKey());
				int num5 = m_statusEffects.IndexOf(trackedEffect);
				if (trackedEffect == null || trackedEffect.IsSuspended || statusEffect4.Suppresses(trackedEffect, num5 > k))
				{
					if (trackedEffect != null && trackedEffect.Applied)
					{
						trackedEffect.Suppress();
					}
					AddTrackedEffect(statusEffect4);
				}
			}
		}
		if (s_PlayFatigueSoundWhenNotLoading && UIInterstitialManager.Instance != null && !UIInterstitialManager.Instance.WindowActive() && !GameState.IsLoading)
		{
			IEnumerable<PartyMemberAI> onlyPrimaryPartyMembers = PartyMemberAI.OnlyPrimaryPartyMembers;
			if (onlyPrimaryPartyMembers != null)
			{
				List<PartyMemberAI> list = new List<PartyMemberAI>();
				CharacterStats characterStats = null;
				foreach (PartyMemberAI item in onlyPrimaryPartyMembers)
				{
					if (!(item == null))
					{
						characterStats = item.GetComponent<CharacterStats>();
						if (characterStats != null && characterStats.CurrentFatigueLevel != 0)
						{
							list.Add(item);
						}
					}
				}
				while (list.Count > 0 && AfflictionData.Instance.TravelFatigueSoundTimer <= 0f)
				{
					PartyMemberAI partyMemberAI = list[OEIRandom.Index(list.Count)];
					PlayPartyMemberFatigueSound(partyMemberAI);
					list.Remove(partyMemberAI);
				}
				if (list != null)
				{
					list.Clear();
					list = null;
				}
			}
			s_PlayFatigueSoundWhenNotLoading = false;
		}
		if (m_stackTracker != null)
		{
			Dictionary<int, Dictionary<int, StatusEffect>>.Enumerator enumerator2 = m_stackTracker.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Dictionary<int, StatusEffect>.Enumerator enumerator3 = enumerator2.Current.Value.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							StatusEffect value = enumerator3.Current.Value;
							if (value != null && !value.HasBeenApplied)
							{
								value.Unsuppress();
								value.ApplyEffect(base.gameObject);
							}
						}
					}
					finally
					{
						enumerator3.Dispose();
					}
				}
			}
			finally
			{
				enumerator2.Dispose();
			}
		}
		for (int l = 0; l < m_statusEffects.Count; l++)
		{
			m_statusEffects[l].Update();
		}
		if (IsPartyMember && (bool)GameCursor.CharacterUnderCursor && (bool)m_equipment)
		{
			PartyMemberAI component = GetComponent<PartyMemberAI>();
			if ((bool)component && component.Selected)
			{
				for (int m = 0; m < m_abilities.Count; m++)
				{
					FlankingAbility flankingAbility = m_abilities[m] as FlankingAbility;
					if ((bool)flankingAbility && flankingAbility.CanSneakAttackEnemy(GameCursor.CharacterUnderCursor, m_equipment.PrimaryAttack))
					{
						if (GameCursor.DesiredCursor == GameCursor.CursorType.Attack)
						{
							GameCursor.DesiredCursor = GameCursor.CursorType.AttackAdvantage;
						}
						GameState.s_playerCharacter.WantsAttackAdvantageCursor = true;
						break;
					}
				}
			}
		}
		if (IsPartyMember && !GameState.InCombat && !TimeController.Instance.Paused)
		{
			int maxLevelCanLevelUpTo = GetMaxLevelCanLevelUpTo();
			if (maxLevelCanLevelUpTo > Level && maxLevelCanLevelUpTo > m_NotifiedLevel)
			{
				GameUtilities.LaunchEffect(InGameHUD.Instance.LevelUpVfx, 1f, base.transform, null);
				UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(807), base.gameObject, 2.5f);
				m_NotifiedLevel = GetMaxLevelCanLevelUpTo();
			}
		}
		if (DebugStats)
		{
			Faction component2 = GetComponent<Faction>();
			if (component2 != null && component2.MousedOver)
			{
				UIDebug.Instance.SetText("Character Stats Debug", GetCharacterStatsDebugOutput(), Color.cyan);
				UIDebug.Instance.SetTextPosition("Character Stats Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
			}
		}
	}

	public string GetCharacterStatsDebugOutput()
	{
		string text = "-- Character Stats Debug --";
		text = text + "\nLevel:" + ScaledLevel;
		text = text + "\nEngageable Level: " + MinimumLevelThatCanEngageMe();
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i] != null && m_statusEffects[i].Origin == null)
			{
				num++;
			}
		}
		if (num > 0)
		{
			text = text + "\n\n-- Status Effects with Problems --" + m_abilities.Count;
			for (int j = 0; j < m_statusEffects.Count; j++)
			{
				if (m_statusEffects[j] != null && m_statusEffects[j].Origin == null)
				{
					text = text + "\n " + m_statusEffects[j].GetDisplayName() + m_statusEffects[j].Params.GetString();
				}
			}
		}
		text = text + "\n\n-- Abilities --" + m_abilities.Count;
		for (int k = 0; k < m_abilities.Count; k++)
		{
			if (m_abilities[k] != null)
			{
				text = text + "\n " + m_abilities[k].ToString();
				if (m_abilities[k].Activated)
				{
					text += "(Activated)";
				}
			}
		}
		text = text + "\n\n-- Status Effects --" + m_statusEffects.Count;
		for (int l = 0; l < m_statusEffects.Count; l++)
		{
			if (m_statusEffects[l] != null)
			{
				text = text + "\n " + m_statusEffects[l].GetDisplayName() + m_statusEffects[l].Params.GetString();
			}
		}
		Health component = GetComponent<Health>();
		AttackBase currentAttack = GetComponent<AIController>().CurrentAttack;
		text = text + "\n\n-- Properties --\n Health: " + component.CurrentHealth + " Endurance: " + component.CurrentStamina + "\n Fatigue: " + CurrentFatigueLevel.ToString() + "\n Deflection: " + GetDefenseString(DefenseType.Deflect) + "\n Accuracy: " + CalculateAccuracy(null, null);
		text = ((!(currentAttack != null)) ? (text + "\n Attack Speed: 0.0") : (text + "\n Attack Speed: " + currentAttack.CalculateAttackSpeed()));
		text = text + "\n Attack Speed Mult: " + StatAttackSpeedMultiplier;
		text = text + "\n Recovery: " + RecoveryTimer;
		return text + "\n Total Recovery: " + TotalRecoveryTime;
	}

	private StatusEffect GetTrackedEffect(StatusEffect.NonstackingType nonType, int stackingKey)
	{
		StatusEffect value = null;
		if (m_stackTracker != null)
		{
			Dictionary<int, StatusEffect> value2 = null;
			m_stackTracker.TryGetValue((int)nonType, out value2);
			value2?.TryGetValue(stackingKey, out value);
		}
		return value;
	}

	private void AddTrackedEffect(StatusEffect effect)
	{
		if (m_stackTracker != null)
		{
			Dictionary<int, StatusEffect> value = null;
			m_stackTracker.TryGetValue((int)effect.NonstackingEffectType, out value);
			int stackingKey = effect.GetStackingKey();
			if (value == null)
			{
				value = s_statusEffectDictionaryPool.Allocate();
				m_stackTracker.Add((int)effect.NonstackingEffectType, value);
				value.Add(stackingKey, effect);
			}
			else if (value.ContainsKey(stackingKey))
			{
				value[stackingKey] = effect;
			}
			else
			{
				value.Add(stackingKey, effect);
			}
		}
	}

	private void ClearStackTracker()
	{
		if (m_stackTracker == null)
		{
			return;
		}
		foreach (KeyValuePair<int, Dictionary<int, StatusEffect>> item in m_stackTracker)
		{
			item.Value.Clear();
			s_statusEffectDictionaryPool.Free(item.Value);
		}
		m_stackTracker.Clear();
	}

	public static float GetStatHealthStaminaMultiplier(int constitution)
	{
		return 1f + (float)(constitution - 10) * 0.05f;
	}

	private void PlayPartyFatigueSoundIfAble()
	{
		if (AfflictionData.Instance.TravelFatigueSoundTimer <= 0f && GameGlobalVariables.IsPlayerWatcher())
		{
			s_PlayFatigueSoundWhenNotLoading = true;
		}
	}

	private bool PlayPartyMemberFatigueSound(PartyMemberAI partyMemberToSpeak)
	{
		if ((bool)partyMemberToSpeak && !GameState.IsLoading && AfflictionData.Instance.TravelFatigueSoundTimer <= 0f && GameGlobalVariables.IsPlayerWatcher() && SoundSet.TryPlayVoiceEffectWithLocalCooldown(partyMemberToSpeak.gameObject, SoundSet.SoundAction.Rest, SoundSet.s_MediumVODelay, forceInterrupt: true))
		{
			AfflictionData.Instance.TravelFatigueSoundTimer = fatigueSoundDelay;
			return true;
		}
		return false;
	}

	public FatigueLevel GetFatigueLevel()
	{
		return CurrentFatigueLevel;
	}

	public void IncreaseFatigueLevel()
	{
		AdjustFatigueLevel(1);
	}

	public void AdjustFatigueLevel(int adjustment)
	{
		CurrentFatigueLevel = (FatigueLevel)Mathf.Clamp((int)(CurrentFatigueLevel + adjustment), 0, 3);
	}

	public static float GetSecondWindAthleticsBonus(int skill)
	{
		return (float)skill * 5f;
	}

	public void NoiseUpdate(float seconds)
	{
		if (m_noiseTimer > 0f)
		{
			m_noiseTimer -= seconds;
			if (m_noiseTimer <= 0f)
			{
				m_noiseLevel = NoiseLevelType.Quiet;
				m_noiseTimer = 0f;
			}
		}
	}

	public void DetectUpdate(float deltaTime)
	{
		if (!IsPartyMember)
		{
			return;
		}
		m_detectTimer -= deltaTime;
		if (!(m_detectTimer <= 0f))
		{
			return;
		}
		Transform transform = base.transform;
		for (int i = 0; i < Detectable.ActiveDetectables.Count; i++)
		{
			Detectable detectable = Detectable.ActiveDetectables[i];
			if (!detectable || detectable.Detected)
			{
				continue;
			}
			float num = DetectionRange(detectable);
			if (num > 0f)
			{
				Vector3 vector = detectable.transform.position;
				Usable component = detectable.GetComponent<Usable>();
				if (component != null)
				{
					vector = component.GetClosestInteractionPoint(transform.position);
					num += component.UsableRadius;
				}
				if ((transform.position - vector).sqrMagnitude <= num * num && GameUtilities.LineofSight(transform.position, vector, 1f, includeDynamics: false))
				{
					detectable.Detect(base.gameObject);
					SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.DetectableFound, SoundSet.s_LongVODelay, forceInterrupt: false);
				}
			}
		}
		m_detectTimer = 0.1f;
	}

	public float DetectionRange(Detectable d)
	{
		int num = CalculateSkill(SkillType.Mechanics);
		if (!Stealth.IsInStealthMode(base.gameObject))
		{
			num -= 4;
		}
		num++;
		if (d != null)
		{
			num -= d.GetDifficulty();
		}
		return num;
	}

	public int CalculateAccuracyForUi(AttackBase attack, GenericAbility ability, GameObject enemy)
	{
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.Attack = attack;
		damageInfo.AccuracyRating = CalculateAccuracy(attack, enemy);
		if ((bool)ability)
		{
			AttackRanged attackRanged = attack as AttackRanged;
			AttackMelee attackMelee = attack as AttackMelee;
			for (int i = 0; i < ability.StatusEffects.Length; i++)
			{
				switch (ability.StatusEffects[i].AffectsStat)
				{
				case StatusEffect.ModifiedStat.Accuracy:
					damageInfo.AccuracyRating += (int)ability.StatusEffects[i].Value;
					break;
				case StatusEffect.ModifiedStat.RangedAccuracy:
					if ((bool)attackRanged)
					{
						damageInfo.AccuracyRating += (int)ability.StatusEffects[i].Value;
					}
					break;
				case StatusEffect.ModifiedStat.MeleeAccuracy:
					if ((bool)attackMelee)
					{
						damageInfo.AccuracyRating += (int)ability.StatusEffects[i].Value;
					}
					break;
				case StatusEffect.ModifiedStat.UnarmedAccuracy:
					if ((bool)attackMelee && attackMelee.Unarmed)
					{
						damageInfo.AccuracyRating += (int)ability.StatusEffects[i].Value;
					}
					break;
				}
			}
		}
		if (!attack || !attack.HasImpactCountRemaining)
		{
			if ((bool)attack)
			{
				damageInfo.AccuracyRating += (int)attack.FindEquipmentLaunchAccuracyBonus();
			}
			for (int j = 0; j < ActiveAbilities.Count; j++)
			{
				if ((bool)ActiveAbilities[j])
				{
					ActiveAbilities[j].UIGetBonusAccuracyOnAttack(base.gameObject, damageInfo);
				}
			}
		}
		return damageInfo.AccuracyRating;
	}

	public int CalculateAccuracy(AttackBase attack, GameObject enemy)
	{
		CharacterStats characterStats = null;
		if ((bool)enemy)
		{
			characterStats = enemy.GetComponent<CharacterStats>();
		}
		AIController aIController = null;
		if ((bool)enemy)
		{
			aIController = enemy.GetComponent<AIController>();
		}
		int statBonusAccuracy = StatBonusAccuracy;
		statBonusAccuracy += AccuracyBonusFromLevel;
		statBonusAccuracy = ((!(attack is AttackRanged)) ? (statBonusAccuracy + MeleeAccuracyBonus) : (statBonusAccuracy + RangedAccuracyBonus));
		if ((((bool)attack && ((bool)attack.AbilityOrigin || (bool)attack.TriggeringAbility)) || (attack == null && CharacterClass == Class.Chanter)) && CharacterClass != Class.PlayerTrap)
		{
			statBonusAccuracy += ScaledLevel;
		}
		if ((bool)attack)
		{
			statBonusAccuracy += attack.AccuracyBonusTotal;
		}
		statBonusAccuracy += GetAccuracyBonus(enemy, attack);
		if ((bool)attack)
		{
			Weapon component = attack.gameObject.GetComponent<Weapon>();
			if (component != null && component.DurabilityState == Equippable.DurabilityStateType.Damaged)
			{
				statBonusAccuracy += -10;
			}
		}
		if ((bool)characterStats)
		{
			if (characterStats.CharacterRace == Race.Vessel)
			{
				statBonusAccuracy += VesselAccuracyBonus;
			}
			else if (characterStats.CharacterRace == Race.Beast)
			{
				statBonusAccuracy += BeastAccuracyBonus;
			}
			else if (characterStats.CharacterRace == Race.Wilder)
			{
				statBonusAccuracy += WilderAccuracyBonus;
			}
			else if (characterStats.CharacterRace == Race.Primordial)
			{
				statBonusAccuracy += PrimordialAccuracyBonus;
			}
			for (int i = 0; i < characterStats.ActiveStatusEffects.Count; i++)
			{
				statBonusAccuracy += characterStats.ActiveStatusEffects[i].AdjustAttackerAccuracy(base.gameObject, enemy, attack);
			}
		}
		if ((bool)attack && attack.IsDisengagementAttack)
		{
			statBonusAccuracy += DisengagementAccuracyBonus;
			statBonusAccuracy += DifficultyDisengagementAccuracyBonus;
		}
		if ((bool)aIController)
		{
			Vector3 normalized = (enemy.transform.position - base.transform.position).normalized;
			float num = Vector3.Dot(enemy.transform.forward, normalized);
			if (aIController.CurrentTarget == null && num > 0f)
			{
				statBonusAccuracy += FlankedAccuracyBonus;
			}
		}
		Equipment component2 = base.gameObject.GetComponent<Equipment>();
		if (component2 != null)
		{
			if ((bool)GetComponent<BestiaryReference>())
			{
				Shield shield = component2.DefaultEquippedItems.Shield;
				if (shield != null)
				{
					statBonusAccuracy += shield.AccuracyBonus;
				}
				else if ((!attack || !attack.AbilityOrigin) && !component2.DefaultEquippedItems.TwoHandedWeapon && !component2.DefaultEquippedItems.DualWielding)
				{
					statBonusAccuracy += AttackData.Instance.Single1HWeapNoShieldAccuracyBonus;
				}
			}
			else
			{
				Shield equippedShield = component2.EquippedShield;
				if (equippedShield != null)
				{
					statBonusAccuracy += equippedShield.AccuracyBonus;
				}
				else if ((!attack || !attack.AbilityOrigin) && !component2.TwoHandedWeapon && !component2.DualWielding)
				{
					statBonusAccuracy += AttackData.Instance.Single1HWeapNoShieldAccuracyBonus;
				}
			}
		}
		if ((bool)enemy)
		{
			statBonusAccuracy += NearestAllyWithSharedTarget(enemy);
		}
		return (int)((float)statBonusAccuracy + DifficultyStatBonus);
	}

	private int NearestAllyWithSharedTarget(GameObject enemy)
	{
		GameObject[] array = GameUtilities.FriendsInRange(base.transform.position, 30f, base.gameObject, includeUnconscious: false);
		if (array == null)
		{
			return 0;
		}
		Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject != null))
			{
				continue;
			}
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			if (component != null && component.NearestAllyWithSharedTargetAccuracyBonus != 0)
			{
				AIController component2 = gameObject.GetComponent<AIController>();
				if ((bool)component2 && component2.CurrentTarget == enemy)
				{
					dictionary.Add(gameObject, component.NearestAllyWithSharedTargetAccuracyBonus);
				}
			}
		}
		if (dictionary.Count == 0)
		{
			return 0;
		}
		foreach (KeyValuePair<GameObject, int> item in dictionary.OrderByDescending(delegate(KeyValuePair<GameObject, int> pair)
		{
			KeyValuePair<GameObject, int> keyValuePair = pair;
			return keyValuePair.Value;
		}))
		{
			GameObject key = item.Key;
			GameObject gameObject2 = base.gameObject;
			float num = (key.transform.position - base.transform.position).sqrMagnitude;
			array2 = array;
			foreach (GameObject gameObject3 in array2)
			{
				if (gameObject3 == null || gameObject3 == key)
				{
					continue;
				}
				AIController component3 = gameObject3.GetComponent<AIController>();
				if ((bool)component3 && !(component3.CurrentTarget != enemy))
				{
					float sqrMagnitude = (key.transform.position - gameObject3.transform.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						gameObject2 = gameObject3;
						num = sqrMagnitude;
					}
				}
			}
			if (gameObject2 == base.gameObject)
			{
				return item.Value;
			}
		}
		return 0;
	}

	public bool TryGetRedirectDefense(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, out int defense)
	{
		string origin;
		return TryGetRedirectDefense(defenseType, attack, enemy, isSecondary, out defense, out origin);
	}

	public bool TryGetRedirectDefense(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, out int defense, out string origin)
	{
		if (HasStatusEffectOfType(StatusEffect.ModifiedStat.MindwebEffect))
		{
			GenericAbility abilityOrigin = FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.MindwebEffect).AbilityOrigin;
			Mindweb mindweb = abilityOrigin as Mindweb;
			if ((bool)mindweb)
			{
				int num = mindweb.CalculateDefense(defenseType, attack, enemy, isSecondary);
				if (num > CalculateDefense(defenseType, attack, enemy, isSecondary, allowRedirect: false))
				{
					defense = num;
					origin = mindweb.Name();
					return true;
				}
			}
			else
			{
				Debug.LogError("MindwebEffect can only be used by a Mindweb ability (bad ability: '" + abilityOrigin.name + "')");
			}
		}
		origin = "";
		defense = 0;
		return false;
	}

	public int CalculateDefense(DefenseType defenseType)
	{
		return CalculateDefense(defenseType, null, null, isSecondary: false, allowRedirect: true);
	}

	public int CalculateDefense(DefenseType defenseType, AttackBase attack, GameObject enemy)
	{
		return CalculateDefense(defenseType, attack, enemy, isSecondary: false, allowRedirect: true);
	}

	public int CalculateDefense(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary)
	{
		return CalculateDefense(defenseType, attack, enemy, isSecondary, allowRedirect: true);
	}

	public int CalculateDefense(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, bool allowRedirect)
	{
		if (allowRedirect && TryGetRedirectDefense(defenseType, attack, enemy, isSecondary, out var defense))
		{
			return defense;
		}
		int num = GetDefenseScore(defenseType, enemy) + DefenseBonusFromLevel;
		num += GetDefenseBonus(defenseType, enemy);
		switch (defenseType)
		{
		case DefenseType.Deflect:
		{
			num += GetStatBonusDeflection(Resolve);
			Equipment component2 = base.gameObject.GetComponent<Equipment>();
			num += GetShieldDeflectBonus(component2);
			AttackRanged attackRanged = attack as AttackRanged;
			if (!attackRanged || !attackRanged.VeilPiercing)
			{
				num += VeilDeflectionBonus;
			}
			break;
		}
		case DefenseType.Fortitude:
			num += GetStatDefenseTypeBonus(Might) + GetStatDefenseTypeBonus(Constitution);
			break;
		case DefenseType.Reflex:
		{
			num += GetStatDefenseTypeBonus(Dexterity) + GetStatDefenseTypeBonus(Perception);
			Equipment component = base.gameObject.GetComponent<Equipment>();
			num += GetShieldReflexBonus(component);
			break;
		}
		case DefenseType.Will:
			num += GetStatDefenseTypeBonus(Intellect) + GetStatDefenseTypeBonus(Resolve);
			break;
		default:
			num += 50;
			break;
		case DefenseType.None:
			break;
		}
		if (attack != null)
		{
			num += GetDefenseBonus(defenseType, attack, isSecondary);
		}
		AnimationController component3 = base.gameObject.GetComponent<AnimationController>();
		if (component3 != null)
		{
			if (component3.CurrentReaction == AnimationController.ReactionType.Knockdown)
			{
				num += WhileKnockeddownDefenseBonus;
			}
			if (component3.CurrentReaction == AnimationController.ReactionType.Stun)
			{
				num += WhileStunnedDefenseBonus;
			}
		}
		if (this.OnDefenseAdjustment != null)
		{
			int defense2 = 0;
			this.OnDefenseAdjustment(defenseType, attack, enemy, isSecondary, ref defense2);
			num += defense2;
		}
		return (int)((float)num + DifficultyStatBonus);
	}

	public bool CalculateIsImmune(DefenseType defenseType, AttackBase attack, GameObject enemy)
	{
		return CalculateIsImmune(defenseType, attack, enemy, isSecondary: false);
	}

	public bool CalculateIsImmune(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary)
	{
		bool isImmune = false;
		if (this.OnCheckImmunity != null)
		{
			this.OnCheckImmunity(defenseType, attack, enemy, isSecondary, ref isImmune);
		}
		return isImmune;
	}

	private int GetDefenseBonus(DefenseType defenseType, AttackBase attack, bool isSecondary)
	{
		int num = 0;
		if (attack.HasStatusEffect(StatusEffect.ModifiedStat.Stunned) || attack.HasStatusEffect(StatusEffect.ModifiedStat.CanStun))
		{
			num += StunDefenseBonus;
		}
		if (attack.HasStatusEffect(StatusEffect.ModifiedStat.KnockedDown))
		{
			num += KnockdownDefenseBonus;
		}
		if (attack.HasKeyword("poison"))
		{
			num += PoisonDefenseBonus;
		}
		else if (isSecondary && attack.HasAfflictionWithKeyword("poison"))
		{
			num += PoisonDefenseBonus;
		}
		if (attack.HasKeyword("disease"))
		{
			num += DiseaseDefenseBonus;
		}
		else if (isSecondary && attack.HasAfflictionWithKeyword("disease"))
		{
			num += DiseaseDefenseBonus;
		}
		if (attack.PushDistance != 0f || attack.HasStatusEffect(StatusEffect.ModifiedStat.Push))
		{
			num += PushDefenseBonus;
		}
		if (attack.IsDisengagementAttack)
		{
			num += DisengagementDefenseBonus;
		}
		if (attack.AbilityOrigin != null && attack.AbilityOrigin is GenericSpell)
		{
			num += SpellDefenseBonus;
		}
		if (attack is AttackRanged && defenseType == DefenseType.Deflect)
		{
			num += RangedDeflectionBonus;
		}
		if (m_equipment != null && m_equipment.TwoHandedWeapon)
		{
			num += TwoHandedDeflectionBonus;
		}
		if (attack is AttackAOE)
		{
			num += DefensiveBondBonus;
		}
		return num;
	}

	public int GetShieldReflexBonus(Equipment equipment)
	{
		int num = 0;
		if (equipment != null)
		{
			Shield equippedShield = equipment.EquippedShield;
			if (equippedShield != null)
			{
				num += equippedShield.ReflexBonus;
				if (HasStatusEffectOfType(StatusEffect.ModifiedStat.ShieldDeflectionExtendToReflex))
				{
					num += GetShieldDeflectBonus(equipment);
				}
			}
		}
		return num;
	}

	public int GetShieldDeflectBonus(Equipment equip)
	{
		int num = 0;
		if (equip != null)
		{
			Shield equippedShield = equip.EquippedShield;
			if (equippedShield != null)
			{
				num += equippedShield.DeflectBonus;
				num += BonusShieldDeflection;
			}
		}
		return num;
	}

	public bool IsEnemyDistant(GameObject enemy)
	{
		return (base.transform.position - enemy.transform.position).sqrMagnitude > 16f;
	}

	public int GetAttackerToHitRollOverride(int roll)
	{
		if (AttackerToHitRollOverride == -1)
		{
			return roll;
		}
		return AttackerToHitRollOverride;
	}

	public void SetAttackerToHitRollOverride(int roll)
	{
		AttackerToHitRollOverride = roll;
	}

	public DamageInfo AdjustDamageForUi(DamageInfo damage)
	{
		float num = ((!damage.Attack || !damage.Attack.IgnoreCharacterStats) ? StatDamageHealMultiplier : 1f);
		damage.DamageMult(num);
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].Params.AffectsStat == StatusEffect.ModifiedStat.BonusDamageMult)
			{
				damage.DamageMult(m_statusEffects[i].CurrentAppliedValue);
			}
			else if (m_statusEffects[i].Params.AffectsStat == StatusEffect.ModifiedStat.BonusDamageMultWithImplements && damage.Attack != null)
			{
				Weapon component = damage.Attack.GetComponent<Weapon>();
				if (component != null && component.IsImplement)
				{
					damage.DamageMult(m_statusEffects[i].CurrentAppliedValue);
				}
			}
		}
		for (int j = 0; j < ActiveStatusEffects.Count; j++)
		{
			if (ActiveStatusEffects[j].Applied)
			{
				damage.DamageAdd(ActiveStatusEffects[j].AdjustDamage(base.gameObject, damage.Target, damage.Attack) * num);
				damage.DamageMult(ActiveStatusEffects[j].AdjustDamageMultiplier(base.gameObject, damage.Target, damage.Attack));
			}
		}
		if ((bool)damage.Attack)
		{
			Equippable component2 = damage.Attack.GetComponent<Equippable>();
			if ((bool)component2)
			{
				component2.AdjustDamageForUi(base.gameObject, damage);
			}
		}
		WeaponSpecializationData.AddWeaponSpecialization(this, damage);
		for (int k = 0; k < ActiveAbilities.Count; k++)
		{
			if ((bool)ActiveAbilities[k])
			{
				ActiveAbilities[k].UIAdjustDamageOnAttack(base.gameObject, damage);
			}
		}
		return damage;
	}

	public void AdjustDamageDealt(GameObject enemy, DamageInfo damage, bool testing)
	{
		float num = ((!damage.Attack || !damage.Attack.IgnoreCharacterStats) ? StatDamageHealMultiplier : 1f);
		damage.DamageMult(num);
		if (!testing && this.OnPreDamageDealt != null)
		{
			this.OnPreDamageDealt(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
		if (!testing && this.OnAddDamage != null)
		{
			this.OnAddDamage(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
		int roll = OEIRandom.DieRoll(100);
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		roll = component.GetAttackerToHitRollOverride(roll);
		int num2 = CalculateAccuracy(damage.Attack, enemy);
		bool immune = component.CalculateIsImmune(damage.DefendedBy, damage.Attack, base.gameObject);
		int num3 = component.CalculateDefense(damage.DefendedBy, damage.Attack, base.gameObject);
		if (damage.DefendedBy != DefenseType.None)
		{
			int hitValue = roll + num2 - num3;
			ComputeHitAdjustment(hitValue, component, damage);
			if (!testing && this.OnAttackRollCalculated != null)
			{
				this.OnAttackRollCalculated(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
			}
			if (damage.IsCriticalHit)
			{
				float num4 = CriticalHitMultiplier;
				Health component2 = enemy.GetComponent<Health>();
				if (component2 != null && component2.StaminaPercentage < 0.1f)
				{
					num4 += CritHitDamageMultiplierBonusEnemyBelow10Percent;
				}
				damage.DamageMult(num4);
			}
			else if (damage.IsGraze)
			{
				damage.DamageMult(GrazeMultiplier);
			}
			else if (damage.IsMiss)
			{
				damage.DamageMult(0f);
			}
		}
		WeaponSpecializationData.AddWeaponSpecialization(this, damage);
		damage.AccuracyRating = num2;
		damage.DefenseRating = num3;
		damage.Immune = immune;
		damage.RawRoll = roll;
		if (!testing && damage.Immune)
		{
			UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2188), enemy);
			if (IsPartyMember)
			{
				SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.TargetImmune, SoundSet.s_LongVODelay, forceInterrupt: false);
			}
		}
		if (!testing && this.OnAdjustCritGrazeMiss != null)
		{
			this.OnAdjustCritGrazeMiss(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
		if (!damage.IsMiss)
		{
			for (int i = 0; i < ActiveStatusEffects.Count; i++)
			{
				if (ActiveStatusEffects[i].Applied)
				{
					damage.DamageAdd(ActiveStatusEffects[i].AdjustDamage(base.gameObject, enemy, damage.Attack) * num);
					damage.DamageMult(ActiveStatusEffects[i].AdjustDamageMultiplier(base.gameObject, enemy, damage.Attack));
				}
			}
			for (int j = 0; j < BonusDamage.Length; j++)
			{
				if (BonusDamage[j] != 0f)
				{
					DamagePacket.DamageProcType item = new DamagePacket.DamageProcType((DamagePacket.DamageType)j, BonusDamage[j]);
					damage.Damage.DamageProc.Add(item);
				}
			}
			AddBonusDamagePerType(damage);
			AddBonusDamagePerRace(damage, component);
			if (damage.Attack != null)
			{
				Equippable component3 = damage.Attack.GetComponent<Equippable>();
				if ((bool)component3)
				{
					if (component3 is Weapon && !(damage.Attack is AttackMelee) && enemy != null && !IsEnemyDistant(enemy))
					{
						damage.DamageMult(BonusRangedWeaponCloseEnemyDamageMult);
					}
					component3.ApplyItemModDamageProcs(damage);
				}
			}
		}
		ComputeInterrupt(component, damage);
		if (!testing && IsPartyMember)
		{
			if ((bool)component)
			{
				component.RevealDefense(damage.DefendedBy);
				component.RevealDT(damage.Damage.Type);
				foreach (DamagePacket.DamageProcType item2 in damage.Damage.DamageProc)
				{
					component.RevealDT(item2.Type);
				}
			}
			if (damage.DefenseRating >= damage.AccuracyRating + 50 || damage.Immune)
			{
				GameState.AutoPause(AutoPauseOptions.PauseEvent.ExtraordinaryDefence, base.gameObject, enemy);
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_GETS_DEFENSE_TOO_HIGH);
			}
		}
		if (!testing && this.OnPostDamageDealt != null)
		{
			this.OnPostDamageDealt(base.gameObject, new CombatEventArgs(damage, base.gameObject, enemy));
		}
	}

	private void AddBonusDamagePerRace(DamageInfo damage, CharacterStats enemyStats)
	{
		if ((int)enemyStats.CharacterRace < BonusDamagePerRace.Length && BonusDamagePerRace[(int)enemyStats.CharacterRace] != 0f)
		{
			damage.DamageMult(GetBonusDamagePerRaceMultiplier(enemyStats.CharacterRace));
		}
	}

	public float GetBonusDamagePerRaceMultiplier(Race enemyRace)
	{
		float num = 0f;
		if ((int)enemyRace < BonusDamagePerRace.Length)
		{
			num += BonusDamagePerRace[(int)enemyRace] / 100f;
		}
		return num + 1f;
	}

	private void AddBonusDamagePerType(DamageInfo damage)
	{
		damage.DamageMult(GetBonusDamagePerType(damage.Damage.Type));
	}

	public float GetBonusDamagePerType(DamagePacket.DamageType type)
	{
		float num = 0f;
		if ((int)type < BonusDamagePerType.Length && BonusDamagePerType[(int)type] != 0f)
		{
			num += BonusDamagePerType[(int)type] / 100f;
		}
		if (type == DamagePacket.DamageType.All)
		{
			float[] bonusDamagePerType = BonusDamagePerType;
			foreach (float num2 in bonusDamagePerType)
			{
				if (num2 != 0f)
				{
					num += num2 / 100f;
				}
			}
		}
		return num + 1f;
	}

	private void ComputeHitAdjustment(int hitValue, CharacterStats enemyStats, DamageInfo damage)
	{
		bool flag = damage.Attack != null && (damage.Attack.AbilityOrigin == null || damage.Attack.IsAutoAttack());
		if ((float)hitValue >= CritThreshhold)
		{
			damage.OriginalHitType = HitType.CRIT;
			float bonusCritToHitPercent = BonusCritToHitPercent;
			if (!flag || !GetBool(bonusCritToHitPercent))
			{
				damage.IsCriticalHit = true;
			}
		}
		else if ((float)hitValue < MinimumRollToGraze)
		{
			damage.OriginalHitType = HitType.MISS;
			float bonusMissToGrazePercent = BonusMissToGrazePercent;
			if (flag && GetBool(bonusMissToGrazePercent))
			{
				damage.IsGraze = true;
			}
			else
			{
				damage.IsMiss = true;
			}
		}
		else if ((float)hitValue <= GrazeThreshhold)
		{
			damage.OriginalHitType = HitType.GRAZE;
			float bonusGrazeToHitPercent = GetBonusGrazeToHitPercent(damage);
			if (!flag || !GetBool(bonusGrazeToHitPercent))
			{
				if (flag && GetBool(BonusGrazeToMissPercent))
				{
					damage.IsMiss = true;
				}
				else
				{
					damage.IsGraze = true;
				}
			}
		}
		else
		{
			float num = GetBonusHitToCritPercent(damage);
			Health component = enemyStats.GetComponent<Health>();
			if ((bool)component && component.StaminaPercentage < 0.1f)
			{
				num += BonusHitToCritPercentEnemyBelow10Percent;
			}
			num += enemyStats.DifficultyHitToCritBonusChance;
			if (flag && GetBool(num))
			{
				damage.IsCriticalHit = true;
			}
			else if (GetBool(BonusHitToCritPercentAll))
			{
				damage.IsCriticalHit = true;
			}
			else if (flag && GetBool(BonusHitToGrazePercent))
			{
				damage.IsGraze = true;
			}
		}
		if (damage.OriginalHitType != damage.HitType)
		{
			damage.AttackerChangedToHitType = damage.HitType;
		}
		if (!(enemyStats != null))
		{
			return;
		}
		if (enemyStats.EvadeEverything)
		{
			damage.OriginalHitType = HitType.MISS;
			damage.IsMiss = true;
		}
		if (damage.IsCriticalHit)
		{
			if (GetBool(enemyStats.EnemyCritToHitPercent))
			{
				damage.IsCriticalHit = false;
			}
		}
		else if (damage.IsPlainHit)
		{
			float num2 = enemyStats.EnemyHitToGrazePercent;
			if (damage.DefendedBy == DefenseType.Deflect || damage.DefendedBy == DefenseType.Reflex)
			{
				num2 += enemyStats.EnemyDeflectReflexHitToGrazePercent;
			}
			if (GetBool(num2))
			{
				damage.IsGraze = true;
			}
			else if (damage.DefendedBy == DefenseType.Reflex && GetBool(enemyStats.EnemyReflexHitToGrazePercent))
			{
				damage.IsGraze = true;
			}
		}
		else if (damage.IsGraze)
		{
			float num3 = enemyStats.EnemyGrazeToMissPercent;
			if (damage.DefendedBy == DefenseType.Fortitude || damage.DefendedBy == DefenseType.Will)
			{
				num3 += enemyStats.EnemyFortitudeWillHitToGrazePercent;
			}
			if (GetBool(num3))
			{
				damage.IsMiss = true;
			}
			else if (damage.DefendedBy == DefenseType.Reflex && GetBool(enemyStats.EnemyReflexGrazeToMissPercent))
			{
				damage.IsMiss = true;
			}
		}
	}

	private float GetBonusGrazeToHitPercent(DamageInfo damage)
	{
		float num = BonusGrazeToHitPercent;
		if (m_equipment != null && damage.Attack == m_equipment.PrimaryAttack && !m_equipment.TwoHandedWeapon && !m_equipment.DualWielding && m_equipment.PrimaryAttack is AttackMelee && m_equipment.PrimaryAttack == damage.Attack && m_equipment.EquippedShield == null)
		{
			num += BonusGrazeToHitPercentMeleeOneHanded;
		}
		return num;
	}

	private float GetBonusHitToCritPercent(DamageInfo damage)
	{
		float num = BonusHitToCritPercent;
		if (m_equipment != null && damage.Attack == m_equipment.PrimaryAttack && !m_equipment.TwoHandedWeapon && !m_equipment.DualWielding && m_equipment.PrimaryAttack is AttackMelee && m_equipment.PrimaryAttack == damage.Attack && m_equipment.EquippedShield == null)
		{
			num += BonusHitToCritPercentMeleeOneHanded;
		}
		return num;
	}

	public int ComputeBaseConcentration()
	{
		return ComputeBaseConcentration(Resolve);
	}

	public static int ComputeBaseConcentration(int attribute)
	{
		return (attribute - 10) * 3;
	}

	public int ComputeBaseInterrupt()
	{
		return ComputeBaseInterrupt(Perception);
	}

	public static int ComputeBaseInterrupt(int attribute)
	{
		return (attribute - 10) * 3;
	}

	public float ComputeInterruptHelper()
	{
		return (float)ComputeBaseInterrupt() + GetStatusEffectValueSum(StatusEffect.ModifiedStat.InterruptBonus);
	}

	public float ComputeConcentrationHelper()
	{
		return AttackData.Instance.BaseConcentration + GetStatusEffectValueSum(StatusEffect.ModifiedStat.ConcentrationBonus) + (float)ComputeBaseConcentration();
	}

	private void ComputeInterrupt(CharacterStats enemyStats, DamageInfo damage)
	{
		if (damage.IsMiss || damage.Attack == null || damage.Attack.BaseInterrupt == AttackData.InterruptScale.None || !damage.AttackIsHostile || (enemyStats != null && enemyStats.HasInterruptBlockingAffliction()))
		{
			return;
		}
		float num = ComputeInterruptHelper();
		if (damage.IsCriticalHit)
		{
			num += 25f;
		}
		else if (damage.IsGraze)
		{
			num -= 25f;
		}
		float num2 = ((!(enemyStats != null)) ? AttackData.Instance.BaseConcentration : enemyStats.ComputeConcentrationHelper());
		int num3 = OEIRandom.DieRoll(100);
		if ((float)num3 + num > num2)
		{
			damage.Interrupts = true;
			if (!damage.TargetPreviouslyDead && (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(base.transform.position) || FogOfWar.Instance.PointVisible(enemyStats.transform.position)))
			{
				Console.AddBatchedMessage(Console.Format(GUIUtils.GetTextWithLinks(1636), NameColored(enemyStats), NameColored(this)), Console.Format("{0}. {1}:{2} + {3} = {4} > {5}.", GUIUtils.Format(1567, (int)num), GUIUtils.GetText(426), num3, (int)num, (int)num + num3, GUIUtils.Format(1729, (int)num2)), AttackBase.GetMessageColor(damage.Owner.GetComponent<Faction>(), enemyStats.GetComponent<Faction>()), damage.Attack);
			}
		}
	}

	public bool HasInterruptBlockingAffliction()
	{
		if (AttackData.Instance.InterruptBlockingAfflictions == null)
		{
			return false;
		}
		Affliction[] interruptBlockingAfflictions = AttackData.Instance.InterruptBlockingAfflictions;
		foreach (Affliction aff in interruptBlockingAfflictions)
		{
			if (HasStatusEffectFromAffliction(aff))
			{
				return true;
			}
		}
		return false;
	}

	public DamageInfo ComputeSecondaryAttack(AttackBase attack, GameObject enemy, DefenseType defendedBy)
	{
		DamageInfo damageInfo = new DamageInfo(enemy, 0f, attack);
		damageInfo.AttackIsHostile = true;
		damageInfo.DefendedBy = defendedBy;
		if (defendedBy == DefenseType.None)
		{
			damageInfo.HitType = HitType.HIT;
		}
		else
		{
			CharacterStats component = enemy.GetComponent<CharacterStats>();
			if (component == null)
			{
				damageInfo.HitType = HitType.HIT;
			}
			else
			{
				int num = OEIRandom.DieRoll(100);
				int num2 = CalculateAccuracy(attack, enemy);
				int num3 = component.CalculateDefense(defendedBy, attack, base.gameObject, isSecondary: true);
				int hitValue = num + num2 - num3;
				ComputeHitAdjustment(hitValue, component, damageInfo);
				damageInfo.RawRoll = num;
				damageInfo.AccuracyRating = num2;
				damageInfo.DefenseRating = num3;
				damageInfo.Immune = component.CalculateIsImmune(defendedBy, attack, base.gameObject, isSecondary: true);
				if (damageInfo.Immune)
				{
					UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2188), enemy);
				}
			}
			RevealSecondaryDefense(enemy, defendedBy);
		}
		return damageInfo;
	}

	private void RevealSecondaryDefense(GameObject enemy, DefenseType defendedBy)
	{
		if (IsPartyMember)
		{
			CharacterStats component = enemy.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.RevealDefense(defendedBy);
			}
		}
	}

	public bool HasKeyword(string keyword)
	{
		if (m_keywords != null)
		{
			return m_keywords.Contains(keyword);
		}
		return false;
	}

	public string GetHitTypeChangeDescription(CharacterStats enemy, HitType from, HitType to)
	{
		IEnumerable<StatusEffect> enumerable = FindHitTypeStatusEffects(from, to);
		float num = enemy.DifficultyHitToCritBonusChance;
		foreach (StatusEffect item in enumerable)
		{
			num += item.CurrentAppliedValue;
		}
		float num2 = OEIRandom.Range(0f, num);
		num = 0f;
		foreach (StatusEffect item2 in enumerable)
		{
			num += item2.CurrentAppliedValue;
			if (num2 < num)
			{
				return item2.BundleName;
			}
		}
		return GUIUtils.GetText(2248);
	}

	public int GetDefenseScore(DefenseType defenseType, GameObject attacker)
	{
		int baseDefense = GetBaseDefense(defenseType);
		if (HasStatusEffectOfType(StatusEffect.ModifiedStat.SetBaseDefense))
		{
			foreach (StatusEffect item in FindStatusEffectsOfType(StatusEffect.ModifiedStat.SetBaseDefense))
			{
				if (item.Params.DefenseType == defenseType)
				{
					return (int)item.CurrentAppliedValue;
				}
			}
			return baseDefense;
		}
		return baseDefense;
	}

	public int GetBaseDefense(DefenseType defenseType)
	{
		return defenseType switch
		{
			DefenseType.Deflect => BaseDeflection, 
			DefenseType.Fortitude => BaseFortitude, 
			DefenseType.Reflex => BaseReflexes, 
			DefenseType.Will => BaseWill, 
			_ => 0, 
		};
	}

	public int GetDefenseBonus(DefenseType defenseType, GameObject attacker)
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].Applied)
			{
				num += m_statusEffects[i].AdjustDefense(attacker, base.gameObject, defenseType);
			}
		}
		return num;
	}

	public int GetAccuracyBonus(GameObject enemy, AttackBase attack)
	{
		int num = 0;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].Applied)
			{
				num += m_statusEffects[i].AdjustAccuracy(base.gameObject, enemy, attack);
			}
		}
		return num;
	}

	public static int GetStatDefenseTypeBonus(int stat)
	{
		return (stat - 10) * 2;
	}

	public int GetAttributeScore(AttributeScoreType attributeType)
	{
		int num = GetBaseAttribute(attributeType);
		if (HasStatusEffectOfType(StatusEffect.ModifiedStat.SetBaseAttribute))
		{
			foreach (StatusEffect item in FindStatusEffectsOfType(StatusEffect.ModifiedStat.SetBaseAttribute))
			{
				if (item != null && item.Params != null && item.Params.AttributeType == attributeType)
				{
					num = (int)item.CurrentAppliedValue;
					break;
				}
			}
		}
		num += GetAttributeBonus(attributeType) + RaceAbilityAdjustment[(int)CharacterRace, (int)attributeType] + CultureAbilityAdjustment[(int)CharacterCulture, (int)attributeType];
		if (!IsPartyMember && (bool)DifficultyScaling.Instance && (bool)m_ScaledContent)
		{
			num += DifficultyScaling.Instance.GetScaleAdditive(m_ScaledContent, (DifficultyScaling.ScaleData scaleData) => scaleData.CreatureAttributeBonus);
		}
		if (num < 1)
		{
			return 1;
		}
		return num;
	}

	public int GetBaseAttribute(AttributeScoreType attributeType)
	{
		switch (attributeType)
		{
		case AttributeScoreType.Resolve:
			return BaseResolve;
		case AttributeScoreType.Might:
			return BaseMight;
		case AttributeScoreType.Dexterity:
			return BaseDexterity;
		case AttributeScoreType.Intellect:
			return BaseIntellect;
		case AttributeScoreType.Constitution:
			return BaseConstitution;
		case AttributeScoreType.Perception:
			return BasePerception;
		default:
			Debug.LogError(string.Concat("GetBaseAttribute: no known attribute '", attributeType, "'."));
			return 0;
		}
	}

	public int GetAttributeBonus(AttributeScoreType attributeType)
	{
		switch (attributeType)
		{
		case AttributeScoreType.Resolve:
			return ResolveBonus;
		case AttributeScoreType.Might:
			return MightBonus;
		case AttributeScoreType.Dexterity:
			return DexterityBonus;
		case AttributeScoreType.Intellect:
			return IntellectBonus;
		case AttributeScoreType.Constitution:
			return ConstitutionBonus;
		case AttributeScoreType.Perception:
			return PerceptionBonus;
		default:
			Debug.LogError(string.Concat("GetAttributeBonus: no known attribute '", attributeType, "'."));
			return 0;
		}
	}

	public void AddAttributeBonus(AttributeScoreType attributeType, int bonus)
	{
		switch (attributeType)
		{
		case AttributeScoreType.Resolve:
			ResolveBonus += bonus;
			break;
		case AttributeScoreType.Might:
			MightBonus += bonus;
			break;
		case AttributeScoreType.Dexterity:
			DexterityBonus += bonus;
			break;
		case AttributeScoreType.Intellect:
			IntellectBonus += bonus;
			break;
		case AttributeScoreType.Constitution:
		{
			Health component = GetComponent<Health>();
			if ((bool)component)
			{
				float healthPercentage = component.HealthPercentage;
				float staminaPercentage = component.StaminaPercentage;
				ConstitutionBonus += bonus;
				component.HealthPercentage = healthPercentage;
				component.StaminaPercentage = staminaPercentage;
			}
			else
			{
				ConstitutionBonus += bonus;
			}
			break;
		}
		case AttributeScoreType.Perception:
			PerceptionBonus += bonus;
			break;
		default:
			Debug.LogError(string.Concat("AddAttributeBonus: no known attribute '", attributeType, "'."));
			break;
		}
	}

	public int GetBaseStat(StatusEffect.ModifiedStat stat)
	{
		AttributeScoreType attributeScoreType = StatusEffect.ModifiedStatToAttributeScoreType(stat);
		if (attributeScoreType != AttributeScoreType.Count)
		{
			return GetBaseAttribute(attributeScoreType);
		}
		SkillType skillType = StatusEffect.ModifiedStatToSkillType(stat);
		if (skillType != SkillType.Count)
		{
			return CalculateSkillLevel(skillType);
		}
		return 0;
	}

	public static float GetStatDamageHealMultiplier(int might)
	{
		return 1f + (float)(might - 10) / 33.3f;
	}

	public static float GetStatDamageHealMultiplierRelative(int mightBonus)
	{
		return 1f + (float)mightBonus / 33.3f;
	}

	public static float GetStatAttackSpeedMultiplier(int dexterity)
	{
		return 1f + (float)(dexterity - 10) / 33.3f;
	}

	public static float GetStatEffectDurationMultiplier(int intellect)
	{
		return 1f + (float)(intellect - 10) / 20f;
	}

	public static float GetStatEffectRadiusMultiplier(int intellect)
	{
		return 1f + (float)(intellect - 10) * 6f / 100f;
	}

	[Obsolete("Interrupt is no longer used as a multiplier.")]
	public static float GetStatInterruptMultiplier(int perception)
	{
		return 1f + (float)(perception - 10) * 3f / 100f;
	}

	public static float GetStatRangedAttackDistanceMultiplier(int perception)
	{
		return 1f;
	}

	[Obsolete("Concentration is no longer used as a multiplier.")]
	public static float GetStatConcentrationMultiplier(int resolve)
	{
		return 1f + (float)(resolve - 10) * 3f / 100f;
	}

	public static int GetStatTrapAccuracyBonus(int skill)
	{
		return skill * 3;
	}

	public static int GetStatBonusAccuracy(int perception)
	{
		return perception - 10;
	}

	public static int GetStatBonusAccuracyRelative(int perceptionBonus)
	{
		return perceptionBonus * 2;
	}

	public static int GetStatBonusDeflection(int stat)
	{
		return stat - 10;
	}

	public FocusTrait FindFocusTrait()
	{
		if (m_abilities != null)
		{
			for (int num = m_abilities.Count - 1; num >= 0; num--)
			{
				FocusTrait focusTrait = m_abilities[num] as FocusTrait;
				if ((bool)focusTrait)
				{
					return focusTrait;
				}
			}
		}
		return null;
	}

	public float CalcMinDamage(float damage, bool crushing, DamageInfo info)
	{
		float num = 0f;
		num = ((!crushing) ? (damage * (AttackData.Instance.MinDamagePercent / 100f)) : (damage * (AttackData.Instance.MinCrushingDamagePercent / 100f)));
		num += DamageMinBonus;
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	public float CalculateDamageTaken(DamageInfo dmg, GameObject attacker)
	{
		if (this.OnPreDamageApplied != null)
		{
			this.OnPreDamageApplied(base.gameObject, new CombatEventArgs(dmg, attacker, base.gameObject));
			if (dmg.AttackAbsorbed)
			{
				return 0f;
			}
		}
		if (dmg.Attack != null && dmg.Attack is AttackAOE)
		{
			dmg.DamageMult(HostileAOEDamageMultiplier);
		}
		float damageAmount = dmg.DamageAmount;
		float num = AdjustDamageByDTDR(damageAmount, dmg.Damage.Type, dmg, attacker);
		CharacterStats characterStats = null;
		if ((bool)attacker)
		{
			characterStats = attacker.GetComponent<CharacterStats>();
		}
		dmg.PostDtDamageMult = characterStats.GetDifficultyDamageMultiplier(dmg.Target ? dmg.Target.GetComponent<Health>() : null);
		num = (dmg.DTAdjustedDamage = num * dmg.PostDtDamageMult);
		if (dmg.Damage != null)
		{
			bool isVeilPiercing = false;
			if (dmg.Attack != null && dmg.Attack is AttackRanged)
			{
				isVeilPiercing = (dmg.Attack as AttackRanged).VeilPiercing;
			}
			foreach (DamagePacket.DamageProcType item in dmg.Damage.DamageProc)
			{
				float num2 = CalcDT(item.Type, isVeilPiercing) * 0.25f;
				float num3 = CalcDR(item.Type) * 0.25f;
				float num4 = damageAmount * item.PercentOfBaseDamage / 100f;
				if (characterStats != null)
				{
					num4 *= characterStats.GetBonusDamagePerType(item.Type);
				}
				num4 -= num2;
				if (num3 >= 100f)
				{
					num4 = 0f;
				}
				else if (num3 > 0f)
				{
					num4 *= (100f - num3) / 100f;
				}
				if (num4 > 0f)
				{
					num += num4;
					if (dmg != null)
					{
						dmg.ProcDamage[(int)item.Type] += num4;
					}
				}
			}
		}
		dmg.FinalAdjustedDamage = num;
		if (dmg.AttackIsHostile && dmg.MinDamage > 0f && (dmg.IsCriticalHit || dmg.IsPlainHit) && dmg.IsIneffective && (dmg.Attack is AttackMelee || dmg.Attack is AttackRanged || dmg.Attack is AttackFirearm))
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(attacker, SoundSet.SoundAction.TargetImmune, SoundSet.s_LongVODelay, forceInterrupt: false);
			GameState.AutoPause(AutoPauseOptions.PauseEvent.WeaponIneffective, attacker, base.gameObject);
		}
		if (this.OnApplyProcs != null)
		{
			this.OnApplyProcs(base.gameObject, new CombatEventArgs(dmg, attacker, base.gameObject));
		}
		if (characterStats != null && characterStats.OnDamageFinal != null)
		{
			characterStats.OnDamageFinal(base.gameObject, new CombatEventArgs(dmg, attacker, base.gameObject));
		}
		TriggerWhenTakesDamage(attacker, dmg);
		if (this.OnPostDamageApplied != null)
		{
			this.OnPostDamageApplied(base.gameObject, new CombatEventArgs(dmg, attacker, base.gameObject));
		}
		return dmg.FinalAdjustedDamage;
	}

	public float AdjustDamageByDTDR(float amount, DamagePacket.DamageType dmgType, DamageInfo dmg, GameObject attacker)
	{
		return AdjustDamageByDTDR(amount, dmgType, dmg, attacker, 1f);
	}

	public float AdjustDamageByDTDR(float amount, DamagePacket.DamageType dmgType, DamageInfo dmg, GameObject attacker, float DTDRmult)
	{
		if (dmg != null && dmg.Damage != null && dmg.Damage.BestOfType != DamagePacket.DamageType.None && dmg.Damage.BestOfType != dmgType)
		{
			float num = AdjustDamageByDTDR_Helper(amount, dmgType, dmg, attacker, DTDRmult, testing: true);
			float num2 = AdjustDamageByDTDR_Helper(amount, dmg.Damage.BestOfType, dmg, attacker, DTDRmult, testing: true);
			if (num < num2)
			{
				dmgType = dmg.Damage.BestOfType;
				dmg.DamageType = dmgType;
			}
		}
		return AdjustDamageByDTDR_Helper(amount, dmgType, dmg, attacker, DTDRmult, testing: false);
	}

	private float AdjustDamageByDTDR_Helper(float amount, DamagePacket.DamageType dmgType, DamageInfo dmg, GameObject attacker, float DTDRmult, bool testing)
	{
		if (dmgType == DamagePacket.DamageType.Raw)
		{
			return amount;
		}
		bool isVeilPiercing = false;
		if (dmg != null && dmg.Attack != null && dmg.Attack is AttackRanged)
		{
			isVeilPiercing = (dmg.Attack as AttackRanged).VeilPiercing;
		}
		float num = CalcDT(dmgType, isVeilPiercing);
		float num2 = CalcDR(dmgType);
		if (dmg != null)
		{
			dmg.PreBypassDT = num * DTDRmult;
		}
		float num3 = 0f;
		if (dmg != null)
		{
			num3 = dmg.AttackerDTBypass;
		}
		if (dmg != null && dmg.Attack != null)
		{
			num3 += dmg.Attack.DTBypassTotal;
		}
		num = ((!(num > num3)) ? 0f : (num - num3));
		num *= DTDRmult;
		num2 *= DTDRmult;
		if (dmg != null && !testing)
		{
			dmg.DTRating = num;
			dmg.DRRating = num2;
			if (dmg.Damage != null)
			{
				dmg.Damage.Type = dmgType;
			}
		}
		float num4 = amount - num;
		if (num2 >= 100f)
		{
			num4 = 0f;
		}
		else if (num2 > 0f)
		{
			num4 *= (100f - num2) / 100f;
		}
		bool crushing = dmgType == DamagePacket.DamageType.Crush;
		float num5 = CalcMinDamage(amount, crushing, dmg);
		if (this.OnApplyDamageThreshhold != null && !testing)
		{
			this.OnApplyDamageThreshhold(base.gameObject, new CombatEventArgs(dmg, attacker, base.gameObject));
		}
		if (float.IsPositiveInfinity(num))
		{
			if (dmg != null)
			{
				dmg.IsMin = true;
			}
			if (!testing)
			{
				UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(2188), base.gameObject);
				if (PartyHelper.IsPartyMember(attacker))
				{
					SoundSet.TryPlayVoiceEffectWithLocalCooldown(attacker, SoundSet.SoundAction.TargetImmune, SoundSet.s_LongVODelay, forceInterrupt: false);
				}
			}
		}
		else if (num4 < num5)
		{
			if (dmg != null)
			{
				dmg.IsMin = true;
			}
			if (!testing && PartyHelper.IsPartyMember(attacker))
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_GETS_MIN_DAMAGE);
			}
			num4 = num5;
		}
		return Mathf.Max(0f, num4);
	}

	public float CalcDT(DamagePacket.DamageType dmgType, bool isVeilPiercing)
	{
		return CalcDT(dmgType, isVeilPiercing, prefab: false);
	}

	public float CalcDT(DamagePacket.DamageType dmgType, bool isVeilPiercing, bool prefab)
	{
		float num = 0f;
		if (dmgType < DamagePacket.DamageType.Count || dmgType == DamagePacket.DamageType.All)
		{
			num = DamageThreshhold[(int)dmgType];
		}
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			num += m_statusEffects[i].AdjustDamageThreshold(base.gameObject, dmgType);
		}
		Equipment equipment = m_equipment;
		if (equipment == null)
		{
			equipment = GetComponent<Equipment>();
		}
		if (equipment != null)
		{
			float num2 = ((!prefab) ? equipment.CalculateDT(dmgType, BonusDTFromArmor, base.gameObject) : equipment.CalculatePrefabDT(dmgType, BonusDTFromArmor, base.gameObject));
			num += num2;
		}
		return num;
	}

	public float CalcDTFromMagic(DamagePacket.DamageType dmgType)
	{
		float num = 0f;
		for (int i = 0; i < m_statusEffects.Count; i++)
		{
			if (m_statusEffects[i].AbilityType == GenericAbility.AbilityType.Spell)
			{
				num += ActiveStatusEffects[i].AdjustDamageThreshold(base.gameObject, dmgType);
			}
		}
		return num;
	}

	public float CalcDR(DamagePacket.DamageType dmgType)
	{
		float result = 0f;
		Equipment component = base.gameObject.GetComponent<Equipment>();
		if (component != null)
		{
			result = component.CalculateDR(dmgType);
		}
		return result;
	}

	public int GetMaxExperienceObtainable()
	{
		if (Conditionals.CommandLineArg("bb"))
		{
			return ExperienceNeededForLevel(8);
		}
		return ExperienceNeededForLevel(Mathf.Max(Level, PlayerLevelCap));
	}

	public void AddExperience(int xp)
	{
		Experience += xp;
		if (m_stronghold != null && GameState.s_playerCharacter.GetComponent<CharacterStats>() == this)
		{
			m_stronghold.AddExperience(xp);
		}
		int maxLevelCanLevelUpTo = GetMaxLevelCanLevelUpTo();
		if (maxLevelCanLevelUpTo > Level && maxLevelCanLevelUpTo > m_LastLevelUpNotified)
		{
			m_LastLevelUpNotified = maxLevelCanLevelUpTo;
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1315), NameColored(this)), Color.green);
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.LevelUp);
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.LEVEL_UP_AVAILABLE);
		}
	}

	public int GetMaxLevelCanLevelUpTo()
	{
		int i;
		for (i = Level; ExperienceNeededForNextLevel(i) <= Experience; i++)
		{
		}
		return i;
	}

	public int GetLevelBasedOnExperience()
	{
		int i;
		for (i = 1; ExperienceNeededForNextLevel(i) <= Experience; i++)
		{
		}
		return i;
	}

	public bool LevelUpAvailable()
	{
		return GetMaxLevelCanLevelUpTo() > Level;
	}

	public void LevelUpSingleLevel()
	{
		if (LevelUpAvailable())
		{
			Level++;
			AddNewAbilities();
			if (this.OnLevelUp != null)
			{
				this.OnLevelUp(this, null);
			}
		}
	}

	public void LevelUpToLevel(int level)
	{
		AbilityProgressionTable progressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(CharacterClass.ToString());
		AbilityProgressionTable talentsProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
		while (Level < level)
		{
			Level++;
			AddNewClassAbilities(progressionTable);
			AddNewTalents(talentsProgressionTable);
		}
		if (this.OnLevelUp != null)
		{
			this.OnLevelUp(this, null);
		}
	}

	public void LevelUpToMaxPossibleLevel()
	{
		if (LevelUpAvailable())
		{
			AbilityProgressionTable progressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(CharacterClass.ToString());
			AbilityProgressionTable talentsProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
			while (ExperienceNeededForNextLevel(Level) <= Experience)
			{
				Level++;
				AddNewClassAbilities(progressionTable);
				AddNewTalents(talentsProgressionTable);
			}
			if (this.OnLevelUp != null)
			{
				this.OnLevelUp(this, null);
			}
		}
	}

	public void AddNewAbilities()
	{
		AbilityProgressionTable progressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(CharacterClass.ToString());
		AbilityProgressionTable talentsProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
		AddNewClassAbilities(progressionTable);
		AddNewTalents(talentsProgressionTable);
	}

	public void ApplyAbility(GameObject ability, GameObject target)
	{
		if (this.OnPreApply != null)
		{
			this.OnPreApply(ability, new CombatEventArgs(base.gameObject, target));
		}
	}

	public void ApplyAbility(GameObject ability, Vector3 target)
	{
		if (this.OnPreApply != null)
		{
			this.OnPreApply(ability, new CombatEventArgs(base.gameObject, target));
		}
	}

	public void DeactivateAbility(GameObject ability, GameObject target)
	{
		if (this.OnDeactivate != null)
		{
			this.OnDeactivate(ability, new CombatEventArgs(base.gameObject, target));
		}
	}

	public static int GetPointsForSkillLevel(int skillLevel)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i <= 100; i++)
		{
			if (num2 == skillLevel)
			{
				return num;
			}
			num2++;
			num += num2;
		}
		return 0;
	}

	public static int CalculateSkillLevelViaPoints(int points)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i <= 100; i++)
		{
			if (num > points)
			{
				return num2 - 1;
			}
			if (num == points)
			{
				return num2;
			}
			num2++;
			num += num2;
		}
		Debug.Log("CalculateSkillLevelFromPoints: ERROR Invalid number points in skill. Using Skill Level 100 as fail safe.");
		return 100;
	}

	public int CalculateSkillLevel(SkillType skillType)
	{
		int points = 0;
		switch (skillType)
		{
		case SkillType.Stealth:
			points = StealthSkill;
			break;
		case SkillType.Athletics:
			points = AthleticsSkill;
			break;
		case SkillType.Lore:
			points = LoreSkill;
			break;
		case SkillType.Mechanics:
			points = MechanicsSkill;
			break;
		case SkillType.Survival:
			points = SurvivalSkill;
			break;
		case SkillType.Crafting:
			points = CraftingSkill;
			break;
		}
		return CalculateSkillLevelViaPoints(points);
	}

	public int CalculateSkillLevelBasedOnChange(SkillType skillType, int change)
	{
		int num = 0;
		switch (skillType)
		{
		case SkillType.Stealth:
			num = StealthSkill;
			break;
		case SkillType.Athletics:
			num = AthleticsSkill;
			break;
		case SkillType.Lore:
			num = LoreSkill;
			break;
		case SkillType.Mechanics:
			num = MechanicsSkill;
			break;
		case SkillType.Survival:
			num = SurvivalSkill;
			break;
		case SkillType.Crafting:
			num = CraftingSkill;
			break;
		}
		return CalculateSkillLevelViaPoints(num + change);
	}

	public int CalculateSkill(SkillType skillType)
	{
		return CalculateSkillInternal(skillType, CalculateSkillLevel(skillType));
	}

	public int CalculateSkillBasedOnChange(SkillType skillType, int change)
	{
		return CalculateSkillInternal(skillType, CalculateSkillLevelBasedOnChange(skillType, change));
	}

	private int CalculateSkillInternal(SkillType skillType, int baseValue)
	{
		int num = baseValue;
		switch (skillType)
		{
		case SkillType.Stealth:
			num += StealthBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 0] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 0];
			break;
		case SkillType.Athletics:
			num += AthleticsBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 1] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 1];
			break;
		case SkillType.Lore:
			num += LoreBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 2] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 2];
			break;
		case SkillType.Mechanics:
			num += MechanicsBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 3] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 3];
			break;
		case SkillType.Survival:
			num += SurvivalBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 4] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 4];
			break;
		case SkillType.Crafting:
			num += CraftingBonus;
			num += (IsPlayableClass(CharacterClass) ? ClassSkillAdjustment[(int)CharacterClass, 5] : 0);
			num += BackgroundSkillAdjustment[(int)CharacterBackground, 5];
			break;
		}
		return num;
	}

	public void AdjustSkillBonus(SkillType skillType, int BonusAdj)
	{
		switch (skillType)
		{
		case SkillType.Stealth:
			StealthBonus += BonusAdj;
			break;
		case SkillType.Athletics:
			AthleticsBonus += BonusAdj;
			break;
		case SkillType.Lore:
			LoreBonus += BonusAdj;
			break;
		case SkillType.Mechanics:
			MechanicsBonus += BonusAdj;
			break;
		case SkillType.Survival:
			SurvivalBonus += BonusAdj;
			break;
		case SkillType.Crafting:
			CraftingBonus += BonusAdj;
			break;
		}
	}

	public void AdjustLoreReveal(GameObject enemy)
	{
		int num = CalculateSkill(SkillType.Lore);
		if (num > 0 && !(enemy == null))
		{
			CharacterStats component = enemy.GetComponent<CharacterStats>();
			if (component != null)
			{
				uint loreReveal = component.LoreReveal;
				loreReveal = (component.LoreReveal = loreReveal + (uint)(num * ScaledLevel));
			}
		}
	}

	public LoreRevealStatus GetLoreRevealStatus(out float percentToNext)
	{
		uint[] array = new uint[4]
		{
			0u,
			(uint)(10 * ScaledLevel),
			(uint)(20 * ScaledLevel),
			(uint)(40 * ScaledLevel)
		};
		uint loreReveal = LoreReveal;
		LoreRevealStatus result = LoreRevealStatus.HealthDefenseDT;
		float num = 100f;
		for (int i = 1; i < 4; i++)
		{
			if (loreReveal < array[i])
			{
				result = (LoreRevealStatus)(i - 1);
				num = (float)(loreReveal - array[i - 1]) / (float)(array[i] - array[i - 1]);
			}
		}
		percentToNext = num;
		return result;
	}

	public int MinimumLevelThatCanEngageMe()
	{
		int num = 0;
		for (int i = 0; i < ActiveStatusEffects.Count; i++)
		{
			if (ActiveStatusEffects[i].Applied && ActiveStatusEffects[i].Params.AffectsStat == StatusEffect.ModifiedStat.ProhibitEnemyEngagementByLevel)
			{
				num = Mathf.Max(num, (int)ActiveStatusEffects[i].CurrentAppliedValue + ScaledLevel);
			}
		}
		return num;
	}

	public void NotifyEngagement(GameObject other)
	{
		if (this.OnEngagement != null)
		{
			this.OnEngagement(other, EventArgs.Empty);
		}
	}

	public void NotifyEngagedByOther(GameObject other)
	{
		if (IsPartyMember)
		{
			TutorialManager.Instance.TriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_GETS_ENGAGED);
			GameState.AutoPause(AutoPauseOptions.PauseEvent.MeleeEngaged, base.gameObject, other);
		}
		CheckToAddFlanked(other);
		if (this.OnEngagedByOther != null)
		{
			this.OnEngagedByOther(other, EventArgs.Empty);
		}
	}

	public void NotifyEngagementBreak(GameObject other)
	{
		CheckToRemoveFlanked();
		if (this.OnEngagementBreak != null)
		{
			this.OnEngagementBreak(other, EventArgs.Empty);
		}
	}

	public void NotifyEngagementByOtherBroken(GameObject other)
	{
		CheckToRemoveFlanked();
		if (this.OnEngagementByOtherBroken != null)
		{
			this.OnEngagementByOtherBroken(other, EventArgs.Empty);
		}
	}

	public void CheckToAddFlankedAll()
	{
		AIController component = GetComponent<AIController>();
		if (!(component == null))
		{
			CheckToAddFlanked(null);
			for (int num = component.EngagedBy.Count - 1; num >= 0; num--)
			{
				CheckToAddFlanked(component.EngagedBy[num]);
			}
		}
	}

	public void CheckToAddFlanked(GameObject newEnemy)
	{
		if (HasStatusEffectFromAffliction(AfflictionData.Flanked))
		{
			return;
		}
		AIController component = GetComponent<AIController>();
		if (component == null || component.EngagedBy.Count < EnemiesNeededToFlank)
		{
			return;
		}
		for (int num = Flankers.Count - 1; num >= 0; num--)
		{
			if (!Flankers[num])
			{
				Flankers.RemoveAt(num);
			}
		}
		if (EnemiesNeededToFlank <= 1 && component.EngagedBy.Count >= EnemiesNeededToFlank)
		{
			ApplyAffliction(AfflictionData.Flanked);
		}
		else
		{
			if (newEnemy == null)
			{
				return;
			}
			foreach (GameObject item in component.EngagedBy)
			{
				if (item == newEnemy || !GameUtilities.AreAttackersOnOppositeSidesOfTarget(item, newEnemy, base.gameObject))
				{
					continue;
				}
				CharacterStats component2 = item.GetComponent<CharacterStats>();
				if ((bool)component2 && !Flankers.Contains(component2))
				{
					Flankers.Add(component2);
					if (component2.OnBeginFlanking != null)
					{
						component2.OnBeginFlanking(item, new GameObjectEventArgs(base.gameObject));
					}
				}
				CharacterStats component3 = newEnemy.GetComponent<CharacterStats>();
				if ((bool)component3 && !Flankers.Contains(component3))
				{
					Flankers.Add(component3);
					if (component3.OnBeginFlanking != null)
					{
						component3.OnBeginFlanking(newEnemy, new GameObjectEventArgs(base.gameObject));
					}
				}
				ApplyAffliction(AfflictionData.Flanked);
				break;
			}
		}
	}

	public void CheckToRemoveFlanked()
	{
		if (!HasStatusEffectFromAffliction(AfflictionData.Flanked))
		{
			return;
		}
		AIController component = GetComponent<AIController>();
		if (component == null)
		{
			return;
		}
		if (component.EngagedBy.Count < EnemiesNeededToFlank)
		{
			ClearFlanking();
			return;
		}
		for (int num = Flankers.Count - 1; num >= 0; num--)
		{
			if (!Flankers[num])
			{
				Flankers.RemoveAt(num);
			}
		}
		for (int num2 = Flankers.Count - 1; num2 >= 0; num2--)
		{
			bool flag = false;
			for (int num3 = Flankers.Count - 1; num3 >= 0; num3--)
			{
				if (num2 != num3 && GameUtilities.AreAttackersOnOppositeSidesOfTarget(Flankers[num2].gameObject, Flankers[num3].gameObject, base.gameObject))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if ((bool)Flankers[num2] && Flankers[num2].OnEndFlanking != null)
				{
					Flankers[num2].OnEndFlanking(Flankers[num2].gameObject, new GameObjectEventArgs(base.gameObject));
				}
				Flankers.RemoveAt(num2);
			}
		}
		if ((EnemiesNeededToFlank > 1 || component.EngagedBy.Count < EnemiesNeededToFlank) && Flankers.Count == 0)
		{
			ClearFlanking();
		}
	}

	private void ClearFlanking()
	{
		ClearEffectFromAffliction(AfflictionData.Flanked);
		for (int i = 0; i < Flankers.Count; i++)
		{
			if ((bool)Flankers[i] && Flankers[i].OnEndFlanking != null)
			{
				Flankers[i].OnEndFlanking(Flankers[i].gameObject, new GameObjectEventArgs(base.gameObject));
			}
		}
		Flankers.Clear();
	}

	public bool InModalRecovery(GenericAbility.ActivationGroup grp)
	{
		if (grp > GenericAbility.ActivationGroup.None && grp < GenericAbility.ActivationGroup.Count)
		{
			return m_modalCooldownTimer[(int)grp] > 0f;
		}
		return false;
	}

	public void SetModalRecovery(GenericAbility.ActivationGroup grp)
	{
		if (grp > GenericAbility.ActivationGroup.None && grp < GenericAbility.ActivationGroup.Count)
		{
			m_modalCooldownTimer[(int)grp] = ModalRecoveryTime;
		}
	}

	public float GetModalRecovery(GenericAbility.ActivationGroup grp)
	{
		if (grp > GenericAbility.ActivationGroup.None && grp < GenericAbility.ActivationGroup.Count)
		{
			return m_modalCooldownTimer[(int)grp];
		}
		return 0f;
	}

	public void CoolDownGrimoire()
	{
		CurrentGrimoireCooldown = GetGrimoireCooldown();
	}

	public float GetGrimoireCooldown()
	{
		return AttackData.Instance.GrimoireCooldownTime + GrimoireCooldownBonus;
	}

	private void SetInvisible(bool isInvis)
	{
		bool flag = !isInvis;
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if ((bool)skinnedMeshRenderer)
			{
				skinnedMeshRenderer.enabled = flag;
			}
		}
		Cloth[] componentsInChildren2 = GetComponentsInChildren<Cloth>(includeInactive: true);
		foreach (Cloth cloth in componentsInChildren2)
		{
			if ((bool)cloth)
			{
				cloth.gameObject.SetActive(flag);
			}
		}
		ParticleSystem[] componentsInChildren3 = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem particleSystem in componentsInChildren3)
		{
			if ((bool)particleSystem)
			{
				particleSystem.gameObject.SetActive(flag);
			}
		}
		if (!flag)
		{
			return;
		}
		Equippable[] componentsInChildren4 = GetComponentsInChildren<Equippable>(includeInactive: true);
		foreach (Equippable equippable in componentsInChildren4)
		{
			if (equippable != null)
			{
				equippable.Renders = true;
			}
		}
	}

	public string Name()
	{
		if (!string.IsNullOrEmpty(OverrideName))
		{
			return OverrideName;
		}
		string result = "*NameError*";
		if (DisplayName.IsValidString)
		{
			result = StringTableManager.GetCharacterName(DisplayName, Gender);
		}
		else
		{
			Trap component = GetComponent<Trap>();
			if ((bool)component)
			{
				result = component.GetDisplayName();
			}
		}
		return result;
	}

	public ChanterTrait GetChanterTrait()
	{
		ChanterTrait chanterTrait = null;
		for (int i = 0; i < ActiveAbilities.Count; i++)
		{
			chanterTrait = ActiveAbilities[i] as ChanterTrait;
			if ((bool)chanterTrait)
			{
				break;
			}
		}
		return chanterTrait;
	}

	public void FindNewSpells(List<GenericSpell> newSpells, CharacterStats casterStats, int maxSpellLevel)
	{
		if (!casterStats)
		{
			return;
		}
		for (int i = 0; i < ActiveAbilities.Count; i++)
		{
			GenericSpell genericSpell = ActiveAbilities[i] as GenericSpell;
			if (!genericSpell || (genericSpell.SpellClass != Class.Wizard && genericSpell.SpellClass != Class.Druid && genericSpell.SpellClass != Class.Priest) || genericSpell.SpellLevel > maxSpellLevel)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < newSpells.Count; j++)
			{
				if (GenericAbility.NameComparer.Instance.Equals(newSpells[j], genericSpell))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			bool flag2 = false;
			foreach (GenericAbility activeAbility in casterStats.ActiveAbilities)
			{
				GenericSpell genericSpell2 = activeAbility as GenericSpell;
				if ((bool)genericSpell2 && ActiveAbilities[i].DisplayName.StringID == genericSpell2.DisplayName.StringID)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				newSpells.Add(genericSpell);
			}
		}
	}

	public float GetArmorSpeedFactor()
	{
		Equipment component = GetComponent<Equipment>();
		if (component == null || component.CurrentItems == null || component.CurrentItems.Chest == null)
		{
			return 1f;
		}
		Armor component2 = component.CurrentItems.Chest.GetComponent<Armor>();
		if (component2 == null)
		{
			return 1f;
		}
		float speedFactor = component2.SpeedFactor;
		speedFactor += ArmorSpeedFactorAdj;
		if (speedFactor > 1f)
		{
			speedFactor = 1f;
		}
		Equippable component3 = component.CurrentItems.Chest.GetComponent<Equippable>();
		if (component3 != null && component3.DurabilityState == Equippable.DurabilityStateType.Damaged)
		{
			speedFactor *= 1.1f;
		}
		return speedFactor;
	}

	public bool HasTrapCooldownTimer(int trapID)
	{
		return m_trapCooldownTimers.ContainsKey(trapID);
	}

	public void SetTrapCooldownTimer(int trapID)
	{
		if (!HasTrapCooldownTimer(trapID))
		{
			m_trapCooldownTimers.Add(trapID, 1f);
		}
	}

	private void TrapCooldownTimerUpdate(float seconds)
	{
		if (m_trapCooldownTimers.Count == 0)
		{
			return;
		}
		foreach (int item in new List<int>(m_trapCooldownTimers.Keys))
		{
			float num = m_trapCooldownTimers[item];
			num -= seconds;
			if (num <= 0f)
			{
				m_trapCooldownTimers.Remove(item);
			}
			else
			{
				m_trapCooldownTimers[item] = num;
			}
		}
	}

	public static int ExperienceNeededForNextLevel(int currentLevel)
	{
		return currentLevel * (currentLevel + 1) * 500;
	}

	public static int ExperienceNeededForLevel(int level)
	{
		return (level - 1) * level * 500;
	}

	public static bool IsOffhandAttack(GameObject parent, AttackBase attack)
	{
		return parent.GetComponent<Equipment>().SecondaryAttack == attack;
	}

	public static string NameColored(MonoBehaviour obj)
	{
		if ((bool)obj)
		{
			return NameColored(obj.gameObject);
		}
		Debug.LogError("Tried to get name of null or destroyed Monobehaviour.");
		return "*NameError*";
	}

	public static string NameColored(GameObject obj)
	{
		string text = (obj ? UIConversationManager.GetColorOrEmpty(obj.gameObject) : "");
		if (string.IsNullOrEmpty(text))
		{
			return Name(obj);
		}
		return text + Name(obj) + "[-]";
	}

	public static string Name(CharacterStats stat)
	{
		if ((bool)stat)
		{
			return stat.Name();
		}
		Debug.LogError("Tried to get name of null or destroyed CharacterStats.");
		return "*NameError*";
	}

	public static string Name(MonoBehaviour obj)
	{
		if ((bool)obj)
		{
			return Name(obj.gameObject);
		}
		Debug.LogError("Tried to get name of null or destroyed Monobehaviour.");
		return "*NameError*";
	}

	public static string Name(GameObject obj)
	{
		if ((bool)obj)
		{
			CharacterStats component = obj.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				return component.Name();
			}
			StoredCharacterInfo component2 = obj.GetComponent<StoredCharacterInfo>();
			if ((bool)component2)
			{
				return component2.DisplayName;
			}
			return obj.name;
		}
		Debug.LogError("Tried to get name of null or destroyed GameObject.");
		return "*NameError*";
	}

	public static string Name(Guid obj)
	{
		if (TryGetName(obj, out var charName))
		{
			return charName;
		}
		Debug.LogError("CharacterStats.Name: Couldn't find object '" + obj.ToString() + "'.");
		return "";
	}

	public static bool TryGetName(Guid obj, out string charName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(obj);
		if ((bool)objectByID)
		{
			charName = Name(objectByID);
			return true;
		}
		StoredCharacterInfo storedCompanion = Stronghold.Instance.GetStoredCompanion(obj);
		if (storedCompanion != null)
		{
			charName = storedCompanion.DisplayName;
			return true;
		}
		charName = "";
		return false;
	}

	public static Gender GetGender(CharacterStats stats)
	{
		if ((bool)stats)
		{
			return stats.Gender;
		}
		return Gender.Neuter;
	}

	public static Gender GetGender(MonoBehaviour obj)
	{
		if ((bool)obj)
		{
			return GetGender(obj.gameObject);
		}
		return Gender.Neuter;
	}

	public static Gender GetGender(GameObject obj)
	{
		if ((bool)obj)
		{
			CharacterStats component = obj.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				return component.Gender;
			}
			StoredCharacterInfo component2 = obj.GetComponent<StoredCharacterInfo>();
			if ((bool)component2)
			{
				return component2.Gender;
			}
			return Gender.Neuter;
		}
		return Gender.Neuter;
	}

	public static bool GetBool(float odds)
	{
		return OEIRandom.FloatValue() < odds;
	}

	public static SkillType SkillFromName(string tag)
	{
		if (tag.Equals("Stealth", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Stealth;
		}
		if (tag.Equals("Athletics", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Athletics;
		}
		if (tag.Equals("Lore", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Lore;
		}
		if (tag.Equals("Mechanics", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Mechanics;
		}
		if (tag.Equals("Survival", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Survival;
		}
		if (tag.Equals("Crafting", StringComparison.OrdinalIgnoreCase))
		{
			return SkillType.Crafting;
		}
		Debug.LogError("Unrecognized skill name: " + tag);
		return SkillType.Athletics;
	}

	public static bool SubraceIsGodlike(Subrace subrace)
	{
		if ((uint)(subrace - 8) <= 3u || subrace == Subrace.Avian_Godlike)
		{
			return true;
		}
		return false;
	}

	public static bool ClassIsDragonOrDrake(Class cl)
	{
		if ((uint)(cl - 29) <= 2u)
		{
			return true;
		}
		return false;
	}

	public Guid GetUniqueID()
	{
		InstanceID component = base.gameObject.GetComponent<InstanceID>();
		if (component != null)
		{
			return component.Guid;
		}
		return Guid.Empty;
	}

	public void ResetSpellUsage(CharacterStats charStats)
	{
		int casterLevel = charStats.Level;
		int[] unlockLevels = GetModifiedEncounterData();

		for (int i = 0; i < unlockLevels.Length; i++)
		{
			if (casterLevel >= unlockLevels[i])
			{
				charStats.SpellCastCount[i] = 0;
			}
			else
			{
				break;  //Ugly optimisation if char is underlevelled
			}
		}
	}

	public int[] GetModifiedEncounterData()
	{
		int[] result = new int[Grimoire.MaxSpellLevel];
		for (int i = 0; i < Grimoire.MaxSpellLevel; i++)
		{
			result[i] = 256;
		}

		switch (IEModOptions.PerEncounterSpellsSetting)
		{
			case IEModOptions.PerEncounterSpells.NoChange:
			default:
				//This isn't used, but we'll fill it in anyway
				result[0] = 9;
				result[1] = 11;
				result[2] = 13;
				break;

			case IEModOptions.PerEncounterSpells.Levels_9_12:
				result[0] = 9;
				result[1] = 12;
				break;

			case IEModOptions.PerEncounterSpells.Levels_6_9_12:
				result[0] = 6;
				result[1] = 9;
				result[2] = 12;
				break;

			case IEModOptions.PerEncounterSpells.Levels_8_10_12_14:
				result[0] = 8;
				result[1] = 10;
				result[2] = 12;
				result[3] = 14;
				break;

			case IEModOptions.PerEncounterSpells.Levels_6_9_12_14:
				result[0] = 6;
				result[1] = 9;
				result[2] = 12;
				result[3] = 14;
				break;

			case IEModOptions.PerEncounterSpells.Levels_6_8_10_12_14:
				result[0] = 6;
				result[1] = 8;
				result[2] = 10;
				result[3] = 12;
				result[4] = 14;
				break;

			case IEModOptions.PerEncounterSpells.Levels_4_6_8_10_12_14:
				result[0] = 4;
				result[1] = 6;
				result[2] = 8;
				result[3] = 10;
				result[4] = 12;
				result[5] = 14;
				break;

			case IEModOptions.PerEncounterSpells.Levels_4_8_12_16:
				result[0] = 4;
				result[1] = 8;
				result[2] = 12;
				result[3] = 16;
				break;

			case IEModOptions.PerEncounterSpells.AllPerEncounter:
				for (int i = 0; i < Grimoire.MaxSpellLevel; i++)
				{
					result[i] = 1;
				}
				break;

			case IEModOptions.PerEncounterSpells.AllPerRest:
				//Body intentionally left blank!
				break;

		}
		return result;
	}



}
