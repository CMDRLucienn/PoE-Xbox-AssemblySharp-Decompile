using System;
using System.Collections.Generic;

[Serializable]
public class BestiaryParent : ITreeListContentWithChildren, ITreeListContent
{
	public string Tag;

	public CharacterDatabaseString Name;

	public CyclopediaDatabaseString Description;

	[ResourcesImageProperty]
	public string Image;

	public bool IsVisible()
	{
		foreach (BestiaryReference child in BestiaryManager.Instance.GetChildren(Tag))
		{
			if (BestiaryManager.Instance.GetEntryVisible(child))
			{
				return true;
			}
		}
		return false;
	}

	public string GetTreeListDisplayName()
	{
		return Name.GetText(Gender.Neuter);
	}

	public void LoadTreeListChildren(UITreeListItem intoItem)
	{
		List<BestiaryReference> children = BestiaryManager.Instance.GetChildren(Tag);
		children.Sort((BestiaryReference x, BestiaryReference y) => x.GetTreeListDisplayName().CompareTo(y.GetTreeListDisplayName()));
		foreach (BestiaryReference item in children)
		{
			if (BestiaryManager.Instance.GetEntryVisible(item))
			{
				intoItem.AddChild(item);
			}
		}
	}
}
