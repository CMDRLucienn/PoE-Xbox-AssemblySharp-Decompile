using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICraftingRequirements : MonoBehaviour
{
	public UICraftingRequirement RootItem;

	public UIGrid Grid;

	public UIGrid GridRequirementLabels;

	public UILabel RequirementTitleLabel;

	public UILabel RequirementLevel;

	public UILabel RequirementEnchantValue;

	public UISprite RequirementEnchantSprite;

	public UICraftingRequirement CurrencyReq;

	private List<UICraftingRequirement> m_Requirements;

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Requirements == null)
		{
			m_Requirements = new List<UICraftingRequirement>();
			RootItem.gameObject.SetActive(value: false);
		}
	}

	public void LoadRequirements(Recipe recipe)
	{
		Init();
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		IEnumerable<RecipeRequirement> source = recipe.CreationRequirements.Where((RecipeRequirement rr) => rr.Type == RecipeRequirement.RecipeRequirementType.PlayerMinimumLevel);
		if (source.Any())
		{
			int value = source.First().Value;
			if (value > 0)
			{
				RequirementLevel.text = GUIUtils.Format(1763, value);
				flag2 = true;
				if (value > GameState.s_playerCharacter.GetComponent<CharacterStats>().ScaledLevel)
				{
					flag = false;
					RequirementLevel.color = UICraftingManager.Instance.ColorInvalid;
				}
				else
				{
					RequirementLevel.color = Color.white;
				}
			}
		}
		RequirementLevel.gameObject.SetActive(flag2);
		ItemMod[] itemModifications = recipe.ItemModifications;
		if (itemModifications != null && itemModifications.Length != 0)
		{
			int num = 0;
			int num2 = 0;
			ItemMod itemMod = null;
			for (int i = 0; i < itemModifications.Length; i++)
			{
				itemMod = itemModifications[i];
				if (itemMod.IsQualityMod)
				{
					num2 += itemMod.Cost;
				}
				num += itemMod.Cost;
			}
			if (num > 0)
			{
				flag3 = true;
				bool flag4 = false;
				int num3 = 0;
				int num4 = 0;
				if (UICraftingManager.Instance.EnchantTarget != null)
				{
					Equippable equippable = UICraftingManager.Instance.EnchantTarget as Equippable;
					if (equippable != null)
					{
						List<ItemModComponent> attachedItemMods = equippable.AttachedItemMods;
						ItemModComponent itemModComponent = null;
						for (int j = 0; j < attachedItemMods.Count; j++)
						{
							itemModComponent = attachedItemMods[j];
							if (itemMod != null && itemModComponent.Mod != null && itemModComponent.Mod.IsQualityMod)
							{
								num4 += itemModComponent.Mod.Cost;
							}
						}
						num3 = equippable.TotalItemModValue();
						int num5 = num + num3;
						if (num4 > 0 && num2 > 0)
						{
							num5 -= num4;
						}
						flag4 = num5 <= ItemMod.MaximumModValue;
					}
				}
				if (!flag4)
				{
					flag = false;
				}
				RequirementEnchantValue.text = num.ToString();
				RequirementEnchantValue.color = (flag4 ? Color.white : UICraftingManager.Instance.ColorInvalid);
				RequirementEnchantSprite.color = (flag4 ? Color.white : UICraftingManager.Instance.ColorInvalid);
			}
		}
		RequirementEnchantValue.gameObject.SetActive(flag3);
		RequirementEnchantSprite.gameObject.SetActive(flag3);
		RequirementTitleLabel.gameObject.SetActive(flag2 || flag3);
		RequirementTitleLabel.color = (flag ? Color.white : UICraftingManager.Instance.ColorInvalid);
		GridRequirementLabels.Reposition();
		int k = 0;
		RecipeIngredient[] ingredients = recipe.Ingredients;
		foreach (RecipeIngredient ingredient in ingredients)
		{
			GetIcon(k).LoadRequirement(recipe, ingredient);
			k++;
		}
		for (; k < m_Requirements.Count; k++)
		{
			m_Requirements[k].gameObject.SetActive(value: false);
		}
		float num6 = (float)recipe.Cost * (float)recipe.GetCostMultiplier(UICraftingManager.Instance.EnchantTarget);
		if (num6 != 0f)
		{
			CurrencyReq.LoadCurrency(num6);
			CurrencyReq.gameObject.SetActive(value: true);
		}
		else
		{
			CurrencyReq.gameObject.SetActive(value: false);
		}
		Grid.Reposition();
	}

	public void ReloadRequirements()
	{
		if (m_Requirements != null)
		{
			for (int i = 0; i < m_Requirements.Count; i++)
			{
				m_Requirements[i].ReloadValues();
			}
			CurrencyReq.ReloadCurrency();
		}
	}

	private UICraftingRequirement GetIcon(int index)
	{
		if (index < m_Requirements.Count)
		{
			m_Requirements[index].gameObject.SetActive(value: true);
			return m_Requirements[index];
		}
		UICraftingRequirement component = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UICraftingRequirement>();
		m_Requirements.Add(component);
		component.gameObject.SetActive(value: true);
		return component;
	}
}
