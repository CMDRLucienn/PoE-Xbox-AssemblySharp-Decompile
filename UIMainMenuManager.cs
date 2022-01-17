using System;
using UnityEngine;

public class UIMainMenuManager : MonoBehaviour
{
	public AudioClip Music;

	private AudioSource m_AudioSource;

	private VolumeAsCategory m_AudioVolume;

	private UIPanel[] MenuPanels;

	private GameObject m_CreditsButton;

	private UIGrid m_GridToUpdate;

	public UIWidget BlackFade;

	public GameObject LoadScreenRoot;

	public Credits m_Credits;

	public UIWidget[] m_loadingSaveCacheWidgets;

	public static bool s_ReturningToMainMenuFromError;

	private bool RefreshMenuButtons;

	private bool m_Active = true;

	private bool m_DesiredActiveState = true;

	public static UIMainMenuManager Instance { get; private set; }

	public bool MenuActive
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_DesiredActiveState = value;
			UpdateActiveState();
		}
	}

	public void SetButtonsEnabled(bool enabled)
	{
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enabled;
		}
		UIMainMenuClickHandler[] componentsInChildren2 = GetComponentsInChildren<UIMainMenuClickHandler>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].RefreshState();
		}
	}

	public bool IsMenuActive()
	{
		return m_Active;
	}

	private void initCreditObject()
	{
		UIMultiSpriteImageButton[] componentsInChildren = GetComponentsInChildren<UIMultiSpriteImageButton>(includeInactive: true);
		foreach (UIMultiSpriteImageButton uIMultiSpriteImageButton in componentsInChildren)
		{
			if (uIMultiSpriteImageButton.name.Contains("Credits"))
			{
				m_CreditsButton = uIMultiSpriteImageButton.gameObject;
				UIGrid[] componentsInChildren2 = GetComponentsInChildren<UIGrid>();
				if (componentsInChildren2.Length != 0)
				{
					m_GridToUpdate = componentsInChildren2[0];
				}
			}
		}
	}

	private void disableCreditObjects(bool disable)
	{
		if (m_CreditsButton == null)
		{
			initCreditObject();
		}
		if (m_CreditsButton != null)
		{
			m_CreditsButton.SetActive(!disable);
			if (m_GridToUpdate != null)
			{
				m_GridToUpdate.repositionNow = true;
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		disableCreditObjects(disable: false);
		GameState.CleanupPersistAcrossSceneLoadObjects();
		InstanceID.ResetActiveList();
		if ((bool)GameState.Instance)
		{
			InstanceID component = GameState.Instance.GetComponent<InstanceID>();
			if ((bool)component)
			{
				InstanceID.AddSpecialObjectID(GameState.Instance.gameObject, component.Guid);
			}
		}
		Time.timeScale = 1f;
		if ((bool)TimeController.Instance)
		{
			TimeController.Instance.ProhibitPause = true;
		}
		SaveGameInfo.OnSaveCachingComplete += OnSaveCachingComplete;
		UIMainMenuClickHandler[] componentsInChildren = GetComponentsInChildren<UIMainMenuClickHandler>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RefreshState();
		}
		GameUtilities.CheckForExpansions();
		GameUtilities.CreateGlobalPrefabObject();
		FadeManager.Instance.SetFadeTarget(BlackFade);
		FadeManager.Instance.CancelFade(FadeManager.FadeType.AreaTransition);
		FadeManager.Instance.CancelFade(FadeManager.FadeType.Cutscene);
		FadeManager.Instance.CancelFade(FadeManager.FadeType.Script);
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 2f);
		ScriptManager.LoadScripts();
		Shader.WarmupAllShaders();
		QuestManager.LoadQuestData();
		GameResources.Cleanup();
		DataManager.LoadData();
		if (AIStateManager.StateManagerPool != null && AIStateManager.StatePool != null && CharacterStats.CombatStaminaRechargeRate >= 0f && GameResources.BasePath == "" && WorldMap.Maps != null)
		{
			Time.timeScale = 1f;
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		SaveGameInfo.OnSaveCachingComplete -= OnSaveCachingComplete;
		UIWindowManager.EnableWindowVisibilityHandling();
	}

	private void OnSaveCachingComplete(object sender, EventArgs e)
	{
		RefreshMenuButtons = true;
	}

	private void Start()
	{
		m_AudioSource = GetComponent<AudioSource>();
		m_AudioSource.clip = Music;
		m_AudioSource.loop = true;
		m_AudioSource.ignoreListenerVolume = true;
		m_AudioVolume = base.gameObject.AddComponent<VolumeAsCategory>();
		m_AudioVolume.Category = MusicManager.SoundCategory.MUSIC;
		m_AudioVolume.ExternalVolume = 1f;
		MenuPanels = GetComponentsInChildren<UIPanel>();
		UIWindowManager.DisableWindowVisibilityHandling();
		GameState.Instance.Reset(GameState.ResetStyle.NewGame);
		GameResources.ClearPrefabReferences();
		GameResources.LoadPrefab<WindowList>("WindowList", instantiate: false);
		if ((bool)BlackFade)
		{
			GameState.PersistAcrossSceneLoadsTracked(BlackFade);
		}
		if ((bool)MusicManager.Instance)
		{
			MusicManager.Instance.FadeOutAreaMusic(resetWhenFaded: true);
		}
		if (s_ReturningToMainMenuFromError)
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(2269), GUIUtils.GetText(2270));
			s_ReturningToMainMenuFromError = false;
		}
		if (!GameState.Option.GetOption(GameOption.BoolOption.ANALYTICS_MESSAGE_SHOWN))
		{
			GameState.Option.SetOption(GameOption.BoolOption.ANALYTICS_MESSAGE_SHOWN, setting: true);
		}
		m_Credits = UnityEngine.Object.FindObjectOfType<Credits>();
		if (Credits.RunRequested || Credits.RunRequestedImmediate)
		{
			UIPanel[] menuPanels = MenuPanels;
			for (int i = 0; i < menuPanels.Length; i++)
			{
				menuPanels[i].alpha = 0f;
			}
			if (UINewsManager.Instance != null)
			{
				UINewsManager.Instance.HideNewsImmediate();
			}
		}
		else if (UINewsManager.Instance != null)
		{
			UINewsManager.Instance.ShowNews();
		}
		if (!GamePassManager.Initialized)
		{
			XboxOneNativeWrapper.Instance.AddUser();
		}
		XboxOneNativeWrapper.Instance.SetPresence("main_menu");
	}

	private static void OnAnalyticsDialogEnd(UIMessageBox.Result result, UIMessageBox sender)
	{
		bool setting = ((result == UIMessageBox.Result.AFFIRMATIVE) ? true : false);
		GameState.Option.SetOption(GameOption.BoolOption.ANALYTICS_ENABLED, setting);
		GameState.Option.SetOption(GameOption.BoolOption.ANALYTICS_MESSAGE_SHOWN, setting: true);
		GameState.Option.SaveToPrefs();
	}

	private void HandleDebugInput()
	{
	}

	private void HandleWaitForSaveCachingDisplay()
	{
		if (m_loadingSaveCacheWidgets == null)
		{
			return;
		}
		UIWidget[] loadingSaveCacheWidgets = m_loadingSaveCacheWidgets;
		foreach (UIWidget uIWidget in loadingSaveCacheWidgets)
		{
			if (!SaveGameInfo.SaveCachingComplete())
			{
				UIAnchor component = uIWidget.GetComponent<UIAnchor>();
				if ((bool)component && component.widgetContainer != null)
				{
					GuiStringLabelOverride component2 = component.widgetContainer.GetComponent<GuiStringLabelOverride>();
					if ((bool)component2)
					{
						component2.BeginOverride();
					}
				}
				uIWidget.alpha = Mathf.Min(1f, uIWidget.alpha + Time.deltaTime * 2f);
			}
			else
			{
				UIAnchor component3 = uIWidget.GetComponent<UIAnchor>();
				if ((bool)component3 && component3.widgetContainer != null)
				{
					GuiStringLabelOverride component4 = component3.widgetContainer.GetComponent<GuiStringLabelOverride>();
					if ((bool)component4)
					{
						component4.EndOverride();
					}
				}
				uIWidget.alpha = Mathf.Max(0f, uIWidget.alpha - Time.deltaTime * 2f);
			}
			if (uIWidget.alpha > 0f && uIWidget is UISprite)
			{
				uIWidget.transform.rotation = Quaternion.Euler(uIWidget.transform.rotation.eulerAngles.x, uIWidget.transform.rotation.eulerAngles.y, uIWidget.transform.rotation.eulerAngles.z - 270f * Time.deltaTime);
			}
		}
	}

	private void Update()
	{
		Time.timeScale = 1f;
		UpdateActiveState();
		if (RefreshMenuButtons)
		{
			UIMainMenuClickHandler[] componentsInChildren = GetComponentsInChildren<UIMainMenuClickHandler>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RefreshState();
			}
			RefreshMenuButtons = false;
		}
		HandleWaitForSaveCachingDisplay();
		UIPanel[] menuPanels;
		if (m_Active)
		{
			HandleDebugInput();
			if ((bool)m_AudioSource)
			{
				if (!m_AudioSource.isPlaying)
				{
					m_AudioSource.Play();
				}
				m_AudioVolume.ExternalVolume = Mathf.Min(1f, m_AudioVolume.ExternalVolume + Time.deltaTime / 0.75f);
			}
			menuPanels = MenuPanels;
			foreach (UIPanel uIPanel in menuPanels)
			{
				uIPanel.alpha = Mathf.Min(1f, uIPanel.alpha + 2f * Time.deltaTime);
			}
			return;
		}
		bool flag = m_Credits != null && m_Credits.GetCreditsState() != Credits.CreditsState.NotPlaying;
		if ((bool)m_AudioSource && m_AudioSource.isPlaying && (FrontEndTitleIntroductionManager.HasStarted() || flag))
		{
			m_AudioVolume.ExternalVolume = Mathf.Max(0f, m_AudioVolume.ExternalVolume - Time.deltaTime / 0.75f);
			if (m_AudioVolume.ExternalVolume == 0f)
			{
				m_AudioSource.Stop();
			}
		}
		menuPanels = MenuPanels;
		foreach (UIPanel uIPanel2 in menuPanels)
		{
			uIPanel2.alpha = Mathf.Max(0f, uIPanel2.alpha - 2f * Time.deltaTime);
		}
	}

	private void UpdateActiveState()
	{
		if (m_Active == m_DesiredActiveState || (m_DesiredActiveState && (bool)FrontEndTitleIntroductionManager.Instance && !FrontEndTitleIntroductionManager.Instance.AllowMenuActivation()))
		{
			return;
		}
		SetButtonsEnabled(m_DesiredActiveState);
		if (UINewsManager.Instance != null)
		{
			if (m_DesiredActiveState)
			{
				UINewsManager.Instance.ShowNews();
			}
			else
			{
				UINewsManager.Instance.HideNews();
			}
		}
		m_Active = m_DesiredActiveState;
	}
}
