using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using Polenter.Serialization;
using UnityEngine;

public class FlowChart : ScriptableObject
{
	public const int INVALID_NODE_ID = -1;

	private static List<FlowChartNode> s_randomNodeSet = new List<FlowChartNode>();

	public static bool DebuggingEnabled { get; set; }

	[Persistent]
	public virtual string Filename { get; set; }

	[ExcludeFromSerialization]
	protected FlowChartData Data { get; set; }

	public int RefCount { get; set; }

	protected List<int> PlayOrder { get; set; }

	public FlowChart()
	{
		PlayOrder = new List<int>();
	}

	public void Load(string filename)
	{
		DebugOutput("Load flow chart " + filename, 0);
		Filename = filename;
		OnLoad(filename);
	}

	protected virtual void OnLoad(string filename)
	{
	}

	public void Unload()
	{
		OnUnload();
	}

	protected virtual void OnUnload()
	{
	}

	public virtual FlowChartPlayer StartFlowChart(FlowChartPlayer player)
	{
		return player;
	}

	public void UpdateFlowChart(FlowChartPlayer player)
	{
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			ExecuteScripts(node.OnUpdateScripts, player);
		}
	}

	public FlowChartPlayer MoveToNode(int nodeID, FlowChartPlayer player)
	{
		DebugOutput("Move To Node " + nodeID, 1);
		FlowChartNode flowChartNode = null;
		if (player.CurrentNodeID != -1)
		{
			flowChartNode = GetNode(player.CurrentNodeID);
			if (flowChartNode != null)
			{
				ExecuteScripts(flowChartNode.OnExitScripts, player);
				if (flowChartNode.IsInContainer && Data.GetNodeByID(flowChartNode.ContainerNodeID) is BankNode bankNode)
				{
					if (bankNode.BankNodePlayType == BankNodePlayType.PlayFirst)
					{
						ExecuteScripts(bankNode.OnExitScripts, player);
					}
					else if (bankNode.BankNodePlayType == BankNodePlayType.PlayAll)
					{
						FlowChartNode nodeByID = Data.GetNodeByID(nodeID);
						if (nodeByID == null || nodeByID.ContainerNodeID != bankNode.NodeID)
						{
							ExecuteScripts(bankNode.OnExitScripts, player);
						}
					}
				}
			}
		}
		player.CurrentNodeID = nodeID;
		PlayOrder.Add(nodeID);
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			TransitionToNode(flowChartNode, node, player);
			ExecuteScripts(node.OnEnterScripts, player);
			if (node is BankNode)
			{
				node = GetNextNodeInBankNode(node as BankNode, player);
				TransitionToNode(flowChartNode, node, player);
				ExecuteScripts(node.OnEnterScripts, player);
			}
			else if (node is TriggerConversationNode)
			{
				TriggerConversationNode triggerConversationNode = node as TriggerConversationNode;
				ExecuteScripts(triggerConversationNode.OnExitScripts, player);
				ConversationManager.Instance.EndConversation(player);
				if (player != null)
				{
					player.CurrentNodeID = -1;
				}
				string conversationFilename = triggerConversationNode.ConversationFilename.Replace('\\', '/');
				player = ConversationManager.Instance.StartConversation(conversationFilename, triggerConversationNode.StartNodeID, player.OwnerObject, player.FlowChartDisplayMode);
			}
		}
		return player;
	}

	public void MoveToPreviousNode(FlowChartPlayer player)
	{
		if (PlayOrder.Count > 1)
		{
			PlayOrder.RemoveAt(PlayOrder.Count - 1);
			player.CurrentNodeID = PlayOrder[PlayOrder.Count - 1];
			DebugOutput("Move To Previous Node " + player.CurrentNodeID, 1);
		}
	}

	protected virtual void TransitionToNode(FlowChartNode previousNode, FlowChartNode currentNode, FlowChartPlayer player)
	{
	}

	public FlowChartNode GetNode(int nodeID)
	{
		if (Data != null)
		{
			return Data.GetNodeByID(nodeID);
		}
		return null;
	}

	public bool CanMoveBack()
	{
		return PlayOrder.Count > 1;
	}

	public List<FlowChartNode> GetAllNodesFromActiveNode(FlowChartPlayer player)
	{
		List<FlowChartNode> list = new List<FlowChartNode>();
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			foreach (FlowChartLink link in node.Links)
			{
				FlowChartNode nodeByID = Data.GetNodeByID(link.ToNodeID);
				if (nodeByID != null && nodeByID.NodeType != FlowChartNodeType.TriggerConversation)
				{
					if (nodeByID.NodeType == FlowChartNodeType.Bank)
					{
						foreach (int childNodeID in (nodeByID as BankNode).ChildNodeIDs)
						{
							FlowChartNode nodeByID2 = Data.GetNodeByID(childNodeID);
							if (nodeByID2 != null && nodeByID2.NodeType != FlowChartNodeType.TriggerConversation)
							{
								list.Add(nodeByID2);
							}
						}
					}
					else
					{
						list.Add(Data.GetNodeByID(link.ToNodeID));
					}
				}
			}
			return list;
		}
		return list;
	}

	public FlowChartNode GetNextNode(FlowChartPlayer player)
	{
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			List<FlowChartLink> links = node.Links;
			if (node is BankNode)
			{
				return GetNextNodeInBankNode(node as BankNode, player);
			}
			if (node.IsInContainer && Data.GetNodeByID(node.ContainerNodeID) is BankNode bankNode)
			{
				if (bankNode.BankNodePlayType == BankNodePlayType.PlayFirst || bankNode.BankNodePlayType == BankNodePlayType.PlayRandom)
				{
					links = bankNode.Links;
				}
				else if (bankNode.BankNodePlayType == BankNodePlayType.PlayAll)
				{
					int num = 0;
					using (List<int>.Enumerator enumerator = bankNode.ChildNodeIDs.GetEnumerator())
					{
						while (enumerator.MoveNext() && enumerator.Current != node.NodeID)
						{
							num++;
						}
					}
					if (num >= bankNode.ChildNodeIDs.Count)
					{
						return null;
					}
					for (int i = num + 1; i < bankNode.ChildNodeIDs.Count; i++)
					{
						FlowChartNode nodeByID = Data.GetNodeByID(bankNode.ChildNodeIDs[i]);
						if (nodeByID != null && PassesConditionals(nodeByID, player))
						{
							return nodeByID;
						}
					}
					links = bankNode.Links;
				}
			}
			return GetNextNode(links, player);
		}
		return null;
	}

	protected virtual FlowChartNode GetNextNode(List<FlowChartLink> links, FlowChartPlayer player)
	{
		foreach (FlowChartLink link in links)
		{
			FlowChartNode nodeByID = Data.GetNodeByID(link.ToNodeID);
			if (nodeByID == null || nodeByID.NodeType == FlowChartNodeType.PlayerResponse)
			{
				continue;
			}
			if (nodeByID.NodeType == FlowChartNodeType.Bank)
			{
				FlowChartNode nextNodeInBankNode = GetNextNodeInBankNode(nodeByID as BankNode, player);
				if (nextNodeInBankNode != null)
				{
					return nextNodeInBankNode;
				}
			}
			else if (PassesConditionals(nodeByID, player))
			{
				return nodeByID;
			}
		}
		return null;
	}

	protected FlowChartNode GetNextNodeInBankNode(BankNode bankNode, FlowChartPlayer player)
	{
		if (PassesConditionals(bankNode, player))
		{
			if (bankNode.BankNodePlayType == BankNodePlayType.PlayRandom)
			{
				s_randomNodeSet.Clear();
				foreach (int childNodeID in bankNode.ChildNodeIDs)
				{
					FlowChartNode nodeByID = Data.GetNodeByID(childNodeID);
					if (nodeByID != null && PassesConditionals(nodeByID, player))
					{
						s_randomNodeSet.Add(nodeByID);
					}
				}
				if (s_randomNodeSet.Count > 0)
				{
					int index = OEIRandom.Index(s_randomNodeSet.Count);
					return s_randomNodeSet[index];
				}
			}
			else
			{
				foreach (int childNodeID2 in bankNode.ChildNodeIDs)
				{
					FlowChartNode nodeByID2 = Data.GetNodeByID(childNodeID2);
					if (nodeByID2 != null && PassesConditionals(nodeByID2, player))
					{
						return nodeByID2;
					}
				}
			}
		}
		return null;
	}

	protected virtual bool PassesConditionals(FlowChartNode node, FlowChartPlayer player)
	{
		return EvaluateExpression(node.Conditionals, player);
	}

	protected virtual bool PassesNonStatConditionals(FlowChartNode node, FlowChartPlayer player)
	{
		ConditionalExpression conditionalExpression = new ConditionalExpression();
		conditionalExpression.Operator = node.Conditionals.Operator;
		List<ExpressionComponent> list = new List<ExpressionComponent>();
		foreach (ExpressionComponent component in node.Conditionals.Components)
		{
			bool flag = false;
			if (component is ConditionalCall conditionalCall)
			{
				MethodInfo methodInfo = ScriptManager.GetMethodInfo(conditionalCall);
				if (methodInfo != null)
				{
					object[] customAttributes = methodInfo.GetCustomAttributes(typeof(StatRequirementAttribute), inherit: false);
					object[] customAttributes2 = methodInfo.GetCustomAttributes(typeof(AbilityRequirementAttribute), inherit: false);
					if (customAttributes.Length != 0 || customAttributes2.Length != 0)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				list.Add(component);
			}
		}
		conditionalExpression.Components = list;
		return EvaluateExpression(conditionalExpression, player);
	}

	protected bool EvaluateExpression(ConditionalExpression expression, FlowChartPlayer player)
	{
		if (expression.Components.Count <= 0)
		{
			return true;
		}
		LogicalOperator logicalOperator = LogicalOperator.Or;
		bool flag = false;
		for (int i = 0; i < expression.Components.Count; i++)
		{
			ExpressionComponent expressionComponent = expression.Components[i];
			bool flag2 = true;
			LogicalOperator logicalOperator2 = LogicalOperator.Or;
			if (expressionComponent is ConditionalCall)
			{
				flag2 = EvaluateConditional(expressionComponent as ConditionalCall, player);
				logicalOperator2 = (expressionComponent as ConditionalCall).Operator;
			}
			else if (expressionComponent is ConditionalExpression)
			{
				flag2 = EvaluateExpression(expressionComponent as ConditionalExpression, player);
				logicalOperator2 = (expressionComponent as ConditionalExpression).Operator;
			}
			if (i == expression.Components.Count - 1)
			{
				switch (logicalOperator)
				{
				case LogicalOperator.And:
					return flag2;
				case LogicalOperator.Or:
					return flag2 || flag;
				}
			}
			else if (i == 0)
			{
				switch (logicalOperator2)
				{
				case LogicalOperator.And:
					if (!flag2)
					{
						return false;
					}
					break;
				case LogicalOperator.Or:
					if (flag2)
					{
						return true;
					}
					break;
				}
			}
			else
			{
				switch (logicalOperator)
				{
				case LogicalOperator.And:
					if (!flag2)
					{
						return false;
					}
					if (logicalOperator2 == LogicalOperator.Or)
					{
						return true;
					}
					break;
				case LogicalOperator.Or:
					if (flag2)
					{
						return true;
					}
					if (logicalOperator2 == LogicalOperator.And)
					{
						return false;
					}
					break;
				}
			}
			flag = flag2;
			logicalOperator = logicalOperator2;
		}
		return false;
	}

	protected bool EvaluateConditional(ConditionalCall conditional, FlowChartPlayer player)
	{
		bool flag = false;
		MethodInfo methodInfo = ScriptManager.GetMethodInfo(conditional);
		if (methodInfo == null)
		{
			Debug.LogWarning("Failed to find conditional " + conditional.Data.FullName + ".");
			return false;
		}
		List<object> list = new List<object>();
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (conditional.Data.Parameters.Count != parameters.Length)
		{
			Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + " because of mismatched parameter count.");
			return false;
		}
		if (player != null)
		{
			FlowChartNode node = GetNode(player.CurrentNodeID);
			if (node != null)
			{
				if (node.GetSpeakerGuid() != Guid.Empty)
				{
					SpecialCharacterInstanceID.Add(node.GetSpeakerGuid(), SpecialCharacterInstanceID.SpecialCharacterInstance.Speaker);
				}
				else
				{
					SpecialCharacterInstanceID.Add(player.OwnerObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Speaker);
				}
			}
			SpecialCharacterInstanceID.Add(player.OwnerObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Owner);
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			if (parameterType.IsEnum)
			{
				object obj = null;
				try
				{
					obj = Enum.Parse(parameterType, conditional.Data.Parameters[i]);
				}
				catch
				{
					obj = Enum.GetValues(parameterType).GetValue(0);
				}
				list.Add(obj);
			}
			else if (parameterType.IsValueType)
			{
				if (parameterType == typeof(Guid))
				{
					Guid empty = Guid.Empty;
					try
					{
						empty = new Guid(conditional.Data.Parameters[i]);
					}
					catch
					{
						empty = Guid.Empty;
					}
					list.Add(empty);
					continue;
				}
				try
				{
					list.Add(Convert.ChangeType(conditional.Data.Parameters[i], parameterType, CultureInfo.InvariantCulture));
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, this);
					Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + ". Could not convert " + conditional.Data.Parameters[i] + " into " + parameterType.Name + ".");
					return false;
				}
			}
			else
			{
				list.Add(conditional.Data.Parameters[i]);
			}
		}
		try
		{
			flag = (bool)methodInfo.Invoke(null, list.ToArray());
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, this);
			Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + ".");
			return false;
		}
		if (conditional.Not)
		{
			return !flag;
		}
		return flag;
	}

	protected void ExecuteScripts(List<ScriptCall> scriptCalls, FlowChartPlayer player)
	{
		foreach (ScriptCall scriptCall in scriptCalls)
		{
			ExecuteScript(scriptCall, player);
		}
	}

	protected void ExecuteScript(ScriptCall scriptCall, FlowChartPlayer player)
	{
		if (player != null)
		{
			FlowChartNode node = GetNode(player.CurrentNodeID);
			if (node != null)
			{
				if (node.GetSpeakerGuid() != Guid.Empty)
				{
					SpecialCharacterInstanceID.Add(node.GetSpeakerGuid(), SpecialCharacterInstanceID.SpecialCharacterInstance.Speaker);
				}
				else
				{
					SpecialCharacterInstanceID.Add(player.OwnerObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Speaker);
				}
			}
			SpecialCharacterInstanceID.Add(player.OwnerObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Owner);
		}
		ScriptEvent.RunScriptHelper(scriptCall, this);
	}

	public void TriggerOnExitScripts(FlowChartPlayer player)
	{
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			ExecuteScripts(node.OnExitScripts, player);
		}
	}

	public int GetHighestNodeID()
	{
		int num = -1;
		foreach (FlowChartNode node in Data.Nodes)
		{
			if (node.NodeID > num)
			{
				num = node.NodeID;
			}
		}
		return num;
	}

	protected void DebugOutput(string message, int indent)
	{
		if (DebuggingEnabled)
		{
			StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(Filename) + ":\t");
			for (int i = 0; i < indent; i++)
			{
				stringBuilder.Append("\t");
			}
			stringBuilder.Append(message);
			Debug.Log(stringBuilder.ToString());
		}
	}
}
