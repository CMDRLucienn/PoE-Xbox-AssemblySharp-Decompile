using System;
using System.Text;
using UnityEngine;

public class UICraftingWorkspace : MonoBehaviour
{
	public UITexture ProductIcon;

	public GameObject ProductCollider;

	public UIDraggablePanel DragPanel;

	public UILabel TitleLabel;

	public UILabel StatisticsTitle;

	public UILabel StatisticsLabel;

	public UILabel MessagesLabel;

	public UILabel SelectARecipe;

	public UICapitularLabel DescriptionLabel;

	public UICraftingRequirements Requirements;

	protected Recipe m_CurrentRecipe;

	public GameObject PostStatistics;

	public int PostStatisticsEarlyOff;

	private float m_PostStatisticsBase = float.MaxValue;

	public virtual void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(ProductCollider);
		uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnRightClickProduct));
		OutputText(string.Empty);
	}

	public void OutputText(string msg)
	{
		if (MessagesLabel != null)
		{
			MessagesLabel.text = msg;
			MessagesLabel.color = Color.white;
		}
	}

	public void OutputErrorText(string msg)
	{
		if (MessagesLabel != null)
		{
			MessagesLabel.text = msg;
			MessagesLabel.color = UICraftingManager.Instance.ColorInvalid;
		}
	}

	public virtual void LoadRecipe(Recipe recipe)
	{
		m_CurrentRecipe = recipe;
		Reload();
	}

	public void Reload()
	{
		if (m_PostStatisticsBase >= float.MaxValue)
		{
			m_PostStatisticsBase = PostStatistics.transform.localPosition.y;
		}
		DragPanel.ResetPosition();
		if ((bool)m_CurrentRecipe)
		{
			DragPanel.panel.alpha = 1f;
			SelectARecipe.alpha = 0f;
			TitleLabel.text = m_CurrentRecipe.DisplayName.GetText();
			if (m_CurrentRecipe.Output.Length != 0 && m_CurrentRecipe.Output[0].Quantity > 1)
			{
				TitleLabel.text += GUIUtils.Format(1731, m_CurrentRecipe.Output[0].Quantity);
				UICraftingManager.Instance.CraftingAmt.DisplayMultiplier = m_CurrentRecipe.Output[0].Quantity;
			}
			else
			{
				UICraftingManager.Instance.CraftingAmt.DisplayMultiplier = 1;
			}
			if (m_CurrentRecipe.Output.Length != 0)
			{
				ProductIcon.mainTexture = m_CurrentRecipe.Output[0].RecipeItem.GetIconLargeTexture();
			}
			else if ((bool)UICraftingManager.Instance.EnchantTarget)
			{
				ProductIcon.mainTexture = UICraftingManager.Instance.EnchantTarget.GetIconLargeTexture();
			}
			else
			{
				ProductIcon.mainTexture = null;
			}
			ProductIcon.MakePixelPerfect();
			ProductIcon.alpha = ((ProductIcon.mainTexture != null) ? 1 : 0);
			if (m_CurrentRecipe.Output.Length != 0)
			{
				DescriptionLabel.text = m_CurrentRecipe.Output[0].RecipeItem.DescriptionText.GetText();
			}
			else
			{
				DescriptionLabel.text = "";
			}
			if (m_CurrentRecipe.Output.Length != 0)
			{
				StatisticsLabel.text = UIItemInspectManager.GetEffectTextFlat(m_CurrentRecipe.Output[0].RecipeItem, GameState.s_playerCharacter.gameObject);
			}
			else if (m_CurrentRecipe.ItemModifications.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < m_CurrentRecipe.ItemModifications.Length; i++)
				{
					if (m_CurrentRecipe.ItemModifications[i] != null)
					{
						stringBuilder.Append(UIItemInspectManager.GetEffectTextFlat(m_CurrentRecipe.ItemModifications[i], GameState.s_playerCharacter.gameObject));
					}
				}
				StatisticsLabel.text = stringBuilder.ToString();
			}
			else
			{
				StatisticsLabel.text = "";
			}
			if (StatisticsLabel.text.Length == 0)
			{
				PostStatistics.transform.localPosition = new Vector3(PostStatistics.transform.localPosition.x, m_PostStatisticsBase + (float)PostStatisticsEarlyOff, PostStatistics.transform.localPosition.z);
				StatisticsTitle.alpha = 0f;
			}
			else
			{
				PostStatistics.transform.localPosition = new Vector3(PostStatistics.transform.localPosition.x, StatisticsLabel.transform.localPosition.y - 20f - StatisticsLabel.transform.localScale.y * StatisticsLabel.relativeSize.y, PostStatistics.transform.localPosition.z);
				StatisticsTitle.alpha = 1f;
			}
			Requirements.LoadRequirements(m_CurrentRecipe);
		}
		else
		{
			DragPanel.panel.alpha = 0f;
			SelectARecipe.alpha = 1f;
		}
		DragPanel.ResetPosition();
	}

	public void RefreshRecipeQty()
	{
		Requirements.ReloadRequirements();
	}

	private void OnRightClickProduct(GameObject sender)
	{
		if (m_CurrentRecipe != null && m_CurrentRecipe.Output.Length != 0)
		{
			UIItemInspectManager.ExamineNoEnchant(m_CurrentRecipe.Output[0].RecipeItem, GameState.s_playerCharacter.gameObject);
		}
		else if ((bool)UICraftingManager.Instance.EnchantTarget)
		{
			UIItemInspectManager.ExamineNoEnchant(UICraftingManager.Instance.EnchantTarget, GameState.s_playerCharacter.gameObject);
		}
	}
}
