using System;
using UnityEngine;

public class UIStoreRecruitRow : MonoBehaviour
{
	public UILabel TitleLabel;

	public UILabel CostLabel;

	public UILabel DescLabel;

	public UIWidget Hovered;

	public GameObject Collider;

	private int m_Level;

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
			if (SpaceToHire())
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", StringUtility.Format(GUIUtils.GetText(1824), m_Level, StringUtility.Format(GUIUtils.GetText(294), Cost()))).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox snd)
				{
					if (result == UIMessageBox.Result.AFFIRMATIVE)
					{
						UIStoreManager.Instance.CancelTransaction();
						if (UIStoreManager.Instance.HideWindow())
						{
							Scripts.OpenCharacterCreationNewCompanion(Cost(), m_Level);
						}
					}
				};
			}
			else
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.Format(1889, PartyMemberAI.MaxAdventurers));
			}
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(721), GUIUtils.Format(836, m_Level));
		}
	}

	private void OnChildHover(GameObject sender, bool over)
	{
		Hovered.alpha = (over ? 1 : 0);
	}

	public int Cost()
	{
		return UIStoreRecruitPage.CostOf(m_Level);
	}

	public bool SpaceToHire()
	{
		int num = 0;
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (onlyPrimaryPartyMember.IsAdventurer)
			{
				num++;
			}
		}
		foreach (StoredCharacterInfo companion in Stronghold.Instance.GetCompanions())
		{
			if (companion.IsAdventurer)
			{
				num++;
			}
		}
		return num < PartyMemberAI.MaxAdventurers;
	}

	public bool CanAfford()
	{
		return (float)GameState.s_playerCharacter.Inventory.currencyTotalValue >= (float)Cost();
	}

	public void Set(int level)
	{
		m_Level = level;
		TitleLabel.text = GUIUtils.Format(829, m_Level, Gender.Neuter);
		CostLabel.text = GUIUtils.Format(466, Cost());
		DescLabel.text = "";
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

	public void HideIfAbove(int above)
	{
		base.gameObject.SetActive(m_Level <= above);
	}
}
