using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Quests;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
[TypeConverter(typeof(ScriptableObjectConverter<Quest>))]
public class Quest : FlowChart, ITreeListContent
{
	private const string QUEST_PATH = "data/quests/";

	private int m_questDescriptionID;

	private static List<int> s_stateQueue = new List<int>();

	private static List<int> s_statesToEnter = new List<int>();

	private static List<int> s_statesToFail = new List<int>();

	[ExcludeFromSerialization]
	public QuestData QuestData { get; set; }

	public List<int> ActiveStates { get; set; }

	private string StringTableName { get; set; }

	public int QuestDescriptionID
	{
		get
		{
			return m_questDescriptionID;
		}
		set
		{
			if (QuestData == null)
			{
				m_questDescriptionID = 0;
			}
			else if (QuestData.GetNodeByID(0) is QuestNode questNode && questNode.AlternateDescriptionIDs.Contains(value))
			{
				m_questDescriptionID = value;
			}
			else
			{
				m_questDescriptionID = 0;
			}
		}
	}

	private QuestManager QuestManagerInstance => QuestManager.Instance;

	public QuestType GetQuestType()
	{
		return (QuestType)QuestData.QuestType;
	}

	public Quest()
	{
		ActiveStates = new List<int>();
		StringTableName = string.Empty;
	}

	public void Restored()
	{
		if (!string.IsNullOrEmpty(Filename))
		{
			OnLoad(Filename);
		}
	}

	protected override void OnLoad(string filename)
	{
		QuestData = QuestData.Load(Application.dataPath + Path.DirectorySeparatorChar + filename);
		if (QuestData == null)
		{
			Debug.LogWarning("Failed to quest file " + filename + ".");
			QuestData = new QuestData();
			base.Data = QuestData;
		}
		else
		{
			base.Data = QuestData;
			StringTableName = GetStringTableName();
			StringTableManager.LoadStringTable(StringTableName);
		}
	}

	protected override void OnUnload()
	{
		StringTableManager.UnloadStringTable(StringTableName);
	}

	private string GetStringTableName()
	{
		int num = Filename.IndexOf("data/quests/", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return string.Empty;
		}
		return "Text/Quests/" + Path.ChangeExtension(Filename.Substring(num + "data/quests/".Length), ".stringtable");
	}

	public void StartQuest()
	{
		StartQuest(0);
	}

	public void StartQuest(int questDescriptionID)
	{
		DebugOutput("Quest Started", 0);
		QuestDescriptionID = questDescriptionID;
		QuestManager.StartingQuest = true;
		EnterState(0);
		Advance();
		QuestManager.StartingQuest = false;
	}

	public bool IsQuestStateActive(int nodeID)
	{
		return ActiveStates.Contains(nodeID);
	}

	public bool IsQuestStateEnded(int nodeID)
	{
		if (IsQuestStateActive(nodeID))
		{
			return QuestManagerInstance.GetEndStateID(this) >= 0;
		}
		return true;
	}

	public bool IsQuestFailed()
	{
		return QuestManager.Instance.IsQuestFailed(this);
	}

	private void EnterState(int nodeID)
	{
		if (nodeID == -10)
		{
			return;
		}
		DebugOutput("Enter Quest State " + nodeID, 0);
		if (QuestManagerInstance.IsStateVisited(this, nodeID))
		{
			DebugOutput("Exit state because it has already been visited " + nodeID, 0);
			DetermineIfQuestHasEnded();
			return;
		}
		if (IsComplete())
		{
			DebugOutput("Exit state because the quest is already completed.", 0);
			return;
		}
		if (IsQuestFailed())
		{
			DebugOutput("Exit state because the quest was already failed.", 0);
			return;
		}
		FlowChartNode node = GetNode(nodeID);
		if (node == null)
		{
			Debug.LogError("Node does not exist in quest " + node.NodeID);
			return;
		}
		if (node is ObjectiveNode)
		{
			QuestManager.Instance.NotifyQuestUpdated(this, (ObjectiveNode)node);
		}
		else if (node is EndStateNode)
		{
			EndStateNode endStateNode = node as EndStateNode;
			QuestManager.Instance.TriggerQuestEndState(this, endStateNode.EndStateID, endStateNode.Failed);
		}
		QuestManagerInstance.SetStateVisited(this, nodeID);
		ActiveStates.Add(nodeID);
		ExecuteScripts(node.OnEnterScripts, null);
	}

