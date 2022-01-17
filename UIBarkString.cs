using System;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIBarkString : UIScreenRectangleItem
{
	private FlowChartPlayer m_ChartPlayer;

	private GameObject m_Speaker;

	private string m_SpeakerName;

	private Health m_SpeakerHealth;

	private NPCDialogue m_SpeakerDialogue;

	private bool m_SpeakerVisible;

	public UILabel Label;

	public UIWidget Background;

	private UIAnchorToWorld m_WorldAnchor;

	public TweenAlpha ShowHideTween;

	private bool m_NeedsShow;

	private float m_DeathTimer;

	private bool m_UseDeathTimer;

	private int m_ShowNodeId;

	private string m_ManualString;

	private bool m_HasSeenCurrentNode;

	private string m_CurrentConsoleMessage;

	private bool m_Kill;

	public event Action<UIBarkString> OnDestroyed;

	public override Rect GetScreenBounds()
	{
		Init();
		return new Rect(BasePosition.x - (float)Label.lineWidth / 2f, BasePosition.y + Label.relativeSize.y * Label.transform.localScale.y, Label.lineWidth, Label.relativeSize.y * Label.transform.localScale.y);
	}

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
		if (m_WorldAnchor == null)
		{
			m_WorldAnchor = GetComponent<UIAnchorToWorld>();
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		if (scene != base.gameObject.scene)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		MyDestroy();
		m_Speaker = null;
		m_SpeakerHealth = null;
		m_SpeakerDialogue = null;
		if ((bool)UIBarkstringManager.Instance)
		{
			UIBarkstringManager.Instance.ReportKill(this);
		}
		if (this.OnDestroyed != null)
		{
			this.OnDestroyed(this);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnApplicationQuit()
	{
		MyDestroy();
	}

	private void MyDestroy()
	{
		if (m_ChartPlayer != null && m_ChartPlayer.CurrentNodeID != -1 && ConversationManager.Instance != null)
		{
			if (!m_ChartPlayer.Completed)
			{
				ConversationManager.Instance.EndConversation(m_ChartPlayer);
			}
			m_ChartPlayer = null;
		}
	}

	public GameObject GetSpeaker()
	{
		return m_Speaker;
	}

	public bool DataIs(FlowChartPlayer data)
	{
		return data == m_ChartPlayer;
	}

	public void SetData(FlowChartPlayer data)
	{
		m_ChartPlayer = data;
		Reload();
	}

	public void ManualSetData(string text, GameObject anchor, float lifetime)
	{
		Init();
		m_ManualString = text;
		m_WorldAnchor.SetAnchor(anchor);
		Reload();
		SetLifetime(lifetime);
	}

	public void ManualSetData(string text, Vector3 position, float lifetime)
	{
		Init();
		m_ManualString = text;
		m_WorldAnchor.SetAnchorPos(position);
		Reload();
		SetLifetime(lifetime);
	}

	public void ManualSetData(string text, float lifetime)
	{
		m_ManualString = text;
		Reload();
		SetLifetime(lifetime);
	}

	public void Kill(bool instant = false, bool finishScripts = false)
	{
		if (finishScripts && m_ChartPlayer != null)
		{
			Conversation conversation = m_ChartPlayer.CurrentFlowChart as Conversation;
			FlowChartNode nextNode = conversation.GetNextNode(m_ChartPlayer);
			if (nextNode != null && m_ChartPlayer.CurrentNodeID != -1)
			{
				while (nextNode != null)
				{
					conversation.MoveToNode(nextNode.NodeID, m_ChartPlayer);
					m_ChartPlayer.CurrentNodeID = nextNode.NodeID;
					nextNode = conversation.GetNextNode(m_ChartPlayer);
				}
				if (!m_ChartPlayer.Completed)
				{
					ConversationManager.Instance.EndConversation(m_ChartPlayer);
				}
				m_ChartPlayer.CurrentNodeID = -1;
			}
		}
		m_Kill = true;
		if ((bool)m_WorldAnchor)
		{
			m_WorldAnchor.enabled = false;
		}
		if (instant)
		{
			GameUtilities.Destroy(base.gameObject);
			return;
		}
		if ((bool)m_Speaker)
		{
			Faction component = m_Speaker.GetComponent<Faction>();
			if ((bool)component)
			{
				component.NotifyBeginSpeaking(null, state: false);
			}
		}
		UIBarkstringManager.Instance.ReportKill(this);
		ShowHideTween.Play(forward: false);
		UpdateTransparency();
	}

	public void SetLifetime(float seconds)
	{
		if (m_ChartPlayer != null)
		{
			m_ChartPlayer.Timer = seconds;
			return;
		}
		m_UseDeathTimer = true;
		m_DeathTimer = seconds;
	}

	private void OnTransitionFinished()
	{
		if (m_Kill)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	public void Show()
	{
		ShowHideTween.Play(forward: true);
	}

	private void Reload()
	{
		if (m_Kill)
		{
			return;
		}
		UpdateTransparency();
		Init();
		if (m_ChartPlayer == null)
		{
			Label.text = m_ManualString;
			return;
		}
		m_ShowNodeId = m_ChartPlayer.CurrentNodeID;
		Conversation conversation = m_ChartPlayer.CurrentFlowChart as Conversation;
		FlowChartNode node = conversation.GetNode(m_ChartPlayer.CurrentNodeID);
		GameObject speaker = m_Speaker;
		if (node != null)
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
			m_SpeakerHealth = m_Speaker.GetComponent<Health>();
			m_SpeakerDialogue = m_Speaker.GetComponent<NPCDialogue>();
		}
		else
		{
			m_SpeakerName = "";
			m_SpeakerHealth = null;
			m_SpeakerDialogue = null;
		}
		if (m_Speaker == null)
		{
			m_Speaker = m_ChartPlayer.OwnerObject;
		}
		if (m_Speaker != speaker)
		{
			if ((bool)m_Speaker)
			{
				Faction component = m_Speaker.GetComponent<Faction>();
				if ((bool)component)
				{
					component.NotifyBeginSpeaking(conversation, state: true);
				}
			}
			if ((bool)speaker)
			{
				Faction component2 = speaker.GetComponent<Faction>();
				if ((bool)component2)
				{
					component2.NotifyBeginSpeaking(null, state: false);
				}
			}
		}
		m_WorldAnchor.SetAnchor(m_Speaker);
		string activeNodeText = conversation.GetActiveNodeText(m_ChartPlayer);
		Label.text = activeNodeText;
		m_NeedsShow = true;
		string characterColor = UIConversationManager.Instance.GetCharacterColor(m_Speaker);
		m_CurrentConsoleMessage = UIConversationManager.Instance.ProcessSpeakerText(Label.text, m_SpeakerName, characterColor, "[FFFFFF]");
		m_HasSeenCurrentNode = false;
	}

	private void UpdateTransparency()
	{
		float fogAlpha = GetFogAlpha();
		Label.alpha = fogAlpha * (m_SpeakerVisible ? 1f : 0f);
		if ((bool)Background)
		{
			Background.alpha = Label.alpha * 0.5f;
		}
	}

	private float GetFogAlpha()
	{
		if (FogOfWar.Instance != null && m_WorldAnchor != null)
		{
			Vector3 worldPosition = m_WorldAnchor.AnchorPos;
			if (m_WorldAnchor.Anchor != null)
			{
				worldPosition = m_WorldAnchor.Anchor.transform.position;
			}
			if (worldPosition.sqrMagnitude > float.Epsilon)
			{
				if (FogOfWar.Instance.PointVisible(worldPosition))
				{
					return 1f;
				}
				return 0f;
			}
		}
		return 1f;
	}

	private void Update()
	{
		if ((bool)m_SpeakerHealth && m_SpeakerHealth.Dead)
		{
			Kill();
		}
		UpdateTransparency();
		if (m_Kill)
		{
			return;
		}
		Init();
		if (m_NeedsShow)
		{
			m_NeedsShow = false;
			Show();
		}
		if (m_Speaker == null)
		{
			m_SpeakerVisible = true;
		}
		else
		{
			m_SpeakerVisible = !m_SpeakerDialogue || m_SpeakerDialogue.IsVisible;
		}
		if (!m_HasSeenCurrentNode && m_SpeakerVisible && !string.IsNullOrEmpty(m_CurrentConsoleMessage))
		{
			Console.AddMessage(m_CurrentConsoleMessage.Trim(), Console.ConsoleState.Dialogue);
			m_HasSeenCurrentNode = true;
		}
		if ((bool)m_Speaker && (bool)m_SpeakerHealth && (m_SpeakerHealth.Dead || m_SpeakerHealth.Unconscious || !m_SpeakerHealth.gameObject.activeInHierarchy))
		{
			ConversationManager.Instance.EndConversation(m_ChartPlayer);
		}
		if (m_ChartPlayer != null)
		{
			m_ChartPlayer.Timer -= TimeController.NotSpedUpDeltaTime;
			if (m_ChartPlayer.IsTimerOrVoFinished())
			{
				m_ChartPlayer.Timer = 0f;
				m_HasSeenCurrentNode = false;
				AdvanceConversation(m_ChartPlayer);
			}
			if (m_ChartPlayer.CurrentNodeID != m_ShowNodeId)
			{
				Reload();
			}
		}
		if (m_UseDeathTimer)
		{
			m_DeathTimer -= Time.unscaledDeltaTime;
			if (m_DeathTimer < 0f && !TimeController.Instance.Paused)
			{
				Kill();
			}
		}
		m_WorldAnchor.UpdatePosition();
		BasePosition = m_WorldAnchor.Position;
		base.transform.localPosition = base.ScreenPosition;
	}

	public static void AdvanceConversation(FlowChartPlayer chartPlayer)
	{
		Conversation conversation = chartPlayer.CurrentFlowChart as Conversation;
		FlowChartNode nextNode = conversation.GetNextNode(chartPlayer);
		DialogueNode dialogueNode = nextNode as DialogueNode;
		bool flag = true;
		while (nextNode != null && (flag || nextNode is ScriptNode || (dialogueNode != null && dialogueNode.DisplayType == DisplayType.Hidden)))
		{
			nextNode = conversation.GetNextNode(chartPlayer);
			dialogueNode = nextNode as DialogueNode;
			conversation.MoveToNode(nextNode.NodeID, chartPlayer);
			chartPlayer.CurrentNodeID = nextNode.NodeID;
			flag = false;
		}
		if (nextNode == null)
		{
			ConversationManager.Instance.EndConversation(chartPlayer);
			chartPlayer.CurrentNodeID = -1;
		}
	}

	public FlowChartNode GetCurrentNode()
	{
		if (m_ChartPlayer != null)
		{
			Conversation conversation = m_ChartPlayer.CurrentFlowChart as Conversation;
			if (conversation != null)
			{
				return conversation.GetActiveNode(m_ChartPlayer);
			}
		}
		return null;
	}
}
