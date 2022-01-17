using System;
using UnityEngine;

public class CompanionStrongholdNode : MonoBehaviour
{
	public CompanionNames.Companions Companion;

	public bool UseDuringAttack;

	public bool UseForAdventurer;

	[HideInInspector]
	public GameObject CompanionObj;

	private bool m_persistenceDestroyed;

	private void Start()
	{
		Stronghold stronghold = GameState.Stronghold;
		if ((bool)stronghold)
		{
			stronghold.AdventurersSpawnedInMap.Clear();
		}
		GameState.OnLevelLoaded += LevelLoaded;
	}

	private void LevelLoaded(object sender, EventArgs e)
	{
		PlaceCompanion();
	}

	public void PlaceCompanion()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (!(stronghold != null) || !(CompanionObj == null) || GlobalVariables.Instance.GetVariable(stronghold.AttackGlobalVariableName) > 0 != UseDuringAttack)
		{
			return;
		}
		foreach (StoredCharacterInfo companion in stronghold.GetCompanions())
		{
			if ((bool)companion && stronghold.IsAvailable(companion))
			{
				if (CompanionInstanceID.GetSpecialGuid(Companion) == companion.GUID)
				{
					ActivateNode(companion.GUID);
					break;
				}
				if (companion.NamedCompanion == CompanionNames.Companions.Invalid && UseForAdventurer && !stronghold.AdventurersSpawnedInMap.Contains(companion.GUID))
				{
					stronghold.AdventurersSpawnedInMap.Add(companion.GUID);
					ActivateNode(companion.GUID);
					break;
				}
			}
		}
	}

	public void RemoveCompanion()
	{
		GameUtilities.DestroyImmediate(CompanionObj);
		CompanionObj = null;
	}

	private void ActivateNode(Guid companionGuid)
	{
		GameObject gameObject = GameState.Stronghold.RestoreCompanionToNode(companionGuid, base.gameObject);
		PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
		if (component != null)
		{
			GameUtilities.Destroy(component);
		}
		NPCDialogue component2 = gameObject.GetComponent<NPCDialogue>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		Health component3 = gameObject.GetComponent<Health>();
		if (component3 != null)
		{
			component3.OnDeath += HandleOnDeath;
		}
		CompanionObj = gameObject;
	}

	private void HandleOnDeath(GameObject myObject, GameEventArgs args)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.DestroyStoredCompanionOnDeath(CompanionObj);
		}
		Persistence persistence = CompanionObj.AddComponent<Persistence>();
		persistence.Mobile = false;
		persistence.SetForDestroy();
		CompanionObj = null;
	}

	private void Update()
	{
		if ((bool)CompanionObj && !m_persistenceDestroyed)
		{
			m_persistenceDestroyed = true;
		}
	}

	private void OnDisable()
	{
		GameState.OnLevelLoaded -= LevelLoaded;
	}
}
