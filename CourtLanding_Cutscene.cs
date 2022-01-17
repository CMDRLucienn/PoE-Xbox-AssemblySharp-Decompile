using System.Collections;
using UnityEngine;

public class CourtLanding_Cutscene : BasePuppetScript
{
	public GameObject PlayerVFX;

	public GameObject PartyVFX;

	public GameObject MagnetPrefab;

	public GameObject[] MagneticSouls;

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
		yield return new WaitForSeconds(0.1f);
		GameObject[] realParty = RealParty;
		foreach (GameObject gameObject in realParty)
		{
			if (!(gameObject == null) && !(gameObject == RealPlayer))
			{
				GameUtilities.LaunchEffect(PartyVFX, 1f, gameObject.transform, null);
			}
		}
		yield return new WaitForSeconds(0.2f);
		int num = 0;
		for (int j = 0; j < MagneticSouls.Length; j++)
		{
			if (!(MagneticSouls[j] != null))
			{
				continue;
			}
			Persistence component = MagneticSouls[j].GetComponent<Persistence>();
			if ((bool)component)
			{
				if (num < RealParty.Length && RealParty[num] == RealPlayer)
				{
					num++;
				}
				if (num < RealParty.Length && RealParty[num] != null)
				{
					MagneticSouls[j].transform.position = RealParty[num].transform.position;
				}
				Scripts.ActivateObject(component.GUID, activate: true);
				num++;
			}
		}
		yield return new WaitForSeconds(0.1f);
		GameUtilities.LaunchEffect(PlayerVFX, 1f, RealPlayer.transform, null);
		EndScene();
	}
}
