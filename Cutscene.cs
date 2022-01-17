using System;
using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
	protected static List<Cutscene> ActiveCutscenes = new List<Cutscene>();

	public List<SceneActor> SceneActorList = new List<SceneActor>();

	public List<PrefabActor> PrefabActorList = new List<PrefabActor>();

	protected List<GameObject> ActorList = new List<GameObject>();

	protected List<GameObject> PartyCleanupList = new List<GameObject>();

	private List<GameObject> SelectedPartyMembers = new List<GameObject>();

	[HideInInspector]
	public List<CutsceneWaypoint> SpawnWaypointList = new List<CutsceneWaypoint>();

	[HideInInspector]
	public List<CutsceneWaypoint> MoveWaypointList = new List<CutsceneWaypoint>();

	[Tooltip("If true, any animal companion in the scene will follow its owner during the cutscene. If false, the animal will be stationary where he stands.")]
	public bool AnimalCompanionsFollowOwner = true;

	protected GameObject RealPlayer;

	protected GameObject ActorPlayer;

	protected GameObject[] RealParty = new GameObject[6];

	protected GameObject[] ActorParty = new GameObject[6];

	protected ScreenTextureScript_Occlusion m_cameraOcclusionPass;

	public bool IncludePartyInActorList = true;

	public bool HideNonActorCharacters = true;

	public bool PauseNonActorCharacters = true;

	public bool UsePartyStartLocation;

	public PartyWaypoint PartyStartLocation;

	public bool UsePartyMoveLocation;

	public PartyWaypoint PartyMoveLocation;

	public CutsceneWaypoint.CutsceneMoveType PartyMoveType;

	public bool UseCameraStartLookAtLocation = true;

	public bool UseCameraEndLookAtLocation;

	public Transform CameraStartLookAtLocation;

	public Transform CameraEndLookAtLocation;

	public float CameraMoveTime;

	public Transform CameraFollowObject;

	public bool AutoFadeOnStart;

	public bool AutoFadeOnEnd;

	public float FailsafeTimer = 10f;

	public bool StartOnEnter;

	public bool DisableFog = true;

	public static bool CutsceneActive => ActiveCutscenes.Count > 0;

	public bool Active { get; protected set; }

	public FogOfWarRevealer[] FowRevealers { get; set; }

	private void Start()
	{
		DisableRevealers();
	}

	private void Update()
	{
		if (StartOnEnter && !GameState.IsLoading)
		{
			StartOnEnter = false;
			StartCutscene();
		}
	}

	private void OnDestroy()
	{
		PartyMemberAI.SafeEnableDisable = false;
		if (ActiveCutscenes.Contains(this))
		{
			ActiveCutscenes.Remove(this);
		}
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: true);
		}
		DisableRevealers();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void ForceEndAllCutscenes(bool callEndScripts)
	{
		Cutscene[] array = UnityEngine.Object.FindObjectsOfType<Cutscene>();
		foreach (Cutscene cutscene in array)
		{
			if (cutscene.Active)
			{
				cutscene.EndCutscene(callEndScripts);
			}
		}
	}

	protected void AddPartyToActorList()
	{
		int num = 1;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			int num2 = 0;
			if (partyMemberAI == null || (partyMemberAI.Secondary && (!AnimalCompanionsFollowOwner || partyMemberAI.SummonType != AIController.AISummonType.AnimalCompanion || partyMemberAI.Summoner == null)))
			{
				continue;
			}
			GameObject gameObject = partyMemberAI.gameObject;
			if ((bool)partyMemberAI.GetComponent<Player>())
			{
				RealPlayer = partyMemberAI.gameObject;
				ActorPlayer = gameObject;
				RealParty[0] = RealPlayer;
				ActorParty[0] = ActorPlayer;
				num2 = 0;
			}
			else if (partyMemberAI.Slot < 6)
			{
				num2 = num;
				RealParty[num] = partyMemberAI.gameObject;
				ActorParty[num] = gameObject;
				num++;
			}
			else
			{
				partyMemberAI.IgnoreAsCutsceneObstacle = true;
			}
			if (UsePartyStartLocation && PartyStartLocation != null)
			{
				CutsceneWaypoint cutsceneWaypoint = new CutsceneWaypoint();
				if (partyMemberAI.Slot < 6)
				{
					cutsceneWaypoint.owner = gameObject;
				}
				else
				{
					cutsceneWaypoint.owner = null;
				}
				cutsceneWaypoint.MoveType = CutsceneWaypoint.CutsceneMoveType.Teleport;
				cutsceneWaypoint.TeleportVFX = null;
				cutsceneWaypoint.Location = PartyStartLocation.Waypoints[num2].transform;
				SpawnWaypointList.Add(cutsceneWaypoint);
			}
			if (UsePartyMoveLocation && PartyMoveLocation != null)
			{
				CutsceneWaypoint cutsceneWaypoint2 = new CutsceneWaypoint();
				if (partyMemberAI.Slot < 6)
				{
					cutsceneWaypoint2.owner = gameObject;
				}
				else
				{
					cutsceneWaypoint2.owner = null;
				}
				cutsceneWaypoint2.MoveType = PartyMoveType;
				cutsceneWaypoint2.TeleportVFX = null;
				cutsceneWaypoint2.Location = PartyMoveLocation.Waypoints[num2].transform;
				MoveWaypointList.Add(cutsceneWaypoint2);
			}
			ActorList.Add(gameObject);
			PartyCleanupList.Add(gameObject);
		}
	}

	protected void RemovePartyFromActorList()
	{
		foreach (GameObject partyCleanup in PartyCleanupList)
		{
			if (ActorList.Contains(partyCleanup))
			{
				ClearCutsceneObtacle(partyCleanup);
				ActorList.Remove(partyCleanup);
			}
		}
		PartyCleanupList.Clear();
	}

	protected void ClearCutsceneObtacle(GameObject obj)
	{
		if (obj != null)
		{
			PartyMemberAI component = obj.GetComponent<PartyMemberAI>();
			if (component != null)
			{
				component.IgnoreAsCutsceneObstacle = false;
			}
		}
	}

	private bool ParentOfObjectInActorList(GameObject obj)
	{
		if (ActorList.Contains(obj))
		{
			return true;
		}
		if ((bool)obj.transform.parent)
		{
			return ParentOfObjectInActorList(obj.transform.parent.gameObject);
		}
		return false;
	}

	private bool ParentIsCharacter(GameObject obj)
	{
		if ((bool)obj.GetComponent<CharacterStats>())
		{
			return true;
		}
		if ((bool)obj.transform.parent)
		{
			return ParentIsCharacter(obj.transform.parent.gameObject);
		}
		return false;
	}

	private bool ParentIsPartyMember(GameObject obj)
	{
		PartyMemberAI component = obj.GetComponent<PartyMemberAI>();
		if ((bool)component && component.IsInSlot)
		{
			return true;
		}
		if ((bool)obj.transform.parent)
		{
			return ParentIsPartyMember(obj.transform.parent.gameObject);
		}
		return false;
	}

	protected void AddSceneActors()
	{
		foreach (SceneActor sceneActor in SceneActorList)
		{
			if (sceneActor.Actor == null)
			{
				continue;
			}
			if (AnimalCompanionsFollowOwner)
			{
				AIPackageController component = sceneActor.Actor.GetComponent<AIPackageController>();
				if (component != null)
				{
					foreach (GameObject summonedCreature in component.SummonedCreatureList)
					{
						AIPackageController component2 = summonedCreature.GetComponent<AIPackageController>();
						if (component2 != null && component2.SummonType == AIController.AISummonType.AnimalCompanion && component2.Summoner != null)
						{
							AddSceneActor(null, summonedCreature, sceneActor.IgnoreIfDead);
							summonedCreature.SetActive(value: true);
						}
					}
				}
			}
			AddSceneActor(sceneActor, sceneActor.Actor, sceneActor.IgnoreIfDead);
		}
	}

	private void AddSceneActor(SceneActor actor, GameObject actorGameObject, bool ignoreIfDead)
	{
		if (ignoreIfDead)
		{
			Health component = actorGameObject.GetComponent<Health>();
			if (component != null && component.Dead)
			{
				return;
			}
		}
		if (actor != null)
		{
			if (actor.ActivateAtStart)
			{
				actorGameObject.SetActive(value: true);
				Persistence component2 = actorGameObject.GetComponent<Persistence>();
				if ((bool)component2)
				{
					component2.SaveObject();
				}
				ConditionalToggle component3 = actorGameObject.GetComponent<ConditionalToggle>();
				if ((bool)component3)
				{
					component3.ForceActivate();
				}
			}
			if (actor.UseSpawnLocation)
			{
				actor.SpawnLocation.owner = actor.Actor;
				SpawnWaypointList.Add(actor.SpawnLocation);
			}
			if (actor.UseMoveLocation)
			{
				actor.MoveLocation.owner = actor.Actor;
				MoveWaypointList.Add(actor.MoveLocation);
			}
		}
		ActorList.Add(actorGameObject);
	}

	protected void RemoveSceneActors()
	{
		foreach (SceneActor sceneActor in SceneActorList)
		{
			if (sceneActor.Actor == null)
			{
				continue;
			}
			if (sceneActor.DeactivateAtEnd)
			{
				sceneActor.Actor.SetActive(value: false);
				Persistence component = sceneActor.Actor.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.SaveObject();
				}
				ConditionalToggle component2 = sceneActor.Actor.GetComponent<ConditionalToggle>();
				if ((bool)component2)
				{
					component2.ActivateOnlyThroughScript = true;
					component2.StartActivated = false;
					InstanceID component3 = sceneActor.Actor.GetComponent<InstanceID>();
					if (component3 != null)
					{
						component3.Load();
						ConditionalToggleManager.Instance.AddToScriptInactiveList(component2);
					}
				}
			}
			ClearCutsceneObtacle(sceneActor.Actor);
			ActorList.Remove(sceneActor.Actor);
		}
	}

	protected void SpawnPrefabActors()
	{
		foreach (PrefabActor prefabActor in PrefabActorList)
		{
			if (prefabActor.Prefab == null)
			{
				UIDebug.Instance.LogOnScreenWarning("Cutscene '" + base.name + "': has null PrefabActor->Prefab.", UIDebug.Department.Design, 10f);
				continue;
			}
			if (prefabActor.SpawnLocation.Location == null)
			{
				UIDebug.Instance.LogOnScreenWarning("Cutscene '" + base.name + "': Prefab Actor '" + prefabActor.Prefab.name + "' has no Spawn Location.", UIDebug.Department.Design, 10f);
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(prefabActor.Prefab, prefabActor.SpawnLocation.Location.position, prefabActor.SpawnLocation.Location.rotation);
			gameObject.name = prefabActor.Prefab.name;
			prefabActor.SpawnedObject = gameObject;
			if ((bool)gameObject)
			{
				ActorList.Add(gameObject);
				if (prefabActor.DeactivateAtStart)
				{
					gameObject.SetActive(value: false);
				}
				prefabActor.SpawnLocation.owner = gameObject;
				prefabActor.MoveLocation.owner = gameObject;
				AIController component = gameObject.GetComponent<AIController>();
				if (component != null)
				{
					component.RecordRetreatPosition(prefabActor.SpawnLocation.Location.position);
				}
				SpawnWaypointList.Add(prefabActor.SpawnLocation);
				if (prefabActor.UseMoveLocation)
				{
					MoveWaypointList.Add(prefabActor.MoveLocation);
				}
			}
		}
	}

	protected void DestroyPrefabActors()
	{
		foreach (PrefabActor prefabActor in PrefabActorList)
		{
			if ((bool)prefabActor.SpawnedObject)
			{
				ClearCutsceneObtacle(prefabActor.SpawnedObject);
				ActorList.Remove(prefabActor.SpawnedObject);
				if (prefabActor.DeleteAtEnd)
				{
					PersistenceManager.RemoveObject(prefabActor.SpawnedObject.GetComponent<Persistence>());
					GameUtilities.Destroy(prefabActor.SpawnedObject);
				}
			}
		}
	}

	protected void AddPuppetControllerToActors()
	{
		PartyMemberAI.SafeEnableDisable = true;
		foreach (GameObject actor in ActorList)
		{
			AddPuppetControllerToActor(actor);
		}
		PartyMemberAI.SafeEnableDisable = false;
	}

	protected void AddPuppetControllerToActor(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		(obj.AddComponent(typeof(PuppetModeController)) as PuppetModeController).ActivatePuppetMode();
		AIController component = obj.GetComponent<AIPackageController>();
		if (component != null)
		{
			component.InterruptAnimationForCutscene();
			if (component.StateManager != null)
			{
				component.StateManager.PopAllStates();
			}
			component.InCutscene = true;
			component.enabled = false;
		}
		PartyMemberAI component2 = obj.GetComponent<PartyMemberAI>();
		if ((bool)component2)
		{
			if (component2.StateManager.CurrentState is Unconscious unconscious)
			{
				unconscious.TriggerRevive();
			}
			component2.InterruptAnimationForCutscene();
			if (component2.StateManager != null)
			{
				component2.StateManager.PopAllStates();
			}
			component2.enabled = false;
		}
	}

	public void RemovePuppetControllerFromActor(GameObject obj)
	{
		PartyMemberAI.SafeEnableDisable = true;
		if (!obj)
		{
			return;
		}
		PuppetModeController component = obj.GetComponent<PuppetModeController>();
		if ((bool)component)
		{
			component.DeactivatePuppetMode();
			GameUtilities.DestroyImmediate(component);
		}
		PartyMemberAI component2 = obj.GetComponent<PartyMemberAI>();
		if (component2 == null || !component2.IsInSlot)
		{
			AIController component3 = obj.GetComponent<AIPackageController>();
			if (component3 != null)
			{
				component3.InCutscene = false;
				component3.enabled = true;
			}
		}
		else
		{
			component2.enabled = true;
		}
		PartyMemberAI.SafeEnableDisable = false;
	}

	protected void RemovePuppetControllerFromActors()
	{
		foreach (GameObject actor in ActorList)
		{
			RemovePuppetControllerFromActor(actor);
		}
	}

	protected void PauseObjects()
	{
		Type[] array = new Type[7]
		{
			typeof(AnimationController),
			typeof(Animator),
			typeof(Mover),
			typeof(Timer),
			typeof(Health),
			typeof(AIController),
			typeof(AttackBeam)
		};
		GameObject[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject gameObject in array2)
		{
			if (ParentOfObjectInActorList(gameObject))
			{
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				Behaviour behaviour = gameObject.GetComponent(array[j]) as Behaviour;
				if ((bool)behaviour && !(behaviour is PartyMemberAI))
				{
					behaviour.enabled = false;
				}
			}
			if (HideNonActorCharacters && (bool)gameObject.GetComponent<Renderer>() && ParentIsCharacter(gameObject))
			{
				gameObject.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void UnPauseObject(GameObject obj)
	{
		Type[] array = new Type[7]
		{
			typeof(AnimationController),
			typeof(Animator),
			typeof(Mover),
			typeof(Timer),
			typeof(Health),
			typeof(AIController),
			typeof(AttackBeam)
		};
		for (int i = 0; i < array.Length; i++)
		{
			Behaviour behaviour = obj.GetComponent(array[i]) as Behaviour;
			if (behaviour is PartyMemberAI)
			{
				if ((behaviour as PartyMemberAI).IsInSlot)
				{
					behaviour.enabled = true;
				}
			}
			else if (behaviour is AIController)
			{
				PartyMemberAI component = obj.GetComponent<PartyMemberAI>();
				if (component == null || !component.IsInSlot)
				{
					behaviour.enabled = true;
				}
			}
			else if ((bool)behaviour)
			{
				behaviour.enabled = true;
			}
		}
		if (HideNonActorCharacters && (bool)obj.GetComponent<Renderer>() && ParentIsCharacter(obj))
		{
			obj.GetComponent<Renderer>().enabled = true;
		}
	}

	protected void UnPauseObjects()
	{
		Type[] array = new Type[7]
		{
			typeof(AnimationController),
			typeof(Animator),
			typeof(Mover),
			typeof(Timer),
			typeof(Health),
			typeof(AIController),
			typeof(AttackBeam)
		};
		GameObject[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject gameObject in array2)
		{
			if (ParentOfObjectInActorList(gameObject))
			{
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				Behaviour behaviour = gameObject.GetComponent(array[j]) as Behaviour;
				if (behaviour is PartyMemberAI)
				{
					if ((behaviour as PartyMemberAI).IsInSlot)
					{
						behaviour.enabled = true;
					}
				}
				else if (behaviour is AIController)
				{
					PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
					if (component == null || !component.IsInSlot)
					{
						behaviour.enabled = true;
					}
				}
				else if ((bool)behaviour)
				{
					behaviour.enabled = true;
				}
			}
			if (HideNonActorCharacters && (bool)gameObject.GetComponent<Renderer>() && ParentIsCharacter(gameObject))
			{
				gameObject.GetComponent<Renderer>().enabled = true;
			}
		}
	}

	protected void MoveSecondaryParty()
	{
		float num = 15f;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && partyMemberAI.Secondary && GameUtilities.V3SqrDistance2D(partyMemberAI.transform.position, partyMemberAI.Summoner.transform.position) > num * num)
			{
				Mover component = partyMemberAI.GetComponent<Mover>();
				if (component != null)
				{
					partyMemberAI.transform.position = GameUtilities.NearestUnoccupiedLocation(partyMemberAI.Summoner.transform.position, component.Radius, num, component);
				}
				else
				{
					partyMemberAI.transform.position = GameUtilities.NearestUnoccupiedLocation(partyMemberAI.Summoner.transform.position, 0.5f, num, null);
				}
			}
		}
	}

	public virtual void StartCutscene()
	{
		if (!GameState.CutsceneAllowed)
		{
			return;
		}
		if ((bool)TimeController.Instance)
		{
			TimeController.Instance.Paused = false;
		}
		SelectedPartyMembers.Clear();
		SelectedPartyMembers.AddRange(PartyMemberAI.SelectedPartyMembers);
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnCutsceneStart);
		}
		PartyMemberAI.PopAllStates();
		SpawnWaypointList.Clear();
		MoveWaypointList.Clear();
		if (IncludePartyInActorList)
		{
			AddPartyToActorList();
		}
		AddSceneActors();
		SpawnPrefabActors();
		AddPuppetControllerToActors();
		Active = true;
		ActiveCutscenes.Add(this);
		CameraControl.Instance.EnablePlayerControl(enableControl: false);
		CameraControl.Instance.EnablePlayerScroll(enableScroll: false);
		if ((bool)GameState.s_playerCharacter)
		{
			GameState.s_playerCharacter.CancelModes(cancelAbility: true);
		}
		InGameHUD.Instance.ShowHUD = false;
		bool flag = false;
		if (FowRevealers != null)
		{
			FogOfWarRevealer[] fowRevealers = FowRevealers;
			foreach (FogOfWarRevealer fogOfWarRevealer in fowRevealers)
			{
				if ((bool)fogOfWarRevealer)
				{
					fogOfWarRevealer.gameObject.SetActive(value: true);
					flag = true;
				}
			}
		}
		if (!flag && DisableFog)
		{
			FogOfWarRender.Instance.gameObject.SetActive(value: false);
		}
		PartyMemberAI.SafeEnableDisable = true;
		if ((bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.CloseAllWindows();
		}
		UIWindowManager.DisableWindowVisibilityHandling();
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: false);
		}
		GameInput.BeginBlockAllKeys();
		if (PauseNonActorCharacters)
		{
			PauseObjects();
		}
		m_cameraOcclusionPass = UnityEngine.Object.FindObjectOfType<ScreenTextureScript_Occlusion>();
		if ((bool)m_cameraOcclusionPass)
		{
			m_cameraOcclusionPass.gameObject.SetActive(value: false);
		}
		BasePuppetScript basePuppetScript = GetComponent<BasePuppetScript>();
		if (basePuppetScript == null)
		{
			basePuppetScript = base.gameObject.AddComponent<BasePuppetScript>();
		}
		if ((bool)basePuppetScript)
		{
			basePuppetScript.ReferencedObjects = ActorList.ToArray();
			basePuppetScript.RealPlayer = RealPlayer;
			basePuppetScript.ActorPlayer = ActorPlayer;
			basePuppetScript.RealParty = RealParty;
			basePuppetScript.ActorParty = ActorParty;
			basePuppetScript.FailSafeTimer = FailsafeTimer;
			basePuppetScript.Run();
		}
	}

	public virtual void EndCutscene(bool callEndScripts)
	{
		Active = false;
		ActiveCutscenes.Remove(this);
		MoveSecondaryParty();
		if (PauseNonActorCharacters)
		{
			UnPauseObjects();
		}
		RemovePuppetControllerFromActors();
		DestroyPrefabActors();
		RemoveSceneActors();
		RemovePartyFromActorList();
		CameraControl.Instance.EnablePlayerControl(enableControl: true);
		CameraControl.Instance.EnablePlayerScroll(enableScroll: true);
		InGameHUD.Instance.ShowHUD = true;
		DisableRevealers();
		FogOfWarRender.Instance.gameObject.SetActive(value: true);
		PartyMemberAI.SafeEnableDisable = false;
		UIWindowManager.EnableWindowVisibilityHandling();
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: true);
		}
		GameInput.EndBlockAllKeys();
		if ((bool)m_cameraOcclusionPass)
		{
			m_cameraOcclusionPass.gameObject.SetActive(value: true);
		}
		m_cameraOcclusionPass = null;
		for (int i = 0; i < SelectedPartyMembers.Count; i++)
		{
			GameObject gameObject = SelectedPartyMembers[i];
			if (!(gameObject == null) && gameObject.activeInHierarchy)
			{
				PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
				if (component != null)
				{
					component.Selected = true;
				}
			}
		}
		SelectedPartyMembers.Clear();
		if (callEndScripts)
		{
			ScriptEvent component2 = GetComponent<ScriptEvent>();
			if ((bool)component2)
			{
				component2.ExecuteScript(ScriptEvent.ScriptEvents.OnCutsceneEnd);
			}
		}
	}

	protected void DisableRevealers()
	{
		if (FowRevealers == null)
		{
			return;
		}
		FogOfWarRevealer[] fowRevealers = FowRevealers;
		foreach (FogOfWarRevealer fogOfWarRevealer in fowRevealers)
		{
			if ((bool)fogOfWarRevealer)
			{
				fogOfWarRevealer.gameObject.SetActive(value: false);
			}
		}
	}

	private void CopyStats(CharacterStats src, CharacterStats dest)
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

	private void CopyPortrait(Portrait src, Portrait dest)
	{
		dest.TextureSmallPath = src.TextureSmallPath;
		dest.TextureLargePath = src.TextureLargePath;
		dest.TextureLarge = src.TextureLarge;
		dest.TextureSmall = src.TextureSmall;
	}

	private void CopyEquipment(Equipment src, Equipment dest)
	{
		for (int i = 0; i < src.DefaultEquippedItems.Slots.Length; i++)
		{
			if (src.CurrentItems.Slots[i].Val != null)
			{
				dest.DefaultEquippedItems.Slots[i].Val = src.CurrentItems.Slots[i].Val.Prefab as Equippable;
			}
		}
		if (src.CurrentItems.GetSelectedWeaponSet().PrimaryWeapon != null)
		{
			dest.DefaultEquippedItems.PrimaryWeapon = src.CurrentItems.GetSelectedWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (src.CurrentItems.GetSelectedWeaponSet().SecondaryWeapon != null)
		{
			dest.DefaultEquippedItems.SecondaryWeapon = src.CurrentItems.GetSelectedWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		dest.DefaultEquippedItems.AlternateWeaponSets = new WeaponSet[1];
		dest.DefaultEquippedItems.AlternateWeaponSets[0] = new WeaponSet();
		if (src.CurrentItems.GetFirstAlternateWeaponSet() != null)
		{
			if (src.CurrentItems.GetFirstAlternateWeaponSet().PrimaryWeapon != null)
			{
				dest.DefaultEquippedItems.AlternateWeaponSets[0].PrimaryWeapon = src.CurrentItems.GetFirstAlternateWeaponSet().PrimaryWeapon.Prefab as Equippable;
			}
			if (src.CurrentItems.GetFirstAlternateWeaponSet().SecondaryWeapon != null)
			{
				dest.DefaultEquippedItems.AlternateWeaponSets[0].SecondaryWeapon = src.CurrentItems.GetFirstAlternateWeaponSet().SecondaryWeapon.Prefab as Equippable;
			}
		}
	}

	private void CopyMover(Mover src, Mover dest)
	{
		dest.Acceleration = src.Acceleration;
		dest.Radius = src.Radius;
		dest.WalkSpeed = src.WalkSpeed;
		dest.RunSpeed = src.RunSpeed;
		dest.GetComponent<Rigidbody>().mass = src.GetComponent<Rigidbody>().mass;
		dest.GetComponent<Rigidbody>().isKinematic = src.GetComponent<Rigidbody>().isKinematic;
		dest.GetComponent<Rigidbody>().interpolation = src.GetComponent<Rigidbody>().interpolation;
		dest.GetComponent<Rigidbody>().collisionDetectionMode = src.GetComponent<Rigidbody>().collisionDetectionMode;
	}

	private void CopyFaction(Faction src, Faction dest)
	{
		dest.ModifyToMatch(src);
	}

	public Transform GetPartyStartTransform(int index)
	{
		if (PartyStartLocation != null && PartyStartLocation.Waypoints.Length > index && (bool)PartyStartLocation.Waypoints[index])
		{
			return PartyStartLocation.Waypoints[index].transform;
		}
		return null;
	}
}
