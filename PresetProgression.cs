using UnityEngine;

public class PresetProgression : MonoBehaviour
{
	public int MinLevel = 1;

	public int MaxLevel = 1;

	[Tooltip("How far from the player's level should this preset progression be when exectuted? Use 0 if you want to match the player's level.")]
	public int DeltaFromPlayerLevel;

	[Tooltip("The prefix of the progression table to use for their preset progression. If the table name is \"TestAbilityProgressionTable\", then just input \"Test\".")]
	public string ProgressionTableName;

	[Persistent]
	private bool m_PresetProgressionSet;

	private AbilityProgressionTable m_ProgressionTable;

	public bool PresetProgressionHandled()
	{
		return m_PresetProgressionSet;
	}

	private bool VerifyData()
	{
		bool result = true;
		if (MinLevel > MaxLevel)
		{
			Debug.Log("PresetProgression Error: Min Level is higher than Max Level. " + base.gameObject.name + ".");
			result = false;
		}
		if (MaxLevel < 1 || MinLevel < 1)
		{
			Debug.Log("PresetProgression Error: Min and Max level should be greater than 0. " + base.gameObject.name + ".");
			result = false;
		}
		if (string.IsNullOrEmpty(ProgressionTableName))
		{
			Debug.Log("PresetProgression Error: Progression Table Name is not filled out. " + base.gameObject.name + ".");
			result = false;
		}
		return result;
	}

	private void Start()
	{
		VerifyData();
	}

	private void ResetCharacter(CharacterStats stats)
	{
		stats.Level = 0;
		stats.ClearAllProgressionAbilitiesAndTalents(stats.CharacterClass);
		stats.AddRacialAbilities();
		stats.AddPresetAbilities();
		stats.AddPresetTalents();
	}

	private void OnDestroy()
	{
		if (m_ProgressionTable != null)
		{
			GameResources.ClearPrefabReference(m_ProgressionTable);
			m_ProgressionTable = null;
		}
	}

	public void LoadPresetProgression()
	{
		if (m_ProgressionTable == null)
		{
			m_ProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(ProgressionTableName);
		}
	}

	private void AddSkillPoints(CharacterStats ownerStats, AbilityProgressionTable progressionTable)
	{
		ownerStats.RemainingSkillPoints = (ownerStats.Level - 1) * 6;
		int num = 0;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Stealth);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.StealthSkill += num;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Athletics);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.AthleticsSkill += num;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Lore);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.LoreSkill += num;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Mechanics);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.MechanicsSkill += num;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Survival);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.SurvivalSkill += num;
		num = m_ProgressionTable.GetSkillPointsToApply(0, ownerStats.Level, CharacterStats.SkillType.Crafting);
		ownerStats.RemainingSkillPoints -= num;
		ownerStats.CraftingSkill += num;
		if (ownerStats.RemainingSkillPoints < 0)
		{
			Debug.LogError("Preset Progression Error: Skill Points added to character add up to more than available to give!");
			ownerStats.RemainingSkillPoints = 0;
		}
	}

	public void HandlePresetProgression(bool forceFromStartingLevel)
	{
		if (!m_PresetProgressionSet || forceFromStartingLevel)
		{
			LoadPresetProgression();
		}
		if ((!forceFromStartingLevel && m_PresetProgressionSet) || !(m_ProgressionTable != null) || !VerifyData() || GameState.s_playerCharacter == null)
		{
			return;
		}
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if (!component)
		{
			return;
		}
		CharacterStats component2 = GetComponent<CharacterStats>();
		if (!component2)
		{
			return;
		}
		ResetCharacter(component2);
		GameObject gameObject = GameUtilities.FindAnimalCompanion(component2.gameObject);
		if (gameObject != null)
		{
			CharacterStats component3 = gameObject.GetComponent<CharacterStats>();
			if ((bool)component3)
			{
				component3.ClearAllProgressionAbilitiesAndTalents(component2.CharacterClass);
				ResetCharacter(component3);
			}
		}
		int maxLevelCanLevelUpTo = component.GetMaxLevelCanLevelUpTo();
		int num = Mathf.Clamp(maxLevelCanLevelUpTo + DeltaFromPlayerLevel, MinLevel, MaxLevel);
		component2.Experience = CharacterStats.ExperienceNeededForLevel(num);
		if (num == maxLevelCanLevelUpTo)
		{
			int num2 = (int)((float)(component.Experience - CharacterStats.ExperienceNeededForLevel(maxLevelCanLevelUpTo)) * 0.9f);
			component2.Experience += num2;
		}
		int num3 = ((!GameState.Option.GetOption(GameOption.BoolOption.AUTO_LEVEL_COMPANIONS) || forceFromStartingLevel) ? 1 : num);
		if (num3 > 0)
		{
			component2.Level = num3;
			AbilityProgressionTable.UnlockableAbility[] abilities = m_ProgressionTable.GetAbilities(component2, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
			AbilityProgressionTable.UnlockableAbility[] array = abilities;
			foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility in array)
			{
				if (!(unlockableAbility.Ability == null))
				{
					AbilityProgressionTable.AddAbilityToCharacter(unlockableAbility, component2);
				}
			}
			abilities = m_ProgressionTable.GetAbilities(component2, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoMasterFilterFlags);
			if (abilities != null)
			{
				array = abilities;
				foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility2 in array)
				{
					if (!(unlockableAbility2.Ability == null))
					{
						GenericAbility component4 = unlockableAbility2.Ability.GetComponent<GenericAbility>();
						if ((bool)component4)
						{
							GenericAbility.MasterAbility(component2, component4);
						}
					}
				}
			}
			AddSkillPoints(component2, m_ProgressionTable);
		}
		else
		{
			Equipment component5 = component2.GetComponent<Equipment>();
			if ((bool)component5 && component5.CurrentItems != null && (bool)component5.CurrentItems.Grimoire)
			{
				Equippable grimoire = component5.CurrentItems.Grimoire;
				if ((bool)grimoire)
				{
					Grimoire component6 = grimoire.GetComponent<Grimoire>();
					if ((bool)component6)
					{
						component6.RemoveAllSpells();
					}
				}
			}
		}
		GameResources.ClearPrefabReference(m_ProgressionTable);
		m_ProgressionTable = null;
		m_PresetProgressionSet = true;
		Health component7 = GetComponent<Health>();
		if ((bool)component7)
		{
			component7.CurrentHealth = component7.MaxHealth;
			component7.CurrentStamina = component7.MaxStamina;
		}
		if (gameObject != null)
		{
			CharacterStats component8 = gameObject.GetComponent<CharacterStats>();
			if ((bool)component8)
			{
				component8.LevelUpToLevel(component2.Level);
			}
		}
	}
}
