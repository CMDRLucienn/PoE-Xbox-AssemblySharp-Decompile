using System;
using UnityEngine;

[Serializable]
public class CyclopediaEntry : ITreeListContent
{
	public string DesignerNote;

	public DatabaseString Title = new DatabaseString(DatabaseString.StringTableType.Cyclopedia);

	public DatabaseString Text = new DatabaseString(DatabaseString.StringTableType.Cyclopedia);

	public Texture2D Image;

	public RecipeRequirement[] VisibilityRequirements;

	public bool IsVisible()
	{
		return RecipeRequirement.CheckRequirements(VisibilityRequirements);
	}

	public string GetTreeListDisplayName()
	{
		return Title.GetText();
	}
}
