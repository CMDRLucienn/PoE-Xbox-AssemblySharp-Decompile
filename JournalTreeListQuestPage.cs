public class JournalTreeListQuestPage : ITreeListContentWithChildren, ITreeListContent
{
	private JournalTreeListQuests m_MainQuest = new JournalTreeListQuests(QuestType.MainQuest);

	private JournalTreeListQuests m_MainQuestPX1 = new JournalTreeListQuests(QuestType.MainQuestPX1);

	private JournalTreeListQuests m_MainQuestPX2 = new JournalTreeListQuests(QuestType.MainQuestPX2);

	private JournalTreeListQuests m_Quests = new JournalTreeListQuests(QuestType.Quest);

	private JournalTreeListQuests m_Tasks = new JournalTreeListQuests(QuestType.Task);

	public string GetTreeListDisplayName()
	{
		return "";
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(m_MainQuest);
		if (GameUtilities.HasPX1())
		{
			intoItem.AddChild(m_MainQuestPX1);
		}
		if (GameUtilities.HasPX2())
		{
			intoItem.AddChild(m_MainQuestPX2);
		}
		intoItem.AddChild(m_Quests);
		intoItem.AddChild(m_Tasks);
	}
}
