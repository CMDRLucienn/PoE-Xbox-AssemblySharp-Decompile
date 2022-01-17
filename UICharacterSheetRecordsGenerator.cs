using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UICharacterSheetRecordsGenerator : MonoBehaviour
{
	public UIDraggablePanel RecordsPanel;

	public void Reload(bool personal)
	{
		StringBuilder stringBuilder = new StringBuilder();
		RecordKeeper.Instance.FindCompanions();
		RecordsPanel.ResetPosition();
		if (!personal)
		{
			if (GameState.s_playerCharacter == null || GameState.s_playerCharacter.gameObject == null)
			{
				return;
			}
			stringBuilder.AppendGuiFormat(389, CharacterStats.Name(GameState.s_playerCharacter.gameObject));
			stringBuilder.AppendLine();
			int[] array = Enum.GetValues(typeof(Disposition.Axis)) as int[];
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				int rank = ReputationManager.Instance.PlayerDisposition.GetRank((Disposition.Axis)i);
				if (rank != 0)
				{
					flag = true;
					stringBuilder.Append("  [" + NGUITools.EncodeColor(UIGlobalColor.Instance.DullGreen) + "]" + FactionUtils.GetDispositionAxisString((Disposition.Axis)i));
					stringBuilder.Append("[-]: ");
					stringBuilder.AppendLine(rank.ToString());
				}
			}
			if (!flag)
			{
				stringBuilder.AppendLine("  " + GUIUtils.GetText(343));
			}
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				string dispositionsString = Religion.Instance.GetDispositionsString(component, positive: true);
				string dispositionsString2 = Religion.Instance.GetDispositionsString(component, positive: false);
				if (!string.IsNullOrEmpty(dispositionsString) || !string.IsNullOrEmpty(dispositionsString2))
				{
					stringBuilder.AppendLine();
					if (!string.IsNullOrEmpty(dispositionsString))
					{
						stringBuilder.AppendLine("  " + AttackBase.FormatWC(GUIUtils.GetText(1877), dispositionsString));
					}
					if (!string.IsNullOrEmpty(dispositionsString2))
					{
						stringBuilder.AppendLine("  " + AttackBase.FormatWC(GUIUtils.GetText(1878), dispositionsString2));
					}
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(GUIUtils.GetText(390));
			flag = false;
			Reputation[] factions = ReputationManager.Instance.Factions;
			foreach (Reputation reputation in factions)
			{
				Reputation.RankType rankType;
				int rank2 = reputation.GetRank(out rankType);
				if (rank2 != 0)
				{
					flag = true;
					stringBuilder.Append("  [" + NGUITools.EncodeColor(UIGlobalColor.Instance.DullGreen) + "]");
					stringBuilder.Append(reputation.Name.GetText());
					stringBuilder.Append("[-]: ");
					string reputationRankString = FactionUtils.GetReputationRankString(rankType);
					if (rankType != 0)
					{
						reputationRankString = StringUtility.Format(GUIUtils.GetFactionRepStrengthString(rank2), reputationRankString);
						stringBuilder.AppendLine(reputation.Title + " (" + reputationRankString + ")");
					}
					else
					{
						stringBuilder.AppendLine(reputationRankString);
					}
				}
			}
			if (!flag)
			{
				stringBuilder.AppendLine("  " + GUIUtils.GetText(343));
			}
			stringBuilder.AppendLine();
			for (int k = 0; k < 13; k++)
			{
				stringBuilder.Append(RecordAggregator.GetPartyStatLine((RecordAggregator.PartyStat)k));
				if (k != 12)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
			}
		}
		else
		{
			if (UICharacterSheetManager.Instance.SelectedCharacter == null)
			{
				return;
			}
			for (int l = 0; l < 10; l++)
			{
				stringBuilder.Append(RecordAggregator.GetPersonalStatLine((RecordAggregator.PersonalStat)l, UICharacterSheetManager.Instance.SelectedCharacter.GetComponent<PartyMemberAI>()));
				if (l != 9)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
			}
		}
		stringBuilder.AppendLine();
		GetComponent<UILabel>().text = stringBuilder.ToString();
		RecordsPanel.ResetPosition();
	}
}
