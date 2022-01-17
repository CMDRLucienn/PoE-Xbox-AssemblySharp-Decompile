using System;
using UnityEngine;

public class UIStrongholdManager : UIHudWindow
{
	[Serializable]
	public class Page
	{
		public UIMultiSpriteImageButton Button;

		public UIWidget ButtonSelected;

		public GameObject Content;

		public GameObject UnscrolledContent;

		public GUIDatabaseString Title;
	}

	public UILabel PageTitle;

	public UIWidget ParchmentBackground;

	public UIDraggablePanel MainPanel;

	public UIStrongholdBreakdown Breakdown;

	public Page[] Pages;

	private Stronghold.WindowPane m_CurrentPane;

	private bool m_Initted;

	private UIMessageBox m_ErrorMb;

	public static UIStrongholdManager Instance { get; private set; }

	public Stronghold Stronghold => GameState.Stronghold;

	public Stronghold.WindowPane ShowForPane { get; set; }

	protected Stronghold.WindowPane CurrentPane
	{
		get
		{
			return m_CurrentPane;
		}
		set
		{
			SelectTab(value);
		}
	}

	public override int CyclePosition
	{
		get
		{
			if (!Stronghold.Activated)
			{
				return -1;
			}
			return 4;
		}
	}

	public float SetParchmentHeight(float height)
	{
		MainPanel.ResetPosition();
		ParchmentBackground.transform.localScale = new Vector3(ParchmentBackground.transform.localScale.x, Mathf.Max(height, MainPanel.panel.clipRange.w - 1f), 1f);
		MainPanel.ResetPosition();
		return ParchmentBackground.transform.localScale.y;
	}

	private void Awake()
	{
		Instance = this;
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
		Page[] pages = Pages;
		for (int i = 0; i < pages.Length; i++)
		{
			UIMultiSpriteImageButton button = pages[i].Button;
			button.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button.onClick, new UIEventListener.VoidDelegate(OnTabClicked));
		}
		UIStrongholdActionsPage[] componentsInChildren = GetComponentsInChildren<UIStrongholdActionsPage>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			componentsInChildren[0].Init();
		}
	}

	private void Update()
	{
		if (WindowActive())
		{
			Stronghold.View();
			SelectTab(CurrentPane);
		}
	}

	private void OnTabClicked(GameObject sender)
	{
		SelectTab(sender);
	}

	private void TriggerTutorial(Stronghold.WindowPane tab)
	{
		TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.STRONGHOLD_SCREEN_OPENED);
		trigger.StrongholdTab = tab;
		TutorialManager.STriggerTutorialsOfType(trigger);
	}

	private void SelectTab(GameObject button)
	{
		if (button == Pages[4].Button.gameObject)
		{
			if ((bool)m_ErrorMb)
			{
				m_ErrorMb.HideWindow();
			}
			UIWindowManager.Instance.SuspendFor(UIPartyManager.Instance);
			UIPartyManager.Instance.ShowWindow();
			return;
		}
		for (int i = 0; i < Pages.Length; i++)
		{
			if (Pages[i].Button.gameObject == button)
			{
				SelectTab((Stronghold.WindowPane)i);
				break;
			}
		}
	}

	public void SelectTab(Stronghold.WindowPane tab)
	{
		if (WindowActive())
		{
			TriggerTutorial(tab);
		}
		if (tab == Stronghold.WindowPane.Companions)
		{
			if ((bool)m_ErrorMb)
			{
				m_ErrorMb.HideWindow();
			}
			UIWindowManager.Instance.SuspendFor(UIPartyManager.Instance);
			UIPartyManager.Instance.ShowWindow();
			return;
		}
		if (tab != m_CurrentPane || !m_Initted)
		{
			m_Initted = true;
			ForceSelectTab(tab);
		}
		if ((bool)PageTitle)
		{
			PageTitle.text = GUIUtils.GetText(873) + " - " + Pages[(int)tab].Title.GetText();
		}
		for (int i = 0; i < Pages.Length; i++)
		{
			bool flag = (bool)Pages[i].Content && Pages[i].Content.activeSelf;
			Pages[i].ButtonSelected.alpha = (flag ? 1 : 0);
			Pages[i].Button.ForceDown(flag);
		}
	}

	public void ForceSelectTab(Stronghold.WindowPane tab)
	{
		if (tab == Stronghold.WindowPane.Companions)
		{
			if ((bool)m_ErrorMb)
			{
				m_ErrorMb.HideWindow();
			}
			UIWindowManager.Instance.SuspendFor(UIPartyManager.Instance);
			UIPartyManager.Instance.ShowWindow();
			return;
		}
		MainPanel.ResetPosition();
		m_CurrentPane = tab;
		for (int i = 0; i < Pages.Length; i++)
		{
			if ((bool)Pages[i].Content)
			{
				Pages[i].Content.SetActive(tab == (Stronghold.WindowPane)i);
			}
			if ((bool)Pages[i].UnscrolledContent)
			{
				Pages[i].UnscrolledContent.SetActive(tab == (Stronghold.WindowPane)i);
			}
		}
		MainPanel.ResetPosition();
	}

	protected override void Show()
	{
		if ((bool)Stronghold && !Stronghold.Activated)
		{
			HideWindow();
			m_ErrorMb = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.GetText(990));
		}
		else if ((bool)Stronghold && Stronghold.Disabled)
		{
			HideWindow();
			m_ErrorMb = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.GetText(1894));
		}
		else
		{
			ForceSelectTab(ShowForPane);
		}
		ShowForPane = Stronghold.WindowPane.Status;
		Stronghold.TryDisplayLastUnshownAdventureReport();
	}

	public void ShowMessage(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", message);
		}
	}
}
