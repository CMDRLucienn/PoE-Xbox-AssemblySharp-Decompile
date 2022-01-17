using System;
using UnityEngine;

[RequireComponent(typeof(Vendor))]
public class Inn : MonoBehaviour
{
	[Serializable]
	public class InnRoom
	{
		public string DesignerNote;

		public Affliction RestBonus;

		public InteractablesDatabaseString DisplayName;

		public CurrencyValue Cost;

		public Texture2D DisplayImage;

		public RecipeRequirement[] VisibilityRequirements;

		public int ActualCost(Inn inn)
		{
			return Mathf.CeilToInt((float)Cost * inn.multiplier);
		}

		public bool CanSee()
		{
			return RecipeRequirement.CheckRequirements(VisibilityRequirements);
		}
	}

	public bool IsPlayerOwned;

	[Persistent]
	public float multiplier = 1f;

	public InnRoom[] Rooms;

	private PlayerInventory m_PlayerInventory;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_PlayerInventory == null && GameState.s_playerCharacter != null)
		{
			m_PlayerInventory = GameState.s_playerCharacter.Inventory;
		}
	}

	public bool CanPurchaseRoom(InnRoom room)
	{
		Init();
		if (m_PlayerInventory.currencyTotalValue.v < (float)room.ActualCost(this) && !IsPlayerOwned)
		{
			return false;
		}
		if (!room.CanSee())
		{
			return false;
		}
		return true;
	}

	public void PurchaseRoom(InnRoom room)
	{
		if (!CanPurchaseRoom(room))
		{
			return;
		}
		Init();
		int num = room.ActualCost(this);
		if (!IsPlayerOwned)
		{
			m_PlayerInventory.currencyTotalValue.v -= num;
			Store component = GetComponent<Store>();
			if ((bool)component)
			{
				component.currencyStoreBank.v += num;
			}
		}
		RestZone.Rest(RestZone.Mode.Inn);
		if (!room.RestBonus)
		{
			return;
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				CharacterStats component2 = partyMemberAI.GetComponent<CharacterStats>();
				if ((bool)component2)
				{
					component2.ClearRestingAffliction();
					component2.ApplyAffliction(room.RestBonus);
				}
			}
		}
	}
}
