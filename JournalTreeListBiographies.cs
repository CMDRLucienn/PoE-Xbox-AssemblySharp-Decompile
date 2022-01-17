public class JournalTreeListBiographies : ITreeListContentWithChildren, ITreeListContent
{
	private static UIJournalBioText.BiographyCategory s_BiographyBioItem = new UIJournalBioText.BiographyCategory(UIJournalBioText.Mode.Biography);

	private static UIJournalBioText.BiographyCategory s_BiographyVisionItem = new UIJournalBioText.BiographyCategory(UIJournalBioText.Mode.Visions);

	public string GetTreeListDisplayName()
	{
		return GUIUtils.GetText(59);
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(s_BiographyBioItem);
		if (GameGlobalVariables.IsPlayerWatcher())
		{
			intoItem.AddChild(s_BiographyVisionItem);
		}
	}
}
