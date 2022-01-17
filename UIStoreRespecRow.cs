using System;
using UnityEngine;

public class UIStoreRespecRow : MonoBehaviour
{
	public UILabel TitleLabel;

	public UILabel CostLabel;

	public UILabel DescLabel;

	public UITexture PortraitTexture;

	public UIWidget Hovered;

	public GameObject Collider;

	private CharacterStats PartyMemberStats;

	private void OnEnable()
	{
		Refresh();
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnChildHover));
		Hovered.alpha = 0f;
	}

	private void OnChildClick(GameObject sender)
	{
		if (CanAfford())
		{
			bool allowAttributesAdjustment = (bool)PartyMemberStats && PartyMemberStats.GetComponent<CompanionInstanceID>() == null;
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", StringUtility.Format(GUIUtils.GetText(allowAttributesAdjustment ? 2178 : 2177), PartyMemberStats.Name(), PartyMemberStats.Level, StringUtility.Format(GUIUtils.GetText(294), Cost()))).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox snd)
			{
				if (result == UIMessageBox.Result.AFFIRMATIVE)
				{
					PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
					if ((bool)inventory)
					{
						inventory.currencyTotalValue.v -= Cost();
					}
					UIStoreManager.Instance.CancelTransaction();
					if (UIStoreManager.Instance.HideWindow())
					{
						PartyMemberStats.Level = ((!allowAttributesAdjustment) ? 1 : 0);
						PartyMemberStats.RefreshAllAbilities();
						PartyMemberStats.StealthSkill = 0;
						PartyMemberStats.AthleticsSkill = 0;
						PartyMemberStats.LoreSkill = 0;
						PartyMemberStats.MechanicsSkill = 0;
						PartyMemberStats.SurvivalSkill = 0;
						PartyMemberStats.RemainingSkillPoints = 0;
						if (PartyMemberStats.Level == 0)
						{
							GameUtilities.RemoveAnimalCompanions(PartyMemberStats.gameObject);
						}
						PresetProgression component = PartyMemberStats.GetComponent<PresetProgression>();
						if ((bool)component)
						{
							int experience = PartyMemberStats.Experience;
							int level = PartyMemberStats.Level;
							component.HandlePresetProgression(forceFromStartingLevel: true);
							PartyMemberStats.Experience = experience;
							PartyMemberStats.Level = level;
						}
						else
						{
							Health component2 = PartyMemberStats.GetComponent<Health>();
							if ((bool)component2)
							{
								component2.CurrentHealth = component2.MaxHealth;
								component2.CurrentStamina = component2.MaxStamina;
							}
						}
						UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.LevelUp, PartyMemberStats.gameObject, 0, PartyMemberStats.Level + 1, PartyMemberStats.Experience);
					}
				}
			};
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(721), GUIUtils.Format(2179, Cost(), PartyMemberStats.Name(), GameState.s_playerCharacter.Inventory.currencyTotalValue));
		}
	}

	private void OnChildHover(GameObject sender, bool over)
	{
		Hovered.alpha = (over ? 1 : 0);
	}

	public int Cost()
	{
		if (!PartyMemberStats)
		{
			return 0;
		}
		return UIStoreRespecPage.CostOf(PartyMemberStats.Level);
	}

	public bool CanAfford()
	{
		return (float)GameState.s_playerCharacter.Inventory.currencyTotalValue >= (float)Cost();
	}

	public void Set(CharacterStats partyMemberStats)
	{
		PartyMemberStats = partyMemberStats;
		TitleLabel.text = GUIUtils.Format(2181, PartyMemberStats.Name(), PartyMemberStats.Level, GUIUtils.GetClassString(PartyMemberStats.CharacterClass, PartyMemberStats.Gender));
		CostLabel.text = GUIUtils.Format(466, Cost());
		DescLabel.text = "";
		Portrait component = partyMemberStats.GetComponent<Portrait>();
		if ((bool)component)
		{
			PortraitTexture.material = null;
			PortraitTexture.mainTexture = component.TextureSmall;
		}
		Refresh();
	}

	private void Refresh()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			CostLabel.color = ((!CanAfford()) ? UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.ERROR) : Color.white);
		}
		else
		{
			CostLabel.color = Color.white;
		}
	}
}
