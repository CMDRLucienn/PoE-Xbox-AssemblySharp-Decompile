using System;
using System.Collections.Generic;
using System.ComponentModel;
using MinigameData;
using UnityEngine;

public static class Conditionals
{
	public enum Operator
	{
		[Description("Equal To")]
		EqualTo,
		[Description("Greater Than")]
		GreaterThan,
		[Description("Less Than")]
		LessThan,
		[Description("Not Equal To")]
		NotEqualTo,
		[Description("Greater Than Or Equal To")]
		GreaterThanOrEqualTo,
		[Description("Less Than Or Equal To")]
		LessThanOrEqualTo
	}

	private static string[] s_CommandLineArgs = Environment.GetCommandLineArgs();

	public static List<string> s_TestCommandLineArgs = new List<string>();

	private static QuestManager QuestManagerInstance => QuestManager.Instance;

	public static bool CompareInt(int param1, int param2, Operator comparisonOperator)
	{
		return comparisonOperator switch
		{
			Operator.EqualTo => param1 == param2, 
			Operator.GreaterThan => param1 > param2, 
			Operator.LessThan => param1 < param2, 
			Operator.NotEqualTo => param1 != param2, 
			Operator.GreaterThanOrEqualTo => param1 >= param2, 
			Operator.LessThanOrEqualTo => param1 <= param2, 
			_ => false, 
		};
	}

	public static bool CompareFloat(float param1, float param2, Operator comparisonOperator)
	{
		return comparisonOperator switch
		{
			Operator.EqualTo => param1 == param2, 
			Operator.GreaterThan => param1 > param2, 
			Operator.LessThan => param1 < param2, 
			Operator.NotEqualTo => param1 != param2, 
			Operator.GreaterThanOrEqualTo => param1 >= param2, 
			Operator.LessThanOrEqualTo => param1 <= param2, 
			_ => false, 
		};
	}

	[ConditionalScript("Are Guids Same Object", "Conditionals\\General")]
	[ScriptParam0("Object 1", "Object to check.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Object 2", "Object to check.", "", Scripts.BrowserType.ObjectGuid)]
	public static bool AreGuidsSameObject(Guid guid1, Guid guid2)
	{
		GameObject objectByID = InstanceID.GetObjectByID(guid1);
		GameObject objectByID2 = InstanceID.GetObjectByID(guid2);
		if ((bool)objectByID && (bool)objectByID2)
		{
			return objectByID == objectByID2;
		}
		return false;
	}

