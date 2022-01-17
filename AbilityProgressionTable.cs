using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AbilityProgressionTable : ScriptableObject
{
	[Flags]
	public enum AbilityFilterFlag
	{
		None = 0,
		UnownedAbilities = 1,
		OwnedAbilities = 2,
		AutoGrantAbilities = 4,
		UnlockedAbilities = 8,
		AutoMasterAbilities = 0x10,
		RequirementsNotMet = 0x20,
		RequirementsMet = 0x40
	}

	[Serializable]
	public class AttributeScorePair
	{
		public CharacterStats.AttributeScoreType Attribute;

		public int MinimumValue;

		public int MaximumValue = 99;
	}

	[Serializable]
	public class AbilityRequirements
	{
		public string Note;

		public bool NotCondition;

		public int MinimumLevel = 1;

		public int MaximumLevel = 99;

		public CharacterStats.Class Class;

		public CharacterStats.Subrace Subrace;

		public CharacterStats.Background Background;

		public Religion.Deity Deity;

		public Religion.PaladinOrder PaladinOrder;

		public bool MustBePlayerCharacter;

		public ProductConfiguration.Package PackageRequired = ProductConfiguration.Package.BaseGame;

		public GenericAbility[] Abilities;

		public AttributeScorePair[] Attributes;
	}

	public enum LogicalOperator
	{
		And,
		Or
	}

	[Serializable]
	public class UnlockableAbility
	{
		public enum AbilityUnlockStyle
		{
			AutoGrant,
			Unlock,
			AutoMaster
		}

		public string Note;

		public CategoryFlag Category = CategoryFlag.General;

		public AbilityUnlockStyle UnlockStyle;

		public AbilityActivationObject ActivationObject;

		public GameObject Ability;

		public GameObject RemovesAbility;

		public LogicalOperator RequirementsOperator = LogicalOperator.Or;

		public AbilityRequirements[] RequirementSets;
	}

	public enum AbilityActivationObject
	{
		Self,
		AnimalCompanion,
		SelfAndAnimalCompanion
	}

	[Flags]
	public enum CategoryFlag
	{
		General = 1,
		Racial = 2,
		Talent = 4,
		Custom1 = 8,
		Custom2 = 0x10,
		Custom3 = 0x20,
		Custom4 = 0x40,
		Custom5 = 0x80
	}

	[Serializable]
	public class AbilityPointUnlock
	{
		[Serializable]
		public class CategoryPointPair
		{
			public CategoryFlag Category = CategoryFlag.General;

			public int PointsGranted;

			public GUIDatabaseString UnlockDescription = new GUIDatabaseString();
		}

		public int Level;

		public CategoryPointPair[] CategoryPointPairs;
	}

	[Serializable]
	public class CategoryName
	{
		public CategoryFlag Category;

		public GUIDatabaseString DisplayName = new GUIDatabaseString();
	}

	[Serializable]
	public class SkillPointUnlock
	{
		public int Level;

		public int StealthPointsToAllocate;

		public int AthleticsPointsToAllocate;

		public int LorePointsToAllocate;

		public int MechanicsPointsToAllocate;

		public int SurvivalPointsToAllocate;

		public int CraftingPointsToAllocate;
	}

	public static AbilityFilterFlag DefaultAutoGrantFilterFlags = AbilityFilterFlag.UnownedAbilities | AbilityFilterFlag.AutoGrantAbilities | AbilityFilterFlag.RequirementsMet;

	public static AbilityFilterFlag DefaultUnlockFilterFlags = AbilityFilterFlag.UnownedAbilities | AbilityFilterFlag.UnlockedAbilities | AbilityFilterFlag.RequirementsMet;

	public static AbilityFilterFlag DefaultAutoMasterFilterFlags = AbilityFilterFlag.UnownedAbilities | AbilityFilterFlag.OwnedAbilities | AbilityFilterFlag.AutoMasterAbilities | AbilityFilterFlag.RequirementsMet;

	public static AbilityFilterFlag AllFilterFlags = AbilityFilterFlag.UnownedAbilities | AbilityFilterFlag.OwnedAbilities | AbilityFilterFlag.AutoGrantAbilities | AbilityFilterFlag.UnlockedAbilities | AbilityFilterFlag.AutoMasterAbilities | AbilityFilterFlag.RequirementsNotMet | AbilityFilterFlag.RequirementsMet;

	public static CategoryFlag AllCategoryFlags = CategoryFlag.General | CategoryFlag.Racial | CategoryFlag.Talent | CategoryFlag.Custom1 | CategoryFlag.Custom2 | CategoryFlag.Custom3 | CategoryFlag.Custom4 | CategoryFlag.Custom5;

	public CategoryName[] CategoryNames;

	public UnlockableAbility[] AbilityUnlocks;

	public AbilityPointUnlock[] AbilityPointUnlocks;

	public SkillPointUnlock[] SkillPointUnlocks;

	private static List<AbilityPointUnlock> s_abilityPointUnlocks = new List<AbilityPointUnlock>();

	private static ChanterTrait GetChanterTrait(CharacterStats.CoreData coreData)
	{
		MonoBehaviour monoBehaviour = coreData.KnownSkills.Find((MonoBehaviour s) => s as ChanterTrait != null);
		if ((bool)monoBehaviour)
		{
			return monoBehaviour as ChanterTrait;
		}
		return null;
	}

	private static ChanterTrait GetChanterTrait(CharacterStats stats)
	{
		return stats.GetChanterTrait();
	}

	private bool HasAbility(GameObject ability, CharacterStats.CoreData coreData)
	{
		List<MonoBehaviour> knownSkills = coreData.KnownSkills;
		return HasAbility(ability, knownSkills);
	}

	private bool HasAbility(GameObject ability, List<MonoBehaviour> knownSkills)
	{
		if (ability == null)
		{
			return false;
		}
		MonoBehaviour monoBehaviour = null;
		if (GetGenericAbility(ability) != null)
		{
			monoBehaviour = knownSkills.Find((MonoBehaviour s) => GetGenericAbility(s) != null && GenericAbility.NameComparer.Instance.Equals(GetGenericAbility(s), GetGenericAbility(ability)));
		}
		else if (GetGenericTalent(ability) != null)
		{
			monoBehaviour = knownSkills.Find((MonoBehaviour s) => GetGenericTalent(s) != null && GenericTalent.NameComparer.Instance.Equals(GetGenericTalent(s), GetGenericTalent(ability)));
		}
		else if (GetPhrase(ability) != null)
		{
			monoBehaviour = knownSkills.Find((MonoBehaviour s) => GetPhrase(s) != null && Phrase.NameComparer.Instance.Equals(GetPhrase(s), GetPhrase(ability)));
		}
		else if (GetRecipe(ability) != null)
		{
			return true;
		}
		return monoBehaviour != null;
	}

	private static bool HasFilterFlag(AbilityFilterFlag flags, AbilityFilterFlag value)
	{
		return (flags & value) == value;
	}

	private static bool HasCategoryFlag(CategoryFlag flags, CategoryFlag value)
	{
		return (flags & value) == value;
	}

	private bool AreValidFilterFlags(AbilityFilterFlag filterFlags)
	{
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.OwnedAbilities) && !HasFilterFlag(filterFlags, AbilityFilterFlag.UnownedAbilities))
		{
			Debug.Log("LevelProgressionTemplate: GetAbilities passed in filter flags that will never return abilities. Must include OwndedAbilities or UnownedAbilities.");
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.AutoGrantAbilities) && !HasFilterFlag(filterFlags, AbilityFilterFlag.UnlockedAbilities) && !HasFilterFlag(filterFlags, AbilityFilterFlag.AutoMasterAbilities))
		{
			Debug.Log("LevelProgressionTemplate: GetAbilities passed in filter flags that will never return abilities. Must include AutoGrantAbilities, UnlockedAbilities, or AutoMasterAbilities.");
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.RequirementsMet) && !HasFilterFlag(filterFlags, AbilityFilterFlag.RequirementsNotMet))
		{
			Debug.Log("LevelProgressionTemplate: GetAbilities passed in filter flags that will never return abilities. Must include RequirementsMet or RequirementsNotMet.");
			return false;
		}
		return true;
	}

	private bool PassesAbilityFilterCheck(UnlockableAbility unlockableAbility, CategoryFlag categoryFlags, CharacterStats.CoreData coreData, AbilityFilterFlag filterFlags)
	{
		bool flag = HasAbility(unlockableAbility.Ability, coreData);
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.OwnedAbilities) && flag)
		{
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.UnownedAbilities) && !flag)
		{
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.AutoGrantAbilities) && unlockableAbility.UnlockStyle == UnlockableAbility.AbilityUnlockStyle.AutoGrant)
		{
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.AutoMasterAbilities) && unlockableAbility.UnlockStyle == UnlockableAbility.AbilityUnlockStyle.AutoMaster)
		{
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.UnlockedAbilities) && unlockableAbility.UnlockStyle == UnlockableAbility.AbilityUnlockStyle.Unlock)
		{
			return false;
		}
		if (!HasCategoryFlag(categoryFlags, unlockableAbility.Category))
		{
			return false;
		}
		return true;
	}

	private bool PassesRequirementsFilterCheck(bool requirementsMet, AbilityFilterFlag filterFlags)
	{
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.RequirementsMet) && requirementsMet)
		{
			return false;
		}
		if (!HasFilterFlag(filterFlags, AbilityFilterFlag.RequirementsNotMet) && !requirementsMet)
		{
			return false;
		}
		return true;
	}

	private bool RequirementsMet(CharacterStats.CoreData coreData, AbilityRequirements abilityRequirement)
	{
		if (abilityRequirement.MustBePlayerCharacter && !coreData.IsPlayerCharacter)
		{
			return false;
		}
		if (abilityRequirement.PackageRequired != ProductConfiguration.Package.BaseGame && (ProductConfiguration.ActivePackage & abilityRequirement.PackageRequired) != abilityRequirement.PackageRequired)
		{
			return false;
		}
		if ((abilityRequirement.MinimumLevel != 0 && coreData.Level < abilityRequirement.MinimumLevel) || (abilityRequirement.MaximumLevel != 0 && coreData.Level > abilityRequirement.MaximumLevel))
		{
			return false;
		}
		if (abilityRequirement.Class != 0 && abilityRequirement.Class != coreData.Class)
		{
			return false;
		}
		if (abilityRequirement.Subrace != 0 && abilityRequirement.Subrace != coreData.Subrace)
		{
			return false;
		}
		if (abilityRequirement.Background != 0 && abilityRequirement.Background != coreData.Background)
		{
			return false;
		}
		if (abilityRequirement.Deity != 0 && abilityRequirement.Deity != coreData.Deity)
		{
			return false;
		}
		if (abilityRequirement.PaladinOrder != 0 && abilityRequirement.PaladinOrder != coreData.PaladinOrder)
		{
			return false;
		}
		GenericAbility[] abilities = abilityRequirement.Abilities;
		foreach (GenericAbility genericAbility in abilities)
		{
			if (!HasAbility(genericAbility.gameObject, coreData))
			{
				return false;
			}
		}
		AttributeScorePair[] attributes = abilityRequirement.Attributes;
		foreach (AttributeScorePair attributeScorePair in attributes)
		{
			if ((attributeScorePair.MinimumValue != 0 && attributeScorePair.MinimumValue > coreData.BaseStats[(int)attributeScorePair.Attribute]) || (attributeScorePair.MaximumValue != 0 && attributeScorePair.MaximumValue < coreData.BaseStats[(int)attributeScorePair.Attribute]))
			{
				return false;
			}
		}
		return true;
	}

	public string GetCategoryName(CategoryFlag categoryFlag)
	{
		if (CategoryNames == null)
		{
			return "";
		}
		CategoryName[] categoryNames = CategoryNames;
		foreach (CategoryName categoryName in categoryNames)
		{
			if (categoryName.Category == categoryFlag)
			{
				if (categoryName.DisplayName == null)
				{
					return "";
				}
				return categoryName.DisplayName.ToString();
			}
		}
		if (categoryFlag == CategoryFlag.Talent)
		{
			return GUIUtils.GetText(808);
		}
		return "";
	}

	public List<AbilityPointUnlock> GetLevelAbilityPoints(int level)
	{
		s_abilityPointUnlocks.Clear();
		AbilityPointUnlock[] abilityPointUnlocks = AbilityPointUnlocks;
		foreach (AbilityPointUnlock abilityPointUnlock in abilityPointUnlocks)
		{
			if (abilityPointUnlock.Level == level)
			{
				s_abilityPointUnlocks.Add(abilityPointUnlock);
			}
		}
		return s_abilityPointUnlocks;
	}

	public UnlockableAbility[] GetAbilities(CharacterStats stats, CategoryFlag categoryFlags, AbilityFilterFlag filterFlags)
	{
		return GetAbilities(stats.GetCopyOfCoreData(), categoryFlags, filterFlags);
	}

	public UnlockableAbility[] GetAbilities(CharacterStats.CoreData coreData, CategoryFlag categoryFlags, AbilityFilterFlag filterFlags)
	{
		if (!AreValidFilterFlags(filterFlags))
		{
			return null;
		}
		List<UnlockableAbility> list = new List<UnlockableAbility>();
		UnlockableAbility[] abilityUnlocks = AbilityUnlocks;
		foreach (UnlockableAbility unlockableAbility in abilityUnlocks)
		{
			if (!PassesAbilityFilterCheck(unlockableAbility, categoryFlags, coreData, filterFlags))
			{
				continue;
			}
			bool requirementsMet = unlockableAbility.RequirementSets.Length == 0 || unlockableAbility.RequirementsOperator == LogicalOperator.And;
			AbilityRequirements[] requirementSets = unlockableAbility.RequirementSets;
			foreach (AbilityRequirements abilityRequirements in requirementSets)
			{
				bool flag = RequirementsMet(coreData, abilityRequirements);
				if (abilityRequirements.NotCondition)
				{
					flag = !flag;
				}
				if (flag && unlockableAbility.RequirementsOperator == LogicalOperator.Or)
				{
					requirementsMet = true;
					break;
				}
				if (!flag && unlockableAbility.RequirementsOperator == LogicalOperator.And)
				{
					requirementsMet = false;
					break;
				}
			}
			if (PassesRequirementsFilterCheck(requirementsMet, filterFlags))
			{
				list.Add(unlockableAbility);
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	public int GetSkillPointsToApply(int minLevel, int maxLevel, CharacterStats.SkillType skillType)
	{
		int num = 0;
		if (SkillPointUnlocks == null)
		{
			return num;
		}
		SkillPointUnlock[] skillPointUnlocks = SkillPointUnlocks;
		foreach (SkillPointUnlock skillPointUnlock in skillPointUnlocks)
		{
			if (skillPointUnlock.Level <= maxLevel && skillPointUnlock.Level >= minLevel)
			{
				switch (skillType)
				{
				case CharacterStats.SkillType.Stealth:
					num += skillPointUnlock.StealthPointsToAllocate;
					break;
				case CharacterStats.SkillType.Athletics:
					num += skillPointUnlock.AthleticsPointsToAllocate;
					break;
				case CharacterStats.SkillType.Lore:
					num += skillPointUnlock.LorePointsToAllocate;
					break;
				case CharacterStats.SkillType.Mechanics:
					num += skillPointUnlock.MechanicsPointsToAllocate;
					break;
				case CharacterStats.SkillType.Survival:
					num += skillPointUnlock.SurvivalPointsToAllocate;
					break;
				case CharacterStats.SkillType.Crafting:
					num += skillPointUnlock.CraftingPointsToAllocate;
					break;
				}
			}
		}
		return num;
	}

	public bool HasAbility(GameObject testAbility)
	{
		if ((bool)GetGenericAbility(testAbility))
		{
			UnlockableAbility[] abilityUnlocks = AbilityUnlocks;
			for (int i = 0; i < abilityUnlocks.Length; i++)
			{
				GenericAbility genericAbility = GetGenericAbility(abilityUnlocks[i].Ability);
				if ((bool)genericAbility && GenericAbility.NameComparer.Instance.Equals(GetGenericAbility(testAbility), genericAbility))
				{
					return true;
				}
			}
		}
		else if ((bool)GetGenericTalent(testAbility))
		{
			UnlockableAbility[] abilityUnlocks = AbilityUnlocks;
			for (int i = 0; i < abilityUnlocks.Length; i++)
			{
				GenericTalent genericTalent = GetGenericTalent(abilityUnlocks[i].Ability);
				if ((bool)genericTalent && GenericTalent.NameComparer.Instance.Equals(GetGenericTalent(testAbility), genericTalent))
				{
					return true;
				}
			}
		}
		else if ((bool)GetPhrase(testAbility))
		{
			UnlockableAbility[] abilityUnlocks = AbilityUnlocks;
			for (int i = 0; i < abilityUnlocks.Length; i++)
			{
				Phrase phrase = GetPhrase(abilityUnlocks[i].Ability);
				if ((bool)phrase && Phrase.NameComparer.Instance.Equals(GetPhrase(testAbility), phrase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTalentGrantingAbility(GenericAbility testAbility)
	{
		UnlockableAbility[] abilityUnlocks = AbilityUnlocks;
		for (int i = 0; i < abilityUnlocks.Length; i++)
		{
			GenericTalent genericTalent = GetGenericTalent(abilityUnlocks[i].Ability);
			if (!genericTalent || genericTalent.Type != 0)
			{
				continue;
			}
			for (int j = 0; j < genericTalent.Abilities.Length; j++)
			{
				if (GenericAbility.NameComparer.Instance.Equals(testAbility, genericTalent.Abilities[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static AbilityProgressionTable LoadAbilityProgressionTable(string tableName)
	{
		return GameResources.LoadPrefab<AbilityProgressionTable>(tableName + "AbilityProgressionTable", instantiate: false);
	}

	public static Phrase GetPhrase(MonoBehaviour ability)
	{
		return ability as Phrase;
	}

	public static Phrase GetPhrase(GameObject ability)
	{
		return ability.GetComponent<Phrase>();
	}

	public static Recipe GetRecipe(MonoBehaviour ability)
	{
		return ability as Recipe;
	}

	public static Recipe GetRecipe(GameObject ability)
	{
		return ability.GetComponent<Recipe>();
	}

	public static GenericAbility GetGenericAbility(MonoBehaviour ability)
	{
		return ability as GenericAbility;
	}

	public static GenericAbility GetGenericAbility(GameObject ability)
	{
		return ability.GetComponent<GenericAbility>();
	}

	public static GenericCipherAbility GetGenericCipherAbility(GameObject ability)
	{
		return ability.GetComponent<GenericCipherAbility>();
	}

	public static GenericSpell GetGenericSpell(GameObject ability)
	{
		return ability.GetComponent<GenericSpell>();
	}

	public static GenericTalent GetGenericTalent(MonoBehaviour ability)
	{
		return ability as GenericTalent;
	}

	public static GenericTalent GetGenericTalent(GameObject ability)
	{
		return ability.GetComponent<GenericTalent>();
	}

	public static string GetAbilityName(MonoBehaviour ability)
	{
		return GetAbilityName(ability.gameObject);
	}

	public static string GetAbilityName(GameObject ability)
	{
		if ((bool)GetGenericAbility(ability))
		{
			return GenericAbility.Name(GetGenericAbility(ability));
		}
		if ((bool)GetGenericTalent(ability))
		{
			return GetGenericTalent(ability).Name(UICharacterCreationManager.Instance.TargetCharacter);
		}
		if ((bool)GetPhrase(ability))
		{
			return GetPhrase(ability).DisplayName.GetText();
		}
		if ((bool)GetRecipe(ability))
		{
			return GetRecipe(ability).DisplayName.GetText();
		}
		return "";
	}

	public static string GetAbilityDesc(GameObject ability)
	{
		if ((bool)GetGenericAbility(ability))
		{
			return GetGenericAbility(ability).Description.GetText();
		}
		if ((bool)GetGenericTalent(ability))
		{
			return GetGenericTalent(ability).Description.GetText();
		}
		if ((bool)GetPhrase(ability))
		{
			return GetPhrase(ability).Description.GetText();
		}
		return "";
	}

	public static string GetAbilityDesc(GameObject ability, GameObject owner, AbilityActivationObject activationObject)
	{
		if ((bool)GetGenericAbility(ability))
		{
			string text = NGUITools.StripColorSymbols(GetGenericAbility(ability).GetStringFlat(summary: true, owner, StatusEffectFormatMode.InspectWindow, activationObject == AbilityActivationObject.AnimalCompanion));
			return GetGenericAbility(ability).Description.GetText() + "\n\n" + text;
		}
		if ((bool)GetGenericTalent(ability))
		{
			string text2 = NGUITools.StripColorSymbols(GetGenericTalent(ability).GetString(owner, StatusEffectFormatMode.InspectWindow, activationObject == AbilityActivationObject.AnimalCompanion));
			return GetGenericTalent(ability).Description.GetText() + "\n\n" + text2;
		}
		if ((bool)GetPhrase(ability))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(GetPhrase(ability).Description.GetText());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			StringEffects stringEffects = new StringEffects();
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(GetPhrase(ability).GetString(null, owner, stringEffects)));
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(AttackBase.StringEffects(stringEffects, targets: true)));
			return stringBuilder.ToString();
		}
		return "";
	}

	public static Texture2D GetAbilityIcon(GameObject ability)
	{
		if ((bool)GetGenericAbility(ability))
		{
			return GetGenericAbility(ability).Icon;
		}
		if ((bool)GetGenericTalent(ability))
		{
			return GetGenericTalent(ability).Icon;
		}
		if ((bool)GetPhrase(ability))
		{
			return GetPhrase(ability).Icon;
		}
		_ = (bool)GetRecipe(ability);
		return null;
	}

	public static ITooltipContent GetToolTipContent(GameObject ability)
	{
		if ((bool)GetGenericAbility(ability))
		{
			return GetGenericAbility(ability);
		}
		if ((bool)GetGenericTalent(ability))
		{
			return GetGenericTalent(ability);
		}
		if ((bool)GetPhrase(ability))
		{
			return GetPhrase(ability);
		}
		_ = (bool)GetRecipe(ability);
		return null;
	}

	public static int GetSpellLevel(GameObject ability)
	{
		if ((bool)GetGenericCipherAbility(ability))
		{
			return GetGenericCipherAbility(ability).SpellLevel;
		}
		if ((bool)GetGenericSpell(ability))
		{
			return GetGenericSpell(ability).SpellLevel;
		}
		if ((bool)GetPhrase(ability))
		{
			return GetPhrase(ability).Level;
		}
		return 0;
	}

	public static IEnumerable<CharacterStats> GetCharactersToAddAbilityTo(UnlockableAbility unlockable, CharacterStats characterStats)
	{
		if (unlockable.ActivationObject == AbilityActivationObject.AnimalCompanion || unlockable.ActivationObject == AbilityActivationObject.SelfAndAnimalCompanion)
		{
			GameObject gameObject = GameUtilities.FindAnimalCompanion(characterStats.gameObject);
			if (!gameObject)
			{
				Debug.Log("Add Ability Error: " + characterStats.Name() + " does not have an animal companion for ability " + unlockable.Ability.name + ". Adding to self instead.");
				yield return characterStats;
				yield break;
			}
			yield return gameObject.GetComponent<CharacterStats>();
		}
		if (unlockable.ActivationObject == AbilityActivationObject.Self || unlockable.ActivationObject == AbilityActivationObject.SelfAndAnimalCompanion)
		{
			yield return characterStats;
		}
	}

	public static void AddAbilityToCharacter(UnlockableAbility unlockable, CharacterStats characterStats, bool executeSummons = false)
	{
		foreach (CharacterStats item in GetCharactersToAddAbilityTo(unlockable, characterStats))
		{
			Summon component = AddAbilityToCharacter(unlockable.Ability, item).GetComponent<Summon>();
			if (executeSummons && (bool)component && component.SummonType == AIController.AISummonType.AnimalCompanion)
			{
				component.InitiateSummoning(component.transform.position);
			}
			if ((bool)unlockable.RemovesAbility)
			{
				if ((bool)GetGenericAbility(unlockable.RemovesAbility))
				{
					item.RemoveAbility(GetGenericAbility(unlockable.RemovesAbility));
				}
				else if ((bool)GetGenericTalent(unlockable.RemovesAbility))
				{
					item.RemoveTalent(GetGenericTalent(unlockable.RemovesAbility));
				}
				else if ((bool)GetPhrase(unlockable.RemovesAbility))
				{
					Debug.Log("AbilityProgressionTable ERROR: No support for removing phrases exist!");
				}
				else if ((bool)GetRecipe(unlockable.RemovesAbility))
				{
					Debug.Log("AbilityProgressionTable ERROR: No support for removing recipes exist!");
				}
				else
				{
					Debug.Log("AbilityProgressionTable ERROR: Ability " + unlockable.RemovesAbility.name + " added is not of type GenericAbility, GenericTalent, Phrase, or Recipe!");
				}
			}
		}
	}

	public static void AddAbilityToCharacter(string abilityName, CharacterStats stats, bool causeIsGameplay = false)
	{
		GameObject gameObject = GameResources.LoadPrefab<GameObject>(abilityName, instantiate: false);
		if ((bool)gameObject)
		{
			AddAbilityToCharacter(gameObject, stats, causeIsGameplay);
			Console.AddMessage(GUIUtils.Format(1812, CharacterStats.NameColored(stats), GetAbilityName(gameObject)));
			if (causeIsGameplay)
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.Format(1812, CharacterStats.Name(stats), GetAbilityName(gameObject)));
			}
		}
		else
		{
			Debug.Log("AddAbility: Error - could not find ability: " + abilityName);
		}
	}

	public static GameObject AddAbilityToCharacter(GameObject ability, CharacterStats characterStats, bool causeIsGameplay = false)
	{
		GameObject result = null;
		if ((bool)GetGenericAbility(ability))
		{
			GenericAbility genericAbility = GetGenericAbility(ability);
			result = characterStats.InstantiateAbility(genericAbility, GetGenericAbility(ability).EffectType).gameObject;
			if (causeIsGameplay && genericAbility is GenericSpell && (genericAbility as GenericSpell).SpellClass == CharacterStats.Class.Wizard)
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.WIZARD_SPELL_LEARNED);
			}
		}
		else if ((bool)GetGenericTalent(ability))
		{
			GetGenericTalent(ability).Purchase(characterStats.gameObject);
			result = ability;
		}
		else if ((bool)GetPhrase(ability))
		{
			GetChanterTrait(characterStats).AddKnownPhrase(GetPhrase(ability));
			result = ability;
		}
		else if (!GetRecipe(ability))
		{
			Debug.Log("AbilityProgressionTable ERROR: Ability " + ability.name + " added is not of type GenericAbility, GenericTalent, or Phrase!");
		}
		return result;
	}

	public static bool RemoveAbilityFromCharacter(GameObject ability, CharacterStats characterStats)
	{
		if ((bool)GetGenericAbility(ability))
		{
			return characterStats.RemoveAbility(ability.GetComponent<GenericAbility>());
		}
		if ((bool)GetGenericTalent(ability))
		{
			return characterStats.RemoveTalent(ability.GetComponent<GenericTalent>());
		}
		return false;
	}

	public static void AddAbilityToCoreData(GameObject ability, CharacterStats.CoreData coreData)
	{
		if ((bool)GetGenericAbility(ability))
		{
			if (coreData.GetKnownSkill(GetGenericAbility(ability)) == null)
			{
				coreData.KnownSkills.Add(GetGenericAbility(ability));
			}
		}
		else if ((bool)GetGenericTalent(ability))
		{
			if (coreData.GetKnownSkill(GetGenericTalent(ability)) == null)
			{
				coreData.KnownSkills.Add(GetGenericTalent(ability));
			}
		}
		else if ((bool)GetPhrase(ability))
		{
			if (coreData.GetKnownSkill(GetPhrase(ability)) == null)
			{
				coreData.KnownSkills.Add(GetPhrase(ability));
			}
		}
		else if (!GetRecipe(ability))
		{
			Debug.Log("AbilityProgressionTable ERROR: Ability " + ability.name + " added is not of type GenericAbility, GenericTalent, or Phrase!");
		}
	}
}
