using System;
using System.Collections.Generic;
using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class ExperienceTable
{
	public const int MAJOR_QUEST_WEIGHT = 4;

	public const int REGULAR_QUEST_WEIGHT = 3;

	public const int MINOR_QUEST_WEIGHT = 1;

	private int mMaxLevel;

	private ExperienceTableData[] mLevelData;

	private List<ExperienceSerializerPacket> mSerializedList = new List<ExperienceSerializerPacket>();

	private Dictionary<string, ExperienceSerializerPacket> mQuestData = new Dictionary<string, ExperienceSerializerPacket>();

	public List<ExperienceSerializerPacket> SerializedList
	{
		get
		{
			mSerializedList.Clear();
			foreach (ExperienceSerializerPacket value in mQuestData.Values)
			{
				mSerializedList.Add(value);
			}
			return mSerializedList;
		}
		set
		{
			mSerializedList = value;
		}
	}

	public void InitializeExperienceTable(int maxLevel)
	{
		mMaxLevel = maxLevel;
		mLevelData = new ExperienceTableData[maxLevel];
		for (int i = 0; i < maxLevel; i++)
		{
			mLevelData[i] = new ExperienceTableData();
			mLevelData[i].Level = i + 1;
			object component = mLevelData[i];
			DataManager.AdjustFromData(ref component);
		}
		mQuestData.Clear();
	}

	public void RegisterQuest(Quest quest)
	{
		ExperienceTableData experienceData = GetExperienceData(quest);
		if (experienceData != null)
		{
			experienceData.QuestCount++;
			experienceData.QuestTotalWeight += GetQuestWeight(quest);
			mQuestData.Add(quest.Filename, new ExperienceSerializerPacket(quest.Filename, 0, 0));
		}
	}

	public void Restored(Dictionary<string, Quest> quests)
	{
		try
		{
			for (int i = 0; i < mLevelData.Length; i++)
			{
				mLevelData[i].QuestsComplete = 0;
				mLevelData[i].TotalExperienceGiven = 0;
				mLevelData[i].TotalStrongholdTurnsGiven = 0f;
			}
		}
		catch (Exception ex)
		{
			if (mLevelData == null)
			{
				Debug.LogWarning("Caught: ExperienceTable.cs > 'ExperienceTableData[] mLevelData' is null\n" + ex.StackTrace + "\n" + ex.Message);
			}
			else
			{
				Debug.LogWarning("Caught: " + ex.StackTrace + "\n" + ex.Message);
			}
		}
		foreach (ExperienceSerializerPacket mSerialized in mSerializedList)
		{
			if (mQuestData.ContainsKey(mSerialized.Name) && quests.ContainsKey(mSerialized.Name))
			{
				mQuestData[mSerialized.Name] = mSerialized;
				Quest quest = quests[mSerialized.Name];
				ExperienceTableData experienceData = GetExperienceData(quest);
				if (quest.IsComplete())
				{
					experienceData.QuestsComplete++;
				}
				experienceData.TotalExperienceGiven += mSerialized.ExperienceGiven;
				experienceData.TotalStrongholdTurnsGiven += mSerialized.StrongholdTurnsGiven;
			}
		}
	}

	public void CompleteQuestObjective(Quest quest, ObjectiveNode completedObjective)
	{
		ExperienceTableData experienceData = GetExperienceData(quest);
		ExperienceSerializerPacket questPacket = GetQuestPacket(quest);
		if (experienceData != null && questPacket != null)
		{
			int questWeight = GetQuestWeight(quest);
			int num = TotalObjectiveWeight(quest);
			int experienceWeight = completedObjective.ExperienceWeight;
			int num2 = (((float)experienceData.QuestTotalWeight != 0f) ? (experienceData.Experience / experienceData.QuestTotalWeight * questWeight) : 0);
			int num3 = (((float)num != 0f) ? (num2 / num * experienceWeight) : 0);
			experienceData.TotalExperienceGiven += num3;
			questPacket.ExperienceGiven += num3;
			float num4 = (((float)experienceData.QuestTotalWeight == 0f) ? 0f : ((float)experienceData.StrongholdTurns / (float)experienceData.QuestTotalWeight * (float)questWeight));
			float num5 = (((float)num == 0f) ? 0f : (num4 / (float)num * (float)experienceWeight));
			float totalStrongholdTurnsGiven = experienceData.TotalStrongholdTurnsGiven;
			experienceData.TotalStrongholdTurnsGiven += num5;
			questPacket.StrongholdTurnsGiven += num5;
			if (num3 >= 0)
			{
				PartyHelper.AssignXPToParty(num3);
			}
			int turns = Mathf.FloorToInt(experienceData.TotalStrongholdTurnsGiven) - Mathf.FloorToInt(totalStrongholdTurnsGiven);
			AssignStrongholdTurns(turns);
		}
	}

	public void CompleteQuest(Quest quest)
	{
		ExperienceTableData experienceData = GetExperienceData(quest);
		ExperienceSerializerPacket questPacket = GetQuestPacket(quest);
		if (experienceData != null && questPacket != null)
		{
			experienceData.QuestsComplete++;
			int questWeight = GetQuestWeight(quest);
			int num = experienceData.Experience / experienceData.QuestTotalWeight * questWeight;
			float num2 = (float)experienceData.StrongholdTurns / (float)experienceData.QuestTotalWeight * (float)questWeight;
			int num3 = num - questPacket.ExperienceGiven;
			float num4 = num2 - questPacket.StrongholdTurnsGiven;
			experienceData.TotalExperienceGiven += num3;
			questPacket.ExperienceGiven += num3;
			float totalStrongholdTurnsGiven = experienceData.TotalStrongholdTurnsGiven;
			experienceData.TotalStrongholdTurnsGiven += num4;
			questPacket.StrongholdTurnsGiven += num4;
			if (num3 >= 0)
			{
				PartyHelper.AssignXPToParty(num3);
				Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1871), PartyHelper.AddBonusXP(num) * PartyHelper.NumPartyMembers), Color.green, Console.ConsoleState.Both);
			}
			int turns = Mathf.FloorToInt(experienceData.TotalStrongholdTurnsGiven) - Mathf.FloorToInt(totalStrongholdTurnsGiven);
			AssignStrongholdTurns(turns);
		}
	}

	private void AssignStrongholdTurns(int turns)
	{
		if (turns >= 0 && GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				stronghold.AddTurnsByObjective(turns);
			}
		}
	}

	private ExperienceTableData GetExperienceData(Quest quest)
	{
		if (quest.GetNode(0) is QuestNode questNode)
		{
			return GetExperienceData(questNode.ExperienceLevel);
		}
		return null;
	}

	private ExperienceSerializerPacket GetQuestPacket(Quest quest)
	{
		if (mQuestData.ContainsKey(quest.Filename))
		{
			return mQuestData[quest.Filename];
		}
		return null;
	}

	private ExperienceTableData GetExperienceData(int level)
	{
		if (level > 0 && level <= mMaxLevel)
		{
			return mLevelData[level - 1];
		}
		return null;
	}

	private int GetQuestWeight(Quest quest)
	{
		QuestNode questNode = quest.GetNode(0) as QuestNode;
		return (new int[3] { 1, 3, 4 })[(int)questNode.ExperienceType];
	}

	private int TotalObjectiveWeight(Quest quest)
	{
		int num = 1;
		int count = quest.QuestData.Nodes.Count;
		for (int i = 0; i < count; i++)
		{
			if (quest.QuestData.Nodes[i] is ObjectiveNode objectiveNode && objectiveNode.ExperienceWeight > 0)
			{
				num += objectiveNode.ExperienceWeight;
			}
		}
		return num;
	}
}
