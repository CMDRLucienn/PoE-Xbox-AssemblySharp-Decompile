using System;
using UnityEngine;

public class UIInterstitialManager : UIHudWindow
{
	public static int ForChapter;

	public UIMultiSpriteImageButton ReplayButton;

	public UIMultiSpriteImageButton DoneButton;

	public UITexture PortraitTexture;

	public UIPanel ScrollPanel;

	public UICapitularLabel CapitularLabel;

	[Tooltip("The max speed of the autoscroll in pixels per second.")]
	public float ScrollSpeed = 8f;

	[Tooltip("The acceleration of the autoscroll in pixels per second per second.")]
	public float ScrollAcceleration = 8f;

	[Tooltip("Delay before scrolling starts, in seconds.")]
	public float ScrollDelay = 3f;

	private float m_TimeToScroll;

	private float m_ScrollVelocity;

	public AudioSource VoiceOver;

	private bool m_begin;

	private float m_contentHeight;

	private bool m_closing;

	private MusicManager.FadeParams m_fadeParams = new MusicManager.FadeParams();

	private bool m_Done;

	private InterstitialData m_LoadedData;

	public static UIInterstitialManager Instance { get; private set; }

	private bool IsDone
	{
		get
		{
			return m_Done;
		}
		set
		{
			ReplayButton.enabled = value;
			m_Done = value;
		}
	}

	private void Awake()
	{
		Instance = this;
		OnWindowHidden = (WindowHiddenDelegate)Delegate.Combine(OnWindowHidden, new WindowHiddenDelegate(OnInterstitialHidden));
		VoiceOver.spatialBlend = 0f;
		m_fadeParams.FadeType = MusicManager.FadeType.FadeOutStart;
		m_fadeParams.FadeOutDuration = 0.5f;
		m_fadeParams.FadeInDuration = 0f;
		m_fadeParams.PauseDuration = 0f;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		UIMultiSpriteImageButton replayButton = ReplayButton;
		replayButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(replayButton.onClick, new UIEventListener.VoidDelegate(Replay));
		UIMultiSpriteImageButton doneButton = DoneButton;
		doneButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(doneButton.onClick, new UIEventListener.VoidDelegate(Done));
	}

	private void Replay(GameObject sender)
	{
		if (IsDone && !m_closing)
		{
			m_begin = true;
			IsDone = false;
			m_TimeToScroll = ScrollDelay;
			ResetPanel();
		}
	}

	private void HandleCloseInterstitial()
	{
		HideWindow();
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(HandleCloseInterstitial));
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
		m_closing = false;
		if (ForChapter == 5)
		{
			QuestManager.Instance.StartPX2Umbrella();
		}
	}

	private void Done(GameObject sender)
	{
		if (!m_closing)
		{
			m_closing = true;
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(HandleCloseInterstitial));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0.75f);
		}
	}

	private void Update()
	{
		if (m_TimeToScroll > 0f)
		{
			m_TimeToScroll -= Time.unscaledDeltaTime;
		}
		else if (ScrollPanel.transform.localPosition.y < m_contentHeight - ScrollPanel.clipRange.w + 2f * ScrollPanel.clipSoftness.y)
		{
			m_ScrollVelocity = Mathf.Min(m_ScrollVelocity + Time.unscaledDeltaTime * ScrollAcceleration, ScrollSpeed);
			float num = m_ScrollVelocity * Time.unscaledDeltaTime;
			ScrollPanel.transform.localPosition = new Vector3(ScrollPanel.transform.localPosition.x, ScrollPanel.transform.localPosition.y + num, ScrollPanel.transform.localPosition.z);
			ScrollPanel.clipRange = new Vector4(ScrollPanel.clipRange.x, ScrollPanel.clipRange.y - num, ScrollPanel.clipRange.z, ScrollPanel.clipRange.w);
		}
		else
		{
			IsDone = true;
		}
		if (m_begin)
		{
			m_begin = false;
			GlobalAudioPlayer.Play(VoiceOver);
			m_contentHeight = NGUIMath.CalculateRelativeWidgetBounds(ScrollPanel.transform).size.y;
		}
	}

	protected override void Show()
	{
		ConversationManager.Instance.KillAllBarkStrings();
		FadeManager.Instance.CancelFade(FadeManager.FadeType.AreaTransition);
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
		m_TimeToScroll = ScrollDelay;
		m_ScrollVelocity = 0f;
		m_LoadedData = GameResources.LoadPrefab<InterstitialData>("interstitial" + ForChapter.ToString("0000"), instantiate: false);
		if ((bool)m_LoadedData)
		{
			ResetPanel();
			VoiceOver.clip = m_LoadedData.VoiceOver;
			m_begin = true;
			IsDone = false;
			CapitularLabel.text = m_LoadedData.Text.GetText();
			PortraitTexture.mainTexture = m_LoadedData.Portrait;
			if (string.IsNullOrEmpty(m_LoadedData.MusicResourcesPath))
			{
				Debug.LogWarning("No music on interstitial data " + m_LoadedData.name + " for loading screen.\n");
				return;
			}
			string text = "Audio/mus/" + m_LoadedData.MusicResourcesPath;
			AudioClip audioClip = Resources.Load<AudioClip>(text);
			if (audioClip != null)
			{
				MusicManager.Instance.PlayMusic(audioClip, m_fadeParams, loop: false);
			}
			else
			{
				Debug.LogError("Can't find path to audio clip '" + text + "' in resources.\n");
			}
		}
		else
		{
			Debug.LogError("Interstial Error: interstitial " + ForChapter + " tried to play but no asset bundle for it was found.");
		}
	}

	protected override bool Hide(bool forced)
	{
		GameResources.ClearPrefabReferences(typeof(InterstitialData));
		PortraitTexture.mainTexture = null;
		VoiceOver.clip = null;
		m_LoadedData = null;
		MusicManager.Instance.ResumeScriptedOrNormalMusic(resumeActiveSource: true);
		return base.Hide(forced);
	}

	private void OnInterstitialHidden(UIHudWindow window)
	{
		ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnInterstitialClosed);
	}

	private void ResetPanel()
	{
		ScrollPanel.clipRange = new Vector4(ScrollPanel.clipRange.x, ScrollPanel.clipRange.y + ScrollPanel.transform.localPosition.y, ScrollPanel.clipRange.z, ScrollPanel.clipRange.w);
		ScrollPanel.transform.localPosition = Vector3.zero;
	}
}
