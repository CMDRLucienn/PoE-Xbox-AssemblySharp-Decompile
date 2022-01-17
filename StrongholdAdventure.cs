using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class StrongholdAdventure
{
	public enum Type
	{
		None,
		Minor,
		Average,
		Major,
		Grand,
		Legendary,
		Count
	}

	public enum RewardType
	{
		XP,
		Copper,
		MinorItem,
		AverageItem,
		MajorItem,
		GrandItem,
		MinorRepBonus,
		AverageRepBonus,
		MajorRepBonus,
		GrandRepBonus,
		SpecificItem
	}

	[Serializable]
	public class Reward
	{
		public RewardType m_type = RewardType.AverageItem;

		public int m_minValue;

		public int m_maxValue;

		[AssetByName(typeof(Item))]
		public string SpecificItem;

		public RewardType RewardType
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type = value;
			}
		}

		public int MinValue
		{
			get
			{
				return m_minValue;
			}
			set
			{
				m_minValue = value;
			}
		}

		public int MaxValue
		{
			get
			{
				return m_maxValue;
			}
			set
			{
				m_maxValue = value;
			}
		}

		public string SpecificItemName => Path.GetFileNameWithoutExtension(SpecificItem);
	}

	public static int PercentXPFromPlayer = 75;

	public Type AdventureType;

	private float OfferExpires;

	public int Duration;

	public int NumRewards;

	public Reward[] RewardList;

	private int m_PremadeAdventureIndex = -1;

	private StoredCharacterInfo m_adventurer;

	private Guid m_serializedAdventurer = Guid.Empty;

	private int m_deferredXP;

	public Type SerializedAdventureType
	{
		get
		{
			return AdventureType;
		}
		set
		{
			AdventureType = value;
		}
	}

	public float SerializedOfferExpires
	{
		get
		{
			return OfferExpires;
		}
		set
		{
			OfferExpires = value;
		}
	}

	public int SerializedDuration
	{
		get
		{
			return Duration;
		}
		set
		{
			Duration = value;
		}
	}

	public int SerializedNumRewards
	{
		get
		{
			return NumRewards;
		}
		set
		{
			NumRewards = value;
		}
	}

	public Reward[] SerializedRewardList
	{
		get
		{
			return RewardList;
		}
		set
		{
			RewardList = value;
		}
	}

	public int PremadeAdventureIndex
	{
		get
		{
			return m_PremadeAdventureIndex;
		}
		set
		{
			m_PremadeAdventureIndex = value;
		}
	}

	[ExcludeFromSerialization]
	public StoredCharacterInfo Adventurer
	{
		get
		{
			return m_adventurer;
		}
		set
		{
			m_adventurer = value;
		}
	}

	public Guid SerializedAdventurer
	{
		get
		{
			if (m_adventurer != null)
			{
				m_serializedAdventurer = m_adventurer.GUID;
			}
			return m_serializedAdventurer;
		}
		set
		{
			m_serializedAdventurer = value;
		}
	}

	public int DeferredXP
	{
		get
		{
			return m_deferredXP;
		}
		set
		{
			m_deferredXP = value;
		}
	}

	public static StrongholdAdventure Create(Type type, Stronghold stronghold)
	{
		if (type == Type.None)
		{
			return null;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < stronghold.PremadeAdventures.Adventures.Length; i++)
		{
			if (stronghold.PremadeAdventures.Adventures[i].AdventureType == type && !stronghold.IsPremadeAdventureComplete(i) && !stronghold.IsPremadeAdventureActive(i))
			{
				list.Add(i);
			}
		}
		StrongholdAdventure strongholdAdventure = null;
		int num = -1;
		if (list.Count > 0)
		{
			num = list[OEIRandom.Index(list.Count)];
			strongholdAdventure = stronghold.PremadeAdventures.Adventures[num];
		}
		else
		{
			StrongholdAdventure[] adventureTemplates = stronghold.AdventureTemplates;
			foreach (StrongholdAdventure strongholdAdventure2 in adventureTemplates)
			{
				if (strongholdAdventure2.AdventureType == type)
				{
					strongholdAdventure = strongholdAdventure2;
					break;
				}
			}
		}
		if (strongholdAdventure == null)
		{
			Debug.Log("Missing stronghold adventure template for type " + type);
			return null;
		}
		StrongholdAdventure strongholdAdventure3 = new StrongholdAdventure();
		strongholdAdventure3.AdventureType = type;
		strongholdAdventure3.PremadeAdventureIndex = num;
		strongholdAdventure3.OfferExpires = (float)OEIRandom.Range(1, 3) * (float)WorldTime.Instance.SecondsPerDay;
		strongholdAdventure3.Duration = strongholdAdventure.Duration;
		strongholdAdventure3.NumRewards = Mathf.Min(strongholdAdventure.NumRewards, strongholdAdventure.RewardList.Length);
		strongholdAdventure3.RewardList = new Reward[strongholdAdventure3.NumRewards];
		if (strongholdAdventure3.NumRewards == strongholdAdventure.RewardList.Length)
		{
			strongholdAdventure.RewardList.CopyTo(strongholdAdventure3.RewardList, 0);
		}
		else
		{
			List<int> list2 = new List<int>();
			for (int k = 0; k < strongholdAdventure.RewardList.Length; k++)
			{
				list2.Add(k);
			}
			for (int l = 0; l < strongholdAdventure3.NumRewards; l++)
			{
				int index = OEIRandom.Index(list2.Count);
				strongholdAdventure3.RewardList[l] = strongholdAdventure.RewardList[list2[index]];
				list2.RemoveAt(index);
			}
		}
		return strongholdAdventure3;
	}

	public string GetTitle(Stronghold stronghold)
	{
		if (PremadeAdventureIndex >= 0)
		{
			return stronghold.PremadeAdventures.Adventures[PremadeAdventureIndex].Title.GetText();
		}
		return GUIUtils.GetStrongholdAventureTypeString(AdventureType);
	}

	public void Finish(Stronghold stronghold)
	{
		if (Adventurer == null)
		{
			return;
		}
		StoredCharacterInfo component = Adventurer.GetComponent<StoredCharacterInfo>();
		if (component == null)
		{
			return;
		}
		List<string> list = new List<string>();
		bool flag = false;
		List<string> list2 = new List<string>();
		Reward[] rewardList = RewardList;
		foreach (Reward reward in rewardList)
		{
			int num = OEIRandom.Range(reward.m_minValue, reward.m_maxValue);
			switch (reward.m_type)
			{
			case RewardType.XP:
			{
				int num2 = DeferredXP * (num + PercentXPFromPlayer) / 100;
				component.Experience += num2;
				string item3 = Stronghold.Format(40, num2);
				list.Add(item3);
				list2.Add(item3);
				break;
			}
			case RewardType.Copper:
			{
				string item2 = Stronghold.Format(41);
				list.Add(item2);
				list2.Add(item2);
				break;
			}
			case RewardType.MinorItem:
				flag = true;
				list2.Add(TextUtils.Plural(GUIUtils.GetText(1289), GUIUtils.GetText(1293), num));
				break;
			case RewardType.AverageItem:
				flag = true;
				list2.Add(TextUtils.Plural(GUIUtils.GetText(1290), GUIUtils.GetText(1294), num));
				break;
			case RewardType.MajorItem:
				flag = true;
				list2.Add(TextUtils.Plural(GUIUtils.GetText(1291), GUIUtils.GetText(1295), num));
				break;
			case RewardType.GrandItem:
				flag = true;
				list2.Add(TextUtils.Plural(GUIUtils.GetText(1292), GUIUtils.GetText(1296), num));
				break;
			case RewardType.MinorRepBonus:
			{
				List<string> collection4 = GiveRewardReputation(stronghold.MinorReputationRewards, num);
				list.AddRange(collection4);
				list2.AddRange(collection4);
				break;
			}
			case RewardType.AverageRepBonus:
			{
				List<string> collection3 = GiveRewardReputation(stronghold.AverageReputationRewards, num);
				list.AddRange(collection3);
				list2.AddRange(collection3);
				break;
			}
			case RewardType.MajorRepBonus:
			{
				List<string> collection2 = GiveRewardReputation(stronghold.MajorReputationRewards, num);
				list.AddRange(collection2);
				list2.AddRange(collection2);
				break;
			}
			case RewardType.GrandRepBonus:
			{
				List<string> collection = GiveRewardReputation(stronghold.GrandReputationRewards, num);
				list.AddRange(collection);
				list2.AddRange(collection);
				break;
			}
			case RewardType.SpecificItem:
			{
				Item item = GameResources.LoadPrefab<Item>(reward.SpecificItemName, instantiate: false);
				if (reward.m_minValue < reward.m_maxValue)
				{
					list.Add(GUIUtils.Format(445, reward.m_minValue, reward.m_maxValue) + " " + item.Name);
				}
				else if (reward.m_minValue != 1)
				{
					list.Add(reward.m_minValue + " " + item.Name);
				}
				else
				{
					list.Add(item.Name);
				}
				break;
			}
			}
		}
		if (flag)
		{
			list.Add(Stronghold.Format(42));
		}
		string text = "";
		if (list.Count == 1)
		{
			text += list[0];
		}
		else if (list.Count == 2)
		{
			text += Stronghold.Format(44, list[0], list[1]);
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				text += list[j];
				if (j < list.Count - 1)
				{
					text += GUIUtils.Comma();
				}
			}
		}
		int num3 = stronghold.RecordAdventureCompletion(this, list2);
		string text2 = Stronghold.Format(10, GetTitle(stronghold), component.DisplayName, text);
		if (PremadeAdventureIndex >= 0)
		{
			text2 += GUIUtils.Format(1731, "[url=completeadventure://" + num3 + "]" + StrongholdUtils.GetText(203) + "[/url]");
		}
		stronghold.LogTurnEvent(text2, Stronghold.NotificationType.Positive);
		AchievementTracker.Instance.TrackAndIncrementIfUnique(AchievementTracker.TrackedAchievementStat.NumUniqueStrongholdAdventureTypesCompleted, AdventureType.ToString());
	}

	public bool CreateItemsAndMoney(Inventory inven, Stronghold stronghold)
	{
		if (inven.IsFull)
		{
			return false;
		}
		Reward[] rewardList = RewardList;
		foreach (Reward reward in rewardList)
		{
			int num = OEIRandom.Range(reward.m_minValue, reward.m_maxValue);
			switch (reward.m_type)
			{
			case RewardType.Copper:
				GiveCurrencyItems(inven, stronghold, num);
				break;
			case RewardType.MinorItem:
				GiveRewardItems(inven, stronghold.MinorItemRewards, num);
				break;
			case RewardType.AverageItem:
				GiveRewardItems(inven, stronghold.AverageItemRewards, num);
				break;
			case RewardType.MajorItem:
				GiveRewardItems(inven, stronghold.MajorItemRewards, num);
				break;
			case RewardType.GrandItem:
				GiveRewardItems(inven, stronghold.GrandItemRewards, num);
				break;
			case RewardType.SpecificItem:
			{
				Item newItem = GameResources.LoadPrefab<Item>(reward.SpecificItemName, instantiate: false);
				inven.AddItem(newItem, num);
				break;
			}
			}
		}
		return true;
	}

	private void GiveCurrencyItems(Inventory inven, Stronghold stronghold, int amount)
	{
		int length = stronghold.CurrencyRewards.GetLength(0);
		if (length > 0)
		{
			int num = OEIRandom.Index(length);
			int num2 = (int)stronghold.CurrencyRewards[num].GetValue();
			if (num2 <= 0)
			{
				num2 = 1;
			}
			int num3 = amount / num2;
			if (num3 == 0)
			{
				num3 = 1;
			}
			inven.AddItem(stronghold.CurrencyRewards[num], num3);
		}
	}

	private void GiveRewardItems(Inventory inven, Item[] items, int count)
	{
		int length = items.GetLength(0);
		if (length > 0)
		{
			for (int i = 0; i < count; i++)
			{
				int num = OEIRandom.Index(length);
				inven.AddItem(items[num], 1);
			}
		}
	}

	private List<string> GiveRewardReputation(Stronghold.ReputationBonus[] bonuses, int count)
	{
		List<string> list = new List<string>();
		int length = bonuses.GetLength(0);
		if (length > 0)
		{
			for (int i = 0; i < count; i++)
			{
				int num = OEIRandom.Index(length);
				ReputationManager.Instance.AddReputation(bonuses[num].factionName, bonuses[num].axis, bonuses[num].strength);
				list.Add(GUIUtils.FormatReputationChangeStrength(bonuses[num].strength, ReputationManager.Instance.GetReputation(bonuses[num].factionName).Name.GetText()));
			}
		}
		return list;
	}

	public string AbstractRewardString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < RewardList.Length; i++)
		{
			Reward reward = RewardList[i];
			int minValue = reward.m_minValue;
			int maxValue = reward.m_maxValue;
			bool flag = false;
			string text = "";
			if (minValue == maxValue)
			{
				if (maxValue != 1)
				{
					text = maxValue.ToString();
					flag = true;
				}
			}
			else
			{
				text = GUIUtils.Format(445, minValue, maxValue);
				flag = true;
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (reward.m_type == RewardType.XP)
				{
					stringBuilder.AppendGuiFormat(1277, text);
				}
				else
				{
					stringBuilder.Append(text);
				}
				stringBuilder.Append(" ");
			}
			bool flag2 = false;
			switch (reward.m_type)
			{
			case RewardType.MinorItem:
				if (flag)
				{
					stringBuilder.Append(GUIUtils.GetText(1293));
				}
				else
				{
					stringBuilder.Append(GUIUtils.GetText(1289));
				}
				break;
			case RewardType.AverageItem:
				if (flag)
				{
					stringBuilder.Append(GUIUtils.GetText(1294));
				}
				else
				{
					stringBuilder.Append(GUIUtils.GetText(1290));
				}
				break;
			case RewardType.MajorItem:
				if (flag)
				{
					stringBuilder.Append(GUIUtils.GetText(1295));
				}
				else
				{
					stringBuilder.Append(GUIUtils.GetText(1291));
				}
				break;
			case RewardType.GrandItem:
				if (flag)
				{
					stringBuilder.Append(GUIUtils.GetText(1296));
				}
				else
				{
					stringBuilder.Append(GUIUtils.GetText(1292));
				}
				break;
			case RewardType.MinorRepBonus:
				stringBuilder.Append(GUIUtils.GetText(1301));
				break;
			case RewardType.AverageRepBonus:
				stringBuilder.Append(GUIUtils.GetText(1302));
				break;
			case RewardType.MajorRepBonus:
				stringBuilder.Append(GUIUtils.GetText(1303));
				break;
			case RewardType.GrandRepBonus:
				stringBuilder.Append(GUIUtils.GetText(1304));
				break;
			case RewardType.Copper:
				stringBuilder.Append(GUIUtils.GetText(1305));
				break;
			case RewardType.XP:
				stringBuilder.Append(GUIUtils.GetText(375));
				break;
			case RewardType.SpecificItem:
			{
				Item item = GameResources.LoadPrefab<Item>(reward.SpecificItemName, instantiate: false);
				stringBuilder.Append(item ? item.Name : "*null*");
				break;
			}
			default:
				Debug.LogError("Unknown reward type in StrongholdAdventure.AbstractRewardString");
				flag2 = true;
				break;
			}
			if (!flag2)
			{
				stringBuilder.Append(GUIUtils.Comma());
			}
		}
		if (stringBuilder.Length >= GUIUtils.Comma().Length)
		{
			stringBuilder = stringBuilder.Remove(stringBuilder.Length - GUIUtils.Comma().Length);
		}
		return stringBuilder.ToString();
	}
}
