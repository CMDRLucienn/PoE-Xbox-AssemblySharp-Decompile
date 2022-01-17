using System;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
	public enum FactionSettings
	{
		DontDoAnything,
		UseTeamFromEncountersFaction,
		CreateNewTeamFromEncountersFaction,
		CreateNewHostileTeam,
		CreateNewNeutralTeam
	}

	public bool SpawnOnStart = true;

	public FactionSettings EncounterFaction;

	[EnumFlags]
	[Tooltip("If set, these scalers will be added to all monsters in this encounter.")]
	public DifficultyScaling.Scaler ScaleChildrenWith;

	public List<EncounterData> encounterList = new List<EncounterData>();

	[Tooltip("When true, the EncounterEnd event won't fire until combat is actually over.")]
	public bool DelayEndEventForCombat;

	[Tooltip("When true, all spawned creatures in the encounter must die before combat will end.")]
	public bool CombatEndsWhenAllAreDead;

	protected ScriptEvent m_ScriptEvent;

	protected Faction m_Faction;

	private List<GameObject> spawnedList = new List<GameObject>();

	private List<GameObject> disabledInstanceList = new List<GameObject>();

	private List<GameObject> disabledList = new List<GameObject>();

	private List<Guid> guidList = new List<Guid>();

	private bool m_restored;

	private bool spawned;

	private bool finished;

	private List<Guid> mMissingSpawnedCreatures = new List<Guid>();

	[Persistent]
	private List<Guid> SerializedGuids
	{
		get
		{
			return guidList;
		}
		set
		{
			guidList = value;
			m_restored = true;
		}
	}

	[Persistent]
	private GameDifficulty SavedDifficulty { get; set; }

	[Persistent]
	public List<Guid> SpawnListIDs
	{
		get
		{
			List<Guid> list = new List<Guid>();
			if (spawnedList != null)
			{
				foreach (GameObject spawned in spawnedList)
				{
					if (!(spawned == null))
					{
						InstanceID component = spawned.GetComponent<InstanceID>();
						if (component != null)
						{
							list.Add(component.Guid);
						}
					}
				}
				return list;
			}
			return list;
		}
		set
		{
			if (spawnedList == null)
			{
				spawnedList = new List<GameObject>();
			}
			foreach (Guid item in value)
			{
				GameObject objectByID = InstanceID.GetObjectByID(item);
				if (objectByID == null)
				{
					mMissingSpawnedCreatures.Add(item);
				}
				else if (!spawnedList.Contains(objectByID))
				{
					spawnedList.Add(objectByID);
					Health component = objectByID.GetComponent<Health>();
					if ((bool)component)
					{
						component.OnDeath += SpawnDied;
					}
					SetupSpawnFaction(objectByID);
					Persistence component2 = objectByID.GetComponent<Persistence>();
					if ((bool)component2)
					{
						component2.Load();
					}
				}
			}
		}
	}

	private void Awake()
	{
		SavedDifficulty = GameState.Instance.Difficulty;
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		VerifyRestoredSpawnCreaturesRelinked();
	}

	private void Start()
	{
		m_ScriptEvent = GetComponent<ScriptEvent>();
		SetupEncounterFaction();
		if (!SpawnOnStart)
		{
			DisableInstances();
		}
		if (guidList.Count != 0)
		{
			return;
		}
		for (int i = 0; i < encounterList.Count; i++)
		{
			InstanceID instanceID = null;
			if ((bool)encounterList[i].Creature)
			{
				instanceID = encounterList[i].Creature.GetComponent<InstanceID>();
			}
			if (instanceID == null)
			{
				guidList.Add(Guid.Empty);
			}
			else if (!InstanceID.ObjectIsActive(instanceID.Guid) && !(instanceID is CompanionInstanceID))
			{
				guidList.Add(Guid.NewGuid());
			}
			else
			{
				guidList.Add(instanceID.Guid);
			}
		}
	}

	public void Restored()
	{
		if (!GameState.LoadedGame || (GameState.LoadedGame && !GameState.IsLoading))
		{
			m_restored = true;
		}
	}

	private void OnEnable()
	{
		for (int i = 0; i < disabledList.Count; i++)
		{
			if (disabledList[i] != null)
			{
				disabledList[i].SetActive(value: true);
				if (i < encounterList.Count && encounterList[i] != null)
				{
					GameUtilities.LaunchEffect(encounterList[i].SpawnVfx, 1f, disabledList[i].transform.position, null);
				}
			}
		}
		disabledList.Clear();
	}

	private void OnDisable()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		disabledList.Clear();
		for (int i = 0; i < spawnedList.Count; i++)
		{
			if (!(spawnedList[i] != null))
			{
				continue;
			}
			PartyMemberAI component = spawnedList[i].GetComponent<PartyMemberAI>();
			if (!(component != null) || !component.enabled)
			{
				disabledList.Add(spawnedList[i]);
				spawnedList[i].SetActive(value: false);
				if (i < encounterList.Count && encounterList[i] != null)
				{
					GameUtilities.LaunchEffect(encounterList[i].DespawnVfx, 1f, spawnedList[i].transform.position, null);
				}
			}
		}
	}

	public void DisableInstances()
	{
		if (spawned)
		{
			return;
		}
		disabledInstanceList.Clear();
		for (int i = 0; i < encounterList.Count; i++)
		{
			if (!(encounterList[i].Creature == null))
			{
				PartyMemberAI component = encounterList[i].Creature.GetComponent<PartyMemberAI>();
				if ((!(component != null) || !component.enabled) && !encounterList[i].IsPrefab)
				{
					GameObject creature = encounterList[i].Creature;
					creature.SetActive(value: false);
					disabledInstanceList.Add(creature);
				}
			}
		}
	}

	public void DisableInstance(GameObject creature)
	{
		foreach (EncounterData encounter in encounterList)
		{
			if (encounter.Creature == creature && !disabledInstanceList.Contains(creature))
			{
				disabledInstanceList.Add(creature);
			}
		}
	}

	private void VerifyRestoredSpawnCreaturesRelinked()
	{
		if (mMissingSpawnedCreatures.Count <= 0)
		{
			return;
		}
		for (int num = mMissingSpawnedCreatures.Count - 1; num > 0; num--)
		{
			GameObject objectByID = InstanceID.GetObjectByID(mMissingSpawnedCreatures[num]);
			if ((bool)objectByID && !spawnedList.Contains(objectByID))
			{
				spawnedList.Add(objectByID);
				Health component = objectByID.GetComponent<Health>();
				if ((bool)component)
				{
					component.OnDeath += SpawnDied;
				}
				SetupSpawnFaction(objectByID);
				Persistence component2 = objectByID.GetComponent<Persistence>();
				if ((bool)component2)
				{
					component2.Load();
				}
				if (CombatEndsWhenAllAreDead)
				{
					SetCombatEndsWhenAllAreDeadOnObject(objectByID);
				}
				mMissingSpawnedCreatures.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		if (m_restored && !spawned && !finished && SpawnOnStart)
		{
			Spawn();
		}
	}

	public void ForceSpawn()
	{
		finished = false;
		spawned = false;
		Spawn();
	}

	private void Spawn()
	{
		spawned = true;
		spawnedList.Clear();
		for (int i = 0; i < encounterList.Count; i++)
		{
			if (encounterList[i].Creature == null)
			{
				continue;
			}
			GameObject gameObject = null;
			bool flag = DoesSpawnAppear(encounterList[i].AppearsInLevelOfDifficulty);
			bool flag2 = encounterList[i].IsPrefab && !disabledInstanceList.Contains(encounterList[i].Creature);
			if (GameState.Stronghold.HasStoredCompanion(guidList[i]))
			{
				flag2 = false;
				flag = false;
			}
			if (flag && flag2)
			{
				Transform transform = base.transform;
				if (encounterList[i].SpawnPoint != null)
				{
					transform = encounterList[i].SpawnPoint.transform;
				}
				gameObject = InstanceID.GetObjectByID(guidList[i]);
				if (gameObject == null)
				{
					gameObject = UnityEngine.Object.Instantiate(encounterList[i].Creature, transform.position, transform.rotation);
				}
				InstanceID component = gameObject.GetComponent<InstanceID>();
				if ((bool)component)
				{
					component.Guid = guidList[i];
				}
				PartyMemberAI component2 = gameObject.GetComponent<PartyMemberAI>();
				if ((bool)component2 && !component2.IsActiveInParty)
				{
					Persistence component3 = gameObject.GetComponent<Persistence>();
					if ((bool)component3)
					{
						component3.Mobile = false;
					}
					Equipment component4 = gameObject.GetComponent<Equipment>();
					if ((bool)component4)
					{
						component4.m_shouldSaveEquipment = false;
					}
				}
				Persistence component5 = gameObject.GetComponent<Persistence>();
				if ((bool)component5 && (component2 == null || !component2.IsActiveInParty))
				{
					component5.Load();
				}
				RemoveDuplicatePartyMember(gameObject);
				GameUtilities.LaunchEffect(encounterList[i].SpawnVfx, 1f, gameObject.transform.position, null);
			}
			else if (flag)
			{
				gameObject = encounterList[i].Creature;
				gameObject.SetActive(value: true);
				GameUtilities.LaunchEffect(encounterList[i].SpawnVfx, 1f, gameObject.transform.position, null);
			}
			else if (!flag2 && !encounterList[i].IsPrefab)
			{
				Persistence component6 = encounterList[i].Creature.GetComponent<Persistence>();
				if ((bool)component6)
				{
					component6.SetForDestroy();
				}
				GameUtilities.Destroy(encounterList[i].Creature);
			}
			if (!gameObject)
			{
				continue;
			}
			spawnedList.Add(gameObject);
			Health component7 = gameObject.GetComponent<Health>();
			if ((bool)component7)
			{
				component7.OnDeath += SpawnDied;
			}
			if (ScaleChildrenWith != 0)
			{
				ScaledContent scaledContent = gameObject.GetComponent<ScaledContent>();
				if (!scaledContent)
				{
					CharacterStats component8 = gameObject.GetComponent<CharacterStats>();
					if ((bool)component8)
					{
						scaledContent = component8.AddScaledContentComponent();
					}
				}
				scaledContent.Scalers |= ScaleChildrenWith;
			}
			SetupSpawnFaction(gameObject);
			SetCombatEndsWhenAllAreDeadOnObject(gameObject);
			AlphaControl alphaControl = gameObject.GetComponent<AlphaControl>();
			if (alphaControl == null && gameObject.GetComponent<AIController>() != null)
			{
				alphaControl = gameObject.AddComponent<AlphaControl>();
			}
			if (alphaControl != null)
			{
				bool flag3 = FogOfWar.Instance.PointVisible(gameObject.transform.position);
				alphaControl.Alpha = (flag3 ? 1f : 0f);
				alphaControl.Refresh();
			}
		}
		if ((bool)m_ScriptEvent)
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnEncounterStart);
		}
	}

	private bool DoesSpawnAppear(DifficultySettings settings)
	{
		if (settings.RequiresAnyOf != 0 && !DifficultyScaling.Instance.IsAnyScalerActive(settings.RequiresAnyOf))
		{
			return false;
		}
		GameDifficulty difficulty = GameDifficulty.Normal;
		if (GameState.Instance.CurrentNextMap != null)
		{
			difficulty = GameState.Instance.CurrentNextMap.StoryTimeSpawnSetting;
		}
		else
		{
			Debug.LogWarning("CurrentNextMap was null in encounter spawning.", this);
		}
		GameDifficulty savedDifficulty = SavedDifficulty;
		if (savedDifficulty == GameDifficulty.StoryTime && settings.AppearsInBaseDifficulty(difficulty))
		{
			return true;
		}
		switch (savedDifficulty)
		{
		case GameDifficulty.PathOfTheDamned:
			return true;
		case GameDifficulty.Easy:
			if (settings.Easy)
			{
				return true;
			}
			break;
		}
		if (savedDifficulty == GameDifficulty.Normal && settings.Normal)
		{
			return true;
		}
		if (savedDifficulty == GameDifficulty.Hard && settings.Hard)
		{
			return true;
		}
		return false;
	}

	public void DeSpawn()
	{
		foreach (GameObject spawned in spawnedList)
		{
			PartyMemberAI component = spawned.GetComponent<PartyMemberAI>();
			if (component == null || !component.enabled)
			{
				PersistenceManager.RemoveObject(spawned.GetComponent<Persistence>());
				GameUtilities.Destroy(spawned);
			}
		}
		spawnedList.Clear();
		if ((bool)m_ScriptEvent)
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnEncounterEnd);
		}
	}

	public void OnDestroy()
	{
		GameState.OnCombatEnd -= GameState_OnCombatEnd;
	}

	private void SpawnDied(GameObject spawnedObject, GameEventArgs args)
	{
		for (int num = spawnedList.Count - 1; num >= 0; num--)
		{
			if (spawnedList[num] == null)
			{
				spawnedList.RemoveAt(num);
			}
		}
		spawnedList.Remove(spawnedObject);
		if (spawnedList.Count == 0 && !finished)
		{
			if (DelayEndEventForCombat)
			{
				GameState.OnCombatEnd += GameState_OnCombatEnd;
			}
			else if ((bool)m_ScriptEvent)
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnEncounterEnd);
			}
			finished = true;
		}
	}

	private void GameState_OnCombatEnd(object sender, EventArgs e)
	{
		GameState.OnCombatEnd -= GameState_OnCombatEnd;
		if ((bool)m_ScriptEvent)
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnEncounterEnd);
		}
	}

	private void SetupSpawnFaction(GameObject spawnedObject)
	{
		if (EncounterFaction != 0)
		{
			Faction faction = spawnedObject.GetComponent<Faction>();
			if (faction == null)
			{
				faction = spawnedObject.AddComponent<Faction>();
			}
			faction.ModifyToMatch(m_Faction);
		}
	}

	private void SetupEncounterFaction()
	{
		if (EncounterFaction == FactionSettings.DontDoAnything)
		{
			return;
		}
		m_Faction = GetComponent<Faction>();
		if (m_Faction == null)
		{
			m_Faction = base.gameObject.AddComponent<Faction>();
		}
		m_Faction.DrawSelectionCircle = false;
		m_Faction.ShowTooltips = false;
		Team currentTeamInstance = m_Faction.CurrentTeamInstance;
		if (currentTeamInstance == null && (EncounterFaction == FactionSettings.UseTeamFromEncountersFaction || EncounterFaction == FactionSettings.CreateNewTeamFromEncountersFaction))
		{
			Debug.LogWarning(StringUtility.Format("Encounter: {0} does not have a team assigned to it. Defaulting to Hostile", base.gameObject.name));
			EncounterFaction = FactionSettings.CreateNewHostileTeam;
		}
		if (EncounterFaction != FactionSettings.UseTeamFromEncountersFaction)
		{
			Team team = ScriptableObject.CreateInstance<Team>();
			if (currentTeamInstance != null)
			{
				team.DefaultRelationship = currentTeamInstance.DefaultRelationship;
				team.FriendlyTeamSet = currentTeamInstance.FriendlyTeamSet;
				team.HostileTeamSet = currentTeamInstance.HostileTeamSet;
				team.NeutralTeamSet = currentTeamInstance.NeutralTeamSet;
			}
			team.ScriptTag = base.name;
			team.Register();
			m_Faction.CurrentTeamInstance = team;
			if (EncounterFaction == FactionSettings.CreateNewHostileTeam)
			{
				m_Faction.CurrentTeamInstance.DefaultRelationship = Faction.Relationship.Hostile;
			}
			else if (EncounterFaction == FactionSettings.CreateNewNeutralTeam)
			{
				m_Faction.CurrentTeamInstance.DefaultRelationship = Faction.Relationship.Neutral;
			}
		}
	}

	private void RemoveDuplicatePartyMember(GameObject newObject)
	{
		if (!newObject.GetComponent<PartyMemberAI>())
		{
			return;
		}
		CompanionInstanceID component = newObject.GetComponent<CompanionInstanceID>();
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				CompanionInstanceID component2 = partyMemberAI.GetComponent<CompanionInstanceID>();
				if (component2 != null && component != null && component2.GetCompanionGuid() == component.GetCompanionGuid() && !partyMemberAI.AddedThroughScript)
				{
					GameObject obj = partyMemberAI.gameObject;
					PartyMemberAI.RemoveFromActiveParty(partyMemberAI, purgePersistencePacket: true);
					GameUtilities.Destroy(obj);
					InstanceID.AddSpecialObjectID(newObject, component.GetCompanionGuid());
					break;
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		foreach (EncounterData encounter in encounterList)
		{
			if (encounter.SpawnPoint != null)
			{
				Gizmos.DrawLine(base.transform.position, encounter.SpawnPoint.transform.position);
			}
			else if ((bool)encounter.Creature && encounter.Creature.activeInHierarchy)
			{
				Gizmos.DrawLine(base.transform.position, encounter.Creature.transform.position);
			}
		}
		Gizmos.DrawCube(base.transform.position, Vector3.one * 0.35f);
	}

	public List<GameObject> GetSpawnedList()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject spawned in spawnedList)
		{
			if (spawned != null)
			{
				list.Add(spawned);
			}
		}
		return list;
	}

	private void SetCombatEndsWhenAllAreDeadOnObject(GameObject newObject)
	{
		if (CombatEndsWhenAllAreDead && newObject != null)
		{
			AIController component = newObject.GetComponent<AIController>();
			if (component != null)
			{
				component.SetParentEncounter(this);
			}
		}
	}

	public void SetCombatEndsWhenAllAreDeadAll(bool set)
	{
		CombatEndsWhenAllAreDead = set;
		if (CombatEndsWhenAllAreDead)
		{
			for (int i = 0; i < spawnedList.Count; i++)
			{
				SetCombatEndsWhenAllAreDeadOnObject(spawnedList[i]);
			}
		}
	}
}
