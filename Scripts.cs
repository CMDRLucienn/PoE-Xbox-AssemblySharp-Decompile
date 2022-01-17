using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.Achievement;
using AI.Plan;
using MinigameData;
using UnityEngine;
using UnityEngine.AI;

public static class Scripts
{
	[Serializable]
	public enum BrowserType
	{
		None,
		GlobalVariable,
		Conversation,
		Quest,
		ObjectGuid
	}

	private static GameObject[] s_SoulMemoryCameras = null;

	public static readonly Color ConsoleNotifyColor = Color.green;

	private static object s_EndDemoMonitor = new object();

	public static AIPackageController GetAIController(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			AIPackageController component = objectByID.GetComponent<AIPackageController>();
			if ((bool)component)
			{
				return component;
			}
			Debug.LogWarning(string.Concat(objectGuid, " doesn't have a AI Package Controller."), objectByID);
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found when searching for an AI Package Controller."), null);
		return null;
	}

	private static bool IsDead(AIController aiController)
	{
		Health component = aiController.gameObject.GetComponent<Health>();
		if (component != null)
		{
			return component.Dead;
		}
		return false;
	}

	[Script("Set Package", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("AI Package", "New package for the AI", "")]
	public static void AISetPackage(Guid objectGuid, AIPackageController.PackageType newType)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		if ((bool)aIController && !IsDead(aIController))
		{
			aIController.ChangeBehavior(newType);
		}
	}

