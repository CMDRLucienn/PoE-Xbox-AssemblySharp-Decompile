using System;
using System.Collections.Generic;
using UnityEngine;

public class StoredCharacterInfo : MonoBehaviour
{
	public Guid DebugGuid;

	private Coroutine m_textureLoadCoroutine;

	[Persistent]
	private string m_portraitSmallPath;

	public static bool RestoringPackedCharacter { get; set; }

	[Persistent]
	public List<Guid> AttachedObjects { get; set; }

	[Persistent]
	public CompanionNames.Companions NamedCompanion { get; set; }

	[Persistent]
	public Guid GUID { get; set; }

	[Persistent]
	public string DisplayName { get; set; }

	public Texture2D SmallPortrait { get; set; }

	[Persistent]
	public Guid AnimalCompanionGUID { get; set; }

	public string PortraitSmallPath
	{
		get
		{
			return m_portraitSmallPath;
		}
		set
		{
			m_portraitSmallPath = value;
			LoadSmallPortraitFromPath();
		}
	}

	[Persistent]
	public bool HasRested { get; set; }

	public bool IsAdventurer => NamedCompanion == CompanionNames.Companions.Invalid;

	public int RestoreSlot { get; set; }

	public int Experience
	{
		get
		{
			object savedValue = PersistenceManager.GetSavedValue(GUID, typeof(CharacterStats), "Experience");
			if (savedValue == null)
			{
				return -1;
			}
			return (int)savedValue;
		}
		set
		{
			PersistenceManager.ModifySavedValue(GUID, typeof(CharacterStats), "Experience", value);
		}
	}

	public int Level
	{
		get
		{
			object savedValue = PersistenceManager.GetSavedValue(GUID, typeof(CharacterStats), "Level");
			if (savedValue == null)
			{
				return -1;
			}
			return (int)savedValue;
		}
		set
		{
			PersistenceManager.ModifySavedValue(GUID, typeof(CharacterStats), "Level", value);
		}
	}

	public int Class
	{
		get
		{
			object savedValue = PersistenceManager.GetSavedValue(GUID, typeof(CharacterStats), "CharacterClass");
			if (savedValue == null)
			{
				return 1;
			}
			return (int)savedValue;
		}
		set
		{
			PersistenceManager.ModifySavedValue(GUID, typeof(CharacterStats), "CharacterClass", value);
		}
	}

	public Gender Gender
	{
		get
		{
			object savedValue = PersistenceManager.GetSavedValue(GUID, typeof(CharacterStats), "Gender");
			if (savedValue == null)
			{
				return Gender.Male;
			}
			return (Gender)savedValue;
		}
		set
		{
			PersistenceManager.ModifySavedValue(GUID, typeof(CharacterStats), "Gender", value);
		}
	}

	public void Restored()
	{
		LoadSmallPortraitFromPath();
	}

