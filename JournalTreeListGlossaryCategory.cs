using System.Collections.Generic;
using System.Linq;

public class JournalTreeListGlossaryCategory : ITreeListContentWithChildren, ITreeListContent
{
	private Glossary.GlossaryCategory m_Category;

	public JournalTreeListGlossaryCategory(Glossary.GlossaryCategory category)
	{
		m_Category = category;
	}

	public string GetTreeListDisplayName()
	{
		return GUIUtils.GetText(Glossary.Instance.GetCategoryNameGuiId(m_Category));
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		IEnumerable<ITreeListContent> children = GetChildren();
		if (children.Any())
		{
			foreach (ITreeListContent item in children)
			{
				intoItem.AddChild(item);
			}
			return;
		}
		intoItem.AddChild(UITreeList.NoChildrenDefaultString.GetText());
	}

	public IEnumerable<ITreeListContent> GetChildren()
	{
		return (from entry in Glossary.Instance.glossaryEntries.Entries
			where (bool)entry && entry.ShowInJournal && entry.IsVisible && entry.Category == m_Category
			orderby entry.GetTreeListDisplayName()
			select entry).Cast<ITreeListContent>();
	}
}
