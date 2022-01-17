using System;
using OEIFormats.FlowCharts;
using UnityEngine;

public class UIEndGameSlidesManager : UIHudWindow
{
	public GameObject NarratorPrefab;

	private GameObject m_Narrator;

	public UICapitularLabel Text;

	public UITexture Image;

	public UIPanel ScrollPanel;

	public ConversationObject ConversationFile;

	public EndGameSlidesImageData ImageData;

	public TweenValue TweenDissolveValue;

	public UIDissolve DissolveController;

	private UIDynamicLoadTexture m_LoadTexture;

	private FlowChartPlayer m_ConvoPlayer;

	public float SlideSpace = 1f;

	public float SlideMinLength = 18f;

	private float m_SlideCounter;

	private bool m_DidFade;

	private bool m_Started;

	private bool m_TransitionToMain;

	private bool m_IsCountingSpace;

	private float m_VODelay;

	private bool m_Complete;

	private string m_newImagePath;

	public static UIEndGameSlidesManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		m_LoadTexture = Image.GetComponent<UIDynamicLoadTexture>();
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)m_Narrator)
		{
			GameUtilities.Destroy(m_Narrator);
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_TransitionToMain)
		{
			GameState.LoadMainMenu(fadeOut: false);
			m_TransitionToMain = false;
		}
		if (m_VODelay > 0f)
		{
			m_VODelay -= Time.unscaledDeltaTime;
			if (m_VODelay <= 0f)
			{
				Conversation conversation = m_ConvoPlayer.CurrentFlowChart as Conversation;
				if ((bool)conversation)
				{
					conversation.PlayVO(conversation.GetNode(m_ConvoPlayer.CurrentNodeID), null, retrieveClipLength: false);
				}
			}
		}
		if (!m_Started)
		{
			return;
		}
		m_SlideCounter -= TimeController.sUnscaledDelta;
		if (!(m_SlideCounter <= 0f))
		{
			return;
		}
		if (!m_Narrator)
		{
			m_Narrator = UnityEngine.Object.Instantiate(NarratorPrefab);
			m_Narrator.name = m_Narrator.name.Replace("(Clone)", "");
			m_ConvoPlayer = ConversationManager.Instance.StartConversation(ConversationFile.Filename, m_Narrator, FlowChartPlayer.DisplayMode.Standard, disableVo: true);
			m_ConvoPlayer.DisableVO = false;
			m_VODelay = 1f;
			m_SlideCounter = SlideMinLength;
			UpdateText();
			return;
		}
		m_VODelay = 0f;
		if (!((Conversation)m_ConvoPlayer.CurrentFlowChart).IsVOPlaying(m_ConvoPlayer))
		{
			if (m_IsCountingSpace)
			{
				m_IsCountingSpace = false;
				Next();
			}
			else
			{
				m_IsCountingSpace = true;
				m_SlideCounter = SlideSpace;
			}
		}
	}

	public override void HandleInput()
	{
		if (GameInput.GetControlUp(MappedControl.CONV_CONTINUE))
		{
			Next();
		}
	}

	private void Next()
	{
		if (m_Complete || m_ConvoPlayer == null)
		{
			return;
		}
		Conversation conversation = m_ConvoPlayer.CurrentFlowChart as Conversation;
		if (!(conversation == null))
		{
			FlowChartNode nextNode = conversation.GetNextNode(m_ConvoPlayer);
			if (nextNode != null)
			{
				m_SlideCounter = SlideMinLength;
				m_ConvoPlayer = conversation.MoveToNode(nextNode.NodeID, m_ConvoPlayer);
				UpdateText();
			}
			else
			{
				HideWindow();
				m_Complete = true;
			}
		}
	}

	public void SetImage(int index)
	{
		if (index >= ImageData.Data.Length || index < 0)
		{
			UIDebug.Instance.LogOnScreenWarning("EndGameSlideImage index '" + index + "' is out of bounds.", UIDebug.Department.Design, 10f);
		}
		else if ((bool)m_LoadTexture)
		{
			m_newImagePath = ImageData.Data[index].Image;
			TweenDissolveValue.onFinished = OnReverseFadeComplete;
			TweenDissolveValue.Play(forward: false);
		}
	}

	private void OnReverseFadeComplete(UITweener tween)
	{
		TweenDissolveValue.onFinished = null;
		m_LoadTexture.SetPath(m_newImagePath);
		DissolveController.RandomOffsetDissolveTexture();
		TweenDissolveValue.Play(forward: true);
	}

	private void UpdateText()
	{
		string activeNodeText = (m_ConvoPlayer.CurrentFlowChart as Conversation).GetActiveNodeText(m_ConvoPlayer);
		Text.text = activeNodeText;
		UIWidgetUtils.UpdateDependents(base.gameObject, 2);
	}

	private void OnIntroFadeComplete()
	{
		float num3 = (OverPanel.alpha = (ScrollPanel.alpha = 1f));
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnIntroFadeComplete));
		FadeManager.Instance.FadeFrom(FadeManager.FadeType.Cutscene, 1f, Color.white);
		m_Started = true;
	}

	protected override void Show()
	{
		m_Started = false;
		m_DidFade = false;
		m_Complete = false;
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnIntroFadeComplete));
		FadeManager.Instance.FadeTo(FadeManager.FadeType.Cutscene, 3f, Color.white);
		float num3 = (OverPanel.alpha = (ScrollPanel.alpha = 0f));
		m_SlideCounter = 0f;
		m_VODelay = 0f;
		Text.text = "";
	}

	private void OnExitFadeComplete()
	{
		m_DidFade = true;
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnExitFadeComplete));
		Credits.RunRequestedImmediate = true;
		m_TransitionToMain = true;
	}

	protected override bool Hide(bool forced)
	{
		m_Started = false;
		if ((bool)m_Narrator)
		{
			GameUtilities.Destroy(m_Narrator);
		}
		if (forced)
		{
			return true;
		}
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(OnExitFadeComplete));
		FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Cutscene, 2f);
		return m_DidFade;
	}
}