	private void LoadSmallPortraitFromPath()
	{
		if (!string.IsNullOrEmpty(m_portraitSmallPath))
		{
			if (m_textureLoadCoroutine != null)
			{
				StopCoroutine(m_textureLoadCoroutine);
			}
			m_textureLoadCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(m_portraitSmallPath, SmallPortraitTextureLoaded));
		}
	}

	private void SmallPortraitTextureLoaded(Texture2D loadedTexture)
	{
		SmallPortrait = loadedTexture;
		m_textureLoadCoroutine = null;
	}

	public static StoredCharacterInfo ConvertCharacterToStored(GameObject character)
	{
		GameObject obj = new GameObject(character.name + "_stored");
		obj.AddComponent<InstanceID>().Guid = Guid.NewGuid();
		obj.AddComponent<Persistence>().Mobile = true;
		StoredCharacterInfo storedCharacterInfo = obj.AddComponent<StoredCharacterInfo>();
		GameState.PersistAcrossSceneLoadsTracked(obj);
		PartyMemberAI component = character.GetComponent<PartyMemberAI>();
		if ((bool)component)
		{
			PartyMemberAI.RemoveFromActiveParty(component, purgePersistencePacket: true);
		}
		storedCharacterInfo.SetStoredObject(character);
		PersistenceManager.SaveAndDestroyObject(character);
		return storedCharacterInfo;
	}

	public GameObject RestoreCharacter(bool keepPacked)
	{
		bool loadedGame = GameState.LoadedGame;
		bool isRestoredLevel = GameState.IsRestoredLevel;
		GameState.LoadedGame = true;
		GameState.IsRestoredLevel = true;
		RestoringPackedCharacter = true;
		GameObject gameObject = PersistenceManager.RestorePackedObject(GUID);
		if (gameObject != null)
		{
			foreach (Guid attachedObject in AttachedObjects)
			{
				GameObject gameObject2 = PersistenceManager.RestorePackedObject(attachedObject);
				if (gameObject2 == null)
				{
					Debug.LogError("Unable to restore an attached object from " + base.name);
					continue;
				}
				Persistence component = gameObject2.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.ImmediateRestore = true;
					component.Load();
					component.ImmediateRestore = false;
					if (keepPacked)
					{
						PersistenceManager.GetPacket(component).Packed = true;
					}
				}
			}
			Persistence component2 = gameObject.GetComponent<Persistence>();
			if ((bool)component2)
			{
				component2.ImmediateRestore = true;
				component2.Load();
				component2.ImmediateRestore = false;
				if (keepPacked)
				{
					PersistenceManager.GetPacket(component2).Packed = true;
				}
			}
		}
		GameState.LoadedGame = loadedGame;
		GameState.IsRestoredLevel = isRestoredLevel;
		RestoringPackedCharacter = false;
		if (!keepPacked)
		{
			GameState.Stronghold.RestoreAnimalCompanion(gameObject, AnimalCompanionGUID);
		}
		Health component3 = gameObject.GetComponent<Health>();
		if ((bool)component3)
		{
			component3.CurrentStamina = component3.MaxStamina;
		}
		return gameObject;
	}

	public void SetStoredObject(GameObject obj)
	{
		InstanceID component = obj.GetComponent<InstanceID>();
		if ((bool)component)
		{
			GUID = component.Guid;
		}
		DebugGuid = component.Guid;
		DisplayName = CharacterStats.Name(obj);
		Portrait component2 = obj.GetComponent<Portrait>();
		if ((bool)component2)
		{
			PortraitSmallPath = component2.TextureSmallPath;
		}
		if (component is CompanionInstanceID)
		{
			NamedCompanion = (component as CompanionInstanceID).Companion;
		}
		else
		{
			NamedCompanion = CompanionNames.Companions.Invalid;
		}
		StoreChildGuids(obj);
		CharacterStats component3 = obj.GetComponent<CharacterStats>();
		if ((bool)component3)
		{
			component3.ClearAllStatusEffects();
		}
	}

	public void StoreChildGuids(GameObject obj)
	{
		Persistence[] componentsInChildren = obj.GetComponentsInChildren<Persistence>();
		AttachedObjects = new List<Guid>();
		AIController component = obj.GetComponent<AIController>();
		if ((bool)component && component.SummonedCreatureList.Count > 0)
		{
			foreach (GameObject summonedCreature in component.SummonedCreatureList)
			{
				AIController component2 = summonedCreature.GetComponent<AIController>();
				if ((bool)component2 && component2.SummonType == AIController.AISummonType.AnimalCompanion)
				{
					InstanceID component3 = summonedCreature.GetComponent<InstanceID>();
					AnimalCompanionGUID = component3.Guid;
					StoredCharacterInfo companion = ConvertCharacterToStored(summonedCreature);
					GameState.Stronghold.StoreAnimalCompanion(companion);
					break;
				}
			}
			component.SummonedCreatureList.Clear();
		}
		Persistence[] array = componentsInChildren;
		foreach (Persistence persistence in array)
		{
			if (persistence.gameObject == obj || persistence.gameObject == null)
			{
				continue;
			}
			Consumable component4 = persistence.gameObject.GetComponent<Consumable>();
			if (!(component4 != null) || !(component4.m_originalItem != null))
			{
				AttachedObjects.Add(persistence.GUID);
				GenericAbility component5 = persistence.GetComponent<GenericAbility>();
				if (component5 != null && (component5.Passive || component5.Modal) && component5.Activated)
				{
					bool hideFromCombatLog = component5.HideFromCombatLog;
					component5.HideFromCombatLog = true;
					component5.ForceDeactivate(persistence.gameObject);
					component5.HideFromCombatLog = hideFromCombatLog;
				}
				Summon component6 = persistence.GetComponent<Summon>();
				if ((bool)component6)
				{
					component6.DeactivateSummons(deactivateAbility: false);
				}
				PersistenceManager.SaveAndDestroyObject(persistence.gameObject);
			}
		}
	}
}
