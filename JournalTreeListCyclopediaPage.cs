using System;
using System.Linq;

public class JournalTreeListCyclopediaPage : ITreeListContentWithChildren, ITreeListContent
{
	private static JournalTreeListBestiary s_BestiaryContent = new JournalTreeListBestiary();

	public string GetTreeListDisplayName()
	{
		return "";
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		intoItem.AddChild(s_BestiaryContent, defaultExpand: false);
		CyclopediaCategory[] categories = CyclopediaManager.Instance.EntryList.Categories;
		for (int i = 0; i < categories.Length; i++)
		{
			intoItem.AddChild(categories[i], defaultExpand: false);
		}
		foreach (Glossary.GlossaryCategory item in from Glossary.GlossaryCategory enm in Enum.GetValues(typeof(Glossary.GlossaryCategory))
			orderby Glossary.Instance.GetCategoryName(enm)
			select enm)
		{
			intoItem.AddChild(new JournalTreeListGlossaryCategory(item), defaultExpand: false);
		}
	}
}