	[ConditionalScript("Is Active", "Conditionals\\General")]
	[ScriptParam0("Object", "Object to check.", "", Scripts.BrowserType.ObjectGuid)]
	public static bool IsActive(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			return objectByID.activeInHierarchy;
		}
		return false;
	}

	[ConditionalScript("Is Companion Active In Party", "Conditionals\\General")]
	[ScriptParam0("Companion", "Companion to check.", "", Scripts.BrowserType.ObjectGuid)]
	public static bool IsCompanionActiveInParty(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID && objectByID.activeInHierarchy)
		{
			foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
			{
				if (onlyPrimaryPartyMember != null && onlyPrimaryPartyMember.gameObject == objectByID)
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConditionalScript("Is Any Companion Active In Party", "Conditionals\\General")]
	public static bool IsAnyCompanionActiveInParty()
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null && onlyPrimaryPartyMember.gameObject != GameState.s_playerCharacter.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	[ConditionalScript("Is Slot Active", "Conditionals\\General")]
	[ScriptParam0("Slot", "Slot to check.", "0")]
	public static bool IsSlotActive(int slot)
	{
		if (slot >= 0 && slot < 6 && PartyMemberAI.PartyMembers[slot] != null)
		{
			return true;
		}
		return false;
	}

	[ConditionalScript("Is Player In Slot", "Conditionals\\General")]
	[ScriptParam0("Slot", "Slot to check.", "0")]
	public static bool IsPlayerInSlot(int slot)
	{
		if (slot >= 0 && slot < 6 && PartyMemberAI.PartyMembers[slot] != null && (bool)PartyMemberAI.PartyMembers[slot].GetComponent<Player>())
		{
			return true;
		}
		return false;
	}

	[ConditionalScript("Is Character In Slot", "Conditionals\\General")]
	[ScriptParam0("Companion", "Companion to check.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Slot to check.", "0")]
	public static bool IsCharacterInSlot(Guid objectGuid, int slot)
	{
		if (slot >= 0 && slot < 6 && PartyMemberAI.PartyMembers[slot] != null)
		{
			InstanceID component = PartyMemberAI.PartyMembers[slot].GetComponent<InstanceID>();
			if ((bool)component && component.Guid == objectGuid)
			{
				return true;
			}
		}
		return false;
	}

	[ConditionalScript("Is Currently Daytime", "Conditionals\\Time")]
	public static bool IsCurrentlyDaytime()
	{
		if (WorldTime.Instance != null)
		{
			return WorldTime.Instance.IsCurrentlyDaytime();
		}
		return true;
	}

	[ConditionalScript("Is Currently Nighttime", "Conditionals\\Time")]
	public static bool IsCurrentlyNighttime()
	{
		if (WorldTime.Instance != null)
		{
			return WorldTime.Instance.IsCurrentlyNighttime();
		}
		return false;
	}

	[ConditionalScript("Is In Combat", "Conditionals\\General")]
	public static bool IsInCombat()
	{
		if (!GameState.InCombat)
		{
			return GameState.CannotSaveBecauseInCombat;
		}
		return true;
	}

	[ConditionalScript("Has Combat Time Elapsed", "Conditionals\\General")]
	[ScriptParam0("Seconds", "The length of time in seconds to compare against combat duration.", "1.0")]
	public static bool HasCombatTimeElapsed(float seconds)
	{
		return GameState.InCombatDuration >= seconds;
	}

	[ConditionalScript("Is In Stealth Mode(DEPRECATED)", "Conditionals\\General")]
	public static bool IsInStealth()
	{
		return false;
	}

	[ConditionalScript("Is User In Stealth", "Conditionals\\General")]
	public static bool IsUserInStealth()
	{
		return Stealth.IsInStealthMode(InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSpecialGUID(SpecialCharacterInstanceID.SpecialCharacterInstance.User)));
	}

	[ConditionalScript("Has Been Detected", "Conditionals\\Detectable")]
	[ScriptParam0("Object", "The detectable", "", Scripts.BrowserType.ObjectGuid)]
	public static bool HasBeenDetected(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return false;
		}
		Detectable component = objectByID.GetComponent<Detectable>();
		if (component != null)
		{
			return component.Detected;
		}
		return false;
	}

	private static bool IsUserBeingPerceivedByHelper(GameObject perceiver, GameObject userObject)
	{
		if (!perceiver || !userObject)
		{
			return false;
		}
		PartyMemberAI component = userObject.GetComponent<PartyMemberAI>();
		if (!component)
		{
			return false;
		}
		Stealth stealthComponent = Stealth.GetStealthComponent(component.gameObject);
		if ((bool)stealthComponent && stealthComponent.IsInStealthMode())
		{
			if (stealthComponent.GetSuspicion(perceiver) >= 200f)
			{
				return true;
			}
			return false;
		}
		float perceptionDistance = component.PerceptionDistance;
		if (GameUtilities.V3SqrDistance2D(perceiver.transform.position, component.gameObject.transform.position) < perceptionDistance * perceptionDistance && GameUtilities.LineofSight(perceiver.transform.position, component.gameObject, 1f))
		{
			return true;
		}
		return false;
	}

	[ConditionalScript("Is User Being Perceived By", "Conditionals\\General")]
	[ScriptParam0("Perceiver Object", "The object that needs to perceive the user", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Check For Any Party Member", "If false, only checks the \"User\" object. If true, any party member being perceived will return true.", "false")]
	public static bool IsUserBeingPerceivedBy(Guid peceiverGuid, bool CheckAnyPartyMember)
	{
		GameObject objectByID = InstanceID.GetObjectByID(peceiverGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSpecialGUID(SpecialCharacterInstanceID.SpecialCharacterInstance.User));
		if (CheckAnyPartyMember)
		{
			for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
			{
				if (!(PartyMemberAI.PartyMembers[i] == null) && !PartyMemberAI.PartyMembers[i].Secondary && IsUserBeingPerceivedByHelper(objectByID, PartyMemberAI.PartyMembers[i].gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return IsUserBeingPerceivedByHelper(objectByID, objectByID2);
	}

	[ConditionalScript("Is In Volume", "Conditionals\\General")]
	[ScriptParam0("Object", "The object to check", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Object", "The object with a collider to check against", "", Scripts.BrowserType.ObjectGuid)]
	public static bool IsInVolume(Guid objectGuid, Guid objectColliderGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(objectColliderGuid);
		if (objectByID != null && objectByID2 != null)
		{
			Collider component = objectByID2.GetComponent<Collider>();
			if (component != null)
			{
				Collider component2 = objectByID.GetComponent<Collider>();
				if (component2 == null)
				{
					if (component.bounds.Contains(objectByID.transform.position))
					{
						return true;
					}
				}
				else if (component.bounds.Intersects(component2.bounds))
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConditionalScript("Command Line Arg Set", "Conditionals\\General")]
	[ScriptParam0("Arg", "The arg to check to see if it was set.", "")]
	public static bool CommandLineArg(string argName)
	{
		for (int i = 0; i < s_CommandLineArgs.Length; i++)
		{
			if (string.Compare(s_CommandLineArgs[i].Replace("-", ""), argName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		for (int j = 0; j < s_TestCommandLineArgs.Count; j++)
		{
			if (string.Compare(s_TestCommandLineArgs[j], argName, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	[ConditionalScript("HasPX1", "Conditionals\\General")]
	public static bool HasPX1()
	{
		return GameUtilities.HasPX1();
	}

	[ConditionalScript("HasPX2", "Conditionals\\General")]
	public static bool HasPX2()
	{
		return GameUtilities.HasPX2();
	}

	[ConditionalScript("HasPX4", "Conditionals\\General")]
	public static bool HasPX4()
	{
		return GameUtilities.HasPX4();
	}

	[ConditionalScript("Has Package", "Conditionals\\General")]
	[ScriptParam0("Package", "The package to check for.", "Expansion4")]
	public static bool HasPackage(ProductConfiguration.Package package)
	{
		return GameUtilities.HasPackage(package);
	}

	[ConditionalScript("Current Map Has Tag", "Conditionals\\General")]
	[ScriptParam0("Tag", "The map tag to check for.", "world")]
	public static bool CurrentMapHasTag(string maptag)
	{
		return GameState.Instance.CurrentMap.IsValidOnMap(maptag);
	}

	[ConditionalScript("Is Global Value", "Conditionals\\Globals")]
	[ScriptParam0("Tag", "Tag of the global variable to query.", "GlobalTag", Scripts.BrowserType.GlobalVariable)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Value", "Compare the global variable against this value.", "0")]
	public static bool IsGlobalValue(string name, Operator comparisonOperator, int globalValue)
	{
		return CompareInt(GlobalVariables.Instance.GetVariable(name), globalValue, comparisonOperator);
	}

	[ConditionalScript("Compare Globals", "Conditionals\\Globals")]
	[ScriptParam0("Global 1", "Tag of the first global variable to query.", "GlobalTag1", Scripts.BrowserType.GlobalVariable)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Global 2", "Tag of the second global variable to query.", "GlobalTag2", Scripts.BrowserType.GlobalVariable)]
	public static bool CompareGlobals(string globalTag1, Operator comparisonOperator, string globalTag2)
	{
		return CompareInt(GlobalVariables.Instance.GetVariable(globalTag1), GlobalVariables.Instance.GetVariable(globalTag2), comparisonOperator);
	}

	[ConditionalScript("Has Conversation Node Been Played", "Conditionals\\Conversation")]
	[ScriptParam0("Conversation", "Name of the conversation.", "", Scripts.BrowserType.Conversation)]
	[ScriptParam1("Conversation Node ID", "Conversation node ID.", "0")]
	public static bool HasConversationNodeBeenPlayed(string conversation, int nodeID)
	{
		return ConversationManager.Instance.HasConversationNodeBeenPlayed(conversation, nodeID);
	}

	[ConditionalScript("Is Quest On Node", "Conditionals\\Quest")]
	[ScriptParam0("Quest Name", "Name of the quest.", "", Scripts.BrowserType.Quest)]
	[ScriptParam1("Quest Node ID", "Quest node ID.", "0")]
	public static bool IsQuestOnNode(string questName, int nodeID)
	{
		return QuestManagerInstance.IsQuestStateActive(questName, nodeID);
	}

	[ConditionalScript("Is Quest Event Triggered", "Conditionals\\Quest")]
	[ScriptParam0("Quest Name", "Name of the quest.", "", Scripts.BrowserType.Quest)]
	[ScriptParam1("Quest Event ID", "Quest vvent ID.", "-1")]
	[QuestEvent]
	public static bool IsQuestEventTriggered(string questName, int questEventID)
	{
		return QuestManagerInstance.IsEventTriggered(questName, questEventID);
	}

	[ConditionalScript("Is Quest Addendum Triggered", "Conditionals\\Quest")]
	[ScriptParam0("Quest Name", "Name of the quest.", "", Scripts.BrowserType.Quest)]
	[ScriptParam1("Addendum ID", "Addendum ID to test.", "0")]
	public static bool IsQuestAddendumTriggered(string questName, int addendumtID)
	{
		return QuestManagerInstance.IsAddendumTriggered(questName, addendumtID);
	}

	[ConditionalScript("Is Quest End State Triggered", "Conditionals\\Quest")]
	[ScriptParam0("Quest Name", "Name of the quest.", "", Scripts.BrowserType.Quest)]
	[ScriptParam1("End State ID", "End State ID to test.", "0")]
	public static bool IsQuestEndStateTriggered(string questName, int endStateID)
	{
		return QuestManagerInstance.IsEndStateTriggered(questName, endStateID);
	}

	[ConditionalScript("Is Quest Failed", "Conditionals\\Quest")]
	[ScriptParam0("Quest Name", "Name of the quest.", "", Scripts.BrowserType.Quest)]
	public static bool IsQuestFailed(string questName)
	{
		return QuestManagerInstance.IsQuestFailed(questName);
	}

	[ConditionalScript("Is Distance", "Conditionals\\Misc")]
	[ScriptParam0("Object A", "Object A to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Object B", "Object B to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the distance between A and B against this value.", "0")]
	public static bool IsDistance(Guid objectGuidA, Guid objectGuidB, Operator comparisonOperator, float distValue)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuidA);
		GameObject objectByID2 = InstanceID.GetObjectByID(objectGuidB);
		if ((bool)objectByID && (bool)objectByID2)
		{
			return CompareFloat(Vector3.Distance(objectByID.transform.position, objectByID2.transform.position), distValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Easy", "Conditionals\\Difficulty")]
	public static bool IsEasy()
	{
		if (GameState.Instance == null)
		{
			return false;
		}
		return GameState.Instance.Difficulty == GameDifficulty.Easy;
	}

	[ConditionalScript("Is Normal", "Conditionals\\Difficulty")]
	public static bool IsNormal()
	{
		if (GameState.Instance == null)
		{
			return false;
		}
		return GameState.Instance.Difficulty == GameDifficulty.Normal;
	}

	[ConditionalScript("Is Hard", "Conditionals\\Difficulty")]
	public static bool IsHard()
	{
		if (GameState.Instance == null)
		{
			return false;
		}
		if (GameState.Instance.Difficulty != GameDifficulty.Hard)
		{
			return GameState.Instance.Difficulty == GameDifficulty.PathOfTheDamned;
		}
		return true;
	}

	[ConditionalScript("Is Story Time", "Conditionals\\Difficulty")]
	public static bool IsStoryTime()
	{
		if (GameState.Instance == null)
		{
			return false;
		}
		return GameState.Instance.Difficulty == GameDifficulty.StoryTime;
	}

	[ConditionalScript("Is Team Relationship", "Scripts\\Faction")]
	[ScriptParam0("Team A", "The first team to check", "")]
	[ScriptParam1("Team B", "The second team to check", "")]
	[ScriptParam2("Relationship", "How team A and B will relate to each other", "Neutral")]
	public static bool IsTeamRelationship(string teamA, string teamB, Faction.Relationship testRelationship)
	{
		Team teamByTag = Team.GetTeamByTag(teamA);
		if (teamByTag == null)
		{
			UIDebug.Instance.LogOnceOnlyWarning("IsTeamRelationship failed: " + teamA + " isn't loaded, script call needs to be delayed", UIDebug.Department.Design, 5f);
			Debug.LogError("IsTeamRelationship has an error. " + teamA + " does not exist. Make sure you match up the script tag, it is also possible the team hasn't been loaded yet.");
			return false;
		}
		Team teamByTag2 = Team.GetTeamByTag(teamB);
		if (teamByTag2 == null)
		{
			UIDebug.Instance.LogOnceOnlyWarning("IsTeamRelationship failed: " + teamB + " isn't loaded yet, script call needs to be delayed.", UIDebug.Department.Design, 5f);
			Debug.LogError("IsTeamRelationship has an error. " + teamB + " does not exist. Make sure you match up the script tag, it is also possible the team hasn't been loaded yet.");
			return false;
		}
		return teamByTag.GetRelationship(teamByTag2) == testRelationship;
	}

	[ConditionalScript("Reputation Rank Equals", "Conditionals\\Faction")]
	[FactionReputationRequirement("axis", "rank", "objectGuid")]
	[ScriptParam0("Object", "Object to check.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Axis", "The reputation axis to check", "Positive")]
	[ScriptParam2("Ranks", "Ranks check amount", "0")]
	public static bool ReputationRankEquals(Guid objectGuid, Reputation.Axis axis, int rank)
	{
		Faction factionComponent = Scripts.GetFactionComponent(objectGuid);
		if (factionComponent != null && factionComponent.Reputation != null)
		{
			if (axis == Reputation.Axis.Negative)
			{
				return factionComponent.Reputation.BadRank == rank;
			}
			return factionComponent.Reputation.GoodRank == rank;
		}
		if (factionComponent.Reputation == null)
		{
			if (factionComponent != null)
			{
				Debug.LogError("ReputationRankEquals failed. " + factionComponent.gameObject.name + " doesn't have a Reputation!");
			}
			else
			{
				Debug.LogError("ReputationRankEquals failed. The specified character wasn't found!");
			}
		}
		return false;
	}

	[ConditionalScript("Tagged Reputation Rank Equals", "Conditionals\\Faction")]
	[FactionReputationRequirement("axis", "rank", "id")]
	[ScriptParam0("Faction Name", "Faction to modify", "None")]
	[ScriptParam1("Axis", "The reputation axis to check", "Positive")]
	[ScriptParam2("Ranks", "Ranks check amount", "0")]
	public static bool ReputationRankByTagEquals(FactionName id, Reputation.Axis axis, int rank)
	{
		Reputation reputation = ReputationManager.Instance.GetReputation(id);
		if (reputation != null)
		{
			if (axis == Reputation.Axis.Negative)
			{
				return reputation.BadRank == rank;
			}
			return reputation.GoodRank == rank;
		}
		Debug.LogError("Faction " + id.ToString() + " isn't setup in the global prefab! (ReputationRankByTagEquals failed)");
		return false;
	}

	[ConditionalScript("Reputation Rank Greater or Equal", "Conditionals\\Faction")]
	[FactionReputationRequirement("axis", "rank", "objectGuid")]
	[ScriptParam0("Object", "Object to check", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Axis", "The reputation axis to check", "Positive")]
	[ScriptParam2("Ranks", "Ranks check amount", "0")]
	public static bool ReputationRankGreater(Guid objectGuid, Reputation.Axis axis, int rank)
	{
		Faction factionComponent = Scripts.GetFactionComponent(objectGuid);
		if (factionComponent != null && factionComponent.Reputation != null)
		{
			if (axis == Reputation.Axis.Negative)
			{
				return factionComponent.Reputation.BadRank >= rank;
			}
			return factionComponent.Reputation.GoodRank >= rank;
		}
		if (factionComponent.Reputation == null)
		{
			if (factionComponent != null)
			{
				Debug.LogError("ReputationRankGreater failed. " + factionComponent.gameObject.name + " doesn't have a Reputation!");
			}
			else
			{
				Debug.LogError("ReputationRankGreater failed. The specified character wasn't found!");
			}
		}
		return false;
	}

	[ConditionalScript("Tagged Reputation Rank Greater or Equal", "Conditionals\\Faction")]
	[FactionReputationRequirement("axis", "rank", "id")]
	[ScriptParam0("Faction Name", "Faction to check", "None")]
	[ScriptParam1("Axis", "The reputation axis to check", "Positive")]
	[ScriptParam2("Ranks", "Ranks change amount", "0")]
	public static bool ReputationTagRankGreater(FactionName id, Reputation.Axis axis, int rank)
	{
		Reputation reputation = ReputationManager.Instance.GetReputation(id);
		if (reputation != null)
		{
			if (axis == Reputation.Axis.Negative)
			{
				return reputation.BadRank >= rank;
			}
			return reputation.GoodRank >= rank;
		}
		Debug.LogError("Faction " + id.ToString() + " isn't setup in the global prefab! (ReputationTagRankGreater failed)");
		return false;
	}

	[ConditionalScript("Disposition Equal", "Conditionals\\Faction")]
	[StatRequirement("axis", "rank", true)]
	[ScriptParam0("Axis", "The disposition type to check", "Benevolent")]
	[ScriptParam1("Rank", "Returns true if rank is equal to this value.", "None")]
	public static bool DispositionEqual(Disposition.Axis axis, Disposition.Rank rank)
	{
		return ReputationManager.Instance.PlayerDisposition.GetRank(axis) == (int)rank;
	}

	[ConditionalScript("Disposition Greater or Equal", "Conditionals\\Faction")]
	[StatRequirement("axis", "rank", true)]
	[ScriptParam0("Axis", "The disposition type to check", "Benevolent")]
	[ScriptParam1("Rank", "Returns true if rank is greater or equal to this value.", "None")]
	public static bool DispositionGreaterOrEqual(Disposition.Axis axis, Disposition.Rank rank)
	{
		return ReputationManager.Instance.PlayerDisposition.GetRank(axis) >= (int)rank;
	}

	public static Health GetHealthComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			Health component = objectByID.GetComponent<Health>();
			if ((bool)component)
			{
				return component;
			}
			Debug.LogWarning(string.Concat(objectGuid, " doesn't have a health component."), objectByID);
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found when searching for health component."), null);
		return null;
	}

	[ConditionalScript("Is Health Value", "Conditionals\\Health")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Health Value", "Compare the object's health against this value.", "0")]
	public static bool IsHealthValue(Guid objectGuid, Operator comparisonOperator, float healthValue)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			return CompareFloat(healthComponent.CurrentHealth, healthValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Health Percentage", "Conditionals\\Health")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Health Percentage", "Compare the object's health against this percentage.", "0")]
	public static bool IsHealthPercentage(Guid objectGuid, Operator comparisonOperator, float healthPercentage)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			return CompareFloat(healthComponent.CurrentHealth / healthComponent.MaxHealth, healthPercentage * 0.01f, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Stamina Value", "Conditionals\\Health")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Stamina Value", "Compare the object's statmina against this value.", "0")]
	public static bool IsStaminaValue(Guid objectGuid, Operator comparisonOperator, float staminaValue)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			return CompareFloat(healthComponent.CurrentStamina, staminaValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Stamina Percentage", "Conditionals\\Health")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Stamina Percentage", "Compare the object's stamina against this percentage.", "0")]
	public static bool IsStaminaPercentage(Guid objectGuid, Operator comparisonOperator, float staminaPercentage)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			return CompareFloat(healthComponent.CurrentStamina / healthComponent.MaxStamina, staminaPercentage * 0.01f, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Item Count", "Conditionals\\Items")]
	[ScriptParam0("Item Name", "Name of the item", "ItemName")]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Count", "Compare the item count against this value.", "0")]
	public static bool IsItemCount(string itemName, Operator comparisonOperator, int itemCount)
	{
		int num = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember && (bool)onlyPrimaryPartyMember.Inventory)
			{
				num += onlyPrimaryPartyMember.Inventory.ItemCount(itemName);
			}
		}
		return CompareInt(num, itemCount, comparisonOperator);
	}

	[ConditionalScript("Is Item Equipped", "Conditionals\\Items")]
	[ScriptParam0("Object", "object", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "Name of the item", "ItemName")]
	public static bool IsItemEquipped(Guid objectGuid, string itemName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return false;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component == null)
		{
			return false;
		}
		return component.IsEquipped(itemName);
	}

	[ConditionalScript("Is Item Equipped On Player", "Conditionals\\Items")]
	[ScriptParam0("Item Name", "Name of the item", "ItemName")]
	public static bool IsItemEquippedOnPlayer(string itemName)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember)
			{
				Equipment component = onlyPrimaryPartyMember.GetComponent<Equipment>();
				if ((bool)component && component.IsEquipped(itemName))
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConditionalScript("Has Money", "Conditionals\\Items")]
	[ScriptParam0("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam1("Amount", "Compare the player's money against this value.", "0")]
	public static bool HasMoney(Operator comparisonOperator, int amount)
	{
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if ((bool)inventory)
		{
			return CompareInt((int)inventory.currencyTotalValue.v, amount, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Object Has Item Count", "Conditionals\\Items")]
	[ScriptParam0("Object", "Object with an inventory.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "Name of the item", "ItemName")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Count", "Compare the item count against this value.", "0")]
	public static bool ObjectHasItemCount(Guid objectGuid, string itemName, Operator comparisonOperator, int itemCount)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return false;
		}
		Inventory component = objectByID.GetComponent<Inventory>();
		if ((bool)component)
		{
			return CompareInt(component.ItemCount(itemName), itemCount, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Weapon Type Equipped In Primary Slot", "Conditionals\\Items")]
	[ScriptParam0("Object", "object", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Weapon Type", "Type of Weapon", "Arbalest")]
	public static bool IsWeaponTypeEquippedInPrimarySlot(Guid objectGuid, WeaponSpecializationData.WeaponType weaponType)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return false;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component == null)
		{
			return false;
		}
		Weapon weapon = component.CurrentItems.GetSelectedWeaponSet().PrimaryWeapon as Weapon;
		if ((bool)weapon)
		{
			return weapon.WeaponType == weaponType;
		}
		return false;
	}

	[ConditionalScript("Is Armor Type Equipped", "Conditionals\\Items")]
	[ScriptParam0("Object", "object", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Armor Type", "Type of armor", "Light")]
	public static bool IsArmorTypeEquipped(Guid objectGuid, Armor.Category armorType)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return false;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (!component)
		{
			return false;
		}
		Armor armor = (component.CurrentItems.Chest ? component.CurrentItems.Chest.GetComponent<Armor>() : null);
		if ((bool)armor)
		{
			return armor.ArmorCategory == armorType;
		}
		return false;
	}

	[ConditionalScript("Is Armor Type Equipped In Party Slot", "Conditionals\\Items")]
	[ScriptParam0("Slot", "Slot to check.", "0")]
	[ScriptParam1("Armor Type", "Type of armor", "Light")]
	public static bool IsArmorTypeEquippedInSlot(int slot, Armor.Category armorType)
	{
		if (slot < 0 || slot >= PartyMemberAI.PartyMembers.Length)
		{
			return false;
		}
		PartyMemberAI partyMemberAI = PartyMemberAI.PartyMembers[slot];
		if (!partyMemberAI)
		{
			return false;
		}
		Equipment component = partyMemberAI.GetComponent<Equipment>();
		if (!component)
		{
			return false;
		}
		Armor armor = (component.CurrentItems.Chest ? component.CurrentItems.Chest.GetComponent<Armor>() : null);
		if ((bool)armor)
		{
			return armor.ArmorCategory == armorType;
		}
		return false;
	}

	[ConditionalScript("Dozens Game Player Result Is", "Conditionals\\Minigame")]
	[ScriptParam0("Result", "Result to check for.", "TALKERS")]
	public static bool DozensGamePlayerResultIs(Dozens.Result result)
	{
		return Dozens.ContestantRolls[0].ResultType == result;
	}

	[ConditionalScript("Dozens Game Opponent Result Is", "Conditionals\\Minigame")]
	[ScriptParam0("Result", "Result to check for.", "TALKERS")]
	public static bool DozensGameOpponentResultIs(Dozens.Result result)
	{
		return Dozens.ContestantRolls[1].ResultType == result;
	}

	[ConditionalScript("Dozens Game Is Player Winning", "Conditionals\\Minigame")]
	public static bool DozensGamePlayerWinning()
	{
		return Dozens.WinnerIs(Contestant.PLAYER);
	}

	[ConditionalScript("Dozens Game Is Opponent Winning", "Conditionals\\Minigame")]
	public static bool DozensGameOpponentWinning()
	{
		return Dozens.WinnerIs(Contestant.OPPONENT);
	}

	[ConditionalScript("Dozens Game Is Tied", "Conditionals\\Minigame")]
	public static bool DozensGameIsTied()
	{
		return Dozens.IsTied();
	}

	[ConditionalScript("Orlan Game Is Player Winning", "Conditionals\\Minigame")]
	public static bool OrlanGamePlayerWinning()
	{
		return OrlansHead.WinnerIs(Contestant.PLAYER);
	}

	[ConditionalScript("Orlan Game Player Result Is", "Conditionals\\Minigame")]
	[ScriptParam0("Result", "The result to check against.", "MISS")]
	public static bool OrlanGamePlayerResultIs(OrlansHead.Result result)
	{
		return OrlansHead.ContestantResults[0] == result;
	}

	[ConditionalScript("Orlan Game Is Opponent Winning", "Conditionals\\Minigame")]
	public static bool OrlanGameOpponentWinning()
	{
		return OrlansHead.WinnerIs(Contestant.OPPONENT);
	}

	[ConditionalScript("Orlan Game Opponent Result Is", "Conditionals\\Minigame")]
	[ScriptParam0("Result", "The result to check against.", "MISS")]
	public static bool OrlanGameOpponentResultIs(OrlansHead.Result result)
	{
		return OrlansHead.ContestantResults[1] == result;
	}

	[ConditionalScript("Orlan Game Is Tied", "Conditionals\\Minigame")]
	public static bool OrlanGameIsTied()
	{
		return OrlansHead.IsTied();
	}

	[ConditionalScript("Orlan Game Round Count Is", "Conditionals\\Minigame")]
	[ScriptParam0("Operator", "Comparison operator.", "GreaterThanOrEqualTo")]
	[ScriptParam1("Value", "Value to check against.", "3")]
	public static bool OrlanGameRoundCountIs(Operator op, int value)
	{
		return CompareInt(OrlansHead.RoundCount, value, op);
	}

	[ConditionalScript("Is OCL In A State", "Conditionals\\OCL")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("State to test", "State", "Closed")]
	public static bool IsOCLInState(Guid objectGuid, OCL.State state)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			OCL component = objectByID.GetComponent<OCL>();
			if ((bool)component)
			{
				return component.CurrentState == state;
			}
		}
		return false;
	}

	private static CharacterStats GetCharacterStats(Guid objectGuid)
	{
		return Scripts.GetComponentByGuid<CharacterStats>(objectGuid);
	}

	public static CharacterStats GetPartyCharacterStats(int slot)
	{
		if (slot < 0 || slot >= 6)
		{
			return null;
		}
		if (PartyMemberAI.PartyMembers[slot] == null)
		{
			return null;
		}
		return PartyMemberAI.PartyMembers[slot].GetComponent<CharacterStats>();
	}

	[ConditionalScript("Is Attribute Score Value", "Conditionals\\RPG")]
	[StatRequirement("attributeScore", "attributeValue", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Attribute", "Attribute to test.", "Resolve")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's attribute score against this value.", "0")]
	public static bool IsAttributeScoreValue(Guid objectGuid, CharacterStats.AttributeScoreType attributeScore, Operator comparisonOperator, int attributeValue)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return CompareInt(characterStats.GetAttributeScore(attributeScore), attributeValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Player Attribute Score Value", "Conditionals\\RPG")]
	[StatRequirement("attributeScore", "attributeValue", false)]
	[ScriptParam0("Attribute", "Attribute to test.", "Resolve")]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Value", "Compare the object's attribute score against this value.", "0")]
	public static bool IsPlayerAttributeScoreValue(CharacterStats.AttributeScoreType attributeScore, Operator comparisonOperator, int attributeValue)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return CompareInt(component.GetAttributeScore(attributeScore), attributeValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Slot Attribute Score Value", "Conditionals\\RPG")]
	[StatRequirement("attributeScore", "attributeValue", false)]
	[ScriptParam0("Slot", "slot to test.", "0")]
	[ScriptParam1("Attribute", "Attribute to test.", "Resolve")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's attribute score against this value.", "0")]
	public static bool IsSlotAttributeScoreValue(int slot, CharacterStats.AttributeScoreType attributeScore, Operator comparisonOperator, int attributeValue)
	{
		if (slot < 0 || slot >= 6)
		{
			return false;
		}
		if (PartyMemberAI.PartyMembers[slot] == null)
		{
			return false;
		}
		CharacterStats component = PartyMemberAI.PartyMembers[slot].GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return CompareInt(component.GetAttributeScore(attributeScore), attributeValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Defense Value", "Conditionals\\RPG")]
	[StatRequirement("defenseType", "defenseValue", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Defense Type", "Defense to test.", "Deflect")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's defense against this value.", "0")]
	public static bool IsDefenseValue(Guid objectGuid, CharacterStats.DefenseType defenseType, Operator comparisonOperator, int defenseValue)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return CompareInt(characterStats.CalculateDefense(defenseType), defenseValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Player Defense Value", "Conditionals\\RPG")]
	[StatRequirement("defenseType", "defenseValue", false)]
	[ScriptParam0("Defense Type", "Defense to test.", "Deflect")]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Value", "Compare the object's defense against this value.", "0")]
	public static bool IsPlayerDefenseValue(CharacterStats.DefenseType defenseType, Operator comparisonOperator, int defenseValue)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return CompareInt(component.CalculateDefense(defenseType), defenseValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Skill Value", "Conditionals\\RPG")]
	[StatRequirement("skillType", "skillValue", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's skill against this value.", "0")]
	public static bool IsSkillValue(Guid objectGuid, CharacterStats.SkillType skillType, Operator comparisonOperator, int skillValue)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return CompareInt(characterStats.CalculateSkill(skillType), skillValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Is Skill Value (Scaled)", "Conditionals\\RPG")]
	[StatRequirement("skillType", "skillValue", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's skill against this value.", "0")]
	[ScriptParam4("Scaler", "Scaler to use, if the player has enabled it.", "0")]
	public static bool IsSkillValueScaled(Guid objectGuid, CharacterStats.SkillType skillType, Operator comparisonOperator, int skillValue, DifficultyScaling.Scaler scaler)
	{
		return IsSkillValue(objectGuid, skillType, comparisonOperator, Mathf.CeilToInt((float)skillValue * DifficultyScaling.Instance.GetScaleMultiplicative(scaler, (DifficultyScaling.ScaleData sd) => sd.SkillCheckMult)));
	}

	[ConditionalScript("Is Party Skill Value Count", "Conditionals\\RPG")]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam1("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Skill Value", "Compare the object's skill against this value.", "0")]
	[ScriptParam3("Party Operator", "Compare the party count with this operator.", "EqualTo")]
	[ScriptParam4("Party Value", "Compare how many party members pass.", "0")]
	public static bool IsPartySkillValueCount(CharacterStats.SkillType skillType, Operator comparisonOperator, int skillValue, Operator partyOperator, int partyValue)
	{
		int num = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null) && CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
				{
					num++;
				}
			}
		}
		return CompareInt(num, partyValue, partyOperator);
	}

	[ConditionalScript("Is Player Skill Value", "Conditionals\\RPG")]
	[StatRequirement("skillType", "skillValue", false)]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam1("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Value", "Compare the object's skill against this value.", "0")]
	public static bool IsPlayerSkillValue(CharacterStats.SkillType skillType, Operator comparisonOperator, int skillValue)
	{
		return IsSkillValue(SpecialCharacterInstanceID.GetSpecialGUID(SpecialCharacterInstanceID.SpecialCharacterInstance.Player), skillType, comparisonOperator, skillValue);
	}

	[ConditionalScript("Is Slot Skill Value", "Conditionals\\RPG")]
	[StatRequirement("skillType", "skillValue", false)]
	[ScriptParam0("Slot", "slot to test.", "0")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Value", "Compare the object's skill against this value.", "0")]
	public static bool IsSlotSkillValue(int slot, CharacterStats.SkillType skillType, Operator comparisonOperator, int skillValue)
	{
		if (slot < 0 || slot >= SpecialCharacterInstanceID.s_slotGuids.Length)
		{
			return false;
		}
		return IsSkillValue(SpecialCharacterInstanceID.GetSpecialGUID(SpecialCharacterInstanceID.s_slotGuids[slot]), skillType, comparisonOperator, skillValue);
	}

	[ConditionalScript("Is Player Character Using S.I.", "Conditionals\\RPG")]
	public static bool IsPlayerCharacterUsingSI()
	{
		Guid guid = GameState.s_playerCharacter.GetComponent<InstanceID>().Guid;
		if (GameState.LastPersonToUseScriptedInteraction != null && GameState.LastPersonToUseScriptedInteraction.GetComponent<InstanceID>().Guid == guid)
		{
			return true;
		}
		return false;
	}

	[ConditionalScript("Is Player Character Skill Check 0", "Conditionals\\RPG\\SkillCheck")]
	public static bool IsPlayerCharacterSkillCheckZero()
	{
		return IsPlayerCharacterSkillCheckX(0);
	}

	[ConditionalScript("Is Player Character Skill Check X", "Conditionals\\RPG\\SkillCheck")]
	[ScriptParam0("Skill Check", "Skill check index to test.", "0")]
	public static bool IsPlayerCharacterSkillCheckX(int check)
	{
		GameObject objectByID = InstanceID.GetObjectByID(SpecialCharacterInstanceID.PlayerGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSkillCheckGuid(check));
		return objectByID == objectByID2;
	}

	[ConditionalScript("Is Party Slot Skill Check X", "Conditionals\\RPG\\SkillCheck")]
	[ScriptParam0("Slot", "Slot to test.", "0")]
	[ScriptParam1("Skill Check", "Skill check index to test.", "0")]
	public static bool IsPartySlotSkillCheckX(int slot, int check)
	{
		GameObject objectByID = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSpecialGUID(SpecialCharacterInstanceID.s_slotGuids[slot]));
		GameObject objectByID2 = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSkillCheckGuid(check));
		return objectByID == objectByID2;
	}

	[ConditionalScript("Is Character Skill Check X", "Conditionals\\RPG\\SkillCheck")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Skill Check", "Skill check index to test.", "0")]
	public static bool IsCharacterSkillCheckX(Guid objectGuid, int check)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSkillCheckGuid(check));
		return objectByID == objectByID2;
	}

	[ConditionalScript("Is Class", "Conditionals\\RPG")]
	[StatRequirement("characterClass", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Class", "Class to test.", "Fighter")]
	public static bool IsClass(Guid objectGuid, CharacterStats.Class characterClass)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return characterClass == characterStats.CharacterClass;
		}
		return false;
	}

	[ConditionalScript("Is Player Class", "Conditionals\\RPG")]
	[StatRequirement("characterClass", "", false)]
	[ScriptParam0("Class", "Class to test.", "Fighter")]
	public static bool IsPlayerClass(CharacterStats.Class characterClass)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return characterClass == component.CharacterClass;
		}
		return false;
	}

	[ConditionalScript("Is Race", "Conditionals\\RPG")]
	[StatRequirement("characterRace", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Race", "Race to test.", "Human")]
	public static bool IsRace(Guid objectGuid, CharacterStats.Race characterRace)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return characterRace == characterStats.CharacterRace;
		}
		return false;
	}

	[ConditionalScript("Is Player Race", "Conditionals\\RPG")]
	[StatRequirement("characterRace", "", false)]
	[ScriptParam0("Race", "Race to test.", "Human")]
	public static bool IsPlayerRace(CharacterStats.Race characterRace)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return characterRace == component.CharacterRace;
		}
		return false;
	}

	[ConditionalScript("Is Subrace", "Conditionals\\RPG")]
	[StatRequirement("characterSubrace", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Subrace", "Subrace to test.", "Meadow_Human")]
	public static bool IsSubrace(Guid objectGuid, CharacterStats.Subrace characterSubrace)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return characterSubrace == characterStats.CharacterSubrace;
		}
		return false;
	}

	[ConditionalScript("Is Player Subrace", "Conditionals\\RPG")]
	[StatRequirement("characterSubRace", "", false)]
	[ScriptParam0("Subrace", "Subrace to test.", "Meadow_Human")]
	public static bool IsPlayerSubrace(CharacterStats.Subrace characterSubRace)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return characterSubRace == component.CharacterSubrace;
		}
		return false;
	}

	[ConditionalScript("Is Gender", "Conditionals\\RPG")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Gender", "Gender to test.", "Male")]
	public static bool IsGender(Guid objectGuid, Gender gender)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return gender == characterStats.Gender;
		}
		return false;
	}

	[ConditionalScript("Is Player Gender", "Conditionals\\RPG")]
	[ScriptParam0("Gender", "Gender to test.", "Male")]
	public static bool IsPlayerGender(Gender gender)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return gender == component.Gender;
		}
		return false;
	}

	[ConditionalScript("Is Slot Gender", "Conditionals\\RPG")]
	[ScriptParam0("Slot", "Slot to test", "0")]
	[ScriptParam1("Gender", "Gender to test.", "Male")]
	public static bool IsSlotGender(int slot, Gender gender)
	{
		if (slot < 0 || slot >= 6)
		{
			return false;
		}
		if (PartyMemberAI.PartyMembers[slot] == null)
		{
			return false;
		}
		CharacterStats component = PartyMemberAI.PartyMembers[slot].GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return gender == component.Gender;
		}
		return false;
	}

	[ConditionalScript("Has Talent or Ability", "Conditionals\\RPG")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Ability Name", "Name of the ability", "AbilityName")]
	public static bool HasTalentOrAbility(Guid objectGuid, string abilityName)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			foreach (GenericAbility activeAbility in characterStats.ActiveAbilities)
			{
				if (activeAbility != null && abilityName.Equals(activeAbility.tag, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConditionalScript("Is Deity", "Conditionals\\RPG")]
	[StatRequirement("deity", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Deity", "Deity to test.", "Berath")]
	public static bool IsDeity(Guid objectGuid, Religion.Deity deity)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return deity == characterStats.Deity;
		}
		return false;
	}

	[ConditionalScript("Is Paladin Order", "Conditionals\\RPG")]
	[StatRequirement("order", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Paladin Order", "Paladin order to test.", "BleakWalkers")]
	public static bool IsPaladinOrder(Guid objectGuid, Religion.PaladinOrder order)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return order == characterStats.PaladinOrder;
		}
		return false;
	}

	[ConditionalScript("Is Culture", "Conditionals\\RPG")]
	[StatRequirement("culture", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Culture", "Culture to test.", "Aedyr")]
	public static bool IsCulture(Guid objectGuid, CharacterStats.Culture culture)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return culture == characterStats.CharacterCulture;
		}
		return false;
	}

	[ConditionalScript("Is Player Culture", "Conditionals\\RPG")]
	[StatRequirement("culture", "", false)]
	[ScriptParam0("Culture", "Culture to test.", "Aedyr")]
	public static bool IsPlayerCulture(CharacterStats.Culture culture)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return culture == component.CharacterCulture;
		}
		return false;
	}

	[ConditionalScript("Is Background", "Conditionals\\RPG")]
	[StatRequirement("background", "", false)]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Background", "Background to test.", "0")]
	public static bool IsBackground(Guid objectGuid, CharacterStats.Background background)
	{
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return background == characterStats.CharacterBackground;
		}
		return false;
	}

	[ConditionalScript("Is Player Background", "Conditionals\\RPG")]
	[StatRequirement("background", "", false)]
	[ScriptParam0("Background", "Background to test.", "0")]
	public static bool IsPlayerBackground(CharacterStats.Background background)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return background == component.CharacterBackground;
		}
		return false;
	}

	[ConditionalScript("Any Party Member Can Use Ability", "Conditionals\\RPG")]
	[AbilityRequirement("nameStringId")]
	[ScriptParam0("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static bool AnyPartyMemberCanUseAbility(int nameStringId)
	{
		if (nameStringId < 0)
		{
			return false;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
			if ((bool)component && component.CanUseAbilityForScript(nameStringId))
			{
				return true;
			}
		}
		return false;
	}

	[ConditionalScript("Character Can Use Ability", "Conditionals\\RPG")]
	[AbilityRequirement("nameStringId")]
	[ScriptParam0("Object", "Object to test.", "", Scripts.BrowserType.ObjectGuid)]
	[ScriptParam1("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static bool CharacterCanUseAbility(Guid objectGuid, int nameStringId)
	{
		if (nameStringId < 0)
		{
			return false;
		}
		CharacterStats characterStats = GetCharacterStats(objectGuid);
		if ((bool)characterStats)
		{
			return characterStats.CanUseAbilityForScript(nameStringId);
		}
		return false;
	}

	[ConditionalScript("Slot Can Use Ability", "Conditionals\\RPG")]
	[AbilityRequirement("nameStringId")]
	[ScriptParam0("Slot", "Slot to test", "0")]
	[ScriptParam1("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static bool SlotCanUseAbility(int slot, int nameStringId)
	{
		if (nameStringId < 0)
		{
			return false;
		}
		CharacterStats partyCharacterStats = GetPartyCharacterStats(slot);
		if ((bool)partyCharacterStats)
		{
			return partyCharacterStats.CanUseAbilityForScript(nameStringId);
		}
		return false;
	}

	[ConditionalScript("Is Player Level", "Conditionals\\RPG")]
	[ScriptParam0("Operator", "Operator to use.", "EqualTo")]
	[ScriptParam1("Target Level", "Level number to compare against.", "1")]
	public static bool IsPlayerLevel(Operator op, int target)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			return CompareInt(component.Level, target, op);
		}
		return false;
	}

	[ConditionalScript("Current Map Is Stringhold", "Conditionals\\Stronghold")]
	public static bool CurrentMapIsStronghold()
	{
		return GameState.Instance.CurrentMapIsStronghold;
	}

	[ConditionalScript("Can Add Prisoner", "Conditionals\\Stronghold")]
	public static bool CanAddPrisoner()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return stronghold.CanAddPrisoner();
		}
		return false;
	}

	[ConditionalScript("Stronghold Has Prisoner", "Conditionals\\Stronghold")]
	[ScriptParam0("Object", "Prisoner object to check against", "", Scripts.BrowserType.ObjectGuid)]
	public static bool StrongholdHasPrisoner(Guid objectGuid)
	{
		Stronghold stronghold = GameState.Stronghold;
		if ((bool)stronghold)
		{
			GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
			if (objectByID != null)
			{
				return stronghold.HasPrisoner(objectByID);
			}
		}
		return false;
	}

	[ConditionalScript("Stronghold Has Prisoner", "Conditionals\\Stronghold")]
	[ScriptParam0("Prisoner Name", "The name of the prisoner", "PrisonerName")]
	public static bool StrongholdHasPrisoner(string prisonerName)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return stronghold.HasPrisoner(prisonerName);
		}
		return false;
	}

	[ConditionalScript("StrongholdHasUpgrade", "Conditionals\\Stronghold")]
	[ScriptParam0("Upgrade Type", "Upgrade to check.", "None")]
	public static bool StrongholdHasUpgrade(StrongholdUpgrade.Type upgradeType)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return stronghold.HasUpgrade(upgradeType);
		}
		return false;
	}

	[ConditionalScript("StrongholdIsBuildingUpgrade", "Conditionals\\Stronghold")]
	[ScriptParam0("Upgrade Type", "Upgrade to check.", "None")]
	public static bool StrongholdIsBuildingUpgrade(StrongholdUpgrade.Type upgradeType)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return stronghold.IsBuildingUpgrade(upgradeType);
		}
		return false;
	}

	[ConditionalScript("Stronghold Is Companion available", "Conditionals\\Stronghold")]
	[ScriptParam0("Object", "Companion object to check against", "", Scripts.BrowserType.ObjectGuid)]
	public static bool StrongholdIsCompanionAvaliable(Guid objectGuid)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return stronghold.IsAvailable(objectGuid);
		}
		return false;
	}

	[ConditionalScript("Stronghold Is Security Value", "Conditionals\\Stronghold")]
	[ScriptParam0("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam1("Value", "Compare the stronghold's Security score against this value.", "0")]
	public static bool StrongholdIsSecurityValue(Operator comparisonOperator, int securityValue)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return CompareInt(stronghold.GetSecurity(), securityValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Stronghold Is Prestige Value", "Conditionals\\Stronghold")]
	[ScriptParam0("Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam1("Value", "Compare the stronghold's Prestige score against this value.", "0")]
	public static bool StrongholdIsPrestigeValue(Operator comparisonOperator, int prestigeValue)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			return CompareInt(stronghold.GetPrestige(), prestigeValue, comparisonOperator);
		}
		return false;
	}

	[ConditionalScript("Stronghold Is Visitor Dead", "Conditionals\\Stronghold")]
	[ScriptParam0("Tag", "The tag of the visitor to check.", "")]
	public static bool StrongholdIsVisitorDead(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			return Stronghold.Instance.IsVisitorDead(visitorByTag);
		}
		Debug.LogError("StrongholdIsVisitorDead: didn't find visitor with tag '" + tag + "'.");
		return false;
	}

	[ConditionalScript("Stronghold Is Visitor Present", "Conditionals\\Stronghold")]
	[ScriptParam0("Tag", "The tag of the visitor to check.", "")]
	public static bool StrongholdIsVisitorPresent(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			return Stronghold.Instance.HasVisitor(visitorByTag);
		}
		Debug.LogError("StrongholdIsVisitorPresent: didn't find visitor with tag '" + tag + "'.");
		return false;
	}
}