	private void ExitState(int nodeID)
	{
		if (nodeID != -10)
		{
			DebugOutput("Exit Quest State " + nodeID, 0);
			FlowChartNode node = GetNode(nodeID);
			if (node is ObjectiveNode)
			{
				QuestManager.MarkObjectiveCompleted(this, node as ObjectiveNode);
				QuestManager.Instance.NotifyQuestUpdatedNoLog(this, node as ObjectiveNode);
			}
			ExecuteScripts(node.OnExitScripts, null);
			if (ActiveStates.Contains(nodeID))
			{
				ActiveStates.Remove(nodeID);
			}
		}
	}

	private void DetermineIfQuestHasEnded()
	{
		if (IsComplete())
		{
			QuestManagerInstance.EndQuest(Filename);
		}
	}

	public void Advance()
	{
		Advance(force: false);
	}

	public void Advance(bool force)
	{
		if (QuestManager.Instance.GetEndStateID(this) != -1 && !force)
		{
			return;
		}
		s_stateQueue.Clear();
		foreach (int activeState in ActiveStates)
		{
			s_stateQueue.Add(activeState);
		}
		if (GetNode(-10) != null)
		{
			s_stateQueue.Add(-10);
		}
		while (s_stateQueue.Count > 0)
		{
			int nodeID = s_stateQueue[0];
			s_stateQueue.RemoveAt(0);
			FlowChartNode node = GetNode(nodeID);
			bool flag = false;
			bool flag2 = true;
			bool flag3 = true;
			s_statesToEnter.Clear();
			s_statesToFail.Clear();
			if (node is ObjectiveNode)
			{
				flag3 = (node as ObjectiveNode).LinkEvaluation == LinkEvaluation.FirstLinkThatPasses;
			}
			else if (node is QuestNode)
			{
				flag3 = (node as QuestNode).LinkEvaluation == LinkEvaluation.FirstLinkThatPasses;
			}
			else if (node is GlobalQuestNode)
			{
				flag3 = false;
			}
			foreach (FlowChartLink link in node.Links)
			{
				FlowChartNode nodeByID = base.Data.GetNodeByID(link.ToNodeID);
				if (nodeByID == null)
				{
					continue;
				}
				QuestLink questLink = link as QuestLink;
				if (force || PassesConditionals(nodeByID, null))
				{
					if (flag3 && flag && !questLink.RequiredToExitObjective)
					{
						continue;
					}
					if (!QuestManagerInstance.IsStateVisited(this, nodeByID.NodeID))
					{
						s_stateQueue.Insert(0, nodeByID.NodeID);
						if (questLink.FailObjective)
						{
							s_statesToFail.Add(nodeByID.NodeID);
						}
					}
					s_statesToEnter.Add(nodeByID.NodeID);
					flag = true;
					force = false;
				}
				else if (questLink.RequiredToExitObjective)
				{
					flag2 = false;
				}
			}
			if (node.Links.Count <= 0 && node is BranchCompleteNode)
			{
				flag = true;
			}
			if (flag && flag2)
			{
				ExitState(nodeID);
			}
			foreach (int item in s_statesToFail)
			{
				QuestManagerInstance.FailState(this, item);
			}
			foreach (int item2 in s_statesToEnter)
			{
				EnterState(item2);
			}
		}
		DetermineIfQuestHasEnded();
	}

	public int GetHighestEventID()
	{
		int num = 0;
		foreach (QuestEvent @event in QuestData.Events)
		{
			if (@event.EventID > num)
			{
				num = @event.EventID;
			}
		}
		return num;
	}

	public int GetHighestEndStateID()
	{
		int num = 0;
		foreach (QuestEndState endState in QuestData.EndStates)
		{
			if (endState.EndStateID > num)
			{
				num = endState.EndStateID;
			}
		}
		return num;
	}

	public int GetHighestAddendumID()
	{
		int num = 0;
		foreach (FlowChartNode node in base.Data.Nodes)
		{
			if (!(node is ObjectiveNode objectiveNode))
			{
				continue;
			}
			foreach (int addendumID in objectiveNode.AddendumIDs)
			{
				if (addendumID > num)
				{
					num = addendumID;
				}
			}
		}
		return num;
	}

