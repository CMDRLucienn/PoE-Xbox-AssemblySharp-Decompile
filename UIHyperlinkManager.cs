using UnityEngine;

public static class UIHyperlinkManager
{
	public static ITooltipContent GetTooltip(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return null;
		}
		if (!SplitUrl(url, out var protocol, out var link))
		{
			return null;
		}
		switch (protocol)
		{
		case "glossary":
			return Glossary.Instance.GetEntryByName(link, allowRedirect: true);
		case "ability":
			return null;
		case "item":
			return GameResources.LoadPrefab<Item>(link, instantiate: false);
		case "itemmod":
			return null;
		case "buffvalue":
		case "debuffvalue":
			return new StringTooltip(link);
		case "completeadventure":
			return null;
		default:
			Debug.LogError("Unrecognized protocol '" + protocol + "'.");
			return null;
		}
	}

	public static void FollowLink(string url)
	{
		if (string.IsNullOrEmpty(url) || !SplitUrl(url, out var protocol, out var link))
		{
			return;
		}
		switch (protocol)
		{
		case "glossary":
			UIJournalManager.Instance.ShowGlossaryEntry(Glossary.Instance.GetEntryByName(link, allowRedirect: true));
			break;
		case "ability":
		{
			GenericAbility genericAbility = GameResources.LoadPrefab<GenericAbility>(link, instantiate: false);
			if ((bool)genericAbility)
			{
				UIItemInspectManager.ExamineNoDescription(genericAbility);
			}
			break;
		}
		case "item":
		{
			Item item = GameResources.LoadPrefab<Item>(link, instantiate: false);
			if ((bool)item)
			{
				UIItemInspectManager.Examine(item);
			}
			break;
		}
		case "itemmod":
		{
			ItemMod itemMod = GameResources.LoadPrefab<ItemMod>(link, instantiate: false);
			if ((bool)itemMod)
			{
				UIItemInspectManager.Examine(itemMod);
			}
			break;
		}
		case "completeadventure":
		{
			if (int.TryParse(link, out var result))
			{
				UIStrongholdAdventureManager.Instance.ShowAdventure(Stronghold.Instance.GetCompleteAdventures[result]);
			}
			break;
		}
		default:
			Debug.LogError("Unrecognized protocol '" + protocol + "'.");
			break;
		case "buffvalue":
			break;
		case "debuffvalue":
			break;
		}
	}

	private static bool SplitUrl(string url, out string protocol, out string link)
	{
		int num = url.IndexOf("://");
		if (num < 0)
		{
			Debug.LogError("No protocol found in URL '" + url + "'.");
			protocol = "";
			link = "";
			return false;
		}
		protocol = url.Substring(0, num);
		link = url.Substring(num + 3);
		return true;
	}
}
