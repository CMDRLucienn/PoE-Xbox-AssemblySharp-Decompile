using System.Collections.Generic;

public class JournalTreeListQuests : ITreeListContentWithChildren, ITreeListContent
{
	private QuestType m_QuestType;

	public JournalTreeListQuests(QuestType questType)
	{
		m_QuestType = questType;
	}

	public string GetTreeListDisplayName()
	{
		return m_QuestType switch
		{
			QuestType.MainQuest => QuestManager.Instance.QuestlineName.GetText(), 
			QuestType.Quest => GUIUtils.GetText(58), 
			QuestType.Task => GUIUtils.GetText(117), 
			QuestType.MainQuestPX1 => QuestManager.Instance.PX1_QuestlineName.GetText(), 
			QuestType.MainQuestPX2 => QuestManager.Instance.PX2_QuestlineName.GetText(), 
			_ => "", 
		};
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		QuestManager questManager = QuestManager.Instance;
		List<Quest> incompleteQuests = questManager.GetIncompleteQuests(m_QuestType);
		incompleteQuests.Sort((Quest x, Quest y) => questManager.GetQuestStartTime(y).CompareTo(questManager.GetQuestStartTime(x)));
		List<Quest> completeQuests = questManager.GetCompleteQuests(m_QuestType);
		completeQuests.Sort(delegate(Quest x, Quest y)
		{
			EternityDateTime questStartTime = questManager.GetQuestStartTime(x);
			return questManager.GetQuestStartTime(y).CompareTo(questStartTime);
		});
		if (incompleteQuests.Count > 0 || completeQuests.Count > 0)
		{
			foreach (Quest item in incompleteQuests)
			{
				intoItem.AddChild(item);
			}
			{
				foreach (Quest item2 in completeQuests)
				{
					intoItem.AddChild(item2).SetVisualDisabled(state: true);
				}
				return;
			}
		}
		intoItem.AddChild(GetNoChildrenString());
	}

	public string GetNoChildrenString()
	{
		switch (m_QuestType)
		{
		case QuestType.MainQuest:
		case QuestType.MainQuestPX1:
		case QuestType.MainQuestPX2:
			return GUIUtils.GetText(119);
		case QuestType.Quest:
			return GUIUtils.GetText(120);
		case QuestType.Task:
			return GUIUtils.GetText(121);
		default:
			return GUIUtils.GetText(120);
		}
	}
}
