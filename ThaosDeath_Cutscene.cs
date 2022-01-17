using System.Collections;
using UnityEngine;

public class ThaosDeath_Cutscene : BasePuppetScript
{
	public static GameObject MyFindPet(GameObject owner)
	{
		if (owner == null)
		{
			return null;
		}
		AIController component = owner.GetComponent<AIController>();
		if ((bool)component)
		{
			foreach (GameObject summonedCreature in component.SummonedCreatureList)
			{
				if (summonedCreature != null)
				{
					AIController component2 = summonedCreature.GetComponent<AIController>();
					if (component2 != null && component2.SummonType == AIController.AISummonType.AnimalCompanion)
					{
						return summonedCreature;
					}
				}
			}
		}
		return null;
	}

	private void MyPopulatePartyList()
	{
		int num = 1;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				if ((bool)partyMemberAI.GetComponent<Player>())
				{
					RealPlayer = partyMemberAI.gameObject;
					RealParty[0] = RealPlayer;
				}
				else if (partyMemberAI.Slot < 6)
				{
					RealParty[num] = partyMemberAI.gameObject;
					num++;
				}
			}
		}
	}

	public override IEnumerator RunScript()
	{
		MyPopulatePartyList();
		GameObject[] realParty = RealParty;
		foreach (GameObject gameObject in realParty)
		{
			if (gameObject == null)
			{
				continue;
			}
			Persistence component = gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				Scripts.RemoveAffliction(component.GUID, "Dominated");
				Scripts.RemoveAffliction(component.GUID, "Confused");
			}
			GameObject gameObject2 = MyFindPet(gameObject);
			if ((bool)gameObject2)
			{
				Persistence component2 = gameObject2.GetComponent<Persistence>();
				if ((bool)component2)
				{
					Scripts.RemoveAffliction(component2.GUID, "Dominated");
					Scripts.RemoveAffliction(component2.GUID, "Confused");
				}
			}
		}
		EndScene();
		yield return null;
	}
}
