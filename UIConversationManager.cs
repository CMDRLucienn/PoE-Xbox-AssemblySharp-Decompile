using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnimationOrTween;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class UIConversationManager : UIHudWindow
{
	public GameObject ConversationWindow;

	public GameObject InteractionWindow;

	public UIPanel ConversationPanel;

	public UIPanel InteractionPanel;

	public bool RemoveNameFromDescLines = true;

	public Color ConversationDescriptionColor;

	public Color ConversationResponseColor;

	public Color ConversationResponseMouseoverColor;

	public Color ConversationResponseReadColor;

	public Color ConversationResponseDisabledColor;

	public Color ConversationSpeakerNameColor;

	public Color ConversationSpeakerTextColor;

	public Color ConversationPlayerNameColor;

	public Color ConversationPlayerTextColor;

	public Color ConversationEventColor;

	[HideInInspector]
	public string ConversationDescriptionColorString = "";

	[HideInInspector]
	public string ConversationSpeakerNameColorString = "";

	public Color InteractionResponseColor;

	public Color InteractionResponseMouseoverColor;

	public Color InteractionResponseReadColor;

	public Color InteractionTextColor;

	public Color InteractionEventColor;

	public UIMultiSpriteImageButton ContinueButton;

	public UITexture SpeakerPortraitTexture;

	public GameObject SpeakerPortraitBox;

	public UIWidget TopHandle;

	public UIPanel ContentPanel;

	public UIConsoleText ConversationTextList;

	public GameObject[] TweenOnShowHide;

	public GameObject Padder;

	public GameObject ConversationBox;

	public UISprite ConversationMoreContentArrow;

	public UITexture InteractionBackground;

	public UIDissolve InteractionDissolveController;

	public UITweener InteractionDissolveTweener;

	public UIPanel InteractionContentPanel;

	public UIConsoleText InteractionTextList;

	public UICapitularLabel InteractionLabel;

	private Texture NextInteractionTexture;

	private FlowChartPlayer m_ActiveFlowChart;

	private int m_CurrentNodeId = -1;

	private Vector3 m_LastSpeakerPos;

	public int HandleMinPosition;

	private bool m_HandlePressed;

	private Vector3 m_TextInitialTransform;

	private Vector3 m_InteractionTextInitialTransform;

	private int mWasDownLine = -1;

	private int m_ControllerSelection = -1;

	private int m_MouseSelection = -1;

	public UISprite Background;

	private float m_LastClickPos;

	private int m_TextListActiveResponseLine = -1;

	private string m_MouseTextPreviousText = "";

	private int m_OutstandingResponses;

	private bool m_handlingFade;

	private bool m_Init;

	private GameObject speaker;

	private bool m_FirstLoad = true;

	private static char[] m_QuotePairs = new char[24]
	{
		'"', '"', '“', '”', '„', '“', '„', '”', '«', '»',
		'»', '«', '슻', '슫', '「', '」', '『', '』', '‹', '›',
		'‚', '‘', '‘', '’'
	};

	private bool m_IncludedName;

	private bool m_ConversationSuspended;

	private bool m_InteractionSuspended;

	public static UIConversationManager Instance { get; private set; }

	private bool IsConversation
	{
		get
		{
			if (m_ActiveFlowChart != null)
			{
				return m_ActiveFlowChart.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Standard;
			}
			return false;
		}
	}

	private bool IsInteraction
	{
		get
		{
			if (m_ActiveFlowChart != null)
			{
				return m_ActiveFlowChart.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Interaction;
			}
			return false;
		}
	}

	private UIConsoleText TextList
	{
		get
		{
			if (IsInteraction)
			{
				return InteractionTextList;
			}
			return ConversationTextList;
		}
	}

	private string m_RespColor => EncodeColor(IsInteraction ? InteractionResponseColor : ConversationResponseColor);

	private string m_RespMousedColor => EncodeColor(IsInteraction ? InteractionResponseMouseoverColor : ConversationResponseMouseoverColor);

	private string m_FollowedRespColor => EncodeColor(IsInteraction ? InteractionResponseReadColor : ConversationResponseReadColor);

	private string m_DisabledRespColor => EncodeColor(ConversationResponseDisabledColor);

	private string m_SpeakerNameColor => EncodeColor(ConversationSpeakerNameColor);

	private string m_SpeakerTextColor => EncodeColor(IsInteraction ? InteractionTextColor : ConversationSpeakerTextColor);

	private string m_EventColor => EncodeColor(IsInteraction ? InteractionEventColor : ConversationEventColor);

	private string m_PlayerSpeakerTextColor => EncodeColor(ConversationPlayerTextColor);

	private Conversation conversation
	{
		get
		{
			if (m_ActiveFlowChart == null)
			{
				return null;
			}
			return m_ActiveFlowChart.CurrentFlowChart as Conversation;
		}
	}

	private int m_HandleMaxPosition => (int)((float)Screen.height * InGameUILayout.toNguiScale / 2f - ConversationBox.transform.localPosition.y) * 2;

	public bool ForceRefresh { private get; set; }

	public static string GetColorOrEmpty(GameObject speaker)
	{
		if (speaker != null)
		{
			NPCAppearance component = speaker.GetComponent<NPCAppearance>();
			if ((bool)component)
			{
				Color primaryColor = component.primaryColor;
				if (primaryColor.grayscale < 0.3f)
				{
					primaryColor.r = 1f - 0.7f * (1f - primaryColor.r);
					primaryColor.b = 1f - 0.7f * (1f - primaryColor.b);
					primaryColor.g = 1f - 0.7f * (1f - primaryColor.g);
				}
				return "[" + NGUITools.EncodeColor(primaryColor) + "]";
			}
		}
		return "";
	}

	public string GetCharacterColor(GameObject speaker)
	{
		string text = GetColorOrEmpty(speaker);
		if (string.IsNullOrEmpty(text))
		{
			text = m_SpeakerNameColor;
		}
		return text;
	}

	private string EncodeColor(Color color)
	{
		return "[" + NGUITools.EncodeColor(color) + "]";
	}

	private void Awake()
	{
		Instance = this;
		InteractionDissolveTweener = InteractionBackground.GetComponent<UITweener>();
		CanDeactivate = false;
		if (ConversationPanel != null)
		{
			ConversationPanel.gameObject.SetActive(value: true);
		}
		if (InteractionPanel != null)
		{
			InteractionPanel.gameObject.SetActive(value: true);
		}
		ConversationDescriptionColorString = "[" + NGUITools.EncodeColor(ConversationDescriptionColor) + "]";
		ConversationSpeakerNameColorString = "[" + NGUITools.EncodeColor(ConversationSpeakerNameColor) + "]";
	}

	private void OnDissolveDone()
	{
		if ((bool)NextInteractionTexture)
		{
			if (InteractionDissolveController != null)
			{
				InteractionDissolveController.RandomOffsetDissolveTexture();
			}
			InteractionBackground.mainTexture = NextInteractionTexture;
			InteractionDissolveTweener.Play(forward: true);
			NextInteractionTexture = null;
		}
	}

	private void Start()
	{
		UIMultiSpriteImageButton continueButton = ContinueButton;
		continueButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(continueButton.onClick, new UIEventListener.VoidDelegate(OnButton));
		UIEventListener uIEventListener = UIEventListener.Get(TopHandle.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(HandlePressed));
		m_TextInitialTransform = ConversationTextList.transform.localPosition;
		m_InteractionTextInitialTransform = InteractionLabel.transform.localPosition;
		UIDraggablePanel component = ContentPanel.GetComponent<UIDraggablePanel>();
		if (component != null)
		{
			component.onScrollChanged = (UIEventListener.VoidDelegate)Delegate.Combine(component.onScrollChanged, new UIEventListener.VoidDelegate(OnConversationScrollChanged));
		}
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if (ContinueButton != null)
		{
			UIMultiSpriteImageButton continueButton = ContinueButton;
			continueButton.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(continueButton.onClick, new UIEventListener.VoidDelegate(OnButton));
		}
		if (TopHandle != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(TopHandle.gameObject);
			if ((bool)uIEventListener)
			{
				uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Remove(uIEventListener.onPress, new UIEventListener.BoolDelegate(HandlePressed));
			}
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void HandleInput()
	{
		if (m_ActiveFlowChart == null)
		{
			return;
		}
		if (m_HandlePressed)
		{
			Vector3 vector = new Vector3(TopHandle.gameObject.transform.localPosition.x, Mathf.Clamp(TopHandle.gameObject.transform.localPosition.y + GameInput.MousePosition.y - m_LastClickPos, HandleMinPosition, m_HandleMaxPosition), TopHandle.gameObject.transform.localPosition.z);
			m_LastClickPos = GameInput.MousePosition.y;
			float num = vector.y - TopHandle.transform.localPosition.y;
			Background.transform.localScale += new Vector3(0f, num, 0f);
			Vector4 clipRange = ContentPanel.clipRange;
			ContentPanel.clipRange = new Vector4(clipRange.x, clipRange.y + num / 2f, clipRange.z, clipRange.w + num);
			float y = Math.Max(ConversationTextList.transform.localPosition.y + num, ContentPanel.clipRange.y - ContentPanel.clipRange.w / 2f + ContentPanel.clipSoftness.y);
			ConversationTextList.transform.localPosition = new Vector3(ConversationTextList.transform.localPosition.x, y, ConversationTextList.transform.localPosition.z);
			Padder.transform.localPosition -= new Vector3(0f, num / 2f, 0f);
		}
		int controllerSelection = m_ControllerSelection;
		int mouseSelection = m_MouseSelection;
		if (GameInput.GetControlDownWithRepeat(MappedControl.UI_UP, handle: true))
		{
			m_ControllerSelection--;
			if (m_ControllerSelection < 0)
			{
				m_ControllerSelection = m_OutstandingResponses - 1;
			}
		}
		else if (GameInput.GetControlDownWithRepeat(MappedControl.UI_DOWN, handle: true))
		{
			m_ControllerSelection++;
			if (m_ControllerSelection >= m_OutstandingResponses)
			{
				m_ControllerSelection = 0;
			}
		}
		m_MouseSelection = LineToResponse(TextList.LineAt(GameInput.MousePosition));
		int num2 = m_MouseSelection;
		int num3 = mouseSelection;
		if (m_MouseSelection != mouseSelection)
		{
			m_ControllerSelection = -1;
		}
		if (m_ControllerSelection >= 0)
		{
			num2 = m_ControllerSelection;
			num3 = controllerSelection;
		}
		if (num2 != num3)
		{
			if (m_TextListActiveResponseLine >= 0)
			{
				TextList.Alter(m_TextListActiveResponseLine, m_MouseTextPreviousText);
			}
			if (num2 >= 0 && num2 < m_OutstandingResponses)
			{
				int num4 = ResponseToLine(num2);
				List<PlayerResponseNode> responseNodes = conversation.GetResponseNodes(m_ActiveFlowChart);
				m_MouseTextPreviousText = TextList.GetParagraph(num4);
				if (num2 >= responseNodes.Count || conversation.PassesConditionalsEx(responseNodes[num2], m_ActiveFlowChart))
				{
					TextList.Alter(num4, m_RespMousedColor + NGUITools.StripColorSymbols(m_MouseTextPreviousText) + "[-]");
				}
				m_TextListActiveResponseLine = num4;
			}
			else
			{
				m_TextListActiveResponseLine = -1;
			}
		}
		bool num5 = !conversation.ShouldShowPlayerResponses(m_ActiveFlowChart);
		if (num5 && GameInput.GetControlDown(MappedControl.CONV_CONTINUE))
		{
			ContinueButton.ForceDown(state: true);
			if (ContinueButton.gameObject.activeInHierarchy)
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ButtonDown);
			}
			Vector3 localPosition = ContinueButton.Label.transform.localPosition;
			localPosition.z = -4f;
			ContinueButton.Label.transform.localPosition = localPosition;
		}
		if (num5 && GameInput.GetControlUp(MappedControl.CONV_CONTINUE))
		{
			if (ContinueButton.gameObject.activeInHierarchy)
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ButtonUp);
			}
			OnButton(null);
			ContinueButton.ForceDown(state: false);
			Vector3 localPosition2 = ContinueButton.Label.transform.localPosition;
			localPosition2.z = -4f;
			ContinueButton.Label.transform.localPosition = localPosition2;
		}
		else if (GameInput.NumberPressed > 0)
		{
			PlayerKeyInput(GameInput.NumberPressed - 1);
		}
		else if (GameInput.GetControlDown(MappedControl.UI_SELECT) || GameInput.GetMouseButtonDown(0, setHandled: true))
		{
			mWasDownLine = ((m_ControllerSelection >= 0) ? m_ControllerSelection : m_MouseSelection);
		}
		else if (mWasDownLine >= 0 && (GameInput.GetMouseButtonUp(0, setHandled: true) || GameInput.GetControlUp(MappedControl.UI_SELECT)))
		{
			if (mWasDownLine == num2)
			{
				PlayerKeyInput(num2);
			}
			mWasDownLine = -1;
		}
		base.HandleInput();
	}

	private int LineToResponse(int line)
	{
		return m_OutstandingResponses - TextList.ParagraphCount + line;
	}

	private int ResponseToLine(int response)
	{
		return response - m_OutstandingResponses + TextList.ParagraphCount;
	}

	private void Update()
	{
		if (!m_Init)
		{
			m_Init = true;
			HideConversation();
			HideInteraction();
		}
		CheckRecreateContent();
	}

	private void PlayerKeyInput(int number)
	{
		if (m_ActiveFlowChart != null && (conversation.ShouldShowPlayerResponses(m_ActiveFlowChart) || number == 0))
		{
			if (Conversation.LocalizationDebuggingEnabled)
			{
				PlayerInputDebug(number);
			}
			else
			{
				PlayerInput(number);
			}
		}
	}

	private void PlayerInput(int number)
	{
		bool num = conversation.ShouldShowPlayerResponses(m_ActiveFlowChart);
		List<PlayerResponseNode> responseNodes = conversation.GetResponseNodes(m_ActiveFlowChart, qualifiedOnly: true);
		List<PlayerResponseNode> responseNodes2 = conversation.GetResponseNodes(m_ActiveFlowChart);
		if (num && number >= 0 && number < responseNodes2.Count)
		{
			if (!conversation.PassesConditionalsEx(responseNodes2[number], m_ActiveFlowChart))
			{
				return;
			}
			CheckRemoveResponses();
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ButtonDown);
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ButtonUp);
			MoveToNode(responseNodes2[number].NodeID);
			string text = GUIUtils.GetText(1883);
			string text2 = CharacterStats.NameColored(GameState.s_playerCharacter.gameObject) + text + m_PlayerSpeakerTextColor + conversation.GetActiveNodeText(m_ActiveFlowChart);
			ConsoleMessage(text2);
			if (!IsInteraction)
			{
				TextList.Add(text2 + Environment.NewLine);
			}
			if (conversation.GetResponseNodes(m_ActiveFlowChart).Count == 0)
			{
				FlowChartNode nextNode = conversation.GetNextNode(m_ActiveFlowChart);
				if (nextNode != null)
				{
					MoveToNode(nextNode.NodeID);
					ForceRefresh = true;
				}
				else
				{
					End();
				}
			}
		}
		else if (responseNodes.Count == 0 && IsInteraction && number == 0)
		{
			FlowChartNode nextNode2 = conversation.GetNextNode(m_ActiveFlowChart);
			if (nextNode2 != null)
			{
				MoveToNode(nextNode2.NodeID);
			}
			else
			{
				End();
			}
		}
	}

	private void PlayerInputDebug(int number)
	{
		List<FlowChartNode> allNodesFromActiveNode = conversation.GetAllNodesFromActiveNode(m_ActiveFlowChart);
		if (number >= 0 && number < allNodesFromActiveNode.Count)
		{
			CheckRemoveResponses();
			MoveToNode(allNodesFromActiveNode[number].NodeID);
			ForceRefresh = true;
		}
		else if (number == allNodesFromActiveNode.Count)
		{
			CheckRemoveResponses();
			conversation.MoveToPreviousNode(m_ActiveFlowChart);
			ForceRefresh = true;
		}
		else if (IsInteraction && number == allNodesFromActiveNode.Count + 1)
		{
			EndInteraction();
		}
	}

	private void CheckRecreateContent()
	{
		if (ConversationManager.Instance == null || !UIWindowManager.Instance || !UIWindowManager.Instance.WindowCanShow(this))
		{
			return;
		}
		FlowChartPlayer activeConversationForHUD = ConversationManager.Instance.GetActiveConversationForHUD();
		if (WindowActive() && activeConversationForHUD == null)
		{
			Console.Instance.ClearDialogueMessages();
			HideWindow(force: true);
		}
		else if (activeConversationForHUD != null && (activeConversationForHUD != m_ActiveFlowChart || (m_ActiveFlowChart != null && m_CurrentNodeId != m_ActiveFlowChart.CurrentNodeID) || ForceRefresh))
		{
			ForceRefresh = m_FirstLoad && IsInteraction;
			m_FirstLoad = false;
			CheckRemoveResponses();
			ContinueButton.gameObject.SetActive(value: false);
			if (m_ActiveFlowChart == null)
			{
				if (activeConversationForHUD.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Standard)
				{
					ShowConversation();
				}
				else if (activeConversationForHUD.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Interaction)
				{
					ShowInteraction();
				}
			}
			else if (activeConversationForHUD == null)
			{
				HideConversation();
				HideInteraction();
				return;
			}
			m_ActiveFlowChart = activeConversationForHUD;
			m_CurrentNodeId = m_ActiveFlowChart.CurrentNodeID;
			if (speaker != null)
			{
				Faction component = speaker.GetComponent<Faction>();
				if ((bool)component)
				{
					component.NotifyBeginSpeaking(null, state: false);
				}
			}
			if (IsInteraction && conversation.GetNode(m_ActiveFlowChart.CurrentNodeID) is PlayerResponseNode)
			{
				FlowChartNode nextNode = conversation.GetNextNode(m_ActiveFlowChart);
				if (nextNode != null)
				{
					MoveToNode(nextNode.NodeID);
				}
				else
				{
					EndConversation();
				}
				return;
			}
			speaker = Conversation.GetSpeaker(m_ActiveFlowChart);
			string text = "";
			if ((bool)speaker && !conversation.IsSpeakerNameOfActiveNodeHidden(m_ActiveFlowChart))
			{
				text = CharacterStats.Name(speaker);
			}
			if (IsInteraction)
			{
				InteractionTextList.Clear();
			}
			if (IsConversation)
			{
				bool flag = false;
				if (speaker != null)
				{
					Faction component2 = speaker.GetComponent<Faction>();
					if ((bool)component2)
					{
						component2.NotifyBeginSpeaking(activeConversationForHUD.CurrentFlowChart as Conversation, state: true);
					}
					PartyMemberAI component3 = speaker.GetComponent<PartyMemberAI>();
					if ((bool)component3 && component3.enabled)
					{
						flag = true;
					}
				}
				if (speaker != null && m_LastSpeakerPos != speaker.transform.position)
				{
					m_LastSpeakerPos = speaker.transform.position;
					if (flag)
					{
						CameraControl.Instance.FocusOnObjectOffsetLimited(speaker, 0.6f, new Vector3(0f, -3f, 0f), 15f);
					}
					else
					{
						CameraControl.Instance.FocusOnObjectOffset(speaker, 0.6f, new Vector3(0f, -3f, 0f));
					}
				}
			}
			bool flag2 = false;
			bool flag3 = true;
			string text2 = conversation.GetActiveNodeText(m_ActiveFlowChart);
			bool suppressIllumatedLetter = GetSuppressIllumatedLetter(m_ActiveFlowChart.CurrentFlowChart.GetNode(m_ActiveFlowChart.CurrentNodeID));
			InteractionLabel.IlluminationEnabled = !suppressIllumatedLetter;
			if (IsInteraction && text2.Length > 0 && text2[0] == '[')
			{
				for (int i = 0; i < text2.Length; i++)
				{
					if (text2[i] == ']' && (i + 1 >= text2.Length || text2[i + 1] != '['))
					{
						text2 = text2.Substring(i + 1) + "\n" + text2.Substring(0, i + 1);
						break;
					}
				}
			}
			int num = 0;
			string[] array = text2.Split('\n');
			foreach (string text3 in array)
			{
				if (string.IsNullOrEmpty(text3))
				{
					num += TextList.Add("");
					continue;
				}
				string characterColor = GetCharacterColor(speaker);
				string text4 = text3;
				string text5 = text3;
				if (IsInteraction && flag3)
				{
					InteractionLabel.text = text5;
					if (!suppressIllumatedLetter)
					{
						bool flag4 = text5.Length > 0 && text5[0] >= '0' && text5[0] <= '9';
						text5 = InteractionLabel.Prepare(text5);
						if (text5.Length > 0 && !flag4 && InteractionLabel.IlluminationEnabled)
						{
							text5 = text5.Remove(0, 1);
						}
					}
					flag3 = false;
				}
				string empty = string.Empty;
				if (IsInteraction)
				{
					empty = EncodeColor(InteractionTextColor) + text5;
					if (!string.IsNullOrEmpty(empty) && suppressIllumatedLetter && !flag2)
					{
						empty = text + GUIUtils.GetText(1883) + empty;
						flag2 = true;
					}
				}
				else
				{
					empty = ProcessSpeakerText(text5, flag2 ? null : text, characterColor, m_SpeakerTextColor);
					flag2 |= m_IncludedName;
				}
				num += TextList.Add(empty);
				if (IsInteraction)
				{
					empty = "[FFFFFF]" + text4;
				}
				ConsoleMessage(empty);
			}
			List<Console.ConsoleMessage> list = Console.Instance.FetchDialogueMessages();
			if (list != null && list.Count > 0)
			{
				num += TextList.Add("");
				foreach (Console.ConsoleMessage item in list)
				{
					string text6 = m_EventColor + item.m_message + "[-]";
					text6 = text6.Replace("[00FF00]", m_EventColor);
					num += TextList.Add(text6);
				}
			}
			num += TextList.Add("");
			SetPortrait(SpeakerPortraitBox, SpeakerPortraitTexture, speaker);
			if (IsInteraction)
			{
				ScriptedInteraction component4 = m_ActiveFlowChart.OwnerObject.GetComponent<ScriptedInteraction>();
				if ((bool)component4)
				{
					if (InteractionBackground.mainTexture == null)
					{
						InteractionBackground.mainTexture = component4.CurrentPortrait;
						InteractionDissolveTweener.Play(forward: true);
					}
					else if (InteractionBackground.mainTexture != component4.CurrentPortrait)
					{
						NextInteractionTexture = component4.CurrentPortrait;
						InteractionDissolveTweener.Play(forward: false);
					}
				}
			}
			int num2 = 0;
			num2 = ((!Conversation.LocalizationDebuggingEnabled) ? DrawResponses() : DrawResponsesDebug());
			num += num2;
			if (IsInteraction)
			{
				UIDynamicFontSize.Guarantee(TextList.textLabel.gameObject);
				int num3 = Mathf.FloorToInt(InteractionContentPanel.clipRange.w / (TextList.textLabel.transform.localScale.y + (float)TextList.textLabel.font.verticalSpacing));
				num3 -= ((StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Cyrillic) ? 1 : 3);
				while (TextList.TotalLines < num3)
				{
					num += TextList.Insert("", num2);
				}
			}
			TextList.RebuildAllLines();
			UIPanel uIPanel = null;
			Transform transform = null;
			float num4 = 0f;
			if (IsConversation)
			{
				uIPanel = ContentPanel;
				transform = ConversationTextList.textLabel.transform;
				num4 = ConversationTextList.textLabel.font.verticalSpacing;
			}
			else
			{
				uIPanel = InteractionContentPanel;
				transform = InteractionLabel.TextLabel.transform;
				num4 = InteractionLabel.TextLabel.font.verticalSpacing;
			}
			UIDraggablePanel component5 = uIPanel.GetComponent<UIDraggablePanel>();
			if (component5 != null)
			{
				component5.ResetPosition();
				float num5 = (float)num * (transform.localScale.y + num4);
				float num6 = component5.GetRealRange() - num5;
				float scroll = 0f;
				if (num6 < 0f)
				{
					scroll = 0f - num6 + uIPanel.clipSoftness.y;
					num6 = uIPanel.clipSoftness.y;
				}
				if (IsConversation)
				{
					ConversationTextList.transform.localPosition = m_TextInitialTransform + new Vector3(0f, num6, 0f);
				}
				else if (StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Cyrillic)
				{
					InteractionLabel.transform.localPosition = new Vector3(m_InteractionTextInitialTransform.x, uIPanel.clipRange.w * 0.5f, m_InteractionTextInitialTransform.z);
				}
				else
				{
					InteractionLabel.transform.localPosition = m_InteractionTextInitialTransform;
				}
				component5.ResetPosition();
				component5.SetScroll(scroll);
			}
		}
		else
		{
			Console.Instance.ClearDialogueMessages();
		}
	}

	private bool GetSuppressIllumatedLetter(FlowChartNode node)
	{
		string extendedPropertyValue = node.ClassExtender.GetExtendedPropertyValue("SuppressIlluminatedLetter");
		bool result = false;
		bool.TryParse(extendedPropertyValue, out result);
		return result;
	}

	private void OnConversationScrollChanged(GameObject go)
	{
		if (m_ActiveFlowChart == null || !(ConversationMoreContentArrow != null))
		{
			return;
		}
		UIDraggablePanel component = ContentPanel.GetComponent<UIDraggablePanel>();
		if (IsConversation && component != null)
		{
			if (Mathf.Abs(component.GetScroll()) > 10f)
			{
				ConversationMoreContentArrow.alpha = 1f;
			}
			else
			{
				ConversationMoreContentArrow.alpha = 0f;
			}
		}
	}

	private void SetPortrait(GameObject box, UITexture texture, GameObject from)
	{
		if (from == null || conversation.IsSpeakerNameOfActiveNodeHidden(m_ActiveFlowChart))
		{
			box.SetActive(value: false);
			return;
		}
		box.SetActive(value: true);
		Portrait component = from.GetComponent<Portrait>();
		if (component == null)
		{
			box.SetActive(value: false);
		}
		else if (component.TextureLarge == null)
		{
			box.SetActive(value: false);
		}
		else
		{
			texture.mainTexture = component.TextureLarge;
		}
	}

	public string ProcessSpeakerText(string text, string speakerName, string speakerNameColor, string defaultColor)
	{
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		string text2 = "";
		if (!string.IsNullOrEmpty(speakerName))
		{
			string text3 = GUIUtils.GetText(1883);
			text2 = speakerNameColor + speakerName + text3;
		}
		stringBuilder.Append(ConversationDescriptionColorString);
		Stack<char> stack = new Stack<char>();
		for (int i = 0; i < text.Length; i++)
		{
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < m_QuotePairs.Length; j += 2)
			{
				if (text[i] == m_QuotePairs[j])
				{
					flag = true;
				}
				if (text[i] == m_QuotePairs[j + 1])
				{
					flag = true;
					if (stack.Count > 0 && stack.Peek() == m_QuotePairs[j])
					{
						flag2 = true;
					}
				}
			}
			if (flag)
			{
				if (flag2)
				{
					stack.Pop();
					stringBuilder.Append(text[i]);
					if (stack.Count == 0)
					{
						stringBuilder.Append(ConversationDescriptionColorString);
					}
				}
				else
				{
					stack.Push(text[i]);
					if (stack.Count > 0)
					{
						stringBuilder.Append(defaultColor);
					}
					stringBuilder.Append(text[i]);
				}
			}
			else if (text[i] == '\n')
			{
				if (stack.Count > 0)
				{
					stack.Clear();
					stringBuilder.Append(defaultColor);
				}
				stringBuilder.Append(text[i]);
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		m_IncludedName = true;
		return text2 + stringBuilder.ToString();
	}

	private bool MarkedAsRead(int response)
	{
		List<PlayerResponseNode> responseNodes = conversation.GetResponseNodes(m_ActiveFlowChart);
		if (response >= 0 && response < responseNodes.Count)
		{
			return MarkedAsRead(responseNodes[response]);
		}
		Debug.LogWarning("Tried to check read status of conversation response '" + response + "' but that index was invalid.");
		return false;
	}

	private bool MarkedAsRead(PlayerResponseNode response)
	{
		if (response.Persistence == PersistenceType.MarkAsRead)
		{
			return ConversationManager.Instance.GetMarkedAsRead(conversation, response.NodeID);
		}
		return false;
	}

	private string GetRespNodeColor(PlayerResponseNode response, FlowChartPlayer player)
	{
		if (response == null)
		{
			return m_RespColor;
		}
		if (!conversation.PassesConditionalsEx(response, player))
		{
			return m_DisabledRespColor;
		}
		if (MarkedAsRead(response))
		{
			return m_FollowedRespColor;
		}
		return m_RespColor;
	}

	private int DrawResponses()
	{
		int num = 0;
		CheckRemoveResponses();
		bool flag = conversation.ShouldShowPlayerResponses(m_ActiveFlowChart);
		List<PlayerResponseNode> responseNodes = conversation.GetResponseNodes(m_ActiveFlowChart);
		int num2 = 1;
		if (flag && responseNodes.Count > 0 && responseNodes.Any((PlayerResponseNode re) => conversation.PassesConditionalsEx(re, m_ActiveFlowChart)))
		{
			string text = GUIUtils.GetText(1884);
			foreach (PlayerResponseNode item in responseNodes)
			{
				if (item.Conditionals.Components.Count > 0)
				{
					TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.CONVERSATION_STAT_OPTION);
				}
				string text2 = GetRespNodeColor(item, m_ActiveFlowChart);
				try
				{
					if (conversation.PassesConditionalsEx(item, m_ActiveFlowChart))
					{
						text2 = text2 + num2 + text + conversation.GetNodeText(m_ActiveFlowChart, item);
					}
					else
					{
						string nodeQualifier = conversation.GetNodeQualifier(item, m_ActiveFlowChart);
						text2 = text2 + num2 + text + nodeQualifier + ((!string.IsNullOrEmpty(nodeQualifier)) ? " " : "") + "[" + GUIUtils.GetText(1425) + "]";
					}
				}
				catch (Exception ex)
				{
					text2 = text2 + num2 + text + "ERROR: " + ex.Message;
					Debug.LogException(ex, this);
				}
				num2++;
				num += TextList.Add(text2);
			}
		}
		if (!flag)
		{
			if (IsInteraction)
			{
				string text3 = GUIUtils.GetText(1884);
				num += TextList.Add(m_RespColor + num2 + text3 + GUIUtils.GetText(27));
				num2++;
			}
			else
			{
				FlowChartNode nextNode = conversation.GetNextNode(m_ActiveFlowChart);
				if (nextNode == null || (nextNode is ScriptNode && nextNode.Links.Count <= 0))
				{
					ContinueButton.gameObject.SetActive(value: true);
					ContinueButton.Label.text = GUIUtils.GetText(28).ToUpper();
				}
				else
				{
					ContinueButton.gameObject.SetActive(value: true);
					ContinueButton.Label.text = GUIUtils.GetText(27).ToUpper();
				}
			}
		}
		m_OutstandingResponses = num2 - 1;
		return num;
	}

	private int DrawResponsesDebug()
	{
		int num = 0;
		CheckRemoveResponses();
		string text = GUIUtils.GetText(1884);
		List<FlowChartNode> allNodesFromActiveNode = conversation.GetAllNodesFromActiveNode(m_ActiveFlowChart);
		int num2 = 1;
		if (allNodesFromActiveNode.Count > 0)
		{
			foreach (DialogueNode item in allNodesFromActiveNode)
			{
				try
				{
					num += TextList.Add(m_RespColor + num2 + text + conversation.GetNodeText(m_ActiveFlowChart, item));
				}
				catch (Exception ex)
				{
					num += TextList.Add(m_RespColor + num2 + text + "ERROR: " + ex.Message);
					Debug.LogException(ex, this);
				}
				num2++;
			}
		}
		num += TextList.Add("[00FFFF]" + num2 + text + "Go back[-]");
		num2++;
		if (IsConversation)
		{
			ContinueButton.gameObject.SetActive(value: true);
			ContinueButton.GetComponentInChildren<UILabel>().text = GUIUtils.GetText(28).ToUpper();
		}
		else if (IsInteraction)
		{
			num += TextList.Add(m_RespColor + num2 + text + "End");
			num2++;
		}
		m_OutstandingResponses = num2 - 1;
		return num;
	}

	public void ShowConversation()
	{
		Stealth.GlobalSetInStealthMode(inStealth: false);
		ConversationWindow.SetActive(value: true);
		InteractionWindow.SetActive(value: false);
		UIBarkstringManager.Instance.gameObject.SetActive(value: false);
		ContinueButton.gameObject.SetActive(value: false);
		GameObject[] tweenOnShowHide = TweenOnShowHide;
		for (int i = 0; i < tweenOnShowHide.Length; i++)
		{
			UITweener[] components = tweenOnShowHide[i].GetComponents<UITweener>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].Play(forward: true);
			}
		}
		ConsoleBegin();
		ConversationTextList.Clear();
		ShowWindow();
	}

	protected override void DoSuspend()
	{
		m_ConversationSuspended = false;
		m_InteractionSuspended = false;
		if (ConversationWindow.activeSelf)
		{
			m_ConversationSuspended = true;
			ConversationWindow.SetActive(value: false);
		}
		if (InteractionWindow.activeSelf)
		{
			m_InteractionSuspended = true;
			InteractionWindow.SetActive(value: false);
		}
	}

	protected override void DoUnsuspend()
	{
		if (m_ConversationSuspended)
		{
			ConversationWindow.SetActive(value: true);
		}
		if (m_InteractionSuspended)
		{
			InteractionWindow.SetActive(value: true);
		}
	}

	protected override bool Hide(bool forced)
	{
		if (ConversationWindow.activeSelf)
		{
			HideConversation();
		}
		if (InteractionWindow.activeSelf)
		{
			HideInteraction();
		}
		UIConversationWatcherMovie.Instance.Hide();
		Console.Instance.ClearDialogueMessages();
		return base.Hide(forced);
	}

	protected override void Show()
	{
		PartyMemberAI.PopAllStates();
		SceneTransition.CancelAllSceneTransitions();
		base.Show();
	}

	public void HideConversation()
	{
		ConversationWindow.SetActive(value: false);
		UIBarkstringManager.Instance.gameObject.SetActive(value: true);
		ConversationTextList.Clear();
		if (speaker != null)
		{
			Faction component = speaker.GetComponent<Faction>();
			if ((bool)component)
			{
				component.NotifyBeginSpeaking(null, state: false);
			}
			speaker = null;
		}
		m_ActiveFlowChart = null;
		m_CurrentNodeId = -1;
		GameObject[] tweenOnShowHide = TweenOnShowHide;
		for (int i = 0; i < tweenOnShowHide.Length; i++)
		{
			UITweener[] components = tweenOnShowHide[i].GetComponents<UITweener>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].Play(forward: false);
			}
		}
		ConsoleEnd();
		HideWindow();
	}

	public void ShowInteraction()
	{
		InteractionTextList.Clear();
		Stealth.GlobalSetInStealthMode(inStealth: false);
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
		InteractionWindow.SetActive(value: true);
		ConversationWindow.SetActive(value: false);
		ContinueButton.gameObject.SetActive(value: false);
		InteractionBackground.mainTexture = null;
		NextInteractionTexture = null;
		InteractionDissolveTweener.Reset();
		ConsoleBegin();
		ShowWindow();
	}

	public void EndShowInteraction(UITweener tween)
	{
		if (tween.direction == Direction.Forward)
		{
			InteractionContentPanel.alpha = 1f;
			InteractionBackground.gameObject.SetActive(value: true);
		}
	}

	public void HideInteraction()
	{
		InteractionWindow.SetActive(value: false);
		m_ActiveFlowChart = null;
		m_CurrentNodeId = -1;
		ConsoleEnd();
		HideWindow();
	}

	private void CheckRemoveResponses()
	{
		for (int i = 0; i < m_OutstandingResponses; i++)
		{
			TextList.RemoveLast();
		}
		m_OutstandingResponses = 0;
		m_TextListActiveResponseLine = -1;
		m_ControllerSelection = -1;
		m_MouseSelection = -1;
	}

	private void OnButton(GameObject go)
	{
		if (m_ActiveFlowChart == null)
		{
			Debug.LogError("ActiveFlowChart is null in ConversationManager.");
			EndConversation();
		}
		if (conversation == null)
		{
			Debug.LogError("ActiveFlowChart.CurrentFlowChart is null or not a Conversation in ConversationManager.");
			EndConversation();
		}
		if (conversation.GetResponseNodes(m_ActiveFlowChart, qualifiedOnly: true).Count == 0)
		{
			if (Conversation.LocalizationDebuggingEnabled)
			{
				End();
			}
			else
			{
				Continue();
			}
		}
	}

	private void Continue()
	{
		FlowChartNode nextNode = conversation.GetNextNode(m_ActiveFlowChart);
		if (nextNode == null)
		{
			End();
		}
		else
		{
			MoveToNode(nextNode.NodeID);
		}
	}

	private void MoveToNode(int nodeid)
	{
		GameObject speakerOrPlayer = conversation.GetSpeakerOrPlayer(m_ActiveFlowChart);
		if ((bool)speakerOrPlayer)
		{
			AudioSource component = speakerOrPlayer.GetComponent<AudioSource>();
			if (component != null)
			{
				component.Stop();
			}
		}
		FlowChartNode flowChartNode = conversation.GetNode(nodeid);
		while (flowChartNode is ScriptNode)
		{
			m_ActiveFlowChart = conversation.MoveToNode(flowChartNode.NodeID, m_ActiveFlowChart);
			flowChartNode = conversation.GetNextNode(m_ActiveFlowChart);
		}
		if (flowChartNode == null)
		{
			End();
		}
		else
		{
			m_ActiveFlowChart = conversation.MoveToNode(nodeid, m_ActiveFlowChart);
		}
	}

	private void End()
	{
		if (m_ActiveFlowChart == null)
		{
			HideConversation();
			HideInteraction();
		}
		else if (m_ActiveFlowChart.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Interaction)
		{
			EndInteraction();
		}
		else if (m_ActiveFlowChart.FlowChartDisplayMode == FlowChartPlayer.DisplayMode.Standard)
		{
			EndConversation();
		}
	}

	private void EndConversation()
	{
		if (m_ActiveFlowChart != null)
		{
			ConversationManager.Instance.EndConversation(m_ActiveFlowChart);
		}
		HideConversation();
		m_LastSpeakerPos = Vector3.zero;
	}

	private void HandleFadeOut()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(HandleFadeOut));
		bool flag = true;
		FlowChartPlayer activeFlowChart = m_ActiveFlowChart;
		HideInteraction();
		if (activeFlowChart != null)
		{
			flag = activeFlowChart.FadeFromBlackOnExit;
			ConversationManager.Instance.EndConversation(activeFlowChart);
		}
		if (flag)
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
		}
		m_handlingFade = false;
	}

	private void EndInteraction()
	{
		if (!m_handlingFade)
		{
			m_handlingFade = true;
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(HandleFadeOut));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0f);
		}
	}

	private void HandlePressed(GameObject go, bool isPressed)
	{
		m_LastClickPos = GameInput.MousePosition.y;
		m_HandlePressed = isPressed;
	}

	private static void ConsoleMessage(string str)
	{
		Console.AddMessage(str.Trim(), Console.ConsoleState.DialogueBig);
	}

	private static void ConsoleBegin()
	{
	}

	private static void ConsoleEnd()
	{
	}
}
