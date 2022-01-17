using System;
using UnityEngine;

public class UICraftingRequirement : MonoBehaviour
{
	public UISprite FrameSprite;

	public UITexture IconTexture;

	public UILabel QuantityLabel;

	private Recipe m_Recipe;

	private RecipeIngredient m_Ingredient;

	private float m_Cost;

	private void Start()
	{
		if (FrameSprite != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(FrameSprite);
			uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnFrameTooltip));
			UIEventListener uIEventListener2 = UIEventListener.Get(FrameSprite);
			uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnFrameRightClick));
		}
	}

	private void OnFrameTooltip(GameObject sender, bool over)
	{
		if (over && m_Ingredient != null)
		{
			UIAbilityTooltip.GlobalShow(FrameSprite, m_Ingredient.RecipeItem);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	private void OnFrameRightClick(GameObject sender)
	{
		UIItemInspectManager.Examine(m_Ingredient.RecipeItem);
	}

	public void LoadRequirement(Recipe recipe, RecipeIngredient ingredient)
	{
		m_Recipe = recipe;
		m_Ingredient = ingredient;
		ReloadValues();
	}

	public void ReloadValues()
	{
		IconTexture.mainTexture = m_Ingredient.RecipeItem.GetIconTexture();
		int costMultiplier = m_Recipe.GetCostMultiplier(UICraftingManager.Instance.EnchantTarget);
		int num = m_Ingredient.Quantity * costMultiplier * UICraftingManager.Instance.RecipeAmount;
		int num2 = PartyHelper.PartyItemCount(m_Ingredient.RecipeItem);
		if (num2 < num)
		{
			QuantityLabel.color = UICraftingManager.Instance.ColorInvalid;
		}
		else
		{
			QuantityLabel.color = Color.white;
		}
		QuantityLabel.text = GUIUtils.Format(451, num2, num);
	}

	public void LoadCurrency(float amt)
	{
		m_Cost = amt;
		ReloadCurrency();
	}

	public void ReloadCurrency()
	{
		QuantityLabel.text = GUIUtils.Format(466, (int)m_Cost * UICraftingManager.Instance.RecipeAmount);
		if ((float)(GameState.s_playerCharacter ? ((int)(float)GameState.s_playerCharacter.Inventory.currencyTotalValue) : 0) < m_Cost)
		{
			QuantityLabel.color = UICraftingManager.Instance.ColorInvalid;
		}
		else
		{
			QuantityLabel.color = Color.white;
		}
	}
}
