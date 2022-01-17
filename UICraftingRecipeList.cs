using System.Collections.Generic;
using UnityEngine;

public class UICraftingRecipeList : MonoBehaviour
{
	public UICraftingRecipeCategory RootCategory;

	private List<UICraftingRecipeCategory> m_Categories;

	private int m_Reposition;

	public GameObject ShowWhenEmpty;

	public UISelectionWidgetHighlight SelectionParent;

	public UIPanel Panel;

	private UIDraggablePanel m_DragPanel;

	public UITable RecipeCategoryGroupsLayout;

	private UICraftingRecipe m_SelectedRecipe;

	public UICraftingRecipe SelectedRecipe => m_SelectedRecipe;

	private void OnEnable()
	{
		Select(null);
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Categories == null)
		{
			m_DragPanel = Panel.GetComponent<UIDraggablePanel>();
			m_Categories = new List<UICraftingRecipeCategory>();
			RootCategory.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (m_Reposition > 0)
		{
			Reposition();
			m_Reposition--;
			m_DragPanel.ResetPosition();
		}
	}

	public void HandleInput()
	{
		if (GameInput.GetKeyUp(KeyCode.UpArrow))
		{
			Navigate(-1);
		}
		else if (GameInput.GetKeyUp(KeyCode.DownArrow))
		{
			Navigate(1);
		}
		if (GameInput.GetKeyUp(KeyCode.LeftArrow))
		{
			CollapseSelected();
		}
		else if (GameInput.GetKeyUp(KeyCode.RightArrow))
		{
			ExpandSelected();
		}
	}

	public void Navigate(int dir)
	{
		Init();
		int num = dir;
		int num2 = m_Categories.IndexOf(GetSelectedCategory());
		while (num != 0)
		{
			UICraftingRecipeCategory selectedCategory = GetSelectedCategory();
			if (!selectedCategory)
			{
				return;
			}
			num = selectedCategory.Navigate(dir);
			if (num < 0)
			{
				if (num2 > 0)
				{
					List<UICraftingRecipe> recipes = m_Categories[num2 - 1].m_Recipes;
					Select(recipes[recipes.Count - 1]);
				}
				break;
			}
			if (num > 0)
			{
				if (num2 < m_Categories.Count - 1)
				{
					List<UICraftingRecipe> recipes2 = m_Categories[num2 + 1].m_Recipes;
					Select(recipes2[0]);
				}
				break;
			}
			selectedCategory = GetSelectedCategory();
			num2 = m_Categories.IndexOf(selectedCategory) + num;
		}
		ExpandSelected();
	}

	public void CollapseSelected()
	{
		GetSelectedCategory().Expanded = false;
	}

	public void ExpandSelected()
	{
		GetSelectedCategory().Expanded = true;
	}

	public void Select(UICraftingRecipe recipe)
	{
		Init();
		if (recipe != null && recipe != m_SelectedRecipe)
		{
			m_SelectedRecipe = recipe;
			foreach (UICraftingRecipeCategory category in m_Categories)
			{
				category.ClearSelected();
			}
			recipe.SetSelected(val: true);
			UICraftingManager.Instance.LoadRecipe(recipe.Recipe);
			SelectionParent.SetWidgetTarget(recipe.Label);
			SelectionParent.gameObject.SetActive(value: true);
			m_DragPanel.ScrollObjectInView(recipe.gameObject, 20f);
		}
		else
		{
			m_SelectedRecipe = null;
			foreach (UICraftingRecipeCategory category2 in m_Categories)
			{
				category2.ClearSelected();
			}
			if ((bool)UICraftingManager.Instance)
			{
				UICraftingManager.Instance.LoadRecipe(null);
			}
			SelectionParent.gameObject.SetActive(value: false);
		}
		SelectionParent.Refresh();
		Panel.Refresh();
	}

	protected UICraftingRecipeCategory GetSelectedCategory()
	{
		if (m_SelectedRecipe == null)
		{
			return null;
		}
		foreach (UICraftingRecipeCategory category in m_Categories)
		{
			if (category.ContainsRecipe(m_SelectedRecipe))
			{
				return category;
			}
		}
		return null;
	}

	public void Reposition()
	{
		if ((bool)m_SelectedRecipe)
		{
			SelectionParent.gameObject.SetActive(m_SelectedRecipe.UICategory.Expanded);
		}
		RecipeCategoryGroupsLayout.Reposition();
		RecipeCategoryGroupsLayout.repositionNow = true;
		Panel.Refresh();
	}

	public void RefreshUsability()
	{
		for (int i = 0; i < m_Categories.Count; i++)
		{
			m_Categories[i].RefreshReceipeUsability();
			m_Categories[i].RefreshCategory();
		}
		UICraftingRecipe selectedRecipe = m_SelectedRecipe;
		m_SelectedRecipe = null;
		Select(selectedRecipe);
	}

	public void Reload()
	{
		Init();
		m_DragPanel.ResetPosition();
		if (!GameUtilities.Instance || GameUtilities.Instance.RecipePrefabs == null)
		{
			return;
		}
		foreach (UICraftingRecipeCategory category in m_Categories)
		{
			GameUtilities.Destroy(category.gameObject);
		}
		m_Categories.Clear();
		Recipe[] recipePrefabs = GameUtilities.Instance.RecipePrefabs;
		foreach (Recipe recipe in recipePrefabs)
		{
			if (!recipe.CanSee(UICraftingManager.Instance.ForLocation) || (UICraftingManager.Instance.EnchantMode && !recipe.IsModifiableItem(UICraftingManager.Instance.EnchantTarget)) || UICraftingManager.Instance.EnchantMode == (recipe.ModifiableItem == Recipe.ModificationType.None))
			{
				continue;
			}
			Persistence component = recipe.GetComponent<Persistence>();
			if ((bool)component && !component.PackageIsValid)
			{
				continue;
			}
			bool flag = false;
			foreach (UICraftingRecipeCategory category2 in m_Categories)
			{
				if (category2.AcceptsRecipe(recipe))
				{
					flag = true;
					category2.AddRecipe(recipe);
					break;
				}
			}
			if (!flag)
			{
				UICraftingRecipeCategory component2 = NGUITools.AddChild(RootCategory.transform.parent.gameObject, RootCategory.gameObject).GetComponent<UICraftingRecipeCategory>();
				component2.SetOwner(this);
				component2.gameObject.SetActive(value: true);
				component2.LoadCategory(recipe.Category);
				component2.AddRecipe(recipe);
				m_Categories.Add(component2);
			}
		}
		m_Categories.Sort((UICraftingRecipeCategory x, UICraftingRecipeCategory y) => x.DisplayName.CompareTo(y.DisplayName));
		for (int j = 0; j < m_Categories.Count; j++)
		{
			m_Categories[j].Sort();
		}
		ShowWhenEmpty.SetActive(m_Categories.Count == 0);
		m_DragPanel.ResetPosition();
		m_Reposition = 2;
		foreach (UICraftingRecipeCategory category3 in m_Categories)
		{
			category3.Expanded = false;
		}
	}
}
