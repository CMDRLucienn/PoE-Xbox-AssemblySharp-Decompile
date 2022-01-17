using UnityEngine;

public class CyclopediaCategory : ScriptableObject, ITreeListContentWithChildren, ITreeListContent
{
	public DatabaseString Name = new DatabaseString(DatabaseString.StringTableType.Cyclopedia);

	public CyclopediaEntry[] Entries = new CyclopediaEntry[0];

	public string GetTreeListDisplayName()
	{
		return Name.GetText();
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		if (Entries.Length != 0)
		{
			CyclopediaEntry[] entries = Entries;
			foreach (CyclopediaEntry cyclopediaEntry in entries)
			{
				if (cyclopediaEntry != null && cyclopediaEntry.IsVisible())
				{
					intoItem.AddChild(cyclopediaEntry);
				}
			}
		}
		else
		{
			intoItem.AddChild(UITreeList.NoChildrenDefaultString.GetText());
		}
	}
}
