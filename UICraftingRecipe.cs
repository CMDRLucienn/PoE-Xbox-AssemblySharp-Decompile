using System;
using UnityEngine;

public class UICraftingRecipe : MonoBehaviour
{
	public UILabel Label;

	public GameObject Collider;

	public GameObject ParentEnchantCost;

	public UISprite EnchantValueSprite;

	public UILabel EnchantValueQty;

	private Recipe m_Recipe;

	[HideInInspector]
	public UICraftingRecipeCategory UICategory;

	public Recipe Recipe => m_Recipe;

	private void Start()
	{
		if ((bool)Collider)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Collider);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
		}
	}

	private void OnClick(GameObject go)
	{
		UICraftingManager.Instance.RecipeList.Select(this);
	}

	public void SetSelected(bool val)
	{
		if (val)
		{
			Label.color = UICraftingManager.Instance.ColorSelected;
		}
		else
		{
			UpdateLabelColoring();
		}
	}

	public void SetRecipe(Recipe recipe, UICraftingRecipeCategory cat)
	{
		UICategory = cat;
		m_Recipe = recipe;
		Label.text = m_Recipe.DisplayName.GetText();
		ParentEnchantCost.gameObject.SetActive(value: false);
		if (recipe.ItemModifications != null && recipe.ItemModifications.Length != 0)
		{
			int num = 0;
			ItemMod[] itemModifications = recipe.ItemModifications;
			for (int i = 0; i < itemModifications.Length; i++)
			{
				num += itemModifications[i].Cost;
			}
			if (num > 0)
			{
				ParentEnchantCost.gameObject.SetActive(value: true);
				EnchantValueSprite.color = UICraftingManager.Instance.ColorEnchantEnabled;
				EnchantValueQty.text = num.ToString();
			}
		}
		SetSelected(val: false);
	}

	private void UpdateLabelColoring()
	{
		EnchantValueQty.color = Color.white;
		if (m_Recipe.CanCreate(UICraftingManager.Instance.EnchantTarget))
		{
			Label.color = Color.white;
			EnchantValueSprite.color = UICraftingManager.Instance.ColorEnchantEnabled;
		}
		else
		{
			Label.color = UICraftingManager.Instance.ColorDisabled;
			EnchantValueSprite.color = UICraftingManager.Instance.ColorDisabled;
			EnchantValueQty.color = UICraftingManager.Instance.ColorDisabled;
		}
	}
}
