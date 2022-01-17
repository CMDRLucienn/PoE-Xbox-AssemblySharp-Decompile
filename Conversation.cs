using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using MinigameData;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class Conversation : FlowChart
{
	private const string CONVERSATION_PATH = "data/conversations/";

	private const float VO_DELAY_BETWEEN_LINES = 0.4f;

	private bool HideResponseForQuestionNodesCurrentPath;

	public static bool LocalizationDebuggingEnabled { get; set; }

	private string StringTableName { get; set; }

	private ConversationData ConversationData { get; set; }

	private List<int> OncePerConversationPlayedNodes { get; set; }

	private Dictionary<int, int> QuestionNodeSelection { get; set; }

	private Dictionary<int, int> QuestionNodeLinks { get; set; }

	private bool ShowQuestionNodeText { get; set; }

	public Conversation()
	{
		StringTableName = string.Empty;
		OncePerConversationPlayedNodes = new List<int>();
		QuestionNodeSelection = new Dictionary<int, int>();
		QuestionNodeLinks = new Dictionary<int, int>();
	}

	public string TryMapSpeaker(Guid speakerGuid)
	{
		IEnumerable<ConversationData.CharacterMapping> source = ConversationData.CharacterMappings.Where((ConversationData.CharacterMapping cm) => cm.Guid == speakerGuid);
		if (source.Any())
		{
			return source.First().InstanceTag;
		}
		return "";
	}

	protected override void OnLoad(string filename)
	{
		ConversationData = ConversationData.Load(Application.dataPath + Path.DirectorySeparatorChar + filename);
		if (ConversationData == null)
		{
			Debug.LogWarning("Failed to load flow chart file " + filename + ".");
			ConversationData = new ConversationData();
			base.Data = ConversationData;
		}
		else
		{
			GameResources.LoadDialogueAudio(Path.GetFileName(filename));
			base.Data = ConversationData;
			ConversationManager.Instance.CreateMarkedAsReadBitArray(this);
			StringTableName = GetStringTableName();
			StringTableManager.LoadStringTable(StringTableName);
		}
	}

	protected override void OnUnload()
	{
		StringTableManager.UnloadStringTable(StringTableName);
		GameResources.UnloadDialogueAudio(Filename);
	}

	public override FlowChartPlayer StartFlowChart(FlowChartPlayer player)
	{
		DebugOutput("Conversation Started", 0);
		OncePerConversationPlayedNodes = new List<int>();
		if (player.CurrentNodeID == 0 && !LocalizationDebuggingEnabled && GetResponseNodes(player, qualifiedOnly: true).Count == 0)
		{
			FlowChartNode nextNode = GetNextNode(player);
			if (nextNode != null)
			{
				DebugOutput("Automatically advance root node.", 1);
				player = MoveToNode(nextNode.NodeID, player);
			}
			else
			{
				DebugOutput("Could not advance root node because no nodes met conditionals.", 1);
				ConversationManager.Instance.EndConversation(player);
			}
		}
		return player;
	}

	private string GetStringTableName()
	{
		string text = "data/conversations/";
		int num = Filename.IndexOf("data/conversations/", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			num = Filename.IndexOf(GameResources.OverridePath, StringComparison.OrdinalIgnoreCase);
			text = GameResources.OverridePath;
		}
		if (num < 0)
		{
			return string.Empty;
		}
		return "Text/Conversations/" + Path.ChangeExtension(Filename.Substring(num + text.Length), ".stringtable");
	}

	public bool HasOncePerConversationNodeBeenPlayed(int nodeID)
	{
		return OncePerConversationPlayedNodes.Contains(nodeID);
	}

	private bool ShouldDisplayQuestionNodeText(DialogueNode node, DialogueLink link)
	{
		if (node.IsQuestionNode && link != null)
		{
			if (link.QuestionNodeTextDisplay == DialogueLink.QuestionNodeDisplayType.ShowNever)
			{
				return false;
			}
			if (link.QuestionNodeTextDisplay == DialogueLink.QuestionNodeDisplayType.ShowOnce && OncePerConversationPlayedNodes.Contains(node.NodeID))
			{
				return false;
			}
		}
		return true;
	}

	protected override void TransitionToNode(FlowChartNode previousNode, FlowChartNode currentNode, FlowChartPlayer player)
	{
		DialogueNode dialogueNode = currentNode as DialogueNode;
		if (dialogueNode != null)
		{
			DialogueLink link = null;
			if (previousNode != null)
			{
				foreach (FlowChartLink link2 in previousNode.Links)
				{
					if (link2.ToNodeID == dialogueNode.NodeID)
					{
						link = link2 as DialogueLink;
						break;
					}
				}
			}
			ShowQuestionNodeText = ShouldDisplayQuestionNodeText(dialogueNode, link);
			ConversationManager.Instance.SetMarkedAsRead(this, dialogueNode.NodeID);
			if ((dialogueNode.Persistence == PersistenceType.OncePerConversation || dialogueNode.IsQuestionNode) && !OncePerConversationPlayedNodes.Contains(dialogueNode.NodeID))
			{
				OncePerConversationPlayedNodes.Add(dialogueNode.NodeID);
			}
			if (previousNode is DialogueNode dialogueNode2)
			{
				if (dialogueNode2.IsQuestionNode)
				{
					if (!QuestionNodeSelection.ContainsKey(dialogueNode2.NodeID))
					{
						QuestionNodeSelection.Add(dialogueNode2.NodeID, dialogueNode.NodeID);
					}
					else
					{
						QuestionNodeSelection[dialogueNode2.NodeID] = dialogueNode.NodeID;
					}
				}
				if (dialogueNode2.PlayType == PlayType.CycleLoop || dialogueNode2.PlayType == PlayType.CycleStop)
				{
					int num = 0;
					using (List<FlowChartLink>.Enumerator enumerator = dialogueNode2.Links.GetEnumerator())
					{
						while (enumerator.MoveNext() && enumerator.Current.ToNodeID != dialogueNode.NodeID)
						{
							num++;
						}
					}
					ConversationManager.Instance.SetNodeCyclePosition(this, dialogueNode2.NodeID, num);
				}
			}
			if (QuestionNodeLinks.ContainsKey(dialogueNode.NodeID))
			{
				int key = QuestionNodeLinks[dialogueNode.NodeID];
				if (!QuestionNodeSelection.ContainsKey(key))
				{
					QuestionNodeSelection.Add(key, dialogueNode.NodeID);
				}
				else
				{
					QuestionNodeSelection[key] = dialogueNode.NodeID;
				}
			}
		}
		QuestManager.Instance.TriggerTalkEvent(Filename, currentNode.NodeID);
		if (previousNode != null)
		{
			GameObject gameObject = ((player == null) ? GetSpeakerOrPlayer(currentNode) : GetSpeakerOrPlayer(player));
			if ((bool)gameObject)
			{
				StopVO(gameObject);
			}
		}
		if (!player.DisableVO)
		{
			bool retrieveClipLength = dialogueNode != null && dialogueNode.DisplayType == DisplayType.Bark;
			PlayVO(currentNode, player, retrieveClipLength);
		}
		if (dialogueNode != null)
		{
			float vODuration = GetVODuration(dialogueNode.NodeID);
			if (vODuration > 0f)
			{
				player.Timer = vODuration + 0.4f;
			}
			else
			{
				player.Timer = 6f;
			}
		}
		QuestionNodeLinks.Clear();
	}

	public void PlayVO(FlowChartNode currentNode, FlowChartPlayer player, bool retrieveClipLength)
	{
		VOAsset dialogueAudio = GameResources.GetDialogueAudio(Filename, currentNode.NodeID, GameResources.ShouldUseFemaleVersion(StringTableName, currentNode.NodeID));
		if (!(dialogueAudio != null))
		{
			return;
		}
		GameObject gameObject = ((player == null) ? GetSpeakerOrPlayer(currentNode) : GetSpeakerOrPlayer(player));
		if ((bool)gameObject)
		{
			AudioSource component = gameObject.GetComponent<AudioSource>();
			if (!gameObject.GetComponent<VolumeAsCategory>())
			{
				VolumeAsCategory volumeAsCategory = gameObject.AddComponent<VolumeAsCategory>();
				volumeAsCategory.Category = MusicManager.SoundCategory.VOICE;
				volumeAsCategory.Source = component;
				volumeAsCategory.Init();
			}
			GlobalAudioPlayer.ClipLoaded onClipLoaded = null;
			object tag = null;
			if (retrieveClipLength)
			{
				onClipLoaded = OnVOClipLoaded;
				tag = player;
			}
			bool bIs3DSound = false;
			if (currentNode is DialogueNode dialogueNode)
			{
				bIs3DSound = dialogueNode.PlayVOAs3DSound;
			}
			GlobalAudioPlayer.StreamClipAtSource(component, dialogueAudio.VOClip.clip, bIs3DSound, onClipLoaded, tag);
		}
	}

	private void OnVOClipLoaded(AudioClip clip, object tag)
	{
		if (!(clip == null) && tag is FlowChartPlayer flowChartPlayer && flowChartPlayer != null && clip.length > 0f)
		{
			flowChartPlayer.Timer = clip.length + 0.4f;
		}
	}

	public bool IsVOPlaying(FlowChartPlayer player)
	{
		GameObject speakerOrPlayer = GetSpeakerOrPlayer(player);
		AudioSource audioSource = (speakerOrPlayer ? speakerOrPlayer.GetComponent<AudioSource>() : null);
		if ((bool)audioSource)
		{
			if (!audioSource.isPlaying)
			{
				if (TimeController.Instance != null)
				{
					return TimeController.Instance.IsSourcePaused(audioSource);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void StopVO(FlowChartPlayer player)
	{
		GameObject speakerOrPlayer = GetSpeakerOrPlayer(player);
		AudioSource audioSource = (speakerOrPlayer ? speakerOrPlayer.GetComponent<AudioSource>() : null);
		if ((bool)audioSource && audioSource.isPlaying)
		{
			audioSource.Stop();
		}
	}

	public float GetVODuration(int nodeID)
	{
		return 0f;
	}

	public static void StopVO(GameObject speaker)
	{
		AudioSource component = speaker.GetComponent<AudioSource>();
		if ((bool)component)
		{
			component.Stop();
		}
		VolumeAsCategory component2 = speaker.GetComponent<VolumeAsCategory>();
		if ((bool)component2)
		{
			GameUtilities.DestroyComponentImmediate(component2);
		}
	}

	public string GetActiveNodeText(FlowChartPlayer player)
	{
		if (base.Data != null)
		{
			FlowChartNode nodeByID = base.Data.GetNodeByID(player.CurrentNodeID);
			if (nodeByID != null)
			{
				if (!(nodeByID is DialogueNode) || ((nodeByID as DialogueNode).IsQuestionNode && !ShowQuestionNodeText))
				{
					return string.Empty;
				}
				return GetNodeText(player, nodeByID);
			}
		}
		return string.Empty;
	}

	public FlowChartNode GetActiveNode(FlowChartPlayer player)
	{
		if (base.Data != null)
		{
			return base.Data.GetNodeByID(player.CurrentNodeID);
		}
		return null;
	}

	public bool IsSpeakerNameOfActiveNodeHidden(FlowChartPlayer player)
	{
		if (base.Data != null && base.Data.GetNodeByID(player.CurrentNodeID) is DialogueNode dialogueNode)
		{
			return dialogueNode.HideSpeaker;
		}
		return false;
	}

	public string GetNodeQualifier(FlowChartNode node, FlowChartPlayer player)
	{
		return GetNodeQualifierHelper(node, player, checkpassing: false, passing: false, firstonly: true);
	}

	public string GetNodeQualifiers(FlowChartNode node, FlowChartPlayer player, bool passing)
	{
		return GetNodeQualifierHelper(node, player, checkpassing: true, passing, firstonly: false);
	}

	private string GetNodeQualifierHelper(FlowChartNode node, FlowChartPlayer player, bool checkpassing, bool passing, bool firstonly)
	{
		if (node != null)
		{
			string text = string.Empty;
			DialogueNode dialogueNode = node as DialogueNode;
			if (dialogueNode.DisplayType == DisplayType.Bark)
			{
				return text;
			}
			{
				foreach (ExpressionComponent component in dialogueNode.Conditionals.Components)
				{
					if (!(component is ConditionalCall conditionalCall))
					{
						continue;
					}
					bool flag = EvaluateConditional(conditionalCall, player);
					if (checkpassing && flag != passing)
					{
						continue;
					}
					MethodInfo methodInfo = ScriptManager.GetMethodInfo(conditionalCall);
					if (!(methodInfo != null))
					{
						continue;
					}
					StatRequirementAttribute statRequirementAttribute = methodInfo.GetCustomAttributes(typeof(StatRequirementAttribute), inherit: false).FirstOrDefault() as StatRequirementAttribute;
					AbilityRequirementAttribute abilityRequirementAttribute = methodInfo.GetCustomAttributes(typeof(AbilityRequirementAttribute), inherit: false).FirstOrDefault() as AbilityRequirementAttribute;
					if (statRequirementAttribute == null && abilityRequirementAttribute == null)
					{
						continue;
					}
					FactionReputationRequirementAttribute factionReputationRequirementAttribute = statRequirementAttribute as FactionReputationRequirementAttribute;
					ObjectReputationRequirementAttribute objectReputationRequirementAttribute = statRequirementAttribute as ObjectReputationRequirementAttribute;
					if (conditionalCall != null && conditionalCall.Not)
					{
						return "";
					}
					int num = -1;
					int num2 = -1;
					int num3 = -1;
					int num4 = -1;
					ParameterInfo[] parameters = methodInfo.GetParameters();
					for (int i = 0; i < Mathf.Min(conditionalCall.Data.Parameters.Count, parameters.Length); i++)
					{
						ParameterInfo parameterInfo = parameters[i];
						if (statRequirementAttribute != null && parameterInfo.Name == statRequirementAttribute.ParamTypeName)
						{
							num = i;
						}
						if (statRequirementAttribute != null && parameterInfo.Name == statRequirementAttribute.ParamValueName)
						{
							num2 = i;
						}
						if (factionReputationRequirementAttribute != null && parameterInfo.Name == factionReputationRequirementAttribute.ParamFactionName)
						{
							num3 = i;
						}
						if (abilityRequirementAttribute != null && parameterInfo.Name == abilityRequirementAttribute.ParamAbilityIdName)
						{
							num4 = i;
						}
					}
					if (firstonly && !flag && text.Length > 0)
					{
						continue;
					}
					string text2 = "";
					if (statRequirementAttribute != null && statRequirementAttribute.IsPersonalityReputation && !(node is PlayerResponseNode) && !GameState.Option.DisplayPersonalityReputationIndicators)
					{
						continue;
					}
					object statObject = null;
					if (num >= 0)
					{
						statObject = TypeDescriptor.GetConverter(parameters[num].ParameterType).ConvertFromString(conditionalCall.Data.Parameters[num]);
					}
					object obj = null;
					if (num2 >= 0)
					{
						obj = TypeDescriptor.GetConverter(parameters[num2].ParameterType).ConvertFromString(conditionalCall.Data.Parameters[num2]);
					}
					if (factionReputationRequirementAttribute != null)
					{
						object statObject2 = TypeDescriptor.GetConverter(parameters[num3].ParameterType).ConvertFromString(conditionalCall.Data.Parameters[num3]);
						text2 = "[" + GUIUtils.GetPlayerStatString(statObject2) + ": " + GUIUtils.GetPlayerStatString(statObject) + " " + GUIUtils.GetPlayerStatString(obj) + "] ";
					}
					else if (objectReputationRequirementAttribute == null)
					{
						if (num4 < 0)
						{
							text2 = ((num2 < 0) ? ("[" + GUIUtils.GetPlayerStatString(statObject) + "] ") : ("[" + GUIUtils.GetPlayerStatString(statObject) + " " + (int)obj + "] "));
						}
						else
						{
							int stringID = (int)TypeDescriptor.GetConverter(parameters[num4].ParameterType).ConvertFromString(conditionalCall.Data.Parameters[num4]);
							text2 = "[" + StringTableManager.GetText(DatabaseString.StringTableType.Abilities, stringID) + "] ";
						}
					}
					if (firstonly && flag)
					{
						return text2;
					}
					text += text2;
				}
				return text;
			}
		}
		return string.Empty;
	}

	public string GetNodeText(FlowChartPlayer player, FlowChartNode node)
	{
		if (node != null)
		{
			if (LocalizationDebuggingEnabled && node.NodeType == FlowChartNodeType.Script)
			{
				return "Script Node " + node.NodeID;
			}
			string text = string.Empty;
			DialogueNode dialogueNode = node as DialogueNode;
			if (dialogueNode != null && dialogueNode.IsTempText)
			{
				text = "[Temp]";
			}
			if (GameState.Mode.Option.DisplayInteractionQualifier)
			{
				text += GetNodeQualifierHelper(node, player, checkpassing: true, passing: true, firstonly: true);
			}
			text += ReplaceTokens(StringTableManager.GetText(StringTableName, node.NodeID), player, node);
			List<ScriptCall> list = new List<ScriptCall>();
			list.AddRange(dialogueNode.OnEnterScripts);
			list.AddRange(dialogueNode.OnExitScripts);
			{
				foreach (ScriptCall item in list)
				{
					MethodInfo methodInfo = ScriptManager.GetMethodInfo(item);
					if (!(methodInfo != null))
					{
						continue;
					}
					object[] customAttributes = methodInfo.GetCustomAttributes(typeof(AdjustStatAttribute), inherit: false);
					if (customAttributes.Length == 0)
					{
						continue;
					}
					AdjustStatAttribute adjustStatAttribute = customAttributes[0] as AdjustStatAttribute;
					int num = 0;
					int num2 = -1;
					int num3 = -1;
					int num4 = -1;
					ParameterInfo[] parameters = methodInfo.GetParameters();
					ParameterInfo[] array = parameters;
					foreach (ParameterInfo obj in array)
					{
						if (string.Compare(obj.Name, adjustStatAttribute.ParamTypeName) == 0 && num < item.Data.Parameters.Count)
						{
							num2 = num;
						}
						if (string.Compare(obj.Name, adjustStatAttribute.ParamValueName) == 0 && num < item.Data.Parameters.Count)
						{
							num3 = num;
						}
						if (string.Compare(obj.Name, adjustStatAttribute.ParamLabelName) == 0 && num < item.Data.Parameters.Count)
						{
							num4 = num;
						}
						num++;
					}
					if (num2 >= 0 && num3 >= 0 && GameState.Mode.Option.DisplayPersonalityReputationIndicators)
					{
						object statObject = TypeDescriptor.GetConverter(parameters[num2].ParameterType).ConvertFromString(item.Data.Parameters[num2]);
						object statObject2 = TypeDescriptor.GetConverter(parameters[num3].ParameterType).ConvertFromString(item.Data.Parameters[num3]);
						if (num4 >= 0)
						{
							object statObject3 = TypeDescriptor.GetConverter(parameters[num4].ParameterType).ConvertFromString(item.Data.Parameters[num4]);
							text += StringUtility.Format(" [{1}: {2} {0}]", GUIUtils.GetPlayerStatString(statObject), GUIUtils.GetPlayerStatString(statObject3), GUIUtils.GetPlayerStatString(statObject2));
						}
						else
						{
							text += StringUtility.Format(" [{0}]", GUIUtils.GetPlayerStatString(statObject), GUIUtils.GetPlayerStatString(statObject2));
						}
					}
				}
				return text;
			}
		}
		return string.Empty;
	}

	public static string ReplacePlayerTokens(string nodeText)
	{
		if ((bool)GameState.s_playerCharacter)
		{
			CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			nodeText = nodeText.Replace("[Player Name]", CharacterStats.Name(GameState.s_playerCharacter.gameObject));
			nodeText = nodeText.Replace("[Player Race]", GUIUtils.GetPlayerRaceString(component.CharacterRace));
			nodeText = nodeText.Replace("[Player Subrace]", GUIUtils.GetPlayerSubraceString(component.CharacterSubrace));
			nodeText = nodeText.Replace("[Player Class]", GUIUtils.GetClassString(component.CharacterClass, component.Gender));
			nodeText = nodeText.Replace("[Player Culture]", GUIUtils.GetPlayerCultureString(component.CharacterCulture));
			nodeText = nodeText.Replace("[Player Deity]", GUIUtils.GetDeityString(component.Deity));
			nodeText = nodeText.Replace("[Player Paladin Order]", GUIUtils.GetPaladinOrderString(component.PaladinOrder, component.Gender));
		}
		return nodeText;
	}

	private string ReplaceTokens(string nodeText, FlowChartPlayer player, FlowChartNode node)
	{
		nodeText = ReplacePlayerTokens(nodeText);
		for (int i = 0; i < 30; i++)
		{
			string text = "[Slot " + i + "]";
			if (nodeText.Contains(text) && PartyMemberAI.PartyMembers[i] != null)
			{
				nodeText = nodeText.Replace(text, CharacterStats.Name(PartyMemberAI.PartyMembers[i].gameObject));
			}
			text = "[SkillCheck " + i + "]";
			if (nodeText.Contains(text))
			{
				nodeText = nodeText.Replace(text, CharacterStats.Name(InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSkillCheckGuid(i))));
			}
		}
		for (int j = 0; j < SpecialCharacterInstanceID.s_specifiedGuids.Length; j++)
		{
			string text2 = "[Specified " + j + "]";
			if (nodeText.Contains(text2))
			{
				GameObject objectByID = InstanceID.GetObjectByID(SpecialCharacterInstanceID.GetSpecifiedGuid(j));
				nodeText = nodeText.Replace(text2, CharacterStats.Name(objectByID));
			}
		}
		if (nodeText.Contains("[NPCBacker]"))
		{
			GameObject gameObject = null;
			gameObject = InstanceID.GetObjectByID(node.GetSpeakerGuid());
			if (gameObject == null)
			{
				gameObject = player.OwnerObject;
			}
			if ((bool)gameObject)
			{
				BackerContent component = gameObject.GetComponent<BackerContent>();
				if ((bool)component)
				{
					nodeText = nodeText.Replace("[NPCBacker]", component.BackerDescription.GetText());
				}
			}
		}
		nodeText = OrlansHead.ReplaceTokens(nodeText);
		nodeText = Dozens.ReplaceTokens(nodeText);
		return nodeText;
	}

	protected override FlowChartNode GetNextNode(List<FlowChartLink> links, FlowChartPlayer player)
	{
		if (links.Count <= 0)
		{
			return null;
		}
		DialogueNode dialogueNode = GetNode(player.CurrentNodeID) as DialogueNode;
		List<FlowChartLink> list = new List<FlowChartLink>();
		if (dialogueNode.PlayType == PlayType.Random)
		{
			List<FlowChartNode> list2 = new List<FlowChartNode>();
			List<DialogueLink> list3 = new List<DialogueLink>();
			int num = 0;
			foreach (FlowChartLink link in links)
			{
				if (link is DialogueLink dialogueLink)
				{
					list.Add(dialogueLink);
					FlowChartNode nextNode = base.GetNextNode(list, player);
					if (nextNode != null)
					{
						list2.Add(nextNode);
						list3.Add(dialogueLink);
						num += dialogueLink.RandomWeight;
					}
					list.Clear();
				}
			}
			if (list2.Count > 0)
			{
				num += (int)dialogueNode.NoPlayRandomWeight;
				int num2 = UnityEngine.Random.Range(0, num);
				int num3 = 0;
				for (int i = 0; i < list2.Count; i++)
				{
					if (num3 + list3[i].RandomWeight - 1 >= num2)
					{
						return list2[i];
					}
					num3 += list3[i].RandomWeight;
				}
			}
			return null;
		}
		if (dialogueNode.PlayType == PlayType.Normal)
		{
			return base.GetNextNode(links, player);
		}
		if (dialogueNode.PlayType == PlayType.CycleStop)
		{
			int nodeCyclePosition = ConversationManager.Instance.GetNodeCyclePosition(this, dialogueNode.NodeID);
			if (nodeCyclePosition >= links.Count - 1)
			{
				list.Add(links[links.Count - 1]);
			}
			else
			{
				for (int j = nodeCyclePosition + 1; j < links.Count; j++)
				{
					list.Add(links[j]);
				}
			}
		}
		else if (dialogueNode.PlayType == PlayType.CycleLoop)
		{
			int num4 = ConversationManager.Instance.GetNodeCyclePosition(this, dialogueNode.NodeID);
			if (num4 >= links.Count - 1)
			{
				num4 = -1;
			}
			num4++;
			for (int k = num4; k < links.Count; k++)
			{
				list.Add(links[k]);
			}
			for (int l = 0; l < num4; l++)
			{
				list.Add(links[l]);
			}
		}
		return base.GetNextNode(list, player);
	}

	public bool PassesConditionalsEx(FlowChartNode node, FlowChartPlayer player)
	{
		return PassesConditionals(node, player);
	}

	protected override bool PassesConditionals(FlowChartNode node, FlowChartPlayer player)
	{
		if (!PassesPersistence(node))
		{
			return false;
		}
		return base.PassesConditionals(node, player);
	}

	protected override bool PassesNonStatConditionals(FlowChartNode node, FlowChartPlayer player)
	{
		if (!PassesPersistence(node))
		{
			return false;
		}
		return base.PassesNonStatConditionals(node, player);
	}

	private bool PassesPersistence(FlowChartNode node)
	{
		if (node is DialogueNode dialogueNode)
		{
			if (dialogueNode.Persistence == PersistenceType.OncePerConversation)
			{
				if (OncePerConversationPlayedNodes.Contains(dialogueNode.NodeID))
				{
					return false;
				}
			}
			else if (dialogueNode.Persistence == PersistenceType.OnceEver && ConversationManager.Instance.GetMarkedAsRead(this, dialogueNode.NodeID))
			{
				return false;
			}
		}
		return true;
	}

	public bool ShouldShowPlayerResponses(FlowChartPlayer player)
	{
		return GetResponseNodes(player, qualifiedOnly: true).Count > 0;
	}

	public List<PlayerResponseNode> GetResponseNodes(FlowChartPlayer player)
	{
		return GetResponseNodes(player, qualifiedOnly: false);
	}

	public List<PlayerResponseNode> GetResponseNodes(FlowChartPlayer player, bool qualifiedOnly)
	{
		List<PlayerResponseNode> list = new List<PlayerResponseNode>();
		FlowChartNode node = GetNode(player.CurrentNodeID);
		if (node != null)
		{
			bool flag = false;
			List<FlowChartLink> links = node.Links;
			if (node.IsInContainer && GetNode(node.ContainerNodeID) is BankNode bankNode)
			{
				if (bankNode.BankNodePlayType == BankNodePlayType.PlayAll)
				{
					bool flag2 = false;
					foreach (int childNodeID in bankNode.ChildNodeIDs)
					{
						if (!flag2)
						{
							if (childNodeID == node.NodeID)
							{
								flag2 = true;
							}
							continue;
						}
						FlowChartNode nodeByID = base.Data.GetNodeByID(childNodeID);
						if (nodeByID == null || !PassesConditionals(nodeByID, player))
						{
							continue;
						}
						return list;
					}
				}
				links = bankNode.Links;
			}
			{
				foreach (FlowChartLink item in links)
				{
					FlowChartNode nodeByID2 = base.Data.GetNodeByID(item.ToNodeID);
					if (nodeByID2 == null)
					{
						continue;
					}
					if (nodeByID2.NodeType == FlowChartNodeType.Bank)
					{
						BankNode bankNode2 = nodeByID2 as BankNode;
						if (!PassesConditionals(nodeByID2, player))
						{
							continue;
						}
						foreach (int childNodeID2 in bankNode2.ChildNodeIDs)
						{
							FlowChartNode nodeByID3 = base.Data.GetNodeByID(childNodeID2);
							if (nodeByID3 != null)
							{
								if (flag && nodeByID3.NodeType != FlowChartNodeType.PlayerResponse)
								{
									continue;
								}
								if (PassesConditionals(nodeByID3, player))
								{
									if (nodeByID3.NodeType != FlowChartNodeType.PlayerResponse)
									{
										return list;
									}
									list.Add(nodeByID3 as PlayerResponseNode);
									flag = true;
								}
							}
							if (bankNode2.BankNodePlayType == BankNodePlayType.PlayAll)
							{
								break;
							}
						}
						continue;
					}
					DialogueNode dialogueNode = nodeByID2 as DialogueNode;
					DialogueLink link = item as DialogueLink;
					bool flag3 = ShouldDisplayQuestionNodeText(dialogueNode, link);
					bool flag4 = dialogueNode.IsQuestionNode && !flag3;
					if (flag && nodeByID2.NodeType != FlowChartNodeType.PlayerResponse && !flag4)
					{
						continue;
					}
					if (flag4)
					{
						if (!PassesConditionals(nodeByID2, player))
						{
							continue;
						}
						if (!QuestionNodeSelection.ContainsKey(dialogueNode.NodeID))
						{
							QuestionNodeSelection.Add(dialogueNode.NodeID, -1);
						}
						int num = QuestionNodeSelection[dialogueNode.NodeID];
						foreach (FlowChartLink link2 in dialogueNode.Links)
						{
							if (HideResponseForQuestionNodesCurrentPath && link2.ToNodeID == num)
							{
								continue;
							}
							FlowChartNode nodeByID4 = base.Data.GetNodeByID(link2.ToNodeID);
							if (nodeByID4.NodeType != FlowChartNodeType.PlayerResponse)
							{
								continue;
							}
							if (PassesConditionals(nodeByID4, player))
							{
								list.Add(nodeByID4 as PlayerResponseNode);
								flag = true;
								if (!QuestionNodeLinks.ContainsKey(nodeByID4.NodeID))
								{
									QuestionNodeLinks.Add(nodeByID4.NodeID, dialogueNode.NodeID);
								}
								else
								{
									QuestionNodeLinks[nodeByID4.NodeID] = dialogueNode.NodeID;
								}
							}
							else if (!qualifiedOnly && GameState.Option.DisplayUnqualifiedInteractions && PassesNonStatConditionals(nodeByID4, player))
							{
								list.Add(nodeByID4 as PlayerResponseNode);
							}
						}
					}
					else if (PassesConditionals(nodeByID2, player))
					{
						if (nodeByID2.NodeType != FlowChartNodeType.PlayerResponse)
						{
							return list;
						}
						list.Add(nodeByID2 as PlayerResponseNode);
						flag = true;
					}
					else if (!qualifiedOnly && GameState.Option.DisplayUnqualifiedInteractions && PassesNonStatConditionals(nodeByID2, player) && nodeByID2.NodeType == FlowChartNodeType.PlayerResponse)
					{
						list.Add(nodeByID2 as PlayerResponseNode);
					}
				}
				return list;
			}
		}
		return list;
	}

	public GameObject GetSpeakerOrPlayer(FlowChartNode node)
	{
		GameObject speaker = GetSpeaker(node);
		if ((bool)speaker)
		{
			return speaker;
		}
		if (!GameState.s_playerCharacter)
		{
			return null;
		}
		return GameState.s_playerCharacter.gameObject;
	}

	public GameObject GetSpeakerOrPlayer(FlowChartPlayer player)
	{
		GameObject speaker = GetSpeaker(player);
		if ((bool)speaker)
		{
			return speaker;
		}
		if (!GameState.s_playerCharacter)
		{
			return null;
		}
		return GameState.s_playerCharacter.gameObject;
	}

	public GameObject GetSpeaker(int nodeId)
	{
		return GetSpeaker(GetNode(nodeId));
	}

	public GameObject GetSpeaker(FlowChartNode node)
	{
		if (node == null)
		{
			return null;
		}
		Guid speakerGuid = node.GetSpeakerGuid();
		string value = TryMapSpeaker(speakerGuid);
		if (!string.IsNullOrEmpty(value))
		{
			return GameObject.Find(value);
		}
		StoredCharacterInfo storedCompanion = Stronghold.Instance.GetStoredCompanion(speakerGuid);
		if (storedCompanion != null)
		{
			return storedCompanion.gameObject;
		}
		if (speakerGuid != Guid.Empty)
		{
			List<GameObject> activeObjectsForSpeakerGuid = ConversationManager.GetActiveObjectsForSpeakerGuid(speakerGuid);
			if (activeObjectsForSpeakerGuid != null)
			{
				return activeObjectsForSpeakerGuid.FirstOrDefault();
			}
		}
		return null;
	}

	public static GameObject GetSpeaker(FlowChartPlayer player)
	{
		Conversation conversation = player.CurrentFlowChart as Conversation;
		if (!conversation)
		{
			return null;
		}
		FlowChartNode node = conversation.GetNode(player.CurrentNodeID);
		if (node == null)
		{
			return null;
		}
		Guid speakerGuid = node.GetSpeakerGuid();
		string value = conversation.TryMapSpeaker(speakerGuid);
		if (!string.IsNullOrEmpty(value))
		{
			return GameObject.Find(value);
		}
		if (SpecialCharacterInstanceID.GetSpecialTypeFromGuid(speakerGuid) != 0)
		{
			return InstanceID.GetObjectByID(speakerGuid);
		}
		StoredCharacterInfo storedCompanion = Stronghold.Instance.GetStoredCompanion(speakerGuid);
		if (storedCompanion != null)
		{
			return storedCompanion.gameObject;
		}
		List<GameObject> activeObjectsForSpeakerGuid = ConversationManager.GetActiveObjectsForSpeakerGuid(speakerGuid);
		if (player != null && (bool)player.OwnerObject && (bool)player.OwnerObject.GetComponent<CharacterStats>() && (activeObjectsForSpeakerGuid == null || !activeObjectsForSpeakerGuid.Any() || activeObjectsForSpeakerGuid.Contains(player.OwnerObject) || speakerGuid == Guid.Empty))
		{
			return player.OwnerObject;
		}
		return activeObjectsForSpeakerGuid?.FirstOrDefault();
	}
}
