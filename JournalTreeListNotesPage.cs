public class JournalTreeListNotesPage : ITreeListContentWithChildren, ITreeListContent
{
	private static JournalTreeListNotes s_Notes = new JournalTreeListNotes();

	public string GetTreeListDisplayName()
	{
		return "";
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(s_Notes);
	}
}
