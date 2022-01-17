using UnityEngine;

public class UIPartyMemberInventorySpace : UIParentSelectorListener
{
	public UILabel FreeSpace;

	public GameObject Full;

	public GameObject Free;

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		PartyMemberAI partyMemberAI = (stats ? stats.GetComponent<PartyMemberAI>() : null);
		if (partyMemberAI == null || partyMemberAI.Inventory == null)
		{
			Full.SetActive(value: false);
			Free.SetActive(value: false);
			return;
		}
		int freeSpace = partyMemberAI.Inventory.GetFreeSpace();
		FreeSpace.text = freeSpace.ToString();
		if (freeSpace <= 0)
		{
			Full.SetActive(value: true);
			Free.SetActive(value: false);
		}
		else
		{
			Full.SetActive(value: false);
			Free.SetActive(value: true);
		}
	}
}
