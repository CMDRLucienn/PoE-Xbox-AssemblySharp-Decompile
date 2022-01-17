using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[AddComponentMenu("Abilities/Summon")]
public class Summon : AttackBase
{
	public class SummonAsyncLoadRequest
	{
		public AssetBundleRequest BundleRequest;

		public string BundleFile;

		public Vector3 SpawnLocation;

		public Faction OwnerFaction;

		public GameObject OwnerObject;

		public SummonAsyncLoadRequest(AssetBundleRequest request, string file, Vector3 location, Faction ownerFaction)
		{
			BundleRequest = request;
			BundleFile = file;
			SpawnLocation = location;
			OwnerFaction = ownerFaction;
		}
	}

	public AIController.AISummonType SummonType = AIController.AISummonType.Summoned;

	public List<string> SummonFileList = new List<string>();

	public List<CharacterStats> SummonList = new List<CharacterStats>();

	protected List<SummonAsyncLoadRequest> m_summonAsyncLoadRequests = new List<SummonAsyncLoadRequest>();

	public bool LifetimeExternallyManaged;

	[Persistent(Persistent.ConversionType.GUIDLink)]
	private List<Health> m_summons = new List<Health>();

	[Persistent(Persistent.ConversionType.GUIDLink)]
	private GameObject m_summoner;

	[HideInInspector]
	[Persistent]
	public string CreatureName = "";

	public GameObject OnSummonVisualEffect;

	public GameObject OnDesummonVisualEffect;

	public bool ForceIntoOwnerGroup = true;

	public bool SummonCopyOfSelf;

	public Equippable EquippablePrefab;

	public Equippable EquippablePrefab2;

	public float Duration;

	private bool m_performSummoning;

	private Vector3 m_summonLocation;

	private Health m_ownerHealth;

	[Persistent]
	private float m_DurationTimer { get; set; }

	public bool DestroyAfterSummonEnds { get; set; }

	public int NumActiveSummons => m_summons.Count;

	protected override void Start()
	{
		base.Start();
		RequiresHitObject = false;
		GameState.OnCombatEnd += HandleSummonOnCombatEnd;
	}

	public void OnDisable()
	{
		DeactivateSummons(deactivateAbility: false);
	}

