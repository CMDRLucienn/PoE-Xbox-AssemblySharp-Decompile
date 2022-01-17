using System.Collections.Generic;

public class JournalTreeListBestiary : ITreeListContentWithChildren, ITreeListContent
{
	public string GetTreeListDisplayName()
	{
		return GUIUtils.GetText(302);
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		List<ITreeListContent> list = new List<ITreeListContent>();
		if (BestiaryManager.Instance.ReferenceList.Prefabs != null)
		{
			BestiaryReference[] prefabs = BestiaryManager.Instance.ReferenceList.Prefabs;
			foreach (BestiaryReference bestiaryReference in prefabs)
			{
				if (bestiaryReference.IsTopLevel && BestiaryManager.Instance.GetEntryVisible(bestiaryReference))
				{
					list.Add(bestiaryReference);
				}
			}
		}
		if (BestiaryManager.Instance.ReferenceList.TopLevelEntries != null)
		{
			BestiaryParent[] topLevelEntries = BestiaryManager.Instance.ReferenceList.TopLevelEntries;
			foreach (BestiaryParent bestiaryParent in topLevelEntries)
			{
				if (bestiaryParent.IsVisible())
				{
					list.Add(bestiaryParent);
				}
			}
		}
		if (list.Count > 0)
		{
			list.Sort((ITreeListContent x, ITreeListContent y) => x.GetTreeListDisplayName().CompareTo(y.GetTreeListDisplayName()));
			{
				foreach (ITreeListContent item in list)
				{
					intoItem.AddChild(item, defaultExpand: false);
				}
				return;
			}
		}
		intoItem.AddChild(UITreeList.NoChildrenDefaultString.GetText());
	}
}
