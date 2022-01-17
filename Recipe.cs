using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Recipe : MonoBehaviour
{
	public enum ModificationType
	{
		None,
		Weapon,
		Shield,
		Armor,
		List
	}

	public enum WhyCantCreate
	{
		NONE,
		COST,
		REQUIREMENTS,
		INGREDIENTS,
		NOT_MODDABLE,
		ALREADY_HAS_MOD,
		MAXIMUM_QUALITY_MODS,
		MAXIMUM_MOD_VALUE,
		MAXIMUM_ENCHANTMENTS,
		EXISTING_QUALITY_MOD_IS_BETTER
	}

	public static bool FreeRecipes;

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Recipes);

	public string CraftingLocation;

	public RecipeCategory Category;

	public RecipeRequirement[] VisibilityRequirements;

	public RecipeRequirement[] CreationRequirements;

	public CurrencyValue Cost;

	public ModificationType ModifiableItem;

	public Item[] ModifiableItemList;

	public RecipeIngredient[] Ingredients;

	public RecipeItemData[] Output;

	public ItemMod[] ItemModifications;

	public static WhyCantCreate CantCreateReason { get; private set; }

	public int GetCostMultiplier(Item modifyItem)
	{
		Equippable equippable = modifyItem as Equippable;
		if (ModifiableItem == ModificationType.None)
		{
			return 1;
		}
		if ((bool)equippable)
		{
			if (!equippable.BothPrimaryAndSecondarySlot)
			{
				return 1;
			}
			return 2;
		}
		return 1;
	}

	public void Start()
	{
		if (ModifiableItem == ModificationType.None)
		{
			if (Output.Length == 0)
			{
				Debug.LogError("Warning! Recipe " + DisplayName.GetText() + " has no Output item(s).");
			}
		}
		else if (Output.Length != 0)
		{
			Debug.LogError("Warning! Recipe " + DisplayName.GetText() + " for modifying items also lists Output item(s).");
		}
	}

	public bool CanSee(string Location)
	{
		if ((!string.IsNullOrEmpty(Location) || !string.IsNullOrEmpty(CraftingLocation)) && Location != CraftingLocation)
		{
			return false;
		}
		return RecipeRequirement.CheckRequirements(VisibilityRequirements);
	}

	public bool CanCreate()
	{
		return CanCreate(null, 1);
	}

	public bool CanCreate(int quantity)
	{
		return CanCreate(null, quantity);
	}

	public bool CanCreate(Item modifyItem)
	{
		return CanCreate(modifyItem, 1);
	}

	public bool CanCreate(Item modifyItem, int quantity)
	{
		CantCreateReason = WhyCantCreate.NONE;
		if (!CanCreateModPartial(modifyItem))
		{
			return false;
		}
		if (FreeRecipes)
		{
			return true;
		}
		int costMultiplier = GetCostMultiplier(modifyItem);
		if (GameState.s_playerCharacter.Inventory.currencyTotalValue.v < (float)costMultiplier * (float)Cost * (float)quantity)
		{
			CantCreateReason = WhyCantCreate.COST;
			return false;
		}
		if (!RecipeRequirement.CheckRequirements(CreationRequirements))
		{
			CantCreateReason = WhyCantCreate.REQUIREMENTS;
			return false;
		}
		if (Ingredients != null)
		{
			RecipeIngredient[] ingredients = Ingredients;
			foreach (RecipeIngredient recipeIngredient in ingredients)
			{
				if (PartyHelper.PartyItemCount(recipeIngredient.RecipeItem) < costMultiplier * recipeIngredient.Quantity * quantity)
				{
					CantCreateReason = WhyCantCreate.INGREDIENTS;
					return false;
				}
			}
		}
		return true;
	}

	public bool CanCreateModPartial(Item modifyItem)
	{
		CantCreateReason = WhyCantCreate.NONE;
		if (ModifiableItem == ModificationType.None)
		{
			return true;
		}
		Equippable modifyEquippable = modifyItem as Equippable;
		if (!modifyEquippable)
		{
			CantCreateReason = WhyCantCreate.NOT_MODDABLE;
			return false;
		}
		if (ModifiableItem != 0)
		{
			if (modifyItem == null)
			{
				if (ModifiableItemsInInventory() == null)
				{
					CantCreateReason = WhyCantCreate.NOT_MODDABLE;
					return false;
				}
			}
			else if (!IsModifiableItem(modifyItem))
			{
				CantCreateReason = WhyCantCreate.NOT_MODDABLE;
				return false;
			}
		}
		if (FreeRecipes)
		{
			return true;
		}
		if (ItemModifications.All((ItemMod im) => modifyEquippable.AttachedItemMods.Any((ItemModComponent im2) => im2.Mod.Equals(im))))
		{
			CantCreateReason = WhyCantCreate.ALREADY_HAS_MOD;
			return false;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		Dictionary<ItemMod.EnchantCategory, int> dictionary = new Dictionary<ItemMod.EnchantCategory, int>();
		foreach (ItemModComponent attachedItemMod in modifyEquippable.AttachedItemMods)
		{
			if (attachedItemMod.Mod.Cost > 0)
			{
				if (!dictionary.ContainsKey(attachedItemMod.Mod.ModEnchantCategory))
				{
					dictionary.Add(attachedItemMod.Mod.ModEnchantCategory, 1);
				}
				else
				{
					dictionary[attachedItemMod.Mod.ModEnchantCategory]++;
				}
			}
			if (attachedItemMod.Mod.IsQualityMod)
			{
				if (flag)
				{
					CantCreateReason = WhyCantCreate.MAXIMUM_QUALITY_MODS;
					return false;
				}
				flag = true;
				num = attachedItemMod.Mod.Cost;
			}
			num2 += attachedItemMod.Mod.Cost;
		}
		ItemMod[] itemModifications = ItemModifications;
		foreach (ItemMod itemMod in itemModifications)
		{
			if (itemMod.ModEnchantCategory == ItemMod.EnchantCategory.Quality)
			{
				if (flag && num > itemMod.Cost)
				{
					CantCreateReason = WhyCantCreate.EXISTING_QUALITY_MOD_IS_BETTER;
					return false;
				}
			}
			else if (itemMod.Cost > 0 && dictionary.ContainsKey(itemMod.ModEnchantCategory) && dictionary[itemMod.ModEnchantCategory] >= 1)
			{
				CantCreateReason = WhyCantCreate.MAXIMUM_ENCHANTMENTS;
				return false;
			}
			num2 = ((!(itemMod.IsQualityMod && flag)) ? (num2 + itemMod.Cost) : (num2 + (itemMod.Cost - num)));
			if (itemMod.Cost > 0 && num2 > ItemMod.MaximumModValue)
			{
				CantCreateReason = WhyCantCreate.MAXIMUM_MOD_VALUE;
				return false;
			}
		}
		return true;
	}

	public int GetRecipeModifierSortValue()
	{
		int num = 0;
		for (int i = 0; i < ItemModifications.Length; i++)
		{
			ItemMod itemMod = ItemModifications[i];
			if (itemMod != null)
			{
				num += itemMod.Cost;
			}
		}
		return num;
	}

	public bool Create()
	{
		return Create(null);
	}

	public bool Create(Item modifyItem)
	{
		if (ModifiableItem != 0 && modifyItem == null)
		{
			return false;
		}
		if (GameState.s_playerCharacter == null)
		{
			return false;
		}
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		if (inventory == null)
		{
			return false;
		}
		if (!CanCreate(modifyItem))
		{
			return false;
		}
		Equippable equippable = modifyItem as Equippable;
		GameObject gameObject = null;
		Equippable.EquipmentSlot slot = Equippable.EquipmentSlot.Waist;
		if ((bool)equippable)
		{
			gameObject = equippable.EquippedOwner;
			slot = equippable.EquippedSlot;
			if ((bool)gameObject)
			{
				equippable.UnEquip(gameObject, slot);
			}
		}
		int costMultiplier = GetCostMultiplier(modifyItem);
		GameState.s_playerCharacter.Inventory.currencyTotalValue.v -= (float)costMultiplier * (float)Cost;
		if (Ingredients != null)
		{
			RecipeIngredient[] ingredients = Ingredients;
			foreach (RecipeIngredient recipeIngredient in ingredients)
			{
				if (recipeIngredient.Destroyed)
				{
					PartyHelper.PartyDestroyItem(recipeIngredient.RecipeItem, recipeIngredient.Quantity * costMultiplier);
				}
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if ((bool)equippable && ItemModifications != null)
		{
			ItemMod[] itemModifications = ItemModifications;
			foreach (ItemMod mod in itemModifications)
			{
				if (!equippable.AttachedItemMods.Any((ItemModComponent im) => im.Mod == mod))
				{
					equippable.AttachItemMod(mod);
				}
				flag3 = true;
				AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumUniqueEnchantmentsCreated, mod.name);
				if (mod.ModEnchantCategory == ItemMod.EnchantCategory.Quality && mod.DisplayName.StringID == 380)
				{
					if (modifyItem.GetComponent<Shield>() != null || modifyItem.GetComponent<Weapon>() != null)
					{
						AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumWeaponOrShieldsLegendaryEnchanted);
					}
					else if (modifyItem.GetComponent<Armor>() != null)
					{
						AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumArmorsLegendaryEnchanted);
					}
				}
			}
		}
		if (Output != null)
		{
			RecipeItemData[] output = Output;
			foreach (RecipeItemData recipeItemData in output)
			{
				Consumable consumable = (recipeItemData.RecipeItem ? recipeItemData.RecipeItem.GetComponent<Consumable>() : null);
				if ((bool)consumable)
				{
					if (consumable.Type == Consumable.ConsumableType.Scroll)
					{
						flag2 = true;
					}
					if (consumable.Type == Consumable.ConsumableType.Potion)
					{
						flag = true;
					}
				}
				inventory.AddItem(recipeItemData.RecipeItem, recipeItemData.Quantity);
				if ((bool)AchievementTracker.Instance)
				{
					Consumable component = recipeItemData.RecipeItem.GetComponent<Consumable>();
					if ((bool)component && component.Type == Consumable.ConsumableType.Scroll)
					{
						AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumUniqueScrollsCreated, recipeItemData.RecipeItem.name);
					}
					else if ((bool)component && component.Type == Consumable.ConsumableType.Ingestible)
					{
						AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumUniqueFoodItemsCreated, recipeItemData.RecipeItem.name);
					}
					else if ((bool)component && component.Type == Consumable.ConsumableType.Potion)
					{
						AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumUniquePotionsCreated, recipeItemData.RecipeItem.name);
					}
				}
			}
		}
		if ((bool)gameObject && (bool)equippable)
		{
			equippable.Equip(gameObject, slot);
		}
		if (GlobalAudioPlayer.Instance != null)
		{
			if (flag3)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ItemEnchanted);
			}
			else if (flag)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ItemCraftedPotion);
			}
			else if (flag2)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ItemCraftedScroll);
			}
			else
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ItemCrafted);
			}
		}
		return true;
	}

	public bool IsModifiableItem(Item item)
	{
		if (!item || item.IsPrefab)
		{
			return false;
		}
		switch (ModifiableItem)
		{
		case ModificationType.Weapon:
			if (item.GetComponent<Weapon>() == null)
			{
				return false;
			}
			break;
		case ModificationType.Shield:
			if (item.GetComponent<Shield>() == null)
			{
				return false;
			}
			break;
		case ModificationType.Armor:
			if (item.GetComponent<Armor>() == null)
			{
				return false;
			}
			break;
		case ModificationType.List:
		{
			bool flag = false;
			Item prefab = item.Prefab;
			for (int i = 0; i < ModifiableItemList.Length; i++)
			{
				if (prefab.DisplayName.StringTableID == ModifiableItemList[i].DisplayName.StringTableID && prefab.DisplayName.StringID == ModifiableItemList[i].DisplayName.StringID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		}
		return true;
	}

	public Item[] ModifiableItemsInInventory()
	{
		List<Item> list = new List<Item>();
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (!(onlyPrimaryPartyMember != null))
			{
				continue;
			}
			Inventory inventory = onlyPrimaryPartyMember.Inventory;
			if (!(inventory != null))
			{
				continue;
			}
			foreach (InventoryItem item in inventory.ItemList)
			{
				if (IsModifiableItem(item.BaseItem))
				{
					list.Add(item.BaseItem);
				}
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}
}
