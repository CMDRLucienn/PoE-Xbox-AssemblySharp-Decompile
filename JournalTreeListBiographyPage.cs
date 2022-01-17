public class JournalTreeListBiographyPage : ITreeListContentWithChildren, ITreeListContent
{
	private static JournalTreeListBiographies s_Content = new JournalTreeListBiographies();

	public string GetTreeListDisplayName()
	{
		return "";
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(s_Content);
	}
}
