using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PartySpawner : MonoBehaviour
{
	public string PlayerSlotString;

	public string Slot1String;

	public string Slot2String;

	public string Slot3String;

	public string Slot4String;

	public string Slot5String;

	public PartyMemberAI PlayerSlot;

	public PartyMemberAI Slot1;

	public PartyMemberAI Slot2;

	public PartyMemberAI Slot3;

	public PartyMemberAI Slot4;

	public PartyMemberAI Slot5;

	private void Awake()
	{
		GameUtilities.CreateInGameGlobalPrefabObject();
		if (GetComponent<LevelStartWrapperEnter>() == null)
		{
			base.gameObject.AddComponent<LevelStartWrapperEnter>();
		}
		if (GetComponent<LevelStartWrapperExit>() == null)
		{
			base.gameObject.AddComponent<LevelStartWrapperExit>();
		}
	}

	private bool PartyMemberExistsInScene(CompanionInstanceID[] companionIDs, CompanionInstanceID companionID)
	{
		foreach (CompanionInstanceID companionInstanceID in companionIDs)
		{
			if (companionInstanceID != companionID && companionInstanceID.GetCompanionGuid() == companionID.GetCompanionGuid())
			{
				return true;
			}
		}
		return false;
	}

	private void DebugHandleDuplicateCompanions(CompanionInstanceID[] companionIDs)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				CompanionInstanceID component = partyMemberAI.GetComponent<CompanionInstanceID>();
				if (component != null && PartyMemberExistsInScene(companionIDs, component) && !partyMemberAI.AddedThroughScript)
				{
					GameUtilities.Destroy(partyMemberAI.gameObject);
					Debug.Log("Party member " + component.gameObject.name + " already found in the scene. Removing member from party.");
				}
			}
		}
	}

	private void Start()
	{
		GameState.GameOver = false;
		GameState.PartyDead = false;
		if (GameState.s_playerCharacter != null || (PlayerSlot == null && string.IsNullOrEmpty(PlayerSlotString)) || GameState.LoadedGame)
		{
			return;
		}
		PartyMemberAI partyMemberAI = null;
		int num = 0;
		int selectedSlot = 0;
		List<PartyMemberAI> list = new List<PartyMemberAI>();
		SceneTransition sceneTransition = null;
		PartyMemberAI[] array = new PartyMemberAI[6]
		{
			(PlayerSlot != null) ? PlayerSlot : ((!string.IsNullOrEmpty(PlayerSlotString)) ? GameResources.LoadPrefab<PartyMemberAI>(PlayerSlotString, instantiate: false) : null),
			(Slot1 != null) ? Slot1 : ((!string.IsNullOrEmpty(Slot1String)) ? GameResources.LoadPrefab<PartyMemberAI>(Slot1String, instantiate: false) : null),
			(Slot2 != null) ? Slot2 : ((!string.IsNullOrEmpty(Slot2String)) ? GameResources.LoadPrefab<PartyMemberAI>(Slot2String, instantiate: false) : null),
			(Slot3 != null) ? Slot3 : ((!string.IsNullOrEmpty(Slot3String)) ? GameResources.LoadPrefab<PartyMemberAI>(Slot3String, instantiate: false) : null),
			(Slot4 != null) ? Slot4 : ((!string.IsNullOrEmpty(Slot4String)) ? GameResources.LoadPrefab<PartyMemberAI>(Slot4String, instantiate: false) : null),
			(Slot5 != null) ? Slot5 : ((!string.IsNullOrEmpty(Slot5String)) ? GameResources.LoadPrefab<PartyMemberAI>(Slot5String, instantiate: false) : null)
		};
		foreach (PartyMemberAI partyMemberAI2 in array)
		{
			if ((bool)partyMemberAI2)
			{
				partyMemberAI = Object.Instantiate(partyMemberAI2);
				Persistence component = partyMemberAI.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.ResetForLoad();
					component.Load();
				}
				partyMemberAI.AssignedSlot = num++;
				PartyMemberAI.AddToActiveParty(partyMemberAI.gameObject, fromScript: false);
				list.Add(partyMemberAI);
				partyMemberAI.gameObject.transform.rotation = base.transform.rotation;
			}
		}
		sceneTransition = StartPoint.FindClosestSceneTransition(base.transform.position);
		foreach (PartyMemberAI item in list)
		{
			if (sceneTransition != null)
			{
				item.transform.position = sceneTransition.GetMarkerPosition(item, reverse: true);
			}
			else
			{
				item.transform.position = item.CalculateFormationPosition(base.transform.position, ignoreSelection: true, out selectedSlot);
			}
			Vector3 position = item.transform.position;
			if (NavMesh.SamplePosition(position, out var hit, 100f, -1))
			{
				position = hit.position;
			}
			item.transform.position = position;
		}
		Resources.UnloadUnusedAssets();
	}
}
