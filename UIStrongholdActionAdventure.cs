using System;
using System.Text;
using UnityEngine;

public class UIStrongholdActionAdventure : UIStrongholdExpandableItem
{
	public UILabel AssignmentLabel;

	public UIMultiSpriteImageButton ActionButton;

	private StrongholdAdventure m_Adventure;

	private int m_Index;

	private void Start()
	{
		UIMultiSpriteImageButton actionButton = ActionButton;
		actionButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(actionButton.onClick, new UIEventListener.VoidDelegate(OnAction));
	}

	private void OnAction(GameObject sender)
	{
		if (m_Adventure.Adventurer == null)
		{
			UIStrongholdCompanionPicker.Instance.ShowWindow();
			UIStrongholdCompanionPicker instance = UIStrongholdCompanionPicker.Instance;
			instance.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Combine(instance.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendCompanion));
		}
		else
		{
			UIStrongholdManager.Instance.Stronghold.AbortAndMakeAvailable(m_Adventure.Adventurer);
		}
		Reload();
	}

	private void OnSendCompanion(UIMessageBox.Result result, GameObject selected)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE && (bool)selected)
		{
			UIStrongholdManager.Instance.Stronghold.EngageAdventure(m_Adventure, selected);
		}
		Reload();
		UIStrongholdCompanionPicker instance = UIStrongholdCompanionPicker.Instance;
		instance.OnDialogEnd = (UIStrongholdCompanionPicker.OnEndDialog)Delegate.Remove(instance.OnDialogEnd, new UIStrongholdCompanionPicker.OnEndDialog(OnSendCompanion));
	}

	public override void Reload()
	{
		if (m_Adventure != null)
		{
			Set(m_Adventure, m_Index);
		}
	}

	public void Set(StrongholdAdventure adventure, int index)
	{
		m_Adventure = adventure;
		m_Index = index;
		GUIStringLabel gUIStringLabel = ComponentUtils.GetComponent<GUIStringLabel>(ActionButton.Label);
		if (gUIStringLabel == null)
		{
			gUIStringLabel = ActionButton.Label.gameObject.AddComponent<GUIStringLabel>();
		}
		if (adventure.Adventurer != null)
		{
			gUIStringLabel.SetString(677);
		}
		else
		{
			gUIStringLabel.SetString(673);
		}
		NameLabel.text = adventure.GetTitle(UIStrongholdManager.Instance.Stronghold);
		if ((bool)adventure.Adventurer)
		{
			AssignmentLabel.text = GUIUtils.Format(707, CharacterStats.Name(adventure.Adventurer));
		}
		else
		{
			AssignmentLabel.text = "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GUIUtils.GetText(1312));
		stringBuilder.Append(": ");
		stringBuilder.Append(adventure.AbstractRewardString());
		stringBuilder.Append(".");
		stringBuilder.AppendLine();
		if (adventure.Adventurer == null)
		{
			stringBuilder.Append(GUIUtils.Format(1306, new EternityTimeInterval((int)adventure.SerializedOfferExpires).FormatNonZero(2)));
			stringBuilder.Append(". ");
			stringBuilder.AppendLine(GUIUtils.Format(1634, GUIUtils.Format(1900, adventure.Duration)));
		}
		else
		{
			int adventureEndTurn = UIStrongholdManager.Instance.Stronghold.GetAdventureEndTurn(adventure);
			stringBuilder.AppendLine(GUIUtils.GetText(1858) + " " + GUIUtils.Format(1900, adventureEndTurn - Stronghold.Instance.CurrentTurn));
		}
		if (adventure.PremadeAdventureIndex >= 0)
		{
			stringBuilder.Append(UIStrongholdManager.Instance.Stronghold.PremadeAdventures.Adventures[adventure.PremadeAdventureIndex].Description.GetText());
		}
		SetDescriptionText(stringBuilder.ToString());
	}
}
