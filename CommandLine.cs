using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CommandLine
{
	private class ObjectComparer : IComparer<GameObject>
	{
		private static ObjectComparer s_instance;

		public static ObjectComparer Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new ObjectComparer();
				}
				return s_instance;
			}
		}

		public int Compare(GameObject a, GameObject b)
		{
			if (a == null || b == null)
			{
				return 0;
			}
			int hashCode = a.GetType().GetHashCode();
			int hashCode2 = b.GetType().GetHashCode();
			if (hashCode > hashCode2)
			{
				return 1;
			}
			if (hashCode == hashCode2)
			{
				return 0;
			}
			return -1;
		}
	}

	private static bool m_CkOn;

	public static void ToggleCursorDebug()
	{
		GameCursor.ShowDebug = !GameCursor.ShowDebug;
	}

	public static void ToggleGameStateDebug()
	{
		GameState.ShowDebug = !GameState.ShowDebug;
	}

	public static void ToggleScriptHistory()
	{
		ScriptEvent.DisplayRecentScripts = !ScriptEvent.DisplayRecentScripts;
	}

	public static void ToggleMusicDebug()
	{
		MusicManager.OnscreenDebug = !MusicManager.OnscreenDebug;
	}

	public static void ToggleResolutionDebug()
	{
		GameUtilities.ShowResolutionDebug = !GameUtilities.ShowResolutionDebug;
	}

	public static void SetExactGlossaryEnabled(bool val)
	{
		if (GameState.Option != null)
		{
			GameState.Option.SetOption(GameOption.BoolOption.GLOSSARY_EXACT, val);
			GameState.Option.SaveToPrefs();
		}
	}

	[Cheat]
	public static void CraftingDebug()
	{
		Recipe[] recipePrefabs = GameUtilities.Instance.RecipePrefabs;
		for (int i = 0; i < recipePrefabs.Length; i++)
		{
			RecipeIngredient[] ingredients = recipePrefabs[i].Ingredients;
			foreach (RecipeIngredient recipeIngredient in ingredients)
			{
				AddItem(recipeIngredient.RecipeItem.name, recipeIngredient.Quantity * 3);
			}
		}
		Console.AddMessage("You magically conjure crafting supplies of all kinds.");
	}

	public static void TooltipCombatAbilities(bool state)
	{
		UICombatTooltipManager.Instance.ShowAbilityTooltips = state;
	}

	[Cheat]
	public static void StrongholdBuildAll()
	{
		for (StrongholdUpgrade.Type type = StrongholdUpgrade.Type.Barbican; type < StrongholdUpgrade.Type.Count; type++)
		{
			Stronghold.Instance.DebugBuildUpgrade(type);
		}
		Console.AddMessage("All stronghold upgrades completed.");
	}

	[Cheat]
	public static void StrongholdForceSpawnIngredients()
	{
		StrongholdEvent.ProcessSpawnIngredients(GameState.s_playerCharacter, Stronghold.Instance);
	}

	[Cheat]
	public static void StrongholdForcePayHirelings()
	{
		StrongholdEvent.ProcessPayHirelings(GameState.s_playerCharacter, Stronghold.Instance);
	}

	[Cheat]
	public static void StrongholdForceKidnapping()
	{
		StrongholdEvent.HandleKidnapping(GameState.s_playerCharacter, Stronghold.Instance);
	}

	[Cheat]
	public static void StrongholdForceBadVisitor()
	{
		StrongholdEvent.HandleBadVisitor(GameState.s_playerCharacter, Stronghold.Instance);
	}

	[Cheat]
	public static void StrongholdForceAttack(int index)
	{
		StrongholdEvent.DebugForceAttack(GameState.s_playerCharacter, Stronghold.Instance, index);
	}

	[Cheat]
	public static void AdvanceDay()
	{
		Scripts.AdvanceTimeByHours(WorldTime.Instance.HoursPerDay);
	}

	[Cheat]
	public static void StrongholdBuild(StrongholdUpgrade.Type type)
	{
		Stronghold.Instance.DebugBuildUpgrade(type);
	}

	[Cheat]
	public static void StrongholdDestroy(StrongholdUpgrade.Type type)
	{
		Stronghold.Instance.DestroyUpgrade(type);
	}

	[Cheat]
	public static void StrongholdForceAdventure(StrongholdAdventure.Type type)
	{
		Stronghold.Instance.AddAdventure(type);
	}

	public static void ShowPrisoners()
	{
		if (!(GameState.s_playerCharacter != null))
		{
			return;
		}
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			int num = stronghold.PrisonerCount();
			Console.AddMessage("Stronghold prisoner count: " + num);
			for (int i = 0; i < num; i++)
			{
				Console.AddMessage("   " + num + ". " + stronghold.PrisonerName(i));
			}
		}
	}

	public static void BloodyMess()
	{
		Health.BloodyMess = !Health.BloodyMess;
		PostToggleMessage("Bloody Mess", Health.BloodyMess);
	}

	public static void ToggleIntroDebug()
	{
		FrontEndTitleIntroductionManager.ShowDebug = !FrontEndTitleIntroductionManager.ShowDebug;
	}

	public static void ToggleAchievementDebug()
	{
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.ShowDebugInfo = !AchievementTracker.Instance.ShowDebugInfo;
		}
	}

	public static void ToggleGhostDebug()
	{
		Ghost.Debug = !Ghost.Debug;
		PostToggleMessage("Ghost debugging", Ghost.Debug);
	}

	[Cheat]
	public static void SkillCheckDebug(bool value)
	{
		SpecialCharacterInstanceID.SkillCheckDebugEnabled = value;
		PostToggleMessage("Skill check debugging", SpecialCharacterInstanceID.SkillCheckDebugEnabled);
	}

	public static void HideGraze()
	{
		Health.HideGrazes = !Health.HideGrazes;
		if (Health.HideGrazes)
		{
			Console.AddMessage("Grazes hidden.");
		}
		else
		{
			Console.AddMessage("Grazes visible.");
		}
	}

	public static void ToggleScaler(DifficultyScaling.Scaler scaler)
	{
		DifficultyScaling.Instance.ToggleScaler(scaler);
		ShowScalers();
	}

	public static void ShowScalers()
	{
		Array values = Enum.GetValues(typeof(DifficultyScaling.Scaler));
		string text = "";
		for (int i = 0; i < values.Length; i++)
		{
			if (DifficultyScaling.Instance.IsAnyScalerActive((DifficultyScaling.Scaler)values.GetValue(i)))
			{
				text = text + ((DifficultyScaling.Scaler)values.GetValue(i)).ToString() + " ";
			}
		}
		Console.AddMessage("Active Scalers: " + ((!string.IsNullOrEmpty(text)) ? text : "<none>"));
	}

	[Cheat]
	public static void LevelUpSoulbind(Guid guid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(guid);
		if (!objectByID)
		{
			return;
		}
		EquipmentSoulbind component = objectByID.GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			if (component.IsBound)
			{
				component.DebugLevelUp();
			}
			else
			{
				Console.AddMessage("'" + objectByID.name + "' is not bound.", Color.yellow);
			}
		}
		else
		{
			Console.AddMessage("'" + objectByID.name + "' is not bindable.", Color.yellow);
		}
	}

	[Cheat]
	public static void SetDeity(Guid character, Religion.Deity deity)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if ((bool)characterStatsComponent)
		{
			Console.AddMessage(string.Concat("Changing '", characterStatsComponent.name, "' deity from '", characterStatsComponent.Deity, "' to '", deity, "'."));
			characterStatsComponent.Deity = deity;
		}
	}

	[Cheat]
	public static void SetPaladinOrder(Guid character, Religion.PaladinOrder order)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if ((bool)characterStatsComponent)
		{
			Console.AddMessage(string.Concat("Changing '", characterStatsComponent.name, "' order from '", characterStatsComponent.PaladinOrder, "' to '", order, "'."));
			characterStatsComponent.PaladinOrder = order;
		}
	}

	[Cheat]
	public static void SetClass(Guid character, CharacterStats.Class cl)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if ((bool)characterStatsComponent)
		{
			Console.AddMessage(string.Concat("Changing '", characterStatsComponent.name, "' class from '", characterStatsComponent.CharacterClass, "' to '", cl, "'."));
			characterStatsComponent.CharacterClass = cl;
		}
	}

	[Cheat]
	public static void ReactivateStronghold()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Disabled = false;
			PostToggleMessage("StrongholdDisabled", stronghold.Disabled);
		}
	}

	[Cheat]
	public static void UnlockBiography()
	{
		JournalBiographyManager.Instance.UnlockAll();
	}

	private static void PostToggleMessage(string name, bool state)
	{
		if (state)
		{
			Console.AddMessage(name + " is now ON.");
		}
		else
		{
			Console.AddMessage(name + " is now OFF.");
		}
	}

	public static void Test(string message)
	{
		Console.AddMessage("Test was " + message + "!!!!");
	}

	public static void IRoll20s()
	{
		GameState.Instance.CheatsEnabled = !GameState.Instance.CheatsEnabled;
		if (GameState.Instance.CheatsEnabled && AchievementTracker.Instance != null)
		{
			AchievementTracker.Instance.DisableAchievements = true;
		}
		if (GameState.Instance.CheatsEnabled)
		{
			Console.AddMessage("Cheats Enabled - Warning - Achievements disabled for this game.");
		}
		else
		{
			Console.AddMessage("Cheats Disabled");
		}
	}

	[Cheat]
	public static void LoadLevel(string name)
	{
		Console.AddMessage("Attemping to load " + name);
		UnityEngine.Debug.Log("\n");
		UnityEngine.Debug.Log("------- BEGIN LEVEL LOAD INITIATED --------        Pending level = " + name + ".");
		SceneManager.LoadScene(name);
	}

	[Cheat]
	public static void God()
	{
		object[] array = UnityEngine.Object.FindObjectsOfType(typeof(PartyMemberAI));
		array = array;
		for (int i = 0; i < array.Length; i++)
		{
			Health component = ((PartyMemberAI)array[i]).GetComponent<Health>();
			if ((bool)component)
			{
				component.TakesDamage = !component.TakesDamage;
				if (!component.TakesDamage)
				{
					Console.AddMessage(component.name + " is now invulnerable!");
				}
				else
				{
					Console.AddMessage(component.name + " is no longer invulnerable!");
				}
			}
		}
	}

	[Cheat]
	public static void NoDam()
	{
		NoDamage(invulnerable: true);
	}

	[Cheat]
	public static void NoDamage(bool invulnerable)
	{
		Health.NoDamage = invulnerable;
		PostToggleMessage("nodamage", invulnerable);
	}

	[Cheat]
	public static void Damage(float amount)
	{
		GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
		foreach (GameObject gameObject in selectedPartyMembers)
		{
			if (!(gameObject == null))
			{
				Health component = gameObject.GetComponent<Health>();
				if ((bool)component)
				{
					component.ApplyStaminaChangeDirectly(0f - amount, applyIfDead: false);
				}
			}
		}
	}

	[Cheat]
	public static void Aggression(string name, string value)
	{
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		if (string.Compare(name, "player", ignoreCase: true) == 0)
		{
			if (GameState.s_playerCharacter == null)
			{
				UnityEngine.Debug.Log("Skill: Error - player character not found.");
				return;
			}
			list.Add(GameState.s_playerCharacter.GetComponent<PartyMemberAI>());
		}
		else if (string.Compare(name, "all", ignoreCase: true) == 0)
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (partyMemberAI != null)
				{
					list.Add(partyMemberAI);
				}
			}
		}
		else
		{
			PartyMemberAI[] array = UnityEngine.Object.FindObjectsOfType<PartyMemberAI>();
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].gameObject.name.ToLower().Contains(name.ToLower()))
				{
					list.Add(array[j]);
					break;
				}
			}
		}
		if (list.Count <= 0)
		{
			UnityEngine.Debug.Log("Skill: Error - stats component not found for " + name);
			return;
		}
		AIController.AggressionType aggression = AIController.AggressionType.Passive;
		if (string.Compare(value, AIController.AggressionType.Aggressive.ToString(), ignoreCase: true) == 0)
		{
			aggression = AIController.AggressionType.Aggressive;
		}
		else if (string.Compare(value, AIController.AggressionType.Defensive.ToString(), ignoreCase: true) == 0)
		{
			aggression = AIController.AggressionType.Defensive;
		}
		else if (string.Compare(value, AIController.AggressionType.DefendMyself.ToString(), ignoreCase: true) == 0)
		{
			aggression = AIController.AggressionType.DefendMyself;
		}
		else if (string.Compare(value, AIController.AggressionType.Passive.ToString(), ignoreCase: true) == 0)
		{
			aggression = AIController.AggressionType.Passive;
		}
		else
		{
			UnityEngine.Debug.Log("Aggression: Error - could not find aggression " + value);
		}
		foreach (PartyMemberAI item in list)
		{
			item.Aggression = aggression;
			item.UpdateAggressionOfSummonedCreatures(includeCompanion: true);
		}
		Console.AddMessage("Aggression for " + name + " is now " + aggression);
	}

	[Cheat]
	public static void UseInstructionSet(string name, int value)
	{
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		if (string.Compare(name, "player", ignoreCase: true) == 0)
		{
			if (GameState.s_playerCharacter == null)
			{
				UnityEngine.Debug.Log("Skill: Error - player character not found.");
				return;
			}
			list.Add(GameState.s_playerCharacter.GetComponent<PartyMemberAI>());
		}
		else if (string.Compare(name, "all", ignoreCase: true) == 0)
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (partyMemberAI != null)
				{
					list.Add(partyMemberAI);
				}
			}
		}
		else
		{
			PartyMemberAI[] array = UnityEngine.Object.FindObjectsOfType<PartyMemberAI>();
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].gameObject.name.ToLower().Contains(name.ToLower()))
				{
					list.Add(array[j]);
					break;
				}
			}
		}
		if (list.Count <= 0)
		{
			UnityEngine.Debug.Log("Skill: Error - stats component not found for " + name);
			return;
		}
		bool useInstructionSet = value != 0;
		foreach (PartyMemberAI item in list)
		{
			item.UseInstructionSet = useInstructionSet;
		}
		Console.AddMessage("Use instruction set for " + name + " is now " + useInstructionSet);
	}

	[Cheat]
	public static void SetInstructionSet(string name, int value)
	{
		PartyMemberAI partyMemberAI = null;
		if (string.Compare(name, "player", ignoreCase: true) == 0)
		{
			if (GameState.s_playerCharacter == null)
			{
				UnityEngine.Debug.Log("Skill: Error - player character not found.");
				return;
			}
			partyMemberAI = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
		}
		else
		{
			PartyMemberAI[] array = UnityEngine.Object.FindObjectsOfType<PartyMemberAI>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.name.ToLower().Contains(name.ToLower()))
				{
					partyMemberAI = array[i];
					break;
				}
			}
		}
		if (partyMemberAI == null)
		{
			UnityEngine.Debug.Log("Skill: Error - stats component not found for " + name);
			return;
		}
		partyMemberAI.SetInstructionSetIndex(value);
		Console.AddMessage("Instruction set index for " + name + " is now " + value);
	}

	[Cheat]
	public static void Invisible()
	{
		object[] array = UnityEngine.Object.FindObjectsOfType(typeof(PartyMemberAI));
		array = array;
		for (int i = 0; i < array.Length; i++)
		{
			Health component = ((PartyMemberAI)array[i]).GetComponent<Health>();
			if ((bool)component)
			{
				component.Targetable = !component.Targetable;
				if (component.Targetable)
				{
					Console.AddMessage(component.name + " is now visible!");
				}
				else
				{
					Console.AddMessage(component.name + " is invisible!");
				}
			}
		}
	}

	[Cheat]
	public static void SetStaminaRechargeDelay(float newTime)
	{
		CharacterStats.StaminaRechargeDelay = newTime;
	}

	[Cheat]
	public static void UnlockAll()
	{
		OCL[] array = UnityEngine.Object.FindObjectsOfType<OCL>();
		foreach (OCL oCL in array)
		{
			if (oCL.CurrentState == OCL.State.Locked)
			{
				oCL.Unlock(null);
				Console.AddMessage(oCL.name + " has been unlocked!");
			}
		}
	}

	[Cheat]
	public static void RevealAll()
	{
		Detectable[] array = UnityEngine.Object.FindObjectsOfType<Detectable>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Detect(GameState.s_playerCharacter.gameObject);
		}
	}

	[Cheat]
	public static void GiveMoney(int amount)
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (!(s_playerCharacter == null))
		{
			PlayerInventory component = s_playerCharacter.GetComponent<PlayerInventory>();
			if (!(component == null))
			{
				component.currencyTotalValue.v += amount;
			}
		}
	}

	public static void ClearAchievements()
	{
	}

	public static void Find(string name)
	{
		bool flag = false;
		Console.AddMessage("");
		foreach (MethodInfo nonCheatMethod in CommandLineRun.GetNonCheatMethods())
		{
			if (nonCheatMethod.Name.ToLower().Contains(name.ToLower()))
			{
				Console.AddMessage(nonCheatMethod.Name + " " + TextUtils.FuncJoin((ParameterInfo pi) => "(" + pi.ParameterType.Name + ")" + pi.Name, nonCheatMethod.GetParameters(), " "));
				flag = true;
			}
		}
		foreach (MethodInfo cheatMethod in CommandLineRun.GetCheatMethods())
		{
			if (cheatMethod.Name.ToLower().Contains(name.ToLower()))
			{
				Console.AddMessage("[FFFF00][cheat] " + cheatMethod.Name + " " + TextUtils.FuncJoin((ParameterInfo pi) => "(" + pi.ParameterType.Name + ")" + pi.Name, cheatMethod.GetParameters(), " "));
				flag = true;
			}
		}
		if (!flag)
		{
			Console.AddMessage("No commands found with the substring '" + name + "'.");
		}
	}

	public static void FindObject(string name)
	{
		Console.AddMessage("");
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
		foreach (Transform transform in array)
		{
			if (transform.name.ToLower().Contains(name.ToLower()))
			{
				Console.AddMessage(transform.name);
			}
		}
	}

	public static void FindCharacter(string name)
	{
		Console.AddMessage("");
		CharacterStats[] array = UnityEngine.Object.FindObjectsOfType<CharacterStats>();
		foreach (CharacterStats characterStats in array)
		{
			if (characterStats.name.ToLower().Contains(name.ToLower()) || CharacterStats.Name(characterStats).ToLower().Contains(name.ToLower()))
			{
				Console.AddMessage(characterStats.name);
			}
		}
	}

	[Cheat]
	public static void Difficulty(GameDifficulty setting)
	{
		GameState.Instance.Difficulty = setting;
		Console.AddMessage("Game difficulty set to '" + setting.ToString() + "'.");
	}

	[Cheat]
	public static void ChallengeMode(string setting)
	{
		if (setting.ToLower().Equals("expert"))
		{
			GameState.Mode.Expert = !GameState.Mode.Expert;
			PostToggleMessage("Expert Mode", GameState.Mode.Expert);
		}
		else if (setting.ToLower().Equals("trial"))
		{
			GameState.Mode.TrialOfIron = !GameState.Mode.TrialOfIron;
			PostToggleMessage("Trial of Iron", GameState.Mode.TrialOfIron);
		}
		else
		{
			Console.AddMessage("No such game mode: " + setting);
		}
	}

	[Cheat]
	public static void UnlockBestiary()
	{
		BestiaryManager.Instance.CheatMaxAll();
		Console.AddMessage("All bestiary entries maxed.");
	}

	[Cheat]
	public static void NoFog()
	{
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueDisable();
		}
		Console.AddMessage("Your spell has mysteriously cleared the fog.");
	}

	public static void PrintGlobal(string name)
	{
		Console.AddMessage("'" + name + "': " + GlobalVariables.Instance.GetVariable(name));
	}

	public static void PopToGround(bool isEnabled)
	{
		Mover.PopToGround = isEnabled;
		UnityEngine.Debug.Log("Pop to ground set to " + Mover.PopToGround);
	}

	public static void Bug()
	{
		CrashLogging.QueueBugCommand(GameResources.GetCrashBuddyRegionAreaArguments());
	}

	public static void ExportGlobals()
	{
		CrashLogging.QueueExportGlobalsCommand();
	}

	[Cheat]
	public static void LearnAllAbilities(Guid character, string table)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if (!characterStatsComponent)
		{
			return;
		}
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(table);
		if ((bool)abilityProgressionTable)
		{
			AbilityProgressionTable.UnlockableAbility[] abilityUnlocks = abilityProgressionTable.AbilityUnlocks;
			for (int i = 0; i < abilityUnlocks.Length; i++)
			{
				AbilityProgressionTable.AddAbilityToCharacter(abilityUnlocks[i], characterStatsComponent);
			}
		}
		else
		{
			Console.AddMessage("No ability progression table '" + table + "'.");
		}
		GameResources.ClearPrefabReferences(typeof(AbilityProgressionTable));
		Resources.UnloadUnusedAssets();
	}

	public static void StringDebug(bool setting)
	{
		StringTableManager.StringDebug = setting;
		PostToggleMessage("String debugging", StringTableManager.StringDebug);
	}

	[Cheat]
	public static void AddItem(string itemName, int count)
	{
		Scripts.GiveItem(itemName, count);
	}

	[Cheat]
	public static void AttributeScore(Guid character, CharacterStats.AttributeScoreType attribute, int score)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if (characterStatsComponent == null)
		{
			UnityEngine.Debug.Log("Attribute Score: Error - stats component not found for '" + character.ToString() + "'.");
			return;
		}
		switch (attribute)
		{
		case CharacterStats.AttributeScoreType.Resolve:
			characterStatsComponent.BaseResolve = score;
			break;
		case CharacterStats.AttributeScoreType.Might:
			characterStatsComponent.BaseMight = score;
			break;
		case CharacterStats.AttributeScoreType.Dexterity:
			characterStatsComponent.BaseDexterity = score;
			break;
		case CharacterStats.AttributeScoreType.Intellect:
			characterStatsComponent.BaseIntellect = score;
			break;
		case CharacterStats.AttributeScoreType.Constitution:
			characterStatsComponent.BaseConstitution = score;
			break;
		case CharacterStats.AttributeScoreType.Perception:
			characterStatsComponent.BasePerception = score;
			break;
		}
		Console.AddMessage(characterStatsComponent.name + "'s " + attribute.ToString() + " is now " + score);
	}

	[Cheat]
	public static void Skill(Guid character, CharacterStats.SkillType skill, int score)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(character);
		if (characterStatsComponent == null)
		{
			UnityEngine.Debug.Log(string.Concat("Skill: Error - stats component not found for '", character, "'."));
			return;
		}
		switch (skill)
		{
		case CharacterStats.SkillType.Stealth:
			characterStatsComponent.StealthBonus = score;
			break;
		case CharacterStats.SkillType.Athletics:
			characterStatsComponent.AthleticsBonus = score;
			break;
		case CharacterStats.SkillType.Lore:
			characterStatsComponent.LoreBonus = score;
			break;
		case CharacterStats.SkillType.Mechanics:
			characterStatsComponent.MechanicsBonus = score;
			break;
		case CharacterStats.SkillType.Survival:
			characterStatsComponent.SurvivalBonus = score;
			break;
		case CharacterStats.SkillType.Crafting:
			characterStatsComponent.CraftingBonus = score;
			break;
		}
		Console.AddMessage(string.Concat(characterStatsComponent.name, "'s ", skill, " is now ", score.ToString()));
	}

	[Cheat]
	public static void AddAbility(Guid character, string abilityName)
	{
		AbilityProgressionTable.AddAbilityToCharacter(abilityName, Scripts.GetCharacterStatsComponent(character));
	}

	[Cheat]
	public static void RemoveAbility(GameObject target, string abilityName)
	{
		CharacterStats component = target.GetComponent<CharacterStats>();
		if (component == null)
		{
			UnityEngine.Debug.LogWarning("RemoveAbility: Error - stats component not found for " + component.Name());
			return;
		}
		GameObject gameObject = GameResources.LoadPrefab<GameObject>(abilityName, instantiate: false);
		if ((bool)gameObject)
		{
			if (AbilityProgressionTable.RemoveAbilityFromCharacter(gameObject, component))
			{
				Console.AddMessage(GUIUtils.Format(1813, CharacterStats.NameColored(target), AbilityProgressionTable.GetAbilityName(gameObject)));
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("RemoveAbility: Error - could not find ability: " + abilityName);
		}
	}

	[Cheat]
	public static void SetTime(string time)
	{
		int num = int.Parse(time);
		int num2 = WorldTime.Instance.SecondsPerMinute * WorldTime.Instance.MinutesPerHour;
		int num3 = WorldTime.Instance.CurrentTime.Second + WorldTime.Instance.CurrentTime.Minute * WorldTime.Instance.SecondsPerMinute + WorldTime.Instance.CurrentTime.Hour * num2;
		int num4 = (int)Mathf.Floor((float)num2 * ((float)num / 1000f) - (float)num3);
		if (num4 < 0)
		{
			num4 += num2 * WorldTime.Instance.HoursPerDay;
		}
		WorldTime.Instance.CurrentTime.AddSeconds(num4);
	}

	public static void CameraMoveDelta(string value)
	{
		GameState.Option.ScrollSpeed = float.Parse(value);
		GameState.Option.SaveToPrefs();
	}

	public static void TogglePartyDebug()
	{
		PartyMemberAI.DebugParty = !PartyMemberAI.DebugParty;
		PostToggleMessage("Party debugging", PartyMemberAI.DebugParty);
	}

	public static void Chipmunk()
	{
		m_CkOn = !m_CkOn;
		AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].pitch = (m_CkOn ? 1.75f : 1f);
		}
	}

	public static void ToggleCharacterStatsDebug()
	{
		CharacterStats.DebugStats = !CharacterStats.DebugStats;
		PostToggleMessage("CharacterStats debugging", CharacterStats.DebugStats);
	}

	[Cheat]
	public static void ToggleSpellLimit()
	{
		GameState.Instance.IgnoreSpellLimits = !GameState.Instance.IgnoreSpellLimits;
		PostToggleMessage("no_spell_limits", GameState.Instance.IgnoreSpellLimits);
	}

	[Cheat]
	public static void ToggleNeedsGrimoire()
	{
		GameState.Instance.IgnoreInGrimoire = !GameState.Instance.IgnoreInGrimoire;
		PostToggleMessage("no_grimoire", GameState.Instance.IgnoreInGrimoire);
	}

	[Cheat]
	public static void UnlockAllMaps()
	{
		foreach (MapType value in Enum.GetValues(typeof(MapType)))
		{
			MapData map = WorldMap.Instance.GetMap(value);
			if (map != null && map.Visibility != MapData.VisibilityType.DeveloperOnly)
			{
				map.Visibility = MapData.VisibilityType.Unlocked;
			}
		}
	}

	public static void DeletePrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	[Cheat]
	public static void FreeRecipesToggle()
	{
		Recipe.FreeRecipes = !Recipe.FreeRecipes;
		PostToggleMessage("free_recipes", Recipe.FreeRecipes);
	}

	public static void HelmetVisibility(bool state)
	{
		GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
		foreach (GameObject gameObject in selectedPartyMembers)
		{
			if ((bool)gameObject)
			{
				gameObject.GetComponent<NPCAppearance>().SetHelmetVisibility(state);
			}
		}
	}

	[Cheat]
	public static void ManageParty()
	{
		UIPartyManager.Instance.ToggleAlt();
	}

	public static void LockMapTooltips(bool state)
	{
		UIMapTooltipManager.Locked = state;
		PostToggleMessage("Map tooltips locked", UIMapTooltipManager.Locked);
	}

	public static void SetZoomRange(float min, float max)
	{
		GameState.Option.MinZoom = Mathf.Clamp(min, 0.1f, 3f);
		GameState.Option.MaxZoom = Mathf.Clamp(max, 0.1f, 3f);
		GameState.Option.SaveToPrefs();
	}

	public static void UseStaticWindowSize(bool isStatic)
	{
		SyncCameraOrthoSettings.m_staticWindowSize = isStatic;
	}

	public static void AuditSaveGame()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MOBILE DATA");
		foreach (ObjectPersistencePacket value in PersistenceManager.MobileObjects.Values)
		{
			stringBuilder.AppendLine(value.ObjectName + "," + value.Parent + "," + value.LevelName + "," + value.GUID);
		}
		stringBuilder.AppendLine("LEVEL DATA");
		foreach (ObjectPersistencePacket value2 in PersistenceManager.PersistentObjects.Values)
		{
			stringBuilder.AppendLine(value2.ObjectName + "," + value2.LevelName + "," + value2.GUID);
		}
		try
		{
			string saveGamePath = GameResources.SaveGamePath;
			saveGamePath = Path.Combine(saveGamePath, "SaveGameAudit");
			string text = saveGamePath;
			int num = 1;
			while (File.Exists(text + ".csv"))
			{
				text = saveGamePath + num.ToString("#00");
				num++;
			}
			saveGamePath = text + ".csv";
			File.WriteAllText(saveGamePath, stringBuilder.ToString());
			Process.Start(saveGamePath);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	public static void ClipCursor(bool active)
	{
		GameState.Mode.Option.SetOption(GameOption.BoolOption.CLIP_CURSOR, active);
		GameState.Mode.Option.SaveToPrefs();
		WinCursor.Clip(state: true);
		PostToggleMessage("Clip cursor", GameState.Mode.Option.GetOption(GameOption.BoolOption.CLIP_CURSOR));
	}

	public static void GetTransform(string objectName)
	{
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		string text = "";
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject.name != objectName) && gameObject != null)
			{
				text = text + objectName + " Transform (Parent: " + gameObject.transform.parent.gameObject.name + "):\nLocal Position: " + gameObject.transform.localPosition.x + ", " + gameObject.transform.localPosition.x + ", " + gameObject.transform.localPosition.z + "\nLocal Rotation: " + gameObject.transform.localRotation.eulerAngles.x + ", " + gameObject.transform.localRotation.eulerAngles.y + ", " + gameObject.transform.localRotation.eulerAngles.z + "\nLocal Scale: " + gameObject.transform.localScale.x + ", " + gameObject.transform.localScale.y + ", " + gameObject.transform.localScale.z + "\nWorld Position: " + gameObject.transform.position.x + ", " + gameObject.transform.position.x + ", " + gameObject.transform.position.z + "\nWorld Rotation: " + gameObject.transform.rotation.eulerAngles.x + ", " + gameObject.transform.rotation.eulerAngles.y + ", " + gameObject.transform.rotation.eulerAngles.z + "\nWorld Scale: " + gameObject.transform.lossyScale.x + ", " + gameObject.transform.lossyScale.y + ", " + gameObject.transform.lossyScale.z + "\n\n";
			}
		}
		UnityEngine.Debug.Log(text);
		UIDebug.Instance.SetText("TransformDebug", text, Color.white, 0.05f, 0.95f);
		UIDebug.Instance.SetTextPosition("TransformDebug", 0.05f, 0.95f, UIWidget.Pivot.TopLeft);
	}

	[Cheat]
	public static void FowDebug(bool value)
	{
		if ((bool)FogOfWar.Instance)
		{
			FogOfWar.Instance.DebugActive = value;
			PostToggleMessage("FOW debugging", FogOfWar.Instance.DebugActive);
		}
	}

	public static void SetMaxFPS(int value)
	{
		Application.targetFrameRate = value;
	}

	public static void PrintMaxFPS()
	{
		Console.AddMessage("FPS Cap: " + Application.targetFrameRate);
	}

	public static void Msaa(int value)
	{
		QualitySettings.antiAliasing = value;
	}

	public static void PrintMsaa()
	{
		Console.AddMessage("MSAA: " + QualitySettings.antiAliasing);
	}

	private static string GetObjectDebugString()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
		List<UnityEngine.Object> list = new List<UnityEngine.Object>(array);
		int count = list.FindAll((UnityEngine.Object obj) => obj as GameObject != null).Count;
		int count2 = list.FindAll((UnityEngine.Object obj) => obj as Component != null).Count;
		int num = array.Length - count - count2;
		string text = "-----------------------------------------------------------------------------------------------------------\n";
		text = text + " BEGIN OBJECT DEBUG: Count = " + array.Length + ", GameObject = " + count + ", Component = " + count2 + ", Other = " + num + "\n";
		text += "-----------------------------------------------------------------------------------------------------------------\n\n";
		list.Sort(delegate(UnityEngine.Object a, UnityEngine.Object b)
		{
			if (a == null || b == null)
			{
				return 0;
			}
			int hashCode = a.GetType().GetHashCode();
			int hashCode2 = b.GetType().GetHashCode();
			if (hashCode > hashCode2)
			{
				return 1;
			}
			return (hashCode != hashCode2) ? (-1) : 0;
		});
		foreach (UnityEngine.Object item in list)
		{
			if (item == null)
			{
				text += "\t- NULL OBJECT\n";
				continue;
			}
			text = string.Concat(text, "  - Type: ", item.GetType(), ",   Name: ", item.name);
			Transform transform = null;
			if (item is GameObject)
			{
				transform = (item as GameObject).transform;
			}
			else if (item is Component)
			{
				transform = (item as Component).transform;
			}
			while (transform != null && transform.parent != null)
			{
				transform = transform.parent;
			}
			if ((bool)transform)
			{
				text = text + ",   Parent: " + transform.name;
			}
			text += "\n";
		}
		text += "------------------------------------\n";
		text += "         END OBJECT DEBUG \n";
		return text + "------------------------------------\n";
	}

	public static void LogObjectDebug()
	{
		UnityEngine.Debug.Log(GetObjectDebugString());
	}

	public static void Eval(string conditional)
	{
		if (string.IsNullOrEmpty(conditional))
		{
			return;
		}
		IList<string> list = StringUtility.CommandLineStyleSplit(conditional);
		MethodInfo[] methods = typeof(Conditionals).GetMethods();
		string error = "";
		foreach (MethodInfo item in (IEnumerable<MethodInfo>)methods)
		{
			if (string.Compare(item.Name, list[0], ignoreCase: true) == 0)
			{
				if (CommandLineRun.FillMethodParams(item, list, out var param, out error))
				{
					object obj = item.Invoke(null, param);
					Console.AddMessage(conditional + " = " + (obj is bool && (bool)obj));
					return;
				}
				break;
			}
		}
		Console.AddMessage("Conditional '" + list[0] + "' parameter error: " + error, Color.yellow);
	}

	public static void FindConditional(string name)
	{
		bool flag = false;
		Console.AddMessage("");
		foreach (MethodInfo item in (IEnumerable<MethodInfo>)typeof(Conditionals).GetMethods())
		{
			if (item.Name.ToLower().Contains(name.ToLower()))
			{
				Console.AddMessage(item.Name + " " + TextUtils.FuncJoin((ParameterInfo pi) => pi.Name, item.GetParameters(), " "));
				flag = true;
			}
		}
		if (!flag)
		{
			Console.AddMessage("No conditionals found with the substring '" + name + "'.");
		}
	}

	[Cheat]
	public static void KillAllEnemies()
	{
		for (int num = Faction.ActiveFactionComponents.Count - 1; num >= 0; num--)
		{
			if (Faction.ActiveFactionComponents[num].RelationshipToPlayer == Faction.Relationship.Hostile)
			{
				Health component = Faction.ActiveFactionComponents[num].GetComponent<Health>();
				if ((bool)component)
				{
					component.CurrentStamina = 0f;
					component.CurrentHealth = 0f;
					component.ApplyDamageDirectly(0f);
				}
			}
		}
	}

	public static void SetMaxAutosaves(int quantity)
	{
		GameState.Option.MaxAutosaves = quantity;
		Console.AddMessage(quantity + " autosaves will be kept per character.");
	}

	public static void RecacheSavegames()
	{
		SaveGameInfo.RecacheSaveGameInfo();
	}

	[Cheat]
	public static void TransferControl(Guid targetGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(targetGuid);
		if (GameState.s_playerCharacter != objectByID)
		{
			Player component = GameState.s_playerCharacter.GetComponent<Player>();
			PartyMemberAI component2 = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
			PlayerInventory component3 = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
			TimeController component4 = GameState.s_playerCharacter.GetComponent<TimeController>();
			PointOfInterest component5 = GameState.s_playerCharacter.GetComponent<PointOfInterest>();
			CraftingInventory component6 = GameState.s_playerCharacter.GetComponent<CraftingInventory>();
			QuestInventory component7 = GameState.s_playerCharacter.GetComponent<QuestInventory>();
			Portrait component8 = GameState.s_playerCharacter.GetComponent<Portrait>();
			component.enabled = false;
			component2.enabled = false;
			component3.enabled = false;
			component4.enabled = false;
			component5.enabled = false;
			component6.enabled = false;
			component7.enabled = false;
			component8.enabled = false;
			ComponentUtils.CopyComponent(component8, objectByID);
			UnityEngine.Object.Destroy(component);
			UnityEngine.Object.Destroy(component2);
			UnityEngine.Object.Destroy(component3);
			UnityEngine.Object.DestroyImmediate(component4);
			UnityEngine.Object.Destroy(component5);
			UnityEngine.Object.Destroy(component6);
			UnityEngine.Object.Destroy(component7);
			UnityEngine.Object.Destroy(component8);
			PartyMemberAI.RemoveFromActiveParty(component2, purgePersistencePacket: true);
			objectByID.AddComponent<Player>();
			objectByID.AddComponent<PartyMemberAI>();
			objectByID.AddComponent<TimeController>();
			objectByID.AddComponent<PointOfInterest>();
			objectByID.AddComponent<CraftingInventory>();
			objectByID.AddComponent<QuestInventory>();
			objectByID.AddComponent<PlayerInventory>();
			objectByID.GetComponentsInChildren<Portrait>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<Player>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<PartyMemberAI>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<PlayerInventory>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<TimeController>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<PointOfInterest>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<CraftingInventory>(includeInactive: true)[0].enabled = true;
			objectByID.GetComponentsInChildren<QuestInventory>(includeInactive: true)[0].enabled = true;
			PartyMemberAI.AddToActiveParty(objectByID, fromScript: false);
		}
	}

	public static void CSharpFile(string file)
	{
		try
		{
			using StreamReader streamReader = new StreamReader(new FileStream(file, FileMode.Open));
			CSharp(streamReader.ReadToEnd());
		}
		catch (Exception ex)
		{
			Console.AddMessage(ex.ToString(), Color.yellow);
		}
	}

	public static void CSharp(string source)
	{
		Console.AddMessage("Compiling code...");
		string text = "using UnityEngine;class DynamicallyCompiled{public static void Run(){";
		source = text + source + "}}";
		CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
		CompilerParameters compilerParameters = new CompilerParameters();
		compilerParameters.GenerateInMemory = true;
		foreach (string item in from a in AppDomain.CurrentDomain.GetAssemblies()
			select a.Location)
		{
			compilerParameters.ReferencedAssemblies.Add(item);
		}
		CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, source);
		bool flag = false;
		foreach (CompilerError error in compilerResults.Errors)
		{
			int num = ((error.Line == 1) ? (error.Column - text.Length) : error.Column);
			if (error.IsWarning)
			{
				Console.AddMessage("Compiler Warning ln " + error.Line + " col " + num + ": " + error.ErrorText, Color.yellow);
			}
			else
			{
				flag = true;
				Console.AddMessage("Compiler Error ln " + error.Line + " col " + num + ": " + error.ErrorText, Color.yellow);
			}
		}
		if (!flag)
		{
			compilerResults.CompiledAssembly.GetType("DynamicallyCompiled").GetMethod("Run").Invoke(null, null);
			Console.AddMessage("Code executed.");
		}
	}
}
