using System;
using System.Collections.Generic;
using UnityEngine;

public class UICraftingRecipeCategory : MonoBehaviour
{
	public UISprite SpriteButtonBG;

	public UILabel TitleLabel;

	public UICraftingRecipe RootItem;

	public UISprite SpriteEnchantCategory;

	public UITable Layout;

	private UICraftingRecipeList m_Owner;

	private bool m_CategoryDisabled;

	private const string SPRITE_BUTTON_UP = "listButton";

	private const string SPRITE_BUTTON_DOWN = "listButton_Dwn";

	private const string SPRITE_BUTTON_HOVER = "listButton_Sel";

	private bool m_Expanded = true;

	[HideInInspector]
	public List<UICraftingRecipe> m_Recipes = new List<UICraftingRecipe>();

	public RecipeCategory RecipeCategory { get; private set; }

	public string DisplayName
	{
		get
		{
			if (!RecipeCategory)
			{
				return "";
			}
			return RecipeCategory.DisplayName.GetText();
		}
	}

	public bool Expanded
	{
		get
		{
			return m_Expanded;
		}
		set
		{
			if (m_Expanded != value)
			{
				m_Expanded = value;
				if (m_Expanded)
				{
					SpriteButtonBG.spriteName = "listButton";
					NGUITools.SetActive(Layout.gameObject, state: true);
				}
				else
				{
					SpriteButtonBG.spriteName = "listButton";
					NGUITools.SetActive(Layout.gameObject, state: false);
				}
				if ((bool)m_Owner)
				{
					m_Owner.Reposition();
				}
			}
		}
	}

	private void Start()
	{
		RootItem.gameObject.SetActive(value: false);
		UIEventListener uIEventListener = UIEventListener.Get(SpriteButtonBG.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTitleClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(SpriteButtonBG.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnTitleHover));
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnTitleClick(GameObject sender)
	{
		Expanded = !Expanded;
	}

	private void OnTitleHover(GameObject sender, bool hover)
	{
		SpriteButtonBG.color = (hover ? Color.white : Color.black);
	}

	public void ClearSelected()
	{
		foreach (UICraftingRecipe recipe in m_Recipes)
		{
			recipe.SetSelected(val: false);
		}
	}

	public void SetOwner(UICraftingRecipeList owner)
	{
		m_Owner = owner;
	}

	public void LoadCategory(RecipeCategory category)
	{
		RecipeCategory = category;
		base.gameObject.name = DisplayName;
		TitleLabel.text = DisplayName;
		if (category.EnchantCategory == ItemMod.EnchantCategory.Quality)
		{
			base.gameObject.name = "_" + DisplayName;
		}
		string categorySpriteName = UICraftingManager.GetCategorySpriteName(category.EnchantCategory);
		SpriteEnchantCategory.gameObject.SetActive(!string.IsNullOrEmpty(categorySpriteName));
		SpriteEnchantCategory.spriteName = categorySpriteName;
		UpdateCategoryForEnchantLimits(UICraftingManager.Instance.EnchantTarget);
	}

	public void AddRecipe(Recipe recipe)
	{
		UICraftingRecipe component = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UICraftingRecipe>();
		component.gameObject.SetActive(value: true);
		component.gameObject.name = recipe.DisplayName.GetText();
		component.SetRecipe(recipe, this);
		m_Recipes.Add(component);
		Layout.Reposition();
		Layout.repositionNow = true;
	}

	public static int SortByValueThenName(UICraftingRecipe a, UICraftingRecipe b)
	{
		if (a == null)
		{
			return -1;
		}
		if (b == null)
		{
			return 1;
		}
		if (a.Recipe != null && b.Recipe != null)
		{
			int recipeModifierSortValue = a.Recipe.GetRecipeModifierSortValue();
			int recipeModifierSortValue2 = b.Recipe.GetRecipeModifierSortValue();
			if (recipeModifierSortValue != recipeModifierSortValue2)
			{
				return recipeModifierSortValue.CompareTo(recipeModifierSortValue2);
			}
			return string.Compare(a.Recipe.DisplayName.GetText(), b.Recipe.DisplayName.GetText());
		}
		return string.Compare(a.name, b.name);
	}

	public void Sort()
	{
		m_Recipes.Sort(SortByValueThenName);
		for (int i = 0; i < m_Recipes.Count; i++)
		{
			m_Recipes[i].name = $"{i:000}_{m_Recipes[i].Recipe.DisplayName}";
		}
		Layout.Reposition();
	}

	public bool AcceptsRecipe(Recipe recipe)
	{
		if (!(RecipeCategory == null))
		{
			return RecipeCategory.Equals(recipe.Category);
		}
		return true;
	}

	public bool ContainsRecipe(UICraftingRecipe recipe)
	{
		return m_Recipes.Contains(recipe);
	}

	public void RefreshCategory()
	{
		UpdateCategoryForEnchantLimits(UICraftingManager.Instance.EnchantTarget);
	}

	public void RefreshReceipeUsability()
	{
		for (int i = 0; i < m_Recipes.Count; i++)
		{
			m_Recipes[i].SetSelected(val: false);
		}
	}

	public int Navigate(int dir)
	{
		int num = m_Recipes.IndexOf(UICraftingManager.Instance.RecipeList.SelectedRecipe);
		if (num < 0)
		{
			return 0;
		}
		if (dir < 0)
		{
			if (num <= 0)
			{
				return -1;
			}
			UICraftingManager.Instance.RecipeList.Select(m_Recipes[num + dir]);
			return 0;
		}
		if (dir > 0)
		{
			if (num >= m_Recipes.Count - 1)
			{
				return 1;
			}
			UICraftingManager.Instance.RecipeList.Select(m_Recipes[num + dir]);
			return 0;
		}
		return 0;
	}

	private void UpdateCategoryForEnchantLimits(Item enchantItem)
	{
		m_CategoryDisabled = false;
		if (!UICraftingManager.Instance.EnchantMode)
		{
			return;
		}
		Equippable component = enchantItem.GetComponent<Equippable>();
		if (!(component == null))
		{
			int numberOfModsOfEnchantCategory = component.GetNumberOfModsOfEnchantCategory(RecipeCategory.EnchantCategory, 0);
			m_CategoryDisabled = numberOfModsOfEnchantCategory >= 1;
			TitleLabel.text = DisplayName + GUIUtils.Format(1731, GUIUtils.Format(451, numberOfModsOfEnchantCategory, 1));
			if (RecipeCategory.EnchantCategory == ItemMod.EnchantCategory.Quality && numberOfModsOfEnchantCategory == 1)
			{
				m_CategoryDisabled = false;
			}
			TitleLabel.color = (m_CategoryDisabled ? UICraftingManager.Instance.ColorDisabled : Color.white);
		}
	}
}