	public int GetAddendumNode(int addendumId)
	{
		for (int i = 0; i <= GetHighestNodeID(); i++)
		{
			if (GetNode(i) is ObjectiveNode objectiveNode && objectiveNode.AddendumIDs.Contains(addendumId))
			{
				return i;
			}
		}
		return -1;
	}

	public List<string> GetActiveStateTitles()
	{
		List<string> list = new List<string>();
		foreach (int activeState in ActiveStates)
		{
			if (GetNode(activeState) is ObjectiveNode node)
			{
				list.Add(GetObjectiveTitle(node));
			}
		}
		return list;
	}

	public void ClearActiveStates()
	{
		ActiveStates.Clear();
	}

	public bool IsActive()
	{
		if (ActiveStates == null)
		{
			return false;
		}
		return ActiveStates.Count > 0;
	}

	public bool IsStarted()
	{
		return QuestManagerInstance.IsStateVisited(this, 0);
	}

	public bool IsComplete()
	{
		if (GetHighestEndStateID() >= 0)
		{
			return QuestManagerInstance.GetEndStateID(this) >= 0;
		}
		if (IsStarted())
		{
			return !IsActive();
		}
		return false;
	}

	public string GetObjectiveTitle(ObjectiveNode node)
	{
		if (node != null)
		{
			return GetObjectiveText(node, node.NodeID);
		}
		return string.Empty;
	}

	public string GetAddendumDescription(ObjectiveNode node, int addendumId)
	{
		if (node != null)
		{
			return GetObjectiveText(node, addendumId + 20000);
		}
		return string.Empty;
	}

	public string GetObjectiveDescription(ObjectiveNode node)
	{
		if (node != null)
		{
			return GetObjectiveText(node, node.NodeID + 10000);
		}
		return string.Empty;
	}

	public string GetEndStateDisplayName(int endStateID)
	{
		foreach (QuestEndState endState in QuestData.EndStates)
		{
			if (endState.EndStateID == endStateID)
			{
				return endState.DisplayName;
			}
		}
		return string.Empty;
	}

	public string GetEndStateText(int endStateID)
	{
		foreach (QuestEndState endState in QuestData.EndStates)
		{
			if (endState.EndStateID == endStateID)
			{
				int descriptionID = endState.DescriptionID;
				return StringTableManager.GetText(StringTableName, descriptionID);
			}
		}
		return string.Empty;
	}

	public string GetQuestTitle()
	{
		if (GetNode(0) is QuestNode questNode)
		{
			return GetQuestText(questNode, questNode.NodeID);
		}
		return "[empty string]";
	}

	public string GetQuestDescription()
	{
		if (GetNode(0) is QuestNode node)
		{
			if (m_questDescriptionID == 0)
			{
				return GetQuestText(node, 10000);
			}
			return GetQuestText(node, 40000 + m_questDescriptionID);
		}
		return "[empty string]";
	}

	public string GetQuestEndState()
	{
		return GetEndStateText(QuestManagerInstance.GetEndStateID(this));
	}

	private uint CurrentObjectivePackageId()
	{
		if (ActiveStates.Count > 0)
		{
			return GetNode(ActiveStates[ActiveStates.Count - 1]).PackageID;
		}
		return GetNode(GetHighestNodeID()).PackageID;
	}

	private string GetObjectiveText(ObjectiveNode node, int stringID)
	{
		string text = StringTableManager.GetText(StringTableName, stringID);
		if (node.IsTempText)
		{
			return "[Temp]" + text;
		}
		return text;
	}

	private string GetQuestText(QuestNode node, int stringID)
	{
		string text = StringTableManager.GetText(StringTableName, stringID);
		if (node.IsTempText)
		{
			return "[Temp]" + text;
		}
		return text;
	}

	public string GetTreeListDisplayName()
	{
		string text = GetQuestTitle();
		if (string.IsNullOrEmpty(text))
		{
			text = "[NO NAME]";
		}
		return text;
	}

	public bool GetTreeListVisualEnabled()
	{
		return !IsComplete();
	}
}
