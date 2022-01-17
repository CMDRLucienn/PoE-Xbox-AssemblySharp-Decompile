using System;
using System.Linq;
using UnityEngine;

public static class PartyHelper
{
	public static int NumPartyMembers
	{
		get
		{
			int num = 0;
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static bool PartyHasTalentOrAbility(string tag)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (!(onlyPrimaryPartyMember != null))
			{
				continue;
			}
			CharacterStats component = onlyPrimaryPartyMember.gameObject.GetComponent<CharacterStats>();
			if (!(component != null))
			{
				continue;
			}
			foreach (GenericAbility activeAbility in component.ActiveAbilities)
			{
				if (activeAbility != null && tag.Equals(activeAbility.tag, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool PartyHasSkill(string tag, int value)
	{
		CharacterStats.SkillType skillType = CharacterStats.SkillFromName(tag);
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null)
			{
				CharacterStats component = onlyPrimaryPartyMember.gameObject.GetComponent<CharacterStats>();
				if (component != null && component.CalculateSkill(skillType) >= value)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int PartyItemCount(Item item)
	{
		return PartyItemCount(item, enforceStash: false);
	}

	public static int PartyItemCount(Item item, bool enforceStash)
	{
		int num = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null)
			{
				Inventory inventory = onlyPrimaryPartyMember.Inventory;
				if (inventory != null)
				{
					num += inventory.ItemCount(item);
				}
				if (inventory is PlayerInventory && (!enforceStash || GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH)))
				{
					StashInventory stashInventory = (inventory as PlayerInventory).StashInventory;
					num += stashInventory.ItemCount(item);
				}
			}
		}
		return num;
	}

	public static bool PartyHasItem(Item item)
	{
		return PartyHasItem(item, enforceStash: false);
	}

	public static bool PartyHasItem(Item item, bool enforceStash)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null)
			{
				Inventory inventory = onlyPrimaryPartyMember.Inventory;
				if (inventory != null && inventory.ItemCount(item) > 0)
				{
					return true;
				}
				if (inventory is PlayerInventory && (!enforceStash || GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH)) && (inventory as PlayerInventory).StashInventory.ItemCount(item) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int PartyDestroyItem(Item item, int quantity)
	{
		return PartyDestroyItem(item, quantity, enforceStash: false);
	}

	public static int PartyDestroyItem(Item item, int quantity, bool enforceStash)
	{
		int num = quantity;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null)
			{
				Inventory inventory = onlyPrimaryPartyMember.Inventory;
				if (inventory != null)
				{
					num = inventory.DestroyItem(item, num);
				}
				if (inventory is PlayerInventory && (!enforceStash || GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH)))
				{
					num = (inventory as PlayerInventory).StashInventory.DestroyItem(item, num);
				}
			}
		}
		return num;
	}

	public static bool PutItem(Item item, GameObject favor)
	{
		if (!item)
		{
			return true;
		}
		return PutItem(new InventoryItem(item, 1), favor);
	}

	public static bool PutItem(InventoryItem item, GameObject favor)
	{
		if ((bool)favor)
		{
			Inventory component = favor.GetComponent<Inventory>();
			if ((bool)component && component.PutItem(item))
			{
				return true;
			}
			PartyMemberAI component2 = favor.GetComponent<PartyMemberAI>();
			if ((bool)component2 && component2.IsActiveInParty)
			{
				return GameState.s_playerCharacter.Inventory.PutItem(item);
			}
		}
		return false;
	}

	public static bool PartyNearPoint(Vector3 point, float range)
	{
		range *= range;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember != null && (onlyPrimaryPartyMember.transform.position - point).sqrMagnitude > range)
			{
				return false;
			}
		}
		return true;
	}

	public static bool PartyCanSeeEnemy()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!partyMemberAI)
			{
				continue;
			}
			GameObject[] array = GameUtilities.CreaturesInRange(partyMemberAI.transform.position, 50f, partyMemberAI.gameObject, includeUnconscious: true);
			if (array == null)
			{
				continue;
			}
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				Health component = gameObject.GetComponent<Health>();
				if (component != null && component.Dead)
				{
					continue;
				}
				Trap component2 = gameObject.GetComponent<Trap>();
				AIController component3 = gameObject.GetComponent<AIController>();
				if (component2 == null && component3 != null)
				{
					Faction component4 = gameObject.GetComponent<Faction>();
					if ((bool)component4 && component4.isFowVisible)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static PartyMemberAI SeekNextPartyMember(PartyMemberAI current)
	{
		bool flag = false;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (flag && (bool)onlyPrimaryPartyMember)
			{
				return onlyPrimaryPartyMember;
			}
			if (onlyPrimaryPartyMember == current)
			{
				flag = true;
			}
		}
		return PartyMemberAI.OnlyPrimaryPartyMembers.FirstOrDefault();
	}

	public static PartyMemberAI SeekPreviousPartyMember(PartyMemberAI current)
	{
		PartyMemberAI partyMemberAI = null;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember == current)
			{
				if ((bool)partyMemberAI)
				{
					return partyMemberAI;
				}
				break;
			}
			partyMemberAI = onlyPrimaryPartyMember;
		}
		return PartyMemberAI.OnlyPrimaryPartyMembers.LastOrDefault();
	}

	public static bool IsPartyMember(GameObject character)
	{
		PartyMemberAI component = ComponentUtils.GetComponent<PartyMemberAI>(character);
		if ((bool)component)
		{
			return component.enabled;
		}
		return false;
	}

	public static void AssignXPToParty(int xp, bool printMessage = true)
	{
		int numPartyMembers = NumPartyMembers;
		if (numPartyMembers == 0)
		{
			return;
		}
		xp = AddBonusXP(xp);
		if (printMessage)
		{
			Console.AddMessage("[" + NGUITools.EncodeColor(Color.green) + "]" + Console.Format(GUIUtils.GetTextWithLinks(1631), xp * numPartyMembers), Console.ConsoleState.Both);
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					component.AddExperience(xp);
				}
			}
		}
	}

	public static int AddBonusXP(int xp)
	{
		int num = 1;
		if ((bool)GameState.s_playerCharacter)
		{
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if (component != null)
			{
				num = component.Level;
			}
		}
		float num2 = 0.05f;
		if (num > 1)
		{
			num2 = 0.1f;
		}
		int numPartyMembers = NumPartyMembers;
		float num3 = 0f;
		if (numPartyMembers < 6)
		{
			num3 = num2 * (float)(6 - numPartyMembers);
		}
		return xp + (int)((float)xp * num3);
	}
}