	[Script("Set Patrol State", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Should Patrol", "The new state of patrolling", "")]
	public static void AISetPatrolling(Guid objectGuid, bool shouldPatrol)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		if ((bool)aIController && !IsDead(aIController))
		{
			aIController.Patroller = shouldPatrol;
			aIController.InitAI();
		}
	}

	[Script("Set Patrol Point", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Patrol Point", "New base patrol point", "", BrowserType.ObjectGuid)]
	public static void AISetPatrolPoint(Guid objectGuid, Guid waypointGuid)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		if ((bool)aIController && !IsDead(aIController))
		{
			aIController.Patroller = true;
			aIController.PreferredPatrolPoint = InstanceID.GetObjectByID(waypointGuid);
			aIController.InitAI();
		}
	}

	[Script("Move to Point", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Point", "Object to path to", "", BrowserType.ObjectGuid)]
	[ScriptParam2("Movement Type", "How to get there", "")]
	public static void AIPathToPoint(Guid objectGuid, Guid waypointGuid, AnimationController.MovementType moveType)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		GameObject objectByID = InstanceID.GetObjectByID(waypointGuid);
		if (!(aIController != null) || !(objectByID != null) || IsDead(aIController))
		{
			return;
		}
		Waypoint component = objectByID.GetComponent<Waypoint>();
		KnockedDown knockedDown = aIController.StateManager.CurrentState as KnockedDown;
		if ((bool)component)
		{
			if (!(aIController.StateManager.CurrentState is Patrol) || !(aIController.CurrentWaypoint == component))
			{
				aIController.Patroller = true;
				if (knockedDown == null)
				{
					aIController.StateManager.PopAllStates();
				}
				else
				{
					aIController.StateManager.ClearQueuedStates();
					knockedDown.Standup();
				}
				aIController.CurrentWaypoint = null;
				aIController.PrevWaypoint = null;
				aIController.RecordRetreatPosition(component.transform.position);
				Patrol patrol = AIStateManager.StatePool.Allocate<Patrol>();
				patrol.StartPoint = component;
				component.WalkOnly = moveType == AnimationController.MovementType.Walk;
				patrol.TargetScanner = aIController.GetTargetScanner();
				aIController.StateManager.PushState(patrol);
				if (aIController.StateManager.DefaultState is ScanForTarget scanForTarget)
				{
					patrol.TargetScanner = scanForTarget.TargetScanner;
				}
				else if (aIController.StateManager.DefaultState is CasterScanForTarget casterScanForTarget)
				{
					patrol.TargetScanner = casterScanForTarget.TargetScanner;
				}
			}
		}
		else
		{
			if (knockedDown == null)
			{
				aIController.StateManager.PopAllStates();
			}
			else
			{
				aIController.StateManager.ClearQueuedStates();
				knockedDown.Standup();
			}
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			pathToPosition.Parameters.Destination = objectByID.transform.position;
			pathToPosition.Parameters.MovementType = moveType;
			aIController.StateManager.PushState(pathToPosition);
		}
	}

	[Script("Force Attack", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Target", "Object to attack", "", BrowserType.ObjectGuid)]
	public static void AIForceAttack(Guid objectGuid, Guid targetGuid)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		GameObject objectByID = InstanceID.GetObjectByID(targetGuid);
		if ((bool)aIController && (bool)objectByID && !IsDead(aIController))
		{
			ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
			aIController.StateManager.PushState(approachTarget);
			approachTarget.TargetScanner = aIController.GetTargetScanner();
			approachTarget.Target = objectByID;
			approachTarget.IsForceAttack = true;
			if (approachTarget.TargetScanner == null)
			{
				approachTarget.Attack = AIController.GetPrimaryAttack(aIController.gameObject);
			}
		}
	}

	[Script("Set Busy State", "Scripts\\AI")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Is Busy", "The new state of busy", "")]
	public static void AISetBusy(Guid objectGuid, bool isBusy)
	{
		AIPackageController aIController = GetAIController(objectGuid);
		if ((bool)aIController && !IsDead(aIController))
		{
			aIController.IsBusy = isBusy;
		}
	}

	[Script("Knock Down", "Scripts\\AI")]
	[ScriptParam0("Target", "Character to knock down", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Duration", "The time is seconds for the character to lie down", "1.0")]
	public static void KnockDown(Guid targetGuid, float duration)
	{
		GameObject objectByID = InstanceID.GetObjectByID(targetGuid);
		if (!objectByID)
		{
			return;
		}
		AIController component = objectByID.GetComponent<AIController>();
		if (!(component == null) && !IsDead(component))
		{
			if (duration < 0f)
			{
				duration = 0.1f;
			}
			if (component.StateManager.CurrentState is KnockedDown knockedDown)
			{
				knockedDown.ResetKnockedDown(duration);
				return;
			}
			KnockedDown knockedDown2 = AIStateManager.StatePool.Allocate<KnockedDown>();
			component.StateManager.PushState(knockedDown2);
			knockedDown2.SetKnockdownTime(duration);
		}
	}

	[Script("Player Safe Mode", "Scripts\\AI")]
	[ScriptParam0("Enabled", "True to disable party character AIs and Input, false to restore", "true")]
	public static void PlayerSafeMode(bool enabled)
	{
		if (enabled && (bool)TimeController.Instance)
		{
			TimeController.Instance.SafePaused = false;
		}
		GameState.PlayerSafeMode = enabled;
		GameInput.DisableInput = enabled;
		UICamera.DisableSelectionInput = enabled;
		if (!enabled)
		{
			return;
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				AIController component = partyMemberAI.gameObject.GetComponent<AIController>();
				if (component != null && !(component.StateManager.CurrentState is Unconscious))
				{
					component.InterruptAnimationForCutscene();
					component.StateManager.PopAllStates();
				}
			}
		}
		if ((bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.CloseAllWindows();
		}
	}

	[Script("Force In Combat Idle", "Scripts\\AI")]
	[ScriptParam0("NPC", "Set the combat idle stance for this character.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("In Combat Idle", "Whether the character should be a combat idle stance.", "true")]
	public static void ForceInCombatIdle(Guid objectGuid, bool inCombatIdle)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			AnimationController component = objectByID.GetComponent<AnimationController>();
			if (!(component == null))
			{
				component.ForceCombatIdle = inCombatIdle;
			}
		}
	}

	[Script("Force Combat Pathing", "Scripts\\AI")]
	[ScriptParam0("NPC", "Set combat pathing for this character.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Use Combat Pathing", "Whether the character should be forced to use combat pathing.", "true")]
	public static void ForceCombatPathing(Guid objectGuid, bool useCombatPathing)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Mover component = objectByID.GetComponent<Mover>();
			if (!(component == null))
			{
				component.ForceCombatPathing = useCombatPathing;
			}
		}
	}

	[Script("Clear Perception State", "Scripts\\AI")]
	[ScriptParam0("Character", "Clear the perception state on this character.", "", BrowserType.ObjectGuid)]
	public static void ClearPerceptionState(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			AIController component = objectByID.GetComponent<AIController>();
			if ((bool)component)
			{
				component.GetTargetScanner()?.ClearPerceptionState();
			}
		}
	}

	[Script("Play Sound From Bank", "Scripts\\Audio")]
	[ScriptParam0("Object", "Object with audiobank", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Sound Cue", "The name of the cue in the audio bank.", "")]
	public static void PlaySound(Guid objectGuid, string audioCue)
	{
		AudioBank componentByGuid = GetComponentByGuid<AudioBank>(objectGuid);
		if ((bool)componentByGuid)
		{
			componentByGuid.PlayFrom(audioCue);
		}
	}

	[Script("Play Sound From Source", "Scripts\\Audio")]
	[ScriptParam0("Object", "Object with AudioSource", "", BrowserType.ObjectGuid)]
	public static void PlaySound(Guid objectGuid)
	{
		AudioSource componentByGuid = GetComponentByGuid<AudioSource>(objectGuid);
		if ((bool)componentByGuid)
		{
			GlobalAudioPlayer.Play(componentByGuid);
		}
	}

	[Script("Play Sound From Sound Set", "Scripts\\Audio")]
	[ScriptParam0("Object", "Object with Sound Set", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Sound Action", "Sound To Play", "Invalid")]
	[ScriptParam2("Sound Variation", "Variation to play", "-1")]
	public static void PlaySoundFromSoundSet(Guid objectGuid, SoundSet.SoundAction action, int variation)
	{
		SoundSetComponent componentByGuid = GetComponentByGuid<SoundSetComponent>(objectGuid);
		if ((bool)componentByGuid && (bool)componentByGuid.SoundSet)
		{
			componentByGuid.PlaySound(action, variation, skipIfConversing: false);
		}
	}

	[Script("Play Commentary", "Scripts\\Audio")]
	[ScriptParam0("Object", "Object with a developer commentary component", "", BrowserType.ObjectGuid)]
	public static void PlayCommentary(Guid objectGuid)
	{
		DeveloperCommentary componentByGuid = GetComponentByGuid<DeveloperCommentary>(objectGuid);
		if ((bool)componentByGuid)
		{
			componentByGuid.Queue();
		}
	}

	[Script("Play Music", "Scripts\\Audio")]
	[ScriptParam0("Audio Clip Filename", "AudioClip path relative to the Resources folder.", "")]
	public static void PlayMusic(string filename)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(filename);
		if (audioClip == null)
		{
			Debug.LogError("PlayMusic failed, file not found or is not 44100hz: " + filename);
			return;
		}
		MusicManager.FadeParams fadeParams = new MusicManager.FadeParams();
		MusicManager.Instance.PlayMusic(audioClip, fadeParams);
	}

	[Script("Play Scripted Music", "Scripts\\Audio")]
	[ScriptParam0("Clip Name", "The scripted clip name specified in Area Music", "")]
	[ScriptParam1("Block Combat Music", "Prevents game from playing normal combat music", "false")]
	[ScriptParam2("Fade Type", "The type of fade to use when transitioning to new music.", "FadeOutPauseFadeIn")]
	[ScriptParam3("Fade Out Duration", "The length in seconds to fade out new music.", "0.5")]
	[ScriptParam4("Fade In Duration", "The length in seconds to fade in new music.", "0.5")]
	[ScriptParam5("Pause Duration", "The length in seconds to fade in new music.", "0.5")]
	[ScriptParam6("Loop", "Designates if the music should loop.", "false")]
	public static void PlayScriptedMusic(string filename, bool blockCombatMusic, MusicManager.FadeType fadeType, float fadeOutDuration, float fadeInDuration, float pauseDuration, bool loop)
	{
		MusicManager.Instance.PlayScriptedMusic(filename, blockCombatMusic, fadeType, fadeOutDuration, fadeInDuration, pauseDuration, loop);
	}

	[Script("End Scripted Music", "Scripts\\Audio")]
	public static void EndScriptedMusic()
	{
		MusicManager.Instance.EndScriptedMusic();
	}

	[Script("Resume Area Music", "Scripts\\Audio")]
	public static void ResumeAreaMusic()
	{
		MusicManager.Instance.ResumeScriptedOrNormalMusic(resumeActiveSource: true);
	}

	[Script("Fade Out Area Music", "Scripts\\Audio")]
	public static void FadeOutAreaMusic()
	{
		MusicManager.Instance.FadeOutAreaMusic(resetWhenFaded: false);
	}

	[Script("Enable Music Loop Cooldown", "Scripts\\Audio")]
	public static void EnableMusicLoopCooldown()
	{
		MusicManager.Instance.EnableLoopCooldown();
	}

	[Script("Disable Music Loop Cooldown", "Scripts\\Audio")]
	public static void DisableMusicLoopCooldown()
	{
		MusicManager.Instance.DisableLoopCooldown();
	}

	[Script("Fade Out Audio", "Scripts\\Audio")]
	[ScriptParam0("Parent Object", "The parent object with the audio sources", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Fade Time", "How long to fade the audio in seconds.", "1.0")]
	public static void FadeOutAudio(Guid parentObject, float fadeTime)
	{
		GameObject objectByID = InstanceID.GetObjectByID(parentObject);
		if ((bool)objectByID)
		{
			MusicManager.Instance.FadeAllAudioSourcesOut(objectByID, fadeTime);
		}
	}

	[Script("Fade In Audio", "Scripts\\Audio")]
	[ScriptParam0("Parent Object", "The parent object with the audio sources", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Fade Time", "How long to fade the audio in seconds.", "1.0")]
	public static void FadeInAudio(Guid parentObject, float fadeTime)
	{
		GameObject objectByID = InstanceID.GetObjectByID(parentObject);
		if ((bool)objectByID)
		{
			MusicManager.Instance.FadeAllAudioSourcesIn(objectByID, fadeTime);
		}
	}

	[Script("Override Fatigue Whispers", "Scripts\\Audio")]
	[ScriptParam0("New Volume", "The new volume to force the fatigue whispers to.", "0.0")]
	public static void OverrideFatigueWhispers(float newVolume)
	{
		if ((bool)FatigueWhispers.Instance)
		{
			FatigueWhispers.Instance.SetVolumeOverride(newVolume);
		}
	}

	[Script("Release Fatigue Whisper Override", "Scripts\\Audio")]
	public static void ReleaseFatigueWhisperOverride()
	{
		if ((bool)FatigueWhispers.Instance)
		{
			FatigueWhispers.Instance.ReleaseVolumeOverride();
		}
	}

	public static Faction GetFactionComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			Faction component = objectByID.GetComponent<Faction>();
			if ((bool)component)
			{
				return component;
			}
			Debug.LogWarning(string.Concat(objectGuid, " doesn't have a faction component."), objectByID);
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found when searching for faction component."), null);
		return null;
	}

	[Script("Set Is Hostile", "Scripts\\Faction")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Is Hostile", "Hostile state towards the player", "true")]
	public static void SetIsHostile(Guid objectGuid, bool isHostile)
	{
		Faction factionComponent = GetFactionComponent(objectGuid);
		if ((bool)factionComponent)
		{
			factionComponent.RelationshipToPlayer = (isHostile ? Faction.Relationship.Hostile : Faction.Relationship.Neutral);
		}
	}

	[Script("Set Team Relationship", "Scripts\\Faction")]
	[ScriptParam0("Team A", "The first team to change", "")]
	[ScriptParam1("Team B", "The second team to change", "")]
	[ScriptParam2("Relationship", "How team A and B will relate to each other", "")]
	public static void SetTeamRelationship(string teamA, string teamB, Faction.Relationship newRelationship)
	{
		Team teamByTag = Team.GetTeamByTag(teamA);
		if (teamByTag == null)
		{
			Debug.LogError("SetTeamRelationship has an error. " + teamA + " does not exist. Make sure you match up the script tag.");
			return;
		}
		Team teamByTag2 = Team.GetTeamByTag(teamB);
		if (teamByTag2 == null)
		{
			Debug.LogError("SetTeamRelationship has an error. " + teamB + " does not exist. Make sure you match up the script tag.");
			return;
		}
		teamByTag.SetRelationship(teamByTag2, newRelationship, mutual: true);
		teamByTag2.SetRelationship(teamByTag, newRelationship, mutual: true);
	}

	[Script("Reputation Add Points By Tag", "Scripts\\Faction")]
	[AdjustStat("axis", "strength", "id")]
	[ScriptParam0("Faction Name", "Faction tag to modify", "")]
	[ScriptParam1("Axis", "Good vs. Bad action", "Positive")]
	[ScriptParam2("Strength", "Severity of the change", "Minor")]
	public static void ReputationAddPoints(FactionName id, Reputation.Axis axis, Reputation.ChangeStrength strength)
	{
		Reputation reputation = ReputationManager.Instance.GetReputation(id);
		if (reputation != null)
		{
			reputation.AddReputation(axis, strength);
			Debug.Log(id.ToString() + " reputation changed on the " + axis.ToString() + " axis (" + strength.ToString() + ").");
		}
		else
		{
			Debug.LogError("Faction " + id.ToString() + " is not setup!");
		}
	}

	[Script("Disposition Add Points", "Scripts\\Disposition")]
	[AdjustStat("axis", "strength")]
	[ScriptParam0("Axis", "Action type", "Aggressive")]
	[ScriptParam1("Strength", "Severity of the change", "Minor")]
	public static void DispositionAddPoints(Disposition.Axis axis, Disposition.Strength strength)
	{
		ReputationManager.Instance.PlayerDisposition.ChangeDisposition(axis, strength);
		Debug.Log("Disposition change: " + axis.ToString() + " " + strength.ToString() + " change.");
	}

	private static bool IsDead(GameObject gameObject)
	{
		Health component = gameObject.GetComponent<Health>();
		if (component != null)
		{
			return component.Dead;
		}
		return false;
	}

	private static bool IsDeadOrMaimed(GameObject gameObject)
	{
		Health component = gameObject.GetComponent<Health>();
		if (component != null)
		{
			if (component.Dead)
			{
				return !component.MaimAvailable();
			}
			return false;
		}
		return false;
	}

	[Script("Print String", "Scripts\\General")]
	[ScriptParam0("Text", "string to print to the console", "")]
	public static void PrintString(string text)
	{
		Console.AddMessage(text);
	}

	[Script("Activate Object With VFX", "Scripts\\General")]
	[ScriptParam0("Object", "Object to activate.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("VFX Prefab Name", "The game object name of the vfx prefab to use.", "")]
	[ScriptParam2("Active", "Active state to set the object to.", "true")]
	public static void ActivateObjectWithVfx(Guid objectGuid, string vfxPrefabName, bool activate)
	{
		ActivateObjectHelper(objectGuid, vfxPrefabName, activate);
	}

	[Script("Activate Object", "Scripts\\General")]
	[ScriptParam0("Object", "Object to activate.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Active", "Active state to set the object to.", "true")]
	public static void ActivateObject(Guid objectGuid, bool activate)
	{
		ActivateObjectHelper(objectGuid, null, activate);
	}

	public static void ActivateObjectHelper(Guid objectGuid, string vfx, bool activate)
	{
		if ((bool)ConditionalToggleManager.Instance)
		{
			GameObject objectByID = ConditionalToggleManager.Instance.GetObjectByID(objectGuid);
			if ((bool)objectByID && objectByID.activeSelf != activate)
			{
				ActivateObjectHelper(objectByID, vfx, activate);
			}
		}
	}

	public static void ActivateObjectHelper(GameObject gameObject, string vfx, bool activate)
	{
		if (!gameObject)
		{
			return;
		}
		if (!activate)
		{
			bool num = ActivationTimers.ActivateWithVfx(gameObject, vfx, state: false);
			Persistence component = gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				component.SaveObject();
			}
			if (!num)
			{
				return;
			}
			ConditionalToggle component2 = gameObject.GetComponent<ConditionalToggle>();
			if ((bool)component2)
			{
				component2.ActivateOnlyThroughScript = true;
				component2.StartActivated = false;
				InstanceID component3 = gameObject.GetComponent<InstanceID>();
				if (component3 != null)
				{
					component3.Load();
					ConditionalToggleManager.Instance.AddToScriptInactiveList(component2);
				}
			}
		}
		else
		{
			bool num2 = ActivationTimers.ActivateWithVfx(gameObject, vfx, state: true);
			Persistence component4 = gameObject.GetComponent<Persistence>();
			if ((bool)component4)
			{
				component4.SaveObject();
			}
			if (num2)
			{
				ConditionalToggle component5 = gameObject.GetComponent<ConditionalToggle>();
				component5.ForceActivate();
				ConditionalToggleManager.Instance.ScriptInactiveList.Remove(component5);
			}
		}
	}

	[Script("Start Conversation Facing Listener", "Scripts\\Conversation")]
	[ScriptParam0("Object", "Speaker Object", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Conversation", "Conversation File", "", BrowserType.Conversation)]
	[ScriptParam2("Conversation Node ID", "Conversation Node ID", "0")]
	public static void StartConversationFacingListener(Guid objectGuid, string conversation, int nodeID)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID && !IsDeadOrMaimed(objectByID))
		{
			FlowChartPlayer flowChartPlayer = ConversationManager.Instance.StartConversation(conversation, nodeID, objectByID, FlowChartPlayer.DisplayMode.Standard);
			List<GameObject> activeObjectsForSpeakerGuid = ConversationManager.GetActiveObjectsForSpeakerGuid(flowChartPlayer.GetCurrentNode().GetListenerGuid());
			if (activeObjectsForSpeakerGuid != null && activeObjectsForSpeakerGuid.Count > 0)
			{
				NPCDialogue.SpeakerFaceUser(flowChartPlayer, objectByID, activeObjectsForSpeakerGuid[0]);
			}
			else
			{
				NPCDialogue.SpeakerFaceUser(flowChartPlayer, objectByID, GameState.s_playerCharacter.gameObject);
			}
		}
	}

	[Script("Start Conversation", "Scripts\\Conversation")]
	[ScriptParam0("Object", "Speaker Object", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Conversation", "Conversation File", "", BrowserType.Conversation)]
	[ScriptParam2("Conversation Node ID", "Conversation Node ID", "0")]
	public static void StartConversation(Guid objectGuid, string conversation, int nodeID)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID && !IsDeadOrMaimed(objectByID))
		{
			ConversationManager.Instance.StartConversation(conversation, nodeID, objectByID, FlowChartPlayer.DisplayMode.Standard);
		}
	}

	[Script("Start Scripted Interaction", "Scripts\\Interaction")]
	[ScriptParam0("Object", "Scripted Interaction Object", "", BrowserType.ObjectGuid)]
	public static void StartScriptedInteraction(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID && !IsDeadOrMaimed(objectByID))
		{
			ScriptedInteraction component = objectByID.GetComponent<ScriptedInteraction>();
			if ((bool)component && GameState.CutsceneAllowed)
			{
				ConversationManager.Instance.KillAllBarkStrings();
				component.StartConversation();
			}
		}
	}

	[Script("Start Cutscene", "Scripts\\Cutscene")]
	[ScriptParam0("Object", "Cutscene Object", "", BrowserType.ObjectGuid)]
	public static void StartCutscene(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			Cutscene component = objectByID.GetComponent<Cutscene>();
			if ((bool)component)
			{
				component.StartCutscene();
			}
		}
	}

	[Script("Area Transition", "Scripts\\Area")]
	[ScriptParam0("Area Name", "The name of the area", "")]
	[ScriptParam1("Start Point Name", "The name of the start point", "")]
	public static void AreaTransition(MapType areaName, StartPoint.PointLocation startPoint)
	{
		GameState.s_playerCharacter.StartPointLink = startPoint;
		UIDifficultyScaling.PromptScalersAndChangeLevel(areaName, null);
	}

	[Script("Add Experience", "Scripts\\Quest")]
	[ScriptParam0("Experience", "Amount of XP to reward", "50")]
	public static void AddExperience(int XP)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				component.AddExperience(XP);
			}
		}
		Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1016), XP), Color.green);
	}

	[Script("Add Experience To Level", "Scripts\\Quest")]
	[ScriptParam0("Level", "The level you want to level to", "12")]
	public static void AddExperienceToLevel(int level)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
			if ((bool)component && component.Level < level)
			{
				component.Experience = CharacterStats.ExperienceNeededForLevel(level);
			}
		}
	}

	[Script("Add Experience Player", "Scripts\\Quest")]
	[ScriptParam0("Experience", "Amount of XP to reward", "50")]
	public static void AddExperiencePlayer(int XP)
	{
		if (GameState.s_playerCharacter == null)
		{
			return;
		}
		PartyMemberAI component = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
		if (component == null)
		{
			return;
		}
		if (!component.Secondary)
		{
			CharacterStats component2 = component.GetComponent<CharacterStats>();
			if ((bool)component2)
			{
				component2.AddExperience(XP);
			}
		}
		Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1016), XP), Color.green);
	}

	[Script("Set Interaction Image", "Scripts\\Interaction")]
	[ScriptParam0("Image Index", "The interaction image index", "0")]
	public static void SetInteractionImage(int index)
	{
		if ((bool)ScriptedInteraction.ActiveInteraction)
		{
			ScriptedInteraction.ActiveInteraction.SetState(index);
		}
	}

	[Script("Play Script Audio Clip", "Scripts\\Interaction")]
	[ScriptParam0("Clip Index", "The scripted audio clip index", "0")]
	public static void PlayInteractionAudioClip(int index)
	{
		if ((bool)ScriptedInteraction.ActiveInteraction)
		{
			ScriptedInteraction.ActiveInteraction.PlayScriptAudioClip(index);
		}
	}

	[Script("Flip Tile", "Scripts\\Tile")]
	[ScriptParam0("Object", "Flip tile object", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Frame", "The interaction image index", "0")]
	public static void FlipTile(Guid objectGuid, int frame)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			TileFlipper component = objectByID.GetComponent<TileFlipper>();
			if ((bool)component)
			{
				component.Flip(frame);
			}
		}
	}

	[Script("Add to Party", "Scripts\\Party")]
	[ScriptParam0("Tag", "Companion to Add", "", BrowserType.ObjectGuid)]
	public static void AddToParty(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			Debug.LogWarning("Could not find object to add to party.");
			return;
		}
		for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
		{
			if (PartyMemberAI.PartyMembers[i] != null && PartyMemberAI.PartyMembers[i].gameObject == objectByID)
			{
				return;
			}
		}
		PartyMemberAI component = objectByID.GetComponent<PartyMemberAI>();
		if ((bool)component)
		{
			component.AssignedSlot = PartyMemberAI.NextAvailablePrimarySlot;
		}
		PartyMemberAI.AddToActiveParty(objectByID, fromScript: true);
	}

	[Script("Remove from Party", "Scripts\\Party")]
	[ScriptParam0("Tag", "Companion to Remove", "", BrowserType.ObjectGuid)]
	public static void RemoveFromParty(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			Debug.LogWarning("Could not find object to remove party.");
			return;
		}
		PartyMemberAI component = objectByID.GetComponent<PartyMemberAI>();
		if (component == null)
		{
			Debug.LogWarning("Object " + objectByID.name + " doesn't have a PartyMemberAI. Cannot remove from party.");
		}
		else
		{
			PartyMemberAI.RemoveFromActiveParty(component, purgePersistencePacket: true);
		}
	}

	[Script("Teleport Party To Location", "Scripts\\Movement")]
	[ScriptParam0("Tag", "Location to teleport to ", "", BrowserType.ObjectGuid)]
	public static void TeleportPartyToLocation(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		PartyWaypoint component = objectByID.GetComponent<PartyWaypoint>();
		if ((bool)component)
		{
			component.TeleportPartyToLocation();
		}
		else
		{
			int num = 0;
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (partyMemberAI != null)
				{
					partyMemberAI.transform.position = objectByID.transform.position;
					partyMemberAI.transform.rotation = objectByID.transform.rotation;
				}
				num++;
			}
		}
		CameraControl.Instance.FocusOnPoint(objectByID.transform.position);
	}

	[Script("Teleport Player To Location", "Scripts\\Movement")]
	[ScriptParam0("Tag", "Location to teleport to ", "", BrowserType.ObjectGuid)]
	public static void TeleportPlayerToLocation(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null) && !(GameState.s_playerCharacter == null))
		{
			GameState.s_playerCharacter.transform.position = objectByID.transform.position;
			GameState.s_playerCharacter.transform.rotation = objectByID.transform.rotation;
			CameraControl.Instance.FocusOnPoint(objectByID.transform.position);
		}
	}

	[Script("Teleport Object To Location", "Scripts\\Movement")]
	[ScriptParam0("Object", "The object to teleport", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Tag", "Location to teleport to.", "", BrowserType.ObjectGuid)]
	public static void TeleportObjectToLocation(Guid objectGuid, Guid targetGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(targetGuid);
		if (!(objectByID2 == null) && !(objectByID == null))
		{
			AIController component = objectByID.GetComponent<AIController>();
			if (component != null)
			{
				component.RecordRetreatPosition(objectByID2.transform.position);
			}
			objectByID.transform.position = objectByID2.transform.position;
			objectByID.transform.rotation = objectByID2.transform.rotation;
		}
	}

	[Script("Start Timer", "Scripts\\Timer")]
	[ScriptParam0("Object", "The timer object", "", BrowserType.ObjectGuid)]
	public static void StartTimer(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Timer component = objectByID.GetComponent<Timer>();
			if ((bool)component)
			{
				component.StartTimer();
			}
		}
	}

	[Script("Stop Timer", "Scripts\\Timer")]
	[ScriptParam0("Object", "The timer object", "", BrowserType.ObjectGuid)]
	public static void StopTimer(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Timer component = objectByID.GetComponent<Timer>();
			if ((bool)component)
			{
				component.StopTimer();
			}
		}
	}

	[Script("Set Timer", "Scripts\\Timer")]
	[ScriptParam0("Object", "The timer object", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Time", "New delay time for timer", "1")]
	public static void SetTimer(Guid objectGuid, float time)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Timer component = objectByID.GetComponent<Timer>();
			if ((bool)component)
			{
				component.Delay = time;
			}
		}
	}

	[Script("Rest", "Scripts\\Time")]
	public static void Rest()
	{
		RestZone.ShowRestUI(RestZone.Mode.Scripted);
	}

	[Script("Rest With Movie ID", "Scripts\\Time")]
	[ScriptParam0("MovieType", "This is the movie enum to display", "Inn")]
	public static void RestWithMovieID(RestMovieMode movie)
	{
		RestZone.ShowRestUI(RestZone.Mode.Scripted, movie);
	}

	[Script("Advance Time By Hours", "Scripts\\Time")]
	[ScriptParam0("Hours", "Hours to advance", "8")]
	public static void AdvanceTimeByHours(int hours)
	{
		WorldTime.Instance.AdvanceTimeByHours(hours, isResting: false);
		RestZone.TriggerOnResting();
	}

	[Script("Advance Time By Hours No Rest", "Scripts\\Time")]
	[ScriptParam0("Hours", "Hours to advance", "8")]
	public static void AdvanceTimeByHoursNoRest(int hours)
	{
		WorldTime.Instance.AdvanceTimeByHours(hours, isResting: false);
	}

	[Script("Advance Time To Hour", "Scripts\\Time")]
	[ScriptParam0("Hour", "Hour to advance to", "0")]
	public static void AdvanceTimeToHour(int hour)
	{
		WorldTime.Instance.AdvanceTimeToHour(hour);
		RestZone.TriggerOnResting();
	}

	[Script("Change Water Level", "Scripts\\Water")]
	[ScriptParam0("Object", "The water plane", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Water Level", "New Water Level", "0")]
	[ScriptParam2("Timer", "Time to move to new level", "1")]
	public static void ChangeWaterLevel(Guid objectGuid, float newLevel, float time)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			WaterLevelController component = objectByID.GetComponent<WaterLevelController>();
			if (component != null)
			{
				component.ChangeWaterLevel(newLevel, time);
			}
		}
	}

	[Script("Set Trigger Enabled", "Scripts\\Trigger")]
	[ScriptParam0("Object", "The trigger", "", BrowserType.ObjectGuid)]
	[ScriptParam1("isEnabled", "New Enabled Value", "true")]
	public static void SetTriggerEnabled(Guid objectGuid, bool isEnabled)
	{
		Trigger componentByGuid = GetComponentByGuid<Trigger>(objectGuid);
		if (componentByGuid != null)
		{
			componentByGuid.IsEnabled = isEnabled;
		}
	}

	[Script("Reset Trigger Charges", "Scripts\\Trigger")]
	[ScriptParam0("Object", "The trigger", "", BrowserType.ObjectGuid)]
	public static void ResetTriggerCharges(Guid objectGuid)
	{
		Trigger componentByGuid = GetComponentByGuid<Trigger>(objectGuid);
		if ((bool)componentByGuid)
		{
			componentByGuid.ResetCharges();
		}
	}

	[Script("Set Switch Enabled", "Scripts\\Switch")]
	[ScriptParam0("Object", "The switch", "", BrowserType.ObjectGuid)]
	[ScriptParam1("isEnabled", "New Enabled Value", "true")]
	public static void SetSwitchEnabled(Guid objectGuid, bool isEnabled)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Switch component = objectByID.GetComponent<Switch>();
			if (component != null)
			{
				component.Enabled = isEnabled;
			}
		}
	}

	[Script("World Map Set Visibility", "Scripts\\Trigger")]
	[ScriptParam0("Map", "Map to change status of", "Map")]
	[ScriptParam1("Visibility", "New visibility status", "Locked")]
	public static void WorldMapSetVisibility(MapType map, MapData.VisibilityType visibility)
	{
		WorldMap.Instance.SetVisibility(map, visibility);
	}

	[Script("Fade To Black", "Scripts\\Fade")]
	[ScriptParam0("Fade Time", "Fade time", "2.0")]
	[ScriptParam1("Fade Music", "Fade music", "true")]
	[ScriptParam2("Fade Ambient Audio", "Fade all ambient audio", "true")]
	public static void FadeToBlack(float time, bool music, bool audio)
	{
		AudioFadeMode audioFadeMode = AudioFadeMode.None;
		if (music)
		{
			audioFadeMode |= AudioFadeMode.Music;
		}
		if (audio)
		{
			audioFadeMode |= AudioFadeMode.Fx;
		}
		FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, time, audioFadeMode);
	}

	[Script("Fade From Black", "Scripts\\Fade")]
	[ScriptParam0("Fade Time", "Fade time", "2.0")]
	[ScriptParam1("Fade Music", "Fade music", "true")]
	[ScriptParam2("Fade Ambient Audio", "Fade all ambient audio", "true")]
	public static void FadeFromBlack(float time, bool music, bool audio)
	{
		AudioFadeMode audioFadeMode = AudioFadeMode.None;
		if (music)
		{
			audioFadeMode |= AudioFadeMode.Music;
		}
		if (audio)
		{
			audioFadeMode |= AudioFadeMode.Fx;
		}
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, time, audioFadeMode);
	}

	[Script("Encounter Spawn", "Scripts\\Encounter")]
	[ScriptParam0("Object", "The Encounter", "", BrowserType.ObjectGuid)]
	public static void EncounterSpawn(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Encounter component = objectByID.GetComponent<Encounter>();
			if (component != null)
			{
				component.ForceSpawn();
			}
		}
	}

	[Script("Encounter Despawn", "Scripts\\Encounter")]
	[ScriptParam0("Object", "The Encounter", "", BrowserType.ObjectGuid)]
	public static void EncounterDespawn(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Encounter component = objectByID.GetComponent<Encounter>();
			if (component != null)
			{
				component.DeSpawn();
			}
		}
	}

	[Script("Encounter Set Combat End When All Are Dead Flag", "Scripts\\Encounter")]
	[ScriptParam0("Object", "The Encounter", "", BrowserType.ObjectGuid)]
	[ScriptParam1("CombatEndWhenAllAreDeadFlag", "Set this value", "true")]
	public static void EncounterSetCombatEndWhenAllAreDeadFlag(Guid objectGuid, bool boolValue)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			Encounter component = objectByID.GetComponent<Encounter>();
			if (component != null)
			{
				component.SetCombatEndsWhenAllAreDeadAll(boolValue);
			}
		}
	}

	[Script("SetNavMeshObstacleActivated", "Scripts\\NavObstacle")]
	[ScriptParam0("Object", "The nav mesh obstactle to affect", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Is Active", "Sets if the obstacle is activated or not.", "true")]
	public static void SetNavMeshObstacleActivated(Guid objectGuid, bool isActive)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID == null))
		{
			NavMeshObstacle component = objectByID.GetComponent<NavMeshObstacle>();
			if (component != null)
			{
				component.carving = isActive;
			}
		}
	}

	[Script("Set Background", "Scripts\\Background")]
	[ScriptParam0("Object", "The character to set background on", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Background", "The background value", "0")]
	public static void SetBackground(Guid objectGuid, CharacterStats.Background background)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if ((bool)characterStatsComponent)
		{
			characterStatsComponent.CharacterBackground = background;
		}
	}

	[Script("Set Player Background", "Scripts\\PlayerBackground")]
	[ScriptParam0("Background", "The background value", "0")]
	public static void SetPlayerBackground(CharacterStats.Background background)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			component.CharacterBackground = background;
		}
	}

	[Script("Unlock Present Story Item", "Scripts\\PlayerBackground")]
	[ScriptParam0("Key", "The string key of the item to unlock", "")]
	public static void UnlockPresentStoryItem(string key)
	{
		if ((bool)JournalBiographyManager.Instance)
		{
			JournalBiographyManager.Instance.UnlockPresentStory(key);
		}
		else
		{
			Debug.LogError("JournalBiography manager not initialized in UnlockPresentStoryItem.");
		}
	}

	[Script("Unlock Past Story Item", "Scripts\\PlayerBackground")]
	[ScriptParam0("Key", "The string key of the item to unlock", "")]
	public static void UnlockPastStoryItem(string key)
	{
		if ((bool)JournalBiographyManager.Instance)
		{
			JournalBiographyManager.Instance.UnlockPastStory(key);
		}
		else
		{
			Debug.LogError("JournalBiography manager not initialized in UnlockPastStoryItem.");
		}
	}

	[Script("End Game", "Scripts\\General")]
	public static void EndGame()
	{
		UIEndGameSlidesManager.Instance.ShowWindow();
	}

	[Script("Autosave", "Scripts\\General")]
	public static void Autosave()
	{
		GameState.Autosave();
	}

	[Script("Screen Shake", "Scripts\\Camera")]
	[ScriptParam0("Duration", "Duration of the shake", "0")]
	[ScriptParam1("Strength", "Strength of the shake", "0")]
	public static void ScreenShake(float duration, float strength)
	{
		CameraControl.Instance.ScreenShake(duration, strength);
	}

	[Script("Increment Tracked Achievement Stat", "Scripts\\General")]
	[ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat")]
	public static void IncrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat)
	{
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(stat);
		}
	}

	[Script("Decrement Tracked Achievement Stat", "Scripts\\General")]
	[ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat")]
	public static void DecrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat)
	{
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.DecrementTrackedStat(stat);
		}
	}

	[Script("Set Tracked Achievement Stat", "Scripts\\General")]
	[ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat")]
	[ScriptParam1("New Value", "The new value we are setting the tracked stat to.", "0")]
	public static void IncrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat, int value)
	{
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.ForceSetTrackedStat(stat, value);
		}
	}

	[Script("Select Weapon Set", "Scripts\\General")]
	[ScriptParam0("Object", "Character", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Weapon Set ID", "The index of the weapon set to apply.", "0")]
	public static void SelectWeaponSet(Guid objectGuid, int weaponSet)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID && !IsDead(objectByID))
		{
			Equipment component = objectByID.GetComponent<Equipment>();
			if ((bool)component)
			{
				component.SelectWeaponSet(weaponSet, enforceRecoveryPenalty: false);
			}
		}
	}

	[Script("Soul Memory Camera Enable", "Scripts\\Camera")]
	[ScriptParam0("Enabled", "Whether the camera should be enabled or disabled", "true")]
	public static void SoulMemoryCameraEnable(bool enabled)
	{
		SoulMemoryCameraEnableHelper(enabled, FullscreenCameraEffectType.Purple, keepMusic: false);
	}

	[Script("Soul Memory Camera Enable Keep Music", "Scripts\\Camera")]
	[ScriptParam0("Enabled", "Whether the camera should be enabled or disabled", "true")]
	public static void SoulMemoryCameraEnableKeepMusic(bool enabled)
	{
		SoulMemoryCameraEnableHelper(enabled, FullscreenCameraEffectType.Purple, keepMusic: true);
	}

	[Script("Soul Memory Gold Camera Enable", "Scripts\\Camera")]
	[ScriptParam0("Enabled", "Whether the camera should be enabled or disabled", "true")]
	public static void SoulMemoryGoldCameraEnable(bool enabled)
	{
		SoulMemoryCameraEnableHelper(enabled, FullscreenCameraEffectType.Gold, keepMusic: false);
	}

	public static void SoulMemoryCameraEnableHelper(bool enabled, FullscreenCameraEffectType type, bool keepMusic)
	{
		if (enabled)
		{
			MusicManager.Instance.FadeAmbientAudioOut(0.3f);
			if (!keepMusic)
			{
				MusicManager.Instance.FadeOutAreaMusic(resetWhenFaded: false);
			}
			if (s_SoulMemoryCameras == null)
			{
				s_SoulMemoryCameras = new GameObject[2];
			}
			if (s_SoulMemoryCameras[(int)type] == null)
			{
				string assetName = ((type == FullscreenCameraEffectType.Purple || type != FullscreenCameraEffectType.Gold) ? "SoulMemoryCamera" : "SoulMemoryCameraGold");
				GameObject gameObject = GameResources.LoadPrefab<GameObject>(assetName, instantiate: true);
				if ((bool)gameObject)
				{
					Persistence component = gameObject.GetComponent<Persistence>();
					if ((bool)component)
					{
						GameUtilities.Destroy(component);
					}
					gameObject.transform.parent = Camera.main.gameObject.transform;
					s_SoulMemoryCameras[(int)type] = gameObject;
				}
			}
			FullscreenCameraEffect component2 = s_SoulMemoryCameras[(int)type].GetComponent<FullscreenCameraEffect>();
			if ((bool)component2)
			{
				component2.FadeIn();
			}
			return;
		}
		MusicManager.Instance.FadeAmbientAudioIn(0.3f);
		MusicManager.Instance.ResumeScriptedOrNormalMusic(resumeActiveSource: true);
		if (s_SoulMemoryCameras != null && s_SoulMemoryCameras[(int)type] != null)
		{
			FullscreenCameraEffect component3 = s_SoulMemoryCameras[(int)type].GetComponent<FullscreenCameraEffect>();
			if ((bool)component3)
			{
				component3.FadeOut();
			}
		}
	}

	[Script("Disable Fog Of War", "Scripts\\Fog of War")]
	public static void DisableFogOfWar()
	{
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueDisable();
		}
	}

	[Script("Fog Of War Reveal All", "Scripts\\Fog of War")]
	public static void RevealAllFogOfWar()
	{
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueRevealAll();
		}
	}

	[Script("Point Of No Return Save", "Scripts\\General")]
	public static void PointOfNoReturnSave()
	{
		if (!GameState.Mode.TrialOfIron)
		{
			GameResources.SaveGame(SaveGameInfo.GetPointOfNoReturnSaveFileName());
		}
	}

	[Script("Game Complete Save", "Scripts\\General")]
	public static void GameCompleteSave()
	{
		try
		{
			GameState.GameComplete = true;
			GameResources.SaveGame(SaveGameInfo.GetGameCompleteSaveFileName());
		}
		finally
		{
			GameState.GameComplete = false;
		}
	}

	private static IEnumerator ProneRoutine(float duration)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				component.ApplyAffliction(AfflictionData.Prone, component.gameObject, GenericAbility.AbilityType.Undefined, null, deleteOnClear: true, duration, null);
			}
		}
		yield return null;
		foreach (PartyMemberAI onlyPrimaryPartyMember2 in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			GameUtilities.FastForwardAnimator(onlyPrimaryPartyMember2.GetComponent<Animator>(), 5);
		}
	}

	[Script("Prone Party", "Scripts\\General")]
	[ScriptParam0("Duration", "How Long to Leave the Party Prone", "3")]
	public static void ProneParty(float duration)
	{
		GameUtilities.Instance.StartCoroutine(ProneRoutine(duration));
	}

	[Script("Specify Character", "Scripts\\General")]
	[ScriptParam0("Character GUID", "The character to store.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Index", "The index to store the character in.", "0")]
	public static void SpecifyCharacter(Guid guid, int index)
	{
		if (index >= 0 && index < SpecialCharacterInstanceID.s_specifiedGuids.Length)
		{
			SpecialCharacterInstanceID.Add(guid, SpecialCharacterInstanceID.s_specifiedGuids[index]);
			return;
		}
		Debug.LogError("Invalid specified index '" + index + "'. Must be within [0," + SpecialCharacterInstanceID.s_specifiedGuids.Length + ").");
	}

	[Script("Show Message Box", "Scripts\\General")]
	[ScriptParam0("String Table", "The string table containing the message.", "Gui")]
	[ScriptParam1("String Id", "The string ID of the message.", "0")]
	public static void ShowMessageBox(DatabaseString.StringTableType table, int id)
	{
		UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", StringTableManager.GetText(table, id));
	}

	[Script("Set Global", "Scripts\\Globals")]
	[ScriptParam0("Name", "Global Name", "GlobalName", BrowserType.GlobalVariable)]
	[ScriptParam1("Value", "Global Value", "0")]
	public static void SetGlobalValue(string name, int globalValue)
	{
		GlobalVariables.Instance.SetVariable(name, globalValue);
	}

	[Script("Increment Global", "Scripts\\Globals")]
	[ScriptParam0("Name", "Global Name", "GlobalName", BrowserType.GlobalVariable)]
	[ScriptParam1("Value", "Increase Global by Value", "1")]
	public static void IncrementGlobalValue(string name, int globalValue)
	{
		int variable = GlobalVariables.Instance.GetVariable(name);
		GlobalVariables.Instance.SetVariable(name, variable + globalValue);
	}

	[Script("Randomize Global", "Scripts\\Globals")]
	[ScriptParam0("Name", "Global Name", "GlobalName", BrowserType.GlobalVariable)]
	[ScriptParam1("MinValue", "Minimum Value for Global", "1")]
	[ScriptParam2("MaxValue", "Maximum Value for Global", "2")]
	public static void RandomizeGlobalValue(string name, int minValue, int maxValue)
	{
		int val = OEIRandom.Range(minValue, maxValue);
		GlobalVariables.Instance.SetVariable(name, val);
	}

	[Script("Randomize Global With Global", "Scripts\\Globals")]
	[ScriptParam0("Name", "Global Name", "GlobalName", BrowserType.GlobalVariable)]
	[ScriptParam1("MinValueGlobal", "Minimum Value for Global", "", BrowserType.GlobalVariable)]
	[ScriptParam2("MaxValueGlobal", "Maximum Value for Global", "", BrowserType.GlobalVariable)]
	public static void RandomizeGlobalValueWithGlobal(string name, string minValue, string maxValue)
	{
		int val = OEIRandom.Range(GlobalVariables.Instance.GetVariable(minValue), GlobalVariables.Instance.GetVariable(maxValue));
		GlobalVariables.Instance.SetVariable(name, val);
	}

	[Script("Get Global Value", "Scripts\\Globals")]
	[ScriptParam0("Name", "Global Name", "GlobalName", BrowserType.GlobalVariable)]
	public static void GetGlobalValue(string name)
	{
		Console.AddMessage(GlobalVariables.Instance.GetVariable(name).ToString());
	}

	public static T GetComponentByGuid<T>(Guid objectGuid) where T : Component
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			T component = objectByID.GetComponent<T>();
			if ((bool)(UnityEngine.Object)component)
			{
				return component;
			}
			Debug.LogWarning(objectByID.name + " doesn't have the component '" + typeof(T).Name + "'.", objectByID);
			return null;
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found."), null);
		return null;
	}

	public static T[] GetComponentsByGuid<T>(Guid objectGuid) where T : Component
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			return objectByID.GetComponents<T>();
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found."), null);
		return null;
	}

	public static Health GetHealthComponent(Guid objectGuid)
	{
		return GetComponentByGuid<Health>(objectGuid);
	}

	public static CharacterStats GetCharacterStatsComponent(Guid objectGuid)
	{
		return GetComponentByGuid<CharacterStats>(objectGuid);
	}

	[Script("Set Health", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Health Value", "Set current health to this value", "100.0")]
	public static void SetHealth(Guid objectGuid, float healthValue)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			if (healthValue > healthComponent.MaxHealth)
			{
				healthValue = healthComponent.MaxHealth;
			}
			healthComponent.AddHealth(healthValue - healthComponent.CurrentHealth);
		}
	}

	[Script("Set Stamina", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Stamina Value", "Set current stamina to this value", "100.0")]
	public static void SetStamina(Guid objectGuid, float staminaValue)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			if (staminaValue > healthComponent.MaxStamina)
			{
				staminaValue = healthComponent.MaxStamina;
			}
			healthComponent.AddStamina(staminaValue - healthComponent.CurrentStamina);
		}
	}

	[Script("Set Fatigue [OBSOLETE]", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Fatigue Value", "Set current fatigue time to this value (in hours)", "24.0")]
	[Obsolete("Fatigue no longer uses a time-based system.")]
	public static void SetFatigue(Guid objectGuid, float fatigueValue)
	{
		UIDebug.Instance.LogOnScreenWarning("Call made to obsolete script 'SetFatigue'.", UIDebug.Department.Design, 10f);
	}

	[Script("Add Fatigue [OBSOLETE]", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	[Obsolete("Fatigue no longer uses a time-based system.")]
	public static void AddFatigue(Guid objectGuid, float fatigueValue)
	{
		UIDebug.Instance.LogOnScreenWarning("Call made to obsolete script 'AddFatigue'.", UIDebug.Department.Design, 10f);
	}

	[Script("Add Fatigue To Party [OBSOLETE]", "Scripts\\Health")]
	[ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	[Obsolete("Fatigue no longer uses a time-based system.")]
	public static void AddFatigueToParty(float fatigueValue)
	{
		UIDebug.Instance.LogOnScreenWarning("Call made to obsolete script 'AddFatigueToParty'.", UIDebug.Department.Design, 10f);
	}

	[Script("Add Fatigue To Slot [OBSOLETE]", "Scripts\\Health")]
	[ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	[ScriptParam1("Slot #", "Party member slot # to affect", "")]
	[Obsolete("Fatigue no longer uses a time-based system.")]
	public static void AddFatigueToSlot(float fatigueValue, int slot)
	{
		UIDebug.Instance.LogOnScreenWarning("Call made to obsolete script 'AddFatigueToSlot'.", UIDebug.Department.Design, 10f);
	}

	[Script("Add Fatigue To Party With Skill Check [OBSOLETE]", "Scripts\\Health")]
	[ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	[Obsolete("Fatigue no longer uses a time-based system.")]
	public static void AddFatigueToPartyWithSkillCheck(float fatigueValue, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		UIDebug.Instance.LogOnScreenWarning("Call made to obsolete script 'AddFatigueToPartyWithSkillCheck'.", UIDebug.Department.Design, 10f);
	}

	[Script("Set Fatigue Level", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Fatigue Level", "Set current fatigue level (0, 1, 2, or 3)", "1")]
	public static void SetFatigue(Guid objectGuid, CharacterStats.FatigueLevel fatigueLevel)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if ((bool)characterStatsComponent)
		{
			characterStatsComponent.CurrentFatigueLevel = fatigueLevel;
		}
	}

	[Script("Increment Fatigue Level", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Increment", "Number of fatigue levels to add or subtract", "1")]
	public static void AdjustFatigueLevel(Guid objectGuid, int increment)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if ((bool)characterStatsComponent)
		{
			characterStatsComponent.AdjustFatigueLevel(increment);
		}
	}

	[Script("Adjust Party Fatigue Level With Skill Check", "Scripts\\Health")]
	[ScriptParam0("Increment", "Number of fatigue levels to add or subtract", "1")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void AdjustPartyFatigueLevelWithSkillCheck(int increment, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if ((bool)component && Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
				{
					component.AdjustFatigueLevel(increment);
				}
			}
		}
	}

	[Script("Heal Party", "Scripts\\Health")]
	public static void HealParty()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null)
			{
				continue;
			}
			Health component = partyMemberAI.GetComponent<Health>();
			if ((bool)component)
			{
				if (component.Unconscious)
				{
					component.OnRevive();
				}
				component.AddHealth(component.MaxHealth - component.CurrentHealth);
				component.AddStamina(component.MaxStamina - component.CurrentStamina);
			}
		}
	}

	[Script("Set Invunerable", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Invunerable", "Sets the venerability of the object", "true")]
	public static void SetInvunerable(Guid objectGuid, bool invunerable)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			healthComponent.TakesDamage = invunerable;
		}
	}

	[Script("Set Prevent Death", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Prevent Death", "Sets the death prevention of the object", "true")]
	public static void SetPreventDeath(Guid objectGuid, bool preventDeath)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if ((bool)characterStatsComponent)
		{
			if (preventDeath)
			{
				characterStatsComponent.DeathPrevented++;
			}
			else
			{
				characterStatsComponent.DeathPrevented--;
			}
		}
	}

	[Script("Kill", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to kill.", "", BrowserType.ObjectGuid)]
	public static void Kill(Guid objectGuid)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if ((bool)healthComponent)
		{
			if ((bool)characterStatsComponent)
			{
				characterStatsComponent.ApplyAffliction(AfflictionData.Maimed);
			}
			healthComponent.CanBeTargeted = true;
			healthComponent.ShouldDecay = true;
			healthComponent.ApplyHealthChangeDirectly((0f - healthComponent.CurrentHealth) * 10f, applyIfDead: false);
			healthComponent.ApplyDamageDirectly(healthComponent.MaxStamina * 100f);
		}
	}

	[Script("Deal Damage", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Damage", "Deals this amount of damage to the object (armor/defense is applied to calculation)", "10.0")]
	public static void DealDamage(Guid objectGuid, float damage)
	{
		Health healthComponent = GetHealthComponent(objectGuid);
		if ((bool)healthComponent)
		{
			bool flag = !healthComponent.CanBeTargeted;
			healthComponent.CanBeTargeted = true;
			healthComponent.ApplyDamageDirectly(damage);
			if (flag)
			{
				healthComponent.CanBeTargeted = false;
			}
		}
	}

	[Script("Apply Affliction", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Affliction", "Applies this affliction to the object", "")]
	public static void ApplyAffliction(Guid objectGuid, string affliction)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent != null)
		{
			Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
			if (affliction2 != null)
			{
				characterStatsComponent.ApplyAffliction(affliction2);
			}
			else
			{
				UIDebug.Instance.LogOnScreenWarning("Couldn't find affliction with tag '" + affliction + "' for script. Make sure it is in InGameGlobal/AfflictionData/AfflictionsForScripts.", UIDebug.Department.Design, 10f);
			}
		}
	}

	[Script("Apply Affliciton To Party Member Slot #", "Scripts\\Health")]
	[ScriptParam0("Affliction", "Applies this affliction to the object", "")]
	[ScriptParam1("Slot #", "Party member slot # to apply affliction to", "")]
	public static void ApplyAfflictionToPartyMember(string affliction, int index)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			Debug.LogWarning(affliction + " could not be found.", null);
		}
		else if (index < PartyMemberAI.PartyMembers.Length)
		{
			CharacterStats component = PartyMemberAI.PartyMembers[index].GetComponent<CharacterStats>();
			if (component == null)
			{
				Debug.LogWarning("Party member index could not be found");
			}
			else
			{
				component.ApplyAffliction(affliction2);
			}
		}
	}

	[Script("Remove Affliction", "Scripts\\Health")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Affliction", "Removes this affliction from the object", "")]
	public static void RemoveAffliction(Guid objectGuid, string affliction)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent != null)
		{
			Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
			if (affliction2 != null)
			{
				characterStatsComponent.ClearEffectFromAffliction(affliction2);
			}
		}
	}

	[Script("Apply Affliction To Party With Skill Check", "Scripts\\Health")]
	[ScriptParam0("Affliction", "Applies this affliction to the object", "")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void ApplyAfflictionToPartyWithSkillCheck(string affliction, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			Debug.LogWarning(affliction + " could not be found.", null);
			return;
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null) && Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
				{
					component.ApplyAffliction(affliction2);
				}
			}
		}
	}

	[Script("Apply Affliction To Party With Skill Check (Scaled)", "Scripts\\Health")]
	[ScriptParam0("Affliction", "Applies this affliction to the object", "")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	[ScriptParam4("Scaler", "Scaler to use, if the player has enabled it.", "0")]
	public static void ApplyAfflictionToPartyWithSkillCheckScaled(string affliction, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue, DifficultyScaling.Scaler scaler)
	{
		ApplyAfflictionToPartyWithSkillCheck(affliction, skillType, comparisonOperator, Mathf.CeilToInt((float)skillValue * DifficultyScaling.Instance.GetScaleMultiplicative(scaler, (DifficultyScaling.ScaleData sd) => sd.SkillCheckMult)));
	}

	[Script("Apply Affliction To Worst Party Member", "Scripts\\Health")]
	[ScriptParam0("Affliction", "Applies this affliction to the object", "")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	public static void ApplyAfflictionToWorstPartyMember(string affliction, CharacterStats.SkillType skillType)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			Debug.LogError("Script Error: Affliction '" + affliction + "' could not be found.", null);
			return;
		}
		int num = int.MaxValue;
		CharacterStats characterStats = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null || partyMemberAI.Secondary)
			{
				continue;
			}
			CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
			if (!(component == null))
			{
				int num2 = component.CalculateSkill(skillType);
				if (num2 < num)
				{
					characterStats = component;
					num = num2;
				}
			}
		}
		if (characterStats != null)
		{
			characterStats.ApplyAffliction(affliction2);
		}
	}

	[Script("Set Affliction To Best Party Member", "Scripts\\Health")]
	[ScriptParam0("Affliction", "Applies this affliction to the object", "")]
	[ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	public static void ApplyAfflictionToBestPartyMember(string affliction, CharacterStats.SkillType skillType)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			Debug.LogError("Script Error: Affliction '" + affliction + "' could not be found.", null);
			return;
		}
		int num = int.MinValue;
		CharacterStats characterStats = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null || partyMemberAI.Secondary)
			{
				continue;
			}
			CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
			if (!(component == null))
			{
				int num2 = component.CalculateSkill(skillType);
				if (num2 > num)
				{
					characterStats = component;
					num = num2;
				}
			}
		}
		if (characterStats != null)
		{
			characterStats.ApplyAffliction(affliction2);
		}
	}

	[Script("Set Skill Check Token", "Scripts\\Health")]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam1("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void SetSkillCheckToken(CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		int i = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null || partyMemberAI.Secondary)
			{
				continue;
			}
			CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
			if (!(component == null) && Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
			{
				InstanceID.AddSpecialObjectID(partyMemberAI.gameObject, SpecialCharacterInstanceID.GetSkillCheckGuid(i));
				i++;
				if (i >= SpecialCharacterInstanceID.s_skillCheckGuids.Length)
				{
					return;
				}
			}
		}
		for (; i < SpecialCharacterInstanceID.s_skillCheckGuids.Length; i++)
		{
			InstanceID.RemoveSpecialObjectID(SpecialCharacterInstanceID.GetSkillCheckGuid(i));
		}
	}

	[Script("Set Skill Check Token (Scaled)", "Scripts\\Health")]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	[ScriptParam1("Skill Operator", "Comparison operator.", "EqualTo")]
	[ScriptParam2("Skill Value", "Compare the object's skill against this value.", "0")]
	[ScriptParam3("Scaler", "Scaler to use, if the player has enabled it.", "0")]
	public static void SetSkillCheckTokenScaled(CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue, DifficultyScaling.Scaler scaler)
	{
		SetSkillCheckToken(skillType, comparisonOperator, Mathf.CeilToInt((float)skillValue * DifficultyScaling.Instance.GetScaleMultiplicative(scaler, (DifficultyScaling.ScaleData sd) => sd.SkillCheckMult)));
	}

	[Script("Set Skill Check Token With Worst Check", "Scripts\\Health")]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	public static void SetSkillCheckTokenWorst(CharacterStats.SkillType skillType)
	{
		for (int i = 0; i < SpecialCharacterInstanceID.s_skillCheckGuids.Length; i++)
		{
			InstanceID.RemoveSpecialObjectID(SpecialCharacterInstanceID.GetSkillCheckGuid(i));
		}
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if ((bool)partyMemberAI && !partyMemberAI.Secondary)
			{
				list.Add(partyMemberAI);
			}
		}
		list.StableSort(delegate(PartyMemberAI a, PartyMemberAI b)
		{
			CharacterStats component = a.GetComponent<CharacterStats>();
			CharacterStats component2 = b.GetComponent<CharacterStats>();
			return component.CalculateSkill(skillType).CompareTo(component2.CalculateSkill(skillType));
		});
		for (int k = 0; k < Mathf.Min(list.Count, SpecialCharacterInstanceID.s_skillCheckGuids.Length); k++)
		{
			InstanceID.AddSpecialObjectID(list[k].gameObject, SpecialCharacterInstanceID.GetSkillCheckGuid(k));
		}
	}

	[Script("Set Skill Check Token With Best Check", "Scripts\\Health")]
	[ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	public static void SetSkillCheckTokenBest(CharacterStats.SkillType skillType)
	{
		for (int i = 0; i < SpecialCharacterInstanceID.s_skillCheckGuids.Length; i++)
		{
			InstanceID.RemoveSpecialObjectID(SpecialCharacterInstanceID.GetSkillCheckGuid(i));
		}
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if ((bool)partyMemberAI && !partyMemberAI.Secondary)
			{
				list.Add(partyMemberAI);
			}
		}
		list.StableSort(delegate(PartyMemberAI a, PartyMemberAI b)
		{
			CharacterStats component = a.GetComponent<CharacterStats>();
			CharacterStats component2 = b.GetComponent<CharacterStats>();
			return -component.CalculateSkill(skillType).CompareTo(component2.CalculateSkill(skillType));
		});
		for (int k = 0; k < Mathf.Min(list.Count, SpecialCharacterInstanceID.s_skillCheckGuids.Length); k++)
		{
			InstanceID.AddSpecialObjectID(list[k].gameObject, SpecialCharacterInstanceID.GetSkillCheckGuid(k));
		}
	}

	[Script("Remove Item", "Scripts\\Items")]
	[ScriptParam0("Item Name", "The name of the item to remove", "ItemName")]
	public static void RemoveItem(string itemName)
	{
		Item item = GameResources.LoadPrefab<Item>(itemName, instantiate: false);
		if (PartyHelper.PartyDestroyItem(item, 1) == 0)
		{
			LogItemRemove(item.Name, 1);
		}
		else
		{
			Debug.LogWarning("Remove Item script call: tried to remove '" + itemName + "' from party but couldn't find it.");
		}
	}

	[Script("Remove Item Including Equipped", "Scripts\\Items")]
	[ScriptParam0("Item Name", "The name of the item to remove", "ItemName")]
	public static void RemoveItemIncludingEquipped(string itemName)
	{
		Item item = GameResources.LoadPrefab<Item>(itemName, instantiate: false);
		int num = PartyHelper.PartyDestroyItem(item, 1);
		if (num == 0)
		{
			LogItemRemove(item.Name, 1);
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			Equipment component = ComponentUtils.GetComponent<Equipment>(PartyMemberAI.PartyMembers[i]);
			if (!component)
			{
				continue;
			}
			Equippable.EquipmentSlot equipmentSlot = component.CurrentItems.FindSlot(itemName);
			if (Equippable.EquipmentSlot.None != equipmentSlot)
			{
				RemoveItemInSlotHelper(component, equipmentSlot);
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		if (num != 0)
		{
			Debug.LogWarning("Remove Item script call: tried to remove '" + itemName + "' from party but couldn't find it.");
		}
	}

	[Script("Remove Item from NPC", "Scripts\\Items")]
	[ScriptParam0("NPC", "Remove item from this NPC.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "The name of the item to remove", "ItemName")]
	public static void RemoveItemFromNPC(Guid objectGuid, string itemName)
	{
		Equipment componentByGuid = GetComponentByGuid<Equipment>(objectGuid);
		if ((bool)componentByGuid)
		{
			Equippable.EquipmentSlot equipmentSlot = componentByGuid.CurrentItems.FindSlot(itemName);
			if (Equippable.EquipmentSlot.None != equipmentSlot)
			{
				RemoveItemInSlot(objectGuid, equipmentSlot);
			}
		}
		Inventory componentByGuid2 = GetComponentByGuid<Inventory>(objectGuid);
		if ((bool)componentByGuid2 && componentByGuid2.DestroyItem(itemName, 1) > 0)
		{
			Debug.LogWarning("Remove Item from NPC script call: tried to remove " + itemName + " from " + componentByGuid2.name + " but couldn't find it.");
		}
	}

	[Script("Give Item By Name", "Scripts\\Items")]
	[ScriptParam0("Item Name", "The name of the item to give", "ItemName")]
	[ScriptParam1("Count", "The number of items to add", "1")]
	public static void GiveItem(string itemName, int count)
	{
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if ((bool)inventory)
		{
			Item item = GameResources.LoadPrefab<Item>(itemName, instantiate: false);
			if ((bool)item)
			{
				inventory.AddItemAndLog(item, count, null);
				return;
			}
			Debug.LogError("Give Item By Name script call: Tried to give " + count + " of item '" + itemName + "' but couldn't find that item.");
		}
		else
		{
			Debug.LogError("Give Item By Name script call: Tried to give " + count + " of item " + itemName + " but couldn't find player inventory.");
		}
	}

	[Script("Give Item And Equip", "Scripts\\Items")]
	[ScriptParam0("Character", "Character to give the item to.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "The name of the item to give", "ItemName")]
	[ScriptParam2("Primary", "Equip the item in the primary slot (left hand).", "false")]
	public static void GiveItemAndEquip(Guid characterGuid, string itemName, bool primary)
	{
		Equipment componentByGuid = GetComponentByGuid<Equipment>(characterGuid);
		if (!componentByGuid)
		{
			return;
		}
		Equippable equippable = GameResources.LoadPrefab<Equippable>(itemName, instantiate: false);
		if ((bool)equippable)
		{
			Equippable equippable2 = GameResources.Instantiate<Equippable>(equippable);
			equippable2.Prefab = equippable;
			Equippable.EquipmentSlot desiredSlot = equippable2.GetPreferredSlot();
			if (equippable2.PrimaryWeaponSlot && equippable2.SecondaryWeaponSlot && !equippable2.BothPrimaryAndSecondarySlot)
			{
				desiredSlot = (primary ? Equippable.EquipmentSlot.PrimaryWeapon : Equippable.EquipmentSlot.SecondaryWeapon);
			}
			if (equippable2.BothPrimaryAndSecondarySlot)
			{
				Equippable equippable3 = componentByGuid.UnEquip(Equippable.EquipmentSlot.SecondaryWeapon);
				if ((bool)equippable3 && !PartyHelper.PutItem(equippable3, componentByGuid.gameObject))
				{
					Debug.LogError("GiveItemAndEquip (" + itemName + "): lost already-equipped secondary item.");
				}
			}
			Equippable equippable4 = componentByGuid.Equip(equippable2, desiredSlot, enforceRecoveryPenalty: false);
			if ((bool)equippable4 && !PartyHelper.PutItem(equippable4, componentByGuid.gameObject))
			{
				Debug.LogError("GiveItemAndEquip (" + itemName + "): lost already-equipped item.");
			}
			NPCAppearance component = componentByGuid.gameObject.GetComponent<NPCAppearance>();
			if ((bool)component)
			{
				component.Generate();
			}
		}
		else
		{
			Debug.LogError("GiveItemAndEquip (" + itemName + "): item is missing or wasn't Equippable.");
		}
	}

	[Script("Bind Item In Slot", "Scripts\\Items")]
	[ScriptParam0("Character", "Character to bind the item to.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to bind", "None")]
	public static void BindItemInSlot(Guid characterGuid, Equippable.EquipmentSlot slot)
	{
		EquipmentSoulbind soulbindInSlot = GetSoulbindInSlot(characterGuid, slot);
		if ((bool)soulbindInSlot)
		{
			soulbindInSlot.Bind(InstanceID.GetObjectByID(characterGuid));
		}
	}

	[Script("Unbind Item In Slot", "Scripts\\Items")]
	[ScriptParam0("Character", "Character to unbind the item on.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to unbind", "None")]
	public static void UnbindItemInSlot(Guid characterGuid, Equippable.EquipmentSlot slot)
	{
		EquipmentSoulbind soulbindInSlot = GetSoulbindInSlot(characterGuid, slot);
		if ((bool)soulbindInSlot)
		{
			soulbindInSlot.Unbind();
		}
	}

	private static EquipmentSoulbind GetSoulbindInSlot(Guid characterGuid, Equippable.EquipmentSlot slot)
	{
		Equipment componentByGuid = GetComponentByGuid<Equipment>(characterGuid);
		if ((bool)componentByGuid)
		{
			Equippable itemInSlot = componentByGuid.CurrentItems.GetItemInSlot(slot);
			if (!itemInSlot)
			{
				return null;
			}
			return itemInSlot.GetComponent<EquipmentSoulbind>();
		}
		return null;
	}

	[Script("Give Item to NPC", "Scripts\\Items")]
	[ScriptParam0("NPC", "NPC to give the item to.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "The name of the item to give", "ItemName")]
	[ScriptParam2("Count", "The number of items to add", "1")]
	public static void GiveItemToNPC(Guid objectGuid, string itemName, int count)
	{
		Inventory componentByGuid = GetComponentByGuid<Inventory>(objectGuid);
		if ((bool)componentByGuid)
		{
			Item item = GameResources.LoadPrefab<Item>(itemName, instantiate: false);
			if ((bool)item)
			{
				componentByGuid.AddItem(item, count);
				return;
			}
			Debug.LogError("GiveItemToNPC script call: Tried to give " + count + " of item '" + itemName + "' but couldn't find that item.");
		}
	}

	[Script("Remove Item Stack", "Scripts\\Items")]
	[ScriptParam0("Item Name", "The name of the item to remove", "ItemName")]
	[ScriptParam1("Count", "The number of items to remove", "1")]
	public static void RemoveItemStack(string itemName, int count)
	{
		string name = GameResources.LoadPrefab<Item>(itemName, instantiate: false).Name;
		int num = count;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			Inventory inventory = onlyPrimaryPartyMember.Inventory;
			if (inventory != null && (num = inventory.DestroyItem(itemName, num)) == 0)
			{
				LogItemRemove(name, count);
				return;
			}
		}
		Debug.LogWarning("Remove Item Stack script call: tried to remove " + count + " of " + itemName + " but only found " + (count - num) + " of those.");
		LogItemRemove(name, count - num);
	}

	[Script("Remove Item Stack From NPC", "Scripts\\Items")]
	[ScriptParam0("NPC", "NPC to lose item.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "The name of the item to remove", "ItemName")]
	[ScriptParam2("Count", "The number of items to remove", "1")]
	public static void RemoveItemStackFromNPC(Guid objectGuid, string itemName, int count)
	{
		Inventory componentByGuid = GetComponentByGuid<Inventory>(objectGuid);
		if ((bool)componentByGuid)
		{
			Item item = GameResources.LoadPrefab<Item>(itemName, instantiate: false);
			int num = count;
			if (!(item != null) || (num = componentByGuid.DestroyItem(itemName, num)) != 0)
			{
				Debug.LogWarning("Remove Item Stack script call: tried to remove " + count + " of " + itemName + " from " + componentByGuid.name + " but only found " + (count - num) + ".");
			}
		}
	}

	[Script("Unequip Item in Slot", "Scripts\\Items")]
	[ScriptParam0("Character", "Character to unequip.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to unequip", "None")]
	public static void UnequipItemInSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		Equippable itemInSlot = component.CurrentItems.GetItemInSlot(slot);
		if (!(itemInSlot == null))
		{
			itemInSlot = component.UnEquip(itemInSlot);
			if ((bool)itemInSlot && !PartyHelper.PutItem(new InventoryItem(itemInSlot), objectByID))
			{
				Debug.LogError(string.Concat("No room in '", objectByID.name, "' inventory for item unequipped by script from '", slot, "' on '", objectByID.name, "'."));
			}
			NPCAppearance component2 = objectByID.GetComponent<NPCAppearance>();
			if ((bool)component2)
			{
				component2.Generate();
			}
		}
	}

	[Script("Give Player Money", "Scripts\\Items")]
	[ScriptParam0("Amount", "The amount of money to add to the player", "1")]
	public static void GivePlayerMoney(int amount)
	{
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if ((bool)inventory)
		{
			inventory.currencyTotalValue.v += amount;
			Console.AddMessage(GUIUtils.FormatWithLinks(295, amount), ConsoleNotifyColor, Console.ConsoleState.Both);
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ReceiveGold);
			}
		}
	}

	[Script("Remove Player Money", "Scripts\\Items")]
	[ScriptParam0("Amount", "The amount of money to remove from the player", "1")]
	public static void RemovePlayerMoney(int amount)
	{
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if ((bool)inventory)
		{
			if (inventory.currencyTotalValue.v >= (float)amount)
			{
				Console.AddMessage(GUIUtils.FormatWithLinks(296, amount), ConsoleNotifyColor, Console.ConsoleState.Both);
				inventory.currencyTotalValue.v -= amount;
			}
			else
			{
				Console.AddMessage(GUIUtils.FormatWithLinks(296, inventory.currencyTotalValue.v), ConsoleNotifyColor, Console.ConsoleState.Both);
				inventory.currencyTotalValue.v = 0f;
			}
		}
	}

	public static void LogItemGet(Item item, int quantity, bool stashed)
	{
		if (!item.IsQuestItem && !item.IsRedirectIngredient)
		{
			string text = item.Name;
			if (quantity != 1)
			{
				text = GUIUtils.Format(1625, quantity, item.Name);
			}
			text = GUIUtils.Format(stashed ? 1728 : 2171, text);
			Console.AddMessage(GUIUtils.FormatWithLinks(297, text), ConsoleNotifyColor, Console.ConsoleState.Both);
		}
	}

	public static void LogItemRemove(string itemDisplayName, int quantity)
	{
		if (quantity != 1)
		{
			itemDisplayName = GUIUtils.Format(1625, quantity, itemDisplayName);
		}
		Console.AddMessage(GUIUtils.FormatWithLinks(298, itemDisplayName), ConsoleNotifyColor, Console.ConsoleState.Both);
	}

	[Script("Log Given Recipe", "Scripts\\Items")]
	[ScriptParam0("Recipe Name", "The name of the recipe learned", "RecipeName")]
	public static void LogRecipeGet(string recipeName)
	{
		Recipe recipe = GameResources.LoadPrefab<Recipe>(recipeName, instantiate: false);
		if ((bool)recipe)
		{
			Console.AddMessage(GUIUtils.FormatWithLinks(1660, recipe.DisplayName), ConsoleNotifyColor, Console.ConsoleState.Both);
		}
	}

	[Script("Remove Item in Slot", "Scripts\\Items")]
	[ScriptParam0("Character", "Remove the item from this character.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to remove", "None")]
	public static void RemoveItemInSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		RemoveItemInSlotHelper(ComponentUtils.GetComponent<Equipment>(InstanceID.GetObjectByID(objectGuid)), slot);
	}

	private static void RemoveItemInSlotHelper(Equipment eq, Equippable.EquipmentSlot slot)
	{
		if (eq == null)
		{
			return;
		}
		Equippable itemInSlot = eq.CurrentItems.GetItemInSlot(slot);
		if (itemInSlot == null)
		{
			return;
		}
		Equippable equippable = eq.UnEquip(itemInSlot);
		if (!(equippable == null))
		{
			PersistenceManager.RemoveObject(equippable.GetComponent<Persistence>());
			GameUtilities.Destroy(equippable.gameObject);
			NPCAppearance component = eq.GetComponent<NPCAppearance>();
			if ((bool)component)
			{
				component.Generate();
			}
		}
	}

	[Script("Lock Equipment Slot", "Scripts\\Testing")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to lock", "None")]
	public static void LockEquipmentSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			Equipment component = objectByID.GetComponent<Equipment>();
			if ((bool)component)
			{
				component.LockSlot(slot);
			}
		}
	}

	[Script("Unlock Equipment Slot", "Scripts\\Testing")]
	[ScriptParam0("Object", "Object to modify.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Slot", "Equipment Slot to unlock", "None")]
	public static void UnlockEquipmentSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			Equipment component = objectByID.GetComponent<Equipment>();
			if ((bool)component)
			{
				component.UnlockSlot(slot);
			}
		}
	}

	[Script("Transfer Item", "Scripts\\Items")]
	[ScriptParam0("From", "Object to take item from.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("To", "Object to give item to.", "", BrowserType.ObjectGuid)]
	[ScriptParam2("Item Name", "Name of the item.", "")]
	[ScriptParam3("Quantity", "Number of items to transfer.", "1")]
	public static void TransferItem(Guid from, Guid to, string itemName, int quantity)
	{
		GameObject objectByID = InstanceID.GetObjectByID(from);
		Inventory componentByGuid = GetComponentByGuid<Inventory>(to);
		quantity = TranferItemHelper(objectByID, componentByGuid, itemName, quantity);
		if (quantity > 0)
		{
			Debug.LogWarning("Transfer Item (Quantity): didn't find enough of item '" + itemName + "' (missed " + quantity + ").");
		}
	}

	[Script("Transfer Item From Party", "Scripts\\Items")]
	[ScriptParam0("To", "Object to give item to.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Item Name", "Name of the item.", "")]
	[ScriptParam2("Quantity", "Number of items to transfer.", "1")]
	public static void TransferItemFromParty(Guid to, string itemName, int quantity)
	{
		Inventory componentByGuid = GetComponentByGuid<Inventory>(to);
		for (int i = 0; i < 6; i++)
		{
			if (quantity <= 0)
			{
				break;
			}
			if ((bool)PartyMemberAI.PartyMembers[i])
			{
				quantity = TranferItemHelper(PartyMemberAI.PartyMembers[i].gameObject, componentByGuid, itemName, quantity);
			}
		}
		if (quantity > 0)
		{
			Debug.LogWarning("Transfer Item (Quantity): didn't find enough of item '" + itemName + "' (missed " + quantity + ").");
		}
	}

	private static int TranferItemHelper(GameObject from, BaseInventory toInventory, string itemName, int quantity)
	{
		itemName = GameResources.LoadPrefab<Item>(itemName, instantiate: false).name;
		BaseInventory[] components = from.GetComponents<BaseInventory>();
		if (!toInventory)
		{
			Debug.LogError("Transfer Item (Quantity): no inventory found to transfer into.");
		}
		else if (components == null || components.Length == 0)
		{
			Debug.LogError("Transfer Item (Quantity): no inventory found to take from.");
		}
		else
		{
			List<InventoryItem> list = new List<InventoryItem>();
			for (int i = 0; i < components.Length; i++)
			{
				if (quantity <= 0)
				{
					break;
				}
				BaseInventory baseInventory = components[i];
				list.Clear();
				for (int j = 0; j < baseInventory.ItemList.Count; j++)
				{
					if (quantity <= 0)
					{
						break;
					}
					InventoryItem inventoryItem = baseInventory.ItemList[j];
					if (inventoryItem.baseItem.Prefab.name == itemName)
					{
						if (inventoryItem.stackSize > quantity)
						{
							toInventory.AddItem(inventoryItem.baseItem, quantity);
							baseInventory.RemoveItem(inventoryItem.baseItem, quantity);
							quantity = 0;
						}
						else
						{
							quantity -= inventoryItem.stackSize;
							list.Add(inventoryItem);
						}
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					toInventory.PutItem(baseInventory.TakeItem(list[k]));
				}
			}
		}
		return quantity;
	}

	[Script("Dozens Game Roll Player", "Scripts\\Minigame")]
	public static void DozensGameRollPlayer()
	{
		Dozens.DoRoll(Contestant.PLAYER);
	}

	[Script("Dozens Game Roll Opponent", "Scripts\\Minigame")]
	public static void DozensGameRollOpponent()
	{
		Dozens.DoRoll(Contestant.OPPONENT);
	}

	[Script("Orlan Game Roll Player", "Scripts\\Minigame")]
	[ScriptParam0("Object", "Character to make the throw.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Approach", "Controls which set of probabilities is used.", "NOSE")]
	public static void OrlanGameRollPlayer(Guid character, OrlansHead.Approach approach)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(character);
		OrlansHead.DoRoll(Contestant.PLAYER, characterStatsComponent, approach);
	}

	[Script("Orlan Game Roll Opponent", "Scripts\\Minigame")]
	[ScriptParam0("Object", "Character to make the throw.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Approach", "Controls which set of probabilities is used.", "NOSE")]
	public static void OrlanGameRollOpponent(Guid character, OrlansHead.Approach approach)
	{
		CharacterStats characterStatsComponent = GetCharacterStatsComponent(character);
		OrlansHead.DoRoll(Contestant.OPPONENT, characterStatsComponent, approach);
	}

	[Script("Orlan Game Round Over", "Scripts\\Minigame")]
	public static void OrlanGameReset()
	{
		OrlansHead.RoundCount++;
	}

	[Script("Orlan Game Reset", "Scripts\\Minigame")]
	public static void OrlanGameRoundOver()
	{
		OrlansHead.Reset();
	}

	public static OCL GetOCLComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			OCL component = objectByID.GetComponent<OCL>();
			if ((bool)component)
			{
				return component;
			}
			Debug.LogWarning(string.Concat(objectGuid, " doesn't have a ocl component."), objectByID);
		}
		Debug.LogWarning(string.Concat(objectGuid, " could not be found when searching for ocl component."), null);
		return null;
	}

	[Script("Open", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to open.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void Open(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Open(null, ignoreLock);
		}
	}

	[Script("Seal Open", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to open.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void SealOpen(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Open(null, ignoreLock);
			oCLComponent.SealOpen();
		}
	}

	[Script("Close", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to close.", "", BrowserType.ObjectGuid)]
	public static void Close(Guid objectGuid)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Close(null);
		}
	}

	[Script("Lock", "Scripts\\OCL")]
	[ScriptParam0("Object", "Name of the object", "", BrowserType.ObjectGuid)]
	public static void Lock(Guid objectGuid)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Lock(null);
		}
	}

	[Script("UnLock", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to unlock.", "", BrowserType.ObjectGuid)]
	public static void Unlock(Guid objectGuid)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Unlock(null);
		}
	}

	[Script("Toggle Open", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to toggle.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void ToggleOpen(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Toggle(null, ignoreLock);
		}
	}

	[Script("Toggle Lock", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to toggle.", "", BrowserType.ObjectGuid)]
	public static void ToggleLock(Guid objectGuid)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.ToggleLock(null);
		}
	}

	[Script("Unseal", "Scripts\\OCL")]
	[ScriptParam0("Object", "Object to unseal (Sealed -> Closed or Sealed Open -> Open).", "", BrowserType.ObjectGuid)]
	public static void Unseal(Guid objectGuid)
	{
		OCL oCLComponent = GetOCLComponent(objectGuid);
		if ((bool)oCLComponent)
		{
			oCLComponent.Unseal();
		}
	}

	[Script("Start Quest", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	public static void StartQuest(string questName)
	{
		QuestManager.Instance.StartQuest(questName, null);
	}

	[Script("Start Quest With Alternate Description", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	[ScriptParam1("Alternate Description ID", "The ID of the alternate description to display.", "0")]
	public static void StartQuestWithAlternateDescription(string questName, int questDiscriptionID)
	{
		QuestManager.Instance.StartQuest(questName, questDiscriptionID, null);
	}

	[Script("Set Quest Alternate Description", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest to update.", "", BrowserType.Quest)]
	[ScriptParam1("Alternate Description ID", "The ID of the alternate description to display.", "0")]
	public static void SetQuestAlternateDescription(string questName, int questDiscriptionID)
	{
		QuestManager.Instance.SetQuestAlternateDescription(questName, questDiscriptionID);
	}

	[Script("Advance Quest", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	public static void AdvanceQuest(string questName)
	{
		QuestManager.Instance.AdvanceQuest(questName);
	}

	[Script("Debug Advance Quest", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	public static void DebugAdvanceQuest(string questName)
	{
		QuestManager.Instance.AdvanceQuest(questName, force: true);
	}

	[Script("Trigger Quest Addendum", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	[ScriptParam1("Addendum ID", "The ID of the addendum to set.", "0")]
	public static void TriggerQuestAddendum(string questName, int addendumID)
	{
		QuestManager.Instance.TriggerQuestAddendum(questName, addendumID);
	}

	[Script("Trigger Quest End State", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	[ScriptParam1("End State ID", "The ID of the end state to set.", "0")]
	public static void TriggerQuestEndState(string questName, int endStateID)
	{
		QuestManager.Instance.TriggerQuestEndState(questName, endStateID, failed: false);
	}

	[Script("Trigger Quest Fail State", "Scripts\\Quest")]
	[ScriptParam0("Quest Name", "The name of the quest.", "", BrowserType.Quest)]
	[ScriptParam1("End State ID", "The ID of the end state to set.", "0")]
	public static void TriggerQuestFailState(string questName, int endStateID)
	{
		QuestManager.Instance.TriggerQuestEndState(questName, endStateID, failed: true);
	}

	[Script("Add Talent", "Scripts\\RPG")]
	[ScriptParam0("Talent Name", "The name of the talent to add to the player.", "TalentName")]
	public static void AddTalent(string talentName)
	{
		AddTalent(SpecialCharacterInstanceID.PlayerGuid, talentName);
	}

	[Script("Add Talent", "Scripts\\RPG")]
	[ScriptParam0("Target character", "The character desired to add talent/ability", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Talent Name", "The name of the talent to add to the character.", "TalentName")]
	public static void AddTalent(Guid objectGuid, string talentName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		AbilityProgressionTable.AddAbilityToCharacter(talentName, objectByID.GetComponent<CharacterStats>(), causeIsGameplay: true);
	}

	[Script("Remove Talent", "Scripts\\RPG")]
	[ScriptParam0("Target character", "The character desired to remove talent/ability", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Talent Name", "The name of the talent to remove from the character. Will also remove abilities.", "TalentName")]
	public static void RemoveTalent(Guid objectGuid, string talentName)
	{
		CommandLine.RemoveAbility(InstanceID.GetObjectByID(objectGuid), talentName);
	}

	[Script("Set Wants To Talk", "Scripts\\RPG")]
	[ScriptParam0("Target Party Member", "The party member desired to show the flag", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Wants To Talk", "The new state for the conversation flag", "false")]
	public static void SetWantsToTalk(Guid objectGuid, bool wantsToTalk)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			NPCDialogue component = objectByID.GetComponent<NPCDialogue>();
			if ((bool)component)
			{
				component.wantsToTalk = wantsToTalk;
			}
			else
			{
				Debug.LogWarning(string.Concat(objectGuid, " doesn't have an NPCDialogue component."), objectByID);
			}
		}
		else
		{
			Debug.LogWarning(string.Concat(objectGuid, " could not be found!"));
		}
	}

	[Script("Mark Conversation Node As Read", "Scripts\\Conversation")]
	[ScriptParam0("Conversation", "Name of the conversation.", "", BrowserType.Conversation)]
	[ScriptParam1("Conversation Node ID", "Conversation node ID.", "0")]
	public static void MarkConversationNodeAsRead(string conversation, int nodeID)
	{
		ConversationManager.Instance.SetMarkedAsRead(conversation, nodeID);
	}

	[Script("Clear Conversation Node As Read", "Scripts\\Conversation")]
	[ScriptParam0("Conversation", "Name of the conversation.", "", BrowserType.Conversation)]
	[ScriptParam1("Conversation Node ID", "Conversation node ID.", "0")]
	public static void ClearConversationNodeAsRead(string conversation, int nodeID)
	{
		ConversationManager.Instance.ClearMarkedAsRead(conversation, nodeID);
	}

	[Script("Any Party Member Use Ability", "Scripts\\RPG")]
	[ScriptParam0("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static void AnyPartyMemberUseAbility(int nameStringId)
	{
		if (nameStringId < 0)
		{
			return;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
			if ((bool)component && component.UseAbilityForScript(nameStringId))
			{
				break;
			}
		}
	}

	[Script("Character Use Ability", "Scripts\\RPG")]
	[ScriptParam0("Object", "Character to use.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static void CharacterUseAbility(Guid objectGuid, int nameStringId)
	{
		if (nameStringId >= 0)
		{
			CharacterStats characterStatsComponent = GetCharacterStatsComponent(objectGuid);
			if ((bool)characterStatsComponent)
			{
				characterStatsComponent.UseAbilityForScript(nameStringId);
			}
		}
	}

	[ConditionalScript("Slot Use Ability", "Conditionals\\RPG")]
	[ScriptParam0("Slot", "Slot to use", "0")]
	[ScriptParam1("Ability Name String Id", "String ID of the ability's name string.", "-1")]
	public static void SlotCanUseAbility(int slot, int nameStringId)
	{
		if (nameStringId >= 0)
		{
			CharacterStats partyCharacterStats = Conditionals.GetPartyCharacterStats(slot);
			if ((bool)partyCharacterStats)
			{
				partyCharacterStats.UseAbilityForScript(nameStringId);
			}
		}
	}

	public static Vendor GetVendorComponent(Guid storeGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(storeGuid);
		if ((bool)objectByID)
		{
			Vendor component = objectByID.GetComponent<Vendor>();
			if ((bool)component)
			{
				return component;
			}
			Debug.LogWarning(string.Concat(storeGuid, " doesn't have a vendor component."), objectByID);
		}
		Debug.LogWarning(string.Concat(storeGuid, " could not be found when searching for vendor component."), null);
		return null;
	}

	[Script("Open Store", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to open.", "", BrowserType.ObjectGuid)]
	public static void OpenStore(Guid storeGuid)
	{
		Vendor vendorComponent = GetVendorComponent(storeGuid);
		if ((bool)vendorComponent)
		{
			vendorComponent.OpenStore();
		}
	}

	[Script("Open Store with Rates", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to open.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Buy Rate", "Buy Rate (default 0.2)", "0.2")]
	[ScriptParam2("Sell Rate", "Sell Rate (default 1.5)", "1.5")]
	public static void OpenStoreWithRates(Guid storeGuid, float buyRate, float sellRate)
	{
		Vendor vendorComponent = GetVendorComponent(storeGuid);
		if ((bool)vendorComponent)
		{
			vendorComponent.OpenStore(buyRate, sellRate);
		}
	}

	[Script("Open Inn", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to open.", "", BrowserType.ObjectGuid)]
	public static void OpenInn(Guid innGuid)
	{
		Vendor vendorComponent = GetVendorComponent(innGuid);
		if ((bool)vendorComponent)
		{
			vendorComponent.OpenInn();
		}
	}

	[Script("Open Inn", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to open.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Rate", "Rate multiplier (default 1.0)", "1.0")]
	public static void OpenInnWithRate(Guid innGuid, float rate)
	{
		Vendor vendorComponent = GetVendorComponent(innGuid);
		if ((bool)vendorComponent)
		{
			vendorComponent.OpenInn(rate);
		}
	}

	[Script("Set Vendor Rates", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to set.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Buy Rate", "Buy Rate (default 0.2)", "0.2")]
	[ScriptParam2("Sell Rate", "Sell Rate (default 1.5)", "1.5")]
	[ScriptParam3("Inn Rate", "Inn Rate (default 1.0)", "1.0")]
	public static void SetVendorRates(Guid vendorGuid, float buyRate, float sellRate, float innRate)
	{
		Vendor vendorComponent = GetVendorComponent(vendorGuid);
		if ((bool)vendorComponent)
		{
			Store component = vendorComponent.GetComponent<Store>();
			if ((bool)component)
			{
				component.buyMultiplier = buyRate;
				component.sellMultiplier = sellRate;
			}
			Inn component2 = vendorComponent.GetComponent<Inn>();
			if ((bool)component2)
			{
				component2.multiplier = innRate;
			}
		}
	}

	[Script("Open Recruitment", "Scripts\\Stores")]
	[ScriptParam0("Vendor", "Vendor object to open.", "", BrowserType.ObjectGuid)]
	public static void OpenRecruitment(Guid storeGuid)
	{
		Vendor vendorComponent = GetVendorComponent(storeGuid);
		if ((bool)vendorComponent)
		{
			vendorComponent.OpenRecruitment();
		}
	}

	[Script("Activate Stronghold", "Scripts\\Stronghold")]
	public static void ActivateStronghold()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null && !stronghold.Activated)
		{
			stronghold.ActivateStronghold(restoring: false);
		}
	}

	[Script("Disable Stronghold", "Scripts\\Stronghold")]
	public static void DisableStronghold()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Disabled = true;
		}
	}

	[Script("Show Stronghold UI", "Scripts\\Stronghold")]
	[ScriptParam0("Window", "Window To Show", "Status")]
	public static void ShowStrongholdUI(Stronghold.WindowPane window)
	{
		UIWindowManager.Instance.SuspendFor(UIStrongholdManager.Instance);
		UIStrongholdManager.Instance.ShowForPane = window;
		UIStrongholdManager.Instance.ShowWindow();
	}

	[Script("Add Prisoner", "Scripts\\Stronghold")]
	[ScriptParam0("Object", "Prisoner object to add to the stronghold prison", "", BrowserType.ObjectGuid)]
	public static void AddPrisoner(Guid objectGuid)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
			if (objectByID != null)
			{
				stronghold.AddPrisoner(objectByID);
			}
			else
			{
				UIDebug.Instance.LogOnScreenWarning(string.Concat("AddPrisoner script could not find object '", objectGuid, "'."), UIDebug.Department.Design, 10f);
			}
		}
	}

	[Script("Remove Prisoner", "Scripts\\Stronghold")]
	[ScriptParam0("Object", "Prisoner object to remove from the stronghold prison", "", BrowserType.ObjectGuid)]
	public static void RemovePrisoner(Guid objectGuid)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
			if (objectByID != null)
			{
				stronghold.RemovePrisoner(objectByID);
			}
		}
	}

	[Script("On Prisoner Death", "Scripts\\Stronghold")]
	[ScriptParam0("Object", "Prisoner object who has been killed", "", BrowserType.ObjectGuid)]
	public static void OnPrisonerDeath(Guid objectGuid)
	{
		RemovePrisoner(objectGuid);
	}

	[Script("On Hireling Death", "Scripts\\Stronghold")]
	[ScriptParam0("Object", "Hireling object who has been killed", "", BrowserType.ObjectGuid)]
	public static void OnHirelingDeath(Guid objectGuid)
	{
		if (!(GameState.s_playerCharacter != null))
		{
			return;
		}
		Stronghold stronghold = GameState.Stronghold;
		if (!(stronghold != null))
		{
			return;
		}
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID != null))
		{
			return;
		}
		CharacterStats component = objectByID.GetComponent<CharacterStats>();
		if (component != null)
		{
			string name = component.Name();
			StrongholdHireling strongholdHireling = stronghold.FindHireling(name);
			if (strongholdHireling != null)
			{
				stronghold.OnHirelingDeath(strongholdHireling);
			}
		}
		Persistence component2 = objectByID.GetComponent<Persistence>();
		if ((bool)component2)
		{
			PersistenceManager.RemoveObject(component2);
			GameUtilities.DestroyComponentImmediate(component2);
		}
	}

	[Script("On Visitor Death", "Scripts\\Stronghold")]
	[ScriptParam0("Object", "Visitor object who has been killed", "", BrowserType.ObjectGuid)]
	public static void OnVisitorDeath(Guid objectGuid)
	{
		if (!(GameState.s_playerCharacter != null))
		{
			return;
		}
		Stronghold stronghold = GameState.Stronghold;
		if (!(stronghold != null))
		{
			return;
		}
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!(objectByID != null))
		{
			return;
		}
		CharacterStats component = objectByID.GetComponent<CharacterStats>();
		if (component != null)
		{
			string name = component.Name();
			StrongholdVisitor strongholdVisitor = stronghold.FindVisitor(name);
			if (strongholdVisitor != null)
			{
				stronghold.OnVisitorDeath(strongholdVisitor);
			}
		}
	}

	[Script("Adjust Security", "Scripts\\Stronghold")]
	[ScriptParam0("Amount", "Amount to adjust stronghold Security", "0")]
	public static void AdjustSecurity(int adj)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Security += adj;
			Console.AddMessage(Stronghold.Format(281, adj), Color.green, Console.ConsoleState.Both);
		}
	}

	[Script("Adjust Prestige", "Scripts\\Stronghold")]
	[ScriptParam0("Amount", "Amount to adjust stronghold Prestige", "0")]
	public static void AdjustPrestige(int adj)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Prestige += adj;
			Console.AddMessage(Stronghold.Format(280, adj), Color.green, Console.ConsoleState.Both);
		}
	}

	[Script("Add Turns", "Scripts\\Stronghold")]
	[ScriptParam0("Count", "Number of turns to add", "1")]
	public static void AddTurns(int amount)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.AddTurns(amount);
		}
	}

	[Script("Send Stronghold Visitor", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to send.", "")]
	public static void SendStrongholdVisitor(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			Stronghold.Instance.SafeAddVisitor(visitorByTag, thwarted: false, almostThwarted: false);
		}
		else
		{
			Debug.LogError("SendStrongholdVisitor script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Send Stronghold Visitor Delayed", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to send.", "")]
	[ScriptParam1("Delay Hours", "Number of hours to wait before sending the visitor.", "27")]
	public static void SendStrongholdVisitorDelayed(string tag, float timeHours)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			Stronghold.Instance.SendVisitorDelayed(visitorByTag, timeHours);
		}
		else
		{
			Debug.LogError("SendStrongholdVisitorDelayed script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Dismiss Stronghold Visitor", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to dismiss.", "")]
	public static void DismissStrongholdVisitor(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			if (Stronghold.Instance.RemoveVisitor(visitorByTag))
			{
				visitorByTag.HandleLeaving(Stronghold.Instance);
			}
		}
		else
		{
			Debug.LogError("DismissStrongholdVisitor script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Kill Stronghold Visitor", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to kill.", "")]
	public static void KillStrongholdVisitor(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			Stronghold.Instance.LogTimeEvent(Stronghold.Format(38, visitorByTag.Name), Stronghold.NotificationType.Positive);
			Stronghold.Instance.RemoveVisitor(visitorByTag);
			Stronghold.Instance.AddVisitorToDeadList(visitorByTag);
		}
		else
		{
			Debug.LogError("KillStrongholdVisitor script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Set Stronghold Visitor No Return", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to kill.", "")]
	public static void SetStrongholdVisitorNoReturn(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			Stronghold.Instance.LogTimeEvent(Stronghold.Format(38, visitorByTag.Name), Stronghold.NotificationType.Positive);
			Stronghold.Instance.AddVisitorToDeadList(visitorByTag);
		}
		else
		{
			Debug.LogError("SetStrongholdVisitorNoReturn script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Execute Stronghold Visitor", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to kill.", "")]
	public static void ExecuteStrongholdVisitor(string tag)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(tag);
		if (visitorByTag != null)
		{
			Stronghold.Instance.LogTimeEvent(Stronghold.Format(274, visitorByTag.Name), Stronghold.NotificationType.Positive);
			Stronghold.Instance.RemoveVisitor(visitorByTag);
			Stronghold.Instance.AddVisitorToDeadList(visitorByTag);
		}
		else
		{
			Debug.LogError("ExecuteStrongholdVisitor script: Stronghold Visitor '" + tag + "' was not found.");
		}
	}

	[Script("Send Companion On Escort", "Scripts\\Stronghold")]
	[ScriptParam0("Companion", "The GUID of the companion to send. They will be sent to the stronghold if they are not already there.", "", BrowserType.ObjectGuid)]
	[ScriptParam1("Visitor Tag", "The tag of the visitor to escort (must be at the stronghold).", "")]
	[ScriptParam2("Escort Index", "The index of the escort (-1 for default escort).", "-1")]
	public static void SendCompanionOnEscort(Guid companion, string visitorTag, int escortIndex)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(visitorTag);
		if (visitorByTag == null)
		{
			Debug.LogError("SendCompanionOnEscort script: Stronghold Visitor '" + visitorTag + "' was not found.");
			return;
		}
		StoredCharacterInfo storedCharacterInfo = Stronghold.Instance.GetStoredCompanion(companion);
		if (storedCharacterInfo == null)
		{
			GameObject objectByID = InstanceID.GetObjectByID(companion);
			if ((bool)objectByID)
			{
				if (!objectByID.GetComponent<PartyMemberAI>())
				{
					Debug.LogError(string.Concat("SendCompanionOnEscort: '", companion, "' is not a party member."));
					return;
				}
				storedCharacterInfo = Stronghold.Instance.StoreCompanion(objectByID);
			}
		}
		if (storedCharacterInfo != null)
		{
			Stronghold.Instance.EscortVisitor(visitorByTag, storedCharacterInfo, escortIndex);
			int num = 0;
			if (escortIndex < visitorByTag.SpecialEscorts.Length)
			{
				num = visitorByTag.SpecialEscorts[escortIndex].Duration;
			}
			Console.AddMessage(GUIUtils.Format(2420, storedCharacterInfo.DisplayName, new EternityTimeInterval(num * WorldTime.Instance.SecondsPerDay).ToString()), Color.white, Console.ConsoleState.Both);
		}
		else
		{
			Debug.LogError(string.Concat("SendCompanionOnEscort: Failed to find companion '", companion, "'."));
		}
	}

	[Script("Send Visitor On Solo Escort", "Scripts\\Stronghold")]
	[ScriptParam0("Visitor Tag", "The tag of the visitor to escort (must be at the stronghold).", "")]
	[ScriptParam1("Escort Index", "The index of the escort (-1 for default escort).", "-1")]
	public static void SendVisitorOnSoloEscort(string visitorTag, int escortIndex)
	{
		StrongholdVisitor visitorByTag = Stronghold.Instance.GetVisitorByTag(visitorTag);
		if (visitorByTag == null)
		{
			Debug.LogError("SendVisitorOnEscort script: Stronghold Visitor '" + visitorTag + "' was not found.");
		}
		else
		{
			Stronghold.Instance.EscortVisitor(visitorByTag, null, escortIndex);
		}
	}

	[Script("Set Erl Tax Active", "Scripts\\Stronghold")]
	[ScriptParam0("Active", "The active state of the tax.", "true")]
	public static void SetErlTaxActive(bool state)
	{
		Stronghold.Instance.IsErlTaxActive = state;
	}

	[Script("Open Character Creation Screen", "Scripts\\UI")]
	public static void OpenCharacterCreation()
	{
		int num = 1;
		UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.NewPlayer, GameState.s_playerCharacter.gameObject, 0, num, CharacterStats.ExperienceNeededForLevel(num));
	}

	[Script("Open Character Creation New Companion Screen", "Scripts\\UI")]
	[ScriptParam0("Player Cost", "How much it will cost if the player completes this companion.", "0")]
	[ScriptParam1("Ending Level", "What level the character will be.", "1")]
	public static void OpenCharacterCreationNewCompanion(int playerCost, int endingLevel)
	{
		GameObject gameObject = GameResources.Instantiate<GameObject>(GameResources.LoadPrefab<GameObject>(UICharacterCreationManager.Instance.NewCompanionPrefabString, instantiate: false), GameState.s_playerCharacter.GetComponent<Rigidbody>().position, GameState.s_playerCharacter.GetComponent<Rigidbody>().rotation);
		PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.NewCompanion, gameObject, playerCost, 1, CharacterStats.ExperienceNeededForLevel(endingLevel));
		gameObject.GetComponent<CharacterStats>().Experience = CharacterStats.ExperienceNeededForLevel(endingLevel);
	}

	[Script("Play Interstitial", "Scripts\\UI")]
	[ScriptParam0("Index", "The index of an interstitial to play (see InterstitialMaster list).", "0")]
	public static void PlayInterstitial(int index)
	{
		if (UIInterstitialManager.Instance == null)
		{
			Debug.LogError("PlayInterstitial: UI hasn't been given a chance to initialize yet.");
			return;
		}
		UIInterstitialManager.ForChapter = index;
		UIInterstitialManager.Instance.ShowWindow();
	}

	[Script("Open Crafting Window", "Scripts\\UI")]
	[ScriptParam0("Crafting Object Type", "A string identifying a unique crafting location to show (Recipe.CraftingLocation)", "")]
	public static void OpenCrafting(string location)
	{
		if (UICraftingManager.Instance == null)
		{
			Debug.LogError("OpenCrafting: UI hasn't been given a chance to initialize yet.");
		}
		else if (!GameState.InCombat)
		{
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.EnchantMode = false;
			UICraftingManager.Instance.ForLocation = location;
			UICraftingManager.Instance.ShowWindow();
		}
	}

	[Script("Open Enchanting Window", "Scripts\\UI")]
	[ScriptParam0("Crafting Object Type", "A string identifying a unique crafting location to show (Recipe.CraftingLocation)", "")]
	[ScriptParam1("Target Item Name", "The name of the item to enchant.", "")]
	public static void OpenEnchanting(string location, string itemTarget)
	{
		if (UICraftingManager.Instance == null)
		{
			Debug.LogError("OpenEnchanting: UI hasn't been given a chance to initialize yet.");
			return;
		}
		Item item = null;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (!onlyPrimaryPartyMember)
			{
				continue;
			}
			BaseInventory[] components = onlyPrimaryPartyMember.GetComponents<BaseInventory>();
			for (int i = 0; i < components.Length; i++)
			{
				InventoryItem inventoryItem = components[i].ItemList.FirstOrDefault((InventoryItem ii) => ii.NameEquals(itemTarget));
				if (inventoryItem != null)
				{
					item = inventoryItem.baseItem;
				}
				if ((bool)item)
				{
					break;
				}
			}
			Equipment component = onlyPrimaryPartyMember.GetComponent<Equipment>();
			if ((bool)component)
			{
				Ref<Equippable> @ref = component.CurrentItems.Slots.FirstOrDefault((Ref<Equippable> re) => (bool)re.Val && re.Val.Prefab.name.Equals(itemTarget));
				if (@ref != null)
				{
					item = @ref.Val;
				}
				if ((bool)item)
				{
					break;
				}
				WeaponSet weaponSet = component.WeaponSets.FirstOrDefault((WeaponSet ws) => (bool)ws.PrimaryWeapon && ws.PrimaryWeapon.Prefab.name.Equals(itemTarget));
				if (weaponSet != null)
				{
					item = weaponSet.PrimaryWeapon;
				}
				if ((bool)item)
				{
					break;
				}
				weaponSet = component.WeaponSets.FirstOrDefault((WeaponSet ws) => (bool)ws.SecondaryWeapon && ws.SecondaryWeapon.Prefab.name.Equals(itemTarget));
				if (weaponSet != null)
				{
					item = weaponSet.SecondaryWeapon;
				}
				if ((bool)item)
				{
					break;
				}
			}
		}
		if ((bool)item)
		{
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.EnchantMode = true;
			UICraftingManager.Instance.ForLocation = location;
			UICraftingManager.Instance.EnchantTarget = item;
			UICraftingManager.Instance.ShowWindow();
		}
	}

	[Script("Return To Main Menu", "Scripts\\UI")]
	public static void ReturnToMainMenu()
	{
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(s_EndDemoMonitor, state: false);
		}
		InGameUILayout inGameUILayout = UnityEngine.Object.FindObjectOfType<InGameUILayout>();
		InGameHUD.Instance.ShowHUD = false;
		inGameUILayout.StartCoroutine(EndDemo());
	}

	public static IEnumerator EndDemo()
	{
		yield return new WaitForSeconds(0.5f);
		string[] array = new string[3] { "EndDemoBackground", "EndDemoObsidian", "EndDemoTitle" };
		for (int i = 0; i < array.Length; i++)
		{
			TweenAlpha component = GameObject.Find(array[i]).GetComponent<TweenAlpha>();
			component.enabled = true;
			component.Play(forward: true);
		}
		yield return new WaitForSeconds(10.5f);
		FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 2f);
		yield return new WaitForSeconds(5f);
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(s_EndDemoMonitor, state: true);
		}
		GameState.LoadMainMenu(fadeOut: false);
	}

	[Script("Trigger Tutorial", "Scripts\\UI")]
	[ScriptParam0("Tutorial Index", "The index in the TutorialMaster list of the tutorial to play.", "0")]
	public static void TriggerTutorial(int index)
	{
		TutorialManager.STriggerTutorial(index);
	}

	[Script("Set End Game Slide", "Scripts\\UI")]
	[ScriptParam0("Image Index", "The index in the EndGameSlidesImages array.", "0")]
	public static void SetEndGameSlide(int imageIndex)
	{
		UIEndGameSlidesManager.Instance.SetImage(imageIndex);
	}

	[Script("Begin Watcher Movie", "Scripts\\UI")]
	public static void BeginWatcherMovie()
	{
		UIConversationWatcherMovie.Instance.Show();
	}

	[Script("End Watcher Movie", "Scripts\\UI")]
	public static void EndWatcherMovie()
	{
		UIConversationWatcherMovie.Instance.Hide();
	}
}
