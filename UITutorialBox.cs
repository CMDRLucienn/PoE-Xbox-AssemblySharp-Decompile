using System;
using UnityEngine;

public class UITutorialBox : MonoBehaviour
{
	public UILabel ContentLabel;

	public UILabel ContentLabelShort;

	public UICheckbox MinimizeCheckBox;

	public UICheckbox DisableCheckBox;

	public UIWidget CloseButton;

	public UIWidget CloseButton2;

	public UIWidget MaximizeButton;

	public UIPanel Maximized;

	public GameObject MaximizedBg;

	public GameObject Minimized;

	public UITweenerAggregator MinimizeTween;

	private int m_CurrentTutorial = -1;

	private float m_AutoCloseTime;

	private bool m_MaximizedHovered;

	public static UITutorialBox Instance { get; private set; }

	public bool Visible
	{
		get
		{
			if (!(Maximized.alpha > 0f))
			{
				return Minimized.gameObject.activeSelf;
			}
			return true;
		}
	}

	private void Awake()
	{
		Instance = this;
		GameResources.OnLoadedSave += OnLoadedSave;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameResources.OnLoadedSave -= OnLoadedSave;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLoadedSave()
	{
		ForceHide();
	}

	private void Start()
	{
		UICheckbox minimizeCheckBox = MinimizeCheckBox;
		minimizeCheckBox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(minimizeCheckBox.onStateChange, new UICheckbox.OnStateChange(OnMinimizeChanged));
		UICheckbox disableCheckBox = DisableCheckBox;
		disableCheckBox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(disableCheckBox.onStateChange, new UICheckbox.OnStateChange(OnDisableChanged));
		UIEventListener uIEventListener = UIEventListener.Get(CloseButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnCloseWindow));
		UIEventListener uIEventListener2 = UIEventListener.Get(CloseButton2);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnCloseWindow));
		UIEventListener uIEventListener3 = UIEventListener.Get(MaximizeButton);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnMaximize));
		UIEventListener uIEventListener4 = UIEventListener.Get(MaximizedBg);
		uIEventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onHover, new UIEventListener.BoolDelegate(OnBackgroundHover));
		Hide();
	}

	private void OnBackgroundHover(GameObject sender, bool over)
	{
		m_MaximizedHovered = over;
	}

	private void OnCloseWindow(GameObject obj)
	{
		Hide();
	}

	private void OnMaximize(GameObject sender)
	{
		Maximized.alpha = 1f;
		MinimizeTween.ResetTo(0f);
		Minimized.gameObject.SetActive(value: false);
	}

	private void OnMinimizeChanged(GameObject sender, bool state)
	{
		TutorialManager.Instance.TutorialsAreMinimized = state;
	}

	private void OnDisableChanged(GameObject sender, bool state)
	{
		GameState.Option.SetOption(GameOption.BoolOption.SHOW_TUTORIALS, !state);
		GameState.Option.SaveToPrefs();
	}

	private void Update()
	{
		if (Minimized.gameObject.activeSelf && Maximized.alpha > 0f)
		{
			if (Time.realtimeSinceStartup > m_AutoCloseTime && !m_MaximizedHovered)
			{
				Hide();
			}
			else if (!GameState.Option.GetOption(GameOption.BoolOption.SHOW_TUTORIALS))
			{
				ForceHide();
			}
		}
	}

	public void ShowTutorial(int index)
	{
		MinimizeCheckBox.isChecked = TutorialManager.Instance.TutorialsAreMinimized;
		DisableCheckBox.isChecked = !GameState.Option.GetOption(GameOption.BoolOption.SHOW_TUTORIALS);
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.TutorialShown);
		if (TutorialManager.Instance.TutorialsAreMinimized)
		{
			if (!Minimized.gameObject.activeSelf)
			{
				MinimizeTween.ResetTo(0f);
			}
			Minimized.gameObject.SetActive(value: true);
			Maximized.alpha = 0f;
			MinimizeTween.Play(forward: true);
		}
		else
		{
			MinimizeTween.ResetTo(0f);
			Minimized.gameObject.SetActive(value: false);
			Maximized.alpha = 1f;
		}
		m_CurrentTutorial = index;
		ContentLabel.text = TutorialManager.Instance.Tutorials[index].Text.GetText();
		ContentLabelShort.text = ContentLabel.text;
		m_AutoCloseTime = Time.realtimeSinceStartup + TutorialManager.Instance.GetAutoCloseTime(ContentLabel.text);
		if (TutorialManager.Instance.Tutorials[index].Pauses)
		{
			TimeController.Instance.SafePaused = true;
		}
	}

	private void ForceHide()
	{
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.TutorialHidden);
		m_CurrentTutorial = -1;
		Maximized.alpha = 0f;
		MinimizeTween.Play(forward: false);
	}

	public void Hide()
	{
		if (m_CurrentTutorial >= 0)
		{
			int followedBy = TutorialManager.Instance.Tutorials[m_CurrentTutorial].FollowedBy;
			if (followedBy >= 0 && TutorialManager.STriggerTutorial(followedBy))
			{
				return;
			}
		}
		ForceHide();
	}
}
