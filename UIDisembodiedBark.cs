using System;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class UIDisembodiedBark : MonoBehaviour
{
	public UILabel[] Labels;

	public GameObject VfxRoot;

	private FlowChartPlayer m_ChartPlayer;

	private GameObject m_Speaker;

	private string m_SpeakerName;

	private int m_ShowNodeId;

	private ParticleSystem[] m_ParticleSystems;

	private TweenAlpha[] m_AlphaTweens;

	private TweenPosition[] m_PositionTweens;

	public static UIDisembodiedBark Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		m_ParticleSystems = VfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		m_AlphaTweens = GetComponentsInChildren<TweenAlpha>(includeInactive: true);
		m_PositionTweens = GetComponentsInChildren<TweenPosition>(includeInactive: true);
		UILabel[] labels = Labels;
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].text = "";
		}
		Hide();
	}

	private void Start()
	{
		ConversationManager instance = ConversationManager.Instance;
		instance.FlowChartPlayerAdded = (ConversationManager.FlowChartPlayerDelegate)Delegate.Combine(instance.FlowChartPlayerAdded, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerAdded));
		ConversationManager instance2 = ConversationManager.Instance;
		instance2.FlowChartPlayerRemoved = (ConversationManager.FlowChartPlayerDelegate)Delegate.Combine(instance2.FlowChartPlayerRemoved, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerRemoved));
	}

	private void OnDestroy()
	{
		if ((bool)ConversationManager.Instance)
		{
			ConversationManager instance = ConversationManager.Instance;
			instance.FlowChartPlayerAdded = (ConversationManager.FlowChartPlayerDelegate)Delegate.Remove(instance.FlowChartPlayerAdded, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerAdded));
			ConversationManager instance2 = ConversationManager.Instance;
			instance2.FlowChartPlayerRemoved = (ConversationManager.FlowChartPlayerDelegate)Delegate.Remove(instance2.FlowChartPlayerRemoved, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerRemoved));
		}
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void FlowChartPlayerAdded(FlowChartPlayer player)
	{
		if (player.CurrentNodeID != -1)
		{
			FlowChartNode flowChartNode = player.GetCurrentNode();
			if (flowChartNode is BankNode)
			{
				flowChartNode = player.CurrentFlowChart.GetNextNode(player);
				player.CurrentFlowChart.MoveToNode(flowChartNode.NodeID, player);
			}
			if ((!(flowChartNode is DialogueNode) || flowChartNode.NodeID == 0 || (flowChartNode as DialogueNode).DisplayType == DisplayType.Overlay) && !player.Completed)
			{
				m_ChartPlayer = player;
			}
		}
	}

	public void Reset()
	{
		UILabel[] labels = Labels;
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].text = "";
		}
		Hide();
		if (m_ParticleSystems != null)
		{
			for (int j = 0; j < m_ParticleSystems.Length; j++)
			{
				if (m_ParticleSystems[j] != null)
				{
					m_ParticleSystems[j].Clear();
				}
			}
		}
		m_ChartPlayer = null;
		m_Speaker = null;
		m_SpeakerName = null;
		m_ShowNodeId = -1;
	}

	private void FlowChartPlayerRemoved(FlowChartPlayer player)
	{
	}

	private void Reload()
	{
		if (m_ChartPlayer == null)
		{
			Hide();
			return;
		}
		m_ShowNodeId = m_ChartPlayer.CurrentNodeID;
		Conversation conversation = m_ChartPlayer.CurrentFlowChart as Conversation;
		if (conversation.GetNode(m_ChartPlayer.CurrentNodeID) != null)
		{
			m_Speaker = Conversation.GetSpeaker(m_ChartPlayer);
		}
		else
		{
			m_Speaker = null;
		}
		if ((bool)m_Speaker)
		{
			m_SpeakerName = CharacterStats.Name(m_Speaker);
		}
		else
		{
			m_SpeakerName = "";
		}
		if (m_Speaker == null)
		{
			m_Speaker = m_ChartPlayer.OwnerObject;
		}
		ShowText(conversation.GetActiveNodeText(m_ChartPlayer));
	}

	public void ShowText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = CharacterStats.NameColored(m_Speaker) + ": " + text;
			for (int i = 0; i < Labels.Length; i++)
			{
				Labels[i].text = text2;
			}
			Show();
			VfxRoot.transform.localScale = new Vector3(Labels[0].transform.localScale.x * Labels[0].relativeSize.x, Labels[0].transform.localScale.y * Labels[0].relativeSize.y, 1f);
			string characterColor = UIConversationManager.Instance.GetCharacterColor(m_Speaker);
			string text3 = UIConversationManager.Instance.ProcessSpeakerText(text, m_SpeakerName, characterColor, "[FFFFFF]");
			if (!string.IsNullOrEmpty(text3))
			{
				Console.AddMessage(text3.Trim(), Console.ConsoleState.Dialogue);
			}
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		for (int i = 0; i < Labels.Length; i++)
		{
			for (int j = 0; j < m_AlphaTweens.Length; j++)
			{
				m_AlphaTweens[j].Play(forward: true);
			}
			for (int k = 0; k < m_PositionTweens.Length; k++)
			{
				m_PositionTweens[k].Reset();
				m_PositionTweens[k].Play(forward: true);
			}
		}
		for (int l = 0; l < m_ParticleSystems.Length; l++)
		{
			m_ParticleSystems[l].Play();
		}
	}

	public void Hide()
	{
		for (int i = 0; i < Labels.Length; i++)
		{
			for (int j = 0; j < m_AlphaTweens.Length; j++)
			{
				if (!(m_AlphaTweens[j].delay > 0f))
				{
					m_AlphaTweens[j].Play(forward: false);
				}
			}
		}
		for (int k = 0; k < m_ParticleSystems.Length; k++)
		{
			m_ParticleSystems[k].Stop();
		}
	}

	private void Update()
	{
		if (m_ChartPlayer == null)
		{
			return;
		}
		if (m_ChartPlayer.Completed)
		{
			m_ChartPlayer = null;
			return;
		}
		m_ChartPlayer.Timer -= TimeController.NotSpedUpDeltaTime;
		if (m_ChartPlayer.IsTimerOrVoFinished())
		{
			m_ChartPlayer.Timer = 0f;
			UIBarkString.AdvanceConversation(m_ChartPlayer);
		}
		if (m_ChartPlayer.CurrentNodeID != m_ShowNodeId)
		{
			Reload();
		}
	}
}
