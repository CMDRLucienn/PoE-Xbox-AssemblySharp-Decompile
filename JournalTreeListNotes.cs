public class JournalTreeListNotes : ITreeListContentWithChildren, ITreeListContent
{
	public string GetTreeListDisplayName()
	{
		return GUIUtils.GetText(61);
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(GUIUtils.GetText(1835), disabled: false).OnClick += UIJournalManager.Instance.OnCreateNewNote;
		intoItem.AddChild("");
		foreach (NotesPage note in NotesManager.Instance.Notes)
		{
			intoItem.AddChild(note);
		}
	}
}
