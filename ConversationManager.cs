using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
	public delegate void FlowChartPlayerDelegate(FlowChartPlayer chartPlayer);

	public FlowChartPlayerDelegate FlowChartPlayerAdded;

	public FlowChartPlayerDelegate FlowChartPlayerRemoved;

	private const string CONVERSATION_PATH = "/data/conversations/";

	private const string CONVERSATION_FILE_EXTENSION = ".conversation";

	private static Dictionary<Guid, List<GameObject>> s_activeSpeakerGuidCache = new Dictionary<Guid, List<GameObject>>();

	private Dictionary<string, Conversation> ActiveConversations { get; set; }

	[Persistent(Persistent.ConversionType.Binary)]
	private Dictionary<string, BitArray> MarkedAsRead { get; set; }

	[Persistent]
	private Dictionary<string, Dictionary<int, int>> NodeCyclePositions { get; set; }

	private List<FlowChartPlayer> ActiveConversationPlayers { get; set; }

	public static ConversationManager Instance { get; private set; }

	public static void AddObjectToActiveSpeakerGuidCache(Guid guid, GameObject gameObject)
	{
		if (!(guid == Guid.Empty))
		{
			if (!s_activeSpeakerGuidCache.TryGetValue(guid, out var value))
			{
				value = new List<GameObject>();
				s_activeSpeakerGuidCache.Add(guid, value);
			}
			value.Add(gameObject);
		}
	}

	public static void RemoveObjectFromActiveSpeakerGuidCache(Guid guid, GameObject gameObject)
	{
		if (!(guid == Guid.Empty) && s_activeSpeakerGuidCache.TryGetValue(guid, out var value))
		{
			value.Remove(gameObject);
			if (value.Count == 0)
			{
				s_activeSpeakerGuidCache.Remove(guid);
			}
		}
	}

	public static List<GameObject> GetActiveObjectsForSpeakerGuid(Guid guid)
	{
		List<GameObject> value = null;
		s_activeSpeakerGuidCache.TryGetValue(guid, out value);
		return value;
	}

	public List<string> FindConversations(string search)
	{
		search = search.ToLower();
		List<string> list = new List<string>();
		foreach (string allConversationPath in GetAllConversationPaths())
		{
			string text = allConversationPath.ToLower();
			if (Path.GetExtension(text).Equals(".conversation") && Path.GetFileNameWithoutExtension(text).Contains(search))
			{
				list.Add("Assets" + text.Substring(Application.dataPath.Length));
			}
		}
		return list;
	}

	public IEnumerable<string> GetAllConversationPaths()
	{
		return Directory.GetFiles(Application.dataPath + "/data/conversations/", "*.conversation", SearchOption.AllDirectories);
	}

	public void KillAll()
	{
		for (int num = ActiveConversationPlayers.Count - 1; num >= 0; num--)
		{
			if (!ActiveConversationPlayers[num].Completed)
			{
				EndConversation(ActiveConversationPlayers[num]);
			}
		}
		foreach (Conversation value in ActiveConversations.Values)
		{
			if (value != null)
			{
				Debug.LogWarning("Conversation: " + value.Filename + " not cleaned up. in KillAll()");
				value.Unload();
				GameUtilities.Destroy(value);
			}
		}
		ActiveConversationPlayers.Clear();
		ActiveConversations.Clear();
	}

	public void KillAllBarkStrings()
	{
		for (int num = ActiveConversationPlayers.Count - 1; num >= 0; num--)
		{
			if (ActiveConversationPlayers[num].FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Standard && !ActiveConversationPlayers[num].Completed)
			{
				Conversation conversation = ActiveConversationPlayers[num].CurrentFlowChart as Conversation;
				if (conversation != null)
				{
					conversation.StopVO(ActiveConversationPlayers[num]);
				}
				EndConversation(ActiveConversationPlayers[num]);
			}
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'ConversationManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		ActiveConversations = new Dictionary<string, Conversation>(StringComparer.OrdinalIgnoreCase);
		MarkedAsRead = new Dictionary<string, BitArray>(StringComparer.OrdinalIgnoreCase);
		NodeCyclePositions = new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);
		ActiveConversationPlayers = new List<FlowChartPlayer>();
		FlowChart.DebuggingEnabled = false;
		Conversation.LocalizationDebuggingEnabled = false;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		for (int i = 0; i < ActiveConversationPlayers.Count; i++)
		{
			FlowChartPlayer flowChartPlayer = ActiveConversationPlayers[i];
			flowChartPlayer.CurrentFlowChart.UpdateFlowChart(flowChartPlayer);
		}
	}

	public FlowChartPlayer StartConversation(string conversationFilename, GameObject owner, FlowChartPlayer.DisplayMode displayMode, bool disableVo = false)
	{
		return StartConversation(conversationFilename, 0, owner, displayMode, disableVo);
	}

	public FlowChartPlayer StartConversation(string conversationFilename, int startNode, GameObject owner, FlowChartPlayer.DisplayMode displayMode, bool disableVo = false)
	{
		conversationFilename = conversationFilename.Replace('\\', '/').ToLower();
		if (conversationFilename.Contains("assets"))
		{
			conversationFilename = conversationFilename.Remove(0, "assets/".Length);
		}
		if (string.IsNullOrEmpty(conversationFilename))
		{
			Debug.LogWarning("Attempting to start an empty conversation.");
			return null;
		}
		if (ActiveConversations.ContainsKey(conversationFilename))
		{
			return StartConversation(ActiveConversations[conversationFilename], startNode, owner, displayMode, disableVo);
		}
		Conversation conversation = ScriptableObject.CreateInstance<Conversation>();
		ActiveConversations.Add(conversationFilename, conversation);
		conversation.Load(conversationFilename);
		conversation.RefCount = 0;
		return StartConversation(conversation, startNode, owner, displayMode, disableVo);
	}

	private FlowChartPlayer StartConversation(Conversation conversation, int startNode, GameObject owner, FlowChartPlayer.DisplayMode displayMode, bool disableVO)
	{
		conversation.RefCount++;
		FlowChartPlayer flowChartPlayer = new FlowChartPlayer(conversation, startNode, owner, displayMode);
		flowChartPlayer.DisableVO = disableVO;
		ActiveConversationPlayers.Add(flowChartPlayer);
		flowChartPlayer = conversation.MoveToNode(startNode, flowChartPlayer);
		flowChartPlayer = conversation.StartFlowChart(flowChartPlayer);
		if (FlowChartPlayerAdded != null)
		{
			FlowChartPlayerAdded(flowChartPlayer);
		}
		return flowChartPlayer;
	}

	public void CreateMarkedAsReadBitArray(Conversation conversation)
	{
		int highestNodeID = conversation.GetHighestNodeID();
		if (!MarkedAsRead.ContainsKey(conversation.Filename))
		{
			MarkedAsRead.Add(conversation.Filename, new BitArray(highestNodeID + 1, defaultValue: false));
			return;
		}
		BitArray bitArray = MarkedAsRead[conversation.Filename];
		if (bitArray.Length < highestNodeID + 1)
		{
			bitArray.Length = highestNodeID + 1;
		}
	}

	public FlowChartPlayer StartScriptedInteraction(string conversationFilename, GameObject owner)
	{
		return StartConversation(conversationFilename, owner, FlowChartPlayer.DisplayMode.Interaction);
	}

	public void EndConversation(FlowChartPlayer player)
	{
		if (player == null || player.CurrentFlowChart == null || player.Completed)
		{
			return;
		}
		Conversation conversation = player.CurrentFlowChart as Conversation;
		if ((bool)conversation)
		{
			GameObject speakerOrPlayer = conversation.GetSpeakerOrPlayer(player);
			if ((bool)speakerOrPlayer)
			{
				Conversation.StopVO(speakerOrPlayer);
			}
		}
		player.SetComplete();
		player.CurrentFlowChart.TriggerOnExitScripts(player);
		ActiveConversationPlayers.Remove(player);
		string filename = player.CurrentFlowChart.Filename;
		if (ActiveConversations.ContainsKey(filename))
		{
			ActiveConversations[filename].RefCount--;
			if (ActiveConversations[filename].RefCount == 0)
			{
				ActiveConversations[filename].Unload();
				GameUtilities.Destroy(ActiveConversations[filename]);
				ActiveConversations.Remove(filename);
			}
		}
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			activeFactionComponent.TryHideRevealer(player.CurrentFlowChart as Conversation);
		}
		if (FlowChartPlayerRemoved != null)
		{
			FlowChartPlayerRemoved(player);
		}
	}

	public bool IsConversationActive(FlowChartPlayer conversationObject)
	{
		if (conversationObject == null)
		{
			return false;
		}
		return ActiveConversationPlayers.Contains(conversationObject);
	}

	public FlowChartPlayer GetActiveConversationForHUD()
	{
		for (int i = 0; i < ActiveConversationPlayers.Count; i++)
		{
			FlowChartPlayer flowChartPlayer = ActiveConversationPlayers[i];
			FlowChartNode node = flowChartPlayer.CurrentFlowChart.GetNode(flowChartPlayer.CurrentNodeID);
			if (node != null && node is DialogueNode dialogueNode && dialogueNode.DisplayType == DisplayType.Conversation)
			{
				return flowChartPlayer;
			}
		}
		return null;
	}

	public bool IsConversationOrSIRunning()
	{
		for (int num = ActiveConversationPlayers.Count - 1; num >= 0; num--)
		{
			FlowChartPlayer flowChartPlayer = ActiveConversationPlayers[num];
			FlowChartNode node = flowChartPlayer.CurrentFlowChart.GetNode(flowChartPlayer.CurrentNodeID);
			if (node != null && node is DialogueNode dialogueNode && dialogueNode.DisplayType == DisplayType.Conversation)
			{
				return true;
			}
		}
		return false;
	}

	public List<FlowChartPlayer> GetActiveBarkstringConversationsForHUD()
	{
		return ActiveConversationPlayers;
	}

	public void SetMarkedAsRead(string conversationFilename, int nodeID)
	{
		conversationFilename = conversationFilename.Replace('\\', '/').ToLower();
		if (conversationFilename.Contains("assets"))
		{
			conversationFilename = conversationFilename.Remove(0, "assets/".Length);
		}
		if (MarkedAsRead.ContainsKey(conversationFilename))
		{
			MarkedAsRead[conversationFilename].Set(nodeID, value: true);
		}
	}

	public void SetMarkedAsRead(Conversation conversation, int nodeID)
	{
		if (MarkedAsRead.ContainsKey(conversation.Filename))
		{
			MarkedAsRead[conversation.Filename].Set(nodeID, value: true);
		}
	}

	public void ClearMarkedAsRead(string conversationFilename, int nodeID)
	{
		conversationFilename = conversationFilename.Replace('\\', '/').ToLower();
		if (conversationFilename.Contains("assets"))
		{
			conversationFilename = conversationFilename.Remove(0, "assets/".Length);
		}
		if (MarkedAsRead.ContainsKey(conversationFilename))
		{
			MarkedAsRead[conversationFilename].Set(nodeID, value: false);
		}
	}

	public void ClearMarkedAsRead(Conversation conversation, int nodeID)
	{
		if (MarkedAsRead.ContainsKey(conversation.Filename))
		{
			MarkedAsRead[conversation.Filename].Set(nodeID, value: false);
		}
	}

	public bool GetMarkedAsRead(Conversation conversation, int nodeID)
	{
		if (MarkedAsRead.ContainsKey(conversation.Filename))
		{
			return MarkedAsRead[conversation.Filename].Get(nodeID);
		}
		return false;
	}

	public void SetNodeCyclePosition(Conversation conversation, int nodeID, int cyclePosition)
	{
		if (!NodeCyclePositions.ContainsKey(conversation.Filename))
		{
			NodeCyclePositions.Add(conversation.Filename, new Dictionary<int, int>());
		}
		Dictionary<int, int> dictionary = NodeCyclePositions[conversation.Filename];
		if (!dictionary.ContainsKey(nodeID))
		{
			dictionary.Add(nodeID, cyclePosition);
		}
		else
		{
			dictionary[nodeID] = cyclePosition;
		}
	}

	public int GetNodeCyclePosition(Conversation conversation, int nodeID)
	{
		if (!NodeCyclePositions.ContainsKey(conversation.Filename))
		{
			NodeCyclePositions.Add(conversation.Filename, new Dictionary<int, int>());
		}
		Dictionary<int, int> dictionary = NodeCyclePositions[conversation.Filename];
		if (dictionary.ContainsKey(nodeID))
		{
			return dictionary[nodeID];
		}
		return -1;
	}

	public bool HasConversationNodeBeenPlayed(string conversationFilename, int nodeID)
	{
		conversationFilename = conversationFilename.Replace('\\', '/').ToLower();
		if (conversationFilename.Contains("assets"))
		{
			conversationFilename = conversationFilename.Remove(0, "assets/".Length);
		}
		BitArray value = null;
		if (MarkedAsRead == null || !MarkedAsRead.TryGetValue(conversationFilename, out value))
		{
			return false;
		}
		if (nodeID < 0 || nodeID >= value.Count)
		{
			return false;
		}
		if (value.Get(nodeID))
		{
			return true;
		}
		Conversation value2 = null;
		if (ActiveConversations.TryGetValue(conversationFilename, out value2) && value2.HasOncePerConversationNodeBeenPlayed(nodeID))
		{
			return true;
		}
		return false;
	}
}
