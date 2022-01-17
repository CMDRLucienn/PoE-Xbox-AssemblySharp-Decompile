using System.Linq;
using UnityEngine;

public class GlossaryEntry : ScriptableObject, ITreeListContent, ITooltipContent
{
	public DatabaseString Title = new DatabaseString(DatabaseString.StringTableType.Cyclopedia);

	public DatabaseString Body = new DatabaseString(DatabaseString.StringTableType.Cyclopedia);

	public Glossary.GlossaryCategory Category;

	public bool ShowInJournal = true;

	public bool CaseSensitive;

	public ProductConfiguration.Package RequiresPackages = ProductConfiguration.Package.BaseGame;

	public GlossaryEntry[] LinkedEntries;

	public bool IsVisible => (RequiresPackages & ProductConfiguration.ActivePackage) != 0;

	public bool IsRedirect => !Body.IsValidString;

	public GlossaryEntry[] VisibleLinkedEntries => LinkedEntries.Where((GlossaryEntry x) => x.ShowInJournal).ToArray();

	public string GetTreeListDisplayName()
	{
		return Title.GetText();
	}

	public string GetTooltipContent(GameObject owner)
	{
		return Body.GetText();
	}

	public string GetTooltipName(GameObject owner)
	{
		return Title.GetText();
	}

	public Texture GetTooltipIcon()
	{
		return null;
	}
}