	protected override void OnDestroy()
	{
		m_performSummoning = false;
		DestroyAsyncRequests();
		GameState.OnCombatEnd -= HandleSummonOnCombatEnd;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public GameObject GetFirstSummon()
	{
		foreach (CharacterStats summon in SummonList)
		{
			if (!(summon == null))
			{
				return summon.gameObject;
			}
		}
		foreach (string summonFile in SummonFileList)
		{
			CharacterStats characterStats = GameResources.LoadPrefab<CharacterStats>(Path.GetFileNameWithoutExtension(summonFile), instantiate: false);
			if (!(characterStats == null))
			{
				return characterStats.gameObject;
			}
		}
		return null;
	}

	public int GetNumSummons()
	{
		return SummonList.Count + SummonFileList.Count;
	}

	private void HandleSummonOnCombatEnd(object sender, EventArgs e)
	{
		DestroyAllSummoned();
		CheckDestroy();
	}

	private void CheckDestroy()
	{
		if (DestroyAfterSummonEnds)
		{
			GameUtilities.Destroy(base.gameObject, 1f);
		}
	}

	public override void Update()
	{
		if (m_performSummoning)
		{
			m_performSummoning = false;
			Faction component = base.Owner.GetComponent<Faction>();
			PerformSummoning(m_summonLocation, component);
		}
		if (SummonType == AIController.AISummonType.AnimalCompanion && (bool)base.Owner && m_summons.Count == 1 && m_summons[0] == null && (bool)base.Owner.GetComponent<PartyMemberAI>())
		{
			FixSummoning();
		}
		UpdateAsyncSpawn();
		base.Update();
		if (LifetimeExternallyManaged || m_summons.Count == 0)
		{
			return;
		}
		bool flag = false;
		for (int num = m_summons.Count - 1; num >= 0; num--)
		{
			if (m_summons[num] == null)
			{
				m_summons.RemoveAt(num);
				flag = true;
			}
			else if (m_summons[num].Dead && m_summons[num].ShouldDecay)
			{
				AIController component2 = base.Owner.GetComponent<AIController>();
				if ((bool)component2)
				{
					component2.SummonedCreatureList.Remove(m_summons[num].gameObject);
					if ((bool)base.Owner.GetComponent<PartyMemberAI>())
					{
						PartyMemberAI component3 = m_summons[num].GetComponent<PartyMemberAI>();
						if ((bool)component3)
						{
							PartyMemberAI.RemoveFromActiveParty(component3, purgePersistencePacket: true);
						}
					}
				}
				m_summons.RemoveAt(num);
				flag = true;
			}
		}
		if (m_summons.Count == 0 && flag)
		{
			DeactivateSummons(deactivateAbility: true);
			CheckDestroy();
		}
		if (SummonType == AIController.AISummonType.Summoned && m_DurationTimer > 0f)
		{
			m_DurationTimer -= Time.deltaTime;
			if (m_DurationTimer <= 0f)
			{
				m_DurationTimer = 0f;
				DestroyAllSummoned();
				CheckDestroy();
			}
		}
		if ((bool)m_ownerHealth && m_ownerHealth.Dead)
		{
			DestroyAllSummoned();
			CheckDestroy();
		}
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		Launch(enemy.transform.position + enemy.transform.forward, enemy, variationOverride);
		return enemy;
	}

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		base.OnImpact(self, enemy);
		OnImpact(self, enemy.transform.position);
	}

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		base.OnImpact(self, enemy, isMainTarget);
		OnImpact(self, enemy.transform.position);
	}

	public void InitiateSummoning(Vector3 location)
	{
		if (m_performSummoning)
		{
			return;
		}
		m_summoner = base.Owner;
		if (base.Owner.GetComponent<Faction>() == null)
		{
			GenericAbility component = GetComponent<GenericAbility>();
			if (component != null)
			{
				component.Owner.GetComponent<Faction>();
				m_summoner = component.Owner;
			}
		}
		if ((bool)m_summoner)
		{
			m_ownerHealth = m_summoner.GetComponent<Health>();
		}
		DestroyAllSummoned();
		m_performSummoning = true;
		m_summonLocation = location;
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		base.OnImpact(self, hitPosition);
		InitiateSummoning(hitPosition);
	}

	private void DestroyAllSummoned()
	{
		m_performSummoning = false;
		if (SummonType != AIController.AISummonType.Summoned || !(m_summoner != null))
		{
			return;
		}
		AIController aIController = GameUtilities.FindActiveAIController(m_summoner.gameObject);
		if (aIController == null)
		{
			return;
		}
		for (int num = aIController.SummonedCreatureList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = aIController.SummonedCreatureList[num];
			if (gameObject == null)
			{
				aIController.SummonedCreatureList.RemoveAt(num);
			}
			else
			{
				AIController aIController2 = GameUtilities.FindActiveAIController(gameObject);
				if (aIController2 != null && aIController2.SummonType == SummonType)
				{
					aIController.SummonedCreatureList.RemoveAt(num);
					Health component = gameObject.GetComponent<Health>();
					if (component != null)
					{
						m_summons.Remove(component);
					}
					if (aIController is PartyMemberAI)
					{
						PartyMemberAI component2 = gameObject.GetComponent<PartyMemberAI>();
						if ((bool)component2)
						{
							PartyMemberAI.RemoveFromActiveParty(component2, purgePersistencePacket: true);
						}
					}
					GameUtilities.LaunchEffect(OnDesummonVisualEffect, 1f, gameObject.transform.position, m_ability);
					GameUtilities.Destroy(gameObject);
				}
			}
		}
	}

	protected virtual void PerformSummoning(Vector3 location, Faction ownerFaction)
	{
		if (SummonAlreadyActive())
		{
			return;
		}
		foreach (CharacterStats summon in SummonList)
		{
			if (!(summon == null))
			{
				SummonCreature(summon, location, ownerFaction);
			}
		}
		foreach (string summonFile in SummonFileList)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(summonFile);
			if (GameResources.IsPrefabLoaded(fileNameWithoutExtension))
			{
				CharacterStats characterStats = GameResources.LoadPrefab<CharacterStats>(fileNameWithoutExtension, instantiate: false);
				if (!(characterStats == null))
				{
					SummonCreature(characterStats, location, ownerFaction);
				}
				continue;
			}
			AssetBundleRequest assetBundleRequest = GameResources.LoadPrefabAsync<CharacterStats>(fileNameWithoutExtension);
			if (assetBundleRequest != null)
			{
				SummonAsyncLoadRequest item = new SummonAsyncLoadRequest(assetBundleRequest, fileNameWithoutExtension, location, ownerFaction);
				m_summonAsyncLoadRequests.Add(item);
			}
		}
	}

	protected virtual void SummonCreature(CharacterStats character, Vector3 location, Faction ownerFaction)
	{
		if ((SummonType == AIController.AISummonType.Summoned && (!GameState.InCombat || GameState.IsInTrapTriggeredCombat)) || SummonAlreadyActive())
		{
			return;
		}
		AIController[] array = null;
		AIController aIController = null;
		if (ownerFaction != null)
		{
			array = ownerFaction.gameObject.GetComponents<AIController>();
		}
		if (array != null)
		{
			AIController[] array2 = array;
			foreach (AIController aIController2 in array2)
			{
				if (aIController2.gameObject.activeInHierarchy)
				{
					aIController = aIController2;
					break;
				}
			}
		}
		if (m_summoner != null)
		{
			CharacterStats component = m_summoner.GetComponent<CharacterStats>();
			if (component != null && (component.HasStatusEffectFromAffliction(AfflictionData.Charmed) || component.HasStatusEffectFromAffliction(AfflictionData.Dominated) || (aIController != null && aIController.IsConfused)))
			{
				return;
			}
		}
		CharacterStats characterStats = null;
		bool flag = true;
		Persistence component2 = m_summoner.GetComponent<Persistence>();
		bool flag2 = true;
		if ((component2 != null && !component2.Mobile) || component2 == null)
		{
			LifetimeExternallyManaged = false;
			flag2 = false;
		}
		if (LifetimeExternallyManaged)
		{
			GameObject objectByID = InstanceID.GetObjectByID(character.gameObject);
			if ((bool)objectByID)
			{
				characterStats = objectByID.GetComponent<CharacterStats>();
				flag = false;
			}
		}
		if (characterStats == null)
		{
			if (SummonCopyOfSelf)
			{
				characterStats = UnityEngine.Object.Instantiate(character);
				MirrorCharacterUtils.MirrorAppearance(characterStats.gameObject, m_summoner);
				flag2 = false;
			}
			else
			{
				characterStats = GameResources.Instantiate<CharacterStats>(character);
				if (SummonType == AIController.AISummonType.Pet)
				{
					InstanceID component3 = characterStats.GetComponent<InstanceID>();
					if ((bool)component3)
					{
						component3.Guid = character.GetComponent<InstanceID>().Guid;
					}
				}
			}
		}
		if (LifetimeExternallyManaged)
		{
			GameState.PersistAcrossSceneLoadsTracked(characterStats.gameObject);
		}
		if (!string.IsNullOrEmpty(CreatureName))
		{
			characterStats.OverrideName = CreatureName;
		}
		if (SummonCopyOfSelf)
		{
			characterStats.OverrideName = GUIUtils.Format(1373, m_summoner.GetComponent<CharacterStats>().Name());
		}
		Vector3 position = location;
		m_DurationTimer = Duration;
		m_summons.Add(characterStats.GetComponent<Health>());
		if (flag)
		{
			Mover component4 = characterStats.GetComponent<Mover>();
			location.y = m_summoner.transform.position.y;
			if (component4 != null)
			{
				characterStats.transform.position = GameUtilities.NearestUnoccupiedLocation(location, component4.Radius, 10f, component4);
			}
			else
			{
				characterStats.transform.position = GameUtilities.NearestUnoccupiedLocation(location, 0.5f, 10f, null);
			}
			characterStats.transform.position = new Vector3(characterStats.transform.position.x, m_summoner.transform.position.y, characterStats.transform.position.z);
			position = characterStats.transform.position;
			if (SummonType == AIController.AISummonType.AnimalCompanion && m_summoner != null)
			{
				characterStats.transform.rotation = m_summoner.transform.rotation;
			}
		}
		GameUtilities.LaunchEffect(OnSummonVisualEffect, 1f, position, m_ability);
		if (ForceIntoOwnerGroup)
		{
			Faction component5 = characterStats.GetComponent<Faction>();
			if (component5 != null && ownerFaction != null)
			{
				component5.ModifyToMatch(ownerFaction);
			}
			SharedStats component6 = characterStats.GetComponent<SharedStats>();
			if ((bool)component6)
			{
				component6.SharedCharacter = m_summoner;
				Health component7 = characterStats.GetComponent<Health>();
				Health component8 = m_summoner.GetComponent<Health>();
				component7.ShouldDecay = component8.ShouldDecay;
			}
			AIPackageController component9 = characterStats.GetComponent<AIPackageController>();
			PartyMemberAI component10 = characterStats.GetComponent<PartyMemberAI>();
			PartyMemberAI component11 = m_summoner.GetComponent<PartyMemberAI>();
			AIPackageController component12 = m_summoner.GetComponent<AIPackageController>();
			bool flag3 = component11 != null && component11.enabled;
			if (Cutscene.CutsceneActive && m_summoner == GameState.s_playerCharacter.gameObject)
			{
				flag3 = true;
			}
			bool flag4 = false;
			if (component9 != null)
			{
				flag4 = component9.AIPackage == AIPackageController.PackageType.Pet;
			}
			if (!flag4 && SummonType == AIController.AISummonType.AnimalCompanion && !flag3 && component12 != null)
			{
				component12.SummonedCreatureList.Add(characterStats.gameObject);
			}
			if (!flag4 && flag3)
			{
				PartyMemberAI.AddSummonToActiveParty(characterStats.gameObject, m_summoner, SummonType, fromScript: false);
			}
			else if (flag4 && flag3)
			{
				component11.SummonedCreatureList.Add(characterStats.gameObject);
			}
			else if (!flag4 && !flag3 && component10 != null)
			{
				GameUtilities.Destroy(component10);
			}
		}
		AIController aIController3 = GameUtilities.FindActiveAIController(characterStats.gameObject);
		if ((bool)aIController3)
		{
			if (m_summoner != null)
			{
				AIController component13 = m_summoner.GetComponent<AIController>();
				if (component13 != null)
				{
					aIController3.MustDieForCombatToEnd = component13.MustDieForCombatToEnd;
				}
			}
			if (ForceIntoOwnerGroup)
			{
				aIController3.Summoner = base.Owner;
				aIController3.SummonType = SummonType;
			}
			aIController3.enabled = true;
			aIController3.StateManager.AbortStateStack();
			aIController3.InitAI();
		}
		if (EquippablePrefab != null && m_summons.Count == 1)
		{
			Equipment component14 = characterStats.GetComponent<Equipment>();
			if (component14 != null)
			{
				component14.DefaultEquippedItems.PrimaryWeapon = EquippablePrefab;
			}
		}
		if (EquippablePrefab2 != null && m_summons.Count == 2)
		{
			Equipment component15 = characterStats.GetComponent<Equipment>();
			if (component15 != null)
			{
				component15.DefaultEquippedItems.PrimaryWeapon = EquippablePrefab2;
			}
		}
		if (SummonType == AIController.AISummonType.Summoned)
		{
			Persistence component16 = characterStats.GetComponent<Persistence>();
			if ((bool)component16)
			{
				GameUtilities.DestroyImmediate(component16);
			}
			Persistence[] componentsInChildren = characterStats.GetComponentsInChildren<Persistence>();
			if (componentsInChildren != null)
			{
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					GameUtilities.DestroyImmediate(componentsInChildren[j]);
				}
			}
		}
		Persistence component17 = characterStats.GetComponent<Persistence>();
		if (component17 != null && component17.Mobile)
		{
			component17.Mobile = flag2;
			if (!flag2)
			{
				GameUtilities.DestroyImmediate(component17);
				Persistence[] componentsInChildren2 = characterStats.GetComponentsInChildren<Persistence>();
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					GameUtilities.DestroyImmediate(componentsInChildren2[k]);
				}
			}
		}
		if (SummonType != AIController.AISummonType.AnimalCompanion)
		{
			Health component18 = characterStats.GetComponent<Health>();
			if ((bool)component18)
			{
				component18.ShouldDecay = true;
			}
		}
		if (aIController != null)
		{
			aIController.UpdateAggressionOfSummonedCreatures(includeCompanion: false);
		}
	}

	public void DeactivateSummons(bool deactivateAbility)
	{
		m_performSummoning = false;
		Cancel();
		DestroyAsyncRequests();
		for (int num = m_summons.Count - 1; num >= 0; num--)
		{
			if ((bool)m_summons[num])
			{
				GameObject gameObject = m_summons[num].gameObject;
				if ((bool)gameObject)
				{
					PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
					if ((bool)component && component.enabled && SummonType == AIController.AISummonType.AnimalCompanion)
					{
						continue;
					}
				}
				if ((bool)m_summoner)
				{
					PartyMemberAI component2 = m_summoner.GetComponent<PartyMemberAI>();
					AIPackageController component3 = m_summoner.GetComponent<AIPackageController>();
					AIController aIController = null;
					if ((bool)component3)
					{
						aIController = component3;
					}
					else if ((bool)component2)
					{
						aIController = component2;
					}
					if ((bool)aIController)
					{
						aIController.SummonedCreatureList.Remove(gameObject);
					}
				}
				GameUtilities.LaunchEffect(OnDesummonVisualEffect, 1f, gameObject.transform.position, m_ability);
				Persistence component4 = gameObject.GetComponent<Persistence>();
				if ((bool)component4)
				{
					ObjectPersistencePacket packet = PersistenceManager.GetPacket(component4);
					if (packet != null && !packet.Packed)
					{
						PersistenceManager.RemoveObject(component4);
					}
				}
				GameUtilities.Destroy(gameObject);
			}
			m_summons.RemoveAt(num);
		}
		if (deactivateAbility)
		{
			GenericAbility component5 = GetComponent<GenericAbility>();
			if (component5 != null)
			{
				component5.Deactivate(base.Owner);
			}
		}
	}

	private void UpdateAsyncSpawn()
	{
		for (int num = m_summonAsyncLoadRequests.Count - 1; num >= 0; num--)
		{
			if (m_summonAsyncLoadRequests[num] != null && m_summonAsyncLoadRequests[num].BundleRequest != null && m_summonAsyncLoadRequests[num].BundleRequest.isDone)
			{
				CharacterStats characterStats = GameResources.LoadPrefabFromAsyncRequest<CharacterStats>(m_summonAsyncLoadRequests[num].BundleFile, m_summonAsyncLoadRequests[num].BundleRequest, instantiate: false);
				Vector3 spawnLocation = m_summonAsyncLoadRequests[num].SpawnLocation;
				Faction ownerFaction = m_summonAsyncLoadRequests[num].OwnerFaction;
				m_summonAsyncLoadRequests.RemoveAt(num);
				if (!(characterStats == null))
				{
					SummonCreature(characterStats, spawnLocation, ownerFaction);
				}
			}
		}
	}

	private bool SummonAlreadyActive()
	{
		if (SummonType == AIController.AISummonType.AnimalCompanion)
		{
			AIController aIController = GameUtilities.FindActiveAIController(m_summoner);
			if ((bool)aIController)
			{
				foreach (GameObject summonedCreature in aIController.SummonedCreatureList)
				{
					AIController aIController2 = GameUtilities.FindActiveAIController(summonedCreature);
					if ((bool)aIController2 && aIController2.SummonType == AIController.AISummonType.AnimalCompanion)
					{
						Health component = summonedCreature.GetComponent<Health>();
						if ((bool)component && !m_summons.Contains(component))
						{
							m_summons.Add(component);
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	private void DestroyAsyncRequests()
	{
		for (int num = m_summonAsyncLoadRequests.Count - 1; num >= 0; num--)
		{
			if (m_summonAsyncLoadRequests[num] != null && m_summonAsyncLoadRequests[num].BundleRequest != null)
			{
				GameResources.StopPrefabAsyncRequest(m_summonAsyncLoadRequests[num].BundleFile, m_summonAsyncLoadRequests[num].BundleRequest);
			}
		}
		m_summonAsyncLoadRequests.Clear();
	}

	public void FixSummoning()
	{
		m_summons.Clear();
		m_performSummoning = true;
		if ((bool)base.Owner)
		{
			m_summonLocation = base.Owner.transform.position;
		}
	}

	public override string GetDurationString(GenericAbility ability)
	{
		string text = base.GetDurationString(ability);
		if (Duration > 0f)
		{
			text = text + "\n" + AttackBase.FormatWC(GUIUtils.GetText(1634), GUIUtils.Format(211, Duration.ToString("#0")));
		}
		return text.Trim();
	}

	public string GetSummonListString(GameObject character)
	{
		if (SummonCopyOfSelf)
		{
			return GUIUtils.Format(1373, character.GetComponent<CharacterStats>().Name());
		}
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<CharacterStats, int> dictionary = new Dictionary<CharacterStats, int>();
		for (int i = 0; i < SummonList.Count; i++)
		{
			if ((bool)SummonList[i])
			{
				if (dictionary.ContainsKey(SummonList[i]))
				{
					dictionary[SummonList[i]]++;
				}
				else
				{
					dictionary[SummonList[i]] = 1;
				}
			}
		}
		for (int j = 0; j < SummonFileList.Count; j++)
		{
			if (string.IsNullOrEmpty(SummonFileList[j]))
			{
				continue;
			}
			UnityEngine.Object @object = GameResources.LoadPrefab(SummonFileList[j], instantiate: false);
			CharacterStats characterStats = null;
			if (@object is MonoBehaviour)
			{
				characterStats = (@object as MonoBehaviour).GetComponent<CharacterStats>();
			}
			else if (@object is GameObject)
			{
				characterStats = (@object as GameObject).GetComponent<CharacterStats>();
			}
			if ((bool)characterStats)
			{
				if (dictionary.ContainsKey(characterStats))
				{
					dictionary[characterStats]++;
				}
				else
				{
					dictionary[characterStats] = 1;
				}
			}
		}
		foreach (KeyValuePair<CharacterStats, int> item in dictionary)
		{
			if (item.Value > 1)
			{
				stringBuilder.Append(GUIUtils.Format(1625, item.Value, CharacterStats.Name(item.Key)));
			}
			else
			{
				stringBuilder.Append(CharacterStats.Name(item.Key));
			}
			stringBuilder.Append(GUIUtils.Comma());
		}
		if (stringBuilder.Length >= GUIUtils.Comma().Length)
		{
			stringBuilder = stringBuilder.Remove(stringBuilder.Length - GUIUtils.Comma().Length);
		}
		return stringBuilder.ToString();
	}

	public virtual void AddSummonEffects(StringEffects stringEffects, GameObject character)
	{
		AddStringEffect(GUIUtils.GetText(1539), GetSummonListString(character), hostile: false, stringEffects);
	}

	public override void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		AddSummonEffects(stringEffects, character);
		base.GetAdditionalEffects(stringEffects, ability, character);
	}
}
